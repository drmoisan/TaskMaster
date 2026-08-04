using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first immediate and upgraded probability contracts for issue #400.</summary>
    [TestClass]
    public sealed class BreadcrumbBridgeCoordinatorProbabilityTests
    {
        private const string FirstPath = "\\Inbox\\Alpha";
        private const string SecondPath = "\\Inbox\\Beta";

        [TestMethod]
        public void SetSuggestions_ImmediatelyPostsScoredFallbackBeforeProviderCompletes()
        {
            // Arrange
            var gate = new TaskCompletionSource<FolderTreeNodeKey>();
            var firstKey = Key("first", FirstPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(FirstPath, It.IsAny<CancellationToken>()))
                .Returns(gate.Task);
            provider
                .Setup(p => p.GetAncestorChainAsync(firstKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(firstKey, "Alpha"));
            var posted = new List<string>();
            var messenger = Messenger(posted);
            var coordinator = CreateCoordinator(messenger.Object, provider.Object);

            // Act
            coordinator.SetSuggestions(new[] { Scored(FirstPath, 0.73) });

            // Assert before hierarchy completion
            coordinator.SuggestionsUpgrade.IsCompleted.Should().BeFalse();
            RenderMessage render = PostedRenders(posted).First();
            render.Rows[0].IsSuggestion.Should().BeTrue();
            render.Rows[0].PercentText.Should().Be(PercentageFormatter.FormatPercent(0.73));

            // Complete the deterministic gate so no work is left in flight.
            gate.SetResult(firstKey);
            coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();
        }

        [TestMethod]
        public void SetSuggestions_SuccessfulUpgradeRetainsScoreAndLatestSelection()
        {
            // Arrange
            var secondGate = new TaskCompletionSource<FolderTreeNodeKey>();
            var firstKey = Key("first", FirstPath);
            var secondKey = Key("second", SecondPath);
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            provider
                .Setup(p => p.ResolveLeafKeyAsync(FirstPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(firstKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(firstKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(firstKey, "Alpha"));
            provider
                .Setup(p => p.ResolveLeafKeyAsync(SecondPath, It.IsAny<CancellationToken>()))
                .Returns(secondGate.Task);
            provider
                .Setup(p => p.GetAncestorChainAsync(secondKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Chain(secondKey, "Beta"));
            var posted = new List<string>();
            var coordinator = CreateCoordinator(Messenger(posted).Object, provider.Object);
            coordinator.AddItems(new[] { FirstPath, SecondPath });
            coordinator.SelectRow(0);
            posted.Clear();

            // Act
            coordinator.SetSuggestions(new[] { Scored(FirstPath, 0.7), Scored(SecondPath, 0.3) });
            coordinator.SelectRow(1);
            secondGate.SetResult(secondKey);
            coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();

            // Assert
            coordinator.GetSelectedFolder().Should().Be(SecondPath);
            RenderMessage final = PostedRenders(posted).Last();
            final.Rows.Select(row => row.PercentText).Should().Equal("70%", "30%");
        }

        [TestMethod]
        public void SetSuggestions_UnresolvedEmptyAndFailureRetainFallbackProbability()
        {
            // Arrange and act
            RenderMessage unresolved = RunFallback(provider =>
                provider
                    .Setup(p => p.ResolveLeafKeyAsync(FirstPath, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((FolderTreeNodeKey)null)
            );
            RenderMessage empty = RunFallback(provider =>
            {
                var key = Key("empty", FirstPath);
                provider
                    .Setup(p => p.ResolveLeafKeyAsync(FirstPath, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(key);
                provider
                    .Setup(p => p.GetAncestorChainAsync(key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new FolderBreadcrumbSegment[0]);
            });
            RenderMessage failed = RunFallback(provider =>
                provider
                    .Setup(p => p.ResolveLeafKeyAsync(FirstPath, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("offline"))
            );

            // Assert
            foreach (RenderMessage render in new[] { unresolved, empty, failed })
            {
                render.Rows[0].PercentText.Should().Be(PercentageFormatter.FormatPercent(0.73));
                render.Rows[0].Cells.Last().Text.Should().Be("Alpha");
            }
        }

        private static RenderMessage RunFallback(Action<Mock<IFolderHierarchyProvider>> configure)
        {
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            configure(provider);
            var posted = new List<string>();
            var coordinator = CreateCoordinator(Messenger(posted).Object, provider.Object);
            coordinator.SetSuggestions(new[] { Scored(FirstPath, 0.73) });
            coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();
            return PostedRenders(posted).Last();
        }

        private static Mock<IWebViewMessenger> Messenger(List<string> posted)
        {
            var messenger = new Mock<IWebViewMessenger>();
            messenger.Setup(m => m.PostJson(It.IsAny<string>())).Callback<string>(posted.Add);
            return messenger;
        }

        private static BreadcrumbBridgeCoordinator CreateCoordinator(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider
        )
        {
            return new BreadcrumbBridgeCoordinator(
                messenger,
                provider,
                BreadcrumbUiDispatcher.CreateForCurrentThreadTests()
            );
        }

        private static IEnumerable<RenderMessage> PostedRenders(IEnumerable<string> posted) =>
            posted.Select(BreadcrumbBridgeSerializer.Parse).OfType<RenderMessage>();

        private static FolderRow Scored(string path, double probability) =>
            new FolderRow(path, FolderRowKind.Suggestion, new FolderScore(path, 100, probability));

        private static FolderTreeNodeKey Key(string entryId, string path) =>
            new FolderTreeNodeKey("store", entryId, path);

        private static IReadOnlyList<FolderBreadcrumbSegment> Chain(
            FolderTreeNodeKey key,
            string name
        ) => new[] { new FolderBreadcrumbSegment(key, name, key.FolderPath, false) };
    }
}
