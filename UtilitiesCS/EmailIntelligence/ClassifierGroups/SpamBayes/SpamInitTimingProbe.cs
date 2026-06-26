using System;
using System.Globalization;

namespace UtilitiesCS.EmailIntelligence
{
    /// <summary>
    /// Diagnosis-only, behavior-preserving sub-step attribution probe for issue #211.
    /// Formats and emits one structured <c>[spam-init]</c> line per measured sub-step of
    /// <see cref="SpamBayes.CreateAsync"/> (and per COM folder access in
    /// <c>SpamBayes.ValidatePathsSet</c>) through an injected sink. The sink defaults to the
    /// production <c>log4net</c> logger; tests inject a list-capturing delegate so the
    /// formatting/emission logic is unit-testable without a live appender, live COM, or a live
    /// timer. Mirrors the structure of <c>EngineInitTimingProbe</c> and
    /// <c>StoreFilterAttribution</c>.
    /// </summary>
    /// <remarks>
    /// This type is intentionally NOT marked <c>[ExcludeFromCodeCoverage]</c>: it contains the
    /// coverable line-formatting/emission seam (AC17). The COM-bound folder reads and the
    /// <see cref="System.Diagnostics.Stopwatch"/> wrapping stay in <c>SpamBayes</c>; this helper
    /// only formats the structured line and routes it to the sink. No timing API is used here
    /// (no clock reads, no <see cref="System.Diagnostics.Stopwatch"/>); the caller supplies the
    /// elapsed milliseconds. No banned timing APIs (<c>DateTime.Now</c>, <c>DateTime.UtcNow</c>,
    /// <c>Random.Shared</c>, <c>Thread.Sleep</c>, <c>Task.Delay</c>) are used. No COM, no I/O.
    /// </remarks>
    public sealed class SpamInitTimingProbe
    {
        private readonly Action<string> _emit;

        /// <summary>
        /// Creates a probe that emits structured attribution lines through the supplied sink.
        /// </summary>
        /// <param name="emit">
        /// The line sink. Production passes <c>s =&gt; logger.Debug(s)</c>; tests pass a delegate
        /// that captures lines into a list. Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="emit"/> is null.</exception>
        public SpamInitTimingProbe(Action<string> emit)
        {
            _emit = emit ?? throw new ArgumentNullException(nameof(emit));
        }

        /// <summary>
        /// Formats a single structured attribution line for a named sub-step and its measured cost.
        /// </summary>
        /// <param name="step">
        /// The sub-step name (for example, <c>ValidatePathsSet</c> or
        /// <c>ValidatePathsSet.JunkCertain</c>). Must not be null.
        /// </param>
        /// <param name="ms">The measured wall-clock duration of the sub-step, in milliseconds.</param>
        /// <returns>
        /// The line <c>[spam-init] step=&lt;step&gt; ms=&lt;ms:F1&gt;</c>, formatted with
        /// <see cref="CultureInfo.InvariantCulture"/> so the decimal separator is deterministic.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null.</exception>
        public string FormatStep(string step, double ms)
        {
            if (step is null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "[spam-init] step={0} ms={1:F1}",
                step,
                ms
            );
        }

        /// <summary>
        /// Formats the attribution line for the given sub-step and routes it to the injected sink.
        /// </summary>
        /// <param name="step">The sub-step name. Must not be null.</param>
        /// <param name="ms">The measured wall-clock duration of the sub-step, in milliseconds.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="step"/> is null.</exception>
        public void EmitStep(string step, double ms)
        {
            _emit(FormatStep(step, ms));
        }
    }
}
