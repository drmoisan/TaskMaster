using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public partial class EmailDataMiner_Tests
    {
        [TestMethod]
        public async Task GetOlFolderSnapshotAsync_WhenArchiveRootMissing_RequestsAllStores()
        {
            FolderTreeRequest captured = null;
            var snapshot = CreateFolderSnapshot(new FolderWrapper(false, 1, 10, "Inbox", "Inbox"));
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .Setup(item =>
                    item.GetSnapshotAsync(
                        It.IsAny<FolderTreeRequest>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Callback<FolderTreeRequest, CancellationToken>((request, _) => captured = request)
                .ReturnsAsync(snapshot);
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(item => item.ArchiveRoot).Returns((Folder)null);
            ol.SetupGet(item => item.FolderTreeService).Returns(service.Object);
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(item => item.Ol).Returns(ol.Object);
            var miner = new EmailDataMiner(globals.Object);

            var result = await miner.GetOlFolderSnapshotAsync();

            result.Should().BeSameAs(snapshot);
            captured.IsAllStores.Should().BeTrue();
            captured.AllowStaleSnapshot.Should().BeTrue();
        }

        [TestMethod]
        public void QueryOlFolderInfo_WhenResolverReturnsMapiFolder_UsesResolvedWrapper()
        {
            var folder = new FolderWrapper(false, 1, 10, "Inbox", "Inbox");
            var snapshot = CreateFolderSnapshot(folder);
            var mapiFolder = new Mock<MAPIFolder>().Object;
            var resolver = new FakeFolderHandleResolver();
            resolver.HandlesByRelativePath["Inbox"] = mapiFolder;
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(item => item.ArchiveRoot).Returns(new Mock<Folder>().Object);
            var td = new Mock<IToDoObjects>();
            td.SetupGet(item => item.FilteredFolderScraping)
                .Returns(new ScoDictionaryNew<string, int>());
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(item => item.Ol).Returns(ol.Object);
            globals.SetupGet(item => item.TD).Returns(td.Object);
            var miner = new FolderTreeBackedEmailDataMiner(globals.Object)
            {
                FolderHandleResolver = resolver,
            };

            var result = miner.QueryOlFolderInfo(snapshot).ToArray();

            result.Should().ContainSingle();
            result[0].OlFolder.Should().BeSameAs(mapiFolder);
        }

        [TestMethod]
        public void QueryOlFolderInfo_WhenResolverFails_UsesSnapshotFallbackWrapper()
        {
            var folder = new FolderWrapper(false, 1, 10, "Inbox", "Inbox");
            var snapshot = CreateFolderSnapshot(folder);
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(item => item.ArchiveRoot).Returns(new Mock<Folder>().Object);
            var td = new Mock<IToDoObjects>();
            td.SetupGet(item => item.FilteredFolderScraping)
                .Returns(new ScoDictionaryNew<string, int>());
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(item => item.Ol).Returns(ol.Object);
            globals.SetupGet(item => item.TD).Returns(td.Object);
            var miner = new FolderTreeBackedEmailDataMiner(globals.Object)
            {
                FolderHandleResolver = new FakeFolderHandleResolver(),
            };

            var result = miner.QueryOlFolderInfo(snapshot).ToArray();

            result.Should().ContainSingle();
            result[0].RelativePath.Should().Be("Inbox");
            result[0].OlFolder.Should().BeNull();
        }

        [TestMethod]
        public void TryResolveMapiHandles_WhenResolverCannotResolveNode_ReturnsFalse()
        {
            var folder = new FolderWrapper(false, 1, 10, "Inbox", "Inbox");
            var snapshot = CreateFolderSnapshot(folder);

            var result = EmailDataMiner.TryResolveMapiHandles(
                snapshot,
                new[] { folder },
                new FakeFolderHandleResolver(),
                new Mock<MAPIFolder>().Object
            );

            result.Should().BeFalse();
        }

        [TestMethod]
        public void TryResolveMapiHandles_WithLegacyTree_ReassignsMatchingHandles()
        {
            var source = new FolderWrapper(false, 1, 10, "Inbox", "Inbox");
            var target = new FolderWrapper(false, 1, 10, "Inbox", "Inbox");
            var root = new Mock<MAPIFolder>();
            root.SetupGet(item => item.FolderPath).Returns("\\Root");
            var folder = new Mock<MAPIFolder>();
            folder.SetupGet(item => item.FolderPath).Returns("\\Root\\Inbox");
            source.OlRoot = root.Object;
            source.OlFolder = folder.Object;
            var tree = CreateFolderTree(source);

            var result = EmailDataMiner.TryResolveMapiHandles(tree, new[] { target });

            result.Should().BeTrue();
            target.OlRoot.Should().BeSameAs(root.Object);
            target.OlFolder.Should().BeSameAs(folder.Object);
        }

        [TestMethod]
        public void TryResolveMapiHandles_WithLegacyTreeAndNullFolders_ReturnsFalse()
        {
            var tree = CreateFolderTree(new FolderWrapper(false, 1, 10, "Inbox", "Inbox"));

            var result = EmailDataMiner.TryResolveMapiHandles(tree, null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void TryResolveMapiHandles_WithLegacyTreeAndMissingPath_ReturnsFalse()
        {
            var source = new FolderWrapper(false, 1, 10, "Inbox", "Inbox");
            var unresolved = new FolderWrapper(false, 1, 10, "Missing", "Missing");
            var tree = CreateFolderTree(source);

            var result = EmailDataMiner.TryResolveMapiHandles(tree, new[] { unresolved });

            result.Should().BeFalse();
        }
    }
}
