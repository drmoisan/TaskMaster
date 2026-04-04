using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ConcurrentObservableCollection.ConcurrentObservableDictionary;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class LockingObservableLinkedList_Tests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldCreateEmptyList()
        {
            var list = new LockingObservableLinkedList<int>();

            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithCollection_ShouldInitializeList()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddFirst_ShouldAddItemAndRaiseCollectionChanged()
        {
            var list = new LockingObservableLinkedList<string>();
            NotifyCollectionChangedAction? lastAction = null;
            list.CollectionChanged += (s, e) => lastAction = e.Action;

            list.AddFirst("hello");

            list.Count.Should().Be(1);
            list.First.Value.Should().Be("hello");
            lastAction.Should().Be(NotifyCollectionChangedAction.Add);
        }

        [TestMethod]
        public void AddLast_ShouldAddItemAndRaiseCollectionChanged()
        {
            var list = new LockingObservableLinkedList<string>();
            NotifyCollectionChangedAction? lastAction = null;
            list.CollectionChanged += (s, e) => lastAction = e.Action;

            list.AddLast("world");

            list.Count.Should().Be(1);
            list.Last.Value.Should().Be("world");
            lastAction.Should().Be(NotifyCollectionChangedAction.Add);
        }

        [TestMethod]
        public void Clear_ShouldRemoveAllItems()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.Clear();

            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void Find_ShouldReturnMatchingNode()
        {
            var list = new LockingObservableLinkedList<string>(new[] { "a", "b", "c" });

            var node = list.Find("b");

            node.Should().NotBeNull();
            node.Value.Should().Be("b");
        }

        [TestMethod]
        public void Find_WhenNotFound_ShouldReturnNull()
        {
            var list = new LockingObservableLinkedList<string>(new[] { "a", "b" });

            var node = list.Find("z");

            node.Should().BeNull();
        }

        [TestMethod]
        public void FindLast_ShouldReturnMatchingNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 1, 3 });

            var node = list.FindLast(1);

            node.Should().NotBeNull();
            node.Value.Should().Be(1);
        }

        [TestMethod]
        public void Find_WithPredicate_ShouldReturnMatchingNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 10, 20, 30 });

            var node = list.Find(x => x > 15);

            node.Should().NotBeNull();
            node.Value.Should().Be(20);
        }

        [TestMethod]
        public void Remove_ByValue_ShouldRemoveNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.Remove(2);

            list.Count.Should().Be(2);
            list.Find(2).Should().BeNull();
        }

        [TestMethod]
        public void RemoveFirst_ShouldRemoveFirstNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.RemoveFirst();

            list.Count.Should().Be(2);
            list.First.Value.Should().Be(2);
        }

        [TestMethod]
        public void RemoveLast_ShouldRemoveLastNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.RemoveLast();

            list.Count.Should().Be(2);
            list.Last.Value.Should().Be(2);
        }

        [TestMethod]
        public void TakeFirst_ShouldRemoveAndReturnFirstNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 10, 20, 30 });

            var result = list.TakeFirst();

            result.Should().Be(10);
            list.Count.Should().Be(2);
        }

        [TestMethod]
        public void TakeFirst_WhenEmpty_ShouldReturnDefault()
        {
            var list = new LockingObservableLinkedList<int>();

            var result = list.TakeFirst();

            result.Should().Be(0);
        }

        [TestMethod]
        public void TakeFirst_N_ShouldReturnNElements()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3, 4, 5 });

            var result = list.TakeFirst(3);

            result.Should().Equal(1, 2, 3);
            list.Count.Should().Be(2);
        }

        [TestMethod]
        public void TakeFirst_N_WhenNExceedsCount_ShouldThrowArgumentOutOfRangeException()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2 });

            Action act = () => list.TakeFirst(5);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void TryTakeFirst_ShouldReturnAvailableElements()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2 });

            var result = list.TryTakeFirst(5);

            result.Should().Equal(1, 2);
        }

        [TestMethod]
        public void TryTakeFirst_WhenNLessThan1_ShouldReturnNull()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2 });

            var result = list.TryTakeFirst(0);

            result.Should().BeNull();
        }

        [TestMethod]
        public void TakeLast_ShouldRemoveAndReturnLastNode()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 10, 20, 30 });

            var result = list.TakeLast();

            result.Should().Be(30);
            list.Count.Should().Be(2);
        }

        [TestMethod]
        public void TakeLast_WhenEmpty_ShouldReturnDefault()
        {
            var list = new LockingObservableLinkedList<int>();

            var result = list.TakeLast();

            result.Should().Be(0);
        }

        [TestMethod]
        public void TryTakeLast_ShouldReturnAvailableElements()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            var result = list.TryTakeLast(5);

            result.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void TryTakeLast_WhenNLessThan1_ShouldReturnNull()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1 });

            var result = list.TryTakeLast(0);

            result.Should().BeNull();
        }

        [TestMethod]
        public void AddOrMoveFirst_WhenItemDoesNotExist_ShouldAdd()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2 });

            list.AddOrMoveFirst(3);

            list.First.Value.Should().Be(3);
            list.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddOrMoveFirst_WhenItemExists_ShouldMoveToFront()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.AddOrMoveFirst(3);

            list.First.Value.Should().Be(3);
            list.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddOrMoveFirst_WhenItemIsAlreadyFirst_ShouldDoNothing()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.AddOrMoveFirst(1);

            list.First.Value.Should().Be(1);
            list.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddOrMoveFirst_WithMax_ShouldRemoveExcessElements()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });

            list.AddOrMoveFirst(4, 3);

            list.Count.Should().Be(3);
            list.First.Value.Should().Be(4);
        }

        [TestMethod]
        public void Remove_ByPredicate_ShouldRemoveMatchingNodes()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3, 4 });

            list.Remove(x => x % 2 == 0);

            list.Count.Should().Be(2);
            list.Find(2).Should().BeNull();
            list.Find(4).Should().BeNull();
        }

        [TestMethod]
        public void AddBefore_ShouldInsertItem()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 3 });
            var node = list.FindLast(3);

            list.AddBefore(node, 2);

            list.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddAfter_ShouldInsertItem()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 3 });
            var node = list.First;

            list.AddAfter(node, 2);

            list.Count.Should().Be(3);
        }

        [TestMethod]
        public void AddPartialObserver_ShouldRegisterObserver()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2 });
            var node = list.First;
            bool called = false;

            list.AddPartialObserver(
                (LockingObservableLinkedListChangedEventArgs<int> e) => called = true,
                node
            );
            list.Remove(node);

            called.Should().BeTrue();
        }

        [TestMethod]
        public void AddPartialObserver_NullObserver_ShouldThrow()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1 });
            var node = list.First;

            Action act = () => list.AddPartialObserver((ILockingLinkedListObserver<int>)null, node);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Add_AndRemove_BothRaiseCollectionChangedWithCorrectActionAndNodeReference()
        {
            // Arrange: subscribe and capture event args separately for Add and Remove operations.
            var list = new LockingObservableLinkedList<string>();
            LockingObservableLinkedListChangedEventArgs<string> addedArgs = null;
            LockingObservableLinkedListChangedEventArgs<string> removedArgs = null;

            list.CollectionChanged += (s, e) =>
            {
                // Route each event to the appropriate capture variable.
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    addedArgs = e;
                else if (
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                )
                    removedArgs = e;
            };

            // Act: add then remove the same item so each path fires once.
            list.AddFirst("hello");
            list.RemoveFirst();

            // Assert Add event: action is Add and the new node carries the correct value.
            addedArgs.Should().NotBeNull();
            addedArgs
                .Action.Should()
                .Be(System.Collections.Specialized.NotifyCollectionChangedAction.Add);
            addedArgs.NewNode.Should().NotBeNull();
            addedArgs.NewNode.Value.Should().Be("hello");

            // Assert Remove event: action is Remove and the old node carried the correct value.
            removedArgs.Should().NotBeNull();
            removedArgs
                .Action.Should()
                .Be(System.Collections.Specialized.NotifyCollectionChangedAction.Remove);
            removedArgs.OldNode.Should().NotBeNull();
            removedArgs.OldNode.Value.Should().Be("hello");
        }

        [TestMethod]
        public void PartialObserver_IsNotNotified_WhenDifferentNodeIsModified()
        {
            // Arrange: three-node list; register observer on the first node only.
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });
            var nodeA = list.First; // value 1 — the observed node
            var nodeC = list.Last; // value 3 — the node that will be removed
            bool observerCalled = false;

            list.AddPartialObserver(
                (LockingObservableLinkedListChangedEventArgs<int> e) => observerCalled = true,
                nodeA
            );

            // Act: remove nodeC, which is registered to no observer.
            list.Remove(nodeC);

            // Assert: the observer registered for nodeA must not have been invoked.
            observerCalled.Should().BeFalse();
        }

        [TestMethod]
        public void MoveOperations_AndTakeLastN_ShouldReorderAndReturnTailValues()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3, 4 });

            list.MoveBefore(list.Find(4), list.Find(2));
            list.MoveAfter(list.Find(1), list.Find(3));
            list.MoveDown(list.Find(4));
            list.MoveUp(list.Find(3));

            var tailValues = list.TakeLast(2);

            tailValues.Should().Equal(4, 1);
            list.Count.Should().Be(2);
            list.First.Value.Should().Be(2);
            list.Last.Value.Should().Be(3);
        }

        [TestMethod]
        public void AddOrMoveFirstWithMax_AndPredicateRemoval_ShouldLeaveRemainingTail()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 2, 3, 4 });

            list.AddOrMoveFirst(1, 5);
            list.Remove(x => x < 3);

            list.Count.Should().Be(2);
            list.First.Value.Should().Be(3);
            list.Last.Value.Should().Be(4);
        }

        [TestMethod]
        public void PartialObserverRemovalApis_ShouldReturnRemovedMappingsAndClearObservers()
        {
            var list = new LockingObservableLinkedList<int>(new[] { 1, 2, 3 });
            var first = list.First;
            var second = first.Next;
            var third = second.Next;
            var phantom = new LockingObservableLinkedListNode<int>(99);
            int observerCalls = 0;
            var observer = new SimpleActionLockingLinkedListObserver<int>(_ => observerCalls++);

            list.AddPartialObserver(observer, first, second);
            list.AddPartialObserver(observer, first);
            var removedByObserverAndKeys = list.RemovePartialObserver(observer, second, third);
            var removedByObserver = list.RemovePartialObserver(observer);
            list.AddPartialObserver(observer, second, third);
            var removedByKeys = list.RemovePartialObserver(second, phantom);
            var removedAll = list.RemoveAllObservers();
            list.Remove(third);

            removedByObserverAndKeys.Keys.Should().ContainSingle().Which.Should().BeSameAs(second);
            removedByObserver.Keys.Should().ContainSingle().Which.Should().BeSameAs(first);
            removedByKeys.Keys.Should().ContainSingle().Which.Should().BeSameAs(second);
            removedAll.Keys.Should().Contain(third);
            observerCalls.Should().Be(0);
        }
    }
}
