using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class Prediction_Tests
    {
        [TestMethod]
        public void Constructor_AssignsClassAndProbability()
        {
            // Arrange

            // Act
            var prediction = new Prediction<string>("Inbox", 0.75d);

            // Assert
            prediction.Class.Should().Be("Inbox");
            prediction.Probability.Should().Be(0.75d);
        }

        [TestMethod]
        public void PropertySetters_AllowMutationIncludingNullClassAndZeroProbability()
        {
            // Arrange
            var prediction = new Prediction<string>();

            // Act
            prediction.Class = null;
            prediction.Probability = 0d;

            // Assert
            prediction.Class.Should().BeNull();
            prediction.Probability.Should().Be(0d);
        }

        [TestMethod]
        public void CompareTo_ReturnsPositiveWhenOtherIsNull()
        {
            // Arrange
            var prediction = new Prediction<string>("Archive", 0.25d);

            // Act
            var result = prediction.CompareTo(null);

            // Assert
            result.Should().Be(1);
        }

        [TestMethod]
        public void CompareTo_OrdersByProbability()
        {
            // Arrange
            var low = new Prediction<string>("Low", 0.1d);
            var high = new Prediction<string>("High", 0.9d);

            // Act
            var lowComparedToHigh = low.CompareTo(high);
            var highComparedToLow = high.CompareTo(low);

            // Assert
            lowComparedToHigh.Should().BeNegative();
            highComparedToLow.Should().BePositive();
        }

        [TestMethod]
        public void CompareTo_HandlesBoundaryProbabilityValues()
        {
            // Arrange
            var zero = new Prediction<string>("Zero", double.MinValue);
            var max = new Prediction<string>("Max", double.MaxValue);

            // Act
            var comparison = zero.CompareTo(max);

            // Assert
            comparison.Should().BeNegative();
        }
    }
}
