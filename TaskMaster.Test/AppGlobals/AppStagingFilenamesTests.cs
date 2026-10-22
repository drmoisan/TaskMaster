using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster.Properties;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Unit tests for <see cref="AppStagingFilenames"/>. Each property has a lazy getter (via
    /// <c>InitProp</c>) that reads the injected settings store, and a setter that writes the backing
    /// field plus the store and saves through it, except <c>EmailInfoStagingFile</c>, whose setter
    /// writes only the backing field. Every test injects its own seeded
    /// <see cref="InMemorySettingsStore"/>, so no shared settings instance, settings file, or other
    /// filesystem state is read or written.
    /// </summary>
    [TestClass]
    public class AppStagingFilenamesTests
    {
        private static readonly string[] SettingNames =
        {
            nameof(Settings.File_ConditionalReminders),
            nameof(Settings.File_Common_Words),
            nameof(Settings.File_Subject_Map),
            nameof(Settings.File_CTF_Inc),
            nameof(Settings.File_CTF_Map),
            nameof(Settings.FileName_EmailSessionTmp),
            nameof(Settings.FileName_EmailSession),
            nameof(Settings.FileName_MovedEmailsBackup),
            nameof(Settings.FileName_Recents),
            nameof(Settings.FileName_EmailInfoStaging),
        };

        private static InMemorySettingsStore CreateSeededStore()
        {
            var store = new InMemorySettingsStore();
            foreach (var name in SettingNames)
            {
                store[name] = $@"seeded\{name}.txt";
            }

            return store;
        }

        [TestMethod]
        public void ConditionalReminders_Getter_ReturnsPersistedSettingsValue()
        {
            // Arrange: a fresh instance reads the lazy getter from the settings store.
            var store = CreateSeededStore();
            store[nameof(Settings.File_ConditionalReminders)] = @"persisted\reminders.json";
            var filenames = new AppStagingFilenames(store);

            // Act
            var result = filenames.ConditionalReminders;

            // Assert
            result
                .Should()
                .Be(@"persisted\reminders.json", "the getter lazily reads the settings store");
            store.SaveCount.Should().Be(0);
        }

        [TestMethod]
        public void ConditionalReminders_Setter_RoundTripsThroughSettingsDefault()
        {
            // Arrange
            var store = CreateSeededStore();
            var filenames = new AppStagingFilenames(store);

            // Act
            var saves = SettingsSaveProbe.CountSavesOnCurrentThread(
                Settings.Default,
                () => filenames.ConditionalReminders = @"new\reminders.json"
            );

            // Assert
            filenames
                .ConditionalReminders.Should()
                .Be(@"new\reminders.json", "the setter caches the value in the backing field");
            store[nameof(Settings.File_ConditionalReminders)]
                .Should()
                .Be(@"new\reminders.json", "the setter persists the value to the settings store");
            store.SaveCount.Should().Be(1);
            saves
                .Should()
                .Be(0, "the test persisted settings to the shared per-host settings file");
        }

        [TestMethod]
        public void CommonWords_Getter_ReturnsPersistedSettingsValue()
        {
            // Arrange
            var store = CreateSeededStore();
            store[nameof(Settings.File_Common_Words)] = @"persisted\common.txt";
            var filenames = new AppStagingFilenames(store);

            // Act
            var result = filenames.CommonWords;

            // Assert
            result.Should().Be(@"persisted\common.txt");
        }

        [TestMethod]
        public void CommonWords_GetterAfterSet_ReturnsCachedValueWithoutReReadingSettings()
        {
            // Arrange: prove the backing field short-circuits the lazy getter once set.
            var store = CreateSeededStore();
            var filenames = new AppStagingFilenames(store);
            filenames.CommonWords = @"cached\value.txt";

            // Act: change the settings store underneath; the cached field should win.
            store[nameof(Settings.File_Common_Words)] = @"changed\underneath.txt";
            var result = filenames.CommonWords;

            // Assert
            result
                .Should()
                .Be(
                    @"cached\value.txt",
                    "the non-null backing field short-circuits the lazy getter"
                );
            store.SaveCount.Should().Be(1);
        }

        [TestMethod]
        public void EmailInfoStagingFile_Setter_DoesNotWriteSettingsDefault()
        {
            // Arrange: this setter is the documented exception that updates only the backing field.
            var store = CreateSeededStore();
            store[nameof(Settings.FileName_EmailInfoStaging)] = @"original\staging.json";
            var filenames = new AppStagingFilenames(store);

            // Act
            filenames.EmailInfoStagingFile = @"local-only\staging.json";

            // Assert
            filenames
                .EmailInfoStagingFile.Should()
                .Be(@"local-only\staging.json", "the backing field is updated");
            store[nameof(Settings.FileName_EmailInfoStaging)]
                .Should()
                .Be(
                    @"original\staging.json",
                    "the EmailInfoStagingFile setter does NOT persist to the settings store"
                );
            store.SaveCount.Should().Be(0);
        }

        [TestMethod]
        public void EmailInfoStagingFile_Getter_LazilyReadsSettingsDefaultWhenUnset()
        {
            // Arrange
            var store = CreateSeededStore();
            store[nameof(Settings.FileName_EmailInfoStaging)] = @"persisted\staging.json";
            var filenames = new AppStagingFilenames(store);

            // Act
            var result = filenames.EmailInfoStagingFile;

            // Assert
            result
                .Should()
                .Be(@"persisted\staging.json", "the getter lazily reads the settings store");
        }

        [TestMethod]
        public void SavingSetters_EachAssignment_WritesStoreAndSavesOnce()
        {
            // Arrange
            var store = CreateSeededStore();
            var filenames = new AppStagingFilenames(store);

            // Act: assign all nine persisting properties.
            filenames.ConditionalReminders = "a";
            filenames.CommonWords = "b";
            filenames.SubjectMap = "c";
            filenames.CtfInc = "d";
            filenames.CtfMap = "e";
            filenames.EmailSessionTemp = "f";
            filenames.EmailSession = "g";
            filenames.MovedMails = "h";
            filenames.RecentsFile = "i";

            // Assert
            store[nameof(Settings.File_ConditionalReminders)].Should().Be("a");
            store[nameof(Settings.File_Common_Words)].Should().Be("b");
            store[nameof(Settings.File_Subject_Map)].Should().Be("c");
            store[nameof(Settings.File_CTF_Inc)].Should().Be("d");
            store[nameof(Settings.File_CTF_Map)].Should().Be("e");
            store[nameof(Settings.FileName_EmailSessionTmp)].Should().Be("f");
            store[nameof(Settings.FileName_EmailSession)].Should().Be("g");
            store[nameof(Settings.FileName_MovedEmailsBackup)].Should().Be("h");
            store[nameof(Settings.FileName_Recents)].Should().Be("i");
            store.SaveCount.Should().Be(9, "each persisting setter saves exactly once");
        }

        [TestMethod]
        public void Getters_UnsetFields_ReadSeededStoreValues()
        {
            // Arrange
            var store = CreateSeededStore();
            var filenames = new AppStagingFilenames(store);

            // Act
            var values = new[]
            {
                filenames.ConditionalReminders,
                filenames.CommonWords,
                filenames.SubjectMap,
                filenames.CtfInc,
                filenames.CtfMap,
                filenames.EmailSessionTemp,
                filenames.EmailSession,
                filenames.MovedMails,
                filenames.RecentsFile,
                filenames.EmailInfoStagingFile,
            };

            // Assert
            for (var i = 0; i < SettingNames.Length; i++)
            {
                values[i]
                    .Should()
                    .Be(
                        $@"seeded\{SettingNames[i]}.txt",
                        $"{SettingNames[i]} reads its seeded key"
                    );
            }

            store.SaveCount.Should().Be(0, "reading a setting does not persist anything");
        }

        [TestMethod]
        public void Constructor_Parameterless_UsesApplicationSettingsStore()
        {
            // Arrange
            var field = typeof(AppStagingFilenames).GetField(
                "_settingsStore",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act: inspect the default store without reading or writing any setting.
            var store = field!.GetValue(new AppStagingFilenames());

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
            Action act = () => _ = new AppStagingFilenames(null!);

            // Act / Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("settingsStore");
        }
    }
}
