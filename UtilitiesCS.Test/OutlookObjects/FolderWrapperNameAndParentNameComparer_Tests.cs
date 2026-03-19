using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class FolderWrapperNameAndParentNameComparer_Tests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNameAndParentNameMatchIgnoringCase()
        {
            // Arrange
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var left = CreateNode("Inbox", parentName: "Projects");
            var right = CreateNode("INBOX", parentName: "projects");

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNamesDiffer()
        {
            // Arrange
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var left = CreateNode("Inbox", parentName: "Projects");
            var right = CreateNode("Archive", parentName: "Projects");

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenParentNamesDiffer()
        {
            // Arrange
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var left = CreateNode("Inbox", parentName: "Projects");
            var right = CreateNode("Inbox", parentName: "Archive");

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldHandleNullNodesAndMissingParents()
        {
            // Arrange
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var rootNode = CreateNode("Inbox");
            var childNode = CreateNode("Inbox", parentName: "Projects");

            // Act / Assert
            comparer.Equals(null, rootNode).Should().BeFalse();
            comparer.Equals(rootNode, null).Should().BeFalse();
            comparer.Equals(rootNode, CreateNode("Inbox")).Should().BeTrue();
            comparer.Equals(rootNode, childNode).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldCombineNameAndParentNameConsistently()
        {
            // Arrange
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var mixedCase = CreateNode("Inbox", parentName: "Projects");
            var upperCase = CreateNode("INBOX", parentName: "PROJECTS");
            var nullNode = (TreeNode<FolderWrapper>)null;

            // Act
            int mixedCaseHash = comparer.GetHashCode(mixedCase);
            int upperCaseHash = comparer.GetHashCode(upperCase);
            int nullHash = comparer.GetHashCode(nullNode);

            // Assert
            mixedCaseHash.Should().Be(upperCaseHash);
            nullHash.Should().Be(0);
        }

        private static TreeNode<FolderWrapper> CreateNode(string name, string parentName = null)
        {
            var node = new TreeNode<FolderWrapper>(CreateFolder(name));
            if (parentName is not null)
            {
                node.Parent = new TreeNode<FolderWrapper>(CreateFolder(parentName));
            }

            return node;
        }

        private static FolderWrapper CreateFolder(
            string name,
            int itemCount = 0,
            long folderSize = 0L
        )
        {
            return new FolderWrapper(
                selected: false,
                itemCount: itemCount,
                folderSize: folderSize,
                name: name,
                relativePath: name ?? string.Empty
            );
        }
    }
}
