using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderTreeServiceTraversalCancellationTests
    {
        [TestMethod]
        public async Task Dispose_CancelsInFlightTraversalBeforeItCanPublish()
        {
            var yield = new CancellationObservingYield();
            var service = CreateYieldingService(yield);
            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            await yield.Started;
            service.Dispose();
            var cancellationWasObservedAtDispose = yield.CancellationObserved.IsCompleted;
            yield.Release();
            await Await(traversal).Should().ThrowAsync<ObjectDisposedException>();
            cancellationWasObservedAtDispose
                .Should()
                .BeTrue(
                    "disposing the service must cancel the active traversal before it can publish"
                );
        }

        [TestMethod]
        public async Task GetSnapshotAsync_CallerCancellationCompletesCanceled()
        {
            var yield = new CancellationObservingYield();
            using var cancellation = new CancellationTokenSource();
            var service = CreateYieldingService(yield);
            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                cancellation.Token
            );
            await yield.Started;
            cancellation.Cancel();
            yield.Release();
            var failure = await Await(traversal).Should().ThrowAsync<OperationCanceledException>();
            failure.Which.CancellationToken.Should().Be(cancellation.Token);
            traversal.Status.Should().Be(TaskStatus.Canceled);
        }

        [TestMethod]
        public async Task Dispose_ReentrantHierarchyReadQueuesCleanupAndReportsOriginalStageFailureOnce()
        {
            var cleanupFailure = new InvalidOperationException("controlled cleanup-stage failure");
            var dispatcher = new QueuedCleanupDispatcher();
            var sink = new RecordingCleanupSink(cleanupFailure);
            var service = CreateReentrantService(dispatcher, sink);
            var observedFailures = new List<Exception>();
            var publicationCount = 0;
            var cancellationOwnedGate = true;
            (
                (CancellationTokenSource)GetPrivateField(service, "_disposeCancellation")
            ).Token.Register(() =>
                cancellationOwnedGate = Monitor.IsEntered(GetPrivateField(service, "_gate"))
            );
            service.ScheduledRefreshFaulted += observedFailures.Add;
            service.SnapshotChanged += (_, _) => publicationCount++;
            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.Dispose();
            dispatcher.PendingActionCount.Should().Be(1);
            sink.RaiseFolderChanged();
            dispatcher.PendingActionCount.Should().Be(1);
            sink.HandlerCount.Should().Be(5);
            dispatcher.Drain();
            await Await(traversal).Should().ThrowAsync<ObjectDisposedException>();
            sink.RemovalAttempts.Should()
                .Equal(
                    "FolderAdded",
                    "FolderRemoved",
                    "FolderChanged",
                    "StoreAdded",
                    "StoreRemoved"
                );
            sink.DisposeCount.Should().Be(1);
            sink.HandlerCount.Should().Be(0);
            sink.CleanupCompleted.Should().BeTrue();
            sink.PostCleanupFolderAccessCount.Should().Be(0);
            publicationCount.Should().Be(0);
            cancellationOwnedGate.Should().BeFalse();
            observedFailures.Should().ContainSingle().Which.Should().BeSameAs(cleanupFailure);
        }

        [TestMethod]
        public async Task Dispose_WhenCancellationCallbackFails_CompletesCleanupAndReportsOriginalFailure()
        {
            var cancellationFailure = new InvalidOperationException(
                "controlled cancellation failure"
            );
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy("store-a", 2);
            var yield = new CancellationObservingYield(cancellationFailure);
            var clock = new FakeDeadlineClock();
            clock.AdvanceToYield();
            var sink = new RecordingCleanupSink(
                new InvalidOperationException("controlled cleanup-stage failure")
            );
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );
            var observedFailures = new List<Exception>();
            service.ScheduledRefreshFaulted += observedFailures.Add;
            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            await yield.Started;
            service.Dispose();
            yield.Release();
            await Await(traversal).Should().ThrowAsync<ObjectDisposedException>();
            sink.RemovalAttempts.Should().HaveCount(5);
            sink.DisposeCount.Should().Be(1);
            observedFailures.Should().ContainSingle().Which.Should().BeSameAs(cancellationFailure);
        }

        [TestMethod]
        public async Task Dispose_WhenCleanupCannotBeQueued_ReportsSchedulingFailureWithoutInlineCleanup()
        {
            var schedulingFailure = new InvalidOperationException(
                "controlled cleanup dispatch failure"
            );
            var dispatcher = new QueuedCleanupDispatcher(schedulingFailure);
            var sink = new RecordingCleanupSink(
                new InvalidOperationException("cleanup stage failure")
            );
            var service = CreateReentrantService(dispatcher, sink);
            var observedFailures = new List<Exception>();
            service.ScheduledRefreshFaulted += observedFailures.Add;
            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.Dispose();
            await Await(traversal).Should().ThrowAsync<ObjectDisposedException>();
            dispatcher.PendingActionCount.Should().Be(0);
            sink.RemovalAttempts.Should().BeEmpty();
            sink.DisposeCount.Should().Be(0);
            observedFailures.Should().ContainSingle().Which.Should().BeSameAs(schedulingFailure);
            var idleSink = new RecordingCleanupSink();
            var idleService = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(new FakeOutlookFolderHierarchyReader()),
                idleSink,
                dispatcher
            );
            var idleFailures = new List<Exception>();
            idleService.ScheduledRefreshFaulted += idleFailures.Add;
            idleService.Dispose();
            idleSink.DisposeCount.Should().Be(0);
            idleFailures.Should().ContainSingle().Which.Should().BeSameAs(schedulingFailure);
        }

        [TestMethod]
        public async Task Dispose_SuppressesRetainedNotificationsAndInFlightRefreshFaults()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy("store-a", 1);
            var clock = new FakeDeadlineClock();
            var yield = new CancellationObservingYield();
            var sink = new RecordingCleanupSink(null, true);
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );
            var observedFailures = new List<Exception>();
            service.ScheduledRefreshFaulted += observedFailures.Add;
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            clock.AdvanceToYield();
            sink.RaiseFolderChanged();
            await yield.Started;
            var refresh = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.Dispose();
            sink.RaiseFolderChanged();
            yield.Fail(new InvalidOperationException("controlled refresh fault"));
            await Await(refresh).Should().ThrowAsync<ObjectDisposedException>();
            reader.EnumerationCount.Should().Be(2);
            observedFailures.Should().BeEmpty();
        }

        [TestMethod]
        public async Task SnapshotChanged_DisposingSubscriberSuppressesLaterSubscriber()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy("store-a", 1);
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader),
                new FakeOutlookFolderNotificationSink()
            );
            var laterSubscriberCount = 0;
            var gate = GetPrivateField(service, "_gate");
            service.SnapshotChanged += (_, _) =>
            {
                Monitor.IsEntered(gate).Should().BeFalse();
                service.Dispose();
            };
            service.SnapshotChanged += (_, _) => laterSubscriberCount++;
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            laterSubscriberCount.Should().Be(0);
        }

        private static OutlookFolderTreeService CreateReentrantService(
            QueuedCleanupDispatcher dispatcher,
            RecordingCleanupSink sink
        )
        {
            OutlookFolderTreeService service = null;
            var folder = new ReentrantFolder(
                () => service.Dispose(),
                () => sink.CleanupCompleted,
                sink.RecordPostCleanupFolderAccess
            );
            var store = new Moq.Mock<OutlookFolderHierarchyReader.IOutlookStoreAdapter>();
            store.SetupGet(item => item.StoreId).Returns("store-a");
            store.Setup(item => item.ShouldInclude(Moq.It.IsAny<StoresWrapper>())).Returns(true);
            store.Setup(item => item.GetRootFolder()).Returns(folder);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new StoresWrapper { ExcludedStoreNameContains = new List<string>() }
            );
            service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader),
                sink,
                dispatcher
            );
            return service;
        }

        private static OutlookFolderTreeService CreateYieldingService(
            CancellationObservingYield yield
        )
        {
            var clock = new FakeDeadlineClock();
            clock.AdvanceToYield();
            return new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(
                    new FakeOutlookFolderHierarchyReader().AddDeepHierarchy("store-a", 2),
                    clock,
                    yield
                ),
                new FakeOutlookFolderNotificationSink()
            );
        }

        private const System.Reflection.BindingFlags NonPublicInstance =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

        private static object GetPrivateField(OutlookFolderTreeService service, string name) =>
            typeof(OutlookFolderTreeService).GetField(name, NonPublicInstance).GetValue(service);

        private static Func<Task> Await(Task task) => async () => await task;

        private sealed class CancellationObservingYield : IDispatcherYield
        {
            private readonly Exception _cancellationFailure;
            private readonly TaskCompletionSource<bool> _started = new(),
                _released = new(),
                _cancellationObserved = new();
            internal Task Started => _started.Task;
            internal Task CancellationObserved => _cancellationObserved.Task;

            internal CancellationObservingYield(Exception cancellationFailure = null) =>
                _cancellationFailure = cancellationFailure;

            public Task YieldAsync(CancellationToken cancellationToken)
            {
                _started.TrySetResult(true);
                cancellationToken.Register(() =>
                {
                    _cancellationObserved.TrySetResult(true);
                    if (_cancellationFailure != null)
                        throw _cancellationFailure;
                });
                return _released.Task;
            }

            internal void Release() => _released.TrySetResult(true);

            internal void Fail(Exception exception) => _released.TrySetException(exception);
        }

        private sealed class ReentrantFolder : OutlookFolderHierarchyReader.IOutlookFolderAdapter
        {
            private readonly Action _dispose;
            private readonly Func<bool> _cleanupCompleted;
            private readonly Action _recordPostCleanupFolderAccess;
            private bool _disposeRequested;

            internal ReentrantFolder(
                Action dispose,
                Func<bool> cleanupCompleted,
                Action recordPostCleanupFolderAccess
            ) =>
                (_dispose, _cleanupCompleted, _recordPostCleanupFolderAccess) = (
                    dispose,
                    cleanupCompleted,
                    recordPostCleanupFolderAccess
                );

            public string EntryID
            {
                get
                {
                    RecordPostCleanupAccess();
                    if (!_disposeRequested)
                    {
                        _disposeRequested = true;
                        _dispose();
                    }
                    return "entry-a";
                }
            }
            public string Name => Read("Root");
            public string FolderPath => Read("\\Root");
            public IReadOnlyList<OutlookFolderHierarchyReader.IOutlookFolderAdapter> Children =>
                Array.Empty<OutlookFolderHierarchyReader.IOutlookFolderAdapter>();

            private void RecordPostCleanupAccess()
            {
                if (_cleanupCompleted())
                    _recordPostCleanupFolderAccess();
            }

            private string Read(string value)
            {
                RecordPostCleanupAccess();
                return value;
            }
        }

        private sealed class QueuedCleanupDispatcher : IUiDispatcher
        {
            private readonly Queue<Action> _pendingActions = new Queue<Action>();
            private readonly Exception _cleanupDispatchFailure;

            internal QueuedCleanupDispatcher(Exception cleanupDispatchFailure = null) =>
                _cleanupDispatchFailure = cleanupDispatchFailure;

            internal int PendingActionCount => _pendingActions.Count;

            public void Invoke(Action action)
            {
                if (_cleanupDispatchFailure != null)
                    throw _cleanupDispatchFailure;
                action();
            }

            public Task InvokeAsync(Action action)
            {
                if (_cleanupDispatchFailure != null)
                    throw _cleanupDispatchFailure;
                _pendingActions.Enqueue(action);
                return Task.CompletedTask;
            }

            public Task InvokeAsync(
                Action action,
                System.Windows.Threading.DispatcherPriority priority,
                CancellationToken token
            )
            {
                token.ThrowIfCancellationRequested();
                action();
                return Task.CompletedTask;
            }

            public IAsyncResult BeginInvoke(Action action) => throw new NotSupportedException();

            public Task<TResult> InvokeAsync<TResult>(Func<TResult> func) =>
                Task.FromResult(func());

            public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> func) => func();

            internal void Drain()
            {
                while (_pendingActions.Count > 0)
                    _pendingActions.Dequeue().Invoke();
            }
        }

        private sealed class RecordingCleanupSink : IOutlookFolderNotificationSink
        {
            private readonly Exception _cleanupFailure;
            private readonly bool _retainFolderChanged;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderAdded;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderRemoved;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _folderChanged;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _storeAdded;
            private EventHandler<FolderTreeSnapshotChangedEventArgs> _storeRemoved;

            internal RecordingCleanupSink(
                Exception cleanupFailure = null,
                bool retainFolderChanged = false
            ) => (_cleanupFailure, _retainFolderChanged) = (cleanupFailure, retainFolderChanged);

            internal List<string> RemovalAttempts { get; } = new List<string>();
            internal int DisposeCount { get; private set; }
            internal int PostCleanupFolderAccessCount { get; private set; }
            internal bool CleanupCompleted => DisposeCount != 0;
            internal int HandlerCount =>
                HandlerCountOf(_folderAdded)
                + HandlerCountOf(_folderRemoved)
                + HandlerCountOf(_folderChanged)
                + HandlerCountOf(_storeAdded)
                + HandlerCountOf(_storeRemoved);
            public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderAdded
            {
                add => Subscribe(ref _folderAdded, value);
                remove => Remove(ref _folderAdded, value, "FolderAdded", true);
            }
            public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderRemoved
            {
                add => Subscribe(ref _folderRemoved, value);
                remove => Remove(ref _folderRemoved, value, "FolderRemoved", false);
            }
            public event EventHandler<FolderTreeSnapshotChangedEventArgs> FolderChanged
            {
                add => Subscribe(ref _folderChanged, value);
                remove => Remove(ref _folderChanged, value, "FolderChanged", false);
            }
            public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreAdded
            {
                add => Subscribe(ref _storeAdded, value);
                remove => Remove(ref _storeAdded, value, "StoreAdded", false);
            }
            public event EventHandler<FolderTreeSnapshotChangedEventArgs> StoreRemoved
            {
                add => Subscribe(ref _storeRemoved, value);
                remove => Remove(ref _storeRemoved, value, "StoreRemoved", false);
            }
            public event EventHandler<FolderTreeSnapshotChangedEventArgs> Disposed;

            public void Start() => _ = Disposed;

            public void AddStore(Microsoft.Office.Interop.Outlook.Store store) { }

            public void RemoveStore(string storeId) { }

            public void Dispose() => DisposeCount++;

            internal void RecordPostCleanupFolderAccess() => PostCleanupFolderAccessCount++;

            internal void RaiseFolderChanged() =>
                _folderChanged?.Invoke(
                    this,
                    FakeOutlookFolderNotificationSink.CreateArgs(
                        FolderTreeRefreshReason.FolderChanged,
                        "store-a"
                    )
                );

            private static void Subscribe(
                ref EventHandler<FolderTreeSnapshotChangedEventArgs> handlers,
                EventHandler<FolderTreeSnapshotChangedEventArgs> handler
            ) => handlers += handler;

            private void Remove(
                ref EventHandler<FolderTreeSnapshotChangedEventArgs> handlers,
                EventHandler<FolderTreeSnapshotChangedEventArgs> handler,
                string stage,
                bool throwFailure
            )
            {
                if (stage != "FolderChanged" || !_retainFolderChanged)
                    handlers -= handler;
                RemovalAttempts.Add(stage);
                if (throwFailure && _cleanupFailure != null)
                    throw _cleanupFailure;
            }

            private static int HandlerCountOf(
                EventHandler<FolderTreeSnapshotChangedEventArgs> handlers
            ) => handlers?.GetInvocationList().Length ?? 0;
        }
    }
}
