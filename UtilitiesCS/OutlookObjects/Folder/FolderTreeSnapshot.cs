using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Immutable folder tree snapshot with key, store, and path lookup helpers.
    /// </summary>
    public sealed class FolderTreeSnapshot
    {
        private readonly IReadOnlyDictionary<FolderTreeNodeKey, FolderTreeSnapshotNode> _nodesByKey;
        private readonly IReadOnlyDictionary<
            string,
            IReadOnlyList<FolderTreeSnapshotNode>
        > _nodesByStore;

        public FolderTreeSnapshot(
            IEnumerable<FolderTreeNodeKey> rootKeys,
            IEnumerable<FolderTreeSnapshotNode> nodes
        )
            : this(rootKeys, nodes, null) { }

        public FolderTreeSnapshot(
            IEnumerable<FolderTreeNodeKey> rootKeys,
            IEnumerable<FolderTreeSnapshotNode> nodes,
            FolderTreeRequest request
        )
        {
            RootKeys = new ReadOnlyCollection<FolderTreeNodeKey>(
                (rootKeys ?? Enumerable.Empty<FolderTreeNodeKey>()).ToArray()
            );
            CoversAllStores = request == null || request.IsAllStores;
            CoveredStoreIds = new ReadOnlyCollection<string>(
                CoversAllStores
                    ? Array.Empty<string>()
                    : request
                        .StoreIds.Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()
            );

            var nodeArray = (nodes ?? Enumerable.Empty<FolderTreeSnapshotNode>()).ToArray();
            _nodesByKey = new ReadOnlyDictionary<FolderTreeNodeKey, FolderTreeSnapshotNode>(
                nodeArray.ToDictionary(node => node.Key)
            );
            _nodesByStore = new ReadOnlyDictionary<string, IReadOnlyList<FolderTreeSnapshotNode>>(
                nodeArray
                    .GroupBy(node => node.StoreId, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group =>
                            (IReadOnlyList<FolderTreeSnapshotNode>)
                                new ReadOnlyCollection<FolderTreeSnapshotNode>(group.ToArray()),
                        StringComparer.OrdinalIgnoreCase
                    )
            );
        }

        public IReadOnlyList<FolderTreeNodeKey> RootKeys { get; }

        public bool CoversAllStores { get; }

        public IReadOnlyList<string> CoveredStoreIds { get; }

        public IReadOnlyDictionary<FolderTreeNodeKey, FolderTreeSnapshotNode> NodesByKey =>
            _nodesByKey;

        public bool Covers(FolderTreeRequest request)
        {
            if (request == null || request.IsAllStores)
            {
                return CoversAllStores;
            }

            return CoversAllStores
                || request.StoreIds.All(storeId =>
                    CoveredStoreIds.Contains(storeId, StringComparer.OrdinalIgnoreCase)
                );
        }

        public bool TryGetNode(FolderTreeNodeKey key, out FolderTreeSnapshotNode node)
        {
            if (key == null)
            {
                node = null;
                return false;
            }

            return _nodesByKey.TryGetValue(key, out node);
        }

        public IReadOnlyList<FolderTreeSnapshotNode> GetNodesForStore(string storeId)
        {
            if (string.IsNullOrWhiteSpace(storeId))
            {
                return Array.Empty<FolderTreeSnapshotNode>();
            }

            return _nodesByStore.TryGetValue(storeId, out var nodes)
                ? nodes
                : Array.Empty<FolderTreeSnapshotNode>();
        }

        public FolderTreeSnapshotNode FindByPath(string storeId, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return null;
            }

            return GetNodesForStore(storeId)
                .FirstOrDefault(node =>
                    string.Equals(node.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)
                );
        }

        public IReadOnlyList<FolderTreeSnapshotNode> GetChildren(FolderTreeNodeKey parentKey)
        {
            if (!TryGetNode(parentKey, out var parent))
            {
                return Array.Empty<FolderTreeSnapshotNode>();
            }

            return new ReadOnlyCollection<FolderTreeSnapshotNode>(
                parent
                    .ChildKeys.Where(_nodesByKey.ContainsKey)
                    .Select(key => _nodesByKey[key])
                    .ToArray()
            );
        }
    }
}
