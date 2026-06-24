using System.Collections.Generic;
using System.Linq;

namespace TaskMaster
{
    /// <summary>
    /// Abstraction over a single folder node for junk-folder path navigation: a folder's
    /// display name plus its direct child folders. This narrow seam lets
    /// <see cref="JunkFolderPathNavigator"/> resolve a configured relative path without a live
    /// Outlook COM object, so the navigation logic is deterministically unit-testable.
    /// </summary>
    internal interface IFolderNode
    {
        /// <summary>The folder's display name (equivalent to <c>MAPIFolder.Name</c>).</summary>
        string Name { get; }

        /// <summary>
        /// The folder's DIRECT child folders. Implementations should enumerate only one level on
        /// demand (no recursion, no eager full-tree walk) so navigation touches only the folders
        /// along the resolution path plus the first-segment breadth-first frontier.
        /// </summary>
        IReadOnlyList<IFolderNode> ChildFolders { get; }
    }

    /// <summary>
    /// Pure, COM-free path navigator that resolves a stored junk-folder relative path to the
    /// matching <see cref="IFolderNode"/>, reproducing EXACTLY the matching semantics of the
    /// legacy <c>new FolderTree(Root)</c> + <c>TreeNode&lt;FolderWrapper&gt;.FindSequentialNode</c>
    /// path it replaces (issue #211, AC10).
    ///
    /// <para>Equivalence contract (binding — a wrong folder would misroute junk email):</para>
    /// <list type="number">
    /// <item>The path is split on <c>'\\'</c> verbatim (no trim; empty segments are produced by
    /// <see cref="string.Split(char[])"/> as-is), then resolved in sequence.</item>
    /// <item>FIRST segment: a breadth-first search starting AT THE ROOT NODE ITSELF — the root is
    /// matched first, otherwise descendants are scanned level by level and the first ordinal
    /// <c>Name ==</c> match (shallowest, breadth-first order) is taken. This mirrors
    /// <c>FindNode(comparator, descendByLevel: true)</c>.</item>
    /// <item>SUBSEQUENT segments: matched ONLY against the current node's DIRECT children, first
    /// ordinal <c>Name ==</c> match wins. This mirrors
    /// <c>node.Children?.Where(...)?.FirstOrDefault()</c>.</item>
    /// <item>Any unmatched segment yields <c>null</c> (not found).</item>
    /// <item>Comparison is ordinal <c>string ==</c>: case-sensitive, no trimming, no culture —
    /// identical to the existing comparator <c>(current, other) =&gt; current.Name == other</c>.</item>
    /// </list>
    /// Resolution touches only the folders along the resolution path plus the breadth-first
    /// frontier required for the first-segment match; it never enumerates the entire tree.
    /// </summary>
    internal static class JunkFolderPathNavigator
    {
        /// <summary>
        /// Resolves <paramref name="relativePath"/> against <paramref name="root"/> using the
        /// equivalence contract described on <see cref="JunkFolderPathNavigator"/>.
        /// </summary>
        /// <param name="root">The root folder node (the default-store root). May be null.</param>
        /// <param name="relativePath">
        /// The stored relative path (root prefix stripped), split on <c>'\\'</c>.
        /// </param>
        /// <returns>The matched <see cref="IFolderNode"/>, or <c>null</c> if any segment is unmatched.</returns>
        internal static IFolderNode ResolvePath(IFolderNode root, string relativePath)
        {
            if (root is null || relativePath is null)
            {
                return null;
            }

            var segments = Split(relativePath);
            if (segments.Length == 0)
            {
                return null;
            }

            var node = MatchFirstSegment(root, segments[0]);
            for (var i = 1; i < segments.Length && node is not null; i++)
            {
                node = MatchChild(node, segments[i]);
            }

            return node;
        }

        /// <summary>Splits the path on <c>'\\'</c> verbatim, mirroring <c>folderPath.Split('\\')</c>.</summary>
        internal static string[] Split(string relativePath) => relativePath.Split('\\');

        /// <summary>
        /// Reproduces <c>FindNode(comparator, descendByLevel: true)</c>: a breadth-first search
        /// starting at <paramref name="root"/> itself, returning the first node (root first, then
        /// level by level) whose <see cref="IFolderNode.Name"/> is ordinally equal to
        /// <paramref name="segment"/>; <c>null</c> if none match.
        /// </summary>
        internal static IFolderNode MatchFirstSegment(IFolderNode root, string segment)
        {
            var level = new List<IFolderNode> { root };
            while (level.Count > 0)
            {
                var match = level.FirstOrDefault(node => node.Name == segment);
                if (match is not null)
                {
                    return match;
                }

                level = NextLevel(level);
            }

            return null;
        }

        /// <summary>
        /// Reproduces <c>node.Children?.Where(x =&gt; comparator(x.Value, next))?.FirstOrDefault()</c>:
        /// matches only the DIRECT children of <paramref name="node"/>, first ordinal
        /// <see cref="IFolderNode.Name"/> match wins; <c>null</c> if none match.
        /// </summary>
        internal static IFolderNode MatchChild(IFolderNode node, string segment)
        {
            var children = node.ChildFolders;
            if (children is null)
            {
                return null;
            }

            foreach (var child in children)
            {
                if (child.Name == segment)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// Expands the breadth-first frontier by one level, mirroring
        /// <c>GetNextLevel</c>: the concatenation of the direct children of the current level.
        /// </summary>
        private static List<IFolderNode> NextLevel(List<IFolderNode> level)
        {
            var next = new List<IFolderNode>();
            foreach (var node in level)
            {
                var children = node.ChildFolders;
                if (children is null)
                {
                    continue;
                }

                foreach (var child in children)
                {
                    if (child is not null)
                    {
                        next.Add(child);
                    }
                }
            }

            return next;
        }
    }
}
