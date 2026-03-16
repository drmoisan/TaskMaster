using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class LazyTry_Tests
    {
        [TestMethod]
        public void ValueFactory_WhenSuccessful_ReturnsComputedValue()
        {
            // Arrange
            var lazy = new LazyTry<string>(() => "ready");

            // Act
            var result = lazy.Value;

            // Assert
            result.Should().Be("ready");
            lazy.IsValueCreated.Should().BeTrue();
        }

        [TestMethod]
        public void ValueFactory_WhenItThrows_ReturnsDefaultValueWithoutThrowing()
        {
            // Arrange
            var lazy = new LazyTry<string>(() => throw new InvalidOperationException("boom"));

            // Act
            var result = lazy.Value;

            // Assert
            result.Should().BeNull();
            lazy.IsValueCreated.Should().BeTrue();
        }

        [TestMethod]
        public void ValueFactory_WhenItThrows_CachesDefaultValueInsteadOfRetrying()
        {
            // Arrange
            var factoryCalls = 0;
            var lazy = new LazyTry<int>(() =>
            {
                Interlocked.Increment(ref factoryCalls);
                throw new InvalidOperationException("boom");
            });

            // Act
            var first = lazy.Value;
            var second = lazy.Value;

            // Assert
            first.Should().Be(0);
            second.Should().Be(0);
            factoryCalls.Should().Be(1);
        }

        [TestMethod]
        public void Constructor_WithNullFactory_ReturnsDefaultValueWhenEvaluated()
        {
            // Arrange
            var lazy = new LazyTry<string>((Func<string>)null);

            // Act
            var result = lazy.Value;

            // Assert
            result.Should().BeNull();
            lazy.IsValueCreated.Should().BeTrue();
        }

        [TestMethod]
        public void Constructors_WithThreadSafetyOptions_StillEvaluateSuccessfully()
        {
            // Arrange
            var threadSafe = new LazyTry<int>(() => 7, isThreadSafe: true);
            var publicationOnly = new LazyTry<int>(() => 9, LazyThreadSafetyMode.PublicationOnly);

            // Act
            var threadSafeResult = threadSafe.Value;
            var publicationOnlyResult = publicationOnly.Value;

            // Assert
            threadSafeResult.Should().Be(7);
            publicationOnlyResult.Should().Be(9);
        }
    }
}