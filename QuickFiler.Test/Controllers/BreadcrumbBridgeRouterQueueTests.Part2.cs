using System;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Partial continuation of <see cref="BreadcrumbBridgeRouterQueueTests"/>. Holds the #498 and
    /// #499 regression tests. Split from the primary file under decision D8 so that neither part
    /// exceeds the 500-line file-size limit. The <c>[TestClass]</c> attribute and the shared
    /// <c>[TestInitialize]</c> arrange block live in the primary file.
    /// </summary>
    public partial class BreadcrumbBridgeRouterQueueTests
    {
        /// <summary>
        /// #498: a <c>segmentDoubleClick</c> whose <c>segmentIndex</c> sits above the row's segment
        /// range must be rejected without a view-state transition and without a render post. The
        /// payload is driven through the synchronous <c>Inbound</c> helper so that the pre-fix
        /// <see cref="ArgumentOutOfRangeException"/> raised by <c>BreadcrumbRow.CollapseAfter</c>
        /// surfaces on the test thread instead of faulting an async void boundary.
        /// </summary>
        [TestMethod]
        public void SegmentDoubleClick_IndexAboveRange_RejectedWithoutTransition()
        {
            // Arrange: bind the two-segment row-0 produced by the shared Setup arrange block.
            Bind();
            int postedBefore = _posted.Count;

            // Act: an index far above the two-segment range arrives on the direct inbound path.
            Action act = () =>
                Inbound(
                    "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":99}"
                );

            // Assert: the router rejects the index rather than letting the row throw, and no
            // outbound render payload is produced.
            act.Should()
                .NotThrow(
                    "an out-of-range segmentIndex must be rejected by the router, not thrown"
                );
            _posted
                .Count.Should()
                .Be(postedBefore, "a rejected segmentIndex must not produce a render post");
        }

        /// <summary>
        /// #498: a <c>segmentDoubleClick</c> carrying a negative <c>segmentIndex</c> must be
        /// rejected without a view-state transition and without a render post. Driven through the
        /// synchronous <c>Inbound</c> helper for the same reason as the above-range case.
        /// </summary>
        [TestMethod]
        public void SegmentDoubleClick_NegativeIndex_RejectedWithoutTransition()
        {
            // Arrange: bind the two-segment row-0 produced by the shared Setup arrange block.
            Bind();
            int postedBefore = _posted.Count;

            // Act: a negative index arrives on the direct inbound path.
            Action act = () =>
                Inbound(
                    "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":-1}"
                );

            // Assert: the router rejects the index rather than letting the row throw, and no
            // outbound render payload is produced.
            act.Should()
                .NotThrow("a negative segmentIndex must be rejected by the router, not thrown");
            _posted
                .Count.Should()
                .Be(postedBefore, "a rejected segmentIndex must not produce a render post");
        }

        /// <summary>
        /// #498 / AC-1: an above-range <c>segmentIndex</c> arriving through the async void
        /// host-message seam must be contained at that boundary. Nothing escapes
        /// <c>_host.Raise(h =&gt; h.MessageReceived += null, ...)</c> and the outbound posted-message
        /// count is unchanged.
        /// </summary>
        [TestMethod]
        public void SegmentDoubleClick_IndexAboveRange_ViaHostEvent_DoesNotThrowAndLeavesStateUnchanged()
        {
            // Arrange: initialized core so outbound payloads would be observable if any were made.
            _initialized = true;
            Bind();
            int postedBefore = _posted.Count;

            // Act: the out-of-range payload arrives through the async void host-event seam.
            Action act = () =>
                _host.Raise(
                    h => h.MessageReceived += null,
                    _host.Object,
                    "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":99}"
                );

            // Assert: contained at the boundary, with no state change.
            act.Should()
                .NotThrow("the range guard must reject the index before the row can throw");
            _posted.Count.Should().Be(postedBefore);
            _router.SelectedFolderPath.Should().BeNull();
        }

        /// <summary>
        /// #498 / AC-1: a negative <c>segmentIndex</c> arriving through the async void host-message
        /// seam must be contained at that boundary, leaving the posted-message count unchanged.
        /// </summary>
        [TestMethod]
        public void SegmentDoubleClick_NegativeIndex_ViaHostEvent_DoesNotThrowAndLeavesStateUnchanged()
        {
            // Arrange: initialized core so outbound payloads would be observable if any were made.
            _initialized = true;
            Bind();
            int postedBefore = _posted.Count;

            // Act: the negative-index payload arrives through the async void host-event seam.
            Action act = () =>
                _host.Raise(
                    h => h.MessageReceived += null,
                    _host.Object,
                    "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":-1}"
                );

            // Assert: contained at the boundary, with no state change.
            act.Should()
                .NotThrow("the range guard must reject the index before the row can throw");
            _posted.Count.Should().Be(postedBefore);
            _router.SelectedFolderPath.Should().BeNull();
        }

        /// <summary>
        /// #498 / AC-3 valid-index clause: the range guard is a rejection of invalid input only. A
        /// valid non-leaf <c>segmentIndex</c> delivered through the same host-event seam still
        /// collapses the row and posts exactly one row-scoped render payload.
        /// </summary>
        [TestMethod]
        public void SegmentDoubleClick_ValidIndex_ViaHostEvent_CollapsesRowAndPostsRender()
        {
            // Arrange: row-0 carries two segments, so index 0 is the valid non-leaf segment.
            _initialized = true;
            Bind();
            int postedBefore = _posted.Count;

            // Act
            _host.Raise(
                h => h.MessageReceived += null,
                _host.Object,
                "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
            );

            // Assert: one new outbound payload, and it is the row-scoped render for row-0.
            _posted
                .Count.Should()
                .Be(postedBefore + 1, "a valid segmentIndex still produces a render post");
            string render = _posted.Single(p => p.Contains("\"type\":\"render\""));
            render.Should().Contain("\"rowId\":\"row-0\"");
        }

        /// <summary>
        /// #498: a banner row never collapses. <c>BreadcrumbRow.CollapseAfter</c> short-circuits on
        /// <c>Kind != BreadcrumbRowKind.Suggestion</c> and returns false BEFORE its own range check
        /// is reached, so a banner row produces no transition and no post. The banner carries a
        /// single inert segment, so index 0 passes the router's range guard and the short-circuit
        /// inside the row is the behavior actually exercised here.
        /// </summary>
        [TestMethod]
        public void SegmentDoubleClick_BannerRow_ViaHostEvent_ShortCircuitsBeforeRangeCheck()
        {
            // Arrange: bind a banner row ahead of the suggestion row, so the banner is row-0.
            _initialized = true;
            _router
                .BindRowsAsync(
                    new[] { "==== Suggested folders ====", LeafPath },
                    Enumerable.Empty<FolderScore>(),
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();
            int postedBefore = _posted.Count;

            // Act: double-click the banner's only segment.
            Action act = () =>
                _host.Raise(
                    h => h.MessageReceived += null,
                    _host.Object,
                    "{\"type\":\"segmentDoubleClick\",\"rowId\":\"row-0\",\"segmentIndex\":0}"
                );

            // Assert: no throw, and the banner produced no render post.
            act.Should().NotThrow();
            _posted.Count.Should().Be(postedBefore, "banner rows never collapse");
        }

        /// <summary>
        /// #499: a re-bind must not leave the previously selected folder path behind. After a
        /// selection sets <c>SelectedFolderPath</c>, binding a fresh row set clears it alongside
        /// the row-id reset, so a caller reading the property after a re-bind does not act on a
        /// folder the user can no longer see.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_AfterSelection_ClearsSelectedFolderPath()
        {
            // Arrange: bind, then select row-0 so that SelectedFolderPath carries a value.
            Bind();
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");
            _router
                .SelectedFolderPath.Should()
                .Be(LeafPath, "the arrange step must establish a non-null selection");

            // Act: re-bind.
            Bind();

            // Assert: the stale selection is gone.
            _router
                .SelectedFolderPath.Should()
                .BeNull("a re-bind invalidates the previous selection");
        }

        /// <summary>
        /// #499: clearing the stale selection is observable. Subscribers of
        /// <c>SelectedFolderPathChanged</c> receive a null payload when a re-bind discards a
        /// previously non-null selection, so downstream consumers can react to the invalidation.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_AfterSelection_RaisesSelectedFolderPathChangedWithNull()
        {
            // Arrange: establish a non-null selection, then subscribe before the second bind so
            // that only the re-bind's notification can be observed.
            Bind();
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");
            string observed = "sentinel";
            _router.SelectedFolderPathChanged += (s, path) => observed = path;

            // Act: re-bind.
            Bind();

            // Assert: the sentinel was overwritten with the null payload.
            observed
                .Should()
                .BeNull("the re-bind must notify subscribers that the selection was cleared");
        }

        /// <summary>
        /// #499 preservation guard: the re-bind clear must not disturb the first of the two
        /// existing <c>SelectedFolderPath</c> write sites. A direct row selection still assigns the
        /// row's filing target, which is the presented row text for a suggestion row.
        /// </summary>
        [TestMethod]
        public void SelectRow_StillAssignsFilingTargetToSelectedFolderPath()
        {
            // Arrange
            Bind();
            string observed = "sentinel";
            _router.SelectedFolderPathChanged += (s, path) => observed = path;

            // Act: select the suggestion row.
            Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");

            // Assert: the filing target is assigned and published, exactly as before #499.
            _router
                .SelectedFolderPath.Should()
                .Be(LeafPath, "SelectRow assigns row.FilingTarget for a suggestion row");
            observed.Should().Be(LeafPath, "the selection is still published to subscribers");
        }

        /// <summary>
        /// #499 preservation guard: the re-bind clear must not disturb the second of the two
        /// existing <c>SelectedFolderPath</c> write sites. Activating an ancestor segment still
        /// assigns the archive-relative form of that segment's full hierarchy path.
        /// </summary>
        [TestMethod]
        public void SelectHierarchyPath_StillAssignsArchiveRelativePathToSelectedFolderPath()
        {
            // Arrange: an ancestor chain rooted under a non-empty archive root, so that the
            // archive-relative projection is actually exercised rather than short-circuited.
            const string ArchiveRoot = @"\\mailbox\Archive";
            const string ClientsPath = @"\\mailbox\Archive\Clients";
            const string NorthPath = @"\\mailbox\Archive\Clients\North";
            _provider
                .Setup(p =>
                    p.GetAncestorChainAsync(
                        It.IsAny<FolderTreeNodeKey>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new[]
                    {
                        Segment(ClientsPath, "Clients", true),
                        Segment(NorthPath, "North", true),
                    }
                );
            _router
                .BindRowsAsync(
                    new[] { @"Clients\North" },
                    Enumerable.Empty<FolderScore>(),
                    ArchiveRoot,
                    CancellationToken.None
                )
                .GetAwaiter()
                .GetResult();

            // Act: activate the non-leaf ancestor segment.
            Inbound("{\"type\":\"segmentActivate\",\"rowId\":\"row-0\",\"segmentIndex\":0}");

            // Assert: the archive root is stripped, exactly as before #499.
            _router
                .SelectedFolderPath.Should()
                .Be(
                    "Clients",
                    "SelectHierarchyPath assigns the archive-relative form of the activated segment"
                )
                .And.NotBe(ClientsPath);
        }

        /// <summary>
        /// #499 / AC-5: the clear notifies only on an actual change. A re-bind that follows no
        /// selection leaves <c>SelectedFolderPath</c> null and raises no
        /// <c>SelectedFolderPathChanged</c> event at all, so subscribers are not woken by a
        /// no-op.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_WithNoPriorSelection_RaisesNoSelectedFolderPathChangedEvent()
        {
            // Arrange: bind once, make no selection, then subscribe.
            Bind();
            int raised = 0;
            _router.SelectedFolderPathChanged += (s, path) => raised++;

            // Act: re-bind while the selection is still null.
            Bind();

            // Assert: silent, and the property is still null.
            raised
                .Should()
                .Be(0, "the clear must notify only when the previous value was non-null");
            _router.SelectedFolderPath.Should().BeNull();
        }

        /// <summary>
        /// #499 / AC-6: the clear introduces no auto-selection side effect. A bind renders the
        /// document with no row marked selected and leaves <c>SelectedFolderPath</c> null;
        /// <c>SelectFirstRow</c> is not invoked from <c>BindRowsAsync</c>.
        /// </summary>
        [TestMethod]
        public void BindRowsAsync_DoesNotAutoSelectAnyRow()
        {
            // Arrange: an initialized core so the rendered document is delivered and observable.
            _initialized = true;

            // Act
            Bind();

            // Assert: a document was delivered, and it marks no row selected.
            _navigated.Should().HaveCount(1);
            _navigated[0]
                .Should()
                .NotContain(
                    "rowwrap selected",
                    "binding must not select a row on the user's behalf"
                );
            _router.SelectedFolderPath.Should().BeNull();
        }
    }
}
