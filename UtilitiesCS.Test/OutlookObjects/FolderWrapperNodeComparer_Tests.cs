using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class FolderWrapperNodeComparer_Tests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNodesParentsAndChildrenMatch()
        {
            // Arrange
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L), ("Sent", 2, 40L)]);
            var right = CreateTree("INBOX", parentName: "PROJECTS", childDescriptors: [("archive", 1, 20L), ("sent", 2, 40L)]);

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenParentDiffers()
        {
            // Arrange
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects");
            var right = CreateTree("Inbox", parentName: "Archive");

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenChildCountsDifferAcrossTreeDepths()
        {
            // Arrange
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L)]);
            var right = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L), ("Sent", 2, 40L)]);

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNodeOrValueIsNull()
        {
            // Arrange
            var comparer = new FolderWrapperNodeComparer();
            var validNode = CreateTree("Inbox", parentName: "Projects");
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);

            // Act / Assert
            comparer.Equals(null, validNode).Should().BeFalse();
            comparer.Equals(validNode, null).Should().BeFalse();
            comparer.Equals(nullValueNode, validNode).Should().BeFalse();
            comparer.Equals(validNode, nullValueNode).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldReturnSameValue_ForEquivalentNodes()
        {
            // Arrange
            var comparer = new FolderWrapperNodeComparer();
            var left = CreateTree("Inbox", parentName: "Projects", childDescriptors: [("Archive", 1, 20L)]);
            var right = CreateTree("INBOX", parentName: "PROJECTS", childDescriptors: [("archive", 1, 20L)]);

            // Act
            int leftHash = comparer.GetHashCode(left);
            int rightHash = comparer.GetHashCode(right);
            int nullHash = comparer.GetHashCode(new TreeNode<FolderWrapper>((FolderWrapper)null));

            // Assert
            leftHash.Should().Be(rightHash);
            nullHash.Should().Be(0);
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
