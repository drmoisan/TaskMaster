#nullable enable
using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007 AC14 defence in depth: after a fetch that attached zero children, neither the
    /// EFC renderer nor the QFC projection shows an expand or collapse affordance on that row or
    /// node. A fetch that attached children keeps today's collapse affordance.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbEmptyExpanderIssue1007Tests
    {
        private const string Clients = @"\Archive\Clients";
        private const string North = Clients + @"\North";
        private const string Q3 = North + @"\Q3";

        /// <summary>AC14: an expanded EFC row whose fetch returned no child shows no leaf affordance.</summary>
        [TestMethod]
        public void EfcRenderer_ZeroChildrenFetch_RendersNoAffordance()
        {
            // Arrange
            BreadcrumbRow row = EfcRow();
            row.SetLeafDescendants(Array.Empty<FolderBreadcrumbSegment>()).Should().BeTrue();
            row.ToggleLeafExpanded().Should().BeTrue();

            // Act
            string html = new BreadcrumbHtmlRenderer().RenderRowFragment(row, true);

            // Assert
            html.Should().NotContain("data-role=\"leaf\"", "the fetch attached zero children");
        }

        /// <summary>AC14: a loaded EFC node with zero children shows no node affordance.</summary>
        [TestMethod]
        public void EfcRenderer_ZeroChildrenDescendantFetch_RendersNoNodeAffordance()
        {
            // Arrange
            BreadcrumbRow row = EfcRow();
            row.SetLeafDescendants(new[] { Segment(Q3, "Q3", true) }).Should().BeTrue();
            row.ToggleLeafExpanded().Should().BeTrue();
            string nodeId = row.DescendantNodes[0].Identity;
            row.TryBeginDescendantExpand(nodeId, out _, out int generation).Should().BeTrue();
            row.TryAttachDescendantChildren(
                    nodeId,
                    generation,
                    Array.Empty<FolderBreadcrumbSegment>()
                )
                .Should()
                .BeTrue();

            // Act
            string html = new BreadcrumbHtmlRenderer().RenderRowFragment(row, true);

            // Assert
            html.Should().NotContain("data-role=\"node\"", "the node's fetch returned no child");
        }

        /// <summary>AC14: a QFC row whose fetch returned no child projects no Plus or Minus cell.</summary>
        [TestMethod]
        public void QfcProjection_ZeroChildrenFetch_EmitsNoPlusOrMinus()
        {
            // Arrange
            var model = QfcModel();
            model.Rows[0].TryExpandLeaf().Should().BeTrue();
            model.Rows[0].SetSubfolders(Array.Empty<FolderBreadcrumbSegment>());

            // Act
            var row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert
            row.Cells.Select(cell => cell.Kind)
                .Should()
                .NotContain(BreadcrumbCellKind.Plus)
                .And.NotContain(BreadcrumbCellKind.Minus);
        }

        /// <summary>AC14: a loaded QFC descendant with zero children is projected without children.</summary>
        [TestMethod]
        public void QfcProjection_ZeroChildrenDescendantFetch_ReportsNoChildren()
        {
            // Arrange
            var model = QfcModel();
            var stateRow = model.Rows[0];
            stateRow.TryExpandLeaf().Should().BeTrue();
            stateRow.SetSubfolders(new[] { Segment(Q3, "Q3", true) });
            var node = stateRow.DescendantRoot!.Children[0];
            node.Expanded = true;
            node.Attach(Array.Empty<FolderBreadcrumbSegment>());

            // Act
            var projected = BreadcrumbRenderProjection.Project(model)[0].Subfolders[0];

            // Assert
            projected.HasChildren.Should().BeFalse("the node's fetch returned no child");
        }

        /// <summary>Guard (AC14): a fetch that attached a child keeps the EFC collapse affordance.</summary>
        [TestMethod]
        public void EfcRenderer_ChildrenFetch_KeepsMinusAffordance()
        {
            // Arrange
            BreadcrumbRow row = EfcRow();
            row.SetLeafDescendants(new[] { Segment(Q3, "Q3", false) }).Should().BeTrue();
            row.ToggleLeafExpanded().Should().BeTrue();

            // Act
            string html = new BreadcrumbHtmlRenderer().RenderRowFragment(row, true);

            // Assert
            html.Should().Contain("data-role=\"leaf\">&#8722;");
        }

        private static BreadcrumbRow EfcRow() =>
            new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[]
                {
                    new BreadcrumbSegment(Clients, "Clients", true),
                    new BreadcrumbSegment(North, "North", true),
                },
                null
            );

        private static BreadcrumbStateModel QfcModel()
        {
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(
                new[] { Segment(Clients, "Clients", true), Segment(North, "North", true) },
                0.5
            );
            model.SelectRow(0);
            return model;
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
