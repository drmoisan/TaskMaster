using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Threading
{
    public class IdleAsyncQueue
    {
        private const int IdleActionDuration = 20;
        private const int GUIActivityThreshold = 700;
        private const double CPUUsageThreshold = 0.15;

        private IdleAsyncQueue() { }

        static IdleAsyncQueue()
        {
            //System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
            ApplicationIdleTimer.ApplicationIdle += new ApplicationIdleTimer.ApplicationIdleEventHandler(OnApplicationIdle);
            ApplicationIdleTimer.GUIActivityThreshold = GUIActivityThreshold;
            ApplicationIdleTimer.CPUUsageThreshold = CPUUsageThreshold;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void AddEntry(bool useUiThread, Func<Task> actionAsync)
        {
            Entries.Enqueue((useUiThread, actionAsync));
        }

        private static ConcurrentQueue<(bool UiThread, Func<Task> AsyncAction)> Entries { get; } = new();

        private static async void OnApplicationIdle(ApplicationIdleTimer.ApplicationIdleEventArgs e)
        {
            if (e.IdleDuration.TotalMilliseconds > IdleActionDuration)
            {
                if (Entries.TryDequeue(out (bool useUiThread, Func<Task> actionAsync) entry))
                {
                    try
                    {
                        if (entry.useUiThread)
                            await UiThread.Dispatcher.InvokeAsync(entry.actionAsync);
                        else
                            await entry.actionAsync();
                        
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to execute {nameof(entry.actionAsync)}");
                        logger.Error(ex.Message);
                    }
                }
            }
        }
    }
}
