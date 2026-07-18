using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the pure <see cref="BreadcrumbSelectionMap"/> (#351 P3-T10): positive mapping
    /// (suggestion row -&gt; full path; expanded subfolder -&gt; its full path; index/item selection
    /// round-trips), negative explicit rejection of unknown ids/indexes, and edge contracts
    /// (byte-identical "Trash to Delete", wildcard Path B strings, empty sets, duplicate paths).
    /// Deterministic; no Outlook, WebView2, or I/O.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbSelectionMapTests
    {
        private const string LeafPath = "\\Inbox\\Projects\\Apollo";

        private static FolderBreadcrumbSegment Segment(
            string entryId,
            string path,
            string name,
            bool hasChildren
        ) =>
            new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store-a", entryId, path),
                name,
                path,
                hasChildren
            );

        private static IReadOnlyList<FolderBreadcrumbSegment> Chain() =>
            new[]
            {
                Segment("root", "\\Inbox", "Inbox", true),
                Segment("mid", "\\Inbox\\Projects", "Projects", true),
                Segment("leaf", LeafPath, "Apollo", true),
            };

        private static BreadcrumbStateModel PopulatedModel()
        {
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(), 0.73);
            model.AddPlainRow("Trash to Delete");
            model.AddPlainRow("*search*result*");
            return model;
        }

        // --- Positive mapping ---

        [TestMethod]
        public void GetSelectedFolder_SuggestionRow_YieldsTheLeafFullPath()
        {
            // Arrange
            var model = PopulatedModel();
            model.SelectRow(0);

            // Act, Assert (FR-7: full folder path for Path A rows).
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().Be(LeafPath);
        }

        [TestMethod]
        public void GetSelectedFolder_ExpandedSubfolderSelection_YieldsTheSubfolderFullPath()
        {
            // Arrange
            var model = PopulatedModel();
            model.SelectRow(0);
            model.Rows[0].TryExpandLeaf();
            model
                .Rows[0]
                .SetSubfolders(new[] { Segment("sub", LeafPath + "\\Reports", "Reports", false) });
            model.SelectSubfolder(0);

            // Act, Assert (AC-5/US-4: an expanded-subfolder selection files to that exact path).
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().Be(LeafPath + "\\Reports");
        }

        [TestMethod]
        public void GetSelectedFolder_NoSelection_ReturnsNull()
        {
            // Arrange, Act, Assert (legacy no-selection contract).
            BreadcrumbSelectionMap.GetSelectedFolder(PopulatedModel()).Should().BeNull();
        }

        [TestMethod]
        public void IndexAndItemSelection_RoundTrip()
        {
            // Arrange
            var model = PopulatedModel();

            // Act: index -> select -> read back (SetFolderSelectedIndex contract).
            model.SelectRow(1);
            var byIndex = BreadcrumbSelectionMap.GetSelectedFolder(model);

            // Item -> index -> select -> read back (SetFolderSelectedItem contract).
            var index = BreadcrumbSelectionMap.IndexOfItem(model, LeafPath);
            BreadcrumbSelectionMap.TrySelectItem(model, LeafPath).Should().BeTrue();

            // Assert
            byIndex.Should().Be("Trash to Delete");
            index.Should().Be(0);
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().Be(LeafPath);
        }

        [TestMethod]
        public void GetFolderItems_ReturnsPerRowOutputStringsInDisplayOrder()
        {
            // Arrange, Act
            var items = BreadcrumbSelectionMap.GetFolderItems(PopulatedModel());

            // Assert (GetFolderItems contract).
            items.Should().Equal(LeafPath, "Trash to Delete", "*search*result*");
        }

        // --- Negative: unknown id/index rejected explicitly ---

        [TestMethod]
        public void IndexOfItem_UnknownItem_ReturnsMinusOneAndTrySelectRefuses()
        {
            // Arrange
            var model = PopulatedModel();
            model.SelectRow(0);

            // Act, Assert: unknown item is rejected without disturbing the selection.
            BreadcrumbSelectionMap.IndexOfItem(model, "\\Nope").Should().Be(-1);
            BreadcrumbSelectionMap.TrySelectItem(model, "\\Nope").Should().BeFalse();
            model.SelectedIndex.Should().Be(0);
        }

        [TestMethod]
        public void NullArguments_AreRejectedExplicitly()
        {
            // Arrange
            var model = PopulatedModel();

            // Act, Assert
            ((Action)(() => BreadcrumbSelectionMap.GetSelectedFolder(null)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => BreadcrumbSelectionMap.GetFolderItems(null)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => BreadcrumbSelectionMap.IndexOfItem(model, null)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => BreadcrumbSelectionMap.FolderContains(null, "x")))
                .Should()
                .Throw<ArgumentNullException>();
        }

        // --- Edge contracts ---

        [TestMethod]
        public void TrashToDelete_IsReturnedByteIdentical()
        {
            // Arrange
            var model = PopulatedModel();
            model.SelectRow(1);

            // Act
            var selected = BreadcrumbSelectionMap.GetSelectedFolder(model);

            // Assert (G10: string identity is contract for the attachment-saving gate).
            selected.Should().Be("Trash to Delete");
            ReferenceEquals(selected, model.Rows[1].VerbatimText).Should().BeTrue();
        }

        [TestMethod]
        public void PathBVerbatimStrings_WithWildcards_SurviveExactly()
        {
            // Arrange
            var model = PopulatedModel();
            model.SelectRow(2);

            // Act, Assert
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().Be("*search*result*");
            BreadcrumbSelectionMap.FolderContains(model, "*search*result*").Should().BeTrue();
        }

        [TestMethod]
        public void EmptyItemSet_YieldsEmptyItemsAndNoMatches()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act, Assert
            BreadcrumbSelectionMap.GetFolderItems(model).Should().BeEmpty();
            BreadcrumbSelectionMap.FolderContains(model, "anything").Should().BeFalse();
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().BeNull();
        }

        [TestMethod]
        public void DuplicatePaths_ResolveToTheFirstMatchingRow()
        {
            // Arrange: two rows with the same output string.
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("\\Inbox\\Dup");
            model.AddPlainRow("\\Inbox\\Dup");

            // Act, Assert
            BreadcrumbSelectionMap.IndexOfItem(model, "\\Inbox\\Dup").Should().Be(0);
            BreadcrumbSelectionMap.TrySelectItem(model, "\\Inbox\\Dup").Should().BeTrue();
            model.SelectedIndex.Should().Be(0);
        }

        [TestMethod]
        public void CaseDiffersFromLegacyOrdinalContract_IsNotAMatch()
        {
            // Arrange: legacy ComboBox Items.Contains was ordinal; preserve that exactness.
            var model = PopulatedModel();

            // Act, Assert
            BreadcrumbSelectionMap.FolderContains(model, "trash to delete").Should().BeFalse();
        }
    }
}
