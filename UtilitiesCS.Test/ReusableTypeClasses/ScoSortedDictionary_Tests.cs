using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoSortedDictionary_Tests
    {
        [TestMethod]
        public void DefaultConstructor_StartsEmpty()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();

            // Assert
            dictionary.Count.Should().Be(0);
            dictionary.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithDictionary_EnumeratesKeysInSortedOrder()
        {
            // Arrange
            var source = new Dictionary<string, int>
            {
                ["b"] = 2,
                ["a"] = 1,
                ["c"] = 3,
            };

            // Act
            var dictionary = new ScoSortedDictionary<string, int>(source);

            // Assert
            dictionary.Keys.Should().Equal("a", "b", "c");
            dictionary.Values.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void AddRemoveAndTryGetValue_WorkAsExpected()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();

            // Act
            dictionary.Add("b", 2);
            dictionary.Add("a", 1);
            var found = dictionary.TryGetValue("a", out var value);
            var removed = dictionary.Remove("b");

            // Assert
            found.Should().BeTrue();
            value.Should().Be(1);
            removed.Should().BeTrue();
            dictionary.Keys.Should().Equal("a");
        }

        [TestMethod]
        public void Add_DuplicateKey_PreservesBothEntries()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            dictionary.Add("a", 1);

            // Act
            Action act = () => dictionary.Add("a", 2);

            // Assert
            act.Should().NotThrow();
            dictionary.Count.Should().Be(2);
            dictionary.Keys.Should().Contain(key => key == "a");
        }

        [TestMethod]
        public async Task ConcurrentIndexerAssignments_PreserveAllEntries()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<int, string>();
            var values = Enumerable.Range(1, 20).ToArray();

            // Act
            await Task.WhenAll(
                values.Select(value => Task.Run(() => dictionary[value] = $"value-{value}"))
            );

            // Assert
            dictionary.Count.Should().Be(values.Length);
            dictionary.Keys.OrderBy(value => value).Should().Equal(values);
        }
    }
}
