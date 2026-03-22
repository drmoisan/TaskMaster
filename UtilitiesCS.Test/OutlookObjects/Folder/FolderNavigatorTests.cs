using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderNavigatorTests
    {
        [TestMethod]
        public void GetOutlookFolder_WhenPathStartsWithDoubleSlash_NormalizesAndTraversesFolders()
        {
            // Arrange
            var projects = CreateFolder("\\\\Mailbox\\Projects", childFolders: null);
            var mailbox = CreateFolder(
                "\\\\Mailbox",
                childFolders: new Dictionary<string, OutlookFolder>
                {
                    ["Projects"] = projects.Object,
                }
            );
            var sessionRoot = CreateFolder(
                "\\\\SessionRoot",
                childFolders: new Dictionary<string, OutlookFolder> { ["Mailbox"] = mailbox.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["Mailbox"] = sessionRoot.Object }
            );

            // Act
            OutlookFolder result = FolderNavigator.GetOutlookFolder(
                "\\\\Mailbox\\Projects",
                app.Object
            );

            // Assert
            result.Should().BeSameAs(projects.Object);
        }

        [TestMethod]
        public void GetOutlookFolder_WhenRootFolderIsMissing_ReturnsNull()
        {
            // Arrange
            var app = CreateApplication(new Dictionary<string, OutlookFolder>());

            // Act
            OutlookFolder result = FolderNavigator.GetOutlookFolder("Mailbox", app.Object);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetOutlookFolder_WhenAnySubFolderLookupReturnsNull_ReturnsNull()
        {
            // Arrange
            var mailbox = CreateFolder(
                "\\\\Mailbox",
                childFolders: new Dictionary<string, OutlookFolder>()
            );
            var sessionRoot = CreateFolder(
                "\\\\SessionRoot",
                childFolders: new Dictionary<string, OutlookFolder> { ["Mailbox"] = mailbox.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["Mailbox"] = sessionRoot.Object }
            );

            // Act
            OutlookFolder result = FolderNavigator.GetOutlookFolder(
                "\\\\Mailbox\\Missing",
                app.Object
            );

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void OlFolderlist_GetAll_WhenArchiveRootHasNestedChildren_ReturnsFlattenedRelativePaths()
        {
            // Arrange
            var fy26 = CreateFolder("\\\\ArchiveRoot\\Projects\\FY26", childFolders: null);
            var projects = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                childFolders: new Dictionary<string, OutlookFolder> { ["FY26"] = fy26.Object },
                enumerableChildren: fy26.Object
            );
            var reference = CreateFolder("\\\\ArchiveRoot\\Reference", childFolders: null);
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                childFolders: new Dictionary<string, OutlookFolder>(),
                enumerableChildren: new[] { projects.Object, reference.Object }
            );
            var sessionRoot = CreateFolder(
                "\\\\SessionRoot",
                childFolders: new Dictionary<string, OutlookFolder>
                {
                    ["ArchiveRoot"] = archiveRoot.Object,
                }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = sessionRoot.Object }
            );
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns("\\\\ArchiveRoot");
            olObjects.SetupGet(x => x.App).Returns(app.Object);

            // Act
            string[] result = FolderNavigator.OlFolderlist_GetAll(olObjects.Object);

            // Assert
            result.Should().Equal("\\Projects", "\\Projects\\FY26", "\\Reference");
        }

        private static Mock<Application> CreateApplication(
            IDictionary<string, OutlookFolder> rootFolders
        )
        {
            var app = new Mock<Application>();
            var nameSpace = new Mock<NameSpace>();

            nameSpace.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(rootFolders).Object);
            app.SetupGet(x => x.Session).Returns(nameSpace.Object);

            return app;
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            IDictionary<string, OutlookFolder> childFolders,
            params OutlookFolder[] enumerableChildren
        )
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder
                .SetupGet(x => x.Folders)
                .Returns(
                    CreateFoldersCollection(
                        childFolders ?? new Dictionary<string, OutlookFolder>(),
                        enumerableChildren
                    ).Object
                );
            return folder;
        }

        private static Mock<OutlookFolders> CreateFoldersCollection(
            IDictionary<string, OutlookFolder> foldersByName,
            params OutlookFolder[] enumerableChildren
        )
        {
            var folders = new Mock<OutlookFolders>();
            var enumerableItems = enumerableChildren ?? [];
            var collection = new ArrayList(enumerableItems);

            folders
                .Setup(x => x[It.IsAny<object>()])
                .Returns<object>(key =>
                {
                    if (
                        key is string name
                        && foldersByName.TryGetValue(name, out OutlookFolder folder)
                    )
                    {
                        return folder;
                    }

                    return null;
                });
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());

            return folders;
        }
    }
}
