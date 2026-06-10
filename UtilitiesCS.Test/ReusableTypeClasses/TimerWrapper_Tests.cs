using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Test.TestHelpers;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    // B1-B3 assert that the TimerWrapper raises, suppresses, and configures its outer Elapsed
    // event correctly. They drive a deterministic manual-fire inner-timer fake (no real
    // System.Timers.Timer, no wall-clock wait) so the assertions are exact and stable. The
    // remaining tests still construct a real System.Timers.Timer-backed wrapper but only inspect
    // synchronous state (interval, enabled, dispose) without waiting on the OS timer.
    // [DoNotParallelize] is retained: the constructor tests touch a real System.Timers.Timer.
    [DoNotParallelize]
    [TestClass]
    public class TimerWrapper_Tests
    {
        [TestMethod]
        public void Constructor_SetsIntervalAndStartsDisabled()
        {
            // Arrange
            var interval = TimeSpan.FromMilliseconds(50);

            // Act
            using var timer = new TimerWrapper(interval);

            // Assert
            timer.Interval.Should().Be(interval);
            timer.Enabled.Should().BeFalse();
        }

        [TestMethod]
        public void StartTimer_RaisesElapsedEvent()
        {
            // Arrange: inject a deterministic inner-timer fake so the underlying tick can be fired
            // synchronously instead of waiting on a real System.Timers.Timer.
            var fake = new ManualFireInnerTimer();
            using var timer = new TimerWrapper(fake);
            timer.AutoReset = false;
            var raisedCount = 0;
            object raisedSender = null;
            timer.Elapsed += (sender, _) =>
            {
                raisedCount++;
                raisedSender = sender;
            };

            // Act: start the wrapper, then deterministically fire the inner timer once.
            timer.StartTimer();
            fake.FireElapsed();

            // Assert: the wrapper forwarded the inner tick to its outer Elapsed event exactly once,
            // raising it with itself as the sender.
            raisedCount.Should().Be(1);
            raisedSender.Should().BeSameAs(timer);
            fake.Started.Should().BeTrue();
        }

        [TestMethod]
        public void StopTimer_PreventsPendingElapsedEvent()
        {
            // Arrange: inject the deterministic inner-timer fake.
            var fake = new ManualFireInnerTimer();
            using var timer = new TimerWrapper(fake);
            timer.AutoReset = false;
            var raisedCount = 0;
            timer.Elapsed += (_, _) => raisedCount++;

            // Act: start then stop the wrapper. StopTimer must propagate to the inner timer so the
            // underlying timer is disabled and cannot raise a pending tick.
            timer.StartTimer();
            timer.StopTimer();

            // Assert: stop is forwarded to the inner timer (Stopped, not Enabled), so no pending
            // outer Elapsed can fire after stop. The outer event was never raised.
            fake.Stopped.Should().BeTrue();
            fake.Enabled.Should().BeFalse();
            raisedCount.Should().Be(0);
        }

        [TestMethod]
        public void StartNew_ConfiguresAutoResetAndInvokesCallback()
        {
            // Arrange: drive StartNew through the inner-timer seam so AutoReset configuration and
            // callback invocation are observed deterministically (no wall-clock wait).
            var fake = new ManualFireInnerTimer();
            var callbackCount = 0;

            // Act
            using var timer = TimerWrapper.StartNew(
                fake,
                autoReset: false,
                callback: () => callbackCount++
            );

            // Assert: AutoReset is configured (false) on both the wrapper and the inner fake, the
            // inner timer was started, and firing the inner tick invokes the callback exactly once.
            timer.AutoReset.Should().BeFalse();
            fake.AutoReset.Should().BeFalse();
            fake.Started.Should().BeTrue();

            fake.FireElapsed();
            callbackCount.Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithZeroInterval_ThrowsArgumentException()
        {
            // Act
            Action act = () => _ = new TimerWrapper(TimeSpan.Zero);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Dispose_CanBeCalledMultipleTimesWithoutThrowing()
        {
            // Arrange
            var timer = new TimerWrapper(TimeSpan.FromMilliseconds(20));

            // Act
            Action act = () =>
            {
                timer.Dispose();
                timer.Dispose();
            };

            // Assert
            act.Should().NotThrow();
        }
    }
}
