using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperTraversalTests
    {
        [TestMethod]
        public void CalculateItemMatchPercentage_WhenMatchingIsNull_ReturnsZero()
        {
            var folderWrapper = CreateFolderWrapper();

            var result = folderWrapper.CalculateItemMatchPercentage(
                matching: null,
                currentOnly: Array.Empty<IItemInfo>(),
                otherOnly: Array.Empty<IItemInfo>()
            );

            result.Should().Be(0.0);
        }

        [TestMethod]
        public void CalculateItemMatchPercentage_WhenAllInputsAreEmpty_ReturnsZero()
        {
            var folderWrapper = CreateFolderWrapper();

            var result = folderWrapper.CalculateItemMatchPercentage(
                matching: Array.Empty<IItemInfo>(),
                currentOnly: Array.Empty<IItemInfo>(),
                otherOnly: Array.Empty<IItemInfo>()
            );

            result.Should().Be(0.0);
        }

        [TestMethod]
        public void CalculateItemMatchPercentage_WhenAllItemsMatch_ReturnsOne()
        {
            var folderWrapper = CreateFolderWrapper();

            var result = folderWrapper.CalculateItemMatchPercentage(
                matching: CreateItemInfos(2),
                currentOnly: Array.Empty<IItemInfo>(),
                otherOnly: Array.Empty<IItemInfo>()
            );

            result.Should().Be(1.0);
        }

        [TestMethod]
        public void CalculateItemMatchPercentage_WhenOneMatchesAndTwoDoNot_ReturnsPointFive()
        {
            var folderWrapper = CreateFolderWrapper();

            var result = folderWrapper.CalculateItemMatchPercentage(
                matching: CreateItemInfos(1),
                currentOnly: CreateItemInfos(1),
                otherOnly: CreateItemInfos(1)
            );

            result.Should().Be(0.5);
        }

        [TestMethod]
        public async Task CompareItemsAsync_WhenGlobalsIsNull_ThrowsArgumentNullException()
        {
            var current = CreateFolderWrapper(itemCount: 1);
            var other = CreateFolderWrapper(itemCount: 1);
            Func<Task> act = () => current.CompareItemsAsync(other, CancellationToken.None);

            var exception = await act.Should().ThrowAsync<ArgumentNullException>();

            exception.Which.ParamName.Should().Be("Globals");
        }

        private static FolderWrapper CreateFolderWrapper(int itemCount = 0)
        {
            return new FolderWrapper(
                selected: false,
                itemCount: itemCount,
                folderSize: 0L,
                name: "Folder",
                relativePath: "Folder"
            );
        }

        private static IItemInfo[] CreateItemInfos(int count)
        {
            return Enumerable.Range(0, count).Select(_ => new Mock<IItemInfo>().Object).ToArray();
        }
    }
}
