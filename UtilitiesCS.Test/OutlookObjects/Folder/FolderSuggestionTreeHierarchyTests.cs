using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Hierarchy-building tests for <see cref="UtilitiesCS.FolderSuggestionTree.BuildFromRows"/>.
    /// Cover section roots, nested children by longest-present prefix, a deep path without its parent
    /// present, banner classification and per-section isolation, empty input, a single node, and null
    /// input. The model is pure and host-neutral, so no Outlook/COM mocking is required.
    /// </summary>
    [TestClass]
    public class FolderSuggestionTreeHierarchyTests
    {
        private const string SuggestionsBanner = "========= SUGGESTIONS =========";
        private const string SearchBanner = "======= SEARCH RESULTS =======";

        [TestMethod]
        public void BuildFromRows_WithNestedPaths_EstablishesParentChildEdgesByLongestPrefix()
        {
            // Arrange
            var rows = new[]
            {
                SuggestionsBanner,
                "Archive\\Projects",
                "Archive\\Projects\\FY26",
                "Archive\\Projects\\FY26\\Q1",
                "Archive\\Other",
            };

            // Act
            var tree = FolderSuggestionTree.BuildFromRows(rows);

            // Assert: top-level roots are the banner plus the two section-root folders, in order.
            tree.Roots.Select(r => r.FullPath)
                .Should()
                .Equal(SuggestionsBanner, "Archive\\Projects", "Archive\\Other");

            var projects = tree.Roots[1];
            projects.Kind.Should().Be(FolderSuggestionNodeKind.Folder);
            projects.HasChildren.Should().BeTrue();
            projects.Depth.Should().Be(0);
            projects.DisplayName.Should().Be("Projects");

            var fy26 = projects.Children.Single();
            fy26.FullPath.Should().Be("Archive\\Projects\\FY26");
            fy26.DisplayName.Should().Be("FY26");
            fy26.Depth.Should().Be(1);
            fy26.HasChildren.Should().BeTrue();

            var q1 = fy26.Children.Single();
            q1.FullPath.Should().Be("Archive\\Projects\\FY26\\Q1");
            q1.DisplayName.Should().Be("Q1");
            q1.Depth.Should().Be(2);
            q1.HasChildren.Should().BeFalse();

            tree.Roots[2].HasChildren.Should().BeFalse();
        }

        [TestMethod]
        public void BuildFromRows_WithDeepPathWithoutParent_ProducesSectionRootNoAncestorSynthesis()
        {
            // Arrange: only the deep path is presented; its ancestors A and A\B are not.
            var rows = new[] { SuggestionsBanner, "A\\B\\C" };

            // Act
            var tree = FolderSuggestionTree.BuildFromRows(rows);

            // Assert: no synthesized ancestor nodes; the deep path renders at the section root.
            tree.Roots.Select(r => r.FullPath).Should().Equal(SuggestionsBanner, "A\\B\\C");
            var deep = tree.Roots[1];
            deep.Kind.Should().Be(FolderSuggestionNodeKind.Folder);
            deep.DisplayName.Should().Be("C");
            deep.Depth.Should().Be(0);
            deep.HasChildren.Should().BeFalse();
        }

        [TestMethod]
        public void BuildFromRows_ClassifiesBannerRowsAndIsolatesEdgesPerSection()
        {
            // Arrange: an identical prefix appears in two different sections; edges must not cross a banner.
            var rows = new[] { SearchBanner, "Foo\\Bar", SuggestionsBanner, "Foo\\Bar\\Baz" };

            // Act
            var tree = FolderSuggestionTree.BuildFromRows(rows);

            // Assert: four top-level nodes; the deep suggestion is NOT parented to the search-section prefix.
            tree.Roots.Should().HaveCount(4);
            tree.Roots[0].Kind.Should().Be(FolderSuggestionNodeKind.Banner);
            tree.Roots[0].DisplayName.Should().Be(SearchBanner);
            tree.Roots[0].HasChildren.Should().BeFalse();

            tree.Roots[1].FullPath.Should().Be("Foo\\Bar");
            tree.Roots[1].HasChildren.Should().BeFalse();

            tree.Roots[2].Kind.Should().Be(FolderSuggestionNodeKind.Banner);
            tree.Roots[3].FullPath.Should().Be("Foo\\Bar\\Baz");
            tree.Roots[3].Kind.Should().Be(FolderSuggestionNodeKind.Folder);
            tree.Roots[3].HasChildren.Should().BeFalse();
        }

        [TestMethod]
        public void BuildFromRows_PreservesPerSectionInputOrderForSiblingRoots()
        {
            // Arrange: three unrelated sibling roots in a deliberate, non-alphabetical order.
            var rows = new[] { SuggestionsBanner, "Zeta", "Alpha", "Mike" };

            // Act
            var tree = FolderSuggestionTree.BuildFromRows(rows);

            // Assert: input order is preserved (no re-sorting of suggestions).
            tree.Roots.Select(r => r.FullPath)
                .Should()
                .Equal(SuggestionsBanner, "Zeta", "Alpha", "Mike");
        }

        [TestMethod]
        public void BuildFromRows_WithEmptyInput_ReturnsEmptyTree()
        {
            // Act
            var tree = FolderSuggestionTree.BuildFromRows(Array.Empty<string>());

            // Assert
            tree.Roots.Should().BeEmpty();
        }

        [TestMethod]
        public void BuildFromRows_WithSingleFolderRow_ReturnsSingleRootLeaf()
        {
            // Act
            var tree = FolderSuggestionTree.BuildFromRows(new[] { "SoloFolder" });

            // Assert: an unseparated path uses the whole string as its display name.
            tree.Roots.Should().HaveCount(1);
            var only = tree.Roots.Single();
            only.FullPath.Should().Be("SoloFolder");
            only.DisplayName.Should().Be("SoloFolder");
            only.Kind.Should().Be(FolderSuggestionNodeKind.Folder);
            only.HasChildren.Should().BeFalse();
            only.Depth.Should().Be(0);
        }

        [TestMethod]
        public void BuildFromRows_WithNullInput_ReturnsEmptyTree()
        {
            // Act
            var tree = FolderSuggestionTree.BuildFromRows(null);

            // Assert
            tree.Roots.Should().BeEmpty();
        }
    }
}
