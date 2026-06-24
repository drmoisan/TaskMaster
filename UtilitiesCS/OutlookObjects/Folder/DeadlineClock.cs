using System;
using System.Diagnostics;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Uses monotonic elapsed time to decide when folder tree work should yield.
    /// </summary>
    public sealed class DeadlineClock : IDeadlineClock
    {
        private readonly TimeSpan _yieldInterval;
        private readonly Stopwatch _stopwatch;

        public DeadlineClock(TimeSpan yieldInterval)
        {
            if (yieldInterval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(yieldInterval));
            }

            _yieldInterval = yieldInterval;
            _stopwatch = Stopwatch.StartNew();
        }

        public bool ShouldYield()
        {
            return _stopwatch.Elapsed >= _yieldInterval;
        }

        public void Reset()
        {
            _stopwatch.Restart();
        }
    }
}
