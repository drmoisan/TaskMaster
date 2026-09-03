using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskMaster
{
    /// <summary>
    /// Non-blocking, pump-independent replacement for <c>Task.Delay</c> (Issue #207, AC10).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="WaitAsync(TimeSpan)"/> returns a <see cref="Task"/> that completes when a one-shot
    /// timer fires its callback. The awaiting code yields control to the message loop, so an STA keeps
    /// pumping window messages during the wait. The timer callback fires on a threadpool thread and
    /// sets a <see cref="TaskCompletionSource{TResult}"/>; the <c>await</c> continuation then resumes
    /// on the captured STA <see cref="SynchronizationContext"/>, so subsequent COM work still runs on
    /// the STA.
    /// </para>
    /// <para>
    /// Unlike the prior <c>DispatcherTimer</c>-backed design, this helper completes whether or not a
    /// <see cref="System.Windows.Threading.Dispatcher"/> is running on the current thread. That
    /// pump-independence is required for the helper to be unit-testable on the pump-less MSTest host
    /// (the <c>DispatcherTimer</c> design completed only on a Dispatcher tick and hung the host).
    /// </para>
    /// <para>
    /// Timer scheduling goes through <see cref="TimeProvider"/> (Issue #729, Finding 1) so a test can
    /// drive completion from virtual time instead of a real <c>Stopwatch</c> wait. The seam is an
    /// explicit overload pair rather than an optional parameter: <c>WaitAsync</c> is consumed as a
    /// method group at <c>StoreRehookCoordinator</c>, and C# ignores a candidate method whose optional
    /// parameter has no corresponding parameter in the target delegate type, which would produce
    /// CS0123 at that call site.
    /// </para>
    /// <para>
    /// Neither <see cref="TimeProvider"/> nor <c>System.Threading.Timer</c> is a banned API (the
    /// banned list is <c>DateTime.Now</c>, <c>DateTime.UtcNow</c>, <c>Random.Shared</c>,
    /// <c>Thread.Sleep</c>, <c>Task.Delay</c>), so the helper satisfies AC10. It carries the new-code
    /// coverage obligation (it is not COM/VSTO-exempt).
    /// </para>
    /// </remarks>
    internal static class NonBlockingDelay
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that completes after <paramref name="delay"/> elapses,
        /// without blocking the calling thread and without requiring a running
        /// <see cref="System.Windows.Threading.Dispatcher"/>. Scheduling is supplied by
        /// <see cref="TimeProvider.System"/>.
        /// </summary>
        /// <param name="delay">The interval to wait before completing the task.</param>
        /// <returns>A task that completes when the one-shot timer callback fires.</returns>
        public static Task WaitAsync(TimeSpan delay)
        {
            return WaitAsync(delay, TimeProvider.System);
        }

        /// <summary>
        /// Returns a <see cref="Task"/> that completes after <paramref name="delay"/> elapses on the
        /// supplied <paramref name="timeProvider"/>'s clock, without blocking the calling thread and
        /// without requiring a running <see cref="System.Windows.Threading.Dispatcher"/>. A one-shot
        /// <see cref="ITimer"/> is created with a due time of <paramref name="delay"/> and an infinite
        /// period; in its callback the returned task is completed and the timer is disposed.
        /// </summary>
        /// <param name="delay">The interval to wait before completing the task.</param>
        /// <param name="timeProvider">The clock that schedules the one-shot completion callback.</param>
        /// <returns>A task that completes when the one-shot timer callback fires.</returns>
        public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            // This file has no project-level <Nullable> element and no whole-file #nullable
            // pragma; this `?` annotation on a self-referencing local (assigned to itself inside
            // its own closure below) needs an explicit annotations context to avoid CS8632.
            // Scoping narrowly to annotations-only avoids introducing new CS86xx diagnostics
            // elsewhere in this file (no behavior change).
#nullable enable annotations
            ITimer? timer = null;
#nullable restore annotations
            timer = timeProvider.CreateTimer(
                _ =>
                {
                    timer?.Dispose();
                    tcs.TrySetResult(true);
                },
                null,
                delay,
                Timeout.InfiniteTimeSpan
            );
            return tcs.Task;
        }
    }
}
