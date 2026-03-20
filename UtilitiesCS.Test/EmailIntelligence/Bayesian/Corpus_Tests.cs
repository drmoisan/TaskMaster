using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    [TestClass]
    public class Corpus_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesEmptyCorpus()
        {
            // Act
            var corpus = new Corpus();

            // Assert
            corpus.TokenFrequency.Should().BeEmpty();
            corpus.TokenCount.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithTokens_GroupsAndCounts()
        {
            // Arrange
            var tokens = new[] { "a", "b", "a", "c", "b", "a" };

            // Act
            var corpus = new Corpus(tokens);

            // Assert
            corpus.TokenFrequency["a"].Should().Be(3);
            corpus.TokenFrequency["b"].Should().Be(2);
            corpus.TokenFrequency["c"].Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithKeyValuePairs_StoresAllEntries()
        {
            // Arrange
            var pairs = new Dictionary<string, int> { ["x"] = 5, ["y"] = 3 };

            // Act
            var corpus = new Corpus(pairs);

            // Assert
            corpus.TokenFrequency["x"].Should().Be(5);
            corpus.TokenFrequency["y"].Should().Be(3);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesCustomComparer()
        {
            // Arrange & Act
            var corpus = new Corpus(StringComparer.OrdinalIgnoreCase);
            corpus.AddOrIncrementToken("Hello");

            // Assert
            corpus.TokenFrequency.ContainsKey("hello").Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_WithPairsAndComparer_Works()
        {
            // Arrange
            var pairs = new Dictionary<string, int> { ["Key"] = 1 };

            // Act
            var corpus = new Corpus(pairs, StringComparer.OrdinalIgnoreCase);

            // Assert
            corpus.TokenFrequency.ContainsKey("key").Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_ConcurrencyAndCapacity_CreatesEmpty()
        {
            // Act
            var corpus = new Corpus(4, 100);

            // Assert
            corpus.TokenFrequency.Should().BeEmpty();
        }

        [TestMethod]
        public void AddOrIncrementToken_NewToken_AddsWithCount1()
        {
            // Arrange
            var corpus = new Corpus();

            // Act
            corpus.AddOrIncrementToken("test");

            // Assert
            corpus.TokenFrequency["test"].Should().Be(1);
        }

        [TestMethod]
        public void AddOrIncrementToken_ExistingToken_IncrementsCount()
        {
            // Arrange
            var corpus = new Corpus();
            corpus.AddOrIncrementToken("test");

            // Act
            corpus.AddOrIncrementToken("test");

            // Assert
            corpus.TokenFrequency["test"].Should().Be(2);
        }

        [TestMethod]
        public void AddOrIncrementTokens_MultipleSameTokens_SumsCorrectly()
        {
            // Arrange
            var corpus = new Corpus();

            // Act
            corpus.AddOrIncrementTokens(new[] { "a", "b", "a" });

            // Assert
            corpus.TokenFrequency["a"].Should().Be(2);
            corpus.TokenFrequency["b"].Should().Be(1);
        }

        [TestMethod]
        public void DecrementOrRemoveToken_CountAboveZero_Decrements()
        {
            // Arrange
            var corpus = new Corpus(new Dictionary<string, int> { ["token"] = 3 });

            // Act
            var result = corpus.DecrementOrRemoveToken("token");

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void DecrementOrRemoveToken_CountReachesZero_RemovesToken()
        {
            // Arrange
            var corpus = new Corpus(new Dictionary<string, int> { ["token"] = 1 });

            // Act
            var result = corpus.DecrementOrRemoveToken("token");

            // Assert
            result.Should().BeFalse();
            corpus.TokenFrequency.Should().NotContainKey("token");
        }

        [TestMethod]
        public void DecrementOrRemoveToken_MissingToken_ReturnsFalse()
        {
            // Arrange
            var corpus = new Corpus();

            // Act
            var result = corpus.DecrementOrRemoveToken("missing");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AddTokenOrSumValues_NewToken_AddsValue()
        {
            // Arrange
            var corpus = new Corpus();

            // Act
            corpus.AddOrSumTokenValue("token", 5);

            // Assert
            corpus.TokenFrequency["token"].Should().Be(5);
        }

        [TestMethod]
        public void AddTokenOrSumValues_ExistingToken_SumsValues()
        {
            // Arrange
            var corpus = new Corpus(new Dictionary<string, int> { ["token"] = 3 });

            // Act
            corpus.AddOrSumTokenValue("token", 5);

            // Assert
            corpus.TokenFrequency["token"].Should().Be(8);
        }

        [TestMethod]
        public void AddTokenOrSumValues_Collection_AddsAll()
        {
            // Arrange
            var corpus = new Corpus();
            var tokens = new Dictionary<string, int> { ["a"] = 2, ["b"] = 3 };

            // Act
            corpus.AddTokenOrSumValues(tokens);

            // Assert
            corpus.TokenFrequency["a"].Should().Be(2);
            corpus.TokenFrequency["b"].Should().Be(3);
        }

        [TestMethod]
        public void SubtractOrRemoveValue_ReducesCount()
        {
            // Arrange
            var corpus = new Corpus(new Dictionary<string, int> { ["token"] = 5 });

            // Act
            corpus.SubtractOrRemoveValue("token", 3);

            // Assert
            corpus.TokenFrequency["token"].Should().Be(2);
        }

        [TestMethod]
        public void SubtractOrRemoveValue_CountGoesBelowZero_RemovesToken()
        {
            // Arrange
            var corpus = new Corpus(new Dictionary<string, int> { ["token"] = 2 });

            // Act
            corpus.SubtractOrRemoveValue("token", 5);

            // Assert
            corpus.TokenFrequency.Should().NotContainKey("token");
        }

        [TestMethod]
        public void SubtractOrRemoveValues_Collection_SubtractsAll()
        {
            // Arrange
            var corpus = new Corpus(new Dictionary<string, int> { ["a"] = 5, ["b"] = 2 });
            var subtract = new Dictionary<string, int> { ["a"] = 2, ["b"] = 5 };

            // Act
            corpus.SubtractOrRemoveValues(subtract);

            // Assert
            corpus.TokenFrequency["a"].Should().Be(3);
            corpus.TokenFrequency.Should().NotContainKey("b");
        }

        [TestMethod]
        public void AddTokenCount_IncrementsCount()
        {
            // Arrange
            var corpus = new Corpus();

            // Act
            var result = corpus.AddTokenCount(10);

            // Assert
            result.Should().Be(10);
            corpus.TokenCount.Should().Be(10);
        }

        [TestMethod]
        public void Indicator_SetAndGet_Works()
        {
            // Arrange
            var corpus = new Corpus();

            // Act
            corpus.Indicator = UtilitiesCS.Enums.Corpus.Positive;

            // Assert
            corpus.Indicator.Should().Be(UtilitiesCS.Enums.Corpus.Positive);
        }

        [TestMethod]
        public void Clone_ReturnsIndependentCopy()
        {
            // Arrange
            var original = new Corpus(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });

            // Act
            var clone = (Corpus)original.Clone();
            clone.AddOrIncrementToken("c");

            // Assert
            clone.TokenFrequency.Should().ContainKey("c");
            original.TokenFrequency.Should().NotContainKey("c");
        }

        [TestMethod]
        public void OperatorPlus_MergesCorpora()
        {
            // Arrange
            var c1 = new Corpus(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });
            var c2 = new Corpus(new Dictionary<string, int> { ["b"] = 3, ["c"] = 4 });

            // Act
            var result = c1 + c2;

            // Assert
            result.TokenFrequency["a"].Should().Be(1);
            result.TokenFrequency["b"].Should().Be(5);
            result.TokenFrequency["c"].Should().Be(4);
        }

        [TestMethod]
        public void OperatorMinus_SubtractsCorpora()
        {
            // Arrange
            var c1 = new Corpus(new Dictionary<string, int> { ["a"] = 5, ["b"] = 3, ["c"] = 1 });
            var c2 = new Corpus(new Dictionary<string, int> { ["a"] = 2, ["c"] = 5 });

            // Act
            var result = c1 - c2;

            // Assert
            result.TokenFrequency["a"].Should().Be(3);
            result.TokenFrequency["b"].Should().Be(3);
            result.TokenFrequency.Should().NotContainKey("c");
        }

        [TestMethod]
        public async Task SubtractAsync_SubtractsCorpora()
        {
            // Arrange
            var c1 = new Corpus(new Dictionary<string, int> { ["a"] = 5, ["b"] = 3 });
            var c2 = new Corpus(new Dictionary<string, int> { ["a"] = 2 });

            // Act
            var result = await Corpus.SubtractAsync(c1, c2, CancellationToken.None);

            // Assert
            result.TokenFrequency["a"].Should().Be(3);
            result.TokenFrequency["b"].Should().Be(3);
        }

        [TestMethod]
        public void SubtractFilter_FiltersAndSubtracts()
        {
            // Arrange
            var all = new Corpus(new Dictionary<string, int> { ["a"] = 10, ["b"] = 5, ["c"] = 1 });
            var match = new Corpus(new Dictionary<string, int> { ["a"] = 3, ["c"] = 1 });

            // Act
            var (notMatch, matchFiltered) = Corpus.SubtractFilter(all, match, 1, 2);

            // Assert
            notMatch.TokenFrequency.Should().ContainKey("a");
            notMatch.TokenFrequency.Should().ContainKey("b");
        }
    }
}
