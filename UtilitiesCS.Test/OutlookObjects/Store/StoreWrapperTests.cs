using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    [DoNotParallelize]
    public class StoreWrapperTests
    {
        [TestMethod]
        public void Init_WhenStoreIsNotPublicFolder_ProjectsDisplayNameRootFolderInboxAndUserEmailAddress()
        {
            // Arrange
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress("owner@example.com");
            var inbox = new Mock<OutlookFolder>();

            store.SetupGet(x => x.DisplayName).Returns("Mailbox");
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(inbox.Object);

            var wrapper = new StoreWrapper(store.Object);

            // Act
            var result = wrapper.Init();

            // Assert
            result.Should().BeSameAs(wrapper);
            wrapper.DisplayName.Should().Be("Mailbox");
            wrapper.RootFolder.Should().BeSameAs(rootFolder.Object);
            wrapper.Inbox.Should().BeSameAs(inbox.Object);
            wrapper.UserEmailAddress.Should().Be("owner@example.com");
        }

        [TestMethod]
        public void Init_WhenStoreIsPublicFolder_DoesNotPopulateInbox()
        {
            // Arrange
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress("public@example.com");

            store.SetupGet(x => x.DisplayName).Returns("Public Folders");
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olExchangePublicFolder);

            var wrapper = new StoreWrapper(store.Object);

            // Act
            wrapper.Init();

            // Assert
            wrapper.Inbox.Should().BeNull();
            wrapper.RootFolder.Should().BeSameAs(rootFolder.Object);
            wrapper.UserEmailAddress.Should().Be("public@example.com");
            store.Verify(x => x.GetDefaultFolder(It.IsAny<OlDefaultFolders>()), Times.Never);
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenExchangeUserIsUnavailable_ReturnsNull()
        {
            // Arrange
            var store = new Mock<OutlookStore>();
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var addressEntry = new Mock<AddressEntry>();

            rootFolder.SetupGet(x => x.Session).Returns(session.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns((ExchangeUser)null);

            var wrapper = new StoreWrapper(store.Object) { RootFolder = rootFolder.Object };

            // Act
            string result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenExchangeLookupThrowsComException_ReturnsNull()
        {
            // Arrange
            var store = new Mock<OutlookStore>();
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var addressEntry = new Mock<AddressEntry>();

            rootFolder.SetupGet(x => x.Session).Returns(session.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            addressEntry.Setup(x => x.GetExchangeUser()).Throws(new COMException("Boom"));

            var wrapper = new StoreWrapper(store.Object) { RootFolder = rootFolder.Object };

            // Act
            string result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().BeNull();
        }

        private static Mock<OutlookFolder> CreateRootFolderWithPrimarySmtpAddress(
            string primarySmtpAddress
        )
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(primarySmtpAddress);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return rootFolder;
        }

        /// <summary>
        /// Builds the same mocked Outlook folder chain as
        /// <see cref="CreateRootFolderWithPrimarySmtpAddress"/>, except that the Exchange primary
        /// SMTP read throws a COM exception and the address entry's own address is supplied by the
        /// caller. Used by the AC6 fallback-ordering cases (issue #797).
        /// </summary>
        private static Mock<OutlookFolder> CreateRootFolderWithFailingPrimarySmtpAddress(
            string addressEntryAddress
        )
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser
                .SetupGet(x => x.PrimarySmtpAddress)
                .Throws(new COMException("The operation failed."));
            addressEntry.SetupGet(x => x.Address).Returns(addressEntryAddress);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return rootFolder;
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenPrimarySmtpAddressIsPresent_ReturnsIt()
        {
            // Arrange (issue #797, AC6 case 1): the first source in the fallback order succeeds.
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress("primary@example.com");
            var wrapper = new StoreWrapper(store.Object)
            {
                RootFolder = rootFolder.Object,
                DisplayName = "Mailbox",
            };

            // Act
            var result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().Be("primary@example.com");
            wrapper
                .LastSmtpLookupError.Should()
                .BeNull("a successful lookup clears the captured reason.");
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenPrimarySmtpThrows_FallsBackToAddressEntryAddress()
        {
            // Arrange (issue #797, AC6 case 2): the Exchange read fails and the address entry's own
            // address contains an at-sign, so it is used.
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithFailingPrimarySmtpAddress("entry@example.com");
            var wrapper = new StoreWrapper(store.Object)
            {
                RootFolder = rootFolder.Object,
                DisplayName = "Mailbox",
            };

            // Act
            var result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().Be("entry@example.com");
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenPrimaryAndAddressEntryFail_FallsBackToDisplayName()
        {
            // Arrange (issue #797, AC6 case 3): the Exchange read fails and the address entry's
            // address carries no at-sign, so the store display name is used because it does.
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithFailingPrimarySmtpAddress("/o=EX/cn=Recipients");
            var wrapper = new StoreWrapper(store.Object)
            {
                RootFolder = rootFolder.Object,
                DisplayName = "display@example.com",
            };

            // Act
            var result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().Be("display@example.com");
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenEveryFallbackFails_ReturnsNullAndCapturesReason()
        {
            // Arrange (issue #797, AC6 case 4): no source yields an at-sign-bearing value.
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithFailingPrimarySmtpAddress("/o=EX/cn=Recipients");
            var wrapper = new StoreWrapper(store.Object)
            {
                RootFolder = rootFolder.Object,
                DisplayName = "Mailbox",
            };

            // Act
            var result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().BeNull();
            wrapper
                .LastSmtpLookupError.Should()
                .NotBeNullOrEmpty(
                    "the dialog renders the captured reason in place of the generic placeholder."
                );
        }

        [TestMethod]
        public void RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow()
        {
            // Arrange (issue #797, AC6): the dialog calls the retry entry point on whichever store
            // is selected, which may have no root folder.
            var store = new Mock<OutlookStore>();
            var wrapper = new StoreWrapper(store.Object) { RootFolder = null };

            // Act
            var act = () => wrapper.RefreshUserEmailAddress();

            // Assert
            act.Should().NotThrow();
            wrapper.RefreshUserEmailAddress().Should().BeNull();
            wrapper.UserEmailAddress.Should().BeNull();
        }

        [TestMethod]
        public void TryRestore_WhenRestoreSucceeds_ShouldReturnTrue()
        {
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress("user@example.com");
            var inbox = new Mock<OutlookFolder>();

            store.SetupGet(x => x.DisplayName).Returns("Mailbox");
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(inbox.Object);

            var wrapper = new StoreWrapper(store.Object);

            var result = wrapper.TryRestore(store.Object);

            result.Should().BeTrue();
            wrapper.DisplayName.Should().Be("Mailbox");
        }

        [TestMethod]
        public void TryRestore_WhenRestoreThrows_ShouldReturnFalse()
        {
            var store = new Mock<OutlookStore>();
            store.SetupGet(x => x.DisplayName).Throws(new COMException("fail"));

            var wrapper = new StoreWrapper(store.Object);
            wrapper.DisplayName = "Old";

            var result = wrapper.TryRestore(store.Object);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void ConfigurableProperties_ShouldHaveDefaultValues()
        {
            var store = new Mock<OutlookStore>();
            var wrapper = new StoreWrapper(store.Object);

            wrapper.ArchiveRoot.Should().NotBeNull();
            wrapper.ArchiveFsRoot.Should().NotBeNull();
            wrapper.JunkPotential.Should().NotBeNull();
            wrapper.JunkCertain.Should().NotBeNull();
        }

        [TestMethod]
        public void GetSmtpAddressFromStore_WhenRootFolderIsNull_ShouldReturnNull()
        {
            var store = new Mock<OutlookStore>();
            var wrapper = new StoreWrapper(store.Object) { RootFolder = null };

            var result = wrapper.GetSmtpAddressFromStore();

            result.Should().BeNull();
        }

        [TestMethod]
        public void Init_WhenStoreIdIsReadable_CapturesStoreId()
        {
            // Arrange (issue #328)
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress("owner@example.com");
            var inbox = new Mock<OutlookFolder>();

            store.SetupGet(x => x.DisplayName).Returns("Mailbox");
            store.SetupGet(x => x.StoreID).Returns("STORE-ID-123");
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(inbox.Object);

            var wrapper = new StoreWrapper(store.Object);

            // Act
            wrapper.Init();

            // Assert
            wrapper.StoreId.Should().Be("STORE-ID-123");
        }

        [TestMethod]
        public void Init_WhenStoreIdReadThrows_IsFailSafeAndLeavesStoreIdNull()
        {
            // Arrange (issue #328): an unreadable StoreID must not throw out of Init.
            var store = new Mock<OutlookStore>();
            var rootFolder = CreateRootFolderWithPrimarySmtpAddress("owner@example.com");
            var inbox = new Mock<OutlookFolder>();

            store.SetupGet(x => x.DisplayName).Returns("Mailbox");
            store.SetupGet(x => x.StoreID).Throws(new COMException("StoreID unavailable"));
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(inbox.Object);

            var wrapper = new StoreWrapper(store.Object);

            // Act
            var result = wrapper.Init();

            // Assert
            result.Should().BeSameAs(wrapper);
            wrapper.StoreId.Should().BeNull();
            wrapper.DisplayName.Should().Be("Mailbox");
        }

        [TestMethod]
        public void StoreId_SerializeRoundTrip_PreservesValue()
        {
            // Arrange (issue #328)
            var store = new Mock<OutlookStore>();
            var wrapper = new StoreWrapper(store.Object) { StoreId = "00FFAABB-STORE-ID" };

            // Act
            var json = JsonConvert.SerializeObject(wrapper);
            var restored = JsonConvert.DeserializeObject<StoreWrapper>(json);

            // Assert
            json.Should().Contain("StoreId");
            json.Should().Contain("00FFAABB-STORE-ID");
            restored.StoreId.Should().Be("00FFAABB-STORE-ID");
        }

        [TestMethod]
        public void StoreId_DeserializeLegacyJsonWithoutKey_DefaultsToNull()
        {
            // Legacy payload predating issue #328: no StoreId key present.
            const string legacyJson = "{\"DisplayName\":\"Mailbox\"}";

            var restored = JsonConvert.DeserializeObject<StoreWrapper>(legacyJson);

            restored.DisplayName.Should().Be("Mailbox");
            restored.StoreId.Should().BeNull();
        }
    }
}
