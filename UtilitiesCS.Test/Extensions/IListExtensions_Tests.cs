using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class IListExtensions_Tests
    {
        [TestMethod]
        public void AddRange_ThrowsForNullArgumentsAndAppendsToListImplementations()
        {
            // Arrange
            IList<int> nullList = null;
            IEnumerable<int> nullItems = null;
            var list = new List<int> { 1 };
            IList<int> collection = new Collection<int> { 1 };

            // Act
            Action nullListAction = () => nullList.AddRange(new[] { 1 });
            Action nullItemsAction = () => list.AddRange(nullItems);
            list.AddRange(new[] { 2, 3 });
            collection.AddRange(new[] { 2, 3 });

            // Assert
            nullListAction.Should().Throw<ArgumentNullException>();
            nullItemsAction.Should().Throw<ArgumentNullException>();
            list.Should().Equal(1, 2, 3);
            collection.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void TryAddRange_ReturnsExpectedResultsForNullValidAndReadOnlyLists()
        {
            // Arrange
            IList<int> nullList = null;
            IEnumerable<int> nullItems = null;
            IList<int> readOnly = Array.AsReadOnly(new[] { 1, 2 });
            IList<int> mutable = new Collection<int> { 1 };

            // Act / Assert
            nullList.TryAddRange(new[] { 1 }).Should().BeFalse();
            mutable.TryAddRange(nullItems).Should().BeFalse();
            readOnly.TryAddRange(new[] { 3 }).Should().BeFalse();
            mutable.TryAddRange(new[] { 2, 3 }).Should().BeTrue();
            mutable.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void ContainsExistsAndFind_ReturnExpectedMatchesForStringsAndPredicates()
        {
            // Arrange
            IList<string> strings = new List<string> { "alpha", "beta", "gamma" };
            IList<int> numbers = new List<int> { 2, 4, 6 };

            // Act / Assert
            IListExtensions
                .Contains(strings, "ALPHA", StringComparison.OrdinalIgnoreCase)
                .Should()
                .BeTrue();
            IListExtensions.Contains(strings, "delta", StringComparison.Ordinal).Should().BeFalse();
            numbers.Exists(value => value > 5).Should().BeTrue();
            numbers.Exists(value => value < 0).Should().BeFalse();
            numbers.Find(value => value > 3).Should().Be(4);
            numbers.Find(value => value > 10).Should().Be(0);
        }

        [TestMethod]
        public void CompareTo_HandlesNullListsSingleItemsAndDifferences()
        {
            // Arrange
            IList<int> left = null;
            IList<int> empty = Array.Empty<int>();
            IList<int> single = new List<int> { 7 };
            IList<int> source = new List<int> { 1, 2, 3 };
            IList<int> other = new List<int> { 2, 3, 4 };

            // Act
            var nullVsEmpty = left.CompareTo(empty);
            var singleVsNull = single.CompareTo(left);
            var difference = source.CompareTo(other);
            Action bothNullAction = () => left.CompareTo(left);

            // Assert
            nullVsEmpty.DifferenceCount.Should().Be(0);
            nullVsEmpty.OnlyThis.Should().BeEmpty();
            nullVsEmpty.OnlyOther.Should().BeEmpty();
            singleVsNull.DifferenceCount.Should().Be(1);
            singleVsNull.OnlyThis.Should().Equal(7);
            singleVsNull.OnlyOther.Should().BeEmpty();
            difference.DifferenceCount.Should().Be(2);
            difference.OnlyThis.Should().Equal(1);
            difference.OnlyOther.Should().Equal(4);
            bothNullAction
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("*both lists were null*");
        }

        [TestMethod]
        public void FindIndices_ReturnsExpectedBoundaryResultsAndValidatesArguments()
        {
            // Arrange
            IList<int> list = new List<int> { 1, 2, 3, 2, 4 };

            // Act
            var allMatches = list.FindIndices(value => value == 2);
            var startMatches = list.FindIndices(0, value => value % 2 == 0);
            var rangeMatches = list.FindIndices(1, 3, value => value % 2 == 0);
            Action invalidStartAction = () => list.FindIndices(6, value => true);
            Action invalidCountAction = () => list.FindIndices(4, 2, value => true);
            Action nullMatchAction = () => list.FindIndices(0, 1, null);

            // Assert
            allMatches.Should().Equal(1, 3);
            startMatches.Should().Equal(1, 3, 4);
            rangeMatches.Should().Equal(1, 3);
            invalidStartAction
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("startIndex");
            invalidCountAction
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("count");
            nullMatchAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("match");
        }

        [TestMethod]
        public void FindIndex_ReturnsExpectedMatchesAndBoundaryValidation()
        {
            // Arrange
            IList<int> numbers = new List<int> { 1, 3, 5, 6 };
            IList<string> strings = new List<string> { "alpha", "beta" };
            Action invalidStartAction = () => numbers.FindIndex(5, value => true);
            Action invalidCountAction = () => numbers.FindIndex(3, 2, value => true);
            Action nullMatchAction = () => numbers.FindIndex(0, 1, null);

            // Act / Assert
            numbers.FindIndex(value => value % 2 == 0).Should().Be(3);
            numbers.FindIndex(2, value => value % 2 == 0).Should().Be(3);
            numbers.FindIndex(1, 2, value => value % 2 == 0).Should().Be(-1);
            IListExtensions
                .FindIndex(strings, "BETA", StringComparison.OrdinalIgnoreCase)
                .Should()
                .Be(1);
            invalidStartAction.Should().Throw<ArgumentOutOfRangeException>();
            invalidCountAction.Should().Throw<ArgumentOutOfRangeException>();
            nullMatchAction.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void FindMaxAndTryFindMax_HandleEmptyListsSelectorsAndTypeVariations()
        {
            // Arrange
            IList<int> numbers = new List<int> { 2, 9, 5 };
            IList<string> strings = new List<string> { "a", "alphabet", "cat" };
            IList<int> empty = Array.Empty<int>();

            // Act
            var maxNumber = numbers.FindMax((left, right) => left >= right ? left : right);
            var maxStringLength = strings.FindMax(
                (left, right) => left.Length >= right.Length ? left : right
            );
            var success = numbers.TryFindMax(
                (left, right) => left >= right ? left : right,
                out var tryMax
            );
            var nullListResult = ((IList<int>)null).TryFindMax(
                (left, right) => left,
                out var nullListMax
            );
            var nullSelectorResult = numbers.TryFindMax<int>(null, out var nullSelectorMax);
            var throwingSelectorResult = numbers.TryFindMax(
                (left, right) => throw new InvalidOperationException("boom"),
                out var throwingMax
            );
            Action emptyAction = () => empty.FindMax((left, right) => left >= right ? left : right);

            // Assert
            maxNumber.Should().Be(9);
            maxStringLength.Should().Be("alphabet");
            success.Should().BeTrue();
            tryMax.Should().Be(9);
            nullListResult.Should().BeFalse();
            nullListMax.Should().Be(0);
            nullSelectorResult.Should().BeFalse();
            nullSelectorMax.Should().Be(0);
            throwingSelectorResult.Should().BeFalse();
            throwingMax.Should().Be(0);
            emptyAction.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void IsNullOrEmptyAndSplit_ReturnExpectedValuesForEmptyAndDuplicateLists()
        {
            // Arrange
            IList<string> nullList = null;
            IList<string> empty = Array.Empty<string>();
            IList<string> values = new List<string> { "Alpha", "beta", "ALPHA", "gamma", "beta" };

            // Act
            var defaultSplit = values.Split(null);
            var caseInsensitiveSplit = values.Split(StringComparer.OrdinalIgnoreCase);
            var nullSplit = ((IList<string>)null).Split(StringComparer.OrdinalIgnoreCase);

            // Assert
            nullList.IsNullOrEmpty().Should().BeTrue();
            empty.IsNullOrEmpty().Should().BeTrue();
            values.IsNullOrEmpty().Should().BeFalse();

            defaultSplit.Unique.Should().Equal("Alpha", "ALPHA", "gamma");
            defaultSplit.Duplicates.Should().Equal("beta", "beta");
            caseInsensitiveSplit.Unique.Should().Equal("gamma");
            caseInsensitiveSplit.Duplicates.Should().Equal("Alpha", "ALPHA", "beta", "beta");
            nullSplit.Unique.Should().BeEmpty();
            nullSplit.Duplicates.Should().BeEmpty();
        }
    }
}
