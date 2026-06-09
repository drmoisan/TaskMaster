using System;
using System.Timers;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.TestHelpers
{
    /// <summary>
    /// Deterministic test double for the INNER timer seam consumed by
    /// <see cref="TimerWrapper"/> (<c>TimerWrapper.IInnerTimer</c>). It never starts a real
    /// <see cref="System.Timers.Timer"/>; instead it records start/stop/configuration state and
    /// exposes a synchronous <see cref="FireElapsed"/> method that raises the inner
    /// <see cref="Elapsed"/> event on demand. This replaces wall-clock waits
    /// (signal.Wait) with deterministic control over when the underlying timer "fires".
    /// </summary>
    /// <remarks>
    /// This is SEPARATE from <see cref="ManualFireTimerWrapper"/>, which implements the OUTER
    /// <see cref="UtilitiesCS.Interfaces.ITimerWrapper"/> contract. This fake implements the inner
    /// abstraction that <see cref="TimerWrapper"/> wraps, so a test can construct a real
    /// <see cref="TimerWrapper"/> over a controllable inner timer and assert the wrapper's
    /// Elapsed-forwarding, stop-suppression, and AutoReset semantics.
    /// </remarks>
    internal sealed class ManualFireInnerTimer : TimerWrapper.IInnerTimer
    {
        private bool _disposed;

        /// <summary>Raised synchronously by <see cref="FireElapsed"/>.</summary>
        public event ElapsedEventHandler Elapsed;

        /// <summary>True after <see cref="Start"/> and before <see cref="Stop"/>.</summary>
        public bool Started { get; private set; }

        /// <summary>True after <see cref="Stop"/> has been called at least once.</summary>
        public bool Stopped { get; private set; }

        /// <summary>Number of times <see cref="Start"/> has been invoked.</summary>
        public int StartCount { get; private set; }

        /// <summary>Number of times <see cref="FireElapsed"/> has been invoked.</summary>
        public int FireCount { get; private set; }

        /// <inheritdoc />
        public bool AutoReset { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public double Interval { get; set; }

        /// <inheritdoc />
        public void Start()
        {
            Started = true;
            Enabled = true;
            StartCount++;
        }

        /// <inheritdoc />
        public void Stop()
        {
            Started = false;
            Stopped = true;
            Enabled = false;
        }

        /// <summary>
        /// Synchronously raises the inner <see cref="Elapsed"/> event with a fresh
        /// <see cref="ElapsedEventArgs"/>, simulating one underlying timer tick. The forwarding
        /// <c>TimerWrapper.WhenTimerElapsed</c> handler runs inline on the caller's thread, so the
        /// outer <c>TimerWrapper.Elapsed</c> is raised deterministically without any wall-clock wait.
        /// </summary>
        public void FireElapsed()
        {
            FireCount++;
            Elapsed?.Invoke(this, CreateElapsedEventArgs());
        }

        /// <summary>
        /// Constructs an <see cref="ElapsedEventArgs"/> deterministically. The type has no public
        /// constructor on this framework, so it is created uninitialized (the same pattern used
        /// elsewhere in this test project for constructor-less types). The wrapper only copies
        /// <c>SignalTime</c> into its outer event args, and the B1-B3 assertions do not depend on a
        /// specific signal-time value, so the default value is acceptable and fully deterministic
        /// (no clock dependency).
        /// </summary>
        private static ElapsedEventArgs CreateElapsedEventArgs() =>
            (ElapsedEventArgs)
                System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                    typeof(ElapsedEventArgs)
                );

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Elapsed = null;
            _disposed = true;
        }
    }
}
