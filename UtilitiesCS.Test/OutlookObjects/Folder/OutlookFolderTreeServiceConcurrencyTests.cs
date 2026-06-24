using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

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
    }
}
