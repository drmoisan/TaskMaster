using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Deedle;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.Test.TestHelpers;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.Extensions
{
    /// <summary>
    /// Deadline-expiry regression tests for <see cref="DfDeedle.GetEmailDataInViewAsync"/>.
    /// The ETL deadline is placed under an injected clock, so no test here waits on wall time.
    /// </summary>
    [TestClass]
    // Not parallelized: this class drives a real Task.Run gate and shares the DfDeedle logger,
    // exactly as DfDeedleQfcColumnTimeoutTests does.
    [DoNotParallelize]
    public class DfDeedleEtlTimeoutTests
    {
        /// <summary>A ProgressTracker whose Report overrides do nothing, so progress reporting
        /// cannot influence timing or produce output during the test.</summary>
        private sealed class SilentProgressTracker : ProgressTracker
        {
            public SilentProgressTracker()
                : base(new CancellationTokenSource()) { }

            public override void Report((int Value, string JobName) report) { }

            public override void Report(double value) { }

            public override void Report(double value, string jobName) { }
        }

        private static ProgressTracker CreateProgressTracker()
        {
            return new ProgressTracker(new SilentProgressTracker(), allocation: 100, startingAt: 0);
        }

        /// <summary>
        /// Builds a folder whose UserDefinedProperties collection carries a Triage entry, so
        /// EnsureTriageColumnExists returns true and no modal-dialog seam is reached.
        /// </summary>
        private static Mock<MAPIFolder> BuildFolderWithTriageUdp()
        {
            var udp = new Mock<UserDefinedProperty>(MockBehavior.Loose);
            udp.SetupGet(p => p.Name).Returns("Triage");

            var udpList = new List<UserDefinedProperty> { udp.Object };
            var udps = new Mock<UserDefinedProperties>(MockBehavior.Loose);
            udps.Setup(u => u.GetEnumerator()).Returns(udpList.GetEnumerator());

            var folder = new Mock<MAPIFolder>(MockBehavior.Loose);
            folder.SetupGet(f => f.UserDefinedProperties).Returns(udps.Object);
            folder.SetupGet(f => f.StoreID).Returns("store-1");
            folder.SetupGet(f => f.Name).Returns("Inbox");
            return folder;
        }

        /// <summary>
        /// Builds a strict one-row Table behind a strict Explorer. The three supplied actions are
        /// the test-owned gates, in the order the production path reaches them: the first runs
        /// inside TableView.GetTable, which is the call the table acquisition makes; the second
        /// runs inside Columns.Add("SentOn"), which is the first call AddQfcColumns makes; and the
        /// third runs inside GetNextRow, which is the first call the ETL row reader makes.
        /// Blocking on them makes the arming order deterministic.
        /// </summary>
        private static Mock<Outlook.Explorer> BuildExplorer(
            Mock<MAPIFolder> folder,
            System.Action onGetTable,
            System.Action onAddSentOnColumn,
            System.Action onGetNextRow
        )
        {
            var row = new Mock<Row>(MockBehavior.Strict);
            row.Setup(x => x.GetValues())
                .Returns(new object[] { "entry-1", "IPM.Note", "2024-01-01", "conv-raw", "A" });
            row.Setup(x => x.BinaryToString(4)).Returns("conv-1");

            var columns = new Mock<Columns>(MockBehavior.Loose);
            var table = new Mock<Table>(MockBehavior.Strict);
            table.SetupGet(x => x.Columns).Returns(columns.Object);
            table.Setup(x => x.MoveToStart());
            table.Setup(x => x.GetRowCount()).Returns(1);

            var currentRow = 0;
            table.Setup(x => x.EndOfTable).Returns(() => currentRow >= 1);
            table
                .Setup(x => x.GetNextRow())
                .Returns(() =>
                {
                    onGetNextRow();
                    currentRow++;
                    return row.Object;
                });

            var columnNames = new[]
            {
                "EntryID",
                "MessageClass",
                "SentOn",
                "ConversationId",
                "Triage",
            };
            columns.Setup(x => x.Count).Returns(columnNames.Length);
            for (var index = 0; index < columnNames.Length; index++)
            {
                var column = new Mock<Column>(MockBehavior.Strict);
                column.SetupGet(x => x.Name).Returns(columnNames[index]);
                columns.Setup(x => x[index + 1]).Returns(column.Object);
            }

            columns.Setup(x => x.Add("SentOn")).Callback(onAddSentOnColumn).Returns((Column)null);

            var tableView = new Mock<TableView>(MockBehavior.Strict);
            tableView
                .Setup(x => x.GetTable())
                .Returns(() =>
                {
                    onGetTable();
                    return table.Object;
                });

            var explorer = new Mock<Outlook.Explorer>(MockBehavior.Strict);
            explorer.SetupGet(x => x.CurrentView).Returns(tableView.Object);
            explorer.SetupGet(x => x.CurrentFolder).Returns(folder.Object);
            return explorer;
        }

        /// <summary>
        /// When the ETL row-read deadline expires, EtlAsync swallows the TimeoutException and
        /// returns a null data array. GetEmailDataInViewAsync must name the folder in an
        /// InvalidOperationException rather than dereferencing the null snapshot.
        /// The clock is an <see cref="ArmingBarrierTimeProvider"/> so it is advanced only after
        /// the deadline it is meant to expire has actually been armed.
        /// </summary>
        [TestMethod]
        public async Task GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder()
        {
            // Arrange: three test-owned gates make the timer-arming order deterministic. The
            // barrier's Armed signal is a latch, so it drops a signal whenever two timers arm
            // inside one await window; one gate per timer keeps each window to a single arming.
            var gateAcquire = new ManualResetEventSlim(false);
            var gateA = new ManualResetEventSlim(false);
            var gateB = new ManualResetEventSlim(false);
            var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider());
            var explorer = BuildExplorer(
                BuildFolderWithTriageUdp(),
                gateAcquire.Wait,
                gateA.Wait,
                gateB.Wait
            );
            var progress = CreateProgressTracker();

            try
            {
                // Act
                Task<Frame<int, string>> call = DfDeedle.GetEmailDataInViewAsync(
                    explorer.Object,
                    CancellationToken.None,
                    new CancellationTokenSource(),
                    progress,
                    timeProvider: barrier
                );

                // Timer 1: the 2000 ms table-acquisition deadline, which is now armed on the
                // caller's clock because GetEmailDataInViewAsync forwards the provider to
                // GetTableInViewAsync. It is deterministic because GetTable is blocked on the
                // acquisition gate and so the work task cannot complete first.
                await barrier.Armed;
                barrier.ReArm();
                gateAcquire.Set();

                // Timer 2: the 3000 ms column-add deadline, deterministic because the adder is
                // blocked on gate A and so the work task cannot complete first.
                await barrier.Armed;
                barrier.ReArm();
                gateA.Set();

                // Timer 3: the 250 ms ETL hop deadline, deterministic because the row read is
                // blocked on gate B. Advancing before this timer exists would hang the test.
                await barrier.Armed;
                barrier.Advance(250);

                // Assert
                await FluentActions
                    .Awaiting(() => call)
                    .Should()
                    .ThrowAsync<InvalidOperationException>()
                    .WithMessage("*Inbox*");
            }
            finally
            {
                // Release all three gates so the orphaned Task.Run bodies complete.
                gateAcquire.Set();
                gateA.Set();
                gateB.Set();
            }
        }

        /// <summary>
        /// The deterministic green path: every deadline on the call chain is armed on a clock
        /// that never moves, so no deadline can fire regardless of host load.
        /// </summary>
        [TestMethod]
        public async Task GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame()
        {
            // Arrange: no gate is engaged, so the production path runs to completion.
            var explorer = BuildExplorer(
                BuildFolderWithTriageUdp(),
                () => { },
                () => { },
                () => { }
            );
            var progress = CreateProgressTracker();

            // Act
            Frame<int, string> result = await DfDeedle.GetEmailDataInViewAsync(
                explorer.Object,
                CancellationToken.None,
                new CancellationTokenSource(),
                progress,
                timeProvider: new FakeTimeProvider()
            );

            // Assert
            result.RowCount.Should().Be(1);
            result
                .ColumnKeys.Should()
                .Contain(new[] { "EntryId", "MessageClass", "ConversationId" });
        }

        /// <summary>
        /// The default-delegate path of the synchronous GetEmailDataInView. Supplying no etl
        /// argument runs the production DefaultTableEtl over the table, which is what every
        /// existing caller does now that the TableEtlInvoker static has been removed.
        /// </summary>
        [TestMethod]
        public void GetEmailDataInView_NoEtlArgument_UsesProductionDefaultDelegate()
        {
            // Arrange
            var explorer = BuildExplorer(
                BuildFolderWithTriageUdp(),
                () => { },
                () => { },
                () => { }
            );

            // Act
            Frame<int, string> result = DfDeedle.GetEmailDataInView(explorer.Object);

            // Assert
            result.Should().NotBeNull();
            result.RowCount.Should().Be(1);
        }
    }
}
