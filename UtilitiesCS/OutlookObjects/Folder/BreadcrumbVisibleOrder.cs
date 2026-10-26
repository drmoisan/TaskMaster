#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// One entry of the visible order (#1007): a top-level row entry, or a shown descendant node
    /// of a row. Immutable; a plain sealed class because net48 has no <c>init</c> accessor.
    /// </summary>
    public sealed class BreadcrumbVisiblePosition
    {
        /// <summary>Creates a visible position.</summary>
        /// <param name="rowIndex">Index of the owning row in its row list.</param>
        /// <param name="nodeId">Host-issued node identity, or null for a row entry.</param>
        /// <param name="parentNodeId">Identity of the parent node, or null at depth 0 and for a row.</param>
        /// <param name="depth">-1 for a row entry, 0 for a first-level node.</param>
        /// <param name="hasChildren">True when the entry can show children.</param>
        /// <param name="isExpanded">True when the entry's children are shown.</param>
        public BreadcrumbVisiblePosition(
            int rowIndex,
            string? nodeId,
            string? parentNodeId,
            int depth,
            bool hasChildren,
            bool isExpanded
        )
        {
            RowIndex = rowIndex;
            NodeId = nodeId;
            ParentNodeId = parentNodeId;
            Depth = depth;
            HasChildren = hasChildren;
            IsExpanded = isExpanded;
        }

        /// <summary>Index of the owning row.</summary>
        public int RowIndex { get; }

        /// <summary>Node identity, or null for a row entry.</summary>
        public string? NodeId { get; }

        /// <summary>Parent node identity, or null for a first-level node or a row entry.</summary>
        public string? ParentNodeId { get; }

        /// <summary>-1 for a row entry; 0 for a first-level node.</summary>
        public int Depth { get; }

        /// <summary>True when the entry can show children (false after a zero-children fetch).</summary>
        public bool HasChildren { get; }

        /// <summary>True when the entry's children are shown.</summary>
        public bool IsExpanded { get; }

        /// <summary>True for a top-level row entry.</summary>
        public bool IsRowEntry => NodeId == null;
    }

    /// <summary>Pure index operations over a visible order (#1007). No I/O.</summary>
    public static class BreadcrumbVisibleOrder
    {
        /// <summary>Index of the entry for the row and node, or -1.</summary>
        /// <param name="order">The visible order. Required.</param>
        /// <param name="rowIndex">The owning row index.</param>
        /// <param name="nodeId">The node identity, or null for the row entry.</param>
        /// <exception cref="ArgumentNullException"><paramref name="order"/> is null.</exception>
        public static int IndexOf(
            IReadOnlyList<BreadcrumbVisiblePosition> order,
            int rowIndex,
            string? nodeId
        )
        {
            Require(order);
            for (int index = 0; index < order.Count; index++)
            {
                if (
                    order[index].RowIndex == rowIndex
                    && string.Equals(order[index].NodeId, nodeId, StringComparison.Ordinal)
                )
                {
                    return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Index after moving by <paramref name="delta"/>, clamped to the first or last entry with
        /// no wrap; -1 when the result equals <paramref name="index"/> or the index is invalid.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="order"/> is null.</exception>
        public static int Step(IReadOnlyList<BreadcrumbVisiblePosition> order, int index, int delta)
        {
            Require(order);
            if (index < 0 || index >= order.Count)
            {
                return -1;
            }
            long target = Math.Min(Math.Max((long)index + delta, 0L), order.Count - 1L);
            return target == index ? -1 : (int)target;
        }

        /// <summary>Index of the first entry, or -1 for an empty order.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="order"/> is null.</exception>
        public static int First(IReadOnlyList<BreadcrumbVisiblePosition> order) =>
            Require(order).Count == 0 ? -1 : 0;

        /// <summary>Index of the last entry, or -1 for an empty order.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="order"/> is null.</exception>
        public static int Last(IReadOnlyList<BreadcrumbVisiblePosition> order) =>
            Require(order).Count - 1;

        /// <summary>
        /// Index of the parent entry: the parent node for a nested node, the row entry for a
        /// first-level node, and -1 for a row entry or an invalid index.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="order"/> is null.</exception>
        public static int ParentOf(IReadOnlyList<BreadcrumbVisiblePosition> order, int index)
        {
            Require(order);
            if (index < 0 || index >= order.Count || order[index].IsRowEntry)
            {
                return -1;
            }
            return IndexOf(order, order[index].RowIndex, order[index].ParentNodeId);
        }

        /// <summary>
        /// Appends shown descendant nodes in pre-order; a node's children are appended only while
        /// it is expanded. Shared by the EFC row and the QFC model.
        /// </summary>
        internal static void AppendNodes(
            int rowIndex,
            IReadOnlyList<BreadcrumbDescendantNode> nodes,
            string? parentNodeId,
            int depth,
            List<BreadcrumbVisiblePosition> target
        )
        {
            foreach (BreadcrumbDescendantNode node in nodes)
            {
                target.Add(
                    new BreadcrumbVisiblePosition(
                        rowIndex,
                        node.Identity,
                        parentNodeId,
                        depth,
                        NodeHasChildren(node),
                        node.Expanded
                    )
                );
                if (node.Expanded)
                {
                    AppendNodes(rowIndex, node.Children, node.Identity, depth + 1, target);
                }
            }
        }

        /// <summary>
        /// True when a node can show children: its segment reports children and a completed fetch
        /// did not return zero (#1007 AC14).
        /// </summary>
        internal static bool NodeHasChildren(BreadcrumbDescendantNode node) =>
            node.Segment.HasChildren && !(node.Loaded && node.Children.Count == 0);

        private static IReadOnlyList<BreadcrumbVisiblePosition> Require(
            IReadOnlyList<BreadcrumbVisiblePosition> order
        ) => order ?? throw new ArgumentNullException(nameof(order));
    }
}
