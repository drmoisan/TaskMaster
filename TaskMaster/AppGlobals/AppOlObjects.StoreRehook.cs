using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;

namespace TaskMaster
{
    /// <summary>
    /// Partial of <see cref="AppOlObjects"/> holding the per-store inbox-resolution primitive
    /// (issue #263, epic #260). Extracted from <see cref="AppOlObjects.LoadInboxes"/>'s per-store
    /// loop body so both the startup bulk load and the runtime rehook path resolve one store's
    /// default inbox through one implementation, following the delegate-injection style of
    /// <see cref="AppOlObjects.EmitPerStoreInboxAttribution"/>. Built on F2's
    /// <c>AppOlObjects.StoreLoading.cs</c> split so <c>AppOlObjects.cs</c> stays within the
    /// file-size ceiling.
    /// </summary>
    public partial class AppOlObjects
    {
        // The live notification sink owned by FolderTreeService, created in LoadFolderTreeService.
        // Held so the runtime rehook coordinator can reach the SAME instance to call AddStore and
        // query IsStoreHooked (issue #263). Concrete type so the coordinator's idempotency predicate
        // can call the sink's StoreID membership query. Null until FolderTreeService is first
        // initialized.
        private OutlookFolderNotificationSink _folderNotificationSink;

        /// <summary>
        /// The live folder/store notification sink used by <see cref="FolderTreeService"/>. Accessing
        /// this ensures <see cref="FolderTreeService"/> is initialized (which creates and starts the
        /// sink), then returns that same instance so the runtime rehook coordinator's
        /// <c>AddStore</c> registers subscriptions on the sink the tree service is listening to.
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal OutlookFolderNotificationSink FolderNotificationSink
        {
            get
            {
                // Force lazy initialization of FolderTreeService, which populates _folderNotificationSink.
                _ = FolderTreeService;
                return _folderNotificationSink;
            }
        }

        /// <summary>
        /// Resolves one store's default inbox folder to add to <see cref="Inboxes"/>, applying the
        /// existing store-inclusion filter and transient-HRESULT policy: a transient "store not
        /// ready" COMException is rethrown so the readiness coordinator/gate routes it to retry,
        /// while a genuinely permanent error is logged and the store is skipped (returns null). An
        /// excluded store also returns null. This is the single per-store inbox-resolution
        /// implementation reused by <see cref="AppOlObjects.LoadInboxes"/> and the runtime rehook
        /// path.
        /// </summary>
        /// <param name="store">The store to resolve an inbox for.</param>
        /// <param name="storesWrapper">The store model used to evaluate store inclusion.</param>
        /// <param name="attributionProbe">The diagnosis-only per-store attribution probe.</param>
        /// <returns>The default-inbox <see cref="Folder"/> to add, or null when excluded or skipped.</returns>
        /// <remarks>
        /// COM/VSTO coverage-exempt by inspection: every delegate it invokes crosses the live
        /// Outlook COM boundary (<c>StoresWrapper.ShouldIncludeStore</c>, <c>Store.GetDefaultFolder</c>,
        /// <c>Store.DisplayName</c>) with no seam below COM, mirroring the pre-existing
        /// <c>LoadInboxes</c> body it was extracted from. The pure per-store attribution logic is
        /// unit-tested separately via <see cref="AppOlObjects.EmitPerStoreInboxAttribution"/>.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal Folder ResolveInboxForStore(
            Store store,
            StoresWrapper storesWrapper,
            StartupInboxAttributionProbe attributionProbe
        )
        {
            try
            {
                var inbox = EmitPerStoreInboxAttribution(
                    () => storesWrapper.ShouldIncludeStore(store),
                    () => store.GetDefaultFolder(OlDefaultFolders.olFolderInbox),
                    () =>
                    {
                        try
                        {
                            return store.DisplayName;
                        }
                        catch (COMException)
                        {
                            return "<unavailable>";
                        }
                    },
                    attributionProbe
                );

                return inbox is null ? null : (Folder)inbox;
            }
            catch (COMException e)
            {
                // Issue #207: a transient "store not ready" HRESULT during cold start must NOT
                // silently drop this store's inbox. Rethrow so the readiness coordinator/gate routes
                // it to retry; only genuinely permanent errors are logged and skipped. The transient
                // HRESULTs are shared as public constants on OutlookReadinessGate.
                uint hresult = unchecked((uint)e.ErrorCode);
                if (
                    hresult == OutlookReadinessGate.TransientStoreNotReadyHResult
                    || hresult == OutlookReadinessGate.TransientOperationFailedHResult
                )
                {
                    throw;
                }

                logger.Error($"Error loading inbox from store. {e.Message}", e);
                return null;
            }
        }
    }
}
