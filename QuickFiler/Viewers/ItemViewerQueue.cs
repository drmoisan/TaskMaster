using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static void BuildQueueWhenIdle(int count)
        {
            for (int i = 0; i < count; i++)
            {
                IdleActionQueue.AddEntry(() =>
                {
                    _queue.Enqueue(new QfcItemViewer());
                    Console.WriteLine($"Enqueued {_queue.Count}");
                });
            }
        }

        public static void BuildQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _queue.Enqueue(new QfcItemViewer());
                Console.WriteLine($"Enqueued {_queue.Count}");
            }
        }

        public static QfcItemViewer Dequeue()
        {
            QfcItemViewer viewer = null;
            if (_queue.Count > 0)
            {
                viewer = _queue.Dequeue();
                Debug.WriteLine($"Dequeued 1, {_queue.Count} remaining");
                BuildQueueWhenIdle(1);
                //Debug.WriteLine($"Exiting dequeue, {_queue.Count} remaining");
            }
            else
            {
                viewer = new QfcItemViewer();
                BuildQueueWhenIdle(10);
            }
            return viewer;
        }

        public static IEnumerable<QfcItemViewer> DequeueChunk(int count)
        {
            if (_queue.Count < count)
            {
                BuildQueue(count - _queue.Count);
            }
            BuildQueueWhenIdle(count);
            return _queue.DequeueChunk(count);
        }

    }
}
