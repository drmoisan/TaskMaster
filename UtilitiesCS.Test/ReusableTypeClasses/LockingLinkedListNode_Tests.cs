using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class LockingLinkedListNode_Tests
    {
        [TestMethod]
        public void Constructor_WithValue_PreservesValue()
        {
            // Act
            var node = new LockingLinkedListNode<int>(42);

            // Assert
            node.Value.Should().Be(42);
            node.List.Should().BeNull();
            node.Next.Should().BeNull();
            node.Previous.Should().BeNull();
        }

        [TestMethod]
        public void Value_Setter_UpdatesStoredValue()
        {
            // Arrange
            var node = new LockingLinkedListNode<string>("before");

            // Act
            node.Value = "after";

            // Assert
            node.Value.Should().Be("after");
        }

        [TestMethod]
        public void Constructor_WithNullValue_PreservesNull()
        {
            // Act
            var node = new LockingLinkedListNode<string>(null);

            // Assert
            node.Value.Should().BeNull();
            node.Next.Should().BeNull();
            node.Previous.Should().BeNull();
        }

        [TestMethod]
        public void NextAndPrevious_FromWrappedNode_ExposeNeighborNodesAndParentList()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });

            // Act
            var middle = list.Find(2);

            // Assert
            middle.Should().NotBeNull();
            middle.List.Should().BeSameAs(list);
            middle.Previous.Should().NotBeNull();
            middle.Previous.Value.Should().Be(1);
            middle.Previous.List.Should().BeSameAs(list);
            middle.Next.Should().NotBeNull();
            middle.Next.Value.Should().Be(3);
            middle.Next.List.Should().BeSameAs(list);
        }

        [TestMethod]
        public void EdgeNodes_HaveNullMissingNeighborReference()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 7, 8 });

            // Act
            var first = list.First;
            var last = list.Last;

            // Assert
            first.Previous.Should().BeNull();
            first.Next.Value.Should().Be(8);
            last.Next.Should().BeNull();
            last.Previous.Value.Should().Be(7);
        }
    }
}
