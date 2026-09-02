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
    /// <summary>
    /// Issue #424 scan-progress and empty-batch tests for high-confidence
    /// <c>QfcHomeController.RunAsync</c>. Relocated here from
    /// <c>QfcHomeControllerRunAsyncHighConfidenceTests.cs</c>: the issue #678 rewrite of the
    /// enabled-mode dequeue setups onto the outcome-returning member took that file from 473
    /// lines to 544, past the 500-line limit. Both tests moved with their bodies otherwise
    /// unchanged. This is a further part of the same partial class, which already carries its
    /// <c>[TestClass]</c> attribute on the base file <c>QfcHomeControllerRunAsyncTests.cs</c>.
    /// </summary>
    public partial class QfcHomeControllerRunAsyncTests
    {
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
            // Issue #678: the enabled-mode call site moved to the outcome-returning member, so the
            // sink is captured from that member instead. The scripted scan and the four argument
            // constraints are unchanged, so the 0-30 band assertion still measures what issue #424
            // wrote it to measure.
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupWithOutcomeAsync(
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
                        return Task.FromResult(
                            new QfcDequeueBatch(
                                new List<MailItem>(),
                                new List<QfcPreScoredItem>(),
                                QfcDequeueStop.QuantitySatisfied
                            )
                        );
                    }
                );
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            // Issue #678: enabled mode loads through the carrier overload.
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()))
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
            // Issue #678: the enabled-mode call site moved to the outcome-returning member.
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupWithOutcomeAsync(
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
                        return Task.FromResult(
                            new QfcDequeueBatch(
                                new List<MailItem>(),
                                new List<QfcPreScoredItem>(),
                                QfcDequeueStop.DeadlineExpired
                            )
                        );
                    }
                );
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            // Issue #678: enabled mode loads through the carrier overload.
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()))
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
                m =>
                    m.LoadItemsAsync(
                        It.Is<IList<QfcPreScoredItem>>(carriers => carriers.Count == 0)
                    ),
                Times.Once,
                "an empty first batch must still reach the form path, not be short-circuited; the "
                    + "carrier overload's guard is null-not-empty, exactly as the plain one's is"
            );
            mockDataModel.Verify(
                m => m.Complete,
                Times.AtLeastOnce,
                "background iteration must still be initiated after the empty first batch"
            );
        }
    }
}
