using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSnapshotNodeTests
    {
        [TestMethod]
        public void Constructor_CopiesChildKeys()
        {
            var child = new FolderTreeNodeKey("store-a", "child", "\\Root\\Child");
            var children = new List<FolderTreeNodeKey> { child };

            var node = CreateNode(childKeys: children);
            children.Clear();

            node.ChildKeys.Should().ContainSingle().Which.Should().Be(child);
        }

        [TestMethod]
        public void Constructor_NullOptionalValues_StoresEmptyCollectionsAndText()
        {
            var node = CreateNode(childKeys: null, staleReason: null);

            node.EntryId.Should().Be("entry-a");
            node.ParentKey.Should().BeNull();
            node.ChildKeys.Should().BeEmpty();
            node.StaleReason.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_PreservesSnapshotMetadata()
        {
            var parent = new FolderTreeNodeKey("store-a", "parent", "\\Root");
            var node = CreateNode(parentKey: parent, isStale: true, staleReason: "folder changed");

            node.DisplayName.Should().Be("Inbox");
            node.StoreId.Should().Be("store-a");
            node.EntryId.Should().Be("entry-a");
            node.ParentKey.Should().Be(parent);
            node.FolderPath.Should().Be("\\Root\\Inbox");
            node.RelativePath.Should().Be("Inbox");
            node.IsStale.Should().BeTrue();
            node.StaleReason.Should().Be("folder changed");
        }

        [TestMethod]
        public void MarkStale_ReturnsNewNodeWithStaleMetadata()
        {
            var node = CreateNode();

            var stale = node.MarkStale("store removed");

            stale.Should().NotBeSameAs(node);
            stale.IsStale.Should().BeTrue();
            stale.StaleReason.Should().Be("store removed");
            node.IsStale.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_BlankDisplayName_Throws()
        {
            Action act = () => CreateNode(displayName: " ");

            act.Should().Throw<ArgumentException>().WithParameterName("displayName");
        }

        [TestMethod]
        public void Constructor_InvalidRequiredValues_Throw()
        {
            Action nullKey = () =>
                new FolderTreeSnapshotNode(
                    null,
                    "Inbox",
                    "store-a",
                    "entry-a",
                    null,
                    "\\Root\\Inbox",
                    "Inbox",
                    null,
                    false,
                    string.Empty
                );
            Action blankStore = () => CreateNode(storeId: " ");
            Action blankFolderPath = () => CreateNode(folderPath: " ");

            nullKey.Should().Throw<ArgumentNullException>().WithParameterName("key");
            blankStore.Should().Throw<ArgumentException>().WithParameterName("storeId");
            blankFolderPath.Should().Throw<ArgumentException>().WithParameterName("folderPath");
        }

        private static FolderTreeSnapshotNode CreateNode(
            string displayName = "Inbox",
            string storeId = "store-a",
            string folderPath = "\\Root\\Inbox",
            FolderTreeNodeKey parentKey = null,
            IEnumerable<FolderTreeNodeKey> childKeys = null,
            bool isStale = false,
            string staleReason = ""
        )
        {
            var key = new FolderTreeNodeKey("store-a", "entry-a", "\\Root\\Inbox");
            return new FolderTreeSnapshotNode(
                key,
                displayName,
                storeId,
                "entry-a",
                parentKey,
                folderPath,
                "Inbox",
                childKeys,
                isStale,
                staleReason
            );
        }
    }
}
