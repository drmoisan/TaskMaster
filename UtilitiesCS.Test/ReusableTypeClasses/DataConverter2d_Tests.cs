using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Matrices;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class DataConverter2d_Tests
    {
        [TestMethod]
        public void ToDouble_WithJaggedIntArray_NormalizesEachValueBy255()
        {
            // Arrange
            var image = new[] { new[] { 0, 255 }, new[] { 128, 64 } };

            // Act
            var result = DataConverter2d.ToDouble(image);

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().Equal(0d, 1d);
            result[1][0].Should().BeApproximately(128d / 255d, 0.0000001d);
            result[1][1].Should().BeApproximately(64d / 255d, 0.0000001d);
        }

        [TestMethod]
        public void ToDouble_WithMatrix_NormalizesEachValueBy255()
        {
            // Arrange
            var image = new Matrix<int>(2, 2);
            image[0, 0] = 0;
            image[1, 0] = 255;
            image[0, 1] = 128;
            image[1, 1] = 64;

            // Act
            var result = DataConverter2d.ToDouble(image);

            // Assert
            result.Width.Should().Be(2);
            result.Height.Should().Be(2);
            result[0, 0].Should().Be(0d);
            result[1, 0].Should().Be(1d);
            result[0, 1].Should().BeApproximately(128d / 255d, 0.0000001d);
            result[1, 1].Should().BeApproximately(64d / 255d, 0.0000001d);
        }

        [TestMethod]
        public void ToDouble_WithNullJaggedArray_ThrowsNullReferenceException()
        {
            // Arrange
            int[][] image = null;

            // Act
            Action act = () => DataConverter2d.ToDouble(image);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void ToDouble_WithEmptyJaggedArray_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            var image = Array.Empty<int[]>();

            // Act
            Action act = () => DataConverter2d.ToDouble(image);

            // Assert
            act.Should().Throw<IndexOutOfRangeException>();
        }

        [TestMethod]
        public void ToDouble_WithNullMatrix_ThrowsNullReferenceException()
        {
            // Arrange
            Matrix<int> image = null;

            // Act
            Action act = () => DataConverter2d.ToDouble(image);

            // Assert
            act.Should().Throw<NullReferenceException>();
        }
    }
}
