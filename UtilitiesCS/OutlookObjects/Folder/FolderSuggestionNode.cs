#nullable enable
using System.Collections.Generic;

namespace UtilitiesCS
{
    /// <summary>
    /// Classifies a <see cref="FolderSuggestionNode"/> so a renderer can distinguish an
    /// ordinary, potentially expandable folder row from a non-selectable, non-expandable
    /// section/banner header row.
    /// </summary>
    public enum FolderSuggestionNodeKind
    {
        /// <summary>A folder suggestion row; may be expandable if it has presented children.</summary>
        Folder,

        /// <summary>A non-selectable, non-expandable section/banner header row (for example "==== SUGGESTIONS ====").</summary>
        Banner,
    }

    /// <summary>
    /// Host-neutral, WinForms/COM-free presentation node for the EfcViewer folder-suggestion tree.
    /// Carries the folder identity (<see cref="FullPath"/>), a leaf <see cref="DisplayName"/>, the
    /// nesting <see cref="Depth"/>, ordered <see cref="Children"/>, the mutable
    /// <see cref="IsExpanded"/> UI state, the consumed upstream <see cref="Probability"/>, and the
    /// row <see cref="Kind"/>. This type contains only pure data plus expand/collapse state and is
    /// the coverage-bearing model shared by both viewers.
    /// </summary>
    public sealed class FolderSuggestionNode
    {
        private readonly List<FolderSuggestionNode> _children = new List<FolderSuggestionNode>();

        /// <summary>
        /// Creates a <see cref="FolderSuggestionNode"/>.
        /// </summary>
        /// <param name="fullPath">The folder identity (full or relative path string, backslash separated).</param>
        /// <param name="displayName">The display label (leaf path segment for a folder; banner text for a banner).</param>
        /// <param name="kind">The row classification.</param>
        public FolderSuggestionNode(
            string fullPath,
            string displayName,
            FolderSuggestionNodeKind kind
        )
        {
            FullPath = fullPath;
            DisplayName = displayName;
            Kind = kind;
        }

        /// <summary>The folder identity (full or relative path string), verbatim from the presented row.</summary>
        public string FullPath { get; }

        /// <summary>The display label: the leaf path segment for a folder row, or the banner text for a banner row.</summary>
        public string DisplayName { get; }

        /// <summary>The nesting depth within the presented hierarchy; roots (including banners) are depth 0.</summary>
        public int Depth { get; internal set; }

        /// <summary>The row classification (<see cref="FolderSuggestionNodeKind.Folder"/> or <see cref="FolderSuggestionNodeKind.Banner"/>).</summary>
        public FolderSuggestionNodeKind Kind { get; }

        /// <summary>The ordered child nodes established from the presented paths; empty for banners and leaves.</summary>
        public IReadOnlyList<FolderSuggestionNode> Children => _children;

        /// <summary>True when at least one presented path is a child of this node; false for banners and leaves.</summary>
        public bool HasChildren => _children.Count > 0;

        /// <summary>The mutable expand/collapse UI state. Always false for a banner row.</summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// The consumed upstream prediction probability in <c>[0,1]</c>, or <c>null</c> when the row
        /// carries no probability (banners, recents, and unmatched search results). Never recomputed here.
        /// </summary>
        public double? Probability { get; set; }

        /// <summary>Appends a child node, preserving insertion (presented) order. Assembly-internal build step.</summary>
        internal void AddChild(FolderSuggestionNode child)
        {
            _children.Add(child);
        }
    }
}
