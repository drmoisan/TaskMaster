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
using UtilitiesCS.Threading;
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

                AddOrRestoreStore(store);

                processedStoreCount++;
            }

            logger.Debug(
                $"[Startup timing] RewireOlObjectsAsync total: {totalStopwatch.ElapsedMilliseconds} ms"
            );
        }

        /// <summary>
        /// Adds a new <see cref="StoreWrapper"/> for <paramref name="store"/> or restores an
        /// existing one, keyed by DisplayName. This is the single per-store hookup implementation
        /// for <see cref="Stores"/>, reused by the bulk <see cref="RewireOlObjectsAsync"/> loop and
        /// by the runtime rehook coordinator (issue #263, epic #260). Idempotency is implicit in
        /// the DisplayName lookup: a found wrapper is <see cref="StoreWrapper.Restore"/>d, not
        /// duplicated, so no new guard is needed here.
        /// </summary>
        /// <remarks>
        /// The expensive COM reads inside <see cref="StoreWrapper.Init"/>
        /// (<c>GetRootFolder</c>/<c>GetDefaultFolder</c>/the SMTP chain) implicated in the epic's
        /// lockup scenario run inside this method; the rehook coordinator therefore calls it only
        /// after its store-scoped readiness gate reports ready, never eagerly.
        /// </remarks>
        /// <param name="store">The live store to add or restore. Must not be null.</param>
        /// <returns>The added or restored <see cref="StoreWrapper"/>.</returns>
        /// <remarks>
        /// Public because the runtime rehook coordinator lives in the <c>TaskMaster</c> assembly and
        /// <c>UtilitiesCS</c> does not grant it <c>InternalsVisibleTo</c>; this method is the shared
        /// per-store primitive both the bulk loop and the coordinator invoke.
        /// </remarks>
        public StoreWrapper AddOrRestoreStore(Outlook.Store store)
        {
            Stores ??= [];

            var perStoreStopwatch = Stopwatch.StartNew();
            var storeDisplayName = store.DisplayName;
            var storeWrapper = Stores.Find(x => x.DisplayName == storeDisplayName);
            var wasCreated = false;

            // why: issue #264. Attribute any UI-thread lockup inside the per-store Init/Restore COM
            // work to this store, using the already-read storeDisplayName (no new COM read). The
            // scope opens and closes entirely within this synchronous method, which the bulk loop
            // enters only after its await Task.Yield(), so no ambient value leaks across the yield.
            using (CurrentStoreContext.Begin(storeDisplayName))
            {
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
            }

            logger.Debug(
                $"[Startup timing] Store '{storeDisplayName}' iteration completed in {perStoreStopwatch.ElapsedMilliseconds} ms (operation={(wasCreated ? "Init" : "Restore")})"
            );

            return storeWrapper;
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

            // why: issue #261. Resolve the identity from the DisplayName/FilePath already read above
            // (no additional COM cost) and test it against the effective disabled set. Passed to
            // Decide as the trailing argument so the Disabled reason is attributed last, after the
            // four existing exclusion checks.
            bool isDisabled = IsEffectivelyDisabled(StoreIdentity.Resolve(displayName, filePath));

            var (included, rule) = StoreFilterAttribution.Decide(
                isPublicFolder,
                displayName,
                filePath,
                ExcludedStoreNameContains,
                ExcludedStoreFilePathContains,
                GwsoFilePathContains,
                ExcludePublicFolderStores,
                ExcludeGwsoStores,
                isDisabled
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
            bool excludeGwsoStores,
            bool isDisabled
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

            // why: issue #261. Checked last, after the four existing exclusion rules. The caller
            // supplies the precomputed effective-disabled result because this static overload has no
            // instance state to consult.
            if (isDisabled)
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

            // why: issue #261. Checked last, after the four existing exclusion rules. Resolves the
            // identity from the DisplayName and the FilePath already read above (no extra COM read of
            // the blocking FilePath property) and excludes the store when it is effectively disabled.
            if (IsEffectivelyDisabled(StoreIdentity.Resolve(store.DisplayName, filePath)))
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

        /// <summary>
        /// Identities of stores disabled for the current and all future sessions (issue #261). Keyed
        /// by resolved <see cref="StoreIdentity.Value"/>, compared case-insensitively by
        /// <see cref="IsEffectivelyDisabled"/>. Persisted (round-trips through the existing
        /// "StoresWrapper" serialization key); no new file or config key is added.
        /// </summary>
        [JsonProperty]
        public List<string> DisabledStoreIdentities { get; set; } = [];

        /// <summary>
        /// Identities of stores disabled for the current session only (issue #261). Not persisted:
        /// Newtonsoft invokes the parameterless constructor before populating properties, so this
        /// field re-initializes to an empty, case-insensitive set on every deserialize and is absent
        /// from emitted JSON.
        /// </summary>
        [JsonIgnore]
        public HashSet<string> SessionDisabledStoreIdentities { get; set; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Single source of truth for the effective-disabled test used by the filter surfaces and the
        /// store-disable service. Returns true only when the identity resolves to a real (non-sentinel,
        /// non-null/whitespace) value that is present, case-insensitively, in the union of the session
        /// and persisted disabled sets. Performs no COM access.
        /// </summary>
        /// <param name="identity">The resolved store identity to test.</param>
        /// <returns>True when the identity is effectively disabled in either scope; otherwise false.</returns>
        internal bool IsEffectivelyDisabled(StoreIdentity identity)
        {
            var value = identity.Value;
            if (
                string.IsNullOrWhiteSpace(value)
                || string.Equals(value, StoreIdentity.UnresolvedSentinel, StringComparison.Ordinal)
            )
            {
                return false;
            }

            if (
                SessionDisabledStoreIdentities is not null
                && SessionDisabledStoreIdentities.Contains(value)
            )
            {
                return true;
            }

            return DisabledStoreIdentities is not null
                && DisabledStoreIdentities.Any(x =>
                    string.Equals(x, value, StringComparison.OrdinalIgnoreCase)
                );
        }
    }
}
