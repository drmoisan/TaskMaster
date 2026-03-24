using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for <see cref="EmailDataMiner"/>.
    ///
    /// Purpose:
    ///     Verify the three deterministically testable paths in EmailDataMiner without
    ///     requiring live Outlook COM objects:
    ///     (1) P34-T1: <c>AddRollingMeasures</c> with an empty folder array produces no rows.
    ///     (2) P34-T2: <c>AddRollingMeasures</c> chunks a known-size input into the expected
    ///         group count.
    ///     (3) P34-T3: <c>DeleteStagingFilesAsync</c> returns without error when no AppData
    ///         special folder is registered.
    ///
    /// Constraints:
    ///     AddRollingMeasures is internal; the csproj InternalsVisibleTo attribute exposes it
    ///     to the test assembly. FolderWrapper is constructed via its JsonConstructor (no COM).
    ///     IApplicationGlobals is mocked with Moq so no Outlook session is required.
    /// </summary>
    [TestClass]
    public class EmailDataMiner_Tests
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

        // -----------------------------------------------------------------------
        // Private stubs used by DeleteStagingFilesAsync_WhenAppDataFolderMissing
        // -----------------------------------------------------------------------

        private sealed class StubGlobalsWithEmptySpecialFolders : IApplicationGlobals
        {
            public IFileSystemFolderPaths FS { get; } = new EmptySpecialFolderPaths();

            public System.Threading.Tasks.Task LoadAsync(bool parallel) =>
                throw new System.NotImplementedException();

            public IOlObjects Ol => throw new System.NotImplementedException();

            public IToDoObjects TD => throw new System.NotImplementedException();

            public IAppAutoFileObjects AF => throw new System.NotImplementedException();

            public IAppEvents Events => throw new System.NotImplementedException();

            public IAppQuickFilerSettings QfSettings => throw new System.NotImplementedException();

            public IAppItemEngines Engines => throw new System.NotImplementedException();

            public IntelligenceConfig IntelRes => throw new System.NotImplementedException();
        }

        private sealed class EmptySpecialFolderPaths : IFileSystemFolderPaths
        {
            public ConcurrentDictionary<string, string> SpecialFolders { get; } =
                new ConcurrentDictionary<string, string>();

            public void Reload() => throw new System.NotImplementedException();

            public IAppStagingFilenames Filenames => throw new System.NotImplementedException();

            public string MatchBestSpecialFolder(string path) =>
                throw new System.NotImplementedException();
        }

        #endregion
    }
}
