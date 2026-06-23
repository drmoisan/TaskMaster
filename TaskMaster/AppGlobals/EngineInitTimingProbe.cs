using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// Diagnosis-only, behavior-preserving per-engine attribution probe for issue #211.
    /// Times the per-engine factory invocation and the upfront <c>Configuration</c> deserialize
    /// in <see cref="AppItemEngines.InitAsync"/> and emits one structured line per measurement
    /// through an injected sink. The sink defaults to the production <c>log4net</c> logger; tests
    /// inject a list-capturing delegate so timing/emission logic is unit-testable without a live
    /// appender, live COM, or a live timer. Mirrors the Phase 1 continuation-resume probe.
    /// </summary>
    /// <remarks>
    /// This type is intentionally NOT marked <c>[ExcludeFromCodeCoverage]</c>: it contains the
    /// coverable timing/emission seam (AC8). The COM-bound engine factory invocation stays in
    /// <see cref="AppItemEngines"/>; this helper only wraps the awaited factory call with a
    /// <see cref="Stopwatch"/> and formats the structured line. Timing uses <see cref="Stopwatch"/>
    /// only; no banned timing APIs (<c>DateTime.Now</c>, <c>DateTime.UtcNow</c>, <c>Random.Shared</c>,
    /// <c>Thread.Sleep</c>, <c>Task.Delay</c>) are used.
    /// </remarks>
    public sealed class EngineInitTimingProbe
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
        public EngineInitTimingProbe(Action<string> emit)
        {
            _emit = emit ?? throw new ArgumentNullException(nameof(emit));
        }

        /// <summary>
        /// Times a single engine factory invocation and emits one <c>[engine-init]</c> line.
        /// </summary>
        /// <param name="engineName">The engine key being initialized (for example, <c>Spam</c>).</param>
        /// <param name="factory">
        /// The bound factory call that creates the engine. Awaited exactly once. If the factory
        /// throws, the exception propagates (fail-fast); no line is emitted for the failed call,
        /// preserving the pre-instrumentation propagation behavior.
        /// </param>
        /// <returns>The engine produced by the factory (may be null).</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="engineName"/> or <paramref name="factory"/> is null.
        /// </exception>
        public async Task<IConditionalEngine<MailItemHelper>?> TimeEngineAsync(
            string engineName,
            Func<Task<IConditionalEngine<MailItemHelper>?>> factory
        )
        {
            if (engineName is null)
            {
                throw new ArgumentNullException(nameof(engineName));
            }
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var stopwatch = Stopwatch.StartNew();
            var engine = await factory();
            stopwatch.Stop();

            var engineNull = engine is null;
            var costHint = engineNull ? "Skip" : "Deserialization";
            _emit(
                $"[engine-init] engineName={engineName} "
                    + $"engineMs={stopwatch.Elapsed.TotalMilliseconds:F1} "
                    + $"engineNull={engineNull} "
                    + $"threadId={Thread.CurrentThread.ManagedThreadId} "
                    + $"costHint={costHint}"
            );

            return engine;
        }

        /// <summary>
        /// Emits one <c>[engine-init-config]</c> line attributing the upfront
        /// <c>Globals.AF.Manager.Configuration</c> deserialize cost (research Candidate 2).
        /// </summary>
        /// <param name="configMs">The measured wall-clock duration of the Configuration await, in ms.</param>
        /// <param name="threadId">The managed id of the thread that resolved the Configuration await.</param>
        public void EmitConfigTiming(double configMs, int threadId)
        {
            _emit($"[engine-init-config] configMs={configMs:F1} threadId={threadId}");
        }
    }
}
