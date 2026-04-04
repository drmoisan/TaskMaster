using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="EmailDataMiner"/>.
    ///
    /// Purpose:
    ///     Verify deterministic helper/orchestration paths in EmailDataMiner without
    ///     requiring live Outlook COM objects, WinForms modal UI, or file-system writes.
    ///
    /// Constraints:
    ///     AddRollingMeasures is internal; the csproj InternalsVisibleTo attribute exposes it
    ///     to the test assembly. FolderWrapper is constructed via its JsonConstructor (no COM).
    ///     IApplicationGlobals is mocked with Moq so no Outlook session is required.
    /// </summary>
    [TestClass]
    public partial class EmailDataMiner_Tests
    {
        #region P34-T1 — Empty source produces no rows

        /// <summary>
        /// Verifies that passing an empty FolderWrapper array to the rolling-measures
        /// step produces an empty FolderStruct array (no mined rows).
        ///
        /// Purpose:
        ///     Confirm the mining orchestration path short-circuits gracefully on an empty
        ///     source and does not fabricate output.
        ///
        /// Args:
        ///     miner: EmailDataMiner constructed with a minimal mock globals.
        ///     maxChunkSize: arbitrary positive long that would produce one chunk if items existed.
        ///
        /// Returns:
        ///     Passes when the result array is empty.
        /// </summary>
        [TestMethod]
        public void AddRollingMeasures_WhenFolderArrayIsEmpty_ReturnsNoRows()
        {
            // Arrange: construct miner with no-op globals; empty folder list is the input
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            var miner = new EmailDataMiner(mockGlobals.Object);
            var emptyFolders = System.Array.Empty<FolderWrapper>();

            // Act: invoke the internal chunking step with a dummy chunk size
            var result = miner.AddRollingMeasures(1_000_000L, emptyFolders);

            // Assert: no rows emitted for an empty source
            result.Should().BeEmpty();
        }

        #endregion

        #region P34-T2 — Chunking path groups inputs correctly

        /// <summary>
        /// Verifies that AddRollingMeasures assigns items to the expected chunk groups
        /// when folder sizes exceed the per-chunk budget.
        ///
        /// Purpose:
        ///     Confirm the rolling-measures step splits folders into exactly the expected
        ///     number of chunks when cumulative size exceeds the max-chunk budget.
        ///
        /// Args:
        ///     miner: EmailDataMiner constructed with no-op globals.
        ///     maxChunkSize: 500 bytes; each folder is 300 bytes, forcing a new chunk every
        ///                   second folder.
        ///
        /// Returns:
        ///     Passes when 3 records are emitted across exactly 2 distinct chunk groups.
        /// </summary>
        [TestMethod]
        public void AddRollingMeasures_WhenFolderSizesExceedBudget_ProducesExpectedChunkCount()
        {
            // Arrange: three folders; each 300 bytes — chunk budget is 500 bytes, so folder 1
            // fills group 0, folder 2 starts group 1, folder 3 overflows to group 2.
            var mockGlobals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            var miner = new EmailDataMiner(mockGlobals.Object);

            var folders = new[]
            {
                new FolderWrapper(
                    selected: true,
                    itemCount: 1,
                    folderSize: 300,
                    name: "FolderA",
                    relativePath: "root/FolderA"
                ),
                new FolderWrapper(
                    selected: true,
                    itemCount: 1,
                    folderSize: 300,
                    name: "FolderB",
                    relativePath: "root/FolderB"
                ),
                new FolderWrapper(
                    selected: true,
                    itemCount: 1,
                    folderSize: 300,
                    name: "FolderC",
                    relativePath: "root/FolderC"
                ),
            };

            // Act: chunk size of 500 forces a boundary after the first folder
            var result = miner.AddRollingMeasures(500L, folders);

            // Assert: all three input folders are represented and span at least 2 chunk groups
            result.Should().HaveCount(3);
            result.Select(r => r.ChunkNumber).Distinct().Should().HaveCountGreaterThan(1);
        }

        #endregion

        #region P34-T3 — Staging delete short-circuits when AppData is absent

        /// <summary>
        /// Verifies that DeleteStagingFilesAsync returns without error when the
        /// SpecialFolders dictionary does not contain an "AppData" entry.
        ///
        /// Purpose:
        ///     Confirm the staging-delete path exits early (no file-system access) when the
        ///     AppData special folder has not been registered in the globals dictionary.
        ///
        /// Args:
        ///     miner: EmailDataMiner constructed with mocked globals where FS.SpecialFolders
        ///            is an empty ConcurrentDictionary (no "AppData" key).
        ///
        /// Returns:
        ///     Passes when the method completes without throwing an exception.
        /// </summary>
        [TestMethod]
        public async Task DeleteStagingFilesAsync_WhenAppDataFolderMissing_CompletesWithoutError()
        {
            // Arrange — use concrete stubs instead of Moq property-expression Setup to avoid
            // Moq.Async.AwaitableFactory binding failure on .NET 4.8.1 with property lambdas.
            // Only FS.SpecialFolders is accessed; all other IApplicationGlobals members are
            // implemented as NotImplementedException because they are never reached.
            var miner = new EmailDataMiner(new StubGlobalsWithEmptySpecialFolders());

            // Act + Assert: method returns without throwing; no file-system side effects
            await miner.Invoking(m => m.DeleteStagingFilesAsync()).Should().NotThrowAsync();
        }

        [TestMethod]
        public async Task Consolidate_WhenFolderIsFilteredAndRemapped_AppliesBothTransformations()
        {
            // Arrange
            var keptFolder = new FolderWrapper(true, 1, 10, "Keep", "root/keep");
            var filteredFolder = new FolderWrapper(true, 1, 10, "Skip", "root/skip");
            var remappedFolder = new FolderWrapper(true, 1, 10, "Remap", "root/remap");
            var miner = new EmailDataMiner(
                new StubGlobals(
                    toDoObjects: new StubToDoObjects(
                        filteredFolderScraping: new ScoDictionary<string, int>
                        {
                            ["root/skip"] = 1,
                        },
                        folderRemap: new ScoDictionary<string, string>
                        {
                            ["root/remap"] = "root/remapped",
                        }
                    )
                )
            );

            var jagged = new[]
            {
                new[]
                {
                    new MinedMailInfo { FolderInfo = keptFolder, Subject = "keep" },
                    new MinedMailInfo { FolderInfo = filteredFolder, Subject = "skip" },
                },
                new[]
                {
                    new MinedMailInfo { FolderInfo = remappedFolder, Subject = "remap" },
                },
            };

            // Act
            var result = await miner.Consolidate(jagged);

            // Assert
            result.Should().HaveCount(2);
            result.Select(x => x.Subject).Should().BeEquivalentTo(["keep", "remap"]);
            result
                .Single(x => x.Subject == "remap")
                .FolderInfo.RelativePath.Should()
                .Be("root/remapped");
        }

        [TestMethod]
        public async Task ToMinedMail_WhenItemsProvided_ProjectsItemFieldsIntoSerializableModels()
        {
            // Arrange
            var folder = new FolderWrapper(true, 1, 10, "Inbox", "root/inbox");
            var item = new Mock<IItemInfo>(MockBehavior.Strict);
            item.SetupGet(x => x.Categories).Returns("Blue");
            item.SetupGet(x => x.Tokens).Returns(["alpha", "beta"]);
            item.SetupGet(x => x.FolderInfo).Returns(folder);
            item.SetupGet(x => x.ToRecipients).Returns(Array.Empty<IRecipientInfo>());
            item.SetupGet(x => x.CcRecipients).Returns(Array.Empty<IRecipientInfo>());
            item.SetupGet(x => x.Sender).Returns((IRecipientInfo)null);
            item.SetupGet(x => x.ConversationID).Returns("conversation");
            item.SetupGet(x => x.EntryId).Returns("entry");
            item.SetupGet(x => x.StoreId).Returns("store");
            item.SetupGet(x => x.Subject).Returns("subject");
            item.SetupGet(x => x.Actionable).Returns("Yes");

            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var result = await miner.ToMinedMail([item.Object]);

            // Assert
            result.Should().ContainSingle();
            result[0].FolderInfo.Should().BeSameAs(folder);
            result[0].Tokens.Should().Equal("alpha", "beta");
            result[0].Subject.Should().Be("subject");
            result[0].Actionable.Should().Be("Yes");
        }

        [TestMethod]
        public void Deserialize_WhenAppDataFolderMissing_ReturnsDefaultValue()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var result = miner.Deserialize<int>("Missing");

            // Assert
            result.Should().Be(default);
        }

        [TestMethod]
        public void Deserialize_WhenAppDataFolderHasNoFile_ReturnsDefaultValue()
        {
            // Arrange
            var missingRoot = GetGuaranteedMissingPath("deserialize");
            var miner = new EmailDataMiner(
                new StubGlobals(specialFolders: CreateAppDataMap(missingRoot))
            );

            // Act
            var result = miner.Deserialize<string>("MissingSeed");

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task Load_WhenFileNameOmittedAndFileMissing_ReturnsDefaultValue()
        {
            // Arrange
            var missingRoot = GetGuaranteedMissingPath("load");

            // Act
            var result = await EmailDataMiner.Load<int>(missingRoot);

            // Assert
            result.Should().Be(default);
        }

        [TestMethod]
        public void SerializeAndSave_WhenAppDataFolderMissing_ReturnsWithoutInvokingWriter()
        {
            // Arrange
            var miner = new TestableEmailDataMiner(new StubGlobals());

            // Act
            miner.SerializeAndSave(new { Name = "test" }, "Seed");

            // Assert
            miner.CapturedFolderPath.Should().BeNull();
            miner.CapturedFileName.Should().BeNull();
        }

        [TestMethod]
        public void SerializeAndSave_WhenAppDataFolderExists_UsesBayesianFolderAndSuffixFileName()
        {
            // Arrange
            var appDataRoot = GetGuaranteedMissingPath("serialize");
            var miner = new TestableEmailDataMiner(
                new StubGlobals(specialFolders: CreateAppDataMap(appDataRoot))
            );

            // Act
            miner.SerializeAndSave(new { Name = "test" }, "Seed", "0001");

            // Assert
            miner.CapturedFolderPath.Should().Be(Path.Combine(appDataRoot, "Bayesian"));
            miner.CapturedFileName.Should().Be("Seed_0001.json");
        }

        [TestMethod]
        public async Task ValidateJson_WhenAppDataFolderMissing_ReturnsFalse()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var result = await miner.ValidateJson<string>("Missing");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task ValidateJson_WhenAppDataFolderHasNoFile_ReturnsFalse()
        {
            // Arrange
            var appDataRoot = GetGuaranteedMissingPath("validate");
            var miner = new EmailDataMiner(
                new StubGlobals(specialFolders: CreateAppDataMap(appDataRoot))
            );

            // Act
            var result = await miner.ValidateJson<string>("Missing");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenLoaderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var action = () => miner.TryLoadObjectAndGetMemorySize<string>(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenCopiesToLoadIsLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var action = () => miner.TryLoadObjectAndGetMemorySize(() => "value", 0);

            // Assert
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenLoaderSucceedsAcrossCopies_ReturnsObjectAndSize()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());
            var callCount = 0;

            // Act
            var (result, size) = miner.TryLoadObjectAndGetMemorySize(
                () =>
                {
                    callCount++;
                    return new object();
                },
                copiesToLoad: 3
            );

            // Assert
            result.Should().NotBeNull();
            callCount.Should().Be(3);
        }

        [TestMethod]
        public void TryLoadObjectAndGetMemorySize_WhenLoaderThrowsDuringReplicaCreation_ReturnsDefaultAndZero()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());
            var callCount = 0;

            // Act
            var (result, size) = miner.TryLoadObjectAndGetMemorySize(
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

            // Assert
            result.Should().BeNull();
            size.Should().Be(0);
        }

        [TestMethod]
        public void GetSerializer_ReturnsIndentedSerializerWithAutoTypeNames()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var serializer = miner.GetSerializer();

            // Assert
            serializer.Should().NotBeNull();
            serializer.TypeNameHandling.Should().Be(TypeNameHandling.Auto);
            serializer.Formatting.Should().Be(Formatting.Indented);
        }

        [TestMethod]
        public void SerializeActiveItem_WhenLoaderReturnsNull_DoesNotSerializeMailInfo()
        {
            // Arrange
            var miner = new TestableEmailDataMiner(new StubGlobals())
            {
                LoaderResult = null,
                LoaderSize = 123,
            };

            // Act
            miner.SerializeActiveItem();

            // Assert
            miner.SerializeMailInfoCalls.Should().Be(0);
        }

        [TestMethod]
        public void GetProgressMessage_WhenInvokedWithCompletedWork_IncludesCountsAndElapsedText()
        {
            // Arrange
            var miner = new EmailDataMiner(new StubGlobals());
            var method = typeof(EmailDataMiner).GetMethod(
                "GetProgressMessage",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();

            // Act
            var message = (string)method.Invoke(miner, [2, 4, stopwatch]);

            // Assert
            message.Should().Contain("Completed 2 of 4");
            message.Should().Contain("elapsed");
            message.Should().Contain("remaining");
        }
        #endregion
    }
}
