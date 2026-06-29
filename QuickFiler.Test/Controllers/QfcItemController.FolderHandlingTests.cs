using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Folder-handling cluster tests (research §5.2). Covers the pure static folder-selection seam
    /// PopulateAndSelectFolder edge cases and the AssignFolderComboBox guard behavior.
    /// </summary>
    [TestClass]
    public class QfcItemController_FolderHandlingTests
    {
        private sealed class FolderController : QfcItemController
        {
            internal FolderController()
                : base() { }
        }

        [TestMethod]
        public void PopulateAndSelectFolder_ExactMatchAtIndexZero_SelectsIndexZero()
        {
            // Arrange — predetermined folder equals items[0]; it must be selected at index 0,
            // not overridden by the index-1 fallback.
            var folders = new[] { @"\\A\predetermined", @"\\A\suggestion1", @"\\A\suggestion2" };
            using (var comboBox = new ComboBox())
            {
                // Act
                var selected = QfcItemController.PopulateAndSelectFolder(
                    comboBox,
                    folders,
                    predeterminedFolder: @"\\A\predetermined"
                );

                // Assert
                comboBox.SelectedIndex.Should().Be(0);
                selected.Should().Be(@"\\A\predetermined");
            }
        }

        [TestMethod]
        public void PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne()
        {
            // Arrange — predetermined folder is not present in the array; the index-1 fallback applies.
            var folders = new[] { @"\\A\header", @"\\A\top", @"\\A\second" };
            using (var comboBox = new ComboBox())
            {
                // Act
                var selected = QfcItemController.PopulateAndSelectFolder(
                    comboBox,
                    folders,
                    predeterminedFolder: @"\\A\not-present"
                );

                // Assert
                comboBox.SelectedIndex.Should().Be(1);
                selected.Should().Be(@"\\A\top");
            }
        }

        [TestMethod]
        public void PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection()
        {
            // Arrange — the production caller guards against an empty FolderArray; calling the seam
            // directly with no items documents that the index-1 selection is out of range.
            var folders = Array.Empty<string>();
            using (var comboBox = new ComboBox())
            {
                // Act
                Action act = () =>
                    QfcItemController.PopulateAndSelectFolder(
                        comboBox,
                        folders,
                        predeterminedFolder: null
                    );

                // Assert
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        /// <summary>
        /// Builds a <see cref="FolderPredictor"/> with a known <c>FolderArray</c> without touching
        /// Outlook COM. The single-arg <c>FolderPredictor(Outlook.Application)</c> constructor performs
        /// no COM work; seeding the private <c>_folderList</c> backing field makes the lazy
        /// <c>FolderArray</c> getter return that list directly.
        /// </summary>
        private static FolderPredictor BuildFolderHandlerWithArray(params string[] folders)
        {
            var ctor = typeof(FolderPredictor)
                .GetConstructors()
                .Single(c =>
                    c.GetParameters().Length == 1
                    && c.GetParameters()[0].ParameterType.Name == "Application"
                );
            var fp = (FolderPredictor)ctor.Invoke(new object[] { null });
            typeof(FolderPredictor)
                .GetField("_folderList", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(fp, new List<string>(folders));
            return fp;
        }

        private static void SetPrivate(QfcItemController controller, string field, object value) =>
            typeof(QfcItemController)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, value);

        [TestMethod]
        public void AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer()
        {
            // Arrange — a populated folder handler with no predetermined folder must route the
            // suggestions into the viewer and select the index-1 top suggestion.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.GetSelectedFolder()).Returns(@"\\A\top");
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithArray(@"\\A\header", @"\\A\top", @"\\A\second")
            );

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Once());
            mock.Verify(v => v.SetFolderSelectedIndex(1), Times.Once());
            mock.Verify(v => v.SetFolderSelectedItem(It.IsAny<string>()), Times.Never());
            controller.SelectedFolder.Should().Be(@"\\A\top");
        }

        [TestMethod]
        public void AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder()
        {
            // Arrange — a predetermined folder that the view reports as present must be preselected
            // by name rather than falling back to the index-1 suggestion.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.FolderContains(@"\\A\chosen")).Returns(true);
            mock.Setup(v => v.GetSelectedFolder()).Returns(@"\\A\chosen");
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_predeterminedFolder", @"\\A\chosen");
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithArray(@"\\A\header", @"\\A\top", @"\\A\chosen")
            );

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(v => v.SetFolderSelectedItem(@"\\A\chosen"), Times.Once());
            mock.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
            controller.SelectedFolder.Should().Be(@"\\A\chosen");
        }

        [TestMethod]
        public void AssignFolderComboBox_WhenFolderHandlerNull_DoesNotTouchViewer()
        {
            // Arrange — with no folder handler the method's guard must short-circuit and perform no
            // folder mutations on the view.
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            var controller = new FolderController();
            typeof(QfcItemController)
                .GetField(
                    "_itemViewer",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .SetValue(controller, mock.Object);

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Never());
            mock.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
        }
    }
}
