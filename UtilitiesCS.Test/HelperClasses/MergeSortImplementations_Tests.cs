using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class MergeSortImplementations_Tests
    {
        [TestMethod]
        public void MergeSort_SortsEmptySingleSortedReverseAndDuplicateSequences()
        {
            // Arrange
            IList<int> empty = new List<int>();
            IList<int> single = new List<int> { 5 };
            IList<int> sorted = new List<int> { 1, 2, 3, 4 };
            IList<int> reverse = new List<int> { 4, 3, 2, 1 };
            IList<int> duplicates = new List<int> { 3, 1, 2, 3, 1 };

            // Act / Assert
            empty.MergeSort((left, right) => left.CompareTo(right)).Should().BeEmpty();
            single.MergeSort((left, right) => left.CompareTo(right)).Should().Equal(5);
            sorted.MergeSort((left, right) => left.CompareTo(right)).Should().Equal(1, 2, 3, 4);
            reverse.MergeSort((left, right) => left.CompareTo(right)).Should().Equal(1, 2, 3, 4);
            duplicates
                .MergeSort((left, right) => left.CompareTo(right))
                .Should()
                .Equal(1, 1, 2, 3, 3);
        }

        [TestMethod]
        public void MergeSort_InPlaceOverload_ReturnsNullAndMutatesOriginalList()
        {
            // Arrange
            IList<int> values = new List<int> { 9, 4, 7, 1, 3 };

            // Act
            var result = values.MergeSort((left, right) => left.CompareTo(right), inplace: true);

            // Assert
            result.Should().BeNull();
            values.Should().Equal(1, 3, 4, 7, 9);
        }

        [TestMethod]
        public void MergeSort_SortsLargeSequenceAndDifferentTypes()
        {
            // Arrange
            IList<int> large = new List<int> { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            IList<string> strings = new List<string> { "delta", "alpha", "charlie", "bravo" };

            // Act
            var sortedLarge = large.MergeSort((left, right) => left.CompareTo(right));
            var sortedStrings = strings.MergeSort(
                (left, right) => string.CompareOrdinal(left, right)
            );

            // Assert
            sortedLarge.Should().Equal(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            sortedStrings.Should().Equal("alpha", "bravo", "charlie", "delta");
        }
    }
}
