using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #735, finding 3: the toggle-state last-writer race, the silently
    /// ignored canceled prime, and the previously untested engines-unavailable guard on the toggle
    /// path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A second partial of the existing coordinator fixture, so the private <c>Harness</c> and
    /// <c>LoggedError</c> types are reused with no duplication. The existing fixture gains only the
    /// <c>partial</c> keyword; the split exists because that file is already close to the 500-line
    /// ceiling.
    /// </para>
    /// <para>
    /// Every interleaving here is driven deterministically by held
    /// <see cref="TaskCompletionSource{TResult}"/> instances and awaited through the coordinator's
    /// own prime handle. No test sleeps, polls, reads the wall clock, touches the filesystem,
    /// creates a temporary file, or starts a message pump.
    /// </para>
    /// </remarks>
    public partial class EngineToggleStateCoordinatorTests
    {
        #region Issue #735 — last-writer race

        /// <summary>
        /// The #525 reproduction. A prime whose activation read BEGAN before a toggle can COMPLETE
        /// after it. Freshness is determined by when the observation began, not by when the write
        /// lands, so the prime's stale value must be refused.
        /// </summary>
        [TestMethod]
        public async Task ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult()
        {
            // Arrange: the first activation read is held open for the prime; the second, which the
            // toggle performs, answers immediately with the post-toggle truth.
            var harness = new Harness();
            var heldPrimeRead = new TaskCompletionSource<bool>();
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(heldPrimeRead.Task)
                .Returns(Task.FromResult(true));
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).Returns(Task.CompletedTask);

            // Act: a cache-miss read starts the prime, which blocks on its activation read.
            harness.Coordinator.GetPressed(SpamEngine);

            // The user's toggle then runs to completion while that prime is still in flight.
            await harness.Coordinator.ExecuteToggleAsync(SpamEngine);

            // Only now does the prime's read resolve, carrying the stale pre-toggle value.
            heldPrimeRead.SetResult(false);
            await harness.Coordinator.GetPrimeTask(SpamEngine);

            // Assert
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue(
                    "the prime's observation began before the toggle's, so its stale value must "
                        + "not overwrite the newer one"
                );
            harness
                .Invalidations.Should()
                .ContainSingle(
                    "only the toggle's write was applied, and a rejected write must not invalidate"
                );
            harness.Errors.Should().BeEmpty("a refused stale write is not a fault");
        }

        /// <summary>
        /// Toggle versus toggle. Two toggles overlap and the one that STARTED first COMPLETES last;
        /// completion order does not track observation order, so the earlier observation must lose.
        /// </summary>
        [TestMethod]
        public async Task ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult()
        {
            // Arrange
            var harness = new Harness();
            var olderRead = new TaskCompletionSource<bool>();
            var newerRead = new TaskCompletionSource<bool>();
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).Returns(Task.CompletedTask);
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(olderRead.Task)
                .Returns(newerRead.Task);

            // Act: start both toggles, so the first holds the older ticket.
            var older = harness.Coordinator.ExecuteToggleAsync(SpamEngine);
            var newer = harness.Coordinator.ExecuteToggleAsync(SpamEngine);

            // The newer observation resolves first and is applied.
            newerRead.SetResult(true);
            await newer;

            // The older observation resolves last, carrying the stale value.
            olderRead.SetResult(false);
            await older;

            // Assert
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("the newer observation must survive regardless of completion order");
            harness
                .Invalidations.Should()
                .ContainSingle("the rejected older write must not issue a second invalidation");
            harness.Errors.Should().BeEmpty("a refused stale write is not a fault");
        }

        /// <summary>
        /// Guards against over-suppression by the new conditional invalidation. With no competing
        /// writer the single write must be applied and must still invalidate exactly once, so the
        /// compare-and-apply guard cannot degenerate into "never invalidate".
        /// </summary>
        [TestMethod]
        public async Task ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce()
        {
            // Arrange
            var harness = new Harness();
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).Returns(Task.CompletedTask);
            harness
                .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                .Returns(Task.FromResult(true));

            // Act
            await harness.Coordinator.ExecuteToggleAsync(SpamEngine);

            // Assert
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("an uncontended write must be applied");
            harness
                .Invalidations.Should()
                .Equal(
                    new[] { SpamToggleControlId },
                    "Office must be told to re-query getPressed for the mapped control, exactly once"
                );
            harness.Errors.Should().BeEmpty("the uncontended path logs nothing");
        }

        #endregion Issue #735 — last-writer race

        #region Issue #735 — CR-3, the engines-unavailable guard on the toggle path

        /// <summary>
        /// CR-3. The toggle path's own guard, reached only by a direct caller because
        /// <c>HandleToggleClickAsync</c> refuses the case first. It must fail explicitly rather
        /// than dereference null, and it must not have touched the engine before failing.
        /// </summary>
        [TestMethod]
        public async Task ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine()
        {
            // Arrange: the pre-SetGlobals window, where the engines accessor yields null.
            var harness = new Harness();
            harness.EnginesAvailable = false;

            // Act
            Func<Task> act = () => harness.Coordinator.ExecuteToggleAsync(SpamEngine);

            // Assert
            var thrown = await act.Should()
                .ThrowAsync<InvalidOperationException>(
                    "a direct caller must fail explicitly rather than with a null dereference"
                );
            thrown
                .Which.Message.Should()
                .Contain(
                    SpamEngine,
                    "the message must name the engine key so the failure is diagnosable"
                );
            harness.Engines.Verify(
                x => x.ToggleEngineAsync(It.IsAny<string>()),
                Times.Never(),
                "the guard runs before any engine call"
            );
            harness.Invalidations.Should().BeEmpty("nothing changed, so nothing is invalidated");
        }

        #endregion Issue #735 — CR-3, the engines-unavailable guard on the toggle path

        #region Issue #735 — CR-2, the canceled prime

        /// <summary>
        /// CR-2. A canceled prime carries no exception, so the pre-fix completion handler returned
        /// early: nothing was logged and the in-flight marker stayed registered, blocking any
        /// re-prime for the rest of the session.
        /// </summary>
        /// <remarks>
        /// Assertion order is load-bearing. The harness engines mock is strict and this test
        /// supplies one setup, so the re-prime triggered by the second read re-enters that same
        /// canceled task and logs a second error. An error-count assertion taken after the re-prime
        /// would therefore be unsatisfiable by construction. The single-error assertion is made
        /// first, and the marker-cleared conclusion is drawn from prime-handle identity, which is
        /// deterministic.
        /// </remarks>
        [TestMethod]
        public async Task GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
            harness.Coordinator.GetPressed(SpamEngine);
            var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Act
            probe.SetCanceled();
            await firstPrime;

            // Assert the logged failure BEFORE anything can trigger a re-prime.
            harness
                .Errors.Should()
                .ContainSingle(
                    "a canceled prime is a failure and must be reported, not silently ignored"
                );
            harness.Errors[0].Message.Should().Contain(SpamEngine);
            harness
                .Errors[0]
                .Exception.Should()
                .BeAssignableTo<OperationCanceledException>(
                    "a canceled task carries no exception to unwrap, so one is synthesized"
                );
            harness.Invalidations.Should().BeEmpty("a failed prime changed no state to display");

            // Act, part two: a later read must be able to re-prime, which is only possible if the
            // in-flight marker was cleared.
            harness.Coordinator.GetPressed(SpamEngine);
            var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
            await secondPrime;

            // Assert: a distinct handle is the deterministic signal that the marker was cleared and
            // a second prime actually started.
            secondPrime
                .Should()
                .NotBeSameAs(
                    firstPrime,
                    "a cleared marker lets a later read start a genuinely new prime"
                );
        }

        /// <summary>
        /// CR-2 companion. Clearing the marker must not be mistaken for recording a value: a
        /// canceled prime observed nothing, so the toggle keeps reporting unchecked.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
            harness.Coordinator.GetPressed(SpamEngine);

            // Act
            probe.SetCanceled();
            await harness.Coordinator.GetPrimeTask(SpamEngine);

            // Assert
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeFalse("a canceled prime stored no value, so the key reports unchecked");

            // Cleanup: the assertion above was a cache miss, so the cleared marker let it re-prime.
            await harness.Coordinator.GetPrimeTask(SpamEngine);
        }

        #endregion Issue #735 — CR-2, the canceled prime
    }
}
