#nullable enable

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Lenient DISPLAY projection of a full Outlook folder path onto its archive-relative stem.
    /// <para>
    /// The projection returns the archive-relative stem when, and only when, the path is strictly
    /// under the configured archive root, and returns the input UNCHANGED in every other case:
    /// a path equal to the root, a path outside the root, a null or empty root, and a
    /// whitespace-only root. That last case is the behaviour change AC4 of issue #799 requires;
    /// the previous per-site logic stripped one leading separator when the root was empty.
    /// </para>
    /// <para>
    /// This is a separate type rather than an additional overload on
    /// <see cref="ArchiveStemContract"/> because that contract is a hard boundary: it yields an
    /// empty string on failure and never passes its input through, which is precisely the
    /// invariant #614 created it to enforce. Every display site needs the opposite fallback —
    /// show the caller's own text rather than nothing — so adding a lenient overload beside the
    /// strict one would blur the boundary the contract exists to defend.
    /// </para>
    /// <para>
    /// Pure by construction: no filesystem, network, COM, logging, or environment access.
    /// </para>
    /// </summary>
    public static class ArchiveStemProjection
    {
        /// <summary>
        /// Projects <paramref name="folderPath"/> onto its archive-relative stem for display.
        /// </summary>
        /// <param name="folderPath">The candidate full Outlook path. Null is returned unchanged.</param>
        /// <param name="archiveRoot">
        /// The configured archive root. Null, empty, and whitespace-only roots disable the
        /// projection and the input is returned unchanged.
        /// </param>
        /// <returns>
        /// The archive-relative stem when the path is strictly under the root; otherwise
        /// <paramref name="folderPath"/> unchanged.
        /// </returns>
        public static string? ToDisplayStem(string? folderPath, string? archiveRoot)
        {
            // The null guard is required, not defensive: ArchiveStemContract.TryMakeArchiveRelative
            // declares both inputs as non-nullable string, so passing either parameter through
            // without narrowing is CS8604 under the nullable gate.
            if (folderPath is null || archiveRoot is null)
            {
                return folderPath;
            }

            // A zero-length stem is the path-equals-root case, which the contract reports as true.
            // An empty display row is worse than the full path, so it is not projected.
            if (
                ArchiveStemContract.TryMakeArchiveRelative(folderPath, archiveRoot, out var stem)
                && stem.Length > 0
            )
            {
                return stem;
            }

            return folderPath;
        }
    }
}
