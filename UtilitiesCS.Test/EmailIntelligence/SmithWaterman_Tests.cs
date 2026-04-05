using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestMethod]
        public void Max_WithSingleValue_ReturnsThatValue()
        {
            // Act
            var result = SmithWaterman.max(42);

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public void Max_WithMultipleValues_ReturnsLargest()
        {
            // Act
            var result = SmithWaterman.max(1, 5, 3, 2);

            // Assert
            result.Should().Be(5);
        }

        [TestMethod]
        public void Max_WithNegativeValues_ReturnsLeastNegative()
        {
            // Act
            var result = SmithWaterman.max(-10, -3, -7);

            // Assert
            result.Should().Be(-3);
        }

        [TestMethod]
        public void Max_WithAllZeros_ReturnsZero()
        {
            // Act
            var result = SmithWaterman.max(0, 0, 0);

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public void LogMatrixState_WithMatrix_DoesNotThrow()
        {
            // Arrange
            int[,] matrix =
            {
                { 0, 1 },
                { 2, 3 },
            };

            // Act
            Action act = () => SmithWaterman.LogMatrixState(matrix);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void LogMatrixState_WithMatrixAndStrings_DoesNotThrow()
        {
            // Arrange
            int[,] matrix =
            {
                { 0, 1 },
                { 2, 3 },
            };

            // Act
            Action act = () => SmithWaterman.LogMatrixState(matrix, "hello", "world");

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void CalculateScore_StringOverload_ByWords_ReturnsZeroAndPopulatesMatrix()
        {
            // Arrange
            var mockSettings = new Mock<IAppAutoFileObjects>();
            mockSettings.SetupGet(s => s.SmithWatterman_MatchScore).Returns(2);
            mockSettings.SetupGet(s => s.SmithWatterman_MismatchScore).Returns(-1);
            mockSettings.SetupGet(s => s.SmithWatterman_GapPenalty).Returns(-1);
            object[,] matrix = null;

            // Act
            var score = SmithWaterman.CalculateScore(
                "hello world",
                "hello world",
                ref matrix,
                mockSettings.Object,
                SmithWaterman.SW_Options.ByWords
            );

            // Assert
            score.Should().Be(0);
            matrix.Should().NotBeNull();
            matrix[3, 1].Should().Be("hello");
            matrix[4, 1].Should().Be("world");
            matrix[1, 3].Should().Be("hello");
            matrix[1, 4].Should().Be("world");
        }

        [TestMethod]
        public void CalculateScore_StringOverload_ByLetters_ReturnsZeroAndPopulatesMatrix()
        {
            // Arrange
            var mockSettings = new Mock<IAppAutoFileObjects>();
            mockSettings.SetupGet(s => s.SmithWatterman_MatchScore).Returns(2);
            mockSettings.SetupGet(s => s.SmithWatterman_MismatchScore).Returns(-1);
            mockSettings.SetupGet(s => s.SmithWatterman_GapPenalty).Returns(-1);
            object[,] matrix = null;

            // Act
            var score = SmithWaterman.CalculateScore(
                "abc",
                "axc",
                ref matrix,
                mockSettings.Object,
                SmithWaterman.SW_Options.ByLetters
            );

            // Assert
            score.Should().Be(0);
            matrix.Should().NotBeNull();
            matrix[3, 1].Should().Be("a");
            matrix[5, 1].Should().Be("c");
        }

        [TestMethod]
        public void CalculateScore_StringOverload_DifferentStrings_ReturnsZero()
        {
            // Arrange
            var mockSettings = new Mock<IAppAutoFileObjects>();
            mockSettings.SetupGet(s => s.SmithWatterman_MatchScore).Returns(2);
            mockSettings.SetupGet(s => s.SmithWatterman_MismatchScore).Returns(-1);
            mockSettings.SetupGet(s => s.SmithWatterman_GapPenalty).Returns(-1);
            object[,] matrix = null;

            // Act
            var score = SmithWaterman.CalculateScore(
                "hello",
                "goodbye",
                ref matrix,
                mockSettings.Object,
                SmithWaterman.SW_Options.ByWords
            );

            // Assert
            score.Should().Be(0);
            matrix.Should().NotBeNull();
        }

        [TestMethod]
        public void GetFormattedMatrixText_WithLargerMatrix_ContainsAllValues()
        {
            // Arrange
            int[,] matrix =
            {
                { 0, 0, 0 },
                { 0, 2, 0 },
                { 0, 0, 4 },
            };

            // Act
            var text = SmithWaterman.GetFormattedMatrixText(matrix);

            // Assert
            text.Should().Contain("2");
            text.Should().Contain("4");
        }

        [TestMethod]
        public void CalculateScore_IntArrays_NullWordsY_ThrowsArgumentNullException()
        {
            // Arrange
            int[] wordsX = { 1 };
            int[] wordLengthX = { 1 };
            int[] wordLengthY = { 1 };

            // Act
            Action act = () =>
                SmithWaterman.CalculateScore(
                    wordsX,
                    wordLengthX,
                    null,
                    wordLengthY,
                    matchScore: 1,
                    mismatchScore: -1,
                    gapPenalty: -1
                );

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void CalculateScore_IntArrays_NullWordLengthX_ThrowsArgumentNullException()
        {
            // Arrange
            int[] wordsX = { 1 };
            int[] wordsY = { 1 };
            int[] wordLengthY = { 1 };

            // Act
            Action act = () =>
                SmithWaterman.CalculateScore(
                    wordsX,
                    null,
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
        public void CalculateScore_IntArrays_MismatchedYLengths_ThrowsArgumentException()
        {
            // Arrange
            int[] wordsX = { 1 };
            int[] wordLengthX = { 1 };
            int[] wordsY = { 1, 2, 3 };
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
        public void CalculateScore_IntArrays_SingleElement_ReturnsExpectedScore()
        {
            // Arrange
            int[] wordsX = { 5 };
            int[] wordLengthX = { 3 };
            int[] wordsY = { 5 };
            int[] wordLengthY = { 3 };

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
            score.Should().Be(6); // matchScore * wordLength = 2 * 3
        }
    }
}
