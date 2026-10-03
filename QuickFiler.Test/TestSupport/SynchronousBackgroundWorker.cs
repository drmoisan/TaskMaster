using System.ComponentModel;

namespace QuickFiler.Test.TestSupport
{
    /// <summary>
    /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on the
    /// calling thread through the protected <c>OnDoWork</c>, so a privately subscribed handler such
    /// as <c>QfcDatamodel.Worker_DoWork</c> runs to its first incomplete await before
    /// <c>InitEmailQueue</c> returns, and no worker a test starts outlives that test. Issue #950
    /// introduced this shape to replace bounded waits on a thread-pool worker; issue #968 (folding
    /// issue #972) consolidated the three per-file copies here. The class adds no fields, handles
    /// or subscriptions, so it does not override <c>Dispose(bool)</c>; disposal stays with the test
    /// that constructs the worker, in a using block.
    /// </summary>
    internal sealed class SynchronousBackgroundWorker : BackgroundWorker
    {
        /// <summary>Raises <c>DoWork</c> on the calling thread.</summary>
        internal void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));

        /// <summary>
        /// The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>. The worker handed
        /// to it must be a <see cref="SynchronousBackgroundWorker"/>.
        /// </summary>
        internal static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();
    }
}
