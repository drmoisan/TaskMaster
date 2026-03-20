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
            await Task.WhenAll(
                values.Select(value => Task.Run(() => dictionary[value] = $"value-{value}"))
            );

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
            var dictionary = new ScoDictionaryNew<string, int> { Name = "numbers" };
            dictionary["one"] = 1;
            dictionary["two"] = 2;

            // Act
            var json = dictionary.SerializeToString();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();
            json.Should().Contain("one");
            json.Should().Contain("two");
        }

        [TestMethod]
        public void Config_IsNotNull()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act & Assert
            dictionary.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Name_SetAndGet_Works()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act
            dictionary.Name = "test";

            // Assert
            dictionary.Name.Should().Be("test");
        }

        [TestMethod]
        public void Constructor_WithCollection_InitializesFromPairs()
        {
            // Arrange
            var pairs = new[]
            {
                new System.Collections.Generic.KeyValuePair<string, int>("a", 1),
                new System.Collections.Generic.KeyValuePair<string, int>("b", 2),
            };

            // Act
            var dictionary = new ScoDictionaryNew<string, int>(pairs);

            // Assert
            dictionary.Should().ContainKey("a").WhoseValue.Should().Be(1);
            dictionary.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesCustomComparer()
        {
            // Arrange & Act
            var dictionary = new ScoDictionaryNew<string, int>(
                System.StringComparer.OrdinalIgnoreCase
            );
            dictionary["Key"] = 1;

            // Assert
            dictionary.TryGetValue("key", out var value).Should().BeTrue();
            value.Should().Be(1);
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            dictionary["key"] = 42;

            // Act
            dictionary.Serialize();

            // Assert
            dictionary.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesEntries()
        {
            // Arrange
            var original = new ScoDictionaryNew<string, int>();
            original["a"] = 1;
            original["b"] = 2;
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            // Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original, settings);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<ScoDictionaryNew<string, int>>(
                json,
                settings
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("a").WhoseValue.Should().Be(1);
            restored.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            dictionary["key"] = 1;

            // Act & Assert
            dictionary.ContainsKey("key").Should().BeTrue();
        }

        [TestMethod]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act & Assert
            dictionary.ContainsKey("missing").Should().BeFalse();
        }
    }
}
