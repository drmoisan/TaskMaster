using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class TimedAsyncTask_Tests
    {
        [TestMethod]
        public async Task RequestTask_WithConfiguredTask_InvokesTaskAfterInterval()
        {
            // Arrange
            var completion = new TaskCompletionSource<bool>();
            var timedTask = new TimedAsyncTask(TimeSpan.FromMilliseconds(20), () =>
            {
                completion.TrySetResult(true);
                return Task.CompletedTask;
            });

            // Act
            timedTask.RequestTask();

            // Assert
            (await Task.WhenAny(completion.Task, Task.Delay(1000))).Should().BeSameAs(completion.Task);
            (await completion.Task).Should().BeTrue();
        }

        [TestMethod]
        public async Task RequestTask_WithProvidedTask_InvokesTaskAfterInterval()
        {
            // Arrange
            var completion = new TaskCompletionSource<bool>();
            var timedTask = new TimedAsyncTask(TimeSpan.FromMilliseconds(20));

            // Act
            timedTask.RequestTask(() =>
            {
                completion.TrySetResult(true);
                return Task.CompletedTask;
            });

            // Assert
            (await Task.WhenAny(completion.Task, Task.Delay(1000))).Should().BeSameAs(completion.Task);
            (await completion.Task).Should().BeTrue();
        }

        [TestMethod]
        public async Task CancelTask_PreventsPendingExecution()
        {
            // Arrange
            var completion = new TaskCompletionSource<bool>();
            var timedTask = new TimedAsyncTask(TimeSpan.FromMilliseconds(150), () =>
            {
                completion.TrySetResult(true);
                return Task.CompletedTask;
            });

            // Act
            timedTask.RequestTask();
            timedTask.CancelTask();

            // Assert
            (await Task.WhenAny(completion.Task, Task.Delay(250))).Should().NotBeSameAs(completion.Task);
        }

        [TestMethod]
        public void RequestTask_WithoutConfiguredTask_ThrowsNullReferenceException()
        {
            // Arrange
            var timedTask = new TimedAsyncTask(TimeSpan.FromMilliseconds(20));

            // Act
            Action act = timedTask.RequestTask;

            // Assert
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public async Task RequestOrResetTask_DefersExecutionUntilLatestRequest()
        {
            // Arrange
            var completion = new TaskCompletionSource<DateTime>();
            var startedAt = DateTime.UtcNow;
            var timedTask = new TimedAsyncTask(TimeSpan.FromMilliseconds(80), () =>
            {
                completion.TrySetResult(DateTime.UtcNow);
                return Task.CompletedTask;
            });

            // Act
            timedTask.RequestOrResetTask();
            await Task.Delay(20);
            timedTask.RequestOrResetTask();
            var finishedAt = await completion.Task;

            // Assert
            finishedAt.Should().BeOnOrAfter(startedAt.AddMilliseconds(70));
        }
    }
}
