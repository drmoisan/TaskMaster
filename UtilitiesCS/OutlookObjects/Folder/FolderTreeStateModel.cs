#nullable enable
using System.Collections.Generic;

namespace UtilitiesCS
{
    /// <summary>
    /// Pure, host-neutral expand/collapse/highlight state machine over a forest of
    /// <see cref="TreeNode{T}"/> of <see cref="FolderNodeViewModel"/>, plus a stable pre-order-DFS
    /// visible-row projection. This is the C3 state-transition driver for the QuickFiler folder
    /// dropdown and enforces INV1-INV8 (see the feature spec). It performs no WinForms, COM, I/O, or
    /// timing work and is NOT coverage-exempt.
    /// </summary>
    public class FolderTreeStateModel
    {
        private readonly IReadOnlyList<TreeNode<FolderNodeViewModel>> _roots;
        private TreeNode<FolderNodeViewModel>? _highlighted;

        /// <summary>
        /// Creates a state model over the given root forest (typically from
        /// <see cref="FolderHierarchyBuilder.Build"/>).
        /// </summary>
        /// <param name="roots">The forest of root nodes; must not be null.</param>
        public FolderTreeStateModel(IReadOnlyList<TreeNode<FolderNodeViewModel>> roots)
        {
            _roots = roots ?? new List<TreeNode<FolderNodeViewModel>>();
        }

        /// <summary>The currently highlighted node, or <c>null</c> when nothing is highlighted (INV3).</summary>
        public TreeNode<FolderNodeViewModel>? Highlighted => _highlighted;

        /// <summary>Expands <paramref name="node"/> when it has children; a no-op for a leaf (INV1).</summary>
        public void Expand(TreeNode<FolderNodeViewModel> node)
        {
            if (node != null && node.Value.HasChildren)
            {
                node.Value.Expanded = true;
            }
        }

        /// <summary>Collapses <paramref name="node"/>; descendant expansion state is preserved (INV5).</summary>
        public void Collapse(TreeNode<FolderNodeViewModel> node)
        {
            if (node != null)
            {
                // Only the node's own Expanded flag changes; descendant flags are left intact so a
                // later re-expand restores the prior inner expansion (INV5).
                node.Value.Expanded = false;
            }
        }

        /// <summary>Toggles <paramref name="node"/> expansion when it has children; leaf no-op (INV1/INV6).</summary>
        public void Toggle(TreeNode<FolderNodeViewModel> node)
        {
            if (node == null || !node.Value.HasChildren)
            {
                return;
            }

            if (node.Value.Expanded)
            {
                Collapse(node);
            }
            else
            {
                Expand(node);
            }
        }

        /// <summary>Highlights <paramref name="node"/>, clearing any prior highlight (INV3).</summary>
        public void Highlight(TreeNode<FolderNodeViewModel> node)
        {
            // A single stored reference inherently enforces at-most-one highlight (INV3).
            _highlighted = node;
        }

        /// <summary>
        /// Expands the highlighted node when it has children and is collapsed; otherwise a no-op
        /// (leaf, already expanded, or no highlight).
        /// </summary>
        public void RightArrow()
        {
            if (
                _highlighted != null
                && _highlighted.Value.HasChildren
                && !_highlighted.Value.Expanded
            )
            {
                Expand(_highlighted);
            }
        }

        /// <summary>
        /// Collapses the highlighted node when it has children and is expanded; otherwise a no-op
        /// (leaf, already collapsed, or no highlight).
        /// </summary>
        public void LeftArrow()
        {
            if (
                _highlighted != null
                && _highlighted.Value.HasChildren
                && _highlighted.Value.Expanded
            )
            {
                Collapse(_highlighted);
            }
        }

        /// <summary>
        /// Projects the currently-visible rows as a stable pre-order DFS over the forest: every root
        /// is visible; a node's children are visible only when the node is
        /// <see cref="FolderNodeViewModel.Expanded"/> (INV2). Row order is deterministic and stable
        /// (INV8); each row's <see cref="FolderNodeViewModel.Depth"/> is the render indent (INV7).
        /// </summary>
        public IReadOnlyList<FolderNodeViewModel> GetVisibleRows()
        {
            var rows = new List<FolderNodeViewModel>();
            foreach (var node in GetVisibleNodes())
            {
                rows.Add(node.Value);
            }
            return rows;
        }

        /// <summary>
        /// The visible nodes in the same stable pre-order-DFS order as <see cref="GetVisibleRows"/>,
        /// returned as tree nodes so host glue can map a clicked/selected row back to a node for
        /// toggling and highlighting.
        /// </summary>
        public IReadOnlyList<TreeNode<FolderNodeViewModel>> GetVisibleNodes()
        {
            var nodes = new List<TreeNode<FolderNodeViewModel>>();
            foreach (var root in _roots)
            {
                AppendVisible(root, nodes);
            }
            return nodes;
        }

        /// <summary>
        /// Stable pre-order DFS: emit the node, then (only when it is expanded) recurse into its
        /// children in their existing forest order, which the builder set from the predictor's
        /// descending-score input (INV8).
        /// </summary>
        private static void AppendVisible(
            TreeNode<FolderNodeViewModel> node,
            List<TreeNode<FolderNodeViewModel>> sink
        )
        {
            sink.Add(node);
            if (node.Value.Expanded)
            {
                foreach (var child in node.Children)
                {
                    AppendVisible(child, sink);
                }
            }
        }
    }
}
