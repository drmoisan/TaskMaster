using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// State-transition-sequence and #398 atomic-replace (<see cref="BreadcrumbStateModel.ReplaceRows"/>)
    /// coverage for the host-neutral <see cref="BreadcrumbStateModel"/> state machine. Split from
    /// BreadcrumbStateModelTests.cs so each file stays under the 500-line limit; this partial reuses the
    /// shared helpers (<c>Key</c>, <c>Segment</c>, <c>ThreeSegmentChain</c>, <c>ModelWithSuggestion</c>)
    /// declared in the sibling partial. Deterministic; no Outlook, WebView2, timers, or temp files.
    /// </summary>
    public sealed partial class BreadcrumbStateModelTests
    {
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

            // #440 corrected contract: Left walks the ancestor chain, so the unhandled press comes
            // only once the root is active. Decision D1: the sequence is extended to the root rather
            // than re-pointed at a single-segment row, because the single-segment boundary is already
            // covered by ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath, and re-pointing
            // would delete the only sequence-level assertion over the three-segment fixture.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeFalse();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(1);
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(0);
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

        /// <summary>
        /// #440 walk-to-root: on a three-segment chain with the leaf active and no subfolder
        /// selected, each of the first two Left presses selects the parent of the currently active
        /// node, and the third press at the root is unhandled. The active segment index is asserted
        /// after every press so the test cannot pass on the boolean alone.
        /// </summary>
        [TestMethod]
        public void LeftArrow_RepeatedOnThreeSegmentChain_WalksToRootThenReportsUnhandled()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2, "the row starts leaf-anchored");

            // Act + Assert: press 1 selects the parent.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(1);

            // Act + Assert: press 2 selects the root of the resolved chain.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(0);

            // Act + Assert: press 3 at the root is unhandled and leaves the root active.
            model.LeftArrow().Should().BeFalse();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(0);
        }

        /// <summary>
        /// #440 walk-to-root from an open leaf expansion: the first Left both clears the expansion,
        /// because the fetched subfolders belonged to the previous node, and selects the parent. The
        /// walk then continues to the root with the expansion staying closed.
        /// </summary>
        [TestMethod]
        public void LeftArrow_WalkFromAnOpenLeafExpansion_ClearsTheExpansionAndStillReachesTheRoot()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeTrue();

            // Act + Assert: press 1 clears the expansion and selects the parent.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(1);
            model.SelectedRow.LeafExpanded.Should().BeFalse();

            // Act + Assert: press 2 reaches the root with the expansion still closed.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(0);
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
