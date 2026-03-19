using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperNameComparerTests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenComparingSameReference()
        {
            var comparer = new FolderWrapperNameComparer();
            var node = CreateNode("Inbox");

            comparer.Equals(node, node).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNodeNamesMatchExactly()
        {
            var comparer = new FolderWrapperNameComparer();
            var left = CreateNode("Projects");
            var right = CreateNode("Projects");

            comparer.Equals(left, right).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNamesDifferOrCaseDoesNotMatch()
        {
            var comparer = new FolderWrapperNameComparer();
            var differentName = CreateNode("Archive");
            var otherName = CreateNode("Inbox");
            var differentCase = CreateNode("inbox");

            comparer.Equals(differentName, otherName).Should().BeFalse();
            comparer.Equals(otherName, differentCase).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenEitherNodeOrFolderNameIsNullOrEmpty()
        {
            var comparer = new FolderWrapperNameComparer();
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);
            var nullNameNode = CreateNode(null);
            var emptyNameNode = CreateNode(string.Empty);
            var namedNode = CreateNode("Inbox");

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
            var comparer = new FolderWrapperNameComparer();
            var mixedCase = CreateNode("Fólder-№1");
            var upperCase = CreateNode("FÓLDER-№1");
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);

            comparer.GetHashCode(mixedCase).Should().Be(comparer.GetHashCode(upperCase));
            comparer.GetHashCode(nullValueNode).Should().Be(0);
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
