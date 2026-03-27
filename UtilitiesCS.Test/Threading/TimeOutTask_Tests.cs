using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    [TestClass]
    public partial class TimeOutTask_Tests
    {
        [TestMethod]
        public async Task TimeoutAfter_GenericTask_ShouldReturnResult_WhenTaskCompletesBeforeTimeout()
        {
            // Arrange
            var task = Task.FromResult(42);

            // Act
            int result = await task.TimeoutAfter(100);

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public async Task TimeoutAfter_GenericTask_ShouldThrowTimeoutException_WhenTaskExceedsTimeout()
        {
            // Arrange
            var task = Task.Delay(200).ContinueWith(_ => 42);

            // Act
            Func<Task> act = async () => await task.TimeoutAfter(10);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_ShouldThrowTimeoutException_ForZeroTimeout()
        {
            // Arrange
            var task = Task.Delay(50);

            // Act
            Func<Task> act = async () => await task.TimeoutAfter(0);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }

        [TestMethod]
        public async Task RunWithTimeout_Func_ShouldReturnResult_WhenWorkCompletesQuickly()
        {
            // Arrange
            Func<int> function = () => 7;

            // Act
            int result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 100,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(7);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFunc_ShouldReturnDefault_WhenTimeoutOccursInNonStrictMode()
        {
            // Arrange
            Func<CancellationToken, Task<int>> function = async token =>
            {
                await Task.Delay(100, token);
                return 7;
            };

            // Act
            int result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 10,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_Func_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int> function = () => throw new InvalidOperationException("boom");

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    CancellationToken.None,
                    milliseconds: 100,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFunc_ShouldHonorCancelledToken()
        {
            // Arrange
            using var source = new CancellationTokenSource();
            source.Cancel();
            Func<CancellationToken, Task<int>> function = async token =>
            {
                await Task.Delay(10, token);
                return 1;
            };

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    source.Token,
                    milliseconds: 100,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_GenericTask_InfiniteTimeout_ReturnsSameTask()
        {
            // Arrange
            var task = Task.FromResult("value");

            // Act
            var result = await task.TimeoutAfter(Timeout.Infinite);

            // Assert
            result.Should().Be("value");
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_CompletesBeforeTimeout()
        {
            // Arrange
            var task = Task.CompletedTask;

            // Act
            var resultTask = task.TimeoutAfter(100);
            await resultTask;

            // Assert
            resultTask.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_ThrowsTimeoutForZeroTimeout()
        {
            // Arrange
            var task = Task.Delay(200);

            // Act
            Func<Task> act = async () => await task.TimeoutAfter(0);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_InfiniteTimeout_ReturnsSameTask()
        {
            // Arrange
            var task = Task.CompletedTask;

            // Act
            var result = task.TimeoutAfter(Timeout.Infinite);
            await result;

            // Assert
            result.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult()
        {
            // Arrange
            var task = Task.FromResult(99);

            // Act
            var result = await task.TimeoutAfter(100, 3);

            // Assert
            result.Should().Be(99);
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully()
        {
            // Arrange
            var task = Task.CompletedTask;

            // Act
            var result = task.TimeoutAfter(100, 3);
            await result;

            // Assert
            result.IsCompleted.Should().BeTrue();
        }
    }
}
