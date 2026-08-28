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
    /// Negative/edge-path tests for <see cref="BreadcrumbBridgeRouter"/> and
    /// <see cref="BreadcrumbOutboundQueue"/> (#349): pre-initialization queueing and in-order
    /// flush, provider failure and cancellation leaving state unchanged, malformed inbound JSON
    /// rejection without state corruption, and idempotent duplicate initialization completion.
    /// No timers, sleeps, or temp files are used.
    /// </summary>
    [TestClass]
    public partial class BreadcrumbBridgeRouterQueueTests
    {
        private const string LeafPath = "Inbox\\Projects\\Alpha";

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

        [TestMethod]
        public void OutboundPayloads_BeforeInitialization_AreQueuedAndFlushedInOrder()
        {
            // Arrange: host core not initialized; bind defers the document.
            Bind();
            _navigated.Should().BeEmpty("NavigateToString requires an initialized core");

            // Act: selection posts a render payload, Up-at-top posts focusSearch — both buffered.
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-0\",\"key\":\"Up\"}");
            _posted.Should().BeEmpty("payloads must buffer while IsCoreInitialized is false");

            // Initialization completes.
            _initialized = true;
            _router.NotifyCoreInitialized();

            // Assert: deferred document delivered, then payloads flushed in enqueue order.
            _navigated.Should().HaveCount(1);
            _posted.Should().HaveCount(2);
            _posted[0].Should().Contain("\"type\":\"render\"");
            _posted[1].Should().Contain("\"type\":\"focusSearch\"");
        }

        [TestMethod]
        public void ProviderFailure_OnLeafExpand_LeavesRowStateUnchanged()
        {
            // Arrange: initialized host, bound rows, faulted subfolder task.
            _initialized = true;
            Bind();
            _provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new InvalidOperationException("provider down"));
            int postedBefore = _posted.Count;

            // Act
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}");

            // Assert: no subfolderResult, no render update — the row stays collapsed/unchanged.
            _posted.Count.Should().Be(postedBefore);
            _posted.Should().NotContain(p => p.Contains("\"type\":\"subfolderResult\""));
        }

        [TestMethod]
        public void CanceledProviderCall_OnLeafExpand_LeavesRowStateUnchanged()
        {
            // Arrange
            _initialized = true;
            Bind();
            _provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(new OperationCanceledException());
            int postedBefore = _posted.Count;

            // Act
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}");

            // Assert
            _posted.Count.Should().Be(postedBefore);
            _posted.Should().NotContain(p => p.Contains("\"type\":\"subfolderResult\""));
        }

        [TestMethod]
        public void MalformedInboundJson_ThrowsCodecExceptionWithoutCorruptingState()
        {
            // Arrange
            _initialized = true;
            Bind();

            // Act: malformed payload is rejected via the codec's specific exception.
            Action act = () => Inbound("{not valid json");

            // Assert
            act.Should().Throw<BreadcrumbMessageException>();
            _router.SelectedFolderPath.Should().BeNull();

            // Router state is intact: a subsequent valid selection still works.
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");
            _router.SelectedFolderPath.Should().Be(LeafPath);
        }

        [TestMethod]
        public void MalformedInboundJson_ViaHostEvent_IsContainedAtTheBoundary()
        {
            // Arrange: the async void host-event boundary catches only the codec exception.
            _initialized = true;
            Bind();

            // Act: raising the host event with a malformed payload must not throw or corrupt state.
            _host.Raise(h => h.MessageReceived += null, _host.Object, "{not valid json");

            // Assert
            _router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public void OutboundQueue_NullArguments_ThrowArgumentNullException()
        {
            // Act / Assert: queue construction and posting fail fast on nulls.
            ((Action)(() => new BreadcrumbOutboundQueue(null)))
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("host");
            var queue = new BreadcrumbOutboundQueue(_host.Object);
            ((Action)(() => queue.PostOrQueue(null)))
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("json");
        }

        [TestMethod]
        public void RouterConstructor_NullCollaborators_ThrowArgumentNullException()
        {
            // Arrange
            var codec = new BreadcrumbMessageCodec();
            var renderer = new BreadcrumbHtmlRenderer();
            var queue = new BreadcrumbOutboundQueue(_host.Object);

            // Act / Assert: every collaborator seam is required.
            (
                (Action)(
                    () => new BreadcrumbBridgeRouter(null, _host.Object, codec, renderer, queue)
                )
            )
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("provider");
            (
                (Action)(
                    () => new BreadcrumbBridgeRouter(_provider.Object, null, codec, renderer, queue)
                )
            )
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("host");
            (
                (Action)(
                    () =>
                        new BreadcrumbBridgeRouter(
                            _provider.Object,
                            _host.Object,
                            null,
                            renderer,
                            queue
                        )
                )
            )
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("codec");
            (
                (Action)(
                    () =>
                        new BreadcrumbBridgeRouter(
                            _provider.Object,
                            _host.Object,
                            codec,
                            null,
                            queue
                        )
                )
            )
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("renderer");
            (
                (Action)(
                    () =>
                        new BreadcrumbBridgeRouter(
                            _provider.Object,
                            _host.Object,
                            codec,
                            renderer,
                            null
                        )
                )
            )
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("outboundQueue");
        }

        [TestMethod]
        public void BindRowsAsync_NullPresentedRows_ThrowsArgumentNullException()
        {
            // Act
            Action act = () =>
                _router
                    .BindRowsAsync(null, new FolderScore[0], CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("presentedRows");
        }

        [TestMethod]
        public void InboundMessage_ForUnknownRowId_IsLoggedNoOp()
        {
            // Arrange
            _initialized = true;
            Bind();
            int postedBefore = _posted.Count;

            // Act
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-99\"}");

            // Assert: unknown targets change nothing.
            _posted.Count.Should().Be(postedBefore);
            _router.SelectedFolderPath.Should().BeNull();
        }

        [TestMethod]
        public void Bind_WhenLeafKeyUnresolved_FallsBackToSingleSegmentRow()
        {
            // Arrange: the provider cannot resolve the presented path.
            _initialized = true;
            _provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((FolderTreeNodeKey)null);

            // Act
            Bind();

            // Assert: the presented path renders as a single leaf-only segment.
            _navigated.Should().HaveCount(1);
            _navigated[0].Should().Contain(">Alpha<");
            _provider.Verify(
                p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public void Bind_WhenProviderCanceled_RendersSingleSegmentFallback()
        {
            // Arrange: a provider cancellation is a diagnosable binding fallback.
            _initialized = true;
            _provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ThrowsAsync(new OperationCanceledException());

            // Act
            Bind();

            // Assert: binding preserves the selectable presented target without hierarchy data.
            _navigated.Should().ContainSingle().Which.Should().Contain(">Alpha<");
            _provider.Verify(
                p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public void LeafExpand_UsesBoundActiveSegmentKeyWithoutResolvingAgain()
        {
            // Arrange: binding captures the hierarchy key for the active leaf segment.
            _initialized = true;
            Bind();
            _provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(key => key.FolderPath == LeafPath),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Array.Empty<FolderBreadcrumbSegment>());
            int postedBefore = _posted.Count;

            // Act
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}");

            // Assert: expansion uses the key captured at bind time and emits its two updates.
            _posted.Count.Should().Be(postedBefore + 2);
            _provider.Verify(
                p => p.ResolveLeafKeyAsync(LeafPath, It.IsAny<CancellationToken>()),
                Times.Once
            );
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(key => key.FolderPath == LeafPath),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        [TestMethod]
        public void LeafExpand_OnLeafWithoutSubfolders_IsNoOpWithoutProviderQuery()
        {
            // Arrange: chain whose leaf has no children.
            _initialized = true;
            _provider
                .Setup(p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(new[] { Segment(LeafPath, "Alpha", false) });
            Bind();
            int postedBefore = _posted.Count;

            // Act
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}");

            // Assert
            _posted.Count.Should().Be(postedBefore);
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        [TestMethod]
        public void DuplicateInitializationCompletion_IsIdempotent()
        {
            // Arrange: queue one payload pre-init, then complete initialization.
            Bind();
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");
            _initialized = true;
            _router.NotifyCoreInitialized();
            int navigatedAfterFirst = _navigated.Count;
            int postedAfterFirst = _posted.Count;

            // Act: pooled-viewer re-init raises completion again.
            _router.NotifyCoreInitialized();

            // Assert: no duplicate navigation and no re-posted payloads.
            _navigated.Count.Should().Be(navigatedAfterFirst);
            _posted.Count.Should().Be(postedAfterFirst);
        }
    }
}
