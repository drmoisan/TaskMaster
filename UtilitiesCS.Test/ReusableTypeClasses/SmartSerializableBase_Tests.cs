using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.Interfaces;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Test.TestHelpers;

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
            var source = new ScoDictionaryNew<string, int>();
            source.TryAdd("k1", 10);
            var settings = sut.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(source, settings);

            // Act
            var result = sut.DeserializeObject<ScoDictionaryNew<string, int>>(json, settings);

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
            var result = sut.DeserializeObject<ScoDictionaryNew<string, int>>(
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
            var json = JsonConvert.SerializeObject(
                new TestData { Name = "test", Value = 99 },
                settings
            );

            // Act
            var result = sut.DeserializeObject<TestData>(json, settings);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("test");
            result.Value.Should().Be(99);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseYes_ReturnsSerializedInstance()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var disk = new FilePathHelper("created.json", @"C:\SmartBase");

            // Act
            var created = sut.ExposeCreateEmpty<BaseTestItem>(DialogResult.Yes, disk);

            // Assert
            created.Should().NotBeNull();
            created.Config.Disk.FilePath.Should().Be(disk.FilePath);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseNo_ThrowsArgumentNullException()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var disk = new FilePathHelper("created.json", @"C:\SmartBase");

            // Act
            Action act = () => sut.ExposeCreateEmpty<BaseTestItem>(DialogResult.No, disk);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void CreateEmpty_WithAltLoader_UsesAltLoaderAndCopiesSettings()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var disk = new FilePathHelper("created.json", @"C:\SmartBase");
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var created = sut.ExposeCreateEmpty(
                DialogResult.Yes,
                disk,
                settings,
                () => new BaseTestItem { Name = "alt", Value = 3 }
            );

            // Assert
            created.Name.Should().Be("alt");
            created.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
            created.Config.Disk.FilePath.Should().Be(disk.FilePath);
        }

        [TestMethod]
        public void CreateEmpty_WithAltLoaderAndResponseNo_ThrowsArgumentNullException()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var disk = new FilePathHelper("created.json", @"C:\SmartBase");
            var settings = new JsonSerializerSettings();

            // Act
            Action act = () =>
                sut.ExposeCreateEmpty(
                    DialogResult.No,
                    disk,
                    settings,
                    () => new BaseTestItem { Name = "alt" }
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AskUser_WhenPromptDisabled_ReturnsYes()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();

            // Act
            var response = sut.ExposeAskUser(false, "ignored");

            // Assert
            response.Should().Be(DialogResult.Yes);
        }

        [TestMethod]
        public void AskUser_WhenPromptEnabled_UsesInjectedDialog()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetShowDialog(
                (message, caption, buttons, icon) =>
                {
                    message.Should().Contain("problem");
                    caption.Should().Be("Error");
                    buttons.Should().Be(MessageBoxButtons.YesNo);
                    icon.Should().Be(MessageBoxIcon.Error);
                    return DialogResult.No;
                }
            );

            // Act
            var response = sut.ExposeAskUser(true, "problem");

            // Assert
            response.Should().Be(DialogResult.No);
        }

        [TestMethod]
        public void Deserialize_DefaultOverload_ReturnsNewInstanceWithDefaultSettings()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => false);

            // Act
            var restored = sut.Deserialize<BaseTestItem>("missing-default.json", @"C:\SmartBase");

            // Assert
            restored.Should().NotBeNull();
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(@"C:\SmartBase", "missing-default.json"));
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void Deserialize_WithMissingDiskAndPromptDisabled_CreatesNewInstance()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => false);

            // Act
            var restored = sut.Deserialize<BaseTestItem>("missing.json", @"C:\SmartBase", false);

            // Assert
            restored.Should().NotBeNull();
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(@"C:\SmartBase", "missing.json"));
        }

        [TestMethod]
        public void Deserialize_WithMissingDiskAndCustomSettings_CopiesSettings()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => false);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var restored = sut.Deserialize<BaseTestItem>(
                "missing.json",
                @"C:\SmartBase",
                false,
                settings
            );

            // Assert
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(@"C:\SmartBase", "missing.json"));
        }

        [TestMethod]
        public void Deserialize_WithLoaderAndMissingDisk_UsesAltLoaderAndCopiesConfig()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => false);
            var loader = new SmartSerializable<BaseLoaderItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\SmartBase", "missing.json");
            loader.Config.JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var restored = sut.Deserialize<BaseTestItem, BaseLoaderItem>(
                loader,
                askUserOnError: false,
                altLoader: () => new BaseTestItem { Name = "fallback", Value = 11 }
            );

            // Assert
            restored.Name.Should().Be("fallback");
            restored.Config.Disk.FilePath.Should().Be(loader.Config.Disk.FilePath);
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
        }

        [TestMethod]
        public void Deserialize_WithLoaderAndInjectedJson_ReturnsInstanceAndCopiesConfig()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => true);
            sut.SetReadAllText(_ =>
                JsonConvert.SerializeObject(new BaseTestItem { Name = "loaded", Value = 19 })
            );
            var loader = new SmartSerializable<BaseLoaderItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\SmartBase", "data.json");

            // Act
            var restored = sut.Deserialize<BaseTestItem, BaseLoaderItem>(loader);

            // Assert
            restored.Name.Should().Be("loaded");
            restored.Value.Should().Be(19);
            restored.Config.Disk.FilePath.Should().Be(loader.Config.Disk.FilePath);
        }

        [TestMethod]
        public void TryDeserialize_WithNullLoader_ReturnsNull()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();

            // Act
            var restored = sut.TryDeserialize<BaseTestItem, BaseLoaderItem>(null);

            // Assert
            restored.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_WithNullLoader_ThrowsArgumentNullException()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();

            // Act
            Action act = () => sut.Deserialize<BaseTestItem, BaseLoaderItem>(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public async Task DeserializeAsync_Overloads_ReturnExpectedInstances()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => false);
            var loader = new SmartSerializable<BaseLoaderItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\SmartBase", "missing.json");

            // Act
            var first = await sut.DeserializeAsync<BaseTestItem, BaseLoaderItem>(loader);
            var second = await sut.DeserializeAsync<BaseTestItem, BaseLoaderItem>(
                loader,
                askUserOnError: false
            );
            var third = await sut.DeserializeAsync<BaseTestItem, BaseLoaderItem>(
                loader,
                askUserOnError: false,
                altLoader: () => new BaseTestItem { Name = "async", Value = 7 }
            );

            // Assert
            first.Should().BeNull();
            second.Should().NotBeNull();
            third.Name.Should().Be("async");
        }

        [TestMethod]
        public void GetConfigAndSetConfig_RoundTripSmartSerializableConfig()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem();
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };

            // Act
            sut.ExposeSetConfig(instance, settings);
            var config = sut.ExposeGetConfig(instance);

            // Assert
            config.Should().NotBeNull();
            config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInjectedWriter_WritesJson()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem { Name = "writer", Value = 21 };
            using var stream = new MemoryStream();
            sut.SetCreateStreamWriter(_ => new StreamWriter(
                stream,
                Encoding.UTF8,
                1024,
                leaveOpen: true
            ));

            // Act
            sut.SerializeThreadSafe(instance, "ignored.json");
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
            json.Should().Contain("writer");
            json.Should().Contain("Value");
        }

        [TestMethod]
        public void DeserializeJson_DefaultSettingsOverload_ReturnsInstance()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            sut.SetDiskExists(_ => true);
            sut.SetReadAllText(_ =>
                JsonConvert.SerializeObject(new BaseTestItem { Name = "default-json", Value = 25 })
            );

            // Act
            var restored = sut.ExposeDeserializeJson<BaseTestItem>(
                new FilePathHelper("data.json", @"C:\SmartBase")
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Name.Should().Be("default-json");
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void SerializeToString_WithConfigurableInstance_ReturnsJson()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem { Name = "text", Value = 5 };

            // Act
            var json = sut.SerializeToString(instance);

            // Assert
            json.Should().Contain("text");
            json.Should().Contain("Value");
        }

        [TestMethod]
        public void SerializeToString_WithInstanceWithoutConfig_ReturnsEmptyString()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();

            // Act
            var json = sut.SerializeToString(new TestData { Name = "none", Value = 1 });

            // Assert
            json.Should().BeEmpty();
        }

        [TestMethod]
        public void SerializeToStream_WithTypeNameHandlingNone_UsesNonAutoBranch()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem { Name = "plain", Value = 4 };
            instance.Config.JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);

            // Act
            sut.SerializeToStream(instance, writer);
            writer.Flush();
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
            var json = reader.ReadToEnd();

            // Assert
            json.Should().Contain("plain");
            json.Should().NotContain("$type");
        }

        [TestMethod]
        public void Serialize_WithConfiguredPath_QueuesTimer()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem();
            instance.Config.Disk.FilePath = Path.Combine(@"C:\SmartBase", "queued.json");

            // Act
            sut.Serialize(instance);

            // Assert
            StopPrivateTimer(sut, typeof(SmartSerializableBase));
            instance.Config.Disk.FilePath.Should().Be(Path.Combine(@"C:\SmartBase", "queued.json"));
        }

        [TestMethod]
        public void Serialize_WithExplicitPath_QueuesTimerAndUpdatesDiskPath()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem();
            var filePath = Path.Combine(@"C:\SmartBase", "queued-explicit.json");

            // Act
            sut.Serialize(instance, filePath);

            // Assert
            instance.Config.Disk.FilePath.Should().Be(filePath);
            StopPrivateTimer(sut, typeof(SmartSerializableBase));
        }

        [TestMethod]
        public void Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite()
        {
            // Arrange
            var sut = new SmartSerializableBaseHarness();
            var instance = new BaseTestItem();
            using var signal = new ManualResetEventSlim(false);
            using var timerStub = new ManualFireTimerWrapper();
            sut.SetTimerFactory(_ => timerStub);
            instance.Config.Disk.FilePath = Path.Combine(@"C:\SmartBase", "queued-trigger.json");
            sut.SetCreateStreamWriter(_ =>
            {
                signal.Set();
                return new StreamWriter(new MemoryStream(), Encoding.UTF8, 1024, leaveOpen: false);
            });

            // Act
            sut.Serialize(instance);

            // The deferred-serialization timer was created via the injected factory and started.
            // Fire it deterministically to trigger the deferred write — no wall-clock wait.
            timerStub.Started.Should().BeTrue();
            timerStub.FireElapsed();

            // Assert
            signal.IsSet.Should().BeTrue();
        }

        private static void StopPrivateTimer(object target, Type declaringType)
        {
            var timerField = declaringType.GetField(
                "_timer",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var timer = timerField?.GetValue(target);

            timer?.GetType().GetMethod("StopTimer")?.Invoke(timer, null);
            timer?.GetType().GetMethod("Dispose")?.Invoke(timer, null);
        }

        private sealed class SmartSerializableBaseHarness : SmartSerializableBase
        {
            public T ExposeCreateEmpty<T>(DialogResult response, FilePathHelper disk)
                where T : class, new() => CreateEmpty<T>(response, disk);

            public T ExposeCreateEmpty<T>(
                DialogResult response,
                FilePathHelper disk,
                JsonSerializerSettings settings,
                Func<T> altLoader
            )
                where T : class, new() => CreateEmpty(response, disk, settings, altLoader);

            public DialogResult ExposeAskUser(bool askUserOnError, string messageText) =>
                AskUser(askUserOnError, messageText);

            public NewSmartSerializableConfig ExposeGetConfig<T>(T instance) => GetConfig(instance);

            public void ExposeSetConfig<T>(T instance, JsonSerializerSettings settings) =>
                SetConfig(instance, settings);

            public T ExposeDeserializeJson<T>(FilePathHelper disk)
                where T : class, new() => DeserializeJson<T>(disk);

            public void SetReadAllText(Func<string, string> readAllText) =>
                ReadAllText = readAllText;

            public void SetDiskExists(Func<FilePathHelper, bool> diskExists) =>
                DiskExists = diskExists;

            public void SetShowDialog(
                Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> showDialog
            ) => ShowDialog = showDialog;

            public void SetCreateStreamWriter(Func<string, StreamWriter> createStreamWriter) =>
                CreateStreamWriter = createStreamWriter;

            public void SetTimerFactory(Func<TimeSpan, ITimerWrapper> timerFactory) =>
                TimerFactory = timerFactory;
        }

        private class BaseTestItem
        {
            public BaseTestItem()
            {
                Config = new NewSmartSerializableConfig();
            }

            public NewSmartSerializableConfig Config { get; set; }

            public string Name { get; set; }

            public int Value { get; set; }
        }

        private sealed class BaseLoaderItem : BaseTestItem, ISmartSerializable<BaseLoaderItem>
        {
            // Required by ISmartSerializable<T> : INotifyPropertyChanged; this stub never
            // raises it (no test needs the notification), so CS0067 fires. Deleting it is not
            // possible (the interface requires it); suppressing narrowly preserves the exact
            // pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0067
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

            public BaseLoaderItem Deserialize(string fileName, string folderPath) => new();

            public BaseLoaderItem Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError
            ) => new();

            public BaseLoaderItem Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError,
                JsonSerializerSettings settings
            ) => new();

            public BaseLoaderItem Deserialize<U>(SmartSerializable<U> loader)
                where U : class, ISmartSerializable<U>, new() => new();

            public BaseLoaderItem Deserialize<U>(
                SmartSerializable<U> loader,
                bool askUserOnError,
                Func<BaseLoaderItem> altLoader
            )
                where U : class, ISmartSerializable<U>, new() => altLoader?.Invoke() ?? new();

            public Task<BaseLoaderItem> DeserializeAsync<U>(SmartSerializable<U> config)
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(new BaseLoaderItem());

            public Task<BaseLoaderItem> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError
            )
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(new BaseLoaderItem());

            public Task<BaseLoaderItem> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError,
                Func<BaseLoaderItem> altLoader
            )
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(altLoader?.Invoke() ?? new BaseLoaderItem());

            public BaseLoaderItem DeserializeObject(string json, JsonSerializerSettings settings) =>
                JsonConvert.DeserializeObject<BaseLoaderItem>(json, settings);

            public void Serialize() { }

            public void Serialize(string filePath)
            {
                Config.Disk.FilePath = filePath;
            }

            public void SerializeThreadSafe(string filePath)
            {
                Config.Disk.FilePath = filePath;
            }
        }

        private class TestData
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}
