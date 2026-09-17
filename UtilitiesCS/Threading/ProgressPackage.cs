#nullable enable
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
    public class ProgressPackage : IDisposable
    {
        public ProgressPackage() { }

        public async Task<ProgressPackage> InitializeAsync(
            CancellationTokenSource? cancelSource = null,
            CancellationToken cancel = default,
            ProgressTracker? progressTracker = null,
            SegmentStopWatch? stopWatch = null,
            Screen? screen = null
        )
        {
            _cancelSource = cancelSource ?? new CancellationTokenSource();
            _ownsCancelSource = cancelSource is null;
            _cancel = cancel == default ? _cancelSource.Token : cancel;
            _progressTracker =
                progressTracker ?? new ProgressTracker(_cancelSource, screen).Initialize();
            _stopWatch = stopWatch ?? await Task.Run(() => new SegmentStopWatch().Start());
            return this;
        }

        public async Task<ProgressPackage> InitializeAsync(
            CancellationTokenSource? cancelSource = null,
            CancellationToken cancel = default,
            ProgressTrackerPane? progressTrackerPane = null,
            SegmentStopWatch? stopWatch = null
        )
        {
            _cancelSource = cancelSource ?? new CancellationTokenSource();
            _ownsCancelSource = cancelSource is null;
            _cancel = cancel == default ? _cancelSource.Token : cancel;
            _progressTrackerPane = progressTrackerPane ?? new ProgressTrackerPane(_cancelSource);
            _stopWatch = stopWatch ?? await Task.Run(() => new SegmentStopWatch().Start());
            return this;
        }

        /// <summary>
        /// Issue #872. The cancellation token source in the returned tuple is transferred to the
        /// caller, and the caller owns its release. The package this factory constructs is discarded
        /// once the tuple is taken, so nothing else can release that source; disposing it here would
        /// release the very source the factory is contractually returning.
        /// </summary>
        public static async Task<(
            CancellationTokenSource? CancelSource,
            CancellationToken Cancel,
            ProgressTracker? ProgressTracker,
            SegmentStopWatch? StopWatch
        )> CreateAsTupleAsync(
            CancellationTokenSource? cancelSource = null,
            CancellationToken cancel = default,
            ProgressTracker? progressTracker = null,
            SegmentStopWatch? stopWatch = null,
            Screen? screen = null
        )
        {
            var package = new ProgressPackage();
            await package.InitializeAsync(cancelSource, cancel, progressTracker, stopWatch, screen);
            return package.ToTuple();
        }

        /// <summary>
        /// Issue #872. The cancellation token source in the returned tuple is transferred to the
        /// caller, and the caller owns its release. The package this factory constructs is discarded
        /// once the tuple is taken, so nothing else can release that source; disposing it here would
        /// release the very source the factory is contractually returning.
        /// </summary>
        public static async Task<(
            CancellationTokenSource? CancelSource,
            CancellationToken Cancel,
            ProgressTrackerPane? ProgressTrackerPane,
            SegmentStopWatch? StopWatch
        )> CreateAsTuplePaneAsync(
            CancellationTokenSource? cancelSource = null,
            CancellationToken cancel = default,
            ProgressTrackerPane? progressTrackerPane = null,
            SegmentStopWatch? stopWatch = null
        )
        {
            var package = new ProgressPackage();
            await package.InitializeAsync(cancelSource, cancel, progressTrackerPane, stopWatch);
            return package.ToTuplePane();
        }

        private CancellationTokenSource? _cancelSource;
        public CancellationTokenSource? CancelSource
        {
            get => _cancelSource;
            set => _cancelSource = value;
        }

        // Issue #872. True only when this package constructed the held cancellation token source, so
        // that Dispose releases a source the package owns and never one the caller lent it. The flag
        // is deliberately not touched by the CancelSource setter: SpawnChild copies the parent's
        // source through that setter, and a setter that claimed ownership would make every child
        // claim its parent's source.
        private bool _ownsCancelSource;

        private CancellationToken _cancel;
        public CancellationToken Cancel
        {
            get => _cancel;
            set => _cancel = value;
        }

        private ProgressTracker? _progressTracker;
        public ProgressTracker? ProgressTracker
        {
            get => _progressTracker;
            set => _progressTracker = value;
        }

        private ProgressTrackerPane? _progressTrackerPane;
        public ProgressTrackerPane? ProgressTrackerPane
        {
            get => _progressTrackerPane;
            set => _progressTrackerPane = value;
        }

        // Note: Should be run on a background thread to avoid locking the UI thread
        public SegmentStopWatch? StopWatch
        {
            get => _stopWatch;
            set => _stopWatch = value;
        }
        private SegmentStopWatch? _stopWatch;

        public ProgressPackage SpawnChild(int allocation)
        {
            return new ProgressPackage
            {
                CancelSource = this.CancelSource,
                Cancel = this.Cancel,
                StopWatch = this.StopWatch,
                ProgressTracker = this.ProgressTracker?.SpawnChild(allocation),
                ProgressTrackerPane = this.ProgressTrackerPane?.SpawnChild(allocation),
            };
        }

        public (
            CancellationTokenSource? CancelSource,
            CancellationToken Cancel,
            ProgressTracker? ProgressTracker,
            SegmentStopWatch? StopWatch
        ) ToTuple()
        {
            return (CancelSource, Cancel, ProgressTracker, StopWatch);
        }

        public (
            CancellationTokenSource? CancelSource,
            CancellationToken Cancel,
            ProgressTrackerPane? ProgressTrackerPane,
            SegmentStopWatch? StopWatch
        ) ToTuplePane()
        {
            return (CancelSource, Cancel, ProgressTrackerPane, StopWatch);
        }

        /// <summary>
        /// Releases the cancellation token source only when this package constructed it. A
        /// caller-supplied source, and a source a spawned child received through the property
        /// setter, belong to their owner and are left usable. The held reference is deliberately
        /// not cleared, so the public getter's observable behaviour for existing callers is
        /// unchanged.
        /// </summary>
        public void Dispose()
        {
            if (_ownsCancelSource)
            {
                _cancelSource?.Dispose();
                _ownsCancelSource = false;
            }
        }
    }
}
