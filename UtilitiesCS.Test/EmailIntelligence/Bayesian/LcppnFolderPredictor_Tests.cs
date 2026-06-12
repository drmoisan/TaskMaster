using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Unit tests for <see cref="LcppnFolderPredictor"/> covering configuration defaults and
    /// validation including <c>BeamWidth &gt;= 1</c> (AC6/AC9), localized incremental Train/UnTrain
    /// (AC11), local new-leaf addition (AC12), and corpus Build behavior. Beam-search descent and
    /// abstention tests live in <see cref="LcppnFolderPredictor_Classify_Tests"/>. All tests are
    /// deterministic, in-memory, and use no temporary files or Outlook COM.
    /// </summary>
    [TestClass]
    public class LcppnFolderPredictor_Tests
    {
        private static LcppnFolderPredictorConfig Config(
            int beamWidth = 3,
            double minimumPathProbability = 0.5,
            double shrinkageLambda = 0.7,
            int minColdStartExamples = 0
        ) =>
            LcppnFolderPredictorConfig.Create(
                useLcppnPredictor: true,
                beamWidth: beamWidth,
                minimumPathProbability: minimumPathProbability,
                shrinkageLambda: shrinkageLambda,
                minColdStartExamples: minColdStartExamples
            );

        // Trains a deterministic three-level hierarchy with one dominant leaf path.
        private static LcppnFolderPredictor CreateTrainedPredictor(
            LcppnFolderPredictorConfig config
        )
        {
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = config.BeamWidth,
                MinimumPathProbability = config.MinimumPathProbability,
                ShrinkageLambda = config.ShrinkageLambda,
                MinColdStartExamples = config.MinColdStartExamples,
            };
            // Projects\Alpha\2024 has the "alpha" "spec" tokens; Clients\Acme is unrelated.
            predictor.Train(@"Projects\Alpha\2024", new[] { "alpha", "spec", "design" }, 1);
            predictor.Train(@"Projects\Beta\2024", new[] { "beta", "rollout" }, 1);
            predictor.Train(@"Clients\Acme", new[] { "invoice", "billing" }, 1);
            return predictor;
        }

        // AC6: BeamWidth >= 1 is validated at config construction.
        [TestMethod]
        public void Config_BeamWidthBelowOne_Throws()
        {
            // Act
            var act = () => LcppnFolderPredictorConfig.Create(beamWidth: 0);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("BeamWidth");
        }

        // Config defaults match the spec.
        [TestMethod]
        public void Config_Defaults_MatchSpecification()
        {
            // Arrange & Act
            var config = new LcppnFolderPredictorConfig();

            // Assert
            config.UseLcppnPredictor.Should().BeFalse();
            config.BeamWidth.Should().Be(3);
            config.MinimumPathProbability.Should().Be(0.5);
            config.ShrinkageLambda.Should().Be(0.7);
            config.MinColdStartExamples.Should().Be(5);
        }

        // AC6/AC9 validation: MinimumPathProbability must be strictly in (0, 1).
        [DataTestMethod]
        [DataRow(0.0)]
        [DataRow(1.0)]
        [DataRow(-0.1)]
        [DataRow(1.5)]
        public void Config_InvalidMinimumPathProbability_Throws(double value)
        {
            // Act
            var act = () => LcppnFolderPredictorConfig.Create(minimumPathProbability: value);

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("MinimumPathProbability");
        }

        // AC9 validation: ShrinkageLambda must be in [0, 1].
        [DataTestMethod]
        [DataRow(-0.1)]
        [DataRow(1.1)]
        public void Config_InvalidShrinkageLambda_Throws(double value)
        {
            // Act
            var act = () => LcppnFolderPredictorConfig.Create(shrinkageLambda: value);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("ShrinkageLambda");
        }

        // Validation: MinColdStartExamples must be non-negative.
        [TestMethod]
        public void Config_NegativeMinColdStartExamples_Throws()
        {
            // Act
            var act = () => LcppnFolderPredictorConfig.Create(minColdStartExamples: -1);

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>()
                .WithParameterName("MinColdStartExamples");
        }

        // AC11: training a leaf updates only the classifiers on that root-to-leaf path.
        [TestMethod]
        public void Train_Leaf_UpdatesOnlyPathClassifiers()
        {
            // Arrange
            var predictor = CreateTrainedPredictor(Config());
            var clientsExamplesBefore = predictor.Nodes["Clients"].TotalExamples;
            var rootChildrenBefore = predictor.Nodes[""].ChildSegments.OrderBy(s => s).ToArray();

            // Act: train an existing path under Projects
            predictor.Train(@"Projects\Alpha\2024", new[] { "alpha", "spec" }, 1);

            // Assert: Clients (off the path) is unchanged; the Projects path nodes grew.
            predictor
                .Nodes["Clients"]
                .TotalExamples.Should()
                .Be(clientsExamplesBefore, "a node off the trained path must be unchanged (AC11)");
            predictor
                .Nodes[""]
                .ChildSegments.OrderBy(s => s)
                .Should()
                .Equal(rootChildrenBefore, "root children set is unchanged for an existing path");
        }

        // AC11: untraining a prior leaf decrements only that path's classifiers.
        [TestMethod]
        public void UnTrain_PriorLeaf_DecrementsOnlyPathClassifiers()
        {
            // Arrange
            var predictor = CreateTrainedPredictor(Config());
            predictor.Train(@"Projects\Alpha\2024", new[] { "alpha" }, 1);
            var clientsBefore = predictor.Nodes["Clients"].TotalExamples;
            var projectsBefore = predictor.Nodes["Projects"].TotalExamples;

            // Act
            predictor.UnTrain(@"Projects\Alpha\2024", new[] { "alpha" }, 1);

            // Assert
            predictor
                .Nodes["Clients"]
                .TotalExamples.Should()
                .Be(clientsBefore, "untraining a Projects leaf must not change Clients (AC11)");
            predictor
                .Nodes["Projects"]
                .TotalExamples.Should()
                .BeLessThan(projectsBefore, "the prior path's classifier is decremented");
        }

        // AC12: registering a new leaf under an existing parent modifies only that parent's node.
        [TestMethod]
        public void Train_NewLeaf_ModifiesOnlyTargetParentClassifier()
        {
            // Arrange
            var predictor = CreateTrainedPredictor(Config());
            var otherParents = predictor
                .Nodes.Where(kvp => kvp.Key != "Projects")
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ChildSegments.OrderBy(s => s).ToArray()
                );

            // Act: add a brand-new child under the existing "Projects" parent
            predictor.Train(@"Projects\Gamma", new[] { "gamma", "new" }, 1);

            // Assert: Projects gained Gamma; every other parent's child set is byte-for-byte unchanged
            predictor.Nodes["Projects"].ChildSegments.Should().Contain("Gamma");
            foreach (var kvp in otherParents)
            {
                predictor
                    .Nodes[kvp.Key]
                    .ChildSegments.OrderBy(s => s)
                    .Should()
                    .Equal(kvp.Value, $"parent '{kvp.Key}' must be unchanged by a new leaf (AC12)");
            }
        }

        // The predictor satisfies the shared seam.
        [TestMethod]
        public void LcppnFolderPredictor_IsAssignableToIFolderPredictor()
        {
            // Arrange & Act
            var predictor = new LcppnFolderPredictor();

            // Assert
            predictor.Should().BeAssignableTo<IFolderPredictor>();
        }

        // Build skips entries with no relative path and never throws on null tokens.
        [TestMethod]
        public void Build_NullCorpus_Throws()
        {
            // Act
            var act = () => LcppnFolderPredictor.Build(null, Config());

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        // Build skips entries whose relative path is null or empty, and tolerates null tokens,
        // exercising the continue/empty-token branches of Build.
        [TestMethod]
        public void Build_SkipsEntriesWithEmptyRelativePathAndNullTokens()
        {
            // Arrange: one valid entry, one empty-path entry, one null-FolderInfo entry.
            var corpus = new[]
            {
                MinedMail(@"Projects\Alpha", new[] { "alpha", "spec" }),
                MinedMail("", new[] { "ignored" }),
                MinedMail(@"Clients\Acme", null),
                new MinedMailInfo { FolderInfo = null, Tokens = new[] { "x" } },
            };

            // Act
            var predictor = LcppnFolderPredictor.Build(
                corpus,
                Config(minimumPathProbability: 0.01)
            );

            // Assert: only the two valid leaves are represented; empty/null entries are skipped.
            predictor.Nodes.Should().ContainKey("");
            predictor.Nodes[""].ChildSegments.Should().Contain(new[] { "Projects", "Clients" });
            predictor.Tree.IsLeaf(@"Clients\Acme").Should().BeTrue("null tokens are tolerated");
        }

        // Train and UnTrain on an empty/whitespace tag are no-ops (SplitPath empty branch), and
        // UnTrain on an unknown leaf path touches no node.
        [TestMethod]
        public void TrainAndUnTrain_EmptyTag_AreNoOps()
        {
            // Arrange
            var predictor = CreateTrainedPredictor(Config(minimumPathProbability: 0.01));
            var nodeCountBefore = predictor.Nodes.Count;

            // Act: empty/whitespace and null tags are ignored; UnTrain on unknown path is a no-op.
            predictor.Train("", new[] { "x" }, 1);
            predictor.Train(@"\\", new[] { "y" }, 1);
            predictor.UnTrain("", new[] { "x" }, 1);
            predictor.UnTrain(@"Unknown\Path", new[] { "z" }, 1);

            // Assert: no nodes were added or removed.
            predictor.Nodes.Count.Should().Be(nodeCountBefore);
        }

        // F2 coverage: Build with a null config fails fast (the config-null guard branch).
        [TestMethod]
        public void Build_NullConfig_Throws()
        {
            // Arrange
            var corpus = new[] { MinedMail(@"Projects\Alpha", new[] { "alpha" }) };

            // Act
            var act = () => LcppnFolderPredictor.Build(corpus, null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("config");
        }

        // F2 coverage: UnTrain on a tag whose intermediate parent key is absent from Nodes skips that
        // segment without throwing (the TryGetValue miss branch in UnTrain). The deep path
        // "Solo\Deep\Leaf" has no node for the intermediate parent "Solo" (only the root exists),
        // so the second iteration's TryGetValue("Solo") miss is exercised.
        [TestMethod]
        public void UnTrain_IntermediateParentMissing_SkipsMissingSegment()
        {
            // Arrange: a predictor whose only registered nodes are the root and "Solo" is absent.
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = 3,
                MinimumPathProbability = 0.001,
                ShrinkageLambda = 0.7,
                MinColdStartExamples = 0,
            };
            predictor.Train("Top", new[] { "tok" }, 1);
            predictor.Nodes.Keys.Should().NotContain("Solo", "the intermediate parent is absent");

            // Act: untrain a deep path whose intermediate parent "Solo" has no classifier in Nodes,
            // so the loop iteration for the "Deep" segment (parentKey == "Solo") hits the miss branch.
            var act = () => predictor.UnTrain(@"Solo\Deep\Leaf", new[] { "tok" }, 1);

            // Assert: no throw; the absent intermediate segment is skipped rather than created.
            act.Should().NotThrow();
            predictor
                .Nodes.Keys.Should()
                .NotContain("Solo", "UnTrain never registers a missing node");
        }

        // Builds a MinedMailInfo with a Moq IFolderWrapper exposing only RelativePath (no COM).
        private static MinedMailInfo MinedMail(string relativePath, string[] tokens)
        {
            var folder = new Mock<IFolderWrapper>();
            folder.SetupGet(x => x.RelativePath).Returns(relativePath);
            return new MinedMailInfo { FolderInfo = folder.Object, Tokens = tokens };
        }
    }
}
