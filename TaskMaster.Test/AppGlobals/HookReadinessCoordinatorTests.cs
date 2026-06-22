using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic unit tests for <see cref="HookReadinessCoordinator"/>, the pure decision/
    /// state-machine seam for the Issue #207 readiness-gated startup hookup. The tests drive a
    /// scripted readiness timeline by calling <see cref="HookReadinessCoordinator.Tick"/> directly
    /// with a <c>Mock&lt;IOutlookReadinessGate&gt;</c>; no live COM, no live timer, no clock, no
    /// network, no filesystem, and no temporary files are used. The end-to-end scenario asserted is
    /// not-ready×N → transient COMException on hookup → ready → exactly-once hookup, plus
    /// never-give-up, run-once-after-complete, Unhook-interaction, and non-transient-propagation.
    /// </summary>
    [TestClass]
    public class HookReadinessCoordinatorTests
    {
        private static COMException MakeComException(uint hresult)
        {
            // COMException.ErrorCode maps to the HRESULT; cast back through int to preserve bits.
            return new COMException("scripted transient", unchecked((int)hresult));
        }

        [TestMethod]
        public void Tick_WhenNotReadyAcrossManyTicks_DoesNotInvokeHookupAndContinuesPolling()
        {
            // Arrange: gate reports not-ready on every probe.
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            gate.Setup(g => g.IsReady()).Returns(false);
            int hookupCalls = 0;
            var coordinator = new HookReadinessCoordinator(gate.Object, () => hookupCalls++);

            // Act / Assert: each of N ticks reports ContinuePolling and never invokes the hookup.
            for (int i = 0; i < 5; i++)
            {
                coordinator.Tick().Should().Be(HookReadinessTickResult.ContinuePolling);
            }

            hookupCalls.Should().Be(0, "the hookup must not run until the gate reports ready");
            coordinator.IsCompleted.Should().BeFalse();
        }

        [TestMethod]
        public void Tick_FullScriptedTimeline_NotReadyThenTransientThenReady_HooksExactlyOnce()
        {
            // Arrange: not-ready twice, then ready for the transient-throw tick, then ready again.
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            gate.SetupSequence(g => g.IsReady())
                .Returns(false) // tick 1: not ready
                .Returns(false) // tick 2: not ready
                .Returns(true) // tick 3: ready, but hookup throws transient
                .Returns(true); // tick 4: ready, hookup succeeds
            var transient = MakeComException(OutlookReadinessGate.TransientStoreNotReadyHResult);
            gate.Setup(g => g.IsTransientError(transient)).Returns(true);

            int hookupCalls = 0;
            var coordinator = new HookReadinessCoordinator(
                gate.Object,
                () =>
                {
                    hookupCalls++;
                    if (hookupCalls == 1)
                    {
                        // First time the gate is ready, the COM hookup raises a transient
                        // not-ready HRESULT (race between probe and subscription).
                        throw transient;
                    }
                }
            );

            // Act / Assert: scripted timeline.
            coordinator.Tick().Should().Be(HookReadinessTickResult.ContinuePolling); // not ready
            coordinator.Tick().Should().Be(HookReadinessTickResult.ContinuePolling); // not ready
            coordinator
                .Tick()
                .Should()
                .Be(
                    HookReadinessTickResult.ContinuePolling,
                    "a transient COMException on hookup must route to retry, not completion"
                );
            coordinator.IsCompleted.Should().BeFalse("the run-once guard stays unset on retry");

            coordinator
                .Tick()
                .Should()
                .Be(HookReadinessTickResult.Completed, "the ready hookup succeeds on retry");

            hookupCalls
                .Should()
                .Be(2, "the hookup is attempted on each ready tick until it succeeds");
            coordinator.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public void Tick_AfterCompleted_DoesNotInvokeHookupAgainAndStaysCompleted()
        {
            // Arrange: gate ready immediately.
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            gate.Setup(g => g.IsReady()).Returns(true);
            int hookupCalls = 0;
            var coordinator = new HookReadinessCoordinator(gate.Object, () => hookupCalls++);

            // Act: first tick completes; subsequent ticks must be no-ops.
            coordinator.Tick().Should().Be(HookReadinessTickResult.Completed);
            coordinator.Tick().Should().Be(HookReadinessTickResult.Completed);
            coordinator.Tick().Should().Be(HookReadinessTickResult.Completed);

            // Assert: run-once guard honored.
            hookupCalls.Should().Be(1, "the hookup must run exactly once across repeated ticks");
        }

        [TestMethod]
        public void Tick_NeverGivesUp_ContinuesPollingFarBeyondAnyPlausibleRetryCap()
        {
            // Arrange: gate never becomes ready.
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            gate.Setup(g => g.IsReady()).Returns(false);
            var coordinator = new HookReadinessCoordinator(gate.Object, () => { });

            // Act / Assert: far beyond any plausible retry cap, polling never terminates on its own.
            for (int i = 0; i < 1000; i++)
            {
                coordinator
                    .Tick()
                    .Should()
                    .Be(
                        HookReadinessTickResult.ContinuePolling,
                        "the coordinator must never give up on its own"
                    );
            }

            coordinator.IsCompleted.Should().BeFalse();
        }

        [TestMethod]
        public void Tick_UnhookInteraction_DoesNotReDriveHookupAfterCompletion()
        {
            // Arrange: gate ready immediately; the hookup models a one-time subscribe whose reversal
            // (Unhook) is performed elsewhere and must never be re-driven by the coordinator.
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            gate.Setup(g => g.IsReady()).Returns(true);
            int hookupCalls = 0;
            var coordinator = new HookReadinessCoordinator(gate.Object, () => hookupCalls++);

            // Act: complete the hookup, then tick repeatedly as the timer would after Unhook.
            coordinator.Tick().Should().Be(HookReadinessTickResult.Completed);
            for (int i = 0; i < 10; i++)
            {
                coordinator.Tick().Should().Be(HookReadinessTickResult.Completed);
            }

            // Assert: the coordinator does not re-invoke the hookup; Unhook reverses exactly once.
            hookupCalls
                .Should()
                .Be(1, "the coordinator must not re-drive the hookup after completion");
        }

        [TestMethod]
        public void Tick_WhenHookupThrowsNonTransientComException_PropagatesAndLeavesGuardUnset()
        {
            // Arrange: gate ready; hookup throws a non-transient COMException.
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            gate.Setup(g => g.IsReady()).Returns(true);
            var nonTransient = MakeComException(0x80004005); // E_FAIL, not a known transient.
            gate.Setup(g => g.IsTransientError(nonTransient)).Returns(false);
            var coordinator = new HookReadinessCoordinator(gate.Object, () => throw nonTransient);

            // Act / Assert: a non-transient COMException propagates.
            Action act = () => coordinator.Tick();
            act.Should()
                .Throw<COMException>("a non-transient hookup failure must not be swallowed");
            coordinator.IsCompleted.Should().BeFalse("the run-once guard stays unset on failure");
        }

        [TestMethod]
        public void Constructor_WithNullGate_Throws()
        {
            Action act = () => new HookReadinessCoordinator(null, () => { });
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_WithNullHookup_Throws()
        {
            var gate = new Mock<IOutlookReadinessGate>(MockBehavior.Strict);
            Action act = () => new HookReadinessCoordinator(gate.Object, null);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
