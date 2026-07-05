using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Additional FolderPredictor coverage for in-memory ranking and input handling paths.
    /// </summary>
    [TestClass]
    public class FolderPredictorCoverageExpansionTests
    {
        [TestMethod]
        public void AddSuggestions_WhenScoresDiffer_AppendsRankedSuggestions()
        {
            // Arrange
            var predictor = CreatePredictorWithScorer();
            predictor.Suggestions.AddSuggestion("Archive\\Low", 5);
            predictor.Suggestions.AddSuggestion("Archive\\High", 50);
            predictor.Suggestions.AddSuggestion("Archive\\Middle", 25);
            var folderList = new List<string>();

            // Act
            predictor.AddSuggestions(ref folderList);

            // Assert
            folderList
                .Should()
                .Equal(
                    "========= SUGGESTIONS =========",
                    "Archive\\High",
                    "Archive\\Middle",
                    "Archive\\Low"
                );
        }

        [TestMethod]
        public void AddSuggestions_WhenHistoryIsEmpty_AppendsOnlySuggestionHeader()
        {
            // Arrange
            var predictor = CreatePredictorWithScorer();
            var folderList = new List<string>();

            // Act
            predictor.AddSuggestions(ref folderList);

            // Assert
            folderList.Should().Equal("========= SUGGESTIONS =========");
        }

        [TestMethod]
        public void FromArrayOrString_WhenInputIsMalformed_ThrowsArgumentException()
        {
            // Arrange
            var predictor = CreatePredictorWithScorer();

            // Act
            Action unsupportedInput = () => predictor.FromArrayOrString(123);
            Action nullInput = () => predictor.FromArrayOrString(null);

            // Assert
            unsupportedInput.Should().Throw<ArgumentException>();
            nullInput.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FromArrayOrString_WhenArrayContainsDuplicates_PreservesCandidateOrder()
        {
            // Arrange
            var predictor = CreatePredictorWithScorer();

            // Act
            predictor.FromArrayOrString(
                new[] { "Archive\\Alpha", "Archive\\Alpha", "Archive\\Beta" }
            );

            // Assert
            predictor
                .FolderArray.Should()
                .Equal("Archive\\Alpha", "Archive\\Alpha", "Archive\\Beta");
        }

        [TestMethod]
        public async Task InitAsync_WhenNoSuggestionsOptionIsUsed_DoesNotReadInput()
        {
            // Arrange
            var predictor = CreatePredictorWithScorer();

            // Act
            var result = await predictor.InitAsync(null, FolderPredictor.InitOptions.NoSuggestions);

            // Assert
            result.Should().BeSameAs(predictor);
            predictor.Suggestions.Count.Should().Be(0);
        }

        [TestMethod]
        public void NormalizePredictionPath_WhenInputIsNull_ReturnsEmptyFallback()
        {
            // Act
            var result = FolderPredictor.NormalizePredictionPath(null);

            // Assert
            result.Should().BeEmpty();
        }

        private static FolderPredictor CreatePredictorWithScorer()
        {
            return new FolderPredictor(new Mock<Outlook.Application>().Object)
            {
                Suggestions = new FolderScorer(),
            };
        }
    }
}
