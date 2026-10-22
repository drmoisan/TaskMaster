using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Covers the remaining boolean properties of <see cref="AppQuickFilerSettings"/> not already
    /// exercised by <c>AppQuickFilerSettingsTests</c> (which covers HighConfidenceModeEnabled and
    /// HighConfidenceThreshold): MoveEntireConversation, SaveAttachments, SavePictures, and
    /// SaveEmailCopy. Each test injects its own seeded <see cref="InMemorySettingsStore"/>; getters
    /// read the fake and each internal setter (reachable via InternalsVisibleTo("TaskMaster.Test"))
    /// writes the fake and saves through it once, so no shared settings instance or file is touched.
    /// </summary>
    [TestClass]
    public class AppQuickFilerSettingsRemainingPropertiesTests
    {
        private static InMemorySettingsStore CreateStore(bool initialValue)
        {
            var store = new InMemorySettingsStore();
            store[nameof(Settings.MoveEntireConversations)] = initialValue;
            store[nameof(Settings.SaveAttachments)] = initialValue;
            store[nameof(Settings.SavePictures)] = initialValue;
            store[nameof(Settings.SaveEmailCopy)] = initialValue;
            return store;
        }

        [TestMethod]
        public void MoveEntireConversation_Getter_ReflectsSettingsDefault()
        {
            // Arrange
            var store = CreateStore(initialValue: true);
            var settings = new AppQuickFilerSettings(store);

            // Act / Assert
            settings.MoveEntireConversation.Should().BeTrue("the getter reads the settings store");

            // Arrange a different stored value
            store[nameof(Settings.MoveEntireConversations)] = false;

            // Act / Assert
            settings.MoveEntireConversation.Should().BeFalse();
            store.SaveCount.Should().Be(0, "reading a setting does not persist anything");
        }

        [TestMethod]
        public void MoveEntireConversation_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            var store = CreateStore(initialValue: false);
            var settings = new AppQuickFilerSettings(store);

            // Act
            settings.MoveEntireConversation = true;

            // Assert
            settings
                .MoveEntireConversation.Should()
                .BeTrue("the setter persists to the settings store");
            store[nameof(Settings.MoveEntireConversations)].Should().Be(true);
            store.SaveCount.Should().Be(1);
        }

        [TestMethod]
        public void SaveAttachments_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            var store = CreateStore(initialValue: false);
            var settings = new AppQuickFilerSettings(store);

            // Act
            settings.SaveAttachments = true;

            // Assert
            settings.SaveAttachments.Should().BeTrue();
            store[nameof(Settings.SaveAttachments)].Should().Be(true);
            store.SaveCount.Should().Be(1);
        }

        [TestMethod]
        public void SavePictures_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            var store = CreateStore(initialValue: false);
            var settings = new AppQuickFilerSettings(store);

            // Act
            settings.SavePictures = true;

            // Assert
            settings.SavePictures.Should().BeTrue();
            store[nameof(Settings.SavePictures)].Should().Be(true);
            store.SaveCount.Should().Be(1);
        }

        [TestMethod]
        public void SaveEmailCopy_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            var store = CreateStore(initialValue: false);
            var settings = new AppQuickFilerSettings(store);

            // Act
            settings.SaveEmailCopy = true;

            // Assert
            settings.SaveEmailCopy.Should().BeTrue();
            store[nameof(Settings.SaveEmailCopy)].Should().Be(true);
            store.SaveCount.Should().Be(1);
        }

        [TestMethod]
        public void SaveAttachments_Getter_ReflectsSettingsDefaultFalseAndTrue()
        {
            // Arrange / Act / Assert: cover both boolean states of the getter.
            var store = CreateStore(initialValue: false);
            var settings = new AppQuickFilerSettings(store);
            settings.SaveAttachments.Should().BeFalse();

            store[nameof(Settings.SaveAttachments)] = true;
            settings.SaveAttachments.Should().BeTrue();
            store.SaveCount.Should().Be(0, "reading a setting does not persist anything");
        }
    }
}
