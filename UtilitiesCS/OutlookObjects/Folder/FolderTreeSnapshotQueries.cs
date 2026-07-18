using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Read-only query helpers for folder tree snapshots.
    /// </summary>
    public static class FolderTreeSnapshotQueries
    {
        public static IReadOnlyList<FolderTreeSnapshotNode> GetSelectedNodes(
            FolderTreeSnapshot snapshot,
            FolderTreeSelectionOverlay selectionOverlay
        )
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var overlay = selectionOverlay ?? new FolderTreeSelectionOverlay(Array.Empty<string>());
            return snapshot.NodesByKey.Values.Where(overlay.IsSelected).ToArray();
        }

        public static FolderTreeSnapshotNode GetArchiveRoot(
            FolderTreeSnapshot snapshot,
            string storeId,
            string relativePath
        )
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return snapshot
                .GetNodesForStore(storeId)
                .FirstOrDefault(node =>
                    string.Equals(
                        node.RelativePath,
                        relativePath,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
        }

        public static IReadOnlyList<string> EnumerateRelativePaths(
            FolderTreeSnapshot snapshot,
            string storeId = null
        )
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var nodes = string.IsNullOrWhiteSpace(storeId)
                ? snapshot.NodesByKey.Values
                : snapshot.GetNodesForStore(storeId);
            return nodes.Select(node => node.RelativePath).OrderBy(path => path).ToArray();
        }

        public static IReadOnlyList<
            Tuple<FolderTreeSnapshotNode, FolderTreeSnapshotNode>
        > GetCompareInputs(FolderTreeSnapshot current, FolderTreeSnapshot other)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return current
                .NodesByKey.Values.Select(node =>
                    Tuple.Create(
                        node,
                        other.NodesByKey.Values.FirstOrDefault(candidate =>
                            string.Equals(
                                candidate.RelativePath,
                                node.RelativePath,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                )
                .ToArray();
        }

        /// <summary>
        /// Returns the ordered root-to-leaf ancestor chain for <paramref name="leafKey"/> by walking
        /// <see cref="FolderTreeSnapshotNode.ParentKey"/> to the store root and reversing to root-first
        /// order.
        /// </summary>
        /// <param name="snapshot">The immutable snapshot to walk. Required.</param>
        /// <param name="leafKey">Identity of the leaf folder node.</param>
        /// <returns>
        /// Nodes ordered root-first / leaf-last, with the last element equal to the requested leaf.
        /// An empty list (never null) when <paramref name="leafKey"/> is null or absent from the
        /// snapshot. A malformed cyclic <see cref="FolderTreeSnapshotNode.ParentKey"/> yields the
        /// partial chain rather than looping.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="snapshot"/> is null.</exception>
        public static IReadOnlyList<FolderTreeSnapshotNode> GetAncestorChain(
            FolderTreeSnapshot snapshot,
            FolderTreeNodeKey leafKey
        )
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (leafKey == null || !snapshot.TryGetNode(leafKey, out var leaf))
            {
                return Array.Empty<FolderTreeSnapshotNode>();
            }

            var chain = new List<FolderTreeSnapshotNode>();
            var visited = new HashSet<FolderTreeNodeKey>();
            var current = leaf;

            // visited.Add returns false on a repeat key, which terminates a malformed cyclic ParentKey
            // walk while preserving the partial chain collected so far.
            while (current != null && visited.Add(current.Key))
            {
                chain.Add(current);
                if (
                    current.ParentKey == null
                    || !snapshot.TryGetNode(current.ParentKey, out var parent)
                )
                {
                    break;
                }

                current = parent;
            }

            chain.Reverse();
            return chain.ToArray();
        }

        public static FolderTreeSnapshot CreateSubtreeSnapshot(
            FolderTreeSnapshot snapshot,
            FolderTreeSnapshotNode rootNode
        )
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode));
            }

            var nodes = new List<FolderTreeSnapshotNode>();
            var stack = new Stack<FolderTreeNodeKey>();
            stack.Push(rootNode.Key);
            while (stack.Count > 0)
            {
                var key = stack.Pop();
                if (!snapshot.TryGetNode(key, out var node))
                {
                    continue;
                }

                nodes.Add(node);
                foreach (var childKey in node.ChildKeys.Reverse())
                {
                    stack.Push(childKey);
                }
            }

            return new(new[] { rootNode.Key }, nodes);
        }
    }
}
