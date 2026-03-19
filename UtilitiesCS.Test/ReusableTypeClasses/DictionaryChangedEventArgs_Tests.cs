using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class DictionaryChangedEventArgs_Tests
    {
        [TestMethod]
        public void Constructor_WithActionOnly_SetsActionAndLeavesPayloadAtDefaults()
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Reset;

            // Act
            var args = new DictionaryChangedEventArgs<string, int>(action);

            // Assert
            args.Action.Should().Be(action);
            args.Key.Should().BeNull();
            args.NewValue.Should().Be(0);
            args.OldValue.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_WithValues_SetsAllProperties()
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Replace;

            // Act
            var args = new DictionaryChangedEventArgs<string, int>(action, "alpha", 5, 3);

            // Assert
            args.Action.Should().Be(action);
            args.Key.Should().Be("alpha");
            args.NewValue.Should().Be(5);
            args.OldValue.Should().Be(3);
        }

        [TestMethod]
        public void Constructor_WithNullReferenceKey_PreservesNullKey()
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Add;

            // Act
            var args = new DictionaryChangedEventArgs<string, string>(action, null, "new", "old");

            // Assert
            args.Action.Should().Be(action);
            args.Key.Should().BeNull();
            args.NewValue.Should().Be("new");
            args.OldValue.Should().Be("old");
        }

        [DataTestMethod]
        [DataRow(NotifyCollectionChangedAction.Add)]
        [DataRow(NotifyCollectionChangedAction.Remove)]
        [DataRow(NotifyCollectionChangedAction.Replace)]
        [DataRow(NotifyCollectionChangedAction.Move)]
        [DataRow(NotifyCollectionChangedAction.Reset)]
        public void Constructor_WithEachSupportedAction_RetainsProvidedAction(
            NotifyCollectionChangedAction action
        )
        {
            // Act
            var args = new DictionaryChangedEventArgs<string, int>(action, "key", 10, 2);

            // Assert
            args.Action.Should().Be(action);
        }
    }
}
