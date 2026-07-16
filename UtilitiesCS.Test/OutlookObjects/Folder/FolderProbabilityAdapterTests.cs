using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for <see cref="UtilitiesCS.FolderProbabilityAdapter"/>: the path-to-probability join by
    /// full-path string equality (matched folder rows carry the probability, unmatched folder rows
    /// stay null, banner rows are never queried and stay null), nested-node coverage, and the
    /// constructor/argument guards. The upstream seam is a Moq <see cref="IFolderProbabilitySource"/>.
    /// </summary>
    [TestClass]
    public class FolderProbabilityAdapterTests
    {
        private const string Banner = "========= SUGGESTIONS =========";

        [TestMethod]
        public void Apply_AssignsProbabilityToMatchedFolderRow_AndLeavesUnmatchedNull()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { Banner, "Root", "Other" });
            var source = new Mock<IFolderProbabilitySource>(MockBehavior.Strict);
            double rootProbability = 0.9;
            source.Setup(s => s.TryGetProbability("Root", out rootProbability)).Returns(true);
            double otherProbability = 0.0;
            source.Setup(s => s.TryGetProbability("Other", out otherProbability)).Returns(false);
            var adapter = new FolderProbabilityAdapter(source.Object);

            // Act
            adapter.Apply(tree);

            // Assert
            tree.Roots.Single(n => n.FullPath == "Root").Probability.Should().Be(0.9);
            tree.Roots.Single(n => n.FullPath == "Other").Probability.Should().BeNull();
        }

        [TestMethod]
        public void Apply_NeverQueriesBannerRows_AndLeavesBannerProbabilityNull()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { Banner, "Root" });
            var source = new Mock<IFolderProbabilitySource>(MockBehavior.Strict);
            double rootProbability = 0.5;
            source.Setup(s => s.TryGetProbability("Root", out rootProbability)).Returns(true);
            var adapter = new FolderProbabilityAdapter(source.Object);

            // Act
            adapter.Apply(tree);

            // Assert: banner untouched, and the source was never asked about the banner text.
            tree.Roots[0].Kind.Should().Be(FolderSuggestionNodeKind.Banner);
            tree.Roots[0].Probability.Should().BeNull();
            double ignored;
            source.Verify(s => s.TryGetProbability(Banner, out ignored), Times.Never);
        }

        [TestMethod]
        public void Apply_AssignsProbabilityToNestedChildNodes()
        {
            // Arrange
            var tree = FolderSuggestionTree.BuildFromRows(new[] { Banner, "Root", "Root\\Child" });
            var source = new Mock<IFolderProbabilitySource>(MockBehavior.Strict);
            double rootProbability = 0.8;
            source.Setup(s => s.TryGetProbability("Root", out rootProbability)).Returns(true);
            double childProbability = 0.4;
            source
                .Setup(s => s.TryGetProbability("Root\\Child", out childProbability))
                .Returns(true);
            var adapter = new FolderProbabilityAdapter(source.Object);

            // Act
            adapter.Apply(tree);

            // Assert: the nested child (not a root) also receives its probability.
            var root = tree.Roots.Single(n => n.FullPath == "Root");
            root.Probability.Should().Be(0.8);
            root.Children.Single().Probability.Should().Be(0.4);
        }

        [TestMethod]
        public void Constructor_NullSource_Throws()
        {
            Action act = () => new FolderProbabilityAdapter(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Apply_NullTree_Throws()
        {
            var source = new Mock<IFolderProbabilitySource>();
            var adapter = new FolderProbabilityAdapter(source.Object);

            Action act = () => adapter.Apply(null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
