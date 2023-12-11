using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.HelperClasses
{
    public class SegmentStopWatch
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SegmentStopWatch() { }

        private Stopwatch _stopwatch = new Stopwatch();
        private long _latestElapsed = 0L;
        private Stack<(string ActionName, long Duration)> _durations = new();

        public void Start() => _stopwatch.Start();

        public void LogDuration(string actionName)
        {
            long duration = _stopwatch.ElapsedMilliseconds - _latestElapsed;
            _durations.Push((actionName, duration));
        }

        public void WriteDurationsToLog()
        {
            var sb = new StringBuilder();
            sb.AppendLine("\nDurations");
            sb.AppendLine("---------");
            while (_durations.Count > 0)
            {
                var (actionName, duration) = _durations.Pop();
                sb.AppendLine($"{actionName}: {duration}");
            }
            logger.Debug(sb.ToString());
            _latestElapsed = 0;
        }

    }
}
