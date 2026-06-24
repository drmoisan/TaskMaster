using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class FolderTreeSelectionOverlayTests
    {
        [TestMethod]
        public void IsSelected_UsesCallerLocalRelativePaths()
        {
            var node = Node("Archive\\Inbox");
            var overlay = new FolderTreeSelectionOverlay(new[] { "archive\\inbox" });

            overlay.IsSelected(node).Should().BeTrue();
            node.IsStale.Should().BeFalse();
        }

        [TestMethod]
        public void IsSelected_NullNode_ReturnsFalse()
        {
            var overlay = new FolderTreeSelectionOverlay(new[] { "Archive\\Inbox" });

            overlay.IsSelected(null).Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_NullSelectedPathInput_CreatesEmptySelection()
        {
            var overlay = new FolderTreeSelectionOverlay(null);

            overlay.SelectedRelativePaths.Should().BeEmpty();
        }

        [TestMethod]
        public void WithSelection_ReturnsNewOverlayWithoutMutatingOriginal()
        {
            var original = new FolderTreeSelectionOverlay(new[] { "A" });

            var updated = original.WithSelection("B", selected: true);

            original.SelectedRelativePaths.Should().ContainSingle().Which.Should().Be("A");
            updated.SelectedRelativePaths.Should().BeEquivalentTo("A", "B");
        }

        [TestMethod]
        public void WithSelection_RemoveSelection_ReturnsOverlayWithoutPath()
        {
            var original = new FolderTreeSelectionOverlay(new[] { "A", "B" });

            var updated = original.WithSelection("a", selected: false);

            original.SelectedRelativePaths.Should().BeEquivalentTo("A", "B");
            updated.SelectedRelativePaths.Should().ContainSingle().Which.Should().Be("B");
        }

        private static FolderTreeSnapshotNode Node(string relativePath)
        {
            var key = new FolderTreeNodeKey("store-a", "entry-a", "\\Root\\" + relativePath);
            return new FolderTreeSnapshotNode(
                key,
                "Inbox",
                "store-a",
                "entry-a",
                null,
                key.FolderPath,
                relativePath,
                new FolderTreeNodeKey[0],
                false,
                string.Empty
            );
        }
    }
}
