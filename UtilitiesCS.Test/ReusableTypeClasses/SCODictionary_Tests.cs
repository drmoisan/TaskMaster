using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SCODictionary_Tests
    {
        [TestMethod]
        public void AddRemoveTryGetValueAndCount_WorkAsExpected()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();

            // Act
            dictionary.Add("alpha", 1);
            dictionary.Add("beta", 2);
            var found = dictionary.TryGetValue("beta", out var betaValue);
            var removed = dictionary.Remove("alpha");

            // Assert
            found.Should().BeTrue();
            betaValue.Should().Be(2);
            removed.Should().BeTrue();
            dictionary.Count.Should().Be(1);
            dictionary.Should().ContainKey("beta");
        }

        [TestMethod]
        public void IndexerKeysAndValues_ReflectCurrentEntries()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();

            // Act
            dictionary["alpha"] = 1;
            dictionary["beta"] = 2;
            dictionary["beta"] = 20;
            var keys = dictionary.Keys.OrderBy(key => key).ToArray();
            var values = dictionary.Values.OrderBy(value => value).ToArray();

            // Assert
            dictionary["beta"].Should().Be(20);
            keys.Should().Equal("alpha", "beta");
            values.Should().Equal(1, 20);
        }

        [TestMethod]
        public void MissingKey_TryGetValueReturnsFalse()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();

            // Act
            var found = dictionary.TryGetValue("missing", out var value);

            // Assert
            found.Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public void DuplicateKey_AddThrowsArgumentException()
        {
            // Arrange
            var dictionary = new ScoDictionary<string, int>();
            dictionary.Add("duplicate", 1);

            // Act
            Action act = () => dictionary.Add("duplicate", 2);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Filename_SetAndGet_Works()
        {
            // Arrange
            var dict = new ScoDictionary<string, int>();

            // Act
            dict.Filename = "test.json";

            // Assert
            dict.Filename.Should().Be("test.json");
        }

        [TestMethod]
        public void Folderpath_SetAndGet_UpdatesFilepath()
        {
            // Arrange
            var dict = new ScoDictionary<string, int>();
            dict.Filename = "test.json";

            // Act
            dict.Folderpath = @"C:\data";

            // Assert
            dict.Folderpath.Should().Be(@"C:\data");
            dict.Filepath.Should().Be(@"C:\data\test.json");
        }

        [TestMethod]
        public void Filepath_SetWithFullPath_SplitsComponents()
        {
            // Arrange
            var dict = new ScoDictionary<string, int>();

            // Act
            dict.Filepath = @"C:\folder\myfile.json";

            // Assert
            dict.Filepath.Should().Be(@"C:\folder\myfile.json");
            dict.Filename.Should().Be("myfile.json");
            dict.Folderpath.Should().Be(@"C:\folder");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var dict = new ScoDictionary<string, int>();
            dict.Add("key", 1);

            // Act
            dict.Serialize();

            // Assert - should not throw
            dict.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_ScoDictionary_PreservesEntries()
        {
            // Arrange
            var dict = new ScoDictionary<string, int>();
            dict.Add("alpha", 1);
            dict.Add("beta", 2);
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            // Act
            var json = JsonConvert.SerializeObject(dict, settings);
            var restored = JsonConvert.DeserializeObject<ScoDictionary<string, int>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("alpha").WhoseValue.Should().Be(1);
            restored.Should().ContainKey("beta").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithSourceDictionary_CopiesEntries()
        {
            // Arrange
            var source = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

            // Act
            var dict = new ScoDictionary<string, int>(source);

            // Assert
            dict.Should().ContainKey("a").WhoseValue.Should().Be(1);
            dict.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesCustomComparer()
        {
            // Arrange & Act
            var dict = new ScoDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dict.Add("Key", 1);

            // Assert
            dict.TryGetValue("key", out var value).Should().BeTrue();
            value.Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithCapacity_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new ScoDictionary<string, int>(100);

            // Assert
            dict.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task ConcurrentAccess_AddsAndReadsAllEntries()
        {
            // Arrange
            var dictionary = new ScoDictionary<int, int>();
            var keys = Enumerable.Range(1, 64).ToArray();

            // Act
            await Task.WhenAll(keys.Select(key => Task.Run(() => dictionary[key] = key * 10)));
            var readResults = await Task.WhenAll(
                keys.Select(key =>
                    Task.Run(() => dictionary.TryGetValue(key, out var value) ? value : -1)
                )
            );

            // Assert
            dictionary.Count.Should().Be(keys.Length);
            readResults.OrderBy(value => value).Should().Equal(keys.Select(key => key * 10));
        }
    }
}
