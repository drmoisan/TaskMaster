using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Windows_Forms;

namespace TaskMaster
{
    public class AppOlObjects : IOlObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppOlObjects(Application olApplication, IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
            _olApplication = olApplication;
            ResetLazyInboxes();
        }

        public async Task LoadAsync()
        {
            await LoadStoresAsync();
        }

        private IApplicationGlobals _globals;
        internal ISmartSerializableNonTyped SmartSerializable { get; set; } = new SmartSerializableNonTyped();

        private Application _olApplication;
        public Application App { get => _olApplication; }

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
                    _toDoFolder = (Folder)NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                return _toDoFolder;
            }
        }

        private Lazy<IEnumerable<Folder>> _inboxes;
        public IEnumerable<Folder> Inboxes => _inboxes.Value;
        internal IEnumerable<Folder> LoadInboxes()
        {
            // TODO: Test with gmail to see if I need to add a filter for non-exchange
            var stores = NamespaceMAPI.Stores.Cast<Store>().Where(
                store => store.ExchangeStoreType != OlExchangeStoreType.olExchangePublicFolder);
            
            var inboxes = new List<Folder>();
            foreach (var store in stores) 
            {
                MAPIFolder inbox = null;
                try
                {                    
                    inbox = store.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                }
                catch (COMException e) 
                { 
                    logger.Error($"Error loading inbox from store. {e.Message}", e);
                }
                if (inbox is not null)
                {
                    inboxes.Add((Folder)inbox);
                }
            }
            return inboxes;
        }
        internal void ResetLazyInboxes() => _inboxes = new Lazy<IEnumerable<Folder>>(LoadInboxes);

        public StoresWrapper StoresWrapper { get; set; }
        async internal Task LoadStoresAsync() => await Task.Run(async () =>
        {
            if (_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config))
            {
                StoresWrapper = await SmartSerializable.DeserializeAsync(config, true, () => new StoresWrapper(_globals).Init());                
            }
            else { logger.Error("StoresWrapper config not found."); }
        }, _globals.AF.CancelToken);

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
                    _inbox = (Folder)App.Session.DefaultStore.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
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
                
        private Folder _junkPotential;
        public Folder JunkPotential => Initializer.GetOrLoad(ref _junkPotential, LoadJunkPotential);
        internal Folder LoadJunkPotential()
        {
            var root = new FolderTree(Root).Roots.FirstOrDefault();
            var folderPath = Properties.Settings.Default.OlJunkPotential;
            if (folderPath.IsNullOrEmpty()) { return null; }
            var sequence = new Queue<string>(folderPath.Split('\\'));
            
            var node = root.FindSequentialNode((current, other) => current.Name == other, sequence);
            var folder = node?.Value?.OlFolder as Folder;
            if (folder is null) 
            { 
                MyBox.ShowDialog("Junk Potential Folder not found. Please select it manually.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                folder = NamespaceMAPI.PickFolder() as Folder;
                if (folder is null) { return null; }
                var wrapper = new FolderWrapper(folder, Root);
                Properties.Settings.Default.OlJunkPotential = wrapper.RelativePath;
                Properties.Settings.Default.Save();
            }
            return folder;
        }

        private Folder _junkCertain;
        //public Folder JunkCertain
        //{
        //    get
        //    {
        //        if (_junkCertain is null)
        //        {
        //            _junkCertain = (Folder)App.Session.DefaultStore.GetDefaultFolder(OlDefaultFolders.olFolderJunk);
        //        }
        //        return _junkCertain;
        //    }
        //}
        public Folder JunkCertain => Initializer.GetOrLoad(ref _junkPotential, LoadJunkCertain);
        internal Folder LoadJunkCertain()
        {
            var root = new FolderTree(Root).Roots.FirstOrDefault();
            var folderPath = Properties.Settings.Default.OlJunkCertain;
            if (folderPath.IsNullOrEmpty()) { return null; }
            var sequence = new Queue<string>(folderPath.Split('\\'));

            var node = root.FindSequentialNode((current, other) => current.Name == other, sequence);
            var folder = node?.Value?.OlFolder as Folder;
            if (folder is null)
            {
                MyBox.ShowDialog("Junk Potential Folder not found. Please select it manually.", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                folder = NamespaceMAPI.PickFolder() as Folder;
                if (folder is null) { return null; }
                var wrapper = new FolderWrapper(folder, Root);
                Properties.Settings.Default.OlJunkCertain = wrapper.RelativePath;
                Properties.Settings.Default.Save();
            }
            return folder;
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
            get
            {
                return _movedMailsStack;
            }
            set
            {
                _movedMailsStack = value;
            }
        }

        private TimedDiskWriter<string> _emailMoveWriter;
        public TimedDiskWriter<string> EmailMoveWriter => Initializer.GetOrLoad(ref _emailMoveWriter, LoadEmailMoveWriter);
        public TimedDiskWriter<string> LoadEmailMoveWriter()
        {
            var writer = new TimedDiskWriter<string>();
            writer.Config.WriteInterval = TimeSpan.FromSeconds(5);
            writer.Config.TryAddTimeout = 20;
            if (_globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                SortEmail.WriteCSV_StartNewFileIfDoesNotExist(_globals.FS.Filenames.MovedMails, myDocuments);
                writer.DiskWriter = async (items) => await FileIO2.WriteTextFileAsync(_globals.FS.Filenames.MovedMails, items.ToArray(), myDocuments, default);
                return writer;
            }
            else { return null; }
        }

        private string _userEmailAddress;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserEmailAddress
        {
            get
            {
                if (_userEmailAddress is null)
                {
                    _userEmailAddress = App.ActiveExplorer().Session.CurrentUser.AddressEntry.GetExchangeUser().PrimarySmtpAddress;
                }
                return _userEmailAddress;
            }
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
            return System.Windows.Forms.Screen.AllScreens.FindMax((s1, s2) =>
            {
                var a1 = Rectangle.Intersect(s1.Bounds, bounds).Area();
                var a2 = Rectangle.Intersect(s2.Bounds, bounds).Area();
                return a2 > a1 ? s2 : s1;
            });
        }
        
        private void NotifyPropertyChanged([CallerMemberName] string propertyName="")
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