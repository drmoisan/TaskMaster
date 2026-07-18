using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the pure <see cref="BreadcrumbRenderProjection"/> (#351 P3-T4): positive
    /// full-chain rendering with percentage, negative null-state rejection, and edge projections
    /// (collapsed row, affordance-less leaf, Path B empty-percentage rows, formatter parity).
    /// Deterministic; no Outlook, WebView2, or I/O.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbRenderProjectionTests
    {
        private static FolderBreadcrumbSegment Segment(
            string entryId,
            string path,
            string name,
            bool hasChildren
        ) =>
            new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store-a", entryId, path),
                name,
                path,
                hasChildren
            );

        private static IReadOnlyList<FolderBreadcrumbSegment> Chain(bool leafHasChildren = true) =>
            new[]
            {
                Segment("root", "\\Inbox", "Folder", true),
                Segment("mid", "\\Inbox\\Sub", "SubFolder", true),
                Segment("leaf", "\\Inbox\\Sub\\Leaf", "Leaf", leafHasChildren),
            };

        [TestMethod]
        public void Project_FullChainSuggestionRow_RendersOrderedSegmentsArrowsAndPercent()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(leafHasChildren: false), 0.732);
            model.SelectRow(0);

            // Act
            var rows = BreadcrumbRenderProjection.Project(model);

            // Assert: Folder -> SubFolder -> Leaf order with arrows between segments (FR-1).
            rows.Should().ContainSingle();
            var row = rows[0];
            row.Selected.Should().BeTrue();
            row.PercentText.Should().Be("73%");
            row.Cells.Select(c => c.Kind)
                .Should()
                .Equal(
                    BreadcrumbCellKind.Segment,
                    BreadcrumbCellKind.Arrow,
                    BreadcrumbCellKind.Segment,
                    BreadcrumbCellKind.Arrow,
                    BreadcrumbCellKind.Segment
                );
            row.Cells.Where(c => c.Kind == BreadcrumbCellKind.Segment)
                .Select(c => c.Text)
                .Should()
                .Equal("Folder", "SubFolder", "Leaf");
        }

        [TestMethod]
        public void Project_NullModel_Throws()
        {
            // Arrange, Act
            Action act = () => BreadcrumbRenderProjection.Project(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("model");
        }

        [TestMethod]
        public void Project_CollapsedRow_RendersPlusThenTerminalSegmentOnly()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(), 0.5);
            model.Rows[0].CollapseAfter(0);

            // Act
            var row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert: plus to the left of the now-terminal segment; downstream hidden (FR-3).
            row.Collapsed.Should().BeTrue();
            row.Cells.Select(c => c.Kind)
                .Should()
                .Equal(BreadcrumbCellKind.Plus, BreadcrumbCellKind.Segment);
            row.Cells[1].Text.Should().Be("Folder");
        }

        [TestMethod]
        public void Project_LeafWithoutSubfolders_RendersNoAffordance()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(leafHasChildren: false), null);

            // Act
            var row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert (FR-2: affordance only when the leaf has subfolders).
            row.Cells.Should()
                .NotContain(c =>
                    c.Kind == BreadcrumbCellKind.Plus || c.Kind == BreadcrumbCellKind.Minus
                );
        }

        [TestMethod]
        public void Project_LeafAffordance_PlusWhenClosedMinusWhenOpen()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(leafHasChildren: true), 0.5);

            // Act, Assert: closed -> plus at the end of the row.
            BreadcrumbRenderProjection
                .Project(model)[0]
                .Cells.Last()
                .Kind.Should()
                .Be(BreadcrumbCellKind.Plus);

            // Open the expansion -> minus, and the fetched subfolders project through.
            model.Rows[0].TryExpandLeaf();
            model
                .Rows[0]
                .SetSubfolders(new[] { Segment("s", "\\Inbox\\Sub\\Leaf\\S", "S", false) });
            var open = BreadcrumbRenderProjection.Project(model)[0];
            open.Cells.Last().Kind.Should().Be(BreadcrumbCellKind.Minus);
            open.LeafExpanded.Should().BeTrue();
            open.Subfolders.Should()
                .ContainSingle(s =>
                    s.DisplayName == "S" && s.FolderPath == "\\Inbox\\Sub\\Leaf\\S"
                );
        }

        [TestMethod]
        public void Project_PathBRow_RendersAncestorSplitChainWithEmptyPercentCell()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("\\Inbox\\Manual\\Target");

            // Act
            var row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert (research §10.4 / FR-7: chains with an empty percentage cell, no affordance).
            row.IsSuggestion.Should().BeFalse();
            row.PercentText.Should().BeEmpty();
            row.Cells.Where(c => c.Kind == BreadcrumbCellKind.Segment)
                .Select(c => c.Text)
                .Should()
                .Equal("Inbox", "Manual", "Target");
            row.Cells.Should()
                .NotContain(c =>
                    c.Kind == BreadcrumbCellKind.Plus || c.Kind == BreadcrumbCellKind.Minus
                );
        }

        [TestMethod]
        public void Project_PathBRowWithoutSeparators_RendersSingleVerbatimSegment()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("Trash to Delete");

            // Act
            var row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert
            row.Cells.Should().ContainSingle();
            row.Cells[0].Text.Should().Be("Trash to Delete");
            row.PercentText.Should().BeEmpty();
        }

        [TestMethod]
        public void Project_PercentFormatting_MatchesPercentageFormatterParity()
        {
            // Arrange: 0, 1, and null probabilities (G3: formatter consumed read-only).
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(), 0.0);
            model.AddSuggestionRow(Chain(), 1.0);
            model.AddSuggestionRow(Chain(), null);

            // Act
            var rows = BreadcrumbRenderProjection.Project(model);

            // Assert
            rows[0].PercentText.Should().Be(PercentageFormatter.FormatPercent(0.0)).And.Be("0%");
            rows[1].PercentText.Should().Be(PercentageFormatter.FormatPercent(1.0)).And.Be("100%");
            rows[2].PercentText.Should().Be(PercentageFormatter.FormatPercent(null)).And.BeEmpty();
        }

        [TestMethod]
        public void Project_TruncationEligibility_MarksInteriorSegmentsOnly()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(), 0.4);

            // Act
            var segments = BreadcrumbRenderProjection
                .Project(model)[0]
                .Cells.Where(c => c.Kind == BreadcrumbCellKind.Segment)
                .ToArray();

            // Assert (FR-1: long paths truncate in middle segments, never first or terminal).
            segments[0].TruncationEligible.Should().BeFalse();
            segments[1].TruncationEligible.Should().BeTrue();
            segments[2].TruncationEligible.Should().BeFalse();
        }

        [TestMethod]
        public void Project_SegmentIndexes_MapBackToChainPositions()
        {
            // Arrange: the JS side routes double-clicks by segment index.
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(Chain(), 0.4);

            // Act
            var row = BreadcrumbRenderProjection.Project(model)[0];

            // Assert
            row.Cells.Where(c => c.Kind == BreadcrumbCellKind.Segment)
                .Select(c => c.SegmentIndex)
                .Should()
                .Equal(0, 1, 2);
            row.Cells.Where(c => c.Kind != BreadcrumbCellKind.Segment)
                .Should()
                .OnlyContain(c => c.SegmentIndex == -1);
        }
    }
}
