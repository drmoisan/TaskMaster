using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers
{
    public partial class QfcDatamodel
    {
        //TODO: Implement UndoMove()
        public void UndoMove()
        {
            throw new NotImplementedException();
        }

        internal void TryUnhookOrReplace(ref List<MailItem> nodes, int i)
        {
            if (nodes is null || nodes.Count == 0 || nodes.Count < i + 1)
            {
                logger.Error(
                    $"Error unhooking item from move monitor. No items in array or index out of range. nodes.Length = {nodes?.Count ?? 0} but index i = {i}"
                );
                return;
            }
            var node = nodes[i];
            bool processing = true;
            while (processing)
            {
                try
                {
                    _moveMonitor.UnhookItem(node);
                    processing = false;
                }
                catch (System.Exception e)
                {
                    logger.Error(
                        $"Error unhooking item from move monitor. Getting next item from Queue {e.Message}"
                    );
                    nodes.Remove(node);
                    node = _masterQueue.TryTakeFirst();
                    if (node is null)
                    {
                        processing = false;
                    }
                    else
                    {
                        nodes.Insert(i, node);
                    }
                }
            }
        }

        public async Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut)
        {
            _token.ThrowIfCancellationRequested();

            if (_masterQueue.Count < quantity)
                await WaitForQueue(quantity, _token);

            var nodes = _masterQueue.TryTakeFirst(quantity)?.ToList();
            if (nodes is null)
            {
                return null;
            }

            try
            {
                // The unhook path now self-marshals its Outlook COM access onto the STA thread
                // (EmailMoveMonitor.UnhookItem), so the redundant Task.Run wrapper is removed.
                var max = nodes.Count;
                for (int i = 0; i < max; i++)
                {
                    TryUnhookOrReplace(ref nodes, i);
                    //var node = nodes[i];
                    //_token.ThrowIfCancellationRequested();
                    //bool processing = true;
                    //while (processing)
                    //{
                    //    try
                    //    {
                    //        await _moveMonitor.UnhookItemAsync(node, _token);
                    //        processing = false;
                    //    }
                    //    catch (System.Exception e)
                    //    {
                    //        logger.Error($"Error unhooking item from move monitor. Getting next item from Queue {e.Message}");
                    //        nodes.Remove(node);
                    //        node = _masterQueue.TryTakeFirst();
                    //        if (node is null)
                    //        {
                    //            processing = false;
                    //        }
                    //        else
                    //        {
                    //            nodes.Insert(i, node);
                    //        }
                    //    }
                    //}
                }
            }
            catch (System.Exception e)
            {
                logger.Error("Error unhooking items from move monitor", e);
                throw;
            }

            return nodes;
        }

        public IList<MailItem> DequeueNextItemGroup(int quantity)
        {
            _token.ThrowIfCancellationRequested();

            var nodes = _masterQueue.TryTakeFirst(quantity)?.ToList();
            try
            {
                var max = nodes.Count;
                for (int i = 0; i < max; i++)
                {
                    TryUnhookOrReplace(ref nodes, i);
                }
            }
            catch (System.Exception e)
            {
                logger.Error("Error unhooking items from move monitor", e);
                throw;
            }
            return nodes;
        }

        internal async Task WaitForQueue(int quantity, CancellationToken token)
        {
            while (_worker.IsBusy && (_masterQueue?.Count < quantity))
            {
                token.ThrowIfCancellationRequested();
                await TimeProvider.Delay(TimeSpan.FromMilliseconds(200), token);
            }
        }
    }
}
