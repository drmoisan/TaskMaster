#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>Failure-first selector mutation interleavings during hierarchy upgrades.</summary>
    [TestClass]
    public sealed class FolderBreadcrumbRouterSelectionConcurrencyTests
    {
        private const string DuplicatePath = "\\Inbox\\Duplicate";
        private const string OtherPath = "\\Inbox\\Other";

        [TestMethod]
        public async Task UpgradeStarted_ClosedMoveToDuplicateRow_RemainsSelectedAfterReplacement()
        {
            // Arrange
            var harness = new UpgradeHarness();

            // Act
            BreadcrumbSelectionTransition moved = harness.Router.MoveSelector(previous: false);
            moved.Handled.Should().BeTrue();
            moved.SelectionChanged.Should().BeTrue();
            harness.Router.Model.SelectedIndex.Should().Be(1);
            harness.CompleteUpgrade();
            await harness.Upgrade.ConfigureAwait(false);

            // Assert
            harness.Router.Model.SelectedIndex.Should().Be(1);
            harness
                .Router.GetSelectorState()
                .CommittedIdentity.Should()
                .Be(harness.Router.Model.Rows[1].Identity);
        }

        [TestMethod]
        public async Task UpgradeStarted_OpenPendingMoveToDuplicateRow_CommitsExactMovedRow()
        {
            // Arrange
            var harness = new UpgradeHarness();
            harness.Router.OpenSelector().Handled.Should().BeTrue();

            // Act
            harness.Router.MoveSelector(previous: false).Handled.Should().BeTrue();
            string pendingIdentity = harness.Router.Model.Rows[1].Identity;
            harness.CompleteUpgrade();
            await harness.Upgrade.ConfigureAwait(false);
            harness.Router.GetSelectorState().PendingIdentity.Should().Be(pendingIdentity);
            BreadcrumbSelectionTransition committed = harness.Router.CommitSelector();

            // Assert
            committed.Handled.Should().BeTrue();
            committed.SelectionChanged.Should().BeTrue();
            harness.Router.Model.SelectedIndex.Should().Be(1);
            harness.Router.GetSelectedFolder().Should().Be(DuplicatePath);
        }

        [TestMethod]
        public async Task UpgradeStarted_ActivationOfDuplicateRow_CommitsExactActivatedRow()
        {
            // Arrange
            var harness = new UpgradeHarness();
            string activatedIdentity = harness.Router.Model.Rows[1].Identity;

            // Act
            BreadcrumbSelectionTransition activated = harness.Router.ActivateSelector(
                activatedIdentity
            );
            harness.CompleteUpgrade();
            await harness.Upgrade.ConfigureAwait(false);

            // Assert
            activated.Handled.Should().BeTrue();
            activated.SelectionChanged.Should().BeTrue();
            harness.Router.Model.SelectedIndex.Should().Be(1);
            harness
                .Router.GetSelectorState()
                .CommittedIdentity.Should()
                .Be(harness.Router.Model.Rows[1].Identity);
        }

        [TestMethod]
        public async Task UpgradeStarted_DirectItemSelectionOfAnotherPath_SurvivesReplacement()
        {
            // Arrange
            var harness = new UpgradeHarness();

            // Act
            BreadcrumbSelectionTransition selected = harness.Router.SelectItem(OtherPath);
            harness.CompleteUpgrade();
            await harness.Upgrade.ConfigureAwait(false);

            // Assert
            selected.Handled.Should().BeTrue();
            harness.Router.Model.SelectedIndex.Should().Be(2);
            harness.Router.GetSelectedFolder().Should().Be(OtherPath);
            harness
                .Router.GetSelectorState()
                .CommittedIdentity.Should()
                .Be(harness.Router.Model.Rows[2].Identity);
        }

        [TestMethod]
        public void PublicStateSnapshot_IsImmutableAndRouterDoesNotExposeMutableModel()
        {
            // Arrange
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            router.AddItems(new[] { "A" });
            BreadcrumbSelectorState state = router.GetSelectorState();

            // Act
            Action mutate = () =>
                ((IList<BreadcrumbSelectorOptionState>)state.Options).Add(state.Options[0]);
            PropertyInfo? publicModel = typeof(FolderBreadcrumbBridgeRouter).GetProperty("Model");

            // Assert
            mutate.Should().Throw<NotSupportedException>();
            publicModel.Should().BeNull();
        }

        [TestMethod]
        public async Task StaleUpgradeCompletion_CannotReplaceNewerRowsOrCommittedSelection()
        {
            // Arrange
            var gate = new TaskCompletionSource<FolderTreeNodeKey?>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var key = new FolderTreeNodeKey("store", "duplicate", DuplicatePath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(DuplicatePath, It.IsAny<CancellationToken>())
                )
                .Returns(gate.Task);
            provider
                .Setup(value => value.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[] { new FolderBreadcrumbSegment(key, "Duplicate", DuplicatePath, false) }
                );
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            Task<string> staleUpgrade = router.SetSuggestionsAsync(
                new[]
                {
                    new FolderRow(
                        DuplicatePath,
                        FolderRowKind.Suggestion,
                        new FolderScore(DuplicatePath, 100, 0.6)
                    ),
                },
                CancellationToken.None
            );
            router.SetSuggestionFallbacks(
                new[] { new FolderRow(OtherPath, FolderRowKind.Recent, null) }
            );
            router.SelectRow(0);
            string committedIdentity = router.GetSelectorState().CommittedIdentity!;

            // Act
            gate.SetResult(key);
            await staleUpgrade.ConfigureAwait(false);

            // Assert
            router.GetFolderItems().Should().Equal(OtherPath);
            router.GetSelectorState().CommittedIdentity.Should().Be(committedIdentity);
            router.Model.Rows[0].Identity.Should().Be(committedIdentity);
        }

        private sealed class UpgradeHarness
        {
            private readonly TaskCompletionSource<FolderTreeNodeKey?> _gate =
                new TaskCompletionSource<FolderTreeNodeKey?>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
            private readonly FolderTreeNodeKey _key = new FolderTreeNodeKey(
                "store",
                "duplicate",
                DuplicatePath
            );

            internal UpgradeHarness()
            {
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                provider
                    .Setup(value =>
                        value.ResolveLeafKeyAsync(DuplicatePath, It.IsAny<CancellationToken>())
                    )
                    .Returns(_gate.Task);
                provider
                    .Setup(value =>
                        value.GetAncestorChainAsync(_key, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(
                        new[]
                        {
                            new FolderBreadcrumbSegment(_key, "Duplicate", DuplicatePath, false),
                        }
                    );
                Router = new FolderBreadcrumbBridgeRouter(provider.Object);
                FolderRow[] rows = Rows();
                Router.SetSuggestionFallbacks(rows);
                Router.SelectRow(0);
                Upgrade = Router.SetSuggestionsAsync(rows, CancellationToken.None);
                Upgrade.IsCompleted.Should().BeFalse("the provider completion is controlled");
            }

            internal FolderBreadcrumbBridgeRouter Router { get; }
            internal Task<string> Upgrade { get; }

            internal void CompleteUpgrade() => _gate.SetResult(_key);

            private static FolderRow[] Rows() =>
                new[]
                {
                    new FolderRow(
                        DuplicatePath,
                        FolderRowKind.Suggestion,
                        new FolderScore(DuplicatePath, 100, 0.6)
                    ),
                    new FolderRow(DuplicatePath, FolderRowKind.Recent, null),
                    new FolderRow(OtherPath, FolderRowKind.Recent, null),
                };
        }
    }
}
