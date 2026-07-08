using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Deterministic unit tests for the pure <see cref="LockupStallDecider"/> (issue #264). No
    /// clock, COM, or threading dependency: the decision is a function of the elapsed-ms input and
    /// the injected threshold only. The boundary contract (confirmed at elapsed &gt;= threshold) is
    /// asserted explicitly.
    /// </summary>
    [TestClass]
    public class LockupStallDeciderTests
    {
        private const double ThresholdMs = 5000.0;

        [TestMethod]
        public void ThresholdMs_ReflectsConstructorArgument()
        {
            // Arrange & Act
            var decider = new LockupStallDecider(ThresholdMs);

            // Assert
            decider.ThresholdMs.Should().Be(ThresholdMs);
        }

        [TestMethod]
        public void IsStallConfirmed_ElapsedStrictlyBelowThreshold_ReturnsFalse()
        {
            // Arrange
            var decider = new LockupStallDecider(ThresholdMs);

            // Act & Assert
            decider.IsStallConfirmed(4999.9).Should().BeFalse();
        }

        [TestMethod]
        public void IsStallConfirmed_ElapsedExactlyAtBoundary_ReturnsTrue()
        {
            // Arrange
            var decider = new LockupStallDecider(ThresholdMs);

            // Act & Assert: documented boundary contract is confirmed at elapsed >= threshold.
            decider.IsStallConfirmed(ThresholdMs).Should().BeTrue();
        }

        [TestMethod]
        public void IsStallConfirmed_ElapsedAboveThreshold_ReturnsTrue()
        {
            // Arrange
            var decider = new LockupStallDecider(ThresholdMs);

            // Act & Assert
            decider.IsStallConfirmed(5000.1).Should().BeTrue();
        }

        [TestMethod]
        public void IsStallConfirmed_LargeElapsed_ReturnsTrue()
        {
            // Arrange
            var decider = new LockupStallDecider(ThresholdMs);

            // Act & Assert
            decider.IsStallConfirmed(600000.0).Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(0.0)]
        [DataRow(-1.0)]
        [DataRow(-100000.0)]
        public void IsStallConfirmed_ZeroOrNegativeElapsed_ReturnsFalse(double elapsedMs)
        {
            // Arrange
            var decider = new LockupStallDecider(ThresholdMs);

            // Act & Assert: a zero or negative elapsed value is never a confirmed lockup.
            decider.IsStallConfirmed(elapsedMs).Should().BeFalse();
        }
    }
}
