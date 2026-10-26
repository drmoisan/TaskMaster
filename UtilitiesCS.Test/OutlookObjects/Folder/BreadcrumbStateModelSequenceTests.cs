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

        /// <summary>
        /// #1007 AC7: Right opens the leaf expansion and a second Right is unhandled; Left closes
        /// the expansion, and the next Left is unhandled because Left never selects a parent.
        /// </summary>
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

            // Left closes the expansion; the next Left changes nothing and is unhandled.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeFalse();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2);
            model.LeftArrow().Should().BeFalse();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2);
        }

        /// <summary>
        /// #1007 AC9 (D-Q23): Right on a row shortened at its active segment lists that segment's
        /// children and keeps the row shortened; a second Right is unhandled.
        /// </summary>
        [TestMethod]
        public void RightArrow_OnRowShortenedAtActiveSegment_ExpandsAndStaysShortened()
        {
            // Arrange: Inbox becomes the active segment and the shortened terminal.
            var model = ModelWithSuggestion();
            model.SelectedRow.ActivateSegment(0).Should().BeTrue();
            model.SelectedRow.CollapseAfter(0);

            // Act + Assert
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeTrue();
            model.SelectedRow.CollapsedAfterIndex.Should().Be(0);
            model.RightArrow().Should().BeFalse();
        }

        /// <summary>
        /// #1007 R3, N-1 (D3): Right on a row collapsed away from its active segment (the leaf
        /// stays active while the public <c>CollapseAfter</c> hides it) restores the full chain and
        /// does not open the leaf expansion.
        /// </summary>
        [TestMethod]
        public void RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain()
        {
            // Arrange: the leaf (index 2) stays active while the row is collapsed after Inbox.
            var model = ModelWithSuggestion();
            model.SelectedRow.CollapseAfter(0);

            // Act
            bool handled = model.RightArrow();

            // Assert
            handled.Should().BeTrue();
            model.SelectedRow.CollapsedAfterIndex.Should().BeNull("the full chain is restored");
            model.SelectedRow.LeafExpanded.Should().BeFalse("the leaf expansion is not opened");
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2);
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

        /// <summary>
        /// #1007 AC7: Left collapses the expansion; the collapse hides the selected subfolder, so
        /// the selection moves to its nearest visible ancestor, the row itself.
        /// </summary>
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
        /// #1007 AC7: the #440 walk is removed, so repeated Left presses on a leaf-active,
        /// non-expanded row are all unhandled and neither move the active segment nor shorten the
        /// row. The active index is asserted after every press so the boolean alone cannot pass.
        /// </summary>
        [TestMethod]
        public void LeftArrow_RepeatedOnNonExpandedChain_IsUnhandledAndKeepsLeafActive()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2, "the row starts leaf-anchored");

            // Act + Assert
            for (int press = 0; press < 3; press++)
            {
                model.LeftArrow().Should().BeFalse();
                model.SelectedRow.ActiveSegmentIndex.Should().Be(2);
                model.SelectedRow.CollapsedAfterIndex.Should().BeNull();
            }
        }

        /// <summary>
        /// #1007 AC7: from an open leaf expansion the first Left only closes the expansion (the
        /// leaf stays active) and the second Left is unhandled.
        /// </summary>
        [TestMethod]
        public void LeftArrow_FromOpenLeafExpansion_CollapsesThenIsUnhandled()
        {
            // Arrange
            var model = ModelWithSuggestion();
            model.RightArrow().Should().BeTrue();
            model.SelectedRow.LeafExpanded.Should().BeTrue();

            // Act + Assert: press 1 clears the expansion and keeps the leaf active.
            model.LeftArrow().Should().BeTrue();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2);
            model.SelectedRow.LeafExpanded.Should().BeFalse();

            // Act + Assert: press 2 is unhandled.
            model.LeftArrow().Should().BeFalse();
            model.SelectedRow.ActiveSegmentIndex.Should().Be(2);
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
