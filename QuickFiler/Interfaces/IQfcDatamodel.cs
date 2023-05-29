using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;


namespace QuickFiler.Interfaces
{
    public enum SortOptionsEnum
    {
        TriageIgnore = 1,
        TriageImportantFirst = 2,
        TriageImportantLast = 4,
        DateRecentFirst = 8,
        DateOldestFirst = 16,
        ConversationUniqueOnly = 32
    }

    public interface IQfcDatamodel
    {
        IList<MailItem> DequeueNextItemGroup(int quantity);
        void UndoMove();
        StackObjectCS<MailItem> StackMovedItems { get; set; }
        bool MoveItems(ref StackObjectCS<MailItem> StackMovedItems);
        IList<MailItem> LoadEmailDataBase(Explorer activeExplorer, IList<MailItem> listEmailsToLoad = null);
        IList<MailItem> MailItemsSort(Items OlItems, SortOptionsEnum options);
    }
}