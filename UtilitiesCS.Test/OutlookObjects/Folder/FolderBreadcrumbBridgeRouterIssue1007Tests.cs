#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007 QFC router over a strict provider: with the list open, Left and Right act on
    /// the pending entry (AC4, AC5, AC7), the pending arrow is classified for the coordinator, a
    /// shortened row expands its active segment (AC9) and files to it (S9). Rows:
    /// <c>Archive &gt; Projects &gt; Alpha</c> (Alpha: Specs with Drafts, Docs) and
    /// <c>Archive &gt; Clients &gt; Beta</c> (no children).
    /// </summary>
    [TestClass]
    public sealed partial class FolderBreadcrumbBridgeRouterIssue1007Tests
    {
        private const string Archive = @"\Archive";
        private const string Projects = Archive + @"\Projects";
        private const string Alpha = Projects + @"\Alpha";
        private const string Specs = Alpha + @"\Specs";
        private const string Beta = Archive + @"\Clients\Beta";

        /// <summary>AC5: open-list Right on a pending row with children expands that row.</summary>
        [TestMethod]
        public async Task OpenRight_OnPendingRowWithChildren_ExpandsPendingRow()
        {
            // Arrange: Beta committed, Alpha pending.
            var (router, provider) = await RouterAsync();
            router.SelectRow(1);
            router.OpenSelector();
            router.MoveSelector(previous: true);

            // Act
            await ArrowAsync(router, "right");

            // Assert
            router.Model.Rows[0].LeafExpanded.Should().BeTrue();
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(Key(Alpha), It.IsAny<CancellationToken>()),
                Times.Once()
            );
            router.Model.SelectedIndex.Should().Be(1, "the committed choice is unchanged");
        }

        /// <summary>AC5: open-list Right on an expanded pending entry pends its first child.</summary>
        [TestMethod]
        public async Task OpenRight_OnExpandedPendingEntry_PendsFirstChild()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            await ExpandAlphaAsync(router);
            router.SelectRow(0);
            router.OpenSelector();

            // Act
            await ArrowAsync(router, "right");

            // Assert
            router.GetSelectorState().PendingNodeIdentity.Should().Be(NodeOf(router, 0).Identity);
            router.GetSelectorState().IsOpen.Should().BeTrue();
        }

        /// <summary>AC7: open-list Left on an expanded pending entry collapses it; the pending stays.</summary>
        [TestMethod]
        public async Task OpenLeft_OnExpandedPendingEntry_CollapsesAndKeepsPending()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            await ExpandAlphaAsync(router);
            router.SelectRow(0);
            router.OpenSelector();

            // Act
            await ArrowAsync(router, "left");

            // Assert
            router.Model.Rows[0].LeafExpanded.Should().BeFalse();
            router.Model.Rows[0].ActiveSegmentIndex.Should().Be(2, "Left never selects a parent");
            router.GetSelectorState().PendingIdentity.Should().Be(router.Model.Rows[0].Identity);
        }

        /// <summary>AC7: a collapse that hides the pending node moves it to the nearest visible ancestor.</summary>
        [TestMethod]
        public async Task CollapseHidingPending_MovesPendingToNearestVisibleAncestor()
        {
            // Arrange: Specs is expanded and its child Drafts is pending.
            var (router, _) = await RouterAsync();
            await ExpandAlphaAsync(router);
            var specs = NodeOf(router, 0);
            await router.ToggleDescendantAsync(
                router.Model.Rows[0].Identity,
                specs.Identity,
                CancellationToken.None
            );
            router.SelectRow(0);
            router.OpenSelector();
            router.MoveSelector(previous: false);
            router.MoveSelector(previous: false);

            // Act
            await router.ToggleDescendantAsync(
                router.Model.Rows[0].Identity,
                specs.Identity,
                CancellationToken.None
            );

            // Assert
            router.GetSelectorState().PendingNodeIdentity.Should().Be(specs.Identity);
        }

        /// <summary>AC4: open-list Left on a non-expanded pending entry commits and closes.</summary>
        [TestMethod]
        public async Task ClassifyPendingArrow_OpenLeftOnNonExpanded_ReturnsCommitAndClose()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(1);
            var closed = router.ClassifyPendingArrow(BreadcrumbArrowDirection.Left);
            router.OpenSelector();

            // Act
            var open = router.ClassifyPendingArrow(BreadcrumbArrowDirection.Left);

            // Assert
            closed
                .Should()
                .Be(BreadcrumbPendingArrowAction.Route, "a closed list routes as before");
            open.Should().Be(BreadcrumbPendingArrowAction.CommitAndClose);
        }

        /// <summary>AC6: open-list Right on a pending entry without children cancels and pops out.</summary>
        [TestMethod]
        public async Task ClassifyPendingArrow_OpenRightOnLeaf_ReturnsCancelAndPopOut()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(1);
            router.OpenSelector();

            // Act
            var onLeaf = router.ClassifyPendingArrow(BreadcrumbArrowDirection.Right);
            router.MoveSelector(previous: true);
            var onParent = router.ClassifyPendingArrow(BreadcrumbArrowDirection.Right);

            // Assert
            onLeaf.Should().Be(BreadcrumbPendingArrowAction.CancelAndPopOut);
            onParent.Should().Be(BreadcrumbPendingArrowAction.Route);
        }

        /// <summary>AC9: Right on a pending row shortened by Ctrl+Left lists the active segment's children.</summary>
        [TestMethod]
        public async Task ArrowRight_OnShortenedPendingRow_ListsSegmentChildren()
        {
            // Arrange
            var (router, provider) = await RouterAsync();
            router.SelectRow(0);
            router.OpenSelector();
            router.MoveSelectorToParent().Handled.Should().BeTrue();

            // Act
            await ArrowAsync(router, "right");

            // Assert
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(Key(Projects), It.IsAny<CancellationToken>()),
                Times.Once()
            );
            router.Model.Rows[0].LeafExpanded.Should().BeTrue();
            router.Model.Rows[0].CollapsedAfterIndex.Should().Be(1);
        }

        /// <summary>S9 (AC8, AC9): a shortened row files to the folder it displays.</summary>
        [TestMethod]
        public async Task GetSelectedFolder_ShortenedRow_ReturnsActiveSegmentValue()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(0);
            router.Model.Rows[0].ActivateSegment(1).Should().BeTrue();
            router.Model.Rows[0].CollapseAfter(1);

            // Act
            string? selected = router.GetSelectedFolder();

            // Assert
            selected.Should().Be(Projects);
        }

        /// <summary>AC9 (D-Q13, D-Q27): a selector segment double-click activates, commits, shortens and closes.</summary>
        [TestMethod]
        public async Task DoubleClickSelectorSegment_ActivatesCommitsAndShortens()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(1);
            router.OpenSelector();

            // Act
            var transition = router.DoubleClickSelectorSegment(router.Model.Rows[0].Identity, 1);

            // Assert
            transition.Handled.Should().BeTrue();
            transition.SelectionChanged.Should().BeTrue();
            transition.OpenStateChanged.Should().BeTrue();
            transition.SelectorState.IsOpen.Should().BeFalse();
            router.GetSelectedFolder().Should().Be(Projects);
            router.Model.Rows[0].ActiveSegmentIndex.Should().Be(1);
            router.Model.Rows[0].CollapsedAfterIndex.Should().Be(1);
        }

        /// <summary>I1: an unknown row identity or an out-of-range segment index is an unhandled no-op.</summary>
        [TestMethod]
        public async Task DoubleClickSelectorSegment_UnknownRowOrIndex_IsUnhandled()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(0);

            // Act
            var unknownRow = router.DoubleClickSelectorSegment("missing-row", 1);
            var outOfRange = router.DoubleClickSelectorSegment(router.Model.Rows[0].Identity, 5);

            // Assert
            unknownRow.Handled.Should().BeFalse();
            outOfRange.Handled.Should().BeFalse();
            router.Model.Rows[0].CollapsedAfterIndex.Should().BeNull();
            router.GetSelectedFolder().Should().Be(Alpha);
        }

        /// <summary>AC9 (D-Q23): the [+] on a shortened row lists the active segment's children with one query.</summary>
        [TestMethod]
        public async Task AffordanceToggle_OnShortenedRow_ExpandsActiveSegmentWithOneQuery()
        {
            // Arrange
            var (router, provider) = await RouterAsync();
            router
                .DoubleClickSelectorSegment(router.Model.Rows[0].Identity, 1)
                .Handled.Should()
                .BeTrue();

            // Act
            await ExpandAlphaAsync(router);

            // Assert
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(Key(Projects), It.IsAny<CancellationToken>()),
                Times.Once()
            );
            router.Model.Rows[0].LeafExpanded.Should().BeTrue();
            router.Model.Rows[0].CollapsedAfterIndex.Should().Be(1, "the row stays shortened");
        }

        /// <summary>AC9 (D-Q18): an expanded shortened row projects the collapse affordance at its terminal.</summary>
        [TestMethod]
        public async Task ShortenedRowExpanded_ProjectsMinusAtTerminal()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router
                .DoubleClickSelectorSegment(router.Model.Rows[0].Identity, 1)
                .Handled.Should()
                .BeTrue();
            await ExpandAlphaAsync(router);

            // Act
            var cells = BreadcrumbRenderProjection.Project(router.Model)[0].Cells;

            // Assert
            cells.Should().Contain(cell => cell.Kind == BreadcrumbCellKind.Minus);
            cells.Should().NotContain(cell => cell.Kind == BreadcrumbCellKind.Plus);
        }

        /// <summary>S5: the bridge-channel segment double-click is a logged no-op without a state change.</summary>
        [TestMethod]
        public async Task BridgeSegmentDoubleClick_IsLoggedNoOpWithoutStateChange()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(0);

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"segmentDoubleClick\",\"rowIndex\":0,\"segmentIndex\":1}",
                CancellationToken.None
            );

            // Assert
            outputs.Should().BeEmpty();
            router.Model.Rows[0].CollapsedAfterIndex.Should().BeNull();
            router.GetSelectedFolder().Should().Be(Alpha);
        }

        /// <summary>D-Q27: the highlight is open-list only; while open it moves only the pending entry.</summary>
        [TestMethod]
        public async Task HighlightSelector_Closed_IsUnhandled()
        {
            // Arrange
            var (router, _) = await RouterAsync();
            router.SelectRow(0);
            string betaIdentity = router.Model.Rows[1].Identity;

            // Act
            var closed = router.HighlightSelector(betaIdentity);
            router.OpenSelector();
            var open = router.HighlightSelector(betaIdentity);

            // Assert
            closed.Handled.Should().BeFalse();
            open.Handled.Should().BeTrue();
            open.SelectionChanged.Should().BeFalse();
            open.SelectorState.PendingIdentity.Should().Be(betaIdentity);
            router.GetSelectedFolder().Should().Be(Alpha);
        }

        /// <summary>AC5, AC7: open Right expands a pending node with children; Left collapses it, pending kept.</summary>
        [TestMethod]
        public async Task OpenRightLeft_OnPendingNodeWithChildren_ExpandsThenCollapsesNode()
        {
            // Arrange: Specs is pending.
            var (router, provider) = await OpenOnAlphaChildAsync(0);
            var specs = NodeOf(router, 0);

            // Act
            await ArrowAsync(router, "right");
            await ArrowAsync(router, "left");

            // Assert: Right fetched Specs' children (expanded), Left collapsed it again.
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(Key(Specs), It.IsAny<CancellationToken>()),
                Times.Once()
            );
            specs.Expanded.Should().BeFalse();
            router.GetSelectorState().PendingNodeIdentity.Should().Be(specs.Identity);
        }

        /// <summary>AC6, AC7: open Right and Left on a pending node without children are unhandled.</summary>
        [TestMethod]
        public async Task OpenArrows_OnPendingLeafNode_ReportUnhandled()
        {
            // Arrange: Docs is pending.
            var (router, _) = await OpenOnAlphaChildAsync(1);

            // Act
            var right = await ArrowAsync(router, "right");
            var left = await ArrowAsync(router, "left");

            // Assert
            IsUnhandled(right).Should().BeTrue();
            IsUnhandled(left).Should().BeTrue();
        }

        /// <summary>AC14: open Right on an expanded pending node whose fetch returned no children is unhandled.</summary>
        [TestMethod]
        public async Task OpenRight_OnExpandedNodeWithNoChildren_IsUnhandled()
        {
            // Arrange: Specs is expanded with zero children and pending.
            var (router, provider) = await RouterAsync();
            Children(provider, Specs);
            await ExpandAlphaAsync(router);
            var specs = NodeOf(router, 0);
            await router.ToggleDescendantAsync(
                router.Model.Rows[0].Identity,
                specs.Identity,
                CancellationToken.None
            );
            router.SelectRow(0);
            router.OpenSelector();
            router.MoveSelector(previous: false);

            // Act
            var outputs = await ArrowAsync(router, "right");

            // Assert
            IsUnhandled(outputs).Should().BeTrue();
        }

        private static bool IsUnhandled(IReadOnlyList<string> outputs) =>
            BreadcrumbBridgeSerializer.Parse(outputs.Single()) is UnhandledArrowMessage;

        private static async Task<(
            FolderBreadcrumbBridgeRouter,
            Mock<IFolderHierarchyProvider>
        )> OpenOnAlphaChildAsync(int child)
        {
            var (router, provider) = await RouterAsync();
            await ExpandAlphaAsync(router);
            router.SelectRow(0);
            router.OpenSelector();
            for (int step = 0; step <= child; step++)
            {
                router.MoveSelector(previous: false);
            }
            return (router, provider);
        }

        private static async Task<(
            FolderBreadcrumbBridgeRouter,
            Mock<IFolderHierarchyProvider>
        )> RouterAsync()
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            Chain(
                provider,
                Alpha,
                Segment(Archive, true),
                Segment(Projects, true),
                Segment(Alpha, true)
            );
            Chain(
                provider,
                Beta,
                Segment(Archive, true),
                Segment(Archive + @"\Clients", true),
                Segment(Beta, false)
            );
            Children(provider, Alpha, Segment(Specs, true), Segment(Alpha + @"\Docs", false));
            Children(provider, Specs, Segment(Specs + @"\Drafts", false));
            Children(
                provider,
                Projects,
                Segment(Alpha, true),
                Segment(Projects + @"\Omega", false)
            );
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            await router.SetSuggestionsAsync(
                new[] { Row(Alpha, 0.7), Row(Beta, 0.2) },
                CancellationToken.None
            );
            return (router, provider);
        }

        private static Task<IReadOnlyList<string>> ExpandAlphaAsync(
            FolderBreadcrumbBridgeRouter router
        ) =>
            router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );

        private static Task<IReadOnlyList<string>> ArrowAsync(
            FolderBreadcrumbBridgeRouter router,
            string direction
        ) =>
            router.RouteAsync(
                "{\"type\":\"arrowKey\",\"direction\":\"" + direction + "\"}",
                CancellationToken.None
            );

        private static BreadcrumbDescendantNode NodeOf(
            FolderBreadcrumbBridgeRouter router,
            int index
        ) => router.Model.Rows[0].DescendantRoot!.Children[index];

        private static void Chain(
            Mock<IFolderHierarchyProvider> provider,
            string leaf,
            params FolderBreadcrumbSegment[] chain
        )
        {
            provider
                .Setup(p => p.ResolveLeafKeyAsync(leaf, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Key(leaf));
            provider
                .Setup(p => p.GetAncestorChainAsync(Key(leaf), It.IsAny<CancellationToken>()))
                .ReturnsAsync(chain);
        }

        private static void Children(
            Mock<IFolderHierarchyProvider> provider,
            string parent,
            params FolderBreadcrumbSegment[] children
        ) =>
            provider
                .Setup(p =>
                    p.GetImmediateSubfoldersAsync(Key(parent), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(children);

        private static FolderRow Row(string path, double probability) =>
            new FolderRow(path, FolderRowKind.Suggestion, new FolderScore(path, 1000, probability));

        private static FolderTreeNodeKey Key(string path) =>
            new FolderTreeNodeKey("store", path, path);

        private static FolderBreadcrumbSegment Segment(string path, bool hasChildren) =>
            new FolderBreadcrumbSegment(
                Key(path),
                path.Substring(path.LastIndexOf('\\') + 1),
                path,
                hasChildren
            );
    }
}
