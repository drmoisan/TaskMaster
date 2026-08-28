using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the pure, Outlook-free queue-state paths of <see cref="QfcQueue"/>. The
    /// instance is constructed with a null home controller and a mocked
    /// <see cref="IApplicationGlobals"/> (the established pattern used by QfcQueueTests); the
    /// primary constructor merely stores these. Only paths that do not touch the WinForms
    /// TableLayoutPanel graph, MailItems, or the UI dispatcher are exercised: the Count and
    /// JobsRunning accessors, the empty-queue early return of TryDequeueAsync, and the
    /// no-jobs-running fast paths of CompleteAddingAsync and JobsToFinish. The TLP/MailItem/
    /// dispatcher-bound members are out of scope (Outlook/WinForms) per the seam verification.
    /// </summary>
    [TestClass]
    public class QfcQueuePurePathsTests
    {
        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private static QfcQueue NewQueue(CancellationToken token)
        {
            var globals = new Mock<IApplicationGlobals>().Object;
            return new QfcQueue(token, (QfcHomeController)null, globals);
        }

        private static QfcDatamodel CreateUninitializedDatamodel() =>
            (QfcDatamodel)FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull($"private field '{name}' should exist on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        [TestMethod]
        public void NewQueue_HasZeroCountAndZeroJobsRunning()
        {
            // Arrange / Act
            var queue = NewQueue(CancellationToken.None);

            // Assert
            queue.Count.Should().Be(0, "a fresh queue holds no entries");
            queue.JobsRunning.Should().Be(0, "a fresh queue has no jobs running");
        }

        [TestMethod]
        public async Task TryDequeueAsync_EmptyQueueNoJobs_ReturnsDefault()
        {
            // Arrange: empty queue and no jobs trigger the documented early return.
            var queue = NewQueue(CancellationToken.None);

            // Act
            var result = await queue.TryDequeueAsync(CancellationToken.None, timeout: 50);

            // Assert
            result
                .Should()
                .Be(default, "an empty idle queue returns the default tuple immediately");
        }

        [TestMethod]
        public async Task CompleteAddingAsync_NoJobsRunning_CompletesWithoutThrowing()
        {
            // Arrange: with _jobsRunning == 0 the while loop is skipped and CompleteAdding is called.
            var queue = NewQueue(CancellationToken.None);

            // Act
            await queue
                .Awaiting(q => q.CompleteAddingAsync(CancellationToken.None, timeout: 100))
                .Should()
                .NotThrowAsync("no running jobs means the method completes adding immediately");
        }

        [TestMethod]
        public async Task JobsToFinish_NoJobsRunning_CompletesImmediately()
        {
            // Arrange
            var queue = NewQueue(CancellationToken.None);

            // Act
            await queue
                .Awaiting(q => q.JobsToFinish(100, CancellationToken.None))
                .Should()
                .NotThrowAsync("with no jobs running the polling loop exits immediately");
        }

        [TestMethod]
        public async Task DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue()
        {
            var model = CreateUninitializedDatamodel();
            var first = new Mock<MailItem>().Object;
            var second = new Mock<MailItem>().Object;
            var masterQueue = new LockingLinkedList<MailItem>();
            masterQueue.AddLast(first);
            masterQueue.AddLast(second);

            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(false);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            moveMonitor.Setup(x => x.UnhookItem(first));
            moveMonitor.Setup(x => x.UnhookItem(second));

            SetPrivateField(model, "_globals", globals.Object);
            SetPrivateField(model, "_masterQueue", masterQueue);
            SetPrivateField(model, "_moveMonitor", moveMonitor.Object);
            SetPrivateField(model, "_worker", new BackgroundWorker());

            IList<MailItem> result = await model.DequeueNextItemGroupAsync(2, 0);

            result.Should().Equal(first, second);
            masterQueue.Count.Should().Be(0);
            moveMonitor.Verify(x => x.UnhookItem(first), Times.Once);
            moveMonitor.Verify(x => x.UnhookItem(second), Times.Once);
        }

        /// <summary>
        /// Issue #426. A candidate the high-confidence dequeue gate discards has already been
        /// removed from the master queue and never reaches <c>UnhookDequeuedNodes</c>, so its
        /// <c>EmailMoveMonitor</c> hook and its live COM reference are retained for the session.
        /// The datamodel must release the hook through its OWN monitor instance exactly once.
        /// Scoring is driven through the <c>ScoringServiceFactory</c> seam so no live Outlook COM
        /// is touched.
        /// </summary>
        [TestMethod]
        public async Task DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var rejectedItem = new Mock<MailItem>().Object;
            var masterQueue = new LockingLinkedList<MailItem>();
            masterQueue.AddLast(rejectedItem);

            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(x => x.HighConfidenceThreshold).Returns(0.90);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            var scoringService = new Mock<IFolderScoringService>(MockBehavior.Strict);
            scoringService
                .Setup(x =>
                    x.ScoreAsync(
                        rejectedItem,
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync((100L, string.Empty));

            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            moveMonitor.Setup(x => x.UnhookItem(rejectedItem));

            SetPrivateField(model, "_globals", globals.Object);
            SetPrivateField(model, "_masterQueue", masterQueue);
            SetPrivateField(model, "_moveMonitor", moveMonitor.Object);
            SetPrivateField(model, "_worker", new BackgroundWorker());
            model.ScoringServiceFactory = () => scoringService.Object;

            // Act
            IList<MailItem> result = await model.DequeueNextItemGroupAsync(1, 0);

            // Assert
            result.Should().BeEmpty("the drop-on-reject contract is unchanged");
            masterQueue.Count.Should().Be(0, "the rejected candidate is still removed from source");
            moveMonitor.Verify(
                x => x.UnhookItem(rejectedItem),
                Times.Once,
                "the datamodel must release the rejected candidate's monitor hook exactly once"
            );
        }

        /// <summary>
        /// Issue #446. A gate result produced by first-batch deadline expiry must be projected
        /// through the datamodel as <c>QfcDequeueStop.DeadlineExpired</c> rather than folded into
        /// the generic quantity-satisfied outcome, otherwise the caller cannot tell a
        /// deadline-bounded empty batch from genuine exhaustion. Driven by
        /// <see cref="FakeTimeProvider"/>: every score consumes one second of a three-second budget
        /// and nothing qualifies, so the deadline exit is the one the gate takes.
        /// </summary>
        [TestMethod]
        public async Task DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var fake = new FakeTimeProvider();
            model.TimeProvider = fake;

            var masterQueue = new LockingLinkedList<MailItem>();
            for (int i = 0; i < 10; i++)
            {
                masterQueue.AddLast(new Mock<MailItem>().Object);
            }

            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(x => x.HighConfidenceThreshold).Returns(0.90);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            var scoringService = new Mock<IFolderScoringService>(MockBehavior.Strict);
            scoringService
                .Setup(x =>
                    x.ScoreAsync(
                        It.IsAny<MailItem>(),
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(() =>
                {
                    fake.Advance(TimeSpan.FromSeconds(1));
                    return Task.FromResult((100L, string.Empty));
                });

            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            SetPrivateField(model, "_globals", globals.Object);
            SetPrivateField(model, "_masterQueue", masterQueue);
            SetPrivateField(model, "_moveMonitor", moveMonitor.Object);
            SetPrivateField(model, "_worker", new BackgroundWorker());
            SetPrivateField(model, "_remainingLoadActive", true);
            model.ScoringServiceFactory = () => scoringService.Object;

            // Act
            QfcDequeueBatch batch = await model.DequeueNextItemGroupWithOutcomeAsync(
                1,
                0,
                TimeSpan.FromSeconds(3),
                null
            );

            // Assert
            batch.Items.Should().BeEmpty("no candidate qualified before the deadline");
            batch
                .Stop.Should()
                .Be(
                    QfcDequeueStop.DeadlineExpired,
                    "a deadline-bounded empty batch must not be reported as quantity satisfaction"
                );
        }
    }
}
