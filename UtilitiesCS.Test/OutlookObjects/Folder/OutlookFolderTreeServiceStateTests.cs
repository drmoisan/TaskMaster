using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderTreeServiceStateTests
    {
        [TestMethod]
        public async Task GetSnapshotAsync_EmptyService_BuildsCurrentSnapshot()
        {
            var service = CreateService();

            var snapshot = await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            snapshot.NodesByKey.Should().ContainSingle();
            service.State.Should().Be(OutlookFolderTreeServiceState.Current);
        }

        [TestMethod]
        public async Task MarkStale_WithCurrentSnapshot_ChangesStateToStaleCurrent()
        {
            var service = CreateService();
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            service.MarkStale("store-a", FolderTreeRefreshReason.FolderChanged);

            service.State.Should().Be(OutlookFolderTreeServiceState.StaleCurrent);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_StaleSnapshotAllowed_ReturnsCachedSnapshot()
        {
            var service = CreateService(out var reader);
            var current = await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.MarkStale("store-a", FolderTreeRefreshReason.FolderChanged);

            var stale = await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(true),
                CancellationToken.None
            );

            stale.Should().BeSameAs(current);
            reader.EnumerationCount.Should().Be(1);
            service.State.Should().Be(OutlookFolderTreeServiceState.StaleCurrent);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_StaleSnapshotDisallowed_RebuildsSnapshot()
        {
            var service = CreateService(out var reader);
            var current = await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.MarkStale("store-a", FolderTreeRefreshReason.FolderChanged);

            var rebuilt = await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            rebuilt.Should().NotBeSameAs(current);
            reader.EnumerationCount.Should().Be(2);
            service.State.Should().Be(OutlookFolderTreeServiceState.Current);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_StaleSnapshotWithNullRequest_RebuildsSnapshot()
        {
            var service = CreateService(out var reader);
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.MarkStale("store-a", FolderTreeRefreshReason.FolderChanged);

            await service.GetSnapshotAsync(null, CancellationToken.None);

            reader.EnumerationCount.Should().Be(2);
            service.State.Should().Be(OutlookFolderTreeServiceState.Current);
        }

        [TestMethod]
        public async Task FolderChanged_PendingRefresh_PublishesCurrentSnapshot()
        {
            var reader = CreateReader();
            var sink = new FakeOutlookFolderNotificationSink();
            var clock = new SwitchableClock();
            var yield = new ManualDispatcherYield();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            var publicationCount = 0;
            service.SnapshotChanged += (_, _) => publicationCount++;
            clock.ShouldYieldNow = true;

            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );

            service.State.Should().Be(OutlookFolderTreeServiceState.Refreshing);
            yield.Release();
            await service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-a", false),
                CancellationToken.None
            );
            service.State.Should().Be(OutlookFolderTreeServiceState.Current);
            reader.EnumerationCount.Should().Be(2);
            publicationCount.Should().Be(1);
        }

        [TestMethod]
        public async Task StoreRemoved_WithoutStoreScope_SchedulesAllStoreRefresh()
        {
            var reader = CreateReader();
            var sink = new FakeOutlookFolderNotificationSink();
            var clock = new SwitchableClock();
            var yield = new ManualDispatcherYield();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader, clock, yield),
                sink
            );
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            clock.ShouldYieldNow = true;

            sink.RaiseStoreRemoved(
                FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.StoreRemoved)
            );

            service.State.Should().Be(OutlookFolderTreeServiceState.Refreshing);
            yield.Release();
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            reader.EnumerationCount.Should().Be(2);
        }

        [TestMethod]
        public void Dispose_ChangesStateAndDisposesNotificationSink()
        {
            var sink = new FakeOutlookFolderNotificationSink();
            var service = CreateService(sink);

            service.Dispose();

            service.State.Should().Be(OutlookFolderTreeServiceState.Disposed);
            sink.DisposeCount.Should().Be(1);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_DisposedService_ThrowsObjectDisposedException()
        {
            var service = CreateService();
            service.Dispose();

            Func<Task> act = () =>
                service.GetSnapshotAsync(
                    FolderTreeRequest.AllStores(false),
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<ObjectDisposedException>();
        }

        [TestMethod]
        public void Constructor_NullDependencies_Throw()
        {
            var builder = new FolderTreeSnapshotBuilder(CreateReader());
            var sink = new FakeOutlookFolderNotificationSink();

            Action nullBuilder = () => new OutlookFolderTreeService(null, sink);
            Action nullSink = () => new OutlookFolderTreeService(builder, null);

            nullBuilder.Should().Throw<ArgumentNullException>().WithParameterName("builder");
            nullSink.Should().Throw<ArgumentNullException>().WithParameterName("notificationSink");
        }

        [TestMethod]
        public async Task GetSnapshotAsync_WhenInitialBuildFails_RestoresEmptyState()
        {
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(new ThrowingReader()),
                new FakeOutlookFolderNotificationSink()
            );

            Func<Task> act = () =>
                service.GetSnapshotAsync(
                    FolderTreeRequest.AllStores(false),
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<InvalidOperationException>();
            service.State.Should().Be(OutlookFolderTreeServiceState.Empty);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_WhenRefreshBuildFails_PreservesStaleSnapshot()
        {
            var reader = new ThrowAfterFirstReader();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(reader),
                new FakeOutlookFolderNotificationSink()
            );
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            service.MarkStale("store-a", FolderTreeRefreshReason.FolderChanged);

            Func<Task> act = () =>
                service.GetSnapshotAsync(
                    FolderTreeRequest.AllStores(false),
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<InvalidOperationException>();
            service.State.Should().Be(OutlookFolderTreeServiceState.StaleCurrent);
        }

        [TestMethod]
        public async Task MarkStale_WhileRefreshing_DoesNotOverwriteRefreshingState()
        {
            var sink = new FakeOutlookFolderNotificationSink();
            var clock = new SwitchableClock();
            var yield = new ManualDispatcherYield();
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(CreateReader(), clock, yield),
                sink
            );
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
            clock.ShouldYieldNow = true;

            sink.RaiseStoreAdded(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.StoreAdded,
                    "store-a"
                )
            );
            service.MarkStale("store-a", FolderTreeRefreshReason.FolderChanged);

            service.State.Should().Be(OutlookFolderTreeServiceState.Refreshing);
            yield.Release();
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );
        }

        private static OutlookFolderTreeService CreateService(
            IOutlookFolderNotificationSink sink = null
        )
        {
            var reader = CreateReader();
            var builder = new FolderTreeSnapshotBuilder(reader);
            return new OutlookFolderTreeService(
                builder,
                sink ?? new FakeOutlookFolderNotificationSink()
            );
        }

        private static OutlookFolderTreeService CreateService(
            out FakeOutlookFolderHierarchyReader reader
        )
        {
            reader = CreateReader();
            var builder = new FolderTreeSnapshotBuilder(reader);
            return new OutlookFolderTreeService(builder, new FakeOutlookFolderNotificationSink());
        }

        private static FakeOutlookFolderHierarchyReader CreateReader()
        {
            return new FakeOutlookFolderHierarchyReader().AddRecord(
                new FakeFolderHierarchyRecord("store-a", "entry-a", "", "Inbox", "\\Inbox", "Inbox")
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

        private sealed class ThrowingReader : IOutlookFolderHierarchyReader
        {
            public IReadOnlyList<FolderTreeSnapshotNode> ReadFolders(
                FolderTreeRequest request,
                CancellationToken cancellationToken
            )
            {
                throw new InvalidOperationException("read failed");
            }
        }

        private sealed class ThrowAfterFirstReader : IOutlookFolderHierarchyReader
        {
            private readonly FakeOutlookFolderHierarchyReader _inner = CreateReader();
            private int _calls;

            public IReadOnlyList<FolderTreeSnapshotNode> ReadFolders(
                FolderTreeRequest request,
                CancellationToken cancellationToken
            )
            {
                _calls++;
                if (_calls > 1)
                {
                    throw new InvalidOperationException("refresh failed");
                }

                return _inner.ReadFolders(request, cancellationToken);
            }
        }
    }
}
