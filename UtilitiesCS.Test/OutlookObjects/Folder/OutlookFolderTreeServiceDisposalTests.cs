using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderTreeServiceDisposalTests
    {
        [TestMethod]
        public async Task Dispose_UnsubscribesNotificationsAndSuppressesLaterEvents()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddRecord(
                new FakeFolderHierarchyRecord("store-a", "entry-a", "", "Inbox", "\\Inbox", "Inbox")
            );
            var sink = new RecordingNotificationSink();
            var service = new OutlookFolderTreeService(new FolderTreeSnapshotBuilder(reader), sink);
            var snapshotChanged = 0;
            service.SnapshotChanged += (_, __) => snapshotChanged++;
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            snapshotChanged = 0;
            var capturedHandler = sink.CaptureFolderChangedHandler();

            service.Dispose();
            service.Dispose();
            capturedHandler.Invoke(
                sink,
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );

            sink.FolderChangedHandlerCount.Should().Be(0);
            sink.DisposeCount.Should().Be(1);
            reader.EnumerationCount.Should().Be(1);
            snapshotChanged.Should().Be(0);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_AfterDispose_Throws()
        {
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(new FakeOutlookFolderHierarchyReader()),
                new FakeOutlookFolderNotificationSink()
            );
            service.Dispose();

            Func<Task> act = () =>
                service.GetSnapshotAsync(
                    FolderTreeRequest.AllStores(false),
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<ObjectDisposedException>();
        }

        [TestMethod]
        public async Task Dispose_DuringBuild_LeavesDisposedWithoutPublicationOrNotification()
        {
            var reader = new BlockingReader();
            var sink = new FakeOutlookFolderNotificationSink();
            var service = new OutlookFolderTreeService(new FolderTreeSnapshotBuilder(reader), sink);
            var snapshotChanged = 0;
            service.SnapshotChanged += (_, __) => snapshotChanged++;

            var build = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            reader.Started.Task.IsCompleted.Should().BeTrue();

            service.Dispose();
            reader.Complete(CreateSnapshot());

            Func<Task> act = async () => await build;
            await act.Should().ThrowAsync<ObjectDisposedException>();
            service.State.Should().Be(OutlookFolderTreeServiceState.Disposed);
            snapshotChanged.Should().Be(0);
            sink.FolderChangedHandlerCount.Should().Be(0);
        }

        [TestMethod]
        public async Task Dispose_DuringRefresh_LeavesDisposedWithoutPublicationOrNotification()
        {
            var reader = new RefreshBlockingReader();
            var sink = new FakeOutlookFolderNotificationSink();
            var service = new OutlookFolderTreeService(new FolderTreeSnapshotBuilder(reader), sink);
            var snapshotChanged = 0;
            service.SnapshotChanged += (_, __) => snapshotChanged++;

            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            snapshotChanged = 0;
            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );
            await reader.RefreshStarted.Task;
            var refresh = service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-a", false),
                CancellationToken.None
            );

            service.Dispose();
            reader.Complete(CreateSnapshot());

            Func<Task> awaitRefresh = async () => await refresh;
            await awaitRefresh.Should().ThrowAsync<ObjectDisposedException>();
            service.State.Should().Be(OutlookFolderTreeServiceState.Disposed);
            snapshotChanged.Should().Be(0);
            sink.FolderChangedHandlerCount.Should().Be(0);
        }

        [TestMethod]
        public async Task NotificationRefreshAndDispose_RunOnTheCapturedDispatcher()
        {
            using (var dispatcherHost = new StaDispatcherHost())
            {
                var dispatcher = new DispatcherUiDispatcher(dispatcherHost.Dispatcher);
                var reader = new RecordingReader();
                var sink = new RecordingNotificationSink();
                var service = await dispatcher.InvokeAsync(() =>
                    new OutlookFolderTreeService(
                        new FolderTreeSnapshotBuilder(reader),
                        sink,
                        dispatcher
                    )
                );

                await service.GetSnapshotAsync(
                    FolderTreeRequest.AllStores(false),
                    CancellationToken.None
                );
                await dispatcher.InvokeAsync(() =>
                    sink.RaiseFolderChanged(
                        FakeOutlookFolderNotificationSink.CreateArgs(
                            FolderTreeRefreshReason.FolderChanged,
                            "store-a"
                        )
                    )
                );
                await service.GetSnapshotAsync(
                    FolderTreeRequest.ForStore("store-a", false),
                    CancellationToken.None
                );

                await Task.Run(service.Dispose);

                reader.AccessThreadIds.Should().OnlyContain(id => id == dispatcherHost.ThreadId);
                sink.SubscriptionAndCleanupThreadIds.Should()
                    .OnlyContain(id => id == dispatcherHost.ThreadId);
                sink.FolderChangedHandlerCount.Should().Be(0);
                sink.DisposeCount.Should().Be(1);
            }
        }

        private static FolderTreeSnapshot CreateSnapshot()
        {
            var key = new FolderTreeNodeKey("store-a", "entry-a", "\\Inbox");
            return new FolderTreeSnapshot(
                new[] { key },
                new[]
                {
                    new FolderTreeSnapshotNode(
                        key,
                        "Inbox",
                        "store-a",
                        "entry-a",
                        null,
                        "\\Inbox",
                        "Inbox",
                        Array.Empty<FolderTreeNodeKey>(),
                        false,
                        string.Empty
                    ),
                }
            );
        }

        private sealed class BlockingReader : IOutlookFolderHierarchyReader
        {
            private readonly TaskCompletionSource<IReadOnlyList<FolderTreeSnapshotNode>> _nodes =
                new TaskCompletionSource<IReadOnlyList<FolderTreeSnapshotNode>>();

            public TaskCompletionSource<bool> Started { get; } = new TaskCompletionSource<bool>();

            public Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
                FolderTreeRequest request,
                IDeadlineClock deadlineClock,
                IDispatcherYield dispatcherYield,
                CancellationToken cancellationToken
            )
            {
                Started.TrySetResult(true);
                return _nodes.Task;
            }

            public void Complete(FolderTreeSnapshot snapshot) =>
                _nodes.TrySetResult(snapshot.NodesByKey.Values.ToArray());
        }

        private sealed class RefreshBlockingReader : IOutlookFolderHierarchyReader
        {
            private readonly TaskCompletionSource<IReadOnlyList<FolderTreeSnapshotNode>> _refresh =
                new TaskCompletionSource<IReadOnlyList<FolderTreeSnapshotNode>>();
            private int _readCount;

            internal TaskCompletionSource<bool> RefreshStarted { get; } =
                new TaskCompletionSource<bool>();

            public Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
                FolderTreeRequest request,
                IDeadlineClock deadlineClock,
                IDispatcherYield dispatcherYield,
                CancellationToken cancellationToken
            )
            {
                if (Interlocked.Increment(ref _readCount) == 1)
                {
                    return Task.FromResult<IReadOnlyList<FolderTreeSnapshotNode>>(
                        CreateSnapshot().NodesByKey.Values.ToArray()
                    );
                }

                RefreshStarted.TrySetResult(true);
                return _refresh.Task;
            }

            internal void Complete(FolderTreeSnapshot snapshot) =>
                _refresh.TrySetResult(snapshot.NodesByKey.Values.ToArray());
        }

        private sealed class RecordingReader : IOutlookFolderHierarchyReader
        {
            private readonly IReadOnlyList<FolderTreeSnapshotNode> _nodes = new[]
            {
                new FolderTreeSnapshotNode(
                    new FolderTreeNodeKey("store-a", "entry-a", "\\Inbox"),
                    "Inbox",
                    "store-a",
                    "entry-a",
                    null,
                    "\\Inbox",
                    "Inbox",
                    Array.Empty<FolderTreeNodeKey>(),
                    false,
                    string.Empty
                ),
            };

            internal List<int> AccessThreadIds { get; } = new List<int>();

            public Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
                FolderTreeRequest request,
                IDeadlineClock deadlineClock,
                IDispatcherYield dispatcherYield,
                CancellationToken cancellationToken
            )
            {
                AccessThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                return Task.FromResult(_nodes);
            }
        }

        private sealed class RecordingNotificationSink : IOutlookFolderNotificationSink
        {
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderAdded;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderRemoved;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderChanged;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _storeAdded;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _storeRemoved;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _disposed;

            internal List<int> SubscriptionAndCleanupThreadIds { get; } = new List<int>();

            internal int FolderChangedHandlerCount { get; private set; }

            internal int DisposeCount { get; private set; }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderAdded
            {
                add => Subscribe(ref _folderAdded, value);
                remove => Unsubscribe(ref _folderAdded, value);
            }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderRemoved
            {
                add => Subscribe(ref _folderRemoved, value);
                remove => Unsubscribe(ref _folderRemoved, value);
            }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderChanged
            {
                add
                {
                    Subscribe(ref _folderChanged, value);
                    FolderChangedHandlerCount++;
                }
                remove
                {
                    Unsubscribe(ref _folderChanged, value);
                    FolderChangedHandlerCount--;
                }
            }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreAdded
            {
                add => Subscribe(ref _storeAdded, value);
                remove => Unsubscribe(ref _storeAdded, value);
            }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreRemoved
            {
                add => Subscribe(ref _storeRemoved, value);
                remove => Unsubscribe(ref _storeRemoved, value);
            }

            public event EventHandler<FolderTreeSnapshotChangedEventArgs> Disposed
            {
                add => Subscribe(ref _disposed, value);
                remove => Unsubscribe(ref _disposed, value);
            }

            public void Start()
            {
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
            }

            public void AddStore(Microsoft.Office.Interop.Outlook.Store store) { }

            public void RemoveStore(string storeId) { }

            public void Dispose()
            {
                DisposeCount++;
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                _disposed?.Invoke(
                    this,
                    FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.Disposal)
                );
            }

            internal void RaiseFolderChanged(FolderTreeSnapshotChangedEventArgs args) =>
                _folderChanged?.Invoke(this, args);

            internal EventHandler<FolderTreeSnapshotChangedEventArgs> CaptureFolderChangedHandler() =>
                _folderChanged;

            private void Subscribe(
                ref EventHandler<FolderTreeSnapshotChangedEventArgs> handlers,
                EventHandler<FolderTreeSnapshotChangedEventArgs> handler
            )
            {
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                handlers += handler;
            }

            private void Unsubscribe(
                ref EventHandler<FolderTreeSnapshotChangedEventArgs> handlers,
                EventHandler<FolderTreeSnapshotChangedEventArgs> handler
            )
            {
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                handlers -= handler;
            }
        }

        private sealed class DispatcherUiDispatcher : IUiDispatcher
        {
            private readonly Dispatcher _dispatcher;

            internal DispatcherUiDispatcher(Dispatcher dispatcher) => _dispatcher = dispatcher;

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
                    ThreadId = Thread.CurrentThread.ManagedThreadId;
                    _ready.Set();
                    System.Windows.Threading.Dispatcher.Run();
                });
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                _ready.WaitOne();
            }

            internal Dispatcher Dispatcher { get; private set; }

            internal int ThreadId { get; private set; }

            public void Dispose()
            {
                Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }
    }
}
