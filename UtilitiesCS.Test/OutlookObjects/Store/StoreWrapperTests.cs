using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
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
            store.SetupGet(x => x.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store.Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox)).Returns(inbox.Object);

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
            store.SetupGet(x => x.ExchangeStoreType).Returns(OlExchangeStoreType.olExchangePublicFolder);

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

            var wrapper = new StoreWrapper(store.Object)
            {
                RootFolder = rootFolder.Object,
            };

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

            var wrapper = new StoreWrapper(store.Object)
            {
                RootFolder = rootFolder.Object,
            };

            // Act
            string result = wrapper.GetSmtpAddressFromStore();

            // Assert
            result.Should().BeNull();
        }

        private static Mock<OutlookFolder> CreateRootFolderWithPrimarySmtpAddress(string primarySmtpAddress)
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
    }
}
