using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ObservableCollectionBatchUpdate_Tests
    {
        [TestMethod]
        public void Add_WithoutBatchUpdate_RaisesSingleAddNotification()
        {
            // Arrange
            var collection = new ObservableCollectionBatchUpdate<int>();
            NotifyCollectionChangedEventArgs lastArgs = null;
            var eventCount = 0;
            collection.CollectionChanged += (_, args) =>
            {
                eventCount++;
                lastArgs = args;
            };

            // Act
            collection.Add(5);

            // Assert
            collection.Should().Equal(5);
            eventCount.Should().Be(1);
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Add);
            lastArgs.NewItems.Count.Should().Be(1);
            lastArgs.NewItems[0].Should().Be(5);
        }

        [TestMethod]
        public void AddAndRemove_DuringBatchUpdate_SuppressesNotificationsUntilBatchEnds()
        {
            // Arrange
            var collection = new ObservableCollectionBatchUpdate<string>();
            var eventCount = 0;
            collection.CollectionChanged += (_, _) => eventCount++;

            // Act
            collection.BeginUpdate();
            collection.Add("alpha");
            collection.Add("beta");
            collection.Remove("alpha");
            collection.EndUpdate();

            // Assert
            collection.Should().Equal("beta");
            eventCount.Should().Be(0);
        }

        [TestMethod]
        public void Clear_OutsideBatchUpdate_RaisesResetNotification()
        {
            // Arrange
            var collection = new ObservableCollectionBatchUpdate<int> { 1, 2, 3 };
            NotifyCollectionChangedEventArgs lastArgs = null;
            collection.CollectionChanged += (_, args) => lastArgs = args;

            // Act
            collection.Clear();

            // Assert
            collection.Should().BeEmpty();
            lastArgs.Should().NotBeNull();
            lastArgs.Action.Should().Be(NotifyCollectionChangedAction.Reset);
        }

        [TestMethod]
        public void BeginAndEndUpdate_WithoutMutations_IsEmptyBatchAndRaisesNoEvents()
        {
            // Arrange
            var collection = new ObservableCollectionBatchUpdate<int>();
            var eventCount = 0;
            collection.CollectionChanged += (_, _) => eventCount++;

            // Act
            collection.BeginUpdate();
            collection.EndUpdate();

            // Assert
            collection.Should().BeEmpty();
            eventCount.Should().Be(0);
        }

        [TestMethod]
        public void NestedBeginUpdate_RequiresOnlyFinalEndUpdateBeforeNotificationsResume()
        {
            // Arrange
            var collection = new ObservableCollectionBatchUpdate<int>();
            var eventCount = 0;
            collection.CollectionChanged += (_, _) => eventCount++;

            // Act
            collection.BeginUpdate();
            collection.BeginUpdate();
            collection.Add(1);
            collection.EndUpdate();
            collection.Add(2);

            // Assert
            collection.Should().Equal(1, 2);
            eventCount.Should().Be(1);
        }
    }
}
