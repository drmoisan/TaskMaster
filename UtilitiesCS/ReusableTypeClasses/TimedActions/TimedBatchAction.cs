using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Threading;

namespace UtilitiesCS.HelperClasses
{
    public class TimedBatchAction
    {
        public TimedBatchAction(TimeSpan frequency)
            : this(frequency, null, CreateTimer)
        {
        }

        public TimedBatchAction(TimeSpan frequency, System.Action action)
            : this(frequency, action, CreateTimer)
        {
        }

        internal TimedBatchAction(TimeSpan frequency, System.Action action, Func<TimeSpan, ITimerWrapper> timerFactory)
        {
            _frequency = frequency;
            _action = action;
            _timerFactory = timerFactory ?? throw new ArgumentNullException(nameof(timerFactory));
        }

        public void SetAction(System.Action action)
        {
            Interlocked.CompareExchange(ref _action, action, null);
        }

        private System.Action _action;

        private TimeSpan _frequency;
        private readonly Func<TimeSpan, ITimerWrapper> _timerFactory;
        private ThreadSafeSingleShotGuard _actionRequested = new();
        private ITimerWrapper _timer;

        public void ResetTimer()
        {
            _timer?.ResetTimer();
        }

        public void CancelAction()
        {
            _timer?.StopTimer();
            _actionRequested = new();
        }

        public void RequestAction()
        {
            if (_actionRequested.CheckAndSetFirstCall)
            {
                if (_action is null)
                {
                    throw new NullReferenceException("Action is null");
                }

                ScheduleAction(_action);
            }
        }

        public void RequestAction(System.Action action)
        {
            if (_actionRequested.CheckAndSetFirstCall)
            {
                if (action is null)
                {
                    throw new NullReferenceException("Action is null");
                }

                ScheduleAction(action);
            }
        }

        private void ScheduleAction(System.Action action)
        {
            var actionToRun = ResetAfterAction(action);
            _timer = _timerFactory(_frequency);
            _timer.AutoReset = false;
            _timer.Elapsed += (sender, e) => actionToRun();
            _timer.StartTimer();
        }

        private System.Action ResetAfterAction(System.Action action)
        {
            return () =>
            {
                action();
                _actionRequested = new();
            };
        }

        private static ITimerWrapper CreateTimer(TimeSpan frequency) => new TimerWrapper(frequency);
    }
}
