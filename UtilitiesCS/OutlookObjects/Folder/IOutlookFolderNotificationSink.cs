using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Owns live Outlook folder and store event subscriptions behind an adapter boundary.
    /// Unit tests must use fakes to raise notifications without Outlook COM.
    /// </summary>
    public interface IOutlookFolderNotificationSink : IDisposable
    {
        event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderAdded;

        event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderRemoved;

        event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderChanged;

        event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreAdded;

        event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreRemoved;

        event EventHandler<FolderTreeSnapshotChangedEventArgs> Disposed;

        void Start();
    }
}
