using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class AsyncLazy_Tests
    {
        [TestMethod]
        public async Task Start_TriggersLazyInitializationOnlyWhenRequestedAsync()
        {
            // Arrange
            var factoryCalls = 0;
            var lazy = new AsyncLazy<int>(() =>
            {
                Interlocked.Increment(ref factoryCalls);
                return 42;
            });

            // Act
            factoryCalls.Should().Be(0);
            lazy.Start();
            var result = await lazy;

            // Assert
            result.Should().Be(42);
            factoryCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task AwaitingMultipleTimes_ReturnsSameReferenceAndInvokesFactoryOnceAsync()
        {
            // Arrange
            var factoryCalls = 0;
            var expected = new SampleReferenceType("alpha");
            var lazy = new AsyncLazy<SampleReferenceType>(() =>
            {
                Interlocked.Increment(ref factoryCalls);
                return expected;
            });

            // Act
            var first = await lazy;
            var second = await lazy;

            // Assert
            first.Should().BeSameAs(expected);
            second.Should().BeSameAs(expected);
            factoryCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task AwaitingFaultedFactory_PropagatesCachedExceptionAsync()
        {
            // Arrange
            var factoryCalls = 0;
            Func<int> factory = () =>
            {
                Interlocked.Increment(ref factoryCalls);
                throw new InvalidOperationException("boom");
            };
            var lazy = new AsyncLazy<int>(factory);

            // Act
            Func<Task> firstAwait = async () => _ = await lazy;
            Func<Task> secondAwait = async () => _ = await lazy;

            // Assert
            await firstAwait.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
            await secondAwait.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
            factoryCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task Constructors_SupportValueTypesAndAsyncFactoriesAsync()
        {
            // Arrange
            var intLazy = new AsyncLazy<int>(() => 13);
            var stringLazy = new AsyncLazy<string>(async () =>
            {
                await Task.Delay(10);
                return "value";
            });

            // Act
            var intResult = await intLazy;
            var stringResult = await stringLazy;

            // Assert
            intResult.Should().Be(13);
            stringResult.Should().Be("value");
        }

        [TestMethod]
        public async Task ConcurrentAwaiters_ShareSingleInitializationAsync()
        {
            // Arrange
            var factoryCalls = 0;
            var lazy = new AsyncLazy<int>(async () =>
            {
                Interlocked.Increment(ref factoryCalls);
                await Task.Delay(25);
                return 99;
            });

            var tasks = Enumerable.Range(0, 5).Select(async _ => await lazy).ToArray();

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().OnlyContain(result => result == 99);
            factoryCalls.Should().Be(1);
        }

        [TestMethod]
        public async Task Constructor_SyncFactory_WithCancellationToken_ProducesValueAsync()
        {
            // Arrange
            var lazy = new AsyncLazy<int>(() => 77, CancellationToken.None);

            // Act
            var result = await lazy;

            // Assert
            result.Should().Be(77);
        }

        [TestMethod]
        public async Task Constructor_AsyncFactory_WithCancellationToken_ProducesValueAsync()
        {
            // Arrange
            var lazy = new AsyncLazy<string>(
                async () =>
                {
                    await Task.Delay(5);
                    return "hello";
                },
                CancellationToken.None
            );

            // Act
            var result = await lazy;

            // Assert
            result.Should().Be("hello");
        }

        [TestMethod]
        public async Task Constructor_SyncFactory_WithCancelledToken_ThrowsCancelledAsync()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            var lazy = new AsyncLazy<int>(() => 1, cts.Token);

            // Act
            Func<Task> act = async () => await lazy;

            // Assert
            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        public async Task GetAwaiter_ReturnsTaskAwaiter_ThatCompletesSuccessfully()
        {
            // Arrange
            var lazy = new AsyncLazy<int>(() => 5);

            // Act
            var awaiter = lazy.GetAwaiter();
            var result = await lazy;

            // Assert
            result.Should().Be(5);
            awaiter.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public async Task AsyncLazyPropertyCachedValues_ReturnsCachedValueFromInternalSampleAsync()
        {
            // Arrange
            var type = GetUtilitiesType("UtilitiesCS.AsyncLazyPropertyCachedValues");
            var instance = Activator.CreateInstance(type);
            var property = type.GetProperty(
                "MyProperty",
                BindingFlags.Instance | BindingFlags.Public
            );

            // Act
            var result = await (dynamic)property.GetValue(instance);

            // Assert
            ((int)result)
                .Should()
                .Be(13);
        }

        [TestMethod]
        public async Task AsyncLazyUsage_UseResource_CompletesForInternalSampleAsync()
        {
            // Arrange
            var type = GetUtilitiesType("UtilitiesCS.AsyncLazyUsage");
            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("UseResource", BindingFlags.Instance | BindingFlags.Public);

            // Act
            var task = (Task)method.Invoke(instance, null);

            // Assert
            await task;
        }

        [TestMethod]
        public async Task DataBoundValues_InitializeAsync_SetsPropertyAndRaisesChangeNotificationAsync()
        {
            // Arrange
            var type = GetUtilitiesType("UtilitiesCS.DataBoundValues");
            var instance = Activator.CreateInstance(type);
            var propertyChanged = type.GetEvent("PropertyChanged");
            var myProperty = type.GetProperty(
                "MyProperty",
                BindingFlags.Instance | BindingFlags.Public
            );
            var initializeAsync = type.GetMethod(
                "InitializeAsync",
                BindingFlags.Instance | BindingFlags.Public
            );
            string changedProperty = null;
            PropertyChangedEventHandler handler = (sender, args) =>
                changedProperty = args.PropertyName;
            propertyChanged.AddEventHandler(instance, handler);

            // Act
            await (Task)initializeAsync.Invoke(instance, null);

            // Assert
            ((int?)myProperty.GetValue(instance))
                .Should()
                .Be(13);
            changedProperty.Should().Be("MyProperty");
        }

        private static Type GetUtilitiesType(string fullName)
        {
            return typeof(AsyncLazy<int>).Assembly.GetType(fullName, throwOnError: true);
        }

        private sealed class SampleReferenceType
        {
            public SampleReferenceType(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
