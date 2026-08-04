using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;
using CapturingSynchronizationContext = QuickFiler.Test.Viewers.BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Deterministic contracts for host-neutral popup-open orchestration.</summary>
    [TestClass]
    public sealed partial class BreadcrumbDropDownOpenCoordinatorTests
    {
        [TestMethod]
        public void ConstructorAndProviderUpdates_GuardEveryRequiredDelegate()
        {
            var harness = new CoordinatorHarness();
            BreadcrumbPopupUiOperations operations = harness.Operations;
            ControlledHost host = harness.Host;
            Func<Rectangle> anchor = () => CoordinatorHarness.Anchor;
            Func<Rectangle> working = () => CoordinatorHarness.WorkingArea;
            Func<int> rows = () => 2;
            Func<bool> state = () => true;
            Func<bool> open = () => false;
            Action cancel = () => { };
            Action detach = () => { };
            Action[] constructorGuards =
            {
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        null,
                        host,
                        anchor,
                        working,
                        rows,
                        state,
                        open,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        null,
                        anchor,
                        working,
                        rows,
                        state,
                        open,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        null,
                        working,
                        rows,
                        state,
                        open,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        anchor,
                        null,
                        rows,
                        state,
                        open,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        anchor,
                        working,
                        null,
                        state,
                        open,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        anchor,
                        working,
                        rows,
                        null,
                        open,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        anchor,
                        working,
                        rows,
                        state,
                        null,
                        cancel,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        anchor,
                        working,
                        rows,
                        state,
                        open,
                        null,
                        detach
                    ),
                () =>
                    new BreadcrumbDropDownOpenCoordinator(
                        operations,
                        host,
                        anchor,
                        working,
                        rows,
                        state,
                        open,
                        cancel,
                        null
                    ),
            };

            using (new AssertionScope())
            {
                foreach (Action guard in constructorGuards)
                    guard.Should().Throw<ArgumentNullException>();
                new Action(() => harness.Coordinator.UpdateRequestProviders(null, working))
                    .Should()
                    .Throw<ArgumentNullException>();
                new Action(() => harness.Coordinator.UpdateRequestProviders(anchor, null))
                    .Should()
                    .Throw<ArgumentNullException>();
                typeof(BreadcrumbDropDownOpenCoordinator).IsNotPublic.Should().BeTrue();
                typeof(BreadcrumbDropDownOpenCoordinator)
                    .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>()
                    .Should()
                    .BeNull();
            }
        }

        [TestMethod]
        public void RequestOpen_ConcurrentCallersShareOneUiBoundSnapshot()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(pending.Task);

            Task<bool> first = harness.Coordinator.RequestOpen();
            Task<bool> second = harness.Coordinator.RequestOpen();
            harness.Context.DrainOne().Should().BeTrue();

            using (new AssertionScope())
            {
                second.Should().BeSameAs(first);
                harness.Coordinator.CurrentOpenTask.Should().BeSameAs(first);
                harness.Host.Requests.Should().ContainSingle();
                harness.Host.RequestThreads.Should().Equal(harness.Context.CreatorThreadId);
                harness.Host.Requests[0].Item1.Should().Be(CoordinatorHarness.Anchor);
                harness.Host.Requests[0].Item2.Should().Be(CoordinatorHarness.WorkingArea);
                harness.Host.Requests[0].Item3.Should().Be(new Size(390, 234));
            }

            pending.SetResult(true);
            harness.Context.DrainUntil(first);
            first.Result.Should().BeTrue();
            harness.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void RequestOpen_SnapshotFailureCancelsOnceAndRetrySucceeds()
        {
            var failure = new InvalidOperationException("anchor snapshot failed");
            var harness = new CoordinatorHarness();
            harness.Coordinator.UpdateRequestProviders(
                () => throw failure,
                () => CoordinatorHarness.WorkingArea
            );

            Task<bool> failed = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(failed);

            failed.Result.Should().BeFalse();
            harness.CancelCount.Should().Be(1);
            harness.Errors.Should().ContainSingle().Which.Should().BeSameAs(failure);
            harness.Host.Requests.Should().BeEmpty();

            harness.SelectorOpen = true;
            harness.Coordinator.UpdateRequestProviders(
                () => CoordinatorHarness.Anchor,
                () => CoordinatorHarness.WorkingArea
            );
            harness.Host.Enqueue(Task.FromResult(true));
            Task<bool> retry = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(retry);
            retry.Result.Should().BeTrue();
            harness.Host.Requests.Should().ContainSingle();
        }

        [TestMethod]
        public void RequestOpen_FalseResultCancelsOnceAndPermitsRetry()
        {
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(Task.FromResult(false));
            harness.Host.Enqueue(Task.FromResult(true));

            Task<bool> first = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(first);
            harness.SelectorOpen = true;
            Task<bool> retry = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(retry);

            first.Result.Should().BeFalse();
            retry.Result.Should().BeTrue();
            harness.CancelCount.Should().Be(1);
            harness.Host.Requests.Should().HaveCount(2);
        }

        [TestMethod]
        public void RequestOpen_SynchronousAndAsynchronousFaultsAreObserved()
        {
            var synchronous = new InvalidOperationException("open threw");
            var asynchronous = new InvalidOperationException("open faulted");
            var harness = new CoordinatorHarness();
            harness.Host.EnqueueThrow(synchronous);
            harness.Host.Enqueue(Task.FromException<bool>(asynchronous));

            Task<bool> first = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(first);
            harness.SelectorOpen = true;
            Task<bool> second = harness.Coordinator.RequestOpen();
            harness.Context.DrainUntil(second);

            first.Result.Should().BeFalse();
            second.Result.Should().BeFalse();
            harness.CancelCount.Should().Be(2);
            harness.Errors.Should().Equal(synchronous, asynchronous);
            harness.Context.ExceptionSnapshot.Should().BeEmpty();
        }

        [TestMethod]
        public void PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(pending.Task);
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainOne().Should().BeTrue();

            harness.Coordinator.SetDroppedDown(false);
            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();
            pending.SetResult(false);
            harness.Context.DrainUntil(opening);

            opening.Result.Should().BeFalse();
            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.Uncommitted);
            harness.CancelCount.Should().Be(0, "the accepting host owns pending rollback");
        }

        [TestMethod]
        public void PendingToggleClose_RejectedHostPerformsOneFallbackCancellation()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness();
            harness.Host.CloseResult = false;
            harness.Host.Enqueue(pending.Task);
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainOne().Should().BeTrue();

            harness.Coordinator.SetDroppedDown(false);
            harness.Context.DrainAll();
            pending.SetResult(false);
            harness.Context.DrainUntil(opening);

            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.Uncommitted);
            harness.CancelCount.Should().Be(1);
        }

        [TestMethod]
        public void PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen()
        {
            var pending = NewCompletion();
            var harness = new CoordinatorHarness();
            harness.Host.Enqueue(pending.Task);
            Task<bool> opening = harness.Coordinator.RequestOpen();
            harness.Context.DrainOne().Should().BeTrue();
            harness.SelectorOpen = false;

            harness.Coordinator.HandleSelectorOpenStateChanged();
            harness.Context.DrainAll();
            pending.SetResult(false);
            harness.Context.DrainUntil(opening);

            harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit);
            harness.CancelCount.Should().Be(0);
        }

        private static TaskCompletionSource<bool> NewCompletion() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private sealed class CoordinatorHarness
        {
            internal static readonly Rectangle Anchor = new Rectangle(120, 240, 390, 25);
            internal static readonly Rectangle WorkingArea = new Rectangle(0, 0, 1920, 1040);

            internal CoordinatorHarness()
            {
                Operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(Context, Errors.Enqueue)
                );
                Coordinator = new BreadcrumbDropDownOpenCoordinator(
                    Operations,
                    Host,
                    () => Anchor,
                    () => WorkingArea,
                    () => 9,
                    () => SelectorOpen,
                    OpenSelector,
                    CancelSelector,
                    () => DetachCount++
                );
            }

            internal CapturingSynchronizationContext Context { get; } =
                new CapturingSynchronizationContext();
            internal ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
            internal ControlledHost Host { get; } = new ControlledHost();
            internal BreadcrumbPopupUiOperations Operations { get; }
            internal BreadcrumbDropDownOpenCoordinator Coordinator { get; }
            internal bool SelectorOpen { get; set; } = true;
            internal int OpenSelectorCount { get; private set; }
            internal int CancelCount { get; private set; }
            internal int DetachCount { get; private set; }

            private bool OpenSelector()
            {
                OpenSelectorCount++;
                if (SelectorOpen)
                    return false;
                SelectorOpen = true;
                Coordinator.HandleSelectorOpenStateChanged();
                return true;
            }

            private void CancelSelector()
            {
                CancelCount++;
                SelectorOpen = false;
            }
        }

        private sealed class ControlledHost : IBreadcrumbDropDownHost
        {
            private readonly Queue<Func<Task<bool>>> _openResults = new Queue<Func<Task<bool>>>();

            public bool IsOpen { get; private set; }
            public IWebViewMessenger PopupMessenger => null;
            public event EventHandler PopupMessengerReady
            {
                add { }
                remove { }
            }

            internal List<Tuple<Rectangle, Rectangle, Size>> Requests { get; } =
                new List<Tuple<Rectangle, Rectangle, Size>>();
            internal List<int> RequestThreads { get; } = new List<int>();
            internal List<BreadcrumbDropDownCloseReason> CloseReasons { get; } =
                new List<BreadcrumbDropDownCloseReason>();
            internal bool CloseResult { get; set; } = true;
            internal Exception CloseFailure { get; set; }
            internal int ResetCount { get; private set; }
            internal int DisposeCount { get; private set; }

            internal void Enqueue(Task<bool> result) => _openResults.Enqueue(() => result);

            internal void EnqueueThrow(Exception failure) =>
                _openResults.Enqueue(() => throw failure);

            internal void SetOpen(bool value) => IsOpen = value;

            public Task<bool> OpenAsync(
                Rectangle anchorScreenBounds,
                Rectangle workingArea,
                Size desiredSize
            )
            {
                Requests.Add(Tuple.Create(anchorScreenBounds, workingArea, desiredSize));
                RequestThreads.Add(Environment.CurrentManagedThreadId);
                Task<bool> result = _openResults.Dequeue()();
                return CompleteOpenAsync(result);
            }

            public bool Close(BreadcrumbDropDownCloseReason reason)
            {
                CloseReasons.Add(reason);
                if (CloseFailure != null)
                    throw CloseFailure;
                if (CloseResult)
                    IsOpen = false;
                return CloseResult;
            }

            public void SetTheme(string theme) { }

            public void Reset()
            {
                ResetCount++;
                IsOpen = false;
            }

            public void Dispose()
            {
                DisposeCount++;
                IsOpen = false;
            }

            private async Task<bool> CompleteOpenAsync(Task<bool> result)
            {
                bool opened = await result.ConfigureAwait(false);
                IsOpen = opened;
                return opened;
            }
        }
    }
}
