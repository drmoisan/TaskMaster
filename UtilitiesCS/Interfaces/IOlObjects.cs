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
        Folder OlEmailRoot { get; }
        Reminders OlReminders { get; }
        Folder ToDoFolder { get; }
        bool ShowInConversations { get; set; }
        StackObjectCS<object> MovedMails_Stack { get; set; }
        string View_Wide { get; }
    }
}