using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic unit tests for <see cref="NonBlockingDelay"/>, the pump-independent
    /// <c>System.Threading.Timer</c>-backed replacement for <c>Task.Delay</c> (Issue #207, AC10).
    /// These tests run on the standard pump-less MSTest host with NO running
    /// <c>System.Windows.Threading.Dispatcher</c>, proving the helper completes whether or not a
    /// Dispatcher is present (the prior <c>DispatcherTimer</c> design could not be tested this way
    /// and hung the host). No Moq, no filesystem, no temporary files, and no banned API
    /// (<c>Thread.Sleep</c>/<c>Task.Delay</c>) are used.
    /// </summary>
    [TestClass]
    public class NonBlockingDelayTests
    {
        /// <summary>
        /// Scenario: with no Dispatcher running on the test thread, awaiting
        /// <see cref="NonBlockingDelay.WaitAsync"/> for a small interval completes successfully.
        /// Expected: the returned task transitions to RanToCompletion; the elapsed time is at least
        /// the requested interval. The outer MSTest <c>[Timeout]</c> guards against a hang regression.
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
            var stopwatch = Stopwatch.StartNew();

            // Act
            var waitTask = NonBlockingDelay.WaitAsync(interval);
            await waitTask;
            stopwatch.Stop();

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "the one-shot timer callback completes the task successfully"
                );
            stopwatch
                .Elapsed.Should()
                .BeGreaterThanOrEqualTo(
                    interval,
                    "the helper must not complete before the requested interval elapses"
                );
        }

        /// <summary>
        /// Scenario: a zero-length wait still completes deterministically without a Dispatcher.
        /// Expected: the task completes successfully under the timeout guard. This confirms the helper
        /// does not depend on any message pump for completion.
        /// </summary>
        [TestMethod]
        [Timeout(5000)]
        public async Task WaitAsync_ZeroDelay_CompletesWithoutPump()
        {
            // Arrange
            var waitTask = NonBlockingDelay.WaitAsync(TimeSpan.Zero);

            // Act
            await waitTask;

            // Assert
            waitTask
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    "a zero-delay wait completes via the timer callback"
                );
        }
    }
}
