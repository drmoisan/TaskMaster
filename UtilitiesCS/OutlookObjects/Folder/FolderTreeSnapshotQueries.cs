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
