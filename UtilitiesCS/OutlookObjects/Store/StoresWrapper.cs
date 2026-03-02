using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook; 
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;
using System.Runtime.Serialization;
using System.Threading;
using System;

namespace UtilitiesCS.OutlookObjects.Store
{
    public class StoresWrapper: SmartSerializable<StoresWrapper>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public StoresWrapper(): base() { base._parent = this; }

        public StoresWrapper(IApplicationGlobals globals)
        {
            base._parent = this;
            Globals = globals;
        }

        public virtual StoresWrapper Init()
        {
            Stores = GetFilteredStores()
                .Select(store => new StoreWrapper(store).Init())
                .ToList();            
            return this;
        }

        public static async Task<StoresWrapper> CreateAsync(IApplicationGlobals globals, CancellationToken cancel)
        {
            return await Task.Run(() => new StoresWrapper(globals).Init(), cancel);
        }

        [OnDeserialized]
        public async void RewireOlObjects(System.Runtime.Serialization.StreamingContext context)
        {
            try
            {
                await RewireOlObjectsAsync(context);
            }
            catch (System.Exception e)
            {
                logger.Error($"Error in {nameof(RewireOlObjects)}: {e.Message}");                
            }
        }

        internal async Task RewireOlObjectsAsync(StreamingContext context)
        {
            this.Stores ??= [];
            var stores = GetFilteredStores();

            foreach (var store in stores)
            {
                
                var storeWrapper = Stores.Find(x => x.DisplayName == store.DisplayName);
                if (storeWrapper is null)
                {
                    storeWrapper = await Task.Run(() => new StoreWrapper(store).Init());
                    Stores.Add(storeWrapper);
                }
                else
                {
                    await Task.Run(() => storeWrapper.Restore(store));
                    //await Task.Run(() => storeWrapper.RestoreGlobalAddresses(Globals.Ol.App));
                    
                }                                
            }
        }

        private IEnumerable<Outlook.Store> GetFilteredStores()
        {
            return Globals.Ol.NamespaceMAPI.Stores
                .Cast<Outlook.Store>()
                .Where(ShouldIncludeStore);
        }


        public static bool StoreIsIncluded(
            Outlook.Store store, 
            IList<string> excludedStoreNameContains,
            IList<string> excludedStoreFilePathContains,
            IList<string> gwsoFilePathContains, 
            bool excludePublicFolderStores, 
            bool excludeGwsoStores)
        {
            if (excludePublicFolderStores && store.ExchangeStoreType == OlExchangeStoreType.olExchangePublicFolder)
            {
                return false;
            }

            if (excludedStoreNameContains is not null
                && excludedStoreNameContains.Any(x => !string.IsNullOrWhiteSpace(x)
                    && (store.DisplayName?.IndexOf(x, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0))
            {
                return false;
            }

            string filePath = null;
            try
            {
                filePath = store.FilePath;
            }
            catch
            {
            }

            if (excludeGwsoStores
                && !string.IsNullOrWhiteSpace(filePath)
                && gwsoFilePathContains.Any(x => !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return false;
            }

            if (excludedStoreFilePathContains is not null
                && !string.IsNullOrWhiteSpace(filePath)
                && excludedStoreFilePathContains.Any(x => !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return false;
            }

            return true;
        }
        public bool ShouldIncludeStore(Outlook.Store store)
        {
            if (ExcludePublicFolderStores && store.ExchangeStoreType == OlExchangeStoreType.olExchangePublicFolder)
            {
                return false;
            }

            if (ExcludedStoreNameContains is not null
                && ExcludedStoreNameContains.Any(x => !string.IsNullOrWhiteSpace(x)
                    && (store.DisplayName?.IndexOf(x, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0))
            {
                return false;
            }

            string filePath = null;
            try
            {
                filePath = store.FilePath;
            }
            catch
            {
            }

            if (ExcludeGwsoStores
                && !string.IsNullOrWhiteSpace(filePath)
                && GwsoFilePathContains.Any(x => !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return false;
            }

            if (ExcludedStoreFilePathContains is not null
                && !string.IsNullOrWhiteSpace(filePath)
                && ExcludedStoreFilePathContains.Any(x => !string.IsNullOrWhiteSpace(x)
                    && filePath.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
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
        [
            @"\Google\Google Apps Sync\",
            @"\Google\Google Workspace Sync\"
        ];

        [JsonProperty]
        public List<string> ExcludedStoreNameContains { get; set; } = [];

        [JsonProperty]
        public List<string> ExcludedStoreFilePathContains { get; set; } = [];

    }
}
