using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers
{
    public partial class QfcHomeController
    {
        public async Task IterateQueueAsync()
        {
            Token.ThrowIfCancellationRequested();

            if (_datamodel.Complete)
            {
                return;
            }
            try
            {
                QfcDequeueBatch batch = await _datamodel.DequeueNextItemGroupWithOutcomeAsync(
                    _formController.ItemsPerIteration,
                    2000,
                    QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
                    null
                );
                IList<MailItem> listObjects = batch.Items;
                if (listObjects.Count > 0)
                {
                    //await UiThread.Dispatcher.InvokeAsync(async () => await QfcQueue.EnqueueAsync(listObjects, _formController.Groups));
                    await QfcQueue
                        .EnqueueAsync(listObjects, _formController.Groups)
                        .ConfigureAwait(false);
                }
                else if (batch.Stop == QfcDequeueStop.SourceExhausted)
                {
                    // Issue #446. Only genuine source exhaustion may close the queue:
                    // CompleteAddingAsync reaches BlockingCollection<T>.CompleteAdding(), which is
                    // irreversible. An empty batch whose stop reason is DeadlineExpired or
                    // QuantitySatisfied leaves the queue open so a later iteration can drain the
                    // items the master queue still holds.
                    //logger.Debug($"{nameof(IterateQueueAsync)} completed");
                    await QfcQueue.CompleteAddingAsync(Token, 10000);
                }
            }
            catch (OperationCanceledException)
            {
                //logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
            }
            catch (System.Exception)
            {
                if (this.Token.IsCancellationRequested)
                {
                    //logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
                }
                else
                {
                    throw;
                }
            }
        }

        public void Iterate()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            bool highConfidenceModeEnabled = Globals?.QfSettings?.HighConfidenceModeEnabled == true;
            IList<MailItem> listObjects = highConfidenceModeEnabled
                ? _datamodel
                    .DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000)
                    .GetAwaiter()
                    .GetResult()
                : _datamodel.DequeueNextItemGroup(_formController.ItemsPerIteration);
            _formController.LoadItems(listObjects);
        }

        public void Iterate2()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            (var tlp, var itemGroups) = QfcQueue.Dequeue();
            _formController.LoadItems(tlp, itemGroups);
            _ = IterateQueueAsync();
        }

        public void SwapStopWatch()
        {
            _stopWatchMoved = _stopWatch;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }
    }
}
