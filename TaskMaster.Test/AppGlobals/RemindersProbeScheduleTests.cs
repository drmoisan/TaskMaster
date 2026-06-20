using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Unit tests for <see cref="RemindersProbeSchedule"/>, the pure decision/scheduling seam for
    /// the Issue #207 increment-3 <c>OlReminders</c> latency probe. All inputs are plain integers
    /// and all assertions are deterministic; no COM, no live timer, no clock, no network, no
    /// filesystem, and no temporary files are used.
    /// </summary>
    [TestClass]
    public class RemindersProbeScheduleTests
    {
        [TestMethod]
        public void Constructor_WithDefaultZero_DoesNotDeferAndResolvesToZeroDelay()
        {
            // Arrange / Act: the default user-setting value is 0 (no probe).
            var schedule = new RemindersProbeSchedule(0);

            // Assert: behavior-preserving synchronous path is selected (ShouldDefer == false).
            schedule
                .ShouldDefer.Should()
                .BeFalse("the default 0 value must preserve synchronous behavior");
            schedule.Delay.Should().Be(TimeSpan.Zero);
        }

        [TestMethod]
        public void Constructor_WithPositiveValue_DefersByThatManySeconds()
        {
            // Arrange / Act: a positive configured value defers the first access.
            var schedule = new RemindersProbeSchedule(30);

            // Assert
            schedule
                .ShouldDefer.Should()
                .BeTrue("a value greater than 0 must defer the first access");
            schedule.Delay.Should().Be(TimeSpan.FromSeconds(30));
        }

        [TestMethod]
        public void Constructor_WithBoundaryValueOne_DefersByOneSecond()
        {
            // Arrange / Act: the smallest deferring value (boundary at 1).
            var schedule = new RemindersProbeSchedule(1);

            // Assert
            schedule.ShouldDefer.Should().BeTrue("1 is the smallest value that must defer");
            schedule.Delay.Should().Be(TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public void Constructor_WithNegativeValue_DoesNotDeferAndResolvesToZeroDelay()
        {
            // Arrange / Act: a negative configured value must be treated as no-defer.
            var schedule = new RemindersProbeSchedule(-5);

            // Assert
            schedule.ShouldDefer.Should().BeFalse("negative values must not defer");
            schedule.Delay.Should().Be(TimeSpan.Zero);
        }
    }
}
