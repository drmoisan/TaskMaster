#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class BreadcrumbDescendantValidationIssue997Tests
    {
        [TestMethod]
        public async Task UnknownUnloadedAndCrossRowAddresses_CannotFetchOrCommit()
        {
            var router = await BreadcrumbDescendantSelectionIssue997Tests.CreateRouterAsync();
            var first = router.Model.Rows[0];
            router.Model.AddSuggestionRow("other-row", first.Chain, 0.5);
            router.Model.AddPlainRow("plain-row", "Recent folder", true);
            router.OpenSelector();
            string? original = router.GetSelectedFolder();
            foreach (
                var address in new[]
                {
                    ("missing", first.DescendantRoot!.Children[0].Identity),
                    (first.Identity, "unloaded-node"),
                    ("other-row", first.DescendantRoot.Children[0].Identity),
                    ("plain-row", "unloaded-node"),
                }
            )
            {
                var activation = router.ActivateSelectorDescendant(address.Item1, address.Item2);
                activation.Handled.Should().BeFalse();
                activation.SelectionChanged.Should().BeFalse();
                activation.OpenStateChanged.Should().BeFalse();
                (
                    await router.ToggleDescendantAsync(
                        address.Item1,
                        address.Item2,
                        CancellationToken.None
                    )
                )
                    .Should()
                    .BeEmpty();
                router.GetSelectedFolder().Should().Be(original);
                router.GetSelectorState().IsOpen.Should().BeTrue();
            }
        }

        [DataTestMethod]
        [DataRow("\\\\mailbox\\Archive2\\Alpha")]
        [DataRow("\\\\mailbox\\Archive")]
        public async Task BoundRoot_InvalidProviderTarget_CannotExpandOrCommit(string invalidPath)
        {
            var router = await BreadcrumbDescendantSelectionIssue997Tests.CreateRouterAsync();
            router.Model.ArchiveRoot = BreadcrumbDescendantSelectionIssue997Tests.Root;
            var row = router.Model.Rows[0];
            row.SetSubfolders(new[] { Segment("bad", invalidPath, true) });
            var node = row.DescendantRoot!.Children[0];
            router.OpenSelector();
            var activation = router.ActivateSelectorDescendant(row.Identity, node.Identity);
            activation.Handled.Should().BeFalse();
            (
                await router.ToggleDescendantAsync(
                    row.Identity,
                    node.Identity,
                    CancellationToken.None
                )
            )
                .Should()
                .BeEmpty();
            router.GetSelectedFolder().Should().Be("Projects\\Apollo");
        }

        [TestMethod]
        public async Task DuplicateNamesAndStoreKeys_KeepRowOccurrencesDistinct()
        {
            var router = await BreadcrumbDescendantSelectionIssue997Tests.CreateRouterAsync();
            var first = router.Model.Rows[0];
            router.Model.AddSuggestionRow(
                "other-store",
                new[] { Segment("apollo", "\\\\other\\Archive\\Apollo", true, "store-b") },
                0.4
            );
            var other = router.Model.Rows[1];
            other.TryExpandLeaf();
            other.SetSubfolders(
                new[] { Segment("alpha", "\\\\other\\Archive\\Apollo\\Alpha", false, "store-b") }
            );
            var child = other.DescendantRoot!.Children[0];
            child.Identity.Should().NotBe(first.DescendantRoot!.Children[0].Identity);
            router.OpenSelector();
            router
                .ActivateSelectorDescendant(first.Identity, child.Identity)
                .Handled.Should()
                .BeFalse();
            router
                .ActivateSelectorDescendant(other.Identity, child.Identity)
                .SelectionChanged.Should()
                .BeTrue();
            router.GetSelectedFolder().Should().Be(child.Segment.FolderPath);
            router.Model.SelectedIndex.Should().Be(1);
        }

        [TestMethod]
        public async Task LegacySelectionChange_OutOfRootKnownChild_DoesNotMutateOrAcknowledge()
        {
            var router = await BreadcrumbDescendantSelectionIssue997Tests.CreateRouterAsync();
            router.Model.ArchiveRoot = BreadcrumbDescendantSelectionIssue997Tests.Root;
            var row = router.Model.Rows[0];
            row.SetSubfolders(new[] { Segment("bad", "\\\\mailbox\\Archive2\\Alpha", false) });
            router.OpenSelector();
            var outputs = await router.RouteAsync(
                "{\"type\":\"selectionChange\",\"rowIndex\":0,\"subfolderIndex\":0}",
                CancellationToken.None
            );
            outputs.Should().ContainSingle();
            BreadcrumbBridgeSerializer.Parse(outputs[0]).Should().BeOfType<BridgeErrorMessage>();
            router.Model.SelectedDescendant.Should().BeNull();
            router.GetSelectedFolder().Should().Be("Projects\\Apollo");
            router.GetSelectorState().IsOpen.Should().BeTrue();
        }

        [TestMethod]
        public async Task LeavesCannotExpand_AndExpansionIsLazy()
        {
            var router = await BreadcrumbDescendantSelectionIssue997Tests.CreateRouterAsync();
            var row = router.Model.Rows[0];
            var alpha = row.DescendantRoot!.Children[0];
            alpha.Loaded.Should().BeFalse();
            await router.ToggleDescendantAsync(
                row.Identity,
                alpha.Identity,
                CancellationToken.None
            );
            var review = alpha.Children[0];
            review.Loaded.Should().BeFalse();
            await router.ToggleDescendantAsync(
                row.Identity,
                review.Identity,
                CancellationToken.None
            );
            var leaf = review.Children[0];
            (
                await router.ToggleDescendantAsync(
                    row.Identity,
                    leaf.Identity,
                    CancellationToken.None
                )
            )
                .Should()
                .BeEmpty();
            leaf.Loaded.Should().BeFalse();
            router.GetSelectedFolder().Should().Be("Projects\\Apollo");
        }

        [TestMethod]
        public async Task ProviderFailure_PreservesSelectionAndPermitsRetry()
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var root = Segment("apollo", BreadcrumbDescendantSelectionIssue997Tests.Parent, true);
            var child = Segment("alpha", BreadcrumbDescendantSelectionIssue997Tests.Child, true);
            provider
                .SetupSequence(p =>
                    p.GetImmediateSubfoldersAsync(child.Key, It.IsAny<CancellationToken>())
                )
                .ThrowsAsync(new InvalidOperationException("provider unavailable"))
                .ReturnsAsync(new[] { Segment("review", child.FolderPath + "\\Review", false) });
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            router.Model.AddSuggestionRow(new[] { root }, null);
            router.SelectRow(0);
            var row = router.Model.Rows[0];
            row.TryExpandLeaf();
            row.SetSubfolders(new[] { child });
            var node = row.DescendantRoot!.Children[0];
            router.OpenSelector();
            router.ActivateSelectorDescendant(row.Identity, node.Identity);
            var failed = await router.ToggleDescendantAsync(
                row.Identity,
                node.Identity,
                CancellationToken.None
            );
            failed[0].Should().Contain("provider unavailable");
            node.Expanded.Should().BeFalse();
            router.GetSelectedFolder().Should().Be(child.FolderPath);
            await router.ToggleDescendantAsync(row.Identity, node.Identity, CancellationToken.None);
            node.Children.Should().ContainSingle();
            node.Expanded.Should().BeTrue();
            router.GetSelectedFolder().Should().Be(child.FolderPath);
            provider.Verify(
                p => p.GetImmediateSubfoldersAsync(child.Key, It.IsAny<CancellationToken>()),
                Times.Exactly(2)
            );
            provider.VerifyNoOtherCalls();
        }

        internal static FolderBreadcrumbSegment Segment(
            string entry,
            string path,
            bool children,
            string store = "store-a"
        ) =>
            new FolderBreadcrumbSegment(
                new FolderTreeNodeKey(store, entry, path),
                "Alpha",
                path,
                children
            );
    }
}
