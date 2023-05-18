using System.IO;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using UtilitiesVB;

namespace TaskMaster
{

    public class AppOlObjects : IOlObjects
    {

        private string _olEmailRootPath;
        private string _olArchiveRootPath;
        private StackObjectVB _movedMails_Stack;
        private string _userEmailAddress;

        public AppOlObjects(Application OlApp)
        {
            App = OlApp;
        }

        public Application App { get; private set; }

        public string View_Wide { get => Properties.Settings.Default.View_Wide; }
        
        public object View_Compact { get => Properties.Settings.Default.View_Wide; }
        
        public NameSpace NamespaceMAPI
        {
            get
            {
                return App.Application.GetNamespace("MAPI");
            }
        }

        public Folder ToDoFolder
        {
            get
            {
                return (Folder)NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
            }
        }

        public Folder Inbox
        {
            get
            {
                return (Folder)NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
            }
        }

        public Reminders OlReminders
        {
            get
            {
                return App.Reminders;
            }
        }

        public Folder OlEmailRoot
        {
            get
            {
                return (Folder)App.Session.DefaultStore.GetRootFolder();
            }
        }

        public string EmailRootPath
        {
            get
            {
                if (_olEmailRootPath is null)
                {
                    _olEmailRootPath = OlEmailRoot.FolderPath;
                }
                return _olEmailRootPath;
            }
        }

        public string ArchiveRootPath
        {
            get
            {
                if (_olArchiveRootPath is null)
                {
                    _olArchiveRootPath = Path.Combine(OlEmailRoot.FolderPath, "Archive");
                }
                return _olArchiveRootPath;
            }
        }

        public StackObjectVB MovedMails_Stack
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

        public bool ShowInConversations
        {
            get
            {
                if (App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                View objView = (View)App.ActiveExplorer().CurrentView;
                if (value == false & App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations") == true)
                {
                    // Turn Off Conversation View
                    objView.XML = Strings.Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, Compare: Constants.vbTextCompare);
                    objView.Save();
                }
                else if (value == true & App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations") == false)
                {
                    // Turn On Conversation View
                    string strReplace = "<arrangement>" + Constants.vbCrLf + "        <upgradetoconv>1</upgradetoconv>";
                    objView.XML = Strings.Replace(objView.XML, "<arrangement>", strReplace, 1, Compare: Constants.vbTextCompare);
                    objView.Save();
                }
            }
        }

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
    }
}