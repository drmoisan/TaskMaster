using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            var parent = new TestSmartItem { Name = "parent", Value = 2 };
            var sm = new SmartSerializableHarness(parent);

            // Act
            var json = sm.SerializeToString();

            // Assert - SerializeToString on SmartSerializable should not throw
            json.Should().Contain("parent");
            json.Should().Contain("Value");
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseYes_ReturnsSerializedNewInstance()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            var disk = new FilePathHelper("created.json", @"C:\Smart");

            // Act
            var created = harness.ExposeCreateEmpty(DialogResult.Yes, disk);

            // Assert
            created.Should().NotBeNull();
            created.LastSerializedFilePath.Should().Be(disk.FilePath);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseNo_ThrowsArgumentNullException()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            var disk = new FilePathHelper("created.json", @"C:\Smart");

            // Act
            Action act = () => harness.ExposeCreateEmpty(DialogResult.No, disk);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void CreateEmpty_WithAltLoader_UsesAltLoaderAndCopiesSettings()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            var disk = new FilePathHelper("created.json", @"C:\Smart");
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var created = harness.ExposeCreateEmpty(
                DialogResult.Yes,
                disk,
                settings,
                () => new TestSmartItem { Name = "alt", Value = 5 }
            );

            // Assert
            created.Name.Should().Be("alt");
            created.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
            created.LastSerializedFilePath.Should().Be(disk.FilePath);
        }

        [TestMethod]
        public void CreateEmpty_WithAltLoaderAndResponseNo_ThrowsArgumentNullException()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            var disk = new FilePathHelper("created.json", @"C:\Smart");
            var settings = new JsonSerializerSettings();

            // Act
            Action act = () =>
                harness.ExposeCreateEmpty(
                    DialogResult.No,
                    disk,
                    settings,
                    () => new TestSmartItem { Name = "alt" }
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AskUser_WhenPromptDisabled_ReturnsYes()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());

            // Act
            var response = harness.ExposeAskUser(false, "ignored");

            // Assert
            response.Should().Be(DialogResult.Yes);
        }

        [TestMethod]
        public void AskUser_WhenPromptEnabled_UsesInjectedDialog()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetShowDialog(
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
            var response = harness.ExposeAskUser(true, "problem");

            // Assert
            response.Should().Be(DialogResult.No);
        }

        [TestMethod]
        public void Deserialize_DefaultOverload_ReturnsNewInstanceWithDefaultSettings()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => false);

            // Act
            var restored = harness.Deserialize("missing-default.json", @"C:\Smart");

            // Assert
            restored.Should().NotBeNull();
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(@"C:\Smart", "missing-default.json"));
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void Deserialize_WithMissingDiskAndPromptDisabled_CreatesNewInstance()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => false);

            // Act
            var restored = harness.Deserialize("missing.json", @"C:\Smart", false);

            // Assert
            restored.Should().NotBeNull();
            restored.Config.Disk.FilePath.Should().Be(Path.Combine(@"C:\Smart", "missing.json"));
        }

        [TestMethod]
        public void Deserialize_WithMissingDiskAndCustomSettings_CreatesNewInstanceWithCopiedSettings()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => false);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var restored = harness.Deserialize("missing.json", @"C:\Smart", false, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
            restored.Config.Disk.FilePath.Should().Be(Path.Combine(@"C:\Smart", "missing.json"));
        }

        [TestMethod]
        public void Deserialize_WithLoaderAndMissingDisk_UsesAltLoaderAndCopiesLoaderConfig()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => false);
            var loader = new SmartSerializable<TestSmartItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "missing.json");
            loader.Config.JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var restored = harness.Deserialize(
                loader,
                askUserOnError: false,
                altLoader: () => new TestSmartItem { Name = "fallback", Value = 12 }
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
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => true);
            harness.SetReadAllText(_ =>
                JsonConvert.SerializeObject(new TestSmartItem { Name = "loaded", Value = 17 })
            );
            var loader = new SmartSerializable<TestSmartItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "data.json");

            // Act
            var restored = harness.Deserialize(loader);

            // Assert
            restored.Should().NotBeNull();
            restored.Name.Should().Be("loaded");
            restored.Value.Should().Be(17);
            restored.Config.Disk.FilePath.Should().Be(loader.Config.Disk.FilePath);
        }

        [TestMethod]
        public void TryDeserialize_WithNullLoader_ReturnsNull()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());

            // Act
            var restored = harness.TryDeserialize<TestSmartItem>(null);

            // Assert
            restored.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_WithNullLoader_ThrowsArgumentNullException()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());

            // Act
            Action act = () => harness.Deserialize<TestSmartItem>(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Deserialize_WithNullInterfaceLoader_ThrowsArgumentNullException()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            ISmartSerializable<TestSmartItem> loader = null;

            // Act
            Action act = () => harness.Deserialize(loader);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Deserialize_WithInterfaceLoaderAndInjectedJson_ReturnsInstanceAndCopiesConfig()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => true);
            harness.SetReadAllText(_ =>
                JsonConvert.SerializeObject(new TestSmartItem { Name = "interface", Value = 23 })
            );
            ISmartSerializable<TestSmartItem> loader = new TestSmartItem();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "data.json");

            // Act
            var restored = harness.Deserialize(loader);

            // Assert
            restored.Name.Should().Be("interface");
            restored.Value.Should().Be(23);
            restored.Config.Disk.FilePath.Should().Be(loader.Config.Disk.FilePath);
        }

        [TestMethod]
        public async Task DeserializeAsync_Overloads_ReturnExpectedInstances()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => false);
            var loader = new SmartSerializable<TestSmartItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "missing.json");

            // Act
            var first = await harness.DeserializeAsync(loader);
            var second = await harness.DeserializeAsync(loader, askUserOnError: false);
            var third = await harness.DeserializeAsync(
                loader,
                askUserOnError: false,
                altLoader: () => new TestSmartItem { Name = "async", Value = 31 }
            );

            // Assert
            first.Should().BeNull();
            second.Should().NotBeNull();
            third.Name.Should().Be("async");
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInjectedWriter_WritesJson()
        {
            // Arrange
            var parent = new TestSmartItem { Name = "writer", Value = 8 };
            var harness = new SmartSerializableHarness(parent);
            using var stream = new MemoryStream();
            harness.SetCreateStreamWriter(_ => new StreamWriter(
                stream,
                Encoding.UTF8,
                1024,
                leaveOpen: true
            ));

            // Act
            harness.SerializeThreadSafe("ignored.json");
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
        public void StaticDeserializeObject_ValidJson_ReturnsInstance()
        {
            // Arrange
            var settings = SmartSerializable<TestSmartItem>.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(
                new TestSmartItem { Name = "static", Value = 44 }
            );

            // Act
            var restored = SmartSerializable<TestSmartItem>.Static.DeseriealizeObject(
                json,
                settings
            );

            // Assert
            restored.Name.Should().Be("static");
            restored.Value.Should().Be(44);
        }

        [TestMethod]
        public void DeserializeJson_DefaultSettingsOverload_ReturnsInstance()
        {
            // Arrange
            var harness = new SmartSerializableHarness(new TestSmartItem());
            harness.SetDiskExists(_ => true);
            harness.SetReadAllText(_ =>
                JsonConvert.SerializeObject(new TestSmartItem { Name = "default-json", Value = 46 })
            );

            // Act
            var restored = harness.ExposeDeserializeJson(
                new FilePathHelper("data.json", @"C:\Smart")
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Name.Should().Be("default-json");
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void Serialize_WithExplicitPath_QueuesTimerAndUpdatesDiskPath()
        {
            // Arrange
            var parent = new TestSmartItem();
            var harness = new SmartSerializableHarness(parent);
            var filePath = Path.Combine(@"C:\Smart", "queued.json");

            // Act
            harness.Serialize(filePath);

            // Assert
            harness.Config.Disk.FilePath.Should().Be(filePath);
            StopPrivateTimer(harness, typeof(SmartSerializable<TestSmartItem>));
        }

        [TestMethod]
        public void Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite()
        {
            // Arrange
            var parent = new TestSmartItem { Name = "queued", Value = 10 };
            var harness = new SmartSerializableHarness(parent);
            using var signal = new ManualResetEventSlim(false);
            using var timerStub = new ManualFireTimerWrapper();
            harness.SetTimerFactory(_ => timerStub);
            harness.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "queued-timer.json");
            harness.SetCreateStreamWriter(_ =>
            {
                signal.Set();
                return new StreamWriter(new MemoryStream(), Encoding.UTF8, 1024, leaveOpen: false);
            });

            // Act
            harness.Serialize();

            // The deferred-serialization timer was created via the injected factory and started.
            // Fire it deterministically to trigger the deferred write — no wall-clock wait.
            timerStub.Started.Should().BeTrue();
            timerStub.FireElapsed();

            // Assert
            signal.IsSet.Should().BeTrue();
        }

        [TestMethod]
        public void SerializeToString_WithNullParent_ReturnsEmptyString()
        {
            // Arrange
            var harness = new SmartSerializable<TestSmartItem>();

            // Act
            var json = harness.SerializeToString();

            // Assert
            json.Should().BeEmpty();
        }

        [TestMethod]
        public void SerializeToStream_WithTypeNameHandlingNone_UsesNonAutoBranch()
        {
            // Arrange
            var parent = new TestSmartItem { Name = "plain", Value = 12 };
            var harness = new SmartSerializableHarness(parent);
            harness.Config.JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);

            // Act
            harness.SerializeToStream(writer);
            writer.Flush();
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
            var json = reader.ReadToEnd();

            // Assert
            json.Should().Contain("plain");
            json.Should().NotContain("$type");
        }

        [TestMethod]
        public void StaticDeserialize_DefaultOverload_ReturnsNewInstance()
        {
            // Act
            var restored = SmartSerializable<TestSmartItem>.Static.Deserialize(
                "missing-default.json",
                @"C:\Smart"
            );

            // Assert
            restored.Should().NotBeNull();
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(@"C:\Smart", "missing-default.json"));
        }

        [TestMethod]
        public void StaticGetDefaultSettings_ReturnsAutoTypeNameHandling()
        {
            // Act
            var settings = SmartSerializable<TestSmartItem>.Static.GetDefaultSettings();

            // Assert
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void StaticDeserialize_WithMissingFileAndPromptDisabled_ReturnsNewInstance()
        {
            // Act
            var restored = SmartSerializable<TestSmartItem>.Static.Deserialize(
                "missing.json",
                @"C:\Smart",
                false
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Config.Disk.FilePath.Should().Be(Path.Combine(@"C:\Smart", "missing.json"));
        }

        [TestMethod]
        public void StaticDeserialize_WithMissingFileAndCustomSettings_ReturnsNewInstance()
        {
            // Arrange
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None,
            };

            // Act
            var restored = SmartSerializable<TestSmartItem>.Static.Deserialize(
                "missing.json",
                @"C:\Smart",
                false,
                settings
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Config.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.None);
        }

        [TestMethod]
        public void StaticDeserialize_WithLoaderMissingDisk_ReturnsNull()
        {
            // Arrange
            var loader = new SmartSerializable<TestSmartItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "missing.json");

            // Act
            var restored = SmartSerializable<TestSmartItem>.Static.Deserialize(loader);

            // Assert
            restored.Should().BeNull();
        }

        [TestMethod]
        public async Task StaticDeserializeAsync_Overloads_ReturnExpectedInstances()
        {
            // Arrange
            var loader = new SmartSerializable<TestSmartItem>();
            loader.Config.Disk.FilePath = Path.Combine(@"C:\Smart", "missing.json");

            // Act
            var first = await SmartSerializable<TestSmartItem>.Static.DeserializeAsync(loader);
            var second = await SmartSerializable<TestSmartItem>.Static.DeserializeAsync(
                loader,
                askUserOnError: false
            );
            var third = await SmartSerializable<TestSmartItem>.Static.DeserializeAsync(
                loader,
                askUserOnError: false,
                altLoader: () => new TestSmartItem { Name = "static-async", Value = 55 }
            );

            // Assert
            first.Should().BeNull();
            second.Should().NotBeNull();
            third.Name.Should().Be("static-async");
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

        private sealed class SmartSerializableHarness : SmartSerializable<TestSmartItem>
        {
            public SmartSerializableHarness(TestSmartItem parent)
                : base(parent) { }

            public TestSmartItem ExposeCreateEmpty(DialogResult response, FilePathHelper disk) =>
                CreateEmpty(response, disk);

            public TestSmartItem ExposeCreateEmpty(
                DialogResult response,
                FilePathHelper disk,
                JsonSerializerSettings settings,
                Func<TestSmartItem> altLoader
            ) => CreateEmpty(response, disk, settings, altLoader);

            public DialogResult ExposeAskUser(bool askUserOnError, string messageText) =>
                AskUser(askUserOnError, messageText);

            public TestSmartItem ExposeDeserializeJson(FilePathHelper disk) =>
                DeserializeJson(disk);

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

        private sealed class TestSmartItem : ISmartSerializable<TestSmartItem>
        {
            public TestSmartItem()
            {
                Config = new NewSmartSerializableConfig();
            }

            public NewSmartSerializableConfig Config { get; set; }

            public string Name { get; set; }

            public int Value { get; set; }

            public string LastSerializedFilePath { get; private set; }

            // Required by ISmartSerializable<T> : INotifyPropertyChanged; this stub never
            // raises it (no test needs the notification), so CS0067 fires. Deleting it is not
            // possible (the interface requires it); suppressing narrowly preserves the exact
            // pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

            public TestSmartItem Deserialize(string fileName, string folderPath) => new();

            public TestSmartItem Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError
            ) => new();

            public TestSmartItem Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError,
                JsonSerializerSettings settings
            ) => new();

            public TestSmartItem Deserialize<U>(SmartSerializable<U> loader)
                where U : class, ISmartSerializable<U>, new() => new();

            public TestSmartItem Deserialize<U>(
                SmartSerializable<U> loader,
                bool askUserOnError,
                Func<TestSmartItem> altLoader
            )
                where U : class, ISmartSerializable<U>, new() => altLoader?.Invoke() ?? new();

            public Task<TestSmartItem> DeserializeAsync<U>(SmartSerializable<U> config)
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(new TestSmartItem());

            public Task<TestSmartItem> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError
            )
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(new TestSmartItem());

            public Task<TestSmartItem> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError,
                Func<TestSmartItem> altLoader
            )
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(altLoader?.Invoke() ?? new TestSmartItem());

            public TestSmartItem DeserializeObject(string json, JsonSerializerSettings settings) =>
                JsonConvert.DeserializeObject<TestSmartItem>(json, settings);

            public void Serialize()
            {
                LastSerializedFilePath = Config.Disk.FilePath;
            }

            public void Serialize(string filePath)
            {
                Config.Disk.FilePath = filePath;
                LastSerializedFilePath = filePath;
            }

            public void SerializeThreadSafe(string filePath)
            {
                Serialize(filePath);
            }
        }
    }
}
