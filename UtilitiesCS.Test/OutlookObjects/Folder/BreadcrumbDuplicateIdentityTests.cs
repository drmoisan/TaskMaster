#nullable enable
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
    /// Failure-first contracts for selectable rows that intentionally share one folder output path.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbDuplicateIdentityTests
    {
        private const string DuplicatePath = "\\Inbox\\Shared";
        private static readonly FolderTreeNodeKey DuplicateKey = new FolderTreeNodeKey(
            "store",
            "shared",
            DuplicatePath
        );

        [TestMethod]
        public void SetSuggestionFallbacks_DuplicateSuggestionAndRecentPathsHaveDistinctIdentities()
        {
            // Arrange
            var router = CreateFallbackRouter(SuggestionAndRecentRows());

            // Act
            string[] identities = router.Model.Rows.Select(row => row.Identity).ToArray();
            string[] outputs = BreadcrumbSelectionMap.GetFolderItems(router.Model);
            IReadOnlyList<BreadcrumbRowRender> render = BreadcrumbRenderProjection.Project(
                router.Model
            );

            // Assert
            identities
                .Should()
                .OnlyHaveUniqueItems("each selectable occurrence needs an identity");
            identities[0].Should().Be("suggestion:0:" + DuplicatePath);
            identities[1].Should().Be("recent:1:" + DuplicatePath);
            outputs.Should().Equal(DuplicatePath, DuplicatePath);
            render[0].PercentText.Should().Be(PercentageFormatter.FormatPercent(0.73));
            render[1].PercentText.Should().BeEmpty();
        }

        [TestMethod]
        public async Task SetSuggestionsAsync_ResolvedUpgradePreservesDistinctFallbackIdentities()
        {
            // Arrange
            Mock<IFolderHierarchyProvider> provider = ResolvedProvider();
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            FolderRow[] rows = SuggestionAndRecentRows();
            router.SetSuggestionFallbacks(rows);
            string[] fallbackIdentities = router.Model.Rows.Select(row => row.Identity).ToArray();

            // Act
            await router.SetSuggestionsAsync(rows, CancellationToken.None);
            string[] resolvedIdentities = router.Model.Rows.Select(row => row.Identity).ToArray();

            // Assert
            resolvedIdentities.Should().Equal(fallbackIdentities);
            resolvedIdentities.Should().OnlyHaveUniqueItems();
            router.Model.Rows[0].IsSuggestion.Should().BeTrue();
            router.Model.Rows[0].Probability.Should().Be(0.73);
            BreadcrumbSelectionMap
                .GetFolderItems(router.Model)
                .Should()
                .Equal(DuplicatePath, DuplicatePath);
        }

        [TestMethod]
        public void SameOutputAcrossAllSources_PreservesOrderKindScoreAndSelectability()
        {
            // Arrange
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            var rows = new[]
            {
                new FolderRow(DuplicatePath, FolderRowKind.SearchResult, null),
                ScoredSuggestion(0.73),
                new FolderRow("Suggested folders", FolderRowKind.Separator, null),
                new FolderRow(DuplicatePath, FolderRowKind.Recent, null),
            };

            // Act
            router.SetSuggestionFallbacks(rows);
            router.AddItems(new[] { DuplicatePath });

            // Assert
            router
                .Model.Rows.Select(row => row.Identity)
                .Should()
                .Equal(
                    "search:0:" + DuplicatePath,
                    "suggestion:1:" + DuplicatePath,
                    "separator:2:Suggested folders",
                    "recent:3:" + DuplicatePath,
                    "plain:4:" + DuplicatePath
                );
            BreadcrumbSelectionMap
                .GetFolderItems(router.Model)
                .Should()
                .Equal(
                    DuplicatePath,
                    DuplicatePath,
                    "Suggested folders",
                    DuplicatePath,
                    DuplicatePath
                );
            router
                .Model.Rows.Select(row => row.IsSelectable)
                .Should()
                .Equal(true, true, false, true, true);
            router
                .Model.Rows.Select(row => row.Probability)
                .Should()
                .Equal(null, 0.73, null, null, null);
            BreadcrumbSelectionMap.FolderContains(router.Model, DuplicatePath).Should().BeTrue();
            BreadcrumbSelectionMap.TrySelectItem(router.Model, DuplicatePath).Should().BeTrue();
            router.Model.SelectedIndex.Should().Be(0);
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(DuplicatePath);
        }

        [TestMethod]
        public void ClosedMoveNext_DuplicateOutputPathsCommitsSecondLogicalRow()
        {
            // Arrange
            var router = CreateFallbackRouter(SuggestionAndRecentRows());
            router.SelectRow(0);
            var session = new BreadcrumbSelectionSession(router.Model);
            string firstIdentity = router.Model.Rows[0].Identity;
            string secondIdentity = router.Model.Rows[1].Identity;

            // Act
            bool moved = session.MoveNext();

            // Assert
            moved.Should().BeTrue();
            secondIdentity.Should().NotBe(firstIdentity);
            session.CommittedIdentity.Should().Be(secondIdentity);
            router.Model.SelectedIndex.Should().Be(1);
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(DuplicatePath);
        }

        [TestMethod]
        public void OpenMoveNextThenCommit_DuplicateOutputPathsCommitsSecondLogicalRow()
        {
            // Arrange
            var router = CreateFallbackRouter(SuggestionAndRecentRows());
            router.SelectRow(0);
            var session = new BreadcrumbSelectionSession(router.Model);
            string secondIdentity = router.Model.Rows[1].Identity;
            session.Open().Should().BeTrue();

            // Act
            bool moved = session.MoveNext();
            bool changed = session.CommitPending();

            // Assert
            moved.Should().BeTrue();
            changed.Should().BeTrue();
            session.CommittedIdentity.Should().Be(secondIdentity);
            router.Model.SelectedIndex.Should().Be(1);
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(DuplicatePath);
        }

        [TestMethod]
        public void Activate_SecondDuplicateIdentityCommitsExactLogicalRow()
        {
            // Arrange
            var router = CreateFallbackRouter(SuggestionAndRecentRows());
            router.SelectRow(0);
            var session = new BreadcrumbSelectionSession(router.Model);
            string secondIdentity = router.Model.Rows[1].Identity;

            // Act
            bool changed = session.Activate(secondIdentity);

            // Assert
            changed.Should().BeTrue();
            router.Model.SelectedIndex.Should().Be(1);
            session.CommittedIdentity.Should().Be(secondIdentity);
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(DuplicatePath);
        }

        [TestMethod]
        public void OpenCommit_CollapsedReadbackUsesSecondDuplicateSuggestionProbability()
        {
            // Arrange
            var router = CreateFallbackRouter(
                new[] { ScoredSuggestion(0.8), ScoredSuggestion(0.25) }
            );
            router.SelectRow(0);
            var session = new BreadcrumbSelectionSession(router.Model);
            session.Open().Should().BeTrue();
            session.MoveNext().Should().BeTrue();

            // Act
            bool changed = session.CommitPending();
            BreadcrumbRowRender collapsed = BreadcrumbRenderProjection
                .ProjectCollapsed(router.Model)
                .Single();

            // Assert
            changed.Should().BeTrue();
            collapsed.RowIndex.Should().Be(1);
            collapsed.Selected.Should().BeTrue();
            collapsed.PercentText.Should().Be(PercentageFormatter.FormatPercent(0.25));
            BreadcrumbSelectionMap.GetSelectedFolder(router.Model).Should().Be(DuplicatePath);
        }

        private static FolderBreadcrumbBridgeRouter CreateFallbackRouter(
            IReadOnlyList<FolderRow> rows
        )
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var router = new FolderBreadcrumbBridgeRouter(provider.Object);
            router.SetSuggestionFallbacks(rows);
            return router;
        }

        private static Mock<IFolderHierarchyProvider> ResolvedProvider()
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(candidate =>
                    candidate.ResolveLeafKeyAsync(DuplicatePath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(DuplicateKey);
            provider
                .Setup(candidate =>
                    candidate.GetAncestorChainAsync(DuplicateKey, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new[]
                    {
                        new FolderBreadcrumbSegment(DuplicateKey, "Shared", DuplicatePath, false),
                    }
                );
            return provider;
        }

        private static FolderRow[] SuggestionAndRecentRows() =>
            new[]
            {
                ScoredSuggestion(0.73),
                new FolderRow(DuplicatePath, FolderRowKind.Recent, null),
            };

        private static FolderRow ScoredSuggestion(double probability) =>
            new FolderRow(
                DuplicatePath,
                FolderRowKind.Suggestion,
                new FolderScore(DuplicatePath, 100, probability)
            );
    }
}
