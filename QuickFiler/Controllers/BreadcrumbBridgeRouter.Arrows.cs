#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Arrow-key and leaf-expansion members of the breadcrumb bridge router, relocated from
    /// <c>BreadcrumbBridgeRouter.cs</c> so that neither part of the type exceeds the 500-line
    /// file limit. Right and Left follow the #1007 decision table over the visible order: Right
    /// expands an entry with children or moves to its first child; Left collapses an expanded
    /// entry and never selects a parent (the #440 walk and the Left segment hiding are removed).
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouter
    {
        private async Task HandleLeafToggleAsync(BreadcrumbRow row)
        {
            // R3: only a collapse that hides the active segment is restored; a row shortened at
            // its active segment expands that segment instead (AC9, D-Q23).
            if (row.IsCollapsed && !row.IsAnchoredCollapse)
            {
                if (row.ReExpand())
                {
                    PostRowRender(row);
                }

                return;
            }

            if (row.IsLeafExpanded)
            {
                CollapseRowLeaf(row);
                return;
            }

            await ExpandLeafAsync(row);
        }

        private async Task HandleArrowKeyAsync(BreadcrumbRow row, string key, int? pageSize)
        {
            switch (key)
            {
                case "Right":
                    await HandleRightAsync(row);
                    break;
                case "Left":
                    HandleLeft(row);
                    break;
                case "ParentLeft":
                    HandleParentLeft(row);
                    break;
                default:
                    // #1007: Up, Down, Home, End and the page keys walk the visible order; the
                    // Navigation partial logs and ignores every other key.
                    HandleVerticalKey(row, key, pageSize);
                    break;
            }
        }

        /// <summary>
        /// Right (AC5, AC6): an entry with children that is not expanded is expanded; an expanded
        /// entry moves to its first child; an entry without children, or a node whose fetch is
        /// still in flight, is left alone.
        /// </summary>
        private async Task HandleRightAsync(BreadcrumbRow row)
        {
            if (row.IsCollapsed && !row.IsAnchoredCollapse)
            {
                if (row.ReExpand())
                {
                    PostRowRender(row);
                }

                return;
            }

            List<BreadcrumbVisiblePosition> order = VisibleOrder();
            int index = PositionOf(order, row);
            if (index < 0 || !order[index].HasChildren)
            {
                return;
            }

            BreadcrumbVisiblePosition position = order[index];
            if (!position.IsExpanded)
            {
                if (position.IsRowEntry)
                {
                    await ExpandLeafAsync(row);
                }
                else
                {
                    await HandleDescendantToggleAsync(row, position.NodeId!);
                }

                return;
            }

            // An expanded entry's first child directly follows it in pre-order at a greater
            // depth; a node whose fetch is in flight has no child there yet.
            int next = index + 1;
            if (next < order.Count && order[next].Depth > position.Depth)
            {
                CommitPosition(order[next]);
            }
        }

        /// <summary>Left (AC7): collapses an expanded entry; any other entry is a no-op.</summary>
        private void HandleLeft(BreadcrumbRow row)
        {
            List<BreadcrumbVisiblePosition> order = VisibleOrder();
            int index = PositionOf(order, row);
            if (index < 0 || !order[index].IsExpanded)
            {
                return;
            }

            if (order[index].IsRowEntry)
            {
                CollapseRowLeaf(row);
            }
            else
            {
                CollapseNode(row, order[index].NodeId!);
            }
        }

        /// <summary>
        /// Collapses the row's expansion. When that hides the selected subfolder of the selected
        /// row, the row entry is re-committed (AC7, S1).
        /// </summary>
        private void CollapseRowLeaf(BreadcrumbRow row)
        {
            bool hidesSelection = row.RowId == _selectedRowId && row.SelectedDescendantId != null;
            // Callers collapse only an expanded row, so the toggle closes it; the guard keeps a
            // refused toggle a silent no-op.
            if (row.ToggleLeafExpanded())
            {
                PostRowRender(row);
                if (hidesSelection)
                {
                    SelectRowEntry(row);
                }
            }
        }

        private async Task ExpandLeafAsync(BreadcrumbRow row)
        {
            BreadcrumbSegment? activeSegment = row.ActiveSegment;
            if (row.Kind != BreadcrumbRowKind.Suggestion || activeSegment?.HasSubfolders != true)
            {
                return; // Active segment without subfolders (or non-suggestion row): no-op.
            }

            // S7: a second Right (key auto-repeat) while the fetch is in flight issues no query.
            if (!row.TryBeginLeafFetch())
            {
                log.Debug(
                    $"Breadcrumb expand for row '{row.RowId}' ignored: a fetch is in flight."
                );
                return;
            }

            string requestId = "req-" + (++_requestSequence);
            try
            {
                FolderTreeNodeKey? key = row.ActiveSegmentKey;
                if (key == null)
                {
                    log.Error(
                        $"Breadcrumb expand {requestId}: no provider key for '{activeSegment.FullPath}'; row '{row.RowId}' left unchanged."
                    );
                    return;
                }

                IReadOnlyList<FolderBreadcrumbSegment> children =
                    await _provider.GetImmediateSubfoldersAsync(key, CancellationToken.None);

                // #1002 I2: row ids are positional and reused across binds, so a completion that
                // arrives after a rebind must not mutate or render the replaced row.
                if (!ReferenceEquals(FindRow(row.RowId), row))
                {
                    log.Error(
                        $"Breadcrumb expand {requestId} completed after row '{row.RowId}' was replaced; result discarded."
                    );
                    return;
                }

                IReadOnlyList<BreadcrumbSegment> mapped = BreadcrumbRowBuilder.MapSegments(
                    children
                );
                row.SetLeafDescendants(children ?? Array.Empty<FolderBreadcrumbSegment>());
                row.ExpandLeaf(); // R4: expand-to-true, never a toggle.
                PostOutbound(new BreadcrumbSubfolderResultMessage(requestId, row.RowId, mapped));
                PostRowRender(row);
            }
            catch (OperationCanceledException)
            {
                log.Error(
                    $"Breadcrumb expand {requestId} canceled for row '{row.RowId}'; state unchanged."
                );
            }
            catch (Exception ex)
            {
                // Provider I/O boundary: log the specific failure and leave row state unchanged.
                log.Error(
                    $"Breadcrumb expand {requestId} failed for row '{row.RowId}': {ex.Message}",
                    ex
                );
            }
            finally
            {
                row.EndLeafFetch();
            }
        }
    }
}
