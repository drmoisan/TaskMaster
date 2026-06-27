using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal sealed class QfcRemainingQueueAdmission
    {
        private readonly IApplicationGlobals _globals;
        private readonly Func<MailItem, CancellationToken, Task<long>> _scoreLoader;
        private readonly Action<MailItem> _addToQueue;
        private readonly Action<MailItem, Action<MailItem>> _hookItem;
        private readonly Action<MailItem> _removeFromQueue;

        internal QfcRemainingQueueAdmission(
            IApplicationGlobals globals,
            Func<MailItem, CancellationToken, Task<long>> scoreLoader,
            Action<MailItem> addToQueue,
            Action<MailItem, Action<MailItem>> hookItem,
            Action<MailItem> removeFromQueue
        )
        {
            _globals = globals;
            _scoreLoader = scoreLoader ?? throw new ArgumentNullException(nameof(scoreLoader));
            _addToQueue = addToQueue ?? throw new ArgumentNullException(nameof(addToQueue));
            _hookItem = hookItem ?? throw new ArgumentNullException(nameof(hookItem));
            _removeFromQueue =
                removeFromQueue ?? throw new ArgumentNullException(nameof(removeFromQueue));
        }

        internal async Task<bool> TryQueueAsync(MailItem mailItem, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (mailItem is null)
            {
                return false;
            }

            if (_globals?.QfSettings?.HighConfidenceModeEnabled == true)
            {
                long cutoff = (long)
                    Math.Round(_globals.QfSettings.HighConfidenceThreshold * 1000, 0);
                long score = await _scoreLoader(mailItem, cancel).ConfigureAwait(false);
                if (score < cutoff)
                {
                    return false;
                }
            }

            _addToQueue(mailItem);
            _hookItem(mailItem, _removeFromQueue);
            return true;
        }
    }
}
