using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #438: contracts for the non-focusing-open latch on
    /// <see cref="BreadcrumbDropDownOpenCoordinator"/>.
    /// <para>
    /// A third continuation partial of <see cref="BreadcrumbDropDownOpenCoordinatorTests"/>. No
    /// <c>[TestClass]</c> attribute is repeated: it is declared once on the primary partial.
    /// </para>
    /// <para>
    /// The coordinator is host-neutral and the harness supplies a hand-written
    /// <c>ControlledHost</c> plus a capturing synchronization context that is drained explicitly,
    /// so ordering is deterministic and no WinForms control, popup, or message pump exists.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbDropDownOpenCoordinatorTests
    {
        /// <summary>
        /// AC-2: a latched open reaches the host exactly once through the 4-parameter overload with
        /// <c>takeFocus: false</c>.
        /// </summary>
        [TestMethod]
        public void LatchedOpen_ReachesTheHostOnceWithoutFocus()
        {
            // Arrange
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(true));
            harness.Coordinator.LatchNextOpenTakesNoFocus();

            // Act
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(opening);

            // Assert
            harness.Host.Requests.Should().HaveCount(1, "a search refresh opens the popup once");
            harness.Host.RequestedTakeFocus.Should().Equal(new[] { false });
            harness.Host.CloseReasons.Should().BeEmpty("a search refresh never closes the popup");
        }

        /// <summary>
        /// AC-2: the latch is consumed by the open it belongs to, so it does not leak onto a later
        /// gesture open.
        /// </summary>
        [TestMethod]
        public void LatchedOpen_SelfClearsSoTheNextOpenTakesFocus()
        {
            // Arrange
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(true));
            harness.Coordinator.LatchNextOpenTakesNoFocus();
            harness.Coordinator.NextOpenTakesNoFocus.Should().BeTrue();

            Task<bool> searchOpen = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(searchOpen);
            harness.Coordinator.NextOpenTakesNoFocus.Should().BeFalse("the latch is consumed once");

            // Act — a later gesture open on a closed host.
            harness.Host.SetOpen(false);
            harness.Host.Enqueue(Task.FromResult(true));
            Task<bool> gestureOpen = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(gestureOpen);

            // Assert
            harness.Host.RequestedTakeFocus.Should().Equal(new[] { false, true });
        }

        /// <summary>
        /// AC-2 / AC-7 / D7: an unlatched (gesture) open uses the pre-existing 3-parameter overload,
        /// which records <c>takeFocus: true</c>. Routing default opens through the 4-parameter
        /// overload would return a null task from the loose mocks used elsewhere in the suite.
        /// </summary>
        [TestMethod]
        public void UnlatchedOpen_UsesTheDefaultFocusingPath()
        {
            // Arrange
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(true));

            // Act
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(opening);

            // Assert
            harness.Coordinator.NextOpenTakesNoFocus.Should().BeFalse();
            harness.Host.RequestedTakeFocus.Should().Equal(new[] { true });
        }

        /// <summary>
        /// AC-2: latching twice before a single open still yields exactly one non-focusing open —
        /// the latch is a flag, not a counter, so repeated keystrokes cannot queue focus intents.
        /// </summary>
        [TestMethod]
        public void LatchingTwiceBeforeOneOpen_ProducesOneNonFocusingOpen()
        {
            // Arrange
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(true));
            harness.Coordinator.LatchNextOpenTakesNoFocus();
            harness.Coordinator.LatchNextOpenTakesNoFocus();

            // Act
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(opening);

            // Assert
            harness.Host.Requests.Should().HaveCount(1);
            harness.Host.RequestedTakeFocus.Should().Equal(new[] { false });
            harness.Coordinator.NextOpenTakesNoFocus.Should().BeFalse();
        }

        /// <summary>
        /// AC-2: latch-then-gesture ordering is FIFO-deterministic on the shared posted-operation
        /// queue — the latched search open is served first and without focus, and the gesture open
        /// that follows takes focus.
        /// </summary>
        [TestMethod]
        public void LatchThenGestureOpen_AreServedFifoWithTheirOwnFocusIntent()
        {
            // Arrange
            var harness = new CoordinatorHarness { SelectorOpen = true };
            harness.Host.Enqueue(Task.FromResult(true));

            // Act — search open first.
            harness.Coordinator.LatchNextOpenTakesNoFocus();
            Task<bool> searchOpen = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(searchOpen);

            // Act — gesture open after the host reports closed again.
            harness.Host.SetOpen(false);
            harness.Host.Enqueue(Task.FromResult(true));
            Task<bool> gestureOpen = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(gestureOpen);

            // Assert
            harness.Host.Requests.Should().HaveCount(2);
            harness
                .Host.RequestedTakeFocus.Should()
                .Equal(
                    new[] { false, true },
                    "each open carries the focus intent latched for it, in request order"
                );
        }

        /// <summary>
        /// A latch requested after the coordinator is released is ignored rather than throwing, and
        /// no open is issued.
        /// </summary>
        [TestMethod]
        public void LatchAfterRelease_IsIgnoredAndIssuesNoOpen()
        {
            // Arrange
            var harness = new CoordinatorHarness();
            harness.Coordinator.Release();
            harness.Context.DrainAll();

            // Act
            harness.Coordinator.LatchNextOpenTakesNoFocus();
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainAll();

            // Assert
            harness.Coordinator.NextOpenTakesNoFocus.Should().BeFalse();
            opening.IsCompleted.Should().BeTrue();
            opening.Result.Should().BeFalse();
            harness.Host.Requests.Should().BeEmpty();
        }

        /// <summary>
        /// Issue #656: after a close that returned true, with the host open again by a path that
        /// reaches neither RequestOpen nor Invalidate, a further close must reach the host rather
        /// than being suppressed by the completed-close flag. Deterministic: one thread, explicit
        /// drain, no timers, no sleeps, no second thread, no temp files.
        /// </summary>
        [TestMethod]
        public void CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain()
        {
            // Arrange: open the drop-down, then drive a close that the host accepts.
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(true));
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(opening);
            opening.Result.Should().BeTrue();

            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();
            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.Uncommitted);
            harness.Host.IsOpen.Should().BeFalse("the host accepted the close");

            // Act: the host becomes open again by a path that bypasses RequestOpen and Invalidate.
            harness.Host.SetOpen(true);
            harness.SelectorOpen = true;
            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();

            // Assert
            harness
                .Host.CloseReasons.Should()
                .Equal(
                    new[]
                    {
                        BreadcrumbDropDownCloseReason.Uncommitted,
                        BreadcrumbDropDownCloseReason.Uncommitted,
                    },
                    "the close after a bypassing reopen must reach _host.Close a second time"
                );
        }
    }
}
