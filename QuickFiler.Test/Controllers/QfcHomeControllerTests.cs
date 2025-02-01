using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using static QuickFiler.QfEnums;
using FluentAssertions;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcHomeControllerTests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockApplicationGlobals;
        //private Mock<IntelligenceConfig> mockIntelligenceConfig;
        private Mock<System.Action> _mockParentCleanup;
        private QfcHomeController _controller;
        private Mock<Outlook.Application> _mockOlApp;
        private Mock<ProgressTracker> _mockProgressTracker;
        private Mock<Explorer> _mockExplorer;

        [TestInitialize]
        public void Setup()
        {
            Console.SetOut(new DebugTextWriter());
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            this._mockApplicationGlobals = this._mockRepository.Create<IApplicationGlobals>();
            this._mockApplicationGlobals.SetupGet(x => x.AF.CancelToken).Returns(CancellationToken.None);
                        
            this._mockOlApp = this._mockRepository.Create<Outlook.Application>();
            this._mockExplorer = this._mockRepository.Create<Explorer>();
            this._mockOlApp.Setup(x => x.ActiveExplorer()).Returns(_mockExplorer.Object);
            this._mockApplicationGlobals.SetupGet(x => x.Ol.App).Returns(_mockOlApp.Object);
            
            _ = SetUpMockIntelRes(_mockApplicationGlobals);
            
            _mockParentCleanup = new Mock<System.Action>();
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
        }

        private Mock<ProgressTracker> SetupMockProgressTracker(CancellationTokenSource cancellationTokenSource)
        {
            var mockProgressTracker = new Mock<ProgressTracker>(cancellationTokenSource);
            mockProgressTracker.SetupAllProperties();
            mockProgressTracker.Setup(m => m.Report(It.IsAny<double>()));
            mockProgressTracker.Setup(m => m.Report(It.IsAny<double>(), It.IsAny<string>()));
            mockProgressTracker.Setup(m => m.Report(It.IsAny<ValueTuple<int, string>>()));
            mockProgressTracker.Setup(m => m.SpawnChild()).Returns(mockProgressTracker.Object);
            mockProgressTracker.Setup(m => m.SpawnChild(It.IsAny<double>())).Returns(mockProgressTracker.Object);
            mockProgressTracker.Setup(m => m.SpawnChild(It.IsAny<int>())).Returns(mockProgressTracker.Object);
            return mockProgressTracker;
        }

        private Mock<IntelligenceConfig> SetUpMockIntelRes(Mock<IApplicationGlobals> mockGlobals)
        {
            var intel = this._mockRepository.Create<IntelligenceConfig>(mockGlobals.Object);
            var config = new Dictionary<string, SmartSerializableLoader>
            {
                { "Folder", new SmartSerializableLoader()   }
            }.ToConcurrentDictionary();
            intel.SetupGet(x => x.Config).Returns(config);
            mockGlobals.SetupGet(x => x.IntelRes).Returns(intel.Object);

            return intel;
        }

        [TestMethod]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);

            // Assert
            Assert.IsNotNull(_controller, "Controller is null");
            Assert.AreEqual(_mockApplicationGlobals.Object, _controller.Globals,"Applications Globals not set correctly");
            Assert.AreEqual(_mockParentCleanup.Object, _controller.ParentCleanup, "ParentCleanup not set correctly");
        }

        [TestMethod]
        public void Init_InitializesCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);

            var mockData = new Mock<IQfcDatamodel>();            
            _controller.QfcDataModelLoader = (globals, token) => mockData.Object;

            var mockExplorer = new Mock<IQfcExplorerController>();
            _controller.QfcExplorerControllerLoader = (initType, globals, homeController) => mockExplorer.Object;

            var mockKeyboardHandlerLoader = new Mock<IQfcKeyboardHandler>();
            _controller.QfcKeyboardHandlerLoader = (viewer, homeController) => mockKeyboardHandlerLoader.Object;

            var mockQueue = new Mock<IQfcQueue>();
            _controller.QfcQueueLoader = (globals, viewer, homeController) => mockQueue.Object;

            var mockFormController = new Mock<IQfcFormController>();
            _controller.QfcFormControllerLoader = (globals, viewer, queue, initType, parentCleanup, 
                homeController, tokenSource, token) => mockFormController.Object;

            // Act
            _controller.Init();

            // Assert
            Assert.AreEqual(mockData.Object, _controller.DataModel, "Data model not set correctly");
            Assert.AreEqual(mockKeyboardHandlerLoader.Object, _controller.KeyboardHandler, "Keyboard handler not set correctly");
            Assert.AreEqual(mockQueue.Object, _controller.QfcQueue, "Queue not set correctly");
            Assert.AreEqual(mockFormController.Object, _controller.FormController, "Form controller not set correctly");
        }
        
        //[TestMethod]
        //public async Task LaunchAsync_InitializesCorrectly()
        //{
        //    // Arrange & Act
        //    UiThread.Init(false);
        //    var controller = await QfcHomeController.LaunchAsync(_mockApplicationGlobals.Object, _mockParentCleanup.Object);

        //    // Assert
        //    Assert.IsNotNull(controller);
        //    Assert.IsTrue(controller.Loaded);
        //}

        [TestMethod]
        public async Task InitAsync_InitializesCorrectly()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            //var progress = new ProgressTracker(tokenSource).Initialize();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            var progress = _mockProgressTracker.Object;

            var mockData = new Mock<IQfcDatamodel>();
            _controller.QfcAsyncDataModelLoader = async (globals, cancel, cancelSource, progressTracker) => await Task.FromResult(mockData.Object);

            var mockExplorer = new Mock<IQfcExplorerController>();
            _controller.QfcExplorerControllerLoader = (initType, globals, homeController) => mockExplorer.Object;

            var mockKeyboardHandlerLoader = new Mock<IQfcKeyboardHandler>();
            _controller.QfcKeyboardHandlerLoader = (viewer, homeController) => mockKeyboardHandlerLoader.Object;

            var mockQueue = new Mock<IQfcQueue>();
            _controller.QfcQueueLoader = (globals, viewer, homeController) => mockQueue.Object;

            var mockFormController = new Mock<IQfcFormController>();
            _controller.QfcFormControllerLoader = (globals, viewer, queue, initType, parentCleanup,
                homeController, cancelSource, cancel) => mockFormController.Object;

            // Act
            await _controller.InitAsync(_mockApplicationGlobals.Object, _mockParentCleanup.Object, tokenSource, token, progress);

            // Assert            
            Assert.AreEqual(mockData.Object, _controller.DataModel, "Data model not set correctly");
            Assert.AreEqual(mockKeyboardHandlerLoader.Object, _controller.KeyboardHandler, "Keyboard handler not set correctly");
            Assert.AreEqual(mockQueue.Object, _controller.QfcQueue, "Queue not set correctly");
            Assert.AreEqual(mockFormController.Object, _controller.FormController, "Form controller not set correctly");
        }

        public class QfcFormViewerDerived: QfcFormViewer
        {
            public QfcFormViewerDerived() : base() { }
            public new virtual void Show() => base.Show();
            //public new virtual DialogResult ShowDialog() => base.ShowDialog();
            public new virtual FormWindowState WindowState { get; set; }
        }
        
        [TestMethod]
        public void Run_ExecutesCorrectly()
        {
            // Arrange            

            // Mock the QfcDataModel
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(x => x.InitEmailQueue(It.IsAny<int>(), It.IsAny<BackgroundWorker>())).Returns(new List<MailItem>());

            _controller.DataModel = mockDataModel.Object;

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(x => x.LoadItems(It.IsAny<IList<MailItem>>())).Verifiable();

            _controller.GetType().GetField("_formController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, mockFormController.Object);

            // Mock the QfcFormViewer
            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.Setup(x => x.ShowDialog()).Returns(DialogResult.OK);
            mockFormViewer.Setup(x => x.Show()).Verifiable();
            var windowState = FormWindowState.Normal;
            mockFormViewer.SetupSet(x => x.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state).Verifiable();
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh()).Verifiable();

            //var formViewer = new QfcFormViewer();            
            _controller.GetType().GetField("_formViewer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, mockFormViewer.Object);            

            // Act
            _controller.Run();

            // Assert
            
            mockFormController.Verify(m => m.LoadItems(It.IsAny<IList<MailItem>>()), Times.Once);

            mockFormViewer.VerifySet(m => m.WindowState = FormWindowState.Maximized);
            mockFormViewer.Verify(m => m.Show(), Times.Once);
            mockFormViewer.Verify(m => m.Refresh(), Times.Once);
            
        }

        [TestMethod]
        public async Task RunAsync_ExecutesCorrectly()
        {
            // Arrange
            
            // Mock the Progress Tracker
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            var progress = _mockProgressTracker.Object;

            // Mock the QfcDataModel
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(x => x.InitEmailQueue(It.IsAny<int>(), It.IsAny<BackgroundWorker>())).Returns(new List<MailItem>());
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(x => x.LoadItemsAsync(It.IsAny<IList<MailItem>>())).Returns(Task.CompletedTask).Verifiable();

            _controller.GetType().GetField("_formController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, mockFormController.Object);

            // Mock the QfcFormViewer
            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.Setup(x => x.ShowDialog()).Returns(DialogResult.OK);
            mockFormViewer.Setup(x => x.Show()).Verifiable();
            var windowState = FormWindowState.Normal;
            mockFormViewer.SetupSet(x => x.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state).Verifiable();
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh()).Verifiable();

            //var formViewer = new QfcFormViewer();            
            _controller.GetType().GetField("_formViewer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, mockFormViewer.Object);

            // Act
            await _controller.RunAsync(progress);

            // Assert
            Assert.IsTrue(_controller.StopWatch.IsRunning);
            mockDataModel.Verify(m => m.InitEmailQueueAsync(It.IsAny<int>(), It.IsAny<BackgroundWorker>(), It.IsAny<CancellationToken>(), It.IsAny<CancellationTokenSource>()), Times.Once);
            mockFormController.Verify(m => m.LoadItemsAsync(It.IsAny<IList<MailItem>>()), Times.Once);
            mockFormViewer.VerifySet(m => m.WindowState = FormWindowState.Maximized);
            mockFormViewer.Verify(m => m.Show(), Times.Once);
            mockFormViewer.Verify(m => m.Refresh(), Times.Once);
            _mockProgressTracker.Verify(m => m.Report(It.IsAny<double>(), It.IsAny<string>()), Times.Exactly(2));
            _mockProgressTracker.Verify(m => m.Report(It.IsAny<double>()), Times.Exactly(1));
        }

        [TestMethod]
        public void Worker_RunWorkerCompleted_HandlesCompletionCorrectly()
        {
            // Arrange
            UiThread.Init(false);
            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.SetupAllProperties();
            var spinner = new NumericUpDown() { Enabled = false };
            var button = new Button() { Enabled = false };
            mockFormViewer.SetupGet(m => m.L1v1L2h5_SpnEmailPerLoad).Returns(spinner).Verifiable();
            mockFormViewer.SetupGet(m => m.L1v1L2h5_BtnSkip).Returns(button).Verifiable();
            _controller.GetType().GetField("_formViewer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockFormViewer.Object);

            var eventArgs = new RunWorkerCompletedEventArgs(null, null, false);

            // Act
            _controller.GetType().GetMethod("Worker_RunWorkerCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(_controller, new object[] { null, eventArgs });

            // Assert
            Assert.IsTrue(spinner.Enabled);
            Assert.IsTrue(button.Enabled);
        }

        [TestMethod]
        public async Task IterateQueueAsync_DataModelComplete()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(true);
            mockDataModel.Setup(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny <int>())).Returns(Task.FromResult((IList<MailItem>)new List<MailItem>()));
            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue.Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            mockQfcQueue.Setup(m => m.EnqueueAsync(It.IsAny <IList<MailItem>>(), It.IsAny<IQfcCollectionController>())).Returns(Task.CompletedTask);
            _controller.DataModel = mockDataModel.Object;
            _controller.QfcQueue = mockQfcQueue.Object;

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            mockQfcQueue.Verify(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Never);
            mockQfcQueue.Verify(m => m.EnqueueAsync(It.IsAny<IList<MailItem>>(), It.IsAny<IQfcCollectionController>()), Times.Never);
        }

        [TestMethod]
        public async Task IterateQueueAsync_QueueEmpty()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(false);
            mockDataModel.Setup(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult((IList<MailItem>)new List<MailItem>()));
            _controller.DataModel = mockDataModel.Object;
            
            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue.Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            mockQfcQueue.Setup(m => m.EnqueueAsync(It.IsAny<IList<MailItem>>(), It.IsAny<IQfcCollectionController>())).Returns(Task.CompletedTask);
            _controller.QfcQueue = mockQfcQueue.Object;

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(m => m.ItemsPerIteration).Returns(8);
            var mockQfcCollectionController = new Mock<IQfcCollectionController>();
            mockFormController.Setup(m => m.Groups).Returns(mockQfcCollectionController.Object);
            _controller.GetType().GetField("_formController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, mockFormController.Object);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockQfcQueue.Verify(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Once);
            mockQfcQueue.Verify(m => m.EnqueueAsync(It.IsAny<IList<MailItem>>(), It.IsAny<IQfcCollectionController>()), Times.Never);
        }

        [TestMethod]
        public async Task IterateQueueAsync_Queue2()
        {
            // Arrange

            // Mock DataModel
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(false);

            // Setup DequeueNextItemGroupAsync to return 2 mail items
            var mockMailItem = new Mock<MailItem>();
            IList<MailItem> mailItems = new List<MailItem> { mockMailItem.Object, mockMailItem.Object };
            mockDataModel.Setup(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(mailItems));
            
            // Set the DataModel in the controller to the mock
            _controller.DataModel = mockDataModel.Object;

            // Mock the QfcQueue
            var mockQfcQueue = new Mock<IQfcQueue>();
            mockQfcQueue.Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>())).Returns(Task.CompletedTask);
            mockQfcQueue.Setup(m => m.EnqueueAsync(It.IsAny<IList<MailItem>>(), It.IsAny<IQfcCollectionController>())).Returns(Task.CompletedTask);
            _controller.QfcQueue = mockQfcQueue.Object;

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(m => m.ItemsPerIteration).Returns(8);
            var mockQfcCollectionController = new Mock<IQfcCollectionController>();
            mockFormController.Setup(m => m.Groups).Returns(mockQfcCollectionController.Object);
            _controller.GetType().GetField("_formController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockFormController.Object);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            mockQfcQueue.Verify(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Never);
            mockQfcQueue.Verify(m => m.EnqueueAsync(It.IsAny<IList<MailItem>>(), It.IsAny<IQfcCollectionController>()), Times.Once);
        }

        [TestMethod]
        public void Iterate_ExecutesCorrectly()
        {
            // Arrange

            // Setup the DataModel to return 2 mocked mail items
            var mockDataModel = new Mock<IQfcDatamodel>();            
            var mockMailItem = new Mock<MailItem>();
            IList<MailItem> mailItems = new List<MailItem> { mockMailItem.Object, mockMailItem.Object };
            mockDataModel.Setup(m => m.DequeueNextItemGroup(It.IsAny<int>()))
                .Returns(mailItems);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            _controller.GetType().GetField("_formController", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, mockFormController.Object);

            // Act
            _controller.Iterate();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroup(It.IsAny<int>()), Times.Once);
            
            mockFormController.Verify(m => m.LoadItems(It.Is<IList<MailItem>>(
                items => items.Count == 2 && items.Contains(mockMailItem.Object))), Times.Once);

        }

        [TestMethod]
        public void Iterate2_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel.Setup(m => m.Complete).Returns(true);
            var mockQfcQueue = new Mock<IQfcQueue>();
            var mockFormController = new Mock<IQfcFormController>();
            _controller.QfcQueue = mockQfcQueue.Object;
            _controller.GetType().GetField("_formController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockFormController.Object);            
            _controller.DataModel = mockDataModel.Object;

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
            var stopWatch = new Stopwatch();
            _controller.GetType().GetField("_stopWatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, stopWatch);

            // Act
            _controller.SwapStopWatch();

            // Assert
            var actual = _controller.GetType().GetField(
                "_stopWatchMoved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_controller) as Stopwatch;
            Assert.AreEqual(stopWatch, actual);
        }

        //[TestMethod]
        //public void QuickFileMetrics_WRITE_ExecutesCorrectly()
        //{
        //    // Arrange
        //    var mockGlobals = new Mock<IApplicationGlobals>();
        //    var mockFormController = new Mock<IFilerFormController>();
        //    _controller.GetType().GetField("_globals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockGlobals.Object);
        //    _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

        //    // Act
        //    _controller.QuickFileMetrics_WRITE("testfile.txt");

        //    // Assert
        //    mockGlobals.Verify(m => m.FS.SpecialFolders.TryGetValue("MyDocuments", out It.Ref<string>.IsAny), Times.Once);
        //}

        //[TestMethod]
        //public async Task WriteMetricsAsync_ExecutesCorrectly()
        //{
        //    // Arrange
        //    var mockGlobals = new Mock<IApplicationGlobals>();
        //    var mockFormController = new Mock<IFilerFormController>();
        //    _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
        //    _controller.GetType().GetField("_globals", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockGlobals.Object);
        //    _controller.GetType().GetProperty("FormController").SetValue(_controller, mockFormController.Object);

        //    // Act
        //    await _controller.WriteMetricsAsync("testfile.txt");

        //    // Assert
        //    mockGlobals.Verify(m => m.FS.SpecialFolders.TryGetValue("MyDocuments", out It.Ref<string>.IsAny), Times.Once);
        //}

        [TestMethod]
        public void Cleanup_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
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
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
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
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            var mockFormController = new Mock<IQfcFormController>();

            // Act
            _controller.GetType().GetField("_formController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockFormController.Object);

            // Assert
            Assert.AreEqual(mockFormController.Object, _controller.FormController);
        }

        [TestMethod]
        public void KeyboardHandler_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
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
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
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
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            var result = _controller.FilerQueue;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UiScheduler_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            var mockUiScheduler = new Mock<TaskScheduler>();

            // Act
            _controller.GetType().GetField("_uiScheduler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockUiScheduler.Object);            

            // Assert
            Assert.AreEqual(mockUiScheduler.Object, _controller.UiScheduler);
        }

        [TestMethod]
        public void StopWatch_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
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
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            var mockTokenSource = new Mock<CancellationTokenSource>();

            // Act
            _controller.GetType().GetField("_tokenSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockTokenSource.Object);            

            // Assert
            Assert.AreEqual(mockTokenSource.Object, _controller.TokenSource);
        }

        [TestMethod]
        public void Token_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            // Act
            _controller.GetType().GetField("_token", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, token);

            // Assert
            Assert.AreEqual(token, _controller.Token);
        }

        [TestMethod]
        public void WorkerComplete_PropertyWorksCorrectly()
        {
            // Arrange & Act
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            _controller.GetType().GetProperty("WorkerComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, true);

            // Assert
            Assert.IsTrue((bool)_controller.GetType().GetProperty("WorkerComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_controller));
        }

        [TestMethod]
        public void UiSyncContext_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(_mockApplicationGlobals.Object, _mockParentCleanup.Object);
            var mockUiSyncContext = new Mock<SynchronizationContext>();

            // Act
            _controller.GetType().GetField("_uiSyncContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_controller, mockUiSyncContext.Object);            

            // Assert
            Assert.AreEqual(mockUiSyncContext.Object, _controller.UiSyncContext);
        }
    }
}

