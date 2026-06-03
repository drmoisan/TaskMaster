using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    // These tests assert that a System.Timers.Timer-backed wrapper raises (or suppresses)
    // its Elapsed callback within a bounded wait. That callback runs on a ThreadPool thread,
    // so under class-level parallel execution a saturated ThreadPool can delay it past the
    // wait window and make the test fail intermittently. Running this class in the
    // non-parallel phase removes the contention, matching ApplicationIdleTimer_Tests.
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
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            using var timer = new TimerWrapper(TimeSpan.FromMilliseconds(20));
            timer.AutoReset = false;
            timer.Elapsed += (_, _) => signal.Set();

            // Act
            timer.StartTimer();

            // Assert
            signal.Wait(500).Should().BeTrue();
        }

        [TestMethod]
        public void StopTimer_PreventsPendingElapsedEvent()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            using var timer = new TimerWrapper(TimeSpan.FromMilliseconds(150));
            timer.AutoReset = false;
            timer.Elapsed += (_, _) => signal.Set();

            // Act
            timer.StartTimer();
            timer.StopTimer();

            // Assert
            signal.Wait(250).Should().BeFalse();
        }

        [TestMethod]
        public void StartNew_ConfiguresAutoResetAndInvokesCallback()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);

            // Act
            using var timer = TimerWrapper.StartNew(
                TimeSpan.FromMilliseconds(20),
                autoReset: false,
                callback: signal.Set
            );

            // Assert
            timer.AutoReset.Should().BeFalse();
            signal.Wait(500).Should().BeTrue();
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
