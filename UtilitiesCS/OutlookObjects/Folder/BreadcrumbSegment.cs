#nullable enable
using System;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Pure, host-neutral breadcrumb segment used by the EfcViewer breadcrumb row model (#349).
    /// Immutable net48-safe class (no <c>record</c>/<c>init</c>); carries no WinForms, COM, or
    /// WebView2 types.
    /// </summary>
    /// <remarks>
    /// Built from the 9101 provider's <see cref="FolderBreadcrumbSegment"/> by
    /// <c>BreadcrumbRowBuilder</c> (mapping <c>FolderPath</c> to <see cref="FullPath"/> and
    /// <c>HasChildren</c> to <see cref="HasSubfolders"/> per the P0-T6 dependency-gate record).
    /// </remarks>
    public sealed class BreadcrumbSegment
    {
        /// <summary>
        /// Creates a breadcrumb segment.
        /// </summary>
        /// <param name="fullPath">Full folder path; also the filing-target selection value.</param>
        /// <param name="displayName">Folder display name shown in the breadcrumb.</param>
        /// <param name="hasSubfolders">
        /// True when the folder has at least one child folder (gates the expand affordance).
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fullPath"/> or <paramref name="displayName"/> is null.
        /// </exception>
        public BreadcrumbSegment(string fullPath, string displayName, bool hasSubfolders)
        {
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            HasSubfolders = hasSubfolders;
        }

        /// <summary>Full folder path; the selection value returned to the host.</summary>
        public string FullPath { get; }

        /// <summary>Folder display name (the leaf path segment).</summary>
        public string DisplayName { get; }

        /// <summary>True when the folder has at least one child folder.</summary>
        public bool HasSubfolders { get; }
    }
}
