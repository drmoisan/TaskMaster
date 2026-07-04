using System.Collections.Generic;
using System.ComponentModel;
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
            mockDataModel
                .Setup(x => x.DequeueNextItemGroupAsync(itemsPerIteration, 1000))
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
                m => m.DequeueNextItemGroupAsync(itemsPerIteration, 1000),
                Times.Once,
                "the first displayed high-confidence page must come from dequeue-time filtering"
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
    }
}
