using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class LockingLinkedList_Tests
    {
        [TestMethod]
        public void Constructor_WithEmptyList_ExposesZeroCountAndNoEndpoints()
        {
            // Arrange
            var list = new LockingLinkedList<int>();

            // Assert
            list.Count.Should().Be(0);
            list.First.Should().BeNull();
            list.Last.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithSingleSeed_ExposesSameHeadAndTailValue()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 42 });

            // Assert
            list.Count.Should().Be(1);
            list.First.Should().NotBeNull();
            list.Last.Should().NotBeNull();
            list.First.Value.Should().Be(42);
            list.Last.Value.Should().Be(42);
        }

        [TestMethod]
        public void AddOperations_MaintainExpectedOrderAndEndpoints()
        {
            // Arrange
            var list = new LockingLinkedList<int>();
            list.AddFirst(2);
            list.AddLast(4);
            list.AddBefore(list.Last, 3);
            list.AddAfter(list.First, 1);

            // Act
            var values = list.ToArray();

            // Assert
            values.Should().Equal(2, 1, 3, 4);
            list.First.Value.Should().Be(2);
            list.Last.Value.Should().Be(4);
            list.Count.Should().Be(4);
        }

        [TestMethod]
        public void AddLast_WithLockedAction_InvokesActionWhileItemExistsInList()
        {
            // Arrange
            var list = new LockingLinkedList<string>();
            string observed = null;
            int observedCount = -1;

            // Act
            list.AddLast(
                "alpha",
                item =>
                {
                    observed = item;
                    observedCount = list.Count;
                }
            );

            // Assert
            observed.Should().Be("alpha");
            observedCount.Should().Be(1);
            list.Should().ContainSingle().Which.Should().Be("alpha");
        }

        [TestMethod]
        public void FindMethods_ReturnMatchingNodes()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3, 2, 4 });

            // Act
            var firstMatch = list.Find(2);
            var predicateMatch = list.Find(value => value > 3);
            var lastMatch = list.FindLast(2);

            // Assert
            firstMatch.Should().NotBeNull();
            firstMatch.Value.Should().Be(2);
            firstMatch.Previous.Value.Should().Be(1);
            predicateMatch.Should().NotBeNull();
            predicateMatch.Value.Should().Be(4);
            lastMatch.Should().NotBeNull();
            lastMatch.Value.Should().Be(2);
            lastMatch.Next.Value.Should().Be(4);
        }

        [TestMethod]
        public void Clear_WithUnwireAction_InvokesActionForEachItemAndEmptiesList()
        {
            // Arrange
            var list = new LockingLinkedList<string>(new[] { "a", "b", "c" });
            var unwired = new List<string>();

            // Act
            list.Clear(unwired.Add);

            // Assert
            unwired.Should().Equal("a", "b", "c");
            list.Should().BeEmpty();
            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void RemoveOperations_UpdateListContents()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3, 4, 5 });
            var middleNode = list.Find(3);

            // Act
            var removedByValue = list.Remove(1);
            list.Remove(middleNode);
            list.Remove(value => value % 2 == 0);
            list.RemoveLast();

            // Assert
            removedByValue.Should().BeTrue();
            list.Should().ContainSingle().Which.Should().Be(4);
            list.First.Value.Should().Be(4);
            list.Last.Value.Should().Be(4);
        }

        [TestMethod]
        public void TakeMethods_RemoveAndReturnExpectedItems()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3, 4, 5 });
            var tryEmptyList = new LockingLinkedList<int>();
            var tryManyList = new LockingLinkedList<int>(new[] { 4, 5 });
            var lastList = new LockingLinkedList<int>(new[] { 9, 10 });

            // Act
            var first = list.TakeFirst();
            var firstMany = list.TakeFirst(2);
            var tryNone = tryEmptyList.TryTakeFirst();
            var tryTooMany = tryManyList.TryTakeFirst(10);
            var last = lastList.TakeLast();

            // Assert
            first.Should().Be(1);
            firstMany.Should().Equal(2, 3);
            tryNone.Should().Be(0);
            tryTooMany.Should().Equal(4, 5);
            last.Should().Be(10);
            list.Should().Equal(4, 5);
            tryManyList.Should().BeEmpty();
            lastList.Should().ContainSingle().Which.Should().Be(9);
        }

        [TestMethod]
        public void CopyTo_AndContains_ReflectCurrentState()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 7, 8, 9 });
            var destination = new int[5];

            // Act
            list.CopyTo(destination, 1);

            // Assert
            list.Contains(8).Should().BeTrue();
            list.Contains(99).Should().BeFalse();
            destination.Should().Equal(0, 7, 8, 9, 0);
        }

        [TestMethod]
        public void ConcurrentAddLast_FromMultipleTasks_PreservesAllItems()
        {
            // Arrange
            var list = new LockingLinkedList<int>();
            var values = Enumerable.Range(1, 50).ToArray();

            // Act
            Parallel.ForEach(values, value => list.AddLast(value));

            // Assert
            list.Count.Should().Be(values.Length);
            list.OrderBy(value => value).Should().Equal(values);
        }

        [TestMethod]
        public void ConcurrentRemove_ByUniqueValue_RemovesAllItems()
        {
            // Arrange
            var values = Enumerable.Range(1, 40).ToArray();
            var list = new LockingLinkedList<int>(values);

            // Act
            Parallel.ForEach(values, value => list.Remove(value));

            // Assert
            list.Should().BeEmpty();
            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void Remove_WithUnwireAction_InvokesActionBeforeRemoval()
        {
            // Arrange
            var list = new LockingLinkedList<string>(new[] { "a", "b" });
            string unwired = null;

            // Act
            var removed = list.Remove("a", item => unwired = item);

            // Assert
            removed.Should().BeTrue();
            unwired.Should().Be("a");
            list.Should().ContainSingle().Which.Should().Be("b");
        }

        [TestMethod]
        public void Remove_LockingLinkedListNode_RemovesSpecificNode()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var node = list.Find(2);

            // Act
            list.Remove(node);

            // Assert
            list.Should().Equal(1, 3);
        }

        [TestMethod]
        public void RemoveFirst_RemovesHeadElement()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 10, 20, 30 });

            // Act
            list.RemoveFirst();

            // Assert
            list.Should().Equal(20, 30);
        }

        [TestMethod]
        public void Clear_WithoutAction_EmptiesList()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });

            // Act
            list.Clear();

            // Assert
            list.Should().BeEmpty();
            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void TakeFirst_N_ThrowsWhenNExceedsCount()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1 });

            // Act
            Action act = () => list.TakeFirst(5);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TryTakeFirst_N_ReturnsNull_WhenNIsZeroOrNegative()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2 });

            // Act
            var result = list.TryTakeFirst(0);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void TakeLast_N_RemovesAndReturnsLastNItems()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3, 4 });

            // Act
            var taken = list.TakeLast(2);

            // Assert
            taken.Should().Equal(3, 4);
            list.Should().Equal(1, 2);
        }

        [TestMethod]
        public void AddBefore_WithLinkedListNode_InsertsBeforeNode()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 3 });
            var baseNode = ((LinkedList<int>)list).Find(3);

            // Act
            list.AddBefore(baseNode, 2);

            // Assert
            list.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void AddAfter_WithLinkedListNode_InsertsAfterNode()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 3 });
            var baseNode = ((LinkedList<int>)list).Find(1);

            // Act
            list.AddAfter(baseNode, 2);

            // Assert
            list.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void AddAfter_WithLockingNode_InsertsAfterNode()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 3 });
            var lockingNode = list.Find(1);

            // Act
            list.AddAfter(lockingNode, 2);

            // Assert
            list.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void MoveAfter_MovesNodeAfterTarget()
        {
            // Arrange – list: 1, 2, 3
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var nodeToMove = list.Find(1);
            var target = list.Find(2);

            // Act
            nodeToMove.MoveAfter(target);

            // Assert – should become 2, 1, 3
            list.Should().Equal(2, 1, 3);
        }

        [TestMethod]
        public void MoveBefore_MovesNodeBeforeTarget()
        {
            // Arrange – list: 1, 2, 3
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var nodeToMove = list.Find(3);
            var target = list.Find(2);

            // Act
            nodeToMove.MoveBefore(target);

            // Assert – should become 1, 3, 2
            list.Should().Equal(1, 3, 2);
        }

        [TestMethod]
        public void MoveUp_MovesNodeOnePositionEarlier()
        {
            // Arrange – list: 1, 2, 3
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var node = list.Find(3);

            // Act
            node.MoveUp();

            // Assert – should become 1, 3, 2
            list.Should().Equal(1, 3, 2);
        }

        [TestMethod]
        public void MoveUp_AtHead_DoesNothing()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var head = list.First;

            // Act
            head.MoveUp();

            // Assert
            list.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void MoveDown_MovesNodeOnePositionLater()
        {
            // Arrange – list: 1, 2, 3
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var node = list.Find(1);

            // Act
            node.MoveDown();

            // Assert – should become 2, 1, 3
            list.Should().Equal(2, 1, 3);
        }

        [TestMethod]
        public void MoveDown_AtTail_DoesNothing()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var tail = list.Last;

            // Act
            tail.MoveDown();

            // Assert
            list.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void Find_Predicate_ReturnsNull_WhenNoMatch()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });

            // Act
            var result = list.Find(v => v > 100);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FindLast_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2 });

            // Act
            var result = list.FindLast(99);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Remove_LinkedListNode_RemovesNode()
        {
            // Arrange
            var list = new LockingLinkedList<int>(new[] { 1, 2, 3 });
            var baseNode = ((LinkedList<int>)list).Find(2);

            // Act
            list.Remove(baseNode);

            // Assert
            list.Should().Equal(1, 3);
        }
    }
}
