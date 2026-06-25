using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotBuilderCancellationTests
    {
        [TestMethod]
        public async Task BuildSnapshotAsync_CanceledBeforeBuild_ThrowsWithoutSnapshot()
        {
            var builder = new FolderTreeSnapshotBuilder(new FakeOutlookFolderHierarchyReader());
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();

                await builder
                    .Invoking(item =>
                        item.BuildSnapshotAsync(FolderTreeRequest.AllStores(false), source.Token)
                    )
                    .Should()
                    .ThrowAsync<OperationCanceledException>();
            }
        }

        [TestMethod]
        public async Task BuildSnapshotAsync_CanceledAtYield_ThrowsWithoutPartialSnapshot()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddDeepHierarchy(
                "store-a",
                depth: 3
            );
            var clock = new FakeDeadlineClock();
            clock.AdvanceToYield();
            var builder = new FolderTreeSnapshotBuilder(
                reader,
                clock,
                new CancelingDispatcherYield()
            );

            await builder
                .Invoking(item =>
                    item.BuildSnapshotAsync(
                        FolderTreeRequest.AllStores(false),
                        CancellationToken.None
                    )
                )
                .Should()
                .ThrowAsync<OperationCanceledException>();
        }

        private sealed class CancelingDispatcherYield : IDispatcherYield
        {
            public Task YieldAsync(CancellationToken cancellationToken)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
