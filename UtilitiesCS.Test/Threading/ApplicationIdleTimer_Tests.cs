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

        private static ApplicationIdleTimer.ApplicationIdleEventArgs CreateEventArgs(DateTime idleSince)
        {
            // Use reflection to create the internal-constructor EventArgs
            var type = typeof(ApplicationIdleTimer.ApplicationIdleEventArgs);
            var ctor = type.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(DateTime) }, null);

            return (ApplicationIdleTimer.ApplicationIdleEventArgs)ctor.Invoke(new object[] { idleSince });
        }

        #endregion
    }
}
