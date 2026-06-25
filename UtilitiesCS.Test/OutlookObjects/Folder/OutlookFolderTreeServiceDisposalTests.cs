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
    public sealed class OutlookFolderTreeServiceDisposalTests
    {
        [TestMethod]
        public async Task Dispose_UnsubscribesNotificationsAndSuppressesLaterEvents()
        {
            var reader = new FakeOutlookFolderHierarchyReader().AddRecord(
                new FakeFolderHierarchyRecord("store-a", "entry-a", "", "Inbox", "\\Inbox", "Inbox")
            );
            var sink = new FakeOutlookFolderNotificationSink();
            var service = new OutlookFolderTreeService(new FolderTreeSnapshotBuilder(reader), sink);
            await service.GetSnapshotAsync(
                FolderTreeRequest.AllStores(false),
                CancellationToken.None
            );

            service.Dispose();
            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(
                    FolderTreeRefreshReason.FolderChanged,
                    "store-a"
                )
            );

            sink.FolderChangedHandlerCount.Should().Be(0);
            reader.EnumerationCount.Should().Be(1);
        }

        [TestMethod]
        public async Task GetSnapshotAsync_AfterDispose_Throws()
        {
            var service = new OutlookFolderTreeService(
                new FolderTreeSnapshotBuilder(new FakeOutlookFolderHierarchyReader()),
                new FakeOutlookFolderNotificationSink()
            );
            service.Dispose();

            Func<Task> act = () =>
                service.GetSnapshotAsync(
                    FolderTreeRequest.AllStores(false),
                    CancellationToken.None
                );

            await act.Should().ThrowAsync<ObjectDisposedException>();
        }
    }
}
