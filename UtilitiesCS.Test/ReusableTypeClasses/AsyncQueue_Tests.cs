using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class AsyncQueue_Tests
    {
        [TestMethod]
        public async Task Enqueue_ThenMoveNextAsync_YieldsQueuedItem()
        {
            // Arrange
            var queue = new AsyncQueue<int>();
            using var cancellationTokenSource = new CancellationTokenSource();
            await using var enumerator = queue.GetAsyncEnumerator(cancellationTokenSource.Token);
            queue.Enqueue(42);

            // Act
            var moved = await enumerator.MoveNextAsync();

            // Assert
            moved.Should().BeTrue();
            enumerator.Current.Should().Be(42);
        }

        [TestMethod]
        public async Task EmptyQueue_MoveNextAsync_WaitsUntilItemIsEnqueued()
        {
            // Arrange
            var queue = new AsyncQueue<string>();
            using var cancellationTokenSource = new CancellationTokenSource();
            await using var enumerator = queue.GetAsyncEnumerator(cancellationTokenSource.Token);
            var pendingMove = enumerator.MoveNextAsync().AsTask();

            // Assert precondition
            pendingMove.IsCompleted.Should().BeFalse();

            // Act
            queue.Enqueue("payload");
            var moved = await pendingMove;

            // Assert
            moved.Should().BeTrue();
            enumerator.Current.Should().Be("payload");
        }

        [TestMethod]
        public async Task CanceledEnumeration_MoveNextAsync_ThrowsOperationCanceledException()
        {
            // Arrange
            var queue = new AsyncQueue<int>();
            using var cancellationTokenSource = new CancellationTokenSource();
            await using var enumerator = queue.GetAsyncEnumerator(cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            // Act
            var act = async () => await enumerator.MoveNextAsync();

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task ConcurrentEnqueue_AndSequentialDequeue_PreserveAllItems()
        {
            // Arrange
            var queue = new AsyncQueue<int>();
            var values = Enumerable.Range(1, 20).ToArray();
            using var cancellationTokenSource = new CancellationTokenSource();
            await using var enumerator = queue.GetAsyncEnumerator(cancellationTokenSource.Token);

            // Act
            await Task.WhenAll(values.Select(value => Task.Run(() => queue.Enqueue(value))));
            var received = new List<int>();
            for (var index = 0; index < values.Length; index++)
            {
                var moved = await enumerator.MoveNextAsync();
                moved.Should().BeTrue();
                received.Add(enumerator.Current);
            }

            // Assert
            received.OrderBy(value => value).Should().Equal(values);
        }
    }
}
