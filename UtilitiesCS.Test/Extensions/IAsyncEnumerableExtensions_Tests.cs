using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class IAsyncEnumerableExtensions_Tests
    {
        [TestMethod]
        public async Task Zip_ShouldZipTwoSequences_StoppingAtShorterSequence()
        {
            // Arrange
            var first = GetValuesAsync(1, 2, 3);
            var second = GetValuesAsync("a", "b");

            // Act
            var result = await IAsyncEnumerableExtensions.Zip(first, second).ToListAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Should().Be((1, "a"));
            result[1].Should().Be((2, "b"));
        }

        [TestMethod]
        public async Task WithProgressReporting_ShouldReportProgressAndReturnAllValues()
        {
            // Arrange
            var reports = new List<int>();
            var source = GetValuesAsync(10, 20, 30, 40);

            // Act
            var result = await source.WithProgressReporting(4, reports.Add).ToListAsync();

            // Assert
            result.Should().Equal(10, 20, 30, 40);
            reports.Should().Equal(25, 50, 75, 100);
        }

        [TestMethod]
        public void WithProgressReporting_ShouldThrowArgumentNullException_WhenSourceIsNull()
        {
            // Arrange
            IAsyncEnumerable<int> source = null;

            // Act
            Action act = () => source.WithProgressReporting(1, _ => { });

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public async Task ToSortedListAsync_ShouldSortBySelectedKey()
        {
            // Arrange
            var source = GetValuesAsync("bravo", "alpha", "charlie");

            // Act
            SortedList<string, string> result = await source.ToSortedListAsync(value => value);

            // Assert
            result.Keys.Should().Equal("alpha", "bravo", "charlie");
            result.Values.Should().Equal("alpha", "bravo", "charlie");
        }

        [TestMethod]
        public async Task ToConcurrentDictionaryAsync_ShouldCreateDictionaryFromSource()
        {
            // Arrange
            var source = GetValuesAsync("alpha", "bravo");

            // Act
            var result = await source.ToConcurrentDictionaryAsync(
                value => value[0],
                value => value.Length
            );

            // Assert
            result.Should().ContainKey('a').WhoseValue.Should().Be(5);
            result.Should().ContainKey('b').WhoseValue.Should().Be(5);
        }

        [TestMethod]
        public async Task ToSortedListAsync_WithComparer_SortsInDescendingOrder()
        {
            // Arrange
            var source = GetValuesAsync("alpha", "charlie", "bravo");
            var reverseComparer = Comparer<string>.Create(
                (a, b) => string.Compare(b, a, StringComparison.Ordinal)
            );

            // Act
            var result = await source.ToSortedListAsync(v => v, reverseComparer);

            // Assert
            result.Keys.Should().Equal("charlie", "bravo", "alpha");
        }

        [TestMethod]
        public async Task ToSortedListAsync_WithElementSelector_TransformsValues()
        {
            // Arrange
            var source = GetValuesAsync("ab", "cde");

            // Act
            var result = await source.ToSortedListAsync(v => v, v => v.Length, comparer: null);

            // Assert
            result["ab"].Should().Be(2);
            result["cde"].Should().Be(3);
        }

        [TestMethod]
        public void ToSortedListAsync_WhenSourceIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IAsyncEnumerable<string> source = null;

            // Act
            Func<Task> act = async () => await source.ToSortedListAsync(v => v);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void ToSortedListAsync_WhenKeySelectorIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var source = GetValuesAsync("a");

            // Act
            Func<Task> act = async () => await source.ToSortedListAsync<string, string>(null);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task ToConcurrentDictionaryAsync_FromKeyValuePairs_CreatesDictionary()
        {
            // Arrange
            var source = GetKvpAsync(
                new KeyValuePair<string, int>("a", 1),
                new KeyValuePair<string, int>("b", 2)
            );

            // Act
            var result = await source.ToConcurrentDictionaryAsync();

            // Assert
            result.Should().ContainKey("a").WhoseValue.Should().Be(1);
            result.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public async Task ToConcurrentDictionaryAsync_WithComparer_UsesCaseInsensitivity()
        {
            // Arrange
            var source = GetValuesAsync("Alpha", "Beta");

            // Act
            var result = await IAsyncEnumerableExtensions.ToConcurrentDictionaryAsync(
                source,
                v => v,
                v => v.Length,
                StringComparer.OrdinalIgnoreCase,
                default
            );

            // Assert
            result.Should().ContainKey("alpha").WhoseValue.Should().Be(5);
        }

        [TestMethod]
        public async Task Zip_BothEmpty_ReturnsEmptySequence()
        {
            // Arrange
            var first = GetValuesAsync<int>();
            var second = GetValuesAsync<string>();

            // Act
            var result = await IAsyncEnumerableExtensions.Zip(first, second).ToListAsync();

            // Assert
            result.Should().BeEmpty();
        }

        private static async IAsyncEnumerable<KeyValuePair<TKey, TValue>> GetKvpAsync<TKey, TValue>(
            params KeyValuePair<TKey, TValue>[] items
        )
        {
            foreach (var item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        private static async IAsyncEnumerable<T> GetValuesAsync<T>(params T[] values)
        {
            foreach (T value in values)
            {
                await Task.Yield();
                yield return value;
            }
        }
    }
}
