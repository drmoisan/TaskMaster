#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Deedle.Internal;
using Microsoft.Graph.Models.TermStore;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Threading;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Store
{
    public class StoreWrapper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region ctor

        public StoreWrapper(Outlook.Store store)
        {
            InnerStore = store;
        }

        public StoreWrapper Init()
        {
            // Issue #211 Phase 3.6: measure the total wall-clock spent in Init (the failing-store
            // logon is a SHARED blocking cost) so a process-global accumulator can attribute it NET
            // of whichever startup phase timer is running. The per-COM [Startup timing] lines below
            // are unchanged; this single method-scope Stopwatch is additive.
            var initStopwatch = Stopwatch.StartNew();

            var storeDisplayNameStopwatch = Stopwatch.StartNew();
            DisplayName = InnerStore!.DisplayName;
            logger.Debug(
                $"[Startup timing] Init '{DisplayName ?? "<null>"}' DisplayName: {storeDisplayNameStopwatch.ElapsedMilliseconds} ms"
            );

            // why: issue #328. Persist the StoreID at Init so the settings controller can read it
            // without a live-COM dependency (stable after deserialize-before-rewire). Guarded so an
            // unreadable StoreID is fail-safe (leaves the default) rather than throwing during startup.
            try
            {
                StoreId = InnerStore!.StoreID;
            }
            catch (System.Exception e)
            {
                logger.Error(
                    $"Error reading StoreID for store '{DisplayName ?? "<null>"}' {e.Message}"
                );
            }

            // why: issue #264. Attribute any UI-thread lockup inside the post-DisplayName blocking
            // COM chain (GetRootFolder / GetDefaultFolder(Inbox) / the SMTP chain) to this store,
            // using the already-cached DisplayName (no new COM read). The scope wraps around the
            // existing #211 [Startup timing] lines without altering them; it is disposed before the
            // method-scope initStopwatch total is recorded.
            using (CurrentStoreContext.Begin(DisplayName))
            {
                var rootFolderStopwatch = Stopwatch.StartNew();
                RootFolder = InnerStore!.GetRootFolder() as Outlook.Folder;
                logger.Debug(
                    $"[Startup timing] Init '{DisplayName ?? "<null>"}' GetRootFolder: {rootFolderStopwatch.ElapsedMilliseconds} ms"
                );

                var exchangeStoreType = InnerStore!.ExchangeStoreType;
                if (exchangeStoreType != Outlook.OlExchangeStoreType.olExchangePublicFolder)
                {
                    var inboxStopwatch = Stopwatch.StartNew();
                    Inbox =
                        InnerStore!.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox)
                        as Outlook.Folder;
                    logger.Debug(
                        $"[Startup timing] Init '{DisplayName ?? "<null>"}' GetDefaultFolder(Inbox): {inboxStopwatch.ElapsedMilliseconds} ms"
                    );
                }

                var smtpLookupStopwatch = Stopwatch.StartNew();
                UserEmailAddress = GetSmtpAddressFromStore();
                logger.Debug(
                    $"[Startup timing] Init '{DisplayName ?? "<null>"}' GetSmtpAddressFromStore: {smtpLookupStopwatch.ElapsedMilliseconds} ms"
                );
            }

            initStopwatch.Stop();
            var initTotalMs = initStopwatch.Elapsed.TotalMilliseconds;
            StoreWrapperInitClock.Add(initTotalMs);
            new StoreWrapperInitProbe(s => logger.Debug(s)).EmitLine(
                DisplayName,
                initTotalMs,
                System.Threading.Thread.CurrentThread.ManagedThreadId
            );

            return this;
        }

        public bool TryRestore(Outlook.Store store)
        {
            try
            {
                Restore(store);
                return true;
            }
            catch (System.Exception e)
            {
                logger.Error(
                    $"Error restoring {nameof(StoreWrapper)} named {DisplayName} {e.Message}"
                );
                return false;
            }
        }

        public void Restore(Outlook.Store store)
        {
            InnerStore = store;
            Init();

            var archiveRestoreStopwatch = Stopwatch.StartNew();
            ArchiveRoot?.RestoreFromRelativePath(RootFolder);
            logger.Debug(
                $"[Startup timing] Restore '{DisplayName ?? "<null>"}' ArchiveRoot.RestoreFromRelativePath: {archiveRestoreStopwatch.ElapsedMilliseconds} ms"
            );

            var junkPotentialRestoreStopwatch = Stopwatch.StartNew();
            JunkPotential?.RestoreFromRelativePath(RootFolder);
            logger.Debug(
                $"[Startup timing] Restore '{DisplayName ?? "<null>"}' JunkPotential.RestoreFromRelativePath: {junkPotentialRestoreStopwatch.ElapsedMilliseconds} ms"
            );

            var junkCertainRestoreStopwatch = Stopwatch.StartNew();
            JunkCertain?.RestoreFromRelativePath(RootFolder);
            logger.Debug(
                $"[Startup timing] Restore '{DisplayName ?? "<null>"}' JunkCertain.RestoreFromRelativePath: {junkCertainRestoreStopwatch.ElapsedMilliseconds} ms"
            );
        }

        public void RestoreGlobalAddresses(Application olApp)
        {
            GlobalAddressBook = InnerStore
                ?.GetGlobalAddressList(olApp)
                ?.AddressEntries?.Cast<AddressEntry>()
                ?.ToList();
        }

        #endregion ctor

        #region Store Properties

        public string? DisplayName { get; set; }

        /// <summary>
        /// The store's Outlook StoreID, captured during <see cref="Init"/> (issue #328). Persisted so
        /// the settings UI can match this store against <c>StoresWrapper.ExcludedStoreIds</c> without a
        /// live-COM read. Additive to serialization and backward-compatible: a legacy payload without
        /// this key deserializes to the default.
        /// </summary>
        [JsonProperty]
        public string? StoreId { get; set; }

        [JsonIgnore]
        public Outlook.Store? InnerStore { get; internal set; }

        [JsonIgnore]
        public Outlook.Folder? Inbox { get; internal set; }

        [JsonIgnore]
        public Outlook.Folder? RootFolder { get; internal set; }

        [JsonIgnore]
        public string? UserEmailAddress { get; internal set; }

        [JsonIgnore]
        public List<AddressEntry>? GlobalAddressBook { get; internal set; }

        internal string? GetSmtpAddressFromStore()
        {
            try
            {
                var currentUserStopwatch = Stopwatch.StartNew();
                var currentUser = RootFolder?.Session?.CurrentUser;
                logger.Debug(
                    $"[Startup timing] GetSmtpAddressFromStore '{DisplayName ?? "<null>"}' CurrentUser: {currentUserStopwatch.ElapsedMilliseconds} ms"
                );

                var addressEntryStopwatch = Stopwatch.StartNew();
                var addressEntry = currentUser?.AddressEntry;
                logger.Debug(
                    $"[Startup timing] GetSmtpAddressFromStore '{DisplayName ?? "<null>"}' AddressEntry: {addressEntryStopwatch.ElapsedMilliseconds} ms"
                );

                var exchangeUserStopwatch = Stopwatch.StartNew();
                var exchangeUser = addressEntry?.GetExchangeUser();
                logger.Debug(
                    $"[Startup timing] GetSmtpAddressFromStore '{DisplayName ?? "<null>"}' GetExchangeUser: {exchangeUserStopwatch.ElapsedMilliseconds} ms"
                );

                var primarySmtpAddressStopwatch = Stopwatch.StartNew();
                var primarySmtpAddress = exchangeUser?.PrimarySmtpAddress;
                logger.Debug(
                    $"[Startup timing] GetSmtpAddressFromStore '{DisplayName ?? "<null>"}' PrimarySmtpAddress: {primarySmtpAddressStopwatch.ElapsedMilliseconds} ms (result={primarySmtpAddress ?? "<null>"})"
                );

                return primarySmtpAddress;
            }
            catch (COMException e)
            {
                logger.Error(
                    $"Error retrieving PrimarySmtpAddress from secondary inbox. {e.Message}",
                    e
                );
                return null;
            }
        }

        #endregion Store Properties

        #region Configurable Properties

        public FolderMinimalWrapper? ArchiveRoot { get; set; } = new();

        public FilePathHelper? ArchiveFsRoot { get; set; } = new();

        public FolderMinimalWrapper? JunkPotential { get; set; } = new();

        public FolderMinimalWrapper? JunkCertain { get; set; } = new();

        #endregion Configurable Properties
    }
}
