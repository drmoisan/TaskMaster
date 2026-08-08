#nullable enable
using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #438: contracts for the pending-only <c>HighlightRow</c> transition used by the
    /// folder-search path. The session is host-neutral — no WinForms control, WebView2 surface, COM
    /// object, or message pump is involved — so every case here is deterministic and headless.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbSelectionSessionHighlightTests
    {
        /// <summary>
        /// AC-4: on an open session the highlight changes only <c>PendingIdentity</c>. The committed
        /// identity, the model's selected row (which backs the collapsed surface and
        /// <c>GetSelectedFolder()</c>), and the opening snapshot are all untouched.
        /// </summary>
        [TestMethod]
        public void HighlightRow_OpenSession_ChangesOnlyPendingIdentity()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();

            // Act
            BreadcrumbSelectionEffects effects = session.HighlightRow(1);

            // Assert
            effects
                .Should()
                .Be(
                    BreadcrumbSelectionEffects.Handled | BreadcrumbSelectionEffects.RenderRequired,
                    "a search highlight must be handled and re-rendered, and nothing else"
                );
            session.PendingIdentity.Should().Be("folder-b");
            session.CommittedIdentity.Should().Be("folder-a");
            session.OriginalIdentity.Should().Be("folder-a");
            model.SelectedIndex.Should().Be(0, "the committed model selection must not move");
            session.IsOpen.Should().BeTrue();
        }

        /// <summary>
        /// AC-4: the transition must publish no <c>SelectionChanged</c> and no
        /// <c>OpenStateChanged</c>, because either would commit or churn the session.
        /// </summary>
        [TestMethod]
        public void HighlightRow_OpenSession_PublishesNoSelectionOrOpenStateChange()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();

            // Act
            BreadcrumbSelectionEffects effects = session.HighlightRow(1);

            // Assert
            effects
                .HasFlag(BreadcrumbSelectionEffects.SelectionChanged)
                .Should()
                .BeFalse("a search highlight must never commit a folder selection");
            effects
                .HasFlag(BreadcrumbSelectionEffects.OpenStateChanged)
                .Should()
                .BeFalse("a search highlight must never open or close the selector");
        }

        /// <summary>
        /// AC-4: the index is inclusive and skips non-selectable rows, so a banner at the requested
        /// index resolves forward to the next selectable row rather than failing.
        /// </summary>
        [TestMethod]
        public void HighlightRow_IndexOnANonSelectableRow_ResolvesToTheNextSelectableRow()
        {
            // Arrange — row 1 is a non-selectable banner between two selectable folders.
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();

            // Act
            session.HighlightRow(1);

            // Assert
            session.PendingIdentity.Should().Be("folder-b");
        }

        /// <summary>
        /// AC-4: index zero resolves to the first selectable row, which is the row the search path
        /// highlights after every refresh.
        /// </summary>
        [TestMethod]
        public void HighlightRow_IndexZero_ResolvesToTheFirstSelectableRow()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(2);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();

            // Act
            session.HighlightRow(0);

            // Assert
            session.PendingIdentity.Should().Be("folder-a");
            session.CommittedIdentity.Should().Be("folder-b");
            model.SelectedIndex.Should().Be(2);
        }

        /// <summary>
        /// AC-5 (session half): Escape after a search highlight restores the identity committed
        /// before the search session opened, and the pending highlight is discarded.
        /// </summary>
        [TestMethod]
        public void Cancel_AfterHighlight_RestoresThePreSearchCommittedIdentity()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();
            session.HighlightRow(1);
            session.PendingIdentity.Should().Be("folder-b");

            // Act
            BreadcrumbSelectionEffects effects = session.CancelSelector();

            // Assert
            effects.HasFlag(BreadcrumbSelectionEffects.Handled).Should().BeTrue();
            effects
                .HasFlag(BreadcrumbSelectionEffects.SelectionChanged)
                .Should()
                .BeFalse("an uncommitted close reports no selection change");
            session.CommittedIdentity.Should().Be("folder-a");
            session.PendingIdentity.Should().BeNull();
            session.IsOpen.Should().BeFalse();
            model.SelectedIndex.Should().Be(0);
        }

        /// <summary>
        /// AC-9: a closed session is a deterministic no-op — the highlight requires an open session,
        /// which is what makes it incapable of committing.
        /// </summary>
        [TestMethod]
        public void HighlightRow_ClosedSession_IsADeterministicNoOp()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);

            // Act
            BreadcrumbSelectionEffects effects = session.HighlightRow(0);

            // Assert
            effects.Should().Be(BreadcrumbSelectionEffects.None);
            session.PendingIdentity.Should().BeNull();
            session.CommittedIdentity.Should().Be("folder-a");
            model.SelectedIndex.Should().Be(0);
        }

        /// <summary>
        /// AC-9: an empty row set is a deterministic no-op and must not throw. The session cannot
        /// even open with no selectable rows, so the highlight is unreachable.
        /// </summary>
        [TestMethod]
        public void HighlightRow_EmptyRowSet_IsANoOpAndDoesNotThrow()
        {
            // Arrange
            var model = new BreadcrumbStateModel();
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeFalse("an empty row set has no selectable row to open on");

            // Act
            Func<BreadcrumbSelectionEffects> act = () => session.HighlightRow(0);

            // Assert
            act.Should().NotThrow();
            act().Should().Be(BreadcrumbSelectionEffects.None);
            session.PendingIdentity.Should().BeNull();
        }

        /// <summary>
        /// AC-9: a banner-only row set has no selectable row, so the highlight is a no-op that does
        /// not throw even when the session state is forced open-like by an earlier populated set.
        /// </summary>
        [TestMethod]
        public void HighlightRow_BannerOnlyRowSet_IsANoOpAndDoesNotThrow()
        {
            // Arrange — open on a populated set, then replace it with a banner-only set.
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();
            model.ReplaceRows(new[] { CreateRow("banner", BannerText, false) });
            session.ReconcileRowsReplaced();

            // Act
            Func<BreadcrumbSelectionEffects> act = () => session.HighlightRow(0);

            // Assert
            act.Should().NotThrow();
            act().Should().Be(BreadcrumbSelectionEffects.None);
            session.PendingIdentity.Should().BeNull("a banner-only set has nothing to highlight");
        }

        /// <summary>
        /// AC-9: an index beyond the last row finds no target and is a no-op that leaves the prior
        /// pending highlight in place.
        /// </summary>
        [TestMethod]
        public void HighlightRow_IndexBeyondTheLastRow_IsANoOp()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();
            session.HighlightRow(1);

            // Act
            BreadcrumbSelectionEffects effects = session.HighlightRow(99);

            // Assert
            effects.Should().Be(BreadcrumbSelectionEffects.None);
            session.PendingIdentity.Should().Be("folder-b", "the prior highlight is retained");
        }

        /// <summary>
        /// Boundary: a negative index clamps to the first selectable row instead of throwing.
        /// </summary>
        [TestMethod]
        public void HighlightRow_NegativeIndex_ClampsToTheFirstSelectableRow()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(2);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();

            // Act
            BreadcrumbSelectionEffects effects = session.HighlightRow(-5);

            // Assert
            effects
                .Should()
                .Be(BreadcrumbSelectionEffects.Handled | BreadcrumbSelectionEffects.RenderRequired);
            session.PendingIdentity.Should().Be("folder-a");
        }

        /// <summary>
        /// State transition: repeated highlights compose, each replacing the previous pending
        /// identity without ever committing — the per-keystroke search behavior.
        /// </summary>
        [TestMethod]
        public void HighlightRow_RepeatedHighlights_NeverCommitAndOnlyMovePending()
        {
            // Arrange
            BreadcrumbStateModel model = CreateModel();
            model.SelectRow(0);
            var session = new BreadcrumbSelectionSession(model);
            session.Open().Should().BeTrue();

            // Act
            session.HighlightRow(2);
            session.HighlightRow(0);
            session.HighlightRow(2);

            // Assert
            session.PendingIdentity.Should().Be("folder-b");
            session.CommittedIdentity.Should().Be("folder-a");
            model.SelectedIndex.Should().Be(0);
            session.IsOpen.Should().BeTrue();
        }

        private const string BannerText = "-- Suggested folders --";

        /// <summary>
        /// Builds a three-row model: selectable "folder-a", a non-selectable banner, selectable
        /// "folder-b" — the same shape used by the existing session suite.
        /// </summary>
        private static BreadcrumbStateModel CreateModel()
        {
            var model = new BreadcrumbStateModel();
            model.AddPlainRow("folder-a", "\\Inbox\\A", true);
            model.AddPlainRow("label", "Suggested folders", false);
            model.AddPlainRow("folder-b", "\\Inbox\\B", true);
            return model;
        }

        private static BreadcrumbStateRow CreateRow(
            string identity,
            string text,
            bool isSelectable
        ) => new BreadcrumbStateRow(identity, text, isSelectable);
    }
}
