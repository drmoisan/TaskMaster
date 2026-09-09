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

        /// <summary>
        /// The reason the most recent SMTP lookup failed, or null when the last lookup succeeded
        /// (issue #797, AC6). Not persisted: it describes one runtime lookup, not stored state.
        /// </summary>
        [JsonIgnore]
        internal string? LastSmtpLookupError { get; private set; }

        /// <summary>
        /// Re-runs the SMTP lookup and republishes the result on
        /// <see cref="UserEmailAddress"/> (issue #797, AC6). Safe to call when
        /// <see cref="RootFolder"/> is null.
        /// </summary>
        /// <returns>The resolved address, or null when every source failed.</returns>
        internal string? RefreshUserEmailAddress()
        {
            // why: issue #797 AC6. The lookup ran once per Init and was never retried, and the
            // resolved address carries JsonIgnore so a success is not cached across restarts.
            // This member itself guarantees nothing about how often the lookup runs: it re-runs
            // the lookup on every call and republishes whatever it returns. The bound lives in
            // the caller. why: issue #812, rescoped by issue #823.
            // StoreWrapperController.PopulateWithCurrent attempts this at most once
            // per controller instance per store, which equals once per store per dialog open only
            // because RibbonController.FolderStoresSettings builds a fresh controller per open.
            // Safe when RootFolder is null: the chain's first read is null-conditional, so the
            // call yields null and records a reason rather than throwing.
            UserEmailAddress = GetSmtpAddressFromStore();
            return UserEmailAddress;
        }

        internal string? GetSmtpAddressFromStore()
        {
            // why: issue #797 AC6. A single outer catch converted every COM failure into null, with
            // no fallback source, no captured reason and no retry, so the settings dialog rendered a
            // generic placeholder on every start. Each step below carries its own COM handling, in
            // the order the specification fixes: the Exchange primary SMTP address; then the address
            // entry's own address when it contains an at-sign; then the store display name when it
            // contains an at-sign; then null. This mirrors the ordering the application globals
            // helper already implements for an address entry.
            string? capturedError = null;
            AddressEntry? addressEntry = null;

            try
            {
                var currentUserStopwatch = Stopwatch.StartNew();
                var currentUser = RootFolder?.Session?.CurrentUser;
                logger.Debug(
                    $"[Startup timing] GetSmtpAddressFromStore '{DisplayName ?? "<null>"}' CurrentUser: {currentUserStopwatch.ElapsedMilliseconds} ms"
                );

                var addressEntryStopwatch = Stopwatch.StartNew();
                addressEntry = currentUser?.AddressEntry;
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

                if (!string.IsNullOrEmpty(primarySmtpAddress))
                {
                    LastSmtpLookupError = null;
                    return primarySmtpAddress;
                }
            }
            catch (COMException e)
            {
                capturedError = e.Message;
                logger.Error(
                    $"Error retrieving PrimarySmtpAddress from secondary inbox. {e.Message}",
                    e
                );
            }

            try
            {
                var address = addressEntry?.Address;
                if (address is not null && address.Contains("@"))
                {
                    LastSmtpLookupError = null;
                    return address;
                }
            }
            catch (COMException e)
            {
                capturedError = e.Message;
                logger.Error(
                    $"Error retrieving the address entry address for '{DisplayName ?? "<null>"}'. {e.Message}",
                    e
                );
            }

            var displayName = DisplayName;
            if (displayName is not null && displayName.Contains("@"))
            {
                LastSmtpLookupError = null;
                return displayName;
            }

            LastSmtpLookupError =
                capturedError
                ?? "No Exchange address, address entry address or store display name yielded an SMTP address.";
            return null;
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
