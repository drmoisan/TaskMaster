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
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Queue<QfcItemViewer> _queue = new Queue<QfcItemViewer>();

        public static void BuildQueueWhenIdle(int count)
        {
            for (int i = 0; i < count; i++)
            {
                IdleActionQueue.AddEntry(() =>
                {
                    _queue.Enqueue(new QfcItemViewer());
                    logger.Debug($"Enqueued {_queue.Count}");
                });
            }
        }

        public static void BuildQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _queue.Enqueue(new QfcItemViewer());
                logger.Debug($"Enqueued {_queue.Count}");
            }
        }

        public static QfcItemViewer Dequeue()
        {
            QfcItemViewer viewer = null;
            if (_queue.Count > 0)
            {
                viewer = _queue.Dequeue();
                logger.Debug($"Dequeued 1, {_queue.Count} remaining");
                BuildQueueWhenIdle(1);
                
            }
            else
            {
                viewer = new QfcItemViewer();
                BuildQueueWhenIdle(1);
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
