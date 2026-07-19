#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Describes the requested store scope and whether a stale snapshot is acceptable.
    /// </summary>
    public sealed class FolderTreeRequest
    {
        public FolderTreeRequest(IEnumerable<string>? storeIds, bool allowStaleSnapshot)
        {
            StoreIds = new ReadOnlyCollection<string>(
                (storeIds ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            );
            AllowStaleSnapshot = allowStaleSnapshot;
        }

        public IReadOnlyList<string> StoreIds { get; }

        public bool AllowStaleSnapshot { get; }

        public bool IsAllStores => StoreIds.Count == 0;

        public static FolderTreeRequest AllStores(bool allowStaleSnapshot)
        {
            return new FolderTreeRequest(Array.Empty<string>(), allowStaleSnapshot);
        }

        public static FolderTreeRequest ForStore(string storeId, bool allowStaleSnapshot)
        {
            if (string.IsNullOrWhiteSpace(storeId))
            {
                throw new ArgumentException("A non-empty value is required.", nameof(storeId));
            }

            return new FolderTreeRequest(new[] { storeId }, allowStaleSnapshot);
        }

        public bool IncludesStore(string storeId)
        {
            return IsAllStores || StoreIds.Contains(storeId, StringComparer.OrdinalIgnoreCase);
        }
    }
}
