using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    public partial class QfcHomeControllerIterationTests
    {
        private MockRepository _mockRepository;
        private Mock<IApplicationGlobals> _mockApplicationGlobals;
        private Mock<System.Action> _mockParentCleanup;
        private QfcHomeController _controller;
        private Mock<Outlook.Application> _mockOlApp;
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

        private void SetupQfSettings(bool highConfidenceEnabled, double threshold)
        {
            var qfSettings = this._mockRepository.Create<IAppQuickFilerSettings>();
            qfSettings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(highConfidenceEnabled);
            qfSettings.SetupGet(x => x.HighConfidenceThreshold).Returns(threshold);
            this._mockApplicationGlobals.SetupGet(x => x.QfSettings).Returns(qfSettings.Object);
        }

        /// <summary>Matcher accepting any value; the default for <c>ArrangeIterate</c>.</summary>
        private static Expression<Func<int, bool>> AnyValue => x => true;

        /// <summary>
        /// Shared arrangement for the queue-iteration tests; returns the mocks it wires into
        /// <c>_controller</c>. The dequeue matchers are expressions, not values, so a pinned call
        /// site stays pinned (<c>q =&gt; q == 8</c>) instead of widening to <c>It.IsAny</c>;
        /// <c>outcome</c> replaces the dequeue result and is how the exception tests throw.
        /// </summary>
        private (
            Mock<IQfcDatamodel> DataModel,
            Mock<IQfcQueue> Queue,
            Mock<IQfcFormController> FormController,
            Mock<IQfcCollectionController> Groups
        ) ArrangeIterate(
            Expression<Func<int, bool>> quantity = null,
            Expression<Func<int, bool>> timeOut = null,
            bool complete = false,
            IList<MailItem> dequeued = null,
            int itemsPerIteration = 8,
            QfcDequeueStop stop = QfcDequeueStop.QuantitySatisfied,
            Func<Task<QfcDequeueBatch>> outcome = null
        )
        {
            IList<MailItem> batch = dequeued ?? new List<MailItem>();
            quantity = quantity ?? AnyValue;
            timeOut = timeOut ?? AnyValue;
            if (outcome == null)
            {
                outcome = () => Task.FromResult(new QfcDequeueBatch(batch, null, stop));
            }

            var dataModel = new Mock<IQfcDatamodel>();
            dataModel.Setup(m => m.Complete).Returns(complete);
            dataModel
                .Setup(m => m.DequeueNextItemGroupAsync(It.Is(quantity), It.Is(timeOut)))
                .Returns(Task.FromResult(batch));
            dataModel
                .Setup(m =>
                    m.DequeueNextItemGroupWithOutcomeAsync(
                        It.Is(quantity),
                        It.Is(timeOut),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<Action<int, int, int>>()
                    )
                )
                .Returns(outcome);

            var queue = new Mock<IQfcQueue>();
            queue
                .Setup(m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);
            queue
                .Setup(m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>(),
                        It.IsAny<IList<QfcPreScoredItem>>()
                    )
                )
                .Returns(Task.CompletedTask);

            var groups = new Mock<IQfcCollectionController>();
            var formController = new Mock<IQfcFormController>();
            formController.SetupGet(m => m.ItemsPerIteration).Returns(itemsPerIteration);
            formController.Setup(m => m.Groups).Returns(groups.Object);

            _controller.DataModel = dataModel.Object;
            _controller.QfcQueue = queue.Object;
            SetPrivateField("_formController", formController.Object);

            return (dataModel, queue, formController, groups);
        }

        /// <summary>Assigns a private instance field on the controller under test.</summary>
        private void SetPrivateField(string name, object value) =>
            _controller
                .GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_controller, value);

        /// <summary>Verifies the complete-adding invocation count on the queue mock.</summary>
        private static void VerifyCompleteAdding(
            Mock<IQfcQueue> queue,
            Func<Times> times,
            string because = null
        ) =>
            queue.Verify(
                m => m.CompleteAddingAsync(It.IsAny<CancellationToken>(), It.IsAny<int>()),
                times,
                because
            );

        /// <summary>Verifies the unconstrained enqueue invocation count on the queue mock.</summary>
        private static void VerifyEnqueue(Mock<IQfcQueue> queue, Func<Times> times) =>
            queue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        It.IsAny<IQfcCollectionController>(),
                        It.IsAny<IList<QfcPreScoredItem>>()
                    ),
                times
            );

        [TestMethod]
        public async Task IterateQueueAsync_DataModelComplete()
        {
            // Arrange
            var (mockDataModel, mockQfcQueue, _, _) = ArrangeIterate(complete: true);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(
                m =>
                    m.DequeueNextItemGroupWithOutcomeAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<Action<int, int, int>>()
                    ),
                Times.Never
            );
            VerifyCompleteAdding(mockQfcQueue, Times.Never);
            VerifyEnqueue(mockQfcQueue, Times.Never);
        }

        [TestMethod]
        public async Task IterateQueueAsync_QueueEmpty()
        {
            // Arrange — issue #446 made an empty batch insufficient on its own to close the queue,
            // so the drained-source stop is now stated explicitly. The assertions are unchanged.
            var (mockDataModel, mockQfcQueue, _, _) = ArrangeIterate(
                stop: QfcDequeueStop.SourceExhausted
            );

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(
                m =>
                    m.DequeueNextItemGroupWithOutcomeAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<Action<int, int, int>>()
                    ),
                Times.Once
            );
            VerifyCompleteAdding(mockQfcQueue, Times.Once);
            VerifyEnqueue(mockQfcQueue, Times.Never);
        }

        [TestMethod]
        public async Task IterateQueueAsync_Queue2()
        {
            // Arrange

            // Setup DequeueNextItemGroupAsync to return 2 mail items
            var mockMailItem = new Mock<MailItem>();
            IList<MailItem> mailItems = new List<MailItem>
            {
                mockMailItem.Object,
                mockMailItem.Object,
            };
            var (mockDataModel, mockQfcQueue, _, _) = ArrangeIterate(dequeued: mailItems);

            // Act
            await _controller.IterateQueueAsync();

            // Assert
            mockDataModel.Verify(
                m =>
                    m.DequeueNextItemGroupWithOutcomeAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<Action<int, int, int>>()
                    ),
                Times.Once
            );
            VerifyCompleteAdding(mockQfcQueue, Times.Never);
            VerifyEnqueue(mockQfcQueue, Times.Once);
        }

        // IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems and the issue #678
        // carrier-forwarding test live in the partial part QfcHomeControllerIterationTests.Part2.cs;
        // see that file for the reason.

        [TestMethod]
        public void Iterate_ExecutesCorrectly()
        {
            // Arrange

            // Setup the DataModel to return 2 mocked mail items
            var mockDataModel = new Mock<IQfcDatamodel>();
            var mockMailItem = new Mock<MailItem>();
            IList<MailItem> mailItems = new List<MailItem>
            {
                mockMailItem.Object,
                mockMailItem.Object,
            };
            mockDataModel.Setup(m => m.DequeueNextItemGroup(It.IsAny<int>())).Returns(mailItems);
            _controller.DataModel = mockDataModel.Object;
            SetupQfSettings(highConfidenceEnabled: false, threshold: 0.90);

            var mockFormController = new Mock<IQfcFormController>();
            SetPrivateField("_formController", mockFormController.Object);

            // Act
            _controller.Iterate();

            // Assert
            mockDataModel.Verify(m => m.DequeueNextItemGroup(It.IsAny<int>()), Times.Once);

            mockFormController.Verify(
                m =>
                    m.LoadItems(
                        It.Is<IList<MailItem>>(items =>
                            items.Count == 2 && items.Contains(mockMailItem.Object)
                        )
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch()
        {
            // Arrange
            var itemsPerIteration = 8;
            var directBatch = new List<MailItem>
            {
                new Mock<MailItem>().Object,
                new Mock<MailItem>().Object,
            };

            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var (mockDataModel, _, mockFormController, _) = ArrangeIterate(
                q => q == itemsPerIteration,
                itemsPerIteration: itemsPerIteration
            );
            mockDataModel.Setup(m => m.DequeueNextItemGroup(It.IsAny<int>())).Returns(directBatch);
            mockFormController.Setup(m => m.LoadItems(It.IsAny<IList<MailItem>>()));

            // Act
            _controller.Iterate();

            // Assert
            mockDataModel.Verify(
                m => m.DequeueNextItemGroup(itemsPerIteration),
                Times.Never,
                "high-confidence synchronous iteration must not use the direct dequeue bypass"
            );
            mockFormController.Verify(
                m => m.LoadItems(directBatch),
                Times.Never,
                "high-confidence synchronous iteration must not load an ungated direct batch"
            );
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
            SetPrivateField("_formController", mockFormController.Object);
            _controller.DataModel = mockDataModel.Object;

            // Act
            _controller.Iterate2();

            // Assert
            mockQfcQueue.Verify(m => m.Dequeue(), Times.Once);
            mockFormController.Verify(
                m => m.LoadItems(It.IsAny<TableLayoutPanel>(), It.IsAny<List<QfcItemGroup>>()),
                Times.Once
            );
        }

        [TestMethod]
        public void SwapStopWatch_ExecutesCorrectly()
        {
            // Arrange
            var stopWatch = new Stopwatch();
            SetPrivateField("_stopWatch", stopWatch);

            // Act
            _controller.SwapStopWatch();

            // Assert
            var actual =
                _controller
                    .GetType()
                    .GetField(
                        "_stopWatchMoved",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    )
                    .GetValue(_controller) as Stopwatch;
            Assert.AreEqual(stopWatch, actual);
        }

        /// <summary>
        /// Issue #446. A deadline-expired empty batch is not source exhaustion — the master queue
        /// may still hold unscanned items — so it must not irreversibly close the UI queue.
        /// </summary>
        [TestMethod]
        public async Task IterateQueueAsync_EmptyBatchWithDeadlineExpired_DoesNotCompleteAdding()
        {
            var (_, queue, _, _) = ArrangeIterate(stop: QfcDequeueStop.DeadlineExpired);

            await _controller.IterateQueueAsync();

            VerifyCompleteAdding(
                queue,
                Times.Never,
                "a deadline-bounded empty batch must not close the queue"
            );
        }

        /// <summary>
        /// Issue #446 negative control for AC2: a genuinely drained source SHOULD close the queue,
        /// so a fix that merely stopped calling <c>CompleteAddingAsync</c> would break this test.
        /// </summary>
        [TestMethod]
        public async Task IterateQueueAsync_EmptyBatchWithSourceExhausted_CompletesAddingOnce()
        {
            var (_, queue, _, _) = ArrangeIterate(stop: QfcDequeueStop.SourceExhausted);

            await _controller.IterateQueueAsync();

            VerifyCompleteAdding(
                queue,
                Times.Once,
                "a drained source is the one empty-batch case that may close the queue"
            );
        }

        /// <summary>
        /// Issue #446 coverage: an <c>OperationCanceledException</c> from the dequeue is swallowed.
        /// </summary>
        [TestMethod]
        public async Task IterateQueueAsync_DequeueThrowsOperationCanceled_SwallowsAndReturns()
        {
            ArrangeIterate(outcome: () => throw new OperationCanceledException());

            Func<Task> act = () => _controller.IterateQueueAsync();

            await act.Should().NotThrowAsync("a cancelled dequeue must not surface to the caller");
        }

        /// <summary>
        /// Issue #446 coverage: a fault raised while cancellation is pending is swallowed. The
        /// token is cancelled from INSIDE the dequeue callback because the entry-guard
        /// <c>Token.ThrowIfCancellationRequested()</c> sits outside the try block, so a token
        /// already cancelled at entry escapes uncaught and never reaches this branch.
        /// </summary>
        [TestMethod]
        public async Task IterateQueueAsync_DequeueThrowsWhenTokenCancelled_SwallowsAndReturns()
        {
            var source = new CancellationTokenSource();
            SetPrivateField("_token", source.Token);
            ArrangeIterate(outcome: () =>
            {
                source.Cancel();
                throw new InvalidOperationException("dequeue failed");
            });

            Func<Task> act = () => _controller.IterateQueueAsync();

            await act.Should().NotThrowAsync("a fault raised after cancellation is a cancellation");
        }

        /// <summary>
        /// Issue #446 coverage: a fault with no cancellation pending is rethrown, not swallowed.
        /// </summary>
        [TestMethod]
        public async Task IterateQueueAsync_DequeueThrowsWhenTokenNotCancelled_Rethrows()
        {
            var fault = new InvalidOperationException("dequeue failed");
            ArrangeIterate(outcome: () => throw fault);

            Func<Task> act = () => _controller.IterateQueueAsync();

            (await act.Should().ThrowAsync<InvalidOperationException>())
                .Which.Should()
                .BeSameAs(fault);
        }
    }
}
