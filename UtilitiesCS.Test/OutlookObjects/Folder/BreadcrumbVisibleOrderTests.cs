#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007 AC1, AC2, AC11: the shared visible order (rows plus shown descendants in
    /// pre-order) and its pure index operations, built from hand-made position lists, an EFC
    /// <see cref="BreadcrumbRow"/> and a QFC <see cref="BreadcrumbStateModel"/>.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbVisibleOrderTests
    {
        private const string Clients = @"\Archive\Clients";
        private const string North = Clients + @"\North";
        private const string Q3 = North + @"\Q3";
        private const string Q4 = North + @"\Q4";

        /// <summary>AC1: Down from an expanded row visits its descendants in pre-order, then the next row.</summary>
        [TestMethod]
        public void Step_DownFromExpandedRow_VisitsDescendantsInPreOrder()
        {
            // Arrange
            var order = HandOrder();
            var visited = new List<string?>();

            // Act
            for (int index = 0; index >= 0; index = BreadcrumbVisibleOrder.Step(order, index, 1))
            {
                visited.Add(order[index].NodeId ?? "row" + order[index].RowIndex);
            }

            // Assert
            visited.Should().Equal("row0", "a", "a1", "b", "row1");
        }

        /// <summary>AC1: the children of a collapsed node are not visible, so Down skips them.</summary>
        [TestMethod]
        public void Step_SkipsChildrenOfCollapsedNode()
        {
            // Arrange: Q3 was expanded and loaded, then collapsed again.
            BreadcrumbRow row = EfcRowWithTree(expandQ3: true);
            string q3 = row.DescendantNodes[0].Identity;
            row.TryCollapseDescendant(q3).Should().BeTrue();
            var order = new List<BreadcrumbVisiblePosition>();
            row.AppendVisiblePositions(0, order);
            int fromQ3 = BreadcrumbVisibleOrder.IndexOf(order, 0, q3);

            // Act
            int next = BreadcrumbVisibleOrder.Step(order, fromQ3, 1);

            // Assert
            order[next].NodeId.Should().Be(row.DescendantNodes[1].Identity, "Q4 follows Q3");
            order.Should().HaveCount(3, "row, Q3 and Q4 only");
        }

        /// <summary>AC2: no wrap; stepping outward from either end, or from an invalid index, is no move.</summary>
        [TestMethod]
        public void Step_FromBoundaryOutward_ReturnsMinusOne()
        {
            // Arrange
            var order = HandOrder();

            // Act / Assert
            BreadcrumbVisibleOrder.Step(order, 0, -1).Should().Be(-1);
            BreadcrumbVisibleOrder.Step(order, order.Count - 1, 1).Should().Be(-1);
            BreadcrumbVisibleOrder.Step(order, -1, 1).Should().Be(-1);
            BreadcrumbVisibleOrder.Step(order, order.Count, -1).Should().Be(-1);
        }

        /// <summary>AC11: a page move is clamped to the first and last entry without wrapping.</summary>
        [TestMethod]
        public void Step_PageDelta_ClampsToFirstAndLast()
        {
            // Arrange
            var order = HandOrder();

            // Act / Assert
            BreadcrumbVisibleOrder.Step(order, 0, 2).Should().Be(2);
            BreadcrumbVisibleOrder.Step(order, 1, 10).Should().Be(order.Count - 1);
            BreadcrumbVisibleOrder.Step(order, 3, -10).Should().Be(0);
        }

        /// <summary>AC11: Home and End resolve to the first and last entry.</summary>
        [TestMethod]
        public void FirstAndLast_ReturnBoundaryIndices()
        {
            // Arrange
            var order = HandOrder();

            // Act / Assert
            BreadcrumbVisibleOrder.First(order).Should().Be(0);
            BreadcrumbVisibleOrder.Last(order).Should().Be(order.Count - 1);
        }

        /// <summary>Edge: an empty order has no first or last entry.</summary>
        [TestMethod]
        public void FirstAndLast_EmptyOrder_ReturnMinusOne()
        {
            // Arrange
            var order = new List<BreadcrumbVisiblePosition>();

            // Act / Assert
            BreadcrumbVisibleOrder.First(order).Should().Be(-1);
            BreadcrumbVisibleOrder.Last(order).Should().Be(-1);
        }

        /// <summary>Negative: an unknown row or node is not located.</summary>
        [TestMethod]
        public void IndexOf_UnknownEntry_ReturnsMinusOne()
        {
            // Arrange
            var order = HandOrder();

            // Act / Assert
            BreadcrumbVisibleOrder.IndexOf(order, 0, "missing").Should().Be(-1);
            BreadcrumbVisibleOrder.IndexOf(order, 7, null).Should().Be(-1);
            BreadcrumbVisibleOrder.IndexOf(order, 0, "a1").Should().Be(2);
            BreadcrumbVisibleOrder.IndexOf(order, 1, null).Should().Be(4);
        }

        /// <summary>Error: every operation rejects a null order explicitly.</summary>
        [TestMethod]
        public void IndexOf_NullOrder_Throws()
        {
            // Arrange
            IReadOnlyList<BreadcrumbVisiblePosition> order = null!;

            // Act
            var calls = new Action[]
            {
                () => BreadcrumbVisibleOrder.IndexOf(order, 0, null),
                () => BreadcrumbVisibleOrder.Step(order, 0, 1),
                () => BreadcrumbVisibleOrder.First(order),
                () => BreadcrumbVisibleOrder.Last(order),
                () => BreadcrumbVisibleOrder.ParentOf(order, 0),
            };

            // Assert
            foreach (Action call in calls)
            {
                call.Should().Throw<ArgumentNullException>();
            }
        }

        /// <summary>AC8: the parent of a nested node is its parent node; of a first-level node, the row.</summary>
        [TestMethod]
        public void ParentOf_NodeEntry_ReturnsParentIndex()
        {
            // Arrange
            var order = HandOrder();

            // Act / Assert
            BreadcrumbVisibleOrder.ParentOf(order, 2).Should().Be(1, "a1's parent is a");
            BreadcrumbVisibleOrder.ParentOf(order, 3).Should().Be(0, "b is a first-level node");
            BreadcrumbVisibleOrder.ParentOf(order, 0).Should().Be(-1, "a row entry has no parent");
            BreadcrumbVisibleOrder.ParentOf(order, 9).Should().Be(-1, "an invalid index");
        }

        /// <summary>AC1: an expanded EFC row appends its entry, then its shown nodes in pre-order.</summary>
        [TestMethod]
        public void AppendVisiblePositions_ExpandedRow_AddsRowThenDescendantsInPreOrder()
        {
            // Arrange
            BreadcrumbRow row = EfcRowWithTree(expandQ3: true);
            var order = new List<BreadcrumbVisiblePosition>();

            // Act
            row.AppendVisiblePositions(3, order);

            // Assert
            string q3 = row.DescendantNodes[0].Identity;
            order.Select(p => p.Depth).Should().Equal(-1, 0, 1, 1, 0);
            order.Should().OnlyContain(p => p.RowIndex == 3);
            order[0].IsRowEntry.Should().BeTrue();
            order[0].IsExpanded.Should().BeTrue();
            order[0].HasChildren.Should().BeTrue();
            order[1].NodeId.Should().Be(q3);
            order[1].IsExpanded.Should().BeTrue();
            order[2].ParentNodeId.Should().Be(q3);
            order[4].NodeId.Should().Be(row.DescendantNodes[1].Identity);
            order[4].HasChildren.Should().BeFalse("Q4 is a leaf");
        }

        /// <summary>Edge: a banner row is never part of the visible order.</summary>
        [TestMethod]
        public void AppendVisiblePositions_BannerRow_AddsNothing()
        {
            // Arrange
            var banner = new BreadcrumbRow(
                "banner",
                BreadcrumbRowKind.Banner,
                new[] { new BreadcrumbSegment("==== Recent", "==== Recent", false) },
                null
            );
            var order = new List<BreadcrumbVisiblePosition>();

            // Act
            banner.AppendVisiblePositions(0, order);

            // Assert
            order.Should().BeEmpty();
        }

        /// <summary>AC14: a node whose fetch attached zero children reports no children.</summary>
        [TestMethod]
        public void AppendVisiblePositions_ZeroChildrenNode_ReportsNoChildren()
        {
            // Arrange
            BreadcrumbRow row = EfcRowWithTree(expandQ3: false);
            string q3 = row.DescendantNodes[0].Identity;
            row.TryBeginDescendantExpand(q3, out _, out int generation).Should().BeTrue();
            row.TryAttachDescendantChildren(q3, generation, Array.Empty<FolderBreadcrumbSegment>());
            var order = new List<BreadcrumbVisiblePosition>();

            // Act
            row.AppendVisiblePositions(0, order);

            // Assert
            order[1].NodeId.Should().Be(q3);
            order[1].HasChildren.Should().BeFalse("the fetch attached zero children");
        }

        /// <summary>AC1: the QFC order skips non-selectable rows and the children of collapsed nodes.</summary>
        [TestMethod]
        public void VisiblePositions_QfcModel_SkipsNonSelectableRowsAndCollapsedChildren()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("banner", "==== Suggestions", false);
            model.AddSuggestionRow(
                new[] { Segment(Clients, "Clients", true), Segment(North, "North", true) },
                0.5
            );
            model.AddPlainRow("plain", "Inbox", true);
            var row = model.Rows[1];
            row.TryExpandLeaf().Should().BeTrue();
            row.SetSubfolders(new[] { Segment(Q3, "Q3", true), Segment(Q4, "Q4", false) });
            var q3 = row.DescendantRoot!.Children[0];
            q3.Expanded = true;
            q3.Attach(new[] { Segment(Q3 + @"\Invoices", "Invoices", false) });
            q3.Collapse();

            // Act
            var order = model.VisiblePositions();

            // Assert
            order.Select(p => p.RowIndex).Should().Equal(1, 1, 1, 2);
            order
                .Select(p => p.NodeId)
                .Should()
                .Equal(null, q3.Identity, row.DescendantRoot!.Children[1].Identity, null);
            order[0].IsExpanded.Should().BeTrue();
            order[1].IsExpanded.Should().BeFalse();
        }

        // row0 > a > a1, row0 > b, row1 (a and row0 expanded).
        private static List<BreadcrumbVisiblePosition> HandOrder() =>
            new List<BreadcrumbVisiblePosition>
            {
                new BreadcrumbVisiblePosition(0, null, null, -1, true, true),
                new BreadcrumbVisiblePosition(0, "a", null, 0, true, true),
                new BreadcrumbVisiblePosition(0, "a1", "a", 1, false, false),
                new BreadcrumbVisiblePosition(0, "b", null, 0, false, false),
                new BreadcrumbVisiblePosition(1, null, null, -1, false, false),
            };

        // Clients > North, expanded, with first level Q3 (has children) and Q4 (leaf).
        private static BreadcrumbRow EfcRowWithTree(bool expandQ3)
        {
            var row = new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[]
                {
                    new BreadcrumbSegment(Clients, "Clients", true),
                    new BreadcrumbSegment(North, "North", true),
                },
                null
            );
            row.SetLeafDescendants(new[] { Segment(Q3, "Q3", true), Segment(Q4, "Q4", false) });
            row.ToggleLeafExpanded();
            if (expandQ3)
            {
                string q3 = row.DescendantNodes[0].Identity;
                row.TryBeginDescendantExpand(q3, out _, out int generation);
                row.TryAttachDescendantChildren(
                    q3,
                    generation,
                    new[]
                    {
                        Segment(Q3 + @"\Invoices", "Invoices", false),
                        Segment(Q3 + @"\Receipts", "Receipts", false),
                    }
                );
            }
            return row;
        }

        private static FolderBreadcrumbSegment Segment(
            string path,
            string name,
            bool hasChildren
        ) =>
            new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store", path, path),
                name,
                path,
                hasChildren
            );
    }
}
