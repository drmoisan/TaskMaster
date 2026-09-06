#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using UtilitiesCS.Windows_Forms;

namespace UtilitiesCS.Threading
{
    public class ProgressTrackerAsync
    {
        internal Action<ProgressViewer> ShowProgressViewer { get; set; } = viewer => viewer.Show();

        #region Constructors and Initializers

        public ProgressTrackerAsync(CancellationTokenSource tokenSource)
        {
            _cancelSource = tokenSource;
        }

        public ProgressTrackerAsync(CancellationTokenSource tokenSource, Screen? screen)
        {
            _cancelSource = tokenSource;
            _screen = screen;
        }

        public async Task<ProgressTrackerAsync> InitializeAsync()
        {
            UiDispatcher = UiThread.Dispatcher;

            await UiDispatcher.InvokeAsync(() =>
            {
                _progressViewer = new ProgressViewer
                {
                    UiDispatcher = UiDispatcher,
                    CancelSource = _cancelSource,
                };

                if (_screen != null)
                {
                    _progressViewer.StartPosition = FormStartPosition.Manual;
                    _progressViewer.TrySwitchScreens(_screen, true);
                }

                //_isRoot = true;
                _progressViewer.JobName.Text = "Initializing...";
                ShowProgressViewer(_progressViewer);
            });

            return this;
        }

        #endregion Constructors and Initializers

        #region Private Fields

        private CancellationTokenSource? _cancelSource;
        private Screen? _screen;
        //private bool _isRoot = false;

        #endregion Private Fields

        #region Public Properties

        private ProgressViewer _progressViewer = null!; // set in InitializeAsync() before use
        public ProgressViewer ProgressViewer
        {
            get => _progressViewer;
            set => _progressViewer = value;
        }

        protected string _jobName = null!; // JobName contract is non-null; set before use
        internal string JobName
        {
            get => _jobName;
            set => _jobName = value;
        }

        //private double _progress;
        //internal double Progress { get => _progress; }

        protected Dispatcher _uiDispatcher = null!; // set in InitializeAsync() before any dispatch
        internal Dispatcher UiDispatcher
        {
            get => _uiDispatcher;
            set => _uiDispatcher = value;
        }

        internal int Allocation
        {
            get => _allocation;
            set => _allocation = value;
        }
        protected int _allocation = 100;

        internal int StartingAt
        {
            get => _startingAt;
            set => _startingAt = value;
        }
        protected int _startingAt = 0;

        #endregion Public Properties
    }
}
