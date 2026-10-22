using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Verifies the high-confidence settings exposed by <see cref="AppQuickFilerSettings"/>. Each
    /// test constructs the system under test over its own seeded <see cref="InMemorySettingsStore"/>,
    /// so getters read the fake, setters write the fake and count one save per assignment, and no
    /// test reads or writes the shared <see cref="Settings.Default"/> instance or its settings file.
    /// </summary>
    [TestClass]
    public class AppQuickFilerSettingsTests
    {
        private static InMemorySettingsStore CreateStore(bool modeEnabled, double threshold)
        {
            var store = new InMemorySettingsStore();
            store[nameof(Settings.HighConfidenceModeEnabled)] = modeEnabled;
            store[nameof(Settings.HighConfidenceThreshold)] = threshold;
            return store;
        }

        [TestMethod]
        public void HighConfidenceModeEnabled_Default_IsFalse()
        {
            // Arrange: the store holds the declared default.
            var store = CreateStore(modeEnabled: false, threshold: 0.9);
            var settings = new AppQuickFilerSettings(store);

            // Act
            var result = settings.HighConfidenceModeEnabled;

            // Assert
            result.Should().BeFalse();
            store.SaveCount.Should().Be(0, "reading a setting does not persist anything");
        }

        [TestMethod]
        public void HighConfidenceThreshold_Default_IsZeroPointNine()
        {
            // Arrange: the store holds the declared default.
            var store = CreateStore(modeEnabled: false, threshold: 0.9);
            var settings = new AppQuickFilerSettings(store);

            // Act
            var result = settings.HighConfidenceThreshold;

            // Assert
            result.Should().Be(0.9);
            store.SaveCount.Should().Be(0, "reading a setting does not persist anything");
        }

        [TestMethod]
        public void HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue()
        {
            // Arrange
            var store = CreateStore(modeEnabled: false, threshold: 0.9);
            var settings = new AppQuickFilerSettings(store);

            // Act
            var saves = SettingsSaveProbe.CountSavesOnCurrentThread(
                Settings.Default,
                () => settings.HighConfidenceModeEnabled = true
            );

            // Assert
            settings.HighConfidenceModeEnabled.Should().BeTrue();
            store[nameof(Settings.HighConfidenceModeEnabled)].Should().Be(true);
            store.SaveCount.Should().Be(1, "the setter persists through the injected store once");
            saves
                .Should()
                .Be(0, "the test persisted settings to the shared per-host settings file");
        }

        [TestMethod]
        public void HighConfidenceThreshold_WhenSet_ReadsBackSameValue()
        {
            // Arrange
            var store = CreateStore(modeEnabled: false, threshold: 0.9);
            var settings = new AppQuickFilerSettings(store);

            // Act
            settings.HighConfidenceThreshold = 0.75;

            // Assert
            settings.HighConfidenceThreshold.Should().Be(0.75);
            store[nameof(Settings.HighConfidenceThreshold)].Should().Be(0.75);
            store.SaveCount.Should().Be(1, "the setter persists through the injected store once");
        }

        [TestMethod]
        public void Constructor_Parameterless_UsesApplicationSettingsStore()
        {
            // Arrange
            var field = typeof(AppQuickFilerSettings).GetField(
                "_settingsStore",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act: inspect the default store without reading or writing any setting.
            var store = field!.GetValue(new AppQuickFilerSettings());

            // Assert
            store
                .Should()
                .BeOfType<ApplicationSettingsStore>(
                    "the production constructor persists through TaskMaster's own settings"
                );
        }

        [TestMethod]
        public void Constructor_NullSettingsStore_ThrowsArgumentNullException()
        {
            // Arrange
            Action act = () => _ = new AppQuickFilerSettings(null!);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("settingsStore");
        }
    }
}
