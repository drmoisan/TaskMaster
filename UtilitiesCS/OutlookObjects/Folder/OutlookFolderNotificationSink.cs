using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
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
            : this(CreateProductionSubscriptions(namespaceMapi)) { }

        [ExcludeFromCodeCoverage]
        internal OutlookFolderNotificationSink(
            IEnumerable<IOutlookFolderNotificationSubscription> subscriptions
        )
        {
            _subscriptions = (
                subscriptions ?? Enumerable.Empty<IOutlookFolderNotificationSubscription>()
            ).ToArray();
        }

        internal int SubscriptionCount => _subscriptions.Count;

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

        [ExcludeFromCodeCoverage]
        private static IReadOnlyList<IOutlookFolderNotificationSubscription> CreateProductionSubscriptions(
            Outlook.NameSpace namespaceMapi
        )
        {
            if (namespaceMapi == null)
            {
                throw new ArgumentNullException(nameof(namespaceMapi));
            }

            var subscriptions = new List<IOutlookFolderNotificationSubscription>();
            var stores = namespaceMapi.Stores;
            if (stores == null)
            {
                return subscriptions;
            }

            subscriptions.Add(new StoresNotificationSubscription(stores));
            AddFolderSubscriptions(stores, subscriptions);
            return subscriptions;
        }

        [ExcludeFromCodeCoverage]
        private static void AddFolderSubscriptions(
            Outlook.Stores stores,
            ICollection<IOutlookFolderNotificationSubscription> subscriptions
        )
        {
            try
            {
                foreach (Outlook.Store store in stores)
                {
                    AddFolderSubscriptions(store, subscriptions);
                }
            }
            catch (COMException)
            {
                return;
            }
            catch (InvalidCastException)
            {
                return;
            }
        }

        [ExcludeFromCodeCoverage]
        private static void AddFolderSubscriptions(
            Outlook.Store store,
            ICollection<IOutlookFolderNotificationSubscription> subscriptions
        )
        {
            if (store == null)
            {
                return;
            }

            var root = store.GetRootFolder() as Outlook.MAPIFolder;
            if (root == null)
            {
                return;
            }

            var stack = new Stack<Outlook.MAPIFolder>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var folder = stack.Pop();
                var children = folder.Folders;
                if (children == null)
                {
                    continue;
                }

                subscriptions.Add(
                    new FoldersNotificationSubscription(children, store.StoreID ?? string.Empty)
                );
                foreach (Outlook.MAPIFolder child in children)
                {
                    stack.Push(child);
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private sealed class StoresNotificationSubscription : IOutlookFolderNotificationSubscription
        {
            private readonly Outlook.Stores _stores;
            private EventHandler<FolderTreeNotification> _handler;

            public StoresNotificationSubscription(Outlook.Stores stores)
            {
                _stores = stores ?? throw new ArgumentNullException(nameof(stores));
            }

            public void Subscribe(EventHandler<FolderTreeNotification> handler)
            {
                _handler += handler;
                _stores.StoreAdd += OnStoreAdd;
                _stores.BeforeStoreRemove += OnBeforeStoreRemove;
            }

            public void Unsubscribe(EventHandler<FolderTreeNotification> handler)
            {
                _stores.StoreAdd -= OnStoreAdd;
                _stores.BeforeStoreRemove -= OnBeforeStoreRemove;
                _handler -= handler;
            }

            private void OnStoreAdd(Outlook.Store store)
            {
                _handler?.Invoke(
                    this,
                    new FolderTreeNotification(FolderTreeRefreshReason.StoreAdded, store?.StoreID)
                );
            }

            private void OnBeforeStoreRemove(Outlook.Store store, ref bool cancel)
            {
                _handler?.Invoke(
                    this,
                    new FolderTreeNotification(FolderTreeRefreshReason.StoreRemoved, store?.StoreID)
                );
            }
        }

        [ExcludeFromCodeCoverage]
        private sealed class FoldersNotificationSubscription
            : IOutlookFolderNotificationSubscription
        {
            private readonly Outlook.Folders _folders;
            private readonly string _storeId;
            private EventHandler<FolderTreeNotification> _handler;

            public FoldersNotificationSubscription(Outlook.Folders folders, string storeId)
            {
                _folders = folders ?? throw new ArgumentNullException(nameof(folders));
                _storeId = storeId ?? string.Empty;
            }

            public void Subscribe(EventHandler<FolderTreeNotification> handler)
            {
                _handler += handler;
                _folders.FolderAdd += OnFolderAdd;
                _folders.FolderChange += OnFolderChange;
                _folders.FolderRemove += OnFolderRemove;
            }

            public void Unsubscribe(EventHandler<FolderTreeNotification> handler)
            {
                _folders.FolderAdd -= OnFolderAdd;
                _folders.FolderChange -= OnFolderChange;
                _folders.FolderRemove -= OnFolderRemove;
                _handler -= handler;
            }

            private void OnFolderAdd(Outlook.MAPIFolder folder)
            {
                _handler?.Invoke(
                    this,
                    new FolderTreeNotification(FolderTreeRefreshReason.FolderAdded, _storeId)
                );
            }

            private void OnFolderChange(Outlook.MAPIFolder folder)
            {
                _handler?.Invoke(
                    this,
                    new FolderTreeNotification(FolderTreeRefreshReason.FolderChanged, _storeId)
                );
            }

            private void OnFolderRemove()
            {
                _handler?.Invoke(
                    this,
                    new FolderTreeNotification(FolderTreeRefreshReason.FolderRemoved, _storeId)
                );
            }
        }
    }
}
