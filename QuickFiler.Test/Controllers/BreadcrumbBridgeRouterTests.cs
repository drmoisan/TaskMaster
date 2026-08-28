using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Controllers.Tests;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Happy-path/interaction tests for <see cref="BreadcrumbBridgeRouter"/> (#349) against
    /// Mock&lt;IFolderHierarchyProvider&gt; and Mock&lt;IBreadcrumbWebHost&gt;: bind/render,
    /// collapse, correlated subfolder query, arrows including focusSearch, selection,
    /// SelectFirstRow, and ApplyTheme. Outbound payloads are asserted on their raw JSON text
    /// (QuickFiler.Test deliberately carries no Newtonsoft reference).
    /// </summary>
    [TestClass]
    public partial class BreadcrumbBridgeRouterTests
    {
        private const string LeafPath = "Inbox\\Projects\\Alpha";

        private Mock<IFolderHierarchyProvider> _provider;
        private Mock<IBreadcrumbWebHost> _host;
        private List<string> _navigated;
        private List<string> _posted;
        private BreadcrumbBridgeRouter _router;

        [TestInitialize]
        public void Setup()
        {
            _provider = new Mock<IFolderHierarchyProvider>();
            _host = new Mock<IBreadcrumbWebHost>();
            _navigated = new List<string>();
            _posted = new List<string>();
            _host.SetupGet(h => h.IsCoreInitialized).Returns(true);
            _host
                .Setup(h => h.NavigateToString(It.IsAny<string>()))
                .Callback<string>(html => _navigated.Add(html));
            _host
                .Setup(h => h.PostMessageJson(It.IsAny<string>()))
                .Callback<string>(json => _posted.Add(json));
            SetupProviderChain(LeafPath, true);
            _router = new BreadcrumbBridgeRouter(
                _provider.Object,
                _host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(_host.Object)
            );
        }

        /// <summary>JSON-escapes a fragment the way the codec embeds HTML in a payload.</summary>
        private static string JsonEscaped(string fragment)
        {
            return fragment.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static FolderTreeNodeKey Key(string path)
        {
            return new FolderTreeNodeKey("store-1", "entry", path);
        }

        private static FolderBreadcrumbSegment ProviderSegment(
            string path,
            string name,
            bool hasChildren
        )
        {
            return new FolderBreadcrumbSegment(Key(path), name, path, hasChildren);
        }

        private void SetupProviderChain(string leafPath, bool leafHasChildren)
        {
            IReadOnlyList<FolderBreadcrumbSegment> chain = new[]
            {
                ProviderSegment("Inbox", "Inbox", true),
                ProviderSegment("Inbox\\Projects", "Projects", true),
                ProviderSegment(leafPath, "Alpha", leafHasChildren),
            };
            _provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync((string path, CancellationToken ct) => Key(path));
            _provider
                .Setup(p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(chain);
            _provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(new[] { ProviderSegment(leafPath + "\\Kid", "Kid", false) });
        }

        private void Bind()
        {
            _router
                .BindRowsAsync(
                    new[] { "==== SUGGESTIONS ====", LeafPath },
                    new[] { new FolderScore(LeafPath, 1000, 0.9) },
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
        public void BindRowsAsync_WithInitializedHost_DeliversGeneratedDocument()
        {
            // Act
            Bind();

            // Assert: full document navigated, containing the breadcrumb and joined percent.
            _navigated.Should().HaveCount(1);
            _navigated[0].Should().Contain("<!DOCTYPE html>");
            _navigated[0].Should().Contain("Alpha");
            _navigated[0].Should().Contain("90%");
        }

        [TestMethod]
        public void SegmentDoubleClick_OnNonLeafSegment_CollapsesAndReRenders()
        {
            // Arrange
            Bind();

            // Act: double-click the root segment of the suggestion row (row-1).
            Inbound("{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-1\",\"segmentIndex\":0}");

            // Assert: a row-scoped render fragment with the re-expand affordance was posted.
            string render = _posted.Single(p => p.Contains("\"type\":\"render\""));
            render.Should().Contain("\"rowId\":\"row-1\"");
            render.Should().Contain(JsonEscaped("data-role=\"reexpand\""));
            render.Should().NotContain(">Alpha<");
        }

        [TestMethod]
        public void LeafExpandToggle_IssuesSubfolderQueryAndPostsCorrelatedResult()
        {
            // Arrange
            Bind();

            // Act
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-1\"}");

            // Assert: provider queried once; subfolderResult correlated by requestId.
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            string result = _posted.Single(p => p.Contains("\"type\":\"subfolderResult\""));
            result.Should().Contain("\"requestId\":\"req-1\"");
            result.Should().Contain("\"rowId\":\"row-1\"");
            result.Should().Contain("\"displayName\":\"Kid\"");
        }

        [TestMethod]
        public void ArrowKeyRight_ThenLeft_ExpandsAndCollapses()
        {
            // Arrange
            Bind();

            // Act: Right expands the leaf (provider query), Left collapses the leaf children.
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");

            // Assert: expand posted a minus-affordance fragment, collapse posted a plus fragment.
            List<string> renders = _posted
                .Where(p => p.Contains("\"type\":\"render\"") && p.Contains("\"rowId\":"))
                .ToList();
            renders.Should().HaveCount(2);
            renders[0].Should().Contain("&#8722;");
            renders[1].Should().Contain(JsonEscaped("data-role=\"leaf\">+</span>"));
        }

        /// <summary>
        /// #440 Efc Left: on a multi-segment row the Left arrow attempts the tree transition
        /// first, moving the active segment exactly one step toward the root. The subsequent
        /// expansion is keyed on the parent segment, which is only true when the active index
        /// moved from the leaf to the segment immediately below it.
        /// </summary>
        [TestMethod]
        public void HandleArrowKey_LeftOnMultiSegmentRow_ActivatesParentSegment()
        {
            // Arrange: row-1 carries Inbox -> Projects -> Alpha with the leaf active after bind.
            Bind();
            _provider.Invocations.Clear();

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");
            Inbound("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-1\"}");

            // Assert
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(k => k.FolderPath == "Inbox\\Projects"),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(k => k.FolderPath == LeafPath),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        /// <summary>
        /// #440 Efc Left: repeated Left presses walk the active segment to the root, and the
        /// press that ActivateSegment refuses falls through (decision D1) to the pre-existing
        /// row.LeftArrow() collapse behavior.
        /// </summary>
        [TestMethod]
        public void HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior()
        {
            // Arrange
            Bind();

            // Act: two presses walk 2 -> 1 -> 0; the third is refused and falls through.
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");

            // Assert: only the third render carries the collapse (re-expand) affordance.
            List<string> renders = _posted
                .Where(p => p.Contains("\"type\":\"render\"") && p.Contains("\"rowId\":\"row-1\""))
                .ToList();
            renders.Should().HaveCount(3);
            renders[0].Should().NotContain(JsonEscaped("data-role=\"reexpand\""));
            renders[1].Should().NotContain(JsonEscaped("data-role=\"reexpand\""));
            renders[2].Should().Contain(JsonEscaped("data-role=\"reexpand\""));
            renders[2].Should().NotContain(">Alpha<");
        }

        /// <summary>
        /// #440 Efc Right on a COLLAPSED row whose activated segment is a non-leaf parent: the
        /// tree transition is attempted before row.ReExpand(), so exactly one
        /// GetImmediateSubfoldersAsync call keyed on the active segment is issued and no
        /// ResolveLeafKeyAsync call is added on the expansion path.
        /// </summary>
        [TestMethod]
        public void HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall()
        {
            // Arrange: collapse after segment 1, then activate segment 1. ActivateSegment leaves
            // CollapsedAfterIndex untouched, so the row is still collapsed when Right arrives.
            Bind();
            _provider.Invocations.Clear();
            Inbound("{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-1\",\"segmentIndex\":1}");
            Inbound("{\"type\":\"segmentActivate\",\"rowId\":\"row-1\",\"segmentIndex\":1}");
            _provider.Invocations.Clear();

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");

            // Assert
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(k => k.FolderPath == "Inbox\\Projects"),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
            _provider.Verify(
                p => p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never()
            );
        }

        /// <summary>
        /// #440 Efc Right after the active segment is already expanded: the descent mechanism
        /// chosen by decision D9 selects child index 0 through BreadcrumbRow.GetActiveChild(0).
        /// When GetActiveChild(0) returns null the descent is unavailable and the decision-D1
        /// fall-through runs, leaving the selection unchanged.
        /// </summary>
        [TestMethod]
        public void HandleArrowKey_RightAfterExpansion_DescendsByChildActivation()
        {
            // Arrange: activate the non-leaf parent, then expand it.
            Bind();
            Inbound("{\"type\":\"segmentActivate\",\"rowId\":\"row-1\",\"segmentIndex\":1}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");

            // Act: the next Right descends to child index 0.
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");

            // Assert
            _router.SelectedFolderPath.Should().Be("Inbox\\Projects\\Alpha\\Kid");

            // Arrange: same gesture where the active segment expands to no children at all.
            _provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(new FolderBreadcrumbSegment[0]);
            Bind();
            Inbound("{\"type\":\"segmentActivate\",\"rowId\":\"row-1\",\"segmentIndex\":1}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");

            // Assert: GetActiveChild(0) returned null, so the selection is unchanged.
            _router.SelectedFolderPath.Should().Be("Inbox\\Projects");
        }

        /// <summary>
        /// #440 decision D1 (handling order): a row whose resolved chain has exactly one segment
        /// has no tree transition available, so Right and Left both take the pre-existing expand
        /// and collapse path and, where none applies, the pre-existing unhandled fall-through.
        /// </summary>
        [TestMethod]
        public void ArrowKey_SingleSegmentRow_TakesPreExistingCollapsePath()
        {
            // Arrange: row-2 resolves to an unknown chain, so it renders as one segment.
            BindThreeRows();
            _provider.Invocations.Clear();

            // Act
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-2\",\"key\":\"Right\"}");
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-2\",\"key\":\"Left\"}");

            // Assert: no tree transition, so no provider expansion and no state change to render.
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );
            _posted
                .Should()
                .NotContain(p =>
                    p.Contains("\"type\":\"render\"") && p.Contains("\"rowId\":\"row-2\"")
                );
        }

        /// <summary>
        /// #440 decision D2 (Efc boundaries): Left at the root and Right on a childless active
        /// node remain silent no-ops. The childless early return in ExpandLeafAsync now tests the
        /// ACTIVE segment rather than the leaf, and ActivateSegment's root refusal is unchanged.
        /// </summary>
        [TestMethod]
        public void Boundary_EfcLeftAtRootAndRightOnChildlessNode_RemainSilentNoOps()
        {
            // Arrange: the predicted leaf has no subfolders, so the active node is childless.
            SetupProviderChain(LeafPath, leafHasChildren: false);
            Bind();
            _provider.Invocations.Clear();
            int postedBeforeRight = _posted.Count;

            // Act + Assert: Right on the childless active node issues no query and no render.
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Right\"}");
            _provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never()
            );
            _posted.Count.Should().Be(postedBeforeRight);

            // Act: four Lefts walk the active node to the root and then collapse to the root.
            for (int i = 0; i < 4; i++)
            {
                Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");
            }
            int postedAfterWalk = _posted.Count;

            // Act + Assert: the fifth Left is refused at the root and posts nothing.
            Inbound("{\"type\":\"arrowKey\",\"rowId\":\"row-1\",\"key\":\"Left\"}");
            _posted.Count.Should().Be(postedAfterWalk);
        }

        /// <summary>
        /// #440 decision D2 (Qfc boundaries): an unhandled arrow still reaches
        /// IQfcKeyboardHandler.BreadcrumbArrowFallThrough at the QfcItemController call site. Only
        /// the interface is mocked, so the modal MyBox.ShowDialog inside the concrete
        /// KeyboardHandler is never constructed and cannot block the run.
        /// </summary>
        [TestMethod]
        public void Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            try
            {
                using (var viewer = new QuickFiler.ItemViewer())
                {
                    var keyboard = new Mock<IQfcKeyboardHandler>(MockBehavior.Strict);
                    keyboard.Setup(handler =>
                        handler.BreadcrumbArrowFallThrough(viewer, BreadcrumbArrowDirection.Left)
                    );
                    var controller = new HarnessController();
                    QfcItemControllerTestSupport.SetField(
                        controller,
                        "_kbdHandler",
                        keyboard.Object
                    );
                    MethodInfo method = typeof(QfcItemController).GetMethod(
                        "OnBreadcrumbUnhandledArrow",
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );

                    // Act
                    method.Invoke(
                        controller,
                        new object[] { viewer, BreadcrumbArrowDirection.Left }
                    );

                    // Assert
                    keyboard.Verify(
                        handler =>
                            handler.BreadcrumbArrowFallThrough(
                                viewer,
                                BreadcrumbArrowDirection.Left
                            ),
                        Times.Once()
                    );
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}
