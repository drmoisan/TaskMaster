#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class BreadcrumbDescendantSelectionIssue997Tests
    {
        internal const string Root = "\\\\mailbox\\Archive";
        internal const string Parent = Root + "\\Projects\\Apollo";
        internal const string Child = Parent + "\\Alpha";

        [TestMethod]
        public async Task ActivateImmediateChild_BoundArchiveRoot_CommitsArchiveRelativeDestination()
        {
            var router = await CreateRouterAsync();
            router.Model.ArchiveRoot = Root;
            router.OpenSelector();
            var transition = router.ActivateSelectorSubfolder(router.Model.Rows[0].Identity, 0);
            transition.SelectionChanged.Should().BeTrue();
            router.GetSelectedFolder().Should().Be("Projects\\Apollo\\Alpha");
            Action validate = () =>
                ArchiveStemContract.RequireArchiveRelativeStem(
                    router.GetSelectedFolder()!,
                    "destination"
                );
            validate.Should().NotThrow();
        }

        [TestMethod]
        public async Task CollapseAncestor_SelectedChild_PreservesCommittedDestination()
        {
            var router = await CreateRouterAsync();
            router.OpenSelector();
            router.ActivateSelectorSubfolder(router.Model.Rows[0].Identity, 0);
            string? committed = router.GetSelectedFolder();
            router.OpenSelector();
            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );
            router.GetSelectedFolder().Should().Be(committed);
        }

        [TestMethod]
        public async Task ReopenThenEscape_SelectedChild_RetainsCommittedDestination()
        {
            var router = await CreateRouterAsync();
            router.OpenSelector();
            router.ActivateSelectorSubfolder(router.Model.Rows[0].Identity, 0);
            string? committed = router.GetSelectedFolder();
            router.OpenSelector();
            router.CancelSelector();
            router.GetSelectedFolder().Should().Be(committed);
        }

        internal static async Task<FolderBreadcrumbBridgeRouter> CreateRouterAsync()
        {
            var parentKey = new FolderTreeNodeKey("store-a", "apollo", Parent);
            var childKey = new FolderTreeNodeKey("store-a", "alpha", Child);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p =>
                    p.ResolveLeafKeyAsync("Projects\\Apollo", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(parentKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(parentKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[] { new FolderBreadcrumbSegment(parentKey, "Apollo", Parent, true) }
                );
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(parentKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[] { new FolderBreadcrumbSegment(childKey, "Alpha", Child, true) }
                );
            var reviewKey = new FolderTreeNodeKey("store-a", "review", Child + "\\Review");
            var leafKey = new FolderTreeNodeKey("store-a", "leaf", Child + "\\Review\\Leaf");
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(childKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        new FolderBreadcrumbSegment(
                            reviewKey,
                            "Review",
                            reviewKey.FolderPath,
                            true
                        ),
                    }
                );
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(reviewKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[]
                    {
                        new FolderBreadcrumbSegment(leafKey, "Leaf", leafKey.FolderPath, false),
                    }
                );
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            await router.SetSuggestionsAsync(
                new[]
                {
                    new FolderRow(
                        "Projects\\Apollo",
                        FolderRowKind.Suggestion,
                        new FolderScore("Projects\\Apollo", 100, 0.73)
                    ),
                },
                CancellationToken.None
            );
            router.SelectRow(0);
            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );
            return router;
        }

        [TestMethod]
        public async Task ActivateIntermediateAndLeaf_ThreeLevels_CommitsKnownRelativeTargets()
        {
            var router = await CreateRouterAsync();
            router.Model.ArchiveRoot = Root;
            var row = router.Model.Rows[0];
            var node = row.DescendantRoot!.Children[0];
            foreach (
                string expected in new[]
                {
                    "Projects\\Apollo\\Alpha",
                    "Projects\\Apollo\\Alpha\\Review",
                    "Projects\\Apollo\\Alpha\\Review\\Leaf",
                }
            )
            {
                router.OpenSelector();
                var activation = router.ActivateSelectorDescendant(row.Identity, node.Identity);
                activation.SelectionChanged.Should().BeTrue();
                activation.OpenStateChanged.Should().BeTrue();
                activation.SelectorState.IsOpen.Should().BeFalse();
                router.GetSelectedFolder().Should().Be(expected);
                ArchiveStemContract.RequireArchiveRelativeStem(
                    router.GetSelectedFolder()!,
                    "destination"
                );
                if (node.Segment.HasChildren)
                {
                    await router.ToggleDescendantAsync(
                        row.Identity,
                        node.Identity,
                        CancellationToken.None
                    );
                    node = node.Children[0];
                }
            }
        }

        [TestMethod]
        public async Task CollapseAndReopen_SelectedDescendant_RendersCommittedSelection()
        {
            var router = await CreateRouterAsync();
            var row = router.Model.Rows[0];
            var node = row.DescendantRoot!.Children[0];
            router.OpenSelector();
            router.ActivateSelectorDescendant(row.Identity, node.Identity);
            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );
            router.OpenSelector();
            var collapsed = (RenderMessage)BreadcrumbBridgeSerializer.Parse(router.RenderJson());
            collapsed.SelectedFolder.Should().Be(Child);
            collapsed.SelectedDescendantIdentity.Should().Be(node.Identity);
            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );
            var expanded = (RenderMessage)BreadcrumbBridgeSerializer.Parse(router.RenderJson());
            expanded.Rows[0].Subfolders[0].Selected.Should().BeTrue();
            router.CancelSelector();
            router.GetSelectedFolder().Should().Be(Child);
        }
    }
}
