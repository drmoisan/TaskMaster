using log4net.Repository.Hierarchy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Threading
{
    public class IdleActionQueue
    {
        private const int IdleActionDuration = 20;
        private const int GUIActivityThreshold = 700;
        private const double CPUUsageThreshold = 0.15;
        
        private IdleActionQueue() { }

        static IdleActionQueue()
        {
            //System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
            //ApplicationIdleTimer.ApplicationIdle += new ApplicationIdleTimer.ApplicationIdleEventHandler(OnApplicationIdle);
            ApplicationIdleTimer.GUIActivityThreshold = GUIActivityThreshold;
            ApplicationIdleTimer.CPUUsageThreshold = CPUUsageThreshold;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void AddEntry(Action action)
        {
            if (_subscribeGuard.CheckAndSetFirstCall)
            {
                ApplicationIdleTimer.Subscribe(OnApplicationIdle);
                logger.Debug($"{nameof(IdleActionQueue)}.{nameof(AddEntry)} subscribed to {nameof(ApplicationIdleTimer)}");
            }
            Entries.Enqueue(action);
        }

        private static ConcurrentQueue<Action> _entries;
        private static ConcurrentQueue<Action> Entries
        {
            get
            {
                _entries ??= new ConcurrentQueue<Action>();
                return _entries;
            }
        }

        private static ThreadSafeSingleShotGuard _subscribeGuard = new ThreadSafeSingleShotGuard();

        private static TimedBatchAction _unsubscribe = new(TimeSpan.FromSeconds(3), () =>
        {
            ApplicationIdleTimer.Unsubscribe(OnApplicationIdle);
            logger.Debug($"{nameof(IdleActionQueue)} unsubscribed from {nameof(ApplicationIdleTimer)}");
            _subscribeGuard = new ThreadSafeSingleShotGuard();
        });

        private static async void OnApplicationIdle(ApplicationIdleTimer.ApplicationIdleEventArgs e)
        {
            if (e.IdleDuration.TotalMilliseconds > IdleActionDuration)
            {
                if (Entries.TryDequeue(out Action action))
                {
                    _unsubscribe.CancelAction();
                    try
                    {
                        await UiThread.Dispatcher.InvokeAsync(() => action());
                        //await UIThreadExtensions.UiSyncContext;
                        //action();
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to execute {nameof(action)}");
                        logger.Error(ex.Message);
                    }
                }
                else
                {
                    _unsubscribe.RequestAction();
                }
            }
        }
    }
}
