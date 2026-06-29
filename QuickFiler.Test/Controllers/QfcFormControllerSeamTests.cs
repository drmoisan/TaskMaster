using System;
using System.Collections.Generic;
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

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType()
                .GetField(
                    fieldName,
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
            return (T)field.GetValue(obj);
        }

        private void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType()
                .GetField(
                    fieldName,
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
            field.SetValue(obj, value);
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

        #endregion Seam D — CaptureItemSettings via CaptureTlpCellStates
    }
}
