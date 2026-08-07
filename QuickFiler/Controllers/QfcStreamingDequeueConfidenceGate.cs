using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers
{
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
        private readonly Func<MailItem, CancellationToken, Task<long>> _scoreLoader;
        private readonly long _cutoff;
        private readonly TimeProvider _timeProvider;
        private readonly Action<string> _debugLog;
        private readonly Func<bool> _sourceActive;
        private readonly TimeSpan _firstBatchDeadline;
        private readonly Action<int, int, int> _progressCallback;

        internal QfcStreamingDequeueConfidenceGate(
            Func<MailItem> tryTakeNext,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader,
            double threshold,
            TimeProvider timeProvider = null,
            Action<string> debugLog = null
        )
            : this(tryTakeNext, scoreLoader, threshold, timeProvider, debugLog, null) { }

        /// <param name="firstBatchDeadline">
        /// Overall budget for the first batch. <see langword="null"/> selects
        /// <see cref="DefaultFirstBatchDeadline"/>; <see cref="Timeout.InfiniteTimeSpan"/> disables the
        /// deadline and reproduces the pre-#424 unbounded behavior. Any other non-positive value is
        /// rejected.
        /// </param>
        /// <param name="progressCallback">
        /// Optional incremental progress sink invoked once per scored candidate with
        /// <c>(scanned, accepted, quantity)</c>. <see langword="null"/> disables reporting. Exceptions
        /// thrown by the callback propagate to the caller (fail fast); they are not swallowed. The
        /// callback must not touch UI directly — callers route reports through <c>ProgressTracker</c>,
        /// which marshals to the UI thread.
        /// </param>
        internal QfcStreamingDequeueConfidenceGate(
            Func<MailItem> tryTakeNext,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader,
            double threshold,
            TimeProvider timeProvider,
            Action<string> debugLog,
            Func<bool> sourceActive,
            TimeSpan? firstBatchDeadline = null,
            Action<int, int, int> progressCallback = null
        )
        {
            _tryTakeNext = tryTakeNext ?? throw new ArgumentNullException(nameof(tryTakeNext));
            _scoreLoader = scoreLoader ?? throw new ArgumentNullException(nameof(scoreLoader));
            _cutoff = (long)Math.Round(threshold * 1000, 0);
            _timeProvider = timeProvider ?? TimeProvider.System;
            _debugLog = debugLog;
            _sourceActive = sourceActive;
            _progressCallback = progressCallback;

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

        internal async Task<IList<MailItem>> DequeueAsync(
            int quantity,
            int timeOut,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var accepted = new List<MailItem>();
            if (quantity <= 0)
            {
                return accepted;
            }

            bool deadlineEnabled = _firstBatchDeadline != Timeout.InfiniteTimeSpan;
            long start = _timeProvider.GetTimestamp();
            int scanned = 0;

            bool alreadyWaitedForEmptySource = false;
            while (accepted.Count < quantity)
            {
                token.ThrowIfCancellationRequested();

                if (deadlineEnabled && _timeProvider.GetElapsedTime(start) >= _firstBatchDeadline)
                {
                    LogDeadlineExpiry(accepted.Count, scanned);
                    return accepted;
                }

                MailItem mailItem = _tryTakeNext();
                if (mailItem == null)
                {
                    bool sourceCanStillProduce = _sourceActive?.Invoke() == true;
                    if (timeOut <= 0 || (alreadyWaitedForEmptySource && !sourceCanStillProduce))
                    {
                        return accepted;
                    }

                    alreadyWaitedForEmptySource = true;
                    await _timeProvider
                        .Delay(TimeSpan.FromMilliseconds(timeOut), token)
                        .ConfigureAwait(false);
                    continue;
                }

                alreadyWaitedForEmptySource = false;
                long score = await _scoreLoader(mailItem, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                scanned++;
                LogScore(mailItem, score);

                if (score >= _cutoff)
                {
                    accepted.Add(mailItem);
                }

                // Report after the accept decision so `accepted` reflects this candidate. Exceptions
                // from the sink propagate deliberately (fail fast); no catch here.
                _progressCallback?.Invoke(scanned, accepted.Count, quantity);
            }

            return accepted;
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
