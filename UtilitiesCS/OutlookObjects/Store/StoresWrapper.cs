using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Store
{
    public class StoresWrapper : SmartSerializable<StoresWrapper>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region ctor

        public StoresWrapper()
            : base()
        {
            base._parent = this;
        }

        public StoresWrapper(IApplicationGlobals globals)
        {
            base._parent = this;
            Globals = globals;
        }

        public virtual StoresWrapper Init()
        {
            // why: issue #211 Phase 3.4 diagnosis-only. The synchronous Init() filter path
            // previously logged no GetFilteredStores summary (only RewireOlObjectsAsync did).
            // Materialize the filtered set once, emit one [store-filter] summary line, then
            // build Stores from that same list. Behavior-preserving: identical included set
            // and order to the prior GetFilteredStores().Select(...). Stopwatch only.
            var filteredStoresStopwatch = Stopwatch.StartNew();
            var filteredStores = GetFilteredStores().ToList();
            logger.Debug(
                $"[store-filter] GetFilteredStores completed: {filteredStores.Count} stores in {filteredStoresStopwatch.ElapsedMilliseconds} ms"
            );
            Stores = filteredStores.Select(store => new StoreWrapper(store).Init()).ToList();
            return this;
        }

        public static Task<StoresWrapper> CreateAsync(
            IApplicationGlobals globals,
            CancellationToken cancel
        )
        {
            cancel.ThrowIfCancellationRequested();
            return Task.FromResult(new StoresWrapper(globals).Init());
        }

        [OnDeserialized]
        public void RewireOlObjects(System.Runtime.Serialization.StreamingContext context)
        {
            _ = RewireAfterDeserializeWithLoggingAsync();
        }

        public virtual Task RewireAfterDeserializeAsync()
        {
            return RewireOlObjectsAsync(default);
        }

        private async Task RewireAfterDeserializeWithLoggingAsync()
        {
            try
            {
                await RewireAfterDeserializeAsync();
            }
            catch (System.Exception e)
            {
                logger.Error($"Error in {nameof(RewireOlObjects)}: {e.Message}");
            }
        }

        internal async Task RewireOlObjectsAsync(StreamingContext context)
        {
            this.Stores ??= [];
            var totalStopwatch = Stopwatch.StartNew();
            var filteredStoresStopwatch = Stopwatch.StartNew();
            var stores = GetFilteredStores().ToList();
            logger.Debug(
                $"[Startup timing] GetFilteredStores completed: {stores.Count} stores in {filteredStoresStopwatch.ElapsedMilliseconds} ms"
            );

            var processedStoreCount = 0;
            foreach (var store in stores)
            {
                if (processedStoreCount > 0)
                {
                    await Task.Yield();
                }

                var perStoreStopwatch = Stopwatch.StartNew();
                var storeDisplayName = store.DisplayName;
                var storeWrapper = Stores.Find(x => x.DisplayName == storeDisplayName);
                var wasCreated = false;

                if (storeWrapper is null)
                {
                    storeWrapper = new StoreWrapper(store).Init();
                    Stores.Add(storeWrapper);
                    wasCreated = true;
                }
                else
                {
                    storeWrapper.Restore(store);
                }

                logger.Debug(
                    $"[Startup timing] Store '{storeDisplayName}' iteration completed in {perStoreStopwatch.ElapsedMilliseconds} ms (operation={(wasCreated ? "Init" : "Restore")})"
                );

                processedStoreCount++;
            }

            logger.Debug(
                $"[Startup timing] RewireOlObjectsAsync total: {totalStopwatch.ElapsedMilliseconds} ms"
            );
        }

        private IEnumerable<Outlook.Store> GetFilteredStores()
        {
            return Globals
                .Ol.NamespaceMAPI.Stores.Cast<Outlook.Store>()
                .Where(ShouldIncludeStoreInstrumented);
        }

        // why: issue #211 Phase 3.4 diagnosis-only attribution of the previously-untimed
        // store FILTER path. Wraps each per-store COM property read (ExchangeStoreType,
        // FilePath) in its own Stopwatch and emits one [store-filter] line per enumerated
        // store via the existing log4net logger, so a slow cold-start capture can identify
        // the largest FilePath/ExchangeStoreType read time and the Gmail/GWSO store's
        // include/exclude decision. The pure decision lives in StoreFilterAttribution.Decide,
        // which mirrors ShouldIncludeStore's exact short-circuit order, so the included set
        // and enumeration order are unchanged. Stopwatch only; no banned APIs. To be removed
        // or gated after diagnosis.
        private bool ShouldIncludeStoreInstrumented(Outlook.Store store)
        {
            string displayName = null;
            try
            {
                displayName = store.DisplayName;
            }
            catch { }

            var exchangeStoreTypeStopwatch = Stopwatch.StartNew();
            bool isPublicFolder =
                store.ExchangeStoreType == OlExchangeStoreType.olExchangePublicFolder;
            exchangeStoreTypeStopwatch.Stop();

            string filePath = null;
            var filePathStopwatch = Stopwatch.StartNew();
            try
            {
                filePath = store.FilePath;
            }
            catch { }
            filePathStopwatch.Stop();

            var (included, rule) = StoreFilterAttribution.Decide(
                isPublicFolder,
                displayName,
                filePath,
                ExcludedStoreNameContains,
                ExcludedStoreFilePathContains,
                GwsoFilePathContains,
                ExcludePublicFolderStores,
                ExcludeGwsoStores
            );

            logger.Debug(
                StoreFilterAttribution.FormatLine(
                    displayName,
                    exchangeStoreTypeStopwatch.Elapsed.TotalMilliseconds,
                    filePathStopwatch.Elapsed.TotalMilliseconds,
                    included,
                    rule
                )
            );

            return included;
        }

        public static bool StoreIsIncluded(
            Outlook.Store store,
            IList<string> excludedStoreNameContains,
            IList<string> excludedStoreFilePathContains,
            IList<string> gwsoFilePathContains,
            bool excludePublicFolderStores,
            bool excludeGwsoStores
        )
        {
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

            string filePath = null;
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
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
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
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return false;
            }

            return true;
        }

        public bool ShouldIncludeStore(Outlook.Store store)
        {
            if (
                ExcludePublicFolderStores
                && store.ExchangeStoreType == OlExchangeStoreType.olExchangePublicFolder
            )
            {
                return false;
            }

            if (
                ExcludedStoreNameContains is not null
                && ExcludedStoreNameContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && (store.DisplayName?.IndexOf(x, StringComparison.OrdinalIgnoreCase) ?? -1)
                        >= 0
                )
            )
            {
                return false;
            }

            string filePath = null;
            try
            {
                filePath = store.FilePath;
            }
            catch { }

            if (
                ExcludeGwsoStores
                && !string.IsNullOrWhiteSpace(filePath)
                && GwsoFilePathContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return false;
            }

            if (
                ExcludedStoreFilePathContains is not null
                && !string.IsNullOrWhiteSpace(filePath)
                && ExcludedStoreFilePathContains.Any(x =>
                    !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0
                )
            )
            {
                return false;
            }

            return true;
        }

        #endregion ctor

        [JsonProperty]
        internal IApplicationGlobals Globals { get; set; }

        [JsonProperty]
        public List<StoreWrapper> Stores { get; set; }

        [JsonProperty]
        public bool ExcludePublicFolderStores { get; set; } = true;

        [JsonProperty]
        public bool ExcludeGwsoStores { get; set; } = true;

        [JsonProperty]
        public List<string> GwsoFilePathContains { get; set; } =
        [@"\Google\Google Apps Sync\", @"\Google\Google Workspace Sync\"];

        [JsonProperty]
        public List<string> ExcludedStoreNameContains { get; set; } = [];

        [JsonProperty]
        public List<string> ExcludedStoreFilePathContains { get; set; } = [];
    }
}
