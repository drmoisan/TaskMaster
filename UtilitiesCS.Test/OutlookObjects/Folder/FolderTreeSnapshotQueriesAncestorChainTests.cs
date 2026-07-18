using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="FolderTreeSnapshotQueries.GetAncestorChain"/> covering the documented
    /// invariants (root-first ordering, leaf identity, parent/child linkage), positive single- and
    /// multi-level chains, root-only chains, and the negative/edge scenarios (null snapshot, null and
    /// unknown leaf keys, defensive cycle guard, duplicate display names distinguished by key).
    /// </summary>
    [TestClass]
    public sealed class FolderTreeSnapshotQueriesAncestorChainTests
    {
        [TestMethod]
        public void GetAncestorChain_RootOnlyLeaf_ReturnsSingleElementChain()
        {
            // Arrange
            var rootKey = Key("store-a", "root", "\\Root");
            var snapshot = new FolderTreeSnapshot(
                new[] { rootKey },
                new[] { Node(rootKey, "Root", null) }
            );

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, rootKey);

            // Assert
            chain.Should().ContainSingle();
            chain[0].Key.Should().Be(rootKey);
        }

        [TestMethod]
        public void GetAncestorChain_SingleLevel_ReturnsRootThenLeaf()
        {
            // Arrange
            var rootKey = Key("store-a", "root", "\\Root");
            var leafKey = Key("store-a", "leaf", "\\Root\\Leaf");
            var snapshot = new FolderTreeSnapshot(
                new[] { rootKey },
                new[] { Node(rootKey, "Root", null, leafKey), Node(leafKey, "Leaf", rootKey) }
            );

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey);

            // Assert
            chain.Select(n => n.Key).Should().Equal(rootKey, leafKey);
            chain.Last().Key.Should().Be(leafKey);
        }

        [TestMethod]
        public void GetAncestorChain_MultiLevel_ReturnsRootFirstLeafLastWithLinkedPairs()
        {
            // Arrange
            var rootKey = Key("store-a", "root", "\\Root");
            var midKey = Key("store-a", "mid", "\\Root\\Clients");
            var leafKey = Key("store-a", "leaf", "\\Root\\Clients\\Acme");
            var snapshot = new FolderTreeSnapshot(
                new[] { rootKey },
                new[]
                {
                    Node(rootKey, "Root", null, midKey),
                    Node(midKey, "Clients", rootKey, leafKey),
                    Node(leafKey, "Acme", midKey),
                }
            );

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey);

            // Assert
            chain.Select(n => n.Key).Should().Equal(rootKey, midKey, leafKey);
            chain.Last().Key.Should().Be(leafKey);
            for (var i = 1; i < chain.Count; i++)
            {
                chain[i].ParentKey.Equals(chain[i - 1].Key).Should().BeTrue();
            }
        }

        [TestMethod]
        public void GetAncestorChain_DuplicateDisplayNamesAtDifferentDepths_DistinguishedByKey()
        {
            // Arrange: two segments named "Archive" at different depths.
            var rootKey = Key("store-a", "root", "\\Archive");
            var leafKey = Key("store-a", "leaf", "\\Archive\\Archive");
            var snapshot = new FolderTreeSnapshot(
                new[] { rootKey },
                new[] { Node(rootKey, "Archive", null, leafKey), Node(leafKey, "Archive", rootKey) }
            );

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey);

            // Assert: identity is by key, not display name.
            chain.Select(n => n.DisplayName).Should().Equal("Archive", "Archive");
            chain.Select(n => n.Key).Should().Equal(rootKey, leafKey);
        }

        [TestMethod]
        public void GetAncestorChain_NullSnapshot_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () =>
                FolderTreeSnapshotQueries.GetAncestorChain(null, Key("store-a", "leaf", "\\Root"));

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("snapshot");
        }

        [TestMethod]
        public void GetAncestorChain_NullLeafKey_ReturnsEmptyListNeverNull()
        {
            // Arrange
            var rootKey = Key("store-a", "root", "\\Root");
            var snapshot = new FolderTreeSnapshot(
                new[] { rootKey },
                new[] { Node(rootKey, "Root", null) }
            );

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, null);

            // Assert
            chain.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void GetAncestorChain_UnknownLeafKey_ReturnsEmptyList()
        {
            // Arrange
            var rootKey = Key("store-a", "root", "\\Root");
            var snapshot = new FolderTreeSnapshot(
                new[] { rootKey },
                new[] { Node(rootKey, "Root", null) }
            );
            var staleKey = Key("store-a", "gone", "\\Root\\Removed");

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, staleKey);

            // Assert
            chain.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void GetAncestorChain_CyclicParentKey_ReturnsPartialChainWithoutHanging()
        {
            // Arrange: a malformed snapshot where leaf -> parent -> leaf (cycle).
            var aKey = Key("store-a", "a", "\\A");
            var bKey = Key("store-a", "b", "\\A\\B");
            var snapshot = new FolderTreeSnapshot(
                new[] { aKey },
                new[]
                {
                    // A's ParentKey points to B and B's ParentKey points to A -> cycle.
                    Node(aKey, "A", bKey, bKey),
                    Node(bKey, "B", aKey),
                }
            );

            // Act
            var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, bKey);

            // Assert: terminates; partial chain contains both visited nodes exactly once, with the
            // requested leaf (bKey) last after the root-first reversal.
            chain.Should().HaveCount(2);
            chain.Select(n => n.Key).Should().OnlyHaveUniqueItems();
            chain.Last().Key.Should().Be(bKey);
        }

        private static FolderTreeNodeKey Key(string storeId, string entryId, string folderPath)
        {
            return new FolderTreeNodeKey(storeId, entryId, folderPath);
        }

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            string displayName,
            FolderTreeNodeKey parentKey,
            params FolderTreeNodeKey[] childKeys
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                key.StoreId,
                key.EntryId,
                parentKey,
                key.FolderPath,
                displayName,
                childKeys,
                false,
                string.Empty
            );
        }
    }
}
