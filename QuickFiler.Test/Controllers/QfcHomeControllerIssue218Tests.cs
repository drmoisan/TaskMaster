using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class QfcHomeControllerIssue218Tests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockApplicationGlobals;
        private QfcHomeController _controller;
        private Mock<Outlook.Application> _mockOlApp;
        private Mock<Explorer> _mockExplorer;
        private Mock<ProgressTracker> _mockProgressTracker;

        [TestInitialize]
        public void Setup()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            this._mockApplicationGlobals = this._mockRepository.Create<IApplicationGlobals>();
            this._mockApplicationGlobals.SetupGet(x => x.AF.CancelToken)
                .Returns(CancellationToken.None);

            this._mockOlApp = this._mockRepository.Create<Outlook.Application>();
            this._mockExplorer = this._mockRepository.Create<Explorer>();
            this._mockOlApp.Setup(x => x.ActiveExplorer()).Returns(_mockExplorer.Object);
            this._mockApplicationGlobals.SetupGet(x => x.Ol.App).Returns(_mockOlApp.Object);

            _controller = new QfcHomeController(_mockApplicationGlobals.Object, () => { });
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
                        It.IsAny<System.ComponentModel.BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    )
                )
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
            var windowState = System.Windows.Forms.FormWindowState.Normal;
            mockFormViewer
                .SetupSet(x => x.WindowState = It.IsAny<System.Windows.Forms.FormWindowState>())
                .Callback<System.Windows.Forms.FormWindowState>(state => windowState = state);
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            return mockFormController;
        }

        [TestMethod]
        public async Task RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch()
        {
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            var progress = _mockProgressTracker.Object;
            var mockFormController = ArrangeRunAsyncController(
                highConfidenceEnabled: true,
                progress
            );

            var preFilterInvoked = false;
            _controller.HighConfidencePreFilterLoader = (items, globals, threshold, token) =>
            {
                preFilterInvoked = true;
                return Task.FromResult<IList<QfcPreScoredItem>>(new List<QfcPreScoredItem>());
            };

            await _controller.RunAsync(progress);

            preFilterInvoked
                .Should()
                .BeFalse("remaining-queue admission now owns high-confidence filtering");
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<MailItem>>()),
                Times.Once,
                "the initial GUI batch must use the plain MailItem load path"
            );
            Mock.Get(_controller.DataModel)
                .Verify(
                    m =>
                        m.DequeueNextItemGroupAsync(
                            It.IsAny<int>(),
                            It.IsAny<int>(),
                            It.IsAny<TimeSpan>(),
                            It.IsAny<System.Action<int, int, int>>()
                        ),
                    Times.Once,
                    "the first displayed page must come from the dequeue-layer gate"
                );
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()),
                Times.Never,
                "RunAsync must not use the carrier-list overload for the initial batch"
            );
        }

        [TestMethod]
        public async Task RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter()
        {
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            var progress = _mockProgressTracker.Object;

            var sequence = new List<string>();

            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x =>
                    x.InitEmailQueueAsync(
                        It.IsAny<int>(),
                        It.IsAny<System.ComponentModel.BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<MailItem>>()))
                .Returns(Task.CompletedTask)
                .Callback(() => sequence.Add("LoadItemsAsync"));
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.Setup(x => x.Show());
            var windowState = System.Windows.Forms.FormWindowState.Normal;
            mockFormViewer
                .SetupSet(x => x.WindowState = It.IsAny<System.Windows.Forms.FormWindowState>())
                .Callback<System.Windows.Forms.FormWindowState>(state => windowState = state);
            mockFormViewer.SetupGet(x => x.WindowState).Returns(() => windowState);
            mockFormViewer.Setup(x => x.Refresh());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            _controller.HighConfidencePreFilterLoader = (items, globals, threshold, token) =>
            {
                sequence.Add("PreFilter");
                return Task.FromResult<IList<QfcPreScoredItem>>(new List<QfcPreScoredItem>());
            };

            await _controller.RunAsync(progress);

            sequence.Should().Equal("LoadItemsAsync");
            mockDataModel.Verify(
                m =>
                    m.DequeueNextItemGroupAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    ),
                Times.Once
            );
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()),
                Times.Never
            );
        }
    }
}
