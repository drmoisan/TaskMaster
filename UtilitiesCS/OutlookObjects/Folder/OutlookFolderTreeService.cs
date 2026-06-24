using System;
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
                if (_snapshot != null && State == OutlookFolderTreeServiceState.Current)
                {
                    return _snapshot;
                }

                if (
                    _snapshot != null
                    && State == OutlookFolderTreeServiceState.StaleCurrent
                    && request?.AllowStaleSnapshot == true
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
                lock (_gate)
                {
                    _snapshot = snapshot;
                    State = OutlookFolderTreeServiceState.Current;
                    _inFlightSnapshot = null;
                }

                SnapshotChanged?.Invoke(
                    this,
                    new FolderTreeSnapshotChangedEventArgs(
                        snapshot,
                        FolderTreeRefreshReason.ManualRefresh,
                        request?.StoreIds
                    )
                );
                return snapshot;
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
                if (_scheduledRefresh != null && !_scheduledRefresh.IsCompleted)
                {
                    return;
                }

                var request = string.IsNullOrWhiteSpace(storeId)
                    ? FolderTreeRequest.AllStores(allowStaleSnapshot: false)
                    : FolderTreeRequest.ForStore(storeId, allowStaleSnapshot: false);
                _scheduledRefresh = GetSnapshotAsync(request, CancellationToken.None);
            }
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
