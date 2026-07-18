using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Supplementary edge tests for <see cref="FolderBreadcrumbBridgeRouter"/> (#351 P7 coverage gate),
    /// split from <c>FolderBreadcrumbBridgeRouterTests.cs</c> to respect the 500-line file ceiling:
    /// subfolderRequest routing (happy path, plain-row error, auto-expand), unroutable inbound
    /// types, empty-chain fallback, subfolder selection routing, and cancellation propagation.
    /// Moq-mocked provider, completed tasks only; deterministic.
    /// </summary>
    [TestClass]
    public sealed class FolderBreadcrumbBridgeRouterEdgeTests
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

        private static Mock<IFolderHierarchyProvider> ProviderMock()
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(LeafPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(LeafKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        Segment(RootKey, "Inbox", true),
                        Segment(MidKey, "Projects", true),
                        Segment(LeafKey, "Apollo", true),
                    }
                );
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Segment(Key("s1", LeafPath + "\\Alpha"), "Alpha", false) });
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

        [TestMethod]
        public async Task Route_SubfolderRequest_AutoExpandsAndReturnsRenderPlusResponse()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act: the page requests subfolders directly (no prior toggle).
            var outputs = await router.RouteAsync(
                "{\"type\":\"subfolderRequest\",\"rowIndex\":0}",
                CancellationToken.None
            );

            // Assert
            outputs.Should().HaveCount(2);
            BreadcrumbBridgeSerializer.Parse(outputs[0]).Should().BeOfType<RenderMessage>();
            ((SubfolderResponseMessage)BreadcrumbBridgeSerializer.Parse(outputs[1]))
                .Subfolders.Should()
                .ContainSingle(s => s.DisplayName == "Alpha");
        }

        [TestMethod]
        public async Task Route_SubfolderRequest_OnPlainRow_ReturnsError()
        {
            // Arrange
            var router = new FolderBreadcrumbBridgeRouter(
                new Mock<IFolderHierarchyProvider>(MockBehavior.Strict).Object
            );
            router.SetItems(new[] { "Trash to Delete" });

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"subfolderRequest\",\"rowIndex\":0}",
                CancellationToken.None
            );

            // Assert
            ((BridgeErrorMessage)BreadcrumbBridgeSerializer.Parse(outputs.Single()))
                .Message.Should()
                .Contain("plain row");
        }

        [TestMethod]
        public async Task Route_OutboundOnlyInboundType_ReturnsUnroutableError()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act: 'error' parses as a known message but is not routable inbound.
            var outputs = await router.RouteAsync(
                "{\"type\":\"error\",\"message\":\"loopback\"}",
                CancellationToken.None
            );

            // Assert
            ((BridgeErrorMessage)BreadcrumbBridgeSerializer.Parse(outputs.Single()))
                .Message.Should()
                .Contain("not routable");
        }

        [TestMethod]
        public async Task SetSuggestions_NullRows_ThrowsExplicitly()
        {
            // Arrange
            var router = new FolderBreadcrumbBridgeRouter(
                new Mock<IFolderHierarchyProvider>(MockBehavior.Strict).Object
            );

            // Act
            Func<Task> act = () => router.SetSuggestionsAsync(null, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task SetSuggestions_ResolvedKeyButEmptyChain_FallsBackToPlainPathRow()
        {
            // Arrange: the provider resolves the key but returns an empty chain (stale snapshot).
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(LeafPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(LeafKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FolderBreadcrumbSegment[0]);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);

            // Act
            await router.SetSuggestionsAsync(
                new[]
                {
                    new FolderRow(
                        LeafPath,
                        FolderRowKind.Suggestion,
                        new FolderScore(LeafPath, 10, 0.5)
                    ),
                },
                CancellationToken.None
            );

            // Assert (G10: the exact path is still the selection value).
            router.Model.Rows[0].IsSuggestion.Should().BeFalse();
            router.Model.Rows[0].VerbatimText.Should().Be(LeafPath);
        }

        [TestMethod]
        public async Task Route_SelectionChangeWithSubfolderIndex_SelectsTheSubfolder()
        {
            // Arrange: expand first so subfolders exist.
            var router = await PopulatedRouterAsync(ProviderMock());
            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );

            // Act
            var outputs = await router.RouteAsync(
                "{\"type\":\"selectionChange\",\"rowIndex\":0,\"subfolderIndex\":0}",
                CancellationToken.None
            );

            // Assert
            router.Model.SelectedSubfolderIndex.Should().Be(0);
            BreadcrumbSelectionMap
                .GetSelectedFolder(router.Model)
                .Should()
                .Be(LeafPath + "\\Alpha");
            outputs.Should().HaveCount(2);
        }

        [TestMethod]
        public async Task Route_CanceledSubfolderFetch_PropagatesCancellationAndReverts()
        {
            // Arrange: the provider observes a canceled token deterministically.
            var provider = ProviderMock();
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromCanceled<IReadOnlyList<FolderBreadcrumbSegment>>(
                        new CancellationToken(true)
                    )
                );
            var router = await PopulatedRouterAsync(provider);

            // Act
            Func<Task> act = () =>
                router.RouteAsync(
                    "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                    CancellationToken.None
                );

            // Assert: cancellation propagates (never converted to an error response) and the
            // expansion state was reverted for consistency.
            await act.Should().ThrowAsync<OperationCanceledException>();
            router.Model.Rows[0].LeafExpanded.Should().BeFalse();
        }
    }
}
