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
    /// v1.1 revision (issue #244): every test below that starts a real <see cref="BackgroundWorker"/>
    /// via <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/> assigns an inert, recording
    /// <see cref="QfcDatamodel.RemainingEmailLoader"/> delegate via the internal seam BEFORE calling
    /// <c>InitEmailQueue</c>. Without this, the started worker's <c>Worker_DoWork</c> reaches the real
    /// <c>LoadRemainingEmailsToQueueAsync</c>, which pops a live <see cref="System.Windows.Forms.MessageBox"/>
    /// dialog and touches Outlook COM (<c>_olApp.GetNamespace("MAPI")</c>) — this is the maintainer-reported
    /// defect in the v1.0 revision of these tests, and this file must never reproduce it.
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
            IList<MailItem> result = null;

            // Act
            System.Action act = () => result = model.InitEmailQueue(0, new BackgroundWorker());

            // Assert
            act.Should().NotThrow();
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Issue #244 AC2: a zero batch size must still set up and start the background worker so
        /// remaining emails continue to load into the master queue. <see cref="BackgroundWorker.WorkerSupportsCancellation"/>
        /// (set synchronously by <see cref="QfcDatamodel.SetupWorker"/>) proves the worker was set up.
        /// Because <c>Worker_DoWork</c> is <c>async void</c>, <see cref="BackgroundWorker.IsBusy"/> can
        /// flip back to <see langword="false"/> almost immediately and a synchronous post-call check on
        /// it races the worker thread, so this test does not assert <c>IsBusy</c>. Instead, it proves the
        /// worker actually started and reached the injected <see cref="QfcDatamodel.RemainingEmailLoader"/>
        /// by waiting (with a bounded timeout, not a fixed sleep) on a <see cref="TaskCompletionSource{TResult}"/>
        /// that the inert loader completes.
        /// </summary>
        [TestMethod]
        public void InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            SetPrivateField(model, "_frame", CreateTwoRowEmailFrame());
            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out var loaderInvokedTcs);
            var worker = new BackgroundWorker();

            // Act
            model.InitEmailQueue(0, worker);

            // Assert
            worker.WorkerSupportsCancellation.Should().BeTrue();
            loaderInvokedTcs
                .Task.Wait(TimeSpan.FromSeconds(5))
                .Should()
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
            var result = model.InitEmailQueue(2, new BackgroundWorker());

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(mailItemsByEntryId.Values);

            var frameField = typeof(QfcDatamodel).GetField("_frame", NonPublicInstance);
            var frame = (Frame<int, string>)frameField.GetValue(model);
            frame.RowCount.Should().Be(0);
        }
    }
}
