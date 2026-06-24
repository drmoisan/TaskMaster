using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeCompatibilityViewDisposalTests
    {
        [TestMethod]
        public void Dispose_UnsubscribesTrackedWrapperHandlers()
        {
            var view = new FolderTreeCompatibilityView(
                CreateSnapshot(),
                new FolderTreeSelectionOverlay(new string[0])
            );

            view.SubscriptionCount.Should().Be(2);
            view.Dispose();

            view.SubscriptionCount.Should().Be(0);
        }

        [TestMethod]
        public void Dispose_RepeatedViewLifetimes_DoNotAccumulateHandlers()
        {
            var first = new FolderTreeCompatibilityView(
                CreateSnapshot(),
                new FolderTreeSelectionOverlay(new string[0])
            );
            first.Dispose();
            var second = new FolderTreeCompatibilityView(
                CreateSnapshot(),
                new FolderTreeSelectionOverlay(new string[0])
            );

            second.SubscriptionCount.Should().Be(2);
            second.Dispose();
            second.SubscriptionCount.Should().Be(0);
        }

        private static FolderTreeSnapshot CreateSnapshot()
        {
            var root = new FolderTreeNodeKey("store-a", "root", "\\Root");
            var child = new FolderTreeNodeKey("store-a", "child", "\\Root\\Child");
            return new FolderTreeSnapshot(
                new[] { root },
                new[] { Node(root, "Root", "Root", child), Node(child, "Child", "Root\\Child") }
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
                "store-a",
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
