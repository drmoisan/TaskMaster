using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Queue-iteration tests that constrain the arguments <c>IterateQueueAsync</c> hands to
    /// <c>IQfcQueue.EnqueueAsync</c>, including the issue #678 leg-B carrier forwarding. Relocated
    /// here from <c>QfcHomeControllerIterationTests.cs</c>, which stood at 497 lines with three
    /// lines of headroom to the 500-line cap; widening the enqueue setup and both verifications for
    /// the new third parameter, and adding the carrier-forwarding test, would have breached it.
    /// No test is deleted or weakened by the move; the base part carries the only <c>[TestClass]</c>
    /// attribute and the shared <c>ArrangeIterate</c> / <c>VerifyCompleteAdding</c> helpers.
    /// </summary>
    public partial class QfcHomeControllerIterationTests
    {
        [TestMethod]
        public async Task IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems()
        {
            var mailItems = Enumerable
                .Range(0, 8)
                .Select(_ => new Mock<MailItem>().Object)
                .ToList();
            var (_, mockQfcQueue, _, mockQfcCollectionController) = ArrangeIterate(
                q => q == 8,
                t => t == 2000,
                dequeued: mailItems
            );

            await _controller.IterateQueueAsync();

            mockQfcQueue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.Is<IList<MailItem>>(items => items.SequenceEqual(mailItems)),
                        mockQfcCollectionController.Object,
                        It.IsAny<IList<QfcPreScoredItem>>()
                    ),
                Times.Once
            );
            VerifyCompleteAdding(mockQfcQueue, Times.Never);
        }

        /// <summary>
        /// AC6 (issue #678), leg B. Every page after the first is built by
        /// <c>IterateQueueAsync</c> handing the dequeued batch to <c>IQfcQueue.EnqueueAsync</c>.
        /// Before this change only <c>batch.Items</c> was forwarded, so the carriers on
        /// <c>batch.PreScored</c> — and with them the folder search handler the dequeue-time gate
        /// had already initialised — were dropped at that hop and every displayed row re-scored its
        /// own item. This test pins that the carrier list reaches the queue intact: same count, and
        /// the same handler instance associated with the same mail item.
        /// </summary>
        [TestMethod]
        public async Task IterateQueueAsync_WhenBatchCarriesPreScoredItems_ForwardsCarriersToEnqueue()
        {
            // Arrange — a one-item batch whose carrier publishes a distinguishable handler.
            MailItem mailItem = new Mock<MailItem>().Object;
            IFolderSearchHandler carriedHandler = new Mock<IFolderSearchHandler>().Object;
            IList<MailItem> items = new List<MailItem> { mailItem };
            IList<QfcPreScoredItem> carriers = new List<QfcPreScoredItem>
            {
                new QfcPreScoredItem(mailItem, @"\\Archive\Projects\Active", carriedHandler),
            };

            var (_, mockQfcQueue, _, mockQfcCollectionController) = ArrangeIterate(
                dequeued: items,
                outcome: () =>
                    Task.FromResult(
                        new QfcDequeueBatch(items, carriers, QfcDequeueStop.QuantitySatisfied)
                    )
            );

            // Act
            await _controller.IterateQueueAsync();

            // Assert — the carriers reach EnqueueAsync as its third argument, carrying the handler.
            mockQfcQueue.Verify(
                m =>
                    m.EnqueueAsync(
                        It.IsAny<IList<MailItem>>(),
                        mockQfcCollectionController.Object,
                        It.Is<IList<QfcPreScoredItem>>(forwarded =>
                            forwarded != null
                            && forwarded.Count == 1
                            && ReferenceEquals(forwarded[0].MailItem, mailItem)
                            && ReferenceEquals(forwarded[0].FolderHandler, carriedHandler)
                        )
                    ),
                Times.Once,
                "leg B must forward batch.PreScored so pages after the first arrive with the "
                    + "already-initialised folder handler instead of re-scoring every row"
            );
        }
    }
}
