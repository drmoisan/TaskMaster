#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Immutable breadcrumb segment describing one folder in an ancestor chain or an immediate
    /// subfolder list. Host-neutral and net48-safe (plain class; no <c>init</c>/<c>record</c>).
    /// </summary>
    /// <remarks>
    /// The segment is deliberately probability-free. Each consuming UI feature joins the prediction
    /// percentage from the existing feature-324 plumbing keyed by <see cref="FolderPath"/>, which
    /// keeps the scoring/probability boundary untouched.
    /// </remarks>
    public sealed class FolderBreadcrumbSegment
    {
        /// <summary>
        /// Creates a breadcrumb segment for a single folder node.
        /// </summary>
        /// <param name="key">
        /// Stable identity of the folder node; used to route the expand-this-segment call. Required.
        /// </param>
        /// <param name="displayName">Folder display name (the leaf path segment).</param>
        /// <param name="folderPath">Full folder path; the selection value returned to the host.</param>
        /// <param name="hasChildren">
        /// True when the node has at least one child folder, so the UI renders the expand affordance.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public FolderBreadcrumbSegment(
            FolderTreeNodeKey key,
            string displayName,
            string folderPath,
            bool hasChildren
        )
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            DisplayName = displayName ?? string.Empty;
            FolderPath = folderPath ?? string.Empty;
            HasChildren = hasChildren;
        }

        /// <summary>Stable identity for the expand-this-segment call.</summary>
        public FolderTreeNodeKey Key { get; }

        /// <summary>Folder display name (the leaf path segment).</summary>
        public string DisplayName { get; }

        /// <summary>Full folder path; the selection value returned to the host.</summary>
        public string FolderPath { get; }

        /// <summary>True when the node has at least one child folder (render the expand affordance).</summary>
        public bool HasChildren { get; }
    }
}
