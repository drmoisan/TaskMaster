using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
    /// Issue #677: the <c>Form.Deactivate</c>-routed focus-parking and selector-cancel handler.
    /// <para>
    /// Modeled on <c>QfcFormControllerSeamTests</c>: the viewer is a <see cref="Mock{T}"/> of
    /// <see cref="IQfcFormViewer"/>, the collection controller is injected by private-field
    /// reflection, and the deactivate event is delivered with <c>Mock.Raise</c>. No window is ever
    /// shown and no WinForms handle is created, so the suite stays headless.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcFormControllerDeactivateTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IAppAutoFileObjects> _mockAF;
        private Mock<IQfcFormViewer> _mockFormViewer;
        private Mock<IQfcQueue> _mockQfcQueue;
        private Mock<IQfcHomeController> _mockParent;
        private CancellationTokenSource _tokenSource;

        [TestInitialize]
        public void Setup()
        {
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockAF = new Mock<IAppAutoFileObjects>();
            _mockGlobals.Setup(g => g.AF).Returns(_mockAF.Object);
            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockQfcQueue = new Mock<IQfcQueue>();
            _mockParent = new Mock<IQfcHomeController>();
            _tokenSource = new CancellationTokenSource();

            // Satisfies the guard at the top of Register/UnregisterFormEventHandlers so the
            // controller reaches the intent-event subscription block.
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

        private QfcFormController CreateController() =>
            new QfcFormController(
                _mockGlobals.Object,
                _mockFormViewer.Object,
                _mockQfcQueue.Object,
                QfEnums.InitTypeEnum.Sort,
                () => { },
                _mockParent.Object,
                _tokenSource,
                _tokenSource.Token
            );

        private static void SetPrivateField(object target, string fieldName, object value) =>
            target.GetType().GetField(fieldName, PrivateInstance).SetValue(target, value);

        /// <summary>
        /// Injects a collection controller whose <c>ItemGroups</c> carries one
        /// <see cref="QfcItemGroup"/> per supplied item controller.
        /// </summary>
        private void InjectGroups(
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
            SetPrivateField(controller, "_groups", collection.Object);
        }

        /// <summary>The deactivate handler must be wired when the form event handlers register.</summary>
        [TestMethod]
        public void RegisterFormEventHandlers_SubscribesFormDeactivated()
        {
            // Arrange
            QfcFormController controller = CreateController();

            // Act
            controller.RegisterFormEventHandlers();

            // Assert
            _mockFormViewer.VerifyAdd(
                x => x.FormDeactivated += It.IsAny<EventHandler>(),
                Times.Once
            );
        }

        /// <summary>The deactivate handler must be released when the form event handlers unregister.</summary>
        [TestMethod]
        public void UnregisterFormEventHandlers_UnsubscribesFormDeactivated()
        {
            // Arrange
            QfcFormController controller = CreateController();
            controller.RegisterFormEventHandlers();

            // Act
            controller.UnregisterFormEventHandlers();

            // Assert
            _mockFormViewer.VerifyRemove(
                x => x.FormDeactivated -= It.IsAny<EventHandler>(),
                Times.Once
            );
        }

        /// <summary>
        /// When a WebView2 holds focus at deactivation, focus is parked exactly once on a benign
        /// non-WebView2 control (the mitigation for WebView2Feedback #951).
        /// </summary>
        [TestMethod]
        public void FormDeactivated_WebView2Focused_ParksFocusOnce()
        {
            // Arrange
            _mockFormViewer.SetupGet(x => x.IsWebView2Focused).Returns(true);
            QfcFormController controller = CreateController();
            controller.RegisterFormEventHandlers();

            // Act
            _mockFormViewer.Raise(x => x.FormDeactivated += null, EventArgs.Empty);

            // Assert
            _mockFormViewer.Verify(x => x.ParkFocusOffWebView2(), Times.Once());
        }

        /// <summary>
        /// When no WebView2 holds focus, the handler leaves focus alone: parking on a control that
        /// is not the problem would break the in-form Escape/commit behavior of issues #438/#400.
        /// </summary>
        [TestMethod]
        public void FormDeactivated_NoWebView2Focus_DoesNotPark()
        {
            // Arrange
            _mockFormViewer.SetupGet(x => x.IsWebView2Focused).Returns(false);
            QfcFormController controller = CreateController();
            controller.RegisterFormEventHandlers();

            // Act
            _mockFormViewer.Raise(x => x.FormDeactivated += null, EventArgs.Empty);

            // Assert
            _mockFormViewer.Verify(x => x.ParkFocusOffWebView2(), Times.Never());
        }

        /// <summary>
        /// Every item's breadcrumb selector is cancelled, so no open <c>ToolStripDropDown</c> — and
        /// therefore no WinForms modal-menu-mode message filter — can outlive deactivation.
        /// </summary>
        [TestMethod]
        public void FormDeactivated_CancelsSelectorOnEveryItemController()
        {
            // Arrange
            var first = new Mock<IQfcItemController>();
            var second = new Mock<IQfcItemController>();
            QfcFormController controller = CreateController();
            InjectGroups(controller, first, second);
            controller.RegisterFormEventHandlers();

            // Act
            _mockFormViewer.Raise(x => x.FormDeactivated += null, EventArgs.Empty);

            // Assert
            first.Verify(x => x.CancelBreadcrumbSelector(), Times.Once());
            second.Verify(x => x.CancelBreadcrumbSelector(), Times.Once());
        }

        /// <summary>
        /// The handler is null-safe over both the collection controller and its item-group list,
        /// which are both null before items are loaded.
        /// </summary>
        [TestMethod]
        public void FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow()
        {
            // Arrange — first with a null collection controller.
            QfcFormController controller = CreateController();
            SetPrivateField(controller, "_groups", null);
            controller.RegisterFormEventHandlers();

            // Act
            Action nullGroups = () =>
                _mockFormViewer.Raise(x => x.FormDeactivated += null, EventArgs.Empty);

            // Arrange — then with a collection controller whose ItemGroups is null.
            var collection = new Mock<IQfcCollectionController>();
            collection.SetupGet(x => x.ItemGroups).Returns((List<QfcItemGroup>)null);

            // Act
            Action nullItemGroups = () =>
            {
                SetPrivateField(controller, "_groups", collection.Object);
                _mockFormViewer.Raise(x => x.FormDeactivated += null, EventArgs.Empty);
            };

            // Assert
            nullGroups.Should().NotThrow();
            nullItemGroups.Should().NotThrow();
        }

        /// <summary>
        /// A failing per-item cancel must neither escape the deactivate handler (a WinForms event
        /// handler that throws would surface as an unhandled UI exception) nor stop the remaining
        /// items from being cancelled.
        /// </summary>
        [TestMethod]
        public void FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues()
        {
            // Arrange
            var failing = new Mock<IQfcItemController>();
            failing
                .Setup(x => x.CancelBreadcrumbSelector())
                .Throws(new InvalidOperationException("cancel failed"));
            var second = new Mock<IQfcItemController>();
            QfcFormController controller = CreateController();
            InjectGroups(controller, failing, second);
            controller.RegisterFormEventHandlers();

            // Act
            Action act = () =>
                _mockFormViewer.Raise(x => x.FormDeactivated += null, EventArgs.Empty);

            // Assert
            act.Should().NotThrow();
            second.Verify(x => x.CancelBreadcrumbSelector(), Times.Once());
        }
    }
}
