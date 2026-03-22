using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class MovedMailInfo_Tests
    {
        [TestMethod]
        public void Constructor_WithNoArguments_LeavesDefaultsAndIsNotReady()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();

            // Assert
            movedMailInfo.FolderPathOld.Should().BeNull();
            movedMailInfo.FolderPathNew.Should().BeNull();
            movedMailInfo.EntryId.Should().BeNull();
            movedMailInfo.StoreId.Should().BeNull();
            movedMailInfo.OlRootPath.Should().BeNull();
            movedMailInfo.MailItem.Should().BeNull();
            movedMailInfo.FolderOld.Should().BeNull();
            movedMailInfo.IsReadyToUndoMove.Should().BeFalse();
        }

        [TestMethod]
        public void Properties_WhenAssigned_PreserveValues()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();

            // Act
            movedMailInfo.FolderPathOld = "Inbox\\Old";
            movedMailInfo.FolderPathNew = "Inbox\\New";
            movedMailInfo.EntryId = "entry-id";
            movedMailInfo.StoreId = "store-id";
            movedMailInfo.OlRootPath = "Mailbox - Root";

            // Assert
            movedMailInfo.FolderPathOld.Should().Be("Inbox\\Old");
            movedMailInfo.FolderPathNew.Should().Be("Inbox\\New");
            movedMailInfo.EntryId.Should().Be("entry-id");
            movedMailInfo.StoreId.Should().Be("store-id");
            movedMailInfo.OlRootPath.Should().Be("Mailbox - Root");
        }

        [TestMethod]
        public void Constructor_WithBeforeAndAfterMove_PopulatesDerivedFields()
        {
            // Arrange
            var beforeFolder = CreateFolder(@"\\Mailbox - Root\Inbox");
            var afterFolder = CreateFolder(@"\\Mailbox - Root\Archive");
            var beforeMove = CreateMailItem("before-id", beforeFolder.Object);
            var afterMove = CreateMailItem("after-id", afterFolder.Object);

            // Act
            var movedMailInfo = new MovedMailInfo(
                beforeMove.Object,
                afterMove.Object,
                @"\\Mailbox - Root"
            );

            // Assert
            movedMailInfo.OlRootPath.Should().Be(@"\\Mailbox - Root");
            movedMailInfo.MailItem.Should().BeSameAs(afterMove.Object);
            movedMailInfo.FolderPathNew.Should().Be("Archive");
            movedMailInfo.StoreId.Should().Be("store-id");
            movedMailInfo.EntryId.Should().Be("after-id");
            movedMailInfo.FolderOld.Should().BeSameAs(beforeFolder.Object);
            movedMailInfo.FolderPathOld.Should().Be("Inbox");
        }

        [TestMethod]
        public void NotNull_WhenAnyParameterIsNull_ReturnsFalse()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();

            // Act
            var result = movedMailInfo.NotNull("value", null, 42);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void NotNull_WhenAllParametersHaveValues_ReturnsTrue()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();

            // Act
            var result = movedMailInfo.NotNull("value", 42, new object());

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void MailItemGetter_WhenMailItemWasAssigned_ReturnsSameReference()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();
            var mailItem = CreateComProxy<MailItem>();
            movedMailInfo.MailItem = mailItem;

            // Act
            var result = movedMailInfo.MailItem;

            // Assert
            result.Should().BeSameAs(mailItem);
        }

        [TestMethod]
        public void FolderOldGetter_WhenFolderWasAssigned_ReturnsSameReference()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();
            var folder = CreateComProxy<Folder>();
            movedMailInfo.FolderOld = folder;

            // Act
            var result = movedMailInfo.FolderOld;

            // Assert
            result.Should().BeSameAs(folder);
        }

        [TestMethod]
        public void OlAppSetter_WhenAssigned_SetsApplicationAndRootPath()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();
            var application = CreateApplication(@"\\Mailbox - Root");

            // Act
            movedMailInfo.OlApp = application.Object;

            // Assert
            movedMailInfo.OlApp.Should().BeSameAs(application.Object);
            movedMailInfo.OlRootPath.Should().Be(@"\\Mailbox - Root");
        }

        [TestMethod]
        public void GlobalsSetter_WhenAssigned_PreservesReference()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();
            var globals = new Mock<IApplicationGlobals>();

            // Act
            movedMailInfo.Globals = globals.Object;

            // Assert
            movedMailInfo.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void MailItemGetter_WhenOlAppCanResolveEntryId_LoadsMailItem()
        {
            // Arrange
            var resolvedMail = CreateMailItem(
                "entry-id",
                CreateFolder(@"\\Mailbox - Root\Archive").Object
            );
            var application = CreateApplication(@"\\Mailbox - Root", resolvedMail.Object);
            var movedMailInfo = new MovedMailInfo
            {
                EntryId = "entry-id",
                StoreId = "store-id",
                OlApp = application.Object,
            };

            // Act
            var result = movedMailInfo.MailItem;

            // Assert
            result.Should().BeSameAs(resolvedMail.Object);
            Mock.Get(application.Object.Session)
                .Verify(x => x.GetItemFromID("entry-id", "store-id"), Times.Once);
        }

        [TestMethod]
        public void MailItemGetter_WhenOutlookLookupThrows_ReturnsNull()
        {
            // Arrange
            var application = CreateApplication(@"\\Mailbox - Root");
            Mock.Get(application.Object.Session)
                .Setup(x => x.GetItemFromID("entry-id", "store-id"))
                .Throws(new InvalidOperationException("lookup failed"));

            var movedMailInfo = new MovedMailInfo
            {
                EntryId = "entry-id",
                StoreId = "store-id",
                OlApp = application.Object,
            };

            // Act
            var result = movedMailInfo.MailItem;

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void IsReadyToUndoMove_WhenMailItemAndFolderOldExist_ReturnsTrue()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo
            {
                MailItem = CreateComProxy<MailItem>(),
                FolderOld = CreateComProxy<Folder>(),
            };

            // Act
            var isReady = movedMailInfo.IsReadyToUndoMove;

            // Assert
            isReady.Should().BeTrue();
        }

        [TestMethod]
        public void UndoMove_WhenMoveIsNotReady_ReturnsNull()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();

            // Act
            var result = movedMailInfo.UndoMove();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void UndoMove_WhenMoveIsReady_MovesMailBackToOriginalFolder()
        {
            // Arrange
            var folderOld = CreateFolder(@"\\Mailbox - Root\Inbox");
            var movedMail = CreateComProxy<MailItem>();
            var mailItem = new Mock<MailItem>();
            mailItem.Setup(x => x.Move(folderOld.Object)).Returns(movedMail);

            var movedMailInfo = new MovedMailInfo
            {
                MailItem = mailItem.Object,
                FolderOld = folderOld.Object,
            };

            // Act
            var result = movedMailInfo.UndoMove();

            // Assert
            result.Should().BeSameAs(movedMail);
        }

        [TestMethod]
        public void UndoMoveMessage_WhenReady_ReturnsFormattedMessage()
        {
            // Arrange
            var sentOn = new DateTime(2026, 3, 22);
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.SentOn).Returns(sentOn);
            mailItem.SetupGet(x => x.Subject).Returns("Quarterly Update");

            var movedMailInfo = new MovedMailInfo
            {
                MailItem = mailItem.Object,
                FolderOld = CreateFolder(@"\\Mailbox - Root\Inbox").Object,
                FolderPathNew = "Archive",
                FolderPathOld = "Inbox",
            };

            // Act
            var message = movedMailInfo.UndoMoveMessage(null);

            // Assert
            message
                .Should()
                .Be(
                    "Undo Move of email?"
                        + Environment.NewLine
                        + "SentOn: 03/22/2026"
                        + Environment.NewLine
                        + "Quarterly Update"
                        + Environment.NewLine
                        + "From: Archive"
                        + Environment.NewLine
                        + "To: Inbox"
                );
        }

        [TestMethod]
        public void UndoMoveMessage_WhenStillNotReadyAfterAssigningApp_ReturnsNull()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo();
            var application = CreateApplication(@"\\Mailbox - Root");

            // Act
            var message = movedMailInfo.UndoMoveMessage(application.Object);

            // Assert
            message.Should().BeNull();
            movedMailInfo.OlApp.Should().BeSameAs(application.Object);
            movedMailInfo.OlRootPath.Should().Be(@"\\Mailbox - Root");
        }

        [TestMethod]
        public void JsonSerializeObject_OmitsJsonIgnoredComProperties()
        {
            // Arrange
            var movedMailInfo = new MovedMailInfo
            {
                FolderPathOld = "Inbox\\Old",
                FolderPathNew = "Inbox\\New",
                EntryId = "entry-id",
                StoreId = "store-id",
                OlRootPath = "Mailbox - Root",
                MailItem = CreateComProxy<MailItem>(),
                FolderOld = CreateComProxy<Folder>(),
            };

            // Act
            var json = JsonConvert.SerializeObject(movedMailInfo);

            // Assert
            json.Should().Contain("FolderPathOld");
            json.Should().Contain("FolderPathNew");
            json.Should().Contain("EntryId");
            json.Should().Contain("StoreId");
            json.Should().Contain("IsReadyToUndoMove");
            json.Should().NotContain("MailItem");
            json.Should().NotContain("FolderOld");
            json.Should().NotContain("OlApp");
            json.Should().NotContain("Globals");
        }

        [TestMethod]
        public void IsReadyToUndoMove_WhenOnlyMailItemSet_ReturnsFalse()
        {
            // Arrange
            var info = new MovedMailInfo { MailItem = CreateComProxy<MailItem>() };

            // Act / Assert
            info.IsReadyToUndoMove.Should().BeFalse();
        }

        [TestMethod]
        public void IsReadyToUndoMove_WhenOnlyFolderOldSet_ReturnsFalse()
        {
            // Arrange
            var info = new MovedMailInfo { FolderOld = CreateComProxy<Folder>() };

            // Act / Assert
            info.IsReadyToUndoMove.Should().BeFalse();
        }

        [TestMethod]
        public void NotNull_WhenAllParametersNonNull_ReturnsTrue()
        {
            // Arrange
            var info = new MovedMailInfo();

            // Act
            var result = info.NotNull("a", 1, new object());

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void NotNull_EmptyParams_ReturnsTrue()
        {
            // Arrange
            var info = new MovedMailInfo();

            // Act
            var result = info.NotNull();

            // Assert
            result.Should().BeTrue();
        }

        private static T CreateComProxy<T>()
            where T : class
        {
            return new Mock<T>(MockBehavior.Loose).Object;
        }

        private static Mock<Folder> CreateFolder(string folderPath, string storeId = "store-id")
        {
            var folder = new Mock<Folder>();
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.StoreID).Returns(storeId);
            return folder;
        }

        private static Mock<MailItem> CreateMailItem(string entryId, Folder parent)
        {
            var mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.Parent).Returns(parent);
            mailItem.SetupGet(x => x.EntryID).Returns(entryId);
            return mailItem;
        }

        private static Mock<Application> CreateApplication(
            string rootFolderPath,
            MailItem resolvedMail = null
        )
        {
            var rootFolder = CreateFolder(rootFolderPath);
            var store = new Mock<Store>();
            store.Setup(x => x.GetRootFolder()).Returns(rootFolder.Object);

            var session = new Mock<NameSpace>();
            session.SetupGet(x => x.DefaultStore).Returns(store.Object);
            session
                .Setup(x => x.GetItemFromID(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(resolvedMail);

            var application = new Mock<Application>();
            application.SetupGet(x => x.Session).Returns(session.Object);

            return application;
        }
    }
}
