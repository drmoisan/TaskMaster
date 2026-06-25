using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderTreeServiceScopeTests
    {
        [TestMethod]
        public async Task GetSnapshotAsync_StoreSnapshotThenAllStoresRequest_RebuildsCoveredScope()
        {
            var service = CreateMultiStoreService(out var reader);
            await service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-a", false),
                CancellationToken.None
            );

            var allStores = await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            reader.EnumerationCount.Should().Be(2);
            allStores.GetNodesForStore("store-a").Should().ContainSingle();
            allStores.GetNodesForStore("store-b").Should().ContainSingle();
        }

        [TestMethod]
        public async Task GetSnapshotAsync_StoreSnapshotThenDifferentStoreRequest_RebuildsRequestedStore()
        {
            var service = CreateMultiStoreService(out var reader);
            await service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-a", false),
                CancellationToken.None
            );

            var storeB = await service.GetSnapshotAsync(
                FolderTreeRequest.ForStore("store-b", false),
                CancellationToken.None
            );

            reader.EnumerationCount.Should().Be(2);
            storeB.GetNodesForStore("store-a").Should().BeEmpty();
            storeB.GetNodesForStore("store-b").Should().ContainSingle();
        }

        [TestMethod]
        public async Task FolderChanged_AfterAllStoreSnapshot_PreservesUnaffectedStoreNodes()
        {
            var reader = CreateMultiStoreReader();
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
            FolderTreeSnapshot publishedSnapshot = null;
            service.SnapshotChanged += (_, args) => publishedSnapshot = args.Snapshot;
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

            publishedSnapshot.Should().NotBeNull();
            publishedSnapshot.GetNodesForStore("store-a").Should().ContainSingle();
            publishedSnapshot.GetNodesForStore("store-b").Should().ContainSingle();
        }

        private static OutlookFolderTreeService CreateMultiStoreService(
            out FakeOutlookFolderHierarchyReader reader
        )
        {
            reader = CreateMultiStoreReader();
            var builder = new FolderTreeSnapshotBuilder(reader);
            return new OutlookFolderTreeService(builder, new FakeOutlookFolderNotificationSink());
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
    }
}
