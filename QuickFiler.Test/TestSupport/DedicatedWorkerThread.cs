using System;
using System.Threading;

namespace QuickFiler.Test.TestSupport
{
    /// <summary>
    /// Runs a delegate on a dedicated, joined background thread for tests that must exercise a
    /// thread-identity guard from a thread that is provably not the calling thread.
    /// </summary>
    /// <remarks>
    /// Issue #900 and issue #931: a <c>Task.Run</c> work item is not guaranteed to run on a
    /// thread other than the caller's, so it cannot stand in for a different thread in a
    /// thread-identity test. A thread this method constructs is
    /// distinct from every live thread by construction. The untimed <c>Join()</c> is a
    /// completion wait on one bounded synchronous call, not a sleep or a wall-clock wait, and
    /// the waiting thread and the waited-for thread are never both thread-pool workers, so
    /// the wait cannot starve the pool under parallel execution. The helper asserts nothing
    /// itself: each test states its own distinctness precondition inside its delegate so that
    /// a failure names the guard under test rather than the helper.
    /// </remarks>
    internal static class DedicatedWorkerThread
    {
        /// <summary>
        /// Runs <paramref name="action"/> on a dedicated background thread, joins it, and
        /// returns the exception it threw, or <see langword="null"/> when it completed
        /// normally.
        /// </summary>
        internal static Exception Run(Action action)
        {
            Exception captured = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception error)
                {
                    captured = error;
                }
            });
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            return captured;
        }
    }
}
