using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

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

        [TestMethod]
        public async Task CompareItemsAsync_WhenHelpersAreLoaded_ReturnsMatchingAndExclusiveItems()
        {
            var shared = CreateItemInfo("shared");
            var currentExclusive = CreateItemInfo("current");
            var otherExclusive = CreateItemInfo("other");
            var globals = new Mock<IApplicationGlobals>().Object;
            var current = CreateFolderWrapper(itemCount: 2);
            var other = CreateFolderWrapper(itemCount: 2);
            current.Globals = globals;
            other.Globals = globals;
            current.ItemHelpers = CreateAsyncLazy(shared, currentExclusive);
            other.ItemHelpers = CreateAsyncLazy(shared, otherExclusive);

            var (matching, currentOnly, otherOnly) = await current.CompareItemsAsync(
                other,
                CancellationToken.None
            );

            matching.Should().ContainSingle().Which.Should().BeSameAs(shared);
            currentOnly.Should().ContainSingle().Which.Should().BeSameAs(currentExclusive);
            otherOnly.Should().ContainSingle().Which.Should().BeSameAs(otherExclusive);
        }

        [TestMethod]
        public async Task CalculateItemMatchPercentageAsync_WhenHelpersAreLoaded_ReturnsExpectedRatio()
        {
            var shared = CreateItemInfo("shared");
            var currentExclusive = CreateItemInfo("current");
            var otherExclusive = CreateItemInfo("other");
            var globals = new Mock<IApplicationGlobals>().Object;
            var current = CreateFolderWrapper(itemCount: 2);
            var other = CreateFolderWrapper(itemCount: 2);
            current.Globals = globals;
            other.Globals = globals;
            current.ItemHelpers = CreateAsyncLazy(shared, currentExclusive);
            other.ItemHelpers = CreateAsyncLazy(shared, otherExclusive);

            var result = await current.CalculateItemMatchPercentageAsync(
                other,
                CancellationToken.None
            );

            result.Should().Be(0.5);
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
            return Enumerable
                .Range(0, count)
                .Select(index => CreateItemInfo($"item-{index}"))
                .ToArray();
        }

        private static AsyncLazy<IItemInfo[]> CreateAsyncLazy(params IItemInfo[] items)
        {
            return new AsyncLazy<IItemInfo[]>(() => items);
        }

        private static IItemInfo CreateItemInfo(string key)
        {
            var sender = new Mock<IRecipientInfo>().Object;
            return new ItemInfo
            {
                Subject = key,
                Body = key,
                Sender = sender,
                CcRecipients = Array.Empty<IRecipientInfo>(),
                ToRecipients = Array.Empty<IRecipientInfo>(),
                SentDate = new DateTime(2026, 3, 19),
            };
        }
    }
}
