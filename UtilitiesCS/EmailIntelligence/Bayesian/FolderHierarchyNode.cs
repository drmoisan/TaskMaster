#nullable enable
using System;
using Newtonsoft.Json;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Immutable description of one node in a folder hierarchy reconstructed from
    /// backslash-delimited relative paths. A node is identified by its full
    /// <see cref="NodeKey"/> (empty string for the synthetic root) and carries the set of
    /// direct child segments beneath it. The type is a pure data record with no I/O or
    /// Outlook COM dependency and is serializable by Newtonsoft.Json.
    /// </summary>
    /// <remarks>
    /// <see cref="Children"/> holds direct child <em>segments</em> (single path components),
    /// not full paths. A leaf node has an empty <see cref="Children"/> array.
    /// </remarks>
    public sealed record FolderHierarchyNode
    {
        /// <summary>
        /// The full backslash-delimited path that identifies this node. The synthetic root
        /// uses the empty string.
        /// </summary>
        public string NodeKey { get; }

        /// <summary>
        /// The direct child segments beneath this node, in insertion order. Empty for a leaf.
        /// </summary>
        public string[] Children { get; }

        /// <summary>
        /// Initializes a new <see cref="FolderHierarchyNode"/>.
        /// </summary>
        /// <param name="nodeKey">The full path key; empty string for the root. Must not be null.</param>
        /// <param name="children">The direct child segments; must not be null (use an empty array for a leaf).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="nodeKey"/> or <paramref name="children"/> is null.</exception>
        [JsonConstructor]
        public FolderHierarchyNode(string nodeKey, string[] children)
        {
            NodeKey = nodeKey ?? throw new ArgumentNullException(nameof(nodeKey));
            Children = children ?? throw new ArgumentNullException(nameof(children));
        }
    }
}
