using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class FolderWrapperNameCountSizeComparer_Tests
    {
        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenNameCountAndSizeMatchIgnoringCase()
        {
            // Arrange
            var comparer = new FolderWrapperNameCountSizeComparer();
            var left = CreateFolder("Inbox", itemCount: 4, folderSize: 512L);
            var right = CreateFolder("INBOX", itemCount: 4, folderSize: 512L);

            // Act
            bool result = comparer.Equals(left, right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenCountOrSizeDiffers()
        {
            // Arrange
            var comparer = new FolderWrapperNameCountSizeComparer();
            var baseline = CreateFolder("Inbox", itemCount: 4, folderSize: 512L);
            var differentCount = CreateFolder("Inbox", itemCount: 5, folderSize: 512L);
            var differentSize = CreateFolder("Inbox", itemCount: 4, folderSize: 513L);

            // Act
            bool countResult = comparer.Equals(baseline, differentCount);
            bool sizeResult = comparer.Equals(baseline, differentSize);

            // Assert
            countResult.Should().BeFalse();
            sizeResult.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenEitherOperandIsNull()
        {
            // Arrange
            var comparer = new FolderWrapperNameCountSizeComparer();
            var folder = CreateFolder("Inbox", itemCount: 0, folderSize: 0L);

            // Act / Assert
            comparer.Equals(null, folder).Should().BeFalse();
            comparer.Equals(folder, null).Should().BeFalse();
            comparer.Equals(folder, folder).Should().BeTrue();
        }

        [TestMethod]
        public void GetHashCode_ShouldIncludeBoundaryValuesAndIgnoreCase()
        {
            // Arrange
            var comparer = new FolderWrapperNameCountSizeComparer();
            var lowerCase = CreateFolder(
                "Archive",
                itemCount: int.MaxValue,
                folderSize: long.MaxValue
            );
            var upperCase = CreateFolder(
                "ARCHIVE",
                itemCount: int.MaxValue,
                folderSize: long.MaxValue
            );

            // Act
            int lowerCaseHash = comparer.GetHashCode(lowerCase);
            int upperCaseHash = comparer.GetHashCode(upperCase);
            int nullHash = comparer.GetHashCode(null);

            // Assert
            lowerCaseHash.Should().Be(upperCaseHash);
            nullHash.Should().Be(0);
        }

        private static FolderWrapper CreateFolder(string name, int itemCount, long folderSize)
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
