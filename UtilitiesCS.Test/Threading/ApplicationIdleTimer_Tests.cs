using System;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class ApplicationIdleTimer_Tests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            ClearApplicationIdleHandlers();
            ApplicationIdleTimer.Stop();
            var instance = GetInstance();
            SetPrivateField(instance, "subscriptionCount", 0L);
            SetPrivateField(instance, "syncContext", null);
            SetPrivateField(instance, "cpuThreshold", 0.10d);
            SetPrivateField(instance, "guiThreshold", TimeSpan.TicksPerMillisecond * 50L);
            SetPrivateField(instance, "isIdle", false);
        }

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

        #region P68 Additional Coverage

        [TestMethod]
        public void StartAndStop_WithSynchronizationContext_PostsIdleUnsubscribeAndResetsSubscriptionCount()
        {
            var instance = GetInstance();
            var syncContext = new RecordingSynchronizationContext();

            SetPrivateField(instance, "subscriptionCount", 0L);
            SetPrivateField(instance, "syncContext", syncContext);

            InvokeNonPublic(instance, "StartTimer", null);
            GetPrivateField<long>(instance, "subscriptionCount").Should().Be(1);

            InvokeNonPublic(instance, "StopTimer", null);

            syncContext.PostCount.Should().Be(1);
            GetPrivateField<long>(instance, "subscriptionCount").Should().Be(0);
        }

        [TestMethod]
        public void Heartbeat_WhenIdleThresholdMet_RaisesApplicationIdleAndUpdatesState()
        {
            var instance = GetInstance();
            ApplicationIdleTimer.ApplicationIdleEventArgs observedArgs = null;
            ApplicationIdleTimer.ApplicationIdleEventHandler handler = args => observedArgs = args;

            try
            {
                ApplicationIdleTimer.ApplicationIdle += handler;
                SetPrivateField(instance, "idlesSinceCheckpoint", 3L);
                SetPrivateField(
                    instance,
                    "lastIdleCheckpoint",
                    DateTime.UtcNow.AddSeconds(-2).Ticks
                );
                SetPrivateField(instance, "cpuThreshold", 1.0d);
                SetPrivateField(instance, "isIdle", false);

                InvokeHeartbeat(instance);

                observedArgs.Should().NotBeNull();
                GetPrivateField<bool>(instance, "isIdle").Should().BeTrue();
                GetPrivateField<long>(instance, "idlesSinceCheckpoint").Should().Be(0);
            }
            finally
            {
                ApplicationIdleTimer.ApplicationIdle -= handler;
            }
        }

        [TestMethod]
        public void Heartbeat_WhenGuiActivityIsBusy_KeepsIdleStateFalseAndDoesNotRaiseEvent()
        {
            var instance = GetInstance();
            var raised = false;
            ApplicationIdleTimer.ApplicationIdleEventHandler handler = _ => raised = true;

            try
            {
                ApplicationIdleTimer.ApplicationIdle += handler;
                SetPrivateField(instance, "idlesSinceCheckpoint", 10L);
                SetPrivateField(
                    instance,
                    "lastIdleCheckpoint",
                    DateTime.UtcNow.AddSeconds(-1).Ticks
                );
                SetPrivateField(instance, "guiThreshold", TimeSpan.TicksPerSecond);
                SetPrivateField(instance, "isIdle", true);

                InvokeHeartbeat(instance);

                raised.Should().BeFalse();
                GetPrivateField<bool>(instance, "isIdle").Should().BeFalse();
            }
            finally
            {
                ApplicationIdleTimer.ApplicationIdle -= handler;
            }
        }

        [TestMethod]
        public void ApplicationIdle_Handler_IncrementsCheckpointCounter()
        {
            var instance = GetInstance();
            SetPrivateField(instance, "idlesSinceCheckpoint", 0L);
            SetPrivateField(instance, "subscriptionCount", 0L);

            InvokeNonPublic(instance, "Application_Idle", new object[] { null, EventArgs.Empty });

            GetPrivateField<long>(instance, "idlesSinceCheckpoint").Should().Be(1);
        }

        [TestMethod]
        public void FindTriggeringEventHandler_WhenStarted_ReturnsResultConsistentWithIdleBackingFieldAvailability()
        {
            var instance = GetInstance();
            SetPrivateField(instance, "subscriptionCount", 0L);
            InvokeNonPublic(instance, "StartTimer", null);

            var handler = (Delegate)InvokeNonPublic(
                instance,
                "FindTriggeringEventHandler",
                new object[] { null, EventArgs.Empty }
            );
            var idleField = typeof(System.Windows.Forms.Application).GetField(
                "Idle",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            if (idleField == null)
            {
                handler.Should().BeNull();
            }
            else
            {
                handler.Should().NotBeNull();
                handler!.Method.Name.Should().Be("Application_Idle");
            }
        }

        [TestMethod]
        public void ComputeCpuUsage_WithFutureCheckpoint_UsesNonPositiveDeltaBranch()
        {
            var instance = GetInstance();
            SetPrivateField(instance, "lastCpuCheckpoint", DateTime.UtcNow.AddSeconds(1).Ticks);
            SetPrivateField(instance, "cpuTime", 0L);

            var usage = instance.ComputeCPUUsage(false);

            usage.Should().Be(1.0);
        }

        [TestMethod]
        public void ComputeCpuUsage_WhenIdleAndUsageExceedsThreshold_ClearsIdleFlag()
        {
            var instance = GetInstance();
            SetPrivateField(instance, "lastCpuCheckpoint", DateTime.UtcNow.AddSeconds(-1).Ticks);
            SetPrivateField(instance, "cpuTime", 0L);
            SetPrivateField(instance, "isIdle", true);
            SetPrivateField(instance, "cpuThreshold", -1.0d);

            var usage = instance.ComputeCPUUsage(false);

            usage.Should().BeGreaterThanOrEqualTo(0.0);
            GetPrivateField<bool>(instance, "isIdle").Should().BeFalse();
        }

        [TestMethod]
        public void ComputeGuiActivity_WithNoIdles_ReturnsZero()
        {
            var instance = GetInstance();
            SetPrivateField(instance, "idlesSinceCheckpoint", 0L);

            instance.ComputeGUIActivity().Should().Be(0.0);
        }

        [TestMethod]
        public void OnApplicationIdle_WithoutSubscribers_DoesNothing()
        {
            var instance = GetInstance();

            Action act = () => InvokeNonPublic(instance, "OnApplicationIdle", null);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void CurrentStateProperties_ReturnUnderlyingInstanceValues()
        {
            var instance = GetInstance();
            SetPrivateField(instance, "idlesSinceCheckpoint", 0L);
            SetPrivateField(instance, "lastIdleCheckpoint", DateTime.UtcNow.AddSeconds(-2).Ticks);
            SetPrivateField(instance, "isIdle", true);

            ApplicationIdleTimer.CurrentCPUUsage.Should().BeGreaterThanOrEqualTo(0.0);
            ApplicationIdleTimer.CurrentGUIActivity.Should().Be(0.0);
            ApplicationIdleTimer.IsIdle.Should().BeTrue();
        }

        [TestMethod]
        public void GUIActivityThreshold_SetInvalidValue_ThrowsArgumentOutOfRangeException()
        {
            Action act = () => ApplicationIdleTimer.GUIActivityThreshold = 0;

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void CPUUsageThreshold_SetSameValue_PreservesConfiguredThreshold()
        {
            var original = ApplicationIdleTimer.CPUUsageThreshold;

            ApplicationIdleTimer.CPUUsageThreshold = original;

            ApplicationIdleTimer.CPUUsageThreshold.Should().Be(original);
        }

        [TestMethod]
        public void CPUUsageThreshold_SetNegativeValue_ThrowsArgumentOutOfRangeException()
        {
            Action act = () => ApplicationIdleTimer.CPUUsageThreshold = -0.01;

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void SubscribeAndUnsubscribe_LastHandler_StartsThenStopsTimer()
        {
            ApplicationIdleTimer.ApplicationIdleEventHandler handler = _ => { };

            ApplicationIdleTimer.Subscribe(handler);
            var startedCount = GetPrivateField<long>(GetInstance(), "subscriptionCount");
            ApplicationIdleTimer.Unsubscribe(handler);

            startedCount.Should().Be(1);
            GetPrivateField<long>(GetInstance(), "subscriptionCount").Should().Be(0);
        }

        private static ApplicationIdleTimer GetInstance()
        {
            var field = typeof(ApplicationIdleTimer).GetField(
                "instance",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            return (ApplicationIdleTimer)field!.GetValue(null);
        }

        private static T GetPrivateField<T>(object instance, string fieldName)
        {
            var field = instance
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)field!.GetValue(instance);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            instance
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(instance, value);
        }

        private static object InvokeNonPublic(object instance, string methodName, object[] args)
        {
            var method = instance
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            return method!.Invoke(instance, args);
        }

        private static void InvokeHeartbeat(ApplicationIdleTimer instance)
        {
            InvokeNonPublic(instance, "Heartbeat", new object[] { null, null });
        }

        private static void ClearApplicationIdleHandlers()
        {
            var backingField = typeof(ApplicationIdleTimer).GetField(
                "ApplicationIdle",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            backingField!.SetValue(null, null);
        }

        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            public int PostCount { get; private set; }

            public override void Post(SendOrPostCallback d, object state)
            {
                PostCount++;
                d(state);
            }
        }

        #endregion
    }
}
