using System.Collections.Generic;
using System.Threading.Tasks;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Test subclass of <see cref="ApplicationGlobals"/> that no-ops the issue #211 host-bound
    /// diagnostics seams (heartbeat DispatcherTimer, live GC reads, and the live
    /// StoreWrapperInitClock sample) and records the phase visit order so focused MSTests can drive
    /// the real <c>LoadSequentialAsync</c> coordinator sequence without a live Outlook/VSTO runtime.
    /// </summary>
    /// <remarks>
    /// Extracted from <c>ApplicationGlobalsTests.cs</c> (issue #211, Phase 3.6, P4-T4) so the seam
    /// override added in P4-T5 keeps every touched test file within the 500-line ceiling. The class
    /// is behavior-preserving relative to its former nested form; only its location and accessibility
    /// changed (private nested -&gt; non-nested internal).
    /// </remarks>
    internal sealed class TestableApplicationGlobals : ApplicationGlobals
    {
        // This file has no project-level <Nullable> and no whole-file #nullable pragma; the
        // pre-existing `?` annotations below need an explicit annotations context to avoid
        // CS8632. Scoping narrowly to annotations-only avoids introducing new CS86xx
        // diagnostics elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
        private readonly IList<string>? _visitedStages;

        public TestableApplicationGlobals(
            OutlookApplication application,
            IList<string>? visitedStages = null
        )
#nullable restore annotations
            : base(application, false)
        {
            _visitedStages = visitedStages;
        }

        public int YieldCount { get; private set; }

        public Task InvokeInitializeEnginesPhaseAsync() => InitializeEnginesPhaseAsync();

        protected internal override Task LoadIntelConfigPhaseAsync()
        {
            _visitedStages?.Add("intel");
            return Task.CompletedTask;
        }

        protected internal override async Task YieldWithContinuationProbeAsync(
            string priorPhaseName
        )
        {
            YieldCount++;
            await base.YieldWithContinuationProbeAsync(priorPhaseName);
        }

        protected internal override Task LoadOlObjectsPhaseAsync()
        {
            _visitedStages?.Add("ol");
            return Task.CompletedTask;
        }

        protected internal override Task LoadToDoPhaseAsync()
        {
            _visitedStages?.Add("todo");
            return Task.CompletedTask;
        }

        protected internal override Task LoadAutoFilePhaseAsync()
        {
            _visitedStages?.Add("auto");
            return Task.CompletedTask;
        }

        protected internal override Task LoadEventsPhaseAsync()
        {
            _visitedStages?.Add("events");
            return Task.CompletedTask;
        }

        // No-op the issue #211 Phase 3.2 host-bound diagnostics seams so the heartbeat
        // DispatcherTimer and the live GC.* reads never execute under the unit-test seam.
        protected internal override void StartStartupUiHeartbeat(
            TaskMaster.StartupDiagnosticsProbe probe
        ) { }

        protected internal override void StopStartupUiHeartbeat() { }

        protected internal override void BeginPhaseGcCapture(string phase) { }

        protected internal override void EmitPhaseGcDelta(
            TaskMaster.StartupDiagnosticsProbe probe,
            string phase
        ) { }

        // No-op the issue #211 Phase 3.6 live StoreWrapperInitClock read so LoadSequentialAsync
        // never touches the process-global accumulator under the unit-test seam (P4-T5).
        protected internal override double SampleStoreWrapperInitTotalMs() => 0.0;
    }
}
