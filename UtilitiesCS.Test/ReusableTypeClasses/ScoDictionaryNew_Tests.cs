using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoDictionaryNew_Tests
    {
        private static readonly string RepoRoot = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..")
        );

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
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<
                ScoDictionaryNew<string, int>
            >(json, settings);

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

        [TestMethod]
        public void DefaultConstructor_StartsEmpty()
        {
            // Arrange & Act
            var dictionary = new ScoDictionaryNew<string, int>();

            // Assert
            dictionary.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithCollectionAndComparer_UsesComparer()
        {
            // Arrange
            var pairs = new[] { new KeyValuePair<string, int>("Alpha", 1) };

            // Act
            var dictionary = new ScoDictionaryNew<string, int>(
                pairs,
                StringComparer.OrdinalIgnoreCase
            );

            // Assert
            dictionary.TryGetValue("alpha", out var value).Should().BeTrue();
            value.Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithConcurrencyAndCapacity_StartsEmpty()
        {
            // Arrange & Act
            var dictionary = new ScoDictionaryNew<string, int>(4, 16);

            // Assert
            dictionary.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithConcurrencyCollectionComparer_InitializesFromPairs()
        {
            // Arrange
            var pairs = new[] { new KeyValuePair<string, int>("Alpha", 1) };

            // Act
            var dictionary = new ScoDictionaryNew<string, int>(
                4,
                pairs,
                StringComparer.OrdinalIgnoreCase
            );

            // Assert
            dictionary.TryGetValue("alpha", out var value).Should().BeTrue();
            value.Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithConcurrencyCapacityComparer_UsesComparer()
        {
            // Arrange & Act
            var dictionary = new ScoDictionaryNew<string, int>(
                4,
                16,
                StringComparer.OrdinalIgnoreCase
            );
            dictionary["Key"] = 5;

            // Assert
            dictionary.TryGetValue("key", out var value).Should().BeTrue();
            value.Should().Be(5);
        }

        [TestMethod]
        public void CopyConstructor_CopiesEntries()
        {
            // Arrange
            var original = new ScoDictionaryNew<string, int>();
            original["alpha"] = 1;

            // Act
            var copy = new ScoDictionaryNew<string, int>(original);

            // Assert
            copy.Should().ContainKey("alpha").WhoseValue.Should().Be(1);
        }

        [TestMethod]
        public void Config_SetAndGet_RoundTrips()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            var config = new NewSmartSerializableConfig();

            // Act
            dictionary.Config = config;

            // Assert
            dictionary.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void Notify_RaisesPropertyChanged()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            string changedProperty = null;
            dictionary.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            // Act
            dictionary.Notify("TrackedProperty");

            // Assert
            changedProperty.Should().Be("TrackedProperty");
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInjectedWriter_WritesJsonToProvidedStream()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            dictionary["alpha"] = 1;
            dictionary["beta"] = 2;
            using var stream = new MemoryStream();
            InjectStreamWriterFactory(
                dictionary,
                _ => new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true)
            );

            // Act
            dictionary.SerializeThreadSafe("ignored.json");
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
        public void DeserializeObject_ValidJson_RestoresEntries()
        {
            // Arrange
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };
            var original = new ScoDictionaryNew<string, int>();
            original["alpha"] = 1;
            var json = JsonConvert.SerializeObject(original, settings);
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act
            var restored = dictionary.DeserializeObject(json, settings);

            // Assert
            restored.Should().ContainKey("alpha").WhoseValue.Should().Be(1);
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void DeserializeObject_InvalidJson_ReturnsNull()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act
            var restored = dictionary.DeserializeObject(
                "{ invalid json }",
                new JsonSerializerSettings()
            );

            // Assert
            restored.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_WithInvalidPath_ReturnsEmptyInstanceAndPreservesRequestedPath()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();

            // Act
            var restored = dictionary.Deserialize(
                "*invalid-sco-dictionary-new.json",
                RepoRoot,
                false
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(RepoRoot, "*invalid-sco-dictionary-new.json"));
            StopPendingTimer(restored);
        }

        [TestMethod]
        public void Deserialize_WithCustomSettings_CopiesJsonSettingsToReturnedInstance()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            // Act
            var restored = dictionary.Deserialize(
                "*invalid-sco-dictionary-new.json",
                RepoRoot,
                false,
                settings
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            StopPendingTimer(restored);
        }

        [TestMethod]
        public void ExplicitInterfaceDeserialize_WithAltLoader_ReturnsFallbackInstance()
        {
            // Arrange
            IScoDictionaryNew<string, int> dictionary = new ScoDictionaryNew<string, int>();
            var loader = CreateLoader();

            // Act
            var restored = dictionary.Deserialize(
                loader,
                askUserOnError: false,
                altLoader: () =>
                {
                    var fallback = new ScoDictionaryNew<string, int>();
                    fallback["fallback"] = 42;
                    return fallback;
                }
            );

            // Assert
            restored.Should().ContainKey("fallback").WhoseValue.Should().Be(42);
            StopPendingTimer(restored);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithAskUserFalse_ReturnsEmptyInstance()
        {
            // Arrange
            IScoDictionaryNew<string, int> dictionary = new ScoDictionaryNew<string, int>();
            var loader = CreateLoader();

            // Act
            var restored = await dictionary.DeserializeAsync(loader, askUserOnError: false);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
            StopPendingTimer(restored);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithAltLoader_ReturnsFallbackInstance()
        {
            // Arrange
            IScoDictionaryNew<string, int> dictionary = new ScoDictionaryNew<string, int>();
            var loader = CreateLoader();

            // Act
            var restored = await dictionary.DeserializeAsync(
                loader,
                askUserOnError: false,
                altLoader: () =>
                {
                    var fallback = new ScoDictionaryNew<string, int>();
                    fallback["async-fallback"] = 7;
                    return fallback;
                }
            );

            // Assert
            restored.Should().ContainKey("async-fallback").WhoseValue.Should().Be(7);
            StopPendingTimer(restored);
        }

        [TestMethod]
        public void GetSettingsJson_IncludesExpectedConverters()
        {
            // Arrange
            var fileSystem = new Mock<IFileSystemFolderPaths>();
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.FS).Returns(fileSystem.Object);
            // Act
            var settings = ScoDictionaryNew<string, int>.GetSettingsJson<
                ScoDictionaryNew<string, int>
            >(globals.Object);

            // Assert
            settings.Formatting.Should().Be(Formatting.Indented);
            settings
                .Converters.Should()
                .Contain(converter => converter.GetType().Name == "AppGlobalsConverter");
            settings
                .Converters.Should()
                .Contain(converter => converter.GetType().Name == "FilePathHelperConverter");
            settings
                .Converters.Should()
                .Contain(converter => converter.GetType().Name.Contains("ScoDictionaryConverter"));
        }

        [TestMethod]
        public void ConfigPropertyChanged_WhenInvoked_RaisesPropertyChanged()
        {
            // Arrange
            var dictionary = new ScoDictionaryNew<string, int>();
            string changedProperty = null;
            dictionary.PropertyChanged += (_, args) => changedProperty = args.PropertyName;
            var method = typeof(ScoDictionaryNew<string, int>).GetMethod(
                "Config_PropertyChanged",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            method.Invoke(
                dictionary,
                new object[] { this, new PropertyChangedEventArgs("ConfigFlag") }
            );

            // Assert
            changedProperty.Should().Be("ConfigFlag");
        }

        private static SmartSerializable<ScoDictionaryNew<string, int>> CreateLoader()
        {
            var loader = new SmartSerializable<ScoDictionaryNew<string, int>>();
            loader.Config.Disk.FilePath = Path.Combine(RepoRoot, "*invalid-loader.json");
            loader.Config.JsonSettings = SmartSerializable<
                ScoDictionaryNew<string, int>
            >.GetDefaultSettings();
            return loader;
        }

        private static void InjectStreamWriterFactory(
            ScoDictionaryNew<string, int> dictionary,
            Func<string, StreamWriter> createStreamWriter
        )
        {
            var smartSerializableProperty = typeof(ScoDictionaryNew<string, int>).GetProperty(
                "ism",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var smartSerializable = smartSerializableProperty.GetValue(dictionary);
            var createStreamWriterField = smartSerializable
                .GetType()
                .GetField("_createStreamWriter", BindingFlags.Instance | BindingFlags.NonPublic);

            createStreamWriterField.SetValue(smartSerializable, createStreamWriter);
        }

        private static void StopPendingTimer(object target)
        {
            var smartSerializableProperty = target
                .GetType()
                .GetProperty("ism", BindingFlags.Instance | BindingFlags.NonPublic);
            var smartSerializable = smartSerializableProperty?.GetValue(target);
            var timerField = smartSerializable
                ?.GetType()
                .GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic);
            var timer = timerField?.GetValue(smartSerializable);

            timer?.GetType().GetMethod("StopTimer")?.Invoke(timer, null);
            timer?.GetType().GetMethod("Dispose")?.Invoke(timer, null);
        }
    }
}
