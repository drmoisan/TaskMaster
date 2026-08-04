using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;
using CapturingSynchronizationContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Continuation partial of <see cref="BreadcrumbDropDownOpenCoordinatorTests"/>; the shared
    /// harness/host helpers and remaining cases live in the sibling primary partial so each file
    /// stays under the 500-line limit. Deterministic; no Outlook, WebView2, timers, or temp files.
    /// </summary>
    public sealed partial class BreadcrumbDropDownOpenCoordinatorTests
    {
        [TestMethod]
        public void Reset_HostAlreadyClosedWithOpenSelector_CancelsExactlyOnce()
        {
            var harness = new CoordinatorHarness { SelectorOpen = true };

            harness.Coordinator.Reset();
            harness.Context.DrainAll();

            harness.CancelCount.Should().Be(1);
            harness.Host.CloseReasons.Should().BeEmpty();
            harness.Host.ResetCount.Should().Be(1);
        }

        [TestMethod]
        public void SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry()
        {
            var failure = new InvalidOperationException("close failed");
            var harness = new CoordinatorHarness();
            harness.Host.SetOpen(true);
            harness.Host.CloseFailure = failure;

            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();

            harness.Host.CloseFailure = null;
            harness.SelectorOpen = true;
            harness.Host.SetOpen(true);
            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();

            harness
                .Host.CloseReasons.Should()
                .Equal(
                    BreadcrumbDropDownCloseReason.Uncommitted,
                    BreadcrumbDropDownCloseReason.Uncommitted
                );
            harness.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            harness.CancelCount.Should().Be(0);
        }

        [TestMethod]
        public void RequestOpen_HostSideCancellationBeforeFalseCompletionIsNotDuplicated()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(pending.Task);
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainOne().Should().BeTrue();

            harness.SelectorOpen = false;
            pending.SetResult(false);
            harness.Context.DrainUntil(opening);

            opening.Result.Should().BeFalse();
            harness.CancelCount.Should().Be(0);
        }

        [TestMethod]
        public void RequestOpen_SelectorClosesBeforeSuccess_ClosesLatePopupExplicitly()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(pending.Task);
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainOne().Should().BeTrue();

            harness.SelectorOpen = false;
            pending.SetResult(true);
            harness.Context.DrainUntil(opening);

            opening.Result.Should().BeFalse();
            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit);
            harness.CancelCount.Should().Be(0);
        }

        [TestMethod]
        public void SetDroppedDown_MouseAndKeyboardPathsShareRequestAndCloseUncommitted()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness { SelectorOpen = false };
            harness.Host.Enqueue(pending.Task);

            harness.Coordinator.SetDroppedDown(true);
            harness.Context.DrainOne().Should().BeTrue();
            Task<bool> mouseRequest = harness.Coordinator.CurrentOpenTask;
            harness.Coordinator.SetDroppedDown(true);
            harness.Context.DrainAll();
            Task<bool> keyboardRequest = harness.Coordinator.CurrentOpenTask;

            keyboardRequest.Should().BeSameAs(mouseRequest);
            harness.Host.Requests.Should().ContainSingle();
            pending.SetResult(true);
            harness.Context.DrainUntil(mouseRequest);

            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();
            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.Uncommitted);
            harness.CancelCount.Should().Be(0);
            harness.OpenSelectorCount.Should().Be(2);
        }

        [TestMethod]
        public void SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired()
        {
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(true));

            harness.Coordinator.HandleSelectorOpenStateChanged();
            harness.Context.DrainAll();
            Task<bool> opening = harness.Coordinator.CurrentOpenTask;
            harness.Context.DrainUntil(opening);
            opening.Result.Should().BeTrue();

            harness.SelectorOpen = false;
            harness.Coordinator.HandleSelectorOpenStateChanged();
            harness.Context.DrainAll();
            harness.Coordinator.HandleSelectorOpenStateChanged();
            harness.Context.DrainAll();

            harness.Host.Requests.Should().ContainSingle();
            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit);
        }

        [TestMethod]
        public void ResetReleaseAndCloseResults_PreserveRetryAndBlockReleasedWork()
        {
            var harness = new CoordinatorHarness();
            harness.Host.SetOpen(true);
            harness.Host.CloseResult = false;
            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();
            harness.CancelCount.Should().Be(1);

            harness.SelectorOpen = true;
            harness.Host.SetOpen(true);
            harness.Host.CloseResult = true;
            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();
            harness.CancelCount.Should().Be(1, "a successful host close owns cancellation");

            harness.SelectorOpen = true;
            harness.Host.SetOpen(true);
            harness.Coordinator.Reset();
            harness.Context.DrainAll();
            harness.DetachCount.Should().Be(1);
            harness.Host.ResetCount.Should().Be(1);
            harness.Host.Enqueue(Task.FromResult(true));
            Task<bool> retry = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(retry);
            retry.Result.Should().BeTrue();

            harness.Coordinator.Release();
            harness.Context.DrainAll();
            harness.Coordinator.Release();
            harness.Context.DrainAll();
            harness.DetachCount.Should().Be(2);
            harness.Host.DisposeCount.Should().Be(1);
            harness.Coordinator.RequestOpen().Result.Should().BeFalse();
            new Action(() =>
                harness.Coordinator.UpdateRequestProviders(
                    () => CoordinatorHarness.Anchor,
                    () => CoordinatorHarness.WorkingArea
                )
            )
                .Should()
                .Throw<ObjectDisposedException>();
        }

        /// <summary>
        /// Coordinator line 99: after <c>Release()</c> the released guard must reject both drop-down
        /// transitions before any operation is posted, so no selector work and no host state change occurs.
        /// </summary>
        [TestMethod]
        public void SetDroppedDown_AfterRelease_PostsNothingAndLeavesHostStateUntouched()
        {
            var probe = new CountingCoordinatorProbe();
            probe.Host.SetOpen(true);
            probe.Coordinator.Release();
            probe.Context.DrainAll();
            int postsAfterRelease = probe.Context.PostCount;
            int detachAfterRelease = probe.DetachCalls;
            int disposalsAfterRelease = probe.Host.DisposeCount;
            bool openStateAfterRelease = probe.Host.IsOpen;

            probe.Coordinator.SetDroppedDown(true);
            probe.Coordinator.SetDroppedDown(false);

            probe
                .Context.PostCount.Should()
                .Be(postsAfterRelease, "the released guard returns first");
            probe.Context.PendingCount.Should().Be(0);
            probe.OpenSelectorCalls.Should().Be(0);
            probe.SelectorOpenReads.Should().Be(0);
            probe.CancelCalls.Should().Be(0);
            probe.DetachCalls.Should().Be(detachAfterRelease);
            probe.Host.IsOpen.Should().Be(openStateAfterRelease);
            probe.Host.DisposeCount.Should().Be(disposalsAfterRelease);
            probe.Host.CloseReasons.Should().BeEmpty();
        }

        /// <summary>
        /// Coordinator line 118: after <c>Release()</c> a selector-state notification must return before
        /// posting, so the selector-open predicate is never consulted and the host is never closed.
        /// </summary>
        [TestMethod]
        public void HandleSelectorOpenStateChanged_AfterRelease_PostsNothingAndSkipsSelectorPredicate()
        {
            var probe = new CountingCoordinatorProbe();
            probe.Host.SetOpen(true);
            probe.Coordinator.Release();
            probe.Context.DrainAll();
            int postsAfterRelease = probe.Context.PostCount;

            probe.Coordinator.HandleSelectorOpenStateChanged();

            probe
                .Context.PostCount.Should()
                .Be(postsAfterRelease, "the released guard returns first");
            probe.Context.PendingCount.Should().Be(0);
            probe.SelectorOpenReads.Should().Be(0);
            probe.Host.CloseReasons.Should().BeEmpty();
            probe.Host.Requests.Should().BeEmpty();
            probe.Host.ResetCount.Should().Be(0);
        }

        /// <summary>
        /// Coordinator line 122: a selector notification queued before <c>Release()</c> must observe the
        /// released generation when it finally drains and leave the host in its exact pre-drain state.
        /// </summary>
        [TestMethod]
        public void HandleSelectorOpenStateChanged_QueuedBodyDrainedAfterRelease_PerformsNoWork()
        {
            var probe = new CountingCoordinatorProbe();
            probe.Host.SetOpen(true);
            probe.Coordinator.HandleSelectorOpenStateChanged();
            probe.Coordinator.Release();
            probe
                .Context.PendingCount.Should()
                .Be(2, "the notification and the release are both queued");

            probe.Context.DrainOne().Should().BeTrue();

            probe
                .SelectorOpenReads.Should()
                .Be(0, "the released body consults no selector predicate");
            probe.Host.Requests.Should().BeEmpty();
            probe.Host.CloseReasons.Should().BeEmpty();
            probe.Host.IsOpen.Should().BeTrue();
            probe.Host.DisposeCount.Should().Be(0);
            probe.Host.ResetCount.Should().Be(0);
            probe.DetachCalls.Should().Be(0);
            probe.Context.PendingCount.Should().Be(1, "only the release operation remains queued");
            probe.Errors.Should().BeEmpty();
        }

        /// <summary>
        /// Coordinator line 133: <c>Reset()</c> after <c>Release()</c> cannot invalidate again, so it must
        /// post nothing and never detach the popup messenger or reset the host.
        /// </summary>
        [TestMethod]
        public void Reset_AfterRelease_PostsNothingAndNeverDetachesOrResetsHost()
        {
            var probe = new CountingCoordinatorProbe();
            probe.Coordinator.Release();
            probe.Context.DrainAll();
            int postsAfterRelease = probe.Context.PostCount;
            int detachAfterRelease = probe.DetachCalls;

            probe.Coordinator.Reset();

            probe.Context.PostCount.Should().Be(postsAfterRelease, "invalidation already released");
            probe.Context.PendingCount.Should().Be(0);
            probe.DetachCalls.Should().Be(detachAfterRelease);
            probe.Host.ResetCount.Should().Be(0);
            probe.Host.CloseReasons.Should().BeEmpty();
        }

        /// <summary>
        /// Coordinator lines 224-226: when the dispatched rollback operation itself throws, the rollback
        /// must absorb the secondary failure, complete the shared open request as an unfaulted
        /// <c>false</c>, and leave the selector-cancel count exactly where the throw found it.
        /// </summary>
        [TestMethod]
        public void RequestOpen_RollbackOperationThrows_CompletesFalseWithoutSurfacingSecondary()
        {
            var openFailure = new InvalidOperationException("open threw");
            var rollbackFailure = new InvalidOperationException("rollback threw");
            var probe = new CountingCoordinatorProbe();
            probe.Host.EnqueueThrow(openFailure);
            probe.SelectorOpenFault = rollbackFailure;
            int cancelsBeforeThrow = probe.CancelCalls;

            Task<bool> opening = probe.Coordinator.RequestOpen();
            probe.Context.DrainUntil(opening);

            opening.Status.Should().Be(TaskStatus.RanToCompletion);
            opening.Result.Should().BeFalse();
            opening.IsFaulted.Should().BeFalse();
            opening.IsCanceled.Should().BeFalse();
            opening
                .Exception.Should()
                .BeNull("the rollback secondary never surfaces to the caller");
            probe.CancelCalls.Should().Be(cancelsBeforeThrow);
            probe.Errors.Should().Equal(openFailure, rollbackFailure);
        }

        /// <summary>
        /// Coordinator harness whose selector-open, open-selector, cancel, and detach seams are counted and
        /// individually faultable, so a guarded body can be proven to consult no seam at all. Deterministic;
        /// no Outlook, WebView2, timers, sleeps, or temp files.
        /// </summary>
        private sealed class CountingCoordinatorProbe
        {
            private static readonly Rectangle ProbeAnchor = new Rectangle(10, 20, 200, 24);
            private static readonly Rectangle ProbeWorkingArea = new Rectangle(0, 0, 1600, 900);

            internal CountingCoordinatorProbe()
            {
                Operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(Context, Errors.Enqueue)
                );
                Coordinator = new BreadcrumbDropDownOpenCoordinator(
                    Operations,
                    Host,
                    () => ProbeAnchor,
                    () => ProbeWorkingArea,
                    () => 4,
                    ReadSelectorOpen,
                    OpenSelector,
                    () => CancelCalls++,
                    () => DetachCalls++
                );
            }

            internal CapturingSynchronizationContext Context { get; } =
                new CapturingSynchronizationContext();
            internal ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
            internal ControlledHost Host { get; } = new ControlledHost();
            internal BreadcrumbPopupUiOperations Operations { get; }
            internal BreadcrumbDropDownOpenCoordinator Coordinator { get; }
            internal bool SelectorOpen { get; set; } = true;
            internal Exception SelectorOpenFault { get; set; }
            internal int SelectorOpenReads { get; private set; }
            internal int OpenSelectorCalls { get; private set; }
            internal int CancelCalls { get; private set; }
            internal int DetachCalls { get; private set; }

            private bool ReadSelectorOpen()
            {
                SelectorOpenReads++;
                if (SelectorOpenFault != null)
                    throw SelectorOpenFault;
                return SelectorOpen;
            }

            private bool OpenSelector()
            {
                OpenSelectorCalls++;
                return false;
            }
        }
    }
}
