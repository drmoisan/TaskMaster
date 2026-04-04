using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScBag_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyBag()
        {
            // Arrange
            var bag = new ScBag<int>();

            // Act
            var tookItem = bag.TryTake(out var value);

            // Assert
            bag.Count.Should().Be(0);
            tookItem.Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public void CollectionConstructor_PopulatesBag()
        {
            // Arrange
            var bag = new ScBag<string>(new[] { "alpha", "beta", "gamma" });

            // Act
            var values = bag.OrderBy(value => value).ToArray();

            // Assert
            values.Should().Equal("alpha", "beta", "gamma");
            bag.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddTryTakeAndEnumeration_WorkForTypicalBagUsage()
        {
            // Arrange
            var bag = new ScBag<int>();
            bag.Add(1);
            bag.Add(2);
            bag.Add(3);

            // Act
            var took = bag.TryTake(out var removed);
            var remaining = bag.OrderBy(value => value).ToArray();

            // Assert
            took.Should().BeTrue();
            removed.Should().BeOneOf(1, 2, 3);
            remaining.Should().HaveCount(2);
            remaining.Should().OnlyHaveUniqueItems();
            remaining.Should().OnlyContain(value => value >= 1 && value <= 3);
        }

        [TestMethod]
        public async Task ConcurrentAdds_PreserveAllItems()
        {
            // Arrange
            var bag = new ScBag<int>();
            var items = Enumerable.Range(1, 64).ToArray();

            // Act
            await Task.WhenAll(items.Select(item => Task.Run(() => bag.Add(item))));
            var ordered = bag.OrderBy(value => value).ToArray();

            // Assert
            bag.Count.Should().Be(items.Length);
            ordered.Should().Equal(items);
        }

        [TestMethod]
        public void FilePathPropertiesAndDiskActivation_SwapActiveStorageProfile()
        {
            // Arrange
            var bag = new ScBag<int>
            {
                LocalDisk = new FilePathHelper("local.json", @"C:\local"),
                NetDisk = new FilePathHelper("net.json", @"C:\net"),
                LocalJsonSettings = new JsonSerializerSettings { Formatting = Formatting.None },
                NetJsonSettings = new JsonSerializerSettings { Formatting = Formatting.Indented },
            };

            // Act
            bag.ActivateLocalDisk();
            var localPath = bag.FilePath;
            var localFormatting = bag.JsonSettings.Formatting;
            bag.ActivateNetDisk();
            var netPath = bag.FilePath;
            var netFormatting = bag.JsonSettings.Formatting;

            // Assert
            localPath.Should().Be(@"C:\local\local.json");
            localFormatting.Should().Be(Formatting.None);
            netPath.Should().Be(@"C:\net\net.json");
            netFormatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void Serialize_WithNoConfiguredPath_IsASafeNoOp()
        {
            // Arrange
            var bag = new ScBag<int>();
            bag.Add(42);

            // Act
            bag.Serialize();

            // Assert
            bag.Count.Should().Be(1);
            bag.Single().Should().Be(42);
        }

        [TestMethod]
        public void GetDefaultSettings_ReturnsIndentedTypeAwareSettings()
        {
            // Arrange

            // Act
            var settings = ScBag<int>.GetDefaultSettings();

            // Assert
            settings.Formatting.Should().Be(Formatting.Indented);
            settings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void JsonSettings_SetAndGet_ReturnsAssignedSettings()
        {
            // Arrange
            var bag = new ScBag<int>();
            var settings = new JsonSerializerSettings { Formatting = Formatting.None };

            // Act
            bag.JsonSettings = settings;

            // Assert
            bag.JsonSettings.Formatting.Should().Be(Formatting.None);
        }

        [TestMethod]
        public void JsonSerialize_ScBagWithItems_ProducesValidJson()
        {
            // Arrange
            var bag = new ScBag<string>(new[] { "alpha", "beta" });
            var settings = ScBag<string>.GetDefaultSettings();

            // Act
            var json = JsonConvert.SerializeObject(bag, settings);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("alpha");
            json.Should().Contain("beta");
        }

        [TestMethod]
        public void JsonDeserialize_ValidJson_RecreatsBag()
        {
            // Arrange
            var original = new ScBag<int>(new[] { 1, 2, 3 });
            var settings = ScBag<int>.GetDefaultSettings();
            var json = JsonConvert.SerializeObject(original, settings);

            // Act
            var restored = JsonConvert.DeserializeObject<ScBag<int>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.OrderBy(x => x).Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void JsonRoundTrip_EmptyBag_PreservesEmpty()
        {
            // Arrange
            var original = new ScBag<double>();
            var settings = ScBag<double>.GetDefaultSettings();

            // Act
            var json = JsonConvert.SerializeObject(original, settings);
            var restored = JsonConvert.DeserializeObject<ScBag<double>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().BeEmpty();
        }

        [TestMethod]
        public void FileName_SetAndGetProperty_Works()
        {
            // Arrange
            var bag = new ScBag<int>();

            // Act
            bag.FileName = "test.json";

            // Assert
            bag.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void FolderPath_SetAndGetProperty_Works()
        {
            // Arrange
            var bag = new ScBag<int>();

            // Act
            bag.FolderPath = @"C:\data";

            // Assert
            bag.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void NetJsonSettings_SetAndGet_Works()
        {
            // Arrange
            var bag = new ScBag<int>();
            var settings = new JsonSerializerSettings { Formatting = Formatting.Indented };

            // Act
            bag.NetJsonSettings = settings;

            // Assert
            bag.NetJsonSettings.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void LocalJsonSettings_SetAndGet_Works()
        {
            // Arrange
            var bag = new ScBag<int>();
            var settings = new JsonSerializerSettings { Formatting = Formatting.None };

            // Act
            bag.LocalJsonSettings = settings;

            // Assert
            bag.LocalJsonSettings.Formatting.Should().Be(Formatting.None);
        }

        [TestMethod]
        public void LocalDiskAndNetDisk_GettersReturnAssignedHelpers()
        {
            // Arrange
            var localDisk = new FilePathHelper("local.json", @"C:\local");
            var netDisk = new FilePathHelper("net.json", @"C:\net");
            var bag = new ScBag<int> { LocalDisk = localDisk, NetDisk = netDisk };

            // Act
            var observedLocalDisk = bag.LocalDisk;
            var observedNetDisk = bag.NetDisk;

            // Assert
            observedLocalDisk.Should().BeSameAs(localDisk);
            observedNetDisk.Should().BeSameAs(netDisk);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseIsYes_ReturnsBagAndTracksFilePath()
        {
            // Arrange
            var disk = new FilePathHelper("*created-scbag.json", @"C:\ScBag");

            // Act
            var bag = TestableScBag<int>.ExposeCreateEmpty(DialogResult.Yes, disk);
            StopPendingSerializationTimer(bag);

            // Assert
            bag.Should().NotBeNull();
            bag.FilePath.Should().Be(disk.FilePath);
        }

        [TestMethod]
        public void CreateEmpty_WithSettingsWhenResponseIsYes_CopiesSettings()
        {
            // Arrange
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
            };
            var disk = new FilePathHelper("*created-scbag-settings.json", @"C:\ScBag");

            // Act
            var bag = TestableScBag<int>.ExposeCreateEmpty(DialogResult.Yes, disk, settings);
            StopPendingSerializationTimer(bag);

            // Assert
            bag.Should().NotBeNull();
            bag.FilePath.Should().Be(disk.FilePath);
            bag.JsonSettings.Should().BeSameAs(settings);
        }

        [TestMethod]
        public void AskUser_WhenPromptDisabled_ReturnsYes()
        {
            // Arrange

            // Act
            var response = TestableScBag<int>.ExposeAskUser(false, "ignored");

            // Assert
            response.Should().Be(DialogResult.Yes);
        }

        [TestMethod]
        public void Deserialize_DefaultOverloadWithMissingPath_ReturnsEmptyBag()
        {
            // Arrange

            // Act
            var bag = ScBag<int>.Deserialize(
                "missing-default-scbag.json",
                @"C:\MissingScBagDefault"
            );
            StopPendingSerializationTimer(bag);

            // Assert
            bag.Should().NotBeNull();
            bag.Should().BeEmpty();
            bag.JsonSettings.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
        }

        [TestMethod]
        public void Deserialize_WithCustomSettingsAndMissingPath_ReturnsEmptyBagWithCopiedSettings()
        {
            // Arrange
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
            };

            // Act
            var bag = ScBag<int>.Deserialize(
                "missing-custom-scbag.json",
                @"C:\MissingScBagCustom",
                false,
                settings
            );
            StopPendingSerializationTimer(bag);

            // Assert
            bag.Should().NotBeNull();
            bag.Should().BeEmpty();
            bag.FilePath.Should()
                .Be(Path.Combine(@"C:\MissingScBagCustom", "missing-custom-scbag.json"));
            bag.JsonSettings.Should().BeSameAs(settings);
        }

        [TestMethod]
        public void Deserialize_WithMissingFileInExistingDirectory_UsesFileNotFoundBranch()
        {
            // Arrange
            var fileName = "__missing-fileonly-scbag.json";
            var folderPath = AppContext.BaseDirectory;
            var expectedPath = Path.Combine(folderPath, fileName);
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
            };

            File.Exists(expectedPath).Should().BeFalse();

            // Act
            var bag = ScBag<int>.Deserialize(fileName, folderPath, false, settings);
            StopPendingSerializationTimer(bag);

            // Assert
            bag.Should().NotBeNull();
            bag.Should().BeEmpty();
            bag.FilePath.Should().Be(expectedPath);
            bag.JsonSettings.Should().BeSameAs(settings);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInvalidPath_DoesNotThrow()
        {
            // Arrange
            var bag = new ScBag<int>(new[] { 1, 2, 3 });
            var invalidPath = Path.Combine(@"C:\ScBag", "*serialize-thread-safe.json");

            // Act
            Action act = () => bag.SerializeThreadSafe(invalidPath);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void SerializeThreadSafe_WithNullDevice_WritesWithoutThrowing()
        {
            // Arrange
            var bag = new ScBag<int>(new[] { 4, 5, 6 });

            // Act
            Action act = () => bag.SerializeThreadSafe("NUL");

            // Assert
            act.Should().NotThrow();
        }

        // -----------------------------------------------------------------------
        // P51-T1 — Deserializing a missing path returns an empty bag
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that Deserialize with a non-existent file path and
        /// askUserOnError=false returns a valid empty bag rather than throwing.
        ///
        /// Purpose:
        ///     Confirm the FileNotFoundException guard clause creates and returns an
        ///     empty bag without invoking any UI dialog when askUserOnError is false.
        ///
        /// Returns:
        ///     Passes when the returned bag is non-null and contains zero items.
        /// </summary>
        [TestMethod]
        public void Deserialize_WithMissingPath_ReturnsEmptyBag()
        {
            // Act: non-existent file, no dialog (askUserOnError=false defaults to Yes)
            var bag = ScBag<int>.Deserialize(
                "p51t1_nonexistent.json",
                @"c:\nonexistent_scbag_p51t1_dir",
                askUserOnError: false
            );

            // Assert: an empty bag is returned with no exception thrown
            bag.Should().NotBeNull();
            bag.Count.Should().Be(0);
        }

        // -----------------------------------------------------------------------
        // P51-T3 — Ask-user branch handles a cancellation response gracefully
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that CreateEmpty propagates the expected exception when the
        /// user-dialog response is No (i.e. the caller cancels bag creation).
        ///
        /// Purpose:
        ///     Confirm that a DialogResult.No response from the ask-user branch
        ///     results in an ArgumentNullException, which is the designed behavior
        ///     when the caller cancels the empty-bag creation step.
        ///
        /// Returns:
        ///     Passes when CreateEmpty(DialogResult.No, ...) throws ArgumentNullException.
        /// </summary>
        [TestMethod]
        public void CreateEmpty_WhenResponseIsNo_ThrowsArgumentNullException()
        {
            // Arrange: subclass to expose the protected static CreateEmpty
            Action act = () =>
                TestableScBag<int>.ExposeCreateEmpty(
                    DialogResult.No,
                    new FilePathHelper("test.json", @"c:\test")
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        private static void StopPendingSerializationTimer<T>(ScBag<T> bag)
        {
            var timerField = typeof(ScBag<T>).GetField(
                "_timer",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var timer = timerField?.GetValue(bag) as TimerWrapper;

            if (timer is null)
            {
                return;
            }

            timer.StopTimer();
            timer.Dispose();
        }
    }

    /// <summary>Exposes the protected static CreateEmpty method for testing.</summary>
    internal sealed class TestableScBag<T> : ScBag<T>
    {
        internal static ScBag<T> ExposeCreateEmpty(DialogResult response, FilePathHelper disk) =>
            CreateEmpty(response, disk);

        internal static ScBag<T> ExposeCreateEmpty(
            DialogResult response,
            FilePathHelper disk,
            JsonSerializerSettings settings
        ) => CreateEmpty(response, disk, settings);

        internal static DialogResult ExposeAskUser(bool askUserOnError, string messageText) =>
            AskUser(askUserOnError, messageText);
    }
}
