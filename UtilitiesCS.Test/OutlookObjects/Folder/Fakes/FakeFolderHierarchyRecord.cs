using System;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder.Fakes
{
    public sealed class FakeFolderHierarchyRecord
    {
        public FakeFolderHierarchyRecord(
            string storeId,
            string entryId,
            string parentEntryId,
            string displayName,
            string folderPath,
            string relativePath
        )
        {
            StoreId = RequireText(storeId, nameof(storeId));
            EntryId = RequireText(entryId, nameof(entryId));
            ParentEntryId = parentEntryId ?? string.Empty;
            DisplayName = RequireText(displayName, nameof(displayName));
            FolderPath = RequireText(folderPath, nameof(folderPath));
            RelativePath = relativePath ?? string.Empty;
        }

        public string StoreId { get; }

        public string EntryId { get; }

        public string ParentEntryId { get; }

        public string DisplayName { get; }

        public string FolderPath { get; }

        public string RelativePath { get; }

        public FolderTreeNodeKey Key => new FolderTreeNodeKey(StoreId, EntryId, FolderPath);

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
