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

        private readonly Func<MailItem> _tryTakeNext;
        private readonly Func<MailItem, CancellationToken, Task<long>> _scoreLoader;
        private readonly long _cutoff;
        private readonly TimeProvider _timeProvider;
        private readonly Action<string> _debugLog;
        private readonly Func<bool> _sourceActive;

        internal QfcStreamingDequeueConfidenceGate(
            Func<MailItem> tryTakeNext,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader,
            double threshold,
            TimeProvider timeProvider = null,
            Action<string> debugLog = null
        )
            : this(tryTakeNext, scoreLoader, threshold, timeProvider, debugLog, null) { }

        internal QfcStreamingDequeueConfidenceGate(
            Func<MailItem> tryTakeNext,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader,
            double threshold,
            TimeProvider timeProvider,
            Action<string> debugLog,
            Func<bool> sourceActive
        )
        {
            _tryTakeNext = tryTakeNext ?? throw new ArgumentNullException(nameof(tryTakeNext));
            _scoreLoader = scoreLoader ?? throw new ArgumentNullException(nameof(scoreLoader));
            _cutoff = (long)Math.Round(threshold * 1000, 0);
            _timeProvider = timeProvider ?? TimeProvider.System;
            _debugLog = debugLog;
            _sourceActive = sourceActive;
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

            bool alreadyWaitedForEmptySource = false;
            while (accepted.Count < quantity)
            {
                token.ThrowIfCancellationRequested();
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
                LogScore(mailItem, score);

                if (score >= _cutoff)
                {
                    accepted.Add(mailItem);
                }
            }

            return accepted;
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
