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
