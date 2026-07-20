#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UtilitiesCS.Interfaces;

namespace UtilitiesCS.HelperClasses
{
    public class TimerWrapper : ITimerWrapper
    {
        /// <summary>
        /// Minimal internal abstraction over the underlying <see cref="System.Timers.Timer"/> that
        /// <see cref="TimerWrapper"/> consumes. This is an INTERNAL implementation seam only: it is
        /// not part of the public <see cref="IGenericTimer"/>/<see cref="ITimerWrapper"/> contract.
        /// It exposes exactly the members the wrapper reads/writes on its timer field, so a test can
        /// inject a deterministic manual-fire fake instead of a real OS timer.
        /// </summary>
        internal interface IInnerTimer : IDisposable
        {
            bool AutoReset { get; set; }
            bool Enabled { get; set; }
            double Interval { get; set; }
            event ElapsedEventHandler Elapsed;
            void Start();
            void Stop();
        }

        /// <summary>
        /// Production adapter implementing <see cref="IInnerTimer"/> by wrapping a real
        /// <see cref="System.Timers.Timer"/> and forwarding every member 1:1. This preserves the
        /// exact runtime behavior of the prior direct field usage.
        /// </summary>
        internal sealed class SystemTimersTimerAdapter : IInnerTimer
        {
            private readonly System.Timers.Timer timer;

            public SystemTimersTimerAdapter(TimeSpan interval)
            {
                this.timer = new System.Timers.Timer(interval.TotalMilliseconds)
                {
                    Enabled = false,
                };
            }

            public bool AutoReset
            {
                get => this.timer.AutoReset;
                set => this.timer.AutoReset = value;
            }

            public bool Enabled
            {
                get => this.timer.Enabled;
                set => this.timer.Enabled = value;
            }

            public double Interval
            {
                get => this.timer.Interval;
                set => this.timer.Interval = value;
            }

            public event ElapsedEventHandler Elapsed
            {
                add => this.timer.Elapsed += value;
                remove => this.timer.Elapsed -= value;
            }

            public void Start() => this.timer.Start();

            public void Stop() => this.timer.Stop();

            public void Dispose() => this.timer.Dispose();
        }

        public TimerWrapper(TimeSpan interval)
            : this(new SystemTimersTimerAdapter(interval)) { }

        /// <summary>
        /// Internal constructor for test injection of a deterministic inner timer. The public
        /// constructor delegates here with the real <see cref="System.Timers.Timer"/> adapter, so
        /// both paths perform identical <see cref="Elapsed"/> wiring.
        /// </summary>
        internal TimerWrapper(IInnerTimer innerTimer)
        {
            this.timer = innerTimer ?? throw new ArgumentNullException(nameof(innerTimer));
            this.timer.Elapsed += this.WhenTimerElapsed;
        }

        public static TimerWrapper StartNew(TimeSpan interval, bool autoReset, Action callback)
        {
            var timer = new TimerWrapper(interval);
            timer.AutoReset = autoReset;
            timer.Elapsed += (sender, args) => callback();
            timer.StartTimer();
            return timer;
        }

        /// <summary>
        /// Internal StartNew overload that accepts a pre-built inner timer so a test can drive the
        /// AutoReset + callback contract deterministically through the inner seam. Mirrors the public
        /// <see cref="StartNew(TimeSpan, bool, Action)"/> behavior apart from the injected inner timer.
        /// </summary>
        internal static TimerWrapper StartNew(
            IInnerTimer innerTimer,
            bool autoReset,
            Action callback
        )
        {
            var timer = new TimerWrapper(innerTimer);
            timer.AutoReset = autoReset;
            timer.Elapsed += (sender, args) => callback();
            timer.StartTimer();
            return timer;
        }

        private readonly IInnerTimer timer;
        private bool disposed = false;

        public event EventHandler<TimeElapsedEventArgs>? Elapsed;

        public bool AutoReset
        {
            get => this.timer.AutoReset;
            set => this.timer.AutoReset = value;
        }
        public bool Enabled
        {
            get => this.timer.Enabled;
            set => this.timer.Enabled = value;
        }

        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(this.timer.Interval);
            set => this.timer.Interval = value.TotalMilliseconds;
        }

        public double IntervalInMilliseconds
        {
            get => this.timer.Interval;
            set => this.timer.Interval = value;
        }

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

        public void ResetTimer()
        {
            this.timer.Stop();
            this.timer.Start();
        }

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
