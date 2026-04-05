using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    public partial class TimeOutTask_Tests
    {
        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2T3_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, int, int, CancellationToken, Task<int>> function = (
                first,
                second,
                third,
                token
            ) => Task.FromException<int>(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    10,
                    20,
                    30,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncActionT1T2T3_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int, int, int, CancellationToken, Task> function = (first, second, third, token) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    return Task.FromCanceled(new CancellationToken(true));
                }

                return Task.CompletedTask;
            };

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    10,
                    20,
                    30,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 1,
                    strict: true
                );

            // Assert
            await act.Should().NotThrowAsync();
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncActionT1T2T3_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, int, int, CancellationToken, Task> function = (first, second, third, token) =>
                Task.FromException(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    10,
                    20,
                    30,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task TimeoutAfter_GenericTask_ShouldThrowTimeoutException_ForZeroTimeout()
        {
            // Arrange
            var task = Task.Delay(50).ContinueWith(_ => 42);

            // Act
            Func<Task> act = async () => await task.TimeoutAfter(0);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }

        [TestMethod]
        public async Task MarshalTaskResults_ShouldTransferFaultedTaskException()
        {
            // Arrange
            var proxy = new TaskCompletionSource<int>();
            var source = Task.FromException(new InvalidOperationException("boom"));

            // Act
            TimeOutTask.MarshalTaskResults(source, proxy);

            // Assert
            Func<Task> act = async () => await proxy.Task;
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task MarshalTaskResults_ShouldTransferCanceledTask()
        {
            // Arrange
            var proxy = new TaskCompletionSource<int>();
            var source = Task.FromCanceled(new CancellationToken(true));

            // Act
            TimeOutTask.MarshalTaskResults(source, proxy);

            // Assert
            Func<Task> act = async () => await proxy.Task;
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task MarshalTaskResults_ShouldTransferGenericTaskResult()
        {
            // Arrange
            var proxy = new TaskCompletionSource<int>();

            // Act
            TimeOutTask.MarshalTaskResults(Task.FromResult(29), proxy);

            // Assert
            var result = await proxy.Task;
            result.Should().Be(29);
        }

        [TestMethod]
        public async Task MarshalTaskResults_ShouldUseDefaultForNonGenericCompletedTask()
        {
            // Arrange
            var proxy = new TaskCompletionSource<int>();

            // Act
            TimeOutTask.MarshalTaskResults(Task.CompletedTask, proxy);

            // Assert
            var result = await proxy.Task;
            result.Should().Be(0);
        }
    }
}
