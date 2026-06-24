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
}
