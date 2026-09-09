using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Test.TestHelpers;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    /// <summary>
    /// Clock-driven tests for <see cref="OlTableExtensions.EtlAsync"/>. The 250 ms per-row
    /// deadline is placed under an injected clock, so both the expiry path and the green path
    /// are deterministic and neither waits on wall time.
    /// </summary>
    [TestClass]
    // Not parallelized: this class drives a real Task.Run gate.
    [DoNotParallelize]
    public class OlTableExtensionsEtlClockTests
    {
        private static Mock<Outlook.Row> CreateRowMock(
            object[] values,
            IDictionary<int, string> binaryStrings,
            IDictionary<int, object> indexedValues
        )
        {
            var mockRow = new Mock<Outlook.Row>();
            mockRow.Setup(r => r.GetValues()).Returns(values);

            foreach (var pair in binaryStrings)
            {
                mockRow.Setup(r => r.BinaryToString(pair.Key)).Returns(pair.Value);
            }

            foreach (var pair in indexedValues)
            {
                mockRow.Setup(r => r[pair.Key]).Returns(pair.Value);
            }

            return mockRow;
        }

        /// <summary>
        /// Builds a Table over the supplied rows. The onGetNextRow action is the test-owned gate:
        /// blocking inside it keeps the row read from completing, so the ETL hop deadline is the
        /// only thing that can end the attempt.
        /// </summary>
        private static Mock<Outlook.Table> CreateTableWithColumns(
            string[] columnNames,
            System.Action onGetNextRow,
            object[,] array,
            params Mock<Outlook.Row>[] rows
        )
        {
            var mockTable = new Mock<Outlook.Table>();
            var mockColumns = new Mock<Outlook.Columns>();
            mockTable.Setup(t => t.Columns).Returns(mockColumns.Object);
            mockColumns.Setup(c => c.Count).Returns(columnNames.Length);

            for (var index = 0; index < columnNames.Length; index++)
            {
                var mockColumn = new Mock<Outlook.Column>();
                mockColumn.Setup(c => c.Name).Returns(columnNames[index]);
                mockColumns.Setup(c => c[index + 1]).Returns(mockColumn.Object);
            }

            mockTable.Setup(t => t.GetRowCount()).Returns(rows.Length);

            var currentRow = 0;
            mockTable.Setup(t => t.MoveToStart()).Callback(() => currentRow = 0);
            mockTable.Setup(t => t.EndOfTable).Returns(() => currentRow >= rows.Length);
            mockTable
                .Setup(t => t.GetNextRow())
                .Returns(() =>
                {
                    onGetNextRow();
                    return rows[currentRow++].Object;
                });

            if (array is not null)
            {
                mockTable.Setup(t => t.GetArray(It.IsAny<int>())).Returns(array);
            }

            return mockTable;
        }

        private static Dictionary<string, Func<object, string>> CreateConverters()
        {
            return new Dictionary<string, Func<object, string>>
            {
                { "MessageRecipients", _ => "Converted Recipients" },
            };
        }

        /// <summary>
        /// Documents the surviving EtlAsync contract on deadline expiry: the TimeoutException is
        /// swallowed, a null data array is returned through the tuple's nullable first element, and
        /// the supplied token source is cancelled. This is the shape the DfDeedle guard now catches.
        /// </summary>
        [TestMethod]
        public async Task EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource()
        {
            // Arrange
            var gate = new ManualResetEventSlim(false);
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-101" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var mockTable = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                gate.Wait,
                null,
                row
            );
            var barrier = new ArmingBarrierTimeProvider(new FakeTimeProvider());
            var tokenSource = new CancellationTokenSource();

            try
            {
                // Act
                Task<(object[,] data, Dictionary<string, int> columnInfo)> call =
                    mockTable.Object.EtlAsync(
                        CancellationToken.None,
                        tokenSource,
                        0,
                        null,
                        CreateConverters(),
                        barrier
                    );

                // The row read is blocked on the gate, so the hop deadline is armed before the
                // clock moves. Advancing earlier would arm the timer past an elapsed deadline.
                await barrier.Armed;
                barrier.Advance(250);
                var (data, _) = await call;

                // Assert
                data.Should().BeNull();
                tokenSource.IsCancellationRequested.Should().BeTrue();
            }
            finally
            {
                // Release the gate so the orphaned Task.Run body completes.
                gate.Set();
            }
        }

        /// <summary>
        /// The deterministic green path: the clock never advances, so the 250 ms deadline cannot
        /// fire regardless of host load, and the rows transform normally.
        /// </summary>
        [TestMethod]
        public async Task EtlAsync_ClockNeverAdvances_ReturnsTransformedRows()
        {
            // Arrange: no gate is engaged, so the row read completes immediately.
            var recipient = new object();
            var row = CreateRowMock(
                new object[] { recipient, "raw-store", "Subject" },
                new Dictionary<int, string> { { 2, "STORE-ID-102" } },
                new Dictionary<int, object> { { 1, recipient } }
            );
            var mockTable = CreateTableWithColumns(
                new[] { "MessageRecipients", "Store", "Subject" },
                () => { },
                null,
                row
            );
            var tokenSource = new CancellationTokenSource();

            // Act
            var (data, columnInfo) = await mockTable.Object.EtlAsync(
                CancellationToken.None,
                tokenSource,
                0,
                null,
                CreateConverters(),
                new FakeTimeProvider()
            );

            // Assert
            columnInfo["Store"].Should().Be(1);
            data[0, 1].Should().Be("STORE-ID-102");
            tokenSource.IsCancellationRequested.Should().BeFalse();
        }

        /// <summary>
        /// The GetArray branch: with no BinaryToStringFields column and no object converters,
        /// EtlAsync snapshots the whole table in one call instead of reading row by row. Its
        /// 250 ms deadline is armed on a clock that never advances, so it cannot fire.
        /// </summary>
        [TestMethod]
        public async Task EtlAsync_NoBinaryOrObjectFields_UsesGetArrayBranchOnControlledClock()
        {
            // Arrange
            var array = new object[,]
            {
                { "entry-1", "Subject" },
            };
            var row = CreateRowMock(
                new object[] { "entry-1", "Subject" },
                new Dictionary<int, string>(),
                new Dictionary<int, object>()
            );
            var mockTable = CreateTableWithColumns(
                new[] { "EntryID", "Subject" },
                () => { },
                array,
                row
            );
            var tokenSource = new CancellationTokenSource();

            // Act
            var (data, columnInfo) = await mockTable.Object.EtlAsync(
                CancellationToken.None,
                tokenSource,
                0,
                null,
                null,
                new FakeTimeProvider()
            );

            // Assert
            columnInfo["Subject"].Should().Be(1);
            data[0, 0].Should().Be("entry-1");
            tokenSource.IsCancellationRequested.Should().BeFalse();
        }
    }
}
