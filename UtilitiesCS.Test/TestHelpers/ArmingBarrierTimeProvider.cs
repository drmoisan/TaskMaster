using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;

namespace UtilitiesCS.Test.TestHelpers
{
    /// <summary>
    /// A <see cref="TimeProvider"/> forwarding every member to an inner
    /// <see cref="FakeTimeProvider"/> unchanged and, after forwarding <c>CreateTimer</c>,
    /// completing a signal that the production loop has armed its next deadline.
    /// </summary>
    /// <remarks>
    /// Forwarding keeps timer ownership with the inner provider. Consecutive <c>Advance</c>
    /// calls are prohibited: each deadline is armed only after the previous proxy faults, so
    /// advancing past a deadline the loop has not created hangs the test. <c>TrySetResult</c>
    /// is used because the loop can arm one more timer than a test drives.
    /// </remarks>
    internal sealed class ArmingBarrierTimeProvider : TimeProvider
    {
        private readonly FakeTimeProvider _inner;
        private volatile TaskCompletionSource<bool> _armed = NewSignal();

        internal ArmingBarrierTimeProvider(FakeTimeProvider inner) => _inner = inner;

        internal Task Armed => _armed.Task;

        internal void ReArm() => _armed = NewSignal();

        internal void Advance(int ms) => _inner.Advance(TimeSpan.FromMilliseconds(ms));

        public override DateTimeOffset GetUtcNow() => _inner.GetUtcNow();

        public override long GetTimestamp() => _inner.GetTimestamp();

        public override TimeZoneInfo LocalTimeZone => _inner.LocalTimeZone;
        public override long TimestampFrequency => _inner.TimestampFrequency;

        public override ITimer CreateTimer(
            TimerCallback callback,
            object state,
            TimeSpan dueTime,
            TimeSpan period
        )
        {
            var timer = _inner.CreateTimer(callback, state, dueTime, period);
            _armed.TrySetResult(true);
            return timer;
        }

        // Continuations run asynchronously so a signal never resumes a test inline.
        internal static TaskCompletionSource<bool> NewSignal() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
