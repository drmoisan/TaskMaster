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
    /// <summary>
    /// Unit tests for <see cref="BreadcrumbBridgeCoordinator"/> (#351 P4-T5) with Moq-mocked
    /// <see cref="IWebViewMessenger"/> and <see cref="IFolderHierarchyProvider"/> (completed tasks
    /// only, G11): positive message wiring (double-click collapse render, expand subfolder
    /// response, selection -&gt; SelectionChanged with the mapped path), negative posting of router
    /// error responses (malformed inbound JSON; provider failure), and edge events
    /// (UnhandledArrow left/right, synthetic arrow FolderKeyDown, Clear emptying selection).
    /// No live WebView2 or Outlook.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeCoordinatorTests
    {
        private const string LeafPath = "\\Inbox\\Projects\\Apollo";

        private static readonly FolderTreeNodeKey RootKey = MakeKey("root", "\\Inbox");
        private static readonly FolderTreeNodeKey MidKey = MakeKey("mid", "\\Inbox\\Projects");
        private static readonly FolderTreeNodeKey LeafKey = MakeKey("leaf", LeafPath);

        private static FolderTreeNodeKey MakeKey(string entryId, string path)
        {
            return new FolderTreeNodeKey("store-a", entryId, path);
        }

        private static FolderBreadcrumbSegment Segment(
            FolderTreeNodeKey key,
            string name,
            bool hasChildren
        )
        {
            return new FolderBreadcrumbSegment(key, name, key.FolderPath, hasChildren);
        }

        private static Mock<IFolderHierarchyProvider> ProviderMock(bool leafHasChildren)
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
                        Segment(LeafKey, "Apollo", leafHasChildren),
                    }
                );
            provider
                .Setup(p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new[] { Segment(MakeKey("s1", LeafPath + "\\Alpha"), "Alpha", false) }
                );
            return provider;
        }

        private sealed class Harness
        {
            public Mock<IWebViewMessenger> Messenger;
            public Mock<IFolderHierarchyProvider> Provider;
            public BreadcrumbBridgeCoordinator Coordinator;
            public List<string> Posted;

            public void Receive(string json)
            {
                Messenger.Raise(m => m.MessageReceived += null, Messenger.Object, json);
                Coordinator.LastDispatch.GetAwaiter().GetResult();
            }

            public List<BreadcrumbBridgeMessage> PostedMessages()
            {
                return Posted.Select(BreadcrumbBridgeSerializer.Parse).ToList();
            }
        }

        private sealed class InlineSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state) => callback(state);
        }

        private static BreadcrumbBridgeCoordinator CreateContextOwnedCoordinator(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider
        )
        {
            SynchronizationContext priorContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(
                    new InlineSynchronizationContext()
                );
                return new BreadcrumbBridgeCoordinator(messenger, provider);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(priorContext);
            }
        }

        private static Harness CreateHarness(bool leafHasChildren = true, bool populate = true)
        {
            var harness = new Harness
            {
                Messenger = new Mock<IWebViewMessenger>(),
                Provider = ProviderMock(leafHasChildren),
                Posted = new List<string>(),
            };
            harness
                .Messenger.Setup(m => m.PostJson(It.IsAny<string>()))
                .Callback<string>(harness.Posted.Add);
            harness.Coordinator = CreateContextOwnedCoordinator(
                harness.Messenger.Object,
                harness.Provider.Object
            );
            if (populate)
            {
                var row = new FolderRow(
                    LeafPath,
                    FolderRowKind.Suggestion,
                    new FolderScore(LeafPath, 1000, 0.73)
                );
                harness
                    .Coordinator.SetSuggestionsAsync(new[] { row }, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                harness.Coordinator.SelectRow(0);
                harness.Posted.Clear();
            }
            return harness;
        }

        // --- Positive wiring ---

        [TestMethod]
        public void InboundDoubleClick_PostsCollapsedRenderJson()
        {
            // Arrange
            var harness = CreateHarness();

            // Act
            harness.Receive("{\"type\":\"segmentDoubleClick\",\"rowIndex\":0,\"segmentIndex\":0}");

            // Assert (FR-3): the posted render payload shows the collapsed row.
            var render = harness.PostedMessages().OfType<RenderMessage>().Single();
            render.Rows[0].Collapsed.Should().BeTrue();
            render.Rows[0].Cells[0].Kind.Should().Be(BreadcrumbCellKind.Plus);
        }

        [TestMethod]
        public void InboundExpand_PostsRenderAndSubfolderResponse()
        {
            // Arrange
            var harness = CreateHarness();

            // Act
            harness.Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");

            // Assert (FR-4): render + subfolderResponse posted, provider queried once.
            var messages = harness.PostedMessages();
            messages.OfType<RenderMessage>().Single().Rows[0].LeafExpanded.Should().BeTrue();
            messages
                .OfType<SubfolderResponseMessage>()
                .Single()
                .Subfolders.Single()
                .FolderPath.Should()
                .Be(LeafPath + "\\Alpha");
            harness.Provider.Verify(
                p => p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [TestMethod]
        public void InboundSelectionMessage_RaisesSelectionChangedWithMappedPath()
        {
            // Arrange
            var harness = CreateHarness();
            string mappedAtEvent = null;
            int raised = 0;
            harness.Coordinator.SelectionChanged += (s, e) =>
            {
                raised++;
                mappedAtEvent = harness.Coordinator.GetSelectedFolder();
            };

            // Act
            harness.Receive("{\"type\":\"selectionChange\",\"rowIndex\":0}");

            // Assert (FR-7): the event observes the mapped full folder path.
            raised.Should().Be(1);
            mappedAtEvent.Should().Be(LeafPath);
        }

        // --- Negative wiring ---

        [TestMethod]
        public void MalformedInboundMessage_PostsRouterErrorResponse()
        {
            // Arrange
            SynchronizationContext priorContext = SynchronizationContext.Current;
            var harness = CreateHarness();
            SynchronizationContext.Current.Should().BeSameAs(priorContext);

            // Act
            harness.Receive("{oops");

            // Assert
            harness
                .PostedMessages()
                .OfType<BridgeErrorMessage>()
                .Single()
                .Message.Should()
                .Contain("Malformed");
        }

        [TestMethod]
        public void ProviderFailure_SurfacesExplicitErrorResponse()
        {
            // Arrange
            var harness = CreateHarness();
            harness
                .Provider.Setup(p =>
                    p.GetImmediateSubfoldersAsync(LeafKey, It.IsAny<CancellationToken>())
                )
                .Returns(
                    Task.FromException<IReadOnlyList<FolderBreadcrumbSegment>>(
                        new InvalidOperationException("store offline")
                    )
                );

            // Act
            harness.Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");

            // Assert
            harness
                .PostedMessages()
                .OfType<BridgeErrorMessage>()
                .Single()
                .Message.Should()
                .Contain("store offline");
        }

        // --- Edge events ---

        [TestMethod]
        public void UnhandledRightArrow_RaisesUnhandledArrowRight()
        {
            // Arrange: leaf without subfolders -> Right cannot expand (legacy Pop Out fall-through).
            var harness = CreateHarness(leafHasChildren: false);
            var directions = new List<BreadcrumbArrowDirection>();
            harness.Coordinator.UnhandledArrow += (s, d) => directions.Add(d);

            // Act
            harness.Receive("{\"type\":\"arrowKey\",\"direction\":\"right\"}");

            // Assert
            directions.Should().Equal(BreadcrumbArrowDirection.Right);
        }

        [TestMethod]
        public void UnhandledLeftArrow_RaisesUnhandledArrowLeft()
        {
            // Arrange: nothing expanded -> Left cannot collapse (legacy close fall-through).
            var harness = CreateHarness();
            var directions = new List<BreadcrumbArrowDirection>();
            harness.Coordinator.UnhandledArrow += (s, d) => directions.Add(d);

            // Act: the page-side report path is honored identically to the routed arrow path.
            harness.Receive("{\"type\":\"unhandledArrow\",\"direction\":\"left\"}");

            // Assert
            directions.Should().Equal(BreadcrumbArrowDirection.Left);
        }

        [TestMethod]
        public void ArrowMessages_RaiseSyntheticFolderKeyDown()
        {
            // Arrange
            var harness = CreateHarness();
            var keys = new List<BreadcrumbArrowDirection>();
            harness.Coordinator.FolderArrowKeyDown += (s, d) => keys.Add(d);

            // Act: a handled Right (expands) and an unhandled Left report.
            harness.Receive("{\"type\":\"arrowKey\",\"direction\":\"right\"}");
            harness.Receive("{\"type\":\"unhandledArrow\",\"direction\":\"left\"}");

            // Assert: the synthetic key seam fires for every arrow message (FR-6).
            keys.Should().Equal(BreadcrumbArrowDirection.Right, BreadcrumbArrowDirection.Left);
        }

        [TestMethod]
        public void Clear_EmptiesSelectionStateAndPostsEmptyRender()
        {
            // Arrange
            var harness = CreateHarness();
            harness.Coordinator.GetSelectedFolder().Should().Be(LeafPath);

            // Act
            harness.Coordinator.Clear();

            // Assert
            harness.Coordinator.GetSelectedFolder().Should().BeNull();
            harness.Coordinator.GetFolderItems().Should().BeEmpty();
            harness.PostedMessages().OfType<RenderMessage>().Single().Rows.Should().BeEmpty();
        }

        [TestMethod]
        public void AddItems_AppendsPlainRowsAndContainsFindsThem()
        {
            // Arrange
            var harness = CreateHarness();

            // Act (Path B: verbatim rows including the literal "Trash to Delete").
            harness.Coordinator.AddItems(new[] { "Trash to Delete" });

            // Assert
            harness.Coordinator.Contains("Trash to Delete").Should().BeTrue();
            harness.Coordinator.GetFolderItems().Should().Equal(LeafPath, "Trash to Delete");
        }

        [TestMethod]
        public void SetSuggestions_SyncFacade_PopulatesImmediatelyThenUpgradesPreservingSelection()
        {
            // Arrange: an empty coordinator (population not yet run).
            var harness = CreateHarness(populate: false);
            var row = new FolderRow(
                LeafPath,
                FolderRowKind.Suggestion,
                new FolderScore(LeafPath, 1000, 0.73)
            );

            // Act: the void IItemViewer.SetFolderSuggestions path.
            harness.Coordinator.SetSuggestions(new[] { row });

            // Assert (immediate): the selection contract holds synchronously via plain-path rows.
            harness.Coordinator.Contains(LeafPath).Should().BeTrue();
            harness.Coordinator.SelectRow(0);
            harness.Coordinator.GetSelectedFolder().Should().Be(LeafPath);

            // Assert (upgrade): completed-task provider -> chain rows with preserved selection.
            harness.Coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();
            harness.Coordinator.GetSelectedFolder().Should().Be(LeafPath);
            var lastRender = harness.PostedMessages().OfType<RenderMessage>().Last();
            lastRender.Rows[0].IsSuggestion.Should().BeTrue("the upgrade attaches the chain");
            lastRender.Rows[0].PercentText.Should().Be("73%");
        }

        [TestMethod]
        public void SetSuggestions_NullRows_Throws()
        {
            var harness = CreateHarness(populate: false);
            ((Action)(() => harness.Coordinator.SetSuggestions(null)))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SelectItem_KnownItemSelects_UnknownItemIsNoOp()
        {
            // Arrange
            var harness = CreateHarness();
            int raised = 0;
            harness.Coordinator.SelectionChanged += (s, e) => raised++;

            // Act + Assert: known item selects and raises the event.
            harness.Coordinator.SelectItem(LeafPath);
            raised.Should().Be(1);
            harness.Coordinator.GetSelectedFolder().Should().Be(LeafPath);

            // Unknown item: legacy ComboBox no-op — no event, selection untouched.
            harness.Coordinator.SelectItem("\\Nope");
            raised.Should().Be(1);
            harness.Coordinator.GetSelectedFolder().Should().Be(LeafPath);
        }

        [TestMethod]
        public void SetTheme_PostsThemeChangeMessage()
        {
            // Arrange
            var harness = CreateHarness();

            // Act
            harness.Coordinator.SetTheme("dark");

            // Assert
            harness
                .PostedMessages()
                .OfType<ThemeChangeMessage>()
                .Single()
                .Theme.Should()
                .Be("dark");
        }

        [TestMethod]
        public void Constructor_NullArguments_Throw()
        {
            // Arrange
            var messenger = new Mock<IWebViewMessenger>();
            var provider = new Mock<IFolderHierarchyProvider>();

            // Act, Assert
            ((Action)(() => new BreadcrumbBridgeCoordinator(null, provider.Object)))
                .Should()
                .Throw<ArgumentNullException>();
            ((Action)(() => new BreadcrumbBridgeCoordinator(messenger.Object, null)))
                .Should()
                .Throw<ArgumentNullException>();
        }

        // --- #398 regression: mid-upgrade host selection must not race the rebuild ---

        [TestMethod]
        public void SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection()
        {
            // Arrange: two scored suggestions. The provider resolves the first path from a
            // completed task (so the rebuild adds one row) but the second path's leaf-key resolve
            // is gated by a TaskCompletionSource, leaving the fire-and-forget upgrade parked inside
            // the rebuild — the exact window that made BreadcrumbStateModel.SelectRow(1) throw
            // ArgumentOutOfRangeException in issue #398 (transient row count of 1).
            const string firstPath = "\\Inbox\\Alpha";
            const string secondPath = "\\Inbox\\Beta";
            var firstKey = MakeKey("k-alpha", firstPath);
            var secondKey = MakeKey("k-beta", secondPath);
            var gate = new TaskCompletionSource<FolderTreeNodeKey>();

            var provider = new Mock<IFolderHierarchyProvider>();
            provider
                .Setup(p => p.ResolveLeafKeyAsync(firstPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(firstKey);
            provider
                .Setup(p => p.GetAncestorChainAsync(firstKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Segment(firstKey, "Alpha", false) });
            provider
                .Setup(p => p.ResolveLeafKeyAsync(secondPath, It.IsAny<CancellationToken>()))
                .Returns(gate.Task);
            provider
                .Setup(p => p.GetAncestorChainAsync(secondKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Segment(secondKey, "Beta", false) });

            var messenger = new Mock<IWebViewMessenger>();
            SynchronizationContext priorContext = SynchronizationContext.Current;
            var coordinator = CreateContextOwnedCoordinator(messenger.Object, provider.Object);
            SynchronizationContext.Current.Should().BeSameAs(priorContext);
            var rows = new[]
            {
                new FolderRow(
                    firstPath,
                    FolderRowKind.Suggestion,
                    new FolderScore(firstPath, 900, 0.6)
                ),
                new FolderRow(
                    secondPath,
                    FolderRowKind.Suggestion,
                    new FolderScore(secondPath, 800, 0.4)
                ),
            };

            // Act: start the synchronous facade (fire-and-forget upgrade parks on the gate), then
            // the host applies the multi-suggestion fallback selection while the upgrade is pending.
            coordinator.SetSuggestions(rows);
            coordinator.SuggestionsUpgrade.IsCompleted.Should().BeFalse("the upgrade is gated");
            Action selectDuringUpgrade = () => coordinator.SelectRow(1);

            // Assert (AC-1): the mid-upgrade selection succeeds and is applied to the second row.
            selectDuringUpgrade.Should().NotThrow<ArgumentOutOfRangeException>();
            coordinator.GetSelectedFolder().Should().Be(secondPath);

            // Release the gate and drain the upgrade: the host selection survives the atomic swap.
            gate.SetResult(secondKey);
            coordinator.SuggestionsUpgrade.GetAwaiter().GetResult();
            coordinator.GetSelectedFolder().Should().Be(secondPath);
        }
    }
}
