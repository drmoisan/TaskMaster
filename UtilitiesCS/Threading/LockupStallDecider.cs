#nullable enable
using System;

namespace UtilitiesCS.Threading
{
    /// <summary>
    /// Immutable value carrying the outcome of a confirmed UI lockup: how long the UI thread has
    /// been unresponsive and the identity of the store being processed when the lockup was
    /// confirmed (issue #264). Read on the watchdog's background thread and handed to the
    /// disable-then-notify orchestrator.
    /// </summary>
    /// <remarks>
    /// Declared as a plain <c>readonly struct</c> with an ordinary constructor and get-only
    /// properties rather than a <c>record struct</c> or a type with an <c>init</c> accessor, because
    /// <c>init</c> accessors require <c>System.Runtime.CompilerServices.IsExternalInit</c>, which is
    /// not available on this .NET Framework 4.8 target (CS0518). Mirrors the <c>StoreIdentity</c> /
    /// <c>DisabledStoreEntry</c> pattern in the same assembly.
    /// </remarks>
    public readonly struct LockupAttribution
    {
        /// <summary>Creates an attribution from the measured stall duration and store identity.</summary>
        /// <param name="stallDuration">How long the UI thread has been unresponsive.</param>
        /// <param name="storeIdentity">
        /// The identity of the store being processed at the moment of confirmation, or
        /// <see langword="null"/> when no per-store context was active ("no context").
        /// </param>
        public LockupAttribution(TimeSpan stallDuration, string? storeIdentity)
        {
            StallDuration = stallDuration;
            StoreIdentity = storeIdentity;
        }

        /// <summary>How long the UI thread has been unresponsive when the lockup was confirmed.</summary>
        public TimeSpan StallDuration { get; }

        /// <summary>
        /// The identity of the store being processed at confirmation, or <see langword="null"/>
        /// when no per-store context was active.
        /// </summary>
        public string? StoreIdentity { get; }
    }

    /// <summary>
    /// Pure, deterministic stall-confirmation helper for the UI-lockup watchdog (issue #264),
    /// following the repository's <c>StartupLifetimeStopDecider</c> split. Given an elapsed
    /// unresponsive duration in milliseconds, it decides whether the configured
    /// lockup-attribution threshold has been crossed. It reads no clock, performs no COM access,
    /// holds no <c>Dispatcher</c>/<c>TimeProvider</c> field, and is intentionally NOT marked
    /// <c>[ExcludeFromCodeCoverage]</c>: it is the coverable decision seam. The live polling loop in
    /// <see cref="ThreadMonitor"/> feeds elapsed values into <see cref="IsStallConfirmed"/>.
    /// </summary>
    public sealed class LockupStallDecider
    {
        private readonly double _lockupAttributionThresholdMs;

        /// <summary>
        /// Creates a decider with the lockup-attribution threshold.
        /// </summary>
        /// <param name="lockupAttributionThresholdMs">
        /// The unresponsive duration, in milliseconds, at or beyond which a stall is confirmed as a
        /// lockup. Distinct from the smaller diagnostic stack-trace cadence threshold.
        /// </param>
        public LockupStallDecider(double lockupAttributionThresholdMs)
        {
            _lockupAttributionThresholdMs = lockupAttributionThresholdMs;
        }

        /// <summary>The lockup-attribution threshold, in milliseconds.</summary>
        public double ThresholdMs => _lockupAttributionThresholdMs;

        /// <summary>
        /// Returns whether an elapsed unresponsive duration confirms a lockup. Boundary contract:
        /// the stall is confirmed when <paramref name="elapsedMs"/> is greater than OR EQUAL to the
        /// threshold; an elapsed value strictly below the threshold (including zero or negative) is
        /// not confirmed.
        /// </summary>
        /// <param name="elapsedMs">The elapsed unresponsive duration in milliseconds.</param>
        /// <returns><see langword="true"/> when the threshold is reached or exceeded; otherwise <see langword="false"/>.</returns>
        public bool IsStallConfirmed(double elapsedMs)
        {
            return elapsedMs >= _lockupAttributionThresholdMs;
        }
    }
}
