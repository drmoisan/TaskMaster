using System;
using System.Globalization;

namespace TaskMaster
{
    /// <summary>
    /// Diagnosis-only, behavior-preserving startup-diagnostics formatter for issue #211
    /// (Phase 3.1). Holds the pure measurement/formatting logic for two startup probes added
    /// around the <c>Engines</c> phase in <see cref="ApplicationGlobals.LoadSequentialAsync"/>:
    /// a UI-thread responsiveness heartbeat and a per-phase GC delta. Both emit one structured
    /// line through an injected sink so the formatting is unit-testable without a live appender,
    /// live COM, a live timer, a live Dispatcher, or live GC reads.
    /// </summary>
    /// <remarks>
    /// This type is intentionally NOT marked <c>[ExcludeFromCodeCoverage]</c>: it contains the
    /// coverable formatting seam (AC13). The COM/UI-host-bound concerns stay in the thin call
    /// site in <see cref="ApplicationGlobals"/>: the <c>DispatcherTimer</c>/<c>Dispatcher</c>
    /// scheduling, the per-tick <see cref="System.Diagnostics.Stopwatch"/>, and the live
    /// <c>GC.*</c>/<c>System.Runtime.GCSettings.*</c> reads. This helper only formats numeric
    /// values supplied by that call site. No <c>Stopwatch</c>, no <c>GC</c>, no <c>Dispatcher</c>,
    /// and no banned timing APIs (<c>DateTime.Now</c>, <c>DateTime.UtcNow</c>, <c>Random.Shared</c>,
    /// <c>Thread.Sleep</c>, <c>Task.Delay</c>) are used here.
    /// </remarks>
    public sealed class StartupDiagnosticsProbe
    {
        private readonly Action<string> _emit;

        /// <summary>
        /// Creates a probe that emits structured diagnostic lines through the supplied sink.
        /// </summary>
        /// <param name="emit">
        /// The line sink. Production passes <c>s =&gt; logger.Debug(s)</c>; tests pass a delegate
        /// that captures lines into a list. Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="emit"/> is null.</exception>
        public StartupDiagnosticsProbe(Action<string> emit)
        {
            _emit = emit ?? throw new ArgumentNullException(nameof(emit));
        }

        /// <summary>
        /// Emits exactly one <c>[ui-heartbeat]</c> line recording a single UI-thread heartbeat
        /// tick. The gap is the measured interval minus the nominal interval; a large positive
        /// gap indicates the UI/STA thread was starved or suspended between ticks.
        /// </summary>
        /// <param name="nominalMs">The scheduled (nominal) interval between ticks, in milliseconds.</param>
        /// <param name="actualMs">
        /// The actual elapsed interval since the previous tick, in milliseconds, as measured by
        /// the call site's <see cref="System.Diagnostics.Stopwatch"/>.
        /// </param>
        public void EmitHeartbeat(double nominalMs, double actualMs)
        {
            var gapMs = actualMs - nominalMs;
            _emit(
                "[ui-heartbeat] "
                    + $"nominalMs={nominalMs.ToString("F1", CultureInfo.InvariantCulture)} "
                    + $"actualMs={actualMs.ToString("F1", CultureInfo.InvariantCulture)} "
                    + $"gapMs={gapMs.ToString("F1", CultureInfo.InvariantCulture)}"
            );
        }

        /// <summary>
        /// Emits exactly one <c>[ui-heartbeat]</c> line annotated with the startup phase that was
        /// in flight when the tick fired (issue #211, Phase 3.2). Identical gap arithmetic and
        /// numeric formatting to <see cref="EmitHeartbeat(double, double)"/>, with an additional
        /// leading <c>phase=&lt;name&gt;</c> field so the all-phase heartbeat trace attributes each
        /// gap to the phase whose body/await was running. A large positive gap indicates the
        /// UI/STA thread was starved or suspended between ticks during that phase.
        /// </summary>
        /// <param name="phase">
        /// The currently-active startup phase name (for example <c>IntelConfig</c>, <c>OlObjects</c>,
        /// <c>ToDo</c>, <c>AutoFile</c>, <c>Engines</c>, <c>Events</c>). Emitted verbatim.
        /// </param>
        /// <param name="nominalMs">The scheduled (nominal) interval between ticks, in milliseconds.</param>
        /// <param name="actualMs">
        /// The actual elapsed interval since the previous tick, in milliseconds, as measured by
        /// the call site's <see cref="System.Diagnostics.Stopwatch"/>.
        /// </param>
        public void EmitHeartbeat(string phase, double nominalMs, double actualMs)
        {
            var gapMs = actualMs - nominalMs;
            _emit(
                "[ui-heartbeat] "
                    + $"phase={phase} "
                    + $"nominalMs={nominalMs.ToString("F1", CultureInfo.InvariantCulture)} "
                    + $"actualMs={actualMs.ToString("F1", CultureInfo.InvariantCulture)} "
                    + $"gapMs={gapMs.ToString("F1", CultureInfo.InvariantCulture)}"
            );
        }

        /// <summary>
        /// Emits exactly one <c>[startup-lifetime-heartbeat]</c> line for a single tick of the
        /// full add-in-startup-lifetime UI heartbeat (issue #211, Phase 3.3). This is the
        /// complementary, full-lifetime counterpart to <see cref="EmitHeartbeat(string, double,
        /// double)"/> (which is scoped to <c>LoadSequentialAsync</c>): it runs continuously across
        /// the entire add-in startup, so the trace shows exactly when the STA/UI thread was frozen
        /// — before, during, and after globals load. The gap is the measured interval minus the
        /// nominal interval; a <c>gapMs</c> far larger than nominal proves the STA was blocked for
        /// that interval. Identical numeric formatting to the other emitters (F1,
        /// <see cref="CultureInfo.InvariantCulture"/>). The <paramref name="stageLabel"/> is emitted
        /// verbatim so the call site can attribute each gap to a coarse startup stage.
        /// </summary>
        /// <param name="stageLabel">
        /// The coarse startup stage in effect when the tick fired (one of the canonical labels in
        /// <see cref="StartupStageLabels"/>). Emitted verbatim.
        /// </param>
        /// <param name="nominalMs">The scheduled (nominal) interval between ticks, in milliseconds.</param>
        /// <param name="actualMs">
        /// The actual elapsed interval since the previous tick, in milliseconds, as measured by the
        /// call site's <see cref="System.Diagnostics.Stopwatch"/>.
        /// </param>
        public void EmitLifetimeHeartbeat(string stageLabel, double nominalMs, double actualMs)
        {
            var gapMs = actualMs - nominalMs;
            _emit(
                "[startup-lifetime-heartbeat] "
                    + $"stageLabel={stageLabel} "
                    + $"nominalMs={nominalMs.ToString("F1", CultureInfo.InvariantCulture)} "
                    + $"actualMs={actualMs.ToString("F1", CultureInfo.InvariantCulture)} "
                    + $"gapMs={gapMs.ToString("F1", CultureInfo.InvariantCulture)}"
            );
        }

        /// <summary>
        /// Emits exactly one <c>[gc-delta]</c> line recording the garbage-collection activity that
        /// occurred across the <c>Engines</c> startup phase. The deltas and bytes are computed by
        /// the call site from the framework GC APIs before and after the phase; this method only
        /// formats them.
        /// </summary>
        /// <param name="gen0Delta">Gen0 collection-count delta across the phase.</param>
        /// <param name="gen1Delta">Gen1 collection-count delta across the phase.</param>
        /// <param name="gen2Delta">Gen2 collection-count delta across the phase.</param>
        /// <param name="allocatedBytesDelta">
        /// The change in <c>GC.GetTotalMemory(false)</c> across the phase, in bytes. May be
        /// negative if a collection during the phase reclaimed more than was allocated.
        /// </param>
        /// <param name="isServerGC">The value of <c>System.Runtime.GCSettings.IsServerGC</c>.</param>
        /// <param name="latencyMode">The value of <c>System.Runtime.GCSettings.LatencyMode</c>.</param>
        public void EmitGcDelta(
            int gen0Delta,
            int gen1Delta,
            int gen2Delta,
            long allocatedBytesDelta,
            bool isServerGC,
            string latencyMode
        )
        {
            _emit(
                "[gc-delta] "
                    + $"gen0={gen0Delta.ToString(CultureInfo.InvariantCulture)} "
                    + $"gen1={gen1Delta.ToString(CultureInfo.InvariantCulture)} "
                    + $"gen2={gen2Delta.ToString(CultureInfo.InvariantCulture)} "
                    + $"allocatedBytesDelta={allocatedBytesDelta.ToString(CultureInfo.InvariantCulture)} "
                    + $"isServerGC={isServerGC} "
                    + $"latencyMode={latencyMode}"
            );
        }

        /// <summary>
        /// Emits exactly one <c>[gc-delta]</c> line annotated with the startup phase across which
        /// the GC activity was measured (issue #211, Phase 3.2). Identical field set and numeric
        /// formatting to <see cref="EmitGcDelta(int, int, int, long, bool, string)"/>, with an
        /// additional leading <c>phase=&lt;name&gt;</c> field so a per-phase GC-delta is recorded
        /// for every phase in <see cref="ApplicationGlobals.LoadSequentialAsync"/>. The deltas and
        /// bytes are computed by the call site from the framework GC APIs before and after the
        /// phase; this method only formats them.
        /// </summary>
        /// <param name="phase">
        /// The startup phase name the delta is attributed to (for example <c>IntelConfig</c>,
        /// <c>OlObjects</c>, <c>ToDo</c>, <c>AutoFile</c>, <c>Engines</c>, <c>Events</c>). Emitted
        /// verbatim.
        /// </param>
        /// <param name="gen0Delta">Gen0 collection-count delta across the phase.</param>
        /// <param name="gen1Delta">Gen1 collection-count delta across the phase.</param>
        /// <param name="gen2Delta">Gen2 collection-count delta across the phase.</param>
        /// <param name="allocatedBytesDelta">
        /// The change in <c>GC.GetTotalMemory(false)</c> across the phase, in bytes. May be
        /// negative if a collection during the phase reclaimed more than was allocated.
        /// </param>
        /// <param name="isServerGC">The value of <c>System.Runtime.GCSettings.IsServerGC</c>.</param>
        /// <param name="latencyMode">The value of <c>System.Runtime.GCSettings.LatencyMode</c>.</param>
        public void EmitGcDelta(
            string phase,
            int gen0Delta,
            int gen1Delta,
            int gen2Delta,
            long allocatedBytesDelta,
            bool isServerGC,
            string latencyMode
        )
        {
            _emit(
                "[gc-delta] "
                    + $"phase={phase} "
                    + $"gen0={gen0Delta.ToString(CultureInfo.InvariantCulture)} "
                    + $"gen1={gen1Delta.ToString(CultureInfo.InvariantCulture)} "
                    + $"gen2={gen2Delta.ToString(CultureInfo.InvariantCulture)} "
                    + $"allocatedBytesDelta={allocatedBytesDelta.ToString(CultureInfo.InvariantCulture)} "
                    + $"isServerGC={isServerGC} "
                    + $"latencyMode={latencyMode}"
            );
        }
    }

    /// <summary>
    /// Canonical coarse stage labels for the full add-in-startup-lifetime UI heartbeat
    /// (issue #211, Phase 3.3). The call site in <c>ThisAddIn.Application_Startup</c> references
    /// these constants instead of holding string literals, and the deterministic tests assert the
    /// exact label set. The labels progress in startup order:
    /// <see cref="PreGlobalsCtor"/> -&gt; <see cref="GlobalsCtor"/> -&gt;
    /// <see cref="AwaitingIdleQueue"/> -&gt; <see cref="Loading"/> -&gt; <see cref="PostLoad"/>.
    /// </summary>
    public static class StartupStageLabels
    {
        /// <summary>Before <c>new ApplicationGlobals(...)</c> is constructed.</summary>
        public const string PreGlobalsCtor = "PreGlobalsCtor";

        /// <summary>While the <c>ApplicationGlobals</c> instance is being constructed.</summary>
        public const string GlobalsCtor = "GlobalsCtor";

        /// <summary>After the load lambda is enqueued, waiting for the idle queue to run it.</summary>
        public const string AwaitingIdleQueue = "AwaitingIdleQueue";

        /// <summary>While <c>_globals.LoadAsync(false)</c> is in flight.</summary>
        public const string Loading = "Loading";

        /// <summary>After the "Finished loading globals" log point.</summary>
        public const string PostLoad = "PostLoad";

        /// <summary>
        /// The complete canonical label set, in startup order. Exposed so tests can assert the
        /// exact set without duplicating the literals.
        /// </summary>
        public static readonly System.Collections.Generic.IReadOnlyList<string> All = new[]
        {
            PreGlobalsCtor,
            GlobalsCtor,
            AwaitingIdleQueue,
            Loading,
            PostLoad,
        };
    }

    /// <summary>
    /// Pure, deterministic stop-condition state machine for the full add-in-startup-lifetime UI
    /// heartbeat (issue #211, Phase 3.3). Given per-tick inputs supplied by the
    /// <c>DispatcherTimer</c> seam in <c>ThisAddIn</c>, it decides when the bounded heartbeat
    /// should self-stop. It reads no clock, constructs no timer, performs no I/O, and uses no
    /// banned timing APIs; it holds only a small mutable counter of consecutive responsive ticks.
    /// </summary>
    /// <remarks>
    /// This type is intentionally NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is the coverable
    /// stop-condition seam. The live <c>Stopwatch</c>/<c>DispatcherTimer</c> stay in the
    /// lifecycle-exempt call site, which feeds elapsed/gap values into <see cref="ShouldStop"/>.
    /// </remarks>
    public sealed class StartupLifetimeStopDecider
    {
        private readonly double _maxCapMs;
        private readonly double _responsiveThresholdMs;
        private readonly int _requiredSustainedTicks;
        private int _consecutiveResponsiveTicks;

        /// <summary>
        /// Creates a stop decider with the bounding parameters.
        /// </summary>
        /// <param name="maxCapMs">
        /// Hard upper bound on heartbeat run time, in milliseconds since the heartbeat started.
        /// When elapsed-since-start reaches this cap the heartbeat stops regardless of stage.
        /// Default 180000 ms (~180 s).
        /// </param>
        /// <param name="responsiveThresholdMs">
        /// The maximum <c>gapMs</c> for a tick to count as "responsive" (UI not blocked). Default
        /// 50 ms.
        /// </param>
        /// <param name="requiredSustainedTicks">
        /// The number of consecutive responsive ticks required, after <c>PostLoad</c> is reached,
        /// to conclude startup is settled and stop. Default 8.
        /// </param>
        public StartupLifetimeStopDecider(
            double maxCapMs = 180000.0,
            double responsiveThresholdMs = 50.0,
            int requiredSustainedTicks = 8
        )
        {
            _maxCapMs = maxCapMs;
            _responsiveThresholdMs = responsiveThresholdMs;
            _requiredSustainedTicks = requiredSustainedTicks;
        }

        /// <summary>
        /// The hard upper bound, in milliseconds since heartbeat start, after which the heartbeat
        /// self-stops regardless of stage.
        /// </summary>
        public double MaxCapMs => _maxCapMs;

        /// <summary>The maximum <c>gapMs</c> for a tick to count as responsive.</summary>
        public double ResponsiveThresholdMs => _responsiveThresholdMs;

        /// <summary>
        /// The required run of consecutive responsive ticks (after <c>PostLoad</c>) to stop.
        /// </summary>
        public int RequiredSustainedTicks => _requiredSustainedTicks;

        /// <summary>
        /// The current count of consecutive responsive ticks. Exposed for test assertions; reset to
        /// zero whenever a non-responsive tick is observed.
        /// </summary>
        public int ConsecutiveResponsiveTicks => _consecutiveResponsiveTicks;

        /// <summary>
        /// Records one tick and returns whether the heartbeat should now self-stop. The heartbeat
        /// stops when EITHER (a) <paramref name="elapsedSinceStartMs"/> has reached the max cap, OR
        /// (b) <paramref name="postLoadReached"/> is true AND the most recent run of consecutive
        /// responsive ticks (<c>gapMs &lt;= responsiveThresholdMs</c>) has reached the required
        /// sustained count. A non-responsive tick resets the consecutive-responsive counter so the
        /// sustained run must restart. Before <c>PostLoad</c> is reached, only the max cap can stop
        /// the heartbeat; sustained responsiveness alone does not.
        /// </summary>
        /// <param name="elapsedSinceStartMs">Milliseconds elapsed since the heartbeat started.</param>
        /// <param name="gapMs">The current tick's gap (actual minus nominal interval), in ms.</param>
        /// <param name="postLoadReached">
        /// True once the "Finished loading globals" log point has been passed.
        /// </param>
        /// <returns><see langword="true"/> if the heartbeat should stop; otherwise <see langword="false"/>.</returns>
        public bool ShouldStop(double elapsedSinceStartMs, double gapMs, bool postLoadReached)
        {
            if (gapMs <= _responsiveThresholdMs)
            {
                _consecutiveResponsiveTicks++;
            }
            else
            {
                _consecutiveResponsiveTicks = 0;
            }

            if (elapsedSinceStartMs >= _maxCapMs)
            {
                return true;
            }

            return postLoadReached && _consecutiveResponsiveTicks >= _requiredSustainedTicks;
        }
    }
}
