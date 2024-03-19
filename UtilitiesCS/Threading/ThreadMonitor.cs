using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.Threading
{
    public class ThreadMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Thread thread;
        private readonly int pollingFrequency;
        private readonly int delayThreshold;
        private readonly int stackTraceIterations;

        public ThreadMonitor(Thread thread, int pollingFrequency = 500, int delayThreshold = 100, int stackTraceIterations = 4)
        {
            this.thread = thread;
            this.pollingFrequency = pollingFrequency;
            this.delayThreshold = delayThreshold;
            this.stackTraceIterations = stackTraceIterations;
        }

        public void Run()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(pollingFrequency);
                    var dispatcher = Dispatcher.FromThread(thread);
                    if (dispatcher is null)
                        UiThread.UiSyncContext.Send((x) => dispatcher = Dispatcher.CurrentDispatcher, null);        
                
                    var task = dispatcher.InvokeAsync(() => { });

                    for (var i = 0; i < stackTraceIterations; i++)
                    {
                        Thread.Sleep(delayThreshold);
                        if (task.Status != DispatcherOperationStatus.Completed)
                        {
                            Debug.WriteLine($"{(i + 1) * 100}ms Delay on thread {thread.Name} ({task.Status})");
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (task.Status == DispatcherOperationStatus.Completed)
                        continue;

                    var stackTrace = GetStackTrace(thread);
                    Log.Debug($"StackTrace of UI Thread: {stackTrace}");
                    Debug.WriteLine($"StackTrace of UI Thread: {stackTrace}");
                }
            });
        }

#pragma warning disable 0618
        private StackTrace GetStackTrace(Thread targetThread)
        {
            StackTrace stackTrace = null;
            var ready = new ManualResetEventSlim();

            new Thread(() =>
            {
                // Backstop to release thread in case of deadlock:
                ready.Set();
                Thread.Sleep(200);
                try { targetThread.Resume(); } catch { }
            }).Start();

            ready.Wait();
            targetThread.Suspend();
            try { stackTrace = new StackTrace(targetThread, true); }
            catch { /* Deadlock */ }
            finally
            {
                try { targetThread.Resume(); }
                catch { stackTrace = null;  /* Deadlock */  }
            }

            return stackTrace;
        }
#pragma warning restore 0618
    }
}
