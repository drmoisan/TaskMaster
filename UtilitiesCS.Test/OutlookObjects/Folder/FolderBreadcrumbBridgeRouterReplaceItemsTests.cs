#nullable enable
using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #438: contracts for <c>FolderBreadcrumbBridgeRouter.ReplaceItemsPreservingSession</c>,
    /// the session-preserving row replacement that removes the per-keystroke close/reopen cycle from
    /// the folder-search path. The router is host-neutral and its hierarchy provider is mocked, so
    /// there is no Outlook, WebView2, WinForms, timer, or temp-file dependency.
    /// </summary>
    [TestClass]
    public sealed class FolderBreadcrumbBridgeRouterReplaceItemsTests
    {
        // Banner rows are identified by BreadcrumbRowBuilder.BannerPrefix ("===="); a row whose text
        // starts with it is rendered as a non-selectable separator.
        private const string BannerText = "==== Suggested folders ====";

        /// <summary>
        /// AC-8: a replacement is a single handled transition reporting <c>RenderRequired</c> and
        /// nothing else — exactly one render payload per surface per state update (issue #400 AC-12).
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_ReportsRenderRequiredOnly()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "A", "B" });

            // Act
            BreadcrumbSelectionTransition transition = router.ReplaceItemsPreservingSession(
                new[] { "A1", "A2", "A3" }
            );

            // Assert
            transition.Handled.Should().BeTrue();
            transition.RenderJson.Should().NotBeNull("exactly one render payload is produced");
            transition
                .SelectionChanged.Should()
                .BeFalse("a search refresh must not commit a folder selection");
            transition
                .OpenStateChanged.Should()
                .BeFalse("a search refresh must not close or reopen the selector");
        }

        /// <summary>
        /// AC-3 / AC-8: replacing rows while the selector session is open keeps the session open and
        /// reconciles the committed, original, and pending identities through
        /// <c>ReconcileRowsReplaced</c>, so no native close/reopen cycle is triggered.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_WhileOpen_KeepsTheSessionOpenAndReconciles()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "A", "B" });
            router.SelectRow(0);
            router.OpenSelector().Handled.Should().BeTrue();
            router.GetSelectorState().IsOpen.Should().BeTrue();

            // Act
            BreadcrumbSelectionTransition transition = router.ReplaceItemsPreservingSession(
                new[] { "A", "B", "C" }
            );

            // Assert
            transition.OpenStateChanged.Should().BeFalse();
            BreadcrumbSelectorState state = router.GetSelectorState();
            state.IsOpen.Should().BeTrue("the open session survives the row swap");
            state.Options.Should().HaveCount(3);
            state.CommittedIdentity.Should().Be("plain:0:A");
            state.PendingIdentity.Should().Be("plain:0:A");
        }

        /// <summary>
        /// AC-3: two consecutive refreshes while open produce two independent render payloads and
        /// still leave the session open — the per-keystroke steady state.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_TwoConsecutiveRefreshes_LeaveTheSessionOpen()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "A", "B" });
            router.SelectRow(0);
            router.OpenSelector();

            // Act
            BreadcrumbSelectionTransition first = router.ReplaceItemsPreservingSession(
                new[] { "in", "inv" }
            );
            BreadcrumbSelectionTransition second = router.ReplaceItemsPreservingSession(
                new[] { "invo", "invoi", "invoic" }
            );

            // Assert
            first.OpenStateChanged.Should().BeFalse();
            second.OpenStateChanged.Should().BeFalse();
            first.RenderJson.Should().NotBeNull();
            second.RenderJson.Should().NotBeNull();
            router.GetSelectorState().IsOpen.Should().BeTrue();
            router.GetFolderItems().Should().Equal(new[] { "invo", "invoi", "invoic" });
        }

        /// <summary>
        /// The replacement is atomic and total: the previous row set is gone and the new strings are
        /// carried verbatim, in order.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_ReplacesEveryRowVerbatimAndInOrder()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "old-one", "old-two" });

            // Act
            router.ReplaceItemsPreservingSession(new[] { @"\\A\one", @"\\A\two", @"\\A\three" });

            // Assert
            router.GetFolderItems().Should().Equal(new[] { @"\\A\one", @"\\A\two", @"\\A\three" });
            router
                .Contains("old-one")
                .Should()
                .BeFalse("the previous row set is replaced, not appended");
        }

        /// <summary>
        /// AC-4 / AC-5: a replacement performed while a search highlight is pending does not commit
        /// the highlight; the committed identity stays where it was before the session opened.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_WithPendingHighlight_DoesNotCommitIt()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "A", "B" });
            router.SelectRow(0);
            router.OpenSelector();
            router.HighlightRow(1).Handled.Should().BeTrue();
            router.GetSelectorState().PendingIdentity.Should().Be("plain:1:B");

            // Act
            router.ReplaceItemsPreservingSession(new[] { "A", "B" });

            // Assert
            BreadcrumbSelectorState state = router.GetSelectorState();
            state.CommittedIdentity.Should().Be("plain:0:A");
            router.GetSelectedFolder().Should().Be("A", "the collapsed surface still shows A");
        }

        /// <summary>
        /// AC-9: an empty replacement is a deterministic no-throw operation that empties the row set
        /// without corrupting session state.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_EmptyInput_IsDeterministicAndDoesNotThrow()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "A", "B" });
            router.SelectRow(0);
            router.OpenSelector();

            // Act
            Func<BreadcrumbSelectionTransition> act = () =>
                router.ReplaceItemsPreservingSession(Array.Empty<string>());

            // Assert
            act.Should().NotThrow();
            router.GetFolderItems().Should().BeEmpty();
            router.GetSelectorState().CommittedIdentity.Should().BeNull();
        }

        /// <summary>
        /// AC-9: a banner-only replacement is deterministic and leaves nothing selectable, so a
        /// following highlight is a no-op rather than a throw.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_BannerOnlyInput_LeavesNothingSelectable()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.AddItems(new[] { "A", "B" });
            router.SelectRow(0);
            router.OpenSelector();

            // Act
            router.ReplaceItemsPreservingSession(new[] { BannerText });
            BreadcrumbSelectionTransition highlight = router.HighlightRow(0);

            // Assert
            BreadcrumbSelectorState state = router.GetSelectorState();
            state.Options.Should().HaveCount(1);
            state.Options[0].IsSelectable.Should().BeFalse("a banner row is never selectable");
            highlight.Handled.Should().BeFalse("there is no selectable row to highlight");
            state.CommittedIdentity.Should().BeNull();
        }

        /// <summary>
        /// A null item list is rejected explicitly rather than producing a corrupt row set.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();

            // Act
            Action act = () => router.ReplaceItemsPreservingSession(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("items");
        }

        /// <summary>
        /// AC-3: replacement on a closed session leaves the session closed — opening is the
        /// coordinator composite's separate, explicit step.
        /// </summary>
        [TestMethod]
        public void ReplaceItemsPreservingSession_WhileClosed_LeavesTheSessionClosed()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();

            // Act
            BreadcrumbSelectionTransition transition = router.ReplaceItemsPreservingSession(
                new[] { "A", "B" }
            );

            // Assert
            transition.OpenStateChanged.Should().BeFalse();
            router.GetSelectorState().IsOpen.Should().BeFalse();
        }

        /// <summary>
        /// AC-4: the router-level highlight pass-through reports <c>RenderRequired</c> without
        /// <c>SelectionChanged</c>, and leaves <c>GetSelectedFolder()</c> unchanged.
        /// </summary>
        [TestMethod]
        public void HighlightRow_OnAnOpenSession_MovesPendingWithoutChangingTheSelectedFolder()
        {
            // Arrange
            FolderBreadcrumbBridgeRouter router = CreateRouter();
            router.ReplaceItemsPreservingSession(new[] { "A", "B" });
            router.SelectRow(0);
            router.OpenSelector();

            // Act
            BreadcrumbSelectionTransition transition = router.HighlightRow(1);

            // Assert
            transition.Handled.Should().BeTrue();
            transition.SelectionChanged.Should().BeFalse();
            transition.OpenStateChanged.Should().BeFalse();
            transition.RenderJson.Should().NotBeNull();
            router.GetSelectorState().PendingIdentity.Should().Be("plain:1:B");
            router.GetSelectedFolder().Should().Be("A");
        }

        /// <summary>
        /// Builds a router over a strict, never-called hierarchy provider. The search path is
        /// Path B (plain strings) and performs no hierarchy resolution, so a strict mock with no
        /// setups proves the replacement never reaches the provider.
        /// </summary>
        private static FolderBreadcrumbBridgeRouter CreateRouter() =>
            new FolderBreadcrumbBridgeRouter(
                new Mock<IFolderHierarchyProvider>(MockBehavior.Strict).Object
            );
    }
}
