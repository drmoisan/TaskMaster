using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperNameAndParentNameComparerTests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNameAndParentNameMatchIgnoringCase()
        {
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var left = CreateNode("Inbox", parentName: "Projects");
            var right = CreateNode("INBOX", parentName: "projects");

            comparer.Equals(left, right).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNamesDiffer()
        {
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var left = CreateNode("Inbox", parentName: "Projects");
            var right = CreateNode("Archive", parentName: "Projects");

            comparer.Equals(left, right).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenParentNamesDiffer()
        {
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var left = CreateNode("Inbox", parentName: "Projects");
            var right = CreateNode("Inbox", parentName: "Archive");

            comparer.Equals(left, right).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldHandleNullNodesAndMissingParents()
        {
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var rootNode = CreateNode("Inbox");
            var childNode = CreateNode("Inbox", parentName: "Projects");

            comparer.Equals(null, rootNode).Should().BeFalse();
            comparer.Equals(rootNode, null).Should().BeFalse();
            comparer.Equals(rootNode, CreateNode("Inbox")).Should().BeTrue();
            comparer.Equals(rootNode, childNode).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldCombineNameAndParentNameConsistently()
        {
            var comparer = new FolderWrapperNameAndParentNameComparer();
            var mixedCase = CreateNode("Inbox", parentName: "Projects");
            var upperCase = CreateNode("INBOX", parentName: "PROJECTS");

            comparer.GetHashCode(mixedCase).Should().Be(comparer.GetHashCode(upperCase));
            comparer.GetHashCode(null).Should().Be(0);
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

        private static FolderWrapper CreateFolder(string name, int itemCount = 0, long folderSize = 0L)
        {
            return new FolderWrapper(selected: false, itemCount: itemCount, folderSize: folderSize, name: name, relativePath: name ?? string.Empty);
        }
    }
}