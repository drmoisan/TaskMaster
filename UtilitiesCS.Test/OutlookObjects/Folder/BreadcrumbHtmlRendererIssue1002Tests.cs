#nullable enable
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue 1002 rendering and bridge-asset contracts: recursive keyed children with the node
    /// affordance after the name, sibling nesting, aria-selected marking, encoding, the unchanged
    /// key-less markup, and the text order of the bridge script branches (no browser engine runs
    /// in CI, so the script order is asserted as text).
    /// </summary>
    [TestClass]
    public class BreadcrumbHtmlRendererIssue1002Tests
    {
        private const string North = @"\Archive\Clients\North";
        private const string Q3 = North + @"\Q3";
        private const string Q4 = North + @"\Q4";
        private const string NodeAffordance = "<span class=\"affordance node\" data-role=\"node\"";

        private readonly BreadcrumbHtmlRenderer _renderer = new BreadcrumbHtmlRenderer();

        [TestMethod]
        public void RenderRowFragment_KeyedChildWithChildren_RendersNodeAffordanceAfterName()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);

            // Act
            string html = _renderer.RenderRowFragment(row, false);

            // Assert
            string node = ChildElement(html, q3);
            node.Should().Contain(">Q3</span>", "the name span is rendered");
            node.IndexOf(">Q3</span>", System.StringComparison.Ordinal)
                .Should()
                .BeLessThan(
                    node.IndexOf(NodeAffordance, System.StringComparison.Ordinal),
                    "the node affordance follows the name"
                );
            node.Should()
                .Contain(
                    NodeAffordance + " role=\"button\" tabindex=\"0\" aria-expanded=\"false\">+"
                );
        }

        [TestMethod]
        public void RenderRowFragment_KeyedLeafChild_RendersNoAffordance()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q4 = IdOf(row, Q4);

            // Act
            string html = _renderer.RenderRowFragment(row, false);

            // Assert
            ChildElement(html, q4).Should().NotContain("data-role=\"node\"", "Q4 has no children");
            Regex.Matches(html, "data-role=\"node\"").Count.Should().Be(1, "only Q3 is expandable");
        }

        [TestMethod]
        public void RenderRowFragment_ExpandedDescendant_NestsChildrenContainerAfterNode()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q3 = IdOf(row, Q3);
            row.TryBeginDescendantExpand(q3, out _, out int gen).Should().BeTrue();
            row.TryAttachDescendantChildren(q3, gen, SecondLevel()).Should().BeTrue();
            string invoices = row.DescendantNodes[0].Children[0].Identity;

            // Act
            string html = _renderer.RenderRowFragment(row, false);

            // Assert
            string q3Element = ChildElement(html, q3);
            q3Element.Should().Contain("aria-expanded=\"true\">&#8722;");
            int q3End = html.IndexOf(q3Element, System.StringComparison.Ordinal) + q3Element.Length;
            html.Substring(q3End)
                .Should()
                .StartWith(
                    "<div class=\"children\"><div class=\"child\" data-child-index=\"0\" data-node-id=\""
                        + invoices
                        + "\" data-depth=\"1\"",
                    "the nested container is a sibling placed after the node element"
                );
        }

        [TestMethod]
        public void RenderRowFragment_SelectedDescendant_MarksAriaSelected()
        {
            // Arrange
            BreadcrumbRow row = RowWithTree();
            string q4 = IdOf(row, Q4);
            row.TrySelectDescendant(q4).Should().BeTrue();

            // Act
            string selectedRow = _renderer.RenderRowFragment(row, true);
            string otherRow = _renderer.RenderRowFragment(row, false);

            // Assert
            ChildElement(selectedRow, q4).Should().Contain("aria-selected=\"true\"");
            Regex.Matches(selectedRow, "aria-selected").Count.Should().Be(1);
            otherRow.Should().NotContain("aria-selected", "only the selected row marks a node");
            selectedRow.Should().NotContain("child selected", "no second class is introduced");
        }

        [TestMethod]
        public void RenderRowFragment_DescendantNames_AreHtmlEncoded()
        {
            // Arrange
            BreadcrumbRow row = ExpandableRow();
            row.SetLeafDescendants(new[] { Segment(North + @"\A<b>&", "A<b>&", false) })
                .Should()
                .BeTrue();
            row.ToggleLeafExpanded().Should().BeTrue();

            // Act
            string html = _renderer.RenderRowFragment(row, false);

            // Assert
            html.Should().Contain(">A&lt;b&gt;&amp;</span>");
            html.Should().Contain("title=\"" + North + @"\A&lt;b&gt;&amp;" + "\"");
            html.Should().NotContain("A<b>", "provider names are never emitted as markup");
        }

        [TestMethod]
        public void RenderRowFragment_KeylessChildren_RenderUnchanged()
        {
            // Arrange
            BreadcrumbRow row = ExpandableRow();
            row.SetLeafChildren(new[] { new BreadcrumbSegment(Q3, "Q3", true) }).Should().BeTrue();
            row.ToggleLeafExpanded().Should().BeTrue();

            // Act
            string html = _renderer.RenderRowFragment(row, false);

            // Assert
            html.Should()
                .Contain(
                    "<div class=\"children\"><div class=\"child\" data-child-index=\"0\" title=\""
                        + Q3
                        + "\">Q3</div></div>"
                );
            html.Should().NotContain("data-node-id");
        }

        [TestMethod]
        public void BridgeJs_NodeAffordanceBranch_PrecedesChildAndLeafBranches()
        {
            // Arrange
            string js = BreadcrumbDocumentAssets.BridgeJs;
            int nodeBranch = js.IndexOf(
                ".affordance[data-role=\"node\"]",
                System.StringComparison.Ordinal
            );

            // Act
            int keyedChild = js.IndexOf(".child[data-node-id]", System.StringComparison.Ordinal);
            int keylessChild = js.IndexOf(
                ".child[data-child-index]",
                System.StringComparison.Ordinal
            );
            int leafToggle = js.IndexOf(
                "type: 'leafExpandToggle'",
                System.StringComparison.Ordinal
            );

            // Assert
            nodeBranch.Should().BeGreaterThan(-1, "the node affordance branch exists");
            nodeBranch.Should().BeLessThan(keyedChild, "the affordance sits inside the child");
            keyedChild.Should().BeLessThan(keylessChild);
            keylessChild.Should().BeLessThan(leafToggle);
            js.Should().Contain("type: 'descendantToggle'");
        }

        [TestMethod]
        public void BridgeJs_DescendantActivateBranch_PrecedesKeylessChildBranch()
        {
            // Arrange
            string js = BreadcrumbDocumentAssets.BridgeJs;

            // Act
            int keyedChild = js.IndexOf(".child[data-node-id]", System.StringComparison.Ordinal);
            int activate = js.IndexOf(
                "type: 'descendantActivate'",
                System.StringComparison.Ordinal
            );
            int keylessChild = js.IndexOf(
                ".child[data-child-index]",
                System.StringComparison.Ordinal
            );

            // Assert
            keyedChild.Should().BeGreaterThan(-1, "the keyed child branch exists");
            activate.Should().BeGreaterThan(keyedChild);
            activate.Should().BeLessThan(keylessChild, "the key-less branch is retained after it");
            js.Should().Contain("type: 'renderedChildActivate'");
        }

        [TestMethod]
        public void BridgeJs_SubfolderResultHandler_WrapsChildNameInNameSpan()
        {
            // Arrange
            string js = BreadcrumbDocumentAssets.BridgeJs;
            int handler = js.IndexOf(
                "msg.type === 'subfolderResult'",
                System.StringComparison.Ordinal
            );

            // Act
            string tail = handler < 0 ? string.Empty : js.Substring(handler);

            // Assert
            handler.Should().BeGreaterThan(-1, "the subfolderResult branch exists");
            tail.Should().Contain("name.className = 'name';");
            tail.Should().Contain("name.textContent = c.displayName;");
            tail.Should().Contain("div.appendChild(name);");
            tail.Should()
                .NotContain(
                    "div.textContent",
                    "bare text would bypass the .child .name truncation"
                );
        }

        [TestMethod]
        public void BridgeJs_NodeAffordanceEnterOrSpace_PrecedesRowConfirmation()
        {
            // Arrange
            string js = BreadcrumbDocumentAssets.BridgeJs;
            int keydown = js.IndexOf("addEventListener('keydown'", System.StringComparison.Ordinal);
            string listener = js.Substring(keydown);

            // Act
            int space = listener.IndexOf("e.key === ' '", System.StringComparison.Ordinal);
            int confirm = listener.IndexOf("type: 'rowConfirmed'", System.StringComparison.Ordinal);
            int toggle = listener.IndexOf(
                "type: 'descendantToggle'",
                System.StringComparison.Ordinal
            );

            // Assert
            keydown.Should().BeGreaterThan(-1);
            space.Should().BeGreaterThan(-1, "Space toggles a focused node affordance");
            space.Should().BeLessThan(confirm, "toggling by keyboard never confirms the row");
            toggle.Should().BeGreaterThan(-1).And.BeLessThan(confirm);
        }

        [TestMethod]
        public void BaseCss_Child_IsFlexRow()
        {
            // Arrange
            string css = BreadcrumbDocumentAssets.BaseCss;

            // Act
            Match rule = Regex.Match(css, @"(?m)^\.child \{[^}]*\}");

            // Assert
            rule.Success.Should().BeTrue("the child rule exists");
            rule.Value.Should().Contain("display: flex").And.Contain("align-items: center");
            Regex.Matches(css, @"(?m)^\.child \{").Count.Should().Be(1, "the rule is merged");
        }

        [TestMethod]
        public void LightThemeCss_SelectedChild_UsesSelectionBackground()
        {
            // Arrange
            string css = BreadcrumbDocumentAssets.LightThemeCss;

            // Act / Assert
            css.Should()
                .Contain(".child[aria-selected=\"true\"] { background: #cce8ff; }")
                .And.Contain(".rowwrap.selected .row { background: #cce8ff; }");
        }

        [TestMethod]
        public void DarkThemeCss_SelectedChild_UsesSelectionBackground()
        {
            // Arrange
            string css = BreadcrumbDocumentAssets.DarkThemeCss;

            // Act / Assert
            css.Should()
                .Contain(".child[aria-selected=\"true\"] { background: #264f78; }")
                .And.Contain(".rowwrap.selected .row { background: #264f78; }");
        }

        [TestMethod]
        public void BaseCss_ChildName_CarriesEllipsisRule()
        {
            // Arrange
            string css = BreadcrumbDocumentAssets.BaseCss;

            // Act
            Match nameRule = Regex.Match(css, @"(?m)^\.child \.name \{[^}]*\}");
            Match childRule = Regex.Match(css, @"(?m)^\.child \{[^}]*\}");

            // Assert
            nameRule.Success.Should().BeTrue("the name rule exists");
            nameRule
                .Value.Should()
                .Contain("min-width: 0")
                .And.Contain("overflow: hidden")
                .And.Contain("text-overflow: ellipsis")
                .And.Contain("white-space: nowrap");
            childRule.Value.Should().NotContain("text-overflow");
            childRule.Value.Should().Contain("display: flex").And.Contain("align-items: center");
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

        private static BreadcrumbRow RowWithTree()
        {
            BreadcrumbRow row = ExpandableRow();
            row.SetLeafDescendants(new[] { Segment(Q3, "Q3", true), Segment(Q4, "Q4", false) })
                .Should()
                .BeTrue("the fixture needs a keyed tree");
            row.ToggleLeafExpanded().Should().BeTrue();
            return row;
        }

        private static FolderBreadcrumbSegment[] SecondLevel() =>
            new[]
            {
                Segment(Q3 + @"\Invoices", "Invoices", false),
                Segment(Q3 + @"\Receipts", "Receipts", true),
            };

        private static string IdOf(BreadcrumbRow row, string path) =>
            row.DescendantNodes.Single(node => node.Segment.FolderPath == path).Identity;

        // The node element runs from its opening tag to its own closing tag (nodes hold no div).
        private static string ChildElement(string html, string nodeId)
        {
            Match match = Regex.Match(
                html,
                "<div class=\"child\" data-child-index=\"\\d+\" data-node-id=\""
                    + nodeId
                    + "\"[^>]*>.*?</div>"
            );
            match.Success.Should().BeTrue($"node {nodeId} must be rendered");
            return match.Value;
        }

        private static FolderTreeNodeKey Key(string path) =>
            new FolderTreeNodeKey("store", path, path);

        private static FolderBreadcrumbSegment Segment(
            string path,
            string name,
            bool hasChildren
        ) => new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
    }
}
