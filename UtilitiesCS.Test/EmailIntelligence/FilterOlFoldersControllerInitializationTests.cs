using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class FilterOlFoldersControllerInitializationTests
    {
        [STATestMethod]
        public async Task CreateAsync_WiresViewerOnlyAfterSnapshotCompletes()
        {
            var snapshotSource = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(snapshotSource.Task);
            var viewer = new RecordingFilterViewer();
            var initialization = FilterOlFoldersController.CreateAsync(
                CreateGlobals(service).Object,
                () => viewer,
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );

            viewer.Controller.Should().BeNull();

            snapshotSource.SetResult(CreateSnapshot());
            await initialization;

            viewer.Controller.Should().NotBeNull();
        }

        [STATestMethod]
        public async Task CreateAsync_ClosedBeforeSnapshotCompletes_DoesNotWireViewerOrRetainHandler()
        {
            var snapshotSource = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(snapshotSource.Task);
            var viewer = new RecordingFilterViewer();
            var initialization = FilterOlFoldersController.CreateAsync(
                CreateGlobals(service).Object,
                () => viewer,
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );
            viewer.Close();
            snapshotSource.SetResult(CreateSnapshot());

            await initialization;

            viewer.Controller.Should().BeNull();
            viewer.CloseCount.Should().Be(1);
            viewer.DisposeCount.Should().Be(1);
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        [STATestMethod]
        public async Task CreateAsync_SnapshotFault_PropagatesAndLeavesViewerUnwired()
        {
            var snapshotSource = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(snapshotSource.Task);
            var viewer = new RecordingFilterViewer();
            var initialization = FilterOlFoldersController.CreateAsync(
                CreateGlobals(service).Object,
                () => viewer,
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );
            snapshotSource.SetException(
                new InvalidOperationException("controlled snapshot failure")
            );
            Func<Task> awaitInitialization = async () => await initialization;

            await awaitInitialization
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("controlled snapshot failure");
            viewer.Controller.Should().BeNull();
            viewer.CloseCount.Should().Be(1);
            viewer.DisposeCount.Should().Be(1);
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        [STATestMethod]
        public async Task InjectedViewerConstructor_Readiness_PropagatesSnapshotFaultAndLeavesViewerUnwired()
        {
            var snapshotSource = new TaskCompletionSource<FolderTreeSnapshot>();
            var service = new DelayedFolderTreeService(snapshotSource.Task);
            var viewer = new RecordingFilterViewer();

            typeof(FilterOlFoldersController)
                .GetConstructor(new[] { typeof(IApplicationGlobals) })
                .Should()
                .NotBeNull("the public legacy construction contract must remain available");
            typeof(FilterOlFoldersController)
                .GetProperty(nameof(FilterOlFoldersController.Readiness))
                .Should()
                .NotBeNull("the public readiness contract must remain available");

            var controller = new FilterOlFoldersController(
                CreateGlobals(service).Object,
                viewer,
                new FilterOlFoldersControllerRefreshDisposalTests.RecordingInlineUiDispatcher()
            );
            snapshotSource.SetException(
                new InvalidOperationException("controlled injected-viewer snapshot failure")
            );
            Func<Task> awaitReadiness = async () => await controller.Readiness;

            await awaitReadiness
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("controlled injected-viewer snapshot failure");
            viewer.Controller.Should().BeNull();
            service.SnapshotChangedHandlerCount.Should().Be(0);
        }

        [STATestMethod]
        public async Task CreateAsync_SynchronousFolderTreeServiceFault_ClosesFactoryViewerAndPreservesOriginalException()
        {
            var originalException = new InvalidOperationException(
                "controlled synchronous folder-tree-service failure"
            );
            var viewer = new RecordingFilterViewer();
            var initialization = FilterOlFoldersController.CreateAsync(
                CreateGlobalsWithFolderTreeServiceFailure(originalException).Object,
                () => viewer
            );
            Func<Task> awaitInitialization = async () => await initialization;

            (await awaitInitialization.Should().ThrowAsync<InvalidOperationException>())
                .Which.Should()
                .BeSameAs(originalException);
            viewer.CloseCount.Should().Be(1);
            viewer.Controller.Should().BeNull();
        }

        [STATestMethod]
        public void InjectedViewerConstructor_SynchronousFolderTreeServiceFault_ClosesViewerAndPreservesOriginalException()
        {
            var originalException = new InvalidOperationException(
                "controlled synchronous folder-tree-service failure"
            );
            var viewer = new RecordingFilterViewer();
            Action constructController = () =>
                _ = new FilterOlFoldersController(
                    CreateGlobalsWithFolderTreeServiceFailure(originalException).Object,
                    viewer
                );

            constructController
                .Should()
                .Throw<InvalidOperationException>()
                .Which.Should()
                .BeSameAs(originalException);
            viewer.CloseCount.Should().Be(1);
            viewer.Controller.Should().BeNull();
        }

        private static Mock<IApplicationGlobals> CreateGlobalsWithFolderTreeServiceFailure(
            InvalidOperationException exception
        )
        {
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.FolderTreeService).Throws(exception);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            return globals;
        }

        [TestMethod]
        public async Task SnapshotChanged_FromWorker_RefreshesOnCapturedStaAndObservesOriginalFault()
        {
            using (var dispatcherHost = new StaDispatcherHost())
            {
                var archiveRootReadThreadIds = new List<int>();
                var service = new WorkerNotificationFolderTreeService(CreateSnapshot());
                RecordingFilterViewer viewer = null;
                RefreshObservingFilterOlFoldersController controller = null;
                await dispatcherHost
                    .Dispatcher.InvokeAsync(() =>
                    {
                        viewer = new RecordingFilterViewer();
                        controller = new RefreshObservingFilterOlFoldersController(
                            CreateGlobals(
                                service,
                                () =>
                                    archiveRootReadThreadIds.Add(
                                        Thread.CurrentThread.ManagedThreadId
                                    )
                            ).Object,
                            viewer
                        );
                    })
                    .Task;
                await controller.Readiness;
                var staThreadId = dispatcherHost.Dispatcher.Thread.ManagedThreadId;

                var workerThreadId = await Task.Run(() =>
                {
                    service.RaiseSnapshotChanged();
                    return Thread.CurrentThread.ManagedThreadId;
                });
                (await controller.RefreshViewApplied).Should().Be(staThreadId);
                workerThreadId.Should().NotBe(staThreadId);
                service.SnapshotRequestThreadIds.Should().OnlyContain(x => x == staThreadId);
                archiveRootReadThreadIds.Should().OnlyContain(x => x == staThreadId);

                var controlledRefreshFault = new InvalidOperationException(
                    "controlled worker refresh failure"
                );
                await Task.Run(service.RaiseSnapshotChanged);
                await service.FaultRefreshRequested.Task;
                service.CompleteFault(controlledRefreshFault);
                (await controller.RefreshFault).Should().BeSameAs(controlledRefreshFault);

                await dispatcherHost.Dispatcher.InvokeAsync(viewer.Close).Task;
                service.SnapshotChangedHandlerCount.Should().Be(0);
            }
        }

        private sealed class RefreshObservingFilterOlFoldersController : FilterOlFoldersController
        {
            private readonly TaskCompletionSource<Exception> _refreshFault = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly TaskCompletionSource<int> _refreshViewApplied = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            internal RefreshObservingFilterOlFoldersController(
                IApplicationGlobals globals,
                IFilterOlFoldersViewer viewer
            )
                : base(globals, viewer) { }

            internal Task<Exception> RefreshFault => _refreshFault.Task;

            internal Task<int> RefreshViewApplied => _refreshViewApplied.Task;

            protected internal override IUiDispatcher CreateFolderTreeUiDispatcher() =>
                new CapturedStaUiDispatcher(System.Windows.Threading.Dispatcher.CurrentDispatcher);

            protected internal override void ObserveFolderTreeRefreshFault(Exception exception) =>
                _refreshFault.TrySetResult(exception);

            protected internal override void OnFolderTreeRefreshViewApplied() =>
                _refreshViewApplied.TrySetResult(Thread.CurrentThread.ManagedThreadId);
        }

        private sealed class WorkerNotificationFolderTreeService : IOutlookFolderTreeService
        {
            private readonly TaskCompletionSource<FolderTreeSnapshot> _faultSnapshot = new(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            private readonly FolderTreeSnapshot _snapshot;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _snapshotChanged;
            private int _requestCount;

            internal WorkerNotificationFolderTreeService(FolderTreeSnapshot snapshot) =>
                _snapshot = snapshot;

            internal TaskCompletionSource<bool> FaultRefreshRequested { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            internal List<int> SnapshotRequestThreadIds { get; } = new List<int>();

            internal int SnapshotChangedHandlerCount { get; private set; }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> SnapshotChanged
            {
                add
                {
                    _snapshotChanged += value;
                    SnapshotChangedHandlerCount++;
                }
                remove
                {
                    _snapshotChanged -= value;
                    SnapshotChangedHandlerCount--;
                }
            }

            public Task<FolderTreeSnapshot> GetSnapshotAsync(
                FolderTreeRequest request,
                CancellationToken cancellationToken
            )
            {
                SnapshotRequestThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                if (Interlocked.Increment(ref _requestCount) < 3)
                {
                    return Task.FromResult(_snapshot);
                }

                FaultRefreshRequested.TrySetResult(true);
                return _faultSnapshot.Task;
            }

            internal void CompleteFault(Exception exception) =>
                _faultSnapshot.TrySetException(exception);

            internal void RaiseSnapshotChanged() =>
                _snapshotChanged?.Invoke(
                    this,
                    new FolderTreeSnapshotChangedEventArgs(
                        _snapshot,
                        FolderTreeRefreshReason.ManualRefresh,
                        null
                    )
                );

            public void MarkStale(string storeId, FolderTreeRefreshReason reason) { }

            public void Dispose() { }
        }

        private sealed class CapturedStaUiDispatcher : IUiDispatcher
        {
            private readonly System.Windows.Threading.Dispatcher _dispatcher;

            internal CapturedStaUiDispatcher(System.Windows.Threading.Dispatcher dispatcher) =>
                _dispatcher = dispatcher;

            public void Invoke(Action action) => _dispatcher.Invoke(action);

            public Task InvokeAsync(Action action) => _dispatcher.InvokeAsync(action).Task;

            public Task InvokeAsync(
                Action action,
                System.Windows.Threading.DispatcherPriority priority,
                CancellationToken token
            ) => _dispatcher.InvokeAsync(action, priority, token).Task;

            public IAsyncResult BeginInvoke(Action action) => _dispatcher.BeginInvoke(action).Task;

            public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
                _dispatcher.InvokeAsync(func).Task;

            public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) =>
                _dispatcher.InvokeAsync(func).Task.Unwrap();
        }

        private sealed class StaDispatcherHost : IDisposable
        {
            private readonly AutoResetEvent _ready = new AutoResetEvent(false);
            private readonly Thread _thread;

            internal StaDispatcherHost()
            {
                _thread = new Thread(() =>
                {
                    Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                    _ready.Set();
                    System.Windows.Threading.Dispatcher.Run();
                });
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                _ready.WaitOne();
            }

            internal System.Windows.Threading.Dispatcher Dispatcher { get; private set; }

            public void Dispose()
            {
                Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }

        internal static Mock<IApplicationGlobals> CreateGlobals(
            IOutlookFolderTreeService service,
            Action archiveRootRead = null
        )
        {
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            ol.SetupGet(x => x.ArchiveRoot)
                .Returns(() =>
                {
                    archiveRootRead?.Invoke();
                    return null;
                });
            ol.SetupGet(x => x.FolderTreeService).Returns(service);
            var toDo = new Mock<IToDoObjects>(MockBehavior.Strict);
            toDo.SetupGet(x => x.FilteredFolderScraping)
                .Returns(new ScoDictionaryNew<string, int>());
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            globals.SetupGet(x => x.TD).Returns(toDo.Object);
            return globals;
        }

        internal static FolderTreeSnapshot CreateSnapshot()
        {
            var key = new FolderTreeNodeKey("store", "root", "\\Root");
            return new FolderTreeSnapshot(
                new[] { key },
                new[]
                {
                    new FolderTreeSnapshotNode(
                        key,
                        "Root",
                        "store",
                        "root",
                        null,
                        "\\Root",
                        "Root",
                        Array.Empty<FolderTreeNodeKey>(),
                        false,
                        string.Empty
                    ),
                }
            );
        }

        internal sealed class RecordingFilterViewer : IFilterOlFoldersViewer
        {
            private FormClosedEventHandler _formClosed;

            public event FormClosedEventHandler FormClosed
            {
                add
                {
                    FormClosedSubscriptionAttempt?.Invoke();
                    if (FormClosedSubscriptionException != null)
                    {
                        throw FormClosedSubscriptionException;
                    }

                    _formClosed += value;
                }
                remove => _formClosed -= value;
            }

            public TreeListView TlvNotFiltered { get; } = new TreeListView();

            public TreeListView TlvFiltered { get; } = new TreeListView();

            public bool InvokeRequired { get; set; }

            public FilterOlFoldersController Controller { get; private set; }

            public int CloseCount { get; private set; }

            public int DisposeCount { get; private set; }

            public int InvokeCount { get; private set; }

            public int ShowCount { get; private set; }

            public Exception FormClosedSubscriptionException { get; set; }

            public Action FormClosedSubscriptionAttempt { get; set; }

            public Exception CloseException { get; set; }

            public void SetController(FilterOlFoldersController controller) =>
                Controller = controller;

            public void Show() => ShowCount++;

            public void Close()
            {
                CloseCount++;
                if (CloseException != null)
                {
                    throw CloseException;
                }

                _formClosed?.Invoke(this, new FormClosedEventArgs(CloseReason.None));
                Dispose();
            }

            public object Invoke(Delegate method)
            {
                InvokeCount++;
                return method.DynamicInvoke();
            }

            public void Dispose()
            {
                DisposeCount++;
                TlvNotFiltered.Dispose();
                TlvFiltered.Dispose();
            }
        }
    }
}
