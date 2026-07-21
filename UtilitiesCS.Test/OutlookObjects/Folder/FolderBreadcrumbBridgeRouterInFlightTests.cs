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
    /// Multi-message state-transition sequences, constructor/null/plain-row edge cases, and the #398
    /// in-flight rebuild invariants (AC-2 / AC-3) for <see cref="FolderBreadcrumbBridgeRouter"/>. Split
    /// from FolderBreadcrumbBridgeRouterTests.cs so each file stays under the 500-line limit; this partial
    /// reuses the shared helpers (<c>Key</c>, <c>Segment</c>, <c>LeafChain</c>, <c>ProviderMock</c>,
    /// <c>PopulatedRouterAsync</c>) declared in the sibling partial. Deterministic; no Outlook, WebView2,
    /// timers, or temp files.
    /// </summary>
    public sealed partial class FolderBreadcrumbBridgeRouterTests
    {
        // --- State-transition sequences across multiple routed messages ---

        [TestMethod]
        public async Task Sequence_ExpandCollapseViaMessages_TransitionsDeterministically()
        {
            // Arrange
            var router = await PopulatedRouterAsync(ProviderMock());

            // Act + Assert stepwise: toggle open -> toggle closed -> double-click collapse ->
            // toggle re-expands the collapsed chain.
            (
                await router.RouteAsync(
                    "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                    CancellationToken.None
                )
            )
                .Should()
                .HaveCount(2);
            router.Model.Rows[0].LeafExpanded.Should().BeTrue();

            (
                await router.RouteAsync(
                    "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                    CancellationToken.None
                )
            )
                .Should()
                .ContainSingle();
            router.Model.Rows[0].LeafExpanded.Should().BeFalse();

            await router.RouteAsync(
                "{\"type\":\"segmentDoubleClick\",\"rowIndex\":0,\"segmentIndex\":1}",
                CancellationToken.None
            );
            router.Model.Rows[0].CollapsedAfterIndex.Should().Be(1);

            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );
            router.Model.Rows[0].CollapsedAfterIndex.Should().BeNull();
        }

        [TestMethod]
        public async Task SetSuggestions_UnresolvablePath_FallsBackToPlainRowPreservingThePath()
        {
            // Arrange: the provider knows nothing about this path (G10 fallback).
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync("\\Ghost", It.IsAny<CancellationToken>()))
                .ReturnsAsync((FolderTreeNodeKey)null);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);

            // Act
            await router.SetSuggestionsAsync(
                new[]
                {
                    new FolderRow(
                        "\\Ghost",
                        FolderRowKind.Suggestion,
                        new FolderScore("\\Ghost", 10, 0.2)
                    ),
                },
                CancellationToken.None
            );

            // Assert
            router.Model.Rows[0].IsSuggestion.Should().BeFalse();
            router.Model.Rows[0].VerbatimText.Should().Be("\\Ghost");
        }

        [TestMethod]
        public void SetItemsAndAddItems_NullInput_ThrowExplicitly()
        {
            // Arrange
            var router = new FolderBreadcrumbBridgeRouter(
                new Mock<IFolderHierarchyProvider>(MockBehavior.Strict).Object
            );

            // Act, Assert
            ((Action)(() => router.SetItems(null)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => router.AddItems(null))).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_NullProvider_Throws()
        {
            // Arrange, Act
            Action act = () => new FolderBreadcrumbBridgeRouter(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("provider");
        }

        [TestMethod]
        public async Task SetItems_PlainRows_RenderVerbatimIncludingTrashToDelete()
        {
            // Arrange
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);

            // Act (Path B population; no provider call is made for plain rows).
            var renderJson = router.SetItems(new[] { "Trash to Delete", "\\Inbox\\Manual" });

            // Assert
            var render = (RenderMessage)BreadcrumbBridgeSerializer.Parse(renderJson);
            render.Rows.Should().HaveCount(2);
            render.Rows[0].Cells[0].Text.Should().Be("Trash to Delete");
            render.Rows[0].PercentText.Should().BeEmpty();
            await Task.CompletedTask;
        }

        // --- #398 in-flight rebuild invariants (AC-2 / AC-3) ---

        private const string SecondPath = "\\Inbox\\Projects\\Zephyr";

        private static readonly FolderTreeNodeKey SecondKey = Key("second", SecondPath);

        /// <summary>
        /// Builds a provider whose first leaf-key resolve is gated on <paramref name="gate"/> so a
        /// <c>SetSuggestionsAsync</c> rebuild parks mid-flight, while the second path resolves from a
        /// completed task. Releasing the gate with <see cref="LeafKey"/> drains the rebuild to the
        /// full two-row suggestion set.
        /// </summary>
        private static Mock<IFolderHierarchyProvider> GatedTwoRowProvider(
            TaskCompletionSource<FolderTreeNodeKey> gate
        )
        {
            var provider = new Mock<IFolderHierarchyProvider>();
            provider
                .Setup(p => p.ResolveLeafKeyAsync(LeafPath, It.IsAny<CancellationToken>()))
                .Returns(gate.Task);
            provider
                .Setup(p => p.GetAncestorChainAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(LeafChain());
            provider
                .Setup(p => p.ResolveLeafKeyAsync(SecondPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SecondKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(SecondKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Segment(SecondKey, "Zephyr", false) });
            return provider;
        }

        private static FolderRow[] TwoScoredRows() =>
            new[]
            {
                new FolderRow(
                    LeafPath,
                    FolderRowKind.Suggestion,
                    new FolderScore(LeafPath, 1000, 0.73)
                ),
                new FolderRow(
                    SecondPath,
                    FolderRowKind.Suggestion,
                    new FolderScore(SecondPath, 500, 0.41)
                ),
            };

        [TestMethod]
        public async Task SetSuggestionsAsync_NonScoredRow_BecomesPlainVerbatimRow()
        {
            // Arrange: a non-scored row (for example a section separator) carries no provider
            // lookup and must be swapped in verbatim as a plain row.
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);

            // Act
            await router.SetSuggestionsAsync(
                new[] { new FolderRow("===== SUGGESTIONS =====", FolderRowKind.Separator, null) },
                CancellationToken.None
            );

            // Assert
            router.Model.Rows.Should().ContainSingle();
            router.Model.Rows[0].IsSuggestion.Should().BeFalse();
            router.Model.Rows[0].VerbatimText.Should().Be("===== SUGGESTIONS =====");
        }

        [TestMethod]
        public async Task SetSuggestionsAsync_WhileUpgradeInFlight_RowCountNeverDropsBelowPreUpgradeCount()
        {
            // Arrange: the coordinator's synchronous immediate population is two plain rows; the
            // rebuild then parks on the gated first resolve.
            var gate = new TaskCompletionSource<FolderTreeNodeKey>();
            var router = new FolderBreadcrumbBridgeRouter(GatedTwoRowProvider(gate).Object);
            router.SetItems(new[] { LeafPath, SecondPath });
            int preUpgradeCount = router.Model.Rows.Count;

            // Act: fire the rebuild; with the fix it mutates no model state while awaiting.
            var upgrade = router.SetSuggestionsAsync(TwoScoredRows(), CancellationToken.None);

            // Assert (AC-2): the observable row count never drops below the pre-upgrade count.
            upgrade.IsCompleted.Should().BeFalse("the rebuild is gated");
            router.Model.Rows.Count.Should().Be(preUpgradeCount);

            // Release the gate and drain: the swapped-in set keeps the same count.
            gate.SetResult(LeafKey);
            await upgrade;
            router.Model.Rows.Count.Should().Be(preUpgradeCount);
            router.Model.Rows[0].IsSuggestion.Should().BeTrue();
        }

        [TestMethod]
        public async Task SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives()
        {
            // Arrange: two plain rows with the host having selected the second row before the
            // rebuild completes.
            var gate = new TaskCompletionSource<FolderTreeNodeKey>();
            var router = new FolderBreadcrumbBridgeRouter(GatedTwoRowProvider(gate).Object);
            router.SetItems(new[] { LeafPath, SecondPath });
            router.SelectRow(1);

            // Act: park the rebuild on the gated first resolve.
            var upgrade = router.SetSuggestionsAsync(TwoScoredRows(), CancellationToken.None);

            // Assert (AC-3): the readback contract stays pre-upgrade-consistent in flight.
            upgrade.IsCompleted.Should().BeFalse();
            BreadcrumbSelectionMap.FolderContains(router.Model, LeafPath).Should().BeTrue();
            BreadcrumbSelectionMap.FolderContains(router.Model, SecondPath).Should().BeTrue();
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(SecondPath);
            ((Action)(() => router.SelectRow(0))).Should().NotThrow();
            ((Action)(() => router.SelectRow(1))).Should().NotThrow();

            // Release the gate and drain: the host-selected index survives the atomic swap.
            gate.SetResult(LeafKey);
            await upgrade;
            router.Model.SelectedIndex.Should().Be(1);
            router.Model.SelectedRow.Identity.Should().Be(SecondPath);
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(SecondPath);
        }

        [TestMethod]
        public async Task SetSuggestionsAsync_OlderCompletionCannotOverwriteNewerGeneration()
        {
            // Arrange: the older request waits while the newer request completes synchronously.
            var oldGate = new TaskCompletionSource<FolderTreeNodeKey>();
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(LeafPath, It.IsAny<CancellationToken>()))
                .Returns(oldGate.Task);
            provider
                .Setup(p => p.GetAncestorChainAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(LeafChain());
            provider
                .Setup(p => p.ResolveLeafKeyAsync(SecondPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SecondKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(SecondKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Segment(SecondKey, "Zephyr", false) });
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            var oldRows = new[] { TwoScoredRows()[0] };
            var newRows = new[] { TwoScoredRows()[1] };

            // Act
            Task<string> oldUpgrade = router.SetSuggestionsAsync(oldRows, CancellationToken.None);
            await router.SetSuggestionsAsync(newRows, CancellationToken.None);
            oldGate.SetResult(LeafKey);
            await oldUpgrade;

            // Assert
            router.Model.Rows.Should().ContainSingle();
            router.Model.Rows[0].Identity.Should().Be(SecondPath);
            BreadcrumbSelectionMap.GetFolderItems(router.Model).Should().Equal(SecondPath);
        }
    }
}
