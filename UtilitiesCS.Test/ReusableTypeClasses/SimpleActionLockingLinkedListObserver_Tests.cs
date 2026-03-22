using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Locking.Observable.LinkedList;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SimpleActionLockingLinkedListObserver_Tests
    {
        [TestMethod]
        public void OnEventOccur_InvokesActionWithArgs()
        {
            // Arrange
            LockingObservableLinkedListChangedEventArgs<int> received = null;
            var observer = new SimpleActionLockingLinkedListObserver<int>(args => received = args);
            var eventArgs = new LockingObservableLinkedListChangedEventArgs<int>(
                NotifyCollectionChangedAction.Add
            );

            // Act
            observer.OnEventOccur(eventArgs);

            // Assert
            received.Should().BeSameAs(eventArgs);
            received.Action.Should().Be(NotifyCollectionChangedAction.Add);
        }

        [TestMethod]
        public void OnEventOccur_CalledMultipleTimes_InvokesActionEachTime()
        {
            // Arrange
            int callCount = 0;
            var observer = new SimpleActionLockingLinkedListObserver<string>(args => callCount++);
            var eventArgs = new LockingObservableLinkedListChangedEventArgs<string>(
                NotifyCollectionChangedAction.Remove
            );

            // Act
            observer.OnEventOccur(eventArgs);
            observer.OnEventOccur(eventArgs);

            // Assert
            callCount.Should().Be(2);
        }

        [TestMethod]
        public void LockingObservableLinkedListChangedEventArgs_ActionOnlyConstructor_SetsAction()
        {
            // Arrange & Act
            var args = new LockingObservableLinkedListChangedEventArgs<int>(
                NotifyCollectionChangedAction.Reset
            );

            // Assert
            args.Action.Should().Be(NotifyCollectionChangedAction.Reset);
            args.NewNode.Should().BeNull();
            args.OldNode.Should().BeNull();
        }
    }
}
