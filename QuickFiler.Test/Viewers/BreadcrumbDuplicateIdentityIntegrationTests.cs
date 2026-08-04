using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Failure-first coordinator contracts for duplicate folder outputs with distinct logical rows.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbDuplicateIdentityIntegrationTests
    {
        private const string DuplicatePath = "\\Inbox\\Shared";

        [TestMethod]
        public void ClosedDown_DuplicateSuggestionAndRecentCommitsRecentOccurrence()
        {
            // Arrange
            using (Harness harness = CreateHarness(SuggestionAndRecentRows()))
            {
                string firstIdentity = harness.Coordinator.CommittedIdentity;
                int selectionChanges = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionChanges++;

                // Act
                bool handled = harness.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);

                // Assert
                handled.Should().BeTrue();
                harness.Coordinator.CommittedIdentity.Should().NotBe(firstIdentity);
                SelectedRender(harness.Posted).RowIndex.Should().Be(1);
                harness.Coordinator.GetSelectedFolder().Should().Be(DuplicatePath);
                selectionChanges.Should().Be(1);
            }
        }

        [TestMethod]
        public void OpenDownThenEnter_DuplicateSuggestionAndRecentCommitsPendingOccurrence()
        {
            // Arrange
            using (Harness harness = CreateHarness(SuggestionAndRecentRows()))
            {
                string[] identities = SelectableIdentities(LatestSelectorView(harness.Posted));
                harness.Coordinator.OpenSelector().Should().BeTrue();

                // Act
                bool moved = harness.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);
                string pending = harness.Coordinator.PendingIdentity;
                bool committed = harness.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Enter);

                // Assert
                identities.Should().HaveCount(2).And.OnlyHaveUniqueItems();
                identities[0].Should().StartWith("suggestion:0:");
                identities[1].Should().StartWith("recent:1:");
                moved.Should().BeTrue();
                pending.Should().Be(identities[1]);
                committed.Should().BeTrue();
                harness.Coordinator.CommittedIdentity.Should().Be(identities[1]);
                SelectedRender(harness.Posted).RowIndex.Should().Be(1);
                harness.Coordinator.GetSelectedFolder().Should().Be(DuplicatePath);
            }
        }

        [TestMethod]
        public void ActivateSelector_SecondPublishedIdentityCommitsExactDuplicateOccurrence()
        {
            // Arrange
            using (Harness harness = CreateHarness(SuggestionAndRecentRows()))
            {
                string[] identities = SelectableIdentities(LatestSelectorView(harness.Posted));
                int selectionChanges = 0;
                harness.Coordinator.SelectionChanged += (sender, args) => selectionChanges++;
                harness.Coordinator.OpenSelector().Should().BeTrue();

                // Act
                bool activated = harness.Coordinator.ActivateSelector(identities[1]);

                // Assert
                activated.Should().BeTrue();
                identities[1].Should().NotBe(identities[0]);
                identities[1].Should().StartWith("recent:1:");
                harness.Coordinator.CommittedIdentity.Should().Be(identities[1]);
                SelectedRender(harness.Posted).RowIndex.Should().Be(1);
                harness.Coordinator.GetSelectedFolder().Should().Be(DuplicatePath);
                selectionChanges.Should().Be(1);
            }
        }

        [TestMethod]
        public void CollapsedReadback_SecondDuplicateSuggestionRetainsItsProbability()
        {
            // Arrange
            FolderRow[] rows = { ScoredSuggestion(0.8), ScoredSuggestion(0.25) };
            using (Harness harness = CreateHarness(rows))
            {
                string[] identities = SelectableIdentities(LatestSelectorView(harness.Posted));
                harness.Coordinator.OpenSelector().Should().BeTrue();
                harness.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down).Should().BeTrue();

                // Act
                harness
                    .Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Enter)
                    .Should()
                    .BeTrue();
                BreadcrumbRowRender selected = SelectedRender(harness.Posted);

                // Assert
                identities.Should().OnlyHaveUniqueItems();
                harness.Coordinator.CommittedIdentity.Should().Be(identities[1]);
                selected.RowIndex.Should().Be(1);
                selected.PercentText.Should().Be(PercentageFormatter.FormatPercent(0.25));
                harness.Coordinator.GetSelectedFolder().Should().Be(DuplicatePath);
            }
        }

        private static Harness CreateHarness(IReadOnlyList<FolderRow> rows)
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var key = new FolderTreeNodeKey("store", "shared", DuplicatePath);
            provider
                .Setup(candidate =>
                    candidate.ResolveLeafKeyAsync(DuplicatePath, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(key);
            provider
                .Setup(candidate =>
                    candidate.GetAncestorChainAsync(key, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new[] { new FolderBreadcrumbSegment(key, "Shared", DuplicatePath, false) }
                );

            var posted = new List<string>();
            var surface = new Mock<IWebViewMessenger>();
            surface
                .Setup(messenger => messenger.PostJson(It.IsAny<string>()))
                .Callback<string>(posted.Add);
            var hub = new BreadcrumbMessengerHub();
            hub.Attach(surface.Object, BreadcrumbSelectorViewMode.Collapsed);
            var coordinator = new BreadcrumbBridgeCoordinator(
                hub,
                provider.Object,
                BreadcrumbUiDispatcher.CreateForCurrentThreadTests()
            );
            coordinator.SetSuggestions(rows);
            coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();
            coordinator.SelectRow(0);
            return new Harness(hub, coordinator, posted);
        }

        private static string LatestSelectorView(IReadOnlyList<string> posted) =>
            posted.Last(json => json.Contains("\"type\":\"selectorView\""));

        private static string[] SelectableIdentities(string selectorView) =>
            Regex
                .Matches(
                    selectorView,
                    @"""identity"":""(?<identity>(?:\\.|[^""])*)"",""isSelectable"":true",
                    RegexOptions.CultureInvariant
                )
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(match => Regex.Unescape(match.Groups["identity"].Value))
                .ToArray();

        private static BreadcrumbRowRender SelectedRender(IEnumerable<string> posted) =>
            posted
                .Where(json => json.Contains("\"type\":\"render\""))
                .Select(BreadcrumbBridgeSerializer.Parse)
                .OfType<RenderMessage>()
                .Last()
                .Rows.Single(row => row.Selected);

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

        private sealed class Harness : IDisposable
        {
            public Harness(
                BreadcrumbMessengerHub hub,
                BreadcrumbBridgeCoordinator coordinator,
                List<string> posted
            )
            {
                Hub = hub;
                Coordinator = coordinator;
                Posted = posted;
            }

            public BreadcrumbMessengerHub Hub { get; }
            public BreadcrumbBridgeCoordinator Coordinator { get; }
            public List<string> Posted { get; }

            public void Dispose()
            {
                Hub.Dispose();
            }
        }
    }
}
