using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook; 
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;
using System.Runtime.Serialization;
using System.Threading;

namespace UtilitiesCS.OutlookObjects.Store
{
    public class StoresWrapper: SmartSerializable<StoresWrapper>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public StoresWrapper() { }

        public StoresWrapper(IApplicationGlobals globals)
        {
            Globals = globals;
        }

        public virtual StoresWrapper Init()
        {
            Stores = Globals.Ol.NamespaceMAPI.Stores
                .Cast<Outlook.Store>()
                .Where(store => store.ExchangeStoreType != OlExchangeStoreType.olExchangePublicFolder)
                .Select(store => new StoreWrapper(store).Init())
                .ToList();            
            return this;
        }

        public static async Task<StoresWrapper> CreateAsync(IApplicationGlobals globals, CancellationToken cancel)
        {
            return await Task.Run(() => new StoresWrapper(globals).Init(), cancel);
        }

        [OnDeserialized]
        public void RewireOlObjects(System.Runtime.Serialization.StreamingContext context)
        {
            this.Stores ??= [];
            var stores = Globals.Ol.NamespaceMAPI.Stores
                .Cast<Outlook.Store>()
                .Where(store => store.ExchangeStoreType != OlExchangeStoreType.olExchangePublicFolder);
            
            foreach (var store in stores)
            {
                var storeWrapper = Stores.Find(x => x.DisplayName == store.DisplayName);
                if (storeWrapper is null)
                {
                    storeWrapper = new StoreWrapper(store).Init();
                    Stores.Add(storeWrapper);
                }
                else
                {
                    storeWrapper.Restore(store);
                }
            }
        }

        #endregion ctor

        [JsonProperty]
        internal IApplicationGlobals Globals { get; set; }

        [JsonProperty]
        public List<StoreWrapper> Stores { get; set; } 

    }
}
