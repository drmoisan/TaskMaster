using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public partial class ClassifierGroupUtilities_Tests
    {
        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithEmptyCollection_ShouldReturnGroupWithZeroCounts()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);

            var result = await utils.CreateClassifierGroupAsync(
                System.Array.Empty<MinedMailInfo>()
            );

            result.Should().NotBeNull();
            result.TotalEmailCount.Should().Be(0);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithTokens_ShouldBuildSharedTokenBase()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);
            var collection = new[]
            {
                new MinedMailInfo { Tokens = new[] { "hello", "world", "hello" } },
                new MinedMailInfo { Tokens = new[] { "world", "test" } },
            };

            var result = await utils.CreateClassifierGroupAsync(collection);

            result.TotalEmailCount.Should().Be(2);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CreateClassifierGroupAsync_WithMinimumCountFilter_ShouldFilterTokens()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);
            var collection = new[]
            {
                new MinedMailInfo { Tokens = new[] { "rare", "common", "common" } },
                new MinedMailInfo { Tokens = new[] { "common" } },
            };

            var result = await utils.CreateClassifierGroupAsync(
                collection,
                minimumCountPerToken: 3
            );

            result.TotalEmailCount.Should().Be(2);
            result.SharedTokenBase.Should().NotBeNull();
        }

        [TestMethod]
        public void Globals_ShouldReturnInjectedGlobals()
        {
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);

            utils.Globals.Should().BeSameAs(globals.Object);
        }

        [TestMethod]
        public void Deserialize_WhenAppDataFolderNotConfigured_ShouldReturnDefault()
        {
            var globals = CreateGlobals();
            var fs = new Mock<IFileSystemFolderPaths>();
            var specialFolders = new ConcurrentDictionary<string, string>();
            fs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            globals.SetupGet(x => x.FS).Returns(fs.Object);
            var utils = new ClassifierGroupUtilities(globals.Object);

            var result = utils.Deserialize<BayesianClassifierGroup>("test");

            result.Should().BeNull();
        }

        // -----------------------------------------------------------------------
        // P43-T1 — Existing loader path resolves to expected classifier group
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that when Deserialize returns a pre-existing group,
        /// GetOrCreateClassifierGroupAsync returns that group without creating a new one.
        ///
        /// Purpose:
        ///     Confirm the "load existing" branch of GetOrCreateClassifierGroupAsync is
        ///     taken when a persisted group is available, and that the returned instance
        ///     is the one provided by the loader rather than a freshly constructed group.
        ///
        /// Returns:
        ///     Passes when the returned group is the same reference as the stubbed group
        ///     and TotalEmailCount matches the preset value.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreate_WhenDeserializedGroupExists_ReturnsExistingGroup()
        {
            // Arrange: stub Deserialize to return a known group with TotalEmailCount = 42
            var globals = CreateGlobalsWithEmptyFs();
            var preexistingGroup = new BayesianClassifierGroup { TotalEmailCount = 42 };
            var utils = new StubClassifierGroupUtilities(globals.Object, preexistingGroup);

            // Act
            var result = await utils.GetOrCreateClassifierGroupAsync(
                Array.Empty<MinedMailInfo>(),
                "KnownGroup"
            );

            // Assert: the pre-existing instance is returned unchanged
            result.Should().BeSameAs(preexistingGroup);
            result.TotalEmailCount.Should().Be(42);
        }

        // -----------------------------------------------------------------------
        // P43-T2 — Missing config returns a fallback or new classifier
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that when Deserialize returns null, GetOrCreateClassifierGroupAsync
        /// creates and returns a valid newly initialized classifier group.
        ///
        /// Purpose:
        ///     Confirm the "create new" branch executes when no persisted group exists
        ///     and that the returned group is non-null with the expected item count.
        ///
        /// Returns:
        ///     Passes when the result is non-null and TotalEmailCount equals the input
        ///     collection size.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreate_WhenDeserializedGroupIsNull_ReturnsFreshGroup()
        {
            // Arrange: stub Deserialize to return null (no persisted group)
            var globals = CreateGlobalsWithEmptyFs();
            var utils = new StubClassifierGroupUtilities(globals.Object, stubbedResult: null);
            var collection = new[] { new MinedMailInfo { Tokens = new[] { "token" } } };

            // Act
            var result = await utils.GetOrCreateClassifierGroupAsync(collection, "NewGroup");

            // Assert: a fresh, non-null group with the correct count is returned
            result.Should().NotBeNull();
            result.TotalEmailCount.Should().Be(1);
        }

        // -----------------------------------------------------------------------
        // P43-T3 — Serialize/deserialize round-trip preserves expected config fields
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that the serializer returned by GetSerializer preserves
        /// TotalEmailCount through a JSON round-trip.
        ///
        /// Purpose:
        ///     Confirm that the configured JsonSerializer settings (TypeNameHandling.Auto,
        ///     Formatting.Indented) produce valid JSON that can be deserialized back to
        ///     a BayesianClassifierGroup with the expected field values.
        ///
        /// Returns:
        ///     Passes when the round-tripped TotalEmailCount equals the original value.
        /// </summary>
        [TestMethod]
        public void GetSerializer_RoundTrip_PreservesExpectedConfigFields()
        {
            // Arrange
            var globals = CreateGlobals();
            var utils = new ClassifierGroupUtilities(globals.Object);
            var serializer = utils.GetSerializer();

            var original = new BayesianClassifierGroup { TotalEmailCount = 99 };

            // Act: serialize to string, then deserialize back
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                serializer.Serialize(sw, original);
            }

            BayesianClassifierGroup roundTripped;
            using (var sr = new StringReader(sb.ToString()))
            using (var jr = new JsonTextReader(sr))
            {
                roundTripped = serializer.Deserialize<BayesianClassifierGroup>(jr);
            }

            // Assert
            roundTripped.Should().NotBeNull();
            roundTripped!.TotalEmailCount.Should().Be(99);
        }

        [TestMethod]
        public void SerializeAndSave_WhenAppDataFolderExists_UsesBayesianFolderAndSuffixFileName()
        {
            var globals = ClassifierGroupUtilitiesTestSupport.CreateGlobalsWithAppData(
                @"C:\AppDataRoot"
            );
            var utils = new RecordingClassifierGroupUtilities(globals.Object);

            utils.SerializeAndSave(new { Name = "fixture" }, "Seed", "0001");

            utils.CapturedFolderPath.Should().Be(Path.Combine(@"C:\AppDataRoot", "Bayesian"));
            utils.CapturedFileName.Should().Be("Seed_0001.json");
        }

        [TestMethod]
        public void LogSizeComparison_WhenCalled_DoesNotThrow()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);

            var action = () => utils.LogSizeComparison("GC", 10, "Json", 20, "MailItem");

            action.Should().NotThrow();
        }

        [TestMethod]
        public void SerializeActiveItem_WhenLoaderReturnsNull_DoesNotSerializeMailInfo()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                LoaderResults = [null],
            };

            utils.SerializeActiveItem();

            utils.SerializeMailInfoCalls.Should().Be(0);
        }

        [TestMethod]
        public void SerializeActiveItem_WhenLoaderReturnsMailItem_InvokesSerializeMailInfo()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                LoaderResults = [new Mock<Microsoft.Office.Interop.Outlook.MailItem>().Object],
                InvokeBaseSerializeMailInfo = false,
            };

            utils.SerializeActiveItem();

            utils.SerializeMailInfoCalls.Should().Be(1);
        }

        [TestMethod]
        public void SerializeMailInfo_WhenAppDataMissing_ReturnsWithoutSavingExamples()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobalsWithEmptyFs().Object);

            utils.SerializeMailInfo(new Mock<Microsoft.Office.Interop.Outlook.MailItem>().Object);

            utils.SavedExampleNames.Should().BeEmpty();
        }

        [TestMethod]
        public void SerializeMailInfo_WhenAppDataConfigured_SavesMailAndDerivedExamples()
        {
            var globals = ClassifierGroupUtilitiesTestSupport.CreateGlobalsWithAppData(
                @"C:\AppDataRoot"
            );
            var utils = new RecordingClassifierGroupUtilities(globals.Object)
            {
                LoaderResults = [null, null],
                LoaderSizes = [11, 22],
            };

            utils.SerializeMailInfo(new Mock<Microsoft.Office.Interop.Outlook.MailItem>().Object);

            utils
                .SavedExampleNames.Should()
                .ContainInOrder("MailItem", "MailItemInfo", "MinedMailInfo");
            utils.CapturedFolderPath.Should().Be(Path.Combine(@"C:\AppDataRoot", "Bayesian"));
            utils.LoggedObjectNames.Should().ContainInOrder("MailItemInfo", "MinedMailInfo");
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenLoaderIsNull_ThrowsArgumentNullException()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);

            var action = () => utils.TryLoadObjectAndGetMemorySize<string>(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenCopiesToLoadLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);

            var action = () => utils.TryLoadObjectAndGetMemorySize(() => "value", 0);

            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenLoaderSucceedsAcrossCopies_ReturnsResult()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);
            var callCount = 0;

            var (result, size) = utils.TryLoadObjectAndGetMemorySize(
                () =>
                {
                    callCount++;
                    return new object();
                },
                copiesToLoad: 3
            );

            result.Should().NotBeNull();
            callCount.Should().Be(3);
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenReplicaLoaderThrows_ReturnsDefaultAndZero()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);
            var callCount = 0;

            var (result, size) = utils.TryLoadObjectAndGetMemorySize(
                () =>
                {
                    callCount++;
                    if (callCount == 2)
                    {
                        throw new InvalidOperationException("boom");
                    }

                    return new object();
                },
                copiesToLoad: 3
            );

            result.Should().BeNull();
            size.Should().Be(0);
        }

        [TestMethod]
        public async Task ValidateJson_WhenDeserializeAsyncReturnsValue_ReturnsTrue()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                ValidationResult = new BayesianClassifierGroup { TotalEmailCount = 7 },
            };

            var result = await utils.ValidateJson<BayesianClassifierGroup>("group");

            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateJson_WhenDeserializeAsyncReturnsNull_ReturnsFalse()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object);

            var result = await utils.ValidateJson<BayesianClassifierGroup>("group");

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task ValidateJson_WhenDeserializeAsyncThrowsWithSuffix_ReturnsFalse()
        {
            var utils = new RecordingClassifierGroupUtilities(CreateGlobals().Object)
            {
                ValidationException = new InvalidOperationException("bad json"),
            };

            var result = await utils.ValidateJson<BayesianClassifierGroup>("group", "backup");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetProgressMessage_WhenCompletedWorkExists_IncludesCountsAndElapsedText()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);
            var method = typeof(ClassifierGroupUtilities).GetMethod(
                "GetProgressMessage",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();

            var message = (string)method.Invoke(utils, [2, 4, stopwatch]);

            message.Should().Contain("Completed 2 of 4");
            message.Should().Contain("elapsed");
            message.Should().Contain("remaining");
        }

        [TestMethod]
        public void GetProgressMessage_WhenCompleteIsZero_UsesZeroRate()
        {
            var utils = new ClassifierGroupUtilities(CreateGlobals().Object);
            var method = typeof(ClassifierGroupUtilities).GetMethod(
                "GetProgressMessage",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();

            var message = (string)method.Invoke(utils, [0, 4, stopwatch]);

            message.Should().Contain("Completed 0 of 4 (0.00 spm)");
        }

        [TestMethod]
        public async Task ToggleOfflineMode_WhenAlreadyOffline_ReturnsTrueWithoutExecutingCommand()
        {
            var commandBars = new Mock<CommandBars>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(x => x.CommandBars).Returns(commandBars.Object);
            var app = new Mock<Application>();
            app.Setup(x => x.ActiveExplorer()).Returns(explorer.Object);
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(x => x.App).Returns(app.Object);
            var globals = CreateGlobals();
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            var method = typeof(ClassifierGroupUtilities).GetMethod(
                "ToggleOfflineMode",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );

            var result = await (Task<bool>)
                method.Invoke(new ClassifierGroupUtilities(globals.Object), [true]);

            result.Should().BeTrue();
            commandBars.Verify(x => x.ExecuteMso(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task ToggleOfflineMode_WhenOnline_ExecutesToggleOnlineAndReturnsFalse()
        {
            var commandBars = new Mock<CommandBars>();
            var explorer = new Mock<Explorer>();
            explorer.Setup(x => x.CommandBars).Returns(commandBars.Object);
            var app = new Mock<Application>();
            app.Setup(x => x.ActiveExplorer()).Returns(explorer.Object);
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(x => x.App).Returns(app.Object);
            var globals = CreateGlobals();
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            var method = typeof(ClassifierGroupUtilities).GetMethod(
                "ToggleOfflineMode",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );

            var result = await (Task<bool>)
                method.Invoke(new ClassifierGroupUtilities(globals.Object), [false]);

            result.Should().BeFalse();
            commandBars.Verify(x => x.ExecuteMso("ToggleOnline"), Times.Once);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static Mock<IApplicationGlobals> CreateGlobals()
        {
            return new Mock<IApplicationGlobals>();
        }

        /// <summary>
        /// Returns a mock <see cref="IApplicationGlobals"/> whose FS.SpecialFolders is
        /// an empty dictionary, causing disk-I/O paths to short-circuit harmlessly.
        /// </summary>
        private static Mock<IApplicationGlobals> CreateGlobalsWithEmptyFs()
        {
            var globals = new Mock<IApplicationGlobals>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            mockFs
                .SetupGet(x => x.SpecialFolders)
                .Returns(new ConcurrentDictionary<string, string>());
            globals.SetupGet(x => x.FS).Returns(mockFs.Object);
            return globals;
        }
    }
}
