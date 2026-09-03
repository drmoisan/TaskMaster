using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers
{
    internal sealed class QfcRemainingQueueAdmission
    {
        private readonly Action<MailItem> _addToQueue;
        private readonly Action<MailItem, Action<MailItem>> _hookItem;
        private readonly Action<MailItem> _removeFromQueue;

        internal QfcRemainingQueueAdmission(
            Action<MailItem> addToQueue,
            Action<MailItem, Action<MailItem>> hookItem,
            Action<MailItem> removeFromQueue
        )
        {
            _addToQueue = addToQueue ?? throw new ArgumentNullException(nameof(addToQueue));
            _hookItem = hookItem ?? throw new ArgumentNullException(nameof(hookItem));
            _removeFromQueue =
                removeFromQueue ?? throw new ArgumentNullException(nameof(removeFromQueue));
        }

        internal Task<bool> TryQueueAsync(MailItem mailItem, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (mailItem is null)
            {
                return Task.FromResult(false);
            }

            _addToQueue(mailItem);
            _hookItem(mailItem, _removeFromQueue);
            return Task.FromResult(true);
        }
    }
}
