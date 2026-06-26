using System.Threading;

namespace UtilitiesCS.OutlookObjects.Store
{
    /// <summary>
    /// Process-global, thread-safe accumulator of the total wall-clock time (in milliseconds)
    /// spent inside <see cref="StoreWrapper.Init"/> across all store wrappers in the process
    /// (issue #211, Phase 3.6).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Purpose: diagnose the maintainer hypothesis that <see cref="StoreWrapper.Init"/> is a
    /// SHARED blocking cost (the failing-store logon) absorbed by whichever startup phase timer is
    /// running when it first fires. A single process-global accumulator lets the per-phase NET
    /// attribution in <c>ApplicationGlobals.LoadSequentialAsync</c> sample the cost before/after each
    /// phase regardless of which <see cref="StoreWrapper"/> instance produced it. Because store
    /// wrappers are created independently and the per-phase read is cross-instance, the accumulator
    /// is intentionally a static (process-global) value rather than instance state.
    /// </para>
    /// <para>
    /// Thread-safety contract: <see cref="Add"/> uses <see cref="Interlocked.Add(ref long, long)"/>
    /// so concurrent store inits on background threads accumulate without lost updates;
    /// <see cref="TotalMs"/> reads an atomic snapshot via <see cref="Interlocked.Read(ref long)"/>;
    /// <see cref="Reset"/> uses <see cref="Interlocked.Exchange(ref long, long)"/>. The counter is
    /// stored in whole microseconds to keep the accumulation integral and overflow-safe.
    /// </para>
    /// <para>
    /// This type performs no <see cref="System.Diagnostics.Stopwatch"/> reads, no clock reads, no COM
    /// access, and no I/O, so it is unit-testable without a live Outlook host and is intentionally NOT
    /// marked <c>[ExcludeFromCodeCoverage]</c>.
    /// </para>
    /// </remarks>
    public static class StoreWrapperInitClock
    {
        private static long _microseconds;

        /// <summary>
        /// Adds the supplied elapsed milliseconds to the process-global accumulator. Negative values
        /// are treated as <c>0</c> (the accumulator never decreases). The value is converted to whole
        /// microseconds before being added atomically.
        /// </summary>
        /// <param name="ms">Elapsed milliseconds to add. Values &lt; 0 are clamped to 0.</param>
        public static void Add(double ms)
        {
            if (ms < 0)
            {
                ms = 0;
            }

            long microseconds = (long)(ms * 1000.0);
            Interlocked.Add(ref _microseconds, microseconds);
        }

        /// <summary>
        /// Gets the current accumulated total, in milliseconds, as an atomic snapshot.
        /// </summary>
        public static double TotalMs => Interlocked.Read(ref _microseconds) / 1000.0;

        /// <summary>
        /// Resets the accumulator to zero. Intended for deterministic test isolation; the accumulator
        /// is process-global static state shared across all callers.
        /// </summary>
        public static void Reset()
        {
            Interlocked.Exchange(ref _microseconds, 0);
        }
    }
}
