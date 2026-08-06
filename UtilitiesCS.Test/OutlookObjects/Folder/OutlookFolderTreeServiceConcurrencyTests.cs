using System;
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
    public sealed class OutlookFolderTreeServiceConcurrencyTests
    {
        [TestMethod]
        public async Task GetSnapshotAsync_ConcurrentInitialRequests_CoalesceOntoOneBuild()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 1
            );
            var yield = new ManualDispatcherYield();
            var builder = new FolderTreeSnapshotBuilder(reader, new AlwaysYieldClock(), yield);
            var service = new OutlookFolderTreeService(
                builder,
                new FakeOutlookFolderNotificationSink()
            );

            var first = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            var second = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            yield.Release();
            var snapshots = await Task.WhenAll(first, second);

            snapshots[0].Should().BeSameAs(snapshots[1]);
            reader.EnumerationCount.Should().Be(1);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher()
        {
            using (var dispatcherHost = new StaDispatcherHost())
            {
                var reader = new RecordingReader();
                var service = new OutlookFolderTreeService(
                    new FolderTreeSnapshotBuilder(
                        reader,
                        new AlwaysYieldClock(),
                        new WpfDispatcherYield()
                    ),
                    new FakeOutlookFolderNotificationSink(),
                    new DispatcherUiDispatcher(dispatcherHost.Dispatcher)
                );

                await Task.Run(async () =>
                    await service.GetSnapshotAsync(
                        FolderTreeRequest.AllStores(false),
                        CancellationToken.None
                    )
                );

                reader.AccessThreadIds.Should().OnlyContain(id => id == dispatcherHost.ThreadId);
            }
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
                _source.SetResult(true);
            }
        }

        private sealed class AlwaysYieldClock : IDeadlineClock
        {
            public bool ShouldYield() => true;

            public void Reset() { }
        }

        private sealed class RecordingReader : IOutlookFolderHierarchyReader
        {
            private readonly FolderTreeSnapshotNode[] _nodes =
            {
                new FolderTreeSnapshotNode(
                    new FolderTreeNodeKey("store-a", "entry-a", "\\Inbox"),
                    "Inbox",
                    "store-a",
                    "entry-a",
                    null,
                    "\\Inbox",
                    "Inbox",
                    new FolderTreeNodeKey[0],
                    false,
                    string.Empty
                ),
            };

            public System.Collections.Generic.IReadOnlyList<int> AccessThreadIds =>
                _accessThreadIds;

            private readonly System.Collections.Generic.List<int> _accessThreadIds =
                new System.Collections.Generic.List<int>();

            public Task<System.Collections.Generic.IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(
                FolderTreeRequest request,
                IDeadlineClock deadlineClock,
                IDispatcherYield dispatcherYield,
                CancellationToken cancellationToken
            )
            {
                _accessThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                return Task.FromResult<System.Collections.Generic.IReadOnlyList<FolderTreeSnapshotNode>>(
                    _nodes
                );
            }
        }

        private sealed class StaDispatcherHost : System.IDisposable
        {
            private readonly AutoResetEvent _ready = new AutoResetEvent(false);
            private readonly Thread _thread;
            private Dispatcher _dispatcher;

            public StaDispatcherHost()
            {
                _thread = new Thread(() =>
                {
                    _dispatcher = Dispatcher.CurrentDispatcher;
                    ThreadId = Thread.CurrentThread.ManagedThreadId;
                    _ready.Set();
                    Dispatcher.Run();
                });
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
                _ready.WaitOne();
            }

            public int ThreadId { get; private set; }

            public Dispatcher Dispatcher => _dispatcher;

            public void Dispose()
            {
                _dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
                _thread.Join();
                _ready.Dispose();
            }
        }

        private sealed class DispatcherUiDispatcher : IUiDispatcher
        {
            private readonly Dispatcher _dispatcher;

            public DispatcherUiDispatcher(Dispatcher dispatcher) => _dispatcher = dispatcher;

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
    }
}
