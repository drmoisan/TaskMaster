using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

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
            this._mockApplicationGlobals.SetupGet(x => x.AF.CancelToken)
                .Returns(CancellationToken.None);

            this._mockOlApp = this._mockRepository.Create<Outlook.Application>();
            this._mockExplorer = this._mockRepository.Create<Explorer>();
            this._mockOlApp.Setup(x => x.ActiveExplorer()).Returns(_mockExplorer.Object);
            this._mockApplicationGlobals.SetupGet(x => x.Ol.App).Returns(_mockOlApp.Object);

            _ = SetUpMockIntelRes(_mockApplicationGlobals);

            _mockParentCleanup = new Mock<System.Action>();
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
        }

        private Mock<ProgressTracker> SetupMockProgressTracker(
            CancellationTokenSource cancellationTokenSource
        )
        {
            var mockProgressTracker = new Mock<ProgressTracker>(cancellationTokenSource);
            mockProgressTracker.SetupAllProperties();
            mockProgressTracker.Setup(m => m.Report(It.IsAny<double>()));
            mockProgressTracker.Setup(m => m.Report(It.IsAny<double>(), It.IsAny<string>()));
            mockProgressTracker.Setup(m => m.Report(It.IsAny<ValueTuple<int, string>>()));
            mockProgressTracker.Setup(m => m.SpawnChild()).Returns(mockProgressTracker.Object);
            mockProgressTracker
                .Setup(m => m.SpawnChild(It.IsAny<double>()))
                .Returns(mockProgressTracker.Object);
            mockProgressTracker
                .Setup(m => m.SpawnChild(It.IsAny<int>()))
                .Returns(mockProgressTracker.Object);
            return mockProgressTracker;
        }

        private Mock<IntelligenceConfig> SetUpMockIntelRes(Mock<IApplicationGlobals> mockGlobals)
        {
            var intel = this._mockRepository.Create<IntelligenceConfig>(mockGlobals.Object);
            var config = new Dictionary<string, SmartSerializableLoader>
            {
                { "Folder", new SmartSerializableLoader() },
            }.ToConcurrentDictionary();
            intel.SetupGet(x => x.Config).Returns(config);
            mockGlobals.SetupGet(x => x.IntelRes).Returns(intel.Object);

            return intel;
        }

        [TestMethod]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );

            // Assert
            Assert.IsNotNull(_controller, "Controller is null");
            Assert.AreEqual(
                _mockApplicationGlobals.Object,
                _controller.Globals,
                "Applications Globals not set correctly"
            );
            Assert.AreEqual(
                _mockParentCleanup.Object,
                _controller.ParentCleanup,
                "ParentCleanup not set correctly"
            );
        }

        [TestMethod]
        public void Init_InitializesCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );

            var mockData = new Mock<IQfcDatamodel>();
            _controller.QfcDataModelLoader = (globals, token) => mockData.Object;

            var mockExplorer = new Mock<IQfcExplorerController>();
            _controller.QfcExplorerControllerLoader = (initType, globals, homeController) =>
                mockExplorer.Object;

            var mockKeyboardHandlerLoader = new Mock<IQfcKeyboardHandler>();
            _controller.QfcKeyboardHandlerLoader = (viewer, homeController) =>
                mockKeyboardHandlerLoader.Object;

            var mockQueue = new Mock<IQfcQueue>();
            _controller.QfcQueueLoader = (globals, viewer, homeController) => mockQueue.Object;

            var mockFormController = new Mock<IQfcFormController>();
            _controller.QfcFormControllerLoader = (
                globals,
                viewer,
                queue,
                initType,
                parentCleanup,
                homeController,
                tokenSource,
                token
            ) => mockFormController.Object;

            // Act
            _controller.Init();

            // Assert
            Assert.AreEqual(mockData.Object, _controller.DataModel, "Data model not set correctly");
            Assert.AreEqual(
                mockKeyboardHandlerLoader.Object,
                _controller.KeyboardHandler,
                "Keyboard handler not set correctly"
            );
            Assert.AreEqual(mockQueue.Object, _controller.QfcQueue, "Queue not set correctly");
            Assert.AreEqual(
                mockFormController.Object,
                _controller.FormController,
                "Form controller not set correctly"
            );
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
            _controller.QfcAsyncDataModelLoader = async (
                globals,
                cancel,
                cancelSource,
                progressTracker
            ) => await Task.FromResult(mockData.Object);

            var mockExplorer = new Mock<IQfcExplorerController>();
            _controller.QfcExplorerControllerLoader = (initType, globals, homeController) =>
                mockExplorer.Object;

            var mockKeyboardHandlerLoader = new Mock<IQfcKeyboardHandler>();
            _controller.QfcKeyboardHandlerLoader = (viewer, homeController) =>
                mockKeyboardHandlerLoader.Object;

            var mockQueue = new Mock<IQfcQueue>();
            _controller.QfcQueueLoader = (globals, viewer, homeController) => mockQueue.Object;

            var mockFormController = new Mock<IQfcFormController>();
            _controller.QfcFormControllerLoader = (
                globals,
                viewer,
                queue,
                initType,
                parentCleanup,
                homeController,
                cancelSource,
                cancel
            ) => mockFormController.Object;

            // Act
            await _controller.InitAsync(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object,
                tokenSource,
                token,
                progress
            );

            // Assert
            Assert.AreEqual(mockData.Object, _controller.DataModel, "Data model not set correctly");
            Assert.AreEqual(
                mockKeyboardHandlerLoader.Object,
                _controller.KeyboardHandler,
                "Keyboard handler not set correctly"
            );
            Assert.AreEqual(mockQueue.Object, _controller.QfcQueue, "Queue not set correctly");
            Assert.AreEqual(
                mockFormController.Object,
                _controller.FormController,
                "Form controller not set correctly"
            );
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
    }
}
