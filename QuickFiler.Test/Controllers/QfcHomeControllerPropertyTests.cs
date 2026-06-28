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
    public class QfcHomeControllerPropertyTests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockApplicationGlobals;
        private Mock<System.Action> _mockParentCleanup;
        private QfcHomeController _controller;
        private Mock<Outlook.Application> _mockOlApp;
        private Mock<Explorer> _mockExplorer;

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
        public void Cleanup_ExecutesCorrectly()
        {
            // Arrange
            var mockDataModel = new Mock<IQfcDatamodel>();
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            _controller
                .GetType()
                .GetField(
                    "_datamodel",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockDataModel.Object);

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
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
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
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var mockFormController = new Mock<IQfcFormController>();

            // Act
            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            // Assert
            Assert.AreEqual(mockFormController.Object, _controller.FormController);
        }

        [TestMethod]
        public void KeyboardHandler_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
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
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var mockDataModel = new Mock<IQfcDatamodel>();

            // Act
            _controller
                .GetType()
                .GetProperty("DataModel")
                .SetValue(_controller, mockDataModel.Object);

            // Assert
            Assert.AreEqual(mockDataModel.Object, _controller.DataModel);
        }

        [TestMethod]
        public void FilerQueue_PropertyWorksCorrectly()
        {
            // Arrange & Act
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var result = _controller.FilerQueue;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UiScheduler_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var mockUiScheduler = new Mock<TaskScheduler>();

            // Act
            _controller
                .GetType()
                .GetField(
                    "_uiScheduler",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockUiScheduler.Object);

            // Assert
            Assert.AreEqual(mockUiScheduler.Object, _controller.UiScheduler);
        }

        [TestMethod]
        public void StopWatch_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var mockStopWatch = new Mock<Stopwatch>();

            // Act
            _controller
                .GetType()
                .GetField(
                    "_stopWatch",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockStopWatch.Object);

            // Assert
            Assert.AreEqual(mockStopWatch.Object, _controller.StopWatch);
        }

        [TestMethod]
        public void TokenSource_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var mockTokenSource = new Mock<CancellationTokenSource>();

            // Act
            _controller
                .GetType()
                .GetField(
                    "_tokenSource",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockTokenSource.Object);

            // Assert
            Assert.AreEqual(mockTokenSource.Object, _controller.TokenSource);
        }

        [TestMethod]
        public void Token_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            // Act
            _controller
                .GetType()
                .GetField(
                    "_token",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, token);

            // Assert
            Assert.AreEqual(token, _controller.Token);
        }

        [TestMethod]
        public void WorkerComplete_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );

            // Act & Assert
            SetPrivateField(_controller, "_workerComplete", true);
            Assert.IsTrue(_controller.WorkerComplete);

            SetPrivateField(_controller, "_workerComplete", false);
            Assert.IsFalse(_controller.WorkerComplete);
        }

        [TestMethod]
        public void UiSyncContext_PropertyWorksCorrectly()
        {
            // Arrange
            _controller = new QfcHomeController(
                _mockApplicationGlobals.Object,
                _mockParentCleanup.Object
            );
            var mockUiSyncContext = new Mock<SynchronizationContext>();

            // Act
            _controller
                .GetType()
                .GetField(
                    "_uiSyncContext",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockUiSyncContext.Object);

            // Assert
            Assert.AreEqual(mockUiSyncContext.Object, _controller.UiSyncContext);
        }
    }
}
