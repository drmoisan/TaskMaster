using System;
using UtilitiesCS.Interfaces;

namespace UtilitiesCS.Test.TestHelpers
{
    /// <summary>
    /// Deterministic test double for <see cref="ITimerWrapper"/>. It never starts a real
    /// <see cref="System.Timers.Timer"/>; instead it records start/stop state and exposes a
    /// synchronous <see cref="FireElapsed"/> method so a test can raise the <see cref="Elapsed"/>
    /// event on demand. This replaces wall-clock waits (Thread.Sleep / ManualResetEventSlim.Wait)
    /// with deterministic control over when the timer "fires".
    /// </summary>
    /// <remarks>
    /// Supports repeated StartTimer/StopTimer cycles without throwing (Risk R2): production code
    /// such as <c>TimedQueueOfActions</c> and <c>Configuration_PropertyChanged</c> may stop and
    /// recreate or restart the timer. The captured <see cref="Elapsed"/> handlers persist across
    /// cycles so a test may fire after a restart.
    /// </remarks>
    public sealed class ManualFireTimerWrapper : ITimerWrapper
    {
        private bool _disposed;

        /// <summary>Raised synchronously by <see cref="FireElapsed"/>.</summary>
        public event EventHandler<TimeElapsedEventArgs>? Elapsed;

        /// <summary>True after <see cref="StartTimer"/> and before <see cref="StopTimer"/>.</summary>
        public bool Started { get; private set; }

        /// <summary>True after <see cref="StopTimer"/> has been called at least once.</summary>
        public bool Stopped { get; private set; }

        /// <summary>Number of times <see cref="StartTimer"/> has been invoked.</summary>
        public int StartCount { get; private set; }

        /// <summary>Number of times <see cref="FireElapsed"/> has been invoked.</summary>
        public int FireCount { get; private set; }

        /// <summary>
        /// When true, <see cref="StartTimer"/> raises <see cref="Elapsed"/> once synchronously. This
        /// lets a test deterministically produce a non-final tick the moment the production code starts
        /// the timer, without a wall-clock wait. Defaults to false so explicit <see cref="FireElapsed"/>
        /// control is the norm.
        /// </summary>
        public bool FireOnStart { get; set; }

        /// <inheritdoc />
        public bool AutoReset { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public TimeSpan Interval { get; set; }

        /// <inheritdoc />
        public double IntervalInMilliseconds
        {
            get => Interval.TotalMilliseconds;
            set => Interval = TimeSpan.FromMilliseconds(value);
        }

        /// <summary>
        /// Records that the timer was started and marks it enabled. Does NOT fire the
        /// <see cref="Elapsed"/> event; the test controls firing via <see cref="FireElapsed"/>.
        /// </summary>
        public void StartTimer()
        {
            Started = true;
            Enabled = true;
            StartCount++;
            if (FireOnStart)
            {
                FireElapsed();
            }
        }

        /// <summary>Records that the timer was stopped and marks it disabled.</summary>
        public void StopTimer()
        {
            Started = false;
            Stopped = true;
            Enabled = false;
        }

        /// <summary>
        /// Restarts the timer deterministically (stop then start) without raising
        /// <see cref="Elapsed"/>. Mirrors the production ResetTimer semantics.
        /// </summary>
        public void ResetTimer()
        {
            StopTimer();
            StartTimer();
        }

        /// <summary>
        /// Synchronously raises the <see cref="Elapsed"/> event with a fresh
        /// <see cref="TimeElapsedEventArgs"/>, simulating one timer tick. Safe to call multiple
        /// times and across start/stop cycles.
        /// </summary>
        public void FireElapsed()
        {
            FireCount++;
            Elapsed?.Invoke(this, new TimeElapsedEventArgs());
        }

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
