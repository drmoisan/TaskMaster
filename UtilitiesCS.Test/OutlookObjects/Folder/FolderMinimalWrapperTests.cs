using System;
using System.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.OutlookObjects.Folder;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderMinimalWrapperTests
    {
        [TestMethod]
        public void Constructor_WithFolders_ProjectsNameAndRelativePathFromLazyState()
        {
            // Arrange
            var root = CreateFolder("\\Mailbox");
            var folder = CreateFolder("\\Mailbox\\Projects\\FY26", name: "FY26");

            // Act
            var wrapper = new FolderMinimalWrapper(folder.Object, root.Object);

            // Assert
            wrapper.Name.Should().Be("FY26");
            wrapper.RelativePath.Should().Be("Projects\\FY26");
        }

        [TestMethod]
        public void ToRelativePath_WhenFolderOrRootIsMissing_ReturnsNull()
        {
            // Arrange
            var wrapper = new FolderMinimalWrapper("Projects", @"Projects\FY26");

            // Act
            string result = wrapper.ToRelativePath();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void ToRelativePath_WhenFolderMatchesRoot_ReturnsFullFolderPath()
        {
            // Arrange
            var root = CreateFolder("\\Mailbox", name: "Mailbox");
            var wrapper = new FolderMinimalWrapper(root.Object, root.Object);

            // Act
            string result = wrapper.ToRelativePath();

            // Assert
            result.Should().Be("\\Mailbox");
        }

        [TestMethod]
        public void ToRelativePath_WhenFolderIsOutsideRoot_ReturnsFullFolderPath()
        {
            // Arrange
            var root = CreateFolder("\\Mailbox", name: "Mailbox");
            var folder = CreateFolder("\\Archive\\Projects", name: "Projects");
            var wrapper = new FolderMinimalWrapper(folder.Object, root.Object);

            // Act
            string result = wrapper.ToRelativePath();

            // Assert
            result.Should().Be("\\Archive\\Projects");
        }

        [TestMethod]
        public void RestoreFromRelativePath_WhenRootIsNullOrRelativePathIsEmpty_LeavesRuntimeFoldersUnset()
        {
            // Arrange
            var wrapperWithNullRoot = new FolderMinimalWrapper("Projects", @"Projects\FY26");
            var wrapperWithEmptyPath = new FolderMinimalWrapper("Projects", string.Empty);
            var root = CreateFolder("\\Mailbox", name: "Mailbox");

            // Act
            wrapperWithNullRoot.RestoreFromRelativePath(null);
            wrapperWithEmptyPath.RestoreFromRelativePath(root.Object);

            // Assert
            wrapperWithNullRoot.OlRoot.Should().BeNull();
            wrapperWithNullRoot.OlFolder.Should().BeNull();
            wrapperWithEmptyPath.OlRoot.Should().BeNull();
            wrapperWithEmptyPath.OlFolder.Should().BeNull();
        }

        [TestMethod]
        public void RestoreFromRelativePath_WhenAllPartsExist_LoadsNestedFolderCaseInsensitively()
        {
            // Arrange
            var grandchild = CreateFolder("\\Mailbox\\Projects\\FY26", name: "FY26");
            var child = CreateFolder("\\Mailbox\\Projects", name: "Projects", children: grandchild.Object);
            var root = CreateFolder("\\Mailbox", name: "Mailbox", children: child.Object);
            var wrapper = new FolderMinimalWrapper("FY26", @"projects\fy26");

            // Act
            wrapper.RestoreFromRelativePath(root.Object);

            // Assert
            wrapper.OlRoot.Should().BeSameAs(root.Object);
            wrapper.OlFolder.Should().BeSameAs(grandchild.Object);
        }

        [TestMethod]
        public void RestoreFromRelativePath_WhenAnyPartIsMissing_LeavesFolderUnsetButRetainsRoot()
        {
            // Arrange
            var child = CreateFolder("\\Mailbox\\Projects", name: "Projects");
            var root = CreateFolder("\\Mailbox", name: "Mailbox", children: child.Object);
            var wrapper = new FolderMinimalWrapper("Missing", @"Projects\Missing");

            // Act
            wrapper.RestoreFromRelativePath(root.Object);

            // Assert
            wrapper.OlRoot.Should().BeSameAs(root.Object);
            wrapper.OlFolder.Should().BeNull();
        }

        [TestMethod]
        public void JsonSerialization_RoundTripsSerializedState_AndIgnoresRuntimeOnlyFolderReferences()
        {
            // Arrange
            var root = CreateFolder("\\Mailbox", name: "Mailbox");
            var folder = CreateFolder("\\Mailbox\\Projects\\FY26", name: "FY26");
            var original = new FolderMinimalWrapper("FY26", @"Projects\FY26")
            {
                OlRoot = root.Object,
                OlFolder = folder.Object,
            };

            // Act
            string json = JsonConvert.SerializeObject(original);
            FolderMinimalWrapper clone = JsonConvert.DeserializeObject<FolderMinimalWrapper>(json);

            // Assert
            clone.Should().NotBeNull();
            clone.Name.Should().Be("FY26");
            clone.RelativePath.Should().Be(@"Projects\FY26");
            clone.OlRoot.Should().BeNull();
            clone.OlFolder.Should().BeNull();
        }

        private static Mock<OutlookFolder> CreateFolder(string folderPath, string name = null, params OutlookFolder[] children)
        {
            var folder = new Mock<OutlookFolder>();
            var folders = new Mock<OutlookFolders>();
            var collection = new ArrayList(children ?? Array.Empty<OutlookFolder>());

            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Name).Returns(name ?? folderPath?.Split('\\')[^1]);
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            folder.SetupGet(x => x.Folders).Returns(folders.Object);

            return folder;
        }
    }
}