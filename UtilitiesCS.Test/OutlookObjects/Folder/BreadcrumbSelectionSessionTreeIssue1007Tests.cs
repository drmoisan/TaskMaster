#nullable enable
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #1007 AC1, AC2, AC4, AC8, AC11: the open QFC list walks the visible order and moves
    /// only the pending highlight, which can rest on a descendant node. The model holds
    /// <c>Archive &gt; Clients &gt; Beta</c> (expanded: Gamma, expanded with Epsilon, then Delta)
    /// and <c>Archive &gt; Projects &gt; Alpha</c>, so the visible order is Beta, Gamma, Epsilon,
    /// Delta, Alpha.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbSelectionSessionTreeIssue1007Tests
    {
        /// <summary>AC1: Down in the open list pends every visible descendant in pre-order.</summary>
        [TestMethod]
        public void SelectorKeyDown_Open_MovesPendingThroughVisibleDescendants()
        {
            // Arrange
            var (model, session) = OpenOn(0);
            var root = model.Rows[0].DescendantRoot!;
            var gamma = root.Children[0];

            // Act / Assert
            session.MoveNext().Should().BeTrue();
            session.PendingNode.Should().BeSameAs(gamma);
            session.MoveNext().Should().BeTrue();
            session.PendingNode.Should().BeSameAs(gamma.Children[0]);
            session.MoveNext().Should().BeTrue();
            session.PendingNode.Should().BeSameAs(root.Children[1]);
            session.MoveNext().Should().BeTrue();
            session.PendingNode.Should().BeNull();
            session.PendingIdentity.Should().Be(model.Rows[1].Identity);
        }

        /// <summary>AC1: Up from the row below an expanded block pends its last visible descendant.</summary>
        [TestMethod]
        public void SelectorKeyUp_Open_FromRowBelowExpandedBlock_PendsLastVisibleDescendant()
        {
            // Arrange
            var (model, session) = OpenOn(1);

            // Act
            session.MovePrevious().Should().BeTrue();

            // Assert
            session.PendingIdentity.Should().Be(model.Rows[0].Identity);
            session.PendingNode.Should().BeSameAs(model.Rows[0].DescendantRoot!.Children[1]);
        }

        /// <summary>AC4: Enter on a pending node commits that descendant.</summary>
        [TestMethod]
        public void CommitPending_WithPendingNode_SelectsDescendant()
        {
            // Arrange
            var (model, session) = OpenOn(1);
            session.MovePrevious();

            // Act
            session.CommitPending().Should().BeTrue();

            // Assert
            model.SelectedIndex.Should().Be(0);
            model.SelectedDescendant.Should().BeSameAs(model.Rows[0].DescendantRoot!.Children[1]);
            session.IsOpen.Should().BeFalse();
            session.PendingNode.Should().BeNull();
        }

        /// <summary>AC4: Escape restores the original choice and clears the pending node.</summary>
        [TestMethod]
        public void Cancel_RestoresOriginalAndClearsPendingNode()
        {
            // Arrange
            var (model, session) = OpenOn(1);
            session.MovePrevious();
            session.PendingNode.Should().NotBeNull("the pending highlight rests on Delta");

            // Act
            session.Cancel().Should().BeTrue();

            // Assert
            session.PendingNode.Should().BeNull();
            session.CommittedIdentity.Should().Be(model.Rows[1].Identity);
            model.SelectedIndex.Should().Be(1);
        }

        /// <summary>AC11: Home and End pend the first and last visible entry.</summary>
        [TestMethod]
        public void MoveToBoundary_Open_PendsFirstAndLastVisibleEntry()
        {
            // Arrange
            var (model, session) = OpenOn(0);
            session.MoveNext();

            // Act / Assert
            session.MoveToBoundary(first: false).Should().BeTrue();
            session.PendingIdentity.Should().Be(model.Rows[1].Identity);
            session.MoveToBoundary(first: true).Should().BeTrue();
            session.PendingIdentity.Should().Be(model.Rows[0].Identity);
            session.PendingNode.Should().BeNull();
        }

        /// <summary>AC11: PageDown and PageUp move by the page size and clamp at the ends.</summary>
        [TestMethod]
        public void MovePage_Open_MovesByPageSizeAndClamps()
        {
            // Arrange
            var (model, session) = OpenOn(0);
            var gamma = model.Rows[0].DescendantRoot!.Children[0];

            // Act / Assert
            session.MovePage(previous: false, pageSize: 2).Should().BeTrue();
            session.PendingNode.Should().BeSameAs(gamma.Children[0]);
            session.MovePage(previous: false, pageSize: 10).Should().BeTrue();
            session.PendingIdentity.Should().Be(model.Rows[1].Identity);
            session.MovePage(previous: true, pageSize: 3).Should().BeTrue();
            session.PendingNode.Should().BeSameAs(gamma);
        }

        /// <summary>S3: a zero or negative page size is a no-op.</summary>
        [TestMethod]
        public void MovePage_InvalidPageSize_IsNoOp()
        {
            // Arrange
            var (model, session) = OpenOn(0);

            // Act / Assert
            session.MovePage(previous: false, pageSize: 0).Should().BeFalse();
            session.MovePage(previous: true, pageSize: -1).Should().BeFalse();
            session.PendingIdentity.Should().Be(model.Rows[0].Identity);
            session.PendingNode.Should().BeNull();
        }

        /// <summary>AC8: Ctrl+Left on a pending node pends its parent and keeps it expanded.</summary>
        [TestMethod]
        public void MoveToParent_Open_OnNode_PendsParentKeepingExpansion()
        {
            // Arrange: pend Epsilon.
            var (model, session) = OpenOn(0);
            session.MoveNext();
            session.MoveNext();
            var gamma = model.Rows[0].DescendantRoot!.Children[0];

            // Act
            session.MoveToParent().Should().BeTrue();

            // Assert
            session.PendingNode.Should().BeSameAs(gamma);
            gamma.Expanded.Should().BeTrue();
        }

        /// <summary>AC8: Ctrl+Left on a pending row shortens it at the parent segment, pending only.</summary>
        [TestMethod]
        public void MoveToParent_Open_OnRow_ShortensPendingRowAtParentSegment()
        {
            // Arrange
            var (model, session) = OpenOn(1);

            // Act
            session.MoveToParent().Should().BeTrue();

            // Assert
            model.Rows[1].ActiveSegmentIndex.Should().Be(1);
            model.Rows[1].CollapsedAfterIndex.Should().Be(1);
            session.IsOpen.Should().BeTrue();
            session.CommittedIdentity.Should().Be(model.Rows[1].Identity);
        }

        /// <summary>AC8: Ctrl+Left stops at the first segment of the chain.</summary>
        [TestMethod]
        public void MoveToParent_AtFirstSegment_IsNoOp()
        {
            // Arrange
            var (model, session) = OpenOn(1);
            session.MoveToParent().Should().BeTrue();
            session.MoveToParent().Should().BeTrue();

            // Act / Assert
            session.MoveToParent().Should().BeFalse();
            model.Rows[1].ActiveSegmentIndex.Should().Be(0);
        }

        /// <summary>AC8 (D-Q24): Escape after Ctrl+Left restores the row shape and the committed value.</summary>
        [TestMethod]
        public void SelectorKeyParent_Open_ThenEscape_RestoresRowShapeAndValue()
        {
            // Arrange
            var (model, session) = OpenOn(1);
            string? before = BreadcrumbSelectionMap.GetSelectedFolder(model);
            session.MoveToParent().Should().BeTrue();

            // Act
            session.Cancel().Should().BeTrue();

            // Assert
            model.Rows[1].ActiveSegmentIndex.Should().Be(2);
            model.Rows[1].CollapsedAfterIndex.Should().BeNull();
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().Be(before);
        }

        /// <summary>Guard (AC2): Down on the last visible entry keeps the pending highlight.</summary>
        [TestMethod]
        public void SelectorKeyDown_Open_OnLastEntry_KeepsPending()
        {
            // Arrange
            var (model, session) = OpenOn(1);

            // Act / Assert
            session.MoveNext().Should().BeFalse();
            session.PendingIdentity.Should().Be(model.Rows[1].Identity);
        }

        /// <summary>Guard (AC4): open moves never change the committed value.</summary>
        [TestMethod]
        public void OpenMove_DoesNotChangeCommittedValue()
        {
            // Arrange
            var (model, session) = OpenOn(0);
            string? before = BreadcrumbSelectionMap.GetSelectedFolder(model);

            // Act
            session.MoveNext();

            // Assert
            session.CommittedIdentity.Should().Be(model.Rows[0].Identity);
            model.SelectedIndex.Should().Be(0);
            BreadcrumbSelectionMap.GetSelectedFolder(model).Should().Be(before);
        }

        /// <summary>AC10, AC11: open-only moves are no-ops when closed; boundary commits need a closed session.</summary>
        [TestMethod]
        public void BoundaryAndPageMoves_RespectSessionState()
        {
            // Arrange
            var (model, session) = Build(0);

            // Act / Assert: closed.
            session.MoveToBoundary(first: false).Should().BeFalse();
            session.MovePage(previous: false, pageSize: 2).Should().BeFalse();
            session.CommitBoundaryRow(first: true).Should().BeFalse("row 0 is already committed");
            session.CommitBoundaryRow(first: false).Should().BeTrue();
            model.SelectedIndex.Should().Be(1);

            // Act / Assert: open.
            session.Open().Should().BeTrue();
            session
                .CommitBoundaryRow(first: true)
                .Should()
                .BeFalse("an open list only moves pending");
            session.MoveToBoundary(first: false).Should().BeFalse("already on the last entry");
        }

        /// <summary>AC8 (D-Q19): closed Ctrl+Left walks node to parent node, to row, to parent segments.</summary>
        [TestMethod]
        public void MoveToParent_Closed_CommitsEachParentUntilFirstSegment()
        {
            // Arrange: Epsilon (under Gamma) is committed.
            var (model, session) = Build(0);
            var gamma = model.Rows[0].DescendantRoot!.Children[0];
            model.SelectDescendant(0, gamma.Children[0]);

            // Act / Assert
            session.MoveToParent().Should().BeTrue();
            model.SelectedDescendant.Should().BeSameAs(gamma);
            session.MoveToParent().Should().BeTrue();
            model.SelectedDescendant.Should().BeNull("depth 0 commits the row entry");
            session.MoveToParent().Should().BeTrue();
            session.MoveToParent().Should().BeTrue();
            model.Rows[0].ActiveSegmentIndex.Should().Be(0);
            model.Rows[0].CollapsedAfterIndex.Should().Be(0);
            session.MoveToParent().Should().BeFalse("the first segment has no parent");
        }

        /// <summary>I1: a closed Ctrl+Left whose committed node is not in the selected row changes nothing.</summary>
        [TestMethod]
        public void MoveToParent_Closed_ForeignCommittedNode_IsNoOp()
        {
            // Arrange
            var (model, session) = Build(0);
            var gamma = model.Rows[0].DescendantRoot!.Children[0];
            model.SelectDescendant(1, gamma);

            // Act / Assert
            session.MoveToParent().Should().BeFalse();
            model.SelectedDescendant.Should().BeSameAs(gamma);
        }

        /// <summary>AC8 (D-Q24): Escape restores a row that was already shortened when the list opened.</summary>
        [TestMethod]
        public void Cancel_AfterParentOnShortenedRow_RestoresShortenedShape()
        {
            // Arrange: Alpha's row is shortened at Projects before opening.
            var (model, session) = Build(1);
            model.Rows[1].ActivateSegment(1).Should().BeTrue();
            model.Rows[1].CollapseAfter(1);
            session.Open().Should().BeTrue();
            session.MoveToParent().Should().BeTrue();

            // Act
            session.Cancel().Should().BeTrue();

            // Assert
            model.Rows[1].ActiveSegmentIndex.Should().Be(1);
            model.Rows[1].CollapsedAfterIndex.Should().Be(1);
        }

        /// <summary>AC4 (D-Q29): opening on a visible committed node pends it; Enter keeps it committed.</summary>
        [TestMethod]
        public void Open_WithVisibleCommittedNode_PendsItAndCommitKeepsIt()
        {
            // Arrange
            var (model, session) = Build(0);
            var delta = model.Rows[0].DescendantRoot!.Children[1];
            model.SelectDescendant(0, delta);

            // Act
            session.Open().Should().BeTrue();
            var pending = session.PendingNode;
            bool changed = session.CommitPending();

            // Assert
            pending.Should().BeSameAs(delta);
            changed.Should().BeFalse("the pending node is already the committed one");
            model.SelectedDescendant.Should().BeSameAs(delta);
        }

        /// <summary>AC7: reconciling a still-visible pending node keeps it; a detached one is not committed.</summary>
        [TestMethod]
        public void PendingNode_VisibleIsKept_DetachedIsNotCommitted()
        {
            // Arrange: pend Delta, then collapse Gamma, which does not hide Delta.
            var (model, session) = OpenOn(0);
            var root = model.Rows[0].DescendantRoot!;
            var delta = root.Children[1];
            session.MoveToBoundary(first: false);
            session.MovePrevious();
            root.Children[0].Collapse();

            // Act
            session.ReconcilePendingNode();
            var kept = session.PendingNode;
            model
                .Rows[0]
                .SetSubfolders(new[] { Segment(@"\Archive\Clients\Beta\Zeta", "Zeta", false) });
            session.CommitPending();

            // Assert
            kept.Should().BeSameAs(delta);
            model
                .SelectedDescendant.Should()
                .BeNull("a node detached by a refetch is not committed");
            model.SelectedIndex.Should().Be(0);
        }

        private static (BreadcrumbStateModel, BreadcrumbSelectionSession) OpenOn(int row)
        {
            var (model, session) = Build(row);
            session.Open().Should().BeTrue();
            return (model, session);
        }

        private static (BreadcrumbStateModel, BreadcrumbSelectionSession) Build(int row)
        {
            var model = new BreadcrumbStateModel();
            model.AddSuggestionRow(
                new[]
                {
                    Segment(@"\Archive", "Archive", true),
                    Segment(@"\Archive\Clients", "Clients", true),
                    Segment(@"\Archive\Clients\Beta", "Beta", true),
                },
                0.6
            );
            model.AddSuggestionRow(
                new[]
                {
                    Segment(@"\Archive", "Archive", true),
                    Segment(@"\Archive\Projects", "Projects", true),
                    Segment(@"\Archive\Projects\Alpha", "Alpha", false),
                },
                0.3
            );
            var beta = model.Rows[0];
            beta.TryExpandLeaf().Should().BeTrue();
            beta.SetSubfolders(
                new[]
                {
                    Segment(@"\Archive\Clients\Beta\Gamma", "Gamma", true),
                    Segment(@"\Archive\Clients\Beta\Delta", "Delta", false),
                }
            );
            var gamma = beta.DescendantRoot!.Children[0];
            gamma.Expanded = true;
            gamma.Attach(
                new[] { Segment(@"\Archive\Clients\Beta\Gamma\Epsilon", "Epsilon", false) }
            );
            model.SelectRow(row);
            return (model, new BreadcrumbSelectionSession(model));
        }

        private static FolderBreadcrumbSegment Segment(
            string path,
            string name,
            bool hasChildren
        ) =>
            new FolderBreadcrumbSegment(
                new FolderTreeNodeKey("store", path, path),
                name,
                path,
                hasChildren
            );
    }
}
