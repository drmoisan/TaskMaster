using System;
using System.Runtime.InteropServices;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// Outcome of a single <see cref="HookReadinessCoordinator.Tick"/> call.
    /// </summary>
    /// <remarks>
    /// A plain <see langword="enum"/> (not a positional <c>record struct</c>) because the net48
    /// target lacks <c>System.Runtime.CompilerServices.IsExternalInit</c> (CS0518).
    /// </remarks>
    internal enum HookReadinessTickResult
    {
        /// <summary>
        /// The gate is not ready (or the hookup raised a transient not-ready
        /// <see cref="COMException"/>); polling should continue. The run-once guard remains unset.
        /// </summary>
        ContinuePolling,

        /// <summary>
        /// The hookup has run exactly once and completed; polling should stop.
        /// </summary>
        Completed,
    }

    /// <summary>
    /// Pure, deterministic decision/state-machine seam for the Issue #207 readiness-gated
    /// startup hookup. Each <see cref="Tick"/> consults an injected
    /// <see cref="IOutlookReadinessGate"/>; when the gate reports ready it invokes the hookup
    /// callback exactly once and reports <see cref="HookReadinessTickResult.Completed"/>.
    /// A transient not-ready <see cref="COMException"/> thrown by the hookup is treated as
    /// not-ready (retry); a non-transient exception propagates.
    /// </summary>
    /// <remarks>
    /// This is the unit-tested decision seam. It contains no COM, no
    /// <see cref="System.Windows.Threading.DispatcherTimer"/>, and no clock, so it is fully
    /// deterministic and is covered by
    /// <c>TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs</c> with a
    /// <c>Mock&lt;IOutlookReadinessGate&gt;</c>. The <c>DispatcherTimer</c> and polling cadence
    /// that drive this coordinator are owned by the COM glue in <c>AppEvents.Hook()</c> and are
    /// COM/VSTO-exempt per the <c>CLAUDE.md</c> coverage exemption. Polling never gives up: there
    /// is no max-retry termination; the coordinator returns
    /// <see cref="HookReadinessTickResult.ContinuePolling"/> indefinitely until the gate is ready.
    /// </remarks>
    internal sealed class HookReadinessCoordinator
    {
        private readonly IOutlookReadinessGate _gate;
        private readonly Action _hookup;
        private bool _completed;

        /// <summary>
        /// Creates a coordinator over a readiness gate and a hookup callback.
        /// </summary>
        /// <param name="gate">
        /// The readiness gate consulted on each tick; must not be null.
        /// </param>
        /// <param name="hookup">
        /// The hookup action invoked exactly once when the gate first reports ready; must not be
        /// null. It is expected to perform the readiness-dependent COM subscriptions on the STA.
        /// </param>
        public HookReadinessCoordinator(IOutlookReadinessGate gate, Action hookup)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
            _hookup = hookup ?? throw new ArgumentNullException(nameof(hookup));
        }

        /// <summary>
        /// <see langword="true"/> once the hookup has run successfully; used by the timer wiring
        /// to stop polling.
        /// </summary>
        public bool IsCompleted => _completed;

        /// <summary>
        /// Advances the state machine by one poll iteration.
        /// </summary>
        /// <returns>
        /// <see cref="HookReadinessTickResult.Completed"/> when the hookup has run (now or
        /// previously); otherwise <see cref="HookReadinessTickResult.ContinuePolling"/>.
        /// </returns>
        /// <exception cref="COMException">
        /// Rethrown when the hookup raises a non-transient <see cref="COMException"/>
        /// (one for which <see cref="IOutlookReadinessGate.IsTransientError"/> is
        /// <see langword="false"/>). The run-once guard is left unset in that case.
        /// </exception>
        public HookReadinessTickResult Tick()
        {
            if (_completed)
            {
                return HookReadinessTickResult.Completed;
            }

            if (!_gate.IsReady())
            {
                return HookReadinessTickResult.ContinuePolling;
            }

            try
            {
                _hookup();
            }
            catch (COMException e) when (_gate.IsTransientError(e))
            {
                // Transient not-ready HRESULT: treat as not-ready, leave the run-once guard
                // unset, and retry on the next tick. The subscription is never dropped.
                return HookReadinessTickResult.ContinuePolling;
            }

            _completed = true;
            return HookReadinessTickResult.Completed;
        }
    }
}
