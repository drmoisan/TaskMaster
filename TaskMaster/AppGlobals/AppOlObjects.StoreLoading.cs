using System;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster
{
    /// <summary>
    /// Store-loading partial of <see cref="AppOlObjects"/> (Issue #262, AC6 file-size relief). The
    /// store-load pipeline — the startup <see cref="LoadAsync"/> entry point, the
    /// <see cref="StoresWrapper"/> model property, the deserialize-rewire seam
    /// <see cref="AwaitStoreRewireAsync"/>, the fresh-build seam
    /// <see cref="BuildFreshStoresWrapper"/>, and <see cref="LoadStoresAsync"/> — was extracted from
    /// <c>AppOlObjects.cs</c> to bring that file under the 500-line cap. Follows the documented
    /// <c>AppOlObjects.JunkFolders.cs</c> precedent (Issue #207, AC8 file-size relief).
    /// </summary>
    public partial class AppOlObjects
    {
        public async Task LoadAsync()
        {
            await LoadStoresAsync();
            await Task.CompletedTask;
        }

        public StoresWrapper StoresWrapper { get; set; }

        protected internal virtual Task AwaitStoreRewireAsync(StoresWrapper storesWrapper) =>
            storesWrapper is null
                ? Task.CompletedTask
                : storesWrapper.RewireAfterDeserializeAsync();

        protected internal virtual StoresWrapper BuildFreshStoresWrapper() =>
            new StoresWrapper(_globals).Init();

        internal async Task LoadStoresAsync()
        {
            try
            {
                if (_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config))
                {
                    var deserialized = SmartSerializable.Deserialize<
                        StoresWrapper,
                        SmartSerializableLoader
                    >(config);
                    if (deserialized is not null)
                    {
                        StoresWrapper = deserialized;
                        await AwaitStoreRewireAsync(StoresWrapper);
                        return;
                    }
                    logger.Warn(
                        "StoresWrapper config deserialized to null; rebuilding from live stores."
                    );
                }
                else
                {
                    logger.Warn("StoresWrapper config not found; rebuilding from live stores.");
                }

                // Fresh build has no persisted disabled-store state to restore. F1 (#261)
                // interaction: once the persisted config is confirmed missing or null, there is no
                // source from which a previously disabled-for-future-sessions store could be
                // recovered, so a store re-enabled here is expected, not a regression in F1/F5.
                StoresWrapper = BuildFreshStoresWrapper();
            }
            catch (Exception e)
            {
                logger.Error(
                    $"Failed to load StoresWrapper; store settings will remain unavailable until this is resolved. {e.Message}",
                    e
                );
            }
        }
    }
}
