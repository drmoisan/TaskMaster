using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Unit tests for <see cref="FolderHierarchyTree"/> covering hierarchy construction from
    /// relative paths (AC1), single-segment edge case (AC2), duplicate-path idempotence (AC3),
    /// new-leaf locality (AC4), the empty-collection case, and the configurable comparer. All
    /// tests are deterministic, in-memory, and use no temporary files or Outlook COM.
    /// </summary>
    [TestClass]
    public class FolderHierarchyTree_Tests
    {
        // AC1: multi-depth construction records each adjacent segment pair as a parent->child edge.
        [TestMethod]
        public void Build_MultiDepthPaths_RecordsEachParentChildEdge()
        {
            // Arrange
            var paths = new[] { @"Projects\Alpha\2024", @"Projects\Beta" };

            // Act
            var tree = FolderHierarchyTree.Build(paths);

            // Assert
            tree.GetChildren(FolderHierarchyTree.RootKey).Should().Equal("Projects");
            tree.GetChildren("Projects").Should().BeEquivalentTo("Alpha", "Beta");
            tree.GetChildren(@"Projects\Alpha").Should().Equal("2024");
            tree.IsLeaf(@"Projects\Alpha\2024")
                .Should()
                .BeTrue("the deepest segment has no children");
            tree.IsLeaf(@"Projects\Beta").Should().BeTrue("Beta has no children");
        }

        // AC2: a single-segment path yields exactly one edge root -> segment, and the node is a leaf.
        [TestMethod]
        public void Build_SingleSegmentPath_YieldsOneRootEdgeAndLeaf()
        {
            // Arrange & Act
            var tree = FolderHierarchyTree.Build(new[] { "Inbox" });

            // Assert
            tree.GetChildren(FolderHierarchyTree.RootKey).Should().Equal("Inbox");
            tree.ContainsNode("Inbox").Should().BeTrue();
            tree.IsLeaf("Inbox").Should().BeTrue("a single-segment node has zero children");
            tree.GetChildren("Inbox").Should().BeEmpty();
        }

        // AC3: building from a list with duplicates yields the same node/children sets as the distinct list.
        [TestMethod]
        public void Build_DuplicatePaths_IsIdempotent()
        {
            // Arrange
            var withDuplicates = new[]
            {
                @"Projects\Alpha",
                @"Projects\Alpha",
                @"Projects\Beta",
                @"Projects\Beta",
            };
            var distinct = new[] { @"Projects\Alpha", @"Projects\Beta" };

            // Act
            var fromDuplicates = FolderHierarchyTree.Build(withDuplicates);
            var fromDistinct = FolderHierarchyTree.Build(distinct);

            // Assert
            fromDuplicates.NodeCount.Should().Be(fromDistinct.NodeCount);
            fromDuplicates
                .GetChildren("Projects")
                .Should()
                .Equal(fromDistinct.GetChildren("Projects"));
            fromDuplicates.GetChildren("Projects").Should().Equal("Alpha", "Beta");
        }

        // AC4: adding a new leaf modifies only the target parent's child set; all other nodes are unchanged.
        [TestMethod]
        public void AddLeaf_NewChild_ModifiesOnlyTargetParent()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha", @"Clients\Acme" });
            var preProjectsChildren = tree.GetChildren("Projects");
            var preRootChildren = tree.GetChildren(FolderHierarchyTree.RootKey);
            var preAlphaChildren = tree.GetChildren(@"Projects\Alpha");

            // Act
            var childKey = tree.AddLeaf("Clients", "Beta");

            // Assert: only Clients' child set changed
            childKey.Should().Be(@"Clients\Beta");
            tree.GetChildren("Clients").Should().Equal("Acme", "Beta");
            tree.GetChildren("Projects")
                .Should()
                .Equal(preProjectsChildren, "Projects must be unaffected");
            tree.GetChildren(FolderHierarchyTree.RootKey)
                .Should()
                .Equal(preRootChildren, "the root child set must be unaffected");
            tree.GetChildren(@"Projects\Alpha")
                .Should()
                .Equal(preAlphaChildren, "Alpha must be unaffected");
            tree.IsLeaf(@"Clients\Beta").Should().BeTrue();
        }

        // AC3 boundary: AddLeaf is idempotent for a repeated child segment.
        [TestMethod]
        public void AddLeaf_RepeatedChild_DoesNotDuplicate()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha" });

            // Act
            tree.AddLeaf("Projects", "Alpha");
            tree.AddLeaf("Projects", "Alpha");

            // Assert
            tree.GetChildren("Projects").Should().Equal("Alpha");
        }

        // Empty-collection case: only the synthetic root exists and it is a leaf.
        [TestMethod]
        public void Build_EmptyCollection_ContainsOnlyRoot()
        {
            // Arrange & Act
            var tree = FolderHierarchyTree.Build(Array.Empty<string>());

            // Assert
            tree.NodeCount.Should().Be(1);
            tree.ContainsNode(FolderHierarchyTree.RootKey).Should().BeTrue();
            tree.GetChildren(FolderHierarchyTree.RootKey).Should().BeEmpty();
            tree.IsLeaf(FolderHierarchyTree.RootKey)
                .Should()
                .BeTrue("an empty tree's root has no children");
        }

        // Null / whitespace entries are ignored rather than throwing.
        [TestMethod]
        public void Build_NullOrEmptyEntries_AreSkipped()
        {
            // Arrange & Act
            var tree = FolderHierarchyTree.Build(new[] { null, "", @"Projects\Alpha" });

            // Assert
            tree.GetChildren(FolderHierarchyTree.RootKey).Should().Equal("Projects");
            tree.GetChildren("Projects").Should().Equal("Alpha");
        }

        // Comparer default is ordinal: case variants are distinct nodes.
        [TestMethod]
        public void Build_DefaultOrdinalComparer_TreatsCaseVariantsAsDistinct()
        {
            // Arrange & Act
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha", @"projects\alpha" });

            // Assert
            tree.GetChildren(FolderHierarchyTree.RootKey)
                .Should()
                .BeEquivalentTo("Projects", "projects");
        }

        // Configured OrdinalIgnoreCase comparer collapses case variants into one node.
        [TestMethod]
        public void Build_OrdinalIgnoreCaseComparer_CollapsesCaseVariants()
        {
            // Arrange & Act
            var tree = FolderHierarchyTree.Build(
                new[] { @"Projects\Alpha", @"projects\alpha" },
                StringComparer.OrdinalIgnoreCase
            );

            // Assert
            tree.GetChildren(FolderHierarchyTree.RootKey)
                .Should()
                .HaveCount(1, "case-insensitive comparison merges Projects and projects");
        }

        // GetNode exposes the immutable snapshot record; absent nodes return null.
        [TestMethod]
        public void GetNode_KnownAndUnknownKeys_BehaveAsContracted()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha" });

            // Act
            var node = tree.GetNode("Projects");
            var missing = tree.GetNode("DoesNotExist");

            // Assert
            node.Should().NotBeNull();
            node!.NodeKey.Should().Be("Projects");
            node.Children.Should().Equal("Alpha");
            missing.Should().BeNull();
        }

        // Build with a null source fails fast.
        [TestMethod]
        public void Build_NullSource_Throws()
        {
            // Act
            var act = () => FolderHierarchyTree.Build(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        // AddLeaf with an empty child segment fails fast.
        [TestMethod]
        public void AddLeaf_EmptyChildSegment_Throws()
        {
            // Arrange
            var tree = new FolderHierarchyTree();

            // Act
            var act = () => tree.AddLeaf(FolderHierarchyTree.RootKey, "");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        // F2 coverage: AddLeaf with a null parent key fails fast (the null-parent guard branch).
        [TestMethod]
        public void AddLeaf_NullParentKey_Throws()
        {
            // Arrange
            var tree = new FolderHierarchyTree();

            // Act
            var act = () => tree.AddLeaf(null, "Child");

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("parentKey", "a null parent key is rejected");
        }

        // F2 coverage: a path consisting only of separators splits to zero non-empty segments and is
        // ignored, leaving the tree with only the synthetic root (the segments.Length == 0 branch).
        [TestMethod]
        public void AddPath_SeparatorsOnly_IsIgnored()
        {
            // Arrange
            var tree = new FolderHierarchyTree();

            // Act
            tree.AddPath(@"\\\");

            // Assert
            tree.NodeCount.Should()
                .Be(1, "a separators-only path yields no segments and no edges");
            tree.GetChildren(FolderHierarchyTree.RootKey).Should().BeEmpty();
        }

        // F2 coverage: GetChildren returns an empty array for a null key and for an unknown node
        // (the null-or-missing early return branch), and the direct children for a known parent.
        [TestMethod]
        public void GetChildren_NullAndUnknownKeys_ReturnEmptyArray()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha" });

            // Act
            var nullResult = tree.GetChildren(null);
            var unknownResult = tree.GetChildren("DoesNotExist");
            var knownResult = tree.GetChildren("Projects");

            // Assert
            nullResult.Should().BeEmpty("a null key has no children");
            unknownResult.Should().BeEmpty("an unknown node has no children");
            knownResult.Should().Equal("Alpha");
        }

        // F2 coverage: NodeKeys returns every node key including the synthetic root.
        [TestMethod]
        public void NodeKeys_PopulatedTree_ReturnsAllNodeKeysIncludingRoot()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha", "Clients" });

            // Act
            var keys = tree.NodeKeys;

            // Assert
            keys.Should()
                .BeEquivalentTo(
                    new[] { FolderHierarchyTree.RootKey, "Projects", @"Projects\Alpha", "Clients" },
                    "every node, including the synthetic root, is enumerated"
                );
        }

        // F2 coverage: GetNode returns null for a null key (the null branch of the guard).
        [TestMethod]
        public void GetNode_NullKey_ReturnsNull()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha" });

            // Act
            var node = tree.GetNode(null);

            // Assert
            node.Should().BeNull("a null key resolves to no node");
        }

        // F2 coverage: IsLeaf is false for a non-existent node and for a node that has children
        // (the two false branches), and true for an existing childless node.
        [TestMethod]
        public void IsLeaf_NonExistentAndParentNodes_ReturnFalse()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha" });

            // Act & Assert
            tree.IsLeaf("DoesNotExist").Should().BeFalse("a missing node is not a leaf");
            tree.IsLeaf(null).Should().BeFalse("a null key is not a leaf");
            tree.IsLeaf("Projects").Should().BeFalse("a node with children is not a leaf");
            tree.IsLeaf(@"Projects\Alpha").Should().BeTrue("a childless existing node is a leaf");
        }

        // F2 coverage: ContainsNode is false for a null key and an unknown key, true for a known key.
        [TestMethod]
        public void ContainsNode_NullUnknownAndKnownKeys_BehaveAsContracted()
        {
            // Arrange
            var tree = FolderHierarchyTree.Build(new[] { @"Projects\Alpha" });

            // Act & Assert
            tree.ContainsNode(null).Should().BeFalse("a null key is not present");
            tree.ContainsNode("DoesNotExist").Should().BeFalse("an unknown key is not present");
            tree.ContainsNode("Projects").Should().BeTrue("a known key is present");
        }
    }
}
