using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotBuilderYieldTests
    {
        [TestMethod]
        public async Task BuildSnapshotAsync_WhenClockRequestsYield_DispatcherYieldRunsPerNode()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 2
            );
            var dispatcherYield = new FakeDispatcherYield();
            var builder = new FolderTreeSnapshotBuilder(
                reader,
                new AlwaysYieldClock(),
                dispatcherYield
            );

            await builder.BuildSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            dispatcherYield.YieldCount.Should().Be(3);
        }

        [TestMethod]
        public async Task BuildSnapshotAsync_WhenClockDoesNotRequestYield_DispatcherDoesNotRun()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 2
            );
            var dispatcherYield = new FakeDispatcherYield();
            var builder = new FolderTreeSnapshotBuilder(
                reader,
                new NeverYieldClock(),
                dispatcherYield
            );

            await builder.BuildSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            dispatcherYield.YieldCount.Should().Be(0);
        }

        private sealed class AlwaysYieldClock : IDeadlineClock
        {
            public bool ShouldYield() => true;

            public void Reset() { }
        }

        private sealed class NeverYieldClock : IDeadlineClock
        {
            public bool ShouldYield() => false;

            public void Reset() { }
        }
    }
}
