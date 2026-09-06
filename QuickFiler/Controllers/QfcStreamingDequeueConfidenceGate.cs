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

        /// <summary>
        /// Issue #791. Cap on candidates scored without a single acceptance. One of the two hard
        /// bounds that terminate the extended zero-acceptance scan after the first-batch deadline
        /// became advisory. An implementation quality bound, not a user setting: it is exposed only
        /// through the optional constructor parameter so tests can drive it deterministically, and
        /// it introduces no settings surface, following the ratified #424 precedent.
        /// </summary>
        internal static readonly int DefaultMaxScanWithoutAcceptance = 250;

        /// <summary>
        /// Issue #791. Time ceiling on the extended zero-acceptance scan. The scan cap alone cannot
        /// bound the pre-UI wait, because the empty-queue wait path does not increment the scanned
        /// count while the loader is still refilling, so a time bound is required in addition. An
        /// implementation quality bound with a constructor test seam and no settings surface.
        /// </summary>
        internal static readonly TimeSpan DefaultZeroAcceptanceCeiling = TimeSpan.FromSeconds(120);

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
        /// <param name="maxScanWithoutAcceptance">
        /// Issue #791. Cap on candidates scored without a single acceptance.
        /// <see langword="null"/> selects <see cref="DefaultMaxScanWithoutAcceptance"/>.
        /// </param>
        /// <param name="zeroAcceptanceCeiling">
        /// Issue #791. Time ceiling on the extended zero-acceptance scan.
        /// <see langword="null"/> selects <see cref="DefaultZeroAcceptanceCeiling"/>.
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
            Action<MailItem> onRejected = null,
            int? maxScanWithoutAcceptance = null,
            TimeSpan? zeroAcceptanceCeiling = null
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
            MaxScanWithoutAcceptance = maxScanWithoutAcceptance ?? DefaultMaxScanWithoutAcceptance;
            ZeroAcceptanceCeiling = zeroAcceptanceCeiling ?? DefaultZeroAcceptanceCeiling;
        }

        /// <summary>
        /// Issue #791. The effective cap on candidates scored without an acceptance. Declared as a
        /// get-only auto-property rather than a <c>private readonly</c> field so the seam is
        /// warning-clean at every point of the change: a private field assigned and never read
        /// raises CS0414, which <c>/p:TreatWarningsAsErrors=true</c> promotes to an error, whereas
        /// an auto-property's compiler-generated backing field is read by its getter.
        /// </summary>
        internal int MaxScanWithoutAcceptance { get; }

        /// <summary>Issue #791. The effective time ceiling on the extended zero-acceptance scan.</summary>
        internal TimeSpan ZeroAcceptanceCeiling { get; }

        internal async Task<QfcGateBatch> DequeueAsync(
            int quantity,
            int timeOut,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            LogLaunch(quantity);

            var accepted = new List<QfcPreScoredItem>();
            int scanned = 0;
            if (quantity <= 0)
            {
                return new QfcGateBatch(accepted, QfcDequeueStop.QuantitySatisfied, scanned);
            }

            bool deadlineEnabled = _firstBatchDeadline != Timeout.InfiniteTimeSpan;
            long start = _timeProvider.GetTimestamp();

            // Issue #791: the checkpoint interval is measured from its own origin, which is reset at
            // every checkpoint, while both hard bounds are measured against the run origin. Sharing
            // one origin would make the first checkpoint also the last, which is the superseded
            // #424 behaviour.
            long checkpointOrigin = start;

            bool alreadyWaitedForEmptySource = false;
            while (accepted.Count < quantity)
            {
                token.ThrowIfCancellationRequested();

                // Issue #791: the whole zero-acceptance policy stays inside the same
                // `deadlineEnabled && accepted.Count == 0` guard the #424 deadline used, so
                // Timeout.InfiniteTimeSpan still means "no bound at all" and a non-empty prefix is
                // still governed by #608 fill-or-exhaust rather than by any bound.
                if (deadlineEnabled && accepted.Count == 0)
                {
                    TimeSpan elapsed = _timeProvider.GetElapsedTime(start);

                    // The bounds are evaluated ahead of the take, so a bounded scan cannot consume
                    // one extra candidate out of the master queue on its way out.
                    if (scanned >= MaxScanWithoutAcceptance || elapsed >= ZeroAcceptanceCeiling)
                    {
                        LogScanBoundReached(accepted.Count, scanned, elapsed);
                        return new QfcGateBatch(accepted, QfcDequeueStop.ScanCapReached, scanned);
                    }

                    if (_timeProvider.GetElapsedTime(checkpointOrigin) >= _firstBatchDeadline)
                    {
                        LogZeroAcceptanceCheckpoint(accepted.Count, scanned, elapsed);
                        checkpointOrigin = _timeProvider.GetTimestamp();
                    }
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

        /// <summary>
        /// Issue #791. One line per dequeue recording the cutoff in force, the requested quantity,
        /// the checkpoint interval and both hard bounds, so an operator reading the log can tell
        /// which configuration a run used instead of inferring it from the outcome. The reported
        /// cutoff (900) was never logged before this change, which is why the field reports could
        /// not be diagnosed.
        /// </summary>
        private void LogLaunch(int quantity)
        {
            string message =
                $"High-confidence dequeue launch [QfcStreamingDequeueConfidenceGate.DequeueAsync] "
                + $"Cutoff={_cutoff} ({_cutoff / 1000.0}) Quantity={quantity} "
                + $"CheckpointInterval={_firstBatchDeadline} ScanCap={MaxScanWithoutAcceptance} "
                + $"Ceiling={ZeroAcceptanceCeiling}";

            _debugLog?.Invoke(message);
            logger.Debug(message);
        }

        /// <summary>
        /// Issue #791. Replaces the #424 expiry line. The first-batch deadline is now an advisory
        /// checkpoint, so this records a decision to continue rather than a bounded return, and
        /// carries the remaining headroom on both bounds alongside the counts.
        /// </summary>
        private void LogZeroAcceptanceCheckpoint(
            int acceptedCount,
            int scannedCount,
            TimeSpan elapsed
        )
        {
            string message =
                $"Zero-acceptance checkpoint [QfcStreamingDequeueConfidenceGate.DequeueAsync] "
                + $"Accepted={acceptedCount} Scanned={scannedCount} Cutoff={_cutoff} "
                + $"Elapsed={elapsed} Interval={_firstBatchDeadline} "
                + $"RemainingScans={MaxScanWithoutAcceptance - scannedCount} "
                + $"RemainingTime={ZeroAcceptanceCeiling - elapsed} Decision=continue";

            _debugLog?.Invoke(message);
            logger.Debug(message);
        }

        /// <summary>
        /// Issue #791. The bounded zero-acceptance exit: which bound was reached, and the counts and
        /// cutoff that produced it. This is the one case in which the gate may now return an empty
        /// batch while candidates remain unscanned, so it is logged explicitly.
        /// </summary>
        private void LogScanBoundReached(int acceptedCount, int scannedCount, TimeSpan elapsed)
        {
            string bound =
                scannedCount >= MaxScanWithoutAcceptance ? "scan-cap" : "zero-acceptance-ceiling";
            string message =
                $"Zero-acceptance scan bound reached [QfcStreamingDequeueConfidenceGate.DequeueAsync] "
                + $"Accepted={acceptedCount} Scanned={scannedCount} Cutoff={_cutoff} "
                + $"Elapsed={elapsed} ScanCap={MaxScanWithoutAcceptance} "
                + $"Ceiling={ZeroAcceptanceCeiling} Bound={bound} Decision=stop";

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
