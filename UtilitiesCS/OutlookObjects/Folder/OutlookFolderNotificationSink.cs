using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Owns Outlook folder and store notification subscriptions for cache invalidation.
    /// </summary>
    public sealed class OutlookFolderNotificationSink : IOutlookFolderNotificationSink
    {
        private readonly IReadOnlyList<IOutlookFolderNotificationSubscription> _subscriptions;
        private bool _started;
        private bool _disposed;

        [ExcludeFromCodeCoverage]
        public OutlookFolderNotificationSink(Outlook.NameSpace namespaceMapi)
            : this(Array.Empty<IOutlookFolderNotificationSubscription>())
        {
            _ = namespaceMapi ?? throw new ArgumentNullException(nameof(namespaceMapi));
        }

        [ExcludeFromCodeCoverage]
        internal OutlookFolderNotificationSink(
            IEnumerable<IOutlookFolderNotificationSubscription> subscriptions
        )
        {
            _subscriptions = (
                subscriptions ?? Enumerable.Empty<IOutlookFolderNotificationSubscription>()
            ).ToArray();
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderAdded;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderRemoved;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderChanged;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreAdded;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreRemoved;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs> Disposed;

        [ExcludeFromCodeCoverage]
        public void Start()
        {
            if (_started)
            {
                return;
            }

            foreach (var subscription in _subscriptions)
            {
                subscription.Subscribe(HandleNotification);
            }

            _started = true;
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var subscription in _subscriptions)
            {
                subscription.Unsubscribe(HandleNotification);
            }

            _disposed = true;
            Disposed?.Invoke(this, CreateArgs(FolderTreeRefreshReason.Disposal, string.Empty));
        }

        [ExcludeFromCodeCoverage]
        private void HandleNotification(object sender, FolderTreeNotification notification)
        {
            var args = CreateArgs(notification.Reason, notification.StoreId);
            switch (notification.Reason)
            {
                case FolderTreeRefreshReason.FolderAdded:
                    FolderAdded?.Invoke(this, args);
                    break;
                case FolderTreeRefreshReason.FolderRemoved:
                    FolderRemoved?.Invoke(this, args);
                    break;
                case FolderTreeRefreshReason.StoreAdded:
                    StoreAdded?.Invoke(this, args);
                    break;
                case FolderTreeRefreshReason.StoreRemoved:
                    StoreRemoved?.Invoke(this, args);
                    break;
                default:
                    FolderChanged?.Invoke(this, args);
                    break;
            }
        }

        private static FolderTreeSnapshotChangedEventArgs CreateArgs(
            FolderTreeRefreshReason reason,
            string storeId
        )
        {
            var stores = string.IsNullOrWhiteSpace(storeId)
                ? Array.Empty<string>()
                : new[] { storeId };
            return new FolderTreeSnapshotChangedEventArgs(
                new FolderTreeSnapshot(
                    Array.Empty<FolderTreeNodeKey>(),
                    Array.Empty<FolderTreeSnapshotNode>()
                ),
                reason,
                stores
            );
        }

        internal interface IOutlookFolderNotificationSubscription
        {
            void Subscribe(EventHandler<FolderTreeNotification> handler);

            void Unsubscribe(EventHandler<FolderTreeNotification> handler);
        }

        internal sealed class FolderTreeNotification : EventArgs
        {
            public FolderTreeNotification(FolderTreeRefreshReason reason, string storeId)
            {
                Reason = reason;
                StoreId = storeId ?? string.Empty;
            }

            public FolderTreeRefreshReason Reason { get; }

            public string StoreId { get; }
        }
    }
}
