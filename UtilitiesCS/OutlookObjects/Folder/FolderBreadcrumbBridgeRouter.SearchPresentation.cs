#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #438: the session-preserving search-presentation surface of the router.
    /// <para>
    /// Held on a second partial-class part so <c>FolderBreadcrumbBridgeRouter.cs</c> (485 lines)
    /// stays clear of the repository's 500-line ceiling.
    /// </para>
    /// </summary>
    public sealed partial class FolderBreadcrumbBridgeRouter
    {
        /// <summary>
        /// Atomically replaces every plain (Path B) row with <paramref name="items"/> while keeping
        /// an open selector session alive.
        /// </summary>
        /// <param name="items">The exact search-result strings, carried verbatim.</param>
        /// <returns>A handled transition reporting <c>RenderRequired</c> only.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
        /// <remarks>
        /// This is the search-refresh counterpart of the suggestions path: it bumps the suggestion
        /// generation so an in-flight <see cref="SetSuggestionsAsync"/> cannot overwrite it, then
        /// performs a single <c>ReplaceRows</c> + <c>ReconcileRowsReplaced</c> pair. The existing
        /// <see cref="SetItems"/> is deliberately NOT reused: it calls <c>_model.Clear()</c>, which
        /// destroys the session's committed/original/pending identities instead of reconciling them,
        /// and it emits an additional intermediate state.
        ///
        /// The transition reports no <c>OpenStateChanged</c>, so a refresh while the popup is open
        /// causes no native close/reopen cycle (issue #438 AC-3), and no <c>SelectionChanged</c>, so
        /// the controller's cached folder is untouched. Exactly one render is produced per call,
        /// preserving the one-render-per-surface contract of issue #400 AC-12 (issue #438 AC-8).
        /// An empty item list is a deterministic no-throw replacement with an empty row set (AC-9).
        /// </remarks>
        public BreadcrumbSelectionTransition ReplaceItemsPreservingSession(
            IReadOnlyList<string> items
        )
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Interlocked.Increment(ref _suggestionGeneration);
            IReadOnlyList<BreadcrumbStateRow> replacement = BuildPlainRows(items);
            return Mutate(() =>
            {
                ReplaceRowsPreservingSession(replacement);
                return BreadcrumbSelectionEffects.Handled
                    | BreadcrumbSelectionEffects.RenderRequired;
            });
        }

        /// <summary>
        /// Highlights the first selectable row at or after <paramref name="index"/> in the open
        /// selector session, changing only the pending identity.
        /// </summary>
        /// <param name="index">The zero-based row index to start searching from.</param>
        /// <returns>
        /// A handled <c>RenderRequired</c>-only transition when a selectable row was found; an
        /// unhandled no-op transition for a closed session, an empty row set, or a banner-only set.
        /// </returns>
        /// <remarks>
        /// The router-level pass-through for <see cref="BreadcrumbSelectionSession.HighlightRow"/>;
        /// the session is router-private, so the search-presentation composite reaches it here.
        /// Publishes no <c>SelectionChanged</c> and leaves the committed model selection untouched
        /// (issue #438 AC-4).
        /// </remarks>
        public BreadcrumbSelectionTransition HighlightRow(int index) =>
            Mutate(() => _selectionSession.HighlightRow(index));

        /// <summary>
        /// Projects verbatim search strings onto plain rows using the same stable-identity and
        /// banner-selectability rules as <c>AddPlainRows</c>, so a replacement snapshot is
        /// identity-compatible with rows produced by the append path.
        /// </summary>
        private static IReadOnlyList<BreadcrumbStateRow> BuildPlainRows(IReadOnlyList<string> items)
        {
            var rows = new List<BreadcrumbStateRow>(items.Count);
            for (int index = 0; index < items.Count; index++)
            {
                string item = items[index];
                rows.Add(
                    new BreadcrumbStateRow(
                        BreadcrumbRowIdentity.ForPlainRow(item, index),
                        item,
                        !BreadcrumbStateRow.IsBanner(item)
                    )
                );
            }
            return rows;
        }
    }
}
