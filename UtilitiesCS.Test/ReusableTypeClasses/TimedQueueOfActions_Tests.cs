using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses.TimedActions;
using UtilitiesCS.Test.TestHelpers;

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
            using var timerStub = new ManualFireTimerWrapper();
            var queue = new TimedQueueOfActions<int>(
                TimeSpan.FromMilliseconds(20),
                items =>
                {
                    received.AddRange(items);
                }
            )
            {
                TimerFactory = _ => timerStub,
            };

            // Act
            queue.Enqueue(1);
            queue.Enqueue(2);

            // Enqueue started the timer via the injected factory; fire it once to dispatch the batch.
            timerStub.FireElapsed();

            // Assert
            received.OrderBy(value => value).Should().Equal(1, 2);
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
            using var timerStub = new ManualFireTimerWrapper();
            var queue = new TimedQueueOfActions<int>(TimeSpan.FromMilliseconds(20), _ => { })
            {
                TimerFactory = _ => timerStub,
            };

            // Act
            queue.StartTimer();

            // The implementation stops the timer after 5 consecutive empty-queue ticks
            // (_emptyQueueChecks > 4). Fire deterministically until it auto-stops, bounded by a
            // small safety cap so a regression that never stops cannot loop forever.
            for (var tick = 0; tick < 10 && queue.TimerActive; tick++)
            {
                timerStub.FireElapsed();
            }

            // Assert – the timer auto-stops after the empty-tick threshold.
            queue.TimerActive.Should().BeFalse();
        }

        [TestMethod]
        public async Task ConcurrentEnqueue_BatchesAllItems()
        {
            // Arrange
            var values = Enumerable.Range(1, 25).ToArray();
            var received = new List<int>();
            using var timerStub = new ManualFireTimerWrapper();
            var queue = new TimedQueueOfActions<int>(
                TimeSpan.FromMilliseconds(20),
                items =>
                {
                    lock (received)
                    {
                        received.AddRange(items);
                    }
                }
            )
            {
                TimerFactory = _ => timerStub,
            };

            // Act – enqueue all 25 items concurrently, then dispatch deterministically.
            await Task.WhenAll(values.Select(value => Task.Run(() => queue.Enqueue(value))));
            timerStub.FireElapsed();

            // Assert – every concurrently enqueued item appears in the dispatched batch.
            lock (received)
            {
                received.OrderBy(value => value).Should().Equal(values);
            }
            queue.StopTimer();
        }

        [TestMethod]
        public void DefaultConstructor_InitializesConfigAndQueue()
        {
            // Arrange & Act
            var queue = new TimedQueueOfActions<int>();

            // Assert
            queue.Config.Should().NotBeNull();
            queue.Queue.Should().NotBeNull();
            queue.BatchActions.Should().BeNull();
            queue.TimerActive.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithMilliseconds_SetsBatchActionsAndConfig()
        {
            // Arrange
            Action<IEnumerable<int>> writer = _ => { };

            // Act
            var queue = new TimedQueueOfActions<int>(100, writer);

            // Assert
            queue.BatchActions.Should().BeSameAs(writer);
            queue.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void StopTimer_WhenTimerNotStarted_DoesNotThrow()
        {
            // Arrange
            var queue = new TimedQueueOfActions<int>();

            // Act
            Action act = () => queue.StopTimer();

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Configuration_PropertyChanged_RestartsTimerOnWriteIntervalChange()
        {
            // Arrange
            using var timerStub = new ManualFireTimerWrapper();
            var queue = new TimedQueueOfActions<int>(TimeSpan.FromMilliseconds(50), _ => { })
            {
                TimerFactory = _ => timerStub,
            };
            queue.StartTimer();
            queue.TimerActive.Should().BeTrue();

            // Act – change WriteInterval triggers PropertyChanged, which stops and restarts the
            // timer synchronously (StopTimer + TryStartTimer) via the injected factory.
            queue.Config.WriteInterval = TimeSpan.FromMilliseconds(100);

            // Assert – timer is still active after the synchronous restart.
            queue.TimerActive.Should().BeTrue();
            queue.StopTimer();
        }

        [TestMethod]
        public void Configuration_TryAddTimeout_RoundTrips()
        {
            // Arrange
            var config = new TimedQueueOfActions<int>.Configuration();

            // Act
            config.TryAddTimeout = 999;

            // Assert
            config.TryAddTimeout.Should().Be(999);
        }

        [TestMethod]
        public void Configuration_NotifyPropertyChanged_RaisesEvent()
        {
            // Arrange
            var config = new TimedQueueOfActions<int>.Configuration(10, TimeSpan.FromSeconds(1));
            string changedProperty = null;
            config.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            // Act
            config.WriteInterval = TimeSpan.FromSeconds(2);

            // Assert
            changedProperty.Should().Be(nameof(config.WriteInterval));
        }

        [TestMethod]
        public void BatchActions_Setter_AllowsReassignment()
        {
            // Arrange
            var queue = new TimedQueueOfActions<int>();
            var items1 = new List<int>();
            var items2 = new List<int>();

            // Act
            queue.BatchActions = items1.AddRange;
            queue.BatchActions = items2.AddRange;

            // Assert
            queue.BatchActions.Should().NotBeNull();
        }

        [TestMethod]
        public void Enqueue_WhenBatchActionsMissingAndTimerStartFails_StillAddsItemToQueue()
        {
            // Arrange
            var queue = new ThrowingStartTimedQueue<int>();

            // Act
            queue.Enqueue(5);

            // Assert
            queue.Queue.TryTake(out var queuedValue).Should().BeTrue();
            queuedValue.Should().Be(5);
        }

        [TestMethod]
        public async Task EnqueueAsync_WhenBatchActionsMissingAndTimerStartFails_StillAddsItemToQueue()
        {
            // Arrange
            var queue = new ThrowingStartTimedQueue<int>();

            // Act
            await queue.EnqueueAsync(7, CancellationToken.None);

            // Assert
            queue.Queue.TryTake(out var queuedValue).Should().BeTrue();
            queuedValue.Should().Be(7);
        }

        private sealed class ThrowingStartTimedQueue<T> : TimedQueueOfActions<T>
        {
            public override bool TimerActive => false;

            public override void StartTimer()
            {
                throw new InvalidOperationException("timer start failed");
            }
        }
    }
}
