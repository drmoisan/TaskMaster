using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression for issue #942: the prime-fault report must precede the in-flight marker's
    /// removal, so any caller that observes the marker absent observes a report that has already
    /// completed. A third partial of the coordinator fixture, so the private <c>Harness</c> and
    /// <c>LoggedError</c> types are reused; the primary file is close to the 500-line ceiling.
    /// </summary>
    public partial class EngineToggleStateCoordinatorTests
    {
        #region Issue #942 — prime fault report precedes marker removal

        /// <summary>
        /// Regression for issue #942. Invariant: for a key whose prime did not run to completion,
        /// the in-flight marker is present until the fault report has returned. The discriminator
        /// is the prime handle observed from inside the error-log sink: it is the still-registered
        /// continuation under the fixed order and <see cref="Task.CompletedTask"/> under the
        /// defective one, on the same thread, so the outcome is a function of program order
        /// rather than of scheduling. No sleep, delay, gate, timer or parallelism attribute.
        /// </summary>
        [TestMethod]
        public async Task GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged()
        {
            // Arrange
            var harness = new Harness();
            var probe = new TaskCompletionSource<bool>();
            var failure = new InvalidOperationException("configuration load failed");
            harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task);
            harness.Coordinator.GetPressed(SpamEngine);
            var prime = harness.Coordinator.GetPrimeTask(SpamEngine);
            Task handleSeenBySink = null;
            harness.OnLogError = (_, _) =>
                handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);

            // Act
            probe.SetException(failure);
            await prime;

            // Assert
            // If this test passes without the production reorder in CompletePrime, the negative
            // control has lost isolation: investigate the run rather than accepting it.
            handleSeenBySink
                .Should()
                .BeSameAs(
                    prime,
                    "while the fault is being reported the prime handle "
                        + "must still be registered, so a caller that fetches it "
                        + "after the trigger awaits the report"
                );
            harness.Errors.Should().ContainSingle("a prime fault is reported exactly once");
            harness
                .Errors[0]
                .Message.Should()
                .Contain(SpamEngine, "the message names the engine whose prime failed");
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
                    "once the handle has completed the marker has been cleared "
                        + "so a later read may re-prime"
                );
        }

        #endregion Issue #942 — prime fault report precedes marker removal
    }
}
