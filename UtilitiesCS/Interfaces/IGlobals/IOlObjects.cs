using Microsoft.Office.Interop.Outlook;
using System.ComponentModel;
using System.Drawing;

namespace UtilitiesCS
{

    public interface IOlObjects: INotifyPropertyChanged
    {
        Application App { get; }
        string EmailRootPath { get; }
        string ArchiveRootPath { get; }
        Folder ArchiveRoot { get; }
        string UserEmailAddress { get; }
        string EmailPrefixToStrip { get; }
        Folder Inbox { get; }
        NameSpace NamespaceMAPI { get; }
        Folder Root { get; }
        Folder EmailRoot { get; }
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
    }
}