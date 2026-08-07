using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

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
        ConversationUniqueOnly = 32,
    }

    public interface IQfcDatamodel
    {
        Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut);

        /// <summary>
        /// Issue #424 overload carrying the dequeue-gate first-batch deadline and an optional
        /// incremental progress sink. The two-argument overload delegates here with
        /// <c>QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline</c> and a null sink.
        /// </summary>
        /// <param name="firstBatchDeadline">
        /// Overall budget for assembling the first batch. <c>Timeout.InfiniteTimeSpan</c> disables it.
        /// </param>
        /// <param name="progress">
        /// Optional sink invoked once per scored candidate with <c>(scanned, accepted, quantity)</c>.
        /// Exceptions thrown by the sink propagate. Ignored outside high-confidence mode.
        /// </param>
        Task<IList<MailItem>> DequeueNextItemGroupAsync(
            int quantity,
            int timeOut,
            TimeSpan firstBatchDeadline,
            Action<int, int, int> progress
        );
        IList<MailItem> DequeueNextItemGroup(int quantity);
        void UndoMove();
        SloStack<IMovedMailInfo> MovedItems { get; }
        IList<MailItem> InitEmailQueue(int batchSize, BackgroundWorker worker);
        Task<IList<MailItem>> InitEmailQueueAsync(
            int batchSize,
            BackgroundWorker worker,
            CancellationToken token,
            CancellationTokenSource tokenSource
        );
        bool Complete { get; set; }
        void Cleanup();
    }
}
