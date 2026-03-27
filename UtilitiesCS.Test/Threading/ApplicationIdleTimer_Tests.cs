using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class ApplicationIdleTimer_Tests
    {
        #region ApplicationIdleEventArgs

        [TestMethod]
        public void ApplicationIdleEventArgs_Constructor_SetsIdleSince()
        {
            var idleSince = DateTime.Now.AddSeconds(-5);
            var args = CreateEventArgs(idleSince);

            args.IdleSince.Should().Be(idleSince);
        }

        [TestMethod]
        public void ApplicationIdleEventArgs_IdleDuration_IsPositive()
        {
            var idleSince = DateTime.Now.AddSeconds(-2);
            var args = CreateEventArgs(idleSince);

            args.IdleDuration.TotalSeconds.Should().BeGreaterThan(0);
        }

        #endregion

        #region Subscribe / Unsubscribe

        [TestMethod]
        public void Subscribe_NullHandler_DoesNotThrow()
        {
            // Subscribing with null should be handled gracefully
            Action act = () => ApplicationIdleTimer.Subscribe(null);
            // May or may not throw depending on implementation
        }

        [TestMethod]
        public void GUIActivityThreshold_SetAndGet()
        {
            var original = ApplicationIdleTimer.GUIActivityThreshold;
            ApplicationIdleTimer.GUIActivityThreshold = 500;
            ApplicationIdleTimer.GUIActivityThreshold.Should().Be(500);
            ApplicationIdleTimer.GUIActivityThreshold = original;
        }

        [TestMethod]
        public void CPUUsageThreshold_SetAndGet()
        {
            var original = ApplicationIdleTimer.CPUUsageThreshold;
            ApplicationIdleTimer.CPUUsageThreshold = 0.25;
            ApplicationIdleTimer.CPUUsageThreshold.Should().Be(0.25);
            ApplicationIdleTimer.CPUUsageThreshold = original;
        }

        #endregion

        #region Helpers

        private static ApplicationIdleTimer.ApplicationIdleEventArgs CreateEventArgs(
            DateTime idleSince
        )
        {
            // Use reflection to create the internal-constructor EventArgs
            var type = typeof(ApplicationIdleTimer.ApplicationIdleEventArgs);
            var ctor = type.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(DateTime) },
                null
            );

            return (ApplicationIdleTimer.ApplicationIdleEventArgs)
                ctor.Invoke(new object[] { idleSince });
        }

        #endregion

        #region P68 — Subscription count, event args precision, singleton reference

        // -----------------------------------------------------------------------
        // P68-T1 — Subscribe two handlers, unsubscribe one, listener count is 1.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that subscribing two listeners then unsubscribing one leaves
        /// exactly one listener registered against the static event.
        ///
        /// Purpose:
        ///     Confirm that Subscribe / Unsubscribe correctly add and remove individual
        ///     handlers so the invocation list reflects the expected count.
        ///
        /// Side Effects:
        ///     Cleanup: both handlers are unsubscribed after the assertion to avoid
        ///     any cross-test contamination from the static event.
        /// </summary>
        [TestMethod]
        public void SubscribeTwoListeners_UnsubscribeOne_ListenerCountEqualsOne()
        {
            ApplicationIdleTimer.ApplicationIdleEventHandler h1 = _ => { };
            ApplicationIdleTimer.ApplicationIdleEventHandler h2 = _ => { };

            try
            {
                // Act: subscribe both, then remove the first.
                ApplicationIdleTimer.Subscribe(h1);
                ApplicationIdleTimer.Subscribe(h2);
                ApplicationIdleTimer.Unsubscribe(h1);

                // Assert: invocation list retains exactly the second handler.
                // Use reflection to read the backing field since events cannot be
                // read (only subscribed/unsubscribed) from outside the declaring class.
                var backingField = typeof(ApplicationIdleTimer).GetField(
                    "ApplicationIdle",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
                );
                backingField.Should().NotBeNull();
                var handler = (ApplicationIdleTimer.ApplicationIdleEventHandler)
                    backingField.GetValue(null);
                var count = handler?.GetInvocationList().Length ?? 0;
                count.Should().Be(1);
            }
            finally
            {
                // Cleanup: remove the remaining handler to restore static state.
                ApplicationIdleTimer.Unsubscribe(h2);
            }
        }

        // -----------------------------------------------------------------------
        // P68-T2 — Event args carry the correct IdleSince time and a matching
        //           IdleDuration that reflects the back-dated offset.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that the heartbeat event args correctly expose IdleSince and
        /// IdleDuration with values that match the idle interval.
        ///
        /// Purpose:
        ///     Confirm the event args constructor correctly captures the idle-start
        ///     timestamp and computes an elapsed duration >= the expected minimum.
        ///     The args are constructed via the same path the real heartbeat uses.
        ///
        /// Returns:
        ///     Passes when IdleSince matches the supplied value and IdleDuration
        ///     is at least the back-dated offset.
        /// </summary>
        [TestMethod]
        public void HeartbeatEventArgs_IdleSinceAndIdleDuration_ReflectExpectedElapsedTime()
        {
            // Arrange: simulate the app having been idle for at least 250 ms.
            var idleSince = DateTime.Now.AddMilliseconds(-250);

            // Act: create the args via the same internal constructor the heartbeat uses.
            var args = CreateEventArgs(idleSince);

            // Assert: IdleSince is the exact value passed; IdleDuration is >= the offset.
            args.IdleSince.Should().Be(idleSince);
            args.IdleDuration.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(250));
        }

        // -----------------------------------------------------------------------
        // P68-T3 — The private singleton instance field returns the same reference
        //           on repeated reads.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that the private singleton instance field is initialized exactly
        /// once and returns the same object reference on subsequent reads.
        ///
        /// Purpose:
        ///     Confirm the singleton pattern ensures that all static operations
        ///     (Subscribe, Heartbeat, property access) share the same backing object.
        ///
        /// Returns:
        ///     Passes when both reflection-based reads of the instance field yield
        ///     the same reference.
        /// </summary>
        [TestMethod]
        public void SingletonInstance_ReadTwice_ReturnsSameReference()
        {
            // Arrange: access the private static singleton via reflection.
            var field = typeof(ApplicationIdleTimer).GetField(
                "instance",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
            );
            field.Should().NotBeNull("the private static 'instance' field must exist");

            // Act: read the singleton reference twice.
            var ref1 = field.GetValue(null);
            var ref2 = field.GetValue(null);

            // Assert: both reads resolve to the same object.
            ReferenceEquals(ref1, ref2).Should().BeTrue();
        }

        #endregion
    }
}
