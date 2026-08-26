using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the host-neutral <see cref="BreadcrumbStateModel"/> /
    /// <see cref="BreadcrumbStateRow"/> collapse/expand state machine (#351 P3-T2): positive
    /// collapse-after/re-expand/affordance flows, negative fail-fast validation, and edge chains. The
    /// state-transition-sequence and #398 ReplaceRows groups live in the sibling partial
    /// BreadcrumbStateModelSequenceTests.cs. Deterministic; no Outlook, WebView2, timers, or temp files.
    /// </summary>
    [TestClass]
    public sealed partial class BreadcrumbStateModelTests
    {
        private static FolderTreeNodeKey Key(string entryId, string path) =>
            new FolderTreeNodeKey("store-a", entryId, path);

        private static FolderBreadcrumbSegment Segment(
            string entryId,
            string path,
            string name,
            bool hasChildren
        ) => new FolderBreadcrumbSegment(Key(entryId, path), name, path, hasChildren);

        private static IReadOnlyList<FolderBreadcrumbSegment> ThreeSegmentChain(
            bool leafHasChildren = true
        ) =>
            new[]
            {
                Segment("root", "\\Inbox", "Inbox", true),
                Segment("mid", "\\Inbox\\Projects", "Projects", true),
                Segment("leaf", "\\Inbox\\Projects\\Apollo", "Apollo", leafHasChildren),
            };

        private static BreadcrumbStateModel ModelWithSuggestion(bool leafHasChildren = true)
        {
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(ThreeSegmentChain(leafHasChildren), 0.73);
            model.SelectRow(0);
            return model;
        }

        // --- Positive flows ---

        [TestMethod]
        public void CollapseAfter_NonLeafSegment_HidesDownstreamAndClosesLeafExpansion()
        {
            // Arrange
            var model = ModelWithSuggestion();
            var row = model.Rows[0];
            row.TryExpandLeaf();
            row.SetSubfolders(
                new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
            );

            // Act
            row.CollapseAfter(0);

            // Assert: collapsed marker set, leaf expansion and subfolders cleared.
            row.CollapsedAfterIndex.Should().Be(0);
            row.LeafExpanded.Should().BeFalse();
            row.Subfolders.Should().BeEmpty();
        }

        [TestMethod]
        public void ReExpand_AfterCollapse_RestoresTheFullChain()
        {
            // Arrange
            var model = ModelWithSuggestion();
            var row = model.Rows[0];
            row.CollapseAfter(1);

            // Act
            row.ReExpand();

            // Assert
            row.CollapsedAfterIndex.Should().BeNull();
            row.Chain.Should().HaveCount(3, "the chain itself is never mutated by collapse");
        }

        [TestMethod]
        public void LeafHasSubfolders_TrueOnlyWhenLeafSegmentHasChildren()
        {
            // Arrange, Act, Assert: affordance gate follows the segment's HasChildren flag (FR-2).
            ModelWithSuggestion(leafHasChildren: true).Rows[0].LeafHasSubfolders.Should().BeTrue();
            ModelWithSuggestion(leafHasChildren: false)
                .Rows[0]
                .LeafHasSubfolders.Should()
                .BeFalse();
        }

        [TestMethod]
        public void TryExpandLeaf_WithAffordance_OpensExpansionAndAcceptsSubfolders()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];

            // Act
            var expanded = row.TryExpandLeaf();
            row.SetSubfolders(
                new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
            );

            // Assert
            expanded.Should().BeTrue();
            row.LeafExpanded.Should().BeTrue();
            row.Subfolders.Should().ContainSingle(s => s.DisplayName == "Sub");
        }

        [TestMethod]
        public void SelectRow_TracksSelectionAndResetsSubfolderSelection()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(ThreeSegmentChain(), 0.5);
            model.AddPlainRow("Trash to Delete");
            model.SelectRow(0);
            model.Rows[0].TryExpandLeaf();
            model
                .Rows[0]
                .SetSubfolders(
                    new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
                );
            model.SelectSubfolder(0);

            // Act
            model.SelectRow(1);

            // Assert
            model.SelectedIndex.Should().Be(1);
            model
                .SelectedSubfolderIndex.Should()
                .Be(-1, "row selection resets subfolder selection");
            model.SelectedRow.IsSuggestion.Should().BeFalse();
        }

        [TestMethod]
        public void AddPlainRow_CarriesVerbatimTextWithoutProbability()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act
            model.AddPlainRow("Trash to Delete");

            // Assert (Path B contract: verbatim string, no probability, no chain).
            var row = model.Rows[0];
            row.IsSuggestion.Should().BeFalse();
            row.VerbatimText.Should().Be("Trash to Delete");
            row.Probability.Should().BeNull();
            row.Chain.Should().BeEmpty();
            row.LeafHasSubfolders.Should().BeFalse();
        }

        // --- Negative flows (fail fast) ---

        [TestMethod]
        public void CollapseAfter_OnTheLeafIndex_Throws()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];

            // Act
            Action act = () => row.CollapseAfter(2);

            // Assert: the leaf cannot be a collapse-after pivot.
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("segmentIndex");
        }

        [TestMethod]
        public void CollapseAfter_OutOfRangeSegmentIndex_Throws()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];

            // Act, Assert
            ((Action)(() => row.CollapseAfter(-1)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
            ((Action)(() => row.CollapseAfter(5))).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CollapseAfter_OnPlainRow_ThrowsInvalidOperation()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("\\Inbox\\Manual");

            // Act
            Action act = () => model.Rows[0].CollapseAfter(0);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void TryExpandLeaf_WithoutAffordance_IsRejectedNoOp()
        {
            // Arrange
            var row = ModelWithSuggestion(leafHasChildren: false).Rows[0];

            // Act, Assert: no-op by contract so the router reports an unhandled arrow.
            row.TryExpandLeaf().Should().BeFalse();
            row.LeafExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void SetSubfolders_WhenNotExpanded_Throws()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];

            // Act
            Action act = () =>
                row.SetSubfolders(
                    new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
                );

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void SelectRow_OutOfRange_Throws()
        {
            // Arrange
            var model = ModelWithSuggestion();

            // Act, Assert
            ((Action)(() => model.SelectRow(1)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
            ((Action)(() => model.SelectRow(-2))).Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SelectSubfolder_WithoutOpenExpansion_Throws()
        {
            // Arrange
            var model = ModelWithSuggestion();

            // Act
            Action act = () => model.SelectSubfolder(0);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void AddSuggestionRow_EmptyOrNullChain_Throws()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act, Assert
            ((Action)(() => model.AddSuggestionRow(null, 0.1)))
                .Should()
                .Throw<ArgumentException>();
            ((Action)(() => model.AddSuggestionRow(new FolderBreadcrumbSegment[0], 0.1)))
                .Should()
                .Throw<ArgumentException>();
        }

        // --- Edge cases ---

        [TestMethod]
        public void SingleSegmentChain_HasNoCollapsePivotAndObeysAffordanceGate()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(new[] { Segment("root", "\\Inbox", "Inbox", false) }, null);
            var row = model.Rows[0];

            // Act, Assert: no non-leaf segment exists, so any collapse index throws.
            ((Action)(() => row.CollapseAfter(0)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
            row.TryExpandLeaf().Should().BeFalse();
        }

        [TestMethod]
        public void CollapseAfter_WhenAlreadyCollapsed_MovesThePivot()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];
            row.CollapseAfter(1);

            // Act: re-collapse at an earlier pivot.
            row.CollapseAfter(0);

            // Assert
            row.CollapsedAfterIndex.Should().Be(0);
        }

        [TestMethod]
        public void Reset_RestoresInitialRowState()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];
            row.TryExpandLeaf();
            row.SetSubfolders(
                new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
            );
            row.CollapseAfter(0);

            // Act
            row.Reset();

            // Assert
            row.CollapsedAfterIndex.Should().BeNull();
            row.LeafExpanded.Should().BeFalse();
            row.Subfolders.Should().BeEmpty();
        }

        // --- #440 Qfc tree navigation ---

        /// <summary>
        /// #440 Qfc Left: on a multi-segment row Left selects the row's parent node before any
        /// pre-existing view transition is considered (decision D1). The parent is proven to be
        /// the selected node because a following Right expands it even though the leaf segment
        /// carries no expansion affordance of its own.
        /// </summary>
        [TestMethod]
        public void LeftArrow_QfcMultiSegmentRow_SelectsParentNode()
        {
            // Arrange: Inbox -> Projects -> Apollo where only the non-leaf segments have children.
            var model = ModelWithSuggestion(leafHasChildren: false);

            // Act
            var handled = model.LeftArrow();

            // Assert: the tree transition handled the key without collapsing the view.
            handled.Should().BeTrue("Left selects the parent node before the pre-existing path");
            model.SelectedRow.CollapsedAfterIndex.Should().BeNull();
            model.SelectedRow.LeafExpanded.Should().BeFalse();

            // Assert: the selected node is the parent, not the leaf.
            model.RightArrow().Should().BeTrue("the selected parent node carries children");
            model.SelectedRow.LeafExpanded.Should().BeTrue();
        }

        /// <summary>
        /// #440 Qfc Right: once the selected parent node is expanded and its children are
        /// attached, the next Right descends into child index 0.
        /// </summary>
        [TestMethod]
        public void RightArrow_QfcSelectedParentNode_ExpandsIntoChildren()
        {
            // Arrange: select the parent node, expand it, and attach one child.
            var model = ModelWithSuggestion();
            model.LeftArrow();
            model.RightArrow();
            model.SelectedRow.SetSubfolders(
                new[] { Segment("kid", "\\Inbox\\Projects\\Kid", "Kid", false) }
            );

            // Act
            var handled = model.RightArrow();

            // Assert
            handled.Should().BeTrue("the descent transition selects the first child");
            model.SelectedSubfolderIndex.Should().Be(0);
        }

        /// <summary>
        /// #440 decision D1 (handling order): a one-segment suggestion row has no parent to
        /// select, so Right and Left take the pre-existing expand and collapse path and, where
        /// none applies, report the pre-existing unhandled fall-through.
        /// </summary>
        [TestMethod]
        public void ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(new[] { Segment("root", "\\Inbox", "Inbox", true) }, 0.5);
            model.SelectRow(0);

            // Act + Assert: Right opens the pre-existing leaf expansion.
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeTrue();

            // Act + Assert: Left closes it through the pre-existing collapse path.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeFalse();

            // Act + Assert: a further Left is the pre-existing unhandled fall-through.
            model.LeftArrow().Should().BeFalse();
        }
    }
}
