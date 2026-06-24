using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotTests
    {
        [TestMethod]
        public void Constructor_CopiesRootsAndBuildsLookup()
        {
            var root = Key("store-a", "root", "\\Root");
            var roots = new List<FolderTreeNodeKey> { root };
            var snapshot = new FolderTreeSnapshot(roots, new[] { Node(root) });
            roots.Clear();

            snapshot.RootKeys.Should().ContainSingle().Which.Should().Be(root);
            snapshot.TryGetNode(root, out var node).Should().BeTrue();
            node.Key.Should().Be(root);
        }

        [TestMethod]
        public void Constructor_NullCollections_CreatesEmptySnapshot()
        {
            var snapshot = new FolderTreeSnapshot(null, null);

            snapshot.RootKeys.Should().BeEmpty();
            snapshot.NodesByKey.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_PreservesRootKeyOrder()
        {
            var first = Key("store-a", "root-a", "\\RootA");
            var second = Key("store-b", "root-b", "\\RootB");

            var snapshot = new FolderTreeSnapshot(
                new[] { first, second },
                new[] { Node(first), Node(second) }
            );

            snapshot.RootKeys.Should().ContainInOrder(first, second);
        }

        [TestMethod]
        public void TryGetNode_MissingKey_ReturnsFalseAndNullNode()
        {
            var root = Key("store-a", "root", "\\Root");
            var missing = Key("store-a", "missing", "\\Missing");
            var snapshot = new FolderTreeSnapshot(new[] { root }, new[] { Node(root) });

            snapshot.TryGetNode(missing, out var node).Should().BeFalse();
            node.Should().BeNull();
        }

        [TestMethod]
        public void TryGetNode_NullKey_ReturnsFalseAndNullNode()
        {
            var root = Key("store-a", "root", "\\Root");
            var snapshot = new FolderTreeSnapshot(new[] { root }, new[] { Node(root) });

            snapshot.TryGetNode(null, out var node).Should().BeFalse();
            node.Should().BeNull();
        }

        [TestMethod]
        public void GetNodesForStore_ReturnsOnlyMatchingStore()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var storeB = Node(Key("store-b", "inbox-b", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(
                new[] { storeA.Key, storeB.Key },
                new[] { storeA, storeB }
            );

            snapshot.GetNodesForStore("STORE-A").Should().ContainSingle().Which.Should().Be(storeA);
        }

        [TestMethod]
        public void GetNodesForStore_MissingStore_ReturnsEmptyList()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(new[] { storeA.Key }, new[] { storeA });

            snapshot.GetNodesForStore("store-b").Should().BeEmpty();
        }

        [TestMethod]
        public void GetNodesForStore_BlankStore_ReturnsEmptyList()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(new[] { storeA.Key }, new[] { storeA });

            snapshot.GetNodesForStore(" ").Should().BeEmpty();
        }

        [TestMethod]
        public void FindByPath_UsesStoreScope()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var storeB = Node(Key("store-b", "inbox-b", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(
                new[] { storeA.Key, storeB.Key },
                new[] { storeA, storeB }
            );

            snapshot.FindByPath("store-b", "\\Inbox").Should().Be(storeB);
        }

        [TestMethod]
        public void FindByPath_MissingPath_ReturnsNull()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(new[] { storeA.Key }, new[] { storeA });

            snapshot.FindByPath("store-a", "\\Missing").Should().BeNull();
        }

        [TestMethod]
        public void FindByPath_BlankPath_ReturnsNull()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(new[] { storeA.Key }, new[] { storeA });

            snapshot.FindByPath("store-a", " ").Should().BeNull();
        }

        [TestMethod]
        public void FindByPath_UsesCaseInsensitivePathComparison()
        {
            var storeA = Node(Key("store-a", "inbox-a", "\\Inbox"));
            var snapshot = new FolderTreeSnapshot(new[] { storeA.Key }, new[] { storeA });

            snapshot.FindByPath("store-a", "\\inbox").Should().Be(storeA);
        }

        [TestMethod]
        public void GetChildren_ReturnsChildNodesInParentOrder()
        {
            var child = Key("store-a", "child", "\\Root\\Child");
            var root = Key("store-a", "root", "\\Root");
            var parent = Node(root, childKeys: new[] { child });
            var snapshot = new FolderTreeSnapshot(new[] { root }, new[] { parent, Node(child) });

            snapshot.GetChildren(root).Should().ContainSingle().Which.Key.Should().Be(child);
        }

        [TestMethod]
        public void GetChildren_MissingParentOrChild_ReturnsOnlyExistingChildren()
        {
            var missingChild = Key("store-a", "missing", "\\Root\\Missing");
            var root = Key("store-a", "root", "\\Root");
            var parent = Node(root, childKeys: new[] { missingChild });
            var snapshot = new FolderTreeSnapshot(new[] { root }, new[] { parent });

            snapshot.GetChildren(root).Should().BeEmpty();
            snapshot.GetChildren(missingChild).Should().BeEmpty();
        }

        private static FolderTreeNodeKey Key(string storeId, string entryId, string path)
        {
            return new FolderTreeNodeKey(storeId, entryId, path);
        }

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            IEnumerable<FolderTreeNodeKey> childKeys = null
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                key.FolderPath.Trim('\\'),
                key.StoreId,
                key.EntryId,
                null,
                key.FolderPath,
                key.FolderPath.Trim('\\'),
                childKeys,
                false,
                string.Empty
            );
        }
    }
}
