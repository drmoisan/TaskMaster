#nullable enable
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// The single predicate deciding whether an EFC selection may be used as a filing
    /// destination (#614 D9). Both the OK action and the keyboard/validation path delegate here
    /// so a value can never be accepted by one and rejected by the other.
    /// </summary>
    internal static class EfcSelectionGuard
    {
        /// <summary>Prefix of the non-selectable suggestion banner rows.</summary>
        private const string BannerPrefix = "===";

        /// <summary>
        /// Reports whether <paramref name="selection"/> is a usable filing destination. Rejects
        /// null, empty, and whitespace-only values; banner sentinels; values shorter than three
        /// characters (the strictness the previous validation path already enforced); and any
        /// full (rooted) Outlook or filesystem path, which is never an archive-relative stem and
        /// is the leak this guard exists to stop.
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
            return value.Length >= 3
                && !value.StartsWith(BannerPrefix, System.StringComparison.Ordinal)
                && !ArchiveStemContract.IsFullOutlookPath(value);
        }
    }
}
