using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Unit tests for <see cref="IdleAsyncQueue"/>.
    ///
    /// Purpose:
    ///     Verify queue management and callback routing of IdleAsyncQueue:
    ///     synchronous (non-UI-thread) tasks execute properly, the useUiThread flag
    ///     routes work through the Dispatcher scheduling path, and an exception from
    ///     one queued item does not block subsequent items.
    ///
    /// Invariants / Constraints:
    ///     IdleAsyncQueue uses static state (_entries property, _subscribeGuard,
    ///     _unsubscribe). Each test calls ResetStaticState() to drain the queue and
    ///     reset guards before asserting.
    ///     Tests invoke OnApplicationIdle via reflection to avoid depending on the
    ///     live ApplicationIdleTimer firing timing.
    /// </summary>
    [TestClass]
    public class IdleAsyncQueue_Tests
    {
        #region Helpers

        /// <summary>
        /// Drains the static Entries queue and resets the subscribe guard and
        /// unsubscribe timer to a clean baseline before each test.
        ///
        /// Purpose:
        ///     Prevents contamination when tests run in sequence within the same
        ///     AppDomain, where static fields persist across test methods.
        ///
        /// Side Effects:
        ///     Calls CancelAction() on _unsubscribe and nulls its _timer field.
        /// </summary>
        private static void ResetStaticState()
        {
            // Drain all items from the queue so each test starts with an empty queue.
            var entries = GetEntries();
            while (entries.TryDequeue(out _)) { }

            // Replace the subscribe guard so the next AddEntry call sees a fresh first-call state.
            typeof(IdleAsyncQueue)
                .GetField("_subscribeGuard", BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, new ThreadSafeSingleShotGuard());

            // Cancel any pending unsubscribe timer and null the backing _timer field.
            var unsubField = typeof(IdleAsyncQueue).GetField(
                "_unsubscribe",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            var unsubscribe = unsubField.GetValue(null) as TimedBatchAction;
            if (unsubscribe != null)
            {
                // CancelAction resets _actionRequested to fresh and stops the timer.
                unsubscribe.CancelAction();

                // Null the _timer reference to get a deterministic baseline for
                // any test that reads this field after reset.
                typeof(TimedBatchAction)
                    .GetField("_timer", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(unsubscribe, null);
            }
        }

        /// <summary>
        /// Returns the static Entries queue via reflection through the private property getter.
        ///
        /// Purpose:
        ///     Allows tests to inspect and drain queue state without going through
        ///     the production AddEntry path.
        ///
        /// Returns:
        ///     The live ConcurrentQueue managed by IdleAsyncQueue.
        /// </summary>
        private static ConcurrentQueue<(bool UiThread, Func<Task> AsyncAction)> GetEntries()
        {
            return (ConcurrentQueue<(bool UiThread, Func<Task> AsyncAction)>)
                typeof(IdleAsyncQueue)
                    .GetProperty("Entries", BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);
        }

        /// <summary>
        /// Creates an ApplicationIdleEventArgs with the given idle-since timestamp
        /// using the internal constructor, following the same pattern as
        /// ApplicationIdleTimer_Tests.
        ///
        /// Args:
        ///     idleSince (DateTime): Reference time from which idle duration is measured.
        ///
        /// Returns:
        ///     Constructed ApplicationIdleEventArgs.
        /// </summary>
        private static ApplicationIdleTimer.ApplicationIdleEventArgs CreateEventArgs(
            DateTime idleSince
        )
        {
            var ctor = typeof(ApplicationIdleTimer.ApplicationIdleEventArgs).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(DateTime) },
                null
            );
            return (ApplicationIdleTimer.ApplicationIdleEventArgs)
                ctor.Invoke(new object[] { idleSince });
        }

        /// <summary>
        /// Invokes OnApplicationIdle via reflection with an idle duration of 1 second,
        /// which exceeds the 20 ms threshold required to enter the processing branch.
        ///
        /// Purpose:
        ///     Centralises the reflection invocation so individual tests remain concise.
        ///
        /// Side Effects:
        ///     Dequeues one entry from Entries (or triggers RequestAction if queue is empty).
        /// </summary>
        private static void InvokeOnIdle()
        {
            typeof(IdleAsyncQueue)
                .GetMethod("OnApplicationIdle", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { CreateEventArgs(DateTime.Now.AddSeconds(-1)) });
        }

        #endregion Helpers

        #region P27-T1 — task runs exactly once

        /// <summary>
        /// Verifies P27-T1: a queued async task with useUiThread=false runs exactly once
        /// when the idle callback is invoked.
        ///
        /// Scenario:
        ///     One entry is added with useUiThread=false and a synchronously completing
        ///     Func(Task). OnApplicationIdle fires once.
        ///
        /// Expected:
        ///     The action executes once; the queue is empty after the invocation.
        /// </summary>
        [TestMethod]
        public void AddEntry_UseUiThreadFalse_ActionRunsExactlyOnce()
        {
            // Arrange: clean queue, one synchronously completing action.
            ResetStaticState();
            int callCount = 0;

            // Synchronous action: await Task.CompletedTask runs inline, so callCount++
            // occurs before InvokeOnIdle returns.
            Func<Task> asyncAction = () =>
            {
                callCount++;
                return Task.CompletedTask;
            };
            IdleAsyncQueue.AddEntry(false, asyncAction);

            // Act: fire the idle callback.
            InvokeOnIdle();

            // Assert: action ran exactly once.
            callCount.Should().Be(1);
        }

        #endregion P27-T1 — task runs exactly once

        #region P27-T2 — UI-thread routing

        /// <summary>
        /// Verifies P27-T2: the useUiThread=true flag routes execution through the
        /// UiThread.Dispatcher scheduling path.
        ///
        /// Scenario:
        ///     One entry is added with useUiThread=true. UiThread.Dispatcher is null
        ///     in the test environment (no WinForms/WPF message loop). When InvokeAsync
        ///     is called on a null Dispatcher, the NullReferenceException is caught by
        ///     the internal try/catch in OnApplicationIdle, which is the expected
        ///     production fault-isolation behaviour.
        ///
        /// Expected:
        ///     No exception escapes the callback. The entry is dequeued regardless.
        ///     The action itself is NOT executed because the Dispatcher is unavailable.
        /// </summary>
        [TestMethod]
        public void AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException()
        {
            // Arrange: one entry routed through the Dispatcher path.
            ResetStaticState();
            int callCount = 0;
            Func<Task> asyncAction = () =>
            {
                callCount++;
                return Task.CompletedTask;
            };
            IdleAsyncQueue.AddEntry(true, asyncAction);

            // Act: InvokeOnIdle triggers the Dispatcher-routing branch; null Dispatcher
            // causes NullReferenceException that is caught internally.
            Action actDelegate = () => InvokeOnIdle();
            actDelegate
                .Should()
                .NotThrow(
                    "exceptions after the await in the Dispatcher path are caught by the internal try/catch"
                );

            // Assert: entry was dequeued regardless of dispatch failure.
            GetEntries()
                .Count.Should()
                .Be(0, "the entry must be dequeued even when dispatch to UiThread fails");

            // Action did not execute because the null Dispatcher prevented InvokeAsync.
            callCount
                .Should()
                .Be(0, "action must not run when the UiThread Dispatcher is unavailable");
        }

        #endregion P27-T2 — UI-thread routing

        #region P27-T3 — exception isolation between items

        /// <summary>
        /// Verifies P27-T3: an exception thrown by one queued item does not prevent
        /// subsequent items from executing.
        ///
        /// Scenario:
        ///     Two entries are queued. The first throws. The second records execution.
        ///     OnApplicationIdle is invoked twice — once per entry.
        ///
        /// Expected:
        ///     After both invocations, the counting action has run exactly once,
        ///     demonstrating that per-item exception isolation is working correctly.
        /// </summary>
        [TestMethod]
        public void OnApplicationIdle_FirstItemThrows_SubsequentItemStillExecutes()
        {
            // Arrange: first action throws, second increments a counter.
            ResetStaticState();
            int callCount = 0;

            // Throwing delegate: the throw is synchronous before any Task is returned,
            // so the exception propagates directly inside the try block in OnApplicationIdle.
            Func<Task> throwingAction = () =>
            {
                throw new InvalidOperationException("deliberate test fault");
            };

            Func<Task> countingAction = () =>
            {
                callCount++;
                return Task.CompletedTask;
            };

            IdleAsyncQueue.AddEntry(false, throwingAction);
            IdleAsyncQueue.AddEntry(false, countingAction);

            // Act: first call dequeues throwingAction — exception caught and logged.
            //      second call dequeues countingAction — runs normally.
            InvokeOnIdle();
            InvokeOnIdle();

            // Assert: second item ran despite first item throwing.
            callCount
                .Should()
                .Be(
                    1,
                    "counting action must execute after the throwing action's exception is isolated"
                );
        }

        #endregion P27-T3 — exception isolation between items
    }
}
