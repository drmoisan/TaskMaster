using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public partial class EmailDataMiner_Tests
    {
        [TestMethod]
        public void QueryOlFolders_WhenTreeContainsUnselectedNodes_ReturnsMappedOutlookFolders()
        {
            var folderOne = new FolderWrapper(false, 1, 10, "One", "root/one");
            var folderTwo = new FolderWrapper(false, 1, 10, "Two", "root/two");
            folderOne.OlFolder = new Mock<MAPIFolder>().Object;
            folderTwo.OlFolder = new Mock<MAPIFolder>().Object;
            var tree = CreateFolderTree(folderOne, folderTwo);
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals()) { FolderTree = tree };

            var folders = miner.QueryOlFolders(tree).ToArray();

            folders.Should().HaveCount(2);
            folders.Should().Contain(folderOne.OlFolder);
            folders.Should().Contain(folderTwo.OlFolder);
        }

        [TestMethod]
        public void QueryOlFolderInfo_WhenTreeContainsUnselectedNodes_ReturnsFolderWrappers()
        {
            var folderOne = new FolderWrapper(false, 1, 10, "One", "root/one");
            var folderTwo = new FolderWrapper(false, 1, 10, "Two", "root/two");
            var tree = CreateFolderTree(folderOne, folderTwo);
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals()) { FolderTree = tree };

            var folders = miner.QueryOlFolderInfo(tree).ToArray();

            folders.Should().ContainInOrder(folderOne, folderTwo);
        }

        [TestMethod]
        public async Task TryResolveMapiHandles_WhenRelativePathsMatch_ReassignsFolderHandles()
        {
            var treeFolder = new FolderWrapper(false, 1, 10, "One", "root/one");
            var resolvedRoot = new Mock<MAPIFolder>().Object;
            var resolvedFolder = new Mock<MAPIFolder>().Object;
            var resolver = new FakeFolderHandleResolver();
            resolver.HandlesByRelativePath["root/one"] = resolvedFolder;

            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals())
            {
                Snapshot = CreateFolderSnapshot(treeFolder),
                FolderHandleResolver = resolver,
                ArchiveRoot = resolvedRoot,
            };

            var result = await miner.TryResolveMapiHandles([treeFolder]);

            result.Should().BeTrue();
            treeFolder.OlRoot.Should().BeSameAs(resolvedRoot);
            treeFolder.OlFolder.Should().BeSameAs(resolvedFolder);
        }

        [TestMethod]
        public async Task GetOlFolderSnapshotAsync_UsesCachedStoreRequestAndFallsBackToFullSnapshot()
        {
            var folder = new FolderWrapper(false, 1, 10, "One", "root/one");
            var snapshot = CreateFolderSnapshot(folder);
            FolderTreeRequest captured = null;
            var service = new Mock<IOutlookFolderTreeService>();
            service
                .Setup(x =>
                    x.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
                )
                .Callback<FolderTreeRequest, CancellationToken>((request, _) => captured = request)
                .ReturnsAsync(snapshot);
            var archiveRoot = new Mock<Folder>();
            archiveRoot.SetupGet(x => x.StoreID).Returns("store-a");
            archiveRoot.SetupGet(x => x.FolderPath).Returns("\\Missing");
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot.Object);
            ol.SetupGet(x => x.FolderTreeService).Returns(service.Object);
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            var miner = new EmailDataMiner(globals.Object);

            var result = await miner.GetOlFolderSnapshotAsync();

            result.Should().BeSameAs(snapshot);
            captured.AllowStaleSnapshot.Should().BeTrue();
            captured.StoreIds.Should().ContainSingle().Which.Should().Be("store-a");
        }

        [TestMethod]
        public void QueryOlFolderInfo_WithSnapshot_ExcludesConfiguredRelativePaths()
        {
            var included = new FolderWrapper(false, 1, 10, "Included", "root/included");
            var excluded = new FolderWrapper(false, 1, 10, "Excluded", "root/excluded");
            var scraping = new ScoDictionaryNew<string, int> { ["root/excluded"] = 1 };
            var td = new Mock<IToDoObjects>();
            td.SetupGet(x => x.FilteredFolderScraping).Returns(scraping);
            var archiveRoot = new Mock<Folder>().Object;
            var ol = new Mock<IOlObjects>();
            ol.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.TD).Returns(td.Object);
            globals.SetupGet(x => x.Ol).Returns(ol.Object);
            var resolver = new FakeFolderHandleResolver();
            var miner = new FolderTreeBackedEmailDataMiner(globals.Object)
            {
                FolderHandleResolver = resolver,
            };

            var folders = miner.QueryOlFolderInfo(CreateFolderSnapshot(included, excluded));

            folders
                .Select(folder => folder.RelativePath)
                .Should()
                .ContainSingle()
                .Which.Should()
                .Be("root/included");
        }

        [TestMethod]
        public async Task ExtractOlFolderChunks_WhenCachedFoldersResolve_ReturnsChunkedGroups()
        {
            var cachedFolders = new[]
            {
                new FolderWrapper(false, 2, 300, "One", "root/one"),
                new FolderWrapper(false, 3, 300, "Two", "root/two"),
            };
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals())
            {
                DeserializedValue = cachedFolders,
                UseBaseTryResolveMapiHandles = false,
                TryResolveMapiHandlesResult = true,
            };

            var chunks = await miner.ExtractOlFolderChunks();

            chunks.Should().NotBeEmpty();
            chunks.SelectMany(group => group).Should().HaveCount(2);
            miner.SavedSeeds.Should().Contain("StagingFolderRecordsWithTotals");
            miner.SavedSeeds.Should().Contain("StagingFolderChunks");
        }

        [TestMethod]
        public void SerializeActiveItem_WhenLoaderReturnsMailItem_InvokesSerializeMailInfo()
        {
            var mailItem = new Mock<MailItem>().Object;
            var miner = new TestableEmailDataMiner(new StubGlobals())
            {
                LoaderResult = mailItem,
                LoaderSize = 123,
            };

            miner.SerializeActiveItem();

            miner.SerializeMailInfoCalls.Should().Be(1);
        }

        [TestMethod]
        public void LogSizeComparison_WhenCalled_CompletesWithoutThrowing()
        {
            var miner = new EmailDataMiner(new StubGlobals());

            var action = () => miner.LogSizeComparison("GC", 10, "Serialize", 20, "MailItem");

            action.Should().NotThrow();
        }

        [TestMethod]
        public void DeleteStagingFiles_WhenBayesianFolderMissing_SkipsEnumeration()
        {
            var getFilesCalled = false;
            var deleteFileCalled = false;

            EmailDataMiner.DeleteStagingFiles(
                @"C:\AppData",
                _ => false,
                _ =>
                {
                    getFilesCalled = true;
                    return [];
                },
                _ => deleteFileCalled = true
            );

            getFilesCalled.Should().BeFalse();
            deleteFileCalled.Should().BeFalse();
        }

        [TestMethod]
        public void DeleteStagingFiles_WhenDeleteThrows_ContinuesRemainingFiles()
        {
            var deleted = new List<string>();

            var action = () =>
                EmailDataMiner.DeleteStagingFiles(
                    @"C:\AppData",
                    _ => true,
                    _ => ["one.json", "two.json"],
                    path =>
                    {
                        deleted.Add(path);
                        if (path == "one.json")
                        {
                            throw new IOException("boom");
                        }
                    }
                );

            action.Should().NotThrow();
            deleted.Should().ContainInOrder("one.json", "two.json");
        }

        [TestMethod]
        public async Task TryResolveMapiHandles_WhenFoldersNull_ReturnsFalse()
        {
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals())
            {
                Snapshot = CreateFolderSnapshot(new FolderWrapper(false, 1, 10, "One", "root/one")),
                FolderHandleResolver = new FakeFolderHandleResolver(),
                ArchiveRoot = new Mock<MAPIFolder>().Object,
            };

            var result = await miner.TryResolveMapiHandles(null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task TryResolveMapiHandles_WhenRelativePathMissing_ReturnsFalse()
        {
            var treeFolder = new FolderWrapper(false, 1, 10, "One", "root/one");
            var unresolvedFolder = new FolderWrapper(false, 1, 10, "Two", "root/two");
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals())
            {
                Snapshot = CreateFolderSnapshot(treeFolder),
                FolderHandleResolver = new FakeFolderHandleResolver(),
                ArchiveRoot = new Mock<MAPIFolder>().Object,
            };

            var result = await miner.TryResolveMapiHandles([unresolvedFolder]);

            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task ScrapeEmails_WhenQuerySeamsReturnMailItems_ReturnsEnumeration()
        {
            var folder = new FolderWrapper(false, 1, 10, "Inbox", "root/inbox");
            var olFolder = CreateOutlookFolder(0);
            olFolder.SetupGet(folderMock => folderMock.Name).Returns("Inbox");
            folder.OlFolder = olFolder.Object;
            folder.RelativePath = "root/inbox";
            var mailOne = new Mock<MailItem>().Object;
            var mailTwo = new Mock<MailItem>().Object;
            var resolver = new FakeFolderHandleResolver();
            resolver.HandlesByRelativePath["root/inbox"] = folder.OlFolder;
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals())
            {
                Snapshot = CreateFolderSnapshot(folder),
                FolderHandleResolver = resolver,
                MailItems = [mailOne, mailTwo],
            };

            var result = await InvokeEnumerableTask(
                miner,
                nameof(EmailDataMiner.ScrapeEmails),
                new CancellationTokenSource()
            );

            result.Should().ContainInOrder(mailOne, mailTwo);
        }

        [TestMethod]
        public async Task ScrapeEmails_WithProgress_WhenQuerySeamsReturnMailItems_ReturnsEnumeration()
        {
            var folder = new FolderWrapper(false, 1, 10, "Inbox", "root/inbox");
            var olFolder = CreateOutlookFolder(0);
            olFolder.SetupGet(folderMock => folderMock.Name).Returns("Inbox");
            folder.OlFolder = olFolder.Object;
            folder.RelativePath = "root/inbox";
            var mail = new Mock<MailItem>().Object;
            var resolver = new FakeFolderHandleResolver();
            resolver.HandlesByRelativePath["root/inbox"] = folder.OlFolder;
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals())
            {
                Snapshot = CreateFolderSnapshot(folder),
                FolderHandleResolver = resolver,
                MailItems = [mail],
            };

            var result = await InvokeEnumerableTask(
                miner,
                nameof(EmailDataMiner.ScrapeEmails),
                new CancellationTokenSource(),
                new NoOpProgressTracker()
            );

            result.Should().ContainSingle().Which.Should().BeSameAs(mail);
        }

        [TestMethod]
        public void QueryMailTuples_WhenFoldersContainMixedItems_ReturnsOnlyMailItems()
        {
            // Arrange
            var mail = new Mock<MailItem>().Object;
            var folder = new FolderWrapper(false, 2, 10, "Inbox", "root/inbox")
            {
                OlFolder = CreateOutlookFolder(2, mail, "not-a-mail").Object,
            };
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var result = InvokeEnumerable(
                miner,
                nameof(EmailDataMiner.QueryMailTuples),
                new object[] { new[] { folder } }
            );

            // Assert
            result.Should().ContainSingle();
            GetTupleField(result[0], "Item1").Should().BeSameAs(mail);
            GetTupleField(result[0], "Item2").Should().BeSameAs(folder);
        }

        [TestMethod]
        public void QueryMailItems_WhenFoldersContainMixedItems_ReturnsOnlyMailItems()
        {
            // Arrange
            var mail = new Mock<MailItem>().Object;
            var folder = CreateOutlookFolder(2, mail, 123).Object;
            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var result = miner.QueryMailItems([folder]).ToArray();

            // Assert
            result.Should().ContainSingle().Which.Should().BeSameAs(mail);
        }

        [TestMethod]
        public void ConsumeLinq_WhenFoldersContainMailItems_LoadsAllMailItems()
        {
            // Arrange
            var folder = CreateOutlookFolder(2).Object;
            var mailOne = new Mock<MailItem>().Object;
            var mailTwo = new Mock<MailItem>().Object;
            var miner = new FolderTreeBackedEmailDataMiner(new StubGlobals());

            // Act
            var result = InvokeEnumerable(
                miner,
                nameof(EmailDataMiner.ConsumeLinq),
                new[] { folder },
                new[] { mailOne, mailTwo },
                new NoOpProgressTracker()
            );

            // Assert
            result.Should().ContainInOrder(mailOne, mailTwo);
        }

        [TestMethod]
        public void DeserializeFromFolder_WhenFileExists_UsesProvidedReaderAndReturnsValue()
        {
            // Arrange
            string capturedPath = null;

            // Act
            var result = EmailDataMiner.DeserializeFromFolder<int>(
                @"C:\AppData\Bayesian",
                "Seed",
                "",
                path =>
                {
                    capturedPath = path;
                    return true;
                },
                _ => "42"
            );

            // Assert
            capturedPath.Should().EndWith("Seed.json");
            result.Should().Be(42);
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenFileExists_UsesProvidedReaderAndReturnsValue()
        {
            // Arrange
            string capturedPath = null;

            // Act
            var result = await EmailDataMiner.DeserializeAsync<string>(
                @"C:\AppData\Bayesian",
                "Seed",
                "0001",
                path =>
                {
                    capturedPath = path;
                    return true;
                },
                _ => Task.FromResult("\"value\"")
            );

            // Assert
            capturedPath.Should().EndWith("Seed_0001.json");
            result.Should().Be("value");
        }

        [TestMethod]
        public async Task ValidateJson_WhenDeserializerReturnsObject_ReturnsTrue()
        {
            // Arrange
            var miner = new TestableEmailDataMiner(
                new StubGlobals(specialFolders: CreateAppDataMap(@"C:\AppData"))
            )
            {
                ValidationDeserializeResult = "value",
            };

            // Act
            var result = await miner.ValidateJson<string>("Seed");

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateJson_WhenDeserializerThrows_ReturnsFalse()
        {
            // Arrange
            var miner = new TestableEmailDataMiner(
                new StubGlobals(specialFolders: CreateAppDataMap(@"C:\AppData"))
            )
            {
                ValidationDeserializeException = new JsonReaderException("bad json"),
            };

            // Act
            var result = await miner.ValidateJson<string>("Seed", "0001");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void SerializeAndSaveCore_WhenWriterProvided_SerializesObjectAndClearsFileName()
        {
            // Arrange
            var disk = new FilePathHelper
            {
                FolderPath = @"C:\AppData\Bayesian",
                FileName = "Seed.json",
            };
            var createdDirectory = string.Empty;
            var serializer = JsonSerializer.Create(
                new JsonSerializerSettings { Formatting = Formatting.Indented }
            );
            var writer = new StringWriter();

            // Act
            EmailDataMiner.SerializeAndSave(
                new { Name = "test" },
                serializer,
                disk,
                path => createdDirectory = path,
                _ => writer
            );

            // Assert
            createdDirectory.Should().Be(@"C:\AppData\Bayesian");
            disk.FileName.Should().BeNull();
            writer.ToString().Should().Contain("Name");
            writer.ToString().Should().Contain("test");
        }
    }
}
