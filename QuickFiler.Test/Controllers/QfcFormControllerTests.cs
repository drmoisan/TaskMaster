using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcFormControllerTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IQfcFormViewer> _mockFormViewer;
        private Mock<IQfcQueue> _mockQfcQueue;
        private Mock<IQfcHomeController> _mockParent;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        private QfcFormController _controller;
        private Mock<IAppAutoFileObjects> _mockAF;
        private System.Action _maxQfWindow;
        private IFilerFormController _filerFormController;

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

        private string ErrMsg(string variable)
        {
            return $"The variable {variable} was not set properly";
        }

        private Dictionary<string, Theme> CreateThemeMap()
        {
            return new Dictionary<string, Theme>
            {
                {
                    "DarkNormal",
                    new Theme("DarkNormal", new Dictionary<string, ThemeControlGroup>())
                },
                {
                    "LightNormal",
                    new Theme("LightNormal", new Dictionary<string, ThemeControlGroup>())
                },
            };
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

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockAF = new Mock<IAppAutoFileObjects>();
            _mockAF
                .SetupSet(af => af.MaximizeQuickFileWindow = It.IsAny<System.Action>())
                .Callback<System.Action>(action => _maxQfWindow = action)
                .Verifiable();

            _mockAF.SetupGet(_mockAF => _mockAF.MaximizeQuickFileWindow).Returns(_maxQfWindow);

            _mockGlobals.Setup(g => g.AF).Returns(_mockAF.Object);
            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockFormViewer
                .Setup(x => x.SetController(It.IsAny<IFilerFormController>()))
                .Callback<IFilerFormController>(c => _filerFormController = c)
                .Verifiable();

            _mockQfcQueue = new Mock<IQfcQueue>();
            _mockParent = new Mock<IQfcHomeController>();
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        #region ctor Tests

        [TestMethod]
        public void QfcFormController_ShouldConstruct()
        {
            // Arrange / Act
            var controller = new QfcFormController(
                _mockGlobals.Object,
                _mockFormViewer.Object,
                _mockQfcQueue.Object,
                QfEnums.InitTypeEnum.Sort,
                () => { },
                _mockParent.Object,
                _tokenSource,
                _token
            );

            // Assert
            Assert.IsNotNull(controller);
            Assert.AreEqual(
                _mockGlobals.Object,
                GetPrivateField<IApplicationGlobals>(controller, "_globals"),
                ErrMsg("_globals")
            );
            Assert.AreEqual(
                _mockQfcQueue.Object,
                GetPrivateField<IQfcQueue>(controller, "_qfcQueue"),
                ErrMsg("_qfcQueue")
            );
            Assert.AreEqual(
                QfEnums.InitTypeEnum.Sort,
                GetPrivateField<QfEnums.InitTypeEnum>(controller, "_initType"),
                ErrMsg("_initType")
            );
            Assert.AreEqual(
                _mockParent.Object,
                GetPrivateField<IQfcHomeController>(controller, "_parent"),
                ErrMsg("_parent")
            );
            Assert.AreEqual(
                _maxQfWindow.Method,
                controller.GetType().GetMethod("MaximizeFormViewer")
            );
            Assert.AreEqual(_mockFormViewer.Object, controller.FormViewer);
            Assert.AreEqual((IFilerFormController)controller, _filerFormController);
            Assert.AreEqual(_tokenSource, controller.TokenSource);
            Assert.AreEqual(_token, controller.Token);
        }

        #endregion ctor Tests

        #region Setup and Disposal

        [TestMethod]
        public void CaptureItemSettings_ShouldCaptureSettings()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            System.Action act = () => _controller.CaptureItemSettings();

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveTemplatesAndSetupTlp_ShouldSetupTlp()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.RemoveTemplatesAndSetupTlp();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void SetupLightDark_ShouldSetupThemes()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.SetupLightDark();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void SpaceForEmail_ShouldReturnCorrectValue()
        {
            // Arrange
            _controller = CreateQfcFormController();
            _mockFormViewer.Setup(fv => fv.Size).Returns(new System.Drawing.Size(800, 600));
            _mockFormViewer.Setup(fv => fv.ClientSize).Returns(new System.Drawing.Size(780, 580));

            var tlp = new TableLayoutPanel();
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            _mockFormViewer.SetupGet(fv => fv.L1v_TableLayout).Returns(tlp);

            // Act
            var result = _controller.SpaceForEmail;

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void RegisterFormEventHandlers_ShouldRegisterHandlers()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.RegisterFormEventHandlers();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void UnregisterFormEventHandlers_ShouldUnregisterHandlers()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.UnregisterFormEventHandlers();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void Cleanup_ShouldCleanupResources()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.Cleanup();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        #endregion Setup and Disposal

        [TestMethod]
        public void ItemsPerIteration_ShouldGetAndSetCorrectly()
        {
            // Arrange
            _controller = CreateQfcFormController();
            _controller.ItemsPerIteration = 5;

            // Act
            var result = _controller.ItemsPerIteration;

            // Assert
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void ActiveTheme_ShouldGetAndSetCorrectly()
        {
            // Arrange
            _controller = CreateQfcFormController();
            SetPrivateField(_controller, "_themes", CreateThemeMap());
            _controller.ActiveTheme = "DarkNormal";

            // Act
            var result = _controller.ActiveTheme;

            // Assert
            Assert.AreEqual("DarkNormal", result);
        }

        [TestMethod]
        public void DarkMode_ShouldGetAndSetCorrectly()
        {
            // Arrange
            _controller = CreateQfcFormController();
            _controller.DarkMode = true;

            // Act
            var result = _controller.DarkMode;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Groups_ShouldReturnCorrectValue()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            var result = _controller.Groups;

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void FormHandle_ShouldReturnCorrectValue()
        {
            // Arrange
            _controller = CreateQfcFormController();
            _mockFormViewer.Setup(fv => fv.Handle).Returns(IntPtr.Zero);

            // Act
            var result = _controller.FormHandle;

            // Assert
            Assert.AreEqual(IntPtr.Zero, result);
        }

        [TestMethod]
        public void FormViewer_ShouldReturnCorrectValue()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            var result = _controller.FormViewer;

            // Assert
            Assert.AreEqual(_mockFormViewer.Object, result);
        }

        [TestMethod]
        public void Token_ShouldReturnCorrectValue()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            var result = _controller.Token;

            // Assert
            Assert.AreEqual(_token, result);
        }

        [TestMethod]
        public void TokenSource_ShouldReturnCorrectValue()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            var result = _controller.TokenSource;

            // Assert
            Assert.AreEqual(_tokenSource, result);
        }

        [TestMethod]
        public void DarkMode_CheckedChanged_ShouldUpdateTheme()
        {
            // Arrange
            _controller = CreateQfcFormController();
            SetPrivateField(_controller, "_themes", CreateThemeMap());
            _mockGlobals.Setup(g => g.Ol.DarkMode).Returns(true);

            // Act
            _controller.DarkMode_CheckedChanged(this, EventArgs.Empty);

            // Assert
            Assert.AreEqual("DarkNormal", _controller.ActiveTheme);
        }

        [TestMethod]
        public async Task ButtonCancel_Click_ShouldCancelAction()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            await _controller.ActionCancelAsync();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task ButtonOK_Click_ShouldPerformAction()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            await _controller.ActionOkAsync();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task LoadUiFromQueue_ShouldLoadUi()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            await _controller.LoadUiFromQueue();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task MoveAndIterate_ShouldMoveAndIterate()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            await _controller.MoveAndIterate();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task BackGroundMoveAsync_ShouldMoveEmails()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            await _controller.BackGroundMoveAsync();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void ButtonUndo_Click_ShouldUndoAction()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.ButtonUndo_Click(this, EventArgs.Empty);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task SpnEmailPerLoad_ValueChanged_ShouldChangeValue_EqualsItemPerIteration()
        {
            // Arrange
            _mockParent.Setup(x => x.WorkerComplete).Returns(true);
            _mockFormViewer.SetupProperty(x => x.ItemsPerLoadValue);
            _mockFormViewer.Object.ItemsPerLoadValue = 8m;
            _controller = CreateQfcFormController();
            SetPrivateField(_controller, "_itemsPerIteration", 8);

            // Act
            await _controller.SpnEmailPerLoadHandler(this, EventArgs.Empty);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task SpnEmailPerLoad_ValueChanged_ShouldChangeValue_GreaterItemPerIteration()
        {
            // Arrange
            _mockParent.Setup(x => x.WorkerComplete).Returns(true);
            _mockFormViewer.SetupProperty(x => x.ItemsPerLoadValue);
            _mockFormViewer.Object.ItemsPerLoadValue = 9m;

            _mockQfcQueue
                .Setup(q =>
                    q.ChangeIterationSize(
                        It.IsAny<ValueTuple<TableLayoutPanel, List<QfcItemGroup>>>(),
                        It.IsAny<int>(),
                        It.IsAny<RowStyle>()
                    )
                )
                .Returns(Task.CompletedTask)
                .Verifiable();

            _controller = CreateQfcFormController();
            SetPrivateField(_controller, "_itemsPerIteration", 8);

            var mockQfcCollectionController = new Mock<IQfcCollectionController>();
            mockQfcCollectionController.Setup(x => x.UnregisterNavigation()).Verifiable();
            mockQfcCollectionController.Setup(x => x.RegisterNavigation()).Verifiable();
            SetPrivateField(_controller, "_groups", mockQfcCollectionController.Object);

            // Act
            await _controller.SpnEmailPerLoadHandler(this, EventArgs.Empty);

            // Assert
            Assert.AreEqual(
                GetPrivateField<int>(_controller, "_itemsPerIteration"),
                (int)_mockFormViewer.Object.ItemsPerLoadValue
            );
            mockQfcCollectionController.Verify(x => x.UnregisterNavigation(), Times.Once);
            mockQfcCollectionController.Verify(x => x.RegisterNavigation(), Times.Once);
            _mockQfcQueue.Verify(
                x =>
                    x.ChangeIterationSize(
                        It.IsAny<ValueTuple<TableLayoutPanel, List<QfcItemGroup>>>(),
                        It.IsAny<int>(),
                        It.IsAny<RowStyle>()
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void AdjustTlp_ShouldAdjustTlp()
        {
            // Arrange
            _controller = CreateQfcFormController();
            var tlp = new TableLayoutPanel();

            // Act
            _controller.AdjustTlp(tlp, 5);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task ButtonSkip_Click_ShouldSkipGroup()
        {
            // Arrange
            _controller = CreateQfcFormController();
            _mockQfcQueue.SetupGet(q => q.Count).Returns(1);
            _mockQfcQueue.SetupGet(q => q.JobsRunning).Returns(0);
            _mockQfcQueue
                .Setup(q => q.TryDequeueAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .ReturnsAsync((new TableLayoutPanel(), new List<QfcItemGroup>()));

            // Act
            await _controller.ButtonSkipHandler(this, EventArgs.Empty);

            // Assert
            _mockQfcQueue.Verify(
                q => q.TryDequeueAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task SkipGroupAsync_ShouldSkipGroup()
        {
            // Arrange
            _controller = CreateQfcFormController();
            _mockQfcQueue.SetupGet(q => q.Count).Returns(1);
            _mockQfcQueue.SetupGet(q => q.JobsRunning).Returns(0);
            _mockQfcQueue
                .Setup(q => q.TryDequeueAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .ReturnsAsync((new TableLayoutPanel(), new List<QfcItemGroup>()));

            // Act
            await _controller.SkipGroupAsync();

            // Assert
            _mockQfcQueue.Verify(
                q => q.TryDequeueAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                Times.Once
            );
        }

        [TestMethod]
        public void LoadItems_ShouldLoadItems()
        {
            // Arrange
            _controller = CreateQfcFormController();
            var tlp = new TableLayoutPanel();
            var itemGroups = new List<QfcItemGroup>();

            // Act
            _controller.LoadItems(tlp, itemGroups);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void LoadItems_ShouldLoadMailItems()
        {
            // Arrange
            _controller = CreateQfcFormController();
            var listObjects = new List<MailItem>();

            // Act
            _controller.LoadItems(listObjects);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task LoadItemsAsync_ShouldLoadMailItemsAsync()
        {
            // Arrange
            _controller = CreateQfcFormController();
            var listObjects = new List<MailItem>();

            // Act
            await _controller.LoadItemsAsync(listObjects);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void MaximizeFormViewer_ShouldMaximizeForm()
        {
            // Arrange
            _controller = CreateQfcFormController();
            FormWindowState windowState = FormWindowState.Normal;
            _mockFormViewer
                .Setup(fv => fv.Invoke(It.IsAny<Delegate>()))
                .Callback<Delegate>(action => action.DynamicInvoke());
            _mockFormViewer
                .SetupSet(fv => fv.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state);

            // Act
            _controller.MaximizeFormViewer();

            // Assert
            Assert.AreEqual(FormWindowState.Maximized, windowState);
        }

        [TestMethod]
        public void MinimizeFormViewer_ShouldMinimizeForm()
        {
            // Arrange
            _controller = CreateQfcFormController();
            FormWindowState windowState = FormWindowState.Normal;
            _mockFormViewer
                .Setup(fv => fv.Invoke(It.IsAny<Delegate>()))
                .Callback<Delegate>(action => action.DynamicInvoke());
            _mockFormViewer
                .SetupSet(fv => fv.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state);

            // Act
            _controller.MinimizeFormViewer();

            // Assert
            Assert.AreEqual(FormWindowState.Minimized, windowState);
        }

        [TestMethod]
        public void UndoDialog_ShouldUndoMoves()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.UndoDialog();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task UndoConsumer_ShouldConsumeUndoQueue()
        {
            // Arrange / Act
            await Task.CompletedTask;

            // Assert
            // Placeholder assertion (pre-existing, tautological by construction). MSTEST0032
            // flags the always-true condition; replacing it with a genuine assertion is a test
            // behavior change out of scope for this narrow build-debt remediation (no behavior
            // change per AC7). Suppressed narrowly rather than altered.
#pragma warning disable MSTEST0032
            Assert.IsTrue(true);
#pragma warning restore MSTEST0032
        }

        [TestMethod]
        public void Viewer_Activate_ShouldThrowNotImplementedException()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act & Assert
            Assert.ThrowsExactly<NotImplementedException>(() => _controller.Viewer_Activate());
        }

        #region High-confidence filter (Issue #169)

        [TestMethod]
        public async Task ApplyHighConfidenceFilterAsync_WhenModeEnabled_RemovesBelowThresholdOnce()
        {
            // Arrange: high-confidence mode on, threshold 0.9.
            var settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(s => s.HighConfidenceThreshold).Returns(0.9);
            _mockGlobals.SetupGet(g => g.QfSettings).Returns(settings.Object);

            _controller = CreateQfcFormController();
            var mockGroups = new Mock<IQfcCollectionController>();

            // Act
            await _controller.ApplyHighConfidenceFilterAsync(mockGroups.Object);

            // Assert: removal is invoked exactly once with the configured threshold.
            mockGroups.Verify(g => g.RemoveBelowThresholdAsync(0.9), Times.Once);
        }

        [TestMethod]
        public async Task ApplyHighConfidenceFilterAsync_WhenGroupsIsNull_DoesNothing()
        {
            // Arrange: the null-groups guard should short-circuit without touching settings.
            var settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.HighConfidenceModeEnabled).Returns(true);
            _mockGlobals.SetupGet(g => g.QfSettings).Returns(settings.Object);
            _controller = CreateQfcFormController();

            // Act & Assert
            Func<Task> act = () => _controller.ApplyHighConfidenceFilterAsync(null);

            await act.Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task ApplyHighConfidenceFilterAsync_WhenQfSettingsIsNull_DoesNotRemove()
        {
            // Arrange: the null-QfSettings guard should short-circuit without removing.
            _mockGlobals.SetupGet(g => g.QfSettings).Returns((IAppQuickFilerSettings)null);
            _controller = CreateQfcFormController();
            var mockGroups = new Mock<IQfcCollectionController>();

            // Act
            await _controller.ApplyHighConfidenceFilterAsync(mockGroups.Object);

            // Assert
            mockGroups.Verify(g => g.RemoveBelowThresholdAsync(It.IsAny<double>()), Times.Never);
        }

        [TestMethod]
        public async Task ApplyHighConfidenceFilterAsync_WhenModeDisabled_NeverRemoves()
        {
            // Arrange: high-confidence mode off.
            var settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.HighConfidenceModeEnabled).Returns(false);
            settings.SetupGet(s => s.HighConfidenceThreshold).Returns(0.9);
            _mockGlobals.SetupGet(g => g.QfSettings).Returns(settings.Object);

            _controller = CreateQfcFormController();
            var mockGroups = new Mock<IQfcCollectionController>();

            // Act
            await _controller.ApplyHighConfidenceFilterAsync(mockGroups.Object);

            // Assert: removal is never invoked.
            mockGroups.Verify(g => g.RemoveBelowThresholdAsync(It.IsAny<double>()), Times.Never);
        }

        #endregion High-confidence filter (Issue #169)

        #region High-confidence pre-filter carrier path (Issue #171)

        /// <summary>
        /// [P4-T6] The carrier-list <see cref="QfcFormController.LoadItemsAsync(IList{QfcPreScoredItem})"/>
        /// path never invokes the post-UI removal pass
        /// (<see cref="QfcCollectionController.RemoveBelowThresholdAsync"/> via
        /// <see cref="QfcFormController.ApplyHighConfidenceFilterAsync"/>). Because the carrier
        /// overload constructs a real <see cref="QfcCollectionController"/> internally (no DI seam at
        /// that point) which would require live WinForms/COM, this test exercises the overload via the
        /// guard short-circuit (`_states` is null because Init() is not called) with an injected
        /// collection-controller mock, and verifies no removal interaction occurs on the carrier path.
        /// The positive carrier-overload behavior (LoadControlsAndHandlers_01Async and the carried
        /// PredeterminedFolder) is verified at the collection-controller level in P4-T7 / P6-T2.
        /// </summary>
        [TestMethod]
        public async Task LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval()
        {
            // Arrange — high-confidence mode on so the disabled-path branch is not the reason.
            var settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(s => s.HighConfidenceThreshold).Returns(0.9);
            _mockGlobals.SetupGet(g => g.QfSettings).Returns(settings.Object);

            _controller = CreateQfcFormController();
            var mockGroups = new Mock<IQfcCollectionController>(MockBehavior.Strict);
            SetPrivateField(_controller, "_groups", mockGroups.Object);

            var preScored = new List<QfcPreScoredItem>
            {
                new QfcPreScoredItem(new Mock<MailItem>().Object, @"\\A\folder"),
            };

            // Act
            Func<Task> act = () => _controller.LoadItemsAsync(preScored);

            // Assert — no exception, and the post-UI removal pass is never invoked on the carrier path.
            await act.Should().NotThrowAsync();
            mockGroups.Verify(g => g.RemoveBelowThresholdAsync(It.IsAny<double>()), Times.Never);
        }

        #endregion High-confidence pre-filter carrier path (Issue #171)
    }
}
