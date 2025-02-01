using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using UtilitiesCS.Threading;
using UtilitiesCS.Windows_Forms;

namespace UtilitiesCS
{
    public class ProgressTracker: IProgress<(int Value, string JobName)>
    {
        public ProgressTracker(CancellationTokenSource tokenSource) 
        {
            _cancelSource = tokenSource;
        }

        public ProgressTracker(CancellationTokenSource tokenSource, Screen screen)
        {
            _cancelSource = tokenSource;
            _screen = screen;
        }

        public virtual ProgressTracker Initialize() 
        {
            UiDispatcher = UiThread.Dispatcher;

            UiDispatcher.Invoke(() =>
            {
                _progressViewer = new ProgressViewer
                {
                    UiDispatcher = UiThread.Dispatcher,
                    CancelSource = _cancelSource
                };
                if (_screen != null)
                {
                    _progressViewer.StartPosition = FormStartPosition.Manual;
                    _progressViewer.TrySwitchScreens(_screen, true);
                }
                var rootProgress = new Progress<(int value, string jobName)>(tup =>
                {
                    _progressViewer.JobName.Text = tup.jobName;
                    _progressViewer.Bar.Value = tup.value;
                    _progressViewer.Refresh();
                });
                _parent = new ParentProgress<(int Value, string JobName)>(rootProgress, 100, 0);
                _isRoot = true;
                _progressViewer.Show();
            });
            return this;
        }

        public ProgressTracker(ProgressTracker parent, int allocation, int startingAt) 
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
        private ParentProgress<(int Value, string JobName)> _parent;
        private ThreadSafeSingleShotGuard _pvIsDisposed = new ThreadSafeSingleShotGuard();
        private CancellationTokenSource _cancelSource;
        private Screen _screen;

        internal Dispatcher UiDispatcher { get => _uiDispatcher; set => _uiDispatcher = value; }
        private Dispatcher _uiDispatcher;
        
        private ProgressViewer _progressViewer;
        public ProgressViewer ProgressViewer { get => _progressViewer; protected set => _progressViewer = value; }

        private double _progress;
        public double Progress { get => _progress; }

        public virtual ProgressTracker Increment(double value, string jobName)
        {
            _jobName = jobName;
            return Increment(value);
        }

        public virtual ProgressTracker Increment(double value)
        {
            var newProgress = Math.Max(Math.Min(_progress + value,100),0);
            Report(newProgress);
            return this;
        }

        public virtual void Report((int Value, string JobName) report)
        {
            Report((double)report.Value, report.JobName);
        }

        public virtual void Report(double value, string jobName)
        {
            if (value < 0)
            {
                var caller = new StackFrame(1, false).GetMethod().Name;
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

        public virtual void Report(double value)
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
                
                // Updates UI
                _parent.Progress.Report((parentProgress, _jobName));
                
                if (_isRoot && parentProgress == 100)
                {
                    if (_pvIsDisposed.CheckAndSetFirstCall)
                    {
                        if (_progressViewer.InvokeRequired)
                            _progressViewer.Invoke(() => 
                            {
                                if (!_progressViewer.IsDisposed)
                                    _progressViewer.Close();    
                            });
                        else
                            _progressViewer.Close();                                       
                    }
                }
            }
        }

        public async virtual Task ReportAsync(double value) 
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
                if (_isRoot && parentProgress == 100)
                {
                    if (_progressViewer.InvokeRequired)
                        await _progressViewer.UiDispatcher.InvokeAsync(() => { if (!_progressViewer.IsDisposed) { _progressViewer.Close(); } });
                    else
                        if (!_progressViewer.IsDisposed) { _progressViewer.Close(); }
                }
            }
        }

        public virtual ProgressTracker SpawnChild(int allocation)
        {
            return new ProgressTracker(this, allocation, (int)_progress);
        }

        public virtual ProgressTracker SpawnChild(double allocation)
        {
            return this.SpawnChild((int)Math.Round(allocation, 0));            
        }

        public virtual ProgressTracker SpawnChild()
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
