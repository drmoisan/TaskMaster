using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;

namespace QuickFiler.Test.TestSupport
{
    /// <summary>
    /// A <see cref="FakeTimeProvider"/> that completes a signal after every <see cref="CreateTimer"/>
    /// call, so a test can prove that a production loop armed its next wait instead of returning,
    /// without a clock advance followed by a yield or a bounded retry.
    /// </summary>
    /// <remarks>
    /// Issue #968. <see cref="Armed"/> completes once the first timer after construction, or after
    /// the last <see cref="ReArm"/>, has been created; <c>TrySetResult</c> is used because a loop
    /// can arm one more timer than a test drives. Signals run their continuations asynchronously
    /// so a test never resumes inside the production <c>CreateTimer</c> call. Consecutive
    /// <c>Advance</c> calls without awaiting <see cref="Armed"/> in between are prohibited: a
    /// deadline the loop has not yet created is not advanced past, and the test would then wait on
    /// a timer that never fires. Modelled on UtilitiesCS.Test ArmingBarrierTimeProvider, as a
    /// subclass rather than a forwarding decorator because this project already subclasses
    /// <see cref="FakeTimeProvider"/>.
    /// </remarks>
    internal sealed class ArmingFakeTimeProvider : FakeTimeProvider
    {
        private volatile TaskCompletionSource<bool> _armed = NewSignal();

        /// <summary>Completes after the next <see cref="CreateTimer"/> call since the last re-arm.</summary>
        internal Task Armed => _armed.Task;

        /// <summary>Replaces the signal so the next <see cref="CreateTimer"/> call completes a fresh task.</summary>
        internal void ReArm() => _armed = NewSignal();

        public override ITimer CreateTimer(
            TimerCallback callback,
            object state,
            TimeSpan dueTime,
            TimeSpan period
        )
        {
            ITimer timer = base.CreateTimer(callback, state, dueTime, period);
            _armed.TrySetResult(true);
            return timer;
        }

        private static TaskCompletionSource<bool> NewSignal() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
