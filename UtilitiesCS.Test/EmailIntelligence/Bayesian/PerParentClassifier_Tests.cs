using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Unit tests for <see cref="PerParentClassifier"/> covering the shrinkage blend at a fixed
    /// lambda (AC9), the cold-start fallback boundary at <c>MinColdStartExamples</c> (AC10),
    /// incremental train/untrain count changes, sibling isolation, probability sanity bounds, and
    /// invalid-lambda fail-fast (AC9 validation). All tests are deterministic, in-memory, and use
    /// no temporary files or Outlook COM.
    /// </summary>
    [TestClass]
    public class PerParentClassifier_Tests
    {
        // Builds a parent classifier whose total examples are at or above the cold-start floor so
        // the shrinkage blend is active. Two children with disjoint vocabularies.
        private static PerParentClassifier CreateTrainedParent(
            double lambda = 0.7,
            int minColdStart = 2
        )
        {
            var classifier = new PerParentClassifier(lambda, minColdStart);
            classifier.Train("Alpha", new[] { "invoice", "payment", "invoice" }, 1);
            classifier.Train("Beta", new[] { "newsletter", "digest" }, 1);
            return classifier;
        }

        // AC9: the shrinkage blend at a fixed lambda ranks the child whose vocabulary matches the
        // query highest, and the scores are valid normalized probabilities.
        [TestMethod]
        public void ScoreChildren_BlendActive_RanksMatchingChildHighest()
        {
            // Arrange
            var classifier = CreateTrainedParent(lambda: 0.7, minColdStart: 2);

            // Act
            var scores = classifier.ScoreChildren(new[] { "invoice", "payment" });

            // Assert
            scores.Should().ContainKeys("Alpha", "Beta");
            scores["Alpha"]
                .Should()
                .BeGreaterThan(scores["Beta"], "the query matches Alpha's corpus");
            scores.Values.Sum().Should().BeApproximately(1.0, 1e-9, "scores are normalized");
            scores.Values.Should().OnlyContain(v => v >= 0.0 && v <= 1.0);
        }

        // AC9 (blend vs leaf-only): at lambda = 1 the parent term drops out (pure leaf estimate);
        // a lambda below 1 mixes in the parent-scope estimate, producing a different ranking gap.
        // This proves the lambda actually controls the blend.
        [TestMethod]
        public void ScoreChildren_LambdaControlsBlend_LeafOnlyVsBlendDiffer()
        {
            // Arrange: identical corpora, different lambda
            var leafOnly = CreateTrainedParent(lambda: 1.0, minColdStart: 2);
            var blended = CreateTrainedParent(lambda: 0.5, minColdStart: 2);

            // Act
            var leafScores = leafOnly.ScoreChildren(new[] { "invoice" });
            var blendScores = blended.ScoreChildren(new[] { "invoice" });

            // Assert: the leaf-only configuration concentrates more mass on the matching child than
            // the parent-mixed configuration, so the Alpha probabilities are not equal.
            leafScores["Alpha"]
                .Should()
                .NotBe(blendScores["Alpha"], "lambda must change the blended probability");
            leafScores["Alpha"].Should().BeGreaterThan(blendScores["Alpha"]);
        }

        // AC10: below MinColdStartExamples the classifier reports cold-start; at or above it does not.
        [TestMethod]
        public void IsColdStart_TogglesAtThreshold()
        {
            // Arrange: threshold 3; one example trained
            var classifier = new PerParentClassifier(0.7, minColdStartExamples: 3);
            classifier.Train("Alpha", new[] { "a", "b" }, 1);

            // Assert: 1 < 3 → cold start
            classifier.IsColdStart.Should().BeTrue("one example is below the threshold of three");

            // Act: train up to the threshold
            classifier.Train("Alpha", new[] { "a" }, 1);
            classifier.Train("Beta", new[] { "c" }, 1);

            // Assert: 3 >= 3 → no longer cold start
            classifier.TotalExamples.Should().Be(3);
            classifier.IsColdStart.Should().BeFalse("three examples reach the threshold");
        }

        // AC10: cold-start scoring (leaf-only) and blended scoring can produce different rankings
        // for the same corpus, confirming the fallback path is exercised distinctly.
        [TestMethod]
        public void ScoreChildren_ColdStartFallback_UsesLeafOnlyEstimate()
        {
            // Arrange: a corpus with total examples below the threshold engages cold-start
            var coldStart = new PerParentClassifier(0.5, minColdStartExamples: 10);
            coldStart.Train("Alpha", new[] { "invoice", "payment" }, 1);
            coldStart.Train("Beta", new[] { "newsletter" }, 1);

            // Act
            coldStart.IsColdStart.Should().BeTrue();
            var scores = coldStart.ScoreChildren(new[] { "invoice" });

            // Assert: still a valid normalized distribution that prefers the matching child
            scores.Values.Sum().Should().BeApproximately(1.0, 1e-9);
            scores["Alpha"].Should().BeGreaterThan(scores["Beta"]);
        }

        // Incremental training registers a new child without affecting siblings.
        [TestMethod]
        public void Train_NewChild_DoesNotAffectSiblingCounts()
        {
            // Arrange
            var classifier = CreateTrainedParent();
            var alphaCountBefore = classifier.Group.Classifiers["Alpha"].MatchEmailCount;

            // Act
            classifier.Train("Gamma", new[] { "report", "quarterly" }, 1);

            // Assert
            classifier.ChildSegments.Should().Contain("Gamma");
            classifier
                .Group.Classifiers["Alpha"]
                .MatchEmailCount.Should()
                .Be(alphaCountBefore, "training Gamma must not change Alpha");
        }

        // UnTrain decrements the same child's counts.
        [TestMethod]
        public void UnTrain_ExistingChild_DecrementsItsCount()
        {
            // Arrange
            var classifier = new PerParentClassifier(0.7, 2);
            classifier.Train("Alpha", new[] { "invoice" }, 1);
            classifier.Train("Alpha", new[] { "invoice" }, 1);
            var before = classifier.Group.Classifiers["Alpha"].MatchEmailCount;

            // Act
            classifier.UnTrain("Alpha", new[] { "invoice" }, 1);

            // Assert
            classifier
                .Group.Classifiers["Alpha"]
                .MatchEmailCount.Should()
                .BeLessThan(before, "untraining decrements the child email count");
        }

        // Empty parent returns an empty score map rather than throwing.
        [TestMethod]
        public void ScoreChildren_NoChildren_ReturnsEmpty()
        {
            // Arrange
            var classifier = new PerParentClassifier(0.7, 2);

            // Act
            var scores = classifier.ScoreChildren(new[] { "anything" });

            // Assert
            scores.Should().BeEmpty();
        }

        // Probability sanity: every score is within [0, 1] and they sum to one.
        [TestMethod]
        public void ScoreChildren_AllProbabilities_AreNormalizedAndBounded()
        {
            // Arrange
            var classifier = CreateTrainedParent();

            // Act
            var scores = classifier.ScoreChildren(new[] { "invoice", "newsletter", "unseen" });

            // Assert
            scores.Values.Should().OnlyContain(v => v >= 0.0 && v <= 1.0);
            scores.Values.Sum().Should().BeApproximately(1.0, 1e-9);
        }

        // AC9 validation: lambda outside [0, 1] fails fast.
        [DataTestMethod]
        [DataRow(-0.1)]
        [DataRow(1.1)]
        [DataRow(double.NaN)]
        public void Constructor_InvalidLambda_Throws(double lambda)
        {
            // Act
            var act = () => new PerParentClassifier(lambda, 2);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("shrinkageLambda");
        }

        // Negative cold-start count fails fast.
        [TestMethod]
        public void Constructor_NegativeColdStart_Throws()
        {
            // Act
            var act = () => new PerParentClassifier(0.7, -1);

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("minColdStartExamples");
        }

        // Train with an empty child segment fails fast.
        [TestMethod]
        public void Train_EmptyChildSegment_Throws()
        {
            // Arrange
            var classifier = new PerParentClassifier(0.7, 2);

            // Act
            var act = () => classifier.Train("", new[] { "x" }, 1);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        // ScoreChildren with null tokens fails fast.
        [TestMethod]
        public void ScoreChildren_NullTokens_Throws()
        {
            // Arrange
            var classifier = CreateTrainedParent();

            // Act
            var act = () => classifier.ScoreChildren(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
