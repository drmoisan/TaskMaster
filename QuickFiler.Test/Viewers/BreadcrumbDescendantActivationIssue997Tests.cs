using System;
using System.Collections.Generic;
using System.Drawing;
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
    [TestClass]
    public sealed class BreadcrumbDescendantActivationIssue997Tests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ActivateDescendant_NotifiesAndClosesPopupOnce(bool leaf)
        {
            using (var harness = new Harness())
            {
                harness.Open();
                var node = harness.Render.Rows[0].Subfolders[0];
                if (leaf)
                {
                    harness.Receive(
                        new BreadcrumbSelectorDescendantMessage(
                            harness.Coordinator.CommittedIdentity,
                            node.Identity,
                            true
                        )
                    );
                    node = harness.Render.Rows[0].Subfolders[0].Children[0];
                }
                harness.Receive(
                    new BreadcrumbSelectorDescendantMessage(
                        harness.Coordinator.CommittedIdentity,
                        node.Identity
                    )
                );
                harness.SelectionCount.Should().Be(1);
                harness.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit);
                harness.FocusCount.Should().Be(1);
                harness.Coordinator.IsSelectorOpen.Should().BeFalse();
                harness
                    .Coordinator.GetSelectedFolder()
                    .Should()
                    .Be(leaf ? "Projects\\Apollo\\Alpha\\Review" : "Projects\\Apollo\\Alpha");
                harness.Render.SelectedDescendantIdentity.Should().Be(node.Identity);
                harness.Open();
                harness.Coordinator.CancelSelector();
                harness.SelectionCount.Should().Be(1);
                harness
                    .Coordinator.GetSelectedFolder()
                    .Should()
                    .Be(leaf ? "Projects\\Apollo\\Alpha\\Review" : "Projects\\Apollo\\Alpha");
            }
        }

        [TestMethod]
        public void ToggleDescendant_DoesNotRaiseSelectionChanged()
        {
            using (var harness = new Harness())
            {
                harness.Open();
                var node = harness.Render.Rows[0].Subfolders[0];
                harness.Receive(
                    new BreadcrumbSelectorDescendantMessage(
                        harness.Coordinator.CommittedIdentity,
                        node.Identity,
                        true
                    )
                );
                harness.Render.Rows[0].Subfolders[0].Children.Should().ContainSingle();
                harness.Receive(
                    new BreadcrumbSelectorDescendantMessage(
                        harness.Coordinator.CommittedIdentity,
                        node.Identity,
                        true
                    )
                );
                harness.Render.Rows[0].Subfolders[0].Expanded.Should().BeFalse();
                harness.SelectionCount.Should().Be(0);
                harness.CloseReasons.Should().BeEmpty();
                harness.FocusCount.Should().Be(0);
                harness.Coordinator.GetSelectedFolder().Should().Be("Projects\\Apollo");
            }
        }

        [TestMethod]
        public void InvalidDescendantMessages_DoNotNotifyCloseFocusOrFetch()
        {
            using (var harness = new Harness())
            {
                harness.Open();
                harness.Receive(
                    "{\"type\":\"selectorDescendantActivate\",\"rowIdentity\":\"row\",\"nodeIdentity\":null}"
                );
                harness.Receive(
                    "{\"type\":\"selectorDescendantActivate\",\"rowIdentity\":\"missing\",\"nodeIdentity\":\"unknown\",\"path\":\"Projects\\\\Apollo\\\\Alpha\"}"
                );
                harness.Receive(
                    new BreadcrumbSelectorDescendantMessage(
                        harness.Coordinator.CommittedIdentity,
                        "unloaded",
                        true
                    )
                );
                harness.SelectionCount.Should().Be(0);
                harness.CloseReasons.Should().BeEmpty();
                harness.FocusCount.Should().Be(0);
                harness.Coordinator.IsSelectorOpen.Should().BeTrue();
            }
        }

        private sealed class Harness : IDisposable
        {
            private const string Root = "\\\\mailbox\\Archive";
            private readonly SynchronizationContext _previous = SynchronizationContext.Current;
            private readonly TrackingSubfolderMessenger _messenger =
                new TrackingSubfolderMessenger();
            private bool _hostOpen;
            private readonly QuickFiler.ItemViewer _viewer;

            internal Harness()
            {
                SynchronizationContext.SetSynchronizationContext(new InlineContext());
                var parent = Segment("apollo", Root + "\\Projects\\Apollo", true);
                var child = Segment("alpha", parent.FolderPath + "\\Alpha", true);
                var leaf = Segment("review", child.FolderPath + "\\Review", false);
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                provider
                    .Setup(p =>
                        p.ResolveLeafKeyAsync("Projects\\Apollo", It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(parent.Key);
                provider
                    .Setup(p => p.GetAncestorChainAsync(parent.Key, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new[] { parent });
                provider
                    .Setup(p =>
                        p.GetImmediateSubfoldersAsync(parent.Key, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(new[] { child });
                provider
                    .Setup(p =>
                        p.GetImmediateSubfoldersAsync(child.Key, It.IsAny<CancellationToken>())
                    )
                    .ReturnsAsync(new[] { leaf });
                _viewer = new QuickFiler.ItemViewer();
                _viewer.InitializeBreadcrumbPipeline(
                    provider.Object,
                    BreadcrumbPopupUiOperations.CaptureCurrent(),
                    Root
                );
                var host = new Mock<IBreadcrumbDropDownHost>();
                host.SetupGet(h => h.IsOpen).Returns(() => _hostOpen);
                host.SetupGet(h => h.PopupMessenger).Returns((IWebViewMessenger)null);
                host.Setup(h =>
                        h.OpenAsync(It.IsAny<Rectangle>(), It.IsAny<Rectangle>(), It.IsAny<Size>())
                    )
                    .Returns(() =>
                    {
                        _hostOpen = true;
                        return Task.FromResult(true);
                    });
                host.Setup(h => h.Close(It.IsAny<BreadcrumbDropDownCloseReason>()))
                    .Returns<BreadcrumbDropDownCloseReason>(reason =>
                    {
                        if (!_hostOpen)
                        {
                            return false;
                        }
                        _hostOpen = false;
                        CloseReasons.Add(reason);
                        FocusCount++;
                        return true;
                    });
                _viewer.ConfigureBreadcrumbDropDown(
                    host.Object,
                    () => new Rectangle(0, 0, 400, 25),
                    () => new Rectangle(0, 0, 1920, 1080)
                );
                _viewer.AttachBreadcrumbMessenger(_messenger);
                Coordinator
                    .SetSuggestionsAsync(
                        new[]
                        {
                            new FolderRow(
                                "Projects\\Apollo",
                                FolderRowKind.Suggestion,
                                new FolderScore("Projects\\Apollo", 100, 0.73)
                            ),
                        },
                        CancellationToken.None
                    )
                    .GetAwaiter()
                    .GetResult();
                Coordinator.SelectRow(0);
                Receive("{\"type\":\"affordanceToggle\",\"rowIndex\":0}");
                Coordinator.SelectionChanged += (sender, args) => SelectionCount++;
            }

            internal BreadcrumbBridgeCoordinator Coordinator => _viewer.BreadcrumbCoordinator;
            internal RenderMessage Render =>
                (RenderMessage)
                    BreadcrumbBridgeSerializer.Parse(
                        _messenger.PostedMessages.Last(json => json.Contains("\"type\":\"render\""))
                    );
            internal int SelectionCount { get; private set; }
            internal int FocusCount { get; private set; }
            internal List<BreadcrumbDropDownCloseReason> CloseReasons { get; } =
                new List<BreadcrumbDropDownCloseReason>();

            internal void Open() => _viewer.SetBreadcrumbDropDownState(true);

            internal void Receive(BreadcrumbSelectorMessage message) =>
                Receive(BreadcrumbSelectorMessageSerializer.Serialize(message));

            internal void Receive(string json)
            {
                _messenger.Receive(json);
                Coordinator.LastDispatch.GetAwaiter().GetResult();
            }

            private static FolderBreadcrumbSegment Segment(
                string entry,
                string path,
                bool children
            ) =>
                new FolderBreadcrumbSegment(
                    new FolderTreeNodeKey("store-a", entry, path),
                    entry,
                    path,
                    children
                );

            public void Dispose()
            {
                try
                {
                    _viewer.Dispose();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(_previous);
                }
            }
        }

        private sealed class InlineContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state) => callback(state);

            public override void Send(SendOrPostCallback callback, object state) => callback(state);
        }
    }
}
