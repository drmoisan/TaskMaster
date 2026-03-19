using System;
using System.Linq;
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
