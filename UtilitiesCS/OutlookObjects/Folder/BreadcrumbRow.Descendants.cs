#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Keyed descendant-tree members of <see cref="BreadcrumbRow"/> (#1002). The first-level
    /// children of the expanded active segment keep their provider keys as
    /// <see cref="BreadcrumbDescendantNode"/> instances, so deeper levels can be fetched and
    /// selected by a host-issued node identity. Every transition validates that the identity
    /// belongs to this row's current tree, and a fetch result is accepted only for the request
    /// generation that started it.
    /// </summary>
    public sealed partial class BreadcrumbRow
    {
        private List<BreadcrumbDescendantNode> _descendants = new List<BreadcrumbDescendantNode>();

        /// <summary>Identity of the selected descendant node, or null when none is selected.</summary>
        public string? SelectedDescendantId { get; private set; }

        /// <summary>
        /// First-level keyed descendant nodes of the expanded active segment; empty when the
        /// children were stored key-less through <see cref="SetLeafChildren"/>.
        /// </summary>
        internal IReadOnlyList<BreadcrumbDescendantNode> DescendantNodes => _descendants;

        /// <summary>
        /// Stores keyed first-level children: the key-less projection used by the existing
        /// first-level contract plus one descendant node per child. Valid only on a suggestion row
        /// whose active segment has subfolders; otherwise a no-op.
        /// </summary>
        /// <param name="children">Immediate keyed children of the active segment.</param>
        /// <returns>True when the children were stored.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="children"/> is null.</exception>
        public bool SetLeafDescendants(IReadOnlyList<FolderBreadcrumbSegment> children)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            if (!CanExpandActiveSegment())
            {
                return false;
            }

            _leafChildren = new List<BreadcrumbSegment>(BreadcrumbRowBuilder.MapSegments(children));
            LeafChildrenLoaded = true;
            var nodes = new List<BreadcrumbDescendantNode>(children.Count);
            foreach (FolderBreadcrumbSegment child in children)
            {
                nodes.Add(new BreadcrumbDescendantNode(child));
            }

            _descendants = nodes;
            SelectedDescendantId = null;
            return true;
        }

        /// <summary>
        /// Starts expanding a known, collapsed descendant that has children: marks it expanded and
        /// advances its request generation.
        /// </summary>
        /// <param name="nodeId">Host-issued node identity.</param>
        /// <param name="key">The node's provider key when the expansion began; otherwise null.</param>
        /// <param name="generation">The request generation to present when attaching.</param>
        /// <returns>True when the expansion began; false for an unknown, leaf, or expanded node.</returns>
        public bool TryBeginDescendantExpand(
            string nodeId,
            out FolderTreeNodeKey? key,
            out int generation
        )
        {
            key = null;
            generation = 0;
            BreadcrumbDescendantNode? node = FindDescendant(nodeId);
            if (node == null || !node.Segment.HasChildren || node.Expanded)
            {
                return false;
            }

            node.Expanded = true;
            node.RequestGeneration++;
            key = node.Segment.Key;
            generation = node.RequestGeneration;
            return true;
        }

        /// <summary>Collapses an expanded descendant and its expanded subtree.</summary>
        /// <param name="nodeId">Host-issued node identity.</param>
        /// <returns>True when a known expanded node was collapsed.</returns>
        public bool TryCollapseDescendant(string nodeId)
        {
            BreadcrumbDescendantNode? node = FindDescendant(nodeId);
            if (node == null || !node.Expanded)
            {
                return false;
            }

            node.Collapse();
            return true;
        }

        /// <summary>
        /// Attaches fetched children to a descendant only when the node is still in this row's
        /// tree, still expanded, and its request generation still equals
        /// <paramref name="generation"/>.
        /// </summary>
        /// <param name="nodeId">Host-issued node identity.</param>
        /// <param name="generation">Generation returned by <see cref="TryBeginDescendantExpand"/>.</param>
        /// <param name="children">The fetched immediate children.</param>
        /// <returns>True when the children were attached; false for a stale completion.</returns>
        public bool TryAttachDescendantChildren(
            string nodeId,
            int generation,
            IReadOnlyList<FolderBreadcrumbSegment> children
        )
        {
            BreadcrumbDescendantNode? node = FindDescendant(nodeId);
            if (node == null || !node.Expanded || node.RequestGeneration != generation)
            {
                return false;
            }

            node.Attach(children);
            return true;
        }

        /// <summary>
        /// Abandons a failed or canceled expansion: collapses the node only when
        /// <paramref name="generation"/> is still its current request, so a later retry begins
        /// from a collapsed node.
        /// </summary>
        /// <param name="nodeId">Host-issued node identity.</param>
        /// <param name="generation">Generation returned by <see cref="TryBeginDescendantExpand"/>.</param>
        /// <returns>True when the expansion was abandoned.</returns>
        public bool TryAbandonDescendantExpand(string nodeId, int generation)
        {
            BreadcrumbDescendantNode? node = FindDescendant(nodeId);
            if (node == null || node.RequestGeneration != generation)
            {
                return false;
            }

            node.Collapse();
            return true;
        }

        /// <summary>Returns the folder path of a descendant, or null for an unknown identity.</summary>
        /// <param name="nodeId">Host-issued node identity.</param>
        public string? GetDescendantPath(string nodeId) =>
            FindDescendant(nodeId)?.Segment.FolderPath;

        /// <summary>
        /// Selects a known descendant, or clears the selection when <paramref name="nodeId"/> is
        /// null. An unknown identity leaves <see cref="SelectedDescendantId"/> unchanged.
        /// </summary>
        /// <param name="nodeId">Host-issued node identity, or null to clear.</param>
        /// <returns>True when the selection was set or cleared.</returns>
        public bool TrySelectDescendant(string? nodeId)
        {
            if (nodeId == null)
            {
                SelectedDescendantId = null;
                return true;
            }

            if (FindDescendant(nodeId) == null)
            {
                return false;
            }

            SelectedDescendantId = nodeId;
            return true;
        }

        /// <summary>
        /// Reports whether the selected descendant is a strict descendant of
        /// <paramref name="nodeId"/> (#1007 R5), that is, whether collapsing that node hides the
        /// selection. False for an unknown node or when no descendant is selected.
        /// </summary>
        /// <param name="nodeId">Host-issued node identity.</param>
        public bool IsSelectedDescendantWithin(string nodeId)
        {
            BreadcrumbDescendantNode? node = FindDescendant(nodeId);
            if (
                node == null
                || SelectedDescendantId == null
                || node.Identity == SelectedDescendantId
            )
            {
                return false;
            }

            return node.Find(SelectedDescendantId) != null;
        }

        /// <summary>
        /// Appends this row's visible entries (#1007): nothing for a banner row; otherwise the row
        /// entry, then the shown descendant nodes in pre-order.
        /// </summary>
        /// <param name="rowIndex">The row's index in its list.</param>
        /// <param name="target">The order being built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> is null.</exception>
        public void AppendVisiblePositions(int rowIndex, List<BreadcrumbVisiblePosition> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (Kind == BreadcrumbRowKind.Banner)
            {
                return;
            }

            target.Add(
                new BreadcrumbVisiblePosition(
                    rowIndex,
                    null,
                    null,
                    -1,
                    CanExpandActiveSegment() && !HasNoLoadedChildren,
                    IsLeafExpanded
                )
            );
            if (IsLeafExpanded)
            {
                BreadcrumbVisibleOrder.AppendNodes(rowIndex, _descendants, null, 0, target);
            }
        }

        // Called exactly where the first-level children stop being shown or are replaced.
        private void ClearDescendants()
        {
            _descendants = new List<BreadcrumbDescendantNode>();
            SelectedDescendantId = null;
        }

        // An identity never issued by this row's current tree (including null or empty) finds
        // nothing, because every issued identity is a non-empty GUID string.
        private BreadcrumbDescendantNode? FindDescendant(string nodeId)
        {
            foreach (BreadcrumbDescendantNode node in _descendants)
            {
                BreadcrumbDescendantNode? found = node.Find(nodeId);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
