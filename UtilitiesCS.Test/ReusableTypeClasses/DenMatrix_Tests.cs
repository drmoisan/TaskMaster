using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Matrices;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class DenMatrix_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyMatrix()
        {
            // Arrange
            var matrix = new DenMatrix<int>();

            // Act
            var isEmpty = matrix.IsEmpty;

            // Assert
            isEmpty.Should().BeTrue();
            matrix.Width.Should().Be(0);
            matrix.Height.Should().Be(0);
            matrix.Length.Should().Be(0);
        }

        [TestMethod]
        public void ConstructorWithDimensions_InitializesBackingStoreAndSupportsIndexerRoundTrip()
        {
            // Arrange
            var matrix = new DenMatrix<int>(2, 3);

            // Act
            matrix[1, 2] = 42;

            // Assert
            matrix.IsEmpty.Should().BeFalse();
            matrix.Width.Should().Be(2);
            matrix.Height.Should().Be(3);
            matrix.Length.Should().Be(6);
            matrix[1, 2].Should().Be(42);
            matrix.Get(0, 0).Should().Be(0);
        }

        [TestMethod]
        public void ConstructorWithArray_UsesObservedDimensionMappingAndPreservesValues()
        {
            // Arrange
            var values = new[,]
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
            };

            // Act
            var matrix = new DenMatrix<int>(values);

            // Assert
            matrix.IsEmpty.Should().BeFalse();
            matrix.Width.Should().Be(3);
            matrix.Height.Should().Be(2);
            matrix.Length.Should().Be(6);
            matrix[0, 0].Should().Be(1);
            matrix[2, 1].Should().Be(6);
        }

        [TestMethod]
        public void To1d_FlattensTwoDimensionalArrayInRowMajorOrder()
        {
            // Arrange
            var values = new[,]
            {
                { 1, 2 },
                { 3, 4 },
            };
            var matrix = new DenMatrix<int>(values);

            // Act
            var flattened = matrix.To1d(values);

            // Assert
            flattened.Should().Equal(1, 2, 3, 4);
        }

        [TestMethod]
        public void Get_WhenMatrixIsEmpty_ThrowsHelpfulException()
        {
            // Arrange
            var matrix = new DenMatrix<int>();

            // Act
            Action act = () => _ = matrix.Get(0, 0);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("array is empty");
        }

        [TestMethod]
        public void Get_WhenCoordinatesAreOutOfBounds_ThrowsMessageWithExceededDimension()
        {
            // Arrange
            var matrix = new DenMatrix<int>(1, 1);

            // Act
            Action xAct = () => _ = matrix.Get(1, 0);
            Action yAct = () => _ = matrix.Get(0, 1);

            // Assert
            xAct.Should().Throw<Exception>()
                .WithMessage("x-value exceeds Width *in Array2d.Get(x,y).");
            yAct.Should().Throw<Exception>()
                .WithMessage("y-value exceeds Height *in Array2d.Get(x,y).");
        }

        [TestMethod]
        public void Set_WhenCoordinatesAreOutOfBounds_ThrowsWithCoordinateSummary()
        {
            // Arrange
            var matrix = new DenMatrix<int>(1, 1);

            // Act
            Action act = () => matrix.Set(1, 0, 5);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("1, 1,0,1");
        }

        [TestMethod]
        public void SetArray_WhenArgumentIsNull_ThrowsHelpfulException()
        {
            // Arrange
            var matrix = new DenMatrix<int>(1, 1);

            // Act
            Action act = () => matrix.Set((int[,])null);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("array is null");
        }

        [TestMethod]
        public void Dispose_ClearsBackingStoreAndResetsDimensions()
        {
            // Arrange
            var matrix = new DenMatrix<int>(new[,] { { 11 } });

            // Act
            matrix.Dispose();

            // Assert
            matrix.IsEmpty.Should().BeTrue();
            matrix.Width.Should().Be(0);
            matrix.Height.Should().Be(0);
            matrix.Length.Should().Be(0);
        }
    }
}
