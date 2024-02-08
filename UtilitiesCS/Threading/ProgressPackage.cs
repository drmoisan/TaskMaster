using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Threading
{
    public class ProgressPackage
    {
        public ProgressPackage() { }

        public async Task<ProgressPackage> InitializeAsync(
            CancellationTokenSource cancelSource = null,
            CancellationToken cancel = default,
            ProgressTracker progressTracker = null,
            SegmentStopWatch stopWatch = null,
            Screen screen = null)
        {
            _cancelSource = cancelSource ?? new CancellationTokenSource();
            _cancel = cancel == default ? _cancelSource.Token : cancel;
            _progressTracker = progressTracker ?? new ProgressTracker(_cancelSource, screen).Initialize();
            _stopWatch = stopWatch ?? await Task.Run(() => new SegmentStopWatch().Start());
            return this;
        }

        public async Task<ProgressPackage> InitializeAsync(
            CancellationTokenSource cancelSource = null,
            CancellationToken cancel = default,
            ProgressTrackerPane progressTrackerPane = null,
            SegmentStopWatch stopWatch = null)
        {
            _cancelSource = cancelSource ?? new CancellationTokenSource();
            _cancel = cancel == default ? _cancelSource.Token : cancel;
            _progressTrackerPane = progressTrackerPane ?? new ProgressTrackerPane(_cancelSource);
            _stopWatch = stopWatch ?? await Task.Run(() => new SegmentStopWatch().Start());
            return this;
        }

        public static async Task<(
            CancellationTokenSource CancelSource,
            CancellationToken Cancel,
            ProgressTracker ProgressTracker,
            SegmentStopWatch StopWatch)> CreateAsTupleAsync(
            CancellationTokenSource cancelSource = null,
            CancellationToken cancel = default,
            ProgressTracker progressTracker = null,
            SegmentStopWatch stopWatch = null,
            Screen screen = null)
        { 
            var package = new ProgressPackage();
            await package.InitializeAsync(cancelSource, cancel, progressTracker, stopWatch, screen);
            return package.ToTuple();
        }

        public static async Task<(
            CancellationTokenSource CancelSource,
            CancellationToken Cancel,
            ProgressTrackerPane ProgressTrackerPane,
            SegmentStopWatch StopWatch)> CreateAsTuplePaneAsync(
            CancellationTokenSource cancelSource = null,
            CancellationToken cancel = default,
            ProgressTrackerPane progressTrackerPane = null,
            SegmentStopWatch stopWatch = null)
        {
            var package = new ProgressPackage();
            await package.InitializeAsync(cancelSource, cancel, progressTrackerPane, stopWatch);
            return package.ToTuplePane();
        }

        private CancellationTokenSource _cancelSource;
        public CancellationTokenSource CancelSource { get => _cancelSource; set => _cancelSource = value; }

        private CancellationToken _cancel;
        public CancellationToken Cancel { get => _cancel; set => _cancel = value; }

        private ProgressTracker _progressTracker;
        public ProgressTracker ProgressTracker { get => _progressTracker; set => _progressTracker = value; }

        private ProgressTrackerPane _progressTrackerPane;
        public ProgressTrackerPane ProgressTrackerPane { get => _progressTrackerPane; set => _progressTrackerPane = value; }

        // Note: Should be run on a background thread to avoid locking the UI thread
        public SegmentStopWatch StopWatch { get => _stopWatch; set => _stopWatch = value; }
        private SegmentStopWatch _stopWatch;
        
        public (
            CancellationTokenSource CancelSource,
            CancellationToken Cancel,
            ProgressTracker ProgressTracker,
            SegmentStopWatch StopWatch) ToTuple()
        {
            return (CancelSource, Cancel, ProgressTracker, StopWatch);
        }

        public (
            CancellationTokenSource CancelSource,
            CancellationToken Cancel,
            ProgressTrackerPane ProgressTrackerPane,
            SegmentStopWatch StopWatch) ToTuplePane()
        {
            return (CancelSource, Cancel, ProgressTrackerPane, StopWatch);
        }
    }
}
