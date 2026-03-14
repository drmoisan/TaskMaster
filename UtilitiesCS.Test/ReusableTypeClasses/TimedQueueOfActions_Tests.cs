using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses.TimedActions;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class TimedQueueOfActions_Tests
    {
        [TestMethod]
        public void Enqueue_InvokesBatchActionsOnTimerInterval()
        {
            // Arrange
            var received = new List<int>();
            using var signal = new ManualResetEventSlim(false);
            var queue = new TimedQueueOfActions<int>(TimeSpan.FromMilliseconds(20), items =>
            {
                lock (received)
                {
                    received.AddRange(items);
                }
                signal.Set();
            });

            // Act
            queue.Enqueue(1);
            queue.Enqueue(2);

            // Assert
            signal.Wait(1000).Should().BeTrue();
            lock (received)
            {
                received.OrderBy(value => value).Should().Equal(1, 2);
            }
            queue.StopTimer();
        }

        [TestMethod]
        public void StartTimer_WithoutBatchActions_ThrowsInvalidOperationException()
        {
            // Arrange
            var queue = new TimedQueueOfActions<int>();

            // Act
            Action act = queue.StartTimer;

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void EmptyQueue_AfterSeveralIntervals_StopsTimer()
        {
            // Arrange
            var queue = new TimedQueueOfActions<int>(TimeSpan.FromMilliseconds(20), _ => { });

            // Act
            queue.StartTimer();

            // Assert
            SpinWait.SpinUntil(() => !queue.TimerActive, 1000).Should().BeTrue();
            queue.TimerActive.Should().BeFalse();
        }

        [TestMethod]
        public async Task ConcurrentEnqueue_BatchesAllItems()
        {
            // Arrange
            var values = Enumerable.Range(1, 25).ToArray();
            var received = new List<int>();
            using var signal = new ManualResetEventSlim(false);
            var queue = new TimedQueueOfActions<int>(TimeSpan.FromMilliseconds(20), items =>
            {
                lock (received)
                {
                    received.AddRange(items);
                    if (received.Count >= values.Length)
                    {
                        signal.Set();
                    }
                }
            });

            // Act
            await Task.WhenAll(values.Select(value => Task.Run(() => queue.Enqueue(value))));

            // Assert
            signal.Wait(1000).Should().BeTrue();
            lock (received)
            {
                received.OrderBy(value => value).Should().Equal(values);
            }
            queue.StopTimer();
        }
    }
}
