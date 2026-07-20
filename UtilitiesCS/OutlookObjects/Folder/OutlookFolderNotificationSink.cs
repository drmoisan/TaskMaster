#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Owns Outlook folder and store notification subscriptions for cache invalidation. Folder
    /// subscriptions are held in a mutable, StoreID-keyed structure so a single store can be added
    /// or removed at runtime (issue #263, epic #260) without rebuilding the whole set; the
    /// app-level <c>Stores.StoreAdd</c>/<c>BeforeStoreRemove</c> subscription remains a single
    /// app-level owner. <see cref="Start"/>/<see cref="Dispose"/> subscribe/unsubscribe the whole
    /// collection once each, as before.
    /// </summary>
    public sealed class OutlookFolderNotificationSink : IOutlookFolderNotificationSink
    {
        // App-level subscriptions not keyed to a specific store (the Stores.StoreAdd/BeforeStoreRemove
        // owner), plus any subscriptions supplied directly through the internal test constructor.
        private readonly List<IOutlookFolderNotificationSubscription> _appLevelSubscriptions;

        // Per-store folder subscriptions keyed by StoreID. A StoreID present here is already hooked;
        // AddStore for the same StoreID is an idempotent no-op success.
        private readonly Dictionary<
            string,
            IReadOnlyList<IOutlookFolderNotificationSubscription>
        > _storeSubscriptions;

        private readonly object _gate = new object();
        private bool _started;
        private bool _disposed;

        [ExcludeFromCodeCoverage]
        public OutlookFolderNotificationSink(Outlook.NameSpace namespaceMapi)
        {
            if (namespaceMapi == null)
            {
                throw new ArgumentNullException(nameof(namespaceMapi));
            }

            _appLevelSubscriptions = new List<IOutlookFolderNotificationSubscription>();
            _storeSubscriptions = new Dictionary<
                string,
                IReadOnlyList<IOutlookFolderNotificationSubscription>
            >(StringComparer.Ordinal);

            var stores = namespaceMapi.Stores;
            if (stores == null)
            {
                return;
            }

            _appLevelSubscriptions.Add(new StoresNotificationSubscription(stores));
            AddAllStores(stores);
        }

        [ExcludeFromCodeCoverage]
        internal OutlookFolderNotificationSink(
            IEnumerable<IOutlookFolderNotificationSubscription> subscriptions
        )
        {
            _appLevelSubscriptions = (
                subscriptions ?? Enumerable.Empty<IOutlookFolderNotificationSubscription>()
            ).ToList();
            _storeSubscriptions = new Dictionary<
                string,
                IReadOnlyList<IOutlookFolderNotificationSubscription>
            >(StringComparer.Ordinal);
        }

        internal int SubscriptionCount
        {
            get
            {
                lock (_gate)
                {
                    return _appLevelSubscriptions.Count
                        + _storeSubscriptions.Values.Sum(list => list.Count);
                }
            }
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? FolderAdded;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? FolderRemoved;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? FolderChanged;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? StoreAdded;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? StoreRemoved;
        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? Disposed;

        [ExcludeFromCodeCoverage]
        public void Start()
        {
            lock (_gate)
            {
                if (_started)
                {
                    return;
                }

                foreach (var subscription in _appLevelSubscriptions)
                {
                    subscription.Subscribe(HandleNotification);
                }

                foreach (var subscriptions in _storeSubscriptions.Values)
                {
                    foreach (var subscription in subscriptions)
                    {
                        subscription.Subscribe(HandleNotification);
                    }
                }

                _started = true;
            }
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                foreach (var subscription in _appLevelSubscriptions)
                {
                    subscription.Unsubscribe(HandleNotification);
                }

                foreach (var subscriptions in _storeSubscriptions.Values)
                {
                    foreach (var subscription in subscriptions)
                    {
                        subscription.Unsubscribe(HandleNotification);
                    }
                }

                _disposed = true;
            }

            Disposed?.Invoke(this, CreateArgs(FolderTreeRefreshReason.Disposal, string.Empty));
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public void AddStore(Outlook.Store store)
        {
            if (store == null)
            {
                return;
            }

            string storeId;
            try
            {
                storeId = store.StoreID ?? string.Empty;
            }
            catch (COMException)
            {
                return;
            }

            lock (_gate)
            {
                // Cheap already-present guard: skip the COM folder traversal for a store already
                // hooked (documented no-op success). The authoritative guard is in
                // AddStoreSubscriptions, which is atomic with the subscribe.
                if (_storeSubscriptions.ContainsKey(storeId))
                {
                    return;
                }
            }

            var subscriptions = BuildStoreFolderSubscriptions(store);
            AddStoreSubscriptions(storeId, subscriptions);
        }

        /// <summary>
        /// Registers the pre-built folder <paramref name="subscriptions"/> for
        /// <paramref name="storeId"/>, keyed by StoreID. If the StoreID is already present this is a
        /// documented no-op success (no duplicate subscription). When the sink is already started,
        /// the newly registered subscriptions are subscribed immediately so a runtime rehook wires
        /// live handlers. This is the COM-free registration seam behind <see cref="AddStore"/>,
        /// exercised directly by tests with fake subscriptions.
        /// </summary>
        /// <param name="storeId">The StoreID key; must not be null.</param>
        /// <param name="subscriptions">The subscriptions to register; must not be null.</param>
        internal void AddStoreSubscriptions(
            string storeId,
            IReadOnlyList<IOutlookFolderNotificationSubscription> subscriptions
        )
        {
            if (storeId == null)
            {
                throw new ArgumentNullException(nameof(storeId));
            }

            if (subscriptions == null)
            {
                throw new ArgumentNullException(nameof(subscriptions));
            }

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                if (_storeSubscriptions.ContainsKey(storeId))
                {
                    // Already present: idempotent no-op success, zero additional subscribes.
                    return;
                }

                _storeSubscriptions[storeId] = subscriptions;

                if (_started)
                {
                    foreach (var subscription in subscriptions)
                    {
                        subscription.Subscribe(HandleNotification);
                    }
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> when folder subscriptions for <paramref name="storeId"/> have been
        /// registered (via <see cref="AddStore"/>). A pure, non-COM StoreID membership query used by
        /// the runtime rehook coordinator's already-fully-hooked predicate. Public because the
        /// coordinator lives in the <c>TaskMaster</c> assembly.
        /// </summary>
        /// <param name="storeId">The StoreID to test.</param>
        public bool IsStoreHooked(string storeId)
        {
            if (storeId == null)
            {
                return false;
            }

            lock (_gate)
            {
                return _storeSubscriptions.ContainsKey(storeId);
            }
        }

        /// <inheritdoc/>
        public void RemoveStore(string storeId)
        {
            if (storeId == null)
            {
                return;
            }

            lock (_gate)
            {
                if (!_storeSubscriptions.TryGetValue(storeId, out var subscriptions))
                {
                    return;
                }

                if (_started && !_disposed)
                {
                    foreach (var subscription in subscriptions)
                    {
                        subscription.Unsubscribe(HandleNotification);
                    }
                }

                _storeSubscriptions.Remove(storeId);
            }
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
            public FolderTreeNotification(FolderTreeRefreshReason reason, string? storeId)
            {
                Reason = reason;
                StoreId = storeId ?? string.Empty;
            }

            public FolderTreeRefreshReason Reason { get; }

            public string StoreId { get; }
        }

        [ExcludeFromCodeCoverage]
        private void AddAllStores(Outlook.Stores stores)
        {
            try
            {
                foreach (Outlook.Store store in stores)
                {
                    AddStore(store);
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
        private static IReadOnlyList<IOutlookFolderNotificationSubscription> BuildStoreFolderSubscriptions(
            Outlook.Store store
        )
        {
            var subscriptions = new List<IOutlookFolderNotificationSubscription>();
            if (store == null)
            {
                return subscriptions;
            }

            var root = store.GetRootFolder() as Outlook.MAPIFolder;
            if (root == null)
            {
                return subscriptions;
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

            return subscriptions;
        }

        [ExcludeFromCodeCoverage]
        private sealed class StoresNotificationSubscription : IOutlookFolderNotificationSubscription
        {
            private readonly Outlook.Stores _stores;
            private EventHandler<FolderTreeNotification>? _handler;

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
            private EventHandler<FolderTreeNotification>? _handler;

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
