#nullable enable
namespace UtilitiesCS
{
    /// <summary>
    /// Host-neutral per-node view model for the QuickFiler folder-dropdown tree. Carries the folder
    /// identity (<see cref="FolderPath"/>), the display segment (<see cref="DisplayName"/>), an
    /// optional prediction <see cref="Probability"/> (sourced from
    /// <see cref="FolderScore.Probability"/>; <c>null</c> for synthesized ancestors, separators,
    /// search results, and recents), the tree <see cref="Depth"/>, whether the node
    /// <see cref="HasChildren"/>, and the mutable <see cref="Expanded"/> state. The derived
    /// <see cref="Glyph"/> and <see cref="FormattedPercentage"/> members supply the exact affordance
    /// character and right-aligned percentage text the owner-draw renderer paints. This seam is NOT
    /// coverage-exempt. net48-safe plain class (no <c>record</c>/<c>init</c>).
    /// </summary>
    public class FolderNodeViewModel
    {
        /// <summary>
        /// Creates a <see cref="FolderNodeViewModel"/>.
        /// </summary>
        /// <param name="folderPath">Full folder path; retained as the node key/selection value.</param>
        /// <param name="displayName">The display segment (typically the last path segment).</param>
        /// <param name="probability">
        /// The prediction probability in <c>[0,1]</c> for a scored leaf, or <c>null</c> when the row
        /// carries no probability.
        /// </param>
        /// <param name="depth">The zero-based tree depth used for indent.</param>
        /// <param name="hasChildren">Whether this node has child nodes (expandable).</param>
        public FolderNodeViewModel(
            string folderPath,
            string displayName,
            double? probability,
            int depth,
            bool hasChildren
        )
        {
            FolderPath = folderPath;
            DisplayName = displayName;
            Probability = probability;
            Depth = depth;
            HasChildren = hasChildren;
        }

        /// <summary>Full folder path; the node key and the value returned to the controller on selection.</summary>
        public string FolderPath { get; }

        /// <summary>The display segment shown in the dropdown (typically the last path segment).</summary>
        public string DisplayName { get; }

        /// <summary>The prediction probability in <c>[0,1]</c>, or <c>null</c> when the row has none.</summary>
        public double? Probability { get; }

        /// <summary>The zero-based tree depth; the rendered indent equals this value (INV7).</summary>
        public int Depth { get; }

        /// <summary>Whether this node has children and is therefore expandable.</summary>
        public bool HasChildren { get; }

        /// <summary>The mutable expand/collapse state. A leaf is never expanded (INV1).</summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// The affordance glyph (INV4): <c>'+'</c> when <see cref="HasChildren"/> and not
        /// <see cref="Expanded"/>, <c>'-'</c> when <see cref="HasChildren"/> and
        /// <see cref="Expanded"/>, and <c>null</c> (no glyph) for a leaf.
        /// </summary>
        public char? Glyph
        {
            get
            {
                if (!HasChildren)
                {
                    return null;
                }
                return Expanded ? '-' : '+';
            }
        }

        /// <summary>
        /// The right-aligned percentage text: <see cref="PercentageFormatter.FormatPercent(double?)"/> of
        /// <see cref="Probability"/> when it is non-null, otherwise the empty string.
        /// </summary>
        public string FormattedPercentage
        {
            get
            {
                return Probability.HasValue
                    ? PercentageFormatter.FormatPercent(Probability.Value)
                    : string.Empty;
            }
        }
    }
}
