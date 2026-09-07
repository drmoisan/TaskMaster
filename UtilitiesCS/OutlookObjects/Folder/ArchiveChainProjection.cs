#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Trims a breadcrumb ancestor chain so that only the lineage BELOW the configured Outlook
    /// archive root is presented (#799 AC1 and AC2).
    /// <para>
    /// Pure by construction: no filesystem, network, COM, logging, or environment access. Segment
    /// instances are passed through by reference; no segment is rebuilt or reordered.
    /// </para>
    /// </summary>
    public static class ArchiveChainProjection
    {
        /// <summary>
        /// Finds the archive-root node in <paramref name="chain"/> and yields the remainder of the
        /// chain that follows it.
        /// <para>
        /// The archive-root node is the FIRST chain index whose segment
        /// <see cref="FolderBreadcrumbSegment.FolderPath"/> is the root itself, detected as
        /// <see cref="ArchiveStemContract.TryMakeArchiveRelative(string, string, out string)"/>
        /// returning true with an EMPTY stem, which is exactly the path-equals-root case. The
        /// method returns the segments after that index.
        /// </para>
        /// <para>
        /// It returns false when no such index exists, and also when that index is the LAST
        /// element: the leaf is then the archive root itself and there is nothing below it to
        /// render. It also returns false for a null or empty chain and for a null, empty, or
        /// whitespace-only root.
        /// </para>
        /// </summary>
        /// <param name="chain">The root-to-leaf ancestor chain. Null returns false.</param>
        /// <param name="archiveRoot">
        /// The configured archive root. Null, empty, and whitespace-only roots return false.
        /// </param>
        /// <param name="trimmed">
        /// The segments below the archive root on success; an empty list on every failing path.
        /// </param>
        /// <returns>True when a proper lineage below the archive root exists; otherwise false.</returns>
        public static bool TryTrimBelowArchiveRoot(
            IReadOnlyList<FolderBreadcrumbSegment>? chain,
            string? archiveRoot,
            out IReadOnlyList<FolderBreadcrumbSegment> trimmed
        )
        {
            trimmed = Array.Empty<FolderBreadcrumbSegment>();

            // The null half of this guard is required, not defensive: ArchiveStemContract declares
            // both inputs as non-nullable string, so passing archiveRoot through without narrowing
            // is CS8604 under the nullable gate. A whitespace-only root is rejected by the
            // contract's own guard, so it needs no separate test here.
            if (chain is null || chain.Count == 0 || archiveRoot is null)
            {
                return false;
            }

            for (int index = 0; index < chain.Count; index++)
            {
                bool isRootNode =
                    ArchiveStemContract.TryMakeArchiveRelative(
                        chain[index].FolderPath,
                        archiveRoot,
                        out var stem
                    )
                    && stem.Length == 0;

                if (!isRootNode)
                {
                    continue;
                }

                // The leaf IS the archive root: there is nothing below it to render.
                if (index == chain.Count - 1)
                {
                    return false;
                }

                var below = new FolderBreadcrumbSegment[chain.Count - index - 1];
                for (int offset = 0; offset < below.Length; offset++)
                {
                    below[offset] = chain[index + 1 + offset];
                }

                trimmed = below;
                return true;
            }

            return false;
        }
    }
}
