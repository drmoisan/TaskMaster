using System;
using System.Collections.Generic;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder.Fakes
{
    public sealed class FakeFolderHandleResolver : IFolderHandleResolver
    {
        private readonly Dictionary<FolderTreeNodeKey, object> _folders =
            new Dictionary<FolderTreeNodeKey, object>();

        public int ResolveCount { get; private set; }

        public FakeFolderHandleResolver Add(FolderTreeNodeKey key, object folder)
        {
            _folders[key ?? throw new ArgumentNullException(nameof(key))] =
                folder ?? throw new ArgumentNullException(nameof(folder));
            return this;
        }

        public object Resolve(FolderTreeSnapshotNode node)
        {
            ResolveCount++;
            if (!TryResolve(node, out var folder))
            {
                throw new KeyNotFoundException(
                    "No fake folder handle exists for the snapshot node."
                );
            }

            return folder;
        }

        public bool TryResolve(FolderTreeSnapshotNode node, out object folder)
        {
            if (node == null)
            {
                folder = null;
                return false;
            }

            return _folders.TryGetValue(node.Key, out folder);
        }
    }
}
