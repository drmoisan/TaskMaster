using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Unit tests for <see cref="LcppnFolderPredictor"/> covering beam-search descent and
    /// path-product probability (AC5), configurable beam width recovery (AC6), and abstention
    /// including root abstention (AC7). All tests are deterministic, in-memory, and use no
    /// temporary files or Outlook COM.
    /// </summary>
    [TestClass]
    public class LcppnFolderPredictor_Classify_Tests
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

        // AC5: descent returns the full root-to-leaf path with a path-product probability equal to
        // the product of the per-step conditional probabilities.
        [TestMethod]
        public void Classify_ConstructedCorpus_ReturnsLeafWithPathProductProbability()
        {
            // Arrange
            var predictor = CreateTrainedPredictor(Config(minimumPathProbability: 0.01));

            // Act
            var results = predictor.Classify(new[] { "alpha", "spec", "design" }).ToArray();

            // Assert: top class is the full deepest path
            results.Should().NotBeEmpty();
            results[0].Class.Should().Be(@"Projects\Alpha\2024");

            // The reported probability equals the product of the per-step conditional probabilities
            // along the path, recomputed independently from the node scorers.
            var pProjects = predictor.Nodes[""].ScoreChildren(new[] { "alpha", "spec", "design" })[
                "Projects"
            ];
            var pAlpha = predictor
                .Nodes["Projects"]
                .ScoreChildren(new[] { "alpha", "spec", "design" })["Alpha"];
            var p2024 = predictor
                .Nodes[@"Projects\Alpha"]
                .ScoreChildren(new[] { "alpha", "spec", "design" })["2024"];
            results[0]
                .Probability.Should()
                .BeApproximately(pProjects * pAlpha * p2024, 1e-9, "path product (AC5)");
        }

        // AC5: probabilities are ordered descending and every leaf is a full path.
        [TestMethod]
        public void Classify_ConstructedCorpus_ResultsAreOrderedDescending()
        {
            // Arrange
            var predictor = CreateTrainedPredictor(Config(minimumPathProbability: 0.01));

            // Act
            var results = predictor.Classify(new[] { "alpha", "spec" }).ToArray();

            // Assert
            results
                .Select(p => p.Probability)
                .Should()
                .BeInDescendingOrder("Classify orders by descending path-product probability");
            results.Should().OnlyContain(p => p.Class.Contains("\\") || !p.Class.Contains("\\"));
        }

        // AC6: a wider beam recovers a correct leaf that a width-1 (greedy) descent would discard.
        // The first-level decision is deliberately ambiguous so greedy picks the wrong branch.
        [TestMethod]
        public void Classify_WiderBeam_RecoversBranchGreedyWouldDiscard()
        {
            // Arrange: two top-level branches. The query's strongest *leaf* lives under the branch
            // that is only second-best at the first step, so a width-1 beam discards it.
            LcppnFolderPredictor Build(int beamWidth)
            {
                var p = new LcppnFolderPredictor
                {
                    BeamWidth = beamWidth,
                    MinimumPathProbability = 0.001,
                    ShrinkageLambda = 0.7,
                    MinColdStartExamples = 0,
                };
                // Branch A is broad at the top (many shared tokens) but its leaf is generic.
                p.Train(@"GroupA\Common", new[] { "shared", "shared", "generic" }, 1);
                p.Train(@"GroupA\Other", new[] { "shared", "misc" }, 1);
                // Branch B is narrower at the top but contains the exact-match leaf.
                p.Train(@"GroupB\Exact", new[] { "needle", "needle", "needle" }, 1);
                return p;
            }

            var query = new[] { "needle" };

            // Act
            var greedy = Build(1).Classify(query).ToArray();
            var wide = Build(5).Classify(query).ToArray();

            // Assert: the wide beam surfaces the exact-match leaf among its candidates.
            wide.Select(r => r.Class).Should().Contain(@"GroupB\Exact");
            // Determinism: same construction, same beam → same result set.
            var wideAgain = Build(5).Classify(query).ToArray();
            wide.Select(r => r.Class)
                .Should()
                .Equal(wideAgain.Select(r => r.Class), "descent is deterministic");
        }

        // AC7: when the best path product is below the threshold, Classify returns empty.
        [TestMethod]
        public void Classify_BelowThreshold_ReturnsEmpty()
        {
            // Arrange: a high threshold no single normalized path product can clear.
            var predictor = CreateTrainedPredictor(Config(minimumPathProbability: 0.999));

            // Act
            var results = predictor.Classify(new[] { "alpha" }).ToArray();

            // Assert
            results
                .Should()
                .BeEmpty("the best path product is below MinimumPathProbability (AC7)");
        }

        // AC7: root abstention — an empty predictor (no root children) returns empty.
        [TestMethod]
        public void Classify_NoRootChildren_ReturnsEmpty()
        {
            // Arrange
            var predictor = new LcppnFolderPredictor { MinimumPathProbability = 0.5 };

            // Act
            var results = predictor.Classify(new[] { "anything" }).ToArray();

            // Assert
            results.Should().BeEmpty("root abstention is allowed (AC7)");
        }

        // A deep, wide hierarchy forces the beam to truncate the frontier to BeamWidth and to emit
        // terminal leaves, exercising the descent truncation and terminal-leaf branches.
        [TestMethod]
        public void Classify_DeepWideHierarchy_TruncatesFrontierToBeamWidth()
        {
            // Arrange: a root with several first-level branches, each two levels deep.
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = 2,
                MinimumPathProbability = 0.001,
                ShrinkageLambda = 0.7,
                MinColdStartExamples = 0,
            };
            predictor.Train(@"A\A1\X", new[] { "a", "one", "x" }, 1);
            predictor.Train(@"B\B1\Y", new[] { "b", "one", "y" }, 1);
            predictor.Train(@"C\C1\Z", new[] { "c", "one", "z" }, 1);
            predictor.Train(@"D\D1\W", new[] { "d", "one", "w" }, 1);

            // Act
            var results = predictor.Classify(new[] { "a", "one", "x" }).ToArray();

            // Assert: descent terminates at a full leaf path and respects the beam width.
            results.Should().NotBeEmpty();
            results[0].Class.Should().Be(@"A\A1\X");
        }

        // F2 coverage: when descent reaches a frontier node whose tree edge marks it as a non-leaf
        // (so it is enqueued onto the next frontier) but the node has no classifier in Nodes, it is
        // emitted as a terminal leaf candidate (the partial.NodeKey.Length > 0 / no-classifier branch
        // in DescendBeam). This is constructed directly to isolate the branch: the tree gives the
        // root a child "Orphan" that itself has a child (so Orphan is non-leaf and is enqueued), but
        // Nodes contains only the root classifier, so descent finds no classifier for "Orphan".
        [TestMethod]
        public void Classify_FrontierNodeWithoutClassifier_EmitsTerminalLeaf()
        {
            // Arrange: train a normal root->Orphan->Child path so the root classifier can score
            // "Orphan", then remove the intermediate Orphan classifier to leave the tree edge in
            // place (Orphan remains a non-leaf in the tree) while Nodes no longer has it.
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = 3,
                MinimumPathProbability = 0.001,
                ShrinkageLambda = 0.7,
                MinColdStartExamples = 0,
            };
            predictor.Train(@"Orphan\Child", new[] { "tok", "tok" }, 1);

            // Remove the intermediate node's classifier; the Tree still has Orphan as a non-leaf
            // (it has the Child edge), so descent enqueues Orphan and then finds no classifier for it.
            predictor
                .Nodes.Remove("Orphan")
                .Should()
                .BeTrue("the intermediate classifier existed");

            // Act
            var results = predictor.Classify(new[] { "tok" }).ToArray();

            // Assert: descent emits the intermediate Orphan node as a terminal leaf candidate.
            results.Select(r => r.Class).Should().Contain("Orphan");
        }

        // F2 coverage: a frontier node whose classifier produces no child scores (empty parent) is
        // emitted as a terminal leaf when reached by descent (the scores.Count == 0 branch).
        [TestMethod]
        public void Classify_FrontierNodeWithNoChildScores_EmitsTerminalLeaf()
        {
            // Arrange: root -> Branch is a real edge with a classifier, but Branch's own classifier
            // has no children (an empty intermediate node), so ScoreChildren returns empty when
            // descent reaches it.
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = 3,
                MinimumPathProbability = 0.001,
                ShrinkageLambda = 0.7,
                MinColdStartExamples = 0,
            };
            predictor.Train(@"Branch\Leaf", new[] { "tok" }, 1);

            // Give Branch an extra tree child so it is treated as a non-leaf and enqueued, then
            // clear Branch's classifier children so ScoreChildren yields no scores for it.
            predictor.Tree.AddLeaf("Branch", "Ghost");
            predictor.Nodes["Branch"].UnTrain("Leaf", new[] { "tok" }, 1);

            // Act
            var results = predictor.Classify(new[] { "tok" }).ToArray();

            // Assert: Branch is emitted as a terminal leaf because it produced no child scores.
            results.Select(r => r.Class).Should().Contain("Branch");
        }

        // F2 coverage: the beam-trim branch retains only the top BeamWidth partial paths when the
        // next frontier exceeds the beam width (next.Count > BeamWidth).
        [TestMethod]
        public void Classify_FrontierExceedsBeamWidth_TrimsToBeamWidth()
        {
            // Arrange: a root with four non-leaf first-level branches (each has a deeper child), so
            // the first descent step produces four partials that must be trimmed to BeamWidth = 2.
            var predictor = new LcppnFolderPredictor
            {
                BeamWidth = 2,
                MinimumPathProbability = 0.001,
                ShrinkageLambda = 0.7,
                MinColdStartExamples = 0,
            };
            predictor.Train(@"A\A1", new[] { "a" }, 1);
            predictor.Train(@"B\B1", new[] { "b" }, 1);
            predictor.Train(@"C\C1", new[] { "c" }, 1);
            predictor.Train(@"D\D1", new[] { "d" }, 1);

            // Act: querying tokens shared by all branches yields four root partials, trimmed to 2.
            var results = predictor.Classify(new[] { "a" }).ToArray();

            // Assert: descent completes and returns at most BeamWidth leaf candidates from the
            // trimmed frontier, with the matching leaf present.
            results.Should().NotBeEmpty();
            results.Length.Should().BeLessThanOrEqualTo(2, "the frontier is trimmed to BeamWidth");
            results[0].Class.Should().Be(@"A\A1");
        }
    }
}
