using System.Collections.Specialized;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace ConcurrentObservableCollection.Tests
{
    /// <summary>
    /// Lock-recursion hazard tests, re-expressed for the clean, Swordfish-free
    /// <see cref="ConcurrentObservableCollection{T}"/>.
    ///
    /// <para>The legacy Swordfish <c>ConcurrentObservableBase&lt;T&gt;</c> raised CollectionChanged
    /// synchronously while a <see cref="System.Threading.ReaderWriterLockSlim"/> write lock was
    /// still held inside <c>DoBaseWrite</c>. A handler that re-read the collection (e.g.
    /// <c>map.Last()</c> or <c>Count</c>) entered <c>DoBaseRead</c>, tried to take a read lock on the
    /// same non-recursive lock, and threw <c>LockRecursionException</c>.</para>
    ///
    /// <para>The clean base derives from <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>,
    /// which does not use a <c>ReaderWriterLockSlim</c>. Rationale for the re-expression (P4-T7):
    /// the lock-recursion hazard is eliminated by construction, so a handler may safely read the
    /// collection from inside a CollectionChanged callback. These tests assert that removal and that
    /// the previously-mandated safe pattern (reading from <c>e.NewItems</c>) still works.</para>
    /// </summary>
    [TestClass]
    public class ConcurrentObservableCollectionLockRecursionTests
    {
        /// <summary>
        /// The Swordfish lock-recursion hazard is gone: reading the collection (Count) from inside a
        /// CollectionChanged handler that fires during Add no longer throws, because the clean base
        /// holds no re-entrant lock during the notification.
        /// </summary>
        [TestMethod]
        public void Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow()
        {
            // Arrange — a handler that re-reads the collection (the formerly-fatal pattern).
            var collection = new ConcurrentObservableCollection<int>();
            int observedCount = -1;

            collection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    // On the clean ObservableCollection base this is safe — no lock is held.
                    observedCount = collection.Count;
                }
            };

            // Act & Assert — no LockRecursionException on the clean base.
            collection
                .Invoking(c => c.Add(42))
                .Should()
                .NotThrow(
                    "the clean ObservableCollection base holds no ReaderWriterLockSlim during "
                        + "the synchronous CollectionChanged callback, so re-reading is safe"
                );

            observedCount.Should().Be(1, "the handler observed the collection after the add");
        }

        /// <summary>
        /// The safe pattern — reading from <c>e.NewItems</c> instead of re-reading the collection —
        /// continues to work and delivers the correct item.
        /// </summary>
        [TestMethod]
        public void Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow()
        {
            // Arrange — subscribe a handler that reads from e.NewItems (the safe pattern).
            var collection = new ConcurrentObservableCollection<int>();
            int capturedItem = -1;

            collection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
                {
                    capturedItem = (int)e.NewItems[0];
                }
            };

            // Act
            collection.Invoking(c => c.Add(42)).Should().NotThrow();

            // Assert — item was captured correctly.
            capturedItem
                .Should()
                .Be(42, "e.NewItems[0] must contain the item that was just added");
        }
    }
}
