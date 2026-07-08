using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Identifies which rule in the store-inclusion predicate produced an exclusion
    /// (or that the store was included). Members are ordered to mirror the short-circuit
    /// evaluation order of <see cref="StoresWrapper.ShouldIncludeStore"/>, with
    /// <see cref="Included"/> last (no exclusion matched).
    /// </summary>
    public enum StoreFilterRule
    {
        /// <summary>Excluded because it is an Exchange public-folder store and public folders are excluded.</summary>
        PublicFolder,

        /// <summary>Excluded because the DisplayName contained a configured excluded-name token.</summary>
        NameContains,

        /// <summary>Excluded because the FilePath contained a configured GWSO/Gmail-sync token.</summary>
        GwsoFilePath,

        /// <summary>Excluded because the FilePath contained a configured excluded-path token.</summary>
        FilePathContains,

        /// <summary>Excluded because the store is in a disabled scope.</summary>
        Disabled,

        /// <summary>No exclusion rule matched; the store is included.</summary>
        Included,
    }

    /// <summary>
    /// Pure, COM-free attribution helpers for the store-filter diagnosis probe (issue #211, Phase 3.4).
    /// Holds the include/exclude DECISION over already-read primitive property values and the
    /// single-line <c>[store-filter]</c> log formatter. This type performs no COM access, no timing,
    /// and no clock reads, so it is unit-testable without a live Outlook host and is intentionally
    /// NOT marked <c>[ExcludeFromCodeCoverage]</c>.
    /// </summary>
    public static class StoreFilterAttribution
    {
        /// <summary>
        /// Mirrors the exact short-circuit evaluation of <see cref="StoresWrapper.ShouldIncludeStore"/>
        /// over already-read primitive property values. Returns the include decision and the rule that
        /// produced the first matching exclusion (or <see cref="StoreFilterRule.Included"/>).
        /// </summary>
        /// <param name="isPublicFolder">Whether the store's ExchangeStoreType is olExchangePublicFolder.</param>
        /// <param name="displayName">The already-read store DisplayName (may be null/empty).</param>
        /// <param name="filePath">The already-read store FilePath (may be null/empty if unavailable).</param>
        /// <param name="excludedStoreNameContains">Configured DisplayName exclusion tokens (may be null).</param>
        /// <param name="excludedStoreFilePathContains">Configured FilePath exclusion tokens (may be null).</param>
        /// <param name="gwsoFilePathContains">Configured GWSO/Gmail-sync FilePath tokens.</param>
        /// <param name="excludePublicFolderStores">Whether public-folder stores are excluded.</param>
        /// <param name="excludeGwsoStores">Whether GWSO/Gmail-sync stores are excluded.</param>
        /// <param name="isDisabled">Whether the store is in a disabled scope (issue #261). Checked last, after the four existing exclusion rules and immediately before the included result.</param>
        /// <returns>A tuple of the include decision and the matched rule.</returns>
        public static (bool Included, StoreFilterRule Rule) Decide(
            bool isPublicFolder,
            string displayName,
            string filePath,
            IList<string> excludedStoreNameContains,
            IList<string> excludedStoreFilePathContains,
            IList<string> gwsoFilePathContains,
            bool excludePublicFolderStores,
            bool excludeGwsoStores,
            bool isDisabled
        )
        {
            if (excludePublicFolderStores && isPublicFolder)
            {
                return (false, StoreFilterRule.PublicFolder);
            }

            if (
                excludedStoreNameContains is not null
                && excludedStoreNameContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && (displayName?.IndexOf(x, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                )
            )
            {
                return (false, StoreFilterRule.NameContains);
            }

            if (
                excludeGwsoStores
                && !string.IsNullOrWhiteSpace(filePath)
                && gwsoFilePathContains is not null
                && gwsoFilePathContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return (false, StoreFilterRule.GwsoFilePath);
            }

            if (
                excludedStoreFilePathContains is not null
                && !string.IsNullOrWhiteSpace(filePath)
                && excludedStoreFilePathContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return (false, StoreFilterRule.FilePathContains);
            }

            if (isDisabled)
            {
                return (false, StoreFilterRule.Disabled);
            }

            return (true, StoreFilterRule.Included);
        }

        /// <summary>
        /// Formats exactly one structured <c>[store-filter]</c> diagnostic line capturing the store
        /// DisplayName, the Stopwatch ms to read ExchangeStoreType and FilePath, the include decision,
        /// and the matched rule. The two ms fields are formatted with "F1" and
        /// <see cref="CultureInfo.InvariantCulture"/>; a null/empty DisplayName renders as <c>&lt;null&gt;</c>.
        /// </summary>
        /// <param name="displayName">The store DisplayName (rendered as &lt;null&gt; when null/empty).</param>
        /// <param name="exchangeStoreTypeMs">Stopwatch milliseconds to read ExchangeStoreType.</param>
        /// <param name="filePathMs">Stopwatch milliseconds to read FilePath.</param>
        /// <param name="included">The final include/exclude decision.</param>
        /// <param name="rule">The rule that produced the decision.</param>
        /// <returns>A single-line, log-ready string.</returns>
        public static string FormatLine(
            string displayName,
            double exchangeStoreTypeMs,
            double filePathMs,
            bool included,
            StoreFilterRule rule
        )
        {
            var name = string.IsNullOrEmpty(displayName) ? "<null>" : displayName;
            return string.Format(
                CultureInfo.InvariantCulture,
                "[store-filter] displayName={0} exchangeStoreTypeMs={1:F1} filePathMs={2:F1} included={3} rule={4}",
                name,
                exchangeStoreTypeMs,
                filePathMs,
                included ? "true" : "false",
                rule
            );
        }
    }
}
