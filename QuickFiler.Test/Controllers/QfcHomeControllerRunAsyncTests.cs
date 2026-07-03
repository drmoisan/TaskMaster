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
    public class QfcHomeControllerRunAsyncTests
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

        // -------------------------------------------------------------------------
        // Issue #171 high-confidence pre-filter tests (P3-T3..T5, P6-T1, P6-T3).
        // -------------------------------------------------------------------------

        /// <summary>
        /// Arranges the controller for a <see cref="QfcHomeController.RunAsync"/> high-confidence
        /// test: a data model returning an empty batch, a mocked form controller, a mocked form
        /// viewer, and QfSettings wired to the supplied mode/threshold. Returns the form controller
        /// mock so each test can assert which overload was invoked.
        /// </summary>
        private Mock<IQfcFormController> ArrangeRunAsyncController(
            bool highConfidenceEnabled,
            ProgressTracker progress
        )
        {
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

            SetupQfSettings(highConfidenceEnabled, threshold: 0.90);

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<MailItem>>()))
                .Returns(Task.CompletedTask);
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()))
                .Returns(Task.CompletedTask);
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.Setup(x => x.Show());
            var windowState = FormWindowState.Normal;
            mockFormViewer
                .SetupSet(x => x.WindowState = It.IsAny<FormWindowState>())
                .Callback<FormWindowState>(state => windowState = state);
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            return mockFormController;
        }

        /// <summary>
        /// [P3-T3] Setup test: the controller can be constructed with the pre-filter delegate
        /// overridden and the form controller mocked, without live COM.
        /// </summary>
        [TestMethod]
        public void HighConfidencePreFilterLoader_CanBeOverridden_ForTesting()
        {
            // Arrange
            var invoked = false;
            _controller.HighConfidencePreFilterLoader = (items, globals, threshold, token) =>
            {
                invoked = true;
                return Task.FromResult<IList<QfcPreScoredItem>>(new List<QfcPreScoredItem>());
            };

            // Act
            var result = _controller.HighConfidencePreFilterLoader(
                new List<MailItem>(),
                _mockApplicationGlobals.Object,
                0.90,
                CancellationToken.None
            );

            // Assert
            invoked.Should().BeTrue("the overridden delegate must be the one invoked");
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void RunAsync_SourceUsesDequeueLayerForFirstDisplayedPage()
        {
            string source = File.ReadAllText(
                ResolveRepositoryPath("QuickFiler", "Controllers", "QfcHomeController.cs")
            );

            source.Should().Contain("InitEmailQueueAsync");
            source.Should().Contain("HighConfidenceModeEnabled");
            source.Should().Contain("DequeueNextItemGroupAsync");
            source.Should().Contain("LoadItemsAsync(listEmail)");
        }

        private static string ResolveRepositoryPath(params string[] pathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (
                directory != null
                && !Directory.Exists(Path.Combine(directory.FullName, "QuickFiler"))
            )
            {
                directory = directory.Parent;
            }

            directory.Should().NotBeNull("source-inspection tests must run under the repository");

            var resolvedPath = directory.FullName;
            foreach (var pathPart in pathParts)
            {
                resolvedPath = Path.Combine(resolvedPath, pathPart);
            }

            return resolvedPath;
        }

        /// <summary>
        /// [P3-T5] With high-confidence mode disabled, RunAsync does NOT invoke the pre-filter and
        /// uses the plain IList&lt;MailItem&gt; LoadItemsAsync overload unchanged.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            var progress = _mockProgressTracker.Object;
            var mockFormController = ArrangeRunAsyncController(
                highConfidenceEnabled: false,
                progress
            );

            var preFilterInvoked = false;
            _controller.HighConfidencePreFilterLoader = (items, globals, threshold, token) =>
            {
                preFilterInvoked = true;
                return Task.FromResult<IList<QfcPreScoredItem>>(new List<QfcPreScoredItem>());
            };

            // Act
            await _controller.RunAsync(progress);

            // Assert
            preFilterInvoked.Should().BeFalse("disabled mode must not run the pre-filter");
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<MailItem>>()),
                Times.Once,
                "disabled mode must use the plain IList<MailItem> overload"
            );
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()),
                Times.Never,
                "disabled mode must NOT use the carrier-list overload"
            );
        }

        /// <summary>
        /// [P6-T3] Disabled-mode path constructs item groups via the plain IList&lt;MailItem&gt;
        /// overload only; no carrier type is involved (standard flow unchanged).
        /// </summary>
        [TestMethod]
        public async Task RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            var progress = _mockProgressTracker.Object;
            var mockFormController = ArrangeRunAsyncController(
                highConfidenceEnabled: false,
                progress
            );

            // Act
            await _controller.RunAsync(progress);

            // Assert
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<MailItem>>()),
                Times.Once
            );
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()),
                Times.Never
            );
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
