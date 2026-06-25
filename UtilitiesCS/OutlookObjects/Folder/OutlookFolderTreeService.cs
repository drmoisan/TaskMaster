using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public enum OutlookFolderTreeServiceState
    {
        Empty,
        Building,
        Current,
        StaleCurrent,
        Refreshing,
        Disposed,
    }

    /// <summary>
    /// Provides a session-scoped cached Outlook folder tree snapshot.
    /// </summary>
    public sealed class OutlookFolderTreeService : IOutlookFolderTreeService
    {
        private readonly FolderTreeSnapshotBuilder _builder;
        private readonly IOutlookFolderNotificationSink _notificationSink;
        private readonly object _gate = new object();
        private FolderTreeSnapshot _snapshot;
        private Task<FolderTreeSnapshot> _inFlightSnapshot;
        private Task<FolderTreeSnapshot> _scheduledRefresh;
        private FolderTreeRequest _pendingRefreshRequest;

        public OutlookFolderTreeService(
            FolderTreeSnapshotBuilder builder,
            IOutlookFolderNotificationSink notificationSink
        )
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _notificationSink =
                notificationSink ?? throw new ArgumentNullException(nameof(notificationSink));
            State = OutlookFolderTreeServiceState.Empty;
            SubscribeNotifications();
            _notificationSink.Start();
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs> SnapshotChanged;

        public OutlookFolderTreeServiceState State { get; private set; }

        public async Task<FolderTreeSnapshot> GetSnapshotAsync(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        )
        {
            Task<FolderTreeSnapshot> buildTask;
            lock (_gate)
            {
                ThrowIfDisposed();
                if (
                    _snapshot != null
                    && State == OutlookFolderTreeServiceState.Current
                    && _snapshot.Covers(request)
                )
                {
                    return _snapshot;
                }

                if (
                    _snapshot != null
                    && State == OutlookFolderTreeServiceState.StaleCurrent
                    && request?.AllowStaleSnapshot == true
                    && _snapshot.Covers(request)
                )
                {
                    return _snapshot;
                }

                if (_inFlightSnapshot != null)
                {
                    buildTask = _inFlightSnapshot;
                }
                else
                {
                    State =
                        _snapshot == null
                            ? OutlookFolderTreeServiceState.Building
                            : OutlookFolderTreeServiceState.Refreshing;
                    buildTask = BuildAndPublishAsync(request, cancellationToken);
                    _inFlightSnapshot = buildTask.IsCompleted ? null : buildTask;
                }
            }

            return await buildTask.ConfigureAwait(false);
        }

        private async Task<FolderTreeSnapshot> BuildAndPublishAsync(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var snapshot = await _builder
                    .BuildSnapshotAsync(request, cancellationToken)
                    .ConfigureAwait(false);
                FolderTreeSnapshot publishedSnapshot;
                lock (_gate)
                {
                    publishedSnapshot = CreatePublishedSnapshot(_snapshot, snapshot, request);
                    _snapshot = publishedSnapshot;
                    _inFlightSnapshot = null;
                    var pendingRefreshRequest = _pendingRefreshRequest;
                    _pendingRefreshRequest = null;
                    if (pendingRefreshRequest == null)
                    {
                        State = OutlookFolderTreeServiceState.Current;
                    }
                    else
                    {
                        State = OutlookFolderTreeServiceState.Refreshing;
                        _scheduledRefresh = BuildAndPublishAsync(
                            pendingRefreshRequest,
                            CancellationToken.None
                        );
                        _inFlightSnapshot = _scheduledRefresh.IsCompleted
                            ? null
                            : _scheduledRefresh;
                    }
                }

                SnapshotChanged?.Invoke(
                    this,
                    new FolderTreeSnapshotChangedEventArgs(
                        publishedSnapshot,
                        FolderTreeRefreshReason.ManualRefresh,
                        request?.StoreIds
                    )
                );
                return publishedSnapshot;
            }
            catch
            {
                lock (_gate)
                {
                    _inFlightSnapshot = null;
                    State =
                        _snapshot == null
                            ? OutlookFolderTreeServiceState.Empty
                            : OutlookFolderTreeServiceState.StaleCurrent;
                }

                throw;
            }
        }

        public void MarkStale(string storeId, FolderTreeRefreshReason reason)
        {
            ThrowIfDisposed();
            if (_snapshot != null && State != OutlookFolderTreeServiceState.Refreshing)
            {
                State = OutlookFolderTreeServiceState.StaleCurrent;
            }
        }

        private void HandleNotification(object sender, FolderTreeSnapshotChangedEventArgs args)
        {
            if (State == OutlookFolderTreeServiceState.Disposed)
            {
                return;
            }

            var storeId = args.AffectedStoreIds.Count > 0 ? args.AffectedStoreIds[0] : string.Empty;
            MarkStale(storeId, args.Reason);
            lock (_gate)
            {
                var request =
                    RequiresAllStoreRefresh(args.Reason) || string.IsNullOrWhiteSpace(storeId)
                        ? FolderTreeRequest.AllStores(allowStaleSnapshot: false)
                        : FolderTreeRequest.ForStore(storeId, allowStaleSnapshot: false);
                if (_scheduledRefresh != null && !_scheduledRefresh.IsCompleted)
                {
                    return;
                }

                if (_inFlightSnapshot != null && !_inFlightSnapshot.IsCompleted)
                {
                    _pendingRefreshRequest = MergeRefreshRequests(_pendingRefreshRequest, request);
                    return;
                }

                _scheduledRefresh = GetSnapshotAsync(request, CancellationToken.None);
            }
        }

        private static FolderTreeSnapshot CreatePublishedSnapshot(
            FolderTreeSnapshot currentSnapshot,
            FolderTreeSnapshot refreshedSnapshot,
            FolderTreeRequest request
        )
        {
            if (
                currentSnapshot == null
                || refreshedSnapshot == null
                || request == null
                || request.IsAllStores
                || !currentSnapshot.CoversAllStores
            )
            {
                return refreshedSnapshot;
            }

            var refreshedStores = new HashSet<string>(
                request.StoreIds,
                StringComparer.OrdinalIgnoreCase
            );
            var mergedNodes = currentSnapshot
                .NodesByKey.Values.Where(node => !refreshedStores.Contains(node.StoreId))
                .Concat(refreshedSnapshot.NodesByKey.Values)
                .ToArray();
            var mergedRoots = currentSnapshot
                .RootKeys.Where(key => !refreshedStores.Contains(key.StoreId))
                .Concat(refreshedSnapshot.RootKeys)
                .Distinct()
                .ToArray();

            return new FolderTreeSnapshot(
                mergedRoots,
                mergedNodes,
                FolderTreeRequest.AllStores(allowStaleSnapshot: false)
            );
        }

        private static bool RequiresAllStoreRefresh(FolderTreeRefreshReason reason)
        {
            return reason == FolderTreeRefreshReason.StoreAdded
                || reason == FolderTreeRefreshReason.StoreRemoved;
        }

        private static FolderTreeRequest MergeRefreshRequests(
            FolderTreeRequest currentRequest,
            FolderTreeRequest incomingRequest
        )
        {
            if (incomingRequest == null || incomingRequest.IsAllStores)
            {
                return FolderTreeRequest.AllStores(allowStaleSnapshot: false);
            }

            if (currentRequest == null)
            {
                return incomingRequest;
            }

            if (currentRequest.IsAllStores)
            {
                return currentRequest;
            }

            return new FolderTreeRequest(
                currentRequest.StoreIds.Concat(incomingRequest.StoreIds),
                allowStaleSnapshot: false
            );
        }

        public void Dispose()
        {
            if (State == OutlookFolderTreeServiceState.Disposed)
            {
                return;
            }

            UnsubscribeNotifications();
            State = OutlookFolderTreeServiceState.Disposed;
            _notificationSink.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (State == OutlookFolderTreeServiceState.Disposed)
            {
                throw new ObjectDisposedException(nameof(OutlookFolderTreeService));
            }
        }

        private void SubscribeNotifications()
        {
            _notificationSink.FolderAdded += HandleNotification;
            _notificationSink.FolderRemoved += HandleNotification;
            _notificationSink.FolderChanged += HandleNotification;
            _notificationSink.StoreAdded += HandleNotification;
            _notificationSink.StoreRemoved += HandleNotification;
        }

        private void UnsubscribeNotifications()
        {
            _notificationSink.FolderAdded -= HandleNotification;
            _notificationSink.FolderRemoved -= HandleNotification;
            _notificationSink.FolderChanged -= HandleNotification;
            _notificationSink.StoreAdded -= HandleNotification;
            _notificationSink.StoreRemoved -= HandleNotification;
        }
    }
}
