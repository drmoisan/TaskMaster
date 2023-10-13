using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using System.Threading.Tasks;
using System.Threading;


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
        Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut);
        void UndoMove();
        ScoStack<IMovedMailInfo> MovedItems { get; }
        IList<MailItem> InitEmailQueue(int batchSize, BackgroundWorker worker);
        Task<IList<MailItem>> InitEmailQueueAsync(int batchSize, BackgroundWorker worker, CancellationToken token, CancellationTokenSource tokenSource);
    }
}