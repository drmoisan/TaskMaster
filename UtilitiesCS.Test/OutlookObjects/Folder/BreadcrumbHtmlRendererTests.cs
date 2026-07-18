using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="BreadcrumbHtmlRenderer"/> invariants (#349): trailing fixed
    /// percent item on every row, affordance gating on HasSubfolders, HTML-encoding of hostile
    /// names, non-interactive banners, selectable trash pseudo-row, themes, collapsed fragments,
    /// and the empty row list.
    /// </summary>
    [TestClass]
    public class BreadcrumbHtmlRendererTests
    {
        private readonly BreadcrumbHtmlRenderer _renderer = new BreadcrumbHtmlRenderer();

        private static BreadcrumbSegment Segment(string name, bool hasSubfolders)
        {
            return new BreadcrumbSegment(@"Inbox\" + name, name, hasSubfolders);
        }

        private static BreadcrumbRow SuggestionRow(bool leafHasSubfolders, double? probability)
        {
            return new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[]
                {
                    Segment("Root", true),
                    Segment("Mid", true),
                    Segment("Leaf", leafHasSubfolders),
                },
                probability
            );
        }

        [TestMethod]
        public void RenderRowFragment_EveryRowKind_EmitsTrailingPctFlexItem()
        {
            // Arrange
            var suggestion = SuggestionRow(leafHasSubfolders: false, probability: 0.87);
            var banner = new BreadcrumbRow(
                "row-1",
                BreadcrumbRowKind.Banner,
                new[] { Segment("==== X ====", false) },
                null
            );
            var trash = new BreadcrumbRow(
                "row-2",
                BreadcrumbRowKind.TrashPseudoRow,
                Array.Empty<BreadcrumbSegment>(),
                null
            );

            // Act / Assert: percent span present on every row and positioned after the crumb div.
            foreach (var row in new[] { suggestion, banner, trash })
            {
                string html = _renderer.RenderRowFragment(row, isSelected: false);
                html.Should()
                    .MatchRegex(
                        "</div><span class=\"pct\">",
                        $"row '{row.RowId}' must end its flex row with the trailing .pct item"
                    );
            }

            _renderer
                .RenderRowFragment(suggestion, false)
                .Should()
                .Contain("<span class=\"pct\">87%</span>");
        }

        [TestMethod]
        public void RenderRowFragment_CollapsedRow_StillEmitsTrailingPercent()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true, probability: 0.5);
            row.CollapseAfter(0);

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert
            html.Should().Contain("<span class=\"pct\">50%</span>");
        }

        [TestMethod]
        public void RenderRowFragment_LeafWithSubfolders_EmitsPlusWhenCollapsedMinusWhenExpanded()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true, probability: null);

            // Act / Assert: plus while leaf children are collapsed.
            _renderer
                .RenderRowFragment(row, false)
                .Should()
                .Contain("data-role=\"leaf\">+</span>");

            // Expand the leaf: minus (U+2212 entity).
            row.SetLeafChildren(new[] { Segment("Child", false) });
            row.ToggleLeafExpanded();
            _renderer
                .RenderRowFragment(row, false)
                .Should()
                .Contain("data-role=\"leaf\">&#8722;</span>");
        }

        [TestMethod]
        public void RenderRowFragment_LeafWithoutSubfolders_EmitsNoAffordance()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: false, probability: null);

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert
            html.Should().NotContain("affordance");
        }

        [TestMethod]
        public void RenderRowFragment_HostileFolderNames_AreHtmlEncoded()
        {
            // Arrange: hostile display name with script tag, ampersand, and quotes.
            string hostile = "<script>alert('x')</script> & \"quotes\"";
            var row = new BreadcrumbRow(
                "row-0",
                BreadcrumbRowKind.Suggestion,
                new[] { new BreadcrumbSegment(@"Inbox\" + hostile, hostile, false) },
                null
            );

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert: raw markup never survives; encoded entities do.
            html.Should().NotContain("<script>");
            html.Should().Contain("&lt;script&gt;");
            html.Should().Contain("&amp;");
            html.Should().Contain("&quot;quotes&quot;");
        }

        [TestMethod]
        public void RenderRowFragment_BannerRow_IsNonInteractive()
        {
            // Arrange
            var row = new BreadcrumbRow(
                "row-1",
                BreadcrumbRowKind.Banner,
                new[] { Segment("========= SUGGESTIONS =========", false) },
                null
            );

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert: no selectable class, no segment indices, no affordance.
            html.Should().Contain("class=\"row banner\"");
            html.Should().NotContain("selectable");
            html.Should().NotContain("data-segment-index");
            html.Should().NotContain("affordance");
        }

        [TestMethod]
        public void RenderRowFragment_TrashPseudoRow_IsSelectableWithoutAffordance()
        {
            // Arrange
            var row = new BreadcrumbRow(
                "row-2",
                BreadcrumbRowKind.TrashPseudoRow,
                Array.Empty<BreadcrumbSegment>(),
                null
            );

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert
            html.Should().Contain("selectable");
            html.Should().Contain("Trash to Delete");
            html.Should().NotContain("affordance");
        }

        [TestMethod]
        public void RenderDocument_DarkVersusLight_EmbedsMatchingThemeBlock()
        {
            // Arrange
            var rows = new[] { SuggestionRow(leafHasSubfolders: false, probability: null) };

            // Act
            string dark = _renderer.RenderDocument(rows, darkMode: true, selectedRowId: null);
            string light = _renderer.RenderDocument(rows, darkMode: false, selectedRowId: null);

            // Assert
            dark.Should().Contain("background: #1e1e1e");
            dark.Should().NotContain("background: #ffffff");
            light.Should().Contain("background: #ffffff");
            light.Should().NotContain("background: #1e1e1e");
        }

        [TestMethod]
        public void RenderRowFragment_CollapsedState_RendersReExpandPlusAtTerminalSegment()
        {
            // Arrange: collapse after segment 1 ("Mid" becomes the terminal segment).
            var row = SuggestionRow(leafHasSubfolders: false, probability: null);
            row.CollapseAfter(1);

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert: hidden leaf is gone; re-expand plus sits to the left of "Mid".
            html.Should().NotContain(">Leaf<");
            html.Should()
                .MatchRegex(
                    "data-role=\"reexpand\">\\+</span><span class=\"seg\" data-segment-index=\"1\"[^>]*>Mid</span>"
                );
        }

        [TestMethod]
        public void RenderRows_EmptyRowList_ProducesEmptyFragmentAndValidDocument()
        {
            // Act
            string fragment = _renderer.RenderRows(
                Array.Empty<BreadcrumbRow>(),
                selectedRowId: null
            );
            string document = _renderer.RenderDocument(
                Array.Empty<BreadcrumbRow>(),
                darkMode: false,
                selectedRowId: null
            );

            // Assert
            fragment.Should().BeEmpty();
            document.Should().Contain("<div class=\"rows\" id=\"rows\"></div>");
        }

        [TestMethod]
        public void RenderRows_WithSelectedRowId_MarksOnlyThatRowSelected()
        {
            // Arrange
            var rows = new[]
            {
                SuggestionRow(leafHasSubfolders: false, probability: null),
                new BreadcrumbRow(
                    "row-9",
                    BreadcrumbRowKind.Suggestion,
                    new[] { Segment("Other", false) },
                    null
                ),
            };

            // Act
            string html = _renderer.RenderRows(rows, selectedRowId: "row-9");

            // Assert
            Regex.Matches(html, "rowwrap selected").Count.Should().Be(1);
            html.Should().Contain("<div class=\"rowwrap selected\" data-row-id=\"row-9\">");
        }

        [TestMethod]
        public void RenderRowFragment_ExpandedLeaf_RendersEncodedChildren()
        {
            // Arrange
            var row = SuggestionRow(leafHasSubfolders: true, probability: null);
            row.SetLeafChildren(new[] { Segment("<Kid>", false) });
            row.ToggleLeafExpanded();

            // Act
            string html = _renderer.RenderRowFragment(row, isSelected: false);

            // Assert
            html.Should().Contain("class=\"child\"");
            html.Should().Contain("&lt;Kid&gt;");
            html.Should().NotContain("><Kid><");
        }
    }
}
