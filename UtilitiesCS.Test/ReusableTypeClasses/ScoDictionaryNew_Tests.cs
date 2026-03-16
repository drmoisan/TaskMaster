using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoDictionaryNew_Tests
    {
        [TestMethod]
        public void Add_TryGetValue_RemoveAndClear_WorkAsExpected()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act
            dictionary["alpha"] = 1;
            dictionary["beta"] = 2;
            var found = dictionary.TryGetValue("alpha", out var value);
            var removed = dictionary.TryRemove("alpha", out var removedValue);
            dictionary.Clear();

            // Assert
            found.Should().BeTrue();
            value.Should().Be(1);
            removed.Should().BeTrue();
            removedValue.Should().Be(1);
            dictionary.Should().BeEmpty();
            dictionary.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task ConcurrentAccess_AddsAndReadsAllEntries()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<int, string>();
            var values = Enumerable.Range(1, 25).ToArray();

            // Act
            await Task.WhenAll(values.Select(value => Task.Run(() => dictionary[value] = $"value-{value}")));

            // Assert
            dictionary.Count.Should().Be(values.Length);
            dictionary.Keys.OrderBy(value => value).Should().Equal(values);
            dictionary.Values.Should().Contain(value => value == "value-1");
            dictionary.Values.Should().Contain(value => value == "value-25");
        }

        [TestMethod]
        public void SerializeToString_ContainsStoredEntries()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>
            {
                Name = "numbers",
            };
            dictionary["one"] = 1;
            dictionary["two"] = 2;

            // Act
            var json = dictionary.SerializeToString();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();
            json.Should().Contain("one");
            json.Should().Contain("two");
        }
    }
}
