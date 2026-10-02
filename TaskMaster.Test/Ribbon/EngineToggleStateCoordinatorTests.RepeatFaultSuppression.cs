using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issue #948: a prime failure is reported through the error-log sink
    /// once per engine key and base-exception type, every failed prime still clears its marker so
    /// the next cache-miss read re-primes, and recovery stays automatic. A further partial of the
    /// coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types and the
    /// fixture constants are reused without adding any harness member.
    /// </summary>
    /// <remarks>
    /// Every activation read returns an already completed task (faulted, canceled or successful),
    /// which models the cached <c>AsyncLazy</c> fault exactly, and every poll awaits the
    /// coordinator's own prime handle, so each outcome is decided by program order. No test
    /// sleeps, polls a condition, reads the wall clock, touches the filesystem or starts a
    /// message pump.
    /// </remarks>
    public partial class EngineToggleStateCoordinatorTests
    {
        private const string TriageEngine = "Triage";

        #region Issue #948 — repeat prime-failure reports are suppressed per engine and fault kind

        /// <summary>
        /// Regression for issue #948 and the test that carries the fail-before obligation. Five
        /// cache-miss polls against one already faulted activation read re-prime five times and
        /// report once. The count is asserted first, through the numeric assertion, so that the
        /// fail-before message carries the observed number of reports: before the fix every poll
        /// reports and the message reads "but found 5".
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("configuration load failed");
            var faulted = Task.FromException<bool>(failure);
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(faulted);

            // Act
            var pressed = await PollAsync(harness, SpamEngine, 5);

            // Assert
            harness
                .Errors.Count.Should()
                .Be(1, "a repeated fault of one kind for one engine is reported once");
            harness
                .Errors[0]
                .Exception.Should()
                .BeSameAs(failure, "the first report carries the injected fault");
            harness.Errors[0].Message.Should().Contain(SpamEngine);
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(5),
                "suppressing the report must not suppress the re-prime"
            );
            pressed.Should().BeFalse("a failed prime leaves the toggle reporting unchecked");
            harness.Invalidations.Should().BeEmpty("a failed prime changed no state to display");
        }

        /// <summary>
        /// A canceled prime synthesizes a fresh cancellation exception on every cycle; the
        /// synthesized exceptions share one type, so repeated cancellations form a single
        /// failure kind and are reported once.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly()
        {
            // Arrange
            var harness = new Harness();
            var canceled = Task.FromCanceled<bool>(new CancellationToken(true));
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(canceled);

            // Act
            var pressed = await PollAsync(harness, SpamEngine, 5);

            // Assert
            harness.Errors.Should().ContainSingle("repeated cancellations are one failure kind");
            harness
                .Errors[0]
                .Exception.Should()
                .BeAssignableTo<OperationCanceledException>(
                    "a canceled task carries no exception to unwrap, so one is synthesized"
                );
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(5),
                "every canceled prime clears its marker, so every poll re-primes"
            );
            pressed.Should().BeFalse("a canceled prime stored no value");
        }

        /// <summary>
        /// Recovery. Two faults of one kind, the second a suppressed repeat, are followed by a
        /// successful read: the value is cached, the control is invalidated exactly once, and no
        /// further prime runs because the key is now a cache hit.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("configuration load failed");
            var faulted = Task.FromException<bool>(failure);
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(faulted)
                .Returns(faulted)
                .Returns(Task.FromResult(true));

            // Act
            await PollAsync(harness, SpamEngine, 3);

            // Assert
            harness
                .Errors.Should()
                .ContainSingle("the second identical fault is a suppressed repeat");
            harness
                .Coordinator.GetPressed(SpamEngine)
                .Should()
                .BeTrue("the successful prime cached the real state, so the read is a cache hit");
            harness
                .Invalidations.Should()
                .Equal(
                    new[] { SpamToggleControlId },
                    "only the successful prime changed state to display"
                );
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(3),
                "each faulted poll re-primed and the cached key primes no more"
            );
        }

        /// <summary>
        /// The suppression key includes the base-exception type, so a fault of a new kind for a key
        /// that already has a reported fault is reported once more. This is the path a transient
        /// startup fault followed by a permanent configuration fault takes.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenFailureKindChanges_LogsNewKindOnce()
        {
            // Arrange
            var harness = new Harness();
            var firstKind = new InvalidOperationException("configuration load failed");
            var secondKind = new IOException("configuration file unreadable");
            var firstFaulted = Task.FromException<bool>(firstKind);
            var secondFaulted = Task.FromException<bool>(secondKind);
            harness
                .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                .Returns(firstFaulted)
                .Returns(firstFaulted)
                .Returns(secondFaulted)
                .Returns(secondFaulted);

            // Act
            await PollAsync(harness, SpamEngine, 4);

            // Assert
            harness.Errors.Should().HaveCount(2, "each distinct failure kind is reported once");
            harness.Errors[0].Exception.Should().BeSameAs(firstKind, "the first kind is reported");
            harness.Errors[1].Exception.Should().BeSameAs(secondKind, "a new kind is not a repeat");
            harness.Engines.Verify(
                x => x.EngineActiveAsync(SpamEngine),
                Times.Exactly(4),
                "every faulted poll re-primed"
            );
        }

        /// <summary>
        /// Suppression is per engine key: a suppressed Spam fault does not suppress the first
        /// Triage fault. The two mapped keys are the only keys a prime can carry.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged()
        {
            // Arrange
            var harness = new Harness();
            var spamFailure = new InvalidOperationException("spam configuration load failed");
            var triageFailure = new InvalidOperationException("triage configuration load failed");
            harness
                .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                .Returns(Task.FromException<bool>(spamFailure));
            harness
                .Engines.Setup(x => x.EngineActiveAsync(TriageEngine))
                .Returns(Task.FromException<bool>(triageFailure));

            // Act
            await PollAsync(harness, SpamEngine, 2);
            await PollAsync(harness, TriageEngine, 1);

            // Assert
            harness
                .Errors.Should()
                .HaveCount(2, "suppression is keyed by engine as well as by kind");
            harness.Errors[0].Message.Should().Contain(SpamEngine);
            harness.Errors[0].Exception.Should().BeSameAs(spamFailure);
            harness.Errors[1].Message.Should().Contain(TriageEngine);
            harness.Errors[1].Exception.Should().BeSameAs(triageFailure);
        }

        /// <summary>
        /// The suppression applies to prime failures only. A toggle fault after a suppressed prime
        /// fault is still reported, because the click boundary reports every click.
        /// </summary>
        [TestMethod]
        public async Task HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault()
        {
            // Arrange
            var harness = new Harness();
            var primeFailure = new InvalidOperationException("configuration load failed");
            var toggleFailure = new InvalidOperationException("toggle failed");
            harness
                .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                .Returns(Task.FromException<bool>(primeFailure));
            harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(toggleFailure);

            // Act
            await PollAsync(harness, SpamEngine, 2);
            await harness.Coordinator.HandleToggleClickAsync(SpamEngine);

            // Assert
            harness
                .Errors.Should()
                .HaveCount(2, "one suppressed prime repeat, every toggle fault");
            harness.Errors[0].Exception.Should().BeSameAs(primeFailure);
            harness
                .Errors[1]
                .Exception.Should()
                .BeSameAs(toggleFailure, "the click boundary reports");
            harness.Invalidations.Should().BeEmpty("neither path changed state to display");
            harness.Notifications.Should().BeEmpty("a fault is logged, not surfaced as a notice");
        }

        /// <summary>
        /// The first prime-failure entry tells the reader that repeats are suppressed, so a log
        /// that shows one entry is not read as a fault that cleared.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain()
        {
            // Arrange
            var harness = new Harness();
            var failure = new InvalidOperationException("configuration load failed");
            harness
                .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                .Returns(Task.FromException<bool>(failure));

            // Act
            await PollAsync(harness, SpamEngine, 1);

            // Assert
            harness.Errors.Should().ContainSingle("one faulted poll produces one report");
            harness
                .Errors[0]
                .Message.Should()
                .Contain("not logged again", "the entry must state that repeats are suppressed");
            harness
                .Errors[0]
                .Message.Should()
                .EndWith("Further failures of this kind for this engine are not logged again.");
        }

        #endregion Issue #948 — repeat prime-failure reports are suppressed per engine and fault kind

        /// <summary>
        /// Runs a fixed number of cache-miss polls, each the synchronous read followed by awaiting
        /// the prime it started, and returns the answer of the last read. The count is a constant
        /// chosen by the caller, never a condition on coordinator state, so the loop cannot spin.
        /// </summary>
        private static async Task<bool> PollAsync(Harness harness, string engineName, int polls)
        {
            var pressed = false;
            for (var poll = 0; poll < polls; poll++)
            {
                pressed = harness.Coordinator.GetPressed(engineName);
                await harness.Coordinator.GetPrimeTask(engineName);
            }

            return pressed;
        }
    }
}
