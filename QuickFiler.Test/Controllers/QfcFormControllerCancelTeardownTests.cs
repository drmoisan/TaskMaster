using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #791 AC2: the ordered, logged, exception-safe Cancel teardown on
    /// <c>QfcFormController</c>.
    /// <para>
    /// Modeled on <c>QfcFormControllerDeactivateTests</c>: the viewer is a <see cref="Mock{T}"/> of
    /// <see cref="IQfcFormViewer"/>, the collection controller is injected by private-field
    /// reflection, and a <see cref="Control.ControlCollection"/> plus an empty exclusion list
    /// satisfy the guard at the top of <c>Register</c>/<c>UnregisterFormEventHandlers</c>. No window
    /// is ever shown and no WinForms handle is created, so the suite stays headless. Ordering is
    /// asserted through a shared invocation-order list populated by <c>Callback</c> handlers,
    /// comparing the first index of each marker.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcFormControllerCancelTeardownTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private const string MarkerToggleKeyboard = "toggle-keyboard";
        private const string MarkerParkFocus = "park-focus";
        private const string MarkerUnregisterNavigation = "unregister-navigation";
        private const string MarkerUnregisterFormHandlers = "unregister-form-handlers";
        private const string MarkerQuiesce = "quiesce-loader";
        private const string MarkerGroupsCleanup = "groups-cleanup";
        private const string MarkerParentCleanup = "parent-cleanup";

        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IAppAutoFileObjects> _mockAF;
        private Mock<IQfcFormViewer> _mockFormViewer;
        private Mock<IQfcQueue> _mockQfcQueue;
        private Mock<IQfcHomeController> _mockParent;
        private Mock<IQfcKeyboardHandler> _mockKeyboardHandler;
        private Mock<IQfcDatamodel> _mockDataModel;
        private CancellationTokenSource _tokenSource;
        private List<string> _order;

        [TestInitialize]
        public void Setup()
        {
            _order = new List<string>();
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockAF = new Mock<IAppAutoFileObjects>();
            _mockGlobals.Setup(g => g.AF).Returns(_mockAF.Object);
            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockQfcQueue = new Mock<IQfcQueue>();
            _mockParent = new Mock<IQfcHomeController>();
            _mockKeyboardHandler = new Mock<IQfcKeyboardHandler>();
            _mockDataModel = new Mock<IQfcDatamodel>();
            _tokenSource = new CancellationTokenSource();

            // Satisfies the guard at the top of Register/UnregisterFormEventHandlers so the
            // controller reaches the intent-event unsubscription block. The exclusion-list read is
            // the observable proof that the form handlers were unregistered.
            _mockFormViewer
                .SetupGet(x => x.Controls)
                .Returns(new Control.ControlCollection(new Control()));
            _mockFormViewer
                .Setup(x => x.GetKeyEventExclusionControls())
                .Returns(new List<Control>())
                .Callback(() => _order.Add(MarkerUnregisterFormHandlers));
            _mockFormViewer
                .Setup(x => x.ParkFocusOffWebView2())
                .Callback(() => _order.Add(MarkerParkFocus));

            _mockKeyboardHandler
                .Setup(x => x.ToggleKeyboardDialog())
                .Callback(() => _order.Add(MarkerToggleKeyboard));
            _mockParent.SetupGet(x => x.KeyboardHandler).Returns(_mockKeyboardHandler.Object);
            _mockParent.SetupGet(x => x.DataModel).Returns(_mockDataModel.Object);
            _mockParent.SetupGet(x => x.TokenSource).Returns(_tokenSource);
            _mockDataModel
                .Setup(x => x.QuiesceLoaderAsync(It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask)
                .Callback(() => _order.Add(MarkerQuiesce));
        }

        private QfcFormController CreateController(System.Action parentCleanup = null) =>
            new QfcFormController(
                _mockGlobals.Object,
                _mockFormViewer.Object,
                _mockQfcQueue.Object,
                QfEnums.InitTypeEnum.Sort,
                parentCleanup ?? (() => _order.Add(MarkerParentCleanup)),
                _mockParent.Object,
                _tokenSource,
                _tokenSource.Token
            );

        private static void SetPrivateField(object target, string fieldName, object value) =>
            target.GetType().GetField(fieldName, PrivateInstance).SetValue(target, value);

        /// <summary>
        /// Injects a collection controller whose <c>ItemGroups</c> carries one
        /// <see cref="QfcItemGroup"/> per supplied item controller, with order markers on the two
        /// members the Cancel path drives.
        /// </summary>
        private Mock<IQfcCollectionController> InjectGroups(
            QfcFormController controller,
            params Mock<IQfcItemController>[] itemControllers
        )
        {
            var groups = new List<QfcItemGroup>();
            foreach (Mock<IQfcItemController> itemController in itemControllers)
            {
                groups.Add(new QfcItemGroup { ItemController = itemController.Object });
            }

            var collection = new Mock<IQfcCollectionController>();
            collection.SetupGet(x => x.ItemGroups).Returns(groups);
            collection
                .Setup(x => x.UnregisterNavigation())
                .Callback(() => _order.Add(MarkerUnregisterNavigation));
            collection.Setup(x => x.Cleanup()).Callback(() => _order.Add(MarkerGroupsCleanup));
            SetPrivateField(controller, "_groups", collection.Object);
            return collection;
        }

        /// <summary>First index of <paramref name="marker"/>, or -1 when it never occurred.</summary>
        private int FirstIndexOf(string marker) => _order.IndexOf(marker);

        /// <summary>
        /// AC2: the keyboard-active flag is reset on the Cancel path. Left set, the Outlook keyboard
        /// stays captured after the dialog closes, which is the reported 37-minute lockout.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive()
        {
            // Arrange
            _mockKeyboardHandler.SetupGet(x => x.KbdActive).Returns(true);
            QfcFormController controller = CreateController();
            InjectGroups(controller);

            // Act
            await controller.ActionCancelAsync();

            // Assert
            _mockKeyboardHandler.Verify(
                x => x.ToggleKeyboardDialog(),
                Times.Once,
                "an active keyboard dialog must be toggled off before the form goes away"
            );
        }

        /// <summary>
        /// AC2 negative control: toggling an already-inactive keyboard dialog would turn it ON, so
        /// the reset must be conditional. Mirrors the OK path.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_DoesNotToggle_WhenInactive()
        {
            // Arrange
            _mockKeyboardHandler.SetupGet(x => x.KbdActive).Returns(false);
            QfcFormController controller = CreateController();
            InjectGroups(controller);

            // Act
            await controller.ActionCancelAsync();

            // Assert
            _mockKeyboardHandler.Verify(
                x => x.ToggleKeyboardDialog(),
                Times.Never,
                "toggling an inactive dialog would activate it, not reset it"
            );
        }

        /// <summary>
        /// AC2: WebView2 focus is parked and every open breadcrumb selector is cancelled on the
        /// Cancel path, which the #677 fix wired to <c>Form.Deactivate</c> only — an event the
        /// Cancel path itself unsubscribes.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors()
        {
            // Arrange
            _mockFormViewer.SetupGet(x => x.IsWebView2Focused).Returns(true);
            var first = new Mock<IQfcItemController>();
            var second = new Mock<IQfcItemController>();
            QfcFormController controller = CreateController();
            InjectGroups(controller, first, second);

            // Act
            await controller.ActionCancelAsync();

            // Assert
            _mockFormViewer.Verify(x => x.ParkFocusOffWebView2(), Times.Once);
            first.Verify(x => x.CancelBreadcrumbSelector(), Times.Once);
            second.Verify(x => x.CancelBreadcrumbSelector(), Times.Once);
        }

        /// <summary>
        /// AC2 ordering: navigation and form keyboard handlers are unregistered BEFORE the item rows
        /// are removed. Reversed — which is what the code did — the recursive unsubscribe no longer
        /// reaches the item controls' PreviewKeyDown/KeyDown subscriptions, because the controls are
        /// already gone.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup()
        {
            // Arrange
            QfcFormController controller = CreateController();
            InjectGroups(controller, new Mock<IQfcItemController>());

            // Act
            await controller.ActionCancelAsync();

            // Assert
            FirstIndexOf(MarkerUnregisterNavigation)
                .Should()
                .BeGreaterThanOrEqualTo(0, "the navigation ledger must be drained on Cancel");
            FirstIndexOf(MarkerGroupsCleanup)
                .Should()
                .BeGreaterThan(
                    FirstIndexOf(MarkerUnregisterNavigation),
                    "rows may only be removed after navigation is unregistered"
                );
            FirstIndexOf(MarkerGroupsCleanup)
                .Should()
                .BeGreaterThan(
                    FirstIndexOf(MarkerUnregisterFormHandlers),
                    "rows may only be removed after the form handlers are unregistered"
                );
        }

        /// <summary>
        /// AC2 ordering: the background loader is stopped and awaited before any datamodel field is
        /// nulled. A completed task is the same shape the timeout path returns, so a timed-out
        /// quiesce still proceeds through the later stages; the timeout path itself is pinned
        /// independently by <c>QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs</c>.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup()
        {
            // Arrange
            QfcFormController controller = CreateController();
            InjectGroups(controller, new Mock<IQfcItemController>());

            // Act
            await controller.ActionCancelAsync();

            // Assert
            _mockDataModel.Verify(x => x.QuiesceLoaderAsync(It.IsAny<TimeSpan>()), Times.Once);
            FirstIndexOf(MarkerGroupsCleanup)
                .Should()
                .BeGreaterThan(
                    FirstIndexOf(MarkerQuiesce),
                    "the loader must be quiesced before the rows and fields are released"
                );
            FirstIndexOf(MarkerParentCleanup)
                .Should()
                .BeGreaterThan(
                    FirstIndexOf(MarkerGroupsCleanup),
                    "a completed quiesce must not short-circuit the remaining stages"
                );
        }

        /// <summary>
        /// AC2: the ribbon release callback runs even when an earlier teardown stage throws. Without
        /// it, <c>RibbonController.ReleaseQuickFiler</c> never runs and both ribbon buttons become
        /// no-ops for the rest of the Outlook session.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup()
        {
            // Arrange
            QfcFormController controller = CreateController();
            Mock<IQfcCollectionController> groups = InjectGroups(controller);
            groups
                .Setup(x => x.Cleanup())
                .Callback(() => _order.Add(MarkerGroupsCleanup))
                .Throws(new InvalidOperationException("groups cleanup failed"));

            // Act
            Func<Task> act = () => controller.ActionCancelAsync();

            // Assert
            await act.Should().NotThrowAsync("a failing stage must not abort the teardown");
            _order
                .Should()
                .Contain(
                    MarkerParentCleanup,
                    "the release callback runs under finally, whichever stage threw"
                );
        }

        /// <summary>
        /// AC2: <c>ButtonCancel_Click</c> is <c>async void</c>, so a rethrown exception becomes an
        /// unhandled Outlook UI-thread failure rather than anything an operator can act on. The
        /// throw is raised from the handler's own body by nulling the private <c>_formViewer</c>
        /// field, so the <c>SetSynchronizationContext</c> call at the top of the handler raises
        /// <see cref="NullReferenceException"/> inside its own <c>try</c>.
        /// <para>
        /// An <c>async void</c> escape is posted to the captured
        /// <see cref="SynchronizationContext"/> rather than propagated to the caller, so a capturing
        /// context is installed for the call: asserting that nothing was posted is the only way to
        /// observe the rethrow, and it is what makes this test false before the fix.
        /// </para>
        /// </summary>
        [TestMethod]
        public void ButtonCancel_Click_ActionThrows_DoesNotRethrow()
        {
            // Arrange
            QfcFormController controller = CreateController();
            InjectGroups(controller);
            SetPrivateField(controller, "_formViewer", null);
            SynchronizationContext previous = SynchronizationContext.Current;
            var capturing = new CapturingSynchronizationContext();

            // Act
            try
            {
                SynchronizationContext.SetSynchronizationContext(capturing);
                controller.ButtonCancel_Click(this, EventArgs.Empty);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }

            // Assert
            capturing
                .Captured.Should()
                .BeEmpty(
                    "a teardown failure must be logged, not rethrown into the Outlook UI thread"
                );
        }

        /// <summary>
        /// AC2: repeat invocation is inert. Double Cancel, or a Cancel after the MoveAndIterate
        /// completion path (which calls the same method), must not invoke the ribbon release
        /// callback twice and must not throw. Repeat invocation is inert by construction rather than
        /// by a flag: the first pass nulls the fields the second pass would use.
        /// </summary>
        [TestMethod]
        public async Task ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce()
        {
            // Arrange
            QfcFormController controller = CreateController();
            InjectGroups(controller, new Mock<IQfcItemController>());

            // Act
            await controller.ActionCancelAsync();
            Func<Task> second = () => controller.ActionCancelAsync();

            // Assert
            await second.Should().NotThrowAsync("a second Cancel must be inert, not a fault");
            _order
                .FindAll(marker => marker == MarkerParentCleanup)
                .Should()
                .ContainSingle("the ribbon release callback must run exactly once");
        }

        /// <summary>
        /// Captures anything posted or sent to it instead of letting it reach the thread pool, which
        /// is where an <c>async void</c> escape would otherwise surface as an unobserved crash.
        /// </summary>
        private sealed class CapturingSynchronizationContext : SynchronizationContext
        {
            public List<Exception> Captured { get; } = new List<Exception>();

            public override void Post(SendOrPostCallback d, object state) => Run(d, state);

            public override void Send(SendOrPostCallback d, object state) => Run(d, state);

            private void Run(SendOrPostCallback d, object state)
            {
                try
                {
                    d(state);
                }
                catch (Exception exception)
                {
                    Captured.Add(exception);
                }
            }
        }
    }
}
