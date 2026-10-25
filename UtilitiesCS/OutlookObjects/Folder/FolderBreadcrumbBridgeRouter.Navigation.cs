#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>How the host acts on a Left or Right arrow while the selector list is open (#1007).</summary>
    public enum BreadcrumbPendingArrowAction
    {
        /// <summary>The router handles the arrow on the pending entry.</summary>
        Route,

        /// <summary>Left on a non-expanded pending entry: commit the pending entry and close.</summary>
        CommitAndClose,

        /// <summary>Right on a pending entry without children: cancel, then pop out.</summary>
        CancelAndPopOut,
    }

    /// <summary>
    /// Issue #1007 selector navigation over the visible order. While the list is open, Left and
    /// Right act on the pending entry (spec decision table), never on the committed row.
    /// </summary>
    public sealed partial class FolderBreadcrumbBridgeRouter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>Moves the open-list pending highlight to the first or last visible entry.</summary>
        public BreadcrumbSelectionTransition MoveSelectorToBoundary(bool first) =>
            Mutate(() => PendingOnly(_selectionSession.MoveToBoundary(first)));

        /// <summary>Moves the open-list pending highlight by a page of visible entries.</summary>
        public BreadcrumbSelectionTransition MoveSelectorPage(bool previous, int pageSize) =>
            Mutate(() => PendingOnly(_selectionSession.MovePage(previous, pageSize)));

        /// <summary>
        /// Moves to the parent entry (Ctrl+Left): pending only while open, committed immediately
        /// while closed (D-Q19). The render reflects a shortened row.
        /// </summary>
        public BreadcrumbSelectionTransition MoveSelectorToParent() =>
            Mutate(() =>
            {
                bool wasOpen = _selectionSession.IsOpen;
                if (!_selectionSession.MoveToParent())
                {
                    return BreadcrumbSelectionEffects.None;
                }
                return BreadcrumbSelectionEffects.Handled
                    | BreadcrumbSelectionEffects.RenderRequired
                    | (
                        wasOpen
                            ? BreadcrumbSelectionEffects.None
                            : BreadcrumbSelectionEffects.SelectionChanged
                    );
            });

        /// <summary>Commits the first or last top-level row of a closed selector (D-Q16).</summary>
        public BreadcrumbSelectionTransition CommitSelectorBoundaryRow(bool first) =>
            Mutate(() =>
                _selectionSession.CommitBoundaryRow(first)
                    ? BreadcrumbSelectionEffects.Handled
                        | BreadcrumbSelectionEffects.SelectionChanged
                        | BreadcrumbSelectionEffects.RenderRequired
                    : BreadcrumbSelectionEffects.None
            );

        /// <summary>
        /// Activates, commits and shortens a row at a segment, closing an open selector (AC9, S5).
        /// </summary>
        /// <param name="rowIdentity">The stable identity of a selectable suggestion row.</param>
        /// <param name="segmentIndex">A non-leaf segment index of that row.</param>
        /// <returns>An unhandled transition for an unknown row or an out-of-range index.</returns>
        public BreadcrumbSelectionTransition DoubleClickSelectorSegment(
            string rowIdentity,
            int segmentIndex
        ) => Mutate(() => _selectionSession.DoubleClickSegment(rowIdentity, segmentIndex));

        /// <summary>Moves only the open-list pending highlight to a row (D-Q27); closed is a no-op.</summary>
        public BreadcrumbSelectionTransition HighlightSelector(string identity) =>
            Mutate(() => _selectionSession.HighlightIdentity(identity));

        /// <summary>
        /// The bridge-channel <c>segmentDoubleClick</c> is a logged no-op (S5): the QFC double-click
        /// travels on the selector channel so it can commit and close, and a stale page must not
        /// shorten a row without committing it.
        /// </summary>
        private static IReadOnlyList<string> IgnoreBridgeSegmentDoubleClick(
            SegmentDoubleClickMessage message
        )
        {
            log.Debug(
                $"Ignored bridge segmentDoubleClick for row {message.RowIndex}, segment {message.SegmentIndex}; "
                    + "the selector channel carries segment double-clicks (#1007 S5)."
            );
            return Array.Empty<string>();
        }

        /// <summary>
        /// Classifies an arrow against the pending entry of an open selector (AC4, AC6). A closed
        /// selector always routes. Left on a non-expanded entry commits and closes (D-Q15); Right
        /// on an entry without children cancels and pops out (D-Q21); every other arrow routes.
        /// </summary>
        public BreadcrumbPendingArrowAction ClassifyPendingArrow(
            BreadcrumbArrowDirection direction
        ) =>
            Read(() =>
            {
                BreadcrumbVisiblePosition? entry = PendingEntryCore(out _);
                if (entry == null)
                {
                    return BreadcrumbPendingArrowAction.Route;
                }
                if (direction == BreadcrumbArrowDirection.Left)
                {
                    return entry.IsExpanded
                        ? BreadcrumbPendingArrowAction.Route
                        : BreadcrumbPendingArrowAction.CommitAndClose;
                }
                return entry.HasChildren
                    ? BreadcrumbPendingArrowAction.Route
                    : BreadcrumbPendingArrowAction.CancelAndPopOut;
            });

        /// <summary>
        /// Open-list Right and Left on the pending entry. Right expands an entry with children or
        /// pends its first child once expanded (AC5); Left collapses an expanded entry and keeps
        /// the pending highlight (AC7). Anything else is reported as an unhandled arrow.
        /// </summary>
        private async Task<IReadOnlyList<string>> PendingArrowAsync(
            BreadcrumbArrowDirection direction,
            CancellationToken cancellationToken
        )
        {
            BreadcrumbStateRow row;
            BreadcrumbDescendantNode? node;
            int rowIndex;
            lock (_sync)
            {
                BreadcrumbVisiblePosition? entry = PendingEntryCore(out var order);
                if (entry == null)
                {
                    return Unhandled(direction);
                }
                rowIndex = entry.RowIndex;
                row = _model.Rows[rowIndex];
                node = entry.NodeId == null ? null : row.DescendantRoot?.Find(entry.NodeId);
                if (direction == BreadcrumbArrowDirection.Left)
                {
                    return CollapsePendingCore(row, node)
                        ? new[] { RenderJsonCore() }
                        : Unhandled(direction);
                }
                if (entry.IsExpanded)
                {
                    int next = order.IndexOf(entry) + 1;
                    bool hasChild =
                        next < order.Count
                        && order[next].RowIndex == rowIndex
                        && order[next].Depth == entry.Depth + 1;
                    if (hasChild)
                    {
                        _selectionSession.Pend(order[next]);
                        return Array.Empty<string>();
                    }
                    // An expanded entry whose fetch is still in flight does nothing (decision table).
                    var expanded = node ?? row.DescendantRoot;
                    return expanded != null && !expanded.Loaded
                        ? Array.Empty<string>()
                        : Unhandled(direction);
                }
                if (node == null)
                {
                    if (!BreadcrumbStateModel.Expand(row))
                    {
                        return Unhandled(direction);
                    }
                    if (!row.LeafExpanded || row.Subfolders.Count != 0)
                    {
                        return new[] { RenderJsonCore() };
                    }
                }
                else if (!BreadcrumbVisibleOrder.NodeHasChildren(node))
                {
                    return Unhandled(direction);
                }
            }
            if (node != null)
            {
                return await ToggleDescendantAsync(row.Identity, node.Identity, cancellationToken)
                    .ConfigureAwait(false);
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>Collapses the pending entry when expanded; a hidden pending node moves up (AC7).</summary>
        private bool CollapsePendingCore(BreadcrumbStateRow row, BreadcrumbDescendantNode? node)
        {
            bool collapsed = node == null ? row.TryCollapseLeaf() : node.Expanded;
            if (node != null && collapsed)
            {
                node.Collapse();
            }
            _selectionSession.ReconcilePendingNode();
            return collapsed;
        }

        /// <summary>The pending entry of an open selector, or null when closed or unresolved.</summary>
        private BreadcrumbVisiblePosition? PendingEntryCore(
            out List<BreadcrumbVisiblePosition> order
        )
        {
            order = _model.VisiblePositions();
            if (!_selectionSession.IsOpen)
            {
                return null;
            }
            int index = _selectionSession.PendingPosition(order);
            return index < 0 ? null : order[index];
        }

        private static BreadcrumbSelectionEffects PendingOnly(bool moved) =>
            moved ? BreadcrumbSelectionEffects.Handled : BreadcrumbSelectionEffects.None;

        private static IReadOnlyList<string> Unhandled(BreadcrumbArrowDirection direction) =>
            new[] { BreadcrumbBridgeSerializer.Serialize(new UnhandledArrowMessage(direction)) };
    }
}
