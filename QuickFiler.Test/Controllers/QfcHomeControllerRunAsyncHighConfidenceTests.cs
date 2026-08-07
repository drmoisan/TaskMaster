using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    public partial class QfcHomeControllerRunAsyncTests
    {
        // -------------------------------------------------------------------------
        // Issue #233 high-confidence RunAsync startup tests.
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

        /// <summary>
        /// Verifies the high-confidence pre-filter delegate can be replaced in tests without live COM.
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
        public async Task RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue()
        {
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            ProgressTracker progress = _mockProgressTracker.Object;
            var unfilteredInitialBatch = new List<MailItem> { new Mock<MailItem>().Object };
            var streamedCandidate = new Mock<MailItem>().Object;
            var streamedBatch = new List<MailItem> { streamedCandidate };
            const int itemsPerIteration = 7;

            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x =>
                    x.InitEmailQueueAsync(
                        0,
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(unfilteredInitialBatch);
            // Issue #424: the pre-UI call site moved to the deadline+progress overload and adopted
            // the 200 ms poll (O1). The sink must be non-null so the ProgressViewer advances.
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupAsync(
                        itemsPerIteration,
                        200,
                        QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
                        It.Is<System.Action<int, int, int>>(sink => sink != null)
                    )
                )
                .ReturnsAsync(streamedBatch);
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            mockFormController
                .Setup(x =>
                    x.LoadItemsAsync(
                        It.Is<IList<MailItem>>(items =>
                            items.Count == 1 && ReferenceEquals(items[0], streamedCandidate)
                        )
                    )
                )
                .Returns(Task.CompletedTask);
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.SetupGet(x => x.Worker).Returns(new BackgroundWorker());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            await _controller.RunAsync(progress);

            mockDataModel.Verify(
                m =>
                    m.InitEmailQueueAsync(
                        0,
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    ),
                Times.Once,
                "high-confidence RunAsync initialization must not request an unfiltered first page"
            );
            mockDataModel.Verify(
                m =>
                    m.DequeueNextItemGroupAsync(
                        itemsPerIteration,
                        200,
                        QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
                        It.Is<System.Action<int, int, int>>(sink => sink != null)
                    ),
                Times.Once,
                "the first displayed high-confidence page must come from dequeue-time filtering, "
                    + "bounded by the default first-batch deadline and reporting scan progress"
            );
            mockFormController.Verify(
                m =>
                    m.LoadItemsAsync(
                        It.Is<IList<MailItem>>(items =>
                            items.Count == 1 && ReferenceEquals(items[0], streamedCandidate)
                        )
                    ),
                Times.Once,
                "RunAsync must load the streamed high-confidence candidate batch"
            );
            mockFormController.Verify(
                m =>
                    m.LoadItemsAsync(
                        It.Is<IList<MailItem>>(items => items == unfilteredInitialBatch)
                    ),
                Times.Never,
                "RunAsync must not load the unfiltered initialization batch"
            );
        }

        /// <summary>
        /// Verifies disabled high-confidence mode does not invoke the pre-filter and uses the plain
        /// <see cref="MailItem"/> list overload unchanged.
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
        /// Verifies disabled high-confidence mode constructs item groups through the plain
        /// <see cref="MailItem"/> list overload only.
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

        /// <summary>
        /// Issue #424 AC 6: the progress sink RunAsync hands to the dequeue overload maps gate
        /// progress into the controller's 0-30 band. Every report the tracker receives between the
        /// "Initializing Email Queue" and "Initializing Qfc Items" reports must lie within [0, 30]
        /// and the sequence must be monotonically non-decreasing.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            ProgressTracker progress = _mockProgressTracker.Object;
            const int itemsPerIteration = 4;

            var reports = new List<(double Value, string Label)>();
            _mockProgressTracker
                .Setup(x => x.Report(It.IsAny<double>(), It.IsAny<string>()))
                .Callback<double, string>((value, label) => reports.Add((value, label)));

            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x =>
                    x.InitEmailQueueAsync(
                        0,
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            // The mock captures the sink and drives it with a scripted scan before returning.
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupAsync(
                        itemsPerIteration,
                        200,
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    )
                )
                .Returns(
                    (
                        int quantity,
                        int timeOut,
                        TimeSpan deadline,
                        System.Action<int, int, int> sink
                    ) =>
                    {
                        sink(1, 0, quantity);
                        sink(2, 1, quantity);
                        sink(3, 1, quantity);
                        sink(4, 2, quantity);
                        sink(5, 4, quantity);
                        return Task.FromResult<IList<MailItem>>(new List<MailItem>());
                    }
                );
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<MailItem>>()))
                .Returns(Task.CompletedTask);
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.SetupGet(x => x.Worker).Returns(new BackgroundWorker());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            // Act
            await _controller.RunAsync(progress);

            // Assert — isolate the reports emitted between the two label reports.
            int start = reports.FindIndex(r => r.Label == "Initializing Email Queue");
            int end = reports.FindIndex(r => r.Label == "Initializing Qfc Items");
            start.Should().BeGreaterThanOrEqualTo(0, "RunAsync opens with the queue-init report");
            end.Should().BeGreaterThan(start, "the Qfc-items report closes the scanning window");

            List<(double Value, string Label)> scanReports = reports
                .Skip(start + 1)
                .Take(end - start - 1)
                .ToList();

            scanReports.Should().HaveCount(5, "one mapped report per scripted gate signal");
            scanReports
                .Should()
                .OnlyContain(r => r.Value >= 0 && r.Value <= 30, "reports stay inside the band");
            scanReports
                .Should()
                .OnlyContain(r => r.Label.StartsWith("Scanning for high-confidence items"));
            for (int i = 1; i < scanReports.Count; i++)
            {
                scanReports[i]
                    .Value.Should()
                    .BeGreaterThanOrEqualTo(
                        scanReports[i - 1].Value,
                        "mapped progress must be monotonically non-decreasing"
                    );
            }

            reports[start].Value.Should().Be(0);
            reports[end].Value.Should().Be(30);
        }

        /// <summary>
        /// Issue #424 AC 2: when the deadline expires with nothing accepted, the empty batch still
        /// reaches <c>LoadItemsAsync</c> (an empty list is not short-circuited by the null-guard at
        /// <c>QfcFormController.Actions.cs:68-79</c>) and background iteration is still initiated.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            ProgressTracker progress = _mockProgressTracker.Object;
            const int itemsPerIteration = 6;
            var sinkInvoked = false;

            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x =>
                    x.InitEmailQueueAsync(
                        0,
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupAsync(
                        itemsPerIteration,
                        200,
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    )
                )
                .Returns(
                    (
                        int quantity,
                        int timeOut,
                        TimeSpan deadline,
                        System.Action<int, int, int> sink
                    ) =>
                    {
                        sink(9, 0, quantity);
                        sinkInvoked = true;
                        return Task.FromResult<IList<MailItem>>(new List<MailItem>());
                    }
                );
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<MailItem>>()))
                .Returns(Task.CompletedTask);
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.SetupGet(x => x.Worker).Returns(new BackgroundWorker());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            // Act
            await _controller.RunAsync(progress);

            // Assert
            sinkInvoked
                .Should()
                .BeTrue("the gate reports scan progress even when nothing qualifies");
            mockFormController.Verify(
                m => m.LoadItemsAsync(It.Is<IList<MailItem>>(items => items.Count == 0)),
                Times.Once,
                "an empty first batch must still reach the form path, not be short-circuited"
            );
            mockDataModel.Verify(
                m => m.Complete,
                Times.AtLeastOnce,
                "background iteration must still be initiated after the empty first batch"
            );
        }
    }
}
