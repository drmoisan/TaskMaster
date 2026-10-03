using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #964: on the refusal path of <c>HandleToggleClickAsync</c>,
    /// taken when the engines accessor yields null, a <c>notifyUnavailable</c> sink that throws
    /// must not escape into the <c>async void</c> Office handler, and its exception must be
    /// reported once through <c>logError</c>; a data-driven refusal-path test for a null or
    /// empty engine key, which must render the null-name token in the single notification; plus
    /// a guard for the issue #948 record placement now that every sink call goes through one
    /// shared guard. A further partial of the
    /// coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types and the
    /// fixture constants are reused. The harness invokes <c>OnNotify</c> and <c>OnLogError</c>
    /// after it has recorded the call, so a throwing hook both records the attempt and models a
    /// throwing sink. No test sleeps, polls, reads the clock or touches the filesystem.
    /// </summary>
    public partial class EngineToggleStateCoordinatorTests
    {
        #region Issue #964 — a throwing notification sink on the refusal path

        /// <summary>
        /// Regression for issue #964 and the test that carries the fail-before obligation.
        /// Invariant: with the engines unavailable, a throwing notification sink does not escape
        /// the click handler. Without the fix the exception escapes the unguarded notification
        /// call, so the awaited call faults with it.
        /// </summary>
        [TestMethod]
        public async Task HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow()
        {
            // Arrange: the pre-SetGlobals window, with a notification sink that throws.
            var harness = new Harness { EnginesAvailable = false };
            harness.OnNotify = _ => throw new InvalidOperationException("notify sink failed");

            // Act
            Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            await act.Should()
                .NotThrowAsync("a throwing notification sink must not escape the refusal path");
        }

        /// <summary>
        /// Regression for issue #964, the reporting guarantee: the notification is attempted
        /// once, its exception reaches the log sink once and unchanged, and the refused click
        /// still touches no engine member and invalidates no control. Without the fix the
        /// exception escapes, so the test method throws before any assertion runs.
        /// </summary>
        [TestMethod]
        public async Task HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing()
        {
            // Arrange
            var harness = new Harness { EnginesAvailable = false };
            var notifyFailure = new InvalidOperationException("notify sink failed");
            harness.OnNotify = _ => throw notifyFailure;

            // Act
            await harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            harness
                .Notifications.Should()
                .ContainSingle("the notification sink is attempted exactly once");
            harness
                .Errors.Should()
                .ContainSingle("a notification failure is reported once through the log sink");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(
                    notifyFailure,
                    "the log sink receives the notification failure unchanged"
                );
            harness.Errors[0].Message.Should().Contain(SpamEngine);
            harness.Engines.VerifyNoOtherCalls();
            harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");
        }

        /// <summary>
        /// Regression for issue #964, both sinks failing: when the log sink also throws while it
        /// reports the notification failure, the click handler still completes without throwing,
        /// because no further reporting channel remains. Without the fix the notification
        /// exception escapes first.
        /// </summary>
        [TestMethod]
        public async Task HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow()
        {
            // Arrange
            var harness = new Harness { EnginesAvailable = false };
            var notifyFailure = new InvalidOperationException("notify sink failed");
            harness.OnNotify = _ => throw notifyFailure;
            harness.OnLogError = (_, _) => throw new InvalidOperationException("log sink failed");

            // Act
            Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            await act.Should().NotThrowAsync("the refusal path contains a failure of both sinks");
            harness.Notifications.Should().ContainSingle("the notification is attempted once");
            harness
                .Errors.Should()
                .ContainSingle("the log sink is attempted once before it throws");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(
                    notifyFailure,
                    "the log sink receives the notification failure unchanged"
                );
            harness.Engines.VerifyNoOtherCalls();
            harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");
        }

        #endregion Issue #964 — a throwing notification sink on the refusal path

        #region Issue #964 — the refusal path with a null or empty engine key

        /// <summary>
        /// Refusal path for an unusable engine key: with the engines unavailable, a null or empty
        /// key is rendered as the <c>(null)</c> token in the one notification, the click does not
        /// throw, nothing is logged, no engine member is invoked and no control is invalidated.
        /// Exercises the null-or-empty arm of the engine-name renderer through the notification
        /// message builder.
        /// </summary>
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing(
            string engineName
        )
        {
            // Arrange: the pre-SetGlobals window, with sinks that record and do not throw.
            var harness = new Harness { EnginesAvailable = false };

            // Act
            Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(engineName);

            // Assert
            await act.Should()
                .NotThrowAsync("a refused click with an unusable key must degrade quietly");
            harness
                .Notifications.Should()
                .ContainSingle("exactly one notice per refused toggle click");
            harness
                .Notifications[0]
                .Should()
                .Contain("(null)", "an unusable key is rendered as the null-engine-name token");
            harness.Errors.Should().BeEmpty("a refused click is not a fault");
            harness.Engines.VerifyNoOtherCalls();
            harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");
        }

        #endregion Issue #964 — the refusal path with a null or empty engine key

        #region Issue #964 — the issue #948 record placement under the shared guard

        /// <summary>
        /// Guard for the issue #948 invariant now that the prime-fault sink call goes through the
        /// shared guard: a log sink that throws while a faulted prime is reported leaves that
        /// failure kind unrecorded, so the next fault of the same kind is reported again. Passes
        /// before and after the issue #964 change; it fails if the record moves ahead of the sink
        /// call or out of the branch taken when the sink returned normally.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain()
        {
            // Arrange: two faulted primes of one kind; the sink throws on the first report only.
            var harness = new Harness();
            var firstProbe = new TaskCompletionSource<bool>();
            var secondProbe = new TaskCompletionSource<bool>();
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(firstProbe.Task)
                .Returns(secondProbe.Task);
            var reports = 0;
            harness.OnLogError = (_, _) =>
            {
                reports++;
                if (reports == 1)
                {
                    throw new InvalidOperationException("log sink failed");
                }
            };
            harness.Coordinator.GetPressed(SpamEngine);
            var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Act
            firstProbe.SetException(new InvalidOperationException("configuration load failed"));
            await firstPrime;
            harness.Coordinator.GetPressed(SpamEngine);
            var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
            secondProbe.SetException(new InvalidOperationException("configuration load failed"));
            await secondPrime;

            // Assert
            secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");
            harness
                .Errors.Should()
                .HaveCount(
                    2,
                    "a report the throwing sink did not accept is still owed, so the repeat is reported"
                );
            harness.Errors[1].Message.Should().Contain(SpamEngine);
        }

        #endregion Issue #964 — the issue #948 record placement under the shared guard
    }
}
