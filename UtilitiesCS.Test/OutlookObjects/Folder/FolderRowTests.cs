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
    /// Tests for the Layer-2 row model on <see cref="UtilitiesCS.FolderPredictor"/>
    /// (<c>FolderRowArray</c> and <c>FindFolderRows</c>). These assert byte-for-byte
    /// <see cref="UtilitiesCS.FolderRow.Text"/> parity with the legacy <c>FolderArray</c> /
    /// <c>FindFolder</c> string outputs (the AC5 golden baseline for the predictor), correct
    /// <see cref="UtilitiesCS.FolderRowKind"/> tagging, and that a non-null
    /// <see cref="UtilitiesCS.FolderRow.Score"/> appears only on <see cref="UtilitiesCS.FolderRowKind.Suggestion"/>
    /// rows. The mocked-Outlook harness mirrors <c>FolderPredictorTests</c>; the COM/model-bound
    /// <c>AddBayesianSuggestionsAsync</c> path is never invoked.
    /// </summary>
    [TestClass]
    public class FolderRowTests
    {
        [TestMethod]
        public void FolderRowArray_WithSuggestionsAndRecents_MatchesFolderArrayTextAndTagsKinds()
        {
            // Arrange
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object, new[] { "Recent\\One" });
            var predictor = new UtilitiesCS.FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion("Archive\\Inbox", 10);

            // Act
            var folderArray = predictor.FolderArray;
            var rows = predictor.FolderRowArray;

            // Assert: byte-for-byte Text parity with FolderArray (AC5 golden baseline).
            rows.Select(r => r.Text).Should().Equal(folderArray);

            // Assert: Kind tagging.
            rows.Select(r => r.Kind)
                .Should()
                .Equal(
                    UtilitiesCS.FolderRowKind.Separator,
                    UtilitiesCS.FolderRowKind.Suggestion,
                    UtilitiesCS.FolderRowKind.Separator,
                    UtilitiesCS.FolderRowKind.Recent
                );

            // Assert: Score is non-null only on the Suggestion row and equals ToScoredArray(5).
            rows.Where(r => r.Kind != UtilitiesCS.FolderRowKind.Suggestion)
                .Should()
                .OnlyContain(r => r.Score == null);
            var suggestionRow = rows.Single(r => r.Kind == UtilitiesCS.FolderRowKind.Suggestion);
            var expectedScore = predictor.Suggestions.ToScoredArray(5).Single();
            suggestionRow.Score.Should().NotBeNull();
            suggestionRow.Score.Value.FolderPath.Should().Be(expectedScore.FolderPath);
            suggestionRow.Score.Value.Score.Should().Be(expectedScore.Score);
            suggestionRow.Score.Value.Probability.Should().Be(expectedScore.Probability);
        }

        [TestMethod]
        public void FolderRowArray_DoesNotAlterFolderArrayOutput()
        {
            // Arrange
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object, new[] { "Recent\\One" });
            var predictor = new UtilitiesCS.FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion("Archive\\Inbox", 10);

            // Act: read FolderRowArray first, then FolderArray, to prove the row getter does not
            // mutate the cached _folderList that FolderArray returns.
            _ = predictor.FolderRowArray;
            var folderArray = predictor.FolderArray;

            // Assert: FolderArray is unchanged byte-for-byte.
            folderArray
                .Should()
                .Equal(
                    "========= SUGGESTIONS =========",
                    "Archive\\Inbox",
                    "======= RECENT SELECTIONS ========",
                    "Recent\\One"
                );
        }

        [TestMethod]
        public void FindFolderRows_WithMatchesSuggestionsAndRecents_MatchesFindFolderTextAndTagsKinds()
        {
            // Arrange: an archive tree containing Projects\FY26 so "*FY26" yields a search result.
            var fy26 = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\FY26",
                new Dictionary<string, OutlookFolder>()
            );
            var projects = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder> { ["FY26"] = fy26.Object },
                fy26.Object
            );
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = projects.Object },
                projects.Object
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object, new[] { "Recent\\One" });
            var predictor = new UtilitiesCS.FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion("Archive\\Inbox", 10);

            // Act
            var findFolder = predictor.FindFolder("*FY26", objItem: null, recalcSuggestions: false);
            var rows = predictor.FindFolderRows("*FY26", objItem: null, recalcSuggestions: false);

            // Assert: byte-for-byte Text parity with FindFolder (AC5 golden baseline).
            rows.Select(r => r.Text).Should().Equal(findFolder);

            // Assert: Kind tagging across all four block types.
            rows.Select(r => r.Kind)
                .Should()
                .Equal(
                    UtilitiesCS.FolderRowKind.Separator, // ======= SEARCH RESULTS =======
                    UtilitiesCS.FolderRowKind.SearchResult, // Projects\FY26
                    UtilitiesCS.FolderRowKind.Separator, // ========= SUGGESTIONS =========
                    UtilitiesCS.FolderRowKind.Suggestion, // Archive\Inbox
                    UtilitiesCS.FolderRowKind.Separator, // ======= RECENT SELECTIONS ========
                    UtilitiesCS.FolderRowKind.Recent // Recent\One
                );

            // Assert: Score non-null only on the Suggestion row.
            rows.Where(r => r.Kind != UtilitiesCS.FolderRowKind.Suggestion)
                .Should()
                .OnlyContain(r => r.Score == null);
            var suggestionRow = rows.Single(r => r.Kind == UtilitiesCS.FolderRowKind.Suggestion);
            var expectedScore = predictor.Suggestions.ToScoredArray(5).Single();
            suggestionRow.Score.Should().NotBeNull();
            suggestionRow.Score.Value.FolderPath.Should().Be(expectedScore.FolderPath);
            suggestionRow.Score.Value.Score.Should().Be(expectedScore.Score);
            suggestionRow.Score.Value.Probability.Should().Be(expectedScore.Probability);
        }

        [TestMethod]
        public void FindFolderRows_WithRecalcSuggestionsAndUnresolvableItem_ThrowsArgumentException()
        {
            // Arrange: a whitespace search string short-circuits GetMatchingFolders (no COM tree
            // needed), so execution reaches the recalcSuggestions branch, which recomputes
            // suggestions from objItem. A non-mail object cannot resolve and must throw — mirroring
            // FindFolder's identical recalcSuggestions block.
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new UtilitiesCS.FolderPredictor(globals.Object);

            // Act
            Action act = () =>
                predictor.FindFolderRows("   ", objItem: new object(), recalcSuggestions: true);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        // ---- Mocked-Outlook harness (mirrors FolderPredictorTests) ----

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
            IDictionary<string, OutlookFolder> childFolders = null,
            params OutlookFolder[] enumerableChildren
        )
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.Name).Returns(GetLeafName(folderPath));
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
            var enumerableItems = enumerableChildren is { Length: > 0 }
                ? enumerableChildren
                : (foldersByName?.Values?.ToArray() ?? Array.Empty<OutlookFolder>());
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

        private static Mock<IApplicationGlobals> CreateGlobals(
            Mock<Outlook.Application> app,
            OutlookFolder rootFolder,
            IEnumerable<string> recents = null
        )
        {
            var recentsList = recents is null
                ? new SloLinkedList<string>()
                : new SloLinkedList<string>(recents);
            var autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(x => x.RecentsList).Returns(recentsList);

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.App).Returns(app.Object);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(rootFolder.FolderPath);
            olObjects.SetupGet(x => x.Root).Returns(rootFolder);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.AF).Returns(autoFile.Object);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static string GetLeafName(string folderPath)
        {
            return folderPath.Split('\\').Last(segment => !string.IsNullOrWhiteSpace(segment));
        }
    }
}
