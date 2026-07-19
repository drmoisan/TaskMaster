#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using log4net;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// Watchdog that periodically pings a monitored UI thread and measures how long it has been
    /// unresponsive (issue #264, epic #260). Two independent thresholds are supported:
    /// <list type="bullet">
    /// <item>the small, original <c>delayThreshold</c> that drives the diagnostic
    /// <c>Thread.Suspend</c>/<c>Thread.Resume</c> stack-trace capture (obsolete APIs, Debug-level,
    /// unchanged); and</item>
    /// <item>the larger, configurable <see cref="LockupStallDecider.ThresholdMs"/> that, when
    /// crossed, raises the injected <c>onLockupDetected</c> callback so a separate orchestrator can
    /// attribute the stall to a store and respond.</item>
    /// </list>
    /// The stall-timing decision is extracted onto the deterministic, injectable
    /// <see cref="EvaluatePoll"/> seam driven by an injected <see cref="TimeProvider"/> and the pure
    /// <see cref="LockupStallDecider"/>, so it is unit-testable without a live <see cref="Dispatcher"/>
    /// or thread. The infinite polling loop stays a thin, host-bound shell (<see cref="Run"/>).
    /// </summary>
    public class ThreadMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType
        );
        private readonly Thread? thread;
        private readonly int pollingFrequency;
        private readonly int delayThreshold;
        private readonly int stackTraceIterations;
        private readonly TimeProvider _timeProvider;
        private readonly LockupStallDecider _decider;
        private readonly Action<LockupAttribution>? _onLockupDetected;

        private DateTimeOffset _lastResponsiveUtc;
        private bool _lockupReported;
        private ITimer? _pollTimer;

        /// <summary>
        /// Creates a thread monitor.
        /// </summary>
        /// <param name="thread">The UI thread to monitor. May be null on the attribution seam path (tests).</param>
        /// <param name="pollingFrequency">The polling cadence in milliseconds.</param>
        /// <param name="delayThreshold">The small diagnostic threshold gating the stack-trace capture cadence.</param>
        /// <param name="stackTraceIterations">How many diagnostic sub-polls to perform per cycle.</param>
        /// <param name="timeProvider">
        /// The clock used to measure elapsed unresponsive time. When null, <see cref="TimeProvider.System"/>
        /// is used (production); tests pass a <c>FakeTimeProvider</c>.
        /// </param>
        /// <param name="lockupAttributionThresholdMs">
        /// The unresponsive duration, in milliseconds, at or beyond which a lockup is confirmed and
        /// the callback fires. Distinct from <paramref name="delayThreshold"/>.
        /// </param>
        /// <param name="onLockupDetected">
        /// Invoked on the watchdog's background thread with the populated <see cref="LockupAttribution"/>
        /// exactly once per stall episode when the attribution threshold is crossed. May be null.
        /// </param>
        public ThreadMonitor(
            Thread? thread,
            int pollingFrequency = 500,
            int delayThreshold = 100,
            int stackTraceIterations = 4,
            TimeProvider? timeProvider = null,
            int lockupAttributionThresholdMs = 5000,
            Action<LockupAttribution>? onLockupDetected = null
        )
        {
            this.thread = thread;
            this.pollingFrequency = pollingFrequency;
            this.delayThreshold = delayThreshold;
            this.stackTraceIterations = stackTraceIterations;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _decider = new LockupStallDecider(lockupAttributionThresholdMs);
            _onLockupDetected = onLockupDetected;
            _lastResponsiveUtc = _timeProvider.GetUtcNow();
        }

        /// <summary>The lockup-attribution threshold, in milliseconds (from the decider).</summary>
        internal double LockupAttributionThresholdMs => _decider.ThresholdMs;

        /// <summary>
        /// Starts the host-bound polling loop. This is the only host-bound member; it constructs a
        /// clock-driven timer (via the injected <see cref="TimeProvider"/>) that re-arms after each
        /// tick, so the inter-poll wait is driven by the provider rather than <c>Thread.Sleep</c>.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Run()
        {
            _lastResponsiveUtc = _timeProvider.GetUtcNow();
            _pollTimer = _timeProvider.CreateTimer(
                _ => Tick(),
                null,
                TimeSpan.FromMilliseconds(pollingFrequency),
                Timeout.InfiniteTimeSpan
            );
        }

        [ExcludeFromCodeCoverage]
        private void Tick()
        {
            try
            {
                var completed = PingAndAwaitDiagnosticWindow();

                // Attribution path (deterministic, testable via EvaluatePoll): decide + callback
                // through the injected clock. No Thread.Sleep, no stack capture here, so a fragile
                // diagnostic path can never delay or prevent auto-disable/notify.
                EvaluatePoll(() => completed);

                // Diagnostic-only path (unchanged from the original watchdog), gated behind the small
                // delayThreshold and the obsolete Thread.Suspend stack capture. Not on the attribution
                // path; Debug-level only.
                if (!completed)
                {
                    // thread is non-null on this production diagnostic path (null only on the test seam).
                    var stackTrace = GetStackTrace(thread!);
                    Log.Debug($"StackTrace of UI Thread: {stackTrace}");
                    Debug.WriteLine($"StackTrace of UI Thread: {stackTrace}");
                }
            }
            finally
            {
                // Re-arm the one-shot timer for the next cycle, keeping ticks strictly sequential.
                _pollTimer?.Change(
                    TimeSpan.FromMilliseconds(pollingFrequency),
                    Timeout.InfiniteTimeSpan
                );
            }
        }

        [ExcludeFromCodeCoverage]
        private bool PingAndAwaitDiagnosticWindow()
        {
            var dispatcher = Dispatcher.FromThread(thread);
            if (dispatcher is null)
            {
                UiThread.UiSyncContext.Send((x) => dispatcher = Dispatcher.CurrentDispatcher, null);
            }

            // dispatcher is assigned via the Send above when FromThread returned null; behavior-preserving.
            var task = dispatcher!.InvokeAsync(() => { });

            for (var i = 0; i < stackTraceIterations; i++)
            {
                Thread.Sleep(delayThreshold);
                if (task.Status == DispatcherOperationStatus.Completed)
                {
                    return true;
                }

                Debug.WriteLine(
                    // thread is non-null on this production ping path (null only on the test seam path).
                    $"{(i + 1) * delayThreshold}ms Delay on thread {thread!.Name} ({task.Status})"
                );
            }

            return task.Status == DispatcherOperationStatus.Completed;
        }

        /// <summary>
        /// Deterministic attribution seam. Consumes an injected responsiveness probe (the host shell
        /// supplies the real <see cref="Dispatcher.InvokeAsync"/> completion status; tests supply a
        /// stub), measures elapsed unresponsive time through the injected <see cref="TimeProvider"/>,
        /// delegates the crossing decision to <see cref="LockupStallDecider"/>, reads
        /// <see cref="CurrentStoreContext.Current"/>, and invokes the <c>onLockupDetected</c> callback
        /// exactly once per stall episode when the attribution threshold is crossed. A responsive poll
        /// resets the stall tracking so a subsequent stall can fire again.
        /// </summary>
        /// <param name="uiResponsiveProbe">Returns true when the monitored UI thread is responsive.</param>
        /// <returns>The attribution raised on this poll, or null when none was raised.</returns>
        internal LockupAttribution? EvaluatePoll(Func<bool> uiResponsiveProbe)
        {
            var now = _timeProvider.GetUtcNow();

            if (uiResponsiveProbe())
            {
                _lastResponsiveUtc = now;
                _lockupReported = false;
                return null;
            }

            var elapsed = now - _lastResponsiveUtc;
            if (_lockupReported || !_decider.IsStallConfirmed(elapsed.TotalMilliseconds))
            {
                return null;
            }

            _lockupReported = true;
            var attribution = new LockupAttribution(elapsed, CurrentStoreContext.Current);
            _onLockupDetected?.Invoke(attribution);
            return attribution;
        }

#pragma warning disable 0618
        [ExcludeFromCodeCoverage]
        private StackTrace? GetStackTrace(Thread targetThread)
        {
            StackTrace? stackTrace = null;
            var ready = new ManualResetEventSlim();

            new Thread(() =>
            {
                // Backstop to release thread in case of deadlock:
                ready.Set();
                Thread.Sleep(200);
                try
                {
                    targetThread.Resume();
                }
                catch { }
            }).Start();

            ready.Wait();
            targetThread.Suspend();
            try
            {
                stackTrace = new StackTrace(targetThread, true);
            }
            catch
            { /* Deadlock */
            }
            finally
            {
                try
                {
                    targetThread.Resume();
                }
                catch
                {
                    stackTrace = null; /* Deadlock */
                }
            }

            return stackTrace;
        }
#pragma warning restore 0618
    }
}
