using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using UtilitiesCS;

namespace TaskMaster
{
    public class AppOlObjects : IOlObjects
    {
        public AppOlObjects(Application olApplication)
        {
            _olApplication = olApplication;
        }

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

        private Folder _inbox;
        public Folder Inbox
        {
            get
            {
                if (_inbox is null)
                    _inbox = (Folder)NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                return _inbox;
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

        private Folder _emailRoot;
        public Folder EmailRoot
        {
            get
            {
                if (_emailRoot is null)
                    _emailRoot = (Folder)App.Session.DefaultStore.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                return _emailRoot;
            }
        }
        
        private string _emailRootPath;
        public string EmailRootPath
        {
            get
            {
                if (_emailRootPath is null)
                {
                    _emailRootPath = EmailRoot.FolderPath;
                }
                return _emailRootPath;
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

        public string EmailPrefixToStrip => Properties.Resources.Email_Prefix_To_Strip;
        
        private StackObjectCS<object> _movedMails_Stack;
        public StackObjectCS<object> MovedMails_Stack
        {
            get
            {
                return _movedMails_Stack;
            }
            set
            {
                _movedMails_Stack = value;
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