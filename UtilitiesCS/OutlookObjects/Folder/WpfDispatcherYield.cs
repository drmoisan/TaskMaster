#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Yields folder tree work through the captured UI dispatcher.
    /// </summary>
    public sealed class WpfDispatcherYield : IDispatcherYield
    {
        private readonly Func<Dispatcher?> _currentThreadDispatcherProvider;
        private readonly Func<Dispatcher?> _fallbackDispatcherProvider;

        /// <summary>
        /// Initializes a yielder that resolves dispatchers exactly as it always has: the
        /// dispatcher affinitized to the calling thread, then the process-global UI dispatcher.
        /// </summary>
        public WpfDispatcherYield()
            : this(null, null) { }

        /// <summary>
        /// Initializes a yielder whose dispatcher lookups are supplied by the caller. Tests use
        /// this to arrange the dispatcher-free case explicitly instead of inheriting it from
        /// ambient thread and process state.
        /// </summary>
        /// <param name="currentThreadDispatcherProvider">
        /// Supplies the dispatcher affinitized to the calling thread. Null selects the production
        /// lookup.
        /// </param>
        /// <param name="fallbackDispatcherProvider">
        /// Supplies the process-global dispatcher used when the calling thread has none. Null
        /// selects the production lookup.
        /// </param>
        internal WpfDispatcherYield(
            Func<Dispatcher?>? currentThreadDispatcherProvider,
            Func<Dispatcher?>? fallbackDispatcherProvider
        )
        {
            _currentThreadDispatcherProvider =
                currentThreadDispatcherProvider
                ?? (() => Dispatcher.FromThread(Thread.CurrentThread));
            _fallbackDispatcherProvider =
                fallbackDispatcherProvider ?? (() => UtilitiesCS.UiThread.Dispatcher);
        }

        public async Task YieldAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Prefer the dispatcher already affinitized to this thread so a traversal that the
            // service marshalled onto a captured dispatcher keeps yielding through that same
            // dispatcher. Only a worker thread with no dispatcher of its own falls back to the
            // process-global UI dispatcher, which is the case Dispatcher.Yield() could not serve.
            // The production fallback provider reads UiThread.Dispatcher, which throws
            // InvalidOperationException directly when the dispatcher has not been captured. The
            // local null guard below is therefore unreachable on the production path; it covers
            // only injected providers, which are typed Func<Dispatcher?> and exist only in tests.
            Dispatcher? dispatcher =
                _currentThreadDispatcherProvider() ?? _fallbackDispatcherProvider();
            if (dispatcher is null)
            {
                throw new InvalidOperationException(UiThread.DispatcherNotInitializedMessage);
            }

            await dispatcher.InvokeAsync(
                () => { },
                DispatcherPriority.Background,
                cancellationToken
            );
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
