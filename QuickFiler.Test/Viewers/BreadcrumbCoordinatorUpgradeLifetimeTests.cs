using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    [TestClass]
    public class BreadcrumbCoordinatorUpgradeLifetimeTests
    {
        [TestMethod]
        public void ArgumentGuards_NullInputsThrowArgumentNullException()
        {
            Action construct = () => new BreadcrumbCoordinatorUpgradeLifetime(null);
            construct.Should().Throw<ArgumentNullException>();

            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
            var lease = lifetime.BeginPopulation();

            Action runSynchronous = () => lifetime.RunSynchronous(lease, null);
            Action guard = () => lifetime.Guard(lease, null);
            Func<Task> runAsyncLease = () => lifetime.RunAsync(null, _ => Task.CompletedTask);
            Func<Task> runAsyncOperation = () => lifetime.RunAsync(lease, null);

            runSynchronous.Should().Throw<ArgumentNullException>();
            guard.Should().Throw<ArgumentNullException>();
            runAsyncLease.Should().ThrowAsync<ArgumentNullException>().GetAwaiter().GetResult();
            runAsyncOperation.Should().ThrowAsync<ArgumentNullException>().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure()
        {
            var reported = new List<Exception>();
            var report = new Mock<Action<Exception>>();
            report.Setup(action => action(It.IsAny<Exception>())).Callback<Exception>(reported.Add);
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(report.Object);
            var lease = lifetime.BeginPopulation();
            var sentinel = new InvalidOperationException("controlled cancellation failure");
            lease.Token.Register(() => throw sentinel);

            Action run = () =>
                lifetime.RunSynchronous(lease, () => throw new InvalidOperationException());

            run.Should().Throw<InvalidOperationException>();
            lease.Cancelled.Should().BeTrue();
            lease.Settled.Should().BeTrue();
            reported.Should().ContainSingle().Which.Should().BeOfType<AggregateException>();
            ((AggregateException)reported[0]).InnerExceptions.Should().Contain(sentinel);
            report.Verify(action => action(It.IsAny<Exception>()), Times.Once);
        }

        [TestMethod]
        public async Task RunAsync_SupersededCancellationIsSwallowedAndSettled()
        {
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
            var lease = lifetime.BeginPopulation();
            lifetime.BeginPopulation();

            await lifetime.RunAsync(lease, token => Task.FromCanceled(token));

            lease.Cancelled.Should().BeTrue();
            lease.Settled.Should().BeTrue();
            lease.SourceDisposed.Should().BeTrue();
        }

        [TestMethod]
        public async Task Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported()
        {
            var sentinel = new InvalidOperationException("controlled lease disposal failure");
            var reported = new List<Exception>();
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(reported.Add);
            var lease = new BreadcrumbUpgradeLease(
                1,
                new ThrowingCancellationTokenSource(sentinel)
            );
            SetCurrentLease(lifetime, lease);

            lifetime.Dispose();
            lifetime.Dispose();
            await lifetime.RunAsync(lease, _ => Task.CompletedTask);

            lifetime.TryDispose().Should().BeFalse();
            lease.SourceDisposed.Should().BeTrue();
            reported.Should().Contain(sentinel);
        }

        /// <summary>
        /// Issue #500 (I-500.1): at the moment the guarded action executes, the calling thread must
        /// not hold <c>BreadcrumbCoordinatorUpgradeLifetime._sync</c>. The probe reads
        /// <see cref="Monitor.IsEntered(object)"/> against the reflected private field from inside the
        /// action itself. <c>Monitor.IsEntered</c> reports whether the CURRENT thread holds the lock,
        /// so the probe is exact on a single thread: no second thread, no timer, no wait.
        /// </summary>
        [TestMethod]
        public void TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync()
        {
            // Arrange
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
            object sync = GetSync(lifetime);
            var lease = lifetime.BeginPopulation();
            bool heldDuringAction = true;

            // Act
            bool invoked = lifetime.TryRunCurrent(
                lease,
                () => heldDuringAction = Monitor.IsEntered(sync)
            );

            // Assert
            invoked.Should().BeTrue("the lease is current, so the action must have been invoked");
            heldDuringAction
                .Should()
                .BeFalse(
                    "no foreign call may be made while the lifetime's _sync is held (I-500.1)"
                );
        }

        /// <summary>
        /// Issue #500 (I-500.3), and the standing regression guard for the cross-cutting NFR (AC-28):
        /// <c>TryRunCurrent</c>'s <c>bool</c> is the ENTRY-TIME currency verdict and must never be
        /// retro-actively falsified. An action that re-entrantly invalidates its own lease still
        /// yields <c>true</c> — the action really was invoked at entry-time currency — while
        /// <c>IsCurrent(lease)</c> reports <c>false</c> immediately afterwards, which is where the
        /// supersession is observable. This test is GREEN on HEAD by design: it documents the
        /// contract the #500 fix must preserve, and it is what would fail if a future change folded a
        /// post-action currency re-check into the return value (research section 6.2 option B).
        /// Determinism comes from an injected re-entrant action: no second thread, no timer, no wait.
        /// </summary>
        [TestMethod]
        public void TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation()
        {
            // Arrange
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
            var lease = lifetime.BeginPopulation();
            bool ran = false;

            // Act: the guarded action supersedes its own lease while it is running.
            bool invoked = lifetime.TryRunCurrent(
                lease,
                () =>
                {
                    ran = true;
                    lifetime.Invalidate();
                }
            );

            // Assert
            ran.Should().BeTrue("the action was invoked");
            invoked
                .Should()
                .BeTrue(
                    "the bool is the entry-time verdict and must not be falsified by the action's own re-entrant mutation"
                );
            lifetime
                .IsCurrent(lease)
                .Should()
                .BeFalse(
                    "the supersession is observable afterwards through IsCurrent, not the bool"
                );
        }

        /// <summary>
        /// Issue #502 companion defect (I-502.3): a lease whose guarded action was SKIPPED must still
        /// reach <c>Settled == true</c> and <c>SourceDisposed == true</c>. On HEAD a skipped
        /// <c>RunSynchronous</c> never calls <c>Complete(lease)</c>, so <c>Settled</c> stays
        /// <c>false</c>, <c>CancelLease</c>'s disposal condition never holds, and the lease's
        /// <see cref="System.Threading.CancellationTokenSource"/> is leaked once per superseded
        /// population. This is the only #502 assertion that compiles against HEAD without a signature
        /// or seam change, which is why it is the failing-first test for #502. The call is written as
        /// a statement so it is valid against both the pre-change <c>void</c> signature and the
        /// post-change <c>bool</c> one. Deterministic: one thread, no second thread, no wait.
        /// </summary>
        [TestMethod]
        public void RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource()
        {
            // Arrange
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
            var lease = lifetime.BeginPopulation();
            lifetime.Invalidate();
            bool ran = false;

            // Act
            lifetime.RunSynchronous(lease, () => ran = true);

            // Assert
            ran.Should().BeFalse("the lease was superseded, so the guarded action must not run");
            lease.Settled.Should().BeTrue("a skipped lease must still be settled (I-502.3)");
            lease
                .SourceDisposed.Should()
                .BeTrue("a settled lease's CancellationTokenSource must be disposed, not leaked");
        }

        /// <summary>
        /// Issue #502 (I-502.1): <c>RunSynchronous</c> returns <c>false</c> when, and ONLY when, the
        /// guarded action did not run. Asserted in both directions in one test, because "only when" is
        /// half the invariant: a superseded lease must yield <c>false</c> with the action skipped, and
        /// a current lease must yield <c>true</c> with the action run. Deterministic: one thread, no
        /// second thread, no timer, no wait.
        /// </summary>
        [TestMethod]
        public void RunSynchronous_SupersededLeaseReportsSkipToCaller()
        {
            // Arrange: direction 1 — a superseded lease.
            var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
            var superseded = lifetime.BeginPopulation();
            lifetime.Invalidate();
            bool supersededRan = false;

            // Act
            bool supersededResult = lifetime.RunSynchronous(superseded, () => supersededRan = true);

            // Assert
            supersededResult.Should().BeFalse("the guarded action did not run");
            supersededRan.Should().BeFalse("a superseded lease must skip its action");

            // Arrange: direction 2 — a current lease.
            var current = lifetime.BeginPopulation();
            bool currentRan = false;

            // Act
            bool currentResult = lifetime.RunSynchronous(current, () => currentRan = true);

            // Assert
            currentResult.Should().BeTrue("the guarded action ran");
            currentRan.Should().BeTrue("a current lease must run its action");
        }

        private static object GetSync(BreadcrumbCoordinatorUpgradeLifetime lifetime)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            return typeof(BreadcrumbCoordinatorUpgradeLifetime)
                .GetField("_sync", Flags)
                .GetValue(lifetime);
        }

        private static void SetCurrentLease(
            BreadcrumbCoordinatorUpgradeLifetime lifetime,
            BreadcrumbUpgradeLease lease
        )
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(BreadcrumbCoordinatorUpgradeLifetime)
                .GetField("_current", Flags)
                .SetValue(lifetime, lease);
            typeof(BreadcrumbCoordinatorUpgradeLifetime)
                .GetField("_generation", Flags)
                .SetValue(lifetime, lease.Generation);
        }

        private sealed class ThrowingCancellationTokenSource : CancellationTokenSource
        {
            private readonly Exception _exception;

            internal ThrowingCancellationTokenSource(Exception exception)
            {
                _exception = exception;
            }

            protected override void Dispose(bool disposing)
            {
                throw _exception;
            }
        }
    }
}
