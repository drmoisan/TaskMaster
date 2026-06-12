using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.Evaluation;

namespace UtilitiesCS.Test.EmailIntelligence.Evaluation
{
    /// <summary>
    /// Unit tests for <see cref="FolderPredictorEvaluator"/> covering deterministic index-proxy
    /// split reproducibility (AC16), per-leaf precision/recall/macro-F1 correctness, and
    /// abstention-as-false-negative accounting (AC8). All tests are deterministic, in-memory, and
    /// use no Outlook COM instances, no external services, and no temporary files. The
    /// <see cref="IFolderWrapper"/> dependency is mocked with only <c>RelativePath</c> configured,
    /// so no Outlook <c>MAPIFolder</c> is ever touched.
    /// </summary>
    [TestClass]
    public class FolderPredictorEvaluator_Tests
    {
        // Builds a MinedMailInfo with the supplied leaf path and tokens; FolderInfo is a Moq stub
        // exposing only RelativePath (no Outlook COM).
        private static MinedMailInfo Mail(string leaf, params string[] tokens)
        {
            var folder = new Mock<IFolderWrapper>();
            folder.SetupGet(x => x.RelativePath).Returns(leaf);
            return new MinedMailInfo { FolderInfo = folder.Object, Tokens = tokens };
        }

        // Builds the LCPPN predictor under test from a corpus slice.
        private static IFolderPredictor BuildPredictor(
            IReadOnlyList<MinedMailInfo> train,
            double minimumPathProbability = 0.01
        )
        {
            var config = LcppnFolderPredictorConfig.Create(
                useLcppnPredictor: true,
                beamWidth: 3,
                minimumPathProbability: minimumPathProbability,
                shrinkageLambda: 0.7,
                minColdStartExamples: 0
            );
            return LcppnFolderPredictor.Build(train, config);
        }

        // AC16: the index-proxy split boundary is deterministic for a given corpus length and
        // fraction, and the full evaluation reproduces identical results across runs.
        [TestMethod]
        public void Evaluate_SameInput_ProducesDeterministicSplitAndResult()
        {
            // Arrange: 10 examples, 70% train fraction -> boundary index 7.
            var corpus = Enumerable
                .Range(0, 10)
                .Select(i => Mail(@"Projects\Alpha", "alpha", "spec"))
                .ToArray();
            var config = new EvaluationConfig(trainFraction: 0.7);

            var first = new FolderPredictorEvaluator(t => BuildPredictor(t), corpus, config);
            var second = new FolderPredictorEvaluator(t => BuildPredictor(t), corpus, config);

            // Act
            var boundary = first.ComputeTrainBoundary();
            var resultA = first.Evaluate();
            var resultB = second.Evaluate();

            // Assert
            boundary.Should().Be(7, "floor(10 * 0.7) = 7 (AC16 index-proxy split)");
            resultA.TestCount.Should().Be(3);
            resultA.MacroF1.Should().Be(resultB.MacroF1, "evaluation is deterministic (AC16)");
            resultA.AbstentionRate.Should().Be(resultB.AbstentionRate);
        }

        // Per-leaf precision/recall/macro-F1 correctness on a separable two-class corpus where the
        // predictor classifies every test example correctly.
        [TestMethod]
        public void Evaluate_SeparableCorpus_ComputesPerfectPrecisionRecallAndMacroF1()
        {
            // Arrange: two well-separated single-level leaves with strong token signals.
            var corpus = new[]
            {
                Mail("Invoices", "invoice", "payment", "due"),
                Mail("Invoices", "invoice", "payment", "due"),
                Mail("Invoices", "invoice", "payment", "due"),
                Mail("News", "newsletter", "weekly", "digest"),
                Mail("News", "newsletter", "weekly", "digest"),
                Mail("News", "newsletter", "weekly", "digest"),
                // Test slice (after boundary): one of each, matching their training signal.
                Mail("Invoices", "invoice", "payment", "due"),
                Mail("News", "newsletter", "weekly", "digest"),
            };
            var config = new EvaluationConfig(trainFraction: 0.75); // floor(8 * 0.75) = 6

            var evaluator = new FolderPredictorEvaluator(t => BuildPredictor(t), corpus, config);

            // Act
            var result = evaluator.Evaluate();

            // Assert: both test examples classified correctly -> perfect metrics, no abstentions.
            result.TestCount.Should().Be(2);
            result.AbstentionRate.Should().Be(0.0);
            result.PerLeaf["Invoices"].Precision.Should().Be(1.0);
            result.PerLeaf["Invoices"].Recall.Should().Be(1.0);
            result.PerLeaf["Invoices"].F1.Should().Be(1.0);
            result.PerLeaf["News"].F1.Should().Be(1.0);
            result.MacroF1.Should().Be(1.0, "every test example is classified correctly");
        }

        // AC8: an abstained test example counts as a false negative for its true class and never as
        // a false positive for any class, lowering the true class's recall without inflating any
        // class's false positives.
        [TestMethod]
        public void Evaluate_AbstainedExample_CountsAsFalseNegativeNotFalsePositive()
        {
            // Arrange: a stub predictor that always abstains (empty classification result). Using a
            // stub isolates the abstention accounting from the LCPPN scoring path.
            var corpus = new[]
            {
                Mail("Invoices", "invoice"),
                Mail("Invoices", "invoice"),
                Mail("Invoices", "invoice"),
                Mail("Invoices", "invoice"), // test slice
            };
            var config = new EvaluationConfig(trainFraction: 0.75); // boundary 3, one test example
            var alwaysAbstain = new Mock<IFolderPredictor>();
            alwaysAbstain
                .Setup(x => x.Classify(It.IsAny<string[]>()))
                .Returns(
                    Array
                        .Empty<Prediction<string>>()
                        .AsParallel()
                        .OrderByDescending(p => p.Probability)
                );

            var evaluator = new FolderPredictorEvaluator(_ => alwaysAbstain.Object, corpus, config);

            // Act
            var result = evaluator.Evaluate();

            // Assert: the single test example abstained.
            result.TestCount.Should().Be(1);
            result.AbstentionRate.Should().Be(1.0);

            // The true class recall drops to 0 (the abstention is a false negative), while precision
            // is not inflated by a false positive (tp + fp == 0 -> precision defined as 0).
            result
                .PerLeaf["Invoices"]
                .Recall.Should()
                .Be(0.0, "abstention is a false negative (AC8)");
            result
                .PerLeaf["Invoices"]
                .Precision.Should()
                .Be(0.0, "abstention never creates a false positive (AC8)");
            result
                .PerLeaf.Should()
                .ContainSingle("only the true class is observed in the test slice");
        }

        // Negative-path: a wrong, non-abstaining prediction is a false positive for the predicted
        // class and a false negative for the true class, distinguishing it from abstention.
        [TestMethod]
        public void Evaluate_WrongPrediction_CountsAsFalsePositiveForPredictedClass()
        {
            // Arrange: a stub predictor that always predicts "Wrong".
            var corpus = new[]
            {
                Mail("Right", "a"),
                Mail("Right", "a"),
                Mail("Right", "a"),
                Mail("Right", "a"), // test slice
            };
            var config = new EvaluationConfig(trainFraction: 0.75);
            var alwaysWrong = new Mock<IFolderPredictor>();
            alwaysWrong
                .Setup(x => x.Classify(It.IsAny<string[]>()))
                .Returns(
                    new[] { new Prediction<string>("Wrong", 0.9) }
                        .AsParallel()
                        .OrderByDescending(p => p.Probability)
                );

            var evaluator = new FolderPredictorEvaluator(_ => alwaysWrong.Object, corpus, config);

            // Act
            var result = evaluator.Evaluate();

            // Assert: no abstention; "Wrong" gets a false positive, "Right" a false negative.
            result.AbstentionRate.Should().Be(0.0);
            result.PerLeaf["Right"].Recall.Should().Be(0.0);
            result
                .PerLeaf["Wrong"]
                .Precision.Should()
                .Be(0.0, "false positive only, no true positive");
        }

        [TestMethod]
        public void Constructor_NullArguments_FailFast()
        {
            var corpus = new[] { Mail("A", "x") };
            var config = new EvaluationConfig();

            Func<IReadOnlyList<MinedMailInfo>, IFolderPredictor> factory = t => BuildPredictor(t);

            ((Action)(() => new FolderPredictorEvaluator(null, corpus, config)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => new FolderPredictorEvaluator(factory, null, config)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => new FolderPredictorEvaluator(factory, corpus, null)))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void EvaluationConfig_InvalidTrainFraction_FailFast()
        {
            ((Action)(() => new EvaluationConfig(0.0)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
            ((Action)(() => new EvaluationConfig(1.0)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
            ((Action)(() => new EvaluationConfig(double.NaN)))
                .Should()
                .Throw<ArgumentOutOfRangeException>();
        }
    }
}
