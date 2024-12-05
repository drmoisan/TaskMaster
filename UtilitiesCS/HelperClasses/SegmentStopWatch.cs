using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace UtilitiesCS.HelperClasses
{
    public class SegmentStopWatch: Stopwatch
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SegmentStopWatch(): base() 
        { 
            if (UiThread.UiThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                
                logger.Warn($"SegmentStopWatch created on UI thread {Thread.CurrentThread.ManagedThreadId}" +
                    $"\nStackTrace: {string.Join(" => ",TraceUtility.GetMyMethodNames(new StackTrace()))}");
            }
        }

        private TimeSpan _latestElapsed = default;
        
        private Stack<(string ActionName, TimeSpan Duration)> _durations = new();
        public Stack<(string ActionName, TimeSpan Duration)> Durations => _durations;

        public new SegmentStopWatch Start() 
        { 
            base.Start();
            return this;
        }
        
        public new SegmentStopWatch Stop()
        {
            base.Stop();
            return this;
        }
                
        public void LogDuration(string actionName)
        {
            TimeSpan duration = this.Elapsed - _latestElapsed;
            _latestElapsed = this.Elapsed;
            _durations.Push((actionName, duration));
        }

        public void LogDuration(string actionName, bool logImmediately)
        {
            TimeSpan duration = this.Elapsed - _latestElapsed;
            _latestElapsed = this.Elapsed;
            _durations.Push((actionName, duration));
            if (logImmediately)
            {
                //logger.Debug($"{actionName} duration was {duration:%m\\:ss\\.ff}");
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
            _durations.Push(("TOTAL", this.Elapsed));
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

        public void MergeDurations(Stack<(string ActionName, TimeSpan Duration)> durations)
        {
            _durations = _durations
                .Reverse()
                .Concat(durations)
                .GroupBy(x => x.ActionName)
                .Select(group =>
                {
                    var actionName = group.Key;
                    var duration = TimeSpan.FromTicks(group.Sum(x => x.Duration.Ticks));
                    return (ActionName: actionName, Duration: duration);
                })
                .ToStack();
        }
        
        public static Stack<(string ActionName, TimeSpan Duration)> GroupDurations(
                       Stack<(string ActionName, TimeSpan Duration)> d1,
                       Stack<(string ActionName, TimeSpan Duration)> d2)
        {
            return d1
                .Concat(d2)
                .GroupBy(x => x.ActionName)
                .Select(group =>
                {
                    var actionName = group.Key;
                    var duration = TimeSpan.FromTicks(group.Sum(x => x.Duration.Ticks));
                    return (ActionName: actionName, Duration: duration);
                })
                .ToStack(); 
        }
    }
}
