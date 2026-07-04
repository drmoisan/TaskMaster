using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    public partial class QfcHomeControllerRunAsyncTests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockApplicationGlobals;
        private Mock<System.Action> _mockParentCleanup;
        private QfcHomeController _controller;
        private Mock<Outlook.Application> _mockOlApp;
        private Mock<ProgressTracker> _mockProgressTracker;
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

        /// <summary>
        /// Sets up <c>Globals.QfSettings</c> on the strict globals mock so the high-confidence branch
        /// in <see cref="QfcHomeController.RunAsync"/> can read the mode flag and threshold.
        /// </summary>
        private Mock<IAppQuickFilerSettings> SetupQfSettings(
            bool highConfidenceEnabled,
            double threshold
        )
        {
            var qfSettings = this._mockRepository.Create<IAppQuickFilerSettings>();
            qfSettings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(highConfidenceEnabled);
            qfSettings.SetupGet(x => x.HighConfidenceThreshold).Returns(threshold);
            this._mockApplicationGlobals.SetupGet(x => x.QfSettings).Returns(qfSettings.Object);
            return qfSettings;
        }

        [TestMethod]
        public void Run_ExecutesCorrectly()
        {
            // Arrange

            // Mock the QfcDataModel
            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x => x.InitEmailQueue(It.IsAny<int>(), It.IsAny<BackgroundWorker>()))
                .Returns(new List<MailItem>());

            _controller.DataModel = mockDataModel.Object;
            SetupQfSettings(highConfidenceEnabled: false, threshold: 0.90);

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.Setup(x => x.LoadItems(It.IsAny<IList<MailItem>>())).Verifiable();

            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            // Mock the QfcFormViewer
            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.Setup(x => x.ShowDialog()).Returns(DialogResult.OK);
            mockFormViewer.Setup(x => x.Show()).Verifiable();
            var windowState = FormWindowState.Normal;
            mockFormViewer
                .SetupSet(x => x.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state)
                .Verifiable();
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh()).Verifiable();

            //var formViewer = new QfcFormViewer();
            _controller
                .GetType()
                .GetField(
                    "_formViewer",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
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
        public void Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch()
        {
            // Arrange
            var itemsPerIteration = 7;
            var unfilteredInitialBatch = new List<MailItem>
            {
                new Mock<MailItem>().Object,
                new Mock<MailItem>().Object,
            };

            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x => x.InitEmailQueue(It.IsAny<int>(), It.IsAny<BackgroundWorker>()))
                .Returns(unfilteredInitialBatch);
            mockDataModel
                .Setup(x => x.DequeueNextItemGroupAsync(itemsPerIteration, It.IsAny<int>()))
                .ReturnsAsync(new List<MailItem>());
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            mockFormController.Setup(x => x.LoadItems(It.IsAny<IList<MailItem>>()));
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            var windowState = FormWindowState.Normal;
            mockFormViewer
                .SetupSet(x => x.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state);
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Show());
            mockFormViewer.Setup(x => x.Refresh());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            // Act
            _controller.Run();

            // Assert
            mockDataModel.Verify(
                m => m.InitEmailQueue(itemsPerIteration, It.IsAny<BackgroundWorker>()),
                Times.Never,
                "high-confidence synchronous startup must not request a fixed unfiltered first batch"
            );
            mockFormController.Verify(
                m => m.LoadItems(unfilteredInitialBatch),
                Times.Never,
                "high-confidence synchronous startup must not load the unfiltered initial batch"
            );
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
            mockDataModel
                .Setup(x =>
                    x.InitEmailQueueAsync(
                        It.IsAny<int>(),
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            mockDataModel
                .Setup(x => x.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<MailItem>());
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            // High-confidence mode disabled => the standard IList<MailItem> path is used unchanged.
            SetupQfSettings(highConfidenceEnabled: false, threshold: 0.90);

            // Mock the QfcFormController
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<MailItem>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _controller
                .GetType()
                .GetField(
                    "_formController",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormController.Object);

            // Mock the QfcFormViewer
            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.Setup(x => x.ShowDialog()).Returns(DialogResult.OK);
            mockFormViewer.Setup(x => x.Show()).Verifiable();
            var windowState = FormWindowState.Normal;
            mockFormViewer
                .SetupSet(x => x.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state)
                .Verifiable();
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh()).Verifiable();

            //var formViewer = new QfcFormViewer();
            _controller
                .GetType()
                .GetField(
                    "_formViewer",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormViewer.Object);

            // Act
            await _controller.RunAsync(progress);

            // Assert
            Assert.IsTrue(_controller.StopWatch.IsRunning);
            mockDataModel.Verify(
                m =>
                    m.InitEmailQueueAsync(
                        It.IsAny<int>(),
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    ),
                Times.Once
            );
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<MailItem>>()),
                Times.Once
            );
            _mockProgressTracker.Verify(
                m => m.Report(It.IsAny<double>(), It.IsAny<string>()),
                Times.Exactly(2)
            );
            _mockProgressTracker.Verify(m => m.Report(It.IsAny<double>()), Times.Exactly(1));
        }

        [TestMethod]
        public void Worker_RunWorkerCompleted_HandlesCompletionCorrectly()
        {
            // Arrange
            UiThread.Init(false);
            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.SetupAllProperties();
            mockFormViewer.SetupProperty(m => m.ItemsPerLoadEnabled, false);
            mockFormViewer.SetupProperty(m => m.SkipButtonEnabled, false);
            _controller
                .GetType()
                .GetField(
                    "_formViewer",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(_controller, mockFormViewer.Object);

            var eventArgs = new RunWorkerCompletedEventArgs(null, null, false);

            // Act
            _controller
                .GetType()
                .GetMethod(
                    "Worker_RunWorkerCompleted",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .Invoke(_controller, new object[] { null, eventArgs });

            // Assert
            Assert.IsTrue(mockFormViewer.Object.ItemsPerLoadEnabled);
            Assert.IsTrue(mockFormViewer.Object.SkipButtonEnabled);
        }
    }
}
