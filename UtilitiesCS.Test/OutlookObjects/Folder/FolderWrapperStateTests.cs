using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

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

        private static Mock<OutlookFolder> CreateOutlookFolder(string folderPath)
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            return folder;
        }
    }
}
