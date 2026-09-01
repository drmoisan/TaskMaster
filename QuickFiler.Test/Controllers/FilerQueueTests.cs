using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the pure, Outlook-free surface of <see cref="FilerQueue"/> and
    /// <see cref="FilerQueueItem"/>. These cover the queue item's construction and validation
    /// contract, the FilerQueue default consumer state, and the drain barrier.
    /// </summary>
    /// <remarks>
    /// The FilerQueue.Enqueue/ConsumeAsync path is exercised deterministically through the
    /// <c>ItemProcessor</c> seam added for issue 633. The seam replaces the hard-coded call to
    /// <c>EmailFiler.SortAsync</c>, which is Outlook-bound and cannot be driven from a unit test, so
    /// the path is now reachable without any external dependency. Every concurrency assertion below is
    /// driven by <see cref="TaskCompletionSource{TResult}"/> gates through that seam: there is no
    /// sleep, no delay, no polling loop, and no timeout-based assertion.
    /// </remarks>
    [TestClass]
    public class FilerQueueTests
    {
        private static List<MailItemHelper> OneHelper() =>
            new List<MailItemHelper> { new MailItemHelper() };

        [TestMethod]
        public void FilerQueueItem_Constructor_StoresFilerAndHelpers()
        {
            // Arrange
            var filer = new EmailFiler();
            var helpers = OneHelper();

            // Act
            var item = new FilerQueueItem(filer, helpers);

            // Assert
            item.Filer.Should().BeSameAs(filer);
            item.Helpers.Should().BeSameAs(helpers);
        }

        [TestMethod]
        public void FilerQueueItem_Constructor_NullFiler_ThrowsArgumentNullException()
        {
            // Arrange / Act
            Action act = () => new FilerQueueItem(null, OneHelper());

            // Assert
            act.Should().Throw<ArgumentNullException>("a null filer is rejected by ThrowIfNull");
        }

        [TestMethod]
        public void FilerQueueItem_Constructor_NullHelpers_ThrowsArgumentNullException()
        {
            // Arrange / Act
            Action act = () => new FilerQueueItem(new EmailFiler(), null);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("a null helpers list is rejected by ThrowIfNull");
        }

        [TestMethod]
        public void FilerQueueItem_Constructor_HelpersContainingNull_ThrowsArgumentNullException()
        {
            // Arrange: a non-null list whose element is null hits the explicit any-null guard.
            var helpers = new List<MailItemHelper> { null };

            // Act
            Action act = () => new FilerQueueItem(new EmailFiler(), helpers);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>("a null element inside the helpers list is rejected");
        }

        [TestMethod]
        public void FilerQueue_NewInstance_HasCompletedConsumerByDefault()
        {
            // Arrange / Act
            var queue = new FilerQueue();

            // Assert
            queue.Consumer.Should().NotBeNull();
            queue
                .Consumer.IsCompleted.Should()
                .BeTrue("a fresh FilerQueue exposes Task.CompletedTask as its consumer");
        }

        /// <summary>Creates a gate whose continuations do not run inline on the releasing thread.</summary>
        private static TaskCompletionSource<bool> NewGate() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Enqueues one item. Every enqueued item carries a real helper because the preserved
        /// worker <c>catch</c> block calls <c>item.Helpers.First()</c>; an empty list would raise
        /// inside the catch, escape the worker loop, and leave the drain permanently incomplete.
        /// </summary>
        private static void EnqueueOne(FilerQueue queue) =>
            queue.Enqueue(new EmailFiler(), OneHelper());

        [TestMethod]
        public void WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask()
        {
            // Arrange
            var queue = new FilerQueue();

            // Act
            Task drain = queue.WhenDrainedAsync();

            // Assert
            drain
                .IsCompleted.Should()
                .BeTrue("a queue with no outstanding work is already drained");
        }

        [TestMethod]
        public async Task WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes()
        {
            // Arrange
            var queue = new FilerQueue();
            TaskCompletionSource<bool> entered = NewGate();
            TaskCompletionSource<bool> gate = NewGate();
            queue.ItemProcessor = async item =>
            {
                entered.TrySetResult(true);
                await gate.Task;
            };

            try
            {
                // Act
                EnqueueOne(queue);
                await entered.Task;
                Task drain = queue.WhenDrainedAsync();

                // Assert
                drain
                    .IsCompleted.Should()
                    .BeFalse("the item is still inside the processor, so the queue is not drained");
            }
            finally
            {
                gate.TrySetResult(true);
            }
        }

        [TestMethod]
        public async Task WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce()
        {
            // Arrange
            var queue = new FilerQueue();
            TaskCompletionSource<bool> entered = NewGate();
            TaskCompletionSource<bool> gate = NewGate();
            int invocations = 0;
            queue.ItemProcessor = async item =>
            {
                Interlocked.Increment(ref invocations);
                entered.TrySetResult(true);
                await gate.Task;
            };

            try
            {
                EnqueueOne(queue);
                await entered.Task;
                Task drain = queue.WhenDrainedAsync();

                // Act
                gate.TrySetResult(true);
                await drain;

                // Assert
                invocations.Should().Be(1, "the single enqueued item is processed exactly once");
            }
            finally
            {
                gate.TrySetResult(true);
            }
        }

        [TestMethod]
        public async Task WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete()
        {
            // Arrange
            var queue = new FilerQueue();
            var gates = new[] { NewGate(), NewGate() };
            var entries = new[] { NewGate(), NewGate() };
            int invocations = 0;
            queue.ItemProcessor = async item =>
            {
                int index = Interlocked.Increment(ref invocations) - 1;
                entries[index].TrySetResult(true);
                await gates[index].Task;
            };

            try
            {
                EnqueueOne(queue);
                EnqueueOne(queue);
                await entries[0].Task;
                Task drain = queue.WhenDrainedAsync();

                // Act: release only the first item.
                gates[0].TrySetResult(true);
                await entries[1].Task;

                // Assert: the second item is still outstanding.
                drain
                    .IsCompleted.Should()
                    .BeFalse("one of the two items is still being processed");

                // Act: release the second item.
                gates[1].TrySetResult(true);
                await drain;

                // Assert
                invocations.Should().Be(2, "both enqueued items are processed");
            }
            finally
            {
                gates[0].TrySetResult(true);
                gates[1].TrySetResult(true);
            }
        }

        [TestMethod]
        public async Task WhenDrainedAsync_AwaitedTwice_BothWaitersComplete()
        {
            // Arrange
            var queue = new FilerQueue();
            TaskCompletionSource<bool> entered = NewGate();
            TaskCompletionSource<bool> gate = NewGate();
            queue.ItemProcessor = async item =>
            {
                entered.TrySetResult(true);
                await gate.Task;
            };

            try
            {
                EnqueueOne(queue);
                await entered.Task;

                // Act: two waiters obtained before the gate releases.
                Task firstWaiter = queue.WhenDrainedAsync();
                Task secondWaiter = queue.WhenDrainedAsync();
                firstWaiter.IsCompleted.Should().BeFalse("work is still outstanding");
                secondWaiter.IsCompleted.Should().BeFalse("work is still outstanding");

                gate.TrySetResult(true);
                await Task.WhenAll(firstWaiter, secondWaiter);

                // Act: a third call made after the queue is idle.
                Task thirdWaiter = queue.WhenDrainedAsync();

                // Assert
                firstWaiter
                    .IsCompleted.Should()
                    .BeTrue("every waiter completes on the same drain");
                secondWaiter.IsCompleted.Should().BeTrue("no waiter can starve another");
                thirdWaiter
                    .IsCompleted.Should()
                    .BeTrue("a call made after the queue is idle returns a completed task");
            }
            finally
            {
                gate.TrySetResult(true);
            }
        }

        [TestMethod]
        public async Task Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch()
        {
            // Arrange
            var queue = new FilerQueue();
            var gates = new[] { NewGate(), NewGate() };
            var entries = new[] { NewGate(), NewGate() };
            int invocations = 0;
            queue.ItemProcessor = async item =>
            {
                int index = Interlocked.Increment(ref invocations) - 1;
                entries[index].TrySetResult(true);
                await gates[index].Task;
            };

            try
            {
                // Act: first batch, released and drained to completion.
                EnqueueOne(queue);
                await entries[0].Task;
                Task firstDrain = queue.WhenDrainedAsync();
                gates[0].TrySetResult(true);
                await firstDrain;

                // Act: a second batch enqueued after the first has fully drained. This is the
                // orphaned-item regression: under the old one-shot start gate the worker could exit its
                // loop before reinstating the guard, so this item could be stranded with no worker.
                EnqueueOne(queue);
                await entries[1].Task;
                Task secondDrain = queue.WhenDrainedAsync();
                gates[1].TrySetResult(true);
                await secondDrain;

                // Assert
                invocations
                    .Should()
                    .Be(2, "the second batch is processed without any further enqueue");
            }
            finally
            {
                gates[0].TrySetResult(true);
                gates[1].TrySetResult(true);
            }
        }

        [TestMethod]
        public async Task ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes()
        {
            // Arrange
            var queue = new FilerQueue();
            int invocations = 0;
            var secondProcessed = NewGate();
            queue.ItemProcessor = item =>
            {
                int index = Interlocked.Increment(ref invocations) - 1;
                if (index == 0)
                {
                    throw new InvalidOperationException("first item fails");
                }

                secondProcessed.TrySetResult(true);
                return Task.CompletedTask;
            };

            // Act
            EnqueueOne(queue);
            EnqueueOne(queue);
            await secondProcessed.Task;
            Task drain = queue.WhenDrainedAsync();
            await drain;

            // Assert
            drain
                .IsFaulted.Should()
                .BeFalse(
                    "an item failure is logged inside the worker, not propagated to the waiter"
                );
            drain.IsCompleted.Should().BeTrue("the throwing item still decrements the counter");
            invocations.Should().Be(2, "the worker loop continues past the failing item");
        }
    }
}
