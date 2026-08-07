using System;
using System.Collections.Generic;
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
    public sealed class OutlookFolderTreeServiceInvalidationTests
    {
        [TestMethod]
        public async Task FolderChanged_StaleMarksAndCoalescesRefresh()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 1
            );
            var clock = new SwitchableClock();
            var yield = new ManualDispatcherYield();
            var sink = new FakeOutlookFolderNotificationSink();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            clock.ShouldYieldNow = true;

            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );
            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );
            service.State.Should().Be(OutlookFolderTreeServiceState.Refreshing);
            yield.Release();

            await service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-a", true),
                CancellationToken.None
            );
            reader.EnumerationCount.Should().Be(2);
        }

        [TestMethod]
        public async Task FolderChanged_DuringInFlightBuild_SchedulesOneFollowUpRefresh()
        {
            var reader = CreateMultiStoreReader();
            var clock = new SwitchableClock { ShouldYieldNow = true };
            var yield = new ManualDispatcherYield();
            var sink = new FakeOutlookFolderNotificationSink();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );

            var initialBuild = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );
            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );

            yield.Release();
            await initialBuild;
            var finalSnapshot = await service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-a", false),
                CancellationToken.None
            );

            reader.EnumerationCount.Should().Be(2);
            finalSnapshot.CoversAllStores.Should().BeTrue();
            finalSnapshot.GetNodesForStore("store-a").Should().ContainSingle();
            finalSnapshot.GetNodesForStore("store-b").Should().ContainSingle();
        }

        [TestMethod]
        public async Task NotificationRefresh_RunsOnCapturedDispatcher()
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

                await dispatcher.InvokeAsync(() =>
                    service.GetSnapshotAsync(
                        FolderTreeRequest.AllStores(false),
                        CancellationToken.None
                    )
                );
                await Task.Run(() =>
                    sink.RaiseFolderChanged(
                        RecordingNotificationSink.CreateArgs(
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
            }
        }

        [TestMethod]
        public async Task NotificationRefreshFault_IsObservedWithoutUnexpectedRetry()
        {
            var refreshFailure = new InvalidOperationException(
                "Controlled notification refresh failure."
            );
            var reader = new FaultingNotificationRefreshReader(refreshFailure);
            var sink = new RecordingNotificationSink();
            var service = new OutlookFolderTreeService(new FolderTreeSnapshotBuilder(reader), sink);
            var observedFailure = new TaskCompletionSource<Exception>();
            var publicationCount = 0;
            service.SnapshotChanged += (_, _) => publicationCount++;
            service.ScheduledRefreshFaulted += exception => observedFailure.TrySetResult(exception);

            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            sink.RaiseFolderChanged(
                RecordingNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );

            var observed = await observedFailure.Task;

            observed.Should().BeSameAs(refreshFailure);
            reader.RequestCount.Should().Be(2);
            publicationCount.Should().Be(1);
            service.State.Should().Be(OutlookFolderTreeServiceState.StaleCurrent);
        }

        private static FakeOutlookFolderHierarchyReader CreateMultiStoreReader()
        {
            return new FakeOutlookFolderHierarchyReader()
                .AddRecord(
                    new FakeFolderHierarchyRecord(
                        "store-a",
                        "entry-a",
                        "",
                        "Inbox",
                        "\\Inbox",
                        "Inbox"
                    )
                )
                .AddRecord(
                    new FakeFolderHierarchyRecord(
                        "store-b",
                        "entry-b",
                        "",
                        "Archive",
                        "\\Archive",
                        "Archive"
                    )
                );
        }

        private sealed class SwitchableClock : IDeadlineClock
        {
            public bool ShouldYieldNow { get; set; }

            public bool ShouldYield() => ShouldYieldNow;

            public void Reset() { }
        }

        private sealed class ManualDispatcherYield : IDispatcherYield
        {
            private readonly TaskCompletionSource<bool> _source = new TaskCompletionSource<bool>();

            public Task YieldAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return _source.Task;
            }

            public void Release()
            {
                _source.TrySetResult(true);
            }
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

        private sealed class FaultingNotificationRefreshReader : IOutlookFolderHierarchyReader
        {
            private readonly Exception _refreshFailure;

            internal FaultingNotificationRefreshReader(Exception refreshFailure) =>
                _refreshFailure = refreshFailure;

            internal int RequestCount { get; private set; }

            public Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
                FolderTreeRequest request,
                IDeadlineClock deadlineClock,
                IDispatcherYield dispatcherYield,
                CancellationToken cancellationToken
            )
            {
                RequestCount++;
                if (RequestCount > 1)
                {
                    return Task.FromException<IReadOnlyList<FolderTreeSnapshotNode>>(
                        _refreshFailure
                    );
                }

                return Task.FromResult<IReadOnlyList<FolderTreeSnapshotNode>>(
                    Array.Empty<FolderTreeSnapshotNode>()
                );
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
                add => Subscribe(ref _folderChanged, value);
                remove => Unsubscribe(ref _folderChanged, value);
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

            public void Start() { }

            public void AddStore(Microsoft.Office.Interop.Outlook.Store store) { }

            public void RemoveStore(string storeId) { }

            public void Dispose() =>
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

            internal void RaiseFolderChanged(FolderTreeSnapshotChangedEventArgs args) =>
                _folderChanged?.Invoke(this, args);

            internal static FolderTreeSnapshotChangedEventArgs CreateArgs(
                FolderTreeRefreshReason reason,
                string storeId
            ) =>
                new(
                    new FolderTreeSnapshot(
                        Array.Empty<FolderTreeNodeKey>(),
                        Array.Empty<FolderTreeSnapshotNode>()
                    ),
                    reason,
                    new[] { storeId }
                );

            private void Subscribe(
                ref EventHandler<FolderTreeSnapshotChangedEventArgs> eventHandler,
                EventHandler<FolderTreeSnapshotChangedEventArgs> handler
            )
            {
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                eventHandler += handler;
            }

            private void Unsubscribe(
                ref EventHandler<FolderTreeSnapshotChangedEventArgs> eventHandler,
                EventHandler<FolderTreeSnapshotChangedEventArgs> handler
            )
            {
                SubscriptionAndCleanupThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                eventHandler -= handler;
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
                    Dispatcher = Dispatcher.CurrentDispatcher;
                    ThreadId = Thread.CurrentThread.ManagedThreadId;
                    _ready.Set();
                    Dispatcher.Run();
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
