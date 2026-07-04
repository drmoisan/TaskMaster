using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

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
                var listObjects = await _datamodel.DequeueNextItemGroupAsync(
                    _formController.ItemsPerIteration,
                    2000
                );
                if (listObjects.Count > 0)
                {
                    //await UiThread.Dispatcher.InvokeAsync(async () => await QfcQueue.EnqueueAsync(listObjects, _formController.Groups));
                    await QfcQueue
                        .EnqueueAsync(listObjects, _formController.Groups)
                        .ConfigureAwait(false);
                }
                else
                {
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
