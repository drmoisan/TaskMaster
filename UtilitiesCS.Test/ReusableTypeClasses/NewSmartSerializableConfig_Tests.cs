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
    public class NewSmartSerializableConfig_Tests
    {
        [TestMethod]
        public void Constructor_InitializesDefaults()
        {
            // Act
            var config = new NewSmartSerializableConfig();

            // Assert
            config.Disk.Should().NotBeNull();
            config.LocalDisk.Should().NotBeNull();
            config.NetDisk.Should().NotBeNull();
            config.ActiveDisk.Should().Be(ISmartSerializableConfig.ActiveDiskEnum.Neither);
            config.ClassifierActivated.Should().BeFalse();
        }

        [TestMethod]
        public void JsonSettings_GetReturnsDefaultSettings()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();

            // Act
            var settings = config.JsonSettings;

            // Assert
            settings.Should().NotBeNull();
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void NetJsonSettings_GetReturnsDefaultSettings()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();

            // Act
            var settings = config.NetJsonSettings;

            // Assert
            settings.Should().NotBeNull();
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void LocalJsonSettings_GetReturnsDefaultSettings()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();

            // Act
            var settings = config.LocalJsonSettings;

            // Assert
            settings.Should().NotBeNull();
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void JsonSettings_Set_UpdatesValueAndNotifies()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var custom = new JsonSerializerSettings { Formatting = Formatting.None };
            var raised = new List<string>();
            config.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            config.JsonSettings = custom;

            // Assert
            config.JsonSettings.Formatting.Should().Be(Formatting.None);
            raised.Should().Contain(nameof(NewSmartSerializableConfig.JsonSettings));
        }

        [TestMethod]
        public void NetJsonSettings_Set_UpdatesValueAndNotifies()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var custom = new JsonSerializerSettings { Formatting = Formatting.None };
            var raised = new List<string>();
            config.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            config.NetJsonSettings = custom;

            // Assert
            config.NetJsonSettings.Formatting.Should().Be(Formatting.None);
            raised.Should().Contain(nameof(NewSmartSerializableConfig.NetJsonSettings));
        }

        [TestMethod]
        public void LocalJsonSettings_Set_UpdatesValueAndNotifies()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var custom = new JsonSerializerSettings { Formatting = Formatting.None };
            var raised = new List<string>();
            config.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            config.LocalJsonSettings = custom;

            // Assert
            config.LocalJsonSettings.Formatting.Should().Be(Formatting.None);
            raised.Should().Contain(nameof(NewSmartSerializableConfig.LocalJsonSettings));
        }

        [TestMethod]
        public void ClassifierActivated_Set_NotifiesPropertyChanged()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var raised = new List<string>();
            config.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            config.ClassifierActivated = true;

            // Assert
            config.ClassifierActivated.Should().BeTrue();
            raised.Should().Contain(nameof(NewSmartSerializableConfig.ClassifierActivated));
        }

        [TestMethod]
        public void Disk_SetAndGet_Works()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var helper = new FilePathHelper();

            // Act
            config.Disk = helper;

            // Assert
            config.Disk.Should().BeSameAs(helper);
        }

        [TestMethod]
        public void LocalDisk_SetAndGet_Works()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var helper = new FilePathHelper();

            // Act
            config.LocalDisk = helper;

            // Assert
            config.LocalDisk.Should().BeSameAs(helper);
        }

        [TestMethod]
        public void NetDisk_SetAndGet_Works()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var helper = new FilePathHelper();

            // Act
            config.NetDisk = helper;

            // Assert
            config.NetDisk.Should().BeSameAs(helper);
        }

        [TestMethod]
        public void ResetLazy_RestoresDefaultSettings()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            config.JsonSettings = new JsonSerializerSettings { Formatting = Formatting.None };
            config.NetJsonSettings = new JsonSerializerSettings { Formatting = Formatting.None };
            config.LocalJsonSettings = new JsonSerializerSettings { Formatting = Formatting.None };

            // Act
            config.ResetLazy();

            // Assert
            config.JsonSettings.Formatting.Should().Be(Formatting.Indented);
            config.NetJsonSettings.Formatting.Should().Be(Formatting.Indented);
            config.LocalJsonSettings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void ResetLazy_WithCustomLazies_UsesProvided()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            var customLocal = new Lazy<JsonSerializerSettings>(
                () => new JsonSerializerSettings { Formatting = Formatting.None }
            );
            var customNet = new Lazy<JsonSerializerSettings>(
                () => new JsonSerializerSettings { Formatting = Formatting.None }
            );
            var customJson = new Lazy<JsonSerializerSettings>(
                () => new JsonSerializerSettings { Formatting = Formatting.None }
            );

            // Act
            config.ResetLazy(customLocal, customNet, customJson);

            // Assert
            config.LocalJsonSettings.Formatting.Should().Be(Formatting.None);
            config.NetJsonSettings.Formatting.Should().Be(Formatting.None);
            config.JsonSettings.Formatting.Should().Be(Formatting.None);
        }

        [TestMethod]
        public void GetDefaultSettings_ReturnsExpectedConfiguration()
        {
            // Act
            var settings = NewSmartSerializableConfig.GetDefaultSettings();

            // Assert
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            settings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void ActivateLocalDisk_SetsActiveDiskToLocal()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            config.LocalDisk.FileName = "local.json";
            config.LocalDisk.FolderPath = @"C:\local";

            // Act
            config.ActivateLocalDisk();

            // Assert
            config.ActiveDisk.Should().Be(ISmartSerializableConfig.ActiveDiskEnum.Local);
        }

        [TestMethod]
        public void ActivateNetDisk_SetsActiveDiskToNet()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            config.NetDisk.FileName = "net.json";
            config.NetDisk.FolderPath = @"\\server\share";

            // Act
            config.ActivateNetDisk();

            // Assert
            config.ActiveDisk.Should().Be(ISmartSerializableConfig.ActiveDiskEnum.Net);
        }

        [TestMethod]
        public void Clone_ReturnsShallowCopy()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            config.ClassifierActivated = true;

            // Act
            var clone = (NewSmartSerializableConfig)config.Clone();

            // Assert
            clone.Should().NotBeSameAs(config);
            clone.ClassifierActivated.Should().BeTrue();
        }

        [TestMethod]
        public void DeepCopy_ReturnsIndependentCopy()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            config.ClassifierActivated = true;
            config.Disk.FileName = "test.json";

            // Act
            var copy = config.DeepCopy();

            // Assert
            copy.Should().NotBeSameAs(config);
            copy.ClassifierActivated.Should().BeTrue();
            copy.Disk.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void CopyFrom_DeepTrue_CopiesAllProperties()
        {
            // Arrange
            var source = new NewSmartSerializableConfig();
            source.ClassifierActivated = true;
            source.Disk.FileName = "source.json";
            var target = new NewSmartSerializableConfig();

            // Act
            target.CopyFrom(source, deep: true);

            // Assert
            target.ClassifierActivated.Should().BeTrue();
            target.Disk.FileName.Should().Be("source.json");
        }

        [TestMethod]
        public void CopyFrom_DeepFalse_CopiesAllProperties()
        {
            // Arrange
            var source = new NewSmartSerializableConfig();
            source.ClassifierActivated = true;
            var target = new NewSmartSerializableConfig();

            // Act
            target.CopyFrom(source, deep: false);

            // Assert
            target.ClassifierActivated.Should().BeTrue();
        }

        [TestMethod]
        public void CopyChanged_ReturnsListOfChangedProperties()
        {
            // Arrange
            var source = new NewSmartSerializableConfig();
            source.ClassifierActivated = true;
            var target = new NewSmartSerializableConfig();

            // Act
            var changed = target.CopyChanged(source, deep: false);

            // Assert
            changed.Should().Contain(nameof(NewSmartSerializableConfig.ClassifierActivated));
        }

        [TestMethod]
        public void CopyChanged_WithNotify_RaisesPropertyChanged()
        {
            // Arrange
            var source = new NewSmartSerializableConfig();
            source.ClassifierActivated = true;
            var target = new NewSmartSerializableConfig();
            var raised = new List<string>();
            target.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            // Act
            target.CopyChanged(source, deep: false, notify: true);

            // Assert
            raised.Should().NotBeEmpty();
        }

        [TestMethod]
        public void CopyChanged_IdenticalConfigs_ReportsOnlyLazySettingsDifferences()
        {
            // Arrange — Lazy-backed JsonSettings use reference equality,
            // so two independently-constructed configs always detect settings as changed.
            var a = new NewSmartSerializableConfig();
            var b = new NewSmartSerializableConfig();

            // Act
            var changed = a.CopyChanged(b, deep: false);

            // Assert — only Lazy-backed settings properties should differ
            changed.Should().OnlyContain(name =>
                name == "JsonSettings" || name == "NetJsonSettings" || name == "LocalJsonSettings");
        }

        [TestMethod]
        public void Notify_RaisesPropertyChangedEvent()
        {
            // Arrange
            var config = new NewSmartSerializableConfig();
            string raisedName = null;
            config.PropertyChanged += (_, e) => raisedName = e.PropertyName;

            // Act
            config.Notify("TestProperty");

            // Assert
            raisedName.Should().Be("TestProperty");
        }
    }
}
