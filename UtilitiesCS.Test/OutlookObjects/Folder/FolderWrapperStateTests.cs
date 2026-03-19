using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperStateTests
    {
        [TestMethod]
        public void JsonCtor_InitializesSerializedState()
        {
            var folderWrapper = CreateJsonFolderWrapper(
                selected: true,
                itemCount: 7,
                folderSize: 4096L,
                name: "Inbox",
                relativePath: @"Archive\Inbox"
            );

            folderWrapper.Selected.Should().BeTrue();
            folderWrapper.ItemCount.Should().Be(7);
            folderWrapper.FolderSize.Should().Be(4096L);
            folderWrapper.Name.Should().Be("Inbox");
            folderWrapper.RelativePath.Should().Be(@"Archive\Inbox");
        }

        [TestMethod]
        public void Selected_Setter_RaisesPropertyChangedForSelected()
        {
            var folderWrapper = CreateJsonFolderWrapper();
            string propertyName = null;
            folderWrapper.PropertyChanged += (_, args) => propertyName = args.PropertyName;

            folderWrapper.Selected = true;

            propertyName.Should().Be(nameof(FolderWrapper.Selected));
        }

        [TestMethod]
        public void SubscribeToPropertyChanged_WhenAllRequested_SetsFullSubscriptionStatus()
        {
            var folderWrapper = CreateJsonFolderWrapper();
            folderWrapper.UnSubscribeToPropertyChanged(IFolderWrapper.PropertyEnum.All);

            folderWrapper.SubscribeToPropertyChanged(IFolderWrapper.PropertyEnum.All);

            folderWrapper.SubscriptionStatus.Should().Be(IFolderWrapper.PropertyEnum.All);
        }

        [TestMethod]
        public void UnSubscribeToPropertyChanged_WhenSpecificFlagsRequested_ClearsOnlyRequestedFlags()
        {
            var folderWrapper = CreateJsonFolderWrapper();

            folderWrapper.UnSubscribeToPropertyChanged(
                IFolderWrapper.PropertyEnum.FolderSize | IFolderWrapper.PropertyEnum.Name
            );

            folderWrapper
                .SubscriptionStatus.Should()
                .Be(
                    IFolderWrapper.PropertyEnum.All
                        & ~IFolderWrapper.PropertyEnum.FolderSize
                        & ~IFolderWrapper.PropertyEnum.Name
                );
        }

        [TestMethod]
        public void NotifyPropertyChanged_RaisesPropertyChangedForProvidedPropertyName()
        {
            var folderWrapper = CreateJsonFolderWrapper();
            string propertyName = null;
            folderWrapper.PropertyChanged += (_, args) => propertyName = args.PropertyName;

            folderWrapper.NotifyPropertyChanged(nameof(FolderWrapper.RelativePath));

            propertyName.Should().Be(nameof(FolderWrapper.RelativePath));
        }

        [TestMethod]
        public void LoadRelativePath_WhenOlFolderAndOlRootAreNull_ReturnsNull()
        {
            var folderWrapper = CreateJsonFolderWrapper();

            folderWrapper.LoadRelativePath().Should().BeNull();
        }

        [TestMethod]
        public void LoadRelativePath_WhenFolderPathEqualsRootPath_ReturnsFullPath()
        {
            const string folderPath = @"\\Mailbox\Inbox";
            var folderWrapper = CreateJsonFolderWrapper();
            var root = CreateOutlookFolder(folderPath);
            var folder = CreateOutlookFolder(folderPath);
            folderWrapper.OlRoot = root.Object;
            folderWrapper.OlFolder = folder.Object;

            folderWrapper.LoadRelativePath().Should().Be(folderPath);
        }

        [TestMethod]
        public void LoadRelativePath_WhenFolderPathDoesNotContainRootPath_ReturnsFullPath()
        {
            const string rootPath = @"\\Mailbox\Archive";
            const string folderPath = @"\\Mailbox\Inbox\Sub";
            var folderWrapper = CreateJsonFolderWrapper();
            var root = CreateOutlookFolder(rootPath);
            var folder = CreateOutlookFolder(folderPath);
            folderWrapper.OlRoot = root.Object;
            folderWrapper.OlFolder = folder.Object;

            folderWrapper.LoadRelativePath().Should().Be(folderPath);
        }

        [TestMethod]
        public void RelativePath_WhenFolderPathContainsRootPath_StripsRootAndDirectorySeparator()
        {
            const string rootPath = @"\\Mailbox\Inbox";
            const string folderPath = @"\\Mailbox\Inbox\Projects";
            var folderWrapper = CreateJsonFolderWrapper(relativePath: "placeholder");
            var root = CreateOutlookFolder(rootPath);
            var folder = CreateOutlookFolder(folderPath);
            folderWrapper.OlRoot = root.Object;
            folderWrapper.OlFolder = folder.Object;
            folderWrapper.ResetLazy();

            folderWrapper.RelativePath.Should().Be("Projects");
        }

        [TestMethod]
        public void FolderSize_WhenItemsExposeOnlySizeProperty_UsesFallbackValue()
        {
            var folderWrapper = CreateJsonFolderWrapper();
            var root = CreateOutlookFolder(@"\\Mailbox");
            var folder = CreateOutlookFolderWithItems(
                @"\\Mailbox\Projects",
                new SizeOnlyItem { Size = 128L },
                new object()
            );
            folderWrapper.OlRoot = root.Object;
            folderWrapper.OlFolder = folder.Object;

            folderWrapper.FolderSize.Should().Be(128L);
        }

        [TestMethod]
        public void State_WhenOlFolderChanges_ResetLazyReloadsNameAndRelativePath()
        {
            var folderWrapper = CreateJsonFolderWrapper(relativePath: "placeholder");
            var root = CreateOutlookFolder(@"\\Mailbox");
            var originalFolder = CreateOutlookFolder(@"\\Mailbox\Projects");
            var updatedFolder = CreateOutlookFolder(@"\\Mailbox\Archive");
            folderWrapper.OlRoot = root.Object;
            folderWrapper.OlFolder = originalFolder.Object;
            _ = folderWrapper.Name;
            _ = folderWrapper.RelativePath;

            folderWrapper.OlFolder = updatedFolder.Object;

            folderWrapper.Name.Should().Be("Archive");
            folderWrapper.RelativePath.Should().Be("Archive");
        }

        [TestMethod]
        public void LoadItemCountSubFolders_WhenChildrenContainNestedItems_ReturnsRecursiveTotal()
        {
            var grandchild = CreateOutlookFolder(@"\\Mailbox\Projects\FY26", itemCount: 3);
            var child = CreateOutlookFolder(@"\\Mailbox\Projects", itemCount: 2, grandchild.Object);
            var root = CreateOutlookFolder(@"\\Mailbox", itemCount: 1, child.Object);
            var folderWrapper = new FolderWrapper(root.Object, root.Object);

            var result = folderWrapper.LoadItemCountSubFolders();

            result.Should().Be(6);
        }

        [TestMethod]
        public async Task LoadLazyAsync_WhenRuntimeFoldersExist_PopulatesLazyState()
        {
            var mailLike = new SizeOnlyItem { Size = 64L };
            var root = CreateOutlookFolder(@"\\Mailbox");
            var folder = CreateOutlookFolder(@"\\Mailbox\Projects", itemCount: 4, items: mailLike);
            var folderWrapper = new FolderWrapper(folder.Object, root.Object);

            await folderWrapper.LoadLazyAsync();

            folderWrapper.Name.Should().Be("Projects");
            folderWrapper.RelativePath.Should().Be("Projects");
            folderWrapper.ItemCount.Should().Be(4);
            folderWrapper.FolderSize.Should().Be(64L);
        }

        private static FolderWrapper CreateJsonFolderWrapper(
            bool selected = false,
            int itemCount = 0,
            long folderSize = 0L,
            string name = "Folder",
            string relativePath = "Folder"
        )
        {
            return new FolderWrapper(selected, itemCount, folderSize, name, relativePath);
        }

        private static Mock<OutlookFolder> CreateOutlookFolder(
            string folderPath,
            int itemCount = 0,
            OutlookFolder child = null,
            params object[] items
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Name).Returns(folderPath.Split('\\')[^1]);
            folder.SetupGet(x => x.Items).Returns(CreateOutlookItems(itemCount, items).Object);
            folder.SetupGet(x => x.Folders).Returns(CreateChildFolders(child).Object);
            return folder;
        }

        private static Mock<OutlookFolder> CreateOutlookFolderWithItems(
            string folderPath,
            params object[] items
        )
        {
            var folder = CreateOutlookFolder(folderPath);
            var outlookItems = new Mock<OutlookItems>(MockBehavior.Strict);
            var collection = new ArrayList(items);
            outlookItems.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            folder.SetupGet(x => x.Items).Returns(outlookItems.Object);
            return folder;
        }

        private static Mock<OutlookItems> CreateOutlookItems(int count, params object[] items)
        {
            var outlookItems = new Mock<OutlookItems>(MockBehavior.Strict);
            outlookItems.SetupGet(x => x.Count).Returns(count);
            var collection = new ArrayList(items ?? Array.Empty<object>());
            outlookItems.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return outlookItems;
        }

        private static Mock<Microsoft.Office.Interop.Outlook.Folders> CreateChildFolders(
            params OutlookFolder[] children
        )
        {
            var folders = new Mock<Microsoft.Office.Interop.Outlook.Folders>(MockBehavior.Strict);
            var filteredChildren = (children ?? Array.Empty<OutlookFolder>())
                .Where(child => child is not null)
                .ToArray();
            var collection = new ArrayList(filteredChildren);
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return folders;
        }

        private sealed class SizeOnlyItem
        {
            public long Size { get; set; }
        }
    }
}
