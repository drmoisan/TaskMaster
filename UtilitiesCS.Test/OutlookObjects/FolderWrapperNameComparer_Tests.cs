using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class FolderWrapperNameComparer_Tests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenComparingSameReference()
        {
            // Arrange
            var comparer = new FolderWrapperNameComparer();
            var node = CreateNode("Inbox");

            // Act
            bool result = comparer.Equals(node, node);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNodeNamesMatchExactly()
        {
            // Arrange
            var comparer = new FolderWrapperNameComparer();
            var left = CreateNode("Projects");
            var right = CreateNode("Projects");

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNamesDifferOrCaseDoesNotMatch()
        {
            // Arrange
            var comparer = new FolderWrapperNameComparer();
            var differentName = CreateNode("Archive");
            var otherName = CreateNode("Inbox");
            var differentCase = CreateNode("inbox");

            // Act
            bool differentNameResult = comparer.Equals(differentName, otherName);
            bool differentCaseResult = comparer.Equals(otherName, differentCase);

            // Assert
            differentNameResult.Should().BeFalse();
            differentCaseResult.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenEitherNodeOrFolderNameIsNullOrEmpty()
        {
            // Arrange
            var comparer = new FolderWrapperNameComparer();
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);
            var nullNameNode = CreateNode(null);
            var emptyNameNode = CreateNode(string.Empty);
            var namedNode = CreateNode("Inbox");

            // Act / Assert
            comparer.Equals(nullValueNode, namedNode).Should().BeFalse();
            comparer.Equals(namedNode, nullValueNode).Should().BeFalse();
            comparer.Equals(nullNameNode, namedNode).Should().BeFalse();
            comparer.Equals(emptyNameNode, namedNode).Should().BeFalse();
            comparer.Equals(null, namedNode).Should().BeFalse();
            comparer.Equals(namedNode, null).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldUseLowerInvariantName_AndSupportSpecialCharacters()
        {
            // Arrange
            var comparer = new FolderWrapperNameComparer();
            var mixedCase = CreateNode("Fólder-№1");
            var upperCase = CreateNode("FÓLDER-№1");
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);

            // Act
            int mixedCaseHash = comparer.GetHashCode(mixedCase);
            int upperCaseHash = comparer.GetHashCode(upperCase);
            int nullHash = comparer.GetHashCode(nullValueNode);

            // Assert
            mixedCaseHash.Should().Be(upperCaseHash);
            nullHash.Should().Be(0);
        }

        private static TreeNode<FolderWrapper> CreateNode(string name)
        {
            var folder = new FolderWrapper(
                selected: false,
                itemCount: 0,
                folderSize: 0L,
                name: name,
                relativePath: name ?? string.Empty
            );
            return new TreeNode<FolderWrapper>(folder);
        }
    }
}
