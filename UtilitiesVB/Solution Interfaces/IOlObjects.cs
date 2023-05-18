using Microsoft.Office.Interop.Outlook;

namespace UtilitiesVB
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
        StackObjectVB MovedMails_Stack { get; set; }
        string View_Wide { get; }
    }
}