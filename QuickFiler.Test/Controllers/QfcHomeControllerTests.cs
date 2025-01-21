using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcHomeControllerTests
    {
        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<System.Action> _mockParentCleanup;
        private QfcHomeController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockParentCleanup = new Mock<System.Action>();
            _controller = new QfcHomeController(_mockGlobals.Object, _mockParentCleanup.Object);
        }

        [TestMethod]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var controller = new QfcHomeController(_mockGlobals.Object, _mockParentCleanup.Object);

            // Assert
            Assert.IsNotNull(controller);
        }

        [TestMethod]
        public async Task LaunchAsync_InitializesCorrectly()
        {
            // Arrange & Act
            var controller = await QfcHomeController.LaunchAsync(_mockGlobals.Object, _mockParentCleanup.Object);

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsTrue(controller.Loaded);
        }

        [TestMethod]
        public async Task InitAsync_InitializesCorrectly()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();

            // Act
            await _controller.InitAsync(_mockGlobals.Object, _mockParentCleanup.Object, tokenSource, token, progress);

            // Assert
            Assert.IsNotNull(_controller.DataModel);
        }

        [TestMethod]
        public void Run_ExecutesCorrectly()
        {
            // Arrange
            //var mockFormController = new Mock<IFilerFormController>();
            var mockFormController = new Mock<QfcFormController>();
            var mockFormViewer = new Mock<QfcFormViewer>();
            var mockStopWatch = new Mock<Stopwatch>();

            _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);
            _controller.GetType().GetField("_formViewer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockFormViewer.Object);
            _controller.GetType().GetField("_stopWatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockStopWatch.Object);

            // Act
            _controller.Run();

            // Assert
            mockFormController.Verify(m => m.LoadItems(It.IsAny<IList<MailItem>>()), Times.Once);
            mockFormViewer.VerifySet(m => m.WindowState = FormWindowState.Maximized);
            mockFormViewer.Verify(m => m.Show(), Times.Once);
            mockFormViewer.Verify(m => m.Refresh(), Times.Once);
            mockStopWatch.Verify(m => m.Start(), Times.Once);
        }

        [TestMethod]
        public async Task RunAsync_ExecutesCorrectly()
        {
            // Arrange
            var progress = new ProgressTracker(new CancellationTokenSource()).Initialize();

            // Act
            await _controller.RunAsync(progress);

            // Assert
            Assert.IsTrue(_controller.StopWatch.IsRunning);
        }

        [TestMethod]
        public void Worker_RunWorkerCompleted_HandlesCompletionCorrectly()
        {
            // Arrange
            var mockFormViewer = new Mock<QfcFormViewer>();
            _controller.GetType().GetField("_formViewer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockFormViewer.Object);

            var eventArgs = new RunWorkerCompletedEventArgs(null, null, false);

            // Act
            _controller.GetType().GetMethod("Worker_RunWorkerCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(_controller, new object[] { null, eventArgs });

            // Assert
            mockFormViewer.VerifySet(m => m.L1v1L2h5_SpnEmailPerLoad.Enabled = true);
            mockFormViewer.VerifySet(m => m.L1v1L2h5_BtnSkip.Enabled = true);
        }

        [TestMethod]
        public async Task IterateQueueAsync_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            _controller.GetType().GetField("_datamodel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockDataModel.Object);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void Iterate_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            var mockFormController = new Mock<QfcFormController>();
            _controller.GetType().GetField("_datamodel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockDataModel.Object);
            _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

            // Act
            _controller.Iterate();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockFormController.Verify(m => m.LoadItems(It.IsAny<IList<MailItem>>()), Times.Once);
        }

        [TestMethod]
        public void Iterate2_ExecutesCorrectly()
        {
            // Arrange
            var mockQfcQueue = new Mock<QfcQueue>();
            var mockFormController = new Mock<QfcFormController>();
            _controller.GetType().GetField("_qfcQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockQfcQueue.Object);
            _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

            // Act
            _controller.Iterate2();

            // Assert
            mockQfcQueue.Verify(m => m.Dequeue(), Times.Once);
            mockFormController.Verify(m => m.LoadItems(It.IsAny<TableLayoutPanel>(), It.IsAny<List<QfcItemGroup>>()), Times.Once);
        }

        [TestMethod]
        public void SwapStopWatch_ExecutesCorrectly()
        {
            // Arrange
            var mockStopWatch = new Mock<Stopwatch>();
            _controller.GetType().GetField("_stopWatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockStopWatch.Object);

            // Act
            _controller.SwapStopWatch();

            // Assert
            mockStopWatch.Verify(m => m.Start(), Times.Once);
        }

        [TestMethod]
        public void QuickFileMetrics_WRITE_ExecutesCorrectly()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockFormController = new Mock<IFilerFormController>();
            _controller.GetType().GetField("_globals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockGlobals.Object);
            _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

            // Act
            _controller.QuickFileMetrics_WRITE("testfile.txt");

            // Assert
            mockGlobals.Verify(m => m.FS.SpecialFolders.TryGetValue("MyDocuments", out It.Ref<string>.IsAny), Times.Once);
        }

        [TestMethod]
        public async Task WriteMetricsAsync_ExecutesCorrectly()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockFormController = new Mock<IFilerFormController>();
            _controller.GetType().GetField("_globals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockGlobals.Object);
            _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

            // Act
            await _controller.WriteMetricsAsync("testfile.txt");

            // Assert
            mockGlobals.Verify(m => m.FS.SpecialFolders.TryGetValue("MyDocuments", out It.Ref<string>.IsAny), Times.Once);
        }

        [TestMethod]
        public void Cleanup_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            _controller.GetType().GetField("_datamodel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockDataModel.Object);

            // Act
            _controller.Cleanup();

            // Assert
            mockDataModel.Verify(m => m.Cleanup(), Times.Once);
            _mockParentCleanup.Verify(m => m.Invoke(), Times.Once);
        }

        [TestMethod]
        public void Loaded_PropertyWorksCorrectly()
        {
            // Arrange & Act
            _controller.Loaded = true;

            // Assert
            Assert.IsTrue(_controller.Loaded);
        }

        [TestMethod]
        public void ExplorerController_PropertyWorksCorrectly()
        {
            // Arrange
            var mockExplorerController = new Mock<IQfcExplorerController>();

            // Act
            _controller.ExplorerController = mockExplorerController.Object;

            // Assert
            Assert.AreEqual(mockExplorerController.Object, _controller.ExplorerController);
        }

        [TestMethod]
        public void FormController_PropertyWorksCorrectly()
        {
            // Arrange
            var mockFormController = new Mock<IFilerFormController>();

            // Act
            _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

            // Assert
            Assert.AreEqual(mockFormController.Object, _controller.FormController);
        }

        [TestMethod]
        public void KeyboardHandler_PropertyWorksCorrectly()
        {
            // Arrange
            var mockKeyboardHandler = new Mock<IQfcKeyboardHandler>();

            // Act
            _controller.KeyboardHandler = mockKeyboardHandler.Object;

            // Assert
            Assert.AreEqual(mockKeyboardHandler.Object, _controller.KeyboardHandler);
        }

        [TestMethod]
        public void DataModel_PropertyWorksCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();

            // Act
            _controller.GetType().GetProperty("DataModel").SetValue(_controller, mockDataModel.Object);

            // Assert
            Assert.AreEqual(mockDataModel.Object, _controller.DataModel);
        }

        [TestMethod]
        public void FilerQueue_PropertyWorksCorrectly()
        {
            // Arrange & Act
            var result = _controller.FilerQueue;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UiScheduler_PropertyWorksCorrectly()
        {
            // Arrange
            var mockUiScheduler = new Mock<TaskScheduler>();

            // Act
            _controller.GetType().GetProperty("UiScheduler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockUiScheduler.Object);

            // Assert
            Assert.AreEqual(mockUiScheduler.Object, _controller.UiScheduler);
        }

        [TestMethod]
        public void StopWatch_PropertyWorksCorrectly()
        {
            // Arrange
            var mockStopWatch = new Mock<Stopwatch>();

            // Act
            _controller.GetType().GetField("_stopWatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockStopWatch.Object);

            // Assert
            Assert.AreEqual(mockStopWatch.Object, _controller.StopWatch);
        }

        [TestMethod]
        public void TokenSource_PropertyWorksCorrectly()
        {
            // Arrange
            var mockTokenSource = new Mock<CancellationTokenSource>();

            // Act
            _controller.GetType().GetProperty("TokenSource").SetValue(_controller, mockTokenSource.Object);

            // Assert
            Assert.AreEqual(mockTokenSource.Object, _controller.TokenSource);
        }

        [TestMethod]
        public void Token_PropertyWorksCorrectly()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            // Act
            _controller.GetType().GetProperty("Token").SetValue(_controller, token);

            // Assert
            Assert.AreEqual(token, _controller.Token);
        }

        [TestMethod]
        public void WorkerComplete_PropertyWorksCorrectly()
        {
            // Arrange & Act
            _controller.GetType().GetProperty("WorkerComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, true);

            // Assert
            Assert.IsTrue((bool)_controller.GetType().GetProperty("WorkerComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_controller));
        }

        [TestMethod]
        public void UiSyncContext_PropertyWorksCorrectly()
        {
            // Arrange
            var mockUiSyncContext = new Mock<SynchronizationContext>();

            // Act
            _controller.GetType().GetProperty("UiSyncContext").SetValue(_controller, mockUiSyncContext.Object);

            // Assert
            Assert.AreEqual(mockUiSyncContext.Object, _controller.UiSyncContext);
        }
    }
}

