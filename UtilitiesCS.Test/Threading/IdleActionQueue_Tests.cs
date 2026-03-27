using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Unit tests for <see cref="IdleActionQueue"/>.
    ///
    /// Purpose:
    ///     Verify the static queue management behavior of IdleActionQueue:
    ///     initialization on first AddEntry call, FIFO ordering of enqueued actions,
    ///     and the unsubscribe-timer path when the queue is empty after an idle callback.
    ///
    /// Invariants / Constraints:
    ///     IdleActionQueue uses static state (_entries, _subscribeGuard, _unsubscribe).
    ///     Each test calls ResetStaticState() to ensure isolation.
    /// </summary>
    [TestClass]
    public class IdleActionQueue_Tests
    {
        #region Helpers

        /// <summary>
        /// Resets all static fields of IdleActionQueue to a clean initial state.
        ///
        /// Purpose:
        ///     Prevents cross-test contamination from accumulated static queue entries,
        ///     spent subscribe guards, or pending unsubscribe timers.
        ///
        /// Side Effects:
        ///     Cancels any active unsubscribe timer and nulls the timer reference.
        /// </summary>
        private static void ResetStaticState()
        {
            // Clear the queue so Entries lazy-creates a fresh ConcurrentQueue on next access.
            typeof(IdleActionQueue)
                .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, null);

            // Replace the subscribe guard so the next AddEntry call sees a fresh first-call state.
            typeof(IdleActionQueue)
                .GetField("_subscribeGuard", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, new ThreadSafeSingleShotGuard());

            // Cancel any pending unsubscribe timer and null _timer so P26-T3 gets a clean baseline.
            var unsubField = typeof(IdleActionQueue).GetField(
                "_unsubscribe",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var unsubscribe = unsubField.GetValue(null) as TimedBatchAction;
            if (unsubscribe != null)
            {
                // CancelAction resets _actionRequested to a fresh guard and stops the timer.
                unsubscribe.CancelAction();

                // Null the _timer reference so the post-invoke assertion can distinguish
                // "timer was requested now" from "timer was started in a prior test".
                typeof(TimedBatchAction)
                    .GetField("_timer", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(unsubscribe, null);
            }
        }

        /// <summary>
        /// Returns the current value of the static _entries field via reflection.
        ///
        /// Purpose:
        ///     Allows tests to inspect internal queue state without going through
        ///     the production AddEntry path.
        ///
        /// Returns:
        ///     The ConcurrentQueue instance, or null if not yet initialized.
        /// </summary>
        private static ConcurrentQueue<Action> GetEntries()
        {
            return (ConcurrentQueue<Action>)
                typeof(IdleActionQueue)
                    .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);
        }

        /// <summary>
        /// Creates an ApplicationIdleEventArgs instance via the internal constructor,
        /// using the same reflection pattern as ApplicationIdleTimer_Tests.
        ///
        /// Args:
        ///     idleSince (DateTime): The point in time from which idle duration is measured.
        ///
        /// Returns:
        ///     Constructed ApplicationIdleEventArgs with the given idle-since timestamp.
        /// </summary>
        private static ApplicationIdleTimer.ApplicationIdleEventArgs CreateEventArgs(
            DateTime idleSince
        )
        {
            var type = typeof(ApplicationIdleTimer.ApplicationIdleEventArgs);
            var ctor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(DateTime) },
                null
            );
            return (ApplicationIdleTimer.ApplicationIdleEventArgs)
                ctor.Invoke(new object[] { idleSince });
        }

        #endregion Helpers

        #region AddEntry — queue initialization

        /// <summary>
        /// Verifies P26-T1: the first AddEntry call initializes the internal queue
        /// and places exactly one entry into it.
        ///
        /// Scenario:
        ///     Static state is fresh. AddEntry is called once.
        ///
        /// Expected:
        ///     _entries is non-null and contains one item.
        /// </summary>
        [TestMethod]
        public void AddEntry_FirstCall_InitializesQueueWithOneEntry()
        {
            // Arrange: ensure _entries starts as null.
            ResetStaticState();
            Action entry = () => { };

            // Act: first call populates the queue.
            IdleActionQueue.AddEntry(entry);

            // Assert: queue was created and has exactly one item.
            var entries = GetEntries();
            entries.Should().NotBeNull();
            entries.Count.Should().Be(1);
        }

        #endregion AddEntry — queue initialization

        #region AddEntry — FIFO ordering

        /// <summary>
        /// Verifies P26-T2: multiple AddEntry calls enqueue actions in FIFO order.
        ///
        /// Scenario:
        ///     Three distinct actions are added in sequence.
        ///
        /// Expected:
        ///     Queue snapshot preserves insertion order — first action added is first
        ///     in the ConcurrentQueue, which guarantees it is first to be dequeued by
        ///     the idle callback.
        /// </summary>
        [TestMethod]
        public void AddEntry_MultipleEntries_EnqueuesInFifoOrder()
        {
            // Arrange: clean state and three identifiable actions.
            ResetStaticState();
            Action action1 = () => { };
            Action action2 = () => { };
            Action action3 = () => { };

            // Act: enqueue in a known order.
            IdleActionQueue.AddEntry(action1);
            IdleActionQueue.AddEntry(action2);
            IdleActionQueue.AddEntry(action3);

            // Assert: queue snapshot matches insertion order, confirming FIFO drain semantics.
            var entries = GetEntries();
            entries.Count.Should().Be(3);

            var snapshot = entries.ToArray();
            snapshot[0].Should().BeSameAs(action1, "first enqueued action must be first in queue");
            snapshot[1]
                .Should()
                .BeSameAs(action2, "second enqueued action must be second in queue");
            snapshot[2].Should().BeSameAs(action3, "third enqueued action must be third in queue");
        }

        #endregion AddEntry — FIFO ordering

        #region OnApplicationIdle — unsubscribe path

        /// <summary>
        /// Verifies P26-T3: when OnApplicationIdle fires with an empty queue, the
        /// unsubscribe timer is requested to clear the idle-callback subscription after
        /// a period of inactivity.
        ///
        /// Scenario:
        ///     Static state is fresh, _entries is empty, idle duration exceeds the
        ///     20-millisecond threshold. OnApplicationIdle is invoked directly via
        ///     reflection to exercise the else-branch (empty queue → RequestAction).
        ///
        /// Expected:
        ///     TimedBatchAction._timer is non-null, indicating RequestAction was called
        ///     and a delayed unsubscribe has been scheduled.
        /// </summary>
        [TestMethod]
        public void OnApplicationIdle_EmptyQueue_RequestsUnsubscribeTimer()
        {
            // Arrange: empty queue and a freshly cancelled unsubscribe batch action.
            ResetStaticState();

            // Create idle args with duration well above the 20ms threshold.
            var idleArgs = CreateEventArgs(DateTime.Now.AddSeconds(-1));

            var unsubField = typeof(IdleActionQueue).GetField(
                "_unsubscribe",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            var timerField = typeof(TimedBatchAction).GetField(
                "_timer",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Act: invoke OnApplicationIdle via reflection; empty queue → else branch →
            // _unsubscribe.RequestAction() runs synchronously before any await.
            var method = typeof(IdleActionQueue).GetMethod(
                "OnApplicationIdle",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Invoke(null, new object[] { idleArgs });

            // Assert: a timer was started, confirming the unsubscribe was requested.
            var unsubscribe = unsubField.GetValue(null);
            var timer = timerField.GetValue(unsubscribe);
            timer
                .Should()
                .NotBeNull(
                    "the unsubscribe timer must be started when the idle callback finds an empty queue"
                );
        }

        #endregion OnApplicationIdle — unsubscribe path
    }
}
