using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class DictionaryExtensions_Tests
    {
        [TestMethod]
        public void ContentEquals_TreatsNullAndEmptyDictionariesAsEquivalentAndDetectsMissingKeys()
        {
            // Arrange
            Dictionary<string, int> left = null;
            Dictionary<string, int> empty = new();
            var singleEntry = new Dictionary<string, int> { ["one"] = 1 };

            // Act / Assert
            left.ContentEquals(empty).Should().BeTrue();
            empty.ContentEquals(left).Should().BeTrue();
            singleEntry.ContentEquals(empty).Should().BeFalse();
            empty.ContentEquals(singleEntry).Should().BeFalse();
        }

        [TestMethod]
        public void ContentEquals_ReturnsTrueForEquivalentDictionariesRegardlessOfOrder()
        {
            // Arrange
            var first = new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2
            };
            var second = new Dictionary<string, int>
            {
                ["beta"] = 2,
                ["alpha"] = 1
            };

            // Act
            var actual = first.ContentEquals(second);

            // Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void ToSortedDictionary_FromKeyValuePairs_ReturnsSortedDictionaryWithExpectedContents()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, int>> source =
            [
                new KeyValuePair<string, int>("zeta", 6),
                new KeyValuePair<string, int>("alpha", 1)
            ];

            // Act
            var actual = source.ToSortedDictionary();

            // Assert
            actual.Keys.Should().Equal("alpha", "zeta");
            actual["alpha"].Should().Be(1);
            actual["zeta"].Should().Be(6);
        }

        [TestMethod]
        public void ToDictionary_FromKeyValuePairs_ReturnsEmptyDictionaryForNullAndConvertsSingleEntry()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, int>> nullSource = null;
            IEnumerable<KeyValuePair<string, int>> singleEntry =
            [
                new KeyValuePair<string, int>("only", 42)
            ];

            // Act
            var nullResult = nullSource.ToDictionary();
            var singleResult = singleEntry.ToDictionary();

            // Assert
            nullResult.Should().BeEmpty();
            singleResult.Should().ContainSingle().Which.Should().BeEquivalentTo(new KeyValuePair<string, int>("only", 42));
        }

        [TestMethod]
        public void ToDictionary_FromKeyValuePairs_WhenDuplicateKeysExist_ThrowsInvalidOperationException()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, int>> duplicateKeys =
            [
                new KeyValuePair<string, int>("dup", 1),
                new KeyValuePair<string, int>("dup", 2)
            ];

            // Act
            Action action = () => duplicateKeys.ToDictionary();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*duplicate keys*");
        }

        [TestMethod]
        public void ToDictionary_FromDictionaryEntries_CastsKeysAndValues()
        {
            // Arrange
            IEnumerable<DictionaryEntry> entries =
            [
                new DictionaryEntry("alpha", 1),
                new DictionaryEntry("beta", 2)
            ];

            // Act
            var actual = entries.ToDictionary<string, int>();

            // Assert
            actual.Should().Equal(new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2
            });
        }

        [TestMethod]
        public void ToConcurrentDictionary_WhenDuplicateKeysExist_ThrowsInvalidOperationException()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, int>> duplicateKeys =
            [
                new KeyValuePair<string, int>("dup", 1),
                new KeyValuePair<string, int>("dup", 2)
            ];

            // Act
            Action action = () => duplicateKeys.ToConcurrentDictionary();

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*duplicate keys*");
        }

        [TestMethod]
        public void ToConcurrentDictionary_WithDistinctKeys_CreatesConcurrentDictionary()
        {
            // Arrange
            IEnumerable<KeyValuePair<string, int>> source =
            [
                new KeyValuePair<string, int>("alpha", 1),
                new KeyValuePair<string, int>("beta", 2)
            ];

            // Act
            var actual = source.ToConcurrentDictionary();

            // Assert
            actual.Should().BeEquivalentTo(new Dictionary<string, int>
            {
                ["alpha"] = 1,
                ["beta"] = 2
            });
        }

        [TestMethod]
        public void ToSortedDictionary_FromDictionary_CopiesExistingEntries()
        {
            // Arrange
            var existing = new Dictionary<string, int>
            {
                ["gamma"] = 3,
                ["alpha"] = 1
            };

            // Act
            var actual = existing.ToSortedDictionary();

            // Assert
            actual.Keys.Should().Equal("alpha", "gamma");
            actual.Should().BeEquivalentTo(existing);
        }

        [TestMethod]
        public void SearchSortedDictKeys_FiltersKeysContainingSearchString()
        {
            // Arrange
            var source = new SortedDictionary<string, bool>
            {
                ["alpha"] = true,
                ["alphabet"] = false,
                ["beta"] = true
            };

            // Act
            var actual = DictionaryExtensions.SearchSortedDictKeys(source, "alpha");

            // Assert
            actual.Keys.Should().Equal("alpha", "alphabet");
            actual["alpha"].Should().BeTrue();
            actual["alphabet"].Should().BeFalse();
        }

        [TestMethod]
        public void TryAddValuesAndTrySubtractValues_UpdateExistingValueAndReturnFalseForMissingKey()
        {
            // Arrange
            var dictionary = new ConcurrentDictionary<string, int>();
            dictionary["count"] = 10;

            // Act
            var addResult = dictionary.TryAddValues("count", 5);
            var subtractResult = dictionary.TrySubtractValues("count", 3);
            var missingKeyResult = dictionary.TryAddValues("missing", 1);

            // Assert
            addResult.Should().BeTrue();
            subtractResult.Should().BeTrue();
            missingKeyResult.Should().BeFalse();
            dictionary["count"].Should().Be(12);
        }

        [TestMethod]
        public void TryOperateValues_UsesProvidedOperationAndReturnsFalseForMissingKey()
        {
            // Arrange
            var dictionary = new ConcurrentDictionary<string, int>();
            dictionary["value"] = 4;

            // Act
            var updateResult = dictionary.TryOperateValues("value", 3, static (existing, operand) => existing * operand);
            var missingResult = dictionary.TryOperateValues("missing", 3, static (existing, operand) => existing * operand);

            // Assert
            updateResult.Should().BeTrue();
            missingResult.Should().BeFalse();
            dictionary["value"].Should().Be(12);
        }

        [TestMethod]
        public async Task TryAddValuesAsync_UpdatesExistingValue()
        {
            // Arrange
            var dictionary = new ConcurrentDictionary<string, int>();
            dictionary["value"] = 8;

            // Act
            var result = await dictionary.TryAddValuesAsync("value", 2, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            dictionary["value"].Should().Be(10);
        }

        [TestMethod]
        public void UpdateOrRemove_RemovesOrUpdatesBasedOnCondition()
        {
            // Arrange
            var removeDictionary = new ConcurrentDictionary<string, int>();
            removeDictionary["remove"] = 5;
            var updateDictionary = new ConcurrentDictionary<string, int>();
            updateDictionary["update"] = 7;
            var missingDictionary = new ConcurrentDictionary<string, int>();

            // Act
            var removeResult = removeDictionary.UpdateOrRemove(
                "remove",
                static (_, value) => value == 5,
                static (_, value) => value + 1,
                out var removedValue);
            var updateResult = updateDictionary.UpdateOrRemove(
                "update",
                static (_, value) => value < 0,
                static (_, value) => value + 2,
                out var updatedValue);
            var missingResult = missingDictionary.UpdateOrRemove(
                "missing",
                static (_, _) => true,
                static (_, value) => value,
                out var missingValue);

            // Assert
            removeResult.Should().Be(Enums.DictionaryResult.KeysChanged);
            removeDictionary.Should().BeEmpty();
            removedValue.Should().Be(0);

            updateResult.Should().Be(Enums.DictionaryResult.KeyExists | Enums.DictionaryResult.ValueChanged);
            updateDictionary["update"].Should().Be(9);
            updatedValue.Should().Be(7);

            missingResult.Should().Be(0);
            missingValue.Should().Be(0);
        }
    }
}
