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
        private TimeSpan _latestElapsed = default;
        
        private Stack<(string ActionName, TimeSpan Duration)> _durations = new();
        public Stack<(string ActionName, TimeSpan Duration)> Durations => _durations;

        public SegmentStopWatch Start() 
        { 
            _stopwatch.Start(); 
            return this;
        }
        
        public SegmentStopWatch Stop()
        {
            _stopwatch.Stop();
            return this;
        }

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public void LogDuration(string actionName)
        {
            TimeSpan duration = _stopwatch.Elapsed - _latestElapsed;
            _latestElapsed = _stopwatch.Elapsed;
            _durations.Push((actionName, duration));
        }

        public void LogDuration(string actionName, bool logImmediately)
        {
            TimeSpan duration = _stopwatch.Elapsed - _latestElapsed;
            _latestElapsed = _stopwatch.Elapsed;
            _durations.Push((actionName, duration));
            if (logImmediately)
            {
                logger.Debug($"{actionName} duration was {duration:%m\\:ss\\.ff}");
            }
        }

        public Stack<(string ActionName, TimeSpan Duration)> GroupByActionName(bool inplace = false) 
        {
            var grouped = _durations
                .Reverse()
                .GroupBy(x => x.ActionName)
                .Select(group => 
                { 
                    var actionName = group.Key;
                    var duration = TimeSpan.FromTicks(group.Sum(x => x.Duration.Ticks));
                    return (ActionName: actionName, Duration: duration);
                })
                .ToStack();
            if (inplace)
            {
                _durations = grouped;
                return null;
            }
            else
            {
                return grouped;
            }
        }

        public string GetDurations([CallerMemberName] string methodName = "")
        {
            _durations.Push(("TOTAL", _stopwatch.Elapsed));
            var durs = _durations
                .Reverse()
                .Select(x => new[] 
                //{ x.Duration.ToString("c"), x.ActionName })
                { x.Duration.ToString("%m\\:ss\\.ff"), x.ActionName })
                .ToArray();

            var text = durs.ToFormattedText(
                ["Duration", "Action"],
                [Enums.Justification.Right, Enums.Justification.Left],
                $"SEGMENT DURATIONS {methodName.ToUpper()}");
            return text;
        }

        public void WriteToLog([CallerMemberName] string methodName = "", bool clear = true)
        {
            var text = GetDurations(methodName);
            if (clear) 
            { _durations.Clear(); }
            logger.Info($"\n{text}");
        }
    }
}
