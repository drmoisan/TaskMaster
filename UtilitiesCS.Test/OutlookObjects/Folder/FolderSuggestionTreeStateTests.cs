using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Expand/collapse state-transition and <see cref="UtilitiesCS.FolderSuggestionTree.VisibleRows"/>
    /// projection tests. Cover the documented no-op rules (leaf, already-expanded Right,
    /// already-collapsed Left, banner row, null node) and the visible-row projection before and after
    /// expand/collapse. The model is pure and host-neutral.
    /// </summary>
    [TestClass]
    public class FolderSuggestionTreeStateTests
    {
        private const string Banner = "========= SUGGESTIONS =========";

        private static FolderSuggestionTree BuildParentWithTwoChildren()
        {
            return FolderSuggestionTree.BuildFromRows(
                new[] { Banner, "Root", "Root\\Child1", "Root\\Child2" }
            );
        }

        [TestMethod]
        public void VisibleRows_WhenCollapsed_HidesChildrenButShowsBannerAndRoot()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();

            // Act
            var visible = tree.VisibleRows();

            // Assert
            visible.Select(n => n.FullPath).Should().Equal(Banner, "Root");
        }

        [TestMethod]
        public void VisibleRows_AfterExpand_RevealsChildrenInOrder()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];

            // Act
            tree.Expand(root);
            var visible = tree.VisibleRows();

            // Assert
            visible
                .Select(n => n.FullPath)
                .Should()
                .Equal(Banner, "Root", "Root\\Child1", "Root\\Child2");
        }

        [TestMethod]
        public void VisibleRows_AfterExpandThenCollapse_HidesChildrenAgain()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];
            tree.Expand(root);

            // Act
            tree.Collapse(root);
            var visible = tree.VisibleRows();

            // Assert
            visible.Select(n => n.FullPath).Should().Equal(Banner, "Root");
        }

        [TestMethod]
        public void Expand_OnLeaf_IsNoOp()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { "Leaf" });
            var leaf = tree.Roots.Single();

            // Act
            tree.Expand(leaf);

            // Assert
            leaf.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void RightArrow_OnLeaf_IsNoOp()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { "Leaf" });
            var leaf = tree.Roots.Single();

            // Act
            tree.RightArrow(leaf);

            // Assert
            leaf.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void LeftArrow_OnLeaf_IsNoOp()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { "Leaf" });
            var leaf = tree.Roots.Single();

            // Act
            tree.LeftArrow(leaf);

            // Assert
            leaf.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void RightArrow_ExpandsCollapsedExpandableRoot()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];

            // Act
            tree.RightArrow(root);

            // Assert
            root.IsExpanded.Should().BeTrue();
        }

        [TestMethod]
        public void RightArrow_OnAlreadyExpandedNode_IsNoOp()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];
            tree.Expand(root);

            // Act
            tree.RightArrow(root);

            // Assert: stays expanded; visible projection unchanged.
            root.IsExpanded.Should().BeTrue();
            tree.VisibleRows()
                .Select(n => n.FullPath)
                .Should()
                .Equal(Banner, "Root", "Root\\Child1", "Root\\Child2");
        }

        [TestMethod]
        public void LeftArrow_OnAlreadyCollapsedNode_IsNoOp()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];

            // Act
            tree.LeftArrow(root);

            // Assert
            root.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void LeftArrow_CollapsesExpandedRoot()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];
            tree.Expand(root);

            // Act
            tree.LeftArrow(root);

            // Assert
            root.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void Toggle_TogglesExpandableRootBothDirections()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var root = tree.Roots[1];

            // Act + Assert
            tree.Toggle(root);
            root.IsExpanded.Should().BeTrue();
            tree.Toggle(root);
            root.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void Toggle_OnLeaf_IsNoOp()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { "Leaf" });
            var leaf = tree.Roots.Single();

            // Act
            tree.Toggle(leaf);

            // Assert
            leaf.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void Transitions_OnBannerRow_AreAllNoOps()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();
            var banner = tree.Roots[0];
            banner.Kind.Should().Be(FolderSuggestionNodeKind.Banner);

            // Act
            tree.Expand(banner);
            tree.RightArrow(banner);
            tree.Toggle(banner);
            tree.LeftArrow(banner);
            tree.Collapse(banner);

            // Assert
            banner.IsExpanded.Should().BeFalse();
        }

        [TestMethod]
        public void Transitions_OnNullNode_DoNotThrow()
        {
            // Arrange
            var tree = BuildParentWithTwoChildren();

            // Act
            Action act = () =>
            {
                tree.Expand(null);
                tree.Collapse(null);
                tree.Toggle(null);
                tree.RightArrow(null);
                tree.LeftArrow(null);
            };

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void VisibleRows_OnEmptyTree_IsEmpty()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(Array.Empty<string>());

            // Act + Assert
            tree.VisibleRows().Should().BeEmpty();
        }

        [TestMethod]
        public void VisibleRows_OnSingleNode_ReturnsThatNode()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { "Solo" });

            // Act + Assert
            tree.VisibleRows().Select(n => n.FullPath).Should().Equal("Solo");
        }
    }
}
