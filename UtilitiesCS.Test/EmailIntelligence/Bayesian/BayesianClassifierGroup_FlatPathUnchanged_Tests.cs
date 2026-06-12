using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Flat-path regression tests (AC13): trains a small fixed corpus and asserts that
    /// <see cref="BayesianClassifierGroup.Classify(string[])"/> ordering and probabilities
    /// are unchanged by the additive <see cref="IFolderPredictor"/> declaration. Behavior
    /// equality is proven by comparing the base-class call path against the interface call
    /// path on identically trained instances, plus determinism across repeated calls.
    /// </summary>
    [TestClass]
    public class BayesianClassifierGroup_FlatPathUnchanged_Tests
    {
        private static readonly string[] InboxTokens = ["invoice", "payment", "due", "invoice"];
        private static readonly string[] ArchiveTokens =
        [
            "newsletter",
            "weekly",
            "digest",
            "newsletter",
        ];

        /// <summary>
        /// Builds a deterministic two-folder classifier group from the fixed corpus,
        /// mirroring the production build path (shared token base + per-tag Train).
        /// </summary>
        private static BayesianClassifierGroup CreateTrainedGroup()
        {
            var group = new BayesianClassifierGroup
            {
                TotalEmailCount = 2,
                SharedTokenBase = new Corpus(InboxTokens.Concat(ArchiveTokens)),
            };
            group.Train("Inbox", InboxTokens, 1);
            group.Train("Archive", ArchiveTokens, 1);
            return group;
        }

        [TestMethod]
        public void Classify_FixedCorpus_RanksMatchingFolderFirstInDescendingOrder()
        {
            // Arrange
            var group = CreateTrainedGroup();

            // Act
            var results = group.Classify(InboxTokens).ToArray();

            // Assert: flat behavior — matching folder ranks first, ordering is descending
            results.Should().NotBeEmpty();
            results[0].Class.Should().Be("Inbox", "the query tokens match the Inbox corpus");
            results
                .Select(p => p.Probability)
                .Should()
                .BeInDescendingOrder("Classify orders by descending probability");
        }

        [TestMethod]
        public void Classify_FixedCorpus_IsDeterministicAcrossIdenticalInstances()
        {
            // Arrange: two independently constructed, identically trained groups
            var first = CreateTrainedGroup();
            var second = CreateTrainedGroup();

            // Act
            var firstResults = first.Classify(InboxTokens).ToArray();
            var secondResults = second.Classify(InboxTokens).ToArray();

            // Assert: identical ordering and probabilities given identical training
            firstResults.Select(p => p.Class).Should().Equal(secondResults.Select(p => p.Class));
            firstResults
                .Select(p => p.Probability)
                .Should()
                .Equal(secondResults.Select(p => p.Probability));
        }

        [TestMethod]
        public void Classify_BaseClassPathAndInterfacePath_ProduceIdenticalOutput()
        {
            // Arrange: same instance queried directly and through the IFolderPredictor seam
            var group = CreateTrainedGroup();
            IFolderPredictor seam = group;

            // Act
            var direct = group.Classify(ArchiveTokens).ToArray();
            var viaInterface = seam.Classify(ArchiveTokens).ToArray();

            // Assert: the additive interface declaration does not alter flat output
            viaInterface.Select(p => p.Class).Should().Equal(direct.Select(p => p.Class));
            viaInterface
                .Select(p => p.Probability)
                .Should()
                .Equal(direct.Select(p => p.Probability));
            direct[0].Class.Should().Be("Archive", "the query tokens match the Archive corpus");
        }

        [TestMethod]
        public void Classify_RepeatedCalls_ReturnIdenticalResults()
        {
            // Arrange
            var group = CreateTrainedGroup();

            // Act
            var firstCall = group.Classify(InboxTokens).ToArray();
            var secondCall = group.Classify(InboxTokens).ToArray();

            // Assert: classification is read-only and repeatable
            firstCall.Select(p => p.Class).Should().Equal(secondCall.Select(p => p.Class));
            firstCall
                .Select(p => p.Probability)
                .Should()
                .Equal(secondCall.Select(p => p.Probability));
        }
    }
}
