using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.Test.OutlookObjects.Folder.Fakes;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderHandleResolverFakeTests
    {
        [TestMethod]
        public void Resolve_ConfiguredNode_ReturnsConfiguredHandle()
        {
            var node = Node();
            var handle = new object();
            var resolver = new FakeFolderHandleResolver().Add(node.Key, handle);

            resolver.Resolve(node).Should().BeSameAs(handle);
            resolver.ResolveCount.Should().Be(1);
        }

        [TestMethod]
        public void TryResolve_MissingNode_ReturnsFalse()
        {
            var resolver = new FakeFolderHandleResolver();

            resolver.TryResolve(Node(), out var handle).Should().BeFalse();
            handle.Should().BeNull();
        }

        [TestMethod]
        public void Resolve_MissingNode_Throws()
        {
            var resolver = new FakeFolderHandleResolver();

            resolver.Invoking(item => item.Resolve(Node())).Should().Throw<KeyNotFoundException>();
        }

        private static FolderTreeSnapshotNode Node()
        {
            var key = new FolderTreeNodeKey("store-a", "entry-a", "\\Inbox");
            return new FolderTreeSnapshotNode(
                key,
                "Inbox",
                "store-a",
                "entry-a",
                null,
                "\\Inbox",
                "Inbox",
                new FolderTreeNodeKey[0],
                false,
                string.Empty
            );
        }
    }
}
