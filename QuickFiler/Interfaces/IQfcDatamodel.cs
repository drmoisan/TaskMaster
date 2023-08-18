using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;


namespace QuickFiler.Interfaces
{
    [Flags]
    public enum SortOptionsEnum
    {
        Default = 42,
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
        ScoStack<IMovedMailInfo> MovedItems { get; }
        IList<MailItem> InitEmailQueueAsync(int batchSize, BackgroundWorker worker);
    }
}