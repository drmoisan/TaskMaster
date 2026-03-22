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
    public class ScDictionary_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new ScDictionary<string, int>();

            // Assert
            dict.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithCollection_InitializesFromPairs()
        {
            // Arrange
            var pairs = new[]
            {
                new KeyValuePair<string, int>("a", 1),
                new KeyValuePair<string, int>("b", 2),
            };

            // Act
            var dict = new ScDictionary<string, int>(pairs);

            // Assert
            dict.Should().HaveCount(2);
            dict["a"].Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesComparer()
        {
            // Arrange & Act
            var dict = new ScDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dict["Key"] = 1;

            // Assert
            dict["key"].Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithCollectionAndComparer_InitializesCorrectly()
        {
            // Arrange
            var pairs = new[] { new KeyValuePair<string, int>("A", 1) };

            // Act
            var dict = new ScDictionary<string, int>(pairs, StringComparer.OrdinalIgnoreCase);

            // Assert
            dict["a"].Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithConcurrencyAndCapacity_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new ScDictionary<string, int>(4, 16);

            // Assert
            dict.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithConcurrencyCollectionComparer_InitializesCorrectly()
        {
            // Arrange
            var pairs = new[] { new KeyValuePair<string, int>("A", 1) };

            // Act
            var dict = new ScDictionary<string, int>(4, pairs, StringComparer.OrdinalIgnoreCase);

            // Assert
            dict["a"].Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithConcurrencyCapacityComparer_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new ScDictionary<string, int>(4, 16, StringComparer.OrdinalIgnoreCase);

            // Assert
            dict.Should().BeEmpty();
        }

        [TestMethod]
        public void CopyConstructor_CopiesEntries()
        {
            // Arrange
            var original = new ScDictionary<string, int>();
            original["x"] = 42;

            // Act
            var copy = new ScDictionary<string, int>(original);

            // Assert
            copy["x"].Should().Be(42);
        }

        [TestMethod]
        public void GetDefaultSettings_ReturnsIndentedWithTypeNameHandling()
        {
            // Arrange & Act
            var settings = ScDictionary<string, int>.GetDefaultSettings();

            // Assert
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void Name_GetSet_RoundTrips()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();

            // Act
            dict.Name = "TestName";

            // Assert
            dict.Name.Should().Be("TestName");
        }

        [TestMethod]
        public void Config_GetSet_RoundTrips()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();
            var config = new NewSmartSerializableConfig();

            // Act
            dict.Config = config;

            // Assert
            dict.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void Notify_RaisesPropertyChanged()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();
            string changedProperty = null;
            ((INotifyPropertyChanged)dict).PropertyChanged += (s, e) =>
                changedProperty = e.PropertyName;

            // Act
            dict.Notify("TestProp");

            // Assert
            changedProperty.Should().Be("TestProp");
        }

        [TestMethod]
        public void DeserializeObject_ThrowsNotImplementedException()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();

            // Act
            System.Action act = () => dict.DeserializeObject("{}", null);

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void ExplicitInterfaceDeserialize_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<ScDictionary<string, int>> dict = new ScDictionary<string, int>();

            // Act
            System.Action act = () =>
                dict.Deserialize<ScDictionary<string, int>>(
                    new SmartSerializable<ScDictionary<string, int>>()
                );

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void ExplicitInterfaceDeserialize_WithAltLoader_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<ScDictionary<string, int>> dict = new ScDictionary<string, int>();

            // Act
            System.Action act = () =>
                dict.Deserialize<ScDictionary<string, int>>(
                    new SmartSerializable<ScDictionary<string, int>>(),
                    false,
                    null
                );

            // Assert
            act.Should().Throw<NotImplementedException>();
        }
    }
}
