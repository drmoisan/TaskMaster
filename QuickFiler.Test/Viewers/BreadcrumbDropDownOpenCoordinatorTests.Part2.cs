using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

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
    }
}
