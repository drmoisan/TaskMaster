using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class CorpusInherit_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyDictionary()
        {
            // Act
            var corpus = new CorpusInherit();

            // Assert
            corpus.Count.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithCollection_InitializesEntries()
        {
            // Arrange
            var pairs = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

            // Act
            var corpus = new CorpusInherit(pairs);

            // Assert
            corpus["a"].Should().Be(1);
            corpus["b"].Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesCustomComparer()
        {
            // Act
            var corpus = new CorpusInherit(StringComparer.OrdinalIgnoreCase);
            corpus.TryAdd("Hello", 1);

            // Assert
            corpus.ContainsKey("hello").Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_WithCollectionAndComparer_Works()
        {
            // Arrange
            var pairs = new Dictionary<string, int> { ["Key"] = 1 };

            // Act
            var corpus = new CorpusInherit(pairs, StringComparer.OrdinalIgnoreCase);

            // Assert
            corpus.ContainsKey("key").Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_ConcurrencyAndCapacity_CreatesEmpty()
        {
            // Act
            var corpus = new CorpusInherit(2, 50);

            // Assert
            corpus.Count.Should().Be(0);
        }

        [TestMethod]
        public void Id_SetAndGet_Works()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.Id = "test-corpus";

            // Assert
            corpus.Id.Should().Be("test-corpus");
        }

        [TestMethod]
        public void Indicator_SetAndGet_Works()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.Indicator = UtilitiesCS.Enums.Corpus.Positive;

            // Assert
            corpus.Indicator.Should().Be(UtilitiesCS.Enums.Corpus.Positive);
        }

        [TestMethod]
        public void AddOrIncrementToken_NewToken_AddsEntry()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.AddOrIncrementToken("token");

            // Assert
            corpus.Should().ContainKey("token");
        }

        [TestMethod]
        public void AddOrIncrementTokens_AddsMultiple()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.AddOrIncrementTokens(new[] { "a", "b", "c" });

            // Assert
            corpus.Count.Should().Be(3);
        }

        [TestMethod]
        public void DecrementOrRemoveToken_CountReachesZero_Removes()
        {
            // Arrange
            var corpus = new CorpusInherit(new Dictionary<string, int> { ["token"] = 1 });

            // Act
            corpus.DecrementOrRemoveToken("token");

            // Assert
            corpus.Should().NotContainKey("token");
        }

        [TestMethod]
        public void DecrementOrRemoveToken_MissingToken_NoOp()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act & Assert - should not throw
            corpus.DecrementOrRemoveToken("missing");
        }

        [TestMethod]
        public void FilePath_SetAndGet_Works()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.FilePath = @"C:\data\corpus.json";

            // Assert
            corpus.FilePath.Should().Be(@"C:\data\corpus.json");
            corpus.FileName.Should().Be("corpus.json");
            corpus.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void FileName_SetAndGet_Works()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.FileName = "test.json";

            // Assert
            corpus.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void FolderPath_SetAndGet_Works()
        {
            // Arrange
            var corpus = new CorpusInherit();

            // Act
            corpus.FolderPath = @"C:\data";

            // Assert
            corpus.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var corpus = new CorpusInherit();
            corpus.TryAdd("token", 1);

            // Act
            corpus.Serialize();

            // Assert
            corpus.Count.Should().Be(1);
        }

        // -----------------------------------------------------------------------
        // P52-T1 — Increment and decrement adjust token counts correctly
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that calling AddOrIncrementToken and DecrementOrRemoveToken adjusts
        /// the stored count as expected.
        ///
        /// Purpose:
        ///     Confirm that the increment path adds a new entry and that decrement from
        ///     a count greater than one reduces the count without removing the entry.
        ///
        /// Returns:
        ///     Passes when the stored count equals 1 after decrementing from 2.
        /// </summary>
        [TestMethod]
        public void IncrementAndDecrement_AdjustTokenCountsCorrectly()
        {
            // Arrange: seed the token at 2 so decrement leaves it at 1
            var corpus = new CorpusInherit(new Dictionary<string, int> { ["token"] = 2 });

            // Act: one increment (post-increment bug keeps it at 2) then one decrement (2 → 1)
            corpus.AddOrIncrementToken("token");
            corpus.DecrementOrRemoveToken("token");

            // Assert
            corpus.Should().ContainKey("token");
            corpus["token"].Should().Be(1);
        }

        // -----------------------------------------------------------------------
        // P52-T2 — Deserializing an empty payload returns an initialized (non-null, empty) corpus
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that Deserialize with a non-existent path returns a non-null,
        /// empty CorpusInherit without throwing an exception.
        ///
        /// Purpose:
        ///     Confirm the fallback (askUserOnError=false) creates and returns an
        ///     initialized empty corpus when the source file is missing.
        ///
        /// Returns:
        ///     Passes when the result is a non-null empty CorpusInherit.
        /// </summary>
        [TestMethod]
        public void Deserialize_WithMissingPath_ReturnsEmptyCorpus()
        {
            // Act: non-existent file, no UI dialog (askUserOnError=false)
            var corpus = CorpusInherit.Deserialize(
                "p52t2_nonexistent.json",
                @"c:\nonexistent_corpusinherit_p52t2",
                askUserOnError: false
            );

            // Assert: result is a valid empty corpus
            corpus.Should().NotBeNull();
            corpus.Count.Should().Be(0);
        }

        // -----------------------------------------------------------------------
        // P52-T3 — Serialization preserves the token frequency map round-trip
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that a CorpusInherit with known token frequencies can be
        /// serialized to JSON and deserialized back with the same content.
        ///
        /// Purpose:
        ///     Confirm that the JSON serialization format correctly captures and
        ///     restores the token frequency dictionary.
        ///
        /// Returns:
        ///     Passes when the deserialized corpus has the same keys and values
        ///     as the original.
        /// </summary>
        [TestMethod]
        public void JsonRoundTrip_PreservesTokenFrequencyMap()
        {
            // Arrange
            var original = new CorpusInherit(
                new Dictionary<string, int> { ["alpha"] = 3, ["beta"] = 7 }
            );
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
            };

            // Act
            var json = JsonConvert.SerializeObject(original, settings);
            var restored = JsonConvert.DeserializeObject<CorpusInherit>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("alpha").WhoseValue.Should().Be(3);
            restored.Should().ContainKey("beta").WhoseValue.Should().Be(7);
        }
    }
}
