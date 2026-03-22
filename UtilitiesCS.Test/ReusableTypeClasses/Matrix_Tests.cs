using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Matrices;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class Matrix_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyMatrix()
        {
            // Arrange
            var matrix = new Matrix<int>();

            // Act
            var isEmpty = matrix.IsEmpty;

            // Assert
            isEmpty.Should().BeTrue();
            matrix.Width.Should().Be(0);
            matrix.Height.Should().Be(0);
        }

        [TestMethod]
        public void ConstructorWithDimensions_InitializesBackingStoreAndSupportsIndexerRoundTrip()
        {
            // Arrange
            var matrix = new Matrix<int>(2, 3);

            // Act
            matrix[1, 2] = 42;

            // Assert
            matrix.IsEmpty.Should().BeFalse();
            matrix.Width.Should().Be(2);
            matrix.Height.Should().Be(3);
            matrix[1, 2].Should().Be(42);
            matrix.Get(0, 0).Should().Be(0);
        }

        [TestMethod]
        public void ConstructorWithArray_UsesArrayDimensionsAndValues()
        {
            // Arrange
            var values = new[,]
            {
                { 1, 2 },
                { 3, 4 },
            };

            // Act
            var matrix = new Matrix<int>(values);

            // Assert
            matrix.IsEmpty.Should().BeFalse();
            matrix.Width.Should().Be(2);
            matrix.Height.Should().Be(2);
            matrix[0, 0].Should().Be(1);
            matrix[1, 1].Should().Be(4);
        }

        [TestMethod]
        public void SetArray_ReplacesDimensionsAndStoredValues()
        {
            // Arrange
            var matrix = new Matrix<int>(1, 1);
            var replacement = new[,]
            {
                { 7 },
                { 8 },
                { 9 },
            };

            // Act
            matrix.Set(replacement);

            // Assert
            matrix.Width.Should().Be(3);
            matrix.Height.Should().Be(1);
            matrix[0, 0].Should().Be(7);
            matrix[2, 0].Should().Be(9);
        }

        [TestMethod]
        public void Get_WhenMatrixIsEmpty_ThrowsHelpfulException()
        {
            // Arrange
            var matrix = new Matrix<int>();

            // Act
            Action act = () => _ = matrix.Get(0, 0);

            // Assert
            act.Should().Throw<Exception>().WithMessage("array is empty");
        }

        [TestMethod]
        public void Get_WhenCoordinatesAreOutOfBounds_ThrowsMessageWithExceededDimension()
        {
            // Arrange
            var matrix = new Matrix<int>(1, 1);

            // Act
            Action xAct = () => _ = matrix.Get(1, 0);
            Action yAct = () => _ = matrix.Get(0, 1);

            // Assert
            xAct.Should()
                .Throw<Exception>()
                .WithMessage("x-value exceeds Width *in Array2d.Get(x,y).");
            yAct.Should()
                .Throw<Exception>()
                .WithMessage("y-value exceeds Height *in Array2d.Get(x,y).");
        }

        [TestMethod]
        public void Set_WhenCoordinatesAreOutOfBounds_ThrowsWithCoordinateSummary()
        {
            // Arrange
            var matrix = new Matrix<int>(1, 1);

            // Act
            Action act = () => matrix.Set(1, 0, 5);

            // Assert
            act.Should().Throw<Exception>().WithMessage("1, 1,0,1");
        }

        [TestMethod]
        public void SetArray_WhenArgumentIsNull_ThrowsHelpfulException()
        {
            // Arrange
            var matrix = new Matrix<int>(1, 1);

            // Act
            Action act = () => matrix.Set((int[,])null);

            // Assert
            act.Should().Throw<Exception>().WithMessage("array is null");
        }

        [TestMethod]
        public void Dispose_ClearsBackingStoreAndResetsDimensions()
        {
            // Arrange
            var matrix = new Matrix<int>(
                new[,]
                {
                    { 11 },
                }
            );

            // Act
            matrix.Dispose();

            // Assert
            matrix.IsEmpty.Should().BeTrue();
            matrix.Width.Should().Be(0);
            matrix.Height.Should().Be(0);
        }
    }
}
