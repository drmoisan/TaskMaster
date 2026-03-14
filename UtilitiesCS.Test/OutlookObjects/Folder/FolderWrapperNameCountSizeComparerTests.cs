using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperNameCountSizeComparerTests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNameCountAndSizeMatchIgnoringCase()
        {
            var comparer = new FolderWrapperNameCountSizeComparer();
            var left = CreateFolder("Inbox", itemCount: 4, folderSize: 512L);
            var right = CreateFolder("INBOX", itemCount: 4, folderSize: 512L);

            comparer.Equals(left, right).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenCountOrSizeDiffers()
        {
            var comparer = new FolderWrapperNameCountSizeComparer();
            var baseline = CreateFolder("Inbox", itemCount: 4, folderSize: 512L);
            var differentCount = CreateFolder("Inbox", itemCount: 5, folderSize: 512L);
            var differentSize = CreateFolder("Inbox", itemCount: 4, folderSize: 513L);

            comparer.Equals(baseline, differentCount).Should().BeFalse();
            comparer.Equals(baseline, differentSize).Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenEitherOperandIsNull()
        {
            var comparer = new FolderWrapperNameCountSizeComparer();
            var folder = CreateFolder("Inbox", itemCount: 0, folderSize: 0L);

            comparer.Equals(null, folder).Should().BeFalse();
            comparer.Equals(folder, null).Should().BeFalse();
            comparer.Equals(folder, folder).Should().BeTrue();
        }

        [TestMethod]
        public void GetHashCode_ShouldIncludeBoundaryValuesAndIgnoreCase()
        {
            var comparer = new FolderWrapperNameCountSizeComparer();
            var lowerCase = CreateFolder("Archive", itemCount: int.MaxValue, folderSize: long.MaxValue);
            var upperCase = CreateFolder("ARCHIVE", itemCount: int.MaxValue, folderSize: long.MaxValue);

            comparer.GetHashCode(lowerCase).Should().Be(comparer.GetHashCode(upperCase));
            comparer.GetHashCode(null).Should().Be(0);
        }

        private static FolderWrapper CreateFolder(string name, int itemCount, long folderSize)
        {
            return new FolderWrapper(selected: false, itemCount: itemCount, folderSize: folderSize, name: name, relativePath: name ?? string.Empty);
        }
    }
}