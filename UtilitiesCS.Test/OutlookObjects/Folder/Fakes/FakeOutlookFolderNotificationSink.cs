using System;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder.Fakes
{
    public sealed class FakeOutlookFolderNotificationSink : IOutlookFolderNotificationSink
    {
        private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderAdded;
        private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderRemoved;
        private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderChanged;
        private EventHandler<FolderTreeSnapshotChangedEventArgs> _storeAdded;
        private EventHandler<FolderTreeSnapshotChangedEventArgs> _storeRemoved;
        private EventHandler<FolderTreeSnapshotChangedEventArgs> _disposed;

        public int FolderAddedHandlerCount { get; private set; }
        public int FolderRemovedHandlerCount { get; private set; }
        public int FolderChangedHandlerCount { get; private set; }
        public int StoreAddedHandlerCount { get; private set; }
        public int StoreRemovedHandlerCount { get; private set; }
        public int DisposedHandlerCount { get; private set; }
        public int StartCount { get; private set; }
        public int DisposeCount { get; private set; }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderAdded
        {
            add
            {
                _folderAdded += value;
                FolderAddedHandlerCount++;
            }
            remove
            {
                _folderAdded -= value;
                FolderAddedHandlerCount--;
            }
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderRemoved
        {
            add
            {
                _folderRemoved += value;
                FolderRemovedHandlerCount++;
            }
            remove
            {
                _folderRemoved -= value;
                FolderRemovedHandlerCount--;
            }
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderChanged
        {
            add
            {
                _folderChanged += value;
                FolderChangedHandlerCount++;
            }
            remove
            {
                _folderChanged -= value;
                FolderChangedHandlerCount--;
            }
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreAdded
        {
            add
            {
                _storeAdded += value;
                StoreAddedHandlerCount++;
            }
            remove
            {
                _storeAdded -= value;
                StoreAddedHandlerCount--;
            }
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreRemoved
        {
            add
            {
                _storeRemoved += value;
                StoreRemovedHandlerCount++;
            }
            remove
            {
                _storeRemoved -= value;
                StoreRemovedHandlerCount--;
            }
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> Disposed
        {
            add
            {
                _disposed += value;
                DisposedHandlerCount++;
            }
            remove
            {
                _disposed -= value;
                DisposedHandlerCount--;
            }
        }

        public void Start()
        {
            StartCount++;
        }

        public void Dispose()
        {
            DisposeCount++;
            RaiseDisposed(CreateArgs(FolderTreeRefreshReason.Disposal));
        }

        public void RaiseFolderAdded(FolderTreeSnapshotChangedEventArgs args) =>
            _folderAdded?.Invoke(this, args);

        public void RaiseFolderRemoved(FolderTreeSnapshotChangedEventArgs args) =>
            _folderRemoved?.Invoke(this, args);

        public void RaiseFolderChanged(FolderTreeSnapshotChangedEventArgs args) =>
            _folderChanged?.Invoke(this, args);

        public void RaiseStoreAdded(FolderTreeSnapshotChangedEventArgs args) =>
            _storeAdded?.Invoke(this, args);

        public void RaiseStoreRemoved(FolderTreeSnapshotChangedEventArgs args) =>
            _storeRemoved?.Invoke(this, args);

        public void RaiseDisposed(FolderTreeSnapshotChangedEventArgs args) =>
            _disposed?.Invoke(this, args);

        public static FolderTreeSnapshotChangedEventArgs CreateArgs(FolderTreeRefreshReason reason)
        {
            return CreateArgs(reason, string.Empty);
        }

        public static FolderTreeSnapshotChangedEventArgs CreateArgs(
            FolderTreeRefreshReason reason,
            string storeId
        )
        {
            return new FolderTreeSnapshotChangedEventArgs(
                new FolderTreeSnapshot(
                    Array.Empty<FolderTreeNodeKey>(),
                    Array.Empty<FolderTreeSnapshotNode>()
                ),
                reason,
                string.IsNullOrWhiteSpace(storeId) ? Array.Empty<string>() : new[] { storeId }
            );
        }
    }
}
