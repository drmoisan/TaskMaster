using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Additional in-memory coverage for FolderScorer candidate and score ordering paths.
    /// </summary>
    [TestClass]
    public class FolderScorerCoverageExpansionTests
    {
        [TestMethod]
        public void AddSuggestion_WhenFolderIsRepeated_AggregatesScoreAndRanksFirst()
        {
            // Arrange
            var scorer = new FolderScorer();

            // Act
            scorer.AddSuggestion("Archive\\Finance", 30);
            scorer.AddSuggestion("Inbox\\Projects", 15);
            scorer.AddSuggestion("Archive\\Finance", 25);

            // Assert
            scorer.Count.Should().Be(2);
            scorer.TopScore().Should().Be(55);
            scorer.ToArray().Should().Equal("Archive\\Finance", "Inbox\\Projects");
        }

        [TestMethod]
        public void ToArray_WhenScoresTie_ReturnsAllTiedCandidatesBeforeLowerScores()
        {
            // Arrange
            var scorer = new FolderScorer();
            scorer.AddSuggestion("Archive\\Alpha", 40);
            scorer.AddSuggestion("Archive\\Beta", 40);
            scorer.AddSuggestion("Archive\\Gamma", 10);

            // Act
            var result = scorer.ToArray(2);

            // Assert
            result.Should().BeEquivalentTo(new[] { "Archive\\Alpha", "Archive\\Beta" });
            result.Should().NotContain("Archive\\Gamma");
        }

        [TestMethod]
        public void EmptyCandidates_WhenRead_ReturnEmptyCollectionsAndZeroTopScore()
        {
            // Arrange
            var scorer = new FolderScorer();

            // Act
            var allCandidates = scorer.ToArray();
            var topCandidates = scorer.ToArray(3);

            // Assert
            scorer.Count.Should().Be(0);
            scorer.TopScore().Should().Be(0);
            allCandidates.Should().BeEmpty();
            topCandidates.Should().BeEmpty();
        }

        [TestMethod]
        public void NullCandidates_WhenAdded_ReturnFalseAndLeaveScoresEmpty()
        {
            // Arrange
            var scorer = new FolderScorer();

            // Act
            var nullSuggestionResult = scorer.AddSuggestion((object)null, 5);
            var nullArrayResult = scorer.AddArray((object)null, topN: -1);

            // Assert
            nullSuggestionResult.Should().BeFalse();
            nullArrayResult.Should().BeFalse();
            scorer.Count.Should().Be(0);
        }

        [TestMethod]
        public void BoundaryScores_WhenAdded_ReturnHighestBoundaryScore()
        {
            // Arrange
            var scorer = new FolderScorer();

            // Act
            scorer.AddSuggestion("Archive\\Low", long.MinValue);
            scorer.AddSuggestion("Archive\\Zero", 0);
            scorer.AddSuggestion("Archive\\High", long.MaxValue);

            // Assert
            scorer.TopScore().Should().Be(long.MaxValue);
            scorer.ToArray(1).Should().Equal("Archive\\High");
        }

        [TestMethod]
        public void EmptyCandidateArray_WhenAdded_ThrowsAndDoesNotAddScores()
        {
            // Arrange
            var scorer = new FolderScorer();

            // Act
            Action act = () => scorer.AddArray(Array.Empty<string>(), topN: -1);

            // Assert
            act.Should().Throw<IndexOutOfRangeException>();
            scorer.Count.Should().Be(0);
        }
    }
}
