using System;
using UtilitiesCS.Interfaces;

namespace UtilitiesCS.Interfaces
{
    public interface ITimerWrapper
    {
        bool AutoReset { get; set; }
        bool Enabled { get; set; }
        TimeSpan Interval { get; set; }
        double IntervalInMilliseconds { get; set; }

        event EventHandler<TimeElapsedEventArgs> Elapsed;

        void Dispose();
        void StartTimer();
        void StopTimer();
    }
}