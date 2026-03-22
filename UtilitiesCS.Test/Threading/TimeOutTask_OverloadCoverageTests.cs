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
        public async Task RunWithTimeout_Func_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int> function = () =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    throw new TaskCanceledException();
                }

                return 17;
            };

            // Act
            var result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be(17);
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_Func_ShouldReturnDefault_WhenStrictIsFalseAndExceptionIsThrown()
        {
            // Arrange
            Func<int> function = () => throw new InvalidOperationException("boom");

            // Act
            var result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFunc_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<CancellationToken, Task<int>> function = token =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    return Task.FromCanceled<int>(new CancellationToken(true));
                }

                return Task.FromResult(23);
            };

            // Act
            var result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be(23);
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFunc_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<CancellationToken, Task<int>> function = token =>
                Task.FromException<int>(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries()
        {
            // Arrange
            Func<int, string> function = value => throw new TimeoutException("timeout");

            // Act
            var result = await function.RunWithTimeout(
                42,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1TResult_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, string> function = value => throw new InvalidOperationException("boom");

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    42,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries()
        {
            // Arrange
            Func<int, CancellationToken, Task<string>> function = (value, token) =>
                Task.FromException<string>(new TimeoutException("timeout"));

            // Act
            var result = await function.RunWithTimeout(
                7,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, CancellationToken, Task<string>> function = (value, token) =>
                Task.FromException<string>(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    7,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1T2TResult_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int, int, int> function = (left, right) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    throw new TaskCanceledException();
                }

                return left + right;
            };

            // Act
            var result = await function.RunWithTimeout(
                3,
                4,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be(7);
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1T2TResult_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, int, int> function = (left, right) =>
                throw new InvalidOperationException("boom");

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    3,
                    4,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2_ShouldReturnDefault_WhenTaskIsCanceledWithoutRetries()
        {
            // Arrange
            Func<int, int, CancellationToken, Task<int>> function = (left, right, token) =>
                Task.FromCanceled<int>(new CancellationToken(true));

            // Act
            var result = await function.RunWithTimeout(
                3,
                5,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, int, CancellationToken, Task<int>> function = (left, right, token) =>
                Task.FromException<int>(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    3,
                    5,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncActionT1T2_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int, int, CancellationToken, Task> function = (left, right, token) =>
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
                    3,
                    5,
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
        public async Task RunWithTimeout_AsyncActionT1T2_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, int, CancellationToken, Task> function = (left, right, token) =>
                Task.FromException(new InvalidOperationException("boom"));

            // Act
            Func<Task> act = async () =>
                await function.RunWithTimeout(
                    3,
                    5,
                    CancellationToken.None,
                    milliseconds: 200,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1T2T3TResult_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int, int, int, int> function = (first, second, third) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    throw new TaskCanceledException();
                }

                return first + second + third;
            };

            // Act
            var result = await function.RunWithTimeout(
                10,
                20,
                30,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be(60);
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1T2T3TResult_ShouldPropagateExceptions_WhenStrictModeIsEnabled()
        {
            // Arrange
            Func<int, int, int, int> function = (first, second, third) =>
                throw new InvalidOperationException("boom");

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
    }
}
