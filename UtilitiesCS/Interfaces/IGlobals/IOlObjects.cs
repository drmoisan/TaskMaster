using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{

    public interface IOlObjects
    {
        Application App { get; }
        string EmailRootPath { get; }
        string ArchiveRootPath { get; }
        string UserEmailAddress { get; }
        Folder Inbox { get; }
        NameSpace NamespaceMAPI { get; }
        Folder Root { get; }
        Folder EmailRoot { get; }
        Reminders OlReminders { get; }
        Folder ToDoFolder { get; }
        StackObjectCS<object> MovedMails_Stack { get; set; }
        string ViewWide { get; }
        string ViewCompact { get; }
    }
}