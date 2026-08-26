using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
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

    /// <summary>
    /// Issue #446. Why the dequeue stopped. A caller cannot otherwise distinguish a
    /// deadline-bounded empty batch from genuine exhaustion of the mail source, and treating the
    /// former as the latter irreversibly closes the UI queue for the rest of the session.
    /// </summary>
    public enum QfcDequeueStop
    {
        /// <summary>The requested quantity was assembled, or the request was degenerate.</summary>
        QuantitySatisfied,

        /// <summary>The mail source is drained and no producer is still loading.</summary>
        SourceExhausted,

        /// <summary>The first-batch deadline expired before any candidate qualified.</summary>
        DeadlineExpired,
    }

    /// <summary>
    /// Issue #446 and Scope 427-A. The dequeue result at the datamodel boundary: the batch, the
    /// pre-scored carriers that survived the high-confidence gate, and the reason the dequeue
    /// stopped. Declared as a <c>readonly struct</c> with get-only properties because
    /// <c>net481</c> has no <c>IsExternalInit</c> and therefore no <c>record</c>,
    /// <c>record struct</c> or <c>init</c> accessor.
    /// </summary>
    public readonly struct QfcDequeueBatch
    {
        private readonly IList<MailItem> _items;
        private readonly IList<QfcPreScoredItem> _preScored;

        /// <summary>
        /// Creates a dequeue result. Null collections are tolerated and surface as empty lists, so
        /// a defaulted struct returned by an unconfigured loose Moq setup is inert rather than a
        /// null-reference trap.
        /// </summary>
        public QfcDequeueBatch(
            IList<MailItem> items,
            IList<QfcPreScoredItem> preScored,
            QfcDequeueStop stop
        )
        {
            _items = items;
            _preScored = preScored;
            Stop = stop;
        }

        /// <summary>The dequeued mail items. Never null; empty when nothing was dequeued.</summary>
        public IList<MailItem> Items => _items ?? new List<MailItem>();

        /// <summary>
        /// The pre-scored carriers for the accepted items, each pairing a mail item with the folder
        /// the gate already computed for it. Never null; empty outside high-confidence mode.
        /// </summary>
        public IList<QfcPreScoredItem> PreScored => _preScored ?? new List<QfcPreScoredItem>();

        /// <summary>Why the dequeue stopped.</summary>
        public QfcDequeueStop Stop { get; }
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

        /// <summary>
        /// Issue #446. Outcome-bearing dequeue: the same batch the four-argument
        /// <see cref="DequeueNextItemGroupAsync(int, int, TimeSpan, Action{int, int, int})"/>
        /// overload produces, plus the reason the dequeue stopped. Without the stop reason a caller
        /// cannot tell a deadline-bounded empty batch from genuine source exhaustion, and treating
        /// the former as the latter irreversibly closes the UI queue for the rest of the session.
        /// </summary>
        Task<QfcDequeueBatch> DequeueNextItemGroupWithOutcomeAsync(
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
