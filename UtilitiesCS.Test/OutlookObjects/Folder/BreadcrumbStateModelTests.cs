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
    /// collapse-after/re-expand/affordance flows, negative fail-fast validation, edge chains, and
    /// state-transition sequences. Deterministic; no Outlook, WebView2, timers, or temp files.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbStateModelTests
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

        // --- State-transition sequences ---

        [TestMethod]
        public void Sequence_CollapseReExpandCollapse_TransitionsDeterministically()
        {
            // Arrange
            var row = ModelWithSuggestion().Rows[0];

            // Act + Assert stepwise
            row.CollapseAfter(1);
            row.CollapsedAfterIndex.Should().Be(1);
            row.ReExpand();
            row.CollapsedAfterIndex.Should().BeNull();
            row.CollapseAfter(0);
            row.CollapsedAfterIndex.Should().Be(0);
        }

        [TestMethod]
        public void Sequence_ExpandListSubfoldersThenCollapse_ClearsTheList()
        {
            // Arrange
            var model = ModelWithSuggestion();
            var row = model.Rows[0];

            // Act
            row.TryExpandLeaf();
            row.SetSubfolders(
                new[]
                {
                    Segment("s1", "\\Inbox\\Projects\\Apollo\\A", "A", false),
                    Segment("s2", "\\Inbox\\Projects\\Apollo\\B", "B", true),
                }
            );
            var collapsed = row.TryCollapseLeaf();

            // Assert
            collapsed.Should().BeTrue();
            row.LeafExpanded.Should().BeFalse();
            row.Subfolders.Should().BeEmpty();
            row.TryCollapseLeaf().Should().BeFalse("already collapsed is a reported no-op");
        }

        [TestMethod]
        public void Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges()
        {
            // Arrange
            var model = ModelWithSuggestion();

            // Act + Assert: Right opens the leaf expansion.
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeTrue();

            // Right again: nothing further to expand -> unhandled (legacy fall-through signal).
            model.RightArrow().Should().BeFalse();

            // Left closes the expansion; a second Left is unhandled.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeFalse();
            model.LeftArrow().Should().BeFalse();
        }

        [TestMethod]
        public void RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.SelectedRow.CollapseAfter(0);

            // Act + Assert: first Right restores the chain, second opens the leaf.
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.CollapsedAfterIndex.Should().BeNull();
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeTrue();
        }

        [TestMethod]
        public void Arrows_WithNoSelection_AreUnhandled()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(ThreeSegmentChain(), 0.4);

            // Act, Assert
            model.RightArrow().Should().BeFalse();
            model.LeftArrow().Should().BeFalse();
        }

        [TestMethod]
        public void SelectSubfolder_OutOfRangeIndex_Throws()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.SelectedRow.TryExpandLeaf();
            model.SelectedRow.SetSubfolders(
                new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
            );

            // Act, Assert
            ((System.Action)(() => model.SelectSubfolder(-1)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
            ((System.Action)(() => model.SelectSubfolder(1)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.SelectedRow.TryExpandLeaf();
            model.SelectedRow.SetSubfolders(
                new[] { Segment("sub", "\\Inbox\\Projects\\Apollo\\Sub", "Sub", false) }
            );
            model.SelectSubfolder(0);

            // Act
            var handled = model.LeftArrow();

            // Assert
            handled.Should().BeTrue();
            model.SelectedSubfolderIndex.Should().Be(-1);
            model.SelectedRow.LeafExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void AddSuggestionRow_NullSegmentInChain_Throws()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            var chain = new[] { Segment("root", "\\Inbox", "Inbox", true), null };

            // Act
            Action act = () => model.AddSuggestionRow(chain, 0.5);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*null segments*");
        }

        [TestMethod]
        public void Clear_RemovesRowsAndSelection()
        {
            // Arrange
            var model = ModelWithSuggestion();

            // Act
            model.Clear();

            // Assert
            model.Rows.Should().BeEmpty();
            model.SelectedIndex.Should().Be(-1);
            model.SelectedRow.Should().BeNull();
        }

        // --- #398 atomic-replace seam (ReplaceRows) ---

        private static IReadOnlyList<BreadcrumbStateRow> PlainRows(params string[] texts)
        {
            var source = new BreadcrumbStateModel();
            foreach (var text in texts)
            {
                source.AddPlainRow(text);
            }
            return source.Rows;
        }

        [TestMethod]
        public void ReplaceRows_NullRows_Throws()
        {
            // Arrange
            var model = new BreadcrumbStateModel();

            // Act
            Action act = () => model.ReplaceRows(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("rows");
        }

        [TestMethod]
        public void ReplaceRows_PreservesSelectionWhenIndexStillValid()
        {
            // Arrange: a two-row model with the second row selected.
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("A");
            model.AddPlainRow("B");
            model.SelectRow(1);

            // Act: swap in an equal-length set so the selected index remains valid.
            model.ReplaceRows(PlainRows("X", "Y"));

            // Assert: the selection carries over and any subfolder selection is reset.
            model.Rows.Should().HaveCount(2);
            model.SelectedIndex.Should().Be(1);
            model.SelectedSubfolderIndex.Should().Be(-1);
        }

        [TestMethod]
        public void ReplaceRows_ClearsSelectionWhenIndexBeyondNewCount()
        {
            // Arrange: a three-row model with the last row selected.
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("A");
            model.AddPlainRow("B");
            model.AddPlainRow("C");
            model.SelectRow(2);

            // Act: swap in a shorter set so the selected index no longer exists.
            model.ReplaceRows(PlainRows("X"));

            // Assert: the out-of-range selection is reset to none.
            model.Rows.Should().ContainSingle();
            model.SelectedIndex.Should().Be(-1);
            model.SelectedRow.Should().BeNull();
        }
    }
}
