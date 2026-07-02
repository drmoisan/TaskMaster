using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// EventHandlers-cluster tests (cycle-2 Phase 5, AC8). Covers the de-exempted checkbox/combo
    /// field-write handlers, the mouse enter/leave theme-color handlers (via reflection-injected
    /// _themes and bare handle-less senders), the delete-item click handler, the search key-down
    /// handler, and the topic-thread selection handler. Private handlers are invoked by reflection
    /// because a live control cannot raise their events in a unit test.
    /// </summary>
    [TestClass]
    public class QfcItemController_EventHandlersTests
    {
        private static readonly Color MouseOver = Color.FromArgb(10, 20, 30);
        private static readonly Color Clicked = Color.FromArgb(40, 50, 60);
        private static readonly Color Back = Color.FromArgb(70, 80, 90);

        private static HarnessController BuildThemedController()
        {
            HarnessController controller = new HarnessController();
            Theme theme = QfcItemControllerTestSupport.BuildColorTheme(MouseOver, Clicked, Back);
            QfcItemControllerTestSupport.InjectThemes(
                controller,
                QfcItemControllerTestSupport.BuildThemeDictionary("LightNormal", theme),
                "LightNormal"
            );
            return controller;
        }

        // ------------------------- Mouse enter/leave (theme colors) -------------------------

        [TestMethod]
        public void Button_MouseEnter_SetsMouseOverColor()
        {
            // Arrange
            HarnessController controller = BuildThemedController();
            Button button = new Button();

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "Button_MouseEnter",
                button,
                EventArgs.Empty
            );

            // Assert
            button.BackColor.Should().Be(MouseOver);
        }

        [TestMethod]
        public void Button_MouseLeave_WhenDialogResultOk_SetsClickedColor()
        {
            // Arrange
            HarnessController controller = BuildThemedController();
            Button button = new Button();
            button.DialogResult = DialogResult.OK;

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "Button_MouseLeave",
                button,
                EventArgs.Empty
            );

            // Assert
            button.BackColor.Should().Be(Clicked);
        }

        [TestMethod]
        public void Button_MouseLeave_WhenNotDialogResultOk_SetsBackColor()
        {
            // Arrange
            HarnessController controller = BuildThemedController();
            Button button = new Button();
            button.DialogResult = DialogResult.None;

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "Button_MouseLeave",
                button,
                EventArgs.Empty
            );

            // Assert
            button.BackColor.Should().Be(Back);
        }

        [TestMethod]
        public void MenuItem_MouseEnter_SetsMouseOverColor()
        {
            // Arrange
            HarnessController controller = BuildThemedController();
            ToolStripMenuItem menuItem = new ToolStripMenuItem();

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "MenuItem_MouseEnter",
                menuItem,
                EventArgs.Empty
            );

            // Assert
            menuItem.BackColor.Should().Be(MouseOver);
        }

        [TestMethod]
        public void MenuItem_MouseLeave_SetsBackColor()
        {
            // Arrange
            HarnessController controller = BuildThemedController();
            ToolStripMenuItem menuItem = new ToolStripMenuItem();

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "MenuItem_MouseLeave",
                menuItem,
                EventArgs.Empty
            );

            // Assert
            menuItem.BackColor.Should().Be(Back);
        }

        // ------------------------- Checkbox / combo field-write handlers -------------------------

        [TestMethod]
        public void CbxConversation_CheckedChanged_WhenSuppressed_StoresCheckedStateWithoutSideEffects()
        {
            // Arrange — SuppressEvents true means neither Collapse nor Enumerate runs; only the field
            // is updated from the viewer's checkbox state.
            QfcItemControllerTestSupport.EnsureSynchronizationContext();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.ConversationModeChecked).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            controller.SuppressEvents = true;

            // Act
            controller.CbxConversation_CheckedChanged(null, EventArgs.Empty);

            // Assert
            QfcItemControllerTestSupport
                .GetField(controller, "_optionConversationChecked")
                .Should()
                .Be(true);
        }

        [TestMethod]
        public void CbxEmailCopy_CheckedChanged_StoresCheckedState()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.EmailCopyChecked).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "CbxEmailCopy_CheckedChanged",
                null,
                EventArgs.Empty
            );

            // Assert
            QfcItemControllerTestSupport
                .GetField(controller, "_optionEmailCopy")
                .Should()
                .Be(true);
        }

        [TestMethod]
        public void CbxAttachments_CheckedChanged_StoresCheckedState()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.AttachmentsChecked).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "CbxAttachments_CheckedChanged",
                null,
                EventArgs.Empty
            );

            // Assert
            QfcItemControllerTestSupport
                .GetField(controller, "_optionAttachments")
                .Should()
                .Be(true);
        }

        [TestMethod]
        public void CboFolders_SelectedIndexChanged_StoresSelectedFolder()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetSelectedFolder()).Returns(@"\\Archive\Chosen");
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "CboFolders_SelectedIndexChanged",
                null,
                EventArgs.Empty
            );

            // Assert
            QfcItemControllerTestSupport
                .GetField(controller, "_selectedFolder")
                .Should()
                .Be(@"\\Archive\Chosen");
        }

        // ------------------------- Delete-item click -------------------------

        [TestMethod]
        public void BtnDelItem_Click_MarksItemForDeletion()
        {
            // Arrange
            QfcItemControllerTestSupport.EnsureSynchronizationContext();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.FolderContains("Trash to Delete")).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.BtnDelItem_Click(null, EventArgs.Empty);

            // Assert
            viewer.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }

        // ------------------------- Search key-down -------------------------

        [TestMethod]
        public void TextBoxSearch_KeyDown_WhenDownArrow_DropsDownAndFocusesFolder()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            KeyEventArgs args = new KeyEventArgs(Keys.Down);

            // Act
            controller.TextBoxSearch_KeyDown(null, args);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(true), Times.Once());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Once());
            args.Handled.Should().BeTrue();
            args.SuppressKeyPress.Should().BeTrue();
        }

        [TestMethod]
        public void TextBoxSearch_KeyDown_WhenNotDownArrow_DoesNothing()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            KeyEventArgs args = new KeyEventArgs(Keys.Up);

            // Act
            controller.TextBoxSearch_KeyDown(null, args);

            // Assert
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Never());
            args.Handled.Should().BeFalse();
        }

        // ------------------------- Topic-thread selection -------------------------

        [TestMethod]
        public void TopicThread_ItemSelectionChanged_WhenItemSelected_NavigatesToItsHtml()
        {
            // Arrange — a single selected MailItemHelper whose (virtual) Html is mocked.
            Mock<MailItemHelper> helper = new Mock<MailItemHelper>();
            helper.SetupGet(h => h.Html).Returns("<html>body</html>");
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer
                .Setup(v => v.GetSelectedConversationItems())
                .Returns(new List<object> { helper.Object });
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "TopicThread_ItemSelectionChanged",
                null,
                null
            );

            // Assert
            viewer.Verify(v => v.NavigateToString("<html>body</html>"), Times.Once());
        }

        [TestMethod]
        public void TopicThread_ItemSelectionChanged_WhenNoSelection_DoesNotNavigate()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetSelectedConversationItems()).Returns(new List<object>());
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "TopicThread_ItemSelectionChanged",
                null,
                null
            );

            // Assert
            viewer.Verify(v => v.NavigateToString(It.IsAny<string>()), Times.Never());
        }
    }
}
