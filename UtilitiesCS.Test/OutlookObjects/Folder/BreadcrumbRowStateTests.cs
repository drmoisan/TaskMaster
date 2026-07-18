using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the <see cref="BreadcrumbRow"/> collapse/expand state machine (#349):
    /// collapse-after-segment, re-expand, leaf toggle gated on HasSubfolders, arrow transitions,
    /// banner/pseudo-row no-ops, VisibleSegments projections, and transition sequences.
    /// </summary>
    [TestClass]
    public class BreadcrumbRowStateTests
    {
        private static BreadcrumbSegment Segment(string name, bool hasSubfolders)
        {
            return new BreadcrumbSegment(@"Inbox\" + name, name, hasSubfolders);
        }

        private static BreadcrumbRow SuggestionRow(bool leafHasSubfolders)
        {
            return new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[]
                {
                    Segment("Root", true),
                    Segment("Mid", true),
                    Segment("Leaf", leafHasSubfolders),
                },
                0.5
            );
        }

        private static BreadcrumbRow BannerRow()
        {
            return new BreadcrumbRow(
                "row-1",
                BreadcrumbRowKind.Banner,
                new[] { Segment("==== BANNER ====", false) },
                null
            );
        }

        private static BreadcrumbRow TrashRow()
        {
            return new BreadcrumbRow(
                "row-2",
                BreadcrumbRowKind.TrashPseudoRow,
                Array.Empty<BreadcrumbSegment>(),
                null
            );
        }

        [TestMethod]
        public void CollapseAfter_OnNonLeafSegment_HidesDownstreamAndMarksTerminal()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);

            // Act: collapse after the first (root) segment.
            bool changed = row.CollapseAfter(0);

            // Assert: downstream segments hidden; terminal index carries the re-expand affordance.
            changed.Should().BeTrue();
            row.IsCollapsed.Should().BeTrue();
            row.CollapsedAfterIndex.Should().Be(0);
            row.VisibleSegments().Select(s => s.DisplayName).Should().Equal("Root");
        }

        [TestMethod]
        public void ReExpand_AfterCollapse_RestoresFullBreadcrumb()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false);
            row.CollapseAfter(1);

            // Act
            bool changed = row.ReExpand();

            // Assert
            changed.Should().BeTrue();
            row.IsCollapsed.Should().BeFalse();
            row.VisibleSegments().Select(s => s.DisplayName).Should().Equal("Root", "Mid", "Leaf");
        }

        [TestMethod]
        public void ReExpand_WhenNotCollapsed_IsNoOp()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false);

            // Act / Assert
            row.ReExpand().Should().BeFalse();
        }

        [TestMethod]
        public void CollapseAfter_OnLeafSegment_IsNoOp()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);

            // Act: index 2 is the leaf; collapse-after applies to non-leaf segments only.
            bool changed = row.CollapseAfter(2);

            // Assert
            changed.Should().BeFalse();
            row.IsCollapsed.Should().BeFalse();
        }

        [TestMethod]
        public void CollapseAfter_WithOutOfRangeIndex_Throws()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);

            // Act
            Action act = () => row.CollapseAfter(7);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void ToggleLeafExpanded_WithSubfolders_TogglesState()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);
            row.SetLeafChildren(new[] { Segment("Child", false) }).Should().BeTrue();

            // Act / Assert: expand then collapse.
            row.ToggleLeafExpanded().Should().BeTrue();
            row.IsLeafExpanded.Should().BeTrue();
            row.LeafChildren.Single().DisplayName.Should().Be("Child");
            row.ToggleLeafExpanded().Should().BeTrue();
            row.IsLeafExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void ToggleLeafExpanded_WithoutSubfolders_IsNoOp()
        {
            // Arrange: leaf has no subfolders, so the toggle is a documented no-op.
            var row = SuggestionRow(leafHasSubfolders: false);

            // Act / Assert
            row.ToggleLeafExpanded().Should().BeFalse();
            row.IsLeafExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void SetLeafChildren_WithoutSubfolders_IsNoOp()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false);

            // Act / Assert
            row.SetLeafChildren(new[] { Segment("Child", false) }).Should().BeFalse();
            row.LeafChildren.Should().BeEmpty();
        }

        [TestMethod]
        public void SetLeafChildren_WithNull_Throws()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);

            // Act
            Action act = () => row.SetLeafChildren(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void RightArrow_WhenCollapsed_RestoresFullBreadcrumb()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false);
            row.CollapseAfter(0);

            // Act / Assert
            row.RightArrow().Should().BeTrue();
            row.IsCollapsed.Should().BeFalse();
        }

        [TestMethod]
        public void RightArrow_WhenExpandedWithLeafSubfolders_ExpandsLeaf()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);

            // Act / Assert
            row.RightArrow().Should().BeTrue();
            row.IsLeafExpanded.Should().BeTrue();
        }

        [TestMethod]
        public void RightArrow_WhenFullyExpandedWithoutLeafSubfolders_IsNoOp()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false);

            // Act / Assert
            row.RightArrow().Should().BeFalse();
        }

        [TestMethod]
        public void LeftArrow_WhenLeafExpanded_CollapsesLeafFirst()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);
            row.ToggleLeafExpanded();

            // Act / Assert: first Left collapses leaf children, breadcrumb stays expanded.
            row.LeftArrow().Should().BeTrue();
            row.IsLeafExpanded.Should().BeFalse();
            row.IsCollapsed.Should().BeFalse();
        }

        [TestMethod]
        public void LeftArrow_WhenFullyExpanded_HidesTrailingSegment()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false);

            // Act / Assert: Left hides the leaf (collapse after the previous segment).
            row.LeftArrow().Should().BeTrue();
            row.CollapsedAfterIndex.Should().Be(1);
            row.VisibleSegments().Select(s => s.DisplayName).Should().Equal("Root", "Mid");
        }

        [TestMethod]
        public void LeftArrow_AtRootSegment_IsNoOp()
        {
            // Arrange: collapse until only the root segment remains.
            var row = SuggestionRow(leafHasSubfolders: false);
            row.LeftArrow();
            row.LeftArrow();

            // Act / Assert: only the root remains; further Left presses are no-ops.
            row.CollapsedAfterIndex.Should().Be(0);
            row.LeftArrow().Should().BeFalse();
        }

        [TestMethod]
        public void BannerRow_AllTransitions_AreNoOps()
        {
            // Arrange
            var row = BannerRow();

            // Act / Assert: banners never collapse or expand.
            row.CollapseAfter(0).Should().BeFalse();
            row.ReExpand().Should().BeFalse();
            row.ToggleLeafExpanded().Should().BeFalse();
            row.SetLeafChildren(new[] { Segment("Child", false) }).Should().BeFalse();
            row.LeftArrow().Should().BeFalse();
            row.RightArrow().Should().BeFalse();
            row.IsCollapsed.Should().BeFalse();
            row.IsLeafExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void TrashPseudoRow_AllTransitions_AreNoOps()
        {
            // Arrange
            var row = TrashRow();

            // Act / Assert: pseudo-rows never collapse or expand.
            row.ReExpand().Should().BeFalse();
            row.ToggleLeafExpanded().Should().BeFalse();
            row.LeftArrow().Should().BeFalse();
            row.RightArrow().Should().BeFalse();
            row.VisibleSegments().Should().BeEmpty();
        }

        [TestMethod]
        public void VisibleSegments_ProjectsEveryState()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);

            // Act / Assert: fully expanded -> all segments.
            row.VisibleSegments().Should().HaveCount(3);

            // Leaf-expanded -> segment projection unchanged (children are a separate list).
            row.ToggleLeafExpanded();
            row.VisibleSegments().Should().HaveCount(3);

            // Collapsed after index 1 -> two segments.
            row.CollapseAfter(1);
            row.VisibleSegments().Select(s => s.DisplayName).Should().Equal("Root", "Mid");

            // Re-expanded -> all segments again.
            row.ReExpand();
            row.VisibleSegments().Should().HaveCount(3);
        }

        [TestMethod]
        public void TransitionSequence_CollapseReExpandLeafExpand_KeepsDataImmutable()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true);
            row.SetLeafChildren(new[] { Segment("Child", false) });

            // Act: collapse -> re-expand -> leaf expand.
            row.CollapseAfter(0).Should().BeTrue();
            row.IsLeafExpanded.Should().BeFalse("collapsing hides any expanded leaf children");
            row.ReExpand().Should().BeTrue();
            row.ToggleLeafExpanded().Should().BeTrue();

            // Assert: view state transitions never altered row data.
            row.IsLeafExpanded.Should().BeTrue();
            row.Segments.Should().HaveCount(3);
            row.Probability.Should().Be(0.5);
            row.RowId.Should().Be("row-0");
            row.LeafSegment!.FullPath.Should().Be(@"Inbox\Leaf");
        }

        [TestMethod]
        public void ToggleLeafExpanded_WhileCollapsed_IsNoOp()
        {
            // Arrange: the leaf is hidden while the row is collapsed.
            var row = SuggestionRow(leafHasSubfolders: true);
            row.CollapseAfter(0);

            // Act / Assert
            row.ToggleLeafExpanded().Should().BeFalse();
            row.IsLeafExpanded.Should().BeFalse();
        }
    }
}
