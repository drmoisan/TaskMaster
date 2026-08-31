using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    internal partial class EfcDataModel
    {
        /// <summary>
        /// Returns its input unchanged unless the input is a full Outlook path under the archive ancestor
        /// supplied by the caller. Returns a value for every input and propagates no exception.
        /// </summary>
        internal static string ToFilingStemOrVerbatim(string candidatePath, string archiveAncestor)
        {
            if (
                ArchiveStemContract.IsFullOutlookPath(candidatePath)
                && ArchiveStemContract.TryMakeArchiveRelative(
                    candidatePath,
                    archiveAncestor,
                    out string stem
                )
                && stem.Length != 0
            )
            {
                return stem;
            }

            return candidatePath;
        }
    }
}
