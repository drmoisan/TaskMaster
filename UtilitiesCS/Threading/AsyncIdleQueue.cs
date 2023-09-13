//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UtilitiesCS.Threading
//{
//    public class AsyncIdleQueue
//    {
//        private AsyncIdleQueue()
//        {
//        }
//        private class QueuedTask
//        {
//            public QueuedTask(Task task, Action callback)
//            {
//                _task = task;
//                _callback = callback;
//            }
            
//            public async Task ExecuteAsync()
//            {
//                await _task;
//                if (_callback != null)
//                {
//                    _callback();
//                }
//            }

//            private Task _task;
//            private Action _callback;
//        }
//        static AsyncIdleQueue()
//        {
//            //System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
//            ApplicationIdleTimer.ApplicationIdle += new ApplicationIdleTimer.ApplicationIdleEventHandler(OnApplicationIdle);
//        }
//        public static void AddEntry(Task result, Action callback)
//        {
//            Entries.Enqueue(new QueuedTask(result, callback));
//        }
//        private static ConcurrentQueue<QueuedTask> Entries
//        {
//            get
//            {
//                if (_entries == null)
//                {
//                    _entries = new ConcurrentQueue<QueuedTask>();
//                }
//                return (_entries);
//            }
//        }
//        //private static async void OnApplicationIdle(object sender, EventArgs e)
//        private static async void OnApplicationIdle(ApplicationIdleTimer.ApplicationIdleEventArgs e)
//        {
//            if (e.IdleDuration.TotalSeconds > 2)
//            {
//                if (Entries.TryDequeue(out QueuedTask d))
//                {
//                    try
//                    {
//                        Console.WriteLine($"ApplicationIdle is trying to execute {nameof(QueuedTask)}");
//                        await d.ExecuteAsync();
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"Failed to execute {nameof(QueuedTask)}");
//                        Console.WriteLine(ex.Message);
//                    }
//                }
//            }
//        }
//        private static ConcurrentQueue<QueuedTask> _entries;
//    }
    // Original Function
    //public class AsyncIdleQueue
    //{
    //    private AsyncIdleQueue()
    //    {
    //    }
    //    private class QueuedDetails
    //    {
    //        public QueuedDetails(IAsyncResult result, AsyncCallback callback)
    //        {
    //            _result = result;
    //            _callback = callback;
    //        }
    //        public bool InvokeIfComplete()
    //        {
    //            bool done = _result.IsCompleted;

    //            if (done)
    //            {
    //                _callback(_result);
    //            }
    //            return (done);
    //        }
    //        private IAsyncResult _result;
    //        private AsyncCallback _callback;
    //    }
    //    static AsyncIdleQueue()
    //    {
    //        System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
    //    }
    //    public static void AddEntry(IAsyncResult result, AsyncCallback callback)
    //    {
    //        Entries.Add(new QueuedDetails(result, callback));
    //    }
    //    private static ArrayList Entries
    //    {
    //        get
    //        {
    //            if (_entries == null)
    //            {
    //                _entries = new ArrayList();
    //            }
    //            return (_entries);
    //        }
    //    }
    //    private static void OnApplicationIdle(object sender, EventArgs e)
    //    {
    //        for (int i = 0; i < Entries.Count; ++i)
    //        {
    //            QueuedDetails d = (QueuedDetails)Entries[i];

    //            if (d.InvokeIfComplete())
    //            {
    //                Entries.Remove(d);
    //                --i;
    //            }
    //        }
    //    }
    //    private static ArrayList _entries;
    //}
//}
