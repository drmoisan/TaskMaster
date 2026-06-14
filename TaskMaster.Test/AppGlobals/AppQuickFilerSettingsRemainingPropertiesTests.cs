using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Covers the remaining pure boolean properties of <see cref="AppQuickFilerSettings"/> not
    /// already exercised by <c>AppQuickFilerSettingsTests</c> (which covers HighConfidenceModeEnabled
    /// and HighConfidenceThreshold): MoveEntireConversation, SaveAttachments, SavePictures, and
    /// SaveEmailCopy. Each getter reads <see cref="Settings.Default"/> and each internal setter
    /// (reachable via InternalsVisibleTo("TaskMaster.Test")) writes Settings.Default and Save()s.
    /// The affected Settings.Default values are snapshotted in <see cref="TestInitialize"/> and
    /// restored in <see cref="TestCleanup"/> so machine state is not mutated; no injectable settings
    /// seam is introduced.
    /// </summary>
    [TestClass]
    public class AppQuickFilerSettingsRemainingPropertiesTests
    {
        private bool _origMoveEntireConversations;
        private bool _origSaveAttachments;
        private bool _origSavePictures;
        private bool _origSaveEmailCopy;

        [TestInitialize]
        public void TestInitialize()
        {
            _origMoveEntireConversations = Settings.Default.MoveEntireConversations;
            _origSaveAttachments = Settings.Default.SaveAttachments;
            _origSavePictures = Settings.Default.SavePictures;
            _origSaveEmailCopy = Settings.Default.SaveEmailCopy;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Settings.Default.MoveEntireConversations = _origMoveEntireConversations;
            Settings.Default.SaveAttachments = _origSaveAttachments;
            Settings.Default.SavePictures = _origSavePictures;
            Settings.Default.SaveEmailCopy = _origSaveEmailCopy;
        }

        [TestMethod]
        public void MoveEntireConversation_Getter_ReflectsSettingsDefault()
        {
            // Arrange
            Settings.Default.MoveEntireConversations = true;
            var settings = new AppQuickFilerSettings();

            // Act / Assert
            settings.MoveEntireConversation.Should().BeTrue("the getter reads Settings.Default");

            // Arrange a different persisted value
            Settings.Default.MoveEntireConversations = false;

            // Act / Assert
            settings.MoveEntireConversation.Should().BeFalse();
        }

        [TestMethod]
        public void MoveEntireConversation_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            Settings.Default.MoveEntireConversations = false;
            var settings = new AppQuickFilerSettings();

            // Act
            settings.MoveEntireConversation = true;

            // Assert
            settings
                .MoveEntireConversation.Should()
                .BeTrue("the setter persists to Settings.Default");
            Settings.Default.MoveEntireConversations.Should().BeTrue();
        }

        [TestMethod]
        public void SaveAttachments_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            Settings.Default.SaveAttachments = false;
            var settings = new AppQuickFilerSettings();

            // Act
            settings.SaveAttachments = true;

            // Assert
            settings.SaveAttachments.Should().BeTrue();
            Settings.Default.SaveAttachments.Should().BeTrue();
        }

        [TestMethod]
        public void SavePictures_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            Settings.Default.SavePictures = false;
            var settings = new AppQuickFilerSettings();

            // Act
            settings.SavePictures = true;

            // Assert
            settings.SavePictures.Should().BeTrue();
            Settings.Default.SavePictures.Should().BeTrue();
        }

        [TestMethod]
        public void SaveEmailCopy_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            Settings.Default.SaveEmailCopy = false;
            var settings = new AppQuickFilerSettings();

            // Act
            settings.SaveEmailCopy = true;

            // Assert
            settings.SaveEmailCopy.Should().BeTrue();
            Settings.Default.SaveEmailCopy.Should().BeTrue();
        }

        [TestMethod]
        public void SaveAttachments_Getter_ReflectsSettingsDefaultFalseAndTrue()
        {
            // Arrange / Act / Assert: cover both boolean states of the getter.
            Settings.Default.SaveAttachments = false;
            var settings = new AppQuickFilerSettings();
            settings.SaveAttachments.Should().BeFalse();

            Settings.Default.SaveAttachments = true;
            settings.SaveAttachments.Should().BeTrue();
        }
    }
}
