#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS
{
    /// <summary>
    /// Host-neutral builder and container for the EfcViewer folder-suggestion tree. Turns the
    /// legacy sectioned <c>string[]</c> produced by <c>FolderPredictor</c> into an ordered forest of
    /// <see cref="FolderSuggestionNode"/> roots, deriving parent/child edges from the presented paths
    /// only (no ancestor synthesis). This type is pure, in-memory, and free of WinForms/COM
    /// dependencies so it can be unit tested directly.
    /// </summary>
    public sealed class FolderSuggestionTree
    {
        private const string BannerPrefix = "====";
        private const string PathSeparator = "\\";

        private readonly List<FolderSuggestionNode> _roots;

        private FolderSuggestionTree(List<FolderSuggestionNode> roots)
        {
            _roots = roots;
        }

        /// <summary>The ordered top-level nodes: banner headers and per-section root folders, in presented order.</summary>
        public IReadOnlyList<FolderSuggestionNode> Roots => _roots;

        /// <summary>
        /// Builds a <see cref="FolderSuggestionTree"/> from a sectioned list of presented rows.
        /// Rows beginning with <c>"===="</c> are non-expandable banner headers that partition the
        /// input into sections; within each section, a path <c>Y</c> is a child of the presented
        /// path <c>X</c> when <c>Y</c> starts with <c>X + "\"</c> and <c>X</c> is the longest such
        /// presented prefix. Paths whose parent prefix is not present become section roots. Per-section
        /// input order is preserved and suggestions are never re-sorted.
        /// </summary>
        /// <param name="rows">The presented rows (full/relative folder paths and banner headers). A null input yields an empty tree.</param>
        /// <returns>An ordered forest with banner classification and parent/child edges.</returns>
        public static FolderSuggestionTree BuildFromRows(IReadOnlyList<string> rows)
        {
            var roots = new List<FolderSuggestionNode>();
            if (rows == null)
            {
                return new FolderSuggestionTree(roots);
            }

            var sectionNodes = new List<FolderSuggestionNode>();

            void FlushSection()
            {
                foreach (var node in sectionNodes)
                {
                    FolderSuggestionNode? parent = FindLongestPrefixParent(node, sectionNodes);
                    if (parent != null)
                    {
                        parent.AddChild(node);
                    }
                    else
                    {
                        roots.Add(node);
                    }
                }

                sectionNodes.Clear();
            }

            foreach (var row in rows)
            {
                if (IsBanner(row))
                {
                    FlushSection();
                    roots.Add(new FolderSuggestionNode(row, row, FolderSuggestionNodeKind.Banner));
                }
                else
                {
                    sectionNodes.Add(
                        new FolderSuggestionNode(
                            row,
                            LeafSegment(row),
                            FolderSuggestionNodeKind.Folder
                        )
                    );
                }
            }

            FlushSection();

            AssignDepth(roots, 0);
            return new FolderSuggestionTree(roots);
        }

        /// <summary>
        /// Projects the tree into the ordered list of currently visible rows via a pre-order flatten.
        /// A node's children are emitted only when <see cref="FolderSuggestionNode.IsExpanded"/> is
        /// true; banner rows are always emitted in section order and are never descended into.
        /// </summary>
        /// <returns>The visible rows in display order for the current expand/collapse state.</returns>
        public IReadOnlyList<FolderSuggestionNode> VisibleRows()
        {
            var result = new List<FolderSuggestionNode>();
            foreach (var root in _roots)
            {
                Flatten(root, result);
            }

            return result;
        }

        private static void Flatten(
            FolderSuggestionNode node,
            List<FolderSuggestionNode> accumulator
        )
        {
            accumulator.Add(node);

            // Banner rows are never expandable, so never descend into them.
            if (node.Kind == FolderSuggestionNodeKind.Banner || !node.IsExpanded)
            {
                return;
            }

            foreach (var child in node.Children)
            {
                Flatten(child, accumulator);
            }
        }

        /// <summary>
        /// Expands <paramref name="node"/> when it is an expandable folder that is currently
        /// collapsed. A banner, a leaf, or an already-expanded node is a no-op.
        /// </summary>
        public void Expand(FolderSuggestionNode node)
        {
            if (node == null || node.Kind == FolderSuggestionNodeKind.Banner)
            {
                return;
            }

            if (node.HasChildren && !node.IsExpanded)
            {
                node.IsExpanded = true;
            }
        }

        /// <summary>
        /// Collapses <paramref name="node"/> when it is currently expanded. A banner, a leaf, or an
        /// already-collapsed node is a no-op.
        /// </summary>
        public void Collapse(FolderSuggestionNode node)
        {
            if (node == null || node.Kind == FolderSuggestionNodeKind.Banner)
            {
                return;
            }

            if (node.IsExpanded)
            {
                node.IsExpanded = false;
            }
        }

        /// <summary>
        /// Toggles the expand/collapse state of an expandable folder node (mouse plus/minus click).
        /// A banner or a leaf node is a no-op.
        /// </summary>
        public void Toggle(FolderSuggestionNode node)
        {
            if (node == null || node.Kind == FolderSuggestionNodeKind.Banner || !node.HasChildren)
            {
                return;
            }

            node.IsExpanded = !node.IsExpanded;
        }

        /// <summary>
        /// Right-arrow keyboard behavior on the highlighted node: expands an expandable, collapsed
        /// folder; a leaf, an already-expanded node, or a banner is a no-op.
        /// </summary>
        public void RightArrow(FolderSuggestionNode node)
        {
            Expand(node);
        }

        /// <summary>
        /// Left-arrow keyboard behavior on the highlighted node: collapses an expanded folder; a
        /// leaf, an already-collapsed node, or a banner is a no-op.
        /// </summary>
        public void LeftArrow(FolderSuggestionNode node)
        {
            Collapse(node);
        }

        /// <summary>True when the row is a section/banner header (begins with <c>"===="</c>).</summary>
        private static bool IsBanner(string row)
        {
            return row != null && row.StartsWith(BannerPrefix, StringComparison.Ordinal);
        }

        /// <summary>Returns the leaf path segment (text after the final backslash), or the whole string when unseparated.</summary>
        private static string LeafSegment(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            int index = path.LastIndexOf(PathSeparator, StringComparison.Ordinal);
            return index >= 0 ? path.Substring(index + PathSeparator.Length) : path;
        }

        /// <summary>
        /// Finds the presented node whose <see cref="FolderSuggestionNode.FullPath"/> is the longest
        /// prefix of <paramref name="node"/>'s path (with a trailing separator), or null when no
        /// presented ancestor exists.
        /// </summary>
        private static FolderSuggestionNode? FindLongestPrefixParent(
            FolderSuggestionNode node,
            List<FolderSuggestionNode> candidates
        )
        {
            FolderSuggestionNode? best = null;
            foreach (var candidate in candidates)
            {
                if (ReferenceEquals(candidate, node))
                {
                    continue;
                }

                string prefix = candidate.FullPath + PathSeparator;
                if (node.FullPath.StartsWith(prefix, StringComparison.Ordinal))
                {
                    if (best == null || candidate.FullPath.Length > best.FullPath.Length)
                    {
                        best = candidate;
                    }
                }
            }

            return best;
        }

        /// <summary>Assigns nesting depth by walking each root's subtree (roots are depth 0).</summary>
        private static void AssignDepth(IReadOnlyList<FolderSuggestionNode> nodes, int depth)
        {
            foreach (var node in nodes)
            {
                node.Depth = depth;
                AssignDepth(node.Children, depth + 1);
            }
        }
    }
}
