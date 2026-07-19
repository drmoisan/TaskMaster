#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Identifies one Outlook folder node by store identity and folder identity.
    /// </summary>
    public sealed class FolderTreeNodeKey : IEquatable<FolderTreeNodeKey>
    {
        public FolderTreeNodeKey(string storeId, string entryId, string folderPath)
        {
            StoreId = RequireText(storeId, nameof(storeId));
            EntryId = entryId ?? string.Empty;
            FolderPath = NormalizePath(folderPath);
        }

        public string StoreId { get; }

        public string EntryId { get; }

        public string FolderPath { get; }

        public bool Equals(FolderTreeNodeKey? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(StoreId, other.StoreId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(EntryId, other.EntryId, StringComparison.Ordinal)
                && string.Equals(FolderPath, other.FolderPath, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as FolderTreeNodeKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(StoreId);
                hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(EntryId);
                hashCode =
                    (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(FolderPath);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{StoreId}:{EntryId}:{FolderPath}";
        }

        private static string NormalizePath(string folderPath)
        {
            return RequireText(folderPath, nameof(folderPath)).Trim();
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
