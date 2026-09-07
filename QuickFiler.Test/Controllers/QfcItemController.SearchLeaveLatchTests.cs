using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #796 (AC4): the SearchOwnedDismissalLatch. <c>TextBoxSearch_Leave</c> previously took
    /// dismissal ownership of the folder drop-down regardless of which gesture opened it, so a
    /// mouse-driven open was dismissed by a leave the mouse gesture itself provoked. The latch
    /// records that the open drop-down is one this search box opened, and the leave handler
    /// dismisses only then.
    /// <para>
    /// Class name follows the convention of the sibling file
    /// QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs, which declares
    /// QfcItemController_EventHandlersTests. No window, no external process, no temporary file.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemController_SearchLeaveLatchTests
    {
        /// <summary>
        /// Builds a folder-search handler whose <c>FindFolder</c> returns a fixed result, so the
        /// search-driven open path can be driven without a live Outlook or COM host.
        /// </summary>
        private static Mock<IFolderSearchHandler> BuildFolderHandler(string[] matched)
        {
            Mock<IFolderSearchHandler> folderHandler = new Mock<IFolderSearchHandler>();
            folderHandler
                .Setup(f =>
                    f.FindFolder(
                        It.IsAny<string>(),
                        It.IsAny<object>(),
                        It.IsAny<bool>(),
                        It.IsAny<List<string>>(),
                        It.IsAny<bool>(),
                        It.IsAny<
                            IEnumerable<(string root, string excludedFolder, bool excludeChildren)>
                        >()
                    )
                )
                .Returns(matched);
            return folderHandler;
        }

        /// <summary>
        /// Issue #796 (AC4). Scenario: the drop-down is open because a mouse gesture opened it, and
        /// the folder search box then loses focus. Expected outcome: the leave handler does not
        /// dismiss the drop-down, because the search box never took dismissal ownership of it.
        /// </summary>
        [TestMethod]
        public void SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown()
        {
            // Arrange — the drop-down is open, but no search-driven open path ever ran, which is
            // exactly the state a mouse gesture on the collapsed breadcrumb produces.
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.IsFolderDropDownOpen).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.TextBoxSearch_Leave(null, EventArgs.Empty);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Never());
        }

        /// <summary>
        /// Issue #796 (AC4), paired positive. Scenario: the drop-down is open because typing in the
        /// folder search box opened it, and the search box then loses focus. Expected outcome: the
        /// leave handler still dismisses the drop-down exactly once, so the issue #680 dismissal
        /// responsibility for a non-capturing search-driven popup is preserved.
        /// </summary>
        [TestMethod]
        public void SearchLeaveAfterSearchDrivenOpen_ClosesDropDown()
        {
            // Arrange
            string[] matched = { @"\\A\one", @"\\A\two" };
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.SearchText).Returns("query");
            viewer.SetupGet(v => v.IsFolderDropDownOpen).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_folderHandler",
                BuildFolderHandler(matched).Object
            );

            // Act — the search-driven open path, then the leave it eventually provokes.
            controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);
            controller.TextBoxSearch_Leave(null, EventArgs.Empty);

            // Assert
            viewer.Verify(v => v.PresentFolderSearchResults(matched), Times.Once());
            viewer.Verify(v => v.SetFolderDroppedDown(false), Times.Once());
        }
    }
}
