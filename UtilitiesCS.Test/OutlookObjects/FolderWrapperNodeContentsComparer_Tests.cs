using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class FolderWrapperNodeContentsComparer_Tests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNodeValuesMatch()
        {
            // Arrange
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            var right = new TreeNode<FolderWrapper>(CreateFolder("INBOX", 3, 120L));

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNodeValuesDiffer()
        {
            // Arrange
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            var right = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 4, 120L));

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldIgnoreChildrenAndReturnFalse_ForNullInputs()
        {
            // Arrange
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            left.Children.Add(new TreeNode<FolderWrapper>(CreateFolder("Child", 1, 10L)));
            var right = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);

            // Act / Assert
            comparer.Equals(left, right).Should().BeTrue();
            comparer.Equals(null, right).Should().BeFalse();
            comparer.Equals(left, null).Should().BeFalse();
            comparer.Equals(nullValueNode, right).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldUseOnlyNodeContents()
        {
            // Arrange
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            left.Children.Add(new TreeNode<FolderWrapper>(CreateFolder("Child", 1, 10L)));
            var right = new TreeNode<FolderWrapper>(CreateFolder("INBOX", 3, 120L));

            // Act
            int leftHash = comparer.GetHashCode(left);
            int rightHash = comparer.GetHashCode(right);
            int nullHash = comparer.GetHashCode(new TreeNode<FolderWrapper>((FolderWrapper)null));

            // Assert
            leftHash.Should().Be(rightHash);
            nullHash.Should().Be(0);
        }

        private static FolderWrapper CreateFolder(string name, int itemCount, long folderSize)
        {
            return new FolderWrapper(selected: false, itemCount: itemCount, folderSize: folderSize, name: name, relativePath: name ?? string.Empty);
        }
    }
}
