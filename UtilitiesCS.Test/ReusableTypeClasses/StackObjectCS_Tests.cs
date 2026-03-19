using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class StackObjectCS_Tests
    {
        [TestMethod]
        public void PushPopAndPeek_FollowStackOrderFromFrontOfList()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<string>();

            // Act
            stack.Push("first");
            stack.Push("second");
            var peeked = stack.Peek();
            var popped = stack.Pop();

            // Assert
            peeked.Should().Be("second");
            popped.Should().Be("second");
            stack.Peek().Should().Be("first");
            stack.Count.Should().Be(1);
        }

        [TestMethod]
        public void PeekAndPop_WithExplicitIndex_TargetRequestedElement()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<int>(new[] { 3, 2, 1 });

            // Act
            var peeked = stack.Peek(1);
            var popped = stack.Pop(2);

            // Assert
            peeked.Should().Be(2);
            popped.Should().Be(1);
            stack.ToArray().Should().Equal(3, 2);
        }

        [TestMethod]
        public void PeekAndPop_WhenEmpty_ThrowInvalidOperationException()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<int>();

            // Act
            Action peekAct = () => stack.Peek();
            Action popAct = () => stack.Pop();

            // Assert
            peekAct
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Stack is empty. No element to peek at");
            popAct
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Stack is empty. Cannot pop an element");
        }

        [TestMethod]
        public void PeekAndPop_WithOutOfRangeIndex_ThrowIndexOutOfRangeException()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<int>(new[] { 5 });

            // Act
            Action peekAct = () => stack.Peek(1);
            Action popAct = () => stack.Pop(1);

            // Assert
            peekAct
                .Should()
                .Throw<IndexOutOfRangeException>()
                .WithMessage("Index 1 out of range. Stack only has 1 elements.");
            popAct
                .Should()
                .Throw<IndexOutOfRangeException>()
                .WithMessage("Index 1 out of range. Stack only has 1 elements.");
        }

        [TestMethod]
        public void TryPeekAndTryPop_ReturnFalseAndDefaultWhenStackIsEmpty()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<int>();

            // Act
            var peeked = stack.TryPeek(out var peekResult);
            var popped = stack.TryPop(out var popResult);

            // Assert
            peeked.Should().BeFalse();
            popped.Should().BeFalse();
            peekResult.Should().Be(0);
            popResult.Should().Be(0);
        }

        [TestMethod]
        public void AddRemoveClearContainsAndEnumeration_ExposeCollectionBehavior()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<int>();

            // Act
            stack.Add(1);
            stack.Add(2);
            stack.Add(3);
            var removed = stack.Remove(2);
            var enumerated = stack.ToArray();
            var containsOne = stack.Contains(1);
            stack.Clear();

            // Assert
            removed.Should().BeTrue();
            enumerated.Should().Equal(3, 1);
            containsOne.Should().BeTrue();
            stack.Count.Should().Be(0);
        }

        [TestMethod]
        public void ToArrayToListAndCopyTo_SupportNormalAndReversedOrdering()
        {
            // Arrange
            var stack = new UtilitiesCS.StackObjectCS<int>(new[] { 3, 2, 1 });
            var destination = new int[5];

            // Act
            var normalArray = stack.ToArray();
            var reversedArray = stack.ToArray(reverse: true);
            var normalList = stack.ToList();
            var reversedList = stack.ToList(reverse: true);
            stack.CopyTo(destination, 1);

            // Assert
            normalArray.Should().Equal(3, 2, 1);
            reversedArray.Should().Equal(1, 2, 3);
            normalList.Should().Equal(3, 2, 1);
            reversedList.Should().Equal(1, 2, 3);
            destination.Should().Equal(0, 3, 2, 1, 0);
            stack.Should().Equal(3, 2, 1);
        }
    }
}
