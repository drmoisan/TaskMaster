using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

namespace UtilitiesCS
{

    public interface IOlObjects: INotifyPropertyChanged
    {
        Application App { get; }
        string InboxPath { get; }
        string ArchiveRootPath { get; }
        Folder ArchiveRoot { get; }        
        string UserEmailAddress { get; }
        string EmailPrefixToStrip { get; }
        Folder Inbox { get; }
        IEnumerable<Folder> Inboxes { get; }
        NameSpace NamespaceMAPI { get; }
        Folder Root { get; }        
        Reminders OlReminders { get; }
        Folder ToDoFolder { get; }
        StackObjectCS<object> MovedMailsStack { get; set; }
        string ViewWide { get; }
        string ViewCompact { get; }
        bool DarkMode { get; set; }
        TimedDiskWriter<string> EmailMoveWriter { get; }
        int GetExplorerScreenNumber();
        System.Windows.Forms.Screen GetExplorerScreen();
        public Folder JunkCertain { get; }
        public Folder JunkPotential { get; }
        public Size GetExplorerScreenSize();
        public Task LoadAsync();
    }
}