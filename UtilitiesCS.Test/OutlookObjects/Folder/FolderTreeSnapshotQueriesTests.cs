using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotQueriesTests
    {
        [TestMethod]
        public void GetSelectedNodes_ReturnsOverlaySelections()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            var selected = FolderTreeSnapshotQueries.GetSelectedNodes(
                snapshot,
                new FolderTreeSelectionOverlay(new[] { "Archive" })
            );

            selected.Should().ContainSingle().Which.RelativePath.Should().Be("Archive");
        }

        [TestMethod]
        public void GetSelectedNodes_NullOverlay_ReturnsNoSelections()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            var selected = FolderTreeSnapshotQueries.GetSelectedNodes(snapshot, null);

            selected.Should().BeEmpty();
        }

        [TestMethod]
        public void GetSelectedNodes_NullSnapshot_Throws()
        {
            Action act = () => FolderTreeSnapshotQueries.GetSelectedNodes(null, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("snapshot");
        }

        [TestMethod]
        public void GetArchiveRoot_UsesStoreAndRelativePath()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            var archive = FolderTreeSnapshotQueries.GetArchiveRoot(snapshot, "STORE-A", "archive");

            archive.DisplayName.Should().Be("Archive");
        }

        [TestMethod]
        public void GetArchiveRoot_MissingArchiveRoot_ReturnsNull()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            var archive = FolderTreeSnapshotQueries.GetArchiveRoot(snapshot, "store-a", "Missing");

            archive.Should().BeNull();
        }

        [TestMethod]
        public void GetArchiveRoot_NullRelativePath_ReturnsNull()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            var archive = FolderTreeSnapshotQueries.GetArchiveRoot(snapshot, "store-a", null);

            archive.Should().BeNull();
        }

        [TestMethod]
        public void EnumerateRelativePaths_ReturnsStoreScopedPaths()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            FolderTreeSnapshotQueries
                .EnumerateRelativePaths(snapshot, "store-a")
                .Should()
                .Equal("Archive", "Root");
        }

        [TestMethod]
        public void EnumerateRelativePaths_BlankStore_ReturnsAllStores()
        {
            var snapshot = CombinedSnapshot(
                Snapshot("store-a", "RootA", "ArchiveA"),
                Snapshot("store-b", "RootB", "ArchiveB")
            );

            FolderTreeSnapshotQueries
                .EnumerateRelativePaths(snapshot, " ")
                .Should()
                .Equal("ArchiveA", "ArchiveB", "RootA", "RootB");
        }

        [TestMethod]
        public void EnumerateRelativePaths_StoreFilterExcludesOtherStores()
        {
            var snapshot = CombinedSnapshot(
                Snapshot("store-a", "RootA", "ArchiveA"),
                Snapshot("store-b", "RootB", "ArchiveB")
            );

            FolderTreeSnapshotQueries
                .EnumerateRelativePaths(snapshot, "store-b")
                .Should()
                .Equal("ArchiveB", "RootB");
        }

        [TestMethod]
        public void GetCompareInputs_MatchesByRelativePath()
        {
            var current = Snapshot("store-a", "Root", "Archive");
            var other = Snapshot("store-b", "Root", "Archive");

            var pairs = FolderTreeSnapshotQueries.GetCompareInputs(current, other);

            pairs
                .Should()
                .Contain(pair => pair.Item1.RelativePath == "Archive" && pair.Item2 != null);
        }

        [TestMethod]
        public void GetCompareInputs_MissingMatches_ReturnsNullOtherNode()
        {
            var current = Snapshot("store-a", "Root", "Archive");
            var other = Snapshot("store-b", "OtherRoot", "OtherArchive");

            var pairs = FolderTreeSnapshotQueries.GetCompareInputs(current, other);

            pairs
                .Should()
                .Contain(pair => pair.Item1.RelativePath == "Archive" && pair.Item2 == null);
        }

        [TestMethod]
        public void GetCompareInputs_NullArguments_Throw()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");

            Action nullCurrent = () => FolderTreeSnapshotQueries.GetCompareInputs(null, snapshot);
            Action nullOther = () => FolderTreeSnapshotQueries.GetCompareInputs(snapshot, null);

            nullCurrent.Should().Throw<ArgumentNullException>().WithParameterName("current");
            nullOther.Should().Throw<ArgumentNullException>().WithParameterName("other");
        }

        [TestMethod]
        public void CreateSubtreeSnapshot_ProjectsRootAndDescendants()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");
            var root = snapshot.NodesByKey[snapshot.RootKeys[0]];

            var subtree = FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, root);

            subtree.RootKeys.Should().ContainSingle().Which.Should().Be(root.Key);
            subtree
                .NodesByKey.Values.Select(node => node.RelativePath)
                .Should()
                .Equal("Root", "Archive");
        }

        [TestMethod]
        public void CreateSubtreeSnapshot_NullArguments_Throw()
        {
            var snapshot = Snapshot("store-a", "Root", "Archive");
            var root = snapshot.NodesByKey[snapshot.RootKeys[0]];

            Action nullSnapshot = () => FolderTreeSnapshotQueries.CreateSubtreeSnapshot(null, root);
            Action nullRoot = () => FolderTreeSnapshotQueries.CreateSubtreeSnapshot(snapshot, null);

            nullSnapshot.Should().Throw<ArgumentNullException>().WithParameterName("snapshot");
            nullRoot.Should().Throw<ArgumentNullException>().WithParameterName("rootNode");
        }

        [TestMethod]
        public void CreateSubtreeSnapshot_SkipsMissingChildKeys()
        {
            var root = new FolderTreeNodeKey("store-a", "root", "\\Root");
            var missing = new FolderTreeNodeKey("store-a", "missing", "\\Root\\Missing");
            var snapshot = new FolderTreeSnapshot(
                new[] { root },
                new[] { Node(root, "Root", "Root", missing) }
            );

            var subtree = FolderTreeSnapshotQueries.CreateSubtreeSnapshot(
                snapshot,
                snapshot.NodesByKey[root]
            );

            subtree.NodesByKey.Values.Should().ContainSingle().Which.Key.Should().Be(root);
        }

        private static FolderTreeSnapshot CombinedSnapshot(
            FolderTreeSnapshot first,
            FolderTreeSnapshot second
        )
        {
            return new FolderTreeSnapshot(
                first.RootKeys.Concat(second.RootKeys),
                first.NodesByKey.Values.Concat(second.NodesByKey.Values)
            );
        }

        private static FolderTreeSnapshot Snapshot(
            string storeId,
            string rootPath,
            string childPath
        )
        {
            var root = new FolderTreeNodeKey(storeId, "root-" + storeId, "\\" + rootPath);
            var child = new FolderTreeNodeKey(
                storeId,
                "child-" + storeId,
                "\\" + rootPath + "\\" + childPath
            );
            return new FolderTreeSnapshot(
                new[] { root },
                new[] { Node(root, rootPath, rootPath, child), Node(child, childPath, childPath) }
            );
        }

        private static FolderTreeSnapshotNode Node(
            FolderTreeNodeKey key,
            string displayName,
            string relativePath,
            params FolderTreeNodeKey[] children
        )
        {
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                key.StoreId,
                key.EntryId,
                null,
                key.FolderPath,
                relativePath,
                children,
                false,
                string.Empty
            );
        }
    }
}
