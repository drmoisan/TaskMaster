using Deedle.Internal;
using Microsoft.Graph.Models.TermStore;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UtilitiesCS.OutlookObjects.Folder;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookObjects.Store
{
    public class StoreWrapper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public StoreWrapper(Outlook.Store store) { InnerStore = store; }
        
        public StoreWrapper Init()
        {
            DisplayName = InnerStore.DisplayName;
            RootFolder = InnerStore.GetRootFolder() as Outlook.Folder;
            if (InnerStore.ExchangeStoreType != Outlook.OlExchangeStoreType.olExchangePublicFolder)
            {
                Inbox = InnerStore.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox) as Outlook.Folder;
            }
            UserEmailAddress = GetSmtpAddressFromStore();
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
                logger.Error($"Error restoring {nameof(StoreWrapper)} named {DisplayName} {e.Message}");
                return false;                
            }
        }
        
        public void Restore(Outlook.Store store) 
        {
            InnerStore = store;
            Init();
            ArchiveRoot?.RestoreFromRelativePath(RootFolder);
            JunkPotential?.RestoreFromRelativePath(RootFolder);
            JunkCertain?.RestoreFromRelativePath(RootFolder);
        }

        public void RestoreGlobalAddresses(Application olApp)
        {
            GlobalAddressBook = InnerStore?
                .GetGlobalAddressList(olApp)?
                .AddressEntries?
                .Cast<AddressEntry>()?
                .ToList();
        }

        #endregion ctor

        #region Store Properties

        public string DisplayName { get; set; }

        [JsonIgnore]
        public Outlook.Store InnerStore { get; internal set; }

        [JsonIgnore]
        public Outlook.Folder Inbox { get; internal set; }

        [JsonIgnore]
        public Outlook.Folder RootFolder { get; internal set; }

        [JsonIgnore]
        public string UserEmailAddress { get; internal set; }

        [JsonIgnore]
        public List<AddressEntry> GlobalAddressBook { get; internal set; }

        internal string GetSmtpAddressFromStore()
        {
            try
            {
                var addressEntry = RootFolder?.Session?.CurrentUser?.AddressEntry;
                var exchangeUser = addressEntry?.GetExchangeUser();
                return exchangeUser?.PrimarySmtpAddress;
            }
            catch (COMException e)
            {
                logger.Error($"Error retrieving PrimarySmtpAddress from secondary inbox. {e.Message}", e);
                return null;
            }
        }

        #endregion Store Properties

        #region Configurable Properties

        public FolderMinimalWrapper ArchiveRoot { get; set; } = new();

        public FilePathHelper ArchiveFsRoot { get; set; } = new();

        public FolderMinimalWrapper JunkPotential { get; set; } = new();

        public FolderMinimalWrapper JunkCertain { get; set; } = new();

        #endregion Configurable Properties
    }
}
