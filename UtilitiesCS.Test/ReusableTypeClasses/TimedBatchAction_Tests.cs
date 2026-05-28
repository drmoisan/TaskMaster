using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Interfaces;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class TimedBatchAction_Tests
    {
        [TestMethod]
        public void RequestAction_WithConfiguredAction_InvokesActionAfterInterval()
        {
            // Arrange
            var timerFactory = new FakeTimerFactory();
            var wasCalled = false;
            var action = new TimedBatchAction(
                TimeSpan.FromMilliseconds(20),
                () => wasCalled = true,
                timerFactory.Create
            );

            // Act
            action.RequestAction();
            timerFactory.SingleCreatedTimer.Fire();

            // Assert
            wasCalled.Should().BeTrue();
        }

        [TestMethod]
        public void RequestAction_WithProvidedAction_InvokesActionAfterInterval()
        {
            // Arrange
            var timerFactory = new FakeTimerFactory();
            var wasCalled = false;
            var action = new TimedBatchAction(TimeSpan.FromMilliseconds(20), null, timerFactory.Create);

            // Act
            action.RequestAction(() => wasCalled = true);
            timerFactory.SingleCreatedTimer.Fire();

            // Assert
            wasCalled.Should().BeTrue();
        }

        [TestMethod]
        public void CancelAction_PreventsPendingExecution()
        {
            // Arrange
            var timerFactory = new FakeTimerFactory();
            var wasCalled = false;
            var action = new TimedBatchAction(
                TimeSpan.FromMilliseconds(150),
                () => wasCalled = true,
                timerFactory.Create
            );

            // Act
            action.RequestAction();
            action.CancelAction();
            timerFactory.SingleCreatedTimer.Fire();

            // Assert
            wasCalled.Should().BeFalse();
        }

        [TestMethod]
        public void RequestAction_WithoutConfiguredAction_ThrowsNullReferenceException()
        {
            // Arrange
            var action = new TimedBatchAction(TimeSpan.FromMilliseconds(20));

            // Act
            Action act = action.RequestAction;

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void RequestAction_TwiceBeforeExecution_OnlyInvokesCallbackOnce()
        {
            // Arrange
            var timerFactory = new FakeTimerFactory();
            var count = 0;
            var action = new TimedBatchAction(
                TimeSpan.FromMilliseconds(20),
                () =>
                {
                    count++;
                },
                timerFactory.Create
            );

            // Act
            action.RequestAction();
            action.RequestAction();
            timerFactory.CreatedTimers.Should().HaveCount(1);
            timerFactory.SingleCreatedTimer.Fire();

            // Assert
            count.Should().Be(1);
        }

        [TestMethod]
        public void AfterActionExecutes_RequestActionCanScheduleAnotherRun()
        {
            // Arrange
            var timerFactory = new FakeTimerFactory();
            var count = 0;
            var action = new TimedBatchAction(
                TimeSpan.FromMilliseconds(20),
                () => count++,
                timerFactory.Create
            );

            // Act
            action.RequestAction();
            timerFactory.CreatedTimers[0].Fire();
            action.RequestAction();
            timerFactory.CreatedTimers.Should().HaveCount(2);
            timerFactory.CreatedTimers[1].Fire();

            // Assert
            count.Should().Be(2);
        }

        private sealed class FakeTimerFactory
        {
            public List<FakeTimer> CreatedTimers { get; } = new List<FakeTimer>();

            public FakeTimer SingleCreatedTimer => CreatedTimers.Should().ContainSingle().Subject;

            public ITimerWrapper Create(TimeSpan interval)
            {
                var timer = new FakeTimer(interval);
                CreatedTimers.Add(timer);
                return timer;
            }
        }

        private sealed class FakeTimer : ITimerWrapper
        {
            public FakeTimer(TimeSpan interval)
            {
                Interval = interval;
                IntervalInMilliseconds = interval.TotalMilliseconds;
            }

            public event EventHandler<TimeElapsedEventArgs> Elapsed;

            public bool AutoReset { get; set; }

            public bool Enabled { get; set; }

            public TimeSpan Interval { get; set; }

            public double IntervalInMilliseconds { get; set; }

            public void Dispose()
            {
            }

            public void Fire()
            {
                if (!Enabled)
                {
                    return;
                }

                if (!AutoReset)
                {
                    Enabled = false;
                }

                Elapsed?.Invoke(this, new TimeElapsedEventArgs(DateTime.UtcNow));
            }

            public void ResetTimer()
            {
                Enabled = true;
            }

            public void StartTimer()
            {
                Enabled = true;
            }

            public void StopTimer()
            {
                Enabled = false;
            }
        }
    }
}
