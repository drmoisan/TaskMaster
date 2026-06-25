using System;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Provides cached folder tree snapshots to callers without exposing live Outlook COM traversal.
    /// Unit tests must use fakes for this service and must not require a live Outlook session.
    /// </summary>
    public interface IOutlookFolderTreeService : IDisposable
    {
        event EventHandler<FolderTreeSnapshotChangedEventArgs> SnapshotChanged;

        Task<FolderTreeSnapshot> GetSnapshotAsync(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        );

        void MarkStale(string storeId, FolderTreeRefreshReason reason);
    }
}
