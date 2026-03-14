using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Bag;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class BagChangedEventArgs_Tests
    {
        [TestMethod]
        public void Constructor_WithActionOnly_SetsActionAndLeavesPayloadAtDefaults()
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;

            // Act
            var args = new BagChangedEventArgs<string>(action);

            // Assert
            args.Action.Should().Be(action);
            args.NewValue.Should().BeNull();
            args.OldValue.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithValues_SetsAllProperties()
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Replace;

            // Act
            var args = new BagChangedEventArgs<string>(action, "new-value", "old-value");

            // Assert
            args.Action.Should().Be(action);
            args.NewValue.Should().Be("new-value");
            args.OldValue.Should().Be("old-value");
        }

        [TestMethod]
        public void Constructor_WithNullItem_PreservesNullPayload()
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Remove;

            // Act
            var args = new BagChangedEventArgs<string>(action, null, null);

            // Assert
            args.Action.Should().Be(action);
            args.NewValue.Should().BeNull();
            args.OldValue.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(NotifyCollectionChangedAction.Add)]
        [DataRow(NotifyCollectionChangedAction.Remove)]
        [DataRow(NotifyCollectionChangedAction.Replace)]
        [DataRow(NotifyCollectionChangedAction.Move)]
        [DataRow(NotifyCollectionChangedAction.Reset)]
        public void Constructor_WithEachSupportedAction_RetainsProvidedAction(NotifyCollectionChangedAction action)
        {
            // Act
            var args = new BagChangedEventArgs<int>(action, 10, 2);

            // Assert
            args.Action.Should().Be(action);
        }
    }
}
