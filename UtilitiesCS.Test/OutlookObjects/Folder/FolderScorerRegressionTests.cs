using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Golden-baseline characterization and ordering-parity regression tests for
    /// <see cref="UtilitiesCS.FolderScorer"/>. These lock the byte-for-byte ordering and content of
    /// the pre-existing name-only <c>ToArray</c> / <c>ToArray(int)</c> outputs (including the
    /// ordinal tie-break) and prove the additive scored projection <c>ToScoredArray</c> preserves
    /// exactly that ordering. They also confirm the <c>"Error"</c> sentinel never enters the scored
    /// contract.
    /// </summary>
    [TestClass]
    public class FolderScorerRegressionTests
    {
        // A populated scorer with a deliberate two-folder score tie ("Finance" and "HR" both 850)
        // so the ordinal tie-break is exercised. Expected ranking order:
        //   1000 Inbox, then the 850 tie ordered ordinally (Finance < HR), then 300 Sent.
        private static UtilitiesCS.FolderScorer CreatePopulatedScorer()
        {
            var scorer = new UtilitiesCS.FolderScorer();
            scorer.AddSuggestion("Archive\\Inbox", 1000);
            scorer.AddSuggestion("Archive\\HR", 850);
            scorer.AddSuggestion("Archive\\Finance", 850);
            scorer.AddSuggestion("Archive\\Sent", 300);
            return scorer;
        }

        [TestMethod]
        public void ToArray_WithPopulatedScorer_ReturnsGoldenOrderingWithOrdinalTieBreak()
        {
            // Arrange
            var scorer = CreatePopulatedScorer();

            // Act
            var result = scorer.ToArray();

            // Assert: golden baseline locking ordering + content, including the 850 tie ordered
            // ordinally (Finance before HR).
            result
                .Should()
                .Equal("Archive\\Inbox", "Archive\\Finance", "Archive\\HR", "Archive\\Sent");
        }

        [TestMethod]
        public void ToArrayTopN_WithPopulatedScorer_ReturnsGoldenTopNSlice()
        {
            // Arrange
            var scorer = CreatePopulatedScorer();

            // Act
            var result = scorer.ToArray(2);

            // Assert
            result.Should().Equal("Archive\\Inbox", "Archive\\Finance");
        }

        [TestMethod]
        public void ToScoredArray_FolderPathOrdering_EqualsToArrayOrdering()
        {
            // Arrange
            var scorer = CreatePopulatedScorer();

            // Act
            var scoredPaths = scorer.ToScoredArray().Select(x => x.FolderPath).ToArray();

            // Assert: scored projection ordering is byte-for-byte identical to the name-only output.
            scoredPaths.Should().Equal(scorer.ToArray());
        }

        [TestMethod]
        public void ToScoredArrayTopN_FolderPathOrdering_EqualsToArrayTopNOrdering()
        {
            // Arrange
            var scorer = CreatePopulatedScorer();

            // Act
            var scoredPaths = scorer.ToScoredArray(2).Select(x => x.FolderPath).ToArray();

            // Assert
            scoredPaths.Should().Equal(scorer.ToArray(2));
        }

        [TestMethod]
        public void ToScoredArray_WithTie_PreservesIdenticalOrdinalTieBreakAsToArray()
        {
            // Arrange: the tie between Finance and HR (both 850) must resolve identically in both
            // projections via the shared OrderedScores() enumeration.
            var scorer = CreatePopulatedScorer();

            // Act
            var namePaths = scorer.ToArray();
            var scoredPaths = scorer.ToScoredArray().Select(x => x.FolderPath).ToArray();

            // Assert
            scoredPaths.Should().Equal(namePaths);
            scoredPaths[1].Should().Be("Archive\\Finance");
            scoredPaths[2].Should().Be("Archive\\HR");
        }

        [TestMethod]
        public void AddSuggestion_WithErrorSentinel_IsRejectedAndAbsentFromScoredContract()
        {
            // Arrange
            var scorer = new UtilitiesCS.FolderScorer();

            // Act: the object overload rejects the "Error" sentinel.
            var accepted = scorer.AddSuggestion((object)"Error", 500);

            // Assert
            accepted.Should().BeFalse();
            scorer.Count.Should().Be(0);
            scorer.ToScoredArray().Should().BeEmpty();
            scorer.ToArray().Should().NotContain("Error");
        }

        [TestMethod]
        public void AddArray_WhenFirstElementIsErrorSentinel_IsRejectedAndScorerStaysEmpty()
        {
            // Arrange
            var scorer = new UtilitiesCS.FolderScorer();

            // Act
            var accepted = scorer.AddArray(new[] { "Error", "Archive\\Ignored" }, -1);

            // Assert
            accepted.Should().BeFalse();
            scorer.Count.Should().Be(0);
            scorer.ToScoredArray().Should().BeEmpty();
            scorer.ToScoredArray().Select(x => x.FolderPath).Should().NotContain("Error");
        }
    }
}
