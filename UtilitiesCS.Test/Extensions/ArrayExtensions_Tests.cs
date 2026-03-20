using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class ArrayExtensions_Tests
    {
        [TestMethod]
        public void ToStringArray_2DArray_ConvertsValuesAndNullsToStrings()
        {
            // Arrange
            string[,] values =
            {
                { "a", null },
                { "c", "d" },
            };

            // Act
            var actual = values.ToStringArray();

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    new string[,]
                    {
                        { "a", "" },
                        { "c", "d" },
                    }
                );
        }

        [TestMethod]
        public void ToStringArray_1DArray_WithNullReplacement_UsesReplacementForNullEntries()
        {
            // Arrange
            string[] values = ["alpha", null, "omega"];

            // Act
            var actual = values.ToStringArray("(null)");

            // Assert
            actual.Should().Equal("alpha", "(null)", "omega");
        }

        [TestMethod]
        public void ToStringArray_1DArray_WhenArrayIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            string[] values = null;

            // Act
            Action action = () => values.ToStringArray();

            // Assert
            action.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void SliceRowAndSliceColumn_ReturnExpectedSequences()
        {
            // Arrange
            int[,] values =
            {
                { 1, 2, 3 },
                { 4, 5, 6 },
            };

            // Act
            var row = values.SliceRow(1).ToArray();
            var column = values.SliceColumn(2).ToArray();

            // Assert
            row.Should().Equal(4, 5, 6);
            column.Should().Equal(3, 6);
        }

        [TestMethod]
        public void To2D_RectangularJaggedArray_Returns2DArray()
        {
            // Arrange
            int[][] source =
            [
                [1, 2],
                [3, 4],
            ];

            // Act
            var actual = source.To2D();

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    new int[,]
                    {
                        { 1, 2 },
                        { 3, 4 },
                    }
                );
        }

        [TestMethod]
        public void To2D_WhenSourceIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            int[][] source = null;

            // Act
            Action action = () => source.To2D();

            // Assert
            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("source");
        }

        [TestMethod]
        public void To2D_WhenRowsAreNullOrNonRectangular_ThrowsInvalidOperationException()
        {
            // Arrange
            int[][] withNullRow =
            [
                [1],
                null,
            ];
            int[][] nonRectangular =
            [
                [1, 2],
                [3],
            ];

            // Act
            Action nullRowAction = () => withNullRow.To2D();
            Action nonRectangularAction = () => nonRectangular.To2D();

            // Assert
            nullRowAction
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*contains null rows*");
            nonRectangularAction
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*not rectangular*");
        }

        [TestMethod]
        public void IsInitialized_ReturnsExpectedResultsForFullyAndPartiallyInitializedArrays()
        {
            // Arrange
            string[] full1D = ["a", "b"];
            string[] partial1D = ["a", null];
            string[,] full2D =
            {
                { "a", "b" },
                { "c", "d" },
            };
            string[,] partial2D =
            {
                { "a", null },
                { null, null },
            };

            // Act / Assert
            full1D.IsInitialized().Should().BeTrue();
            partial1D.IsInitialized().Should().BeFalse();
            partial1D.IsInitialized(partially: true).Should().BeTrue();
            ((string[])null).IsInitialized().Should().BeFalse();

            full2D.IsInitialized().Should().BeTrue();
            partial2D.IsInitialized().Should().BeFalse();
            partial2D.IsInitialized(partially: true).Should().BeTrue();
            ((string[,])null).IsInitialized(partially: true).Should().BeFalse();
        }

        [TestMethod]
        public void SearchArry4Str_ReturnsExpectedResultsForSupportedOptions()
        {
            // Arrange
            string[] source = ["hello", "yellow", "world", "HELLO"];

            // Act
            var standard = source.SearchArry4Str("*ell*", ArrayExtensions.SearchOptions.Standard);
            var complement = source.SearchArry4Str(
                "*ell*",
                ArrayExtensions.SearchOptions.Complement
            );
            var deleteFromMatches = source.SearchArry4Str(
                "h*o",
                ArrayExtensions.SearchOptions.DeleteFromMatches
            );
            var exactMatch = source.SearchArry4Str(
                "hello",
                ArrayExtensions.SearchOptions.ExactMatch
            );
            var exactComplement = source.SearchArry4Str(
                "hello",
                ArrayExtensions.SearchOptions.ExactComplement
            );
            var blankSearch = source.SearchArry4Str();

            // Assert
            standard.Should().Equal("hello", "yellow", "HELLO");
            complement.Should().Equal("world");
            deleteFromMatches.Should().Equal("ell", "ELL");
            exactMatch.Should().Equal("hello");
            exactComplement.Should().Equal("yellow", "world", "HELLO");
            blankSearch.Should().BeSameAs(source);
        }

        [TestMethod]
        public void FlattenArrayTree_ReturnsFlattenedValuesForNestedTypedArrays()
        {
            // Arrange
            object node = new object[]
            {
                new[] { "alpha", "beta" },
                new object[] { new[] { "gamma" } },
            };

            // Act
            var actual = node.FlattenArrayTree<string>();

            // Assert
            actual.Should().Equal("alpha", "beta", "gamma");
        }

        [TestMethod]
        public void FlattenArrayTree_WhenStrictAndEncounteringInvalidNode_ThrowsArgumentException()
        {
            // Arrange
            object node = new object[] { new[] { "alpha" }, 42 };

            // Act
            Action action = () => node.FlattenArrayTree<string>();

            // Assert
            action.Should().Throw<ArgumentException>().WithMessage("*FlattenArrayTree*");
        }

        [TestMethod]
        public void TryFlattenArrayTree_WhenNodeCannotBeFlattened_ReturnsNull()
        {
            // Arrange
            object invalidNode = 42;

            // Act
            var actual = invalidNode.TryFlattenArrayTree<string>();

            // Assert
            actual.Should().BeNull();
        }

        [TestMethod]
        public void IsArrayHelpers_DetectArraysAndElementTypes()
        {
            // Arrange
            object stringArray = new[] { "a", "b" };
            object objectArray = new object[] { "a", 1 };

            // Act / Assert
            stringArray.IsArray().Should().BeTrue();
            stringArray.IsArray<string>().Should().BeTrue();
            stringArray.IsArray<int>().Should().BeFalse();
            objectArray.IsArray().Should().BeTrue();
            objectArray.IsArray<string>().Should().BeFalse();
        }

        [TestMethod]
        public void SentenceJoin_FormatsEmptySingleDoubleAndMultipleInputs()
        {
            // Arrange
            IEnumerable<string> enumerable = new List<string> { "red", "green", "blue" };
            string[] single = ["solo"];
            string[] pair = ["left", "right"];
            char[] letters = ['a', 'b', 'c'];

            // Act / Assert
            Array.Empty<string>().SentenceJoin().Should().BeEmpty();
            single.SentenceJoin().Should().Be("solo");
            pair.SentenceJoin().Should().Be("left and right");
            enumerable.SentenceJoin().Should().Be("red, green and blue");
            letters.SentenceJoin().Should().Be("a, b and c");
        }

        [TestMethod]
        public void FlattenStringTree_FlattensNestedStringArraysAndCanReturnErrorForInvalidNodes()
        {
            // Arrange
            object[] valid = { "alpha", new object[] { "beta", "gamma" } };
            object[] invalid = { "alpha", 5 };

            // Act
#pragma warning disable CS0618 // FlattenStringTree is intentionally covered because it is still public on ArrayExtensions.
            var validResult = valid.FlattenStringTree();
            var invalidResult = invalid.FlattenStringTree(strictValidation: false);
#pragma warning restore CS0618

            // Assert
            validResult.Should().Be("alpha, beta, gamma");
            invalidResult.Should().Be("Error");
        }

        [TestMethod]
        public void ToStringArray_1DArray_ConvertsNonNullValuesToStrings()
        {
            // Arrange
            int[] values = [10, 20, 30];

            // Act
            var actual = values.ToStringArray();

            // Assert
            actual.Should().Equal("10", "20", "30");
        }

        [TestMethod]
        public void ToStringArray_2DArray_WithNullReplacement_UsesReplacementForNullEntries()
        {
            // Arrange
            string[,] values =
            {
                { "a", null },
                { null, "d" },
            };

            // Act
            var actual = values.ToStringArray("N/A");

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    new string[,]
                    {
                        { "a", "N/A" },
                        { "N/A", "d" },
                    }
                );
        }

        [TestMethod]
        public void To2D_EmptyJaggedArray_ReturnsEmpty2DArray()
        {
            // Arrange
            int[][] source = [];

            // Act
            var actual = source.To2D();

            // Assert
            actual.GetLength(0).Should().Be(0);
            actual.GetLength(1).Should().Be(0);
        }

        [TestMethod]
        public void IsInitialized_1DArrayPartially_WithAllNulls_ReturnsFalse()
        {
            // Arrange
            string[] allNulls = [null, null, null];

            // Act
            var result = allNulls.IsInitialized(partially: true);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsInitialized_2DArrayPartially_WithAllNulls_ReturnsFalse()
        {
            // Arrange
            string[,] allNulls =
            {
                { null, null },
                { null, null },
            };

            // Act
            var result = allNulls.IsInitialized(partially: true);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void SentenceJoin_CharArrayEdgeCases_FormatsCorrectly()
        {
            // Act / Assert
            Array.Empty<char>().SentenceJoin().Should().BeEmpty();
            new[] { 'x' }.SentenceJoin().Should().Be("x");
            new[] { 'a', 'b' }.SentenceJoin().Should().Be("a and b");
        }

        [TestMethod]
        public void SentenceJoin_IEnumerableOverload_EmptyAndSingleCases()
        {
            // Arrange
            IEnumerable<string> empty = Array.Empty<string>();
            IEnumerable<string> single = new List<string> { "only" };
            IEnumerable<string> pair = new List<string> { "first", "second" };

            // Act / Assert
            empty.SentenceJoin().Should().BeEmpty();
            single.SentenceJoin().Should().Be("only");
            pair.SentenceJoin().Should().Be("first and second");
        }

        [TestMethod]
        public void TryFlattenArrayTree_WithValidNestedArray_ReturnsFlattenedArray()
        {
            // Arrange
            object node = new object[] { new[] { "a", "b" }, new[] { "c" } };

            // Act
            var actual = node.TryFlattenArrayTree<string>();

            // Assert
            actual.Should().Equal("a", "b", "c");
        }

        [TestMethod]
        public void ArrayIsAllocated_ReturnsExpectedValuesForArrayAndStringArrayOverloads()
        {
            // Arrange
            Array allocatedArray = new[] { "value" };
            Array emptyArray = Array.Empty<string>();
            string[] allocatedStringArray = ["value"];
            string[] nullStringArray = null;

            // Act / Assert
            ArrayIsAllocated.IsAllocated(ref allocatedArray).Should().BeTrue();
            ArrayIsAllocated.IsAllocated(ref emptyArray).Should().BeFalse();
            ArrayIsAllocated.IsAllocated(ref allocatedStringArray).Should().BeTrue();
            ArrayIsAllocated.IsAllocated(ref nullStringArray).Should().BeFalse();
        }
    }
}
