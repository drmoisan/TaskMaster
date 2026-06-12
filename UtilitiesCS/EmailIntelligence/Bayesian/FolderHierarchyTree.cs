using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Builds and holds a parent-to-children folder hierarchy reconstructed from
    /// backslash-delimited relative paths. The hierarchy is a
    /// <see cref="Dictionary{TKey, TValue}"/> keyed by full node path (empty string for the
    /// synthetic root). Each adjacent segment pair in a path becomes a parent-to-child edge.
    /// This type is pure logic with no Outlook COM or filesystem dependency.
    /// </summary>
    /// <remarks>
    /// Path comparison defaults to <see cref="StringComparer.Ordinal"/>. Supplying
    /// <paramref name="comparer"/> as <see cref="StringComparer.OrdinalIgnoreCase"/> at
    /// construction makes node keys and child segments case-insensitive. Child registration is
    /// idempotent: duplicate paths produce no duplicate children because each parent tracks its
    /// children in a set.
    /// </remarks>
    public sealed class FolderHierarchyTree
    {
        private const char PathSeparator = '\\';

        /// <summary>The synthetic root key (the parent of every top-level segment).</summary>
        public const string RootKey = "";

        private readonly StringComparer _comparer;

        // Per-node ordered, deduplicated child segment sets. The dictionary itself and each
        // child set use the configured comparer so case sensitivity is consistent throughout.
        private readonly Dictionary<string, ChildSet> _children;

        /// <summary>
        /// Initializes an empty tree containing only the synthetic root with no children.
        /// </summary>
        /// <param name="comparer">
        /// Comparer for node keys and child segments. Defaults to <see cref="StringComparer.Ordinal"/>
        /// when null.
        /// </param>
        public FolderHierarchyTree(StringComparer comparer = null)
        {
            _comparer = comparer ?? StringComparer.Ordinal;
            _children = new Dictionary<string, ChildSet>(_comparer);
            EnsureNode(RootKey);
        }

        /// <summary>
        /// Builds a tree from a collection of backslash-delimited relative paths. Null or empty
        /// path entries are skipped. Construction is idempotent with respect to duplicate paths.
        /// </summary>
        /// <param name="relativePaths">The relative paths to parse; must not be null.</param>
        /// <param name="comparer">Optional comparer; defaults to <see cref="StringComparer.Ordinal"/>.</param>
        /// <returns>A populated <see cref="FolderHierarchyTree"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativePaths"/> is null.</exception>
        public static FolderHierarchyTree Build(
            IEnumerable<string> relativePaths,
            StringComparer comparer = null
        )
        {
            if (relativePaths is null)
            {
                throw new ArgumentNullException(nameof(relativePaths));
            }

            var tree = new FolderHierarchyTree(comparer);
            foreach (var path in relativePaths)
            {
                tree.AddPath(path);
            }

            return tree;
        }

        /// <summary>
        /// Adds a full backslash-delimited path to the tree, registering each adjacent segment
        /// pair as a parent-to-child edge. A single-segment path yields exactly one edge from the
        /// root to that segment. Null, empty, or whitespace-only paths are ignored.
        /// </summary>
        /// <param name="relativePath">The path to add.</param>
        public void AddPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var segments = relativePath
                .Split(PathSeparator)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (segments.Length == 0)
            {
                return;
            }

            var parentKey = RootKey;
            foreach (var segment in segments)
            {
                AddLeaf(parentKey, segment);
                parentKey = Combine(parentKey, segment);
            }
        }

        /// <summary>
        /// Adds a single child segment to one parent node only, leaving all other parents
        /// unchanged. The child node is registered (as a leaf with zero children) if it does not
        /// already exist. Registration is idempotent.
        /// </summary>
        /// <param name="parentKey">The full key of the parent (empty string for the root).</param>
        /// <param name="childSegment">The direct child segment to add; must be non-empty.</param>
        /// <returns>The full key of the added child node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentKey"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="childSegment"/> is null or empty.</exception>
        public string AddLeaf(string parentKey, string childSegment)
        {
            if (parentKey is null)
            {
                throw new ArgumentNullException(nameof(parentKey));
            }

            if (string.IsNullOrEmpty(childSegment))
            {
                throw new ArgumentException(
                    "Child segment must be a non-empty string.",
                    nameof(childSegment)
                );
            }

            EnsureNode(parentKey).Add(childSegment);
            var childKey = Combine(parentKey, childSegment);
            EnsureNode(childKey);
            return childKey;
        }

        /// <summary>
        /// Gets the immutable hierarchy node for <paramref name="nodeKey"/>, or null when the
        /// node is not present.
        /// </summary>
        /// <param name="nodeKey">The full node key (empty string for the root).</param>
        /// <returns>The node snapshot, or null when absent.</returns>
        public FolderHierarchyNode GetNode(string nodeKey)
        {
            if (nodeKey is null || !_children.TryGetValue(nodeKey, out var set))
            {
                return null;
            }

            return new FolderHierarchyNode(nodeKey, set.ToArray());
        }

        /// <summary>
        /// Returns the direct child segments of <paramref name="nodeKey"/> in insertion order.
        /// Returns an empty array for an unknown or leaf node.
        /// </summary>
        /// <param name="nodeKey">The full node key (empty string for the root).</param>
        /// <returns>The direct child segments.</returns>
        public string[] GetChildren(string nodeKey)
        {
            if (nodeKey is null || !_children.TryGetValue(nodeKey, out var set))
            {
                return Array.Empty<string>();
            }

            return set.ToArray();
        }

        /// <summary>
        /// Indicates whether the node exists and has zero children (a leaf). The synthetic root is
        /// a leaf only when the tree is empty.
        /// </summary>
        /// <param name="nodeKey">The full node key.</param>
        /// <returns>True when the node exists and has no children; otherwise false.</returns>
        public bool IsLeaf(string nodeKey)
        {
            return nodeKey != null && _children.TryGetValue(nodeKey, out var set) && set.Count == 0;
        }

        /// <summary>Indicates whether a node with the given key exists in the tree.</summary>
        /// <param name="nodeKey">The full node key.</param>
        /// <returns>True when the node exists; otherwise false.</returns>
        public bool ContainsNode(string nodeKey)
        {
            return nodeKey != null && _children.ContainsKey(nodeKey);
        }

        /// <summary>Gets the full set of node keys in the tree, including the root.</summary>
        public IReadOnlyCollection<string> NodeKeys => _children.Keys.ToArray();

        /// <summary>Gets the number of nodes in the tree, including the synthetic root.</summary>
        public int NodeCount => _children.Count;

        private ChildSet EnsureNode(string nodeKey)
        {
            if (!_children.TryGetValue(nodeKey, out var set))
            {
                set = new ChildSet(_comparer);
                _children[nodeKey] = set;
            }

            return set;
        }

        private static string Combine(string parentKey, string childSegment)
        {
            return parentKey.Length == 0 ? childSegment : parentKey + PathSeparator + childSegment;
        }

        // Ordered, deduplicated set of direct child segments for a single parent. Insertion order
        // is preserved for deterministic enumeration; the configured comparer enforces uniqueness.
        private sealed class ChildSet
        {
            private readonly List<string> _order;
            private readonly HashSet<string> _seen;

            public ChildSet(StringComparer comparer)
            {
                _order = new List<string>();
                _seen = new HashSet<string>(comparer);
            }

            public int Count => _order.Count;

            public void Add(string segment)
            {
                if (_seen.Add(segment))
                {
                    _order.Add(segment);
                }
            }

            public string[] ToArray() => _order.ToArray();
        }
    }
}
