using System;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #680 regression suite for search-popup dismissal ownership.
    /// <para>
    /// A search-driven open is presented non-capturing (<c>ToolStripDropDown.AutoClose == false</c>),
    /// which is the WinForms framework's own opt-out from <c>ModalMenuFilter</c> menu-mode entry.
    /// Menu mode is what previously dismissed the popup on a focus change and consumed Escape, so
    /// the controller must now own both paths: the search textbox's <c>Leave</c> event and an
    /// Escape keystroke each route exactly one close intent, which the existing pipeline turns into
    /// <c>CancelSelector</c>.
    /// </para>
    /// <para>
    /// The seam is entirely headless — a <see cref="Mock{IItemViewer}"/> injected into a bare
    /// <c>HarnessController</c> by reflection, with no WinForms control, window handle, or message
    /// pump — mirroring <c>QfcItemController.SearchFocusRegressionTests</c>.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemController_SearchDismissalTests
    {
        /// <summary>
        /// Escape while the drop-down is open routes exactly one close intent and swallows the key,
        /// because Escape no longer reaches the popup through WinForms menu mode.
        /// </summary>
        [TestMethod]
        public void TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer(isOpen: true);
            HarnessController controller = BuildController(viewer);
            var e = new KeyEventArgs(Keys.Escape);

            // Act
            controller.TextBoxSearch_KeyDown(null, e);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Once());
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Once());
            e.Handled.Should().BeTrue("the dismissal consumed the keystroke");
        }

        /// <summary>
        /// Edge control: Escape with the drop-down already closed routes no intent and leaves the key
        /// unhandled, so Escape keeps whatever meaning it has elsewhere in the form.
        /// </summary>
        [TestMethod]
        public void TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer(isOpen: false);
            HarnessController controller = BuildController(viewer);
            var e = new KeyEventArgs(Keys.Escape);

            // Act
            controller.TextBoxSearch_KeyDown(null, e);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never());
            e.Handled.Should().BeFalse("Escape is not swallowed when there is nothing to dismiss");
        }

        /// <summary>
        /// The search textbox losing focus while the drop-down is open routes exactly one close
        /// intent — the dismissal WinForms menu mode used to provide for a capturing popup.
        /// </summary>
        [TestMethod]
        public void TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer(isOpen: true);
            HarnessController controller = BuildController(viewer);

            // Act
            controller.TextBoxSearch_Leave(null, EventArgs.Empty);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Once());
        }

        /// <summary>
        /// Edge control: a leave with the drop-down already closed produces no spurious close intent.
        /// </summary>
        [TestMethod]
        public void TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer(isOpen: false);
            HarnessController controller = BuildController(viewer);

            // Act
            controller.TextBoxSearch_Leave(null, EventArgs.Empty);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never());
        }

        /// <summary>
        /// The Down-arrow gesture moves focus onto the same-form breadcrumb control, which raises the
        /// textbox's <c>Leave</c>. That one leave must not dismiss the popup the gesture just claimed,
        /// so the handoff arms a suppression latch consumed exactly once: the first leave is
        /// suppressed and the next one dismisses normally.
        /// </summary>
        [TestMethod]
        public void TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer(isOpen: true);
            HarnessController controller = BuildController(viewer);

            // Act — the gesture, unchanged.
            controller.TextBoxSearch_KeyDown(null, new KeyEventArgs(Keys.Down));

            // Assert — gesture behavior is untouched.
            viewer.Verify(v => v.SetFolderDroppedDown(true), Times.Once());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Once());

            // Act — the focus handoff's leave.
            controller.TextBoxSearch_Leave(null, EventArgs.Empty);

            // Assert — the latch swallowed it.
            viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Never());

            // Act — a genuine later leave.
            controller.TextBoxSearch_Leave(null, EventArgs.Empty);

            // Assert — the latch was consumed exactly once, so this leave dismisses.
            viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Once());
        }

        /// <summary>
        /// Control pinning the automated half of spec AC-2: the Down-arrow gesture still opens and
        /// focuses the drop-down and still suppresses the key.
        /// </summary>
        [TestMethod]
        public void TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer(isOpen: false);
            HarnessController controller = BuildController(viewer);
            var e = new KeyEventArgs(Keys.Down);

            // Act
            controller.TextBoxSearch_KeyDown(null, e);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(true), Times.Once());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Once());
            e.SuppressKeyPress.Should().BeTrue();
        }

        /// <summary>A viewer mock reporting the requested drop-down open state.</summary>
        private static Mock<IItemViewer> BuildViewer(bool isOpen)
        {
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.IsFolderDropDownOpen).Returns(isOpen);
            return viewer;
        }

        /// <summary>Injects the viewer into a bare controller harness by reflection.</summary>
        private static HarnessController BuildController(Mock<IItemViewer> viewer)
        {
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            return controller;
        }
    }
}
