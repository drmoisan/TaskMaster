using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeNotificationFakeTests
    {
        [TestMethod]
        public void Events_AddAndRemoveHandlers_UpdateCounts()
        {
            var sink = new FakeOutlookFolderNotificationSink();
            void Handler(object sender, FolderTreeSnapshotChangedEventArgs args) { }

            sink.FolderAdded += Handler;
            sink.FolderRemoved += Handler;
            sink.FolderChanged += Handler;
            sink.StoreAdded += Handler;
            sink.StoreRemoved += Handler;
            sink.Disposed += Handler;
            sink.FolderAdded -= Handler;

            sink.FolderAddedHandlerCount.Should().Be(0);
            sink.FolderRemovedHandlerCount.Should().Be(1);
            sink.FolderChangedHandlerCount.Should().Be(1);
            sink.StoreAddedHandlerCount.Should().Be(1);
            sink.StoreRemovedHandlerCount.Should().Be(1);
            sink.DisposedHandlerCount.Should().Be(1);
        }

        [TestMethod]
        public void RaiseMethods_InvokeExpectedHandlers()
        {
            var sink = new FakeOutlookFolderNotificationSink();
            var observed = 0;
            sink.FolderAdded += (sender, args) =>
                observed += args.Reason == FolderTreeRefreshReason.FolderAdded ? 1 : 0;
            sink.FolderRemoved += (sender, args) =>
                observed += args.Reason == FolderTreeRefreshReason.FolderRemoved ? 1 : 0;
            sink.FolderChanged += (sender, args) =>
                observed += args.Reason == FolderTreeRefreshReason.FolderChanged ? 1 : 0;
            sink.StoreAdded += (sender, args) =>
                observed += args.Reason == FolderTreeRefreshReason.StoreAdded ? 1 : 0;
            sink.StoreRemoved += (sender, args) =>
                observed += args.Reason == FolderTreeRefreshReason.StoreRemoved ? 1 : 0;
            sink.Disposed += (sender, args) =>
                observed += args.Reason == FolderTreeRefreshReason.Disposal ? 1 : 0;

            sink.RaiseFolderAdded(
                FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.FolderAdded)
            );
            sink.RaiseFolderRemoved(
                FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.FolderRemoved)
            );
            sink.RaiseFolderChanged(
                FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.FolderChanged)
            );
            sink.RaiseStoreAdded(
                FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.StoreAdded)
            );
            sink.RaiseStoreRemoved(
                FakeOutlookFolderNotificationSink.CreateArgs(FolderTreeRefreshReason.StoreRemoved)
            );
            sink.Dispose();

            observed.Should().Be(6);
        }
    }
}
