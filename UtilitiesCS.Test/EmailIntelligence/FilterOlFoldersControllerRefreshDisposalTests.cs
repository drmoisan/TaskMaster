#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.OlFolderTools.FilterOlFolders;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public sealed partial class FilterOlFoldersControllerRefreshDisposalTests
    {
        [STATestMethod]
        public async Task CloseDuringRefresh_SuppressesViewMutationAndPropertyNotification()
        {
            var refreshSnapshot = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(
                Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot()),
                refreshSnapshot.Task
            );
            var viewer = new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer();
            var controller = new RefreshTrackingFilterOlFoldersController(
                FilterOlFoldersControllerInitializationTests.CreateGlobals(service).Object,
                viewer
            );

            await controller.Readiness;
            viewer.InvokeRequired = true;
            service.RaiseSnapshotChanged();
            await service.RefreshRequested;
            var refreshOperation = controller.LastAsyncOperation;
            var capturedHandler = service.CapturedSnapshotChangedHandler;

            viewer.Close();
            service.RemovedSnapshotChangedHandler.Should().Be(capturedHandler);
            service.SnapshotChangedHandlerCount.Should().Be(0);
            service.RaiseSnapshotChanged();
            service.SnapshotRequestCount.Should().Be(2);
            service.InvokeCapturedSnapshotChangedHandler();
            var lateCallbackOperation = controller.LastAsyncOperation;
            refreshSnapshot.SetResult(
                FilterOlFoldersControllerInitializationTests.CreateSnapshot()
            );
            await refreshOperation;
            await lateCallbackOperation;

            controller.FolderTreeView.Should().BeNull();
            viewer.InvokeCount.Should().Be(0);
            controller.RefreshViewAppliedCount.Should().Be(0);
            controller.RefreshFault.Should().BeNull();
            service.SnapshotRequestCount.Should().Be(2);
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        [TestMethod]
        public async Task CloseDuringRefresh_OnPumpingSta_AwaitsEveryQueuedControllerOperation()
        {
            var host = await PumpingStaHost.CreateAsync();
            try
            {
                var refreshSnapshot = new TaskCompletionSource<FolderTreeSnapshot>();
                var service = new DelayedFolderTreeService(
                    Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot()),
                    refreshSnapshot.Task
                );
                RefreshTrackingFilterOlFoldersController? controller = null;
                FilterOlFoldersControllerInitializationTests.RecordingFilterViewer? viewer = null;

                await host
                    .Dispatcher.InvokeAsync(() =>
                    {
                        viewer =
                            new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer();
                        controller = new RefreshTrackingFilterOlFoldersController(
                            FilterOlFoldersControllerInitializationTests
                                .CreateGlobals(service)
                                .Object,
                            viewer
                        );
                    })
                    .Task;
                await controller!.Readiness;

                service.RaiseSnapshotChanged();
                await service.RefreshRequested;
                var refreshOperation = controller.LastAsyncOperation;
                await host.Dispatcher.InvokeAsync(viewer!.Close).Task;
                service.InvokeCapturedSnapshotChangedHandler();
                var lateCallbackOperation = controller.LastAsyncOperation;
                refreshSnapshot.SetResult(
                    FilterOlFoldersControllerInitializationTests.CreateSnapshot()
                );
                await refreshOperation;
                await lateCallbackOperation;

                var pumpProbe = new TaskCompletionSource<int>();
                await host
                    .Dispatcher.InvokeAsync(() =>
                        pumpProbe.TrySetResult(Thread.CurrentThread.ManagedThreadId)
                    )
                    .Task;
                (await pumpProbe.Task).Should().Be(host.ThreadId);
                controller.RefreshViewAppliedCount.Should().Be(0);
                controller.RefreshFault.Should().BeNull();
                service.SnapshotChangedHandlerCount.Should().Be(0);
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task CreateAsync_NullGlobals_DisposesFactoryViewer()
        {
            FilterOlFoldersControllerInitializationTests.RecordingFilterViewer? viewer = null;
            var factoryCalls = 0;
            Func<Task> create = async () =>
                await FilterOlFoldersController.CreateAsync(
                    null!,
                    () =>
                    {
                        factoryCalls++;
                        viewer =
                            new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer();
                        viewer.Show();
                        return viewer;
                    }
                );

            var exception = (await create.Should().ThrowAsync<ArgumentNullException>()).Which;
            exception.ParamName.Should().Be("appGlobals");
            exception.InnerException.Should().BeNull();
            using (new AssertionScope())
            {
                factoryCalls.Should().Be(1);
                viewer.Should().NotBeNull();
                viewer!.ShowCount.Should().Be(1);
                viewer.CloseCount.Should().Be(1);
                viewer.DisposeCount.Should().Be(1);
            }
        }

        [TestMethod]
        public async Task CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal()
        {
            FilterOlFoldersControllerInitializationTests.RecordingFilterViewer? viewer = null;
            var subscriptionFault = new InvalidOperationException(
                "controlled FormClosed add failure"
            );
            var service = new DelayedFolderTreeService(
                Task.FromResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot())
            );
            var serviceAcquired = false;
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(value => value.FolderTreeService)
                .Returns(() =>
                {
                    serviceAcquired = true;
                    return service;
                });
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(value => value.Ol).Returns(ol.Object);
            var factoryCalls = 0;
            Func<Task> create = async () =>
                await FilterOlFoldersController.CreateAsync(
                    globals.Object,
                    () =>
                    {
                        factoryCalls++;
                        viewer =
                            new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer
                            {
                                FormClosedSubscriptionException = subscriptionFault,
                                FormClosedSubscriptionAttempt = () =>
                                {
                                    serviceAcquired.Should().BeTrue();
                                    throw subscriptionFault;
                                },
                            };
                        viewer.Show();
                        return viewer;
                    }
                );

            (await create.Should().ThrowAsync<InvalidOperationException>())
                .Which.Should()
                .BeSameAs(subscriptionFault);
            using (new AssertionScope())
            {
                factoryCalls.Should().Be(1);
                viewer.Should().NotBeNull();
                viewer!.ShowCount.Should().Be(1);
                viewer.CloseCount.Should().Be(1);
                viewer.DisposeCount.Should().Be(1);
                service.SnapshotChangedHandlerCount.Should().Be(0);
            }
        }

        [TestMethod]
        public async Task InitialArchiveRootClose_BeforeCompatibilityView_DoesNotCommit()
        {
            var snapshot = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(snapshot.Task);
            var viewer = new FilterOlFoldersControllerInitializationTests.RecordingFilterViewer();
            var archiveRootReads = 0;
            var controller = new RefreshTrackingFilterOlFoldersController(
                FilterOlFoldersControllerInitializationTests
                    .CreateGlobals(
                        service,
                        () =>
                        {
                            if (Interlocked.Increment(ref archiveRootReads) == 2)
                            {
                                viewer.Close();
                            }
                        }
                    )
                    .Object,
                viewer
            );
            snapshot.SetResult(FilterOlFoldersControllerInitializationTests.CreateSnapshot());
            await controller.Readiness;

            controller.FolderTreeView.Should().BeNull();
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        private sealed class RefreshTrackingFilterOlFoldersController : FilterOlFoldersController
        {
            private RecordingInlineUiDispatcher? _inlineDispatcher;
            private PumpingStaUiDispatcher? _pumpingStaDispatcher;

            internal RefreshTrackingFilterOlFoldersController(
                IApplicationGlobals globals,
                IFilterOlFoldersViewer viewer
            )
                : base(globals, viewer) { }

            internal int RefreshViewAppliedCount { get; private set; }

            internal Exception? RefreshFault { get; private set; }

            internal Task LastAsyncOperation =>
                _pumpingStaDispatcher?.LastAsyncOperation ?? Dispatcher.LastAsyncOperation;

            private RecordingInlineUiDispatcher Dispatcher =>
                _inlineDispatcher ??= new RecordingInlineUiDispatcher();

            protected internal override IUiDispatcher CreateFolderTreeUiDispatcher()
            {
                var dispatcher = System.Windows.Threading.Dispatcher.FromThread(
                    Thread.CurrentThread
                );
                return dispatcher is null
                    ? Dispatcher
                    : _pumpingStaDispatcher ??= new PumpingStaUiDispatcher(dispatcher);
            }

            protected internal override void OnFolderTreeRefreshViewApplied() =>
                RefreshViewAppliedCount++;

            protected internal override void ObserveFolderTreeRefreshFault(Exception exception) =>
                RefreshFault = exception;
        }

        private sealed class PumpingStaUiDispatcher : IUiDispatcher
        {
            private readonly System.Windows.Threading.Dispatcher _dispatcher;

            internal PumpingStaUiDispatcher(System.Windows.Threading.Dispatcher dispatcher) =>
                _dispatcher = dispatcher;

            internal Task LastAsyncOperation { get; private set; } = Task.CompletedTask;

            public void Invoke(Action action) => _dispatcher.Invoke(action);

            public Task InvokeAsync(Action action) => _dispatcher.InvokeAsync(action).Task;

            public Task InvokeAsync(
                Action action,
                DispatcherPriority priority,
                CancellationToken token
            ) => _dispatcher.InvokeAsync(action, priority, token).Task;

            public IAsyncResult BeginInvoke(Action action) => _dispatcher.BeginInvoke(action).Task;

            public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
                _dispatcher.InvokeAsync(func).Task;

            public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func)
            {
                var operation = _dispatcher.InvokeAsync(func).Task.Unwrap();
                LastAsyncOperation = operation;
                return operation;
            }
        }

        private sealed class PumpingStaHost
        {
            private readonly TaskCompletionSource<bool> _ready = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly TaskCompletionSource<bool> _stopped = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly Thread _thread;

            private PumpingStaHost()
            {
                _thread = new Thread(() =>
                {
                    try
                    {
                        Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                        ThreadId = Thread.CurrentThread.ManagedThreadId;
                        _ready.TrySetResult(true);
                        System.Windows.Threading.Dispatcher.Run();
                    }
                    catch (Exception exception)
                    {
                        _ready.TrySetException(exception);
                    }
                    finally
                    {
                        _stopped.TrySetResult(true);
                    }
                });
                _thread.SetApartmentState(ApartmentState.STA);
            }

            internal static async Task<PumpingStaHost> CreateAsync()
            {
                var host = new PumpingStaHost();
                host._thread.Start();
                await host._ready.Task.ConfigureAwait(false);
                return host;
            }

            internal System.Windows.Threading.Dispatcher Dispatcher { get; private set; } = null!;

            internal int ThreadId { get; private set; }

            internal async Task StopAsync()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                await _stopped.Task.ConfigureAwait(false);
                _thread.Join();
                if (_thread.IsAlive)
                {
                    throw new InvalidOperationException(
                        "The pumping STA thread did not terminate."
                    );
                }
            }
        }

        internal sealed class RecordingInlineUiDispatcher : IUiDispatcher
        {
            internal Task LastAsyncOperation { get; private set; } = Task.CompletedTask;

            public void Invoke(Action action) => action();

            public Task InvokeAsync(Action action)
            {
                action();
                return Task.CompletedTask;
            }

            public Task InvokeAsync(
                Action action,
                DispatcherPriority priority,
                CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();
                action();
                return Task.CompletedTask;
            }

            public IAsyncResult BeginInvoke(Action action)
            {
                action();
                return Task.CompletedTask;
            }

            public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
                Task.FromResult(func());

            public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func)
            {
                var operation = func();
                LastAsyncOperation = operation;
                return operation;
            }
        }
    }

    internal sealed class DelayedFolderTreeService : IOutlookFolderTreeService
    {
        private readonly Task<FolderTreeSnapshot> _initialSnapshot;
        private readonly Task<FolderTreeSnapshot>? _refreshSnapshot;
        private readonly TaskCompletionSource<bool> _refreshRequested = new();
        private EventHandler<FolderTreeSnapshotChangedEventArgs>? _snapshotChanged;
        private int _snapshotRequestCount;

        internal DelayedFolderTreeService(Task<FolderTreeSnapshot> initialSnapshot)
        {
            _initialSnapshot = initialSnapshot;
        }

        internal DelayedFolderTreeService(
            Task<FolderTreeSnapshot> initialSnapshot,
            Task<FolderTreeSnapshot> refreshSnapshot
        )
        {
            _initialSnapshot = initialSnapshot;
            _refreshSnapshot = refreshSnapshot;
        }

        internal Task RefreshRequested => _refreshRequested.Task;

        internal int SnapshotChangedHandlerCount { get; private set; }

        internal int SnapshotRequestCount => _snapshotRequestCount;

        internal EventHandler<FolderTreeSnapshotChangedEventArgs>? CapturedSnapshotChangedHandler
        {
            get;
            private set;
        }

        internal EventHandler<FolderTreeSnapshotChangedEventArgs>? RemovedSnapshotChangedHandler
        {
            get;
            private set;
        }

        public event EventHandler<FolderTreeSnapshotChangedEventArgs>? SnapshotChanged
        {
            add
            {
                _snapshotChanged += value;
                CapturedSnapshotChangedHandler = value;
                SnapshotChangedHandlerCount++;
            }
            remove
            {
                _snapshotChanged -= value;
                RemovedSnapshotChangedHandler = value;
                SnapshotChangedHandlerCount--;
            }
        }

        public Task<FolderTreeSnapshot> GetSnapshotAsync(
            FolderTreeRequest request,
            CancellationToken cancellationToken
        )
        {
            if (Interlocked.Increment(ref _snapshotRequestCount) == 1)
            {
                return _initialSnapshot;
            }

            _refreshRequested.TrySetResult(true);
            return _refreshSnapshot ?? _initialSnapshot;
        }

        internal void RaiseSnapshotChanged() => InvokeSnapshotChangedHandler(_snapshotChanged);

        internal void InvokeCapturedSnapshotChangedHandler() =>
            InvokeSnapshotChangedHandler(CapturedSnapshotChangedHandler);

        public void MarkStale(string storeId, FolderTreeRefreshReason reason) { }

        public void Dispose() { }

        private void InvokeSnapshotChangedHandler(
            EventHandler<FolderTreeSnapshotChangedEventArgs>? handler
        ) =>
            handler?.Invoke(
                this,
                new FolderTreeSnapshotChangedEventArgs(
                    FilterOlFoldersControllerInitializationTests.CreateSnapshot(),
                    FolderTreeRefreshReason.ManualRefresh,
                    null
                )
            );
    }
}
