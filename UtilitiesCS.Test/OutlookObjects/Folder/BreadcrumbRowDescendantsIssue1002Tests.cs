#nullable enable
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue 1002 row state: the keyed descendant tree on <see cref="BreadcrumbRow"/>, its
    /// identity-addressed transitions, the request-generation stale guard, and the reset points
    /// shared with the first-level children.
    /// </summary>
    [TestClass]
    public class BreadcrumbRowDescendantsIssue1002Tests
    {
        private const string North = @"\Archive\Clients\North";
        private const string Q3 = North + @"\Q3";
        private const string Q4 = North + @"\Q4";
        private const string UnknownId = "ffffffffffffffffffffffffffffffff";

        [TestMethod]
        public void SetLeafDescendants_KeyedChildren_BuildsOneNodePerChild()
        {
            // Arrange
            BreadcrumbRow row = ExpandableRow();

            // Act
            bool stored = row.SetLeafDescendants(FirstLevel());

            // Assert
            stored.Should().BeTrue("the active leaf has subfolders");
            row.DescendantNodes.Select(node => node.Segment.Key)
                .Should()
                .Equal(new[] { Key(Q3), Key(Q4) }, "one node per keyed child, in provider order");
            row.LeafChildren.Select(child => child.FullPath)
                .Should()
                .Equal(new[] { Q3, Q4 }, "the key-less projection stays populated");
        }

        [TestMethod]
        public void SetLeafDescendants_DuplicateDisplayNames_KeepsDistinctIdentities()
        {
            // Arrange
            BreadcrumbRow row = ExpandableRow();
            var children = new[]
            {
                Segment(North + @"\A\Reports", "Reports", false),
                Segment(North + @"\B\Reports", "Reports", false),
            };

            // Act
            row.SetLeafDescendants(children);

            // Assert
            IReadOnlyList<BreadcrumbDescendantNode> nodes = row.DescendantNodes;
            nodes.Should().HaveCount(2);
            nodes[0].Identity.Should().NotBe(nodes[1].Identity, "identities are per node");
            row.GetDescendantPath(nodes[0].Identity).Should().Be(North + @"\A\Reports");
            row.GetDescendantPath(nodes[1].Identity).Should().Be(North + @"\B\Reports");
        }

        [TestMethod]
        public void SetLeafDescendants_RowWithoutExpandableLeaf_ReturnsFalse()
        {
            // Arrange
            var row = new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[] { new BreadcrumbSegment(North, "North", false) },
                null
            );

            // Act
            bool stored = row.SetLeafDescendants(FirstLevel());

            // Assert
            stored.Should().BeFalse("a leaf without subfolders cannot hold descendants");
            row.DescendantNodes.Should().BeEmpty();
        }

        [TestMethod]
        public void SetLeafDescendants_NullChildren_Throws()
        {
            // Arrange
            BreadcrumbRow row = ExpandableRow();

            // Act
            System.Action act = () => row.SetLeafDescendants(null!);

            // Assert
            act.Should().Throw<System.ArgumentNullException>().WithParameterName("children");
        }

        [TestMethod]
        public void TryBeginDescendantExpand_KnownNodeWithChildren_ReturnsKeyAndGeneration()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);

            // Act
            bool begun = row.TryBeginDescendantExpand(q3, out FolderTreeNodeKey? key, out int gen);

            // Assert
            begun.Should().BeTrue("Q3 is known and has children");
            key.Should().Be(Key(Q3), "the provider is queried with the node's own key");
            gen.Should().Be(1, "the first request advances the generation from zero");
            row.DescendantNodes[0].Expanded.Should().BeTrue();
        }

        [TestMethod]
        public void TryBeginDescendantExpand_UnknownNode_ReturnsFalse()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();

            // Act
            bool begun = row.TryBeginDescendantExpand(UnknownId, out FolderTreeNodeKey? key, out _);

            // Assert
            begun.Should().BeFalse("an identity the row never issued is rejected");
            key.Should().BeNull();
        }

        [TestMethod]
        public void TryBeginDescendantExpand_LeafNode_ReturnsFalse()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();

            // Act
            bool begun = row.TryBeginDescendantExpand(IdOf(row, Q4), out _, out _);

            // Assert
            begun.Should().BeFalse("a node without children has nothing to expand");
            row.DescendantNodes[1].Expanded.Should().BeFalse();
        }

        [TestMethod]
        public void TryCollapseDescendant_ExpandedNode_CollapsesIt()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out _).Should().BeTrue();

            // Act
            bool collapsed = row.TryCollapseDescendant(q3);

            // Assert
            collapsed.Should().BeTrue();
            row.DescendantNodes[0].Expanded.Should().BeFalse();
        }

        [TestMethod]
        public void TryCollapseDescendant_CollapsedNode_ReturnsFalse()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();

            // Act
            bool collapsed = row.TryCollapseDescendant(IdOf(row, Q3));

            // Assert
            collapsed.Should().BeFalse("a collapsed node is not collapsed again");
        }

        [TestMethod]
        public void TryAttachDescendantChildren_MatchingGeneration_AttachesChildren()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int gen).Should().BeTrue();

            // Act
            bool attached = row.TryAttachDescendantChildren(q3, gen, SecondLevel());

            // Assert
            attached.Should().BeTrue();
            row.DescendantNodes[0]
                .Children.Select(node => node.Segment.FolderPath)
                .Should()
                .Equal(Q3 + @"\Invoices", Q3 + @"\Receipts");
        }

        [TestMethod]
        public void TryAttachDescendantChildren_SupersededGeneration_IsRejected()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int first).Should().BeTrue();
            row.TryCollapseDescendant(q3).Should().BeTrue();
            row.TryBeginDescendantExpand(q3, out _, out int second).Should().BeTrue();

            // Act
            bool attached = row.TryAttachDescendantChildren(q3, first, SecondLevel());

            // Assert
            attached.Should().BeFalse("a later request superseded the first generation");
            second.Should().BeGreaterThan(first);
            row.DescendantNodes[0].Children.Should().BeEmpty();
        }

        [TestMethod]
        public void TryAttachDescendantChildren_CollapsedNode_IsRejected()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int gen).Should().BeTrue();
            row.TryCollapseDescendant(q3).Should().BeTrue();

            // Act
            bool attached = row.TryAttachDescendantChildren(q3, gen, SecondLevel());

            // Assert
            attached.Should().BeFalse("the node was collapsed while the fetch was pending");
            row.DescendantNodes[0].Children.Should().BeEmpty();
        }

        [TestMethod]
        public void TryAttachDescendantChildren_RemovedNode_IsRejected()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int gen).Should().BeTrue();
            row.SetLeafDescendants(FirstLevel()).Should().BeTrue();

            // Act
            bool attached = row.TryAttachDescendantChildren(q3, gen, SecondLevel());

            // Assert
            attached.Should().BeFalse("the rebuilt tree no longer contains the old node");
        }

        [TestMethod]
        public void TryAbandonDescendantExpand_CurrentGeneration_CollapsesNode()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int gen).Should().BeTrue();

            // Act
            bool abandoned = row.TryAbandonDescendantExpand(q3, gen);

            // Assert
            abandoned.Should().BeTrue();
            row.DescendantNodes[0].Expanded.Should().BeFalse("a failed fetch leaves no expansion");
            row.TryBeginDescendantExpand(q3, out _, out _).Should().BeTrue("a retry is allowed");
        }

        [TestMethod]
        public void TryAbandonDescendantExpand_SupersededGeneration_LeavesNodeUnchanged()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int first).Should().BeTrue();
            row.TryCollapseDescendant(q3).Should().BeTrue();
            row.TryBeginDescendantExpand(q3, out _, out _).Should().BeTrue();

            // Act
            bool abandoned = row.TryAbandonDescendantExpand(q3, first);

            // Assert
            abandoned.Should().BeFalse("only the current request may abandon the expansion");
            row.DescendantNodes[0].Expanded.Should().BeTrue();
        }

        [TestMethod]
        public void GetDescendantPath_KnownNode_ReturnsFolderPath()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();

            // Act
            string? path = row.GetDescendantPath(IdOf(row, Q4));

            // Assert
            path.Should().Be(Q4);
        }

        [TestMethod]
        public void GetDescendantPath_UnknownNode_ReturnsNull()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();

            // Act
            string? path = row.GetDescendantPath(UnknownId);

            // Assert
            path.Should().BeNull("an unknown identity resolves to no path");
        }

        [TestMethod]
        public void TrySelectDescendant_KnownNode_SetsSelectedDescendantId()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q4 = IdOf(row, Q4);

            // Act
            bool selected = row.TrySelectDescendant(q4);

            // Assert
            selected.Should().BeTrue();
            row.SelectedDescendantId.Should().Be(q4);
            row.TrySelectDescendant(null).Should().BeTrue("null clears the selection");
            row.SelectedDescendantId.Should().BeNull();
        }

        [TestMethod]
        public void TrySelectDescendant_UnknownNode_LeavesSelectionUnchanged()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q4 = IdOf(row, Q4);
            row.TrySelectDescendant(q4).Should().BeTrue();

            // Act
            bool selected = row.TrySelectDescendant(UnknownId);

            // Assert
            selected.Should().BeFalse();
            row.SelectedDescendantId.Should().Be(q4, "a rejected identity keeps the prior one");
        }

        [TestMethod]
        public void ActivateSegment_ClearsDescendantsAndSelection()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            row.SetSegmentKey(0, Key(@"\Archive\Clients"));
            row.TrySelectDescendant(IdOf(row, Q4)).Should().BeTrue();

            // Act
            bool activated = row.ActivateSegment(0);

            // Assert
            activated.Should().BeTrue();
            row.DescendantNodes.Should().BeEmpty("the active segment changed");
            row.SelectedDescendantId.Should().BeNull();
        }

        [TestMethod]
        public void CollapseAfter_ClearsDescendantsAndSelection()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            row.TrySelectDescendant(IdOf(row, Q4)).Should().BeTrue();

            // Act
            bool collapsed = row.CollapseAfter(0);

            // Assert
            collapsed.Should().BeTrue();
            row.DescendantNodes.Should().BeEmpty("the leaf children are hidden");
            row.SelectedDescendantId.Should().BeNull();
        }

        [TestMethod]
        public void LeftArrow_ExpandedLeaf_ClearsDescendantsAndSelection()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            row.TrySelectDescendant(IdOf(row, Q4)).Should().BeTrue();

            // Act
            bool changed = row.LeftArrow();

            // Assert
            changed.Should().BeTrue();
            row.IsLeafExpanded.Should().BeFalse();
            row.DescendantNodes.Should().BeEmpty("the leaf children were collapsed");
            row.SelectedDescendantId.Should().BeNull();
        }

        [TestMethod]
        public void ToggleLeafExpanded_Collapse_ClearsDescendantsAndSelection()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            row.TrySelectDescendant(IdOf(row, Q4)).Should().BeTrue();

            // Act
            bool changed = row.ToggleLeafExpanded();

            // Assert
            changed.Should().BeTrue();
            row.IsLeafExpanded.Should().BeFalse();
            row.DescendantNodes.Should().BeEmpty("the leaf children were collapsed");
            row.SelectedDescendantId.Should().BeNull();
        }

        [TestMethod]
        public void SetLeafChildren_KeylessChildren_HasNoDescendants()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();

            // Act
            bool stored = row.SetLeafChildren(new[] { new BreadcrumbSegment(Q3, "Q3", true) });

            // Assert
            stored.Should().BeTrue();
            row.DescendantNodes.Should().BeEmpty("key-less children carry no expandable nodes");
            row.SelectedDescendantId.Should().BeNull();
        }

        private static BreadcrumbRow ExpandableRow() =>
            new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[]
                {
                    new BreadcrumbSegment(@"\Archive\Clients", "Clients", true),
                    new BreadcrumbSegment(North, "North", true),
                },
                null
            );

        // A row whose expanded leaf holds the keyed first level Q3 (has children) and Q4 (leaf).
        private static BreadcrumbRow RowWithTree()
        {
            BreadcrumbRow row = ExpandableRow();
            row.SetLeafDescendants(FirstLevel()).Should().BeTrue("the fixture needs a keyed tree");
            row.ToggleLeafExpanded().Should().BeTrue();
            return row;
        }

        private static string IdOf(BreadcrumbRow row, string path)
        {
            BreadcrumbDescendantNode? node = row.DescendantNodes.FirstOrDefault(candidate =>
                candidate.Segment.FolderPath == path
            );
            node.Should().NotBeNull($"the keyed tree must contain {path}");
            return node!.Identity;
        }

        private static FolderBreadcrumbSegment[] FirstLevel() =>
            new[] { Segment(Q3, "Q3", true), Segment(Q4, "Q4", false) };

        private static FolderBreadcrumbSegment[] SecondLevel() =>
            new[]
            {
                Segment(Q3 + @"\Invoices", "Invoices", false),
                Segment(Q3 + @"\Receipts", "Receipts", true),
            };

        private static FolderTreeNodeKey Key(string path) =>
            new FolderTreeNodeKey("store", path, path);

        private static FolderBreadcrumbSegment Segment(
            string path,
            string name,
            bool hasChildren
        ) => new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
    }
}
