using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Deterministic unit tests for the <see cref="ThreadMonitor"/> attribution seam
    /// (<c>EvaluatePoll</c>), issue #264. Time is advanced through a <c>FakeTimeProvider</c>; the
    /// responsiveness probe is a stub. No live Outlook, no real <see cref="System.Windows.Threading.Dispatcher"/>,
    /// no real waits or timers. Marked <c>[DoNotParallelize]</c> because the tests read/write the
    /// process-global <see cref="CurrentStoreContext"/>.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ThreadMonitorTests
    {
        private const int ThresholdMs = 5000;

        private static readonly Func<bool> Stalled = () => false;
        private static readonly Func<bool> Responsive = () => true;

        [TestMethod]
        public void LockupAttributionThresholdMs_ReflectsConstructorArgument()
        {
            // Arrange & Act
            var monitor = new ThreadMonitor(
                thread: null,
                timeProvider: new FakeTimeProvider(),
                lockupAttributionThresholdMs: ThresholdMs
            );

            // Assert
            monitor.LockupAttributionThresholdMs.Should().Be(ThresholdMs);
        }

        [TestMethod]
        public void EvaluatePoll_FiresExactlyOnce_WhenThresholdCrossed_AndNotBefore()
        {
            // Arrange
            var fake = new FakeTimeProvider();
            var fired = new List<LockupAttribution>();
            var monitor = new ThreadMonitor(
                thread: null,
                timeProvider: fake,
                lockupAttributionThresholdMs: ThresholdMs,
                onLockupDetected: fired.Add
            );

            // Act & Assert: elapsed 0 -> not confirmed
            monitor.EvaluatePoll(Stalled);
            fired.Should().BeEmpty();

            // 1 ms below threshold -> still not confirmed
            fake.Advance(TimeSpan.FromMilliseconds(ThresholdMs - 1));
            monitor.EvaluatePoll(Stalled);
            fired.Should().BeEmpty("elapsed is strictly below the attribution threshold");

            // Reach the threshold exactly -> fires exactly once
            fake.Advance(TimeSpan.FromMilliseconds(1));
            monitor.EvaluatePoll(Stalled);
            fired.Should().HaveCount(1);

            // Continued stall -> no duplicate fire for the same episode
            fake.Advance(TimeSpan.FromMilliseconds(10000));
            monitor.EvaluatePoll(Stalled);
            fired.Should().HaveCount(1, "the callback fires once per stall episode");
        }

        [TestMethod]
        public void EvaluatePoll_CarriesCurrentStoreContextAndStallDuration()
        {
            // Arrange
            var fake = new FakeTimeProvider();
            var fired = new List<LockupAttribution>();
            var monitor = new ThreadMonitor(
                thread: null,
                timeProvider: fake,
                lockupAttributionThresholdMs: ThresholdMs,
                onLockupDetected: fired.Add
            );

            // Act
            LockupAttribution? raised;
            using (CurrentStoreContext.Begin("Mailbox X"))
            {
                fake.Advance(TimeSpan.FromMilliseconds(6000));
                raised = monitor.EvaluatePoll(Stalled);
            }

            // Assert
            raised.Should().NotBeNull();
            fired.Should().ContainSingle();
            fired[0].StoreIdentity.Should().Be("Mailbox X");
            fired[0].StallDuration.Should().Be(TimeSpan.FromMilliseconds(6000));
        }

        [TestMethod]
        public void EvaluatePoll_NoContext_CarriesNullIdentity()
        {
            // Arrange
            var fake = new FakeTimeProvider();
            var fired = new List<LockupAttribution>();
            var monitor = new ThreadMonitor(
                thread: null,
                timeProvider: fake,
                lockupAttributionThresholdMs: ThresholdMs,
                onLockupDetected: fired.Add
            );

            // Act: no CurrentStoreContext scope active
            fake.Advance(TimeSpan.FromMilliseconds(6000));
            monitor.EvaluatePoll(Stalled);

            // Assert: the callback still fires (attribution is downstream's concern) with null identity.
            fired.Should().ContainSingle();
            fired[0].StoreIdentity.Should().BeNull();
        }

        [TestMethod]
        public void EvaluatePoll_ResponsivePoll_ResetsEpisode_SoASubsequentStallFiresAgain()
        {
            // Arrange
            var fake = new FakeTimeProvider();
            var fired = new List<LockupAttribution>();
            var monitor = new ThreadMonitor(
                thread: null,
                timeProvider: fake,
                lockupAttributionThresholdMs: ThresholdMs,
                onLockupDetected: fired.Add
            );

            // Act & Assert: first stall episode fires once
            fake.Advance(TimeSpan.FromMilliseconds(6000));
            monitor.EvaluatePoll(Stalled);
            fired.Should().HaveCount(1);

            // UI becomes responsive -> resets the episode and the stall clock
            monitor.EvaluatePoll(Responsive);

            // A new stall crossing the threshold fires again
            fake.Advance(TimeSpan.FromMilliseconds(6000));
            monitor.EvaluatePoll(Stalled);
            fired
                .Should()
                .HaveCount(2, "a responsive poll resets the episode so a new stall re-fires");
        }

        [TestMethod]
        public void EvaluatePoll_AttributionPath_RequiresNoThreadOrDispatcher()
        {
            // Arrange: thread is null and no Dispatcher exists. The attribution path must not touch
            // the diagnostic stack-capture path (Thread.Suspend/Dispatcher), so this must not throw.
            var fake = new FakeTimeProvider();
            var fired = new List<LockupAttribution>();
            var monitor = new ThreadMonitor(
                thread: null,
                timeProvider: fake,
                lockupAttributionThresholdMs: ThresholdMs,
                onLockupDetected: fired.Add
            );

            // Act
            fake.Advance(TimeSpan.FromMilliseconds(6000));
            Action act = () => monitor.EvaluatePoll(Stalled);

            // Assert
            act.Should().NotThrow();
            fired.Should().ContainSingle();
        }
    }
}
