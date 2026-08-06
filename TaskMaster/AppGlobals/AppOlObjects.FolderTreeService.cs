using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;

namespace TaskMaster
{
    public partial class AppOlObjects
    {
        private readonly object _folderTreeServiceGate = new object();
        private IOutlookFolderTreeService _folderTreeService;
        private TaskCompletionSource<IOutlookFolderTreeService> _folderTreeServiceInitialization;
        private IUiDispatcher _folderTreeServiceDispatcher;
        private bool _folderTreeServiceCompositionStarted;
        private int _folderTreeServiceCompositionThreadId;
        private bool _disposed;

        public IOutlookFolderTreeService FolderTreeService
        {
            get
            {
                TaskCompletionSource<IOutlookFolderTreeService> initialization;
                IUiDispatcher dispatcher;
                var queueComposition = false;
                var composeOnCurrentThread = false;
                var reentrantComposition = false;
                Exception setupFailure = null;
                var setupFailureCompleted = false;
                lock (_folderTreeServiceGate)
                {
                    if (_disposed)
                    {
                        throw new ObjectDisposedException(nameof(AppOlObjects));
                    }

                    if (_folderTreeService is not null)
                    {
                        return _folderTreeService;
                    }

                    initialization = _folderTreeServiceInitialization;
                    reentrantComposition =
                        initialization is not null
                        && _folderTreeServiceCompositionStarted
                        && _folderTreeServiceCompositionThreadId
                            == Thread.CurrentThread.ManagedThreadId;
                    if (initialization is null)
                    {
                        initialization = new TaskCompletionSource<IOutlookFolderTreeService>(
                            TaskCreationOptions.RunContinuationsAsynchronously
                        );
                        _folderTreeServiceInitialization = initialization;
                        try
                        {
                            _folderTreeServiceDispatcher = CreateFolderTreeServiceDispatcher();
                            if (_folderTreeServiceDispatcher is null)
                            {
                                setupFailure = new InvalidOperationException(
                                    "Folder tree service dispatcher factory returned null."
                                );
                            }
                        }
                        catch (Exception exception)
                        {
                            setupFailure = exception;
                        }
                        queueComposition = true;
                    }

                    dispatcher = _folderTreeServiceDispatcher;
                    if (setupFailure is null)
                    {
                        try
                        {
                            composeOnCurrentThread = IsFolderTreeServiceDispatcherThread(
                                dispatcher
                            );
                        }
                        catch (Exception exception)
                        {
                            setupFailure = exception;
                        }
                    }

                    if (setupFailure is not null)
                    {
                        setupFailureCompleted = initialization.TrySetException(setupFailure);
                        _folderTreeServiceInitialization = null;
                        _folderTreeServiceDispatcher = null;
                        _folderTreeServiceCompositionStarted = false;
                        _folderTreeServiceCompositionThreadId = 0;
                    }
                    else if (reentrantComposition)
                    {
                        setupFailure = new InvalidOperationException(
                            "Folder tree service composition cannot reenter on its composing thread."
                        );
                        setupFailureCompleted = initialization.TrySetException(setupFailure);
                        _folderTreeServiceInitialization = null;
                        _folderTreeServiceDispatcher = null;
                        _folderTreeServiceCompositionStarted = false;
                        _folderTreeServiceCompositionThreadId = 0;
                    }
                }

                if (setupFailure is not null)
                {
                    if (setupFailureCompleted)
                    {
                        NotifyFolderTreeServiceInitializationTerminal(initialization.Task);
                    }

                    return initialization.Task.GetAwaiter().GetResult();
                }

                if (composeOnCurrentThread)
                {
                    CompleteFolderTreeServiceComposition(initialization, dispatcher);
                }
                else if (queueComposition)
                {
                    Task dispatchTask;
                    try
                    {
                        dispatchTask = dispatcher.InvokeAsync(() =>
                            CompleteFolderTreeServiceComposition(initialization, dispatcher)
                        );
                    }
                    catch (Exception exception)
                    {
                        CompleteFolderTreeServiceCompositionFailure(initialization, exception);
                        return initialization.Task.GetAwaiter().GetResult();
                    }

                    if (dispatchTask is null)
                    {
                        CompleteFolderTreeServiceCompositionFailure(
                            initialization,
                            new InvalidOperationException(
                                "Folder tree service dispatcher returned a null task."
                            )
                        );
                    }
                    else
                    {
                        _ = dispatchTask.ContinueWith(
                            completedTask =>
                                ObserveFolderTreeServiceDispatchTerminal(
                                    initialization,
                                    completedTask
                                ),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default
                        );

                        if (dispatchTask.IsCompleted)
                        {
                            ObserveFolderTreeServiceDispatchTerminal(initialization, dispatchTask);
                        }
                    }
                }

                return initialization.Task.GetAwaiter().GetResult();
            }
        }

        private void CompleteFolderTreeServiceComposition(
            TaskCompletionSource<IOutlookFolderTreeService> initialization,
            IUiDispatcher dispatcher
        )
        {
            lock (_folderTreeServiceGate)
            {
                if (
                    !ReferenceEquals(initialization, _folderTreeServiceInitialization)
                    || _folderTreeServiceCompositionStarted
                )
                {
                    return;
                }

                _folderTreeServiceCompositionStarted = true;
                _folderTreeServiceCompositionThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            IOutlookFolderTreeService service = null;
            OutlookFolderNotificationSink notificationSink = null;
            try
            {
                OnFolderTreeServiceCompositionStarting();
                service = LoadFolderTreeService(dispatcher, out notificationSink);
                OnFolderTreeServiceBeforeInitializationCompletion(service);
                var discardService = false;
                var terminallyCompleted = false;
                lock (_folderTreeServiceGate)
                {
                    discardService =
                        _disposed
                        || !ReferenceEquals(initialization, _folderTreeServiceInitialization);
                    if (!discardService)
                    {
                        _folderTreeService = service;
                        _folderNotificationSink = notificationSink;
                        terminallyCompleted = initialization.TrySetResult(service);
                        _folderTreeServiceInitialization = null;
                        _folderTreeServiceDispatcher = null;
                        _folderTreeServiceCompositionStarted = false;
                        _folderTreeServiceCompositionThreadId = 0;
                    }
                    else if (ReferenceEquals(initialization, _folderTreeServiceInitialization))
                    {
                        terminallyCompleted = initialization.TrySetException(
                            new ObjectDisposedException(nameof(AppOlObjects))
                        );
                        _folderTreeServiceInitialization = null;
                        _folderTreeServiceDispatcher = null;
                        _folderTreeServiceCompositionStarted = false;
                        _folderTreeServiceCompositionThreadId = 0;
                    }
                }

                if (discardService)
                {
                    DisposeFolderTreeServiceCandidate(service, notificationSink);
                }
                if (terminallyCompleted)
                {
                    NotifyFolderTreeServiceInitializationTerminal(initialization.Task);
                }

                service = null;
            }
            catch (Exception exception)
            {
                DisposeFolderTreeServiceCandidate(service, notificationSink);
                CompleteFolderTreeServiceCompositionFailure(initialization, exception);
            }
        }

        private static void DisposeFolderTreeServiceCandidate(
            IOutlookFolderTreeService service,
            OutlookFolderNotificationSink notificationSink
        )
        {
            try
            {
                service?.Dispose();
            }
            catch (Exception) { }

            try
            {
                notificationSink?.Dispose();
            }
            catch (Exception) { }
        }

        private void CompleteFolderTreeServiceCompositionFailure(
            TaskCompletionSource<IOutlookFolderTreeService> initialization,
            Exception exception
        )
        {
            var completed = false;
            lock (_folderTreeServiceGate)
            {
                if (ReferenceEquals(initialization, _folderTreeServiceInitialization))
                {
                    completed = initialization.TrySetException(exception);
                    _folderTreeServiceInitialization = null;
                    _folderTreeServiceDispatcher = null;
                    _folderTreeServiceCompositionStarted = false;
                    _folderTreeServiceCompositionThreadId = 0;
                }
            }

            if (completed)
            {
                NotifyFolderTreeServiceInitializationTerminal(initialization.Task);
            }
        }

        private void ObserveFolderTreeServiceDispatchTerminal(
            TaskCompletionSource<IOutlookFolderTreeService> initialization,
            Task dispatchTask
        )
        {
            if (!dispatchTask.IsCanceled && !dispatchTask.IsFaulted)
            {
                return;
            }

            try
            {
                dispatchTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException exception) when (dispatchTask.IsCanceled)
            {
                CompleteFolderTreeServiceCompositionCancellation(
                    initialization,
                    exception.CancellationToken
                );
            }
            catch (Exception exception) when (dispatchTask.IsFaulted)
            {
                CompleteFolderTreeServiceCompositionFailure(initialization, exception);
            }
        }

        private void CompleteFolderTreeServiceCompositionCancellation(
            TaskCompletionSource<IOutlookFolderTreeService> initialization,
            CancellationToken cancellationToken
        )
        {
            var completed = false;
            lock (_folderTreeServiceGate)
            {
                if (ReferenceEquals(initialization, _folderTreeServiceInitialization))
                {
                    completed = initialization.TrySetCanceled(cancellationToken);
                    _folderTreeServiceInitialization = null;
                    _folderTreeServiceDispatcher = null;
                    _folderTreeServiceCompositionStarted = false;
                    _folderTreeServiceCompositionThreadId = 0;
                }
            }

            if (completed)
            {
                NotifyFolderTreeServiceInitializationTerminal(initialization.Task);
            }
        }

        private void NotifyFolderTreeServiceInitializationTerminal(
            Task<IOutlookFolderTreeService> terminalInitialization
        )
        {
            try
            {
                OnFolderTreeServiceInitializationTerminal(terminalInitialization);
            }
            catch (Exception) { }
        }

        protected internal virtual IUiDispatcher CreateFolderTreeServiceDispatcher() =>
            new WpfUiDispatcher();

        protected internal virtual bool IsFolderTreeServiceDispatcherThread(
            IUiDispatcher dispatcher
        ) => dispatcher is WpfUiDispatcher && UiThread.Dispatcher.CheckAccess();

        protected internal virtual void OnFolderTreeServiceCompositionStarting() { }

        protected internal virtual void OnFolderTreeServiceBeforeInitializationCompletion(
            IOutlookFolderTreeService service
        ) { }

        protected internal virtual void OnFolderTreeServiceInitializationTerminal(
            Task<IOutlookFolderTreeService> terminalInitialization
        ) { }

        protected internal virtual IOutlookFolderTreeService LoadFolderTreeService(
            IUiDispatcher dispatcher,
            out OutlookFolderNotificationSink notificationSink
        )
        {
            var reader = new OutlookFolderHierarchyReader(NamespaceMAPI, StoresWrapper);
            var builder = new FolderTreeSnapshotBuilder(
                reader,
                new DeadlineClock(TimeSpan.FromMilliseconds(15)),
                new WpfDispatcherYield()
            );
            // why: issue #263. Hold the sink instance so the runtime rehook coordinator can reach
            // the SAME live sink (via FolderNotificationSink) to call AddStore; a separate instance
            // would subscribe COM events not wired to this tree service's cache-invalidation.
            notificationSink = new OutlookFolderNotificationSink(NamespaceMAPI);
            return new OutlookFolderTreeService(builder, notificationSink, dispatcher);
        }

        public void Dispose()
        {
            IOutlookFolderTreeService folderTreeService;
            TaskCompletionSource<IOutlookFolderTreeService> initialization;
            var initializationCompleted = false;
            lock (_folderTreeServiceGate)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                folderTreeService = _folderTreeService;
                _folderTreeService = null;
                initialization = _folderTreeServiceInitialization;
                _folderTreeServiceInitialization = null;
                _folderTreeServiceDispatcher = null;
                _folderTreeServiceCompositionStarted = false;
                _folderTreeServiceCompositionThreadId = 0;
                if (initialization is not null)
                {
                    initializationCompleted = initialization.TrySetException(
                        new ObjectDisposedException(nameof(AppOlObjects))
                    );
                }
            }

            if (initializationCompleted)
            {
                NotifyFolderTreeServiceInitializationTerminal(initialization.Task);
            }

            folderTreeService?.Dispose();
        }
    }
}
