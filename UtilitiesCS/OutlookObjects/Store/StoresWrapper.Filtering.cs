#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Filtering surface of <see cref="StoresWrapper"/>. Holds the static
    /// <see cref="StoreIsIncluded"/> overload, relocated here (issue #328) to keep
    /// <c>StoresWrapper.cs</c> within the 500-line file-size limit after the additive
    /// StoreID-exclusion changes. Behavior-preserving relocation only.
    /// </summary>
    public partial class StoresWrapper
    {
        public static bool StoreIsIncluded(
            Outlook.Store store,
            IList<string> excludedStoreNameContains,
            IList<string> excludedStoreFilePathContains,
            IList<string> gwsoFilePathContains,
            bool excludePublicFolderStores,
            bool excludeGwsoStores,
            bool isDisabled,
            string? storeId = null,
            IReadOnlyCollection<string>? excludedStoreIds = null
        )
        {
            // why: issue #328. StoreID exclusion is the most authoritative rule and is evaluated
            // first, in lockstep with the instance ShouldIncludeStore and StoreFilterAttribution.Decide.
            // Exact-match, case-insensitive; null/whitespace StoreID or entries are ignored (fail-open).
            // The two parameters are optional so pre-existing callers that do not exclude by StoreID
            // remain a single unchanged code path.
            if (
                !string.IsNullOrWhiteSpace(storeId)
                && excludedStoreIds is not null
                && excludedStoreIds.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && string.Equals(x, storeId, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return false;
            }

            if (
                excludePublicFolderStores
                && store.ExchangeStoreType == OlExchangeStoreType.olExchangePublicFolder
            )
            {
                return false;
            }

            if (
                excludedStoreNameContains is not null
                && excludedStoreNameContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && (store.DisplayName?.IndexOf(x, StringComparison.OrdinalIgnoreCase) ?? -1)
                        >= 0
                )
            )
            {
                return false;
            }

            string? filePath = null;
            try
            {
                filePath = store.FilePath;
            }
            catch { }

            if (
                excludeGwsoStores
                && !string.IsNullOrWhiteSpace(filePath)
                && gwsoFilePathContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && filePath!.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return false;
            }

            if (
                excludedStoreFilePathContains is not null
                && !string.IsNullOrWhiteSpace(filePath)
                && excludedStoreFilePathContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && filePath!.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return false;
            }

            // why: issue #261. Checked last, after the four existing exclusion rules. The caller
            // supplies the precomputed effective-disabled result because this static overload has no
            // instance state to consult.
            if (isDisabled)
            {
                return false;
            }

            return true;
        }
    }
}
