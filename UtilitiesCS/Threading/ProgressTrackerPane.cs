using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.TaskPane;

namespace UtilitiesCS
{
    public class ProgressTrackerPane : IProgress<(int Value, string JobName)>
    {
        public ProgressTrackerPane(CancellationTokenSource tokenSource)
        {
            UiThread.Dispatcher.Invoke(() =>
            {
                _progressViewer = new ProgressPane();
                _progressViewer.UiDispatcher = UiThread.Dispatcher;
                _progressViewer.SetCancellationTokenSource(tokenSource);
            });

            var rootProgress = new Progress<(int value, string jobName)>(tup =>
            {
                _progressViewer.Bar.Value = tup.value;
                _progressViewer.JobName.Text = tup.jobName;
                _progressViewer.Refresh();
            });
            _parent = new ParentProgress<(int Value, string JobName)>(rootProgress, 100, 0);

            this.Report(0, "Initializing");
            _isRoot = true;

            if (_progressViewer.InvokeRequired)
                _progressViewer.Invoke(() => _progressViewer.Show());
            else
                _progressViewer.Show();
        }

        public ProgressTrackerPane(ProgressTrackerPane parent, int allocation, int startingAt)
        {
            _parent = new ParentProgress<(int Value, string JobName)>(parent, allocation, startingAt);
            _jobName = parent._jobName;
            _progressViewer = parent.ProgressViewer;
        }

        //public ProgressTracker(IProgress<(int Value, string JobName)> parent, int allocation, int startingAt)
        //{
        //    _parent = new ParentProgress<(int Value, string JobName)>(parent, allocation, startingAt);
        //}

        protected string _jobName;
        private bool _isRoot = false;
        private bool _root100 = false;
        private ParentProgress<(int Value, string JobName)> _parent;

        private ProgressPane _progressViewer;
        public ProgressPane ProgressViewer { get => _progressViewer; protected set => _progressViewer = value; }

        private double _progress;
        public double Progress { get => _progress; }

        public ProgressTrackerPane Increment(double value, string jobName)
        {
            _jobName = jobName;
            return Increment(value);
        }

        public ProgressTrackerPane Increment(double value)
        {
            var newProgress = Math.Max(Math.Min(_progress + value, 100), 0);
            Report(newProgress);
            return this;
        }

        public void Report((int Value, string JobName) report)
        {
            Report((double)report.Value, report.JobName);
        }

        public void Report(double value, string jobName)
        {
            if (value < 0)
            {
                var caller = new StackTrace().GetMyTraceString();
                throw new ArgumentOutOfRangeException($"Progress reported " +
                    $"by {caller} must be an integer between 0 and 100");
            }
            if (value > 100)
            {
                Report(100);
            }
            else
            {
                _jobName = jobName;
                Report(value);
            }
        }

        internal void ChangeBarColor(System.Drawing.Color color)
        {
            SafeAction(() => _progressViewer.Bar.BackColor = color);
        }

        internal void SafeAction(Action action)
        {
            if (_progressViewer.IsDisposed) { return; }
            if (_progressViewer.InvokeRequired)
                _progressViewer.Invoke(action);
            else
                action();
        }

        public void Report(double value)
        {
            if (value < 0)
            {
                var caller = new StackFrame(1, false).GetMethod().Name;
                throw new ArgumentOutOfRangeException($"Progress reported by {caller} must be an integer between 0 and 100");
            }
            else if (value > 100)
            {
                Report(100);
            }
            else
            {
                _progress = value;
                var parentProgress = (int)Math.Round(_parent.Allocation * value / 100, 0) + _parent.StartingAt;
                _parent.Progress.Report((parentProgress, _jobName));
                if (_isRoot) 
                { 
                    if (parentProgress == 100 || _root100) 
                    {
                        ChangeBarColor(_root100 ? System.Drawing.Color.Blue : System.Drawing.Color.Green);
                        _root100 = !_root100;
                    }
                } 
            }
        }

        //public async Task ReportAsync(double value)
        //{
        //    if (value < 0)
        //    {
        //        var caller = new StackFrame(1, false).GetMethod().Name;
        //        throw new ArgumentOutOfRangeException($"Progress reported by {caller} must be an integer between 0 and 100");
        //    }
        //    else if (value > 100)
        //    {
        //        Report(100);
        //    }
        //    else
        //    {
        //        _progress = value;
        //        var parentProgress = (int)Math.Round(_parent.Allocation * value / 100, 0) + _parent.StartingAt;
        //        _parent.Progress.Report((parentProgress, _jobName));
        //        if (_isRoot && parentProgress == 100)
        //        {
        //            //if (_progressViewer.InvokeRequired)
        //            //    await _progressViewer.UiDispatcher.InvokeAsync(() => { if (!_progressViewer.IsDisposed) { _progressViewer.Dispose(); } });
        //            //else
        //            //    if (!_progressViewer.IsDisposed) { _progressViewer.Dispose(); }
        //        }
        //    }
        //}

        public ProgressTrackerPane SpawnChild(int allocation)
        {
            return new ProgressTrackerPane(this, allocation, (int)_progress);
        }

        public ProgressTrackerPane SpawnChild(double allocation)
        {
            return this.SpawnChild((int)Math.Round(allocation, 0));
        }

        public ProgressTrackerPane SpawnChild()
        {
            var progress = (int)_progress;
            var remaining = 100 - progress;
            return new ProgressTrackerPane(this, remaining, progress);
        }
    }

}

