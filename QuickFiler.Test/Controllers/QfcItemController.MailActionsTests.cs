using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Mail-actions cluster tests (research §5.2). Covers PackageItems single-item packaging and
    /// MarkItemForDeletion add-when-absent / select-when-present routing through the narrowed
    /// IItemViewer folder intent members.
    /// </summary>
    [TestClass]
    public class QfcItemController_MailActionsTests
    {
        private sealed class MailController : QfcItemController
        {
            internal MailController()
                : base() { }
        }

        private static void SetField(QfcItemController controller, string name, object value) =>
            typeof(QfcItemController)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, value);

        [TestMethod]
        public void PackageItems_WhenConversationUnchecked_ReturnsSingleItem()
        {
            // Arrange — conversation mode off: PackageItems returns only the controller's own item.
            var controller = new MailController();
            var helper = new MailItemHelper();
            controller.ItemHelper = helper;
            SetField(controller, "_optionConversationChecked", false);

            // Act
            IList<MailItemHelper> packaged = controller.PackageItems();

            // Assert
            packaged.Should().ContainSingle().Which.Should().BeSameAs(helper);
        }

        [TestMethod]
        public void MarkItemForDeletion_WhenTrashFolderAbsent_AddsAndSelectsIt()
        {
            // Arrange — the "Trash to Delete" pseudo-folder is not present; it must be added, then
            // selected.
            var mock = new Mock<IItemViewer>();
            mock.Setup(v => v.FolderContains("Trash to Delete")).Returns(false);
            var controller = new MailController();
            SetField(controller, "_itemViewer", mock.Object);

            // Act
            controller.MarkItemForDeletion();

            // Assert
            mock.Verify(
                v =>
                    v.SetFolderItems(
                        It.Is<string[]>(a => a.Length == 1 && a[0] == "Trash to Delete")
                    ),
                Times.Once()
            );
            mock.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }

        [TestMethod]
        public void MarkItemForDeletion_WhenTrashFolderPresent_SelectsWithoutAdding()
        {
            // Arrange — the pseudo-folder already exists; it must be selected without re-adding.
            var mock = new Mock<IItemViewer>();
            mock.Setup(v => v.FolderContains("Trash to Delete")).Returns(true);
            var controller = new MailController();
            SetField(controller, "_itemViewer", mock.Object);

            // Act
            controller.MarkItemForDeletion();

            // Assert
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Never());
            mock.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }

        // ---------------------------------------------------------------------------
        // Cycle-2 Phase 5 (AC8) de-exemption coverage: RightKeyActions / RightKeyActionsAsync getters
        // (dictionary-membership; the lambda bodies are not invoked so no COM is touched), and the
        // CollapseConversation / EnumerateConversation collaborator routing.
        // ---------------------------------------------------------------------------

        private static ConversationResolver BuildResolverWithCount(int sameFolder)
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockMail = new Mock<MailItem>();
            var resolver = new ConversationResolver(mockGlobals.Object, mockMail.Object);
            resolver.Count = new Pair<int>(sameFolder: sameFolder, expanded: sameFolder);
            return resolver;
        }

        [TestMethod]
        public void RightKeyActions_Getter_ContainsExpectedMenuKeys()
        {
            // Arrange
            var controller = new MailController();

            // Act
            Dictionary<string, System.Action> actions = controller.RightKeyActions;

            // Assert
            actions.Should().ContainKey("&Pop Out");
            actions.Should().ContainKey("&Expand");
            actions.Should().ContainKey("&Cancel");
        }

        [TestMethod]
        public void RightKeyActionsAsync_Getter_ContainsExpectedMenuKeys()
        {
            // Arrange
            var controller = new MailController();

            // Act
            Dictionary<string, System.Func<Task>> actions = controller.RightKeyActionsAsync;

            // Assert
            actions.Should().ContainKey("&Pop Out");
            actions.Should().ContainKey("&Expand");
            actions.Should().ContainKey("&Cancel");
        }

        [TestMethod]
        public void CollapseConversation_WhenConvOriginIdSet_TogglesGroupWithThatId()
        {
            // Arrange — a non-empty _convOriginID selects the origin id branch, avoiding the COM
            // Mail.EntryID fallback (deferred to the Phase 6 IMailItemActions seam).
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetFolderItems()).Returns(new[] { @"\\Archive\A" });
            var parent = new Mock<IQfcCollectionController>();
            var controller = new MailController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_parent", parent.Object);
            controller.ConvOriginID = "origin-123";

            // Act
            controller.CollapseConversation();

            // Assert
            parent.Verify(p => p.ToggleGroupConv("origin-123"), Times.Once());
        }

        [TestMethod]
        public void EnumerateConversation_TogglesUnGroupWithResolverEntryIdAndCount()
        {
            // Arrange — the resolver and entry id are read from mockable collaborators; the EntryID now
            // comes from the Phase 6 IMailItemActions seam (P6-T7) instead of a live Mail.EntryID.
            var folderItems = new[] { @"\\Archive\A" };
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetFolderItems()).Returns(folderItems);
            var parent = new Mock<IQfcCollectionController>();
            var mailActions = new Mock<IMailItemActions>();
            mailActions.SetupGet(m => m.EntryID).Returns("entry-xyz");
            var resolver = BuildResolverWithCount(4);
            var controller = new MailController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_parent", parent.Object);
            SetField(controller, "_conversationResolver", resolver);
            SetField(controller, "_mailActions", mailActions.Object);

            // Act
            controller.EnumerateConversation();

            // Assert
            parent.Verify(
                p => p.ToggleUnGroupConv(resolver, "entry-xyz", 4, folderItems),
                Times.Once()
            );
        }
    }
}
