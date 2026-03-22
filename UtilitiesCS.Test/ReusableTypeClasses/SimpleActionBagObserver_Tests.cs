using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Bag;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SimpleActionBagObserver_Tests
    {
        [TestMethod]
        public void OnEventOccur_InvokesActionWithArgs()
        {
            // Arrange
            BagChangedEventArgs<int> received = null;
            var observer = new SimpleActionBagObserver<int>(args => received = args);
            var eventArgs = new BagChangedEventArgs<int>(NotifyCollectionChangedAction.Add, 42, 0);

            // Act
            observer.OnEventOccur(eventArgs);

            // Assert
            received.Should().BeSameAs(eventArgs);
            received.Action.Should().Be(NotifyCollectionChangedAction.Add);
            received.NewValue.Should().Be(42);
        }

        [TestMethod]
        public void OnEventOccur_CalledMultipleTimes_InvokesActionEachTime()
        {
            // Arrange
            int callCount = 0;
            var observer = new SimpleActionBagObserver<string>(args => callCount++);
            var eventArgs = new BagChangedEventArgs<string>(NotifyCollectionChangedAction.Remove);

            // Act
            observer.OnEventOccur(eventArgs);
            observer.OnEventOccur(eventArgs);
            observer.OnEventOccur(eventArgs);

            // Assert
            callCount.Should().Be(3);
        }

        [TestMethod]
        public void BagChangedEventArgs_ActionOnlyConstructor_SetsAction()
        {
            // Arrange & Act
            var args = new BagChangedEventArgs<int>(NotifyCollectionChangedAction.Reset);

            // Assert
            args.Action.Should().Be(NotifyCollectionChangedAction.Reset);
            args.NewValue.Should().Be(default);
            args.OldValue.Should().Be(default);
        }

        [TestMethod]
        public void BagChangedEventArgs_FullConstructor_SetsAllProperties()
        {
            // Arrange & Act
            var args = new BagChangedEventArgs<string>(
                NotifyCollectionChangedAction.Replace,
                "new",
                "old"
            );

            // Assert
            args.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            args.NewValue.Should().Be("new");
            args.OldValue.Should().Be("old");
        }
    }
}
