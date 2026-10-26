using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using F = QuickFiler.Test.Controllers.EfcDescendantRouterFixture;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #1007 EFC keyboard navigation through the real <c>BreadcrumbBridgeRouter</c> over the
    /// strict-mock <see cref="EfcDescendantRouterFixture"/>: vertical keys (Navigation partial),
    /// horizontal keys (this part) and segment double-click (DoubleClick partial). Node identities
    /// are read from the rendered <c>data-node-id</c> attributes, never asserted as literals.
    /// </summary>
    [TestClass]
    public sealed partial class BreadcrumbBridgeRouterIssue1007Tests
    {
        /// <summary>AC5: Right on an expanded row selects its first descendant and highlights it.</summary>
        [TestMethod]
        public async Task ArrowRight_OnExpandedRow_SelectsFirstDescendantWithNodeId()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
            SelectedNodeTitle(fixture).Should().Be(F.Q3);
        }

        /// <summary>AC5: Right on a selected subfolder that has children expands it.</summary>
        [TestMethod]
        public async Task ArrowRight_OnUnexpandedNodeWithChildren_ExpandsNode()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            await fixture.ActivateAsync("row-0", fixture.NodeId(F.Q3));

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            VerifyFetched(fixture, F.Q3, Times.Once());
            fixture.LatestRender().Should().Contain(">Invoices</span>");
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
        }

        /// <summary>AC5: Right on an expanded subfolder moves to its first child.</summary>
        [TestMethod]
        public async Task ArrowRight_OnExpandedNode_SelectsFirstChildNode()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            string q3 = fixture.NodeId(F.Q3);
            await fixture.ToggleAsync("row-0", q3);
            await fixture.ActivateAsync("row-0", q3);

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3\Invoices");
        }

        /// <summary>S7: a second Right during the row fetch issues no query and the row ends expanded.</summary>
        [TestMethod]
        public async Task ArrowRight_ReentrantDuringFetch_EndsExpandedWithOneQuery()
        {
            // Arrange: hold the North fetch open.
            var fixture = await BoundAsync(North, South);
            var pending = Hold(fixture, F.North);
            await SelectAsync(fixture, "row-0");

            // Act: the second Right arrives while the first fetch is in flight. Neither key is
            // awaited before the fetch is released, so a second fetch cannot block the test.
            Task first = KeyAsync(fixture, "row-0", "Right");
            Task second = KeyAsync(fixture, "row-0", "Right");
            pending.SetResult(fixture.Children[F.North]);
            await Task.WhenAll(first, second);

            // Assert
            VerifyFetched(fixture, F.North, Times.Once());
            fixture.LatestRender().Should().Contain(">Q3</span>", "the row ends expanded");
        }

        /// <summary>AC7: Left on an expanded row collapses it and keeps the row selected.</summary>
        [TestMethod]
        public async Task ArrowLeft_OnExpandedRow_CollapsesAndKeepsSelection()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            await ExpandAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "Left");

            // Assert: collapsed, still on North; Right re-expands North, not a parent segment.
            fixture.LatestRender().Should().NotContain(">Q3</span>");
            fixture.Router.SelectedFolderPath.Should().Be(North);
            await KeyAsync(fixture, "row-0", "Right");
            fixture.LatestRender().Should().Contain(">Q3</span>");
        }

        /// <summary>AC7: Left on an expanded subfolder collapses only that subfolder.</summary>
        [TestMethod]
        public async Task ArrowLeft_OnExpandedNode_CollapsesOnlyThatNode()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            string q3 = fixture.NodeId(F.Q3);
            await fixture.ToggleAsync("row-0", q3);
            await fixture.ActivateAsync("row-0", q3);

            // Act
            await KeyAsync(fixture, "row-0", "Left");

            // Assert
            string html = fixture.LatestRender();
            html.Should().NotContain(">Invoices</span>");
            html.Should().Contain(">Q4</span>", "the row stays expanded");
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
        }

        /// <summary>AC7: Left on a non-expanded row does nothing (no parent walk, no hiding).</summary>
        [TestMethod]
        public async Task ArrowLeft_OnNonExpandedRow_IsNoOp()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");
            int before = fixture.Posts.Count;

            // Act
            await KeyAsync(fixture, "row-0", "Left");

            // Assert
            fixture.Posts.Count.Should().Be(before);
            fixture.Router.SelectedFolderPath.Should().Be(North);
        }

        /// <summary>AC7: Left on a collapsed subfolder does nothing.</summary>
        [TestMethod]
        public async Task ArrowLeft_OnCollapsedNode_IsNoOp()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            await fixture.ActivateAsync("row-0", fixture.NodeId(F.Q3));
            int before = fixture.Posts.Count;

            // Act
            await KeyAsync(fixture, "row-0", "Left");

            // Assert
            fixture.Posts.Count.Should().Be(before);
        }

        /// <summary>AC7 (S1): a collapse that hides the selection re-commits the nearest visible ancestor.</summary>
        [TestMethod]
        public async Task CollapseHidingSelection_RecommitsNearestVisibleAncestor()
        {
            // Arrange: Invoices (under Q3) is selected.
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            string q3 = fixture.NodeId(F.Q3);
            await fixture.ToggleAsync("row-0", q3);
            await fixture.ActivateAsync("row-0", fixture.NodeId(F.Invoices));

            // Act / Assert: collapsing Q3 moves the selection to Q3.
            await fixture.ToggleAsync("row-0", q3);
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
            SelectedNodeTitle(fixture).Should().Be(F.Q3);

            // Act / Assert: collapsing the row hides Q3, so the row itself is re-committed.
            await ExpandAsync(fixture, "row-0");
            fixture.Router.SelectedFolderPath.Should().Be(North);
        }

        /// <summary>AC8: Ctrl+Left on a row activates the parent segment, commits it and shortens.</summary>
        [TestMethod]
        public async Task ParentLeft_OnRow_ActivatesParentSegmentCommitsAndShortens()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "ParentLeft");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be("Clients");
            string row = fixture.LatestRender();
            row.Should().Contain("data-role=\"reexpand\"");
            row.Should().NotContain(">North</span>");
        }

        /// <summary>AC8: Ctrl+Left on a subfolder selects its parent and leaves the parent expanded.</summary>
        [TestMethod]
        public async Task ParentLeft_OnNode_SelectsParentKeepingExpansion()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            await fixture.ToggleAsync("row-0", fixture.NodeId(F.Q3));
            await fixture.ActivateAsync("row-0", fixture.NodeId(F.Invoices));

            // Act
            await KeyAsync(fixture, "row-0", "ParentLeft");

            // Assert
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
            fixture.LatestRender().Should().Contain(">Invoices</span>");
        }

        /// <summary>Guard (AC5): Right on a non-expanded row with children expands it.</summary>
        [TestMethod]
        public async Task ArrowRight_OnUnexpandedRowWithChildren_Expands()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await SelectAsync(fixture, "row-0");

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            VerifyFetched(fixture, F.North, Times.Once());
            fixture.LatestRender().Should().Contain(">Q3</span>");
        }

        /// <summary>Guard (decision table): Right on an expanded subfolder whose fetch is in flight does nothing.</summary>
        [TestMethod]
        public async Task ArrowRight_OnExpandedNodeWithFetchInFlight_DoesNothing()
        {
            // Arrange: Q3 is selected and its expansion fetch is held open.
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            string q3 = fixture.NodeId(F.Q3);
            await fixture.ActivateAsync("row-0", q3);
            var pending = Hold(fixture, F.Q3);
            Task toggle = fixture.ToggleAsync("row-0", q3);

            // Act: the key is not awaited before the fetch is released.
            Task right = KeyAsync(fixture, "row-0", "Right");
            pending.SetResult(fixture.Children[F.Q3]);
            await Task.WhenAll(toggle, right);

            // Assert
            VerifyFetched(fixture, F.Q3, Times.Once());
            fixture.Router.SelectedFolderPath.Should().Be(@"Clients\North\Q3");
        }

        /// <summary>Guard (AC6): Right on a subfolder without children posts nothing.</summary>
        [TestMethod]
        public async Task ArrowRight_OnLeaf_PostsNothing()
        {
            // Arrange
            var fixture = await BoundAsync(North, South);
            await ExpandAsync(fixture, "row-0");
            await fixture.ActivateAsync("row-0", fixture.NodeId(F.Q4));
            int before = fixture.Posts.Count;

            // Act
            await KeyAsync(fixture, "row-0", "Right");

            // Assert
            fixture.Posts.Count.Should().Be(before);
        }

        /// <summary>Guard (AC8): Ctrl+Left with the first segment active does nothing.</summary>
        [TestMethod]
        public async Task ParentLeft_AtFirstSegment_IsNoOp()
        {
            // Arrange: activate segment 0 (Clients).
            var fixture = await BoundAsync(North, South);
            await fixture.Router.ProcessInboundAsync(
                "{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
            );
            int before = fixture.Posts.Count;

            // Act
            await KeyAsync(fixture, "row-0", "ParentLeft");

            // Assert
            fixture.Posts.Count.Should().Be(before);
            fixture.Router.SelectedFolderPath.Should().Be("Clients");
        }

        // Holds the provider fetch for one folder on a TaskCompletionSource (no real wait).
        private static TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>> Hold(
            EfcDescendantRouterFixture fixture,
            string path
        )
        {
            var pending = new TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>>();
            fixture
                .Provider.Setup(p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(k => k.FolderPath == path),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(pending.Task);
            return pending;
        }

        private static void VerifyFetched(
            EfcDescendantRouterFixture fixture,
            string path,
            Times times
        ) =>
            fixture.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        It.Is<FolderTreeNodeKey>(k => k.FolderPath == path),
                        It.IsAny<CancellationToken>()
                    ),
                times
            );
    }
}
