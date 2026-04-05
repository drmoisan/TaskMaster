using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        public void Notify_WithoutSubscribers_DoesNotThrow()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();

            // Act
            Action act = () => dict.Notify("NoSubscribers");

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Serialize_WithNoConfiguredFilePath_IsASafeNoOp()
        {
            // Arrange
            var dict = new ScDictionary<string, int> { ["alpha"] = 1 };

            // Act
            Action act = () => dict.Serialize();

            // Assert
            act.Should().NotThrow();
            dict.Should().ContainKey("alpha").WhoseValue.Should().Be(1);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInjectedWriter_WritesJsonToProvidedStream()
        {
            // Arrange
            var dict = new ScDictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 };
            using var stream = new MemoryStream();
            InjectStreamWriterFactory(
                dict,
                _ => new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true)
            );

            // Act
            dict.SerializeThreadSafe("ignored.json");
            stream.Position = 0;
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true
            );
            var json = reader.ReadToEnd();

            // Assert
            json.Should().Contain("alpha");
            json.Should().Contain("beta");
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

        [TestMethod]
        public async Task ExplicitInterfaceDeserializeAsync_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<ScDictionary<string, int>> dict = new ScDictionary<string, int>();

            // Act
            Func<Task> act = async () =>
                await dict.DeserializeAsync(new SmartSerializable<ScDictionary<string, int>>());

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [TestMethod]
        public async Task ExplicitInterfaceDeserializeAsync_WithAskUserOnError_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<ScDictionary<string, int>> dict = new ScDictionary<string, int>();

            // Act
            Func<Task> act = async () =>
                await dict.DeserializeAsync(
                    new SmartSerializable<ScDictionary<string, int>>(),
                    askUserOnError: false
                );

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [TestMethod]
        public async Task ExplicitInterfaceDeserializeAsync_WithAltLoader_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<ScDictionary<string, int>> dict = new ScDictionary<string, int>();

            // Act
            Func<Task> act = async () =>
                await dict.DeserializeAsync(
                    new SmartSerializable<ScDictionary<string, int>>(),
                    askUserOnError: false,
                    altLoader: null
                );

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [TestMethod]
        public void ConfigPropertyChanged_WhenInvoked_RaisesPropertyChangedOnDictionary()
        {
            // Arrange
            var dict = new ScDictionary<string, int>();
            string changedProperty = null;
            dict.PropertyChanged += (_, args) => changedProperty = args.PropertyName;
            var method = typeof(ScDictionary<string, int>).GetMethod(
                "Config_PropertyChanged",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            method.Invoke(dict, new object[] { this, new PropertyChangedEventArgs("ConfigFlag") });

            // Assert
            changedProperty.Should().Be("ConfigFlag");
        }

        private static void InjectStreamWriterFactory(
            ScDictionary<string, int> dict,
            Func<string, StreamWriter> createStreamWriter
        )
        {
            var smartSerializableField = typeof(ScDictionary<string, int>).GetField(
                "ism",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var smartSerializable = smartSerializableField.GetValue(dict);
            var createStreamWriterField = smartSerializable
                .GetType()
                .GetField("_createStreamWriter", BindingFlags.Instance | BindingFlags.NonPublic);

            createStreamWriterField.SetValue(smartSerializable, createStreamWriter);
        }
    }
}
