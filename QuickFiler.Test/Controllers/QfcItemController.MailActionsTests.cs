using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
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
    }
}
