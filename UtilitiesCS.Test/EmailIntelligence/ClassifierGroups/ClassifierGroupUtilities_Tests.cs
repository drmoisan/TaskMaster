using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class ClassifierGroupUtilities_Tests
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

        /// <summary>
        /// Testable subclass that overrides the virtual Deserialize method to return a
        /// predetermined value, keeping all file-system I/O out of unit tests.
        /// </summary>
        private sealed class StubClassifierGroupUtilities(
            IApplicationGlobals globals,
            BayesianClassifierGroup stubbedResult
        ) : ClassifierGroupUtilities(globals)
        {
            internal override T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
            {
                if (stubbedResult is T result)
                    return result;
                return default;
            }
        }
    }
}
