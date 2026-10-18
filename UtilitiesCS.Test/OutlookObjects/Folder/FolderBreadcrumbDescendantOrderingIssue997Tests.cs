#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderBreadcrumbDescendantOrderingIssue997Tests
    {
        [TestMethod]
        public async Task CollapseDuringFetch_DiscardsLateChildren()
        {
            var completion = Completion();
            var harness = Create(completion);
            var pending = harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            pending.IsCompleted.Should().BeFalse();
            await harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            completion.SetResult(new[] { Child("Late") });
            await pending;
            harness.Node.Children.Should().BeEmpty();
            harness.Node.Expanded.Should().BeFalse();
            harness
                .Router.GetSelectedFolder()
                .Should()
                .Be(BreadcrumbDescendantSelectionIssue997Tests.Parent);
        }

        [TestMethod]
        public async Task ReplacePopulationDuringFetch_DiscardsOldChildren()
        {
            var completion = Completion();
            var harness = Create(completion);
            var pending = harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            harness.Router.SetItems(new[] { "Replacement" });
            harness.Router.SelectRow(0);
            completion.SetResult(new[] { Child("Late") });
            await pending;
            harness.Node.Children.Should().BeEmpty();
            harness.Router.GetSelectedFolder().Should().Be("Replacement");
        }

        [TestMethod]
        public async Task ClearAndRebindDuringFetch_DiscardsOldOccurrence()
        {
            var completion = Completion();
            var harness = Create(completion);
            var pending = harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            harness.Router.Clear();
            harness.Router.Model.AddSuggestionRow(harness.Row.Identity, harness.Row.Chain, null);
            harness.Router.SelectRow(0);
            var replacement = harness.Router.Model.Rows[0];
            replacement.TryExpandLeaf();
            replacement.SetSubfolders(new[] { harness.Node.Segment });
            completion.SetResult(new[] { Child("Late") });
            await pending;
            replacement.DescendantRoot!.Children[0].Children.Should().BeEmpty();
            replacement.DescendantRoot.Children[0].Identity.Should().NotBe(harness.Node.Identity);
            harness.Router.OpenSelector();
            harness
                .Router.ActivateSelectorDescendant(replacement.Identity, harness.Node.Identity)
                .Handled.Should()
                .BeFalse();
        }

        [TestMethod]
        public async Task ReverseCompletion_OnlyCurrentNodeRequestApplies()
        {
            var first = Completion();
            var second = Completion();
            var harness = Create(first, second);
            var older = harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            await harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            var newer = harness.Router.ToggleDescendantAsync(
                harness.Row.Identity,
                harness.Node.Identity,
                CancellationToken.None
            );
            second.SetResult(new[] { Child("Current") });
            await newer;
            first.SetResult(new[] { Child("Obsolete") });
            await older;
            harness
                .Node.Children.Should()
                .ContainSingle()
                .Which.Segment.FolderPath.Should()
                .EndWith("\\Current");
            harness.Node.Expanded.Should().BeTrue();
            harness.Provider.Verify(
                p =>
                    p.GetImmediateSubfoldersAsync(
                        harness.Node.Segment.Key,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Exactly(2)
            );
            harness.Provider.VerifyNoOtherCalls();
        }

        private static TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>> Completion() =>
            new TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

        private static FolderBreadcrumbSegment Child(string name) =>
            BreadcrumbDescendantValidationIssue997Tests.Segment(
                name,
                BreadcrumbDescendantSelectionIssue997Tests.Child + "\\" + name,
                false
            );

        private static Harness Create(
            params TaskCompletionSource<IReadOnlyList<FolderBreadcrumbSegment>>[] completions
        )
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var parent = BreadcrumbDescendantValidationIssue997Tests.Segment(
                "parent",
                BreadcrumbDescendantSelectionIssue997Tests.Parent,
                true
            );
            var child = BreadcrumbDescendantValidationIssue997Tests.Segment(
                "child",
                BreadcrumbDescendantSelectionIssue997Tests.Child,
                true
            );
            int invocation = 0;
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(child.Key, It.IsAny<CancellationToken>()))
                .Returns(() => completions[invocation++].Task);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            router.Model.AddSuggestionRow(new[] { parent }, null);
            router.SelectRow(0);
            var row = router.Model.Rows[0];
            row.TryExpandLeaf();
            row.SetSubfolders(new[] { child });
            return new Harness(router, row, row.DescendantRoot!.Children[0], provider);
        }

        private sealed class Harness
        {
            internal Harness(
                FolderBreadcrumbBridgeRouter router,
                BreadcrumbStateRow row,
                BreadcrumbDescendantNode node,
                Mock<IFolderHierarchyProvider> provider
            )
            {
                Router = router;
                Row = row;
                Node = node;
                Provider = provider;
            }

            internal FolderBreadcrumbBridgeRouter Router { get; }
            internal BreadcrumbStateRow Row { get; }
            internal BreadcrumbDescendantNode Node { get; }
            internal Mock<IFolderHierarchyProvider> Provider { get; }
        }
    }
}
