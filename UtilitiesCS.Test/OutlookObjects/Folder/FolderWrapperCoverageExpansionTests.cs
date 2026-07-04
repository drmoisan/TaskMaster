using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Additional Moq-backed coverage for FolderWrapper construction and traversal paths.
    /// </summary>
    [TestClass]
    public class FolderWrapperCoverageExpansionTests
    {
        [TestMethod]
        public void Constructor_WhenFolderAndRootAreProvided_LoadsNameAndRelativePath()
        {
            // Arrange
            var root = CreateOutlookFolder(@"\\Mailbox");
            var folder = CreateOutlookFolder(@"\\Mailbox\Projects", itemCount: 3);

            // Act
            var wrapper = new FolderWrapper(folder.Object, root.Object);

            // Assert
            wrapper.Name.Should().Be("Projects");
            wrapper.RelativePath.Should().Be("Projects");
            wrapper.ItemCount.Should().Be(3);
        }

        [TestMethod]
        public void FolderNameAccess_WhenFolderIsChanged_ReloadsLazyName()
        {
            // Arrange
            var root = CreateOutlookFolder(@"\\Mailbox");
            var original = CreateOutlookFolder(@"\\Mailbox\Projects");
            var updated = CreateOutlookFolder(@"\\Mailbox\Archive");
            var wrapper = new FolderWrapper(original.Object, root.Object);
            wrapper.Name.Should().Be("Projects");

            // Act
            wrapper.OlFolder = updated.Object;

            // Assert
            wrapper.Name.Should().Be("Archive");
            wrapper.RelativePath.Should().Be("Archive");
        }

        [TestMethod]
        public void LoadItemCountSubFolders_WhenChildrenExist_ReturnsRecursiveTotal()
        {
            // Arrange
            var grandchild = CreateOutlookFolder(@"\\Mailbox\Projects\FY26", itemCount: 4);
            var child = CreateOutlookFolder(
                @"\\Mailbox\Projects",
                itemCount: 2,
                new Dictionary<string, OutlookFolder> { ["FY26"] = grandchild.Object }
            );
            var root = CreateOutlookFolder(
                @"\\Mailbox",
                itemCount: 1,
                new Dictionary<string, OutlookFolder> { ["Projects"] = child.Object }
            );
            var wrapper = new FolderWrapper(root.Object, root.Object);

            // Act
            var result = wrapper.LoadItemCountSubFolders();

            // Assert
            result.Should().Be(7);
        }

        [TestMethod]
        public void LoadRelativePath_WhenFolderIsNull_ReturnsNull()
        {
            // Arrange
            var root = CreateOutlookFolder(@"\\Mailbox");
            var wrapper = new FolderWrapper(null, root.Object);

            // Act
            var result = wrapper.LoadRelativePath();

            // Assert
            result.Should().BeNull();
            wrapper.LoadName().Should().BeNull();
        }

        [TestMethod]
        public async Task CompareItemsAsync_WhenGlobalsIsMissing_ThrowsArgumentNullException()
        {
            // Arrange
            var root = CreateOutlookFolder(@"\\Mailbox", itemCount: 1);
            var current = new FolderWrapper(root.Object, root.Object);
            var other = new FolderWrapper(root.Object, root.Object);

            // Act
            Func<Task> act = () => current.CompareItemsAsync(other, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ArgumentNullException>();
            exception.Which.ParamName.Should().Be("Globals");
        }

        private static Mock<OutlookFolder> CreateOutlookFolder(
            string folderPath,
            int itemCount = 0,
            IDictionary<string, OutlookFolder> childFolders = null,
            params object[] items
        )
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Items).Returns(CreateItems(itemCount, items).Object);
            folder
                .SetupGet(x => x.Folders)
                .Returns(
                    CreateFolders(childFolders ?? new Dictionary<string, OutlookFolder>()).Object
                );
            return folder;
        }

        private static Mock<OutlookItems> CreateItems(int count, params object[] items)
        {
            var outlookItems = new Mock<OutlookItems>();
            var enumerableItems = new ArrayList(items ?? Array.Empty<object>());
            outlookItems.SetupGet(x => x.Count).Returns(count);
            outlookItems
                .Setup(x => x.GetEnumerator())
                .Returns(() => enumerableItems.GetEnumerator());
            return outlookItems;
        }

        private static Mock<OutlookFolders> CreateFolders(
            IDictionary<string, OutlookFolder> foldersByName
        )
        {
            var folders = new Mock<OutlookFolders>();
            var enumerableFolders = new ArrayList(foldersByName.Values.Cast<object>().ToArray());
            folders.Setup(x => x.GetEnumerator()).Returns(() => enumerableFolders.GetEnumerator());
            folders
                .Setup(x => x[It.IsAny<object>()])
                .Returns<object>(key =>
                    key is string name && foldersByName.TryGetValue(name, out var folder)
                        ? folder
                        : null
                );
            return folders;
        }

        private static string GetLeafName(string folderPath)
        {
            return folderPath.Split('\\')[^1];
        }
    }
}
