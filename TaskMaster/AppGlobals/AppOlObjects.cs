using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Windows_Forms;

namespace TaskMaster
{
    public partial class AppOlObjects : IOlObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public AppOlObjects(Application olApplication, IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
            _olApplication = olApplication;
            ResetLazyInboxes();
        }

        public async Task LoadAsync()
        {
            await LoadStoresAsync();
            await Task.CompletedTask;
        }

        private IApplicationGlobals _globals;
        internal ISmartSerializableNonTyped SmartSerializable { get; set; } =
            new SmartSerializableNonTyped();

        private Application _olApplication;
        public Application App
        {
            get => _olApplication;
        }

        private string _viewWide;
        public string ViewWide
        {
            get
            {
                if (_viewWide is null)
                    _viewWide = Properties.Settings.Default.View_Wide;
                return _viewWide;
            }
        }

        private string _viewCompact;
        public string ViewCompact
        {
            get
            {
                if (_viewCompact is null)
                    _viewCompact = Properties.Settings.Default.View_Wide;
                return _viewCompact;
            }
        }

        private NameSpace _namespaceMAPI;
        public NameSpace NamespaceMAPI
        {
            get
            {
                if (_namespaceMAPI is null)
                {
                    _namespaceMAPI = App.GetNamespace("MAPI");
                }
                return App.Application.GetNamespace("MAPI");
            }
        }

        private Folder _toDoFolder;
        public Folder ToDoFolder
        {
            get
            {
                if (_toDoFolder is null)
                    _toDoFolder = (Folder)
                        NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                return _toDoFolder;
            }
        }

        private Lazy<IEnumerable<Folder>> _inboxes;
        public IEnumerable<Folder> Inboxes => _inboxes.Value;

        internal IEnumerable<Folder> LoadInboxes()
        {
            var storesWrapper = StoresWrapper ?? new StoresWrapper() { };
            var stores = NamespaceMAPI.Stores.Cast<Store>();

            var inboxes = new List<Folder>();
            foreach (var store in stores)
            {
                try
                {
                    if (!storesWrapper.ShouldIncludeStore(store))
                    {
                        continue;
                    }

                    var inbox = store.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                    if (inbox is not null)
                    {
                        inboxes.Add((Folder)inbox);
                    }
                }
                catch (COMException e)
                {
                    // Issue #207: a transient "store not ready" HRESULT during cold start must NOT
                    // silently drop this store's inbox subscription. Rethrow so the readiness
                    // coordinator/gate routes it to retry; only genuinely permanent errors are
                    // logged and skipped. The transient HRESULTs are shared as public constants on
                    // OutlookReadinessGate to avoid duplicating literals.
                    uint hresult = unchecked((uint)e.ErrorCode);
                    if (
                        hresult == OutlookReadinessGate.TransientStoreNotReadyHResult
                        || hresult == OutlookReadinessGate.TransientOperationFailedHResult
                    )
                    {
                        throw;
                    }

                    logger.Error($"Error loading inbox from store. {e.Message}", e);
                }
            }
            return inboxes;
        }

        internal void ResetLazyInboxes() => _inboxes = new Lazy<IEnumerable<Folder>>(LoadInboxes);

        public StoresWrapper StoresWrapper { get; set; }

        protected internal virtual Task AwaitStoreRewireAsync(StoresWrapper storesWrapper) =>
            storesWrapper is null
                ? Task.CompletedTask
                : storesWrapper.RewireAfterDeserializeAsync();

        internal async Task LoadStoresAsync()
        {
            if (_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config))
            {
                StoresWrapper = SmartSerializable.Deserialize<
                    StoresWrapper,
                    SmartSerializableLoader
                >(config);
                await AwaitStoreRewireAsync(StoresWrapper);
            }
            else
            {
                logger.Error("StoresWrapper config not found.");
            }
        }

        private Reminders _olReminders;
        public Reminders OlReminders
        {
            get
            {
                if (_olReminders is null)
                    _olReminders = App.Reminders;
                return _olReminders;
            }
        }

        private Folder _root;
        public Folder Root
        {
            get
            {
                if (_root is null)
                    _root = (Folder)App.Session.DefaultStore.GetRootFolder();
                return _root;
            }
        }

        private Folder _inbox;
        public Folder Inbox
        {
            get
            {
                if (_inbox is null)
                    _inbox = (Folder)
                        App.Session.DefaultStore.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                return _inbox;
            }
        }

        private string _inboxRootPath;
        public string InboxPath
        {
            get
            {
                if (_inboxRootPath is null)
                {
                    _inboxRootPath = Inbox.FolderPath;
                }
                return _inboxRootPath;
            }
        }

        private string _archiveRootPath;
        public string ArchiveRootPath
        {
            get
            {
                if (_archiveRootPath is null)
                {
                    _archiveRootPath = Path.Combine(Root.FolderPath, "Archive");
                }
                return _archiveRootPath;
            }
        }

        private Folder _archiveRoot;
        public Folder ArchiveRoot => Initializer.GetOrLoad(ref _archiveRoot, LoadArchiveRoot);

        internal Folder LoadArchiveRoot()
        {
            var folderHandler = new FolderPredictor(_globals);
            return folderHandler.GetFolder(Root.Folders, "Archive");
        }

        public string EmailPrefixToStrip => Properties.Resources.Email_Prefix_To_Strip;

        private StackObjectCS<object> _movedMailsStack;
        public StackObjectCS<object> MovedMailsStack
        {
            get { return _movedMailsStack; }
            set { _movedMailsStack = value; }
        }

        private TimedDiskWriter<string> _emailMoveWriter;
        public TimedDiskWriter<string> EmailMoveWriter =>
            Initializer.GetOrLoad(ref _emailMoveWriter, LoadEmailMoveWriter);

        public TimedDiskWriter<string> LoadEmailMoveWriter()
        {
            var writer = new TimedDiskWriter<string>();
            writer.Config.WriteInterval = TimeSpan.FromSeconds(5);
            writer.Config.TryAddTimeout = 20;
            if (_globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
                    _globals.FS.Filenames.MovedMails,
                    myDocuments
                );
                writer.DiskWriter = async (items) =>
                    await FileIO2.WriteTextFileAsync(
                        _globals.FS.Filenames.MovedMails,
                        items.ToArray(),
                        myDocuments,
                        default
                    );
                return writer;
            }
            else
            {
                return null;
            }
        }

        private string _userEmailAddress;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserEmailAddress
        {
            get
            {
                if (_userEmailAddress is null)
                {
                    _userEmailAddress = ResolveCurrentUserEmailAddress();
                }
                return _userEmailAddress;
            }
        }

        internal string ResolveCurrentUserEmailAddress()
        {
            // Outlook COM objects must be accessed from the STA thread on which they were
            // created. If this method is called from a background (MTA/ThreadPool) thread,
            // marshal synchronously to the UI thread to avoid COMException 0xEF640201.
            if (Thread.CurrentThread.ManagedThreadId != UiThread.UiThreadId)
            {
                string result = string.Empty;
                UiThread.UiSyncContext.Send(_ => result = ResolveCurrentUserEmailAddress(), null);
                return result;
            }

            try
            {
                var session = App?.Session ?? NamespaceMAPI;
                var addressEntry = session?.CurrentUser?.AddressEntry;
                return TryGetSmtpAddress(addressEntry) ?? string.Empty;
            }
            catch (COMException e)
            {
                logger.Warn($"Error retrieving current user SMTP address. {e.Message}", e);
                return string.Empty;
            }
        }

        internal static string TryGetSmtpAddress(AddressEntry addressEntry)
        {
            if (addressEntry is null)
            {
                return null;
            }

            try
            {
                var primarySmtpAddress = addressEntry.GetExchangeUser()?.PrimarySmtpAddress;
                if (!string.IsNullOrWhiteSpace(primarySmtpAddress))
                {
                    return primarySmtpAddress;
                }
            }
            catch (COMException) { }

            try
            {
                var address = addressEntry.Address;
                if (!string.IsNullOrWhiteSpace(address) && address.Contains("@"))
                {
                    return address;
                }
            }
            catch (COMException) { }

            return null;
        }

        private bool _darkMode = Properties.Settings.Default.DarkMode;

        [NotifyParentProperty(true)]
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                _darkMode = value;
                Properties.Settings.Default.DarkMode = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int GetExplorerScreenNumber()
        {
            System.Windows.Forms.Screen screen = GetExplorerScreen();
            return System.Windows.Forms.Screen.AllScreens.ToList().IndexOf(screen);
        }

        public Size GetExplorerScreenSize()
        {
            var explorer = App.ActiveExplorer();
            Rectangle bounds = new(explorer.Left, explorer.Top, explorer.Width, explorer.Height);
            return bounds.Size;
        }

        public System.Windows.Forms.Screen GetExplorerScreen()
        {
            var explorer = App.ActiveExplorer();
            Rectangle bounds = new(explorer.Left, explorer.Top, explorer.Width, explorer.Height);
            return System.Windows.Forms.Screen.AllScreens.FindMax(
                (s1, s2) =>
                {
                    var a1 = Rectangle.Intersect(s1.Bounds, bounds).Area();
                    var a2 = Rectangle.Intersect(s2.Bounds, bounds).Area();
                    return a2 > a1 ? s2 : s1;
                }
            );
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public bool ShowInConversations
        //{
        //    get
        //    {
        //        if (App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations"))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    set
        //    {
        //        View objView = (View)App.ActiveExplorer().CurrentView;
        //        if (value == false & App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations") == true)
        //        {
        //            // Turn Off Conversation View
        //            objView.XML = Strings.Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, Compare: Constants.vbTextCompare);
        //            objView.Save();
        //        }
        //        else if (value == true & App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations") == false)
        //        {
        //            // Turn On Conversation View
        //            string strReplace = "<arrangement>" + Constants.vbCrLf + "        <upgradetoconv>1</upgradetoconv>";
        //            objView.XML = Strings.Replace(objView.XML, "<arrangement>", strReplace, 1, Compare: Constants.vbTextCompare);
        //            objView.Save();
        //        }
        //    }
        //}
    }
}
