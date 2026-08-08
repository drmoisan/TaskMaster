#nullable enable
using System;
using System.Collections.Generic;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #438: the folder-search presentation composite.
    /// <para>
    /// Held on a second partial-class part so <c>BreadcrumbBridgeCoordinator.cs</c> (487 lines)
    /// stays clear of the repository's 500-line ceiling.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbBridgeCoordinator
    {
        /// <summary>
        /// Presents one folder-search result set: replaces the rows, opens the selector if it is
        /// closed, and highlights the first selectable row without committing it.
        /// </summary>
        /// <param name="items">The exact search-result strings, carried verbatim.</param>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
        /// <remarks>
        /// This is the single presentation intent behind <c>IItemViewer.PresentFolderSearchResults</c>
        /// and replaces the controller's former
        /// <c>ClearFolderItems</c> + <c>SetFolderItems</c> + <c>SetFolderSelectedIndex</c> +
        /// <c>SetFolderDroppedDown</c> composition.
        ///
        /// The order is load-bearing:
        /// <list type="number">
        /// <item>replace rows while preserving any open session (no native close/reopen, AC-3);</item>
        /// <item>open the selector only when it is closed, so a refresh causes no open-state churn;</item>
        /// <item>highlight the first selectable row, which requires an open session and therefore
        /// can only ever move pending state (AC-4).</item>
        /// </list>
        ///
        /// Exactly one render is posted per surface per call: the router mutations run synchronously
        /// and only the final state is published, so the intermediate replace and open renders are
        /// coalesced (AC-8, preserving issue #400 AC-12). The open-state notification is still raised
        /// when the selector actually opened, because that event is what drives the native open
        /// through the posted FIFO queue. <c>SelectionChanged</c> is never raised.
        ///
        /// Empty and banner-only result sets are deterministic no-ops for the open and highlight
        /// steps: <c>OpenSelector</c> refuses to open with no selectable row, and <c>HighlightRow</c>
        /// reports an unhandled transition (AC-9).
        /// </remarks>
        public void PresentSearchResults(IReadOnlyList<string> items)
        {
            _ = items ?? throw new ArgumentNullException(nameof(items));

            // A search refresh supersedes any in-flight suggestion upgrade, exactly as the legacy
            // Clear() step did, so a late upgrade cannot repopulate the row set behind the search.
            // Invalidate returns false once the coordinator is disposed; presenting into a disposed
            // pipeline is a no-op rather than a throw.
            if (!_upgradeLifetime.Invalidate())
            {
                return;
            }

            BreadcrumbSelectionTransition replaced = _router.ReplaceItemsPreservingSession(items);

            bool opened = false;
            if (!_router.GetSelectorState().IsOpen)
            {
                BreadcrumbSelectionTransition open = _router.OpenSelector();
                opened = open.Handled && open.OpenStateChanged;
            }

            BreadcrumbSelectionTransition highlighted = _router.HighlightRow(0);

            // Publish the final state once. RenderJson from the last handled transition already
            // reflects every preceding mutation, because each is produced from the same model.
            BreadcrumbSelectionTransition publication = highlighted.Handled
                ? highlighted
                : replaced;
            PublishSearchPresentation(publication, opened);
        }

        /// <summary>
        /// Posts exactly one render and one selector-state message for the composite, and raises
        /// <c>SelectorOpenStateChanged</c> only when the selector actually opened.
        /// </summary>
        private void PublishSearchPresentation(
            BreadcrumbSelectionTransition transition,
            bool openStateChanged
        )
        {
            _ = _dispatcher.Dispatch(() =>
            {
                if (transition.RenderJson != null)
                {
                    _messenger.PostJson(transition.RenderJson);
                }
                PostSelectorStateCore(_router.GetSelectorState());
                if (openStateChanged)
                {
                    SelectorOpenStateChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }
    }
}
