using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SloLinkedList_Tests
    {
        private static readonly string RepoRoot = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..")
        );

        [TestMethod]
        public void Constructor_WithEmptyList_ExposesNoEndpoints()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Assert
            list.Count.Should().Be(0);
            list.First.Should().BeNull();
            list.Last.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithSingleSeed_ExposesHeadAndTail()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 42 });

            // Assert
            list.Count.Should().Be(1);
            list.First.Value.Should().Be(42);
            list.Last.Value.Should().Be(42);
        }

        [TestMethod]
        public void AddFindRemoveAndEnumerate_WorkAsExpected()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act
            list.AddLast(2);
            list.AddFirst(1);
            list.AddLast(3);
            var found = list.Find(2);
            list.Remove(2);

            // Assert
            found.Should().NotBeNull();
            found.Value.Should().Be(2);
            list.Should().Equal(1, 3);
            list.First.Value.Should().Be(1);
            list.Last.Value.Should().Be(3);
        }

        [TestMethod]
        public async Task ConcurrentOperations_AddAndRemove_LeaveExpectedState()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var values = Enumerable.Range(1, 30).ToArray();

            // Act
            await Task.WhenAll(values.Select(value => Task.Run(() => list.AddLast(value))));
            await Task.WhenAll(
                values
                    .Where(value => value % 2 == 0)
                    .Select(value => Task.Run(() => list.Remove(value)))
            );

            // Assert
            list.Count.Should().Be(15);
            list.OrderBy(value => value).Should().Equal(values.Where(value => value % 2 != 0));
        }

        [TestMethod]
        public void Config_IsNotNull()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act & Assert
            list.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Config_Set_UpdatesConfig()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var config = new UtilitiesCS.ReusableTypeClasses.NewSmartSerializableConfig();

            // Act
            list.Config = config;

            // Assert
            list.Config.Should().BeSameAs(config);
        }

        [TestMethod]
        public void Name_SetAndGet_Works()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act
            list.Name = "TestList";

            // Assert
            list.Name.Should().Be("TestList");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            list.AddLast(42);

            // Act
            list.Serialize();

            // Assert
            list.Count.Should().Be(1);
        }

        [TestMethod]
        public void DeserializeObject_ThrowsNotImplementedException()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var settings = new Newtonsoft.Json.JsonSerializerSettings();

            // Act
            System.Action act = () => list.DeserializeObject("{}", settings);

            // Assert
            act.Should().Throw<System.NotImplementedException>();
        }

        [TestMethod]
        public void PropertyChanged_RaisedOnConfigChange()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var raised = new System.Collections.Generic.List<string>();
            list.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            list.Notify("TestProp");

            // Assert
            raised.Should().Contain("TestProp");
        }

        [TestMethod]
        public void Clear_RemovesAllNodes()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 1, 2, 3 });

            // Act
            list.Clear();

            // Assert
            list.Count.Should().Be(0);
            list.First.Should().BeNull();
            list.Last.Should().BeNull();
        }

        [TestMethod]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 1, 2, 3 });

            // Act & Assert
            list.Contains(2).Should().BeTrue();
        }

        [TestMethod]
        public void Contains_MissingItem_ReturnsFalse()
        {
            // Arrange
            var list = new SloLinkedList<int>(new[] { 1, 2, 3 });

            // Act & Assert
            list.Contains(99).Should().BeFalse();
        }

        [TestMethod]
        public void Serialize_WithExplicitPath_UpdatesConfigDiskPath()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var invalidPath = CreateInvalidFilePath();

            // Act
            list.Serialize(invalidPath);

            // Assert
            list.Config.Disk.FilePath.Should().Be(invalidPath);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInvalidPath_IsSwallowedByProductionErrorHandling()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act
            System.Action act = () => list.SerializeThreadSafe(CreateInvalidFilePath());

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Deserialize_WithInvalidPath_ReturnsEmptyInstanceAndPreservesRequestedPath()
        {
            // Arrange
            var list = new SloLinkedList<int>();

            // Act
            var restored = list.Deserialize("*invalid-slo-linked-list.json", WorkspaceRoot, false);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
            restored
                .Config.Disk.FilePath.Should()
                .Be(Path.Combine(WorkspaceRoot, "*invalid-slo-linked-list.json"));
        }

        [TestMethod]
        public void Deserialize_WithCustomSettings_CopiesSettingsToReturnedInstance()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            // Act
            var restored = list.Deserialize(
                "*invalid-slo-linked-list.json",
                WorkspaceRoot,
                false,
                settings
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Config.JsonSettings.Should().BeSameAs(settings);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithAskUserFalse_ReturnsEmptyInstance()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            var loader = CreateLoader();

            // Act
            var restored = await list.DeserializeAsync(loader, askUserOnError: false);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
        }

        [TestMethod]
        public void ExplicitInterfaceDeserialize_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<SloLinkedList<int>> list = new SloLinkedList<int>();
            var loader = CreateLoader();

            // Act
            System.Action act = () => list.Deserialize(loader);

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void ExplicitInterfaceDeserializeWithAltLoader_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<SloLinkedList<int>> list = new SloLinkedList<int>();
            var loader = CreateLoader();

            // Act
            System.Action act = () =>
                list.Deserialize(
                    loader,
                    askUserOnError: false,
                    altLoader: () => new SloLinkedList<int>()
                );

            // Assert
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public async Task ExplicitInterfaceDeserializeAsyncWithAltLoader_ThrowsNotImplementedException()
        {
            // Arrange
            ISmartSerializable<SloLinkedList<int>> list = new SloLinkedList<int>();
            var loader = CreateLoader();

            // Act
            Func<Task> act = async () =>
                await list.DeserializeAsync(
                    loader,
                    askUserOnError: false,
                    altLoader: () => new SloLinkedList<int>()
                );

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [TestMethod]
        public void ConfigPropertyChanged_WhenInvoked_RaisesPropertyChanged()
        {
            // Arrange
            var list = new SloLinkedList<int>();
            string changedProperty = null;
            list.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

            // Act
            typeof(SloLinkedList<int>)
                .GetMethod(
                    "Config_PropertyChanged",
                    System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic
                )
                .Invoke(
                    list,
                    new object[]
                    {
                        this,
                        new System.ComponentModel.PropertyChangedEventArgs("ConfigFlag"),
                    }
                );

            // Assert
            changedProperty.Should().Be("ConfigFlag");
        }

        [TestMethod]
        public void StaticDeserialize_WithInvalidPath_ReturnsEmptyInstance()
        {
            // Act
            var restored = SloLinkedList<int>.Static.Deserialize(
                "*invalid-static-slo-linked-list.json",
                WorkspaceRoot,
                false
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
        }

        [TestMethod]
        public async Task StaticDeserializeAsync_WithAskUserFalse_ReturnsEmptyInstance()
        {
            // Arrange
            var loader = CreateLoader();

            // Act
            var restored = await SloLinkedList<int>.Static.DeserializeAsync(
                loader,
                askUserOnError: false
            );

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
        }

        private static SmartSerializable<SloLinkedList<int>> CreateLoader()
        {
            var loader = new SmartSerializable<SloLinkedList<int>>();
            loader.Config.Disk.FilePath = Path.Combine(WorkspaceRoot, "*invalid-slo-loader.json");
            loader.Config.JsonSettings = SmartSerializable<SloLinkedList<int>>.GetDefaultSettings();
            return loader;
        }

        private static string WorkspaceRoot => Path.Combine(RepoRoot, "TaskMaster");

        private static string CreateInvalidFilePath()
        {
            return Path.Combine(WorkspaceRoot, "*invalid-slo-linked-list.json");
        }
    }
}
