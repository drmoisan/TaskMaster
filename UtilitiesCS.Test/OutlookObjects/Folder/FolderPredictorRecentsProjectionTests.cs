using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for AC5 of issue #799: recent-folder entries pass through the same archive-stem
    /// display projection as suggestion entries before they are displayed. Today an
    /// archive-rooted recent entry is appended verbatim, so a recent selection renders as a full
    /// store path beside suggestions that render as archive-relative stems.
    /// <para>
    /// Every test seeds the recents list with one archive-rooted entry AND one already-relative
    /// entry, so the projection is observable and the identity case is pinned by the same fixture.
    /// The mocked-Outlook harness mirrors <c>FolderRowTests</c>; no live Outlook process, COM
    /// server, or temporary file is used.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class FolderPredictorRecentsProjectionTests
    {
        private const string ArchiveRootPath = "\\\\ArchiveRoot";
        private const string RootedRecent = "\\\\ArchiveRoot\\Recent\\One";
        private const string RootedRecentStem = "Recent\\One";
        private const string RelativeRecent = "Recent\\Two";
        private const string OutOfRootRecent = "\\\\OtherRoot\\Recent\\Three";
        private const string RecentsSeparator = "======= RECENT SELECTIONS ========";

        /// <summary>
        /// AC5 on the legacy string surface: the archive-rooted recent entry is projected to its
        /// archive-relative stem, and the already-relative entry is left alone.
        /// </summary>
        [TestMethod]
        public void FolderArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem()
        {
            // Arrange
            var predictor = PredictorWithRecents(RootedRecent, RelativeRecent);

            // Act
            var folderArray = predictor.FolderArray;

            // Assert
            folderArray.Should().Equal(RecentsSeparator, RootedRecentStem, RelativeRecent);
        }

        /// <summary>
        /// AC5 on the row-model surface: <c>FolderRowArray</c> projects the archive-rooted recent
        /// entry identically, and still tags both entries as recent selections.
        /// </summary>
        [TestMethod]
        public void FolderRowArray_RootedRecentEntry_IsProjectedToTheArchiveRelativeStem()
        {
            // Arrange
            var predictor = PredictorWithRecents(RootedRecent, RelativeRecent);

            // Act
            var rows = predictor.FolderRowArray;

            // Assert
            rows.Select(r => r.Text)
                .Should()
                .Equal(RecentsSeparator, RootedRecentStem, RelativeRecent);
            rows.Where(r => r.Kind == UtilitiesCS.FolderRowKind.Recent)
                .Select(r => r.Text)
                .Should()
                .Equal(RootedRecentStem, RelativeRecent);
        }

        /// <summary>
        /// The text-parity contract documented on <c>FolderRowArray</c> is currently unasserted for
        /// recents. Projecting one surface without the other would break it silently, so parity is
        /// pinned here as its own criterion rather than as a side effect of the two tests above.
        /// </summary>
        [TestMethod]
        public void FolderRowArray_AndFolderArray_AgreeOnRecentTextAfterProjection()
        {
            // Arrange
            var predictor = PredictorWithRecents(RootedRecent, RelativeRecent);

            // Act
            var folderArray = predictor.FolderArray;
            var rows = predictor.FolderRowArray;

            // Assert
            rows.Select(r => r.Text)
                .Should()
                .Equal(folderArray, "the row model mirrors FolderArray byte for byte");
        }

        /// <summary>
        /// The projection is lenient: a recent entry that is NOT under the archive root is left
        /// exactly as stored, because there is no stem to show and the full path is the only
        /// meaningful text.
        /// </summary>
        [TestMethod]
        public void FolderArray_OutOfRootRecentEntry_IsLeftUnchanged()
        {
            // Arrange
            var predictor = PredictorWithRecents(RootedRecent, RelativeRecent, OutOfRootRecent);

            // Act
            var folderArray = predictor.FolderArray;

            // Assert
            folderArray
                .Should()
                .Equal(RecentsSeparator, RootedRecentStem, RelativeRecent, OutOfRootRecent);
        }

        private static UtilitiesCS.FolderPredictor PredictorWithRecents(params string[] recents)
        {
            var archiveRoot = CreateFolder(
                ArchiveRootPath,
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object, recents);
            return new UtilitiesCS.FolderPredictor(globals.Object);
        }

        // ---- Mocked-Outlook harness (mirrors FolderRowTests) ----

        private static Mock<IApplicationGlobals> CreateGlobals(
            Mock<Outlook.Application> app,
            OutlookFolder rootFolder,
            IEnumerable<string> recents
        )
        {
            var autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(x => x.RecentsList).Returns(new SloLinkedList<string>(recents));

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.App).Returns(app.Object);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(rootFolder.FolderPath);
            olObjects.SetupGet(x => x.Root).Returns(rootFolder);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.AF).Returns(autoFile.Object);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static Mock<Outlook.Application> CreateApplication(
            IDictionary<string, OutlookFolder> rootFolders
        )
        {
            var app = new Mock<Outlook.Application>();
            var nameSpace = new Mock<Outlook.NameSpace>();
            nameSpace.SetupGet(x => x.Folders).Returns(CreateFoldersCollection(rootFolders).Object);
            app.SetupGet(x => x.Session).Returns(nameSpace.Object);
            return app;
        }

        private static Mock<OutlookFolder> CreateFolder(
            string folderPath,
            IDictionary<string, OutlookFolder> childFolders
        )
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
            folder.SetupGet(x => x.FolderPath).Returns(folderPath);
            folder
                .SetupGet(x => x.Folders)
                .Returns(
                    CreateFoldersCollection(
                        childFolders ?? new Dictionary<string, OutlookFolder>()
                    ).Object
                );
            return folder;
        }

        private static Mock<OutlookFolders> CreateFoldersCollection(
            IDictionary<string, OutlookFolder> foldersByName
        )
        {
            var folders = new Mock<OutlookFolders>();
            var enumerableItems =
                foldersByName?.Values?.ToArray() ?? Array.Empty<OutlookFolder>();
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

        private static string GetLeafName(string folderPath)
        {
            return folderPath.Split('\\').Last(segment => !string.IsNullOrWhiteSpace(segment));
        }
    }
}
