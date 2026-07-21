#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// One visible row of the QuickFiler breadcrumb control: either a Path A suggestion row carrying
    /// a root-to-leaf ancestor chain of <see cref="FolderBreadcrumbSegment"/> plus an optional
    /// probability, or a Path B plain-string row carried verbatim without probability. Holds the
    /// per-row collapse/expand state; all transitions validate their preconditions and fail fast.
    /// </summary>
    public sealed class BreadcrumbStateRow
    {
        private static readonly IReadOnlyList<FolderBreadcrumbSegment> EmptySegments =
            new FolderBreadcrumbSegment[0];

        internal BreadcrumbStateRow(
            IReadOnlyList<FolderBreadcrumbSegment> chain,
            double? probability
        )
            : this(IdentityFromChain(chain), chain, probability) { }

        internal BreadcrumbStateRow(
            string identity,
            IReadOnlyList<FolderBreadcrumbSegment> chain,
            double? probability
        )
        {
            if (chain == null || chain.Count == 0)
            {
                throw new ArgumentException(
                    "A suggestion row requires a non-empty ancestor chain.",
                    nameof(chain)
                );
            }
            if (chain.Any(segment => segment == null))
            {
                throw new ArgumentException(
                    "The ancestor chain must not contain null segments.",
                    nameof(chain)
                );
            }

            Identity = RequireIdentity(identity);
            Chain = chain.ToArray();
            Probability = probability;
            VerbatimText = null;
            IsSelectable = true;
            IsScoredFallback = false;
            Subfolders = EmptySegments;
        }

        internal BreadcrumbStateRow(string verbatimText)
            : this(
                DefaultPlainIdentity(verbatimText),
                verbatimText,
                !IsBanner(verbatimText),
                null,
                false
            ) { }

        internal BreadcrumbStateRow(string identity, string verbatimText, bool isSelectable)
            : this(identity, verbatimText, isSelectable, null, false) { }

        internal BreadcrumbStateRow(string identity, string fallbackText, double? probability)
            : this(identity, fallbackText, true, probability, true) { }

        private BreadcrumbStateRow(
            string identity,
            string verbatimText,
            bool isSelectable,
            double? probability,
            bool isScoredFallback
        )
        {
            Identity = RequireIdentity(identity);
            VerbatimText = verbatimText ?? throw new ArgumentNullException(nameof(verbatimText));
            Chain = EmptySegments;
            Probability = probability;
            IsSelectable = isSelectable;
            IsScoredFallback = isScoredFallback;
            Subfolders = EmptySegments;
        }

        /// <summary>Stable row identity retained while fallback display data is upgraded.</summary>
        public string Identity { get; }

        /// <summary>True for a Path A suggestion row; false for a Path B plain-string row.</summary>
        public bool IsSuggestion => VerbatimText == null;

        /// <summary>True for an unresolved scored suggestion carrying fallback display text.</summary>
        public bool IsScoredFallback { get; }

        /// <summary>True when selector navigation and activation may choose this row.</summary>
        public bool IsSelectable { get; }

        /// <summary>Fallback display text for an unresolved scored suggestion; otherwise null.</summary>
        public string? FallbackText => IsScoredFallback ? VerbatimText : null;

        /// <summary>Root-first ancestor chain for a suggestion row; empty for a plain row.</summary>
        public IReadOnlyList<FolderBreadcrumbSegment> Chain { get; }

        /// <summary>The consumed prediction probability, or null when the row carries none.</summary>
        public double? Probability { get; }

        /// <summary>The exact Path B string (returned verbatim on selection), or null for Path A rows.</summary>
        public string? VerbatimText { get; }

        /// <summary>
        /// The segment index after which the row is collapsed (that segment is the visible terminal),
        /// or null when the full chain is visible.
        /// </summary>
        public int? CollapsedAfterIndex { get; private set; }

        /// <summary>True while the leaf's subfolder list is expanded.</summary>
        public bool LeafExpanded { get; private set; }

        /// <summary>The fetched immediate subfolders shown while <see cref="LeafExpanded"/>.</summary>
        public IReadOnlyList<FolderBreadcrumbSegment> Subfolders { get; private set; }

        /// <summary>
        /// True when the leaf carries the plus/minus affordance: a fully-expanded suggestion row
        /// whose leaf segment has real subfolders (FR-2).
        /// </summary>
        public bool LeafHasSubfolders =>
            IsSuggestion && CollapsedAfterIndex == null && Chain[Chain.Count - 1].HasChildren;

        /// <summary>
        /// Collapses the row after the non-leaf segment at <paramref name="segmentIndex"/> (FR-3):
        /// downstream segments and the leaf are hidden and the segment becomes the visible terminal.
        /// Any open leaf expansion is closed.
        /// </summary>
        /// <exception cref="InvalidOperationException">The row is a plain (Path B) row.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="segmentIndex"/> is negative, beyond the chain, or the leaf index (the
        /// leaf cannot be collapsed-after).
        /// </exception>
        public void CollapseAfter(int segmentIndex)
        {
            if (!IsSuggestion)
            {
                throw new InvalidOperationException(
                    "Collapse is defined only for suggestion rows with an ancestor chain."
                );
            }
            if (segmentIndex < 0 || segmentIndex >= Chain.Count - 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segmentIndex),
                    segmentIndex,
                    $"Collapse-after requires a non-leaf segment index in [0, {Chain.Count - 2}]."
                );
            }

            CollapsedAfterIndex = segmentIndex;
            LeafExpanded = false;
            Subfolders = EmptySegments;
        }

        /// <summary>Restores the full chain after a collapse; a no-op when not collapsed (FR-3).</summary>
        public void ReExpand()
        {
            CollapsedAfterIndex = null;
        }

        /// <summary>
        /// Opens the leaf subfolder expansion when the affordance is available (FR-2); returns false
        /// (no-op by contract) for plain rows, collapsed rows, affordance-less leaves, or when
        /// already expanded, so the caller can fall through to legacy behavior.
        /// </summary>
        public bool TryExpandLeaf()
        {
            if (!LeafHasSubfolders || LeafExpanded)
            {
                return false;
            }

            LeafExpanded = true;
            return true;
        }

        /// <summary>
        /// Closes the leaf subfolder expansion; returns false (no-op) when nothing is expanded.
        /// </summary>
        public bool TryCollapseLeaf()
        {
            if (!LeafExpanded)
            {
                return false;
            }

            LeafExpanded = false;
            Subfolders = EmptySegments;
            return true;
        }

        /// <summary>
        /// Stores the fetched immediate subfolders for the open expansion (FR-4).
        /// </summary>
        /// <exception cref="InvalidOperationException">The leaf is not expanded.</exception>
        public void SetSubfolders(IReadOnlyList<FolderBreadcrumbSegment> subfolders)
        {
            if (!LeafExpanded)
            {
                throw new InvalidOperationException(
                    "Subfolders can be attached only while the leaf expansion is open."
                );
            }

            Subfolders = (subfolders ?? EmptySegments).ToArray();
        }

        /// <summary>Resets collapse, expansion, and subfolder state to the initial full chain.</summary>
        public void Reset()
        {
            CollapsedAfterIndex = null;
            LeafExpanded = false;
            Subfolders = EmptySegments;
        }

        private static string IdentityFromChain(IReadOnlyList<FolderBreadcrumbSegment> chain)
        {
            if (chain == null || chain.Count == 0)
            {
                throw new ArgumentException(
                    "A suggestion row requires a non-empty ancestor chain.",
                    nameof(chain)
                );
            }
            return chain[chain.Count - 1]?.Key.ToString()
                ?? throw new ArgumentException(
                    "The ancestor chain must not contain null segments.",
                    nameof(chain)
                );
        }

        private static string DefaultPlainIdentity(string verbatimText)
        {
            if (verbatimText == null)
            {
                throw new ArgumentNullException(nameof(verbatimText));
            }
            return string.IsNullOrWhiteSpace(verbatimText) ? "plain-empty" : verbatimText;
        }

        private static bool IsBanner(string verbatimText) =>
            verbatimText?.StartsWith(BreadcrumbRowBuilder.BannerPrefix, StringComparison.Ordinal)
            == true;

        private static string RequireIdentity(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity))
            {
                throw new ArgumentException(
                    "A non-empty stable identity is required.",
                    nameof(identity)
                );
            }
            return identity;
        }
    }

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
            _rows.Add(new BreadcrumbStateRow(chain, probability));
        }

        /// <summary>Appends a resolved scored suggestion with an explicit stable identity.</summary>
        public void AddSuggestionRow(
            string identity,
            IReadOnlyList<FolderBreadcrumbSegment> chain,
            double? probability
        )
        {
            _rows.Add(new BreadcrumbStateRow(identity, chain, probability));
        }

        /// <summary>Appends a scored suggestion before hierarchy display data is available.</summary>
        public void AddScoredFallbackRow(string identity, string fallbackText, double? probability)
        {
            _rows.Add(new BreadcrumbStateRow(identity, fallbackText, probability));
        }

        /// <summary>Appends a Path B plain-string row carried verbatim without probability.</summary>
        public void AddPlainRow(string verbatimText)
        {
            _rows.Add(new BreadcrumbStateRow(verbatimText));
        }

        /// <summary>Appends a plain row with explicit stable identity and selectability.</summary>
        public void AddPlainRow(string identity, string verbatimText, bool isSelectable)
        {
            _rows.Add(new BreadcrumbStateRow(identity, verbatimText, isSelectable));
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
            if (row.CollapsedAfterIndex != null)
            {
                row.ReExpand();
                return true;
            }
            return row.TryExpandLeaf();
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
            if (_selectedSubfolderIndex >= 0)
            {
                _selectedSubfolderIndex = -1;
            }
            return row.TryCollapseLeaf();
        }
    }
}
