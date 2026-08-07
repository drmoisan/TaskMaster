#nullable enable
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
            await UtilitiesCS.UiThread.Dispatcher.InvokeAsync(
                () => { },
                DispatcherPriority.Background,
                cancellationToken
            );
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
