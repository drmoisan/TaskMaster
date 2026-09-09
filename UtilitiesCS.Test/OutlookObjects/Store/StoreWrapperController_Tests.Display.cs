using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookRecipient = Microsoft.Office.Interop.Outlook.Recipient;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Display-partial behaviour of <see cref="StoreWrapperController"/> (issue #797): the AC6 retry
    /// of the SMTP lookup on dialog open, the AC7 store-prefix trim on the Inbox and Root Folder
    /// labels, and the AC8 guards that render placeholders instead of throwing when no store is
    /// selected. Partial of <see cref="StoreWrapperController_Tests"/> so it reuses the base mock
    /// harness. No live Outlook process and no user interface is required, and no file is created.
    /// </summary>
    public partial class StoreWrapperController_Tests
    {
        #region AC6 — retry on dialog open

        /// <summary>
        /// Builds a mocked Outlook folder chain whose Exchange primary SMTP read succeeds.
        /// </summary>
        private static Mock<OutlookFolder> CreateDisplaySmtpRootFolder(string primarySmtpAddress)
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<OutlookRecipient>();
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
        /// Builds a mocked Outlook folder chain in which every SMTP source fails, so the lookup
        /// yields null and captures a failure reason.
        /// </summary>
        private static Mock<OutlookFolder> CreateDisplayFailingSmtpRootFolder(string reason)
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<OutlookRecipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Throws(new COMException(reason));
            addressEntry.SetupGet(x => x.Address).Returns("/o=EX/cn=Recipients");
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return rootFolder;
        }

        /// <summary>
        /// Sibling of <see cref="CreateDisplayFailingSmtpRootFolder"/> that additionally returns
        /// the <c>ExchangeUser</c> mock, so a test can count invocations of the
        /// <c>PrimarySmtpAddress</c> getter and therefore count how many times the SMTP lookup was
        /// actually attempted. The original helper is deliberately left unchanged, because the
        /// three #797 AC6 tests below depend on its exact behaviour.
        /// </summary>
        private static (
            Mock<OutlookFolder> RootFolder,
            Mock<ExchangeUser> ExchangeUser
        ) CreateDisplayFailingSmtpRootFolderWithUser(string reason)
        {
            var rootFolder = new Mock<OutlookFolder>();
            var session = new Mock<NameSpace>();
            var currentUser = new Mock<OutlookRecipient>();
            var addressEntry = new Mock<AddressEntry>();
            var exchangeUser = new Mock<ExchangeUser>();

            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Throws(new COMException(reason));
            addressEntry.SetupGet(x => x.Address).Returns("/o=EX/cn=Recipients");
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            currentUser.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            session.SetupGet(x => x.CurrentUser).Returns(currentUser.Object);
            rootFolder.SetupGet(x => x.Session).Returns(session.Object);

            return (rootFolder, exchangeUser);
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress()
        {
            // Arrange (issue #797, AC6): the address is null because the startup lookup failed, so
            // opening the dialog must retry it rather than render the generic placeholder forever.
            var (controller, _) = CreateControllerWithViewer();
            var rootFolder = CreateDisplaySmtpRootFolder("retried@example.com");
            controller.Current = new StoreWrapper(null)
            {
                RootFolder = rootFolder.Object,
                UserEmailAddress = null,
                DisplayName = "Mailbox",
            };

            // Act
            controller.PopulateWithCurrent();

            // Assert
            controller.Current.UserEmailAddress.Should().Be("retried@example.com");
            controller.Viewer.UserEmail.Text.Should().Be("retried@example.com");
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup()
        {
            // Arrange (issue #797, AC6, corrected by #812): the retry is attempted at most once
            // per controller instance and only when the address is null, which bounds the added
            // UI-thread latency. The mocked chain would yield a different address, so an
            // unchanged value proves no retry ran.
            var (controller, _) = CreateControllerWithViewer();
            var rootFolder = CreateDisplaySmtpRootFolder("would-have-retried@example.com");
            controller.Current = new StoreWrapper(null)
            {
                RootFolder = rootFolder.Object,
                UserEmailAddress = "already@example.com",
                DisplayName = "Mailbox",
            };

            // Act
            controller.PopulateWithCurrent();

            // Assert
            controller.Current.UserEmailAddress.Should().Be("already@example.com");
            controller.Viewer.UserEmail.Text.Should().Be("already@example.com");
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason()
        {
            // Arrange (issue #797, AC6): on total failure the label must carry a specific message
            // including the reason, not the generic placeholder shared with the other two labels.
            var (controller, _) = CreateControllerWithViewer();
            var rootFolder = CreateDisplayFailingSmtpRootFolder("The operation failed.");
            controller.Current = new StoreWrapper(null)
            {
                RootFolder = rootFolder.Object,
                UserEmailAddress = null,
                DisplayName = "Mailbox",
            };

            // Act
            controller.PopulateWithCurrent();

            // Assert
            controller.Viewer.UserEmail.Text.Should().Contain("The operation failed.");
            controller.Viewer.UserEmail.Text.Should().NotBe("Error Loading");
        }

        /// <summary>
        /// AC4 of issue #812: the #797 AC6 retry is unbounded. <c>PopulateWithCurrent</c> runs on
        /// every store re-selection, not only on dialog open, and a failing lookup leaves
        /// <c>UserEmailAddress</c> null, so the guard condition stays true and the COM lookup is
        /// re-attempted on every pass. One controller instance must attempt it at most once.
        /// </summary>
        [TestMethod]
        public void PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce()
        {
            // Arrange
            var (controller, _) = CreateControllerWithViewer();
            var (rootFolder, exchangeUser) = CreateDisplayFailingSmtpRootFolderWithUser(
                "The operation failed."
            );
            controller.Current = new StoreWrapper(null)
            {
                RootFolder = rootFolder.Object,
                UserEmailAddress = null,
                DisplayName = "Mailbox",
            };

            // Act
            controller.PopulateWithCurrent();
            controller.PopulateWithCurrent();

            // Assert
            controller
                .Current.UserEmailAddress.Should()
                .BeNull("the lookup fails, which is what leaves the guard condition true");
            exchangeUser.VerifyGet(x => x.PrimarySmtpAddress, Times.Once());
        }

        /// <summary>
        /// The bound is per controller instance, not per store and not per process. A second
        /// controller over the same failing store gets its own single attempt, which is what makes
        /// the bound equal to "once per dialog open" given that
        /// <c>RibbonController.FolderStoresSettings</c> constructs a fresh controller per open.
        /// </summary>
        [TestMethod]
        public void PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore()
        {
            // Arrange
            var (firstController, _) = CreateControllerWithViewer();
            var (secondController, _) = CreateControllerWithViewer();
            var (rootFolder, exchangeUser) = CreateDisplayFailingSmtpRootFolderWithUser(
                "The operation failed."
            );
            var sharedStore = new StoreWrapper(null)
            {
                RootFolder = rootFolder.Object,
                UserEmailAddress = null,
                DisplayName = "Mailbox",
            };
            firstController.Current = sharedStore;
            secondController.Current = sharedStore;

            // Act
            firstController.PopulateWithCurrent();
            secondController.PopulateWithCurrent();

            // Assert
            exchangeUser.VerifyGet(x => x.PrimarySmtpAddress, Times.Exactly(2));
        }

        /// <summary>
        /// Issue #823 (R1): the retry budget is one attempt per controller instance per store. A
        /// single controller shown two distinct failing stores must therefore attempt the SMTP
        /// lookup once against each of them. Before the fix, one controller-scoped flag was
        /// consumed by whichever store was displayed first and the second store's lookup never ran,
        /// so its address label stayed at the generic placeholder for the life of the dialog.
        /// </summary>
        [TestMethod]
        public void PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce()
        {
            // Arrange
            var (controller, _) = CreateControllerWithViewer();
            var (rootFolderA, exchangeUserA) = CreateDisplayFailingSmtpRootFolderWithUser(
                "Store A lookup failed."
            );
            var (rootFolderB, exchangeUserB) = CreateDisplayFailingSmtpRootFolderWithUser(
                "Store B lookup failed."
            );
            var storeA = new StoreWrapper(null)
            {
                RootFolder = rootFolderA.Object,
                UserEmailAddress = null,
                DisplayName = "Mailbox A",
            };
            var storeB = new StoreWrapper(null)
            {
                RootFolder = rootFolderB.Object,
                UserEmailAddress = null,
                DisplayName = "Mailbox B",
            };

            // Act
            controller.Current = storeA;
            controller.PopulateWithCurrent();
            controller.Current = storeB;
            controller.PopulateWithCurrent();

            // Assert
            exchangeUserA.VerifyGet(x => x.PrimarySmtpAddress, Times.Once());
            exchangeUserB.VerifyGet(x => x.PrimarySmtpAddress, Times.Once());
        }

        /// <summary>
        /// The latch does not replace the null check: when the address is already populated the
        /// lookup must not be attempted at all, so the added UI-thread latency stays at zero for
        /// the ordinary case.
        /// </summary>
        [TestMethod]
        public void PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup()
        {
            // Arrange
            var (controller, _) = CreateControllerWithViewer();
            var (rootFolder, exchangeUser) = CreateDisplayFailingSmtpRootFolderWithUser(
                "The operation failed."
            );
            controller.Current = new StoreWrapper(null)
            {
                RootFolder = rootFolder.Object,
                UserEmailAddress = "already@example.com",
                DisplayName = "Mailbox",
            };

            // Act
            controller.PopulateWithCurrent();

            // Assert
            controller.Current.UserEmailAddress.Should().Be("already@example.com");
            exchangeUser.VerifyGet(x => x.PrimarySmtpAddress, Times.Never());
        }

        #endregion AC6

        #region AC7 — the leading store prefix is trimmed for display

        [TestMethod]
        public void TrimStorePrefix_WithLeadingStorePrefix_RemovesIt()
        {
            StoreWrapperController
                .TrimStorePrefix(@"\\mailbox@example.com\Inbox")
                .Should()
                .Be(@"mailbox@example.com\Inbox");
        }

        [TestMethod]
        public void TrimStorePrefix_WithNoLeadingBackslash_ReturnsInputUnchanged()
        {
            StoreWrapperController
                .TrimStorePrefix(@"mailbox@example.com\Inbox")
                .Should()
                .Be(@"mailbox@example.com\Inbox");
        }

        [TestMethod]
        public void TrimStorePrefix_WithSingleLeadingBackslash_ReturnsInputUnchanged()
        {
            StoreWrapperController
                .TrimStorePrefix(@"\mailbox@example.com\Inbox")
                .Should()
                .Be(@"\mailbox@example.com\Inbox");
        }

        [TestMethod]
        public void TrimStorePrefix_WithEmptyString_ReturnsEmptyString()
        {
            StoreWrapperController.TrimStorePrefix(string.Empty).Should().BeEmpty();
        }

        [TestMethod]
        public void TrimStorePrefix_WithNull_ReturnsNull()
        {
            StoreWrapperController.TrimStorePrefix(null).Should().BeNull();
        }

        [TestMethod]
        public void TrimStorePrefix_WithOnlyTheStorePrefix_ReturnsEmptyString()
        {
            StoreWrapperController.TrimStorePrefix(@"\\").Should().BeEmpty();
        }

        [TestMethod]
        public void PopulateWithCurrent_RendersInboxAndRootFolderWithoutStorePrefix()
        {
            // Arrange (issue #797, AC7): MAPIFolder.FolderPath is Outlook's native form and is read
            // straight into the label today, so the store prefix is visible to the user.
            var (controller, _) = CreateControllerWithViewer();
            var inbox = new Mock<OutlookFolder>();
            var rootFolder = new Mock<OutlookFolder>();
            inbox.SetupGet(x => x.FolderPath).Returns(@"\\mailbox@example.com\Inbox");
            rootFolder.SetupGet(x => x.FolderPath).Returns(@"\\mailbox@example.com");
            controller.Current = new StoreWrapper(null)
            {
                Inbox = inbox.Object,
                RootFolder = rootFolder.Object,
                UserEmailAddress = "user@example.com",
                DisplayName = "Mailbox",
            };

            // Act
            controller.PopulateWithCurrent();

            // Assert
            controller.Viewer.Inbox.Text.Should().Be(@"mailbox@example.com\Inbox");
            controller.Viewer.RootFolder.Text.Should().Be("mailbox@example.com");
        }

        #endregion AC7

        #region AC8 — a null current store renders placeholders instead of throwing

        [TestMethod]
        public void PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow()
        {
            // Arrange (issue #797, AC8): List.Find returns null when no store matches, and the four
            // dereferences at the top of the method threw before any placeholder could render.
            var (controller, _) = CreateControllerWithViewer();
            controller.Current = null;
            controller.FsConverter = _ => (string.Empty, string.Empty);

            // Act
            var act = () => controller.PopulateWithCurrent();

            // Assert
            act.Should().NotThrow();
            controller.Viewer.Inbox.Text.Should().Be("Error Loading");
            controller.Viewer.RootFolder.Text.Should().Be("Error Loading");
            controller.Viewer.ArchiveOutlook.Text.Should().Be("Please select an archive");
            controller.Viewer.ArchiveFS.Text.Should().Be("Please select an archive");
            controller.Viewer.JunkEmail.Text.Should().Be("Please select a folder");
            controller.Viewer.JunkPotential.Text.Should().Be("Please select a folder");
        }

        [TestMethod]
        public void GetRelativeFsPath_WithNullCurrent_ReturnsPlaceholderAndDoesNotThrow()
        {
            // Arrange (issue #797, AC8): the same unguarded dereference exists in the relative-path
            // helper, which the populate path calls.
            var controller = CreateController();
            controller.Current = null;
            controller.FsConverter = _ => (string.Empty, string.Empty);

            // Act
            var act = () => controller.GetRelativeFsPath();

            // Assert
            act.Should().NotThrow();
            controller.GetRelativeFsPath().Should().Be("Please select an archive");
        }

        #endregion AC8
    }
}
