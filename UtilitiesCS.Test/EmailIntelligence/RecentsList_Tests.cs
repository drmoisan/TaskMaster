using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class RecentsList_Tests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldCreateEmptyListWithDefaultMax()
        {
            var list = new RecentsList<string>();

            list.Count.Should().Be(0);
            list.Max.Should().Be(5);
        }

        [TestMethod]
        public void Constructor_WithList_ShouldInitializeWithItems()
        {
            var items = new List<string> { "a", "b", "c" };
            var list = new RecentsList<string>(items, 10);

            list.Count.Should().Be(3);
            list.Max.Should().Be(10);
        }

        [TestMethod]
        public void Constructor_WithEnumerable_ShouldInitializeWithItems()
        {
            IEnumerable<string> items = new[] { "x", "y" };
            var list = new RecentsList<string>(items, 3);

            list.Count.Should().Be(2);
            list.Max.Should().Be(3);
        }

        [TestMethod]
        public void Max_SetAndGet_ShouldWork()
        {
            var list = new RecentsList<string>();

            list.Max = 20;

            list.Max.Should().Be(20);
        }

        [TestMethod]
        public void Constructor_WithEnumerableAndMax_ShouldSetMax()
        {
            // The filename/folderpath constructor attempts file-system deserialization,
            // so use the IEnumerable constructor instead for deterministic testing.
            var list = new RecentsList<string>(new[] { "a", "b" }, 7);

            list.Max.Should().Be(7);
            list.Count.Should().Be(2);
        }

        // -----------------------------------------------------------------------
        // P69-T1 — Re-adding an existing item moves it to the front.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that AddThreadsafe moves an already-present item to position 0
        /// rather than inserting a duplicate.
        ///
        /// Purpose:
        ///     Confirm the "most-recently-used" promotion: duplicates are removed
        ///     from their current position and re-inserted at the head of the list.
        ///
        /// Returns:
        ///     Passes when re-added item is at index 0 and the count does not grow.
        /// </summary>
        [TestMethod]
        public void AddThreadsafe_DuplicateItem_MovesExistingEntryToFront()
        {
            // Arrange: list with two distinct items.
            var list = new RecentsList<string> { Max = 5 };
            AddThreadsafe(list, "A");
            AddThreadsafe(list, "B");

            // Act: re-add "A" (already present at index 1).
            AddThreadsafe(list, "A");

            // Assert: "A" is now at the front; no duplicate was inserted.
            list[0].Should().Be("A");
            list.Should().HaveCount(2, "re-adding a duplicate must not grow the list");
        }

        // -----------------------------------------------------------------------
        // P69-T2 — Adding beyond Max trims the oldest entry.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that when the list is at max capacity, adding a new distinct item
        /// removes the oldest (last) entry so the count stays at Max.
        ///
        /// Purpose:
        ///     Confirm the capacity guard in AddThreadsafe: the item at the tail of
        ///     the list (the least-recently added/seen item) is evicted.
        ///
        /// Returns:
        ///     Passes when the count equals Max and the oldest item is absent.
        /// </summary>
        [TestMethod]
        public void AddThreadsafe_ExceedsMaxCapacity_TrimsOldestEntry()
        {
            // Arrange: fill to capacity (Max = 3) with items in insertion order.
            var list = new RecentsList<string> { Max = 3 };
            AddThreadsafe(list, "first");
            AddThreadsafe(list, "second");
            AddThreadsafe(list, "third"); // list is now [third, second, first]

            // Act: add a fourth item — "first" (oldest, at the tail) should be evicted.
            AddThreadsafe(list, "fourth");

            // Assert
            list.Should().HaveCount(3, "count must stay at Max after eviction");
            list.Should().NotContain("first", "the oldest entry must be trimmed");
            list[0].Should().Be("fourth", "the newest entry must be at position 0");
        }

        // -----------------------------------------------------------------------
        // P69-T3 — Items are stored in most-recently-added-first order.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Verifies that items added to the list via AddThreadsafe are stored with
        /// the most-recently-added item at the head, preserving MRU order throughout.
        ///
        /// Purpose:
        ///     Confirm insertion-order semantics: each new item is inserted at index 0
        ///     so a ToList() snapshot always reflects most-recently-used ordering.
        ///     This mirrors the order that would be preserved across a serialization
        ///     round-trip (serialize/deserialize of the underlying list).
        ///
        /// Returns:
        ///     Passes when the list sequence matches the expected MRU order.
        /// </summary>
        [TestMethod]
        public void AddThreadsafe_ThreeDistinctItems_StoredInMostRecentlyAddedFirstOrder()
        {
            // Arrange
            var list = new RecentsList<string> { Max = 10 };

            // Act: add three distinct items in sequence.
            AddThreadsafe(list, "alpha");
            AddThreadsafe(list, "beta");
            AddThreadsafe(list, "gamma");

            // Assert: MRU order — gamma (newest) first, alpha (oldest) last.
            list.ToList().Should().ContainInConsecutiveOrder("gamma", "beta", "alpha");
        }

        /// <summary>
        /// Invokes the private AddThreadsafe method on the given list via reflection.
        ///
        /// Purpose:
        ///     The public Add method routes through a BlockingCollection whose consumer
        ///     is currently disabled; AddThreadsafe contains the observable list logic
        ///     and must be reached directly for deterministic unit tests.
        ///
        /// Args:
        ///     list (RecentsList{T}): Target list instance.
        ///     item (T): Item to add via the internal logic.
        /// </summary>
        private static void AddThreadsafe<T>(RecentsList<T> list, T item)
        {
            var method = typeof(RecentsList<T>).GetMethod(
                "AddThreadsafe",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            method.Should().NotBeNull("AddThreadsafe private method must exist on RecentsList<T>");
            method.Invoke(list, new object[] { item });
        }
    }
}
