#nullable enable
using System;
using System.Collections.Generic;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Issue #1007 keyboard navigation over the visible order: the top-level rows plus every shown
    /// descendant, in pre-order. The position is the existing pair of the addressed row and, when
    /// that row is selected, its selected descendant (no new state). A move onto a node commits
    /// through <c>SelectHierarchyPath</c> and a move onto a row through <c>SelectRowEntry</c>, so
    /// every keyboard selection obeys the archive-root contract (#614). All work runs on the UI
    /// context; nothing here awaits or locks (I6).
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouter
    {
        /// <summary>Builds the current visible order from every row.</summary>
        private List<BreadcrumbVisiblePosition> VisibleOrder()
        {
            var order = new List<BreadcrumbVisiblePosition>();
            for (int index = 0; index < _rows.Count; index++)
            {
                _rows[index].AppendVisiblePositions(index, order);
            }
            return order;
        }

        /// <summary>
        /// The position of the addressed row (R2): its selected descendant when the row is the
        /// selected row and the descendant is visible, otherwise its row entry; -1 when absent.
        /// </summary>
        private int PositionOf(IReadOnlyList<BreadcrumbVisiblePosition> order, BreadcrumbRow row)
        {
            int rowIndex = IndexOf(row);
            if (row.RowId == _selectedRowId && row.SelectedDescendantId != null)
            {
                int node = BreadcrumbVisibleOrder.IndexOf(
                    order,
                    rowIndex,
                    row.SelectedDescendantId
                );
                if (node >= 0)
                {
                    return node;
                }
            }
            return BreadcrumbVisibleOrder.IndexOf(order, rowIndex, null);
        }

        /// <summary>
        /// Up, Down, Home, End, PageUp and PageDown (AC1, AC2, AC11). Up with no previous entry
        /// hands focus to the search box; Home never does. A page key needs a positive page size.
        /// </summary>
        private void HandleVerticalKey(BreadcrumbRow row, string key, int? pageSize)
        {
            List<BreadcrumbVisiblePosition> order = VisibleOrder();
            int current = PositionOf(order, row);
            int target;
            switch (key)
            {
                case "Up":
                    target = BreadcrumbVisibleOrder.Step(order, current, -1);
                    if (target < 0)
                    {
                        // Up at the first entry: hand focus back to the search box.
                        PostOutbound(new BreadcrumbFocusSearchMessage());
                        FocusSearchRequested?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    break;
                case "Down":
                    target = BreadcrumbVisibleOrder.Step(order, current, 1);
                    break;
                case "Home":
                    target = BreadcrumbVisibleOrder.First(order);
                    break;
                case "End":
                    target = BreadcrumbVisibleOrder.Last(order);
                    break;
                default:
                    if (!TryPageDelta(key, pageSize, row.RowId, out int delta))
                    {
                        return;
                    }
                    target = BreadcrumbVisibleOrder.Step(order, current, delta);
                    break;
            }

            if (target >= 0 && target != current)
            {
                CommitPosition(order[target]);
            }
        }

        /// <summary>
        /// D-Q26: with nothing selected (an empty row id), Down and Home select the first entry
        /// and Up and End select the last; every other key is a logged no-op.
        /// </summary>
        private void HandleNoSelectionKey(string key)
        {
            List<BreadcrumbVisiblePosition> order = VisibleOrder();
            int target;
            switch (key)
            {
                case "Down":
                case "Home":
                    target = BreadcrumbVisibleOrder.First(order);
                    break;
                case "Up":
                case "End":
                    target = BreadcrumbVisibleOrder.Last(order);
                    break;
                default:
                    log.Debug($"Breadcrumb key '{key}' with no selection is ignored.");
                    return;
            }

            if (target >= 0)
            {
                CommitPosition(order[target]);
            }
        }

        // S3: a page key carries the measured fully visible row count; a missing or non-positive
        // value is rejected explicitly rather than replaced by a guess.
        private static bool TryPageDelta(string key, int? pageSize, string rowId, out int delta)
        {
            delta = 0;
            if (key != "PageUp" && key != "PageDown")
            {
                log.Error($"Unknown breadcrumb arrow key '{key}' for row '{rowId}'.");
                return false;
            }
            if (!pageSize.HasValue || pageSize.Value < 1)
            {
                log.Error(
                    $"Breadcrumb {key} for row '{rowId}' ignored: invalid page size '{pageSize}'."
                );
                return false;
            }
            delta = key == "PageUp" ? -pageSize.Value : pageSize.Value;
            return true;
        }

        /// <summary>
        /// Ctrl+Left (AC8): on a node, selects the parent entry and keeps its expansion; on a row
        /// entry, performs the parent-segment double-click; with the first segment active (one
        /// level below the archive root), does nothing.
        /// </summary>
        private void HandleParentLeft(BreadcrumbRow row)
        {
            List<BreadcrumbVisiblePosition> order = VisibleOrder();
            int index = PositionOf(order, row);
            if (index < 0)
            {
                return;
            }

            if (!order[index].IsRowEntry)
            {
                int parent = BreadcrumbVisibleOrder.ParentOf(order, index);
                if (parent >= 0)
                {
                    CommitPosition(order[parent]);
                }

                return;
            }

            int? active = row.ActiveSegmentIndex;
            if (active.HasValue && active.Value > 0)
            {
                ShortenAt(row, active.Value - 1);
            }
        }

        /// <summary>
        /// Segment double-click (AC9, S10), atomic: activate the segment when it is not active,
        /// commit it, then shorten the row there with one row render.
        /// </summary>
        private void HandleSegmentDoubleClick(BreadcrumbRow row, int? requestedIndex)
        {
            // #498: the bridge is an untrusted boundary, so the segment index is validated
            // here rather than relying on BreadcrumbRow.CollapseAfter to throw. An index
            // that escaped this arm reached the async void host-event seam as an unhandled
            // exception, which the single catch (BreadcrumbMessageException) cannot contain.
            if (
                !requestedIndex.HasValue
                || requestedIndex.Value < 0
                || requestedIndex.Value >= row.Segments.Count
            )
            {
                log.Error(
                    $"Inbound segmentDoubleClick for row '{row.RowId}' carries segment index "
                        + $"'{requestedIndex}', which is outside the valid range "
                        + $"[0, {row.Segments.Count - 1}]; rejected without a transition."
                );
                return;
            }

            if (row.Kind != BreadcrumbRowKind.Suggestion)
            {
                return; // Banner and pseudo rows never shorten and post nothing.
            }

            ShortenAt(row, requestedIndex.Value);
        }

        // Activates (when needed), commits and shortens at a non-leaf segment. A segment that
        // cannot be activated (no provider key) keeps the pre-#1007 collapse-only behavior.
        private void ShortenAt(BreadcrumbRow row, int segmentIndex)
        {
            if (segmentIndex >= row.Segments.Count - 1)
            {
                return; // The leaf is never shortened.
            }

            if (row.ActiveSegmentIndex == segmentIndex || row.ActivateSegment(segmentIndex))
            {
                SelectHierarchyPath(row, row.Segments[segmentIndex].FullPath);
            }
            else
            {
                log.Debug(
                    $"Breadcrumb segment {segmentIndex} of row '{row.RowId}' has no key; shortened without a commit."
                );
            }

            if (row.CollapseAfter(segmentIndex))
            {
                PostRowRender(row);
            }
        }

        /// <summary>Commits a visible entry as a click does (AC3).</summary>
        private void CommitPosition(BreadcrumbVisiblePosition position)
        {
            BreadcrumbRow row = _rows[position.RowIndex];
            if (position.NodeId == null)
            {
                SelectRowEntry(row);
                return;
            }

            string? path = row.GetDescendantPath(position.NodeId);
            if (path != null)
            {
                SelectHierarchyPath(row, path, position.NodeId);
            }
        }
    }
}
