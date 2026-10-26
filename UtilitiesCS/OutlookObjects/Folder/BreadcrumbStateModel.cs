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

        internal BreadcrumbDescendantNode? SelectedDescendant { get; private set; }

        internal string? ArchiveRoot { get; set; }

        internal void SelectDescendant(
            int rowIndex,
            BreadcrumbDescendantNode node,
            int legacyIndex = -1
        )
        {
            SelectRow(rowIndex);
            SelectedDescendant = node;
            _selectedSubfolderIndex = legacyIndex;
        }

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
            SelectedDescendant = null;
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
            SelectedDescendant = null;
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
        /// The visible order (#1007): each selectable row entry followed by its shown descendant
        /// nodes in pre-order.
        /// </summary>
        internal List<BreadcrumbVisiblePosition> VisiblePositions()
        {
            var order = new List<BreadcrumbVisiblePosition>();
            for (int index = 0; index < _rows.Count; index++)
            {
                BreadcrumbStateRow row = _rows[index];
                if (!row.IsSelectable)
                {
                    continue;
                }
                var root = row.LeafExpanded ? row.DescendantRoot : null;
                bool zeroLoaded = root != null && root.Loaded && root.Children.Count == 0;
                order.Add(
                    new BreadcrumbVisiblePosition(
                        index,
                        null,
                        null,
                        -1,
                        row.ActiveSegmentHasSubfolders && !zeroLoaded,
                        row.LeafExpanded
                    )
                );
                if (root != null)
                {
                    BreadcrumbVisibleOrder.AppendNodes(index, root.Children, null, 0, order);
                }
            }
            return order;
        }

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
            SelectedDescendant = null;
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
            SelectedDescendant = row.DescendantRoot!.Children[subfolderIndex];
        }

        /// <summary>
        /// Right-arrow transition on the selected row (#1007, D-Q4): expands the active segment
        /// when its affordance is available. A row shortened at its active segment keeps its
        /// shape and lists that segment's children (R3, D-Q23); a collapse anchored elsewhere is
        /// restored as before. Returns false when nothing changed so the caller can report an
        /// unhandled arrow (FR-6 legacy fall-through). A successful expansion still requires the
        /// caller to fetch and attach subfolders.
        /// </summary>
        public bool RightArrow()
        {
            var row = SelectedRow;
            if (row == null)
            {
                return false;
            }
            return Expand(row);
        }

        /// <summary>Applies the Right expansion rule to <paramref name="row"/> (#1007 R3).</summary>
        internal static bool Expand(BreadcrumbStateRow row)
        {
            if (
                row.CollapsedAfterIndex != null
                && row.CollapsedAfterIndex != row.ActiveSegmentIndex
            )
            {
                row.ReExpand();
                return true;
            }
            return row.TryExpandActiveSegment();
        }

        /// <summary>
        /// Left-arrow transition on the selected row: closes an open expansion and never selects a
        /// parent (#1007 AC7, D-Q9). A subfolder selection hidden by the collapse moves to its
        /// nearest visible ancestor, the row itself. Returns false when nothing changed so the
        /// caller can report an unhandled arrow (FR-6 legacy fall-through).
        /// </summary>
        public bool LeftArrow()
        {
            var row = SelectedRow;
            if (row == null || !row.TryCollapseLeaf())
            {
                return false;
            }
            _selectedSubfolderIndex = -1;
            SelectedDescendant = null;
            return true;
        }
    }
}
