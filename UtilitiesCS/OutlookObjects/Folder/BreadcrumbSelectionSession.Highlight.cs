#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #438: the pending-only highlight transition used by the folder-search path.
    /// <para>
    /// Held on a second partial-class part so <c>BreadcrumbSelectionSession.cs</c> (474 lines) stays
    /// clear of the repository's 500-line ceiling.
    /// </para>
    /// </summary>
    internal sealed partial class BreadcrumbSelectionSession
    {
        /// <summary>
        /// Highlights the first selectable row at or after <paramref name="index"/> by changing only
        /// the open session's <see cref="PendingIdentity"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based row index to start searching from. Values below zero are treated as zero;
        /// values at or beyond the row count find no target and are a no-op.
        /// </param>
        /// <returns>
        /// <c>Handled | RenderRequired</c> when a selectable row was found;
        /// <see cref="BreadcrumbSelectionEffects.None"/> otherwise.
        /// </returns>
        /// <remarks>
        /// This is the search-path counterpart of open Up/Down navigation (<see cref="Move"/>), which
        /// also changes only pending state while the session is open. It deliberately reports neither
        /// <c>SelectionChanged</c> nor <c>OpenStateChanged</c> and never touches the committed model
        /// selection, so the collapsed surface and <c>GetSelectedFolder()</c> keep the identity that
        /// was committed before the search session opened, and Escape restores it through the
        /// existing <see cref="Cancel"/> semantics (issue #438 AC-4, AC-5).
        ///
        /// A closed session, an empty row set, and a banner-only row set are deterministic no-ops
        /// that return <see cref="BreadcrumbSelectionEffects.None"/> without throwing (AC-9).
        /// Requiring an open session is what makes the highlight incapable of committing: the
        /// coordinator composite opens the session before highlighting.
        /// </remarks>
        public BreadcrumbSelectionEffects HighlightRow(int index)
        {
            if (!IsOpen)
            {
                return BreadcrumbSelectionEffects.None;
            }

            // NextSelectableIndex scans from start + step, so start one before the requested index to
            // make the search inclusive of index itself. A negative index clamps to the first row.
            int target = NextSelectableIndex(index < 0 ? -1 : index - 1, 1);
            if (target < 0)
            {
                return BreadcrumbSelectionEffects.None;
            }

            PendingIdentity = _model.Rows[target].Identity;
            return BreadcrumbSelectionEffects.Handled | BreadcrumbSelectionEffects.RenderRequired;
        }
    }
}
