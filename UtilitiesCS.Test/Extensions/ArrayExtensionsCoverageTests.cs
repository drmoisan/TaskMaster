using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class ArrayExtensionsCoverageTests
    {
        [TestMethod]
        public void ToStringArray_WhenOneDimensionalValuesContainNull_UsesReplacement()
        {
            string[] values = ["alpha", null, "omega"];

            string[] result = values.ToStringArray("<missing>");

            result.Should().Equal("alpha", "<missing>", "omega");
        }

        [TestMethod]
        public void ToStringArray_WhenTwoDimensionalArrayIsEmpty_ReturnsEmptyArray()
        {
            object[,] values = new object[0, 0];

            string[,] result = values.ToStringArray();

            result.GetLength(0).Should().Be(0);
            result.GetLength(1).Should().Be(0);
        }

        [TestMethod]
        public void SliceRowAndSliceColumn_WhenBoundaryIndexIsUsed_ReturnExpectedValues()
        {
            int[,] values =
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
                { 7, 8, 9 },
            };

            values.SliceRow(0).Should().Equal(1, 2, 3);
            values.SliceColumn(2).Should().Equal(3, 6, 9);
        }

        [TestMethod]
        public void To2D_WhenSourceIsEmpty_ReturnsZeroByZeroArray()
        {
            int[][] source = [];

            int[,] result = source.To2D();

            result.GetLength(0).Should().Be(0);
            result.GetLength(1).Should().Be(0);
        }

        [TestMethod]
        public void To2D_WhenSourceContainsNullRow_ThrowsInvalidOperationException()
        {
            int[][] source =
            [
                [1, 2],
                null,
            ];

            Action act = () => source.To2D();

            act.Should().Throw<InvalidOperationException>().WithMessage("*contains null rows*");
        }

        [TestMethod]
        public void IsInitialized_WhenArraysAreNull_ReturnsFalse()
        {
            string[] oneDimensional = null;
            string[,] twoDimensional = null;

            oneDimensional.IsInitialized().Should().BeFalse();
            oneDimensional.IsInitialized(partially: true).Should().BeFalse();
            twoDimensional.IsInitialized().Should().BeFalse();
            twoDimensional.IsInitialized(partially: true).Should().BeFalse();
        }

        [TestMethod]
        public void SearchArry4Str_WhenSearchStringIsBlank_ReturnsOriginalArray()
        {
            string[] values = ["alpha", "beta"];

            string[] result = values.SearchArry4Str("   ");

            result.Should().BeSameAs(values);
        }

        [TestMethod]
        public void SentenceJoin_WhenCustomSeparatorsAreProvided_UsesConfiguredSeparators()
        {
            string[] values = ["alpha", "beta", "gamma"];

            string result = values.SentenceJoin(" | ", " or ");

            result.Should().Be("alpha | beta or gamma");
        }
    }
}
