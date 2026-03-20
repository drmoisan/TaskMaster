using System;
using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SmartSerializable_Tests
    {
        [TestMethod]
        public void Constructor_Default_InitializesConfigAndName()
        {
            // Act
            var sm = new SmartSerializable<ScDictionary<string, int>>();

            // Assert
            sm.Config.Should().NotBeNull();
            sm.Name.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithParent_InitializesConfig()
        {
            // Arrange
            var parent = new ScDictionary<string, int>();

            // Act
            var sm = new SmartSerializable<ScDictionary<string, int>>(parent);

            // Assert
            sm.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Config_Set_UnsubscribesOldAndSubscribesNew()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();
            var oldConfig = sm.Config;
            var newConfig = new NewSmartSerializableConfig();
            var raised = new List<string>();
            sm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            sm.Config = newConfig;
            newConfig.ClassifierActivated = true; // triggers Config_PropertyChanged

            // Assert
            sm.Config.Should().BeSameAs(newConfig);
            raised.Should().Contain(nameof(NewSmartSerializableConfig.ClassifierActivated));
        }

        [TestMethod]
        public void Config_SetNull_DoesNotThrow()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();

            // Act
            Action act = () => sm.Config = null;

            // Assert
            act.Should().NotThrow();
            sm.Config.Should().BeNull();
        }

        [TestMethod]
        public void Name_SetAndGet_Works()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();

            // Act
            sm.Name = "TestName";

            // Assert
            sm.Name.Should().Be("TestName");
        }

        [TestMethod]
        public void Notify_RaisesPropertyChanged()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();
            string raisedName = null;
            sm.PropertyChanged += (_, e) => raisedName = e.PropertyName;

            // Act
            sm.Notify("TestProp");

            // Assert
            raisedName.Should().Be("TestProp");
        }

        [TestMethod]
        public void GetDefaultSettings_ReturnsExpected()
        {
            // Act
            var settings = SmartSerializable<ScDictionary<string, int>>.GetDefaultSettings();

            // Assert
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void DeserializeObject_ValidJson_ReturnsInstance()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();
            dict.TryAdd("x", 1);
            var settings = NewSmartSerializableConfig.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(dict, settings);
            var sm = new SmartSerializable<ScDictionary<string, int>>();

            // Act
            var result = sm.DeserializeObject(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKey("x").WhoseValue.Should().Be(1);
        }

        [TestMethod]
        public void DeserializeObject_InvalidJson_ReturnsNull()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();
            var settings = NewSmartSerializableConfig.GetDefaultSettings();

            // Act
            var result = sm.DeserializeObject("{ totally invalid }", settings);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void DeserializeObject_ValidJson_SetsConfigJsonSettings()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();
            dict.TryAdd("key", 42);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };
            var json = JsonConvert.SerializeObject(dict, settings);
            var sm = new SmartSerializable<ScDictionary<string, int>>();

            // Act
            var result = sm.DeserializeObject(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();

            // Act - empty Disk.FilePath means no serialization
            sm.Serialize();

            // Assert - should not throw
            sm.Config.Disk.FilePath.Should().BeEmpty();
        }

        [TestMethod]
        public void SerializeToString_ProducesJson()
        {
            // Arrange
            var sm = new SmartSerializable<ScDictionary<string, int>>();
            var parent = new ScDictionary<string, int>();
            parent.TryAdd("a", 1);
            parent.TryAdd("b", 2);

            // Act
            var json = sm.SerializeToString();

            // Assert - SerializeToString on SmartSerializable should not throw
            json.Should().NotBeNull();
        }
    }
}
