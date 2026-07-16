using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// #325 controller-injection tests: <see cref="QfcItemController"/> must hand the row-model
    /// (<see cref="FolderRow"/> array from the predictor's <c>FolderRowArray</c>) to the additive
    /// <see cref="IItemViewer.SetFolderSuggestions"/> intent member, while the retained
    /// <see cref="IItemViewer.SetFolderItems"/> call sites (index-1/predetermined selection and the
    /// "Trash to Delete" append) remain satisfied. All dependencies are mocked/faked; no COM.
    /// </summary>
    [TestClass]
    public class QfcItemController_FolderSuggestionsTests
    {
        private sealed class FolderController : QfcItemController
        {
            internal FolderController()
                : base() { }
        }

        // Minimal in-memory IFolderSearchHandler so the row model handed to the controller is fully
        // controlled and COM-free.
        private sealed class FakeFolderHandler : IFolderSearchHandler
        {
            public string[] FolderArray { get; set; }
            public FolderScorer Suggestions { get; set; }
            public FolderRow[] FolderRowArray { get; set; }

            public string[] FindFolder(
                string searchString,
                object objItem,
                bool reloadCTFStagingFiles = true,
                List<string> emailSearchRoots = null,
                bool recalcSuggestions = false,
                IEnumerable<(string root, string excludedFolder, bool excludeChildren)> exclusions =
                    null
            ) => FolderArray;
        }

        private static void SetPrivate(QfcItemController controller, string field, object value) =>
            typeof(QfcItemController)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, value);

        private static FolderRow[] SampleRows() =>
            new[]
            {
                new FolderRow("========= SUGGESTIONS =========", FolderRowKind.Separator, null),
                new FolderRow(
                    "Archive\\Finance",
                    FolderRowKind.Suggestion,
                    new FolderScore("Archive\\Finance", 900, 0.9)
                ),
                new FolderRow("======= RECENT SELECTIONS ========", FolderRowKind.Separator, null),
                new FolderRow("Recent Folder", FolderRowKind.Recent, null),
            };

        [TestMethod]
        public void AssignFolderComboBox_HandsPredictorRowArrayToSetFolderSuggestions()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.GetSelectedFolder()).Returns("Archive\\Finance");
            IReadOnlyList<FolderRow> captured = null;
            mock.Setup(v => v.SetFolderSuggestions(It.IsAny<IReadOnlyList<FolderRow>>()))
                .Callback<IReadOnlyList<FolderRow>>(r => captured = r);

            var handler = new FakeFolderHandler
            {
                FolderArray = new[] { "header", "Archive\\Finance", "second" },
                Suggestions = new FolderScorer(),
                FolderRowArray = SampleRows(),
            };
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_folderHandler", handler);

            // Act
            controller.AssignFolderComboBox();

            // Assert — the row model reached SetFolderSuggestions with contract-correct classification.
            mock.Verify(
                v => v.SetFolderSuggestions(It.IsAny<IReadOnlyList<FolderRow>>()),
                Times.Once()
            );
            captured.Should().NotBeNull();
            captured.Should().HaveCount(4);
            captured
                .Single(r => r.Kind == FolderRowKind.Suggestion)
                .Score.Should()
                .NotBeNull("only Suggestion rows carry a FolderScore");
            captured
                .Where(r => r.Kind != FolderRowKind.Suggestion)
                .Should()
                .OnlyContain(r => r.Score == null, "separators and recents carry a null score");
            captured
                .Single(r => r.Kind == FolderRowKind.Suggestion)
                .Score.Value.Probability.Should()
                .Be(0.9);
        }

        [TestMethod]
        public void AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.GetSelectedFolder()).Returns("Archive\\Finance");
            var handler = new FakeFolderHandler
            {
                FolderArray = new[] { "header", "Archive\\Finance", "second" },
                Suggestions = new FolderScorer(),
                FolderRowArray = SampleRows(),
            };
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_folderHandler", handler);

            // Act
            controller.AssignFolderComboBox();

            // Assert — the retained SetFolderItems(string[]) population and index-1 selection remain.
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Once());
            mock.Verify(v => v.SetFolderSelectedIndex(1), Times.Once());
            mock.Verify(v => v.SetFolderSelectedItem(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.FolderContains("Archive\\Finance")).Returns(true);
            mock.Setup(v => v.GetSelectedFolder()).Returns("Archive\\Finance");
            var handler = new FakeFolderHandler
            {
                FolderArray = new[] { "header", "top", "Archive\\Finance" },
                Suggestions = new FolderScorer(),
                FolderRowArray = SampleRows(),
            };
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_predeterminedFolder", "Archive\\Finance");
            SetPrivate(controller, "_folderHandler", handler);

            // Act
            controller.AssignFolderComboBox();

            // Assert — predetermined preselection retained alongside the new suggestion injection.
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Once());
            mock.Verify(
                v => v.SetFolderSuggestions(It.IsAny<IReadOnlyList<FolderRow>>()),
                Times.Once()
            );
            mock.Verify(v => v.SetFolderSelectedItem("Archive\\Finance"), Times.Once());
            mock.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.Setup(v => v.FolderContains("Trash to Delete")).Returns(false);
            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);

            // Act
            controller.MarkItemForDeletion();

            // Assert — the retained "Trash to Delete" append path is unchanged.
            mock.Verify(
                v =>
                    v.SetFolderItems(
                        It.Is<string[]>(a => a.Length == 1 && a[0] == "Trash to Delete")
                    ),
                Times.Once()
            );
            mock.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }
    }
}
