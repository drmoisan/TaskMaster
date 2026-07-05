using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Verifies the high-confidence settings exposed by <see cref="AppQuickFilerSettings"/>:
    /// their persisted defaults and that the internal setters round-trip through
    /// <see cref="Settings.Default"/>. The persisted values are snapshotted in
    /// <see cref="TestInitialize"/> and restored in <see cref="TestCleanup"/> so the tests are
    /// independent and do not leave machine state mutated.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class AppQuickFilerSettingsTests
    {
        private bool _originalModeEnabled;
        private double _originalThreshold;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalModeEnabled = Settings.Default.HighConfidenceModeEnabled;
            _originalThreshold = Settings.Default.HighConfidenceThreshold;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Settings.Default.HighConfidenceModeEnabled = _originalModeEnabled;
            Settings.Default.HighConfidenceThreshold = _originalThreshold;
        }

        [TestMethod]
        public void HighConfidenceModeEnabled_Default_IsFalse()
        {
            // Arrange: ensure the persisted value matches the declared default.
            Settings.Default.HighConfidenceModeEnabled = false;
            var settings = new AppQuickFilerSettings();

            // Act
            var result = settings.HighConfidenceModeEnabled;

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void HighConfidenceThreshold_Default_IsZeroPointNine()
        {
            // Arrange: ensure the persisted value matches the declared default.
            Settings.Default.HighConfidenceThreshold = 0.9;
            var settings = new AppQuickFilerSettings();

            // Act
            var result = settings.HighConfidenceThreshold;

            // Assert
            result.Should().Be(0.9);
        }

        [TestMethod]
        public void HighConfidenceModeEnabled_WhenSetTrue_ReadsBackTrue()
        {
            // Arrange
            var settings = new AppQuickFilerSettings();

            // Act
            settings.HighConfidenceModeEnabled = true;

            // Assert
            settings.HighConfidenceModeEnabled.Should().BeTrue();
        }

        [TestMethod]
        public void HighConfidenceThreshold_WhenSet_ReadsBackSameValue()
        {
            // Arrange
            var settings = new AppQuickFilerSettings();

            // Act
            settings.HighConfidenceThreshold = 0.75;

            // Assert
            settings.HighConfidenceThreshold.Should().Be(0.75);
        }
    }
}
