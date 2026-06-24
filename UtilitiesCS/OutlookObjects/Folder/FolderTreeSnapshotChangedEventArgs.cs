using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Describes a published folder tree snapshot change.
    /// </summary>
    public sealed class FolderTreeSnapshotChangedEventArgs : EventArgs
    {
        public FolderTreeSnapshotChangedEventArgs(
            FolderTreeSnapshot snapshot,
            FolderTreeRefreshReason reason,
            IEnumerable<string> affectedStoreIds
        )
        {
            Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Reason = reason;
            AffectedStoreIds = new ReadOnlyCollection<string>(
                (affectedStoreIds ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            );
        }

        public FolderTreeSnapshot Snapshot { get; }

        public FolderTreeRefreshReason Reason { get; }

        public IReadOnlyList<string> AffectedStoreIds { get; }
    }
}
