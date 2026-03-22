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
    }
}
