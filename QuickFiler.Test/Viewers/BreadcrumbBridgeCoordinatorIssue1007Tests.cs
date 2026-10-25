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
    /// Issue #1007 QFC coordinator keyboard and click routing through a messenger hub over a
    /// strict provider. Rows: Archive &gt; Projects &gt; Alpha (children Specs, Docs) and
    /// Archive &gt; Clients &gt; Beta (no children). Events are recorded in raise order.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbBridgeCoordinatorIssue1007Tests
    {
        private const string Projects = @"\Archive\Projects";
        private const string Alpha = Projects + @"\Alpha";
        private const string Beta = @"\Archive\Clients\Beta";

        /// <summary>AC4: open-list Left on a non-expanded pending entry commits it and closes.</summary>
        [TestMethod]
        public void OpenLeft_OnNonExpandedEntry_CommitsAndCloses()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true);
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);
            // Act
            h.Receive(Arrow("left"));
            // Assert
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
            h.Coordinator.GetSelectedFolder().Should().Be(Beta);
            h.Events.Should().Equal("selection", "open-state");
        }

        /// <summary>AC6 (D-Q21): open-list Right on a leaf closes the list first, then pops out.</summary>
        [TestMethod]
        public void OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true);
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);
            // Act
            h.Receive(Arrow("right"));
            // Assert
            h.Events.Should().Equal("open-state", "unhandled:Right");
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha, "the original choice is kept");
        }

        /// <summary>AC8: open-list Ctrl+Left shortens the pending row and changes nothing committed.</summary>
        [TestMethod]
        public void SelectorKeyParent_Open_MovesPendingOnly()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true);
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);
            // Act
            h.Receive(Key("parent"));
            // Assert
            h.Coordinator.IsSelectorOpen.Should().BeTrue();
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
            h.Events.Should().BeEmpty();
            h.LastRender().Should().Contain("\"collapsed\":true");
        }

        /// <summary>AC11: open-list Home and End move the pending highlight to the first and last entry.</summary>
        [TestMethod]
        public void SelectorKeyHomeEnd_Open_MovePendingToFirstAndLastVisibleEntry()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true);
            // Act
            h.Receive(Key("end"));
            string afterEnd = h.Coordinator.PendingIdentity;
            h.Receive(Key("home"));
            // Assert
            afterEnd.Should().Be(h.Identity(1));
            h.Coordinator.PendingIdentity.Should().Be(h.Identity(0));
            h.Events.Should().BeEmpty();
        }

        /// <summary>AC11: open-list PageDown moves the pending highlight by the page size.</summary>
        [TestMethod]
        public void SelectorKeyPageDown_Open_MovesPendingByPageSize()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true, expandAlpha: true);
            // Act
            h.Receive("{\"type\":\"selectorKey\",\"key\":\"pagedown\",\"pageSize\":2}");
            // Assert
            h.LastView()
                .Should()
                .Contain("\"pendingNodeIdentity\":\"" + h.NodeIdentity("Docs") + "\"");
        }

        /// <summary>AC4: the selector view carries the pending node identity for the page highlight.</summary>
        [TestMethod]
        public void SelectorView_PendingNode_CarriesPendingNodeIdentity()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true, expandAlpha: true);
            // Act
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);
            // Assert
            h.LastView()
                .Should()
                .Contain("\"pendingNodeIdentity\":\"" + h.NodeIdentity("Specs") + "\"");
        }

        /// <summary>AC4: Enter on a pending node commits the descendant and raises one selection change.</summary>
        [TestMethod]
        public void SelectorKeyEnter_Open_WithPendingNode_CommitsDescendantAndRaisesSelectionChanged()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true, expandAlpha: true);
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down);
            // Act
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Enter);
            // Assert
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha + @"\Specs");
            h.Events.Should().Equal("selection", "open-state");
        }

        /// <summary>AC10 (D-Q29): closed Right opens the list with the pending on the visible committed subfolder.</summary>
        [TestMethod]
        public void ClosedRight_OpensSelectorWithPendingOnVisibleCommittedSubfolder()
        {
            // Arrange
            var h = new Harness(commit: 0, expandAlpha: true);
            h.CommitSpecs();
            // Act
            h.Receive(Arrow("right"));
            // Assert
            h.Coordinator.IsSelectorOpen.Should().BeTrue();
            h.LastView()
                .Should()
                .Contain("\"pendingNodeIdentity\":\"" + h.NodeIdentity("Specs") + "\"");
            h.Events.Should().Equal("open-state");
            h.KeyDowns.Should().Be(0, "a consumed arrow raises no synthetic key");
        }

        /// <summary>AC10 (D-Q29): with the committed subfolder hidden, closed Right pends its row.</summary>
        [TestMethod]
        public void ClosedRight_SubfolderHidden_PendingOnRow()
        {
            // Arrange
            var h = new Harness(commit: 0, expandAlpha: true);
            h.CommitSpecs();
            h.Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");
            // Act
            h.Receive(Arrow("right"));
            // Assert
            h.Coordinator.IsSelectorOpen.Should().BeTrue();
            h.Coordinator.PendingIdentity.Should().Be(h.Identity(0));
            h.LastView().Should().NotContain("pendingNodeIdentity");
        }

        /// <summary>AC10 (D-Q14): closed Left is consumed: no event, no synthetic key, no change.</summary>
        [TestMethod]
        public void ClosedLeft_IsConsumedWithoutEffect()
        {
            // Arrange
            var h = new Harness(commit: 0);
            // Act
            h.Receive(Arrow("left"));
            // Assert
            h.Events.Should().BeEmpty();
            h.KeyDowns.Should().Be(0);
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
        }

        /// <summary>AC10 (D-Q16): closed Home and End commit the first and last top-level row.</summary>
        [TestMethod]
        public void ClosedHomeEnd_CommitFirstAndLastTopLevelRow()
        {
            // Arrange
            var h = new Harness(commit: 0, expandAlpha: true);
            // Act
            h.Receive(Key("end"));
            string afterEnd = h.Coordinator.GetSelectedFolder();
            h.Receive(Key("home"));
            // Assert
            afterEnd.Should().Be(Beta);
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
            h.Events.Should().Equal("selection", "selection");
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
        }

        /// <summary>AC8, AC10 (D-Q19): closed Ctrl+Left commits the parent segment immediately.</summary>
        [TestMethod]
        public void SelectorKeyParent_Closed_CommitsImmediately()
        {
            // Arrange
            var h = new Harness(commit: 0);
            // Act
            h.Receive(Key("parent"));
            // Assert
            h.Coordinator.GetSelectedFolder().Should().Be(Projects);
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
            h.Events.Should().Equal("selection");
            h.LastRender().Should().Contain("\"collapsed\":true");
        }

        /// <summary>Guard (AC10, D-Q25): closed PageUp and PageDown do nothing.</summary>
        [TestMethod]
        public void ClosedPageKeys_AreNoOps()
        {
            // Arrange
            var h = new Harness(commit: 0, expandAlpha: true);
            // Act
            h.Receive("{\"type\":\"selectorKey\",\"key\":\"pagedown\",\"pageSize\":2}");
            h.Receive("{\"type\":\"selectorKey\",\"key\":\"pageup\",\"pageSize\":2}");
            // Assert
            h.Events.Should().BeEmpty();
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
        }

        /// <summary>Guard (AC10, D-Q22): closed Up and Down commit the adjacent top-level row, skipping subfolders.</summary>
        [TestMethod]
        public void ClosedUpDown_KeepDirectCommitOfAdjacentTopLevelRow()
        {
            // Arrange
            var h = new Harness(commit: 0, expandAlpha: true);
            // Act
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down).Should().BeTrue();
            string afterDown = h.Coordinator.GetSelectedFolder();
            h.Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Up).Should().BeTrue();
            // Assert
            afterDown.Should().Be(Beta);
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
            h.Events.Should().Equal("selection", "selection");
        }

        /// <summary>AC9 (D-Q27, S5): an open-list segment double-click commits that segment's folder and closes.</summary>
        [TestMethod]
        public void SelectorSegmentDoubleClick_CommitsSegmentFolderAndCloses()
        {
            // Arrange
            var h = new Harness(commit: 1, open: true);
            // Act
            h.Receive(DoubleClick(h.Identity(0), 1));
            // Assert
            h.Coordinator.GetSelectedFolder().Should().Be(Projects);
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
            h.Events.Should().Equal("selection", "open-state");
            h.LastRender().Should().Contain("\"collapsed\":true");
        }

        /// <summary>AC9 (D-Q13): a closed-list segment double-click commits the segment folder.</summary>
        [TestMethod]
        public void SelectorSegmentDoubleClick_Closed_CommitsSegmentFolder()
        {
            // Arrange
            var h = new Harness(commit: 1);
            // Act
            h.Receive(DoubleClick(h.Identity(0), 1));
            // Assert
            h.Coordinator.GetSelectedFolder().Should().Be(Projects);
            h.Coordinator.IsSelectorOpen.Should().BeFalse();
            h.Events.Should().Equal("selection");
        }

        /// <summary>AC9 (D-Q13, D-Q23): after the double-click, the [+] lists every child of the segment.</summary>
        [TestMethod]
        public void SelectorSegmentDoubleClick_ThenAffordanceToggle_ListsSegmentChildren()
        {
            // Arrange
            var h = new Harness(commit: 0);
            h.Receive(DoubleClick(h.Identity(0), 1));
            // Act
            h.Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");
            // Assert
            h.LastRender().Should().Contain("\"displayName\":\"Omega\"");
            h.LastRender().Should().Contain("\"collapsed\":true");
        }

        /// <summary>AC9 (D-Q27): an open-list single click on a non-leaf segment moves only the pending highlight.</summary>
        [TestMethod]
        public void SelectorHighlight_Open_MovesPendingOnlyAndKeepsListOpen()
        {
            // Arrange
            var h = new Harness(commit: 0, open: true);
            string beta = h.Identity(1);
            // Act
            h.Receive(
                "{\"type\":\"selectorHighlight\",\"identity\":\""
                    + beta.Replace("\\", "\\\\")
                    + "\"}"
            );
            // Assert
            h.Coordinator.PendingIdentity.Should().Be(beta);
            h.Coordinator.IsSelectorOpen.Should().BeTrue();
            h.Coordinator.GetSelectedFolder().Should().Be(Alpha);
            h.Events.Should().BeEmpty();
        }

        private static string DoubleClick(string rowIdentity, int segmentIndex) =>
            "{\"type\":\"selectorSegmentDoubleClick\",\"rowIdentity\":\""
            + rowIdentity.Replace("\\", "\\\\")
            + "\",\"segmentIndex\":"
            + segmentIndex
            + "}";

        private static string Arrow(string direction) =>
            "{\"type\":\"arrowKey\",\"direction\":\"" + direction + "\"}";

        private static string Key(string key) =>
            "{\"type\":\"selectorKey\",\"key\":\"" + key + "\"}";

        private sealed class Harness
        {
            private readonly Page _page = new Page();

            internal Harness(int commit, bool open = false, bool expandAlpha = false)
            {
                var hub = new BreadcrumbMessengerHub();
                hub.Attach(_page, BreadcrumbSelectorViewMode.Expanded);
                SynchronizationContext prior = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new InlineContext());
                try
                {
                    Coordinator = new BreadcrumbBridgeCoordinator(hub, ProviderMock().Object);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(prior);
                }
                FolderRow[] rows = { Row(Alpha), Row(Beta) };
                Coordinator
                    .SetSuggestionsAsync(rows, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
                if (expandAlpha)
                {
                    Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");
                }
                Coordinator.SelectRow(commit);
                if (open)
                {
                    Coordinator.OpenSelector().Should().BeTrue();
                }
                Coordinator.SelectionChanged += (s, e) => Events.Add("selection");
                Coordinator.SelectorOpenStateChanged += (s, e) => Events.Add("open-state");
                Coordinator.UnhandledArrow += (s, d) => Events.Add("unhandled:" + d);
                Coordinator.FolderArrowKeyDown += (s, d) => KeyDowns++;
            }

            internal BreadcrumbBridgeCoordinator Coordinator { get; }

            internal List<string> Events { get; } = new List<string>();

            internal int KeyDowns { get; private set; }

            /// <summary>Commits the Specs subfolder of the expanded Alpha row, then clears the event log.</summary>
            internal void CommitSpecs()
            {
                Coordinator.OpenSelector().Should().BeTrue();
                Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down).Should().BeTrue();
                Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Enter).Should().BeTrue();
                Coordinator.GetSelectedFolder().Should().Be(Alpha + @"\Specs");
                Events.Clear();
            }

            internal void Receive(string json)
            {
                _page.Raise(json);
                Coordinator.LastDispatch.GetAwaiter().GetResult();
            }

            internal string LastView() => _page.Posted.Last(p => p.Contains("\"selectorView\""));

            internal string LastRender() =>
                _page.Posted.Last(p => p.Contains("\"type\":\"render\""));

            internal string Identity(int row) =>
                Regex.Unescape(
                    Regex.Matches(LastView(), "\"identity\":\"([^\"]+)\"")[row].Groups[1].Value
                );

            internal string NodeIdentity(string name) =>
                Regex
                    .Match(
                        LastRender(),
                        "\"displayName\":\"" + name + "\"[^}]*?\"identity\":\"([0-9a-f]+)\""
                    )
                    .Groups[1]
                    .Value;

            private static Mock<IFolderHierarchyProvider> ProviderMock()
            {
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                Chain(
                    provider,
                    Alpha,
                    Seg(@"\Archive", true),
                    Seg(Projects, true),
                    Seg(Alpha, true)
                );
                Chain(
                    provider,
                    Beta,
                    Seg(@"\Archive", true),
                    Seg(@"\Archive\Clients", true),
                    Seg(Beta, false)
                );
                Children(
                    provider,
                    Alpha,
                    Seg(Alpha + @"\Specs", true),
                    Seg(Alpha + @"\Docs", false)
                );
                Children(provider, Projects, Seg(Alpha, true), Seg(Projects + @"\Omega", false));
                return provider;
            }

            private static void Chain(
                Mock<IFolderHierarchyProvider> p,
                string leaf,
                params FolderBreadcrumbSegment[] chain
            )
            {
                p.Setup(x => x.ResolveLeafKeyAsync(leaf, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(K(leaf));
                p.Setup(x => x.GetAncestorChainAsync(K(leaf), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(chain);
            }

            private static void Children(
                Mock<IFolderHierarchyProvider> p,
                string parent,
                params FolderBreadcrumbSegment[] kids
            ) =>
                p.Setup(x =>
                        x.GetImmediateSubfoldersAsync(K(parent), It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(kids);

            private static FolderRow Row(string path) =>
                new FolderRow(path, FolderRowKind.Suggestion, new FolderScore(path, 1000, 0.5));

            private static FolderTreeNodeKey K(string path) =>
                new FolderTreeNodeKey("store", path, path);

            private static FolderBreadcrumbSegment Seg(string path, bool hasChildren) =>
                new FolderBreadcrumbSegment(
                    K(path),
                    path.Substring(path.LastIndexOf('\\') + 1),
                    path,
                    hasChildren
                );
        }

        private sealed class Page : IWebViewMessenger
        {
            public event EventHandler<string> MessageReceived;

            internal List<string> Posted { get; } = new List<string>();

            public void PostJson(string json) => Posted.Add(json);

            internal void Raise(string json) => MessageReceived?.Invoke(this, json);
        }

        private sealed class InlineContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state) => callback(state);
        }
    }
}
