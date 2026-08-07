using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    public sealed partial class OutlookFolderTreeServiceTraversalCancellationTests
    {
        [TestMethod]
        public async Task PendingRefresh_DisposalDuringPublicationPreventsAuthorizationAndCleansNotifications()
        {
            var yield = new CancellationObservingYield();
            var clock = new FakeDeadlineClock();
            clock.AdvanceToYield();
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy("store-a", 1);
            var sink = new RecordingCleanupSink();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );
            var publicationCount = 0;
            service.SnapshotChanged += (_, _) =>
            {
                publicationCount++;
                service.Dispose();
            };

            var initialBuild = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            await yield.Started;
            sink.RaiseFolderChanged();
            yield.Release();

            await initialBuild;

            service.State.Should().Be(OutlookFolderTreeServiceState.Disposed);
            publicationCount.Should().Be(1);
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
            sink.RaiseFolderChanged();
            reader.EnumerationCount.Should().Be(1);
        }

        [TestMethod]
        public void Dispose_CleanupObserverFailureIsContainedAndTerminalCleanupCompletes()
        {
            var cleanupFailure = new InvalidOperationException("controlled cleanup failure");
            var observerFailure = new InvalidOperationException("controlled observer failure");
            var sink = new RecordingCleanupSink(cleanupFailure);
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(new FakeOutlookFolderHierarchyReader()),
                sink
            );
            service.ScheduledRefreshFaulted += _ => throw observerFailure;

            Action dispose = service.Dispose;

            dispose.Should().NotThrow();
            service.State.Should().Be(OutlookFolderTreeServiceState.Disposed);
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
        }

        [TestMethod]
        public async Task PendingBuild_StoreAddedNotificationSchedulesAnAllStoresRefresh()
        {
            var yield = new CancellationObservingYield();
            var clock = new FakeDeadlineClock();
            clock.AdvanceToYield();
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy("store-a", 1);
            var sink = new FakeOutlookFolderNotificationSink();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );

            var initialBuild = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            await yield.Started;
            sink.RaiseStoreAdded(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.StoreAdded,
                    "store-b"
                )
            );
            yield.Release();

            await initialBuild;
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            reader.EnumerationCount.Should().Be(2);
            service.State.Should().Be(OutlookFolderTreeServiceState.Current);
        }

        [TestMethod]
        public async Task Dispose_RepeatedQueuedCleanupActionExecutesCleanupStagesOnlyOnce()
        {
            var dispatcher = new StrictCleanupDispatcher();
            var sink = new RecordingCleanupSink();
            var service = CreateReentrantService(dispatcher, sink);

            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            await Await(traversal).Should().ThrowAsync<ObjectDisposedException>();

            dispatcher.CleanupActionInvocationCount.Should().Be(2);
            sink.RemovalAttempts.Should().HaveCount(5);
            sink.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public async Task Dispose_QueuedCleanupDispatchFaultReportsFailureWithoutInlineCleanup()
        {
            var schedulingFailure = new InvalidOperationException(
                "controlled queued cleanup failure"
            );
            var dispatcher = new FaultingCleanupDispatcher(schedulingFailure);
            var sink = new RecordingCleanupSink();
            var service = CreateReentrantService(dispatcher, sink);
            var observedFailures = new System.Collections.Generic.List<Exception>();
            service.ScheduledRefreshFaulted += observedFailures.Add;

            var traversal = service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            await Await(traversal).Should().ThrowAsync<ObjectDisposedException>();

            sink.RemovalAttempts.Should().BeEmpty();
            sink.DisposeCount.Should().Be(0);
            observedFailures.Should().ContainSingle().Which.Should().BeSameAs(schedulingFailure);
        }

        private static OutlookFolderTreeService CreateReentrantService(
            UtilitiesCS.Threading.IUiDispatcher dispatcher,
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
            store
                .Setup(item =>
                    item.ShouldInclude(
                        Moq.It.IsAny<UtilitiesCS.OutlookObjects.Store.StoresWrapper>()
                    )
                )
                .Returns(true);
            store.Setup(item => item.GetRootFolder()).Returns(folder);
            var reader = new OutlookFolderHierarchyReader(
                () => new[] { store.Object },
                new UtilitiesCS.OutlookObjects.Store.StoresWrapper
                {
                    ExcludedStoreNameContains = new System.Collections.Generic.List<string>(),
                }
            );
            service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader),
                sink,
                dispatcher
            );
            return service;
        }

        private sealed class StrictCleanupDispatcher : UtilitiesCS.Threading.IUiDispatcher
        {
            internal int CleanupActionInvocationCount { get; private set; }

            public void Invoke(Action action) => action();

            public Task InvokeAsync(Action action)
            {
                CleanupActionInvocationCount++;
                action();
                CleanupActionInvocationCount++;
                action();
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
        }

        private sealed class FaultingCleanupDispatcher : UtilitiesCS.Threading.IUiDispatcher
        {
            private readonly Exception _failure;

            internal FaultingCleanupDispatcher(Exception failure) => _failure = failure;

            public void Invoke(Action action) => action();

            public Task InvokeAsync(Action action) => Task.FromException(_failure);

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
        }
    }
}
