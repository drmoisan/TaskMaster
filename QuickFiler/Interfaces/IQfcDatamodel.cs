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

        /// <summary>
        /// The first-batch deadline expired before any candidate qualified. Issue #791 made that
        /// deadline advisory: expiry with zero acceptances is now a logged checkpoint that resets
        /// its interval and continues scanning, so the gate no longer returns this member. It is
        /// retained for compatibility — existing callers, mocks and switch arms that name it still
        /// compile, and a caller must continue to treat it exactly as it treats
        /// <see cref="ScanCapReached"/>: the queue stays open because unscanned candidates may
        /// remain.
        /// </summary>
        DeadlineExpired,

        /// <summary>
        /// Issue #791. The extended zero-acceptance scan reached one of its hard bounds — the cap on
        /// candidates scanned without an acceptance, or the time ceiling that bounds the wait while
        /// the background loader is still refilling — before any candidate qualified. This reports a
        /// bounded exit, not exhaustion, and must be treated exactly as
        /// <see cref="DeadlineExpired"/> is: the mail source may still hold unscanned candidates, so
        /// the caller must leave the UI queue open.
        /// </summary>
        ScanCapReached,
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

        /// <summary>
        /// Issue #791. Stops the background remaining-email loader and waits for it, bounded by
        /// <paramref name="timeout"/>. Cancels the datamodel's token source, then awaits the loader
        /// task against a <see cref="TimeProvider"/> delay of the supplied bound, and logs whether
        /// the loader completed or the bound expired.
        /// <para>
        /// Returns when the loader completes or when the bound expires, whichever happens first.
        /// It never throws for the timeout case: a bounded-out loader is reported, not raised. It is
        /// awaited from the Cancel path before any datamodel field is nulled, so a loader still in
        /// flight cannot observe a released field. It must not be converted into a blocking wait
        /// inside <see cref="Cleanup"/>, which runs on the UI thread (issue #731 finding 4).
        /// </para>
        /// </summary>
        /// <param name="timeout">The upper bound on the wait. Supplied by the caller as a constant.</param>
        Task QuiesceLoaderAsync(TimeSpan timeout);

        void Cleanup();
    }
}
