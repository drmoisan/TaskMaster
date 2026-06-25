using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class DeadlineClockTests
    {
        [TestMethod]
        public void ShouldYield_ZeroInterval_ReturnsTrueWithoutWallClock()
        {
            var clock = new DeadlineClock(TimeSpan.Zero);

            clock.ShouldYield().Should().BeTrue();
        }

        [TestMethod]
        public void ShouldYield_LongInterval_ReturnsFalseWithoutWaiting()
        {
            var clock = new DeadlineClock(TimeSpan.FromDays(1));

            clock.ShouldYield().Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_NegativeInterval_Throws()
        {
            Action act = () => new DeadlineClock(TimeSpan.FromMilliseconds(-1));

            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("yieldInterval");
        }

        [TestMethod]
        public void Reset_PositiveInterval_RestartsNoYieldWindow()
        {
            var clock = new DeadlineClock(TimeSpan.FromDays(1));
            clock.ShouldYield().Should().BeFalse();

            clock.Reset();

            clock.ShouldYield().Should().BeFalse();
        }
    }
}
