#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Classifies a breadcrumb row in the EfcViewer suggestion list (#349).
    /// </summary>
    public enum BreadcrumbRowKind
    {
        /// <summary>A ranked folder suggestion rendered as a breadcrumb.</summary>
        Suggestion,

        /// <summary>A <c>"===="</c>-prefixed section banner; never interactive.</summary>
        Banner,

        /// <summary>The <c>"Trash to Delete"</c> pseudo-row; selectable, no segments.</summary>
        TrashPseudoRow,
    }

    /// <summary>
    /// Per-row breadcrumb model plus its collapse/expand view-state machine. Pure and host-neutral
    /// (no WinForms/COM/WebView2 types). Transitions mutate ONLY view state; <see cref="Segments"/>,
    /// <see cref="Probability"/>, and the filing target path are immutable after construction.
    /// </summary>
    /// <remarks>
    /// No-op rules: <see cref="BreadcrumbRowKind.Banner"/> and
    /// <see cref="BreadcrumbRowKind.TrashPseudoRow"/> rows never collapse or expand; a leaf without
    /// subfolders is a toggle no-op. Arrow semantics (#1007): Right expands the active segment
    /// (restoring a collapse that hides it first), Left collapses the expanded children and never
    /// hides a segment.
    /// </remarks>
    public sealed partial class BreadcrumbRow
    {
        private readonly List<BreadcrumbSegment> _segments;
        private List<BreadcrumbSegment> _leafChildren = new List<BreadcrumbSegment>();
        private readonly Dictionary<int, FolderTreeNodeKey> _segmentKeys =
            new Dictionary<int, FolderTreeNodeKey>();
        private bool _leafFetchInFlight;

        /// <summary>
        /// Creates a breadcrumb row.
        /// </summary>
        /// <param name="rowId">Stable identifier used to correlate bridge messages. Required.</param>
        /// <param name="kind">Row kind; only <see cref="BreadcrumbRowKind.Suggestion"/> rows carry state.</param>
        /// <param name="segments">Ordered root-to-leaf segments (may be empty for banner/pseudo rows).</param>
        /// <param name="probability">Prediction probability joined by full-path equality, or null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rowId"/> or <paramref name="segments"/> is null.</exception>
        public BreadcrumbRow(
            string rowId,
            BreadcrumbRowKind kind,
            IEnumerable<BreadcrumbSegment> segments,
            double? probability,
            string? filingTarget = null
        )
        {
            RowId = rowId ?? throw new ArgumentNullException(nameof(rowId));
            Kind = kind;
            _segments = new List<BreadcrumbSegment>(
                segments ?? throw new ArgumentNullException(nameof(segments))
            );
            Probability = probability;
            FilingTarget =
                filingTarget
                ?? (_segments.Count == 0 ? string.Empty : _segments[_segments.Count - 1].FullPath);
            ActiveSegmentIndex =
                kind == BreadcrumbRowKind.Suggestion && _segments.Count > 0
                    ? _segments.Count - 1
                    : (int?)null;
        }

        /// <summary>Stable identifier used to correlate bridge messages.</summary>
        public string RowId { get; }

        /// <summary>Row kind (suggestion, banner, or trash pseudo-row).</summary>
        public BreadcrumbRowKind Kind { get; }

        /// <summary>Ordered root-to-leaf segments; immutable after construction.</summary>
        public IReadOnlyList<BreadcrumbSegment> Segments => _segments;

        /// <summary>Prediction probability, or null when no score was joined.</summary>
        public double? Probability { get; }

        /// <summary>
        /// Original presented filing target. This is independent of the full hierarchy paths
        /// carried by <see cref="Segments"/> and is used for normal folder selection.
        /// </summary>
        public string FilingTarget { get; }

        /// <summary>
        /// Current expand/select segment index. Suggestion rows begin with the predicted leaf
        /// active, while a typed non-leaf activation replaces it after validation.
        /// </summary>
        public int? ActiveSegmentIndex { get; private set; }

        /// <summary>Current active segment, or null for banner and pseudo rows.</summary>
        public BreadcrumbSegment? ActiveSegment =>
            ActiveSegmentIndex.HasValue ? _segments[ActiveSegmentIndex.Value] : null;

        /// <summary>Stable hierarchy key for the active segment, or null until provider-bound.</summary>
        public FolderTreeNodeKey? ActiveSegmentKey =>
            ActiveSegmentIndex.HasValue
            && _segmentKeys.TryGetValue(ActiveSegmentIndex.Value, out FolderTreeNodeKey key)
                ? key
                : null;

        /// <summary>
        /// Index of the segment after which the breadcrumb is collapsed, or null when fully
        /// expanded. The segment at this index is the now-terminal segment carrying the re-expand
        /// affordance.
        /// </summary>
        public int? CollapsedAfterIndex { get; private set; }

        /// <summary>True when the breadcrumb is collapsed after a non-leaf segment.</summary>
        public bool IsCollapsed => CollapsedAfterIndex.HasValue;

        /// <summary>True when the leaf's immediate subfolders are shown.</summary>
        public bool IsLeafExpanded { get; private set; }

        /// <summary>Immediate subfolders of the leaf, populated via <see cref="SetLeafChildren"/>.</summary>
        public IReadOnlyList<BreadcrumbSegment> LeafChildren => _leafChildren;

        /// <summary>
        /// True once a fetch stored the active segment's children (#1007 AC14), so an empty
        /// <see cref="LeafChildren"/> means "no children" rather than "not fetched yet".
        /// </summary>
        internal bool LeafChildrenLoaded { get; private set; }

        /// <summary>True when a fetch stored zero children for the active segment (#1007 AC14).</summary>
        internal bool HasNoLoadedChildren => LeafChildrenLoaded && _leafChildren.Count == 0;

        /// <summary>The anchored predicted leaf segment, or null when the row has no segments.</summary>
        public BreadcrumbSegment? LeafSegment =>
            _segments.Count > 0 ? _segments[_segments.Count - 1] : null;

        /// <summary>
        /// Associates a root-to-leaf provider identity with a segment. Invalid or non-suggestion
        /// indexes are ignored so malformed bridge state cannot create a selectable key.
        /// </summary>
        public void SetSegmentKey(int segmentIndex, FolderTreeNodeKey key)
        {
            if (
                Kind != BreadcrumbRowKind.Suggestion
                || key == null
                || segmentIndex < 0
                || segmentIndex >= _segments.Count
            )
            {
                return;
            }

            _segmentKeys[segmentIndex] = key;
        }

        /// <summary>
        /// Activates a validated non-leaf segment. The active segment owns subsequent expansion
        /// and its immediate-child projection; invalid, leaf, banner, and pseudo-row requests are
        /// deterministic no-ops.
        /// </summary>
        public bool ActivateSegment(int segmentIndex)
        {
            if (
                Kind != BreadcrumbRowKind.Suggestion
                || segmentIndex < 0
                || segmentIndex >= _segments.Count - 1
                || !_segmentKeys.ContainsKey(segmentIndex)
            )
            {
                return false;
            }

            if (ActiveSegmentIndex == segmentIndex)
            {
                return false;
            }

            ActiveSegmentIndex = segmentIndex;
            _leafChildren = new List<BreadcrumbSegment>();
            LeafChildrenLoaded = false;
            ClearDescendants();
            IsLeafExpanded = false;
            return true;
        }

        /// <summary>Returns the active child at the validated index, or null for an invalid request.</summary>
        public BreadcrumbSegment? GetActiveChild(int childIndex)
        {
            if (
                Kind != BreadcrumbRowKind.Suggestion
                || !IsLeafExpanded
                || childIndex < 0
                || childIndex >= _leafChildren.Count
            )
            {
                return null;
            }

            return _leafChildren[childIndex];
        }

        /// <summary>
        /// Collapses the breadcrumb after the non-leaf segment at <paramref name="segmentIndex"/>:
        /// all downstream segments (including the leaf) are hidden and the now-terminal segment
        /// carries the re-expand affordance. Collapsing also hides any expanded leaf children.
        /// </summary>
        /// <param name="segmentIndex">Index of a non-leaf segment (0-based).</param>
        /// <returns>True when the view state changed; false for the documented no-ops.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="segmentIndex"/> is outside the segment list of a suggestion row.
        /// </exception>
        public bool CollapseAfter(int segmentIndex)
        {
            if (Kind != BreadcrumbRowKind.Suggestion)
            {
                return false; // Banner/pseudo rows never collapse.
            }

            if (segmentIndex < 0 || segmentIndex >= _segments.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segmentIndex),
                    segmentIndex,
                    $"Segment index must be within [0, {_segments.Count - 1}] for row '{RowId}'."
                );
            }

            if (segmentIndex == _segments.Count - 1)
            {
                return false; // Collapse-after applies to non-leaf segments only.
            }

            if (CollapsedAfterIndex == segmentIndex)
            {
                return false;
            }

            CollapsedAfterIndex = segmentIndex;
            IsLeafExpanded = false;
            ClearDescendants();
            return true;
        }

        /// <summary>
        /// Restores the full breadcrumb after a collapse. No-op when not collapsed.
        /// </summary>
        /// <returns>True when the view state changed.</returns>
        public bool ReExpand()
        {
            if (!IsCollapsed)
            {
                return false;
            }

            CollapsedAfterIndex = null;
            return true;
        }

        /// <summary>
        /// Stores the leaf's immediate subfolders. Valid only on a suggestion row whose leaf has
        /// subfolders; otherwise a no-op.
        /// </summary>
        /// <param name="children">Immediate child segments of the leaf.</param>
        /// <returns>True when the children list was stored.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="children"/> is null.</exception>
        public bool SetLeafChildren(IEnumerable<BreadcrumbSegment> children)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            if (!CanExpandActiveSegment())
            {
                return false;
            }

            _leafChildren = new List<BreadcrumbSegment>(children);
            LeafChildrenLoaded = true;
            ClearDescendants(); // Key-less children carry no expandable descendants.
            return true;
        }

        /// <summary>
        /// Toggles the active segment's expand state. No-op for banner/pseudo rows, for a segment
        /// without subfolders, and while a collapse hides the active segment. A row shortened at
        /// its active segment (#1007 R3) can expand that segment.
        /// </summary>
        /// <returns>True when the view state changed.</returns>
        public bool ToggleLeafExpanded()
        {
            if (!CanExpandActiveSegment())
            {
                return false;
            }

            IsLeafExpanded = !IsLeafExpanded;
            if (!IsLeafExpanded)
            {
                ClearDescendants();
            }

            return true;
        }

        /// <summary>
        /// Left-arrow transition: collapses the expanded children. Left never hides a segment
        /// (#1007 AC7), so a row without an expansion is a no-op, as are banner/pseudo rows.
        /// </summary>
        /// <returns>True when the view state changed.</returns>
        public bool LeftArrow()
        {
            if (Kind != BreadcrumbRowKind.Suggestion || !IsLeafExpanded)
            {
                return false;
            }

            IsLeafExpanded = false;
            ClearDescendants();
            return true;
        }

        /// <summary>
        /// Right-arrow transition: restores a collapse that hides the active segment; otherwise
        /// expands the active segment when it has subfolders, including on a row shortened at its
        /// active segment (#1007 R3). No-op for banner/pseudo rows, a segment without subfolders,
        /// and an expanded row.
        /// </summary>
        /// <returns>True when the view state changed.</returns>
        public bool RightArrow()
        {
            if (Kind != BreadcrumbRowKind.Suggestion || _segments.Count == 0)
            {
                return false;
            }

            if (IsCollapsed && !IsAnchoredCollapse)
            {
                return ReExpand();
            }

            return ExpandLeaf();
        }

        /// <summary>
        /// Idempotent expand-to-true of the active segment (#1007 R4): false when already
        /// expanded or when the segment cannot expand, so overlapping completions cannot toggle
        /// the row back to collapsed.
        /// </summary>
        /// <returns>True when the view state changed.</returns>
        public bool ExpandLeaf()
        {
            if (!CanExpandActiveSegment() || IsLeafExpanded)
            {
                return false;
            }

            IsLeafExpanded = true;
            return true;
        }

        /// <summary>
        /// Starts the single in-flight fetch of the active segment's children (#1007 S7).
        /// </summary>
        /// <returns>False while another fetch for this row is in flight.</returns>
        public bool TryBeginLeafFetch()
        {
            if (_leafFetchInFlight)
            {
                return false;
            }

            _leafFetchInFlight = true;
            return true;
        }

        /// <summary>Ends the fetch started by <see cref="TryBeginLeafFetch"/>.</summary>
        public void EndLeafFetch() => _leafFetchInFlight = false;

        /// <summary>
        /// True when the row is shortened at its active segment (#1007 R3), as a segment
        /// double-click or Ctrl+Left leaves it; the active segment is then the visible terminal.
        /// </summary>
        public bool IsAnchoredCollapse =>
            CollapsedAfterIndex.HasValue && CollapsedAfterIndex == ActiveSegmentIndex;

        /// <summary>
        /// Projects the currently visible segments: all segments when fully expanded, or the
        /// segments up to and including <see cref="CollapsedAfterIndex"/> when collapsed.
        /// </summary>
        /// <returns>The visible segment list in root-to-terminal order.</returns>
        public IReadOnlyList<BreadcrumbSegment> VisibleSegments()
        {
            if (!CollapsedAfterIndex.HasValue)
            {
                return _segments;
            }

            return _segments.GetRange(0, CollapsedAfterIndex.Value + 1);
        }

        // R3: a collapse hides the active segment unless it is anchored at that segment.
        private bool CanExpandActiveSegment()
        {
            return Kind == BreadcrumbRowKind.Suggestion
                && ActiveSegment?.HasSubfolders == true
                && (!IsCollapsed || IsAnchoredCollapse);
        }
    }
}
