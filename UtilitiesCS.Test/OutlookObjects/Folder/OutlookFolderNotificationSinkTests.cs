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

        [TestMethod]
        public void AddStore_NewStoreId_SubscribesItsSubscriptions()
        {
            // Arrange: a started sink with no store subscriptions yet.
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );
            sink.Start();
            var storeSub = new FakeSubscription();

            // Act: register a new store's subscriptions via the COM-free registration seam that
            // AddStore(Outlook.Store) delegates to.
            sink.AddStoreSubscriptions(
                "store-x",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[]
                {
                    storeSub,
                }
            );

            // Assert
            storeSub.SubscribeCount.Should().Be(1, "a new store's subscriptions are wired live");
            sink.SubscriptionCount.Should().Be(1);
        }

        [TestMethod]
        public void AddStore_AlreadyPresentStoreId_IsNoOpWithZeroAdditionalSubscribes()
        {
            // Arrange
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );
            sink.Start();
            var first = new FakeSubscription();
            var second = new FakeSubscription();

            // Act: second registration for the same StoreID must be a no-op success.
            sink.AddStoreSubscriptions(
                "store-x",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[] { first }
            );
            sink.AddStoreSubscriptions(
                "store-x",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[]
                {
                    second,
                }
            );

            // Assert
            first.SubscribeCount.Should().Be(1);
            second
                .SubscribeCount.Should()
                .Be(0, "an already-present StoreID performs zero additional subscribes");
            sink.SubscriptionCount.Should().Be(1);
        }

        [TestMethod]
        public void RemoveStore_UnsubscribesThatStoreAndDoesNotAffectOthers()
        {
            // Arrange
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );
            sink.Start();
            var storeX = new FakeSubscription();
            var storeY = new FakeSubscription();
            sink.AddStoreSubscriptions(
                "store-x",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[]
                {
                    storeX,
                }
            );
            sink.AddStoreSubscriptions(
                "store-y",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[]
                {
                    storeY,
                }
            );

            // Act
            sink.RemoveStore("store-x");

            // Assert
            storeX
                .UnsubscribeCount.Should()
                .Be(1, "the removed store's subscriptions are unsubscribed");
            storeY.UnsubscribeCount.Should().Be(0, "other stores are unaffected");
            sink.SubscriptionCount.Should().Be(1, "only store-y's subscription remains");
        }

        [TestMethod]
        public void IsStoreHooked_ReflectsRegistrationState()
        {
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );
            sink.Start();

            sink.IsStoreHooked("store-x").Should().BeFalse("not registered yet");
            sink.IsStoreHooked(null).Should().BeFalse("a null StoreID is never hooked");

            sink.AddStoreSubscriptions(
                "store-x",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[]
                {
                    new FakeSubscription(),
                }
            );

            sink.IsStoreHooked("store-x").Should().BeTrue("registered via AddStoreSubscriptions");
            sink.RemoveStore("store-x");
            sink.IsStoreHooked("store-x").Should().BeFalse("removed via RemoveStore");
        }

        [TestMethod]
        public void AddStoreSubscriptions_NullArguments_Throw()
        {
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );

            var subs = new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[0];
            ((Action)(() => sink.AddStoreSubscriptions(null, subs)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => sink.AddStoreSubscriptions("store-x", null)))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddStoreSubscriptions_AfterDispose_IsNoOp()
        {
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );
            sink.Start();
            sink.Dispose();
            var sub = new FakeSubscription();

            sink.AddStoreSubscriptions(
                "store-x",
                new OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription[] { sub }
            );

            sub.SubscribeCount.Should().Be(0, "a disposed sink registers nothing");
            sink.IsStoreHooked("store-x").Should().BeFalse();
        }

        [TestMethod]
        public void RemoveStore_NullOrAbsentStoreId_IsNoOp()
        {
            var sink = new OutlookFolderNotificationSink(
                Array.Empty<OutlookFolderNotificationSink.IOutlookFolderNotificationSubscription>()
            );
            sink.Start();

            // No throw for null or an absent StoreID.
            ((Action)(() => sink.RemoveStore(null)))
                .Should()
                .NotThrow();
            ((Action)(() => sink.RemoveStore("never-added"))).Should().NotThrow();
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
