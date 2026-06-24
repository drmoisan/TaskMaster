using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class OutlookFolderHandleResolverTests
    {
        [TestMethod]
        public void TryResolve_LookupReturnsHandle_ReturnsTrue()
        {
            var handle = new object();
            var lookup = new Mock<OutlookFolderHandleResolver.IFolderLookup>();
            lookup.Setup(item => item.GetFolderFromId("entry-a", "store-a")).Returns(handle);
            var resolver = new OutlookFolderHandleResolver(lookup.Object);

            resolver.TryResolve(Node(), out var resolved).Should().BeTrue();

            resolved.Should().BeSameAs(handle);
        }

        [TestMethod]
        public void TryResolve_LookupReturnsNull_ReturnsFalse()
        {
            var lookup = new Mock<OutlookFolderHandleResolver.IFolderLookup>();
            var resolver = new OutlookFolderHandleResolver(lookup.Object);

            resolver.TryResolve(Node(), out var resolved).Should().BeFalse();

            resolved.Should().BeNull();
        }

        [TestMethod]
        public void Resolve_LookupReturnsNull_Throws()
        {
            var lookup = new Mock<OutlookFolderHandleResolver.IFolderLookup>();
            var resolver = new OutlookFolderHandleResolver(lookup.Object);

            Action act = () => resolver.Resolve(Node());

            act.Should().Throw<InvalidOperationException>();
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
                Array.Empty<FolderTreeNodeKey>(),
                false,
                string.Empty
            );
        }
    }
}
