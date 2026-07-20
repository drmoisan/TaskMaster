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
    /// <see cref="Timer"/> fires its callback. The awaiting code yields control to the message loop,
    /// so an STA keeps pumping window messages during the wait. The timer callback fires on a
    /// threadpool thread and sets a <see cref="TaskCompletionSource{TResult}"/>; the <c>await</c>
    /// continuation then resumes on the captured STA <see cref="SynchronizationContext"/>, so
    /// subsequent COM work still runs on the STA.
    /// </para>
    /// <para>
    /// Unlike the prior <c>DispatcherTimer</c>-backed design, this helper completes whether or not a
    /// <see cref="System.Windows.Threading.Dispatcher"/> is running on the current thread. That
    /// pump-independence is required for the helper to be unit-testable on the pump-less MSTest host
    /// (the <c>DispatcherTimer</c> design completed only on a Dispatcher tick and hung the host).
    /// </para>
    /// <para>
    /// <see cref="Timer"/> is not a banned API (the banned list is <c>DateTime.Now</c>,
    /// <c>DateTime.UtcNow</c>, <c>Random.Shared</c>, <c>Thread.Sleep</c>, <c>Task.Delay</c>), so the
    /// helper satisfies AC10. It carries the new-code coverage obligation (it is not COM/VSTO-exempt).
    /// </para>
    /// </remarks>
    internal static class NonBlockingDelay
    {
        /// <summary>
        /// Returns a <see cref="Task"/> that completes after <paramref name="delay"/> elapses,
        /// without blocking the calling thread and without requiring a running
        /// <see cref="System.Windows.Threading.Dispatcher"/>. A one-shot
        /// <see cref="Timer"/> is created with a due time of <paramref name="delay"/> and an infinite
        /// period; in its callback the returned task is completed and the timer is disposed.
        /// </summary>
        /// <param name="delay">The interval to wait before completing the task.</param>
        /// <returns>A task that completes when the one-shot timer callback fires.</returns>
        public static Task WaitAsync(TimeSpan delay)
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            // This file has no project-level <Nullable> element and no whole-file #nullable
            // pragma; this pre-existing `?` annotation on a self-referencing local (assigned to
            // itself inside its own closure below) needs an explicit annotations context to
            // avoid CS8632. Scoping narrowly to annotations-only avoids introducing new CS86xx
            // diagnostics elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
            Timer? timer = null;
#nullable restore annotations
            timer = new Timer(
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
