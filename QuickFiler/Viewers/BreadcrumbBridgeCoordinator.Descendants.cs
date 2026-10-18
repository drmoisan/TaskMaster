#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    public sealed partial class BreadcrumbBridgeCoordinator
    {
        private async Task DispatchDescendantMessageAsync(string json)
        {
            BreadcrumbSelectorDescendantMessage message;
            try
            {
                message = (BreadcrumbSelectorDescendantMessage)
                    BreadcrumbSelectorMessageSerializer.Parse(json);
            }
            catch (FormatException)
            {
                return;
            }
            if (!message.Toggle)
            {
                await _dispatcher
                    .Dispatch(() =>
                        ApplyTransition(
                            _router.ActivateSelectorDescendant(
                                message.RowIdentity,
                                message.NodeIdentity
                            )
                        )
                    )
                    .ConfigureAwait(false);
                return;
            }
            var outputs = await _router
                .ToggleDescendantAsync(
                    message.RowIdentity,
                    message.NodeIdentity,
                    CancellationToken.None
                )
                .ConfigureAwait(false);
            await _dispatcher.Dispatch(() => PublishRouterOutputs(outputs)).ConfigureAwait(false);
        }
    }
}
