using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderNotificationSinkTests
    {
        [TestMethod]
        public void StartAndDispose_SubscribeAndUnsubscribeAllSources()
        {
            var folder = new FakeSubscription();
            var store = new FakeSubscription();
            var sink = new OutlookFolderNotificationSink(new[] { folder, store });

            sink.Start();
            sink.Dispose();

            folder.SubscribeCount.Should().Be(1);
            folder.UnsubscribeCount.Should().Be(1);
            store.SubscribeCount.Should().Be(1);
            store.UnsubscribeCount.Should().Be(1);
        }

        [TestMethod]
        public void Notifications_RaiseExpectedSinkEvents()
        {
            var source = new FakeSubscription();
            var sink = new OutlookFolderNotificationSink(new[] { source });
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

            sink.Start();
            source.Raise(FolderTreeRefreshReason.FolderAdded);
            source.Raise(FolderTreeRefreshReason.FolderRemoved);
            source.Raise(FolderTreeRefreshReason.FolderChanged);
            source.Raise(FolderTreeRefreshReason.StoreAdded);
            source.Raise(FolderTreeRefreshReason.StoreRemoved);
            sink.Dispose();

            observed.Should().Be(6);
        }

        private sealed class FakeSubscription
            : OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription
        {
            private EventHandler<OutlookFolderNotificationSink.FolderTreeNotification> _handler;

            public int SubscribeCount { get; private set; }
            public int UnsubscribeCount { get; private set; }

            public void Subscribe(
                EventHandler<OutlookFolderNotificationSink.FolderTreeNotification> handler
            )
            {
                SubscribeCount++;
                _handler += handler;
            }

            public void Unsubscribe(
                EventHandler<OutlookFolderNotificationSink.FolderTreeNotification> handler
            )
            {
                UnsubscribeCount++;
                _handler -= handler;
            }

            public void Raise(FolderTreeRefreshReason reason)
            {
                _handler?.Invoke(
                    this,
                    new OutlookFolderNotificationSink.FolderTreeNotification(reason, "store-a")
                );
            }
        }
    }
}
