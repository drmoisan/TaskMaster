using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

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
