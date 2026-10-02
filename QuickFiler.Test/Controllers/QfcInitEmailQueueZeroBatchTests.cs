using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Deedle;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #244 regression coverage: <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/>
    /// must not attempt to project an empty (<c>batchSize &lt;= 0</c>) batch through
    /// <c>Frame.GetRowsAs&lt;IEmailSortInfo&gt;()</c>, which throws when the sliced frame's column
    /// index is empty. Uses the same uninitialized-instance-plus-reflection-field-assignment pattern
    /// as <c>QfcDatamodelTests</c> to exercise the method without a live Outlook process.
    /// </summary>
    /// <remarks>
    /// v1.1 revision (issue #244): every test below assigns an inert, recording
    /// <see cref="QfcDatamodel.RemainingEmailLoader"/> delegate via the internal seam BEFORE
    /// calling <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/>. Without this,
    /// the started worker's <c>Worker_DoWork</c> reaches the real
    /// <c>LoadRemainingEmailsToQueueAsync</c>, which pops a live
    /// <see cref="System.Windows.Forms.MessageBox"/> dialog and touches Outlook COM
    /// (<c>_olApp.GetNamespace("MAPI")</c>) — this is the maintainer-reported defect in the v1.0
    /// revision of these tests, and this file must never reproduce it. Issue #950: every test also
    /// assigns the <c>WorkerStarter</c> seam a starter that raises <c>DoWork</c> synchronously on
    /// the test thread through the nested <c>SynchronousBackgroundWorker</c>, so no test starts a
    /// thread-pool worker and none outlives the test.
    /// </remarks>
    [TestClass]
    public class QfcInitEmailQueueZeroBatchTests
    {
        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Builds a <see cref="QfcDatamodel"/> without running its COM-bound constructors so
        /// <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/> can be exercised in
        /// isolation. Fields the method under test reads are assigned explicitly via
        /// <see cref="SetPrivateField"/>. Because this bypasses all constructors, the
        /// <see cref="QfcDatamodel.RemainingEmailLoader"/> seam is <see langword="null"/> on the
        /// returned instance until a test assigns it explicitly.
        /// </summary>
        private static QfcDatamodel CreateUninitializedDatamodel() =>
            (QfcDatamodel)FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Builds a two-row <see cref="Frame{TRowKey, TColumnKey}"/> whose columns match
        /// <see cref="IEmailSortInfo"/> exactly, mirroring the shape of a real, well-formed
        /// <c>_frame</c> so <c>GetRowsAs&lt;IEmailSortInfo&gt;()</c> succeeds against it.
        /// </summary>
        private static Frame<int, string> CreateTwoRowEmailFrame()
        {
            var records = new[]
            {
                new
                {
                    EntryId = "EntryId-1",
                    MessageClass = "IPM.Note",
                    SentOn = new DateTime(2026, 1, 1),
                    ConversationId = "Conversation-1",
                    Triage = "A",
                    StoreId = "Store-1",
                },
                new
                {
                    EntryId = "EntryId-2",
                    MessageClass = "IPM.Note",
                    SentOn = new DateTime(2026, 1, 2),
                    ConversationId = "Conversation-2",
                    Triage = "B",
                    StoreId = "Store-2",
                },
            };
            return Frame.FromRecords(records);
        }

        /// <summary>
        /// Builds an inert <see cref="QfcDatamodel.RemainingEmailLoader"/> replacement that records
        /// invocation via <paramref name="invoked"/> and returns a completed <c>true</c> result without
        /// ever touching <see cref="System.Windows.Forms.MessageBox"/> or Outlook COM (<c>_olApp</c>).
        /// Assigning this delegate before starting a real <see cref="BackgroundWorker"/> is what makes
        /// it safe to call <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/> with a real
        /// worker in a unit test.
        /// </summary>
        private static Func<CancellationToken, Task<bool>> CreateInertRemainingEmailLoader(
            out TaskCompletionSource<bool> invoked
        )
        {
            var completionSource = new TaskCompletionSource<bool>();
            invoked = completionSource;
            return _ =>
            {
                completionSource.TrySetResult(true);
                return Task.FromResult(true);
            };
        }

        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so no worker started by
        /// <c>InitEmailQueue</c> outlives the test (issue #950). Duplicated per file, following
        /// the convention documented on <c>QfcDatamodelLivenessTests</c>.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();

        /// <summary>
        /// Issue #244 AC1: a zero batch size must not throw the Deedle "The interface member
        /// 'EntryId' does not exist in the column index." exception, and must return an empty,
        /// non-null list. The inert <see cref="QfcDatamodel.RemainingEmailLoader"/> is assigned before
        /// the call so the worker <c>InitEmailQueue</c> starts cannot reach live UX/COM regardless of
        /// whether the <c>batchSize &lt;= 0</c> guard is present.
        /// </summary>
        [TestMethod]
        public void InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            SetPrivateField(model, "_frame", CreateTwoRowEmailFrame());
            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out _);
            model.WorkerStarter = StartSynchronously;
            IList<MailItem> result = null;

            // Act
            System.Action act = () =>
                result = model.InitEmailQueue(0, new SynchronousBackgroundWorker());

            // Assert
            act.Should().NotThrow();
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Issue #244 AC2: a zero batch size must still set up and start the background worker so
        /// remaining emails continue to load into the master queue. <see cref="BackgroundWorker.WorkerSupportsCancellation"/>
        /// (set synchronously by <see cref="QfcDatamodel.SetupWorker"/>) proves the worker was set up.
        /// Issue #950: the worker is started through the <c>WorkerStarter</c> seam with a starter
        /// that raises <c>DoWork</c> on this thread, and the inert loader completes its
        /// <see cref="TaskCompletionSource{TResult}"/> synchronously, so the loader-invoked signal
        /// is read without any wait as soon as <c>InitEmailQueue</c> returns.
        /// </summary>
        [TestMethod]
        public void InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            SetPrivateField(model, "_frame", CreateTwoRowEmailFrame());
            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out var loaderInvokedTcs);
            model.WorkerStarter = StartSynchronously;
            var worker = new SynchronousBackgroundWorker();

            // Act
            model.InitEmailQueue(0, worker);

            // Assert
            worker.WorkerSupportsCancellation.Should().BeTrue();
            loaderInvokedTcs
                .Task.IsCompleted.Should()
                .BeTrue("the injected RemainingEmailLoader must be invoked by the started worker");
        }

        /// <summary>
        /// Issue #244 AC3: a positive batch size must retain the pre-existing behavior — the first
        /// batch is projected through <c>GetRowsAs&lt;IEmailSortInfo&gt;()</c> and resolved to
        /// <see cref="MailItem"/> instances via <c>_olApp.GetNamespace("MAPI").GetItemFromID</c>, and
        /// the source frame drops the consumed rows. This test must pass both before and after the
        /// fix, proving the <c>batchSize &gt; 0</c> path is unchanged by the zero-batch guard. The inert
        /// <see cref="QfcDatamodel.RemainingEmailLoader"/> is assigned before the call so the worker
        /// <c>InitEmailQueue</c> starts (against the now-drained <c>_frame</c>) cannot reach the real
        /// loader and pop the "Email Frame is empty" <see cref="System.Windows.Forms.MessageBox"/> dialog.
        /// </summary>
        [TestMethod]
        public void InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            SetPrivateField(model, "_frame", CreateTwoRowEmailFrame());
            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out _);
            model.WorkerStarter = StartSynchronously;

            var mailItemsByEntryId = new Dictionary<string, MailItem>
            {
                ["EntryId-1"] = new Mock<MailItem>().Object,
                ["EntryId-2"] = new Mock<MailItem>().Object,
            };

            var nameSpace = new Mock<NameSpace>(MockBehavior.Loose);
            nameSpace
                .Setup(x => x.GetItemFromID(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string entryId, string storeId) => mailItemsByEntryId[entryId]);

            var application = new Mock<Application>(MockBehavior.Loose);
            application.Setup(x => x.GetNamespace("MAPI")).Returns(nameSpace.Object);

            SetPrivateField(model, "_olApp", application.Object);

            // Act
            var result = model.InitEmailQueue(2, new SynchronousBackgroundWorker());

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(mailItemsByEntryId.Values);

            var frameField = typeof(QfcDatamodel).GetField("_frame", NonPublicInstance);
            var frame = (Frame<int, string>)frameField.GetValue(model);
            frame.RowCount.Should().Be(0);
        }
    }
}
