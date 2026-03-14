using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperNodeContentsComparerTests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNodeValuesMatch()
        {
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            var right = new TreeNode<FolderWrapper>(CreateFolder("INBOX", 3, 120L));

            comparer.Equals(left, right).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenNodeValuesDiffer()
        {
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            var right = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 4, 120L));

            comparer.Equals(left, right).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldIgnoreChildrenAndReturnFalse_ForNullInputs()
        {
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            left.Children.Add(new TreeNode<FolderWrapper>(CreateFolder("Child", 1, 10L)));
            var right = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            var nullValueNode = new TreeNode<FolderWrapper>((FolderWrapper)null);

            comparer.Equals(left, right).Should().BeTrue();
            comparer.Equals(null, right).Should().BeFalse();
            comparer.Equals(left, null).Should().BeFalse();
            comparer.Equals(nullValueNode, right).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldUseOnlyNodeContents()
        {
            var comparer = new FolderWrapperNodeContentsComparer();
            var left = new TreeNode<FolderWrapper>(CreateFolder("Inbox", 3, 120L));
            left.Children.Add(new TreeNode<FolderWrapper>(CreateFolder("Child", 1, 10L)));
            var right = new TreeNode<FolderWrapper>(CreateFolder("INBOX", 3, 120L));

            comparer.GetHashCode(left).Should().Be(comparer.GetHashCode(right));
            comparer.GetHashCode(new TreeNode<FolderWrapper>((FolderWrapper)null)).Should().Be(0);
        }

        private static FolderWrapper CreateFolder(string name, int itemCount, long folderSize)
        {
            return new FolderWrapper(selected: false, itemCount: itemCount, folderSize: folderSize, name: name, relativePath: name ?? string.Empty);
        }
    }
}