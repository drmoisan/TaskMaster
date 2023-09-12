using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler
{
    public static class ItemViewerQueue
    {
        //public ItemViewerQueue()
        //{
        //    _queue = new Queue<QfcItemViewer>();
        //    _queueSize = Properties.Settings.Default.ItemViewerQueueSize;
        //    _ = BuildQueueAsync(_queueSize);
        //}

        //public ItemViewerQueue(int queueSize)
        //{
        //    _queue = new Queue<QfcItemViewer>();
        //    _queueSize = queueSize;
        //    _ = BuildQueueAsync(_queueSize);
        //}

        private static Queue<QfcItemViewer> _queue = new Queue<QfcItemViewer>();

        public async static Task BuildQueueAsync(int count)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(EnqueueAsync());
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        
        internal async static Task EnqueueAsync() 
        {             
            await TaskPriority.Run(()=>_queue.Enqueue(new QfcItemViewer()), PriorityScheduler.Lowest);
        }

        public static QfcItemViewer Dequeue()
        {
            QfcItemViewer viewer = null;
            if (_queue.Count > 0)
            {
                viewer = _queue.Dequeue();
                _ = EnqueueAsync();
            }
            else 
            { 
                viewer = new QfcItemViewer();
                _ = BuildQueueAsync(10);
            }
            return _queue.Dequeue();
        }

        public async static Task<IEnumerable<QfcItemViewer>> DequeueAsync(int count)
        {
            if (count > _queue.Count)
            {
                await BuildQueueAsync(count - _queue.Count);
            }
            _ = BuildQueueAsync(count);
            return _queue.DequeueChunk(count);
        }
    }
}
