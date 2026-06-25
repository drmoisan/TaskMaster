using System;
using System.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using Outlook = Microsoft.Office.Interop.Outlook;

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

        [TestMethod]
        public void PublicNamespaceConstructor_CreatesProductionSubscriptionOwners()
        {
            var namespaceMapi = new Mock<Outlook.NameSpace>(MockBehavior.Strict);
            var stores = new Mock<Outlook.Stores>(MockBehavior.Strict);
            stores
                .As<IEnumerable>()
                .Setup(item => item.GetEnumerator())
                .Returns(Array.Empty<Outlook.Store>().GetEnumerator());
            namespaceMapi.SetupGet(item => item.Stores).Returns(stores.Object);

            var sink = new OutlookFolderNotificationSink(namespaceMapi.Object);

            sink.SubscriptionCount.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void FakeSources_CoverFolderStoreAndDisposeNotificationLifecycle()
        {
            var folderSource = new FakeSubscription();
            var storeSource = new FakeSubscription();
            var sink = new OutlookFolderNotificationSink(new[] { folderSource, storeSource });
            var folderAdded = 0;
            var folderRemoved = 0;
            var folderChanged = 0;
            var storeAdded = 0;
            var storeRemoved = 0;
            var disposed = 0;
            sink.FolderAdded += (sender, args) =>
                folderAdded += args.Reason == FolderTreeRefreshReason.FolderAdded ? 1 : 0;
            sink.FolderRemoved += (sender, args) =>
                folderRemoved += args.Reason == FolderTreeRefreshReason.FolderRemoved ? 1 : 0;
            sink.FolderChanged += (sender, args) =>
                folderChanged += args.Reason == FolderTreeRefreshReason.FolderChanged ? 1 : 0;
            sink.StoreAdded += (sender, args) =>
                storeAdded += args.Reason == FolderTreeRefreshReason.StoreAdded ? 1 : 0;
            sink.StoreRemoved += (sender, args) =>
                storeRemoved += args.Reason == FolderTreeRefreshReason.StoreRemoved ? 1 : 0;
            sink.Disposed += (sender, args) =>
                disposed += args.Reason == FolderTreeRefreshReason.Disposal ? 1 : 0;

            sink.Start();
            folderSource.Raise(FolderTreeRefreshReason.FolderAdded, "store-a");
            folderSource.Raise(FolderTreeRefreshReason.FolderRemoved, "store-a");
            folderSource.Raise(FolderTreeRefreshReason.FolderChanged, "store-a");
            folderSource.Raise(FolderTreeRefreshReason.FolderChanged, "store-a");
            storeSource.Raise(FolderTreeRefreshReason.StoreAdded, "store-b");
            storeSource.Raise(FolderTreeRefreshReason.StoreRemoved, "store-b");
            sink.Dispose();

            folderAdded.Should().Be(1);
            folderRemoved.Should().Be(1);
            folderChanged.Should().Be(2, "folder move and rename both stale-mark the folder scope");
            storeAdded.Should().Be(1);
            storeRemoved.Should().Be(1);
            disposed.Should().Be(1);
            folderSource.UnsubscribeCount.Should().Be(1);
            storeSource.UnsubscribeCount.Should().Be(1);
        }

        [TestMethod]
        public void Dispose_WhenCalledTwice_UnsubscribesFakeSourcesOnce()
        {
            var source = new FakeSubscription();
            var sink = new OutlookFolderNotificationSink(new[] { source });

            sink.Start();
            sink.Dispose();
            sink.Dispose();

            source.SubscribeCount.Should().Be(1);
            source.UnsubscribeCount.Should().Be(1);
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
                Raise(reason, "store-a");
            }

            public void Raise(FolderTreeRefreshReason reason, string storeId)
            {
                _handler?.Invoke(
                    this,
                    new OutlookFolderNotificationSink.FolderTreeNotification(reason, storeId)
                );
            }
        }
    }
}
