#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Pure, host-neutral collapse/expand state machine for the QuickFiler WebView2 breadcrumb
    /// (#351): ordered rows, selected-row/subfolder tracking, and keyboard transitions. Mirrors the
    /// tested <see cref="FolderTreeStateModel"/> precedent — no WinForms, COM, WebView2, or I/O work
    /// — and is NOT coverage-exempt.
    /// </summary>
    public sealed class BreadcrumbStateModel
    {
        private List<BreadcrumbStateRow> _rows = new List<BreadcrumbStateRow>();
        private int _selectedIndex = -1;
        private int _selectedSubfolderIndex = -1;

        /// <summary>The rows in display order.</summary>
        public IReadOnlyList<BreadcrumbStateRow> Rows => _rows;

        /// <summary>The selected row index, or -1 when nothing is selected.</summary>
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// The selected subfolder index within the selected row's open expansion, or -1 when the
        /// selection is the row itself.
        /// </summary>
        public int SelectedSubfolderIndex => _selectedSubfolderIndex;

        /// <summary>The selected row, or null when nothing is selected.</summary>
        public BreadcrumbStateRow? SelectedRow => _selectedIndex < 0 ? null : _rows[_selectedIndex];

        /// <summary>Removes all rows and clears the selection.</summary>
        public void Clear()
        {
            _rows.Clear();
            _selectedIndex = -1;
            _selectedSubfolderIndex = -1;
        }

        /// <summary>
        /// Atomically replaces all rows with <paramref name="rows"/> via a single backing-list
        /// reference swap, so an observer never sees a transiently cleared or partially-populated
        /// model during a rebuild (#398). The current selection is preserved when its index is still
        /// valid against the new row count; any subfolder selection is reset. No intervening
        /// mutation or <c>await</c> occurs, so a concurrent host selection cannot race an empty
        /// window.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="rows"/> is null.</exception>
        public void ReplaceRows(IReadOnlyList<BreadcrumbStateRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            var replacement = new List<BreadcrumbStateRow>(rows);
            BreadcrumbRowIdentity.RequireUnique(replacement);
            // Reconcile the selection against the new count BEFORE publishing the new list so no
            // reader can observe the replacement list paired with a stale out-of-range index.
            if (_selectedIndex >= replacement.Count)
            {
                _selectedIndex = -1;
            }
            _selectedSubfolderIndex = -1;
            _rows = replacement;
        }

        /// <summary>Appends a Path A suggestion row (root-first chain, optional probability).</summary>
        public void AddSuggestionRow(
            IReadOnlyList<FolderBreadcrumbSegment> chain,
            double? probability
        )
        {
            AddSuggestionRow(BreadcrumbStateRow.IdentityFromChain(chain), chain, probability);
        }

        /// <summary>Appends a resolved scored suggestion with an explicit stable identity.</summary>
        public void AddSuggestionRow(
            string identity,
            IReadOnlyList<FolderBreadcrumbSegment> chain,
            double? probability
        )
        {
            _rows.Add(new BreadcrumbStateRow(UniqueIdentity(identity), chain, probability));
        }

        /// <summary>Appends a scored suggestion before hierarchy display data is available.</summary>
        public void AddScoredFallbackRow(string identity, string fallbackText, double? probability)
        {
            _rows.Add(new BreadcrumbStateRow(UniqueIdentity(identity), fallbackText, probability));
        }

        /// <summary>Appends a Path B plain-string row carried verbatim without probability.</summary>
        public void AddPlainRow(string verbatimText)
        {
            AddPlainRow(
                BreadcrumbStateRow.DefaultPlainIdentity(verbatimText),
                verbatimText,
                !BreadcrumbStateRow.IsBanner(verbatimText)
            );
        }

        /// <summary>Appends a plain row with explicit stable identity and selectability.</summary>
        public void AddPlainRow(string identity, string verbatimText, bool isSelectable)
        {
            _rows.Add(new BreadcrumbStateRow(UniqueIdentity(identity), verbatimText, isSelectable));
        }

        private string UniqueIdentity(string identity) =>
            BreadcrumbRowIdentity.Disambiguate(identity, _rows);

        /// <summary>
        /// Selects the row at <paramref name="index"/> (or -1 to clear the selection) and resets any
        /// subfolder selection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not -1 and not a valid row index.</exception>
        public void SelectRow(int index)
        {
            if (index < -1 || index >= _rows.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Row selection requires -1 or an index in [0, {_rows.Count - 1}]."
                );
            }

            _selectedIndex = index;
            _selectedSubfolderIndex = -1;
        }

        /// <summary>
        /// Selects a subfolder of the selected row's open leaf expansion.
        /// </summary>
        /// <exception cref="InvalidOperationException">No row is selected, or the selected row has no open expansion.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="subfolderIndex"/> is outside the fetched subfolder list.</exception>
        public void SelectSubfolder(int subfolderIndex)
        {
            var row = SelectedRow;
            if (row == null || !row.LeafExpanded)
            {
                throw new InvalidOperationException(
                    "Subfolder selection requires a selected row with an open leaf expansion."
                );
            }
            if (subfolderIndex < 0 || subfolderIndex >= row.Subfolders.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(subfolderIndex),
                    subfolderIndex,
                    $"Subfolder selection requires an index in [0, {row.Subfolders.Count - 1}]."
                );
            }

            _selectedSubfolderIndex = subfolderIndex;
        }

        /// <summary>
        /// Right-arrow transition on the selected row: re-expands a collapsed chain, else opens the
        /// leaf expansion when the affordance is available. Returns false when nothing changed so the
        /// caller can report an unhandled arrow (FR-6 legacy fall-through). A successful leaf
        /// expansion still requires the caller to fetch and attach subfolders.
        /// </summary>
        public bool RightArrow()
        {
            var row = SelectedRow;
            if (row == null)
            {
                return false;
            }
            // #440: attempt the tree transition first (decision D1 handling order).
            if (TryRightTreeTransition(row))
            {
                return true;
            }
            if (row.CollapsedAfterIndex != null)
            {
                row.ReExpand();
                return true;
            }
            return row.TryExpandLeaf();
        }

        /// <summary>
        /// #440 Right tree transition. Available only once a non-leaf node has been selected:
        /// expands that node when its expansion is closed, and otherwise descends into child
        /// index 0 of the fetched subfolders. Returns false when no transition applies, so the
        /// caller falls through to the pre-existing behavior.
        /// </summary>
        private bool TryRightTreeTransition(BreadcrumbStateRow row)
        {
            int? activeIndex = row.ActiveSegmentIndex;
            if (!activeIndex.HasValue || activeIndex.Value >= row.Chain.Count - 1)
            {
                return false; // Leaf-anchored: no node has been selected yet.
            }
            if (!row.LeafExpanded)
            {
                // The collapse is cleared as PART of the transition, because the expansion of the
                // selected node is not visible while downstream segments are hidden.
                row.ReExpand();
                return row.TryExpandActiveSegment();
            }
            if (row.GetActiveChild(0) == null)
            {
                return false; // Nothing to descend into.
            }

            SelectSubfolder(0);
            return true;
        }

        /// <summary>
        /// Left-arrow transition on the selected row: closes an open leaf expansion. Returns false
        /// when nothing changed so the caller can report an unhandled arrow (FR-6 legacy fall-through).
        /// </summary>
        public bool LeftArrow()
        {
            var row = SelectedRow;
            if (row == null)
            {
                return false;
            }
            // #440: attempt the tree transition first (decision D1 handling order). It selects the
            // parent of the leaf-anchored node; once a non-leaf node is selected, or while a child
            // of the open expansion is selected, no parent-select is available and the pre-existing
            // behavior runs unchanged.
            int? activeIndex = row.ActiveSegmentIndex;
            if (
                _selectedSubfolderIndex < 0
                && activeIndex.HasValue
                && activeIndex.Value == row.Chain.Count - 1
                && row.ActivateSegment(activeIndex.Value - 1)
            )
            {
                return true;
            }
            if (_selectedSubfolderIndex >= 0)
            {
                _selectedSubfolderIndex = -1;
            }
            return row.TryCollapseLeaf();
        }
    }
}
