using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for <see cref="UtilitiesCS.FolderHierarchyBuilder"/>: suggestion-path splitting with
    /// find-or-add ancestor synthesis, leaf-only probability placement, non-suggestion rows as
    /// depth-0 leaves preserving text and order, DisplayName segmentation, and full-path key retention.
    /// </summary>
    [TestClass]
    public class FolderHierarchyBuilderTests
    {
        private static FolderRow Suggestion(string path, double probability)
        {
            return new FolderRow(
                path,
                FolderRowKind.Suggestion,
                new FolderScore(path, 0, probability)
            );
        }

        private static FolderRow NonSuggestion(string text, FolderRowKind kind)
        {
            return new FolderRow(text, kind, null);
        }

        [TestMethod]
        public void Build_MultiSegmentSuggestion_SynthesizesAncestorAndAttachesLeafProbability()
        {
            // Arrange
            var rows = new List<FolderRow> { Suggestion("Archive\\Finance", 0.85) };

            // Act
            var forest = new FolderHierarchyBuilder().Build(rows);

            // Assert: single root ancestor "Archive", one leaf child "Finance".
            forest.Should().HaveCount(1);
            var archive = forest[0].Value;
            archive.FolderPath.Should().Be("Archive");
            archive.DisplayName.Should().Be("Archive");
            archive.Probability.Should().BeNull("synthesized ancestors carry no probability");
            archive.HasChildren.Should().BeTrue();
            archive.Depth.Should().Be(0);

            forest[0].Children.Should().HaveCount(1);
            var finance = forest[0].Children[0].Value;
            finance
                .FolderPath.Should()
                .Be("Archive\\Finance", "full path is retained as the node key");
            finance.DisplayName.Should().Be("Finance", "DisplayName is the last path segment");
            finance.Probability.Should().Be(0.85, "probability attaches at the full-folder leaf");
            finance.HasChildren.Should().BeFalse();
            finance.Depth.Should().Be(1);
        }

        [TestMethod]
        public void Build_SiblingSuggestions_ShareFindOrAddAncestor()
        {
            // Arrange
            var rows = new List<FolderRow> { Suggestion("A\\B", 0.9), Suggestion("A\\C", 0.8) };

            // Act
            var forest = new FolderHierarchyBuilder().Build(rows);

            // Assert: one shared ancestor "A" with two leaf children.
            forest.Should().HaveCount(1);
            forest[0].Value.FolderPath.Should().Be("A");
            forest[0].Value.HasChildren.Should().BeTrue();
            forest[0]
                .Children.Select(c => c.Value.FolderPath)
                .Should()
                .Equal(new[] { "A\\B", "A\\C" });
            forest[0].Children[0].Value.Probability.Should().Be(0.9);
            forest[0].Children[1].Value.Probability.Should().Be(0.8);
        }

        [TestMethod]
        public void Build_NonSuggestionRows_AreDepthZeroLeavesWithNoProbability()
        {
            // Arrange
            var rows = new List<FolderRow>
            {
                NonSuggestion("========= SUGGESTIONS =========", FolderRowKind.Separator),
                NonSuggestion("Inbox\\Search Hit", FolderRowKind.SearchResult),
                NonSuggestion("Recent Folder", FolderRowKind.Recent),
            };

            // Act
            var forest = new FolderHierarchyBuilder().Build(rows);

            // Assert: three depth-0 leaf roots, text preserved verbatim, no probability, no children.
            forest.Should().HaveCount(3);
            forest
                .Select(n => n.Value.FolderPath)
                .Should()
                .Equal(
                    new[]
                    {
                        "========= SUGGESTIONS =========",
                        "Inbox\\Search Hit",
                        "Recent Folder",
                    }
                );
            forest.Should().OnlyContain(n => n.Value.Probability == null);
            forest.Should().OnlyContain(n => n.Value.HasChildren == false);
            forest.Should().OnlyContain(n => n.Value.Depth == 0);
            forest.Should().OnlyContain(n => n.Children.Count == 0);
            // DisplayName preserves the verbatim text (not split) for non-suggestion rows.
            forest[1].Value.DisplayName.Should().Be("Inbox\\Search Hit");
        }

        [TestMethod]
        public void Build_MixedRows_PreservesInputOrderInForest()
        {
            // Arrange
            var rows = new List<FolderRow>
            {
                NonSuggestion("===== SUGGESTIONS =====", FolderRowKind.Separator),
                Suggestion("A\\B", 0.9),
                NonSuggestion("Recent", FolderRowKind.Recent),
            };

            // Act
            var forest = new FolderHierarchyBuilder().Build(rows);

            // Assert: separator root, suggestion ancestor root "A", recent root — in input order.
            forest
                .Select(n => n.Value.FolderPath)
                .Should()
                .Equal(new[] { "===== SUGGESTIONS =====", "A", "Recent" });
        }

        [TestMethod]
        public void Build_SingleSegmentSuggestion_IsDepthZeroLeafWithProbability()
        {
            // Arrange
            var rows = new List<FolderRow> { Suggestion("Inbox", 0.5) };

            // Act
            var forest = new FolderHierarchyBuilder().Build(rows);

            // Assert
            forest.Should().HaveCount(1);
            forest[0].Value.FolderPath.Should().Be("Inbox");
            forest[0].Value.DisplayName.Should().Be("Inbox");
            forest[0].Value.Probability.Should().Be(0.5);
            forest[0].Value.HasChildren.Should().BeFalse();
            forest[0].Value.Depth.Should().Be(0);
        }
    }
}
