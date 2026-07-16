using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for the <see cref="UtilitiesCS.FolderScore"/> value type and the
    /// <see cref="UtilitiesCS.FolderScorer"/> scored projection (<c>ToScoredArray</c>). The scored
    /// projection is exercised only through the <c>AddSuggestion(string, long)</c> seam, which every
    /// score source (Bayesian, conversation, word-sequence) funnels through; the COM/model-bound
    /// <c>AddBayesianSuggestionsAsync</c> path is never invoked. Bayesian scale is represented by
    /// <c>probability * 1000</c> values (the exact mapping <c>AddBayesianSuggestionsAsync</c> uses:
    /// <c>score = round(prediction.Probability * 1000)</c>).
    /// </summary>
    [TestClass]
    public class FolderScoreTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod]
        public void Constructor_StoresFolderPathScoreAndProbabilityVerbatim()
        {
            // Arrange / Act
            var score = new UtilitiesCS.FolderScore("Archive\\Finance", 850, 0.85);

            // Assert
            score.FolderPath.Should().Be("Archive\\Finance");
            score.Score.Should().Be(850);
            score.Probability.Should().BeApproximately(0.85, Tolerance);
        }

        [TestMethod]
        public void ToScoredArray_BayesianScale_MapsProbabilityTimesThousandToNormalizedValue()
        {
            // Arrange: Bayesian predictions of 1.0 and 0.8 are stored as 1000 and 800
            // (probability * 1000), the exact scale AddBayesianSuggestionsAsync produces.
            var scorer = new UtilitiesCS.FolderScorer();
            scorer.AddSuggestion("Bayes\\Certain", 1000);
            scorer.AddSuggestion("Bayes\\Likely", 800);

            // Act
            var scored = scorer.ToScoredArray();

            // Assert: max-normalized against the top score (1000), so 1000 -> 1.0 and 800 -> 0.8.
            scored.Should().HaveCount(2);
            scored[0].FolderPath.Should().Be("Bayes\\Certain");
            scored[0].Score.Should().Be(1000);
            scored[0].Probability.Should().BeApproximately(1.0, Tolerance);
            scored[1].FolderPath.Should().Be("Bayes\\Likely");
            scored[1].Score.Should().Be(800);
            scored[1].Probability.Should().BeApproximately(0.8, Tolerance);
        }

        [TestMethod]
        public void ToScoredArray_AcrossThreeSourceScales_ProjectsAllRowsWithinZeroToOne()
        {
            // Arrange: one folder per source scale, all funneled through AddSuggestion:
            //   - conversation-scale weighted integer (large),
            //   - Bayesian scale (probability * 1000),
            //   - word-sequence-scale integer (small).
            var scorer = new UtilitiesCS.FolderScorer();
            scorer.AddSuggestion("Conversation\\Thread", 4200);
            scorer.AddSuggestion("Bayes\\Likely", 800);
            scorer.AddSuggestion("WordSeq\\Match", 37);

            // Act
            var scored = scorer.ToScoredArray();

            // Assert: ranking order by score, every Probability within [0,1], top folder = 1.0.
            scored
                .Select(x => x.FolderPath)
                .Should()
                .Equal("Conversation\\Thread", "Bayes\\Likely", "WordSeq\\Match");
            scored.Should().OnlyContain(x => x.Probability >= 0.0 && x.Probability <= 1.0);
            scored[0].Probability.Should().BeApproximately(1.0, Tolerance);
            scored[1].Probability.Should().BeApproximately(800.0 / 4200.0, Tolerance);
            scored[2].Probability.Should().BeApproximately(37.0 / 4200.0, Tolerance);
        }

        [TestMethod]
        public void ToScoredArray_MixedSourceAccumulation_SumsScorePerFolderAndKeepsProbabilityBounded()
        {
            // Arrange: the same folder receives a Bayesian, a conversation, and a word-sequence
            // contribution; AddSuggestion accumulates them. A second smaller folder confirms the
            // accumulated folder normalizes to <= 1 even though its raw score exceeds 1000.
            var scorer = new UtilitiesCS.FolderScorer();
            scorer.AddSuggestion("Archive\\Shared", 800); // Bayesian scale
            scorer.AddSuggestion("Archive\\Shared", 4200); // conversation scale
            scorer.AddSuggestion("Archive\\Shared", 37); // word-sequence scale
            scorer.AddSuggestion("Archive\\Other", 100);

            // Act
            var scored = scorer.ToScoredArray();

            // Assert
            var shared = scored.Single(x => x.FolderPath == "Archive\\Shared");
            shared.Score.Should().Be(5037);
            shared.Probability.Should().BeApproximately(1.0, Tolerance);
            scored.Should().OnlyContain(x => x.Probability >= 0.0 && x.Probability <= 1.0);
            scored
                .Single(x => x.FolderPath == "Archive\\Other")
                .Probability.Should()
                .BeApproximately(100.0 / 5037.0, Tolerance);
        }

        [TestMethod]
        public void ToScoredArray_EmptyScorer_ReturnsEmptyArrayWithoutDivideByZero()
        {
            // Arrange
            var scorer = new UtilitiesCS.FolderScorer();

            // Act
            var scored = scorer.ToScoredArray();
            var scoredTopN = scorer.ToScoredArray(5);

            // Assert: no exception, empty result (zero-guard path with no division).
            scored.Should().BeEmpty();
            scoredTopN.Should().BeEmpty();
        }

        [TestMethod]
        public void ToScoredArray_AllZeroSeeds_YieldsZeroProbabilityForEveryRow()
        {
            // Arrange: seed folders added via AddArray carry score 0, so the top score is 0.
            var scorer = new UtilitiesCS.FolderScorer();
            scorer.AddArray(new[] { "Seed\\A", "Seed\\B", "Seed\\C" }, -1);

            // Act
            var scored = scorer.ToScoredArray();

            // Assert: zero-guard makes every Probability 0 (no confidence signal, no divide-by-zero).
            scored.Should().HaveCount(3);
            scored.Should().OnlyContain(x => x.Score == 0 && x.Probability == 0.0);
        }

        [TestMethod]
        public void ToScoredArrayTopN_WhenTopNExceedsCount_ReturnsAllRows()
        {
            // Arrange
            var scorer = new UtilitiesCS.FolderScorer();
            scorer.AddSuggestion("Archive\\A", 500);
            scorer.AddSuggestion("Archive\\B", 250);

            // Act
            var scored = scorer.ToScoredArray(10);

            // Assert
            scored.Should().HaveCount(2);
            scored.Select(x => x.FolderPath).Should().Equal("Archive\\A", "Archive\\B");
        }
    }
}
