using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SmartSerializableNonTyped_Tests
    {
        private SmartSerializableNonTyped _sut;

        [TestInitialize]
        public void Init()
        {
            _sut = new SmartSerializableNonTyped();
        }

        [TestMethod]
        public void IsSmartSerializable_ScoDictionaryInstance_ReturnsFalse()
        {
            // Arrange — ScoDictionary does not implement ISmartSerializable<>
            var instance = new ScoDictionary<string, int>();

            // Act
            var result = _sut.IsSmartSerializable(instance);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_PlainObject_ReturnsFalse()
        {
            // Arrange
            var instance = "hello";

            // Act
            var result = _sut.IsSmartSerializable(instance);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_TypeOverload_ScoDictionary_ReturnsFalse()
        {
            // Arrange — ScoDictionary does not implement ISmartSerializable<>
            var type = typeof(ScoDictionary<string, int>);

            // Act
            var result = _sut.IsSmartSerializable(type);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsSmartSerializable_TypeOverload_String_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = _sut.IsSmartSerializable(type);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void DeserializeObject_ValidJson_ReturnsInstance()
        {
            // Arrange
            var dict = new ScoDictionary<string, int>();
            dict.Add("key", 42);
            var settings = NewSmartSerializableConfig.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(dict, settings);

            // Act
            var result = _sut.DeserializeObject<ScoDictionary<string, int>>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKey("key").WhoseValue.Should().Be(42);
        }

        [TestMethod]
        public void DeserializeObject_InvalidJson_ReturnsDefault()
        {
            // Arrange
            var settings = NewSmartSerializableConfig.GetDefaultSettings();

            // Act
            var result = _sut.DeserializeObject<ScoDictionary<string, int>>(
                "{ invalid json!!!",
                settings
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void DeserializeObject_NonSmartSerializable_ReturnsInstance()
        {
            // Arrange
            var settings = NewSmartSerializableConfig.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(new SimpleData { Value = 42 }, settings);

            // Act
            var result = _sut.DeserializeObject<SimpleData>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().Be(42);
        }

        [TestMethod]
        public void DeserializeObject_SmartSerializable_SetsConfigJsonSettings()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();
            dict.TryAdd("key", 99);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var json = JsonConvert.SerializeObject(dict, settings);

            // Act
            var result = _sut.DeserializeObject<ScDictionary<string, int>>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Config.Should().NotBeNull();
            result.Config.JsonSettings.Should().NotBeNull();
            result.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        private class SimpleData
        {
            public int Value { get; set; }
        }
    }
}
