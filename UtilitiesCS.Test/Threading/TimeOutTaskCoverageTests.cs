using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class TimeOutTaskCoverageTests
    {
        [TestMethod]
        public async Task TimeoutAfter_WithCompletedGenericTask_ReturnsCompletedResult()
        {
            // Arrange
            Task<int> source = Task.FromResult(42);

            // Act
            Task<int> proxy = source.TimeoutAfter(millisecondsTimeout: 100);
            int result = await proxy;

            // Assert
            proxy.Should().BeSameAs(source);
            result.Should().Be(42);
        }

        [TestMethod]
        public async Task TimeoutAfter_WithZeroTimeout_FaultsGenericAndNonGenericTasks()
        {
            // Arrange
            var genericSource = new TaskCompletionSource<int>();
            var nonGenericSource = new TaskCompletionSource<bool>();

            // Act
            Func<Task> genericAct = async () => await genericSource.Task.TimeoutAfter(0);
            Func<Task> nonGenericAct = async () =>
                await ((Task)nonGenericSource.Task).TimeoutAfter(0);

            // Assert
            await genericAct.Should().ThrowAsync<TimeoutException>();
            await nonGenericAct.Should().ThrowAsync<TimeoutException>();
        }

        [TestMethod]
        public async Task TimeoutAfter_WithInfiniteTimeout_ReturnsOriginalTasks()
        {
            // Arrange
            Task<int> genericSource = Task.FromResult(7);
            Task nonGenericSource = Task.CompletedTask;

            // Act
            Task<int> genericProxy = genericSource.TimeoutAfter(Timeout.Infinite);
            Task nonGenericProxy = nonGenericSource.TimeoutAfter(Timeout.Infinite);

            // Assert
            genericProxy.Should().BeSameAs(genericSource);
            nonGenericProxy.Should().BeSameAs(nonGenericSource);
            (await genericProxy).Should().Be(7);
            await nonGenericProxy;
        }

        [TestMethod]
        public async Task TimeoutAfter_MarshalsFaultAndCancellationFromControlledTasks()
        {
            // Arrange
            var faultedSource = new TaskCompletionSource<int>();
            var canceledSource = new TaskCompletionSource<int>();
            Task<int> faultedProxy = faultedSource.Task.TimeoutAfter(Timeout.Infinite);
            Task<int> canceledProxy = canceledSource.Task.TimeoutAfter(Timeout.Infinite);

            // Act
            faultedSource.SetException(new InvalidOperationException("boom"));
            canceledSource.SetCanceled();

            // Assert
            Func<Task> faultedAct = async () => await faultedProxy;
            Func<Task> canceledAct = async () => await canceledProxy;
            await faultedAct.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
            await canceledAct.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task RunWithTimeout_WithImmediateCompletion_ReturnsResult()
        {
            // Arrange
            Func<int> function = () => 9;

            // Act
            int result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: Timeout.Infinite,
                maxAttempts: 0,
                strict: true
            );

            // Assert
            result.Should().Be(9);
        }

        [TestMethod]
        public async Task RunWithTimeout_WithPreCanceledToken_ThrowsOperationCanceledException()
        {
            // Arrange
            using var source = new CancellationTokenSource();
            source.Cancel();
            Func<int> function = () => 9;

            // Act
            Func<Task> action = async () =>
                await function.RunWithTimeout(
                    source.Token,
                    milliseconds: 100,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task RunWithTimeout_WithTaskCancellation_ReturnsDefaultAfterAttempts()
        {
            // Arrange
            var attempts = 0;
            Func<CancellationToken, Task<int>> function = _ =>
            {
                Interlocked.Increment(ref attempts);
                return Task.FromCanceled<int>(new CancellationToken(canceled: true));
            };

            // Act
            int result = await function.RunWithTimeout(
                CancellationToken.None,
                milliseconds: 100,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            result.Should().Be(0);
            attempts.Should().Be(2);
        }

        [TestMethod]
        public async Task RunWithTimeout_WithStrictException_PropagatesException()
        {
            // Arrange
            Func<int, int, int> function = (_, _) => throw new InvalidOperationException("boom");

            // Act
            Func<Task> action = async () =>
                await function.RunWithTimeout(
                    1,
                    2,
                    CancellationToken.None,
                    milliseconds: Timeout.Infinite,
                    maxAttempts: 0,
                    strict: true
                );

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        }

        [TestMethod]
        public async Task RunWithTimeout_WithNonStrictException_ReturnsDefault()
        {
            // Arrange
            Func<int, CancellationToken, Task<string>> function = (_, _) =>
                Task.FromException<string>(new InvalidOperationException("boom"));

            // Act
            string result = await function.RunWithTimeout(
                1,
                CancellationToken.None,
                milliseconds: 100,
                maxAttempts: 0,
                strict: false
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task RunWithTimeout_WithAsyncActionRetry_CompletesAfterCancellation()
        {
            // Arrange
            var attempts = 0;
            Func<int, int, CancellationToken, Task> action = (_, _, _) =>
            {
                if (Interlocked.Increment(ref attempts) == 1)
                {
                    return Task.FromCanceled(new CancellationToken(canceled: true));
                }

                return Task.CompletedTask;
            };

            // Act
            await action.RunWithTimeout(
                1,
                2,
                CancellationToken.None,
                milliseconds: 100,
                maxAttempts: 1,
                strict: true
            );

            // Assert
            attempts.Should().Be(2);
        }
    }
}
