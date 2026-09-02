#nullable enable
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// The predicates deciding what an EFC selection may be used for (#614 D9). The OK/filing
    /// action and the folder-creation path each delegate to the predicate scoped to their own
    /// rule set, so neither path can silently inherit the other's strictness, and a value can
    /// never be accepted by one route into the same operation and rejected by another.
    /// </summary>
    internal static class EfcSelectionGuard
    {
        /// <summary>
        /// Prefix a selection must not begin with for either predicate to accept it.
        /// <para>
        /// This value is deliberately a PROPER PREFIX of
        /// <see cref="BreadcrumbRowBuilder.BannerPrefix"/>, the four-character prefix both row
        /// producers emit. It is therefore not a copy of the producers' constant and must not be
        /// kept in step with it.
        /// </para>
        /// <para>
        /// Because every row beginning with the producers' four-character prefix also begins with
        /// this three-character one, the guard rejects a strict superset of the producers' banner
        /// rows: every row a producer emits, plus a three-equals row that no producer emits today.
        /// </para>
        /// <para>
        /// It must not be widened to the producers' four-character value. That edit reads like a
        /// consistency fix and is a behavioural relaxation: this prefix is the only mechanism
        /// rejecting a three-equals row at either EFC classification site, because
        /// <see cref="MinimumCreationLength"/> is 3 and so the length rule accepts that input.
        /// Widening it would make <see cref="IsValidFilingSelection"/> and
        /// <see cref="IsValidCreationSelection"/> both return true for a three-equals row. The
        /// test BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates in
        /// QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs guards against that edit.
        /// </para>
        /// </summary>
        private const string BannerRejectionPrefix = "===";

        /// <summary>
        /// Shortest name the folder-creation path accepts. This is a creation rule only: it
        /// guards against a new archive folder being created under an accidental one- or
        /// two-keystroke name, and it has never applied to filing into an existing folder.
        /// </summary>
        private const int MinimumCreationLength = 3;

        /// <summary>
        /// Reports whether <paramref name="selection"/> is a usable filing destination.
        /// <para>
        /// Rejects null, empty, and whitespace-only values; banner sentinels; and every full
        /// (rooted) path. Rooted values are rejected as such at this surface so the predicate
        /// agrees with <see cref="ArchiveStemContract.RequireArchiveRelativeStem"/>. Producer-side
        /// normalization in BreadcrumbBridgeRouter.SelectRow is implemented by issue #637.
        /// </para>
        /// <para>
        /// The filing predicate carries NO minimum-length rule (CR-1). Filing into an existing
        /// archive folder whose name is one or two characters long is ordinary, and the length
        /// rule lives only in <see cref="IsValidCreationSelection"/>, where a new folder is being
        /// named rather than an existing one selected.
        /// </para>
        /// </summary>
        /// <param name="selection">The candidate selection, typically a folder row's text.</param>
        /// <returns>True when the value may be filed to; otherwise false.</returns>
        internal static bool IsValidFilingSelection(string? selection)
        {
            if (string.IsNullOrWhiteSpace(selection))
            {
                return false;
            }

            string value = selection!;
            return !value.StartsWith(BannerRejectionPrefix, System.StringComparison.Ordinal)
                && !ArchiveStemContract.IsFullOutlookPath(value);
        }

        /// <summary>
        /// Reports whether <paramref name="selection"/> may be used as the parent selection for
        /// creating a new archive folder.
        /// <para>
        /// Rejects null, empty, and whitespace-only values; banner sentinels; names shorter than
        /// the creation minimum; and any full (rooted) path, because the creation path
        /// concatenates the selection beneath the archive root and a rooted value is therefore
        /// never a valid creation stem. The minimum-length rule lives here and only here (CR-1);
        /// applying it to filing would make an existing two-character archive folder unfilable.
        /// </para>
        /// </summary>
        /// <param name="selection">The candidate selection, typically a folder row's text.</param>
        /// <returns>True when the value may be used for folder creation; otherwise false.</returns>
        internal static bool IsValidCreationSelection(string? selection)
        {
            if (string.IsNullOrWhiteSpace(selection))
            {
                return false;
            }

            string value = selection!;
            return value.Length >= MinimumCreationLength
                && !value.StartsWith(BannerRejectionPrefix, System.StringComparison.Ordinal)
                && !ArchiveStemContract.IsFullOutlookPath(value);
        }
    }
}
