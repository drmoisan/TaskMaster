#nullable enable

using System;
using System.Collections.Generic;
using System.Configuration;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    /// <summary>
    /// Unit tests for <see cref="ApplicationSettingsStore"/>. The wrapped settings instance is a
    /// <see cref="RecordingApplicationSettings"/> whose indexer and <c>Save</c> overrides never call
    /// the base members, so no settings provider runs and no file is read or written.
    /// </summary>
    [TestClass]
    public class ApplicationSettingsStoreTests
    {
        [TestMethod]
        public void Constructor_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            Action act = () => _ = new ApplicationSettingsStore(null!);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>("a store without settings has nothing to forward to")
                .Which.ParamName.Should()
                .Be("settings");
        }

        [TestMethod]
        public void Indexer_Get_ReturnsWrappedSettingsValue()
        {
            // Arrange
            var settings = new RecordingApplicationSettings();
            settings["MaxLengthOfID"] = 4;
            var store = new ApplicationSettingsStore(settings);

            // Act
            var value = store["MaxLengthOfID"];

            // Assert
            value.Should().Be(4, "the getter forwards to the wrapped settings indexer");
        }

        [TestMethod]
        public void Indexer_Set_WritesWrappedSettingsValue()
        {
            // Arrange
            var settings = new RecordingApplicationSettings();
            var store = new ApplicationSettingsStore(settings);

            // Act
            store["HighConfidenceThreshold"] = 0.75;

            // Assert
            settings["HighConfidenceThreshold"]
                .Should()
                .Be(0.75, "the setter forwards to the wrapped settings indexer");
            settings.SaveCount.Should().Be(0, "assigning a value does not persist it");
        }

        [TestMethod]
        public void Save_Invoked_ForwardsToWrappedSettingsOnce()
        {
            // Arrange
            var settings = new RecordingApplicationSettings();
            var store = new ApplicationSettingsStore(settings);

            // Act
            store.Save();

            // Assert
            settings
                .SaveCount.Should()
                .Be(1, "Save forwards exactly one call to the wrapped settings");
        }

        /// <summary>
        /// Provider-free settings double: the indexer is dictionary-backed and <c>Save</c> only counts
        /// calls, so neither member reaches <see cref="ApplicationSettingsBase"/> persistence.
        /// </summary>
        private sealed class RecordingApplicationSettings : ApplicationSettingsBase
        {
            private readonly Dictionary<string, object> _values = new();

            public int SaveCount { get; private set; }

            public override object this[string propertyName]
            {
                get => _values[propertyName];
                set => _values[propertyName] = value;
            }

            public override void Save() => SaveCount++;
        }
    }
}
