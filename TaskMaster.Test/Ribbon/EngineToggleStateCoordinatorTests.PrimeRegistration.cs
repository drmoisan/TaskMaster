using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #944: the prime marker must be registered before the prime
    /// starts, so a prime that completes on any thread always finds its own marker to remove and
    /// a finished failed or canceled prime never blocks a later re-prime. A fourth partial of the
    /// coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types and the
    /// fixture constants are reused without adding any harness member.
    /// </summary>
    public partial class EngineToggleStateCoordinatorTests
    {
        #region Issue #944 — prime marker registration precedes the prime start

        /// <summary>
        /// Regression for issue #944 and the test that carries the fail-before obligation.
        /// Invariant: the prime handle is registered before the activation read runs.
        /// The read's setup callback runs synchronously inside the prime start, on the test
        /// thread, and only records the handle it observes; every assertion runs after the
        /// callback has returned, because an assertion thrown inside it would become a prime
        /// fault. Without the fix no handle is registered during the read, so the recorded
        /// handle is the already completed <see cref="Task.CompletedTask"/>, and the outcome is
        /// decided by program order alone.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("configuration load failed");
            Task handleSeenDuringRead = null;
            // Initialized to true so that a callback that never runs fails the first assertion.
            var handleCompletedDuringRead = true;
            harness
                .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                .Returns(() =>
                {
                    handleSeenDuringRead = harness.Coordinator.GetPrimeTask(SpamEngine);
                    handleCompletedDuringRead = handleSeenDuringRead.IsCompleted;
                    return Task.FromException<bool>(failure);
                });

            // Act
            harness.Coordinator.GetPressed(SpamEngine);

            // Assert
            handleCompletedDuringRead
                .Should()
                .BeFalse(
                    "the prime handle must be registered before the activation read runs, "
                        + "so a prime that completes on any thread finds its own marker"
                );
            await handleSeenDuringRead;
            harness.Errors.Should().ContainSingle("a faulted prime is reported exactly once");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(failure, "the sink receives the injected exception unchanged");
            var handleAfterward = harness.Coordinator.GetPrimeTask(SpamEngine);
            handleAfterward
                .Should()
                .NotBeSameAs(
                    handleSeenDuringRead,
                    "a failed prime removes its marker before its handle completes"
                );
            handleAfterward
                .IsCompleted.Should()
                .BeTrue("with no marker registered the returned handle is already complete");
        }

        /// <summary>
        /// Regression guard for issue #944: after a prime whose activation read returns an
        /// already faulted task, a later read starts a new prime. With the fix the outcome is
        /// deterministic; without it this test fails only when a thread-pool thread removes the
        /// marker before it is stored, so it guards the user-visible outcome and does not carry
        /// the fail-before obligation.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("configuration load failed");
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(Task.FromException<bool>(failure))
                .Returns(Task.FromResult(true));

            // Act
            harness.Coordinator.GetPressed(SpamEngine);
            await harness.Coordinator.GetPrimeTask(SpamEngine);
            harness.Coordinator.GetPressed(SpamEngine);
            var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
            await secondPrime;

            // Assert
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(2),
                "a failed prime leaves no marker behind, so the later read starts a new prime"
            );
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("the new prime read the engine as active and cached that value");
            harness
                .Invalidations.Should()
                .Equal(
                    new[] { SpamToggleControlId },
                    "only the successful prime changed state to display"
                );
            harness.Errors.Should().ContainSingle("only the first prime failed");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(failure, "the sink receives the injected exception unchanged");
        }

        /// <summary>
        /// Regression guard for issue #944, canceled variant: after a prime whose activation read
        /// returns an already canceled task, a later read starts a new prime. As with the faulted
        /// variant, only the fixed code makes this outcome deterministic, so this test does not
        /// carry the fail-before obligation.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime()
        {
            // Arrange
            var harness = new Harness();
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(Task.FromCanceled<bool>(new CancellationToken(true)))
                .Returns(Task.FromResult(true));

            // Act
            harness.Coordinator.GetPressed(SpamEngine);
            await harness.Coordinator.GetPrimeTask(SpamEngine);
            harness.Coordinator.GetPressed(SpamEngine);
            var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
            await secondPrime;

            // Assert
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(2),
                "a canceled prime leaves no marker behind, so the later read starts a new prime"
            );
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("the new prime read the engine as active and cached that value");
            harness
                .Invalidations.Should()
                .Equal(
                    new[] { SpamToggleControlId },
                    "only the successful prime changed state to display"
                );
            harness.Errors.Should().ContainSingle("only the first prime was canceled");
            harness
                .Errors[0]
                .Exception.Should()
                .BeAssignableTo<OperationCanceledException>(
                    "a canceled task carries no exception to unwrap, so one is synthesized"
                );
        }

        #endregion Issue #944 — prime marker registration precedes the prime start
    }
}
