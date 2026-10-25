#nullable enable
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007: open-list navigation over the visible order (rows plus shown descendants), the
    /// pending descendant node, boundary, page and parent moves, and the row-shape snapshot.
    /// While the list is open every move changes only <see cref="PendingIdentity"/> and
    /// <see cref="PendingNode"/>; the committed model selection is untouched until
    /// <see cref="CommitPending"/> (AC4).
    /// </summary>
    internal sealed partial class BreadcrumbSelectionSession
    {
        private readonly List<(BreadcrumbStateRow Row, int? Active, int? Collapsed)> _openShape =
            new List<(BreadcrumbStateRow Row, int? Active, int? Collapsed)>();

        /// <summary>The pending descendant node on the pending row, or null for a row entry.</summary>
        public BreadcrumbDescendantNode? PendingNode { get; private set; }

        /// <summary>
        /// Moves the open-list pending highlight to the first or last visible entry (AC11).
        /// </summary>
        /// <returns>True when the pending entry changed; false when closed or already there.</returns>
        public bool MoveToBoundary(bool first)
        {
            if (!IsOpen)
            {
                return false;
            }
            var order = _model.VisiblePositions();
            int target = first
                ? BreadcrumbVisibleOrder.First(order)
                : BreadcrumbVisibleOrder.Last(order);
            return target >= 0 && target != PendingPosition(order) && Pend(order[target]);
        }

        /// <summary>
        /// Moves the open-list pending highlight by <paramref name="pageSize"/> visible entries,
        /// clamped at both ends without wrap (AC11, S3).
        /// </summary>
        /// <returns>False when closed, when <paramref name="pageSize"/> is below 1, or at the end.</returns>
        public bool MovePage(bool previous, int pageSize)
        {
            if (!IsOpen || pageSize < 1)
            {
                return false;
            }
            return MoveOpen(previous ? -pageSize : pageSize);
        }

        /// <summary>
        /// Ctrl+Left (AC8, D-Q19). Open: a pending node pends its parent entry and keeps the
        /// expansion; a pending row is shortened at its parent segment, pending only, and Escape
        /// restores the shape (D-Q24). Closed: the parent is committed immediately. Active index 0
        /// is a no-op.
        /// </summary>
        public bool MoveToParent()
        {
            if (!IsOpen)
            {
                return CommitParent();
            }
            var order = _model.VisiblePositions();
            int current = PendingPosition(order);
            if (current < 0)
            {
                return false;
            }
            if (!order[current].IsRowEntry)
            {
                return Pend(order[BreadcrumbVisibleOrder.ParentOf(order, current)]);
            }
            BreadcrumbStateRow row = _model.Rows[order[current].RowIndex];
            int? active = row.ActiveSegmentIndex;
            if (!active.HasValue || !row.ActivateSegment(active.Value - 1))
            {
                return false;
            }
            row.CollapseAfter(active.Value - 1);
            return true;
        }

        /// <summary>
        /// Commits the first or last selectable top-level row of a closed session (AC10, D-Q16).
        /// </summary>
        /// <returns>False when open, when no row is selectable, or when that row entry is already committed.</returns>
        public bool CommitBoundaryRow(bool first)
        {
            int target = first ? FirstSelectableIndex() : LastSelectableIndex();
            if (
                IsOpen
                || target < 0
                || (target == _model.SelectedIndex && _model.SelectedDescendant == null)
            )
            {
                return false;
            }
            _model.SelectRow(target);
            CommittedIdentity = _model.Rows[target].Identity;
            return true;
        }

        /// <summary>
        /// Closed-list Ctrl+Left (D-Q19): commits the parent immediately. A committed node commits
        /// its parent node, or the row entry at depth 0, keeping the expansion; a committed row
        /// entry activates and commits its parent segment and is shortened there.
        /// </summary>
        private bool CommitParent()
        {
            int rowIndex = _model.SelectedIndex;
            BreadcrumbStateRow? row = rowIndex < 0 ? null : _model.Rows[rowIndex];
            var node = _model.SelectedDescendant;
            var path = new List<BreadcrumbDescendantNode>();
            if (row == null || (node != null && !FindPath(row.DescendantRoot!, node, path)))
            {
                return false;
            }
            if (node != null)
            {
                var parent = path.Count > 1 ? path[path.Count - 2] : null;
                if (parent == null)
                {
                    _model.SelectRow(rowIndex);
                }
                else if (BreadcrumbSelectionMap.TryDescendantValue(_model, parent, out _))
                {
                    _model.SelectDescendant(rowIndex, parent);
                }
                return parent == null || ReferenceEquals(_model.SelectedDescendant, parent);
            }
            int? active = row.ActiveSegmentIndex;
            if (!active.HasValue || !row.ActivateSegment(active.Value - 1))
            {
                return false;
            }
            row.CollapseAfter(active.Value - 1);
            return true;
        }

        /// <summary>
        /// Selector segment double-click (AC9, D-Q13, D-Q27, S5): activates the segment when it is
        /// not already active, commits the row so it files to that segment's folder (S9), shortens
        /// the row there, and closes an open session. The leaf and out-of-range indices, plain rows
        /// and unknown identities are no-ops.
        /// </summary>
        internal BreadcrumbSelectionEffects DoubleClickSegment(string rowIdentity, int segmentIndex)
        {
            int rowIndex = IndexOfSelectable(rowIdentity);
            BreadcrumbStateRow? row = rowIndex < 0 ? null : _model.Rows[rowIndex];
            if (
                row == null
                || !row.IsSuggestion
                || segmentIndex < 0
                || segmentIndex >= row.Chain.Count - 1
            )
            {
                return BreadcrumbSelectionEffects.None;
            }
            bool wasOpen = IsOpen;
            row.ActivateSegment(segmentIndex);
            row.CollapseAfter(segmentIndex);
            _model.SelectRow(rowIndex);
            CommittedIdentity = row.Identity;
            if (wasOpen)
            {
                EndOpenSession();
            }
            return BreadcrumbSelectionEffects.Handled
                | BreadcrumbSelectionEffects.SelectionChanged
                | BreadcrumbSelectionEffects.RenderRequired
                | (
                    wasOpen
                        ? BreadcrumbSelectionEffects.OpenStateChanged
                        : BreadcrumbSelectionEffects.None
                );
        }

        /// <summary>
        /// Open-list single click on a non-leaf segment (D-Q27): moves only the pending highlight to
        /// the row; a closed session or an unknown identity is a no-op.
        /// </summary>
        internal BreadcrumbSelectionEffects HighlightIdentity(string identity)
        {
            int index = IndexOfSelectable(identity);
            return index < 0 ? BreadcrumbSelectionEffects.None : HighlightRow(index);
        }

        /// <summary>
        /// Moves a pending node hidden by a collapse to its nearest visible ancestor, or to the row
        /// entry when the row's own expansion closed (AC7, D-Q11).
        /// </summary>
        internal void ReconcilePendingNode()
        {
            var node = PendingNode;
            int rowIndex = IndexOfSelectable(PendingIdentity);
            if (node == null)
            {
                return;
            }
            BreadcrumbStateRow? row = rowIndex < 0 ? null : _model.Rows[rowIndex];
            var path = new List<BreadcrumbDescendantNode>();
            if (row == null || !row.LeafExpanded || !FindPath(row.DescendantRoot!, node, path))
            {
                PendingNode = null;
                return;
            }
            foreach (BreadcrumbDescendantNode ancestor in path)
            {
                if (ancestor != node && !ancestor.Expanded)
                {
                    PendingNode = ancestor;
                    return;
                }
            }
        }

        private bool MoveOpen(int delta)
        {
            var order = _model.VisiblePositions();
            int current = PendingPosition(order);
            int target =
                current >= 0 ? BreadcrumbVisibleOrder.Step(order, current, delta)
                : delta > 0 ? BreadcrumbVisibleOrder.First(order)
                : BreadcrumbVisibleOrder.Last(order);
            return target >= 0 && Pend(order[target]);
        }

        /// <summary>Visible-order index of the pending entry; a hidden node falls back to its row.</summary>
        internal int PendingPosition(IReadOnlyList<BreadcrumbVisiblePosition> order)
        {
            int rowIndex = IndexOfSelectable(PendingIdentity);
            if (rowIndex < 0)
            {
                return -1;
            }
            int index = BreadcrumbVisibleOrder.IndexOf(order, rowIndex, PendingNode?.Identity);
            return index >= 0 ? index : BreadcrumbVisibleOrder.IndexOf(order, rowIndex, null);
        }

        /// <summary>Moves the pending entry to <paramref name="position"/>; always true.</summary>
        internal bool Pend(BreadcrumbVisiblePosition position)
        {
            BreadcrumbStateRow row = _model.Rows[position.RowIndex];
            PendingIdentity = row.Identity;
            PendingNode =
                position.NodeId == null ? null : row.DescendantRoot?.Find(position.NodeId);
            return true;
        }

        /// <summary>Starts open-list state: the row-shape snapshot and the D-Q29 pending node.</summary>
        private void BeginOpenTree()
        {
            _openShape.Clear();
            foreach (BreadcrumbStateRow row in _model.Rows)
            {
                if (row.IsSuggestion)
                {
                    _openShape.Add((row, row.ActiveSegmentIndex, row.CollapsedAfterIndex));
                }
            }
            var descendant = _model.SelectedDescendant;
            bool visible =
                descendant != null
                && BreadcrumbVisibleOrder.IndexOf(
                    _model.VisiblePositions(),
                    _model.SelectedIndex,
                    descendant.Identity
                ) >= 0;
            PendingNode = visible ? descendant : null;
        }

        /// <summary>Restores every row whose shape an open-list Ctrl+Left changed (D-Q24).</summary>
        private void RestoreOpenShape()
        {
            foreach (var (row, active, collapsed) in _openShape)
            {
                if (row.ActiveSegmentIndex == active && row.CollapsedAfterIndex == collapsed)
                {
                    continue;
                }
                // Reset is the only way back to the leaf-anchored state; the expansion it drops was
                // already closed by the segment activation that changed the shape.
                row.Reset();
                if (active.HasValue && active.Value < row.Chain.Count - 1)
                {
                    row.ActivateSegment(active.Value);
                }
                if (collapsed.HasValue)
                {
                    row.CollapseAfter(collapsed.Value);
                }
            }
        }

        private void EndOpenTree()
        {
            PendingNode = null;
            _openShape.Clear();
        }

        /// <summary>
        /// Commits the pending node as the model's descendant selection (AC4).
        /// </summary>
        /// <returns>True when the committed descendant changed.</returns>
        private bool CommitPendingNode(int rowIndex)
        {
            var node = PendingNode!;
            var root = _model.Rows[rowIndex].DescendantRoot;
            if (
                root?.Find(node.Identity) != node
                || !BreadcrumbSelectionMap.TryDescendantValue(_model, node, out _)
            )
            {
                return false;
            }
            if (
                _model.SelectedIndex == rowIndex
                && ReferenceEquals(_model.SelectedDescendant, node)
            )
            {
                return false;
            }
            int legacyIndex = -1;
            for (int index = 0; index < root.Children.Count; index++)
            {
                legacyIndex = ReferenceEquals(root.Children[index], node) ? index : legacyIndex;
            }
            _model.SelectDescendant(rowIndex, node, legacyIndex);
            return true;
        }

        /// <summary>True when the committed descendant exists but is not in the visible order.</summary>
        private bool IsCommittedDescendantHidden()
        {
            var descendant = _model.SelectedDescendant;
            return descendant != null
                && BreadcrumbVisibleOrder.IndexOf(
                    _model.VisiblePositions(),
                    _model.SelectedIndex,
                    descendant.Identity
                ) < 0;
        }

        private static bool FindPath(
            BreadcrumbDescendantNode current,
            BreadcrumbDescendantNode target,
            List<BreadcrumbDescendantNode> path
        )
        {
            foreach (BreadcrumbDescendantNode child in current.Children)
            {
                path.Add(child);
                if (child == target || FindPath(child, target, path))
                {
                    return true;
                }
                path.RemoveAt(path.Count - 1);
            }
            return false;
        }
    }
}
