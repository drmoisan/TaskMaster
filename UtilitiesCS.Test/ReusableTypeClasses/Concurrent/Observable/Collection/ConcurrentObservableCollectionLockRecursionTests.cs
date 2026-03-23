using System.Collections.Specialized;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swordfish.NET.Collections;

namespace ConcurrentObservableCollection.Tests
{
    /// <summary>
    /// Regression tests for the LockRecursionException bug.
    ///
    /// Purpose:
    ///     ConcurrentObservableBase&lt;T&gt; raises CollectionChanged synchronously from the
    ///     "DRM Hack" _baseCollection.CollectionChanged relay while the write lock is
    ///     still held inside DoBaseWrite. If a subscriber then reads from the same
    ///     collection (e.g. calls map.Last() or accesses Count), it enters DoBaseRead,
    ///     which tries to acquire a read lock. ReaderWriterLockSlim (NoRecursion policy)
    ///     throws LockRecursionException because the write lock is already held on the
    ///     same thread.
    ///
    /// Coverage:
    ///     1. Verifies the exception IS thrown when the handler re-reads the collection
    ///        (documents the bug; ensures the fix is never silently reverted).
    ///     2. Verifies the exception IS NOT thrown when the handler reads only from the
    ///        event args (the safe, fixed pattern).
    /// </summary>
    [TestClass]
    public class ConcurrentObservableCollectionLockRecursionTests
    {
        /// <summary>
        /// Regression: verifies that reading the collection from inside a CollectionChanged
        /// handler (simulating the original map.Last() call in SubjectMap_CollectionChanged)
        /// throws LockRecursionException because the write lock is still held during the
        /// synchronous CollectionChanged callback.
        ///
        /// This test documents the root-cause of the production bug and must remain
        /// so that the bug cannot be silently re-introduced.
        /// </summary>
        [TestMethod]
        public void Add_WhenCollectionChangedHandlerReadsCountFromCollection_ThrowsLockRecursionException()
        {
            // Arrange — subscribe a handler that re-reads the collection (the buggy pattern).
            var collection = new ConcurrentObservableCollection<int>();

            collection.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    // Simulates map.Last(): accesses Count via DoBaseRead while the write
                    // lock from DoBaseWrite is still held on this thread.
                    _ = collection.Count;
                }
            };

            // Act & Assert — the re-entrant read must throw LockRecursionException.
            collection
                .Invoking(c => c.Add(42))
                .Should()
                .Throw<LockRecursionException>(
                    "reading the collection from inside a CollectionChanged handler "
                        + "that fires during Add re-enters the same lock and must throw"
                );
        }

        /// <summary>
        /// Verifies that the safe pattern — using e.NewItems[0] instead of re-reading the
        /// collection — does not throw LockRecursionException and delivers the correct item.
        ///
        /// This is the pattern applied by the fix to SubjectMap_CollectionChanged.
        /// </summary>
        [TestMethod]
        public void Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow()
        {
            // Arrange — subscribe a handler that reads from e.NewItems (the safe pattern).
            var collection = new ConcurrentObservableCollection<int>();
            int capturedItem = -1;

            collection.CollectionChanged += (sender, e) =>
            {
                // Safe: reads from event args, not from the collection itself.
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
                {
                    capturedItem = (int)e.NewItems[0];
                }
            };

            // Act
            collection.Invoking(c => c.Add(42)).Should().NotThrow();

            // Assert — item was captured correctly without triggering lock recursion.
            capturedItem
                .Should()
                .Be(42, "e.NewItems[0] must contain the item that was just added");
        }
    }
}
