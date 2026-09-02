using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Issue #446 and Scope 427-A. The gate's own result: the accepted candidates with the folder
    /// each was already scored against, the reason the scan stopped, and how many candidates were
    /// scanned. Declared as a <c>readonly struct</c> with get-only properties because
    /// <c>net481</c> has no <c>IsExternalInit</c> and therefore no <c>record</c>,
    /// <c>record struct</c> or <c>init</c> accessor.
    /// </summary>
    internal readonly struct QfcGateBatch
    {
        private readonly IList<QfcPreScoredItem> _accepted;

        /// <summary>
        /// Creates a gate result. A null accepted collection surfaces as an empty list so a
        /// defaulted struct is inert rather than a null-reference trap.
        /// </summary>
        public QfcGateBatch(IList<QfcPreScoredItem> accepted, QfcDequeueStop stop, int scanned)
        {
            _accepted = accepted;
            Stop = stop;
            Scanned = scanned;
        }

        /// <summary>The accepted candidates, each carrying its predetermined folder.</summary>
        public IList<QfcPreScoredItem> Accepted => _accepted ?? new List<QfcPreScoredItem>();

        /// <summary>Why the scan stopped.</summary>
        public QfcDequeueStop Stop { get; }

        /// <summary>How many candidates were scored during the scan.</summary>
        public int Scanned { get; }
    }

    internal sealed class QfcStreamingDequeueConfidenceGate
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// Default overall budget for assembling the first batch. Bounds the pre-UI wait in High
        /// Confidence mode: without it the scan length is limited only by folder size, because the
        /// loop keeps scoring candidates until <c>quantity</c> qualifiers are found (issue #424).
        /// Not a user setting — it is an implementation quality bound, exposed only through the
        /// constructor seam so tests can drive it deterministically.
        /// </summary>
        internal static readonly TimeSpan DefaultFirstBatchDeadline = TimeSpan.FromSeconds(12);

        private readonly Func<MailItem> _tryTakeNext;

        // Issue #678: the loader publishes the handler its scoring pass initialised, so an accepted
        // candidate carries it forward instead of the consumer re-initialising a second predictor.
        private readonly Func<
            MailItem,
            CancellationToken,
            Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>
        > _scoreLoader;
        private readonly long _cutoff;
        private readonly TimeProvider _timeProvider;
        private readonly Action<string> _debugLog;
        private readonly Func<bool> _sourceActive;
        private readonly TimeSpan _firstBatchDeadline;
        private readonly Action<int, int, int> _progressCallback;
        private readonly Action<MailItem> _onRejected;

        internal QfcStreamingDequeueConfidenceGate(
            Func<MailItem> tryTakeNext,
            Func<
                MailItem,
                CancellationToken,
                Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>
            > scoreLoader,
            double threshold,
            TimeProvider timeProvider = null,
            Action<string> debugLog = null
        )
            : this(tryTakeNext, scoreLoader, threshold, timeProvider, debugLog, null) { }

        /// <param name="firstBatchDeadline">
        /// Overall budget for an empty first-batch result. <see langword="null"/> selects
        /// <see cref="DefaultFirstBatchDeadline"/>; <see cref="Timeout.InfiniteTimeSpan"/> disables the
        /// deadline. Issue #608 restores #233 fill-or-exhaust behavior after a non-empty prefix while
        /// retaining #424's empty deadline result; #446 continues to own empty-result interpretation.
        /// Deadline expiry cannot authorize a non-empty undersized batch. Any other non-positive value is
        /// rejected.
        /// </param>
        /// <param name="progressCallback">
        /// Optional incremental progress sink invoked once per scored candidate with
        /// <c>(scanned, accepted, quantity)</c>. <see langword="null"/> disables reporting. Exceptions
        /// thrown by the callback propagate to the caller (fail fast); they are not swallowed. The
        /// callback must not touch UI directly — callers route reports through <c>ProgressTracker</c>,
        /// which marshals to the UI thread.
        /// </param>
        /// <param name="onRejected">
        /// Issue #426. Optional sink invoked once for every candidate the gate discards because its
        /// score is below the cutoff. A rejected candidate has already been removed from the source
        /// queue and never reaches the accepted-path unhook, so without this sink its
        /// <c>EmailMoveMonitor</c> hook and its live COM reference are retained for the session.
        /// <see langword="null"/> disables the sink. The drop-on-reject contract is unchanged: the
        /// candidate is still discarded and is still absent from the result.
        /// </param>
        internal QfcStreamingDequeueConfidenceGate(
            Func<MailItem> tryTakeNext,
            Func<
                MailItem,
                CancellationToken,
                Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>
            > scoreLoader,
            double threshold,
            TimeProvider timeProvider,
            Action<string> debugLog,
            Func<bool> sourceActive,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progressCallback = null,
            Action<MailItem> onRejected = null
        )
        {
            _tryTakeNext = tryTakeNext ?? throw new ArgumentNullException(nameof(tryTakeNext));
            _scoreLoader = scoreLoader ?? throw new ArgumentNullException(nameof(scoreLoader));
            _cutoff = (long)Math.Round(threshold * 1000, 0);
            _timeProvider = timeProvider ?? TimeProvider.System;
            _debugLog = debugLog;
            _sourceActive = sourceActive;
            _progressCallback = progressCallback;
            _onRejected = onRejected;

            TimeSpan deadline = firstBatchDeadline ?? DefaultFirstBatchDeadline;
            if (deadline != Timeout.InfiniteTimeSpan && deadline <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(firstBatchDeadline),
                    deadline,
                    "The first-batch deadline must be positive, or Timeout.InfiniteTimeSpan to disable it."
                );
            }

            _firstBatchDeadline = deadline;
        }

        internal async Task<QfcGateBatch> DequeueAsync(
            int quantity,
            int timeOut,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var accepted = new List<QfcPreScoredItem>();
            int scanned = 0;
            if (quantity <= 0)
            {
                return new QfcGateBatch(accepted, QfcDequeueStop.QuantitySatisfied, scanned);
            }

            bool deadlineEnabled = _firstBatchDeadline != Timeout.InfiniteTimeSpan;
            long start = _timeProvider.GetTimestamp();

            bool alreadyWaitedForEmptySource = false;
            while (accepted.Count < quantity)
            {
                token.ThrowIfCancellationRequested();

                if (
                    deadlineEnabled
                    && accepted.Count == 0
                    && _timeProvider.GetElapsedTime(start) >= _firstBatchDeadline
                )
                {
                    LogDeadlineExpiry(accepted.Count, scanned);
                    return new QfcGateBatch(accepted, QfcDequeueStop.DeadlineExpired, scanned);
                }

                MailItem mailItem = _tryTakeNext();
                if (mailItem == null)
                {
                    bool sourceCanStillProduce = _sourceActive?.Invoke() == true;
                    if (timeOut <= 0 || (alreadyWaitedForEmptySource && !sourceCanStillProduce))
                    {
                        return new QfcGateBatch(accepted, QfcDequeueStop.SourceExhausted, scanned);
                    }

                    alreadyWaitedForEmptySource = true;
                    await _timeProvider
                        .Delay(TimeSpan.FromMilliseconds(timeOut), token)
                        .ConfigureAwait(false);
                    continue;
                }

                alreadyWaitedForEmptySource = false;
                (long score, string topFolder, IFolderSearchHandler handler) = await _scoreLoader(
                        mailItem,
                        token
                    )
                    .ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                scanned++;
                LogScore(mailItem, score);

                if (score >= _cutoff)
                {
                    // Issue #678: the accepted candidate carries the handler the scoring pass just
                    // initialised, so the item controller adopts it rather than scoring again.
                    accepted.Add(new QfcPreScoredItem(mailItem, topFolder, handler));
                }
                else
                {
                    // Issue #426. The discarded candidate is already out of the source queue and
                    // never reaches the accepted-path unhook, so it is reported here. A monitor
                    // failure must not abort the scan, hence the catch: the candidate is still
                    // dropped either way, and aborting would strand the rest of the batch.
                    try
                    {
                        _onRejected?.Invoke(mailItem);
                    }
                    catch (System.Exception e)
                    {
                        logger.Error(
                            "Rejection sink threw [QfcStreamingDequeueConfidenceGate.DequeueAsync]; "
                                + "the candidate is still discarded and the scan continues.",
                            e
                        );
                    }
                }

                // Report after the accept decision so `accepted` reflects this candidate. Exceptions
                // from the sink propagate deliberately (fail fast); no catch here.
                _progressCallback?.Invoke(scanned, accepted.Count, quantity);
            }

            return new QfcGateBatch(accepted, QfcDequeueStop.QuantitySatisfied, scanned);
        }

        private void LogDeadlineExpiry(int acceptedCount, int scannedCount)
        {
            string message =
                $"First-batch deadline expired [QfcStreamingDequeueConfidenceGate.DequeueAsync] "
                + $"Accepted={acceptedCount} Scanned={scannedCount} Deadline={_firstBatchDeadline}";

            _debugLog?.Invoke(message);
            logger.Debug(message);
        }

        private void LogScore(MailItem mailItem, long score)
        {
            string message =
                $"Probability debug [QfcStreamingDequeueConfidenceGate.DequeueAsync] "
                + $"Subject='{mailItem.Subject}' EntryID='{mailItem.EntryID}' Score={score}";

            _debugLog?.Invoke(message);
            logger.Debug(message);
        }
    }
}
