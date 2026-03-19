using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoStack_Tests
    {
        [TestMethod]
        public void PushPeekAndCount_FollowLifoOrder()
        {
            // Arrange
            var stack = new ScoStack<int>();

            // Act
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);

            // Assert
            stack.Count.Should().Be(3);
            stack.Peek().Should().Be(3);
            stack.Peek(1).Should().Be(2);
        }

        [TestMethod]
        public void PopAndTryPop_RemoveItemsFromTheFront()
        {
            // Arrange
            var stack = new ScoStack<string>(new[] { "top", "middle", "bottom" });

            // Act
            var popped = stack.Pop();
            var tryPopped = stack.TryPop(out var second);
            var remaining = stack.Single();

            // Assert
            popped.Should().Be("top");
            tryPopped.Should().BeTrue();
            second.Should().Be("middle");
            remaining.Should().Be("bottom");
            stack.Count.Should().Be(1);
        }

        [TestMethod]
        public void PopAndPeekByIndex_WorkForNonZeroIndices()
        {
            // Arrange
            var stack = new ScoStack<int>(new[] { 10, 20, 30, 40 });

            // Act
            var peeked = stack.Peek(2);
            var popped = stack.Pop(1);
            var remaining = stack.AsEnumerable().ToArray();

            // Assert
            peeked.Should().Be(30);
            popped.Should().Be(20);
            remaining.Should().Equal(10, 30, 40);
        }

        [TestMethod]
        public void EmptyStack_ThrowsForPeekAndPopAndReturnsFalseForTryPop()
        {
            // Arrange
            var stack = new ScoStack<int>();

            // Act
            Action peek = () => stack.Peek();
            Action pop = () => stack.Pop();
            var tryPopped = stack.TryPop(out var value);

            // Assert
            peek.Should().Throw<InvalidOperationException>();
            pop.Should().Throw<InvalidOperationException>();
            tryPopped.Should().BeFalse();
            value.Should().Be(0);
        }

        [TestMethod]
        public async Task ConcurrentPushes_PreserveAllItems()
        {
            // Arrange
            var stack = new ScoStack<int>();
            var items = Enumerable.Range(1, 64).ToArray();

            // Act
            await Task.WhenAll(items.Select(item => Task.Run(() => stack.Push(item))));
            var ordered = stack.OrderBy(value => value).ToArray();

            // Assert
            stack.Count.Should().Be(items.Length);
            ordered.Should().Equal(items);
        }
    }
}
