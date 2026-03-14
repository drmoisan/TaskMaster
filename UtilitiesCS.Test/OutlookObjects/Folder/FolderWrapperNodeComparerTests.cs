using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperNodeComparerTests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNodesParentsAndChildrenMatch()
        {
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L), ("Sent", 2, 40L)]);
            var right = CreateTree("INBOX", parentName: "PROJECTS", childDescriptors: [("archive", 1, 20L), ("sent", 2, 40L)]);

            comparer.Equals(left, right).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenParentDiffers()
        {
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects");
            var right = CreateTree("Inbox", parentName: "Archive");

            comparer.Equals(left, right).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenChildCountsDifferAcrossTreeDepths()
        {
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L)]);
            var right = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L), ("Sent", 2, 40L)]);

            comparer.Equals(left, right).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNodeOrValueIsNull()
        {
            var comparer = new FolderWrapperNodeComparer();
            var validNode = CreateTree("Inbox", parentName: "Projects");
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);

            comparer.Equals(null, validNode).Should().BeFalse();
            comparer.Equals(validNode, null).Should().BeFalse();
            comparer.Equals(nullValueNode, validNode).Should().BeFalse();
            comparer.Equals(validNode, nullValueNode).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldReturnSameValue_ForEquivalentNodes()
        {
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L)]);
            var right = CreateTree("INBOX", parentName: "PROJECTS", childDescriptors: [("archive", 1, 20L)]);

            comparer.GetHashCode(left).Should().Be(comparer.GetHashCode(right));
            comparer.GetHashCode(new TreeNode<FolderWrapper>((FolderWrapper)null)).Should().Be(0);
        }

        private static TreeNode<FolderWrapper> CreateTree(
            string name,
            string parentName = null,
            (string name, int itemCount, long folderSize)[] childDescriptors = null)
        {
            var node = new TreeNode<FolderWrapper>(CreateFolder(name, 2, 200L));
            if (parentName is not null)
            {
                node.Parent = new TreeNode<FolderWrapper>(CreateFolder(parentName, 0, 0L));
            }

            foreach (var childDescriptor in childDescriptors ?? [])
            {
                node.Children.Add(new TreeNode<FolderWrapper>(CreateFolder(childDescriptor.name, childDescriptor.itemCount, childDescriptor.folderSize)));
            }

            return node;
        }

        private static FolderWrapper CreateFolder(string name, int itemCount, long folderSize)
        {
            return new FolderWrapper(selected: false, itemCount: itemCount, folderSize: folderSize, name: name, relativePath: name ?? string.Empty);
        }
    }
}