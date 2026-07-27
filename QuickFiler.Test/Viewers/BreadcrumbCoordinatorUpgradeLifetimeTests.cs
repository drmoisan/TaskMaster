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
