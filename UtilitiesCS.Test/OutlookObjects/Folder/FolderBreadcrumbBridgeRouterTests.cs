using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for the string-in/string-out <see cref="FolderBreadcrumbBridgeRouter"/> (#351 P3-T8)
    /// with a Moq-mocked <see cref="IFolderHierarchyProvider"/> returning completed tasks only:
    /// positive routing (expand -&gt; render+subfolderResponse, double-click collapse, Right-arrow
    /// expand), negative routing (provider exception -&gt; explicit error; malformed JSON -&gt;
    /// error), and edge fall-throughs (unhandledArrow left/right; theme re-render). The multi-message
    /// state-transition sequences and #398 in-flight rebuild invariants live in the sibling partial
    /// FolderBreadcrumbBridgeRouterInFlightTests.cs. Deterministic; no Outlook, WebView2, timers, or
    /// temp files.
    /// </summary>
    [TestClass]
    public sealed partial class FolderBreadcrumbBridgeRouterTests
    {
        private const string LeafPath = "\\Inbox\\Projects\\Apollo";

        private static readonly FolderTreeNodeKey RootKey = Key("root", "\\Inbox");
        private static readonly FolderTreeNodeKey MidKey = Key("mid", "\\Inbox\\Projects");
        private static readonly FolderTreeNodeKey LeafKey = Key("leaf", LeafPath);

        private static FolderTreeNodeKey Key(string entryId, string path) =>
            new FolderTreeNodeKey("store-a", entryId, path);

        private static FolderBreadcrumbSegment Segment(
            FolderTreeNodeKey key,
            string name,
            bool hasChildren
        ) => new FolderBreadcrumbSegment(key, name, key.FolderPath, hasChildren);

        private static IReadOnlyList<FolderBreadcrumbSegment> LeafChain(
            bool leafHasChildren = true
        ) =>
            new[]
            {
                Segment(RootKey, "Inbox", true),
                Segment(MidKey, "Projects", true),
                Segment(LeafKey, "Apollo", leafHasChildren),
            };

        private static Mock<IFolderHierarchyProvider> ProviderMock(bool leafHasChildren = true)
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(LeafPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(LeafKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(LeafChain(leafHasChildren));
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        Segment(Key("s1", LeafPath + "\\Alpha"), "Alpha", false),
                        Segment(Key("s2", LeafPath + "\\Beta"), "Beta", true),
                    }
                );
            return provider;
        }

        private static async Task<FolderBreadcrumbBridgeRouter> PopulatedRouterAsync(
            Mock<IFolderHierarchyProvider> provider
        )
        {
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            var suggestion = new FolderRow(
                LeafPath,
                FolderRowKind.Suggestion,
                new FolderScore(LeafPath, 1000, 0.73)
            );
            await router.SetSuggestionsAsync(new[] { suggestion }, CancellationToken.None);
            router.SelectRow(0);
            return router;
        }

        // --- Positive routing ---

        [TestMethod]
        public async Task Route_AffordanceToggleExpand_QueriesProviderAndReturnsRenderPlusResponse()
        {
            // Arrange
            var provider = ProviderMock();
            var router = await PopulatedRouterAsync(provider);

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );

            // Assert: render then subfolderResponse, with the real provider children (FR-4).
            outputs.Should().HaveCount(2);
            var render = (RenderMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]);
            render.Rows[0].LeafExpanded.Should().BeTrue();
            var response = (SubfolderResponseMessage)BreadcrumbBridgeSerializer.Parse(outputs[1]);
            response.RowIndex.Should().Be(0);
            response.Subfolders.Should().HaveCount(2);
            response.Subfolders[0].FolderPath.Should().Be(LeafPath + "\\Alpha");
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task Route_SegmentDoubleClick_ProducesCollapsedRenderPayload()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"segmentDoubleClick\",\"rowIndex\":0,\"segmentIndex\":0}",
                CancellationToken.None
            );

            // Assert (FR-3): collapsed row renders plus + terminal segment only.
            outputs.Should().ContainSingle();
            var render = (RenderMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]);
            render.Rows[0].Collapsed.Should().BeTrue();
            render
                .Rows[0]
                .Cells.Should()
                .SatisfyRespectively(
                    plus => plus.Kind.Should().Be(BreadcrumbCellKind.Plus),
                    segment =>
                    {
                        segment.Kind.Should().Be(BreadcrumbCellKind.Segment);
                        segment.Text.Should().Be("Inbox");
                    }
                );
        }

        [TestMethod]
        public async Task Route_RightArrow_ExpandsWhenExpandable()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"arrowKey\",\"direction\":\"right\"}",
                CancellationToken.None
            );

            // Assert: expansion happened and the subfolder query was routed (AC-7).
            outputs.Should().HaveCount(2);
            ((RenderMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
                .Rows[0]
                .LeafExpanded.Should()
                .BeTrue();
            BreadcrumbBridgeSerializer
                .Parse(outputs[1])
                .Should()
                .BeOfType<SubfolderResponseMessage>();
        }

        [TestMethod]
        public async Task Route_SelectionChange_UpdatesModelAndAcksSelection()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"selectionChange\",\"rowIndex\":0}",
                CancellationToken.None
            );

            // Assert
            router.Model.SelectedIndex.Should().Be(0);
            outputs.Should().HaveCount(2);
            var ack = (SelectionChangeMessage)BreadcrumbBridgeSerializer.Parse(outputs[1]);
            ack.RowIndex.Should().Be(0);
            ack.SubfolderIndex.Should().Be(-1);
        }

        // --- Negative routing ---

        [TestMethod]
        public async Task Route_ProviderException_SurfacesExplicitErrorResponseAndRevertsExpansion()
        {
            // Arrange
            var provider = ProviderMock();
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromException<IReadOnlyList<FolderBreadcrumbSegment>>(
                        new InvalidOperationException("store offline")
                    )
                );
            var router = await PopulatedRouterAsync(provider);

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );

            // Assert: explicit error, and the model reverted to a consistent collapsed state.
            outputs.Should().ContainSingle();
            var error = (BridgeErrorMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]);
            error.Message.Should().Contain("store offline");
            router.Model.Rows[0].LeafExpanded.Should().BeFalse();
        }

        [TestMethod]
        public async Task Route_MalformedInboundJson_ReturnsErrorResponse()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync("{broken", CancellationToken.None);

            // Assert
            outputs.Should().ContainSingle();
            BreadcrumbBridgeSerializer
                .Parse(outputs[0])
                .Should()
                .BeOfType<BridgeErrorMessage>()
                .Subject.Message.Should()
                .Contain("Malformed");
        }

        [TestMethod]
        public async Task Route_OutOfRangeRowIndex_ReturnsErrorResponse()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":9}",
                CancellationToken.None
            );

            // Assert
            BreadcrumbBridgeSerializer.Parse(outputs[0]).Should().BeOfType<BridgeErrorMessage>();
        }

        // --- Edge fall-throughs ---

        [TestMethod]
        public async Task Route_RightArrow_NothingToExpand_ReportsUnhandledRight()
        {
            // Arrange: leaf without subfolders -> no affordance -> arrow is unconsumed (FR-6).
            var provider = ProviderMock(leafHasChildren: false);
            var router = await PopulatedRouterAsync(provider);

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"arrowKey\",\"direction\":\"right\"}",
                CancellationToken.None
            );

            // Assert
            outputs.Should().ContainSingle();
            ((UnhandledArrowMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
                .Direction.Should()
                .Be(BreadcrumbArrowDirection.Right);
        }

        [TestMethod]
        public async Task Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"arrowKey\",\"direction\":\"left\"}",
                CancellationToken.None
            );

            // Assert
            outputs.Should().ContainSingle();
            ((UnhandledArrowMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
                .Direction.Should()
                .Be(BreadcrumbArrowDirection.Left);
        }

        [TestMethod]
        public async Task Route_ThemeChange_EchoesThemeAndReRenders()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"themeChange\",\"theme\":\"dark\"}",
                CancellationToken.None
            );

            // Assert
            outputs.Should().HaveCount(2);
            ((ThemeChangeMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
                .Theme.Should()
                .Be("dark");
            BreadcrumbBridgeSerializer.Parse(outputs[1]).Should().BeOfType<RenderMessage>();
        }
    }
}
