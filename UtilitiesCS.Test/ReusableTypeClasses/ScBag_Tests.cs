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
    public class ScBag_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyBag()
        {
            // Arrange
            var bag = new ScBag<int>();

            // Act
            var tookItem = bag.TryTake(out var value);

            // Assert
            bag.Count.Should().Be(0);
            tookItem.Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public void CollectionConstructor_PopulatesBag()
        {
            // Arrange
            var bag = new ScBag<string>(new[] { "alpha", "beta", "gamma" });

            // Act
            var values = bag.OrderBy(value => value).ToArray();

            // Assert
            values.Should().Equal("alpha", "beta", "gamma");
            bag.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddTryTakeAndEnumeration_WorkForTypicalBagUsage()
        {
            // Arrange
            var bag = new ScBag<int>();
            bag.Add(1);
            bag.Add(2);
            bag.Add(3);

            // Act
            var took = bag.TryTake(out var removed);
            var remaining = bag.OrderBy(value => value).ToArray();

            // Assert
            took.Should().BeTrue();
            removed.Should().BeOneOf(1, 2, 3);
            remaining.Should().HaveCount(2);
            remaining.Should().OnlyHaveUniqueItems();
            remaining.Should().OnlyContain(value => value >= 1 && value <= 3);
        }

        [TestMethod]
        public async Task ConcurrentAdds_PreserveAllItems()
        {
            // Arrange
            var bag = new ScBag<int>();
            var items = Enumerable.Range(1, 64).ToArray();

            // Act
            await Task.WhenAll(items.Select(item => Task.Run(() => bag.Add(item))));
            var ordered = bag.OrderBy(value => value).ToArray();

            // Assert
            bag.Count.Should().Be(items.Length);
            ordered.Should().Equal(items);
        }

        [TestMethod]
        public void FilePathPropertiesAndDiskActivation_SwapActiveStorageProfile()
        {
            // Arrange
            var bag = new ScBag<int>
            {
                LocalDisk = new FilePathHelper("local.json", @"C:\local"),
                NetDisk = new FilePathHelper("net.json", @"C:\net"),
                LocalJsonSettings = new JsonSerializerSettings { Formatting = Formatting.None },
                NetJsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented },
            };

            // Act
            bag.ActivateLocalDisk();
            var localPath = bag.FilePath;
            var localFormatting = bag.JsonSettings.Formatting;
            bag.ActivateNetDisk();
            var netPath = bag.FilePath;
            var netFormatting = bag.JsonSettings.Formatting;

            // Assert
            localPath.Should().Be(@"C:\local\local.json");
            localFormatting.Should().Be(Formatting.None);
            netPath.Should().Be(@"C:\net\net.json");
            netFormatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void Serialize_WithNoConfiguredPath_IsASafeNoOp()
        {
            // Arrange
            var bag = new ScBag<int>();
            bag.Add(42);

            // Act
            bag.Serialize();

            // Assert
            bag.Count.Should().Be(1);
            bag.Single().Should().Be(42);
        }

        [TestMethod]
        public void GetDefaultSettings_ReturnsIndentedTypeAwareSettings()
        {
            // Arrange

            // Act
            var settings = ScBag<int>.GetDefaultSettings();

            // Assert
            settings.Formatting.Should().Be(Formatting.Indented);
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void JsonSettings_SetAndGet_ReturnsAssignedSettings()
        {
            // Arrange
            var bag = new ScBag<int>();
            var settings = new JsonSerializerSettings { Formatting = Formatting.None };

            // Act
            bag.JsonSettings = settings;

            // Assert
            bag.JsonSettings.Formatting.Should().Be(Formatting.None);
        }

        [TestMethod]
        public void JsonSerialize_ScBagWithItems_ProducesValidJson()
        {
            // Arrange
            var bag = new ScBag<string>(new[] { "alpha", "beta" });
            var settings = ScBag<string>.GetDefaultSettings();

            // Act
            var json = JsonConvert.SerializeObject(bag, settings);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("alpha");
            json.Should().Contain("beta");
        }

        [TestMethod]
        public void JsonDeserialize_ValidJson_RecreatsBag()
        {
            // Arrange
            var original = new ScBag<int>(new[] { 1, 2, 3 });
            var settings = ScBag<int>.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(original, settings);

            // Act
            var restored = JsonConvert.DeserializeObject<ScBag<int>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.OrderBy(x => x).Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void JsonRoundTrip_EmptyBag_PreservesEmpty()
        {
            // Arrange
            var original = new ScBag<double>();
            var settings = ScBag<double>.GetDefaultSettings();

            // Act
            var json = JsonConvert.SerializeObject(original, settings);
            var restored = JsonConvert.DeserializeObject<ScBag<double>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
        }

        [TestMethod]
        public void FileName_SetAndGetProperty_Works()
        {
            // Arrange
            var bag = new ScBag<int>();

            // Act
            bag.FileName = "test.json";

            // Assert
            bag.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void FolderPath_SetAndGetProperty_Works()
        {
            // Arrange
            var bag = new ScBag<int>();

            // Act
            bag.FolderPath = @"C:\data";

            // Assert
            bag.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void NetJsonSettings_SetAndGet_Works()
        {
            // Arrange
            var bag = new ScBag<int>();
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };

            // Act
            bag.NetJsonSettings = settings;

            // Assert
            bag.NetJsonSettings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void LocalJsonSettings_SetAndGet_Works()
        {
            // Arrange
            var bag = new ScBag<int>();
            var settings = new JsonSerializerSettings { Formatting = Formatting.None };

            // Act
            bag.LocalJsonSettings = settings;

            // Assert
            bag.LocalJsonSettings.Formatting.Should().Be(Formatting.None);
        }
    }
}
