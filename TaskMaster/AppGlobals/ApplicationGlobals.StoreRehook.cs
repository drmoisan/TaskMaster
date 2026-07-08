using System.Diagnostics.CodeAnalysis;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    /// <summary>
    /// Partial of <see cref="ApplicationGlobals"/> holding the composition root for the F3 runtime
    /// store-rehook coordinator (issue #263, epic #260). Split into its own file to keep
    /// <c>ApplicationGlobals.cs</c> within the 500-line ceiling. The coordinator is constructed in
    /// <c>LoadBasicMethod()</c> and injected as F1's <c>IStoreRehookService</c> collaborator into
    /// <c>StoreDisableService</c>; its expensive sink/tree-service dependencies are supplied as lazy
    /// accessors so no eager COM read occurs at startup.
    /// </summary>
    /// <remarks>
    /// The members here are the VSTO/COM-bound composition root: they read the live
    /// <c>NamespaceMAPI.Stores</c> collection and default-inbox folders directly (no injectable seam
    /// below the COM boundary) and wire the coordinator's collaborators. They are therefore
    /// COM/VSTO coverage-exempt by inspection per the repository's documented COM/VSTO exemption
    /// (CLAUDE.md). The coordinator's decision logic (which these delegates feed) is unit-tested
    /// separately in <c>StoreRehookCoordinatorTests</c> with fully mocked seams.
    /// </remarks>
    public partial class ApplicationGlobals
    {
        // Held for reachability/symmetry with _storeDisableService; the coordinator is reached by F1
        // via constructor injection, not through the IApplicationGlobals aggregate (no StoreRehook
        // accessor is added to IApplicationGlobals).
        private StoreRehookCoordinator _storeRehookCoordinator;

        /// <summary>
        /// Builds the real <see cref="StoreRehookCoordinator"/> from the concrete collaborators.
        /// Cheap: constructs only the readiness gate and captures lazy delegates/accessors; the
        /// expensive notification-sink and folder-tree-service dependencies are resolved only when
        /// the coordinator runs a rehook (at reenable time), never here at startup.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private StoreRehookCoordinator BuildStoreRehookCoordinator()
        {
            var readinessGate = new OutlookReadinessGate(_outlookApp);
            return new StoreRehookCoordinator(
                readinessGate,
                ResolveLiveStore,
                storeId =>
                    _events.IsInboxHooked(storeId)
                    && _olObjects.FolderNotificationSink.IsStoreHooked(storeId),
                store => _olObjects.StoresWrapper?.AddOrRestoreStore(store),
                SubscribeStoreInbox,
                () => _olObjects.FolderNotificationSink,
                () => _olObjects.FolderTreeService
            );
        }

        /// <summary>
        /// Resolves a store identity (F1's DisplayName-primary key) to a live
        /// <see cref="Outlook.Store"/> by enumerating the MAPI namespace stores, or null when none
        /// matches. Reads only DisplayName (guarded) per store; performs no expensive folder access.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private Outlook.Store ResolveLiveStore(string identity)
        {
            var stores = _olObjects?.NamespaceMAPI?.Stores;
            if (stores == null)
            {
                return null;
            }

            foreach (Outlook.Store store in stores)
            {
                string displayName;
                try
                {
                    displayName = store.DisplayName;
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    continue;
                }

                if (string.Equals(displayName, identity, System.StringComparison.OrdinalIgnoreCase))
                {
                    return store;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the given store's default inbox and subscribes its item-add handler through the
        /// idempotent AppEvents primitive. Invoked by the coordinator only after the store-scoped
        /// readiness gate reports ready.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void SubscribeStoreInbox(Outlook.Store store)
        {
            var inbox =
                store.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox) as Outlook.Folder;
            if (inbox != null)
            {
                _events.SubscribeInboxForStore(store, inbox);
            }
        }
    }
}
