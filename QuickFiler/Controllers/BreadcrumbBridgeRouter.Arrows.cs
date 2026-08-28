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
    /// file limit. Behavior is unchanged by the relocation.
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouter
    {
        private async Task HandleLeafToggleAsync(BreadcrumbRow row)
        {
            if (row.IsCollapsed)
            {
                if (row.ReExpand())
                {
                    PostRowRender(row);
                }

                return;
            }

            if (row.IsLeafExpanded)
            {
                if (row.ToggleLeafExpanded())
                {
                    PostRowRender(row);
                }

                return;
            }

            await ExpandLeafAsync(row);
        }

        private async Task HandleArrowKeyAsync(BreadcrumbRow row, string key)
        {
            switch (key)
            {
                case "Right":
                    // #440: attempt the tree transition first. It is available only on a row
                    // whose ACTIVE segment is an attached non-leaf with subfolders; otherwise the
                    // decision-D1 fall-through to the pre-existing behavior runs unchanged.
                    if (await TryRightTreeTransitionAsync(row))
                    {
                        break;
                    }

                    if (row.IsCollapsed)
                    {
                        if (row.ReExpand())
                        {
                            PostRowRender(row);
                        }
                    }
                    else if (!row.IsLeafExpanded)
                    {
                        await ExpandLeafAsync(row);
                    }

                    break;
                case "Left":
                    // #440: attempt the tree transition first — move the active segment exactly
                    // one step toward the root. ActivateSegment refuses at the root, on a
                    // non-suggestion row, and where no key is attached, and the decision-D1
                    // fall-through to the pre-existing collapse behavior then runs unchanged.
                    if (
                        row.ActiveSegmentIndex.HasValue
                        && row.ActivateSegment(row.ActiveSegmentIndex.Value - 1)
                    )
                    {
                        PostRowRender(row);
                        break;
                    }

                    if (row.LeftArrow())
                    {
                        PostRowRender(row);
                    }

                    break;
                case "Up":
                    HandleUpArrow(row);
                    break;
                case "Down":
                    MoveSelection(row, step: 1);
                    break;
                default:
                    log.Error($"Unknown breadcrumb arrow key '{key}' for row '{row.RowId}'.");
                    break;
            }
        }

        /// <summary>
        /// #440 Right tree transition. Expands the active non-leaf segment when it is not yet
        /// expanded, clearing any collapse as PART of the transition because
        /// <see cref="BreadcrumbRow.ToggleLeafExpanded"/> is a no-op while the row is collapsed;
        /// once expanded, descends by activating child index 0 (decision D9). Returns false when
        /// no transition is available, in which case the caller runs the pre-existing behavior.
        /// </summary>
        private async Task<bool> TryRightTreeTransitionAsync(BreadcrumbRow row)
        {
            int? activeIndex = row.ActiveSegmentIndex;
            if (
                row.Kind != BreadcrumbRowKind.Suggestion
                || !activeIndex.HasValue
                || activeIndex.Value >= row.Segments.Count - 1
                || row.ActiveSegmentKey == null
                || row.ActiveSegment?.HasSubfolders != true
            )
            {
                return false;
            }

            if (!row.IsLeafExpanded)
            {
                if (row.ReExpand())
                {
                    PostRowRender(row);
                }

                await ExpandLeafAsync(row);
                return true;
            }

            BreadcrumbSegment? child = row.GetActiveChild(0);
            if (child == null)
            {
                return false; // Nothing to descend into: decision-D1 fall-through.
            }

            SelectHierarchyPath(row, child.FullPath);
            return true;
        }

        private void HandleUpArrow(BreadcrumbRow row)
        {
            BreadcrumbRow? previous = FindSelectable(IndexOf(row) - 1, step: -1);
            if (previous == null)
            {
                // Up at the top row: hand focus back to the search box.
                PostOutbound(new BreadcrumbFocusSearchMessage());
                FocusSearchRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            SelectRow(previous);
        }

        private void MoveSelection(BreadcrumbRow row, int step)
        {
            BreadcrumbRow? next = FindSelectable(IndexOf(row) + step, step);
            if (next != null)
            {
                SelectRow(next);
            }
        }

        private async Task ExpandLeafAsync(BreadcrumbRow row)
        {
            BreadcrumbSegment? activeSegment = row.ActiveSegment;
            if (row.Kind != BreadcrumbRowKind.Suggestion || activeSegment?.HasSubfolders != true)
            {
                return; // Active segment without subfolders (or non-suggestion row): no-op.
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
                IReadOnlyList<BreadcrumbSegment> mapped = BreadcrumbRowBuilder.MapSegments(
                    children
                );
                row.SetLeafChildren(mapped);
                row.ToggleLeafExpanded();
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
        }
    }
}
