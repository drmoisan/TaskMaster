using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Unit tests for <see cref="AppStagingFilenames"/>. The class delegates each property to the
    /// static <see cref="Settings.Default"/> singleton (lazy getter via <c>InitProp</c>, and a
    /// setter that writes the backing field plus Settings.Default and calls Save() — except
    /// <c>EmailInfoStagingFile</c>, whose setter writes only the backing field). There is no
    /// injectable settings type, so the established pattern from <c>AppQuickFilerSettingsTests</c>
    /// is used: the affected Settings.Default values are snapshotted in <see cref="TestInitialize"/>
    /// and restored in <see cref="TestCleanup"/> so machine state is not mutated and tests stay
    /// independent. No filesystem access and no temp files are used.
    /// </summary>
    [TestClass]
    public class AppStagingFilenamesTests
    {
        private string _origConditionalReminders;
        private string _origCommonWords;
        private string _origEmailInfoStaging;

        [TestInitialize]
        public void TestInitialize()
        {
            _origConditionalReminders = Settings.Default.File_ConditionalReminders;
            _origCommonWords = Settings.Default.File_Common_Words;
            _origEmailInfoStaging = Settings.Default.FileName_EmailInfoStaging;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Settings.Default.File_ConditionalReminders = _origConditionalReminders;
            Settings.Default.File_Common_Words = _origCommonWords;
            Settings.Default.FileName_EmailInfoStaging = _origEmailInfoStaging;
        }

        [TestMethod]
        public void ConditionalReminders_Getter_ReturnsPersistedSettingsValue()
        {
            // Arrange: a fresh instance reads the lazy getter from Settings.Default.
            Settings.Default.File_ConditionalReminders = @"persisted\reminders.json";
            var filenames = new AppStagingFilenames();

            // Act
            var result = filenames.ConditionalReminders;

            // Assert
            result
                .Should()
                .Be(@"persisted\reminders.json", "the getter lazily reads Settings.Default");
        }

        [TestMethod]
        public void ConditionalReminders_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            var filenames = new AppStagingFilenames();

            // Act
            filenames.ConditionalReminders = @"new\reminders.json";

            // Assert
            filenames
                .ConditionalReminders.Should()
                .Be(@"new\reminders.json", "the setter caches the value in the backing field");
            Settings
                .Default.File_ConditionalReminders.Should()
                .Be(@"new\reminders.json", "the setter persists the value to Settings.Default");
        }

        [TestMethod]
        public void CommonWords_Getter_ReturnsPersistedSettingsValue()
        {
            // Arrange
            Settings.Default.File_Common_Words = @"persisted\common.txt";
            var filenames = new AppStagingFilenames();

            // Act
            var result = filenames.CommonWords;

            // Assert
            result.Should().Be(@"persisted\common.txt");
        }

        [TestMethod]
        public void CommonWords_GetterAfterSet_ReturnsCachedValueWithoutReReadingSettings()
        {
            // Arrange: prove the backing field short-circuits the lazy getter once set.
            var filenames = new AppStagingFilenames();
            filenames.CommonWords = @"cached\value.txt";

            // Act: change Settings.Default underneath; the cached field should win.
            Settings.Default.File_Common_Words = @"changed\underneath.txt";
            var result = filenames.CommonWords;

            // Assert
            result
                .Should()
                .Be(
                    @"cached\value.txt",
                    "the non-null backing field short-circuits the lazy getter"
                );
        }

        [TestMethod]
        public void EmailInfoStagingFile_Setter_DoesNotWriteSettingsDefault()
        {
            // Arrange: this setter is the documented exception that updates only the backing field.
            Settings.Default.FileName_EmailInfoStaging = @"original\staging.json";
            var filenames = new AppStagingFilenames();

            // Act
            filenames.EmailInfoStagingFile = @"local-only\staging.json";

            // Assert
            filenames
                .EmailInfoStagingFile.Should()
                .Be(@"local-only\staging.json", "the backing field is updated");
            Settings
                .Default.FileName_EmailInfoStaging.Should()
                .Be(
                    @"original\staging.json",
                    "the EmailInfoStagingFile setter does NOT persist to Settings.Default"
                );
        }

        [TestMethod]
        public void EmailInfoStagingFile_Getter_LazilyReadsSettingsDefaultWhenUnset()
        {
            // Arrange
            Settings.Default.FileName_EmailInfoStaging = @"persisted\staging.json";
            var filenames = new AppStagingFilenames();

            // Act
            var result = filenames.EmailInfoStagingFile;

            // Assert
            result
                .Should()
                .Be(@"persisted\staging.json", "the getter lazily reads Settings.Default");
        }
    }
}
