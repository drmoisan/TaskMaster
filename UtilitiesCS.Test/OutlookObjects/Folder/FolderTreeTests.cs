using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;
using OutlookItems = Microsoft.Office.Interop.Outlook.Items;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderTreeTests
    {
        [TestMethod]
        public void CompareMembers_WithFolderWrapperLists_ReturnsIntersectionAndDifferences()
        {
            var tree = new FolderTree();
            var current = new List<FolderWrapper>
            {
                CreateFolderWrapper("Inbox", itemCount: 2, folderSize: 10L),
                CreateFolderWrapper("Archive", itemCount: 1, folderSize: 5L),
            };
            var other = new List<FolderWrapper>
            {
                CreateFolderWrapper("INBOX", itemCount: 2, folderSize: 10L),
                CreateFolderWrapper("Sent", itemCount: 1, folderSize: 5L),
            };

            var (same, onlyCurrent, onlyOther) = tree.CompareMembers(
                current,
                other,
                new FolderWrapperNameCountSizeComparer()
            );

            same.Should().ContainSingle(x => x.Name == "Inbox");
            onlyCurrent.Should().ContainSingle(x => x.Name == "Archive");
            onlyOther.Should().ContainSingle(x => x.Name == "Sent");
        }

        [TestMethod]
        public void CompareMembers_WithTreeNodeLists_ReturnsIntersectionAndDifferences()
        {
            var tree = new FolderTree();
            var currentRoot = new TreeNode<FolderWrapper>(CreateFolderWrapper("Mailbox"));
            var otherRoot = new TreeNode<FolderWrapper>(CreateFolderWrapper("Mailbox"));
            var current = new List<TreeNode<FolderWrapper>>
            {
                currentRoot.AddChild(CreateFolderWrapper("Inbox", itemCount: 2, folderSize: 10L)),
                currentRoot.AddChild(CreateFolderWrapper("Projects", itemCount: 1, folderSize: 5L)),
            };
            var other = new List<TreeNode<FolderWrapper>>
            {
                otherRoot.AddChild(CreateFolderWrapper("INBOX", itemCount: 2, folderSize: 10L)),
                otherRoot.AddChild(CreateFolderWrapper("Sent", itemCount: 1, folderSize: 5L)),
            };

            var (same, onlyCurrent, onlyOther) = tree.CompareMembers(
                current,
                other,
                new FolderWrapperNodeComparer()
            );

            same.Should().ContainSingle(x => x.Value.Name == "Inbox");
            onlyCurrent.Should().ContainSingle(x => x.Value.Name == "Projects");
            onlyOther.Should().ContainSingle(x => x.Value.Name == "Sent");
        }

        [TestMethod]
        public void FilterSelected_WhenSomeNodesSelected_ReturnsOnlySelectedSubtree()
        {
            var fy26 = CreateFolder(@"\\Mailbox\Projects\FY26");
            var projects = CreateFolder(@"\\Mailbox\Projects", fy26.Object);
            var reference = CreateFolder(@"\\Mailbox\Reference");
            var root = CreateFolder(@"\\Mailbox", projects.Object, reference.Object);
            var tree = new FolderTree((Outlook.MAPIFolder)root.Object);
            var nodes = tree.FlattenNodes();
            nodes.Single(x => x.Value.Name == "Projects").Value.Selected = true;
            nodes.Single(x => x.Value.Name == "FY26").Value.Selected = true;

            var result = tree.FilterSelected(include: true);

            result.Should().ContainSingle();
            result[0].Value.Name.Should().Be("Projects");
            result[0].Children.Should().ContainSingle();
            result[0].Children[0].Value.Name.Should().Be("FY26");
            result
                .SelectMany(node => node.Flatten())
                .Should()
                .OnlyContain(folder => folder.Selected);
        }

        [TestMethod]
        public void Flatten_WithPopulatedTree_ReturnsAllFolderWrappers()
        {
            var child = CreateFolder(@"\\Mailbox\Projects");
            var root = CreateFolder(@"\\Mailbox", child.Object);
            var tree = new FolderTree((Outlook.MAPIFolder)root.Object);

            var result = tree.Flatten();

            result.Select(folder => folder.Name).Should().Equal("Mailbox", "Projects");
        }

        private static FolderWrapper CreateFolderWrapper(
            string name,
            int itemCount = 0,
            long folderSize = 0L
        )
        {
            return new FolderWrapper(
                selected: false,
                itemCount: itemCount,
                folderSize: folderSize,
                name: name,
                relativePath: name
            );
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            params OutlookFolder[] children
        )
        {
            var folder = new Mock<OutlookFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(children).Object);
            folder.SetupGet(x => x.Items).Returns(CreateItems().Object);
            return folder;
        }

        private static Mock<OutlookFolders> CreateFoldersCollection(params OutlookFolder[] children)
        {
            var folders = new Mock<OutlookFolders>(MockBehavior.Strict);
            var enumerableChildren = children ?? [];
            var collection = new ArrayList(enumerableChildren);
            folders.SetupGet(x => x.Count).Returns(enumerableChildren.Length);
            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return folders;
        }

        private static Mock<OutlookItems> CreateItems(int count = 0)
        {
            var items = new Mock<OutlookItems>(MockBehavior.Strict);
            items.SetupGet(x => x.Count).Returns(count);
            return items;
        }

        private static string GetLeafName(string folderPath)
        {
            return folderPath.Split('\\').Last(segment => !string.IsNullOrWhiteSpace(segment));
        }
    }
}
