#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Immutable folder metadata captured from an Outlook folder hierarchy.
    /// </summary>
    public sealed class FolderTreeSnapshotNode
    {
        public FolderTreeSnapshotNode(
            FolderTreeNodeKey key,
            string displayName,
            string storeId,
            string entryId,
            FolderTreeNodeKey? parentKey,
            string folderPath,
            string relativePath,
            IEnumerable<FolderTreeNodeKey>? childKeys,
            bool isStale,
            string staleReason
        )
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            DisplayName = RequireText(displayName, nameof(displayName));
            StoreId = RequireText(storeId, nameof(storeId));
            EntryId = entryId ?? string.Empty;
            ParentKey = parentKey;
            FolderPath = RequireText(folderPath, nameof(folderPath));
            RelativePath = relativePath ?? string.Empty;
            ChildKeys = new ReadOnlyCollection<FolderTreeNodeKey>(
                (childKeys ?? Enumerable.Empty<FolderTreeNodeKey>()).ToArray()
            );
            IsStale = isStale;
            StaleReason = staleReason ?? string.Empty;
        }

        public FolderTreeNodeKey Key { get; }

        public string DisplayName { get; }

        public string StoreId { get; }

        public string EntryId { get; }

        public FolderTreeNodeKey? ParentKey { get; }

        public string FolderPath { get; }

        public string RelativePath { get; }

        public IReadOnlyList<FolderTreeNodeKey> ChildKeys { get; }

        public bool IsStale { get; }

        public string StaleReason { get; }

        public FolderTreeSnapshotNode MarkStale(string reason)
        {
            return new FolderTreeSnapshotNode(
                Key,
                DisplayName,
                StoreId,
                EntryId,
                ParentKey,
                FolderPath,
                RelativePath,
                ChildKeys,
                true,
                reason
            );
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A non-empty value is required.", parameterName);
            }

            return value.Trim();
        }
    }
}
