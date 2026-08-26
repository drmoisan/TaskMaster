using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Tests for the testability seams introduced for issue #223:
    /// Seam B intent command events / skip-button state, and Seam D
    /// (<see cref="IQfcFormViewer.CaptureTlpCellStates"/> /
    /// <see cref="IQfcFormViewer.GetKeyEventExclusionControls"/>). These tests verify behavior
    /// that was previously unverifiable because the form exposed raw WinForms control types.
    /// Kept in a separate <see cref="TestClass"/> so the pre-existing
    /// <c>QfcFormControllerTests.cs</c> file is not grown further.
    /// </summary>
    [TestClass]
    public class QfcFormControllerSeamTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IQfcFormViewer> _mockFormViewer;
        private Mock<IQfcQueue> _mockQfcQueue;
        private Mock<IQfcHomeController> _mockParent;
        private Mock<IAppAutoFileObjects> _mockAF;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private QfcFormController _controller;

        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private T GetPrivateField<T>(object obj, string fieldName) =>
            (T)obj.GetType().GetField(fieldName, PrivateInstance).GetValue(obj);

        private void SetPrivateField<T>(object obj, string fieldName, T value) =>
            obj.GetType().GetField(fieldName, PrivateInstance).SetValue(obj, value);

        private static string ReadControllerSource(string fileName) =>
            File.ReadAllText(ResolveRepositoryPath("QuickFiler", "Controllers", fileName));

        private static string ResolveRepositoryPath(params string[] pathParts)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "QuickFiler")))
            {
                dir = dir.Parent;
            }

            dir.Should().NotBeNull("source-inspection tests must run under the repository");
            return pathParts.Aggregate(dir.FullName, Path.Combine);
        }

        private QfcFormController CreateQfcFormController()
        {
            return new QfcFormController(
                _mockGlobals.Object,
                _mockFormViewer.Object,
                _mockQfcQueue.Object,
                QfEnums.InitTypeEnum.Sort,
                () => { },
                _mockParent.Object,
                _tokenSource,
                _token
            );
        }

        /// <summary>
        /// Satisfies the guard in <c>RegisterFormEventHandlers</c> so the controller reaches the
        /// intent-event subscriptions: a non-null (empty) Controls collection, a keyboard handler,
        /// and an empty key-event exclusion list.
        /// </summary>
        private void SetupForRegister()
        {
            _mockFormViewer
                .SetupGet(x => x.Controls)
                .Returns(new Control.ControlCollection(new Control()));
            _mockFormViewer
                .Setup(x => x.GetKeyEventExclusionControls())
                .Returns(new List<Control>());
            _mockParent
                .SetupGet(x => x.KeyboardHandler)
                .Returns(new Mock<IQfcKeyboardHandler>().Object);
        }

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockAF = new Mock<IAppAutoFileObjects>();
            _mockGlobals.Setup(g => g.AF).Returns(_mockAF.Object);
            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockQfcQueue = new Mock<IQfcQueue>();
            _mockParent = new Mock<IQfcHomeController>();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        #region Seam B — intent command event routing

        [TestMethod]
        public void RegisterFormEventHandlers_WiresAllIntentCommandEvents()
        {
            // Arrange
            SetupForRegister();
            _controller = CreateQfcFormController();

            // Act
            _controller.RegisterFormEventHandlers();

            // Assert: every intent command event is subscribed exactly once.
            _mockFormViewer.VerifyAdd(x => x.OkClicked += It.IsAny<EventHandler>(), Times.Once);
            _mockFormViewer.VerifyAdd(x => x.CancelClicked += It.IsAny<EventHandler>(), Times.Once);
            _mockFormViewer.VerifyAdd(x => x.UndoClicked += It.IsAny<EventHandler>(), Times.Once);
            _mockFormViewer.VerifyAdd(x => x.SkipClicked += It.IsAny<EventHandler>(), Times.Once);
            _mockFormViewer.VerifyAdd(
                x => x.ItemsPerLoadValueChanged += It.IsAny<EventHandler>(),
                Times.Once
            );
        }

        [TestMethod]
        public void RegisterFormEventHandlers_UsesExclusionControlsFromFormViewer()
        {
            // Arrange
            SetupForRegister();
            _controller = CreateQfcFormController();

            // Act
            _controller.RegisterFormEventHandlers();

            // Assert: the keyboard-exclusion list is sourced from the interface seam, not raw controls.
            _mockFormViewer.Verify(x => x.GetKeyEventExclusionControls(), Times.Once);
        }

        [TestMethod]
        public void OkClicked_WhenRaised_RoutesToControllerWithoutThrowing()
        {
            // Arrange
            SetupForRegister();
            _controller = CreateQfcFormController();
            _controller.RegisterFormEventHandlers();

            // Act
            Action act = () => _mockFormViewer.Raise(x => x.OkClicked += null, EventArgs.Empty);

            // Assert: the OK intent event routes into the controller's handler without error.
            act.Should().NotThrow();
        }

        [TestMethod]
        public void CancelClicked_WhenRaised_CancelsParentTokenSource()
        {
            // Arrange
            SetupForRegister();
            using (var parentCts = new CancellationTokenSource())
            {
                _mockParent.SetupGet(p => p.TokenSource).Returns(parentCts);
                _controller = CreateQfcFormController();
                _controller.RegisterFormEventHandlers();

                // Act: raising Cancel routes to ActionCancelAsync, which cancels the parent token.
                _mockFormViewer.Raise(x => x.CancelClicked += null, EventArgs.Empty);

                // Assert
                parentCts.IsCancellationRequested.Should().BeTrue();
            }
        }

        [TestMethod]
        public void UndoClicked_WhenRaised_RoutesToControllerWithoutThrowing()
        {
            // Arrange
            SetupForRegister();
            _controller = CreateQfcFormController();
            _controller.RegisterFormEventHandlers();

            // Act
            Action act = () => _mockFormViewer.Raise(x => x.UndoClicked += null, EventArgs.Empty);

            // Assert: the Undo intent event routes into the controller's handler without error
            // (the UndoDialog guard short-circuits because no moved items exist).
            act.Should().NotThrow();
        }

        [TestMethod]
        public void ItemsPerLoadValueChanged_WhenRaised_RoutesToSpinnerHandler()
        {
            // Arrange: WorkerComplete true so the spinner handler runs without the polling delay,
            // and the value equals the current iteration count so the handler is a no-op.
            SetupForRegister();
            _mockParent.SetupGet(p => p.WorkerComplete).Returns(true);
            _mockFormViewer.SetupProperty(x => x.ItemsPerLoadValue);
            _mockFormViewer.Object.ItemsPerLoadValue = 8m;
            _controller = CreateQfcFormController();
            SetPrivateField(_controller, "_itemsPerIteration", 8);
            _controller.RegisterFormEventHandlers();

            // Act
            Action act = () =>
                _mockFormViewer.Raise(x => x.ItemsPerLoadValueChanged += null, EventArgs.Empty);

            // Assert: routes into SpnEmailPerLoadHandler; equal-count branch leaves the value unchanged.
            act.Should().NotThrow();
            ((int)_mockFormViewer.Object.ItemsPerLoadValue).Should().Be(8);
        }

        #endregion Seam B — intent command event routing

        #region Seam B — skip flow state transitions

        [TestMethod]
        public void SkipClicked_WhenRaised_TogglesSkipButtonTextAndEnabled()
        {
            // Arrange: empty queue so SkipGroupAsync completes synchronously.
            SetupForRegister();
            _mockFormViewer.SetupProperty(x => x.SkipButtonText);
            _mockFormViewer.SetupProperty(x => x.SkipButtonEnabled);
            _mockQfcQueue.SetupGet(q => q.Count).Returns(0);
            _mockQfcQueue.SetupGet(q => q.JobsRunning).Returns(0);
            _controller = CreateQfcFormController();
            _controller.RegisterFormEventHandlers();

            // Act
            _mockFormViewer.Raise(x => x.SkipClicked += null, EventArgs.Empty);

            // Assert: skip flow drives text and enabled state through the intent properties.
            _mockFormViewer.VerifySet(x => x.SkipButtonEnabled = false, Times.Once);
            _mockFormViewer.VerifySet(x => x.SkipButtonText = "Skipping...", Times.Once);
            _mockFormViewer.VerifySet(x => x.SkipButtonText = "Skip Group", Times.Once);
            _mockFormViewer.VerifySet(x => x.SkipButtonEnabled = true, Times.Once);
        }

        [TestMethod]
        public async Task ButtonSkipHandler_WhenInvoked_TogglesSkipButtonTextAndEnabled()
        {
            // Arrange: empty queue so SkipGroupAsync completes synchronously.
            _mockFormViewer.SetupProperty(x => x.SkipButtonText);
            _mockFormViewer.SetupProperty(x => x.SkipButtonEnabled);
            _mockQfcQueue.SetupGet(q => q.Count).Returns(0);
            _mockQfcQueue.SetupGet(q => q.JobsRunning).Returns(0);
            _controller = CreateQfcFormController();

            // Act
            await _controller.ButtonSkipHandler(this, EventArgs.Empty);

            // Assert
            _mockFormViewer.VerifySet(x => x.SkipButtonEnabled = false, Times.Once);
            _mockFormViewer.VerifySet(x => x.SkipButtonText = "Skipping...", Times.Once);
            _mockFormViewer.VerifySet(x => x.SkipButtonText = "Skip Group", Times.Once);
            _mockFormViewer.VerifySet(x => x.SkipButtonEnabled = true, Times.Once);
        }

        #endregion Seam B — skip flow state transitions

        #region Seam D — CaptureItemSettings via CaptureTlpCellStates

        private static TableLayoutPanel CreateTlpWithRowStyles()
        {
            var tlp = new TableLayoutPanel();
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            return tlp;
        }

        [TestMethod]
        public void CaptureItemSettings_WhenCellStatesPopulated_StoresStates()
        {
            // Arrange
            _mockFormViewer
                .SetupGet(x => x.L1v0L2L3v_TableLayout)
                .Returns(CreateTlpWithRowStyles());
            var states = new TlpCellStates();
            _mockFormViewer.Setup(x => x.CaptureTlpCellStates()).Returns(states);
            _controller = CreateQfcFormController();

            // Act
            _controller.CaptureItemSettings();

            // Assert: the populated snapshot is stored, and the form is hidden afterward.
            GetPrivateField<TlpCellStates>(_controller, "_states").Should().BeSameAs(states);
            _mockFormViewer.Verify(x => x.Hide(), Times.Once);
        }

        [TestMethod]
        public void CaptureItemSettings_WhenCellStatesNull_StoresNullAndHides()
        {
            // Arrange
            _mockFormViewer
                .SetupGet(x => x.L1v0L2L3v_TableLayout)
                .Returns(CreateTlpWithRowStyles());
            _mockFormViewer.Setup(x => x.CaptureTlpCellStates()).Returns((TlpCellStates)null);
            _controller = CreateQfcFormController();

            // Act
            _controller.CaptureItemSettings();

            // Assert: a null snapshot leaves _states null and still hides the form.
            GetPrivateField<TlpCellStates>(_controller, "_states").Should().BeNull();
            _mockFormViewer.Verify(x => x.CaptureTlpCellStates(), Times.Once);
            _mockFormViewer.Verify(x => x.Hide(), Times.Once);
        }

        [TestMethod]
        public void CaptureItemSettings_WhenRowStylesNull_ReturnsEarly()
        {
            // Arrange: null TLP means RowStyles is null, so the method returns before any snapshot.
            _mockFormViewer.SetupGet(x => x.L1v0L2L3v_TableLayout).Returns((TableLayoutPanel)null);
            _controller = CreateQfcFormController();

            // Act
            _controller.CaptureItemSettings();

            // Assert: the early return means neither the snapshot nor Show is invoked.
            _mockFormViewer.Verify(x => x.CaptureTlpCellStates(), Times.Never);
            _mockFormViewer.Verify(x => x.Show(), Times.Never);
        }

        [TestMethod]
        public void LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval()
        {
            string source = ReadControllerSource("QfcFormController.Actions.cs");
            int mailItemOverload = source.IndexOf(
                "public async Task LoadItemsAsync(IList<MailItem> listObjects, ProgressTracker progress)",
                StringComparison.Ordinal
            );
            int preScoredOverload = source.IndexOf(
                "public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored)",
                StringComparison.Ordinal
            );

            mailItemOverload.Should().BeGreaterThanOrEqualTo(0);
            preScoredOverload.Should().BeGreaterThan(mailItemOverload);
            string mailItemPath = source.Substring(
                mailItemOverload,
                preScoredOverload - mailItemOverload
            );

            mailItemPath.Should().NotContain("ApplyHighConfidenceFilterAsync");
            mailItemPath.Should().NotContain("RemoveBelowThresholdAsync");
        }

        #endregion Seam D — CaptureItemSettings via CaptureTlpCellStates

        #region Issue #448 — undo-consumer termination and idle timer

        /// <summary>A <see cref="FakeTimeProvider"/> that counts the delays it is asked for.</summary>
        private sealed class CountingTimeProvider : FakeTimeProvider
        {
            public int DelayRequests { get; private set; }

            public override ITimer CreateTimer(TimerCallback cb, object s, TimeSpan due, TimeSpan p)
            {
                DelayRequests++;
                return base.CreateTimer(cb, s, due, p);
            }
        }

        /// <summary>
        /// Runs the undo consumer inline against <paramref name="clock"/>, with a processor seam so
        /// no live COM or dispatcher call is made (UT4, D-Plan-3), and optional pre-queued items.
        /// </summary>
        private QfcFormController ArrangeUndoConsumer(
            TimeProvider clock,
            Func<IMovedMailInfo, Task> processor = null,
            int queuedItems = 0
        )
        {
            QfcFormController c = CreateQfcFormController();
            c.TimeProvider = clock;
            c.UndoConsumerStarter = body => body();
            c.UndoItemProcessor = processor ?? (_ => Task.CompletedTask);
            var q = GetPrivateField<BlockingCollection<IMovedMailInfo>>(c, "_undoQueue");
            while (queuedItems-- > 0)
            {
                q.Add(new Mock<IMovedMailInfo>().Object);
            }
            return c;
        }

        /// <summary>
        /// Issue #448. Idle iterations must wait through the injected clock, or the threshold cannot
        /// be driven. The task is deliberately not awaited: the pre-fix loop never ends (D5).
        /// </summary>
        [TestMethod]
        public void UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay()
        {
            // Arrange
            var clock = new CountingTimeProvider();
            QfcFormController controller = ArrangeUndoConsumer(clock);
            // Act — runs inline until the first idle wait, then returns.
            _ = controller.UndoConsumerStarter(controller.UndoConsumer);
            // Assert
            clock.DelayRequests.Should().BeGreaterThanOrEqualTo(1, "idle waits use the seam");
        }

        /// <summary>
        /// Issue #448. An idle consumer past the threshold must terminate; before the rewrite the
        /// exit flag fed a disjunction that kept the loop alive for the session.
        /// </summary>
        [TestMethod]
        [Timeout(10000)]
        public async Task UndoConsumer_IdleBeyondThreshold_Completes()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = ArrangeUndoConsumer(clock);
            // Act
            Task consumer = controller.UndoConsumerStarter(controller.UndoConsumer);
            clock.Advance(TimeSpan.FromSeconds(11));
            await consumer.ConfigureAwait(false);
            // Assert
            consumer.Status.Should().Be(TaskStatus.RanToCompletion, "an idle consumer must exit");
        }

        /// <summary>
        /// Issue #448. The threshold measures time since the last take, not since start. Three takes
        /// advance the clock six seconds each (eighteen in aggregate, past the ten-second threshold)
        /// while every idle gap stays at zero, so the consumer drains and then waits; a session timer
        /// exits instead, which the completion flag and the delay count detect.
        /// </summary>
        [TestMethod]
        [Timeout(10000)]
        public async Task UndoConsumer_SuccessfulTake_ResetsIdleTimer()
        {
            // Arrange — the fake processor keeps live COM and the dispatcher out (UT4).
            var clock = new CountingTimeProvider();
            var processed = new List<IMovedMailInfo>();
            QfcFormController controller = ArrangeUndoConsumer(
                clock,
                item =>
                {
                    processed.Add(item);
                    clock.Advance(TimeSpan.FromSeconds(6));
                    return Task.CompletedTask;
                },
                queuedItems: 3
            );
            // Act — drains all three takes inline, then parks on its first idle wait.
            Task consumer = controller.UndoConsumerStarter(controller.UndoConsumer);
            // Assert
            processed.Should().HaveCount(3, "18 s of takes must not end the consumer");
            consumer.IsCompleted.Should().BeFalse("the consumer parked instead of exiting");
            clock.DelayRequests.Should().Be(1, "it took the idle branch, not the exit branch");
            // Idle past the threshold measured from the last take does end it.
            clock.Advance(TimeSpan.FromSeconds(11));
            await consumer.ConfigureAwait(false);
            consumer.Status.Should().Be(TaskStatus.RanToCompletion);
        }

        /// <summary>
        /// Issue #448. Every exit path must clear <c>_undoConsumerTask</c> so a later
        /// <c>UndoDialog()</c> starts a fresh consumer. A sentinel is planted first, so a path that
        /// fails to clear the field leaves the sentinel behind and the assertion fails.
        /// </summary>
        [TestMethod]
        [Timeout(10000)]
        public async Task UndoConsumer_OnExit_ResetsUndoConsumerTask()
        {
            // Arrange — one consumer per exit path; the throwing processor stands in for the
            // exception disposing _undoQueue mid-take produces. The sentinel makes the assertion
            // real: without it the field starts null and every path would pass vacuously.
            var idleClock = new FakeTimeProvider();
            QfcFormController idle = ArrangeUndoConsumer(idleClock);
            QfcFormController bad = ArrangeUndoConsumer(
                new FakeTimeProvider(),
                _ => throw new InvalidOperationException("undo failed"),
                queuedItems: 1
            );
            SetPrivateField(idle, "_undoConsumerTask", Task.CompletedTask);
            SetPrivateField(bad, "_undoConsumerTask", Task.CompletedTask);
            // Act — the idle exit, then the exception exit.
            Task idleConsumer = idle.UndoConsumerStarter(idle.UndoConsumer);
            idleClock.Advance(TimeSpan.FromSeconds(11));
            await idleConsumer.ConfigureAwait(false);
            Func<Task> act = () => bad.UndoConsumerStarter(bad.UndoConsumer);
            await act.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);
            // Assert — both exit paths cleared the planted sentinel.
            GetPrivateField<Task>(idle, "_undoConsumerTask").Should().BeNull("idle path clears");
            GetPrivateField<Task>(bad, "_undoConsumerTask").Should().BeNull("throw path clears");
        }

        #endregion Issue #448 — undo-consumer termination and idle timer
    }
}
