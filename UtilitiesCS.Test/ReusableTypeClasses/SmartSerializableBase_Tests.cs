using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SmartSerializableBase_Tests
    {
        // SmartSerializableBase has public constructor
        private SmartSerializableBase CreateSut() => new SmartSerializableBase();

        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            // Act
            var sut = CreateSut();

            // Assert
            sut.Should().NotBeNull();
        }

        [TestMethod]
        public void GetDefaultSettings_ReturnsAutoTypeNameHandling()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var settings = sut.GetDefaultSettings();

            // Assert
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void DeserializeObject_ValidJson_ReturnsInstance()
        {
            // Arrange
            var sut = CreateSut();
            var source = new ScoDictionary<string, int>();
            source.Add("k1", 10);
            var settings = sut.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(source, settings);

            // Act
            var result = sut.DeserializeObject<ScoDictionary<string, int>>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKey("k1").WhoseValue.Should().Be(10);
        }

        [TestMethod]
        public void DeserializeObject_InvalidJson_ReturnsNull()
        {
            // Arrange
            var sut = CreateSut();
            var settings = sut.GetDefaultSettings();

            // Act
            var result = sut.DeserializeObject<ScoDictionary<string, int>>(
                "{ not valid json at all !!!",
                settings
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void DeserializeObject_SmartSerializableType_SetsConfigJsonSettings()
        {
            // Arrange
            var sut = CreateSut();
            var source = new ScDictionary<string, int>();
            source.TryAdd("a", 1);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var json = JsonConvert.SerializeObject(source, settings);

            // Act
            var result = sut.DeserializeObject<ScDictionary<string, int>>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Config.Should().NotBeNull();
            result.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void DeserializeObject_NonSmartSerializableType_ReturnsInstance()
        {
            // Arrange
            var sut = CreateSut();
            var settings = sut.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(new TestData { Name = "test", Value = 99 }, settings);

            // Act
            var result = sut.DeserializeObject<TestData>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("test");
            result.Value.Should().Be(99);
        }

        private class TestData
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}
