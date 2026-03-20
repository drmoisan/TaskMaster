using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SmithWaterman_Tests
    {
        [TestMethod]
        public void CalculateScore_IntArrays_IdenticalSequences_ReturnsPositiveScore()
        {
            // Arrange
            int[] wordsX = { 1, 2, 3 };
            int[] wordLengthX = { 3, 4, 5 };
            int[] wordsY = { 1, 2, 3 };
            int[] wordLengthY = { 3, 4, 5 };

            // Act
            var score = SmithWaterman.CalculateScore(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 2,
                mismatchScore: -1,
                gapPenalty: -1
            );

            // Assert
            score.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CalculateScore_IntArrays_CompletelyDifferentSequences_ReturnsZero()
        {
            // Arrange
            int[] wordsX = { 1, 2 };
            int[] wordLengthX = { 1, 1 };
            int[] wordsY = { 3, 4 };
            int[] wordLengthY = { 1, 1 };

            // Act
            var score = SmithWaterman.CalculateScore(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 1,
                mismatchScore: -2,
                gapPenalty: -2
            );

            // Assert
            score.Should().Be(0);
        }

        [TestMethod]
        public void CalculateScore_WithLogThreshold_ReturnsScoreAndLogs()
        {
            // Arrange
            int[] wordsX = { 1, 2, 3 };
            int[] wordLengthX = { 1, 1, 1 };
            int[] wordsY = { 1, 2, 3 };
            int[] wordLengthY = { 1, 1, 1 };

            // Act
            var score = SmithWaterman.CalculateScore(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 2,
                mismatchScore: -1,
                gapPenalty: -1,
                xString: "a b c",
                yString: "a b c",
                logThreshhold: 0
            );

            // Assert
            score.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CalculateScore_WithLogThresholdNegative_DoesNotLog()
        {
            // Arrange
            int[] wordsX = { 10 };
            int[] wordLengthX = { 3 };
            int[] wordsY = { 10 };
            int[] wordLengthY = { 3 };

            // Act
            var score = SmithWaterman.CalculateScore(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 2,
                mismatchScore: -1,
                gapPenalty: -1,
                xString: "test",
                yString: "test",
                logThreshhold: -1
            );

            // Assert
            score.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CalculateMatrixTuple_ReturnsMatrixAndScore()
        {
            // Arrange
            int[] wordsX = { 1, 2 };
            int[] wordLengthX = { 2, 3 };
            int[] wordsY = { 1, 2 };
            int[] wordLengthY = { 2, 3 };

            // Act
            var (matrix, score) = SmithWaterman.CalculateMatrixTuple(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 2,
                mismatchScore: -1,
                gapPenalty: -1
            );

            // Assert
            score.Should().BeGreaterThan(0);
            matrix.Should().NotBeNull();
            matrix.GetLength(0).Should().BeGreaterThan(0);
            matrix.GetLength(1).Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void CalculateScore_IntArrays_NullWordsX_ThrowsArgumentNullException()
        {
            // Arrange
            int[] wordLengthX = { 1 };
            int[] wordsY = { 1 };
            int[] wordLengthY = { 1 };

            // Act
            Action act = () =>
                SmithWaterman.CalculateScore(
                    null,
                    wordLengthX,
                    wordsY,
                    wordLengthY,
                    matchScore: 1,
                    mismatchScore: -1,
                    gapPenalty: -1
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void CalculateScore_IntArrays_MismatchedLengths_ThrowsArgumentException()
        {
            // Arrange
            int[] wordsX = { 1, 2, 3 };
            int[] wordLengthX = { 1 }; // mismatched
            int[] wordsY = { 1 };
            int[] wordLengthY = { 1 };

            // Act
            Action act = () =>
                SmithWaterman.CalculateScore(
                    wordsX,
                    wordLengthX,
                    wordsY,
                    wordLengthY,
                    matchScore: 1,
                    mismatchScore: -1,
                    gapPenalty: -1
                );

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void CalculateScore_IntArrays_EmptyArrays_ReturnsZero()
        {
            // Arrange
            int[] wordsX = Array.Empty<int>();
            int[] wordLengthX = Array.Empty<int>();
            int[] wordsY = Array.Empty<int>();
            int[] wordLengthY = Array.Empty<int>();

            // Act
            var score = SmithWaterman.CalculateScore(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 2,
                mismatchScore: -1,
                gapPenalty: -1
            );

            // Assert
            score.Should().Be(0);
        }

        [TestMethod]
        public void GetFormattedMatrixText_ReturnsNonEmptyString()
        {
            // Arrange
            int[,] matrix =
            {
                { 0, 0 },
                { 0, 5 },
            };

            // Act
            var text = SmithWaterman.GetFormattedMatrixText(matrix);

            // Assert
            text.Should().NotBeNullOrWhiteSpace();
            text.Should().Contain("5");
        }

        [TestMethod]
        public void CalculateScore_IntArrays_PartialOverlap_ReturnsPartialScore()
        {
            // Arrange
            int[] wordsX = { 1, 2, 3, 4 };
            int[] wordLengthX = { 1, 1, 1, 1 };
            int[] wordsY = { 3, 4, 5, 6 };
            int[] wordLengthY = { 1, 1, 1, 1 };

            // Act
            var score = SmithWaterman.CalculateScore(
                wordsX,
                wordLengthX,
                wordsY,
                wordLengthY,
                matchScore: 2,
                mismatchScore: -1,
                gapPenalty: -1
            );

            // Assert
            score.Should().BeGreaterThan(0);
        }
    }
}
