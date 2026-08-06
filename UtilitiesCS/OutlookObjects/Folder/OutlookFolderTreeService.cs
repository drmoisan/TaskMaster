#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Threading;

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

    public sealed class OutlookFolderTreeService : IOutlookFolderTreeService
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            typeof(OutlookFolderTreeService)
        );
        private readonly FolderTreeSnapshotBuilder _builder;
        private readonly IOutlookFolderNotificationSink _notificationSink;
        private readonly IUiDispatcher? _dispatcher;
        private readonly CancellationTokenSource _disposeCancellation = new();
        private readonly object _gate = new object();
        private FolderTreeSnapshot? _snapshot;
        private Task<FolderTreeSnapshot>? _inFlightSnapshot;
        private Task<FolderTreeSnapshot>? _scheduledRefresh;
        private FolderTreeRequest? _pendingRefreshRequest;
        private int _cleanupStarted;

        public OutlookFolderTreeService(
            FolderTreeSnapshotBuilder builder,
            IOutlookFolderNotificationSink notificationSink,
            IUiDispatcher? dispatcher = null
        )
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _notificationSink =
                notificationSink ?? throw new ArgumentNullException(nameof(notificationSink));
            _dispatcher = dispatcher;
            State = OutlookFolderTreeServiceState.Empty;
            SubscribeNotifications();
            _notificationSink.Start();
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? SnapshotChanged;
        internal event Action<Exception>? ScheduledRefreshFaulted;
        public OutlookFolderTreeServiceState State { get; private set; }

        public async Task<FolderTreeSnapshot> GetSnapshotAsync(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        )
        {
            Task<FolderTreeSnapshot> buildTask;
            TaskCompletionSource<FolderTreeSnapshot>? completion = null;
            lock (_gate)
            {
                ThrowIfDisposed();
                if (
                    _snapshot != null
                    && State == OutlookFolderTreeServiceState.Current
                    && _snapshot.Covers(request)
                )
                    return _snapshot;
                if (
                    _snapshot != null
                    && State == OutlookFolderTreeServiceState.StaleCurrent
                    && request?.AllowStaleSnapshot == true
                    && _snapshot.Covers(request)
                )
                    return _snapshot;
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
                    completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    buildTask = completion.Task;
                    _inFlightSnapshot = buildTask;
                }
            }
            if (completion is not null && TryAuthorizeBuild(completion))
                _ = CompleteBuildAsync(completion, request, cancellationToken);
            return await buildTask.ConfigureAwait(false);
        }

        private async Task CompleteBuildAsync(
            TaskCompletionSource<FolderTreeSnapshot> completion,
            FolderTreeRequest? request,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                completion.TrySetResult(await BuildAndPublishAsync(request, cancellationToken));
            }
            catch (OperationCanceledException exception)
            {
                var token = cancellationToken.IsCancellationRequested
                    ? cancellationToken
                    : exception.CancellationToken;
                completion.TrySetCanceled(token);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        }

        private bool TryAuthorizeBuild(TaskCompletionSource<FolderTreeSnapshot> completion)
        {
            lock (_gate)
            {
                if (State != OutlookFolderTreeServiceState.Disposed)
                    return true;
                completion.TrySetException(
                    new ObjectDisposedException(nameof(OutlookFolderTreeService))
                );
                return false;
            }
        }

        private async Task<FolderTreeSnapshot> BuildAndPublishAsync(
            FolderTreeRequest? request,
            CancellationToken cancellationToken
        )
        {
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _disposeCancellation.Token
            );
            try
            {
                var snapshot = await (
                    _dispatcher is null
                        ? _builder.BuildSnapshotAsync(request, linkedCancellation.Token)
                        : _dispatcher.InvokeAsync(() =>
                            _builder.BuildSnapshotAsync(request, linkedCancellation.Token)
                        )
                ).ConfigureAwait(false);
                FolderTreeSnapshot publishedSnapshot;
                TaskCompletionSource<FolderTreeSnapshot>? pendingCompletion = null;
                FolderTreeRequest? pendingRequest = null;
                EventHandler<FolderTreeSnapshotChangedEventArgs>? snapshotChanged;
                lock (_gate)
                {
                    ThrowIfDisposed();
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
                        pendingRequest = pendingRefreshRequest;
                        pendingCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                        _scheduledRefresh = pendingCompletion.Task;
                        ObserveScheduledRefresh(_scheduledRefresh);
                        _inFlightSnapshot = _scheduledRefresh;
                    }
                    snapshotChanged = SnapshotChanged;
                }
                var args = new FolderTreeSnapshotChangedEventArgs(
                    publishedSnapshot,
                    FolderTreeRefreshReason.ManualRefresh,
                    request?.StoreIds
                );
                foreach (
                    var handler in snapshotChanged?.GetInvocationList() ?? Array.Empty<Delegate>()
                )
                {
                    lock (_gate)
                    {
                        if (State == OutlookFolderTreeServiceState.Disposed)
                            break;
                    }
                    ((EventHandler<FolderTreeSnapshotChangedEventArgs>)handler)(this, args);
                }
                if (pendingCompletion is not null && TryAuthorizeBuild(pendingCompletion))
                    _ = CompleteBuildAsync(pendingCompletion, pendingRequest);
                return publishedSnapshot;
            }
            catch
            {
                lock (_gate)
                {
                    if (State == OutlookFolderTreeServiceState.Disposed)
                        throw new ObjectDisposedException(nameof(OutlookFolderTreeService));

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
            lock (_gate)
            {
                ThrowIfDisposed();
                if (_snapshot != null && State != OutlookFolderTreeServiceState.Refreshing)
                    State = OutlookFolderTreeServiceState.StaleCurrent;
            }
        }

        private void HandleNotification(object sender, FolderTreeSnapshotChangedEventArgs args)
        {
            var storeId = args.AffectedStoreIds.Count > 0 ? args.AffectedStoreIds[0] : string.Empty;
            TaskCompletionSource<FolderTreeSnapshot>? completion = null;
            FolderTreeRequest? refreshRequest = null;
            lock (_gate)
            {
                if (State == OutlookFolderTreeServiceState.Disposed)
                    return;

                if (_snapshot != null && State != OutlookFolderTreeServiceState.Refreshing)
                    State = OutlookFolderTreeServiceState.StaleCurrent;

                var request =
                    RequiresAllStoreRefresh(args.Reason) || string.IsNullOrWhiteSpace(storeId)
                        ? FolderTreeRequest.AllStores(allowStaleSnapshot: false)
                        : FolderTreeRequest.ForStore(storeId, allowStaleSnapshot: false);
                if (_scheduledRefresh != null && !_scheduledRefresh.IsCompleted)
                    return;

                if (_inFlightSnapshot != null && !_inFlightSnapshot.IsCompleted)
                {
                    _pendingRefreshRequest = MergeRefreshRequests(_pendingRefreshRequest, request);
                    return;
                }

                State = OutlookFolderTreeServiceState.Refreshing;
                refreshRequest = request;
                completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
                _scheduledRefresh = completion.Task;
                _inFlightSnapshot = _scheduledRefresh;
                ObserveScheduledRefresh(_scheduledRefresh);
            }

            if (completion is not null && TryAuthorizeBuild(completion))
                _ = CompleteBuildAsync(completion, refreshRequest);
        }

        private void ObserveScheduledRefresh(Task<FolderTreeSnapshot> scheduledRefresh) =>
            ObserveFault(scheduledRefresh, ReportScheduledRefreshFailure);

        private void ReportScheduledRefreshFailure(Exception exception)
        {
            Action<Exception>? observer;
            lock (_gate)
            {
                if (State == OutlookFolderTreeServiceState.Disposed)
                    return;
                observer = ScheduledRefreshFaulted;
            }
            logger.Error("A notification-scheduled folder-tree refresh failed.", exception);
            NotifyObserver(observer, exception, "The folder-tree refresh failure observer failed.");
        }

        private static FolderTreeSnapshot CreatePublishedSnapshot(
            FolderTreeSnapshot? currentSnapshot,
            FolderTreeSnapshot refreshedSnapshot,
            FolderTreeRequest? request
        )
        {
            if (
                currentSnapshot == null
                || request == null
                || request.IsAllStores
                || !currentSnapshot.CoversAllStores
            )
                return refreshedSnapshot!;
            var refreshedStores = new HashSet<string>(
                request.StoreIds,
                StringComparer.OrdinalIgnoreCase
            );
            return new FolderTreeSnapshot(
                currentSnapshot
                    .RootKeys.Where(key => !refreshedStores.Contains(key.StoreId))
                    .Concat(refreshedSnapshot.RootKeys)
                    .Distinct()
                    .ToArray(),
                currentSnapshot
                    .NodesByKey.Values.Where(node => !refreshedStores.Contains(node.StoreId))
                    .Concat(refreshedSnapshot.NodesByKey.Values)
                    .ToArray(),
                FolderTreeRequest.AllStores(allowStaleSnapshot: false)
            );
        }

        private static bool RequiresAllStoreRefresh(FolderTreeRefreshReason reason) =>
            reason == FolderTreeRefreshReason.StoreAdded
            || reason == FolderTreeRefreshReason.StoreRemoved;

        private static FolderTreeRequest MergeRefreshRequests(
            FolderTreeRequest? currentRequest,
            FolderTreeRequest? incomingRequest
        )
        {
            if (incomingRequest == null || incomingRequest.IsAllStores)
                return FolderTreeRequest.AllStores(allowStaleSnapshot: false);

            if (currentRequest == null || currentRequest.IsAllStores)
                return currentRequest ?? incomingRequest;

            return new FolderTreeRequest(
                currentRequest.StoreIds.Concat(incomingRequest.StoreIds),
                false
            );
        }

        public void Dispose()
        {
            var queueCleanup = false;
            var dispatcher = _dispatcher;
            Action<Exception>? cleanupFailureObserver;
            lock (_gate)
            {
                if (State == OutlookFolderTreeServiceState.Disposed)
                    return;

                cleanupFailureObserver = ScheduledRefreshFaulted;
                ScheduledRefreshFaulted = null;
                queueCleanup =
                    State == OutlookFolderTreeServiceState.Building
                    || State == OutlookFolderTreeServiceState.Refreshing
                    || (_inFlightSnapshot != null && !_inFlightSnapshot.IsCompleted)
                    || (_scheduledRefresh != null && !_scheduledRefresh.IsCompleted);
                State = OutlookFolderTreeServiceState.Disposed;
                _pendingRefreshRequest = null;
                SnapshotChanged = null;
            }
            Exception? cancellationFailure = null;
            try
            {
                _disposeCancellation.Cancel();
            }
            catch (Exception exception)
            {
                cancellationFailure = GetPrimaryFailure(exception);
            }
            if (dispatcher == null)
            {
                ExecuteCleanup(cancellationFailure, cleanupFailureObserver);
                return;
            }

            try
            {
                if (queueCleanup)
                {
                    ObserveFault(
                        dispatcher.InvokeAsync(() =>
                            ExecuteCleanup(cancellationFailure, cleanupFailureObserver)
                        ),
                        dispatchFailure =>
                            ReportCleanupFailure(
                                cancellationFailure ?? dispatchFailure,
                                cleanupFailureObserver
                            )
                    );
                    return;
                }

                dispatcher.Invoke(() =>
                    ExecuteCleanup(cancellationFailure, cleanupFailureObserver)
                );
            }
            catch (Exception dispatchFailure)
            {
                ReportCleanupFailure(
                    cancellationFailure ?? GetPrimaryFailure(dispatchFailure),
                    cleanupFailureObserver
                );
            }
        }

        private static Exception GetPrimaryFailure(Exception exception) =>
            exception is AggregateException aggregate && aggregate.InnerExceptions.Count > 0
                ? aggregate.Flatten().InnerExceptions[0]
                : exception;

        private static void ObserveFault(Task task, Action<Exception> onFault) =>
            _ = task.ContinueWith(
                completedTask => onFault(GetPrimaryFailure(completedTask.Exception)),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously
                    | TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default
            );

        private void ExecuteCleanup(
            Exception? initialFailure,
            Action<Exception>? cleanupFailureObserver
        )
        {
            if (Interlocked.Exchange(ref _cleanupStarted, 1) != 0)
                return;

            var cleanupFailure = initialFailure;
            foreach (
                var cleanupStage in new Action[]
                {
                    () => _notificationSink.FolderAdded -= HandleNotification,
                    () => _notificationSink.FolderRemoved -= HandleNotification,
                    () => _notificationSink.FolderChanged -= HandleNotification,
                    () => _notificationSink.StoreAdded -= HandleNotification,
                    () => _notificationSink.StoreRemoved -= HandleNotification,
                    _notificationSink.Dispose,
                }
            )
            {
                TryCleanupStage(cleanupStage, ref cleanupFailure);
            }

            if (cleanupFailure != null)
                ReportCleanupFailure(cleanupFailure, cleanupFailureObserver);
        }

        private static void TryCleanupStage(Action cleanupStage, ref Exception? cleanupFailure)
        {
            try
            {
                cleanupStage();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
        }

        private static void ReportCleanupFailure(
            Exception cleanupFailure,
            Action<Exception>? cleanupFailureObserver
        )
        {
            logger.Error("Folder-tree disposal cleanup failed.", cleanupFailure);
            NotifyObserver(
                cleanupFailureObserver,
                cleanupFailure,
                "The folder-tree cleanup failure observer failed."
            );
        }

        private static void NotifyObserver(
            Action<Exception>? observer,
            Exception exception,
            string failureMessage
        )
        {
            try
            {
                observer?.Invoke(exception);
            }
            catch (Exception observerException)
            {
                logger.Error(failureMessage, observerException);
            }
        }

        private void ThrowIfDisposed()
        {
            if (State == OutlookFolderTreeServiceState.Disposed)
                throw new ObjectDisposedException(nameof(OutlookFolderTreeService));
        }

        private void SubscribeNotifications()
        {
            _notificationSink.FolderAdded += HandleNotification;
            _notificationSink.FolderRemoved += HandleNotification;
            _notificationSink.FolderChanged += HandleNotification;
            _notificationSink.StoreAdded += HandleNotification;
            _notificationSink.StoreRemoved += HandleNotification;
        }
    }
}
