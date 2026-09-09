using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    public partial class FolderPredictorTests
    {
        [TestMethod]
        public void FolderArray_WhenSuggestionsAndRecentsExist_ReturnsSuggestionsThenRecents()
        {
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object, new[] { "Recent\\One" });
            var predictor = new FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion("Archive\\Inbox", 10);

            var result = predictor.FolderArray;

            result
                .Should()
                .Equal(
                    "========= SUGGESTIONS =========",
                    "Archive\\Inbox",
                    "======= RECENT SELECTIONS ========",
                    "Recent\\One"
                );
        }

        [TestMethod]
        public void Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths()
        {
            const string archiveRootPath = @"\\mailbox@example.com\Archive";
            const string inRootFullPath = @"\\mailbox@example.com\Archive\Clients\North";
            const string relativePath = @"Clients\North";
            const string outOfRootFullPath = @"\\other@example.com\Archive\Clients\North";
            var archiveRoot = CreateFolder(archiveRootPath);
            var globals = CreateGlobals(new Mock<Outlook.Application>(), archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion(inRootFullPath, 30);
            predictor.Suggestions.AddSuggestion(relativePath, 20);
            predictor.Suggestions.AddSuggestion(outOfRootFullPath, 10);

            var folderArray = predictor.FolderArray;
            var folderRows = predictor.FolderRowArray;
            var inRootSuggestionRow = folderRows[1];
            var relativeSuggestionRow = folderRows[2];
            var outOfRootSuggestionRow = folderRows.Single(row => row.Text == outOfRootFullPath);

            folderArray.Should().Contain("========= SUGGESTIONS =========");
            folderArray.Should().NotContain(inRootFullPath);
            folderArray.Count(path => path == relativePath).Should().Be(2);
            folderArray.Should().Contain(outOfRootFullPath);
            inRootSuggestionRow.Text.Should().Be(relativePath);
            inRootSuggestionRow.Score.Should().NotBeNull();
            inRootSuggestionRow.Score!.Value.FolderPath.Should().Be(relativePath);
            relativeSuggestionRow.Text.Should().Be(relativePath);
            relativeSuggestionRow.Score.Should().NotBeNull();
            relativeSuggestionRow.Score!.Value.FolderPath.Should().Be(relativePath);
            outOfRootSuggestionRow.Score.Should().NotBeNull();
            outOfRootSuggestionRow.Score!.Value.FolderPath.Should().Be(outOfRootFullPath);
        }

        [TestMethod]
        public void Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath()
        {
            const string archiveRootPath = @"\\mailbox@example.com\Archive";
            const string caseVariantInRootFullPath = @"\\MAILBOX@EXAMPLE.COM\archive\Clients\North";
            const string relativePath = @"Clients\North";
            var archiveRoot = CreateFolder(archiveRootPath);
            var globals = CreateGlobals(new Mock<Outlook.Application>(), archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion(caseVariantInRootFullPath, 30);

            var folderArray = predictor.FolderArray;
            var folderRows = predictor.FolderRowArray;
            var suggestionRow = folderRows[1];

            folderArray.Should().Equal("========= SUGGESTIONS =========", relativePath);
            folderRows
                .Select(row => row.Text)
                .Should()
                .Equal("========= SUGGESTIONS =========", relativePath);
            suggestionRow.Score.Should().NotBeNull();
            suggestionRow.Score!.Value.FolderPath.Should().Be(relativePath);
        }

        [TestMethod]
        public void AddRecents_WhenRecentsExist_AppendsHeaderAndEntries()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object,
                new[] { "Recent\\One", "Recent\\Two" }
            );
            var predictor = new FolderPredictor(globals.Object);
            var folderList = new List<string>();

            predictor.AddRecents(ref folderList);

            folderList
                .Should()
                .Equal("======= RECENT SELECTIONS ========", "Recent\\One", "Recent\\Two");
        }

        [TestMethod]
        public void AddSuggestions_WhenSuggestionsExist_AppendsHeaderAndTopSuggestions()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion("Archive\\A", 10);
            predictor.Suggestions.AddSuggestion("Archive\\B", 5);
            var folderList = new List<string>();

            predictor.AddSuggestions(ref folderList);

            folderList
                .Should()
                .Equal("========= SUGGESTIONS =========", "Archive\\A", "Archive\\B");
        }
    }
}
