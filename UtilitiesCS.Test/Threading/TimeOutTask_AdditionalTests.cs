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
        public async Task TimeoutAfter_GenericTask_ShouldPropagateFaultedSourceException_WhenSourceFaultsLater()
        {
            // Arrange
            var source = new TaskCompletionSource<int>();
            var proxy = source.Task.TimeoutAfter(100);

            // Act
            source.SetException(new InvalidOperationException("boom"));

            // Assert
            Func<Task> act = async () => await proxy;
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task TimeoutAfter_GenericTask_ShouldPropagateCancellation_WhenSourceCancelsLater()
        {
            // Arrange
            var source = new TaskCompletionSource<int>();
            var proxy = source.Task.TimeoutAfter(100);

            // Act
            source.SetCanceled();

            // Assert
            Func<Task> act = async () => await proxy;
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_ShouldPropagateFaultedSourceException_WhenSourceFaultsLater()
        {
            // Arrange
            var source = new TaskCompletionSource<bool>();
            var proxy = ((Task)source.Task).TimeoutAfter(100);

            // Act
            source.SetException(new InvalidOperationException("boom"));

            // Assert
            Func<Task> act = async () => await proxy;
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task TimeoutAfter_NonGenericTask_ShouldPropagateCancellation_WhenSourceCancelsLater()
        {
            // Arrange
            var source = new TaskCompletionSource<bool>();
            var proxy = ((Task)source.Task).TimeoutAfter(100);

            // Act
            source.SetCanceled();

            // Assert
            Func<Task> act = async () => await proxy;
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task RunWithTimeout_Func_ShouldReturnDefault_WhenTaskIsCanceledWithoutRetries()
        {
            // Arrange
            Func<int> function = () => throw new TaskCanceledException();

            // Act
            var result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFunc_ShouldReturnDefault_WhenTaskIsCanceledWithoutRetries()
        {
            // Arrange
            Func<CancellationToken, Task<int>> function = token =>
                Task.FromCanceled<int>(new CancellationToken(true));

            // Act
            var result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFunc_ShouldReturnDefault_WhenStrictIsFalseAndExceptionIsThrown()
        {
            // Arrange
            Func<CancellationToken, Task<int>> function = token =>
                Task.FromException<int>(new InvalidOperationException("boom"));

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
        public async Task RunWithTimeout_FuncT1TResult_ShouldReturnResult()
        {
            // Arrange
            Func<int, string> function = arg => $"result-{arg}";

            // Act
            var result = await function.RunWithTimeout(
                42,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be("result-42");
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException()
        {
            // Arrange
            int attempts = 0;
            Func<int, string> function = arg =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    throw new TimeoutException("timeout");
                }

                return $"result-{arg}";
            };

            // Act
            var result = await function.RunWithTimeout(
                42,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be("result-42");
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenStrictIsFalseAndExceptionIsThrown()
        {
            // Arrange
            Func<int, string> function = arg => throw new InvalidOperationException("boom");

            // Act
            var result = await function.RunWithTimeout(
                42,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1T2TResult_ShouldReturnResult()
        {
            // Arrange
            Func<int, int, int> function = (a, b) => a + b;

            // Act
            var result = await function.RunWithTimeout(
                3,
                4,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(7);
        }

        [TestMethod]
        public async Task RunWithTimeout_FuncT1T2TResult_ShouldReturnDefault_WhenTaskIsCanceledWithoutRetries()
        {
            // Arrange
            Func<int, int, int> function = (a, b) => throw new TaskCanceledException();

            // Act
            var result = await function.RunWithTimeout(
                3,
                4,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1_ShouldReturnResult()
        {
            // Arrange
            Func<int, CancellationToken, Task<string>> function = async (arg, ct) =>
            {
                await Task.Delay(5, ct);
                return $"async-{arg}";
            };

            // Act
            var result = await function.RunWithTimeout(
                7,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be("async-7");
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1_ShouldRetryAfterTimeoutException()
        {
            // Arrange
            int attempts = 0;
            Func<int, CancellationToken, Task<string>> function = (arg, ct) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    return Task.FromException<string>(new TimeoutException("timeout"));
                }

                return Task.FromResult($"async-{arg}");
            };

            // Act
            var result = await function.RunWithTimeout(
                7,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be("async-7");
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1_ShouldReturnDefault_WhenTaskIsCanceled()
        {
            // Arrange
            int attempts = 0;
            Func<int, CancellationToken, Task<string>> function = (arg, ct) =>
            {
                Interlocked.Increment(ref attempts);
                return Task.FromCanceled<string>(new CancellationToken(true));
            };

            // Act
            var result = await function.RunWithTimeout(
                7,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().BeNull();
            attempts.Should().Be(1);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1_ShouldReturnDefault_WhenStrictIsFalseAndExceptionIsThrown()
        {
            // Arrange
            Func<int, CancellationToken, Task<string>> function = (arg, ct) =>
                Task.FromException<string>(new InvalidOperationException("boom"));

            // Act
            var result = await function.RunWithTimeout(
                7,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2_ShouldReturnResult()
        {
            // Arrange
            Func<int, int, CancellationToken, Task<int>> function = async (a, b, ct) =>
            {
                await Task.Delay(5, ct);
                return a * b;
            };

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
            result.Should().Be(15);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int, int, CancellationToken, Task<int>> function = (a, b, ct) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    return Task.FromCanceled<int>(new CancellationToken(true));
                }

                return Task.FromResult(a * b);
            };

            // Act
            var result = await function.RunWithTimeout(
                3,
                5,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be(15);
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2_ShouldReturnDefault_WhenStrictIsFalseAndExceptionIsThrown()
        {
            // Arrange
            Func<int, int, CancellationToken, Task<int>> function = (a, b, ct) =>
                Task.FromException<int>(new InvalidOperationException("boom"));

            // Act
            var result = await function.RunWithTimeout(
                3,
                5,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2T3_ShouldReturnResult()
        {
            // Arrange
            Func<int, int, int, CancellationToken, Task<int>> function = async (a, b, c, ct) =>
            {
                await Task.Delay(5, ct);
                return a + b + c;
            };

            // Act
            var result = await function.RunWithTimeout(
                10,
                20,
                30,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(60);
        }

        [TestMethod]
        public async Task RunWithTimeout_AsyncFuncT1T2T3_ShouldRetryAfterTaskCanceledException()
        {
            // Arrange
            int attempts = 0;
            Func<int, int, int, CancellationToken, Task<int>> function = (a, b, c, ct) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    return Task.FromCanceled<int>(new CancellationToken(true));
                }

                return Task.FromResult(a + b + c);
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
        public async Task RunWithTimeout_AsyncFuncT1T2T3_ShouldReturnDefault_WhenStrictIsFalseAndExceptionIsThrown()
        {
            // Arrange
            Func<int, int, int, CancellationToken, Task<int>> function = (a, b, c, ct) =>
                Task.FromException<int>(new InvalidOperationException("boom"));

            // Act
            var result = await function.RunWithTimeout(
                10,
                20,
                30,
                CancellationToken.None,
                milliseconds: 200,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().Be(0);
        }
    }
}
