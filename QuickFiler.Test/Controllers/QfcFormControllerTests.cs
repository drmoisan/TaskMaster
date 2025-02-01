using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Interfaces.IWinForm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;

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
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }

        private void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private string ErrMsg(string variable)
        {
            return $"The variable {variable} was not set properly";
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
                _token);
        }

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockAF = new Mock<IAppAutoFileObjects>();
            _mockAF.SetupSet(af => af.MaximizeQuickFileWindow = It.IsAny<System.Action>())
                .Callback<System.Action>(action => _maxQfWindow = action)
                .Verifiable();            

            _mockAF.SetupGet(_mockAF => _mockAF.MaximizeQuickFileWindow).Returns(_maxQfWindow);

            _mockGlobals.Setup(g => g.AF).Returns(_mockAF.Object);
            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockFormViewer.Setup(x => x.SetController(It.IsAny<IFilerFormController>()))
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
                _token);

            
            // Assert
            Assert.IsNotNull(controller);
            Assert.AreEqual(_mockGlobals.Object, GetPrivateField<IApplicationGlobals>(controller, "_globals"), ErrMsg("_globals"));
            Assert.AreEqual(_mockQfcQueue.Object, GetPrivateField<IQfcQueue>(controller, "_qfcQueue"), ErrMsg("_qfcQueue"));
            Assert.AreEqual(QfEnums.InitTypeEnum.Sort, GetPrivateField<QfEnums.InitTypeEnum>(controller, "_initType"), ErrMsg("_initType"));            
            Assert.AreEqual(_mockParent.Object, GetPrivateField<IQfcHomeController>(controller, "_parent"), ErrMsg("_parent"));
            Assert.AreEqual(_maxQfWindow.Method, controller.GetType().GetMethod("MaximizeFormViewer"));
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
            _mockFormViewer.Setup(fv => fv.Show());
            _mockFormViewer.Setup(fv => fv.Hide());

            // Act
            _controller.CaptureItemSettings();

            // Assert
            _mockFormViewer.Verify(fv => fv.Show(), Times.Once);
            _mockFormViewer.Verify(fv => fv.Hide(), Times.Once);
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
            _mockFormViewer.Setup(fv => fv.GetScreen()).Returns(Screen.PrimaryScreen);

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
            var spn = new NumericUpDown();
            spn.Value = 8;
            _mockFormViewer.SetupGet(x => x.L1v1L2h5_SpnEmailPerLoad).Returns(spn);
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
            var spn = new NumericUpDown();
            spn.Value = 9;
            _mockFormViewer.SetupGet(x => x.L1v1L2h5_SpnEmailPerLoad).Returns(spn);
            
            _mockQfcQueue.Setup(q => q.ChangeIterationSize(
                It.IsAny<ValueTuple<TableLayoutPanel, List<QfcItemGroup>>>(),
                It.IsAny<int>(), 
                It.IsAny<RowStyle>()))
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
            Assert.AreEqual(GetPrivateField<int>(_controller, "_itemsPerIteration"), (int)spn.Value);
            mockQfcCollectionController.Verify(x => x.UnregisterNavigation(), Times.Once);
            mockQfcCollectionController.Verify(x => x.RegisterNavigation(), Times.Once);
            _mockQfcQueue.Verify(x => x.ChangeIterationSize(
                It.IsAny<ValueTuple<TableLayoutPanel, List<QfcItemGroup>>>(),
                It.IsAny<int>(),
                It.IsAny<RowStyle>()), Times.Once);
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

            // Act
            await _controller.ButtonSkipHandler(this, EventArgs.Empty);

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public async Task SkipGroupAsync_ShouldSkipGroup()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            await _controller.SkipGroupAsync();

            // Assert
            // Add assertions based on the expected behavior of the method
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

            // Act
            _controller.MaximizeFormViewer();

            // Assert
            _mockFormViewer.Verify(fv => fv.WindowState == FormWindowState.Maximized, Times.Once);
        }

        [TestMethod]
        public void MinimizeFormViewer_ShouldMinimizeForm()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act
            _controller.MinimizeFormViewer();

            // Assert
            _mockFormViewer.Verify(fv => fv.WindowState == FormWindowState.Minimized, Times.Once);
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
            await Task.CompletedTask;
            throw new NotImplementedException();
            // If not properly designed, this method will cause the test runner to
            // permanently lock up a process and crash. This prevents recompiling
            // until the computer is restarted.

            //// Arrange
            //_controller = CreateQfcFormController();

            //// Act
            //await _controller.UndoConsumer();

            // Assert
            // Add assertions based on the expected behavior of the method
        }

        [TestMethod]
        public void Viewer_Activate_ShouldThrowNotImplementedException()
        {
            // Arrange
            _controller = CreateQfcFormController();

            // Act & Assert
            Assert.ThrowsException<NotImplementedException>(() => _controller.Viewer_Activate());
        }
    }
}
