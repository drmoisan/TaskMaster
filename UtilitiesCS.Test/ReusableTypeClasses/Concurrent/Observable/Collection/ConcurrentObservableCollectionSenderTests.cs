using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace ConcurrentObservableCollection.Tests
{
    /// <summary>
    /// Sender-identity tests for the clean, Swordfish-free
    /// <see cref="ConcurrentObservableCollection{T}"/>. The type derives from
    /// <see cref="ObservableCollection{T}"/> and raises CollectionChanged with itself as the
    /// sender, so a subscriber that casts sender to the wrapper type (the SubjectMapSco pattern)
    /// receives the wrapper instance rather than a raw inner collection.
    /// </summary>
    [TestClass]
    public class ConcurrentObservableCollectionSenderTests
    {
        /// <summary>
        /// Verify that the sender passed to CollectionChanged on Add is the
        /// ConcurrentObservableCollection wrapper, not the inner
        /// ObservableCollection.
        /// </summary>
        [TestMethod]
        public void CollectionChanged_Add_SenderIsWrapperNotInnerCollection()
        {
            // Arrange
            var collection = new ConcurrentObservableCollection<int>();
            object capturedSender = null;

            collection.CollectionChanged += (sender, _) =>
            {
                capturedSender = sender;
            };

            // Act
            collection.Add(42);

            // Assert — sender must be the wrapper, not ObservableCollection<int>
            capturedSender.Should().NotBeNull("CollectionChanged must fire on Add");
            capturedSender
                .Should()
                .BeSameAs(
                    collection,
                    "sender should be the ConcurrentObservableCollection wrapper, "
                        + "not the internal ObservableCollection"
                );
            capturedSender
                .Should()
                .NotBeOfType<ObservableCollection<int>>(
                    "sender must not be the raw ObservableCollection"
                );
        }

        /// <summary>
        /// Verify that sender is the wrapper when an item is removed.
        /// </summary>
        [TestMethod]
        public void CollectionChanged_Remove_SenderIsWrapperNotInnerCollection()
        {
            // Arrange
            var collection = new ConcurrentObservableCollection<string>();
            collection.Add("item");
            object capturedSender = null;

            collection.CollectionChanged += (sender, _) =>
            {
                capturedSender = sender;
            };

            // Act
            collection.Remove("item");

            // Assert
            capturedSender.Should().NotBeNull("CollectionChanged must fire on Remove");
            capturedSender.Should().BeSameAs(collection);
        }

        /// <summary>
        /// Verify that a subscriber can safely cast sender to the concrete
        /// wrapper type — the pattern used by SubjectMap_CollectionChanged.
        /// </summary>
        [TestMethod]
        public void CollectionChanged_SenderCanBeCastToConcreteWrapperType()
        {
            // Arrange
            var collection = new ConcurrentObservableCollection<int>();
            ConcurrentObservableCollection<int> castSender = null;

            collection.CollectionChanged += (sender, _) =>
            {
                // This cast mirrors the real-world SubjectMapSco cast pattern.
                // Before the fix, this throws InvalidCastException.
                castSender = (ConcurrentObservableCollection<int>)sender;
            };

            // Act
            collection.Add(1);

            // Assert
            castSender
                .Should()
                .BeSameAs(
                    collection,
                    "casting sender to the wrapper type must succeed and return the same instance"
                );
        }
    }
}
