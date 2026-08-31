using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    internal partial class EfcDataModel
    {
        /// <summary>
        /// Provides the #637 seam for the string filing overload; normalization lands in P4-T1.
        /// </summary>
        internal static string ToFilingStemOrVerbatim(string candidatePath, string archiveAncestor)
        {
            _ = ArchiveStemContract.TryMakeArchiveRelative(candidatePath, archiveAncestor, out _);
            return candidatePath;
        }
    }
}
