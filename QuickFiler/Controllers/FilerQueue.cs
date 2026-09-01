using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    public class FilerQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        internal BlockingCollection<FilerQueueItem> Queue { get; private set; } = [];

        /// <summary>
        /// Serializes the outstanding-work counter, the queue add, and the worker start/stop decision,
        /// so that an enqueue and the "is a worker running" question are atomic with respect to the
        /// worker's loop exit. Never held across an <c>await</c>.
        /// </summary>
        private readonly object _sync = new object();

        /// <summary>
        /// Count of items enqueued but not yet finished processing. Incremented in <c>Enqueue</c> and
        /// decremented after each item on every exit path, so a throwing item still decrements.
        /// </summary>
        private int _outstanding;

        /// <summary>
        /// Drain signal, null while the queue is idle. Created lazily by <see cref="WhenDrainedAsync"/>
        /// when work is outstanding, then completed and cleared when the counter reaches zero.
        /// </summary>
        private TaskCompletionSource<bool> _drainSignal;

        /// <summary>
        /// True while a worker is draining the queue. Set when a worker is started and cleared in the
        /// same critical section in which <c>TryTake</c> fails, which is what closes the orphaned-item
        /// window that the previous one-shot start gate left open.
        /// </summary>
        private bool _consumerRunning;

        public void Enqueue(FilerQueueItem item)
        {
            bool startWorker;

            lock (_sync)
            {
                _outstanding++;
                Queue.Add(item);
                startWorker = !_consumerRunning;
                if (startWorker)
                {
                    _consumerRunning = true;
                }
            }

            if (startWorker)
            {
                Consumer = ConsumeAsync();
            }
        }

        public void Enqueue(EmailFiler filer, IList<MailItemHelper> helpers)
        {
            // Constructed in this frame deliberately: it is what keeps a null or null-containing helper
            // list surfacing as a synchronous ArgumentNullException to the caller.
            Enqueue(new FilerQueueItem(filer, helpers));
        }

        public Task Consumer { get; private set; } = Task.CompletedTask;

        /// <summary>
        /// Per-item processing seam introduced for issue 633. The production default forwards to
        /// <see cref="EmailFiler.SortAsync(IList{MailItemHelper})"/>, so production behaviour is
        /// unchanged by the presence of this seam.
        /// </summary>
        /// <remarks>
        /// Tests assign a fake so that no live Outlook COM call is made. That substitution is what
        /// makes the queue's concurrency assertions deterministic: the real
        /// <see cref="EmailFiler.SortAsync(IList{MailItemHelper})"/> is non-virtual and casts to a COM
        /// folder, so it cannot be driven from a unit test.
        /// </remarks>
        internal Func<FilerQueueItem, Task> ItemProcessor { get; set; } =
            item => item.Filer.SortAsync(item.Helpers);

        /// <summary>
        /// Completes when the queue has no outstanding work. Returns an already-completed task when
        /// nothing is outstanding at the moment of the call. Introduced for issue 633 so that the
        /// batch-move path can express its ordering dependency on the queue's filing work as a
        /// control-flow property.
        /// </summary>
        /// <remarks>
        /// Idempotent, and safe to call and await repeatedly or concurrently: every returned task
        /// completes when the count next reaches zero, and no caller can starve another. The returned
        /// task completes and never faults, so an item failure that the worker logs is not converted
        /// into an unhandled exception on the batch-move path.
        /// </remarks>
        public Task WhenDrainedAsync()
        {
            lock (_sync)
            {
                if (_outstanding == 0)
                {
                    return Task.CompletedTask;
                }

                _drainSignal ??= new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                return _drainSignal.Task;
            }
        }

        public async Task ConsumeAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    FilerQueueItem item;

                    lock (_sync)
                    {
                        // Clearing the flag in the same critical section in which TryTake fails is what
                        // closes the orphaned-item window: a producer cannot observe "a worker is
                        // running" after this worker has decided to stop.
                        if (!Queue.TryTake(out item))
                        {
                            _consumerRunning = false;
                            return;
                        }
                    }

                    try
                    {
                        await ItemProcessor(item);
                    }
                    catch (Exception e)
                    {
                        var first = item.Helpers.First();
                        logger.Error(
                            $"Error sorting mail items Subject: {first.Subject} Sent On: {first.SentOn} from {first.SenderName} {e.Message}",
                            e
                        );
                    }
                    finally
                    {
                        CompleteItem();
                    }
                }
            });
        }

        /// <summary>
        /// Decrements the outstanding-work counter and, when it reaches zero, completes and clears the
        /// drain signal. The signal is captured under the monitor and completed outside it.
        /// </summary>
        private void CompleteItem()
        {
            TaskCompletionSource<bool> signal = null;

            lock (_sync)
            {
                _outstanding--;
                if (_outstanding == 0 && _drainSignal is not null)
                {
                    signal = _drainSignal;
                    _drainSignal = null;
                }
            }

            signal?.TrySetResult(true);
        }
    }

    public class FilerQueueItem
    {
        public FilerQueueItem(EmailFiler filer, IList<MailItemHelper> helpers)
        {
            Filer = filer.ThrowIfNull();
            Helpers = helpers.ThrowIfNull();
            if (helpers.Any(h => h is null))
            {
                throw new ArgumentNullException("Helpers cannot contain null values");
            }
        }

        public EmailFiler Filer { get; private set; }
        public IList<MailItemHelper> Helpers { get; private set; }
    }
}
