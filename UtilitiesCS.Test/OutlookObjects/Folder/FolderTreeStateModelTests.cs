using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Exhaustive tests for <see cref="UtilitiesCS.FolderTreeStateModel"/> covering INV1-INV8, arrow-key
    /// no-ops at root and leaf, and the collapse/re-expand round-trip. Forests are built directly from
    /// in-memory <see cref="FolderNodeViewModel"/> nodes; no WinForms, COM, or timing.
    /// </summary>
    [TestClass]
    public class FolderTreeStateModelTests
    {
        private static TreeNode<FolderNodeViewModel> N(
            string path,
            string name,
            double? prob,
            int depth,
            bool hasChildren
        )
        {
            return new TreeNode<FolderNodeViewModel>(
                new FolderNodeViewModel(path, name, prob, depth, hasChildren)
            );
        }

        // Builds:
        //   A            (parent, depth 0)
        //     A\B        (parent, depth 1)
        //       A\B\C    (leaf,   depth 2, 0.9)
        //     A\D        (leaf,   depth 1, 0.8)
        //   E            (leaf,   depth 0, 0.7)
        private static IReadOnlyList<TreeNode<FolderNodeViewModel>> BuildTree(
            out TreeNode<FolderNodeViewModel> a,
            out TreeNode<FolderNodeViewModel> ab,
            out TreeNode<FolderNodeViewModel> abc,
            out TreeNode<FolderNodeViewModel> ad,
            out TreeNode<FolderNodeViewModel> e
        )
        {
            a = N("A", "A", null, 0, true);
            ab = a.AddChild(new FolderNodeViewModel("A\\B", "B", null, 1, true));
            abc = ab.AddChild(new FolderNodeViewModel("A\\B\\C", "C", 0.9, 2, false));
            ad = a.AddChild(new FolderNodeViewModel("A\\D", "D", 0.8, 1, false));
            e = N("E", "E", 0.7, 0, false);
            return new List<TreeNode<FolderNodeViewModel>> { a, e };
        }

        [TestMethod]
        public void INV1_ExpandOrToggleLeaf_IsNoOp()
        {
            var roots = BuildTree(out _, out _, out _, out var ad, out var e);
            var model = new FolderTreeStateModel(roots);

            model.Expand(ad);
            model.Toggle(e);

            ad.Value.Expanded.Should().BeFalse("a leaf is never expanded");
            e.Value.Expanded.Should().BeFalse("a leaf is never expanded");
        }

        [TestMethod]
        public void INV2_ChildVisibleOnlyWhenAncestorsExpanded_RootsAlwaysVisible()
        {
            var roots = BuildTree(out var a, out var ab, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);

            // Nothing expanded: only roots A and E are visible.
            model.GetVisibleRows().Select(r => r.FolderPath).Should().Equal(new[] { "A", "E" });

            // Expand A only: A, A\B, A\D, E (A\B\C still hidden because A\B is collapsed).
            model.Expand(a);
            model
                .GetVisibleRows()
                .Select(r => r.FolderPath)
                .Should()
                .Equal(new[] { "A", "A\\B", "A\\D", "E" });

            // Expand A\B too: A\B\C becomes visible.
            model.Expand(ab);
            model
                .GetVisibleRows()
                .Select(r => r.FolderPath)
                .Should()
                .Equal(new[] { "A", "A\\B", "A\\B\\C", "A\\D", "E" });
        }

        [TestMethod]
        public void INV3_HighlightIsSingle()
        {
            var roots = BuildTree(out var a, out _, out _, out _, out var e);
            var model = new FolderTreeStateModel(roots);

            model.Highlight(a);
            model.Highlighted.Should().BeSameAs(a);

            model.Highlight(e);
            model
                .Highlighted.Should()
                .BeSameAs(e, "the prior highlight is cleared (single highlight)");
        }

        [TestMethod]
        public void INV4_GlyphBijectionTracksExpansion()
        {
            var roots = BuildTree(out var a, out _, out _, out var ad, out _);
            var model = new FolderTreeStateModel(roots);

            a.Value.Glyph.Should().Be('+', "collapsed parent");
            ad.Value.Glyph.Should().BeNull("leaf has no glyph");

            model.Expand(a);
            a.Value.Glyph.Should().Be('-', "expanded parent");
        }

        [TestMethod]
        public void INV5_CollapsePreservesDescendantExpansion_ReExpandRestores()
        {
            var roots = BuildTree(out var a, out var ab, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);

            model.Expand(a);
            model.Expand(ab);

            // Collapse the parent A; A\B keeps its Expanded state (only visibility changes).
            model.Collapse(a);
            ab.Value.Expanded.Should()
                .BeTrue("collapsing an ancestor preserves descendant expansion");
            model.GetVisibleRows().Select(r => r.FolderPath).Should().Equal(new[] { "A", "E" });

            // Re-expand A: the previously-expanded A\B shows its child again.
            model.Expand(a);
            model
                .GetVisibleRows()
                .Select(r => r.FolderPath)
                .Should()
                .Equal(new[] { "A", "A\\B", "A\\B\\C", "A\\D", "E" });
        }

        [TestMethod]
        public void INV6_ToggleIsInvolutionOnParent()
        {
            var roots = BuildTree(out var a, out _, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);

            var original = a.Value.Expanded;
            model.Toggle(a);
            model.Toggle(a);
            a.Value.Expanded.Should().Be(original, "Toggle . Toggle == identity on a parent node");
        }

        [TestMethod]
        public void INV7_VisibleRowDepthEqualsStructuralDepth()
        {
            var roots = BuildTree(out var a, out var ab, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);
            model.Expand(a);
            model.Expand(ab);

            var rows = model.GetVisibleRows();
            rows.Single(r => r.FolderPath == "A").Depth.Should().Be(0);
            rows.Single(r => r.FolderPath == "A\\B").Depth.Should().Be(1);
            rows.Single(r => r.FolderPath == "A\\B\\C").Depth.Should().Be(2);
            rows.Single(r => r.FolderPath == "A\\D").Depth.Should().Be(1);
            rows.Single(r => r.FolderPath == "E").Depth.Should().Be(0);
        }

        [TestMethod]
        public void INV8_VisibleOrderIsStablePreOrderDfs_AndDeterministic()
        {
            // Suggestions arranged in descending score/probability order by the builder input; the
            // state model preserves that order via a stable pre-order DFS (A before its children,
            // higher-scored sibling A\B's subtree before lower-scored A\D).
            var roots = BuildTree(out var a, out var ab, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);
            model.Expand(a);
            model.Expand(ab);

            var expected = new[] { "A", "A\\B", "A\\B\\C", "A\\D", "E" };
            model.GetVisibleRows().Select(r => r.FolderPath).Should().Equal(expected);
            // Determinism: a second projection yields the identical order.
            model.GetVisibleRows().Select(r => r.FolderPath).Should().Equal(expected);
        }

        [TestMethod]
        public void RightArrow_ExpandsHighlightedCollapsedParent()
        {
            var roots = BuildTree(out var a, out _, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);
            model.Highlight(a);

            model.RightArrow();

            a.Value.Expanded.Should().BeTrue();
        }

        [TestMethod]
        public void LeftArrow_CollapsesHighlightedExpandedParent()
        {
            var roots = BuildTree(out var a, out _, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);
            model.Expand(a);
            model.Highlight(a);

            model.LeftArrow();

            a.Value.Expanded.Should().BeFalse();
        }

        [TestMethod]
        public void RightArrow_OnLeafOrAlreadyExpanded_IsNoOp()
        {
            var roots = BuildTree(out var a, out _, out _, out var ad, out _);
            var model = new FolderTreeStateModel(roots);

            // Leaf highlighted: no-op.
            model.Highlight(ad);
            model.RightArrow();
            ad.Value.Expanded.Should().BeFalse();

            // Already-expanded parent highlighted: still expanded, no throw, no change.
            model.Expand(a);
            model.Highlight(a);
            model.RightArrow();
            a.Value.Expanded.Should().BeTrue();
        }

        [TestMethod]
        public void LeftArrow_OnLeafOrAlreadyCollapsed_IsNoOp()
        {
            var roots = BuildTree(out var a, out _, out _, out var ad, out _);
            var model = new FolderTreeStateModel(roots);

            // Collapsed parent highlighted: remains collapsed.
            model.Highlight(a);
            model.LeftArrow();
            a.Value.Expanded.Should().BeFalse();

            // Leaf highlighted: no-op.
            model.Highlight(ad);
            model.LeftArrow();
            ad.Value.Expanded.Should().BeFalse();
        }

        [TestMethod]
        public void Arrows_WithNoHighlight_AreNoOp()
        {
            var roots = BuildTree(out var a, out _, out _, out _, out _);
            var model = new FolderTreeStateModel(roots);

            model.RightArrow();
            model.LeftArrow();

            a.Value.Expanded.Should().BeFalse();
            model.Highlighted.Should().BeNull();
        }
    }
}
