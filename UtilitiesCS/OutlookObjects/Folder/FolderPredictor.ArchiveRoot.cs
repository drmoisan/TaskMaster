#nullable enable
using System;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS
{
    /// <summary>
    /// The archive-root read used by the DISPLAY surfaces of <see cref="FolderPredictor"/>.
    /// <para>
    /// Issue #812: the suggestion and recent-selection surfaces read
    /// <c>IOlObjects.ArchiveRootPath</c> only to compute a cosmetic archive-relative stem, but a
    /// store that cannot resolve its archive root turned the whole folder list into an exception.
    /// The guarded accessor here degrades that one read, so the list renders unprojected instead
    /// of not rendering at all. The five FUNCTIONAL reads in
    /// <c>FolderPredictor.cs</c> are deliberately not routed through it and still throw.
    /// </para>
    /// </summary>
    public partial class FolderPredictor
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// Reads the configured archive root for DISPLAY purposes, yielding <see langword="null"/>
        /// when it cannot be resolved.
        /// <para>
        /// Only <see cref="InvalidOperationException"/> is absorbed. That is the exception an
        /// unresolvable archive root raises, and it is the only failure a cosmetic projection may
        /// degrade past. Every other failure, including a COM failure, still propagates, because a
        /// COM fault is not evidence that the root is merely unconfigured and hiding it would
        /// convert a real store problem into a silently wrong folder list.
        /// </para>
        /// <para>
        /// A null return is not an error path for the caller: the display projection treats a null
        /// root as "no projection configured" and returns each folder path unchanged.
        /// </para>
        /// <para>
        /// The warning withholds the archive-root path and any mailbox address. Both are personal
        /// data and neither is needed to act on the warning; the exception carried alongside the
        /// message retains the detail for a maintainer reading the log.
        /// </para>
        /// </summary>
        /// <returns>
        /// The configured archive root, or <see langword="null"/> when it cannot be resolved or
        /// when this instance was built through the navigation-only constructor.
        /// </returns>
        private string? GetArchiveRootForDisplayOrNull()
        {
            try
            {
                // Null-conditional is load-bearing: the navigation-only constructor sets _globals
                // to null!, and the pre-change display read already tolerated that.
                return _globals?.Ol.ArchiveRootPath;
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn(
                    "Cannot resolve the Outlook archive root for display. The folder list is "
                        + "rendered without archive-relative stems. Details are withheld from this "
                        + "message because they contain a mailbox address.",
                    ex
                );
                return null;
            }
        }

        /// <summary>
        /// Projects one suggestion path onto its archive-relative stem for display. Relocated here
        /// from <c>FolderPredictor.cs</c> unchanged apart from the added
        /// <paramref name="archiveRoot"/> parameter, so the callers can resolve the root ONCE
        /// above their loop rather than once per element.
        /// </summary>
        private string ProjectSuggestionPath(string folderPath, string? archiveRoot)
        {
            // Null-forgiving: ToDisplayStem returns null only for a null input, which this
            // non-nullable parameter excludes; unsuppressed the return is CS8603 (#799 AC4).
            return ArchiveStemProjection.ToDisplayStem(folderPath, archiveRoot)!;
        }
    }
}
