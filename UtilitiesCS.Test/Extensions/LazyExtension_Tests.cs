using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Extensions.Lazy;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class LazyExtension_Tests
    {
        [TestMethod]
        public void ToLazy_AndAsFunc_ForReferenceTypes_PreserveDeferredAndRealizedValues()
        {
            // Arrange
            var value = new SampleReferenceType("alpha");

            // Act
            var lazy = value.ToLazy();
            var asFunc = value.AsFunc();

            // Assert
            lazy.IsValueCreated.Should().BeFalse();
            lazy.Value.Should().BeSameAs(value);
            lazy.IsValueCreated.Should().BeTrue();
            asFunc().Should().BeSameAs(value);
        }

        [TestMethod]
        public void ToLazyValue_AndToLazyTryValue_ForValueTypes_ReturnWrappedValues()
        {
            // Arrange
            const int number = 42;

            // Act
            var lazyValue = number.ToLazyValue();
            var lazyTryValue = number.ToLazyTryValue();

            // Assert
            lazyValue.IsValueCreated.Should().BeFalse();
            lazyValue.Value.Should().Be(42);
            lazyTryValue.IsValueCreated.Should().BeFalse();
            lazyTryValue.Value.Should().Be(42);
        }

        [TestMethod]
        public void ToLazyTry_ForReferenceTypes_ReturnsOriginalValueIncludingNullReference()
        {
            // Arrange
            SampleReferenceType value = new SampleReferenceType("beta");
            SampleReferenceType nullValue = null;

            // Act
            var lazyTry = value.ToLazyTry();
            var nullLazyTry = nullValue.ToLazyTry();

            // Assert
            lazyTry.Value.Should().BeSameAs(value);
            nullLazyTry.Value.Should().BeNull();
        }

        [TestMethod]
        public async Task ToAsyncLazy_ForReferenceTypes_ReturnsOriginalValueIncludingNullReferenceAsync()
        {
            // Arrange
            SampleReferenceType value = new SampleReferenceType("gamma");
            SampleReferenceType nullValue = null;

            // Act
            var lazy = value.ToAsyncLazy();
            var nullLazy = nullValue.ToAsyncLazy();
            var result = await lazy;
            var nullResult = await nullLazy;

            // Assert
            result.Should().BeSameAs(value);
            nullResult.Should().BeNull();
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
