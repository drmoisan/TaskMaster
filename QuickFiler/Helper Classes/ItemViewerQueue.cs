using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler
{
    public static class ItemViewerQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Queue<ItemViewer> _queue = new Queue<ItemViewer>();

        public static void BuildQueueWhenIdle(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _ = UIThreadExtensions.UiDispatcher.InvokeAsync(
                    () =>
                    {
                        _queue.Enqueue(new ItemViewer());
                        //logger.Debug($"Enqueued {_queue.Count}");
                    },
                    System.Windows.Threading.DispatcherPriority.ContextIdle);
                
                // IdleActionQueue implementation
                //IdleActionQueue.AddEntry(() =>
                //{
                //    _queue.Enqueue(new ItemViewer());
                //    //logger.Debug($"Enqueued {_queue.Count}");
                //});
            }
        }

        public static void BuildQueueBackground(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _ = UIThreadExtensions.UiDispatcher.InvokeAsync(
                    () =>
                    {
                        _queue.Enqueue(new ItemViewer());
                        //logger.Debug($"Enqueued {_queue.Count}");
                    }, 
                    System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        public static void BuildQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _queue.Enqueue(new ItemViewer());
                //logger.Debug($"Enqueued {_queue.Count}");
            }
        }

        public static ItemViewer Dequeue(CancellationToken token)
        {
            ItemViewer viewer = null;
            if (_queue.Count > 0)
            {
                viewer = _queue.Dequeue();
                //logger.Debug($"Dequeued 1, {_queue.Count} remaining");
                BuildQueueWhenIdle(1);
                
            }
            else
            {
                viewer = UIThreadExtensions.UiDispatcher.Invoke(() => new ItemViewer(), DispatcherPriority.Render);
                BuildQueueWhenIdle(1);
            }
            return viewer;
        }

        public static IEnumerable<ItemViewer> DequeueChunk(int count)
        {
            var countOriginal = _queue.Count;
            if (countOriginal < count)
            {
                UIThreadExtensions.UiDispatcher.Invoke(() => BuildQueue(count - countOriginal), DispatcherPriority.Render);
            }
            BuildQueueWhenIdle(countOriginal);
            return _queue.DequeueChunk(count);
        }

    }
}
