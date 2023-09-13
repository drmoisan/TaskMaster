using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Threading
{
    public class IdleActionQueue
    {
        private IdleActionQueue() { }

        static IdleActionQueue()
        {
            //System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
            ApplicationIdleTimer.ApplicationIdle += new ApplicationIdleTimer.ApplicationIdleEventHandler(OnApplicationIdle);
            ApplicationIdleTimer.GUIActivityThreshold = 500;
        }
        public static void AddEntry(Action action)
        {
            Entries.Enqueue(action);
        }

        private static ConcurrentQueue<Action> _entries;
        private static ConcurrentQueue<Action> Entries
        {
            get
            {
                if (_entries == null)
                {
                    _entries = new ConcurrentQueue<Action>();
                }
                return (_entries);
            }
        }

        private static async void OnApplicationIdle(ApplicationIdleTimer.ApplicationIdleEventArgs e)
        {
            if (e.IdleDuration.TotalMilliseconds > 50)
            {
                if (Entries.TryDequeue(out Action action))
                {
                    try
                    {
                        await UIThreadExtensions.GetUiContext();
                        Console.WriteLine($"ApplicationIdle is trying to execute {nameof(action)}");
                        action();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to execute {nameof(action)}");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
