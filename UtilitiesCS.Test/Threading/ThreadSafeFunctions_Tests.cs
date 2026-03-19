using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class ThreadSafeFunctions_Tests
    {
        [TestMethod]
        public void AddThreadSafe_ShouldAddRequestedAmount()
        {
            // Arrange
            double value = 1.5d;

            // Act
            ThreadSafeFunctions.AddThreadSafe(ref value, 2.25d);

            // Assert
            value.Should().BeApproximately(3.75d, 0.0001d);
        }

        [TestMethod]
        public void IncrementAndDecrementThreadSafe_ShouldRespectConfiguredBounds()
        {
            // Arrange
            double upperBoundValue = 9d;
            double lowerBoundValue = 1d;

            // Act
            ThreadSafeFunctions.IncrementThreadSafe(ref upperBoundValue, 9d);
            ThreadSafeFunctions.DecrementThreadSafe(ref lowerBoundValue, 1d);

            // Assert
            upperBoundValue.Should().Be(9d);
            lowerBoundValue.Should().Be(1d);
        }

        [TestMethod]
        public void AdjustThreadSafe_ShouldApplyAdjustmentThenLimit()
        {
            // Arrange
            double value = 10d;

            // Act
            ThreadSafeFunctions.AdjustThreadSafe(
                ref value,
                current => current * 3,
                adjusted => Math.Min(adjusted, 25d)
            );

            // Assert
            value.Should().Be(25d);
        }

        [TestMethod]
        public void AddThreadSafe_ShouldThrow_WhenMaxAttemptsIsLessThanOne()
        {
            // Arrange
            double value = 1d;

            // Act
            Action act = () => ThreadSafeFunctions.AddThreadSafe(ref value, 1d, 0);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*maxAttempts*");
        }

        [TestMethod]
        public void AddThreadSafe_ShouldHandleConcurrentCalls()
        {
            // Arrange
            double value = 0d;

            // Act
            Parallel.ForEach(
                Enumerable.Range(0, 100),
                _ => ThreadSafeFunctions.AddThreadSafe(ref value, 1d)
            );

            // Assert
            value.Should().Be(100d);
        }
    }
}
