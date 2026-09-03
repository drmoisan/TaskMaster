using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic unit tests for <see cref="NonBlockingDelay"/>, the pump-independent
    /// <see cref="TimeProvider"/>-backed replacement for <c>Task.Delay</c> (Issue #207 AC10;
    /// Issue #729 Finding 1). These tests run on the standard pump-less MSTest host with NO running
    /// <c>System.Windows.Threading.Dispatcher</c>, proving the helper completes whether or not a
    /// Dispatcher is present. Virtual time is supplied by <see cref="FakeTimeProvider"/>, so no
    /// elapsed-time measurement and no real wall-clock wait is used. No Moq, no filesystem, no
    /// temporary files, and no banned API (<c>Thread.Sleep</c>/<c>Task.Delay</c>) are used.
    /// </summary>
    [TestClass]
    public class NonBlockingDelayTests
    {
        /// <summary>
        /// Scenario: with no Dispatcher running on the test thread, the task returned by the
        /// <see cref="TimeProvider"/> overload stays incomplete until virtual time reaches the
        /// requested interval, then completes.
        /// Expected: the task is not completed before <c>Advance</c>, and transitions to
        /// RanToCompletion after it. Asserting non-completion before the advance is strictly stronger
        /// than the previous elapsed-time check, because it proves the task cannot complete early.
        /// The outer MSTest <c>[Timeout]</c> is a deadlock bound, not a wait.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_WithNoDispatcher_CompletesAfterInterval()
        {
            // Arrange
            SynchronizationContext
                .Current.Should()
                .BeNull(
                    "the pump-less MSTest host must not have a Dispatcher SynchronizationContext"
                );
            var interval = TimeSpan.FromMilliseconds(30);
            var fakeTimeProvider = new FakeTimeProvider();

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(interval, fakeTimeProvider);
            waitTask
                .IsCompleted.Should()
                .BeFalse(
                    "the one-shot timer must not fire before virtual time reaches the interval"
                );
            fakeTimeProvider.Advance(interval);
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "the one-shot timer callback completes the task successfully"
                );
        }

        /// <summary>
        /// Scenario: a zero-length wait still completes deterministically without a Dispatcher.
        /// Expected: a zero-due-time one-shot timer is invoked by <see cref="FakeTimeProvider"/>
        /// during <c>CreateTimer</c> itself, so the returned task is already completed when control
        /// returns from the two-argument overload and no virtual-time advance is required. That is an
        /// observed behaviour, recorded by the executed run in
        /// nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md, not an assumption.
        /// Awaiting the already-completed task then shows RanToCompletion, which confirms the
        /// helper does not depend on any message pump.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_ZeroDelay_CompletesWithoutPump()
        {
            // Arrange
            var fakeTimeProvider = new FakeTimeProvider();

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(TimeSpan.Zero, fakeTimeProvider);
            waitTask
                .IsCompleted.Should()
                .BeTrue(
                    "FakeTimeProvider invokes a zero-due-time one-shot timer during CreateTimer, so "
                        + "the task is already completed when the overload returns"
                );
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "a zero-delay wait completes via the timer callback"
                );
        }

        /// <summary>
        /// Scenario: the single-argument overload, which is the one production callers bind to as a
        /// method group, completes on the real system clock.
        /// Expected: the task completes successfully under the timeout guard. This is a completion
        /// assertion, not a duration assertion, so no wall-clock dependency is reintroduced. The test
        /// exists because StoreRehookCoordinatorTests supplies an explicit delay at both construction
        /// sites and therefore never reaches the NonBlockingDelay.WaitAsync fallback, leaving the
        /// single-argument body otherwise uncovered.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider()
        {
            // Arrange
            var interval = TimeSpan.Zero;

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(interval);
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "the single-argument overload delegates to TimeProvider.System and completes "
                        + "without a Dispatcher"
                );
        }
    }
}
