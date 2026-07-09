using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookRecipient = Microsoft.Office.Interop.Outlook.Recipient;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Tests for <see cref="StoresWrapper.AddOrRestoreStore"/>, the single per-store hookup
    /// primitive shared by the bulk rewire loop and the F3 runtime rehook coordinator (issue #263).
    /// Both branches (absent → create + add; present → restore, no duplicate) are driven with a
    /// COM-mocked <see cref="OutlookStore"/> using <see cref="MockBehavior.Strict"/>; no live
    /// Outlook, no temp files, no timers.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public sealed class StoresWrapperRehookTests
    {
        private static Mock<OutlookStore> CreateStore(string displayName)
        {
            var store = new Mock<OutlookStore>(MockBehavior.Strict);
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<OutlookRecipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(displayName + "@example.com");
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            store.SetupGet(x => x.DisplayName).Returns(displayName);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(new Mock<OutlookFolder>().Object);

            return store;
        }

        [TestMethod]
        public void AddOrRestoreStore_WhenStoreAbsent_AddsNewWrapperToStores()
        {
            // Arrange
            var wrapper = new StoresWrapper
            {
                Stores = new System.Collections.Generic.List<StoreWrapper>(),
            };
            var store = CreateStore("Mailbox - Absent");

            // Act
            var result = wrapper.AddOrRestoreStore(store.Object);

            // Assert
            wrapper
                .Stores.Should()
                .ContainSingle("an absent store produces exactly one new wrapper");
            result.Should().BeSameAs(wrapper.Stores[0], "the returned wrapper is the one added");
            result.DisplayName.Should().Be("Mailbox - Absent");
        }

        [TestMethod]
        public void AddOrRestoreStore_WhenStorePresent_RestoresExistingWrapperWithoutDuplicating()
        {
            // Arrange
            var existingStore = CreateStore("Mailbox - Present");
            var existingWrapper = new StoreWrapper(existingStore.Object).Init();
            var wrapper = new StoresWrapper
            {
                Stores = new System.Collections.Generic.List<StoreWrapper> { existingWrapper },
            };
            var incomingStore = CreateStore("Mailbox - Present");

            // Act
            var result = wrapper.AddOrRestoreStore(incomingStore.Object);

            // Assert
            wrapper.Stores.Should().ContainSingle("a present store is restored, never duplicated");
            result.Should().BeSameAs(existingWrapper, "the existing wrapper instance is reused");
            result
                .InnerStore.Should()
                .BeSameAs(incomingStore.Object, "Restore rebinds the inner store");
        }
    }
}
