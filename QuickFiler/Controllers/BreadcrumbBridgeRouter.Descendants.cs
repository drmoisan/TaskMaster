#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Descendant toggle and activation handlers of the breadcrumb bridge router (#1002). Every
    /// request is addressed by a host-issued node identity that must belong to the addressed
    /// row's current tree (I1); a fetch result is applied only to the same row instance and the
    /// same request generation that started it (I2); a toggle changes expansion only, unless the
    /// collapse hides the selected node (I3 as narrowed by #1007 S1);
    /// and an activation commits only through <c>SelectHierarchyPath</c> (I4). Awaits resume on
    /// the captured UI context, so no lock is taken and the provider await keeps the default
    /// context capture (I6).
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouter
    {
        private async Task HandleDescendantToggleAsync(BreadcrumbRow row, string nodeId)
        {
            if (CollapseNode(row, nodeId))
            {
                return;
            }

            if (
                !row.TryBeginDescendantExpand(
                    nodeId,
                    out FolderTreeNodeKey? key,
                    out int generation
                )
            )
            {
                log.Error(
                    $"Breadcrumb descendant toggle rejected for row '{row.RowId}': node '{nodeId}' is unknown or has no children."
                );
                return;
            }

            IReadOnlyList<FolderBreadcrumbSegment> children;
            try
            {
                // Null-forgiving: TryBeginDescendantExpand returns true only with the node's key,
                // and net48 has no NotNullWhen attribute to carry that guarantee.
                children = await _provider.GetImmediateSubfoldersAsync(
                    key!,
                    CancellationToken.None
                );
            }
            catch (OperationCanceledException)
            {
                log.Error(
                    $"Breadcrumb descendant fetch canceled for row '{row.RowId}' node '{nodeId}'; state unchanged."
                );
                row.TryAbandonDescendantExpand(nodeId, generation);
                return;
            }
            catch (Exception ex)
            {
                // Provider I/O boundary: log the specific failure and permit a later retry.
                log.Error(
                    $"Breadcrumb descendant fetch failed for row '{row.RowId}' node '{nodeId}': {ex.Message}",
                    ex
                );
                row.TryAbandonDescendantExpand(nodeId, generation);
                return;
            }

            if (
                !ReferenceEquals(FindRow(row.RowId), row)
                || !row.TryAttachDescendantChildren(
                    nodeId,
                    generation,
                    children ?? Array.Empty<FolderBreadcrumbSegment>()
                )
            )
            {
                log.Error(
                    $"Breadcrumb descendant fetch for row '{row.RowId}' node '{nodeId}' was superseded; result discarded."
                );
                return;
            }

            PostRowRender(row);
        }

        /// <summary>
        /// Collapses a known expanded node. I3 is narrowed by #1007 S1: when the collapse hides
        /// the selected descendant of the selected row, the collapsed node (its nearest visible
        /// ancestor) is re-committed through <c>SelectHierarchyPath</c>.
        /// </summary>
        /// <returns>True when the node was collapsed.</returns>
        private bool CollapseNode(BreadcrumbRow row, string nodeId)
        {
            bool hidesSelection =
                row.RowId == _selectedRowId && row.IsSelectedDescendantWithin(nodeId);
            if (!row.TryCollapseDescendant(nodeId))
            {
                return false;
            }

            PostRowRender(row);
            string? path = row.GetDescendantPath(nodeId);
            if (hidesSelection && path != null)
            {
                SelectHierarchyPath(row, path, nodeId);
            }

            return true;
        }

        private void ActivateDescendant(BreadcrumbRow row, string nodeId)
        {
            string? path = row.GetDescendantPath(nodeId);
            if (path == null)
            {
                log.Error(
                    $"Breadcrumb descendant activation rejected for row '{row.RowId}': node '{nodeId}' is unknown."
                );
                return;
            }

            SelectHierarchyPath(row, path, nodeId);
        }
    }
}
