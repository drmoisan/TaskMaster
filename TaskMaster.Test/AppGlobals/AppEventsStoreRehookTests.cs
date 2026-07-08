using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Tests for the F3 per-store inbox-item-subscribe primitive
    /// (<see cref="AppEvents.SubscribeInboxForStore"/>) and its StoreID-keyed idempotency guard
    /// (<see cref="AppEvents.IsInboxHooked"/>) (issue #263). Uses Moq to verify the
    /// <c>ItemAdd</c> subscription count without live Outlook; confirms a repeat call for the same
    /// StoreID performs zero additional subscribes.
    /// </summary>
    [TestClass]
    public sealed class AppEventsStoreRehookTests
    {
        private static AppEvents CreateAppEvents()
        {
            // SubscribeInboxForStore / IsInboxHooked do not touch Globals; a bare mock satisfies
            // the constructor.
            return new AppEvents(Mock.Of<IApplicationGlobals>());
        }

        private static (Mock<Store> store, Mock<Items> items, Folder inbox) CreateStoreWithInbox(
            string storeId
        )
        {
            var items = new Mock<Items>(MockBehavior.Loose);
            var store = new Mock<Store>(MockBehavior.Strict);
            store.SetupGet(x => x.StoreID).Returns(storeId);
            var inbox = Mock.Of<Folder>(x => x.Items == items.Object);
            return (store, items, inbox);
        }

        [TestMethod]
        public void SubscribeInboxForStore_FirstCall_SubscribesItemAddOnceAndRecordsStoreId()
        {
            // Arrange
            var sut = CreateAppEvents();
            var (store, items, inbox) = CreateStoreWithInbox("store-A");

            // Act
            sut.SubscribeInboxForStore(store.Object, inbox);

            // Assert
            items.VerifyAdd(
                m => m.ItemAdd += It.IsAny<ItemsEvents_ItemAddEventHandler>(),
                Times.Once()
            );
            sut.IsInboxHooked("store-A")
                .Should()
                .BeTrue("the store's inbox was recorded as hooked");
        }

        [TestMethod]
        public void SubscribeInboxForStore_SecondCallSameStoreId_PerformsZeroAdditionalSubscribes()
        {
            // Arrange
            var sut = CreateAppEvents();
            var (store, items, inbox) = CreateStoreWithInbox("store-A");

            // Act
            sut.SubscribeInboxForStore(store.Object, inbox);
            sut.SubscribeInboxForStore(store.Object, inbox);

            // Assert
            items.VerifyAdd(
                m => m.ItemAdd += It.IsAny<ItemsEvents_ItemAddEventHandler>(),
                Times.Once()
            );
            sut.IsInboxHooked("store-A").Should().BeTrue();
        }

        [TestMethod]
        public void IsInboxHooked_ForUnhookedStoreId_ReturnsFalse()
        {
            // Arrange
            var sut = CreateAppEvents();

            // Act / Assert
            sut.IsInboxHooked("never-hooked").Should().BeFalse();
        }

        [TestMethod]
        public void IsInboxHooked_ForNullStoreId_ReturnsFalse()
        {
            // Arrange
            var sut = CreateAppEvents();

            // Act / Assert
            sut.IsInboxHooked(null).Should().BeFalse("a null StoreID is never hooked");
        }

        [TestMethod]
        public void SubscribeInboxForStore_WithNullStoreOrInbox_IsNoOp()
        {
            // Arrange
            var sut = CreateAppEvents();
            var (store, _, inbox) = CreateStoreWithInbox("store-A");

            // Act / Assert: null guards short-circuit without throwing and record nothing.
            sut.SubscribeInboxForStore(null, inbox);
            sut.SubscribeInboxForStore(store.Object, null);
            sut.IsInboxHooked("store-A").Should().BeFalse();
        }
    }
}
