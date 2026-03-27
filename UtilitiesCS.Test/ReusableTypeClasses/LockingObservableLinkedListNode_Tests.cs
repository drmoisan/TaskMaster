using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class LockingObservableLinkedListNode_Tests
    {
        [TestMethod]
        public void Constructor_WithValue_SetsItem()
        {
            // Arrange & Act
            var node = new LockingObservableLinkedListNode<int>(42);

            // Assert
            node.Value.Should().Be(42);
        }

        [TestMethod]
        public void Value_SetNewValue_Updates()
        {
            // Arrange
            var node = new LockingObservableLinkedListNode<string>("original");

            // Act
            node.Value = "updated";

            // Assert
            node.Value.Should().Be("updated");
        }

        [TestMethod]
        public void List_OnStandaloneNode_ReturnsNull()
        {
            // Arrange
            var node = new LockingObservableLinkedListNode<int>(1);

            // Act & Assert
            node.List.Should().BeNull();
        }

        [TestMethod]
        public void Next_OnStandaloneNode_ReturnsNull()
        {
            // Arrange
            var node = new LockingObservableLinkedListNode<int>(1);

            // Act & Assert
            node.Next.Should().BeNull();
        }

        [TestMethod]
        public void Previous_OnStandaloneNode_ReturnsNull()
        {
            // Arrange
            var node = new LockingObservableLinkedListNode<int>(1);

            // Act & Assert
            node.Previous.Should().BeNull();
        }

        [TestMethod]
        public void ListIntegration_AddedNode_HasCorrectNextPrevious()
        {
            // Arrange
            var list = new LockingObservableLinkedList<int>();
            list.AddLast(1);
            list.AddLast(2);
            list.AddLast(3);

            // Act
            var first = list.First;
            var second = first.Next;
            var third = second.Next;

            // Assert
            first.Value.Should().Be(1);
            second.Value.Should().Be(2);
            third.Value.Should().Be(3);
            third.Next.Should().BeNull();
            first.Previous.Should().BeNull();
        }

        [TestMethod]
        public void ListIntegration_Previous_NavigatesBackward()
        {
            // Arrange
            var list = new LockingObservableLinkedList<int>();
            list.AddLast(10);
            list.AddLast(20);

            // Act
            var last = list.Last;
            var first = last.Previous;

            // Assert
            first.Value.Should().Be(10);
            first.Previous.Should().BeNull();
        }
    }
}
