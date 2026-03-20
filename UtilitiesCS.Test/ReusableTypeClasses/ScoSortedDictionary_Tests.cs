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

        [TestMethod]
        public void Filename_SetAndGet_Works()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();

            // Act
            dict.FileName = "test.json";

            // Assert
            dict.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void Folderpath_SetAndGet_UpdatesFilepath()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.FileName = "test.json";

            // Act
            dict.FolderPath = @"C:\data";

            // Assert
            dict.FilePath.Should().Be(@"C:\data\test.json");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.Add("key", 1);

            // Act
            dict.Serialize();

            // Assert
            dict.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesEntries()
        {
            // Arrange
            var original = new ScoSortedDictionary<string, int>();
            original.Add("b", 2);
            original.Add("a", 1);
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            // Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original, settings);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<ScoSortedDictionary<string, int>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("a").WhoseValue.Should().Be(1);
            restored.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesCustomComparer()
        {
            // Arrange & Act
            var dict = new ScoSortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dict.Add("Key", 1);

            // Assert
            dict.TryGetValue("KEY", out var value).Should().BeTrue();
            value.Should().Be(1);
        }

        [TestMethod]
        public void IndexerSet_ExistingKey_UpdatesValue()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict["key"] = 1;

            // Act
            dict["key"] = 99;

            // Assert
            dict["key"].Should().Be(99);
        }

        [TestMethod]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.Add("test", 42);

            // Act & Assert
            dict.ContainsKey("test").Should().BeTrue();
        }

        [TestMethod]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();

            // Act & Assert
            dict.ContainsKey("missing").Should().BeFalse();
        }

        [TestMethod]
        public void Clear_RemovesAllEntries()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.Add("a", 1);
            dict.Add("b", 2);

            // Act
            dict.Clear();

            // Assert
            dict.Count.Should().Be(0);
        }
    }
}
