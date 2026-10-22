using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue 1002 EFC descendant routing through the real router: identity-addressed toggle and
    /// activation over strict provider and host mocks, with node identities read from the
    /// <c>data-node-id</c> attributes of the captured render posts.
    /// </summary>
    [TestClass]
    public class BreadcrumbBridgeRouterIssue1002Tests
    {
        private EfcDescendantRouterFixture fixture;

        [TestInitialize]
        public void Setup()
        {
            fixture = new EfcDescendantRouterFixture();
        }

        [TestMethod]
        public async Task DescendantToggle_SecondLevelNode_FetchesWithNodeKeyAndRendersChildren()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            int before = fixture.Posts.Count;

            // Act
            await fixture.ToggleAsync("row-0", q3);

            // Assert
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            fixture.Posts.Count.Should().Be(before + 1, "one row render carries the children");
            string html = fixture.LatestRender();
            html.Should()
                .Contain("data-depth=\"1\" title=\"" + EfcDescendantRouterFixture.Invoices);
            html.IndexOf(">Q3</span>", StringComparison.Ordinal)
                .Should()
                .BeLessThan(html.IndexOf(">Invoices</span>", StringComparison.Ordinal));
        }

        [TestMethod]
        public async Task DescendantToggle_ExpandedNode_CollapsesWithoutProviderCall()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            await fixture.ToggleAsync("row-0", q3);
            int before = fixture.Posts.Count;

            // Act
            await fixture.ToggleAsync("row-0", q3);

            // Assert
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            fixture.Posts.Count.Should().Be(before + 1, "the collapse posts one row render");
            fixture.LatestRender().Should().NotContain("Invoices");
        }

        [TestMethod]
        public async Task DescendantToggle_DoesNotChangeSelection()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            await fixture.InboundAsync("rowSelected", "row-0");
            string selection = fixture.Router.SelectedFolderPath;
            fixture.SelectionEvents.Clear();

            // Act
            await fixture.ToggleAsync("row-0", q3);
            await fixture.ToggleAsync("row-0", q3);

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(selection);
            fixture.SelectionEvents.Should().BeEmpty("a toggle changes expansion only");
        }

        [TestMethod]
        public async Task DescendantToggle_PostsNoSubfolderResult()
        {
            // Arrange
            string q3 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            int before = fixture.Posts.Count;

            // Act
            await fixture.ToggleAsync("row-0", q3);

            // Assert
            fixture
                .Posts.Skip(before)
                .Should()
                .NotContain(post => post.Contains("subfolderResult"));
        }

        [TestMethod]
        public async Task DescendantActivate_Grandchild_CommitsArchiveRelativeStem()
        {
            // Arrange
            string invoices = await fixture.ExpandToInvoicesAsync();
            fixture.SelectionEvents.Clear();

            // Act
            await fixture.ActivateAsync("row-0", invoices);

            // Assert
            const string stem = @"Clients\North\Q3\Invoices";
            fixture.Router.SelectedFolderPath.Should().Be(stem);
            fixture.SelectionEvents.Should().Equal(new[] { stem }, "one change event is raised");
            EfcSelectionGuard.IsValidFilingSelection(stem).Should().BeTrue();
            string html = fixture.LatestRender();
            html.Should().Contain("class=\"rowwrap selected\" data-row-id=\"row-0\"");
            html.Should()
                .Contain(
                    "data-node-id=\""
                        + invoices
                        + "\" data-depth=\"1\" title=\""
                        + EfcDescendantRouterFixture.Invoices
                        + "\" aria-selected=\"true\""
                );
        }

        [TestMethod]
        public async Task DescendantActivate_ThenRowConfirmed_RaisesConfirmationOnce()
        {
            // Arrange
            string invoices = await fixture.ExpandToInvoicesAsync();
            await fixture.ActivateAsync("row-0", invoices);

            // Act
            await fixture.InboundAsync("rowConfirmed", "row-0");

            // Assert
            fixture.Confirmations.Should().Be(1);
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3\Invoices");
        }

        [TestMethod]
        public async Task DescendantActivate_OutsideArchiveRoot_LeavesSelectionAndHighlightUnchanged()
        {
            // Arrange
            fixture.Children[EfcDescendantRouterFixture.Q3] = new[]
            {
                EfcDescendantRouterFixture.Segment(
                    EfcDescendantRouterFixture.Invoices,
                    "Invoices",
                    false
                ),
                EfcDescendantRouterFixture.Segment(
                    EfcDescendantRouterFixture.Receipts,
                    "Receipts",
                    true
                ),
                EfcDescendantRouterFixture.Segment(
                    @"\\other\Mailbox\Elsewhere",
                    "Elsewhere",
                    false
                ),
            };
            string invoices = await fixture.ExpandToInvoicesAsync();
            await fixture.ActivateAsync("row-0", invoices);
            string elsewhere = fixture.NodeId(@"\\other\Mailbox\Elsewhere");
            string receipts = fixture.NodeId(EfcDescendantRouterFixture.Receipts);
            fixture.SelectionEvents.Clear();
            int before = fixture.Posts.Count;

            // Act
            await fixture.ActivateAsync("row-0", elsewhere);

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3\Invoices");
            fixture.SelectionEvents.Should().BeEmpty();
            fixture.Posts.Count.Should().Be(before, "a non-selection posts nothing");
            await fixture.ToggleAsync("row-0", receipts);
            string html = fixture.LatestRender();
            html.Should().Contain("data-node-id=\"" + invoices + "\"");
            Regex.Matches(html, "aria-selected=\"true\"").Count.Should().Be(1);
            html.Should()
                .Contain(
                    WebUtility.HtmlEncode(EfcDescendantRouterFixture.Invoices)
                        + "\" aria-selected=\"true\"",
                    "the prior descendant keeps the highlight"
                );
        }

        [TestMethod]
        public async Task DescendantRequest_UnknownNode_IsRejectedWithoutStateChange()
        {
            // Arrange
            await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q3);
            await fixture.InboundAsync("rowSelected", "row-0");
            string selection = fixture.Router.SelectedFolderPath;
            fixture.SelectionEvents.Clear();
            int before = fixture.Posts.Count;
            const string unknown = "ffffffffffffffffffffffffffffffff";

            // Act
            await fixture.ToggleAsync("row-0", unknown);
            await fixture.ActivateAsync("row-0", unknown);
            await fixture.ToggleAsync("row-9", unknown);
            await fixture.ActivateAsync("row-9", unknown);

            // Assert
            fixture.Posts.Count.Should().Be(before, "a rejected identity posts nothing");
            fixture.SelectionEvents.Should().BeEmpty();
            fixture.Router.SelectedFolderPath.Should().Be(selection);
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once,
                "only the first-level expansion reached the provider"
            );
        }

        [TestMethod]
        public async Task DescendantToggle_LeafNode_IsRejectedWithoutProviderCall()
        {
            // Arrange
            string q4 = await fixture.BindAndExpandAsync(EfcDescendantRouterFixture.Q4);
            int before = fixture.Posts.Count;

            // Act
            await fixture.ToggleAsync("row-0", q4);

            // Assert
            fixture.Posts.Count.Should().Be(before);
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q4),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public async Task DescendantRequest_NodeFromAnotherRow_IsRejected()
        {
            // Arrange
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented, @"Clients\South");
            await fixture.InboundAsync("leafExpandToggle", "row-0");
            string q3 = fixture.NodeId(EfcDescendantRouterFixture.Q3);
            await fixture.InboundAsync("leafExpandToggle", "row-1");
            fixture.SelectionEvents.Clear();
            int before = fixture.Posts.Count;

            // Act
            await fixture.ToggleAsync("row-1", q3);
            await fixture.ActivateAsync("row-1", q3);

            // Assert
            fixture.Posts.Count.Should().Be(before, "row-1 never issued that identity");
            fixture.SelectionEvents.Should().BeEmpty();
            fixture.Router.SelectedFolderPath.Should().BeNull();
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        EfcDescendantRouterFixture.Key(EfcDescendantRouterFixture.Q3),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public async Task ExpandLeaf_KeyedChildren_RenderNodeAffordances()
        {
            // Arrange
            await fixture.BindAsync(EfcDescendantRouterFixture.Presented);

            // Act
            await fixture.InboundAsync("leafExpandToggle", "row-0");

            // Assert
            string html = fixture.LatestRender();
            Regex.Matches(html, "data-role=\"node\"").Count.Should().Be(1, "only Q3 has children");
            html.IndexOf(">Q3</span>", StringComparison.Ordinal)
                .Should()
                .BeGreaterThan(-1)
                .And.BeLessThan(html.IndexOf("data-role=\"node\"", StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Shared in-memory fixture for the issue 1002 EFC router tests: a real router over strict
    /// provider and host mocks, with an archive root bound and a three-level folder tree.
    /// </summary>
    internal sealed class EfcDescendantRouterFixture
    {
        internal const string Root = @"\\mailbox\Archive";
        internal const string Presented = @"Clients\North";
        internal const string North = Root + @"\Clients\North";
        internal const string Q3 = North + @"\Q3";
        internal const string Q4 = North + @"\Q4";
        internal const string Invoices = Q3 + @"\Invoices";
        internal const string Receipts = Q3 + @"\Receipts";

        internal EfcDescendantRouterFixture()
        {
            Children = new Dictionary<string, FolderBreadcrumbSegment[]>
            {
                [North] = new[] { Segment(Q3, "Q3", true), Segment(Q4, "Q4", false) },
                [Q3] = new[]
                {
                    Segment(Invoices, "Invoices", false),
                    Segment(Receipts, "Receipts", true),
                },
                [Root + @"\Clients\South"] = new[]
                {
                    Segment(Root + @"\Clients\South\S1", "S1", true),
                },
            };
            Host.SetupGet(h => h.IsCoreInitialized).Returns(() => CoreInitialized);
            Host.Setup(h => h.NavigateToString(It.IsAny<string>())).Callback<string>(Documents.Add);
            Host.Setup(h => h.PostMessageJson(It.IsAny<string>())).Callback<string>(Posts.Add);
            Provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((string path, CancellationToken token) => Key(path));
            Provider
                .Setup(p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync((FolderTreeNodeKey key, CancellationToken token) => Chain(key));
            Provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    (FolderTreeNodeKey key, CancellationToken token) => ChildrenOf(key.FolderPath)
                );
            Router = new BreadcrumbBridgeRouter(
                Provider.Object,
                Host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(Host.Object)
            );
            Router.SelectedFolderPathChanged += (sender, path) => SelectionEvents.Add(path);
            Router.ConfirmationRequested += (sender, args) => Confirmations++;
        }

        internal Mock<IFolderHierarchyProvider> Provider { get; } =
            new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);

        internal Mock<IBreadcrumbWebHost> Host { get; } =
            new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);

        internal BreadcrumbBridgeRouter Router { get; }

        internal Dictionary<string, FolderBreadcrumbSegment[]> Children { get; }

        internal List<string> Posts { get; } = new List<string>();

        internal List<string> Documents { get; } = new List<string>();

        internal List<string> SelectionEvents { get; } = new List<string>();

        internal int Confirmations { get; private set; }

        internal bool CoreInitialized { get; set; } = true;

        internal Task BindAsync(params string[] rows) =>
            Router.BindRowsAsync(rows, Array.Empty<FolderScore>(), Root, CancellationToken.None);

        internal Task InboundAsync(string type, string rowId) =>
            Router.ProcessInboundAsync("{\"type\":\"" + type + "\",\"rowId\":\"" + rowId + "\"}");

        internal Task ToggleAsync(string rowId, string nodeId) =>
            NodeMessageAsync("descendantToggle", rowId, nodeId);

        internal Task ActivateAsync(string rowId, string nodeId) =>
            NodeMessageAsync("descendantActivate", rowId, nodeId);

        /// <summary>Binds the presented row, expands its leaf, and returns the node id of a child.</summary>
        internal async Task<string> BindAndExpandAsync(string childPath)
        {
            await BindAsync(Presented);
            await InboundAsync("leafExpandToggle", "row-0");
            return NodeId(childPath);
        }

        /// <summary>Expands the leaf and Q3, then returns the node id of the Invoices grandchild.</summary>
        internal async Task<string> ExpandToInvoicesAsync()
        {
            string q3 = await BindAndExpandAsync(Q3);
            await ToggleAsync("row-0", q3);
            return NodeId(Invoices);
        }

        /// <summary>The HTML of the most recent posted render message, JSON-unescaped.</summary>
        internal string LatestRender()
        {
            string post = Posts.LastOrDefault(json => json.Contains("\"type\":\"render\""));
            post.Should().NotBeNull("a render message must have been posted");
            return Regex.Unescape(post);
        }

        /// <summary>Reads the node identity rendered for a folder path from the latest render.</summary>
        internal string NodeId(string folderPath)
        {
            System.Text.RegularExpressions.Match match = Regex.Match(
                LatestRender(),
                "data-node-id=\"([0-9a-f]+)\" data-depth=\"\\d+\" title=\""
                    + Regex.Escape(WebUtility.HtmlEncode(folderPath))
                    + "\""
            );
            match.Success.Should().BeTrue($"the latest render must carry a node for {folderPath}");
            return match.Groups[1].Value;
        }

        internal static FolderTreeNodeKey Key(string path) =>
            new FolderTreeNodeKey("store", path, path);

        internal static FolderBreadcrumbSegment Segment(
            string path,
            string name,
            bool hasChildren
        ) => new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);

        private Task NodeMessageAsync(string type, string rowId, string nodeId) =>
            Router.ProcessInboundAsync(
                "{\"type\":\""
                    + type
                    + "\",\"rowId\":\""
                    + rowId
                    + "\",\"nodeId\":\""
                    + nodeId
                    + "\"}"
            );

        private IReadOnlyList<FolderBreadcrumbSegment> ChildrenOf(string path)
        {
            FolderBreadcrumbSegment[] children;
            return Children.TryGetValue(path, out children)
                ? children
                : Array.Empty<FolderBreadcrumbSegment>();
        }

        // The ancestor chain of Root\Clients\<leaf>: the Clients folder, then the leaf itself.
        private static IReadOnlyList<FolderBreadcrumbSegment> Chain(FolderTreeNodeKey key)
        {
            string leaf = key.FolderPath.Substring(key.FolderPath.LastIndexOf('\\') + 1);
            return new[]
            {
                Segment(Root + @"\Clients", "Clients", true),
                Segment(key.FolderPath, leaf, true),
            };
        }
    }
}
