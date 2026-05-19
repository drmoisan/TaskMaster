using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class TimedBatchAction_Tests
    {
        [TestMethod]
        public void RequestAction_WithConfiguredAction_InvokesActionAfterInterval()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            var action = new TimedBatchAction(TimeSpan.FromMilliseconds(20), signal.Set);

            // Act
            action.RequestAction();

            // Assert
            signal.Wait(500).Should().BeTrue();
        }

        [TestMethod]
        public void RequestAction_WithProvidedAction_InvokesActionAfterInterval()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            var action = new TimedBatchAction(TimeSpan.FromMilliseconds(20));

            // Act
            action.RequestAction(signal.Set);

            // Assert
            signal.Wait(500).Should().BeTrue();
        }

        [TestMethod]
        public void CancelAction_PreventsPendingExecution()
        {
            // Arrange
            using var signal = new ManualResetEventSlim(false);
            var action = new TimedBatchAction(TimeSpan.FromMilliseconds(150), signal.Set);

            // Act
            action.RequestAction();
            action.CancelAction();

            // Assert
            signal.Wait(250).Should().BeFalse();
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
            var count = 0;
            using var signal = new ManualResetEventSlim(false);
            var action = new TimedBatchAction(
                TimeSpan.FromMilliseconds(20),
                () =>
                {
                    Interlocked.Increment(ref count);
                    signal.Set();
                }
            );

            // Act
            action.RequestAction();
            action.RequestAction();
            // Allow 2 000 ms for the 20 ms timer to fire under full-suite thread-pool load.
            signal.Wait(2000).Should().BeTrue();
            // Wait long enough to detect any spurious second fire from the single-shot timer.
            Thread.Sleep(200);

            // Assert
            count.Should().Be(1);
        }

        [TestMethod]
        public void AfterActionExecutes_RequestActionCanScheduleAnotherRun()
        {
            // Arrange
            var count = 0;
            using var first = new ManualResetEventSlim(false);
            using var second = new ManualResetEventSlim(false);
            var action = new TimedBatchAction(
                TimeSpan.FromMilliseconds(20),
                () =>
                {
                    var current = Interlocked.Increment(ref count);
                    if (current == 1)
                    {
                        first.Set();
                    }
                    else
                    {
                        second.Set();
                    }
                }
            );

            // Act
            action.RequestAction();
            first.Wait(500).Should().BeTrue();
            action.RequestAction();

            // Assert
            second.Wait(500).Should().BeTrue();
            count.Should().Be(2);
        }
    }
}
