using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #947: a <c>logError</c> sink that throws while a faulted or
    /// canceled prime is reported must not leave the prime marker registered, must not move the
    /// report after the clear, and must leave no faulted task behind; a sink that throws while
    /// the click boundary reports a toggle fault must not escape that boundary. A fifth partial
    /// of the coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types
    /// and the fixture constants are reused without adding any harness member. The harness
    /// invokes <c>OnLogError</c> after it has recorded the error, so a throwing hook both
    /// records the report and models a throwing sink.
    /// </summary>
    public partial class EngineToggleStateCoordinatorTests
    {
        #region Issue #947 — a throwing log sink leaves no stale prime marker

        /// <summary>
        /// Regression for issue #947 and the faulted variant that carries the fail-before
        /// obligation. Invariant: when the sink throws while a faulted prime is reported, the
        /// marker is still removed, so a later read starts a new prime. The first prime handle is
        /// captured before the trigger, because the fixed code clears the marker before that
        /// handle completes. Without the fix the sink exception skips the clear, the later read
        /// finds the stale marker and starts nothing, and the activation read is verified once
        /// rather than twice. No sleep, delay, timer or parallelism attribute.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            var failure = new InvalidOperationException("configuration load failed");
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(probe.Task)
                .Returns(Task.FromResult(true));
            harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed");
            harness.Coordinator.GetPressed(SpamEngine);
            var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Act
            probe.SetException(failure);
            await firstPrime;
            harness.Coordinator.GetPressed(SpamEngine);
            var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Assert
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(2),
                "a throwing sink leaves no marker behind, so the later read starts a new prime"
            );
            secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");
            await secondPrime;
            harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(failure, "the sink receives the injected exception unchanged");
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
        }

        /// <summary>
        /// Regression for issue #947, canceled variant: when the sink throws while a canceled
        /// prime is reported with the synthesized cancellation exception, the marker is still
        /// removed and a later read starts a new prime. Fails before the fix for the same reason
        /// as the faulted variant.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(probe.Task)
                .Returns(Task.FromResult(true));
            harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed");
            harness.Coordinator.GetPressed(SpamEngine);
            var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Act
            probe.SetCanceled();
            await firstPrime;
            harness.Coordinator.GetPressed(SpamEngine);
            var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Assert
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(2),
                "a throwing sink leaves no marker behind, so the later read starts a new prime"
            );
            secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");
            await secondPrime;
            harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
            harness
                .Errors[0]
                .Exception.Should()
                .BeAssignableTo<OperationCanceledException>(
                    "a canceled task carries no exception to unwrap, so one is synthesized"
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
        }

        /// <summary>
        /// Regression for issue #947, the no-unobserved-fault guarantee. The hook probes the prime
        /// handle from inside the throwing sink, so report-then-clear is observed under a throwing
        /// sink as well. The first prime handle, captured before the trigger, must end
        /// ran-to-completion, and once it has completed the marker must be cleared, which is only
        /// possible when the sink exception was contained inside the coordinator. Without the fix
        /// the marker stays registered after the handle completes.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            var failure = new InvalidOperationException("configuration load failed");
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
            harness.Coordinator.GetPressed(SpamEngine);
            var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
            Task handleSeenBySink = null;
            harness.OnLogError = (_, _) =>
            {
                handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);
                throw new InvalidOperationException("sink failed");
            };

            // Act
            probe.SetException(failure);
            await firstPrime;

            // Assert
            firstPrime
                .Status.Should()
                .Be(TaskStatus.RanToCompletion, "the prime handle never faults");
            handleSeenBySink
                .Should()
                .BeSameAs(firstPrime, "the report is attempted before the marker is cleared");
            harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(failure, "the sink receives the injected exception unchanged");
            harness.Invalidations.Should().BeEmpty("a failed prime leaves nothing to display");
            harness
                .Coordinator.GetPrimeTask(SpamEngine)
                .Should()
                .BeSameAs(
                    Task.CompletedTask,
                    "the sink exception is contained, so the marker is still cleared"
                );
        }

        /// <summary>
        /// Regression for issue #947, the click-boundary call site. Invariant: when the toggle
        /// faults and the sink then throws, the click boundary still attempts the report and
        /// does not throw, because its caller is an <c>async void</c> Office handler. Without
        /// the fix the sink exception escapes the boundary, so the awaited call faults with the
        /// sink exception.
        /// </summary>
        [TestMethod]
        public async Task HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("toggle failed");
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(failure);
            harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed");

            // Act
            Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            await act.Should()
                .NotThrowAsync("the click boundary contains a failure of the sink itself");
            harness.Errors.Should().ContainSingle("the sink was invoked once before it threw");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(failure, "the sink receives the toggle fault unchanged");
            harness.Invalidations.Should().BeEmpty("a failed toggle changed no state to display");
            harness.Notifications.Should().BeEmpty("a fault is logged, not surfaced as a notice");
        }

        #endregion Issue #947 — a throwing log sink leaves no stale prime marker
    }
}
