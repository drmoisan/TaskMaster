using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilitiesCS.Interfaces;
using System.Timers;

namespace UtilitiesCS.HelperClasses
{
    public class TimerWrapper : IGenericTimer, ITimerWrapper
    {
        public TimerWrapper(TimeSpan interval)
        {
            this.timer = new System.Timers.Timer(interval.TotalMilliseconds) { Enabled = false };
            this.timer.Elapsed += this.WhenTimerElapsed;
        }

        private readonly System.Timers.Timer timer;
        private bool disposed = false;

        public event EventHandler<TimeElapsedEventArgs> Elapsed;

        public bool AutoReset { get => this.timer.AutoReset; set => this.timer.AutoReset = value; }
        public bool Enabled { get => this.timer.Enabled; set => this.timer.Enabled = value; }

        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(this.timer.Interval);
            set => this.timer.Interval = value.TotalMilliseconds;
        }

        public double IntervalInMilliseconds { get => this.timer.Interval; set => this.timer.Interval = value; }

        private void WhenTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var handler = this.Elapsed;
            if (handler != null)
            {
                handler(this, new TimeElapsedEventArgs(elapsedEventArgs.SignalTime));
            }
        }

        public void StartTimer() => this.timer.Start();

        public void StopTimer() => this.timer.Stop();

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.timer.Elapsed -= this.WhenTimerElapsed;
                    this.timer.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}
