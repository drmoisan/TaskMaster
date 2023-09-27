using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.Threading;
using UtilitiesCS;
using System.Diagnostics;

namespace QuickFiler
{
    public static class EfcViewerQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Queue<EfcViewer> _queue = new Queue<EfcViewer>();
                
        public static void BuildQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                IdleActionQueue.AddEntry(()=> 
                { 
                    _queue.Enqueue(new EfcViewer());
                    //logger.Debug($"Enqueued {_queue.Count}");
                });
            }
        }
                        
        public static EfcViewer Dequeue()
        {
            EfcViewer viewer = null;
            if (_queue.Count > 0)
            {
                viewer = _queue.Dequeue();
                logger.Debug($"Dequeued 1, {_queue.Count} remaining");
                BuildQueue(1);
                //Debug.WriteLine($"Exiting dequeue, {_queue.Count} remaining");
            }
            else
            {
                viewer = new EfcViewer();
                BuildQueue(2);
                //_ = BuildQueueAsync(2);
            }
            return viewer;
        }

    }
}

