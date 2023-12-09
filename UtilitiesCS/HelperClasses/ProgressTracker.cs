using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class ProgressTracker: IProgress<(int Value, string JobName)>
    {
        public ProgressTracker(CancellationTokenSource tokenSource) 
        { 
            _progressViewer = new ProgressViewer();
            _progressViewer.SetCancellationTokenSource(tokenSource);

            var rootProgress = new Progress<(int value, string jobName)>(tup =>
            {
                _progressViewer.Bar.Value = tup.value;
                _progressViewer.JobName.Text = tup.jobName;
            });
            _parent = new ParentProgress<(int Value, string JobName)>(rootProgress, 100, 0);
            this.Report(0, "Initializing");
            _progressViewer.Show();
            _isRoot = true;
        }
        
        public ProgressTracker(IProgress<(int Value, string JobName)> parent, int allocation, int startingAt) 
        { 
            _parent = new ParentProgress<(int Value, string JobName)>(parent, allocation, startingAt);
        }
        
        private string _jobName;
        private bool _isRoot = false;
        private ProgressViewer _progressViewer;
        private ParentProgress<(int Value, string JobName)> _parent;
        

        private double _progress;
        public double Progress { get => _progress; }

        public ProgressTracker Increment(double value, string jobName)
        {
            _jobName = jobName;
            return Increment(value);
        }

        public ProgressTracker Increment(double value)
        {
            var newProgress = Math.Max(Math.Min(_progress + value,100),0);
            Report(newProgress);
            return this;
        }

        public void Report((int Value, string JobName) report)
        {
            if (report.Value < 0 || report.Value > 100)
            {
                var caller = new StackFrame(1, false).GetMethod().Name;
                throw new ArgumentOutOfRangeException($"Progress reported by {caller} must be an integer between 0 and 100");
            }
            Report((double)report.Value, report.JobName);
        }

        public void Report(double value, string jobName)
        {
            if (value < 0 || value > 100)
            {
                var caller = new StackFrame(1, false).GetMethod().Name;
                throw new ArgumentOutOfRangeException($"Progress reported by {caller} must be an integer between 0 and 100");
            }
            _jobName = jobName;
            Report(value);
        }

        public void Report(double value)
        {
            if (value < 0 || value > 100)
            {
                var caller = new StackFrame(1, false).GetMethod().Name;
                throw new ArgumentOutOfRangeException($"Progress reported by {caller} must be an integer between 0 and 100");
            }
            _progress = value;
            var parentProgress = (int)Math.Round(_parent.Allocation * value / 100,0) + _parent.StartingAt;
            _parent.Progress.Report((parentProgress, _jobName));
            if(_isRoot && parentProgress == 100)
            {
                if (_progressViewer.InvokeRequired)
                    _progressViewer.Invoke(()=> { if (!_progressViewer.IsDisposed) { _progressViewer.Close(); } });
                else
                    if (!_progressViewer.IsDisposed) { _progressViewer.Close(); }
            }
        }

        public ProgressTracker SpawnChild(int allocation)
        {
            return new ProgressTracker(this, allocation, (int)_progress);
        }

        public ProgressTracker SpawnChild()
        {
            var progress = (int)_progress;
            var remaining = 100 - progress;
            return new ProgressTracker(this, remaining, progress);
        }
    }

    internal struct ParentProgress<T>
    {
        public ParentProgress(IProgress<T> progress, int allocation, int startingAt)
        {
            _progress = progress;
            _allocation = allocation;
            _startingAt = startingAt;
        }
        private IProgress<T> _progress;
        public IProgress<T> Progress { get => _progress; set => _progress = value; }

        private int _allocation;
        public int Allocation { get => _allocation; set => _allocation = value; }

        private int _startingAt;
        public int StartingAt { get => _startingAt; set => _startingAt = value; }
    }
}
