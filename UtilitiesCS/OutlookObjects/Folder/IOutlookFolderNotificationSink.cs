#nullable enable
using System;
using Outlook = Microsoft.Office.Interop.Outlook;

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

        /// <summary>
        /// Idempotently registers folder subscriptions for one store, keyed by its
        /// <c>StoreID</c>. A call for a <c>StoreID</c> that is already present is a documented
        /// no-op success and creates no duplicate subscription. Used by both startup population and
        /// the runtime rehook path so both share one per-store subscription implementation.
        /// </summary>
        /// <param name="store">The store whose folder subscriptions to register.</param>
        void AddStore(Outlook.Store store);

        /// <summary>
        /// Unsubscribes and removes the subscriptions previously registered for
        /// <paramref name="storeId"/>. A call for a <c>StoreID</c> that is not present is a no-op.
        /// Does not affect subscriptions registered for other stores.
        /// </summary>
        /// <param name="storeId">The <c>StoreID</c> whose subscriptions to remove.</param>
        void RemoveStore(string storeId);
    }
}
