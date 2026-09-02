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
            // Issue #678: enabled-mode RunAsync reads the outcome-returning member, which is the
            // only overload that surfaces the carriers. The plain overloads above stay configured
            // so the disabled-mode tests in this class continue to exercise their own path.
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupWithOutcomeAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    )
                )
                .ReturnsAsync(
                    new QfcDequeueBatch(
                        new List<MailItem>(),
                        new List<QfcPreScoredItem>(),
                        QfcDequeueStop.QuantitySatisfied
                    )
                );
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
            // Issue #678: the streamed page now arrives as carriers, each pairing the candidate with
            // the folder handler the gate already initialised for it.
            var streamedHandler = new Mock<IFolderSearchHandler>().Object;
            var streamedCarriers = new List<QfcPreScoredItem>
            {
                new QfcPreScoredItem(streamedCandidate, @"\\A\streamed", streamedHandler),
            };
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
            // Issue #678: that call site moved again, to the outcome-returning member. The four
            // argument constraints are unchanged, so the deadline bound and the progress sink stay
            // pinned exactly as issue #424 left them.
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupWithOutcomeAsync(
                        itemsPerIteration,
                        200,
                        QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
                        It.Is<System.Action<int, int, int>>(sink => sink != null)
                    )
                )
                .ReturnsAsync(
                    new QfcDequeueBatch(
                        streamedBatch,
                        streamedCarriers,
                        QfcDequeueStop.QuantitySatisfied
                    )
                );
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            mockFormController
                .Setup(x =>
                    x.LoadItemsAsync(
                        It.Is<IList<QfcPreScoredItem>>(carriers =>
                            carriers.Count == 1
                            && ReferenceEquals(carriers[0].MailItem, streamedCandidate)
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
                    m.DequeueNextItemGroupWithOutcomeAsync(
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
                        It.Is<IList<QfcPreScoredItem>>(carriers =>
                            carriers.Count == 1
                            && ReferenceEquals(carriers[0].MailItem, streamedCandidate)
                            && ReferenceEquals(carriers[0].FolderHandler, streamedHandler)
                        )
                    ),
                Times.Once,
                "RunAsync must load the streamed high-confidence candidate batch as carriers, "
                    + "each still holding the folder handler the gate initialised for it"
            );
            // Issue #678: this must constrain the CARRIER overload. Left on the IList<MailItem>
            // form it would be satisfied trivially after the change, because that overload is no
            // longer invoked at all in enabled mode, so the assertion would hold whatever the
            // production code did with the unfiltered batch.
            mockFormController.Verify(
                m =>
                    m.LoadItemsAsync(
                        It.Is<IList<QfcPreScoredItem>>(carriers =>
                            carriers.Count == unfilteredInitialBatch.Count
                            && carriers.Count > 0
                            && ReferenceEquals(carriers[0].MailItem, unfilteredInitialBatch[0])
                        )
                    ),
                Times.Never,
                "RunAsync must not load a carrier list projected from the unfiltered "
                    + "initialization batch"
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
        // RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand and
        // RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration live in the
        // partial part QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs; see that file.
    }
}
