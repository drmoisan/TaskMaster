using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
