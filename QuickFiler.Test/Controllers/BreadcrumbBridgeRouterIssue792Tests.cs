using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    /// Regression tests for issue #792 on <see cref="BreadcrumbBridgeRouter"/>: a failed
    /// CoreWebView2 initialization discards the stashed document, clears the selection and
    /// navigates the error banner; a later initialization must not replay the stale stash. The
    /// arrangement mirrors <c>BreadcrumbBridgeRouterQueueTests</c>. No timers, sleeps or temp files.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeRouterIssue792Tests
    {
        private const string LeafPath = "Inbox\\Projects\\Alpha";
        private const string RowSelectedPayload = "{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}";
        private const string ErrorBannerToken = "Folder list unavailable";
        private const string StaleRowToken = "Alpha";

        private Mock<IFolderHierarchyProvider> _provider;
        private Mock<IBreadcrumbWebHost> _host;
        private bool _initialized;
        private List<string> _navigated;
        private List<string> _posted;
        private BreadcrumbBridgeRouter _router;

        [TestInitialize]
        public void Setup()
        {
            _provider = new Mock<IFolderHierarchyProvider>();
            _host = new Mock<IBreadcrumbWebHost>();
            _initialized = false;
            _navigated = new List<string>();
            _posted = new List<string>();
            _host.SetupGet(h => h.IsCoreInitialized).Returns(() => _initialized);
            _host
                .Setup(h => h.NavigateToString(It.IsAny<string>()))
                .Callback<string>(html => _navigated.Add(html));
            _host
                .Setup(h => h.PostMessageJson(It.IsAny<string>()))
                .Callback<string>(json => _posted.Add(json));
            _provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    (string path, CancellationToken ct) =>
                        new FolderTreeNodeKey("store-1", "entry", path)
                );
            _provider
                .Setup(p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new[] { Segment("Inbox", "Inbox", true), Segment(LeafPath, "Alpha", true) }
                );
            _router = new BreadcrumbBridgeRouter(
                _provider.Object,
                _host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(_host.Object)
            );
        }

        private static FolderBreadcrumbSegment Segment(string path, string name, bool hasChildren)
        {
            return new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store-1", "entry", path),
                name,
                path,
                hasChildren
            );
        }

        private void Bind()
        {
            _router
                .BindRowsAsync(
                    new[] { LeafPath },
                    Enumerable.Empty<FolderScore>(),
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
        }

        private void Inbound(string json)
        {
            _router.ProcessInboundAsync(json).GetAwaiter().GetResult();
        }

        /// <summary>
        /// AC-U1/AC-U2: with a stashed document and a live selection, the failure entry point
        /// navigates exactly one document (the banner, not the stale folder document) and clears
        /// the selection, notifying subscribers with null. Before the fix the declaration-only body
        /// navigates nothing, so the first assertion fails.
        /// </summary>
        [TestMethod]
        public void NotifyInitializationFailed_ClearsThePendingDocumentAndNavigatesTheErrorBanner()
        {
            // Arrange: the uninitialized host stashes the bound document; a selection exists.
            Bind();
            _navigated.Should().BeEmpty("the bound document is stashed while uninitialized");
            Inbound(RowSelectedPayload);
            _router.SelectedFolderPath.Should().NotBeNull("a selection must exist to be cleared");
            string observed = "sentinel";
            _router.SelectedFolderPathChanged += (s, path) => observed = path;

            // Act
            _router.NotifyInitializationFailed(new InvalidOperationException("boom"));

            // Assert
            _navigated
                .Should()
                .ContainSingle("the failure must navigate exactly one document, the error banner")
                .Which.Should()
                .Contain(ErrorBannerToken, "the navigated document must be the error banner")
                .And.NotContain(StaleRowToken, "the stashed folder document must be discarded");
            _router
                .SelectedFolderPath.Should()
                .BeNull("a failed initialization clears the selection");
            observed.Should().BeNull("clearing a selection must notify subscribers with null");
        }

        /// <summary>
        /// AC-U2: after the failure, a later initialization must find no stash to replay. Before
        /// the fix the stash survives, so the only navigation is the stale folder document, which
        /// contains the leaf name: the count assertion passes and the content assertion fails.
        /// </summary>
        [TestMethod]
        public void NotifyInitializationFailed_LeavesNoStashForALaterInitialization()
        {
            // Arrange
            Bind();
            Inbound(RowSelectedPayload);

            // Act
            _router.NotifyInitializationFailed(new InvalidOperationException("boom"));
            _initialized = true;
            _router.NotifyCoreInitialized();

            // Assert
            _navigated.Should().HaveCount(1, "only the error banner may be navigated");
            _navigated[0]
                .Should()
                .Contain(ErrorBannerToken, "the single navigation must be the error banner")
                .And.NotContain(StaleRowToken, "the stale stash must not be replayed");
        }

        /// <summary>Control: a null failure is rejected at the boundary.</summary>
        [TestMethod]
        public void NotifyInitializationFailed_WithNullFailure_Throws()
        {
            // Arrange
            Action act = () => _router.NotifyInitializationFailed(null);

            // Act and Assert
            act.Should()
                .Throw<ArgumentNullException>("the failure is required")
                .Which.ParamName.Should()
                .Be("failure");
        }

        /// <summary>Retained-behaviour control: a stash is still delivered on initialization.</summary>
        [TestMethod]
        public void NotifyCoreInitialized_AfterAnEarlierStash_StillNavigatesIt()
        {
            // Arrange
            Bind();
            _initialized = true;

            // Act
            _router.NotifyCoreInitialized();

            // Assert
            _navigated
                .Should()
                .ContainSingle("the stashed document is delivered once on initialization")
                .Which.Should()
                .Contain(StaleRowToken, "the delivered document is the bound folder document");
        }
    }
}
