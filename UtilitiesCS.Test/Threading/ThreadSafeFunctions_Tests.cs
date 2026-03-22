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

        [TestMethod]
        public void IncrementThreadSafe_NoLimit_IncrementsBy1()
        {
            // Arrange
            double value = 5d;

            // Act
            ThreadSafeFunctions.IncrementThreadSafe(ref value);

            // Assert
            value.Should().Be(6d);
        }

        [TestMethod]
        public void DecrementThreadSafe_NoLimit_DecrementsBy1()
        {
            // Arrange
            double value = 5d;

            // Act
            ThreadSafeFunctions.DecrementThreadSafe(ref value);

            // Assert
            value.Should().Be(4d);
        }

        [TestMethod]
        public void IncrementThreadSafe_WithMaxValueAndAttempts_ClampsAtMax()
        {
            // Arrange
            double value = 10d;

            // Act
            ThreadSafeFunctions.IncrementThreadSafe(ref value, 10d, 50);

            // Assert
            value.Should().Be(10d);
        }

        [TestMethod]
        public void DecrementThreadSafe_WithMinValueAndAttempts_ClampsAtMin()
        {
            // Arrange
            double value = 0d;

            // Act
            ThreadSafeFunctions.DecrementThreadSafe(ref value, 0d, 50);

            // Assert
            value.Should().Be(0d);
        }

        [TestMethod]
        public void AddThreadSafe_WithLimit_ClampsAtLimit()
        {
            // Arrange
            double value = 8d;

            // Act – add 5 with limit of 10
            ThreadSafeFunctions.AddThreadSafe(ref value, 5d, 10d, 100);

            // Assert
            value.Should().Be(10d);
        }

        [TestMethod]
        public void SubtractThreadSafe_Double_ClampsAtLimit()
        {
            // Arrange
            double value = 3d;

            // Act – subtract 5 with limit of 0
            ThreadSafeFunctions.SubtractThreadSafe(ref value, 5d, 0d, 100);

            // Assert
            value.Should().Be(0d);
        }

        [TestMethod]
        public void SubtractThreadSafe_Int_ClampsAtLimit()
        {
            // Arrange
            int value = 2;

            // Act – subtract 5 with limit of 0
            ThreadSafeFunctions.SubtractThreadSafe(ref value, 5, 0, 100);

            // Assert
            value.Should().Be(0);
        }

        [TestMethod]
        public void SubtractThreadSafe_Int_SubtractsWithinRange()
        {
            // Arrange
            int value = 10;

            // Act
            ThreadSafeFunctions.SubtractThreadSafe(ref value, 3, 0, 100);

            // Assert
            value.Should().Be(7);
        }

        [TestMethod]
        public void AdjustThreadSafe_WithMaxAttempts_ShouldThrow_WhenMaxAttemptsIsLessThanOne()
        {
            // Arrange
            double value = 1d;

            // Act
            Action act = () =>
                ThreadSafeFunctions.AdjustThreadSafe(ref value, v => v + 1, v => v, 0);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*maxAttempts*");
        }

        [TestMethod]
        public void AddThreadSafe_WithLimit_ThrowsWhenMaxAttemptsLessThan1()
        {
            // Arrange
            double value = 1d;

            // Act
            Action act = () => ThreadSafeFunctions.AddThreadSafe(ref value, 1d, 100d, 0);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*maxAttempts*");
        }

        [TestMethod]
        public void SubtractThreadSafe_Double_ThrowsWhenMaxAttemptsLessThan1()
        {
            // Arrange
            double value = 1d;

            // Act
            Action act = () => ThreadSafeFunctions.SubtractThreadSafe(ref value, 1d, 0d, 0);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*maxAttempts*");
        }

        [TestMethod]
        public void SubtractThreadSafe_Int_ThrowsWhenMaxAttemptsLessThan1()
        {
            // Arrange
            int value = 1;

            // Act
            Action act = () => ThreadSafeFunctions.SubtractThreadSafe(ref value, 1, 0, 0);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*maxAttempts*");
        }
    }
}
