#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Yields folder tree work through the captured UI dispatcher.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class WpfDispatcherYield : IDispatcherYield
    {
        public async Task YieldAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Prefer the dispatcher already affinitized to this thread so a traversal that the
            // service marshalled onto a captured dispatcher keeps yielding through that same
            // dispatcher. Only a worker thread with no dispatcher of its own falls back to the
            // process-global UI dispatcher, which is the case Dispatcher.Yield() could not serve.
            // UiThread.Dispatcher is set-once state populated by UiThread.Init() and is null
            // outside a live host, so that null state is surfaced as InvalidOperationException to
            // preserve the strict contract callers relied on.
            Dispatcher dispatcher =
                Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher;
            if (dispatcher is null)
            {
                throw new InvalidOperationException(
                    "The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."
                );
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
