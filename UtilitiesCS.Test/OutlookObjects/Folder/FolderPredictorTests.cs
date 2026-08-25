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
    [TestClass]
    public class FolderPredictorTests
    {
        [TestMethod]
        public void Predictor_returns_highest_ranked_match_from_seed_data()
        {
            FolderPredictor.NormalizePredictionPath(null).Should().BeEmpty();
        }

        [TestMethod]
        public void Predictor_returns_controlled_result_when_user_choice_is_cancelled()
        {
            FolderPredictor.NormalizePredictionPath("x").Should().Be("x");
        }

        [TestMethod]
        public void NormalizePredictionPath_returns_empty_string_for_empty_string_input()
        {
            FolderPredictor.NormalizePredictionPath(string.Empty).Should().BeEmpty();
        }

        [TestMethod]
        public async Task InitAsync_WithNoSuggestionsOption_ReturnsSelf()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);

            var result = await predictor.InitAsync(
                "ignored",
                FolderPredictor.InitOptions.NoSuggestions
            );

            result.Should().BeSameAs(predictor);
        }

        [TestMethod]
        public async Task InitAsync_WithUnknownOption_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () => predictor.InitAsync("ignored", (FolderPredictor.InitOptions)999);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public void Constructor_WithGlobalsObjectAndOptions_InitializesSuggestionsAndFlags()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );

            var predictor = new FolderPredictor(
                globals.Object,
                new object(),
                FolderPredictor.InitOptions.NoSuggestions
            );

            predictor.Suggestions.Should().NotBeNull();
            predictor.BlUpdateSuggestions.Should().BeFalse();
        }

        [TestMethod]
        public async Task InitAsync_WithFromArrayOrString_PopulatesFolderArray()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);

            var result = await predictor.InitAsync(
                new[] { @"Archive\Inbox", @"Archive\Sent" },
                FolderPredictor.InitOptions.FromArrayOrString
            );

            result.Should().BeSameAs(predictor);
            predictor.FolderArray.Should().Equal(@"Archive\Inbox", @"Archive\Sent");
        }

        [TestMethod]
        public async Task InitAsync_WithFromFieldAndUnsupportedObject_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () =>
                predictor.InitAsync(new object(), FolderPredictor.InitOptions.FromField);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InitAsync_WithRecalculateAndUnsupportedObject_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () =>
                predictor.InitAsync(new object(), FolderPredictor.InitOptions.Recalculate);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InitializeFromEmail_WhenNullPassed_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () => predictor.InitializeFromEmail(null);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public void FromArrayOrString_WhenNullPassed_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Action act = () => predictor.FromArrayOrString(null);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FromArrayOrString_WhenStringPassed_AddsSuggestionToScorer()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object)
            {
                Suggestions = new FolderScorer(),
            };

            predictor.FromArrayOrString(@"Archive\Inbox");

            predictor.Suggestions.Count.Should().Be(1);
            predictor.Suggestions[0].Should().Be(@"Archive\Inbox");
        }

        [TestMethod]
        public void FromArrayOrString_WhenStringArrayPassed_DoesNotThrow()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Action act = () =>
                predictor.FromArrayOrString(new[] { @"Archive\Inbox", @"Archive\Sent" });

            act.Should().NotThrow();
            predictor.FolderArray.Should().Equal(@"Archive\Inbox", @"Archive\Sent");
        }

        [TestMethod]
        public void FromArrayOrString_WhenUnsupportedTypePassed_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Action act = () => predictor.FromArrayOrString(123);

            act.Should().Throw<ArgumentException>();
        }

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

        [TestMethod]
        public void FindFolder_WhenMatchesSuggestionsAndRecentsExist_ReturnsAllSections()
        {
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
            var predictor = new FolderPredictor(globals.Object);
            predictor.Suggestions.AddSuggestion("Archive\\Inbox", 10);

            var result = predictor.FindFolder("*FY26", objItem: null, recalcSuggestions: false);

            result
                .Should()
                .ContainInOrder(
                    "======= SEARCH RESULTS =======",
                    "Projects\\FY26",
                    "========= SUGGESTIONS =========",
                    "Archive\\Inbox",
                    "======= RECENT SELECTIONS ========",
                    "Recent\\One"
                );
        }

        [TestMethod]
        public void GetFolder_WhenRootedPathExists_ReturnsNestedFolder()
        {
            var fy26 = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\FY26",
                new Dictionary<string, OutlookFolder>()
            );
            var projects = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder> { ["FY26"] = fy26.Object }
            );
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = projects.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);

            var result = predictor.GetFolder("\\\\ArchiveRoot\\Projects\\FY26");

            result.Should().BeSameAs(fy26.Object);
        }

        [TestMethod]
        public void GetFolder_WithThrowExTrue_WhenFolderMissing_ThrowsArgumentException()
        {
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);

            Action act = () => predictor.GetFolder("\\\\ArchiveRoot\\Missing", throwEx: true);

            act.Should().Throw<ArgumentException>().WithParameterName("folderpath");
        }

        [TestMethod]
        public void GetMatchingFolders_WhenSearchMatchesAndParentIsExcludedWithoutChildren_ReturnsChildMatches()
        {
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
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);

            var result = predictor.GetMatchingFolders(
                "*FY26",
                "\\\\ArchiveRoot",
                includeChildren: true,
                new[] { (excludedFolder: "Projects", excludeChildren: false) }
            );

            result.Should().Equal("Projects\\FY26");
        }

        [TestMethod]
        public void RefreshSuggestions_WhenObjectCannotResolveToMail_ThrowsArgumentException()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new FolderPredictor(globals.Object);
            Action act = () => predictor.RefreshSuggestions(new object());

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public async Task DefaultPromptAndUiSeams_WhenInjected_UseBaseImplementationHooks()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new FolderPredictor(globals.Object);
            var originalPrompt = FolderPredictor.PromptForFolderNameDialog;
            var originalPromptWithDefault = FolderPredictor.PromptForFolderNameWithDefaultDialog;
            var originalMessage = FolderPredictor.ShowPromptMessageAction;
            var originalUi = FolderPredictor.EnterUiContextAsyncAction;
            var originalDirectory = FolderPredictor.CreateDirectoryPathFactory;
            var shownMessages = new List<string>();

            try
            {
                FolderPredictor.PromptForFolderNameDialog = (prompt, title) => $"{title}:{prompt}";
                FolderPredictor.PromptForFolderNameWithDefaultDialog = (
                    prompt,
                    title,
                    defaultValue
                ) => $"{title}:{defaultValue}";
                FolderPredictor.ShowPromptMessageAction = shownMessages.Add;
                FolderPredictor.EnterUiContextAsyncAction = () => Task.CompletedTask;
                FolderPredictor.CreateDirectoryPathFactory = path => new DirectoryInfo(path);

                predictor.PromptForFolderName("Prompt", "Title").Should().Be("Title:Prompt");
                predictor.PromptForFolderName("Prompt", "Title", "Seed").Should().Be("Title:Seed");
                predictor.ShowPromptMessage("hello");
                await predictor.EnterUiContextAsync();
                predictor
                    .CreateDirectoryPath("C:\\Temp\\Predictor")
                    .FullName.Should()
                    .Contain("Predictor");
                shownMessages.Should().ContainSingle().Which.Should().Be("hello");
            }
            finally
            {
                FolderPredictor.PromptForFolderNameDialog = originalPrompt;
                FolderPredictor.PromptForFolderNameWithDefaultDialog = originalPromptWithDefault;
                FolderPredictor.ShowPromptMessageAction = originalMessage;
                FolderPredictor.EnterUiContextAsyncAction = originalUi;
                FolderPredictor.CreateDirectoryPathFactory = originalDirectory;
            }
        }

        [TestMethod]
        public async Task EnterUiContextAsync_WhenUiSyncContextPostsSynchronously_CompletesUsingDefaultAction()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new FolderPredictor(globals.Object);
            var uiThreadType = typeof(UiThread);
            var syncContextField = uiThreadType.GetField(
                "_uiSyncContext",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
            );
            syncContextField.Should().NotBeNull();
            var originalSyncContext = (SynchronizationContext)syncContextField.GetValue(null);

            try
            {
                syncContextField.SetValue(null, new ImmediateSynchronizationContext());

                await predictor.EnterUiContextAsync();
            }
            finally
            {
                syncContextField.SetValue(null, originalSyncContext);
            }
        }

        [TestMethod]
        public void GetFolder_WithThrowExFalse_WhenFolderMissing_UsesPromptSeamAndReturnsNull()
        {
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);
            var originalMessage = FolderPredictor.ShowPromptMessageAction;
            var shownMessages = new List<string>();

            try
            {
                FolderPredictor.ShowPromptMessageAction = shownMessages.Add;

                var result = predictor.GetFolder("\\\\ArchiveRoot\\Missing", throwEx: false);

                result.Should().BeNull();
                shownMessages.Should().ContainSingle();
                shownMessages[0].Should().Contain("does not exist");
            }
            finally
            {
                FolderPredictor.ShowPromptMessageAction = originalMessage;
            }
        }

        [TestMethod]
        public void GetMatchingFolders_WhenSearchStringIsWhitespace_ReturnsEmptyList()
        {
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);

            var result = predictor.GetMatchingFolders(
                "   ",
                "\\\\ArchiveRoot",
                includeChildren: true,
                Array.Empty<(string excludedFolder, bool excludeChildren)>()
            );

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetMatchingFolders_WhenFolderIsExcludedWithChildren_SkipsExcludedBranch()
        {
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
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);

            var result = predictor.GetMatchingFolders(
                "*FY26",
                "\\\\ArchiveRoot",
                includeChildren: true,
                new[] { (excludedFolder: "Projects", excludeChildren: true) }
            );

            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetOlSubpath_WhenAncestorEndsWithSlashOrChildrenExcluded_ReturnsExpectedSegment()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new FolderPredictor(globals.Object);

            predictor
                .GetOlSubpath("\\\\ArchiveRoot\\Projects\\FY26", "\\\\ArchiveRoot\\", true)
                .Should()
                .Be("Projects\\FY26");
            predictor
                .GetOlSubpath("\\\\ArchiveRoot\\Projects\\FY26", "\\\\ArchiveRoot", false)
                .Should()
                .Be("FY26");
        }

        [TestMethod]
        public void CreateFolder_WhenParentBranchStartsWithSeparator_UsesCombinedPathWithoutDoubleSlash()
        {
            var createdFolder = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\FY26",
                new Dictionary<string, OutlookFolder>()
            );
            var childFolders = new Mock<OutlookFolders>();
            childFolders
                .Setup(x => x.Add("FY26", It.IsAny<object>()))
                .Returns((Outlook.MAPIFolder)createdFolder.Object);
            childFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList().GetEnumerator());
            childFolders.Setup(x => x[It.IsAny<object>()]).Returns((Outlook.MAPIFolder)null);
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder>()
            );
            parent.SetupGet(x => x.Folders).Returns(childFolders.Object);
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = parent.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new TestableFolderPredictor(globals.Object, "FY26");

            var result = predictor.CreateFolder("\\Projects", "\\\\ArchiveRoot", "C:\\OneDrive");

            result.Should().BeSameAs(createdFolder.Object);
            predictor.CreatedDirectories.Should().ContainSingle("C:\\OneDrive\\Projects\\FY26");
        }

        [TestMethod]
        public async Task CreateFolderAsync_WhenParentDoesNotExist_ReturnsNull()
        {
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new TestableFolderPredictor(globals.Object, "FY26");

            var result = await predictor.CreateFolderAsync(
                "Projects",
                "\\\\ArchiveRoot",
                "C:\\OneDrive",
                CancellationToken.None
            );

            result.Should().BeNull();
        }

        [TestMethod]
        public async Task CreateFolderAsync_WhenAsyncValidationRetries_CoversMessagesAndCreatesDirectory()
        {
            var existingChild = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\Existing",
                new Dictionary<string, OutlookFolder>()
            );
            var createdFolder = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\Fresh",
                new Dictionary<string, OutlookFolder>()
            );
            var childFolders = new Mock<OutlookFolders>();
            childFolders
                .Setup(x => x.Add("Fresh", It.IsAny<object>()))
                .Returns((Outlook.MAPIFolder)createdFolder.Object);
            childFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList(new[] { existingChild.Object }).GetEnumerator());
            childFolders
                .Setup(x => x[It.IsAny<object>()])
                .Returns<object>(key =>
                    key is string name && name == "Existing" ? existingChild.Object : null
                );
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder>()
            );
            parent.SetupGet(x => x.Folders).Returns(childFolders.Object);
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = parent.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new FolderPredictor(globals.Object);
            var originalPrompt = FolderPredictor.PromptForFolderNameDialog;
            var originalMessage = FolderPredictor.ShowPromptMessageAction;
            var originalUi = FolderPredictor.EnterUiContextAsyncAction;
            var originalDirectory = FolderPredictor.CreateDirectoryPathFactory;
            var promptResponses = new Queue<string>(
                new[] { "Bad?Name", new string('L', 31), "Existing", "Fresh" }
            );
            var shownMessages = new List<string>();
            var createdPaths = new List<string>();

            try
            {
                FolderPredictor.PromptForFolderNameDialog = (_, _) => promptResponses.Dequeue();
                FolderPredictor.ShowPromptMessageAction = shownMessages.Add;
                FolderPredictor.EnterUiContextAsyncAction = () => Task.CompletedTask;
                FolderPredictor.CreateDirectoryPathFactory = path =>
                {
                    createdPaths.Add(path);
                    return new DirectoryInfo(path);
                };

                var result = await predictor.CreateFolderAsync(
                    "Projects",
                    "\\\\ArchiveRoot",
                    "C:\\OneDrive",
                    CancellationToken.None
                );

                result.Should().BeSameAs(createdFolder.Object);
                shownMessages.Should().Contain(message => message.Contains("illegal characters"));
                shownMessages.Should().Contain(message => message.Contains("30 characters"));
                shownMessages.Should().Contain(message => message.Contains("already exists"));
                createdPaths
                    .Should()
                    .ContainSingle()
                    .Which.Should()
                    .Be("C:\\OneDrive\\Projects\\Fresh");
                promptResponses.Should().BeEmpty();
            }
            finally
            {
                FolderPredictor.PromptForFolderNameDialog = originalPrompt;
                FolderPredictor.ShowPromptMessageAction = originalMessage;
                FolderPredictor.EnterUiContextAsyncAction = originalUi;
                FolderPredictor.CreateDirectoryPathFactory = originalDirectory;
            }
        }

        [TestMethod]
        public void InjectedPrompt_InputFoldername_WhenNameIsIllegalThenValid_ReturnsValidName()
        {
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder>()
            );
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new TestableFolderPredictor(globals.Object, "Bad?Name", "ValidName");

            var result = predictor.InputFoldername(parent.Object);

            result.Should().Be("ValidName");
            predictor
                .Messages.Should()
                .ContainSingle(message => message.Contains("illegal characters"));
        }

        [TestMethod]
        public void InputFoldername_WhenNameIsTooLongOrDuplicate_PromptsUntilValid()
        {
            var existingChild = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\Existing",
                new Dictionary<string, OutlookFolder>()
            );
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder> { ["Existing"] = existingChild.Object },
                existingChild.Object
            );
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );
            var predictor = new TestableFolderPredictor(
                globals.Object,
                new string('L', 31),
                "Existing",
                "Fresh"
            );

            var result = predictor.InputFoldername(parent.Object);

            result.Should().Be("Fresh");
            predictor.Messages.Should().Contain(message => message.Contains("30 characters"));
            predictor.Messages.Should().Contain(message => message.Contains("already exists"));
        }

        [TestMethod]
        public void InjectedDirectory_CreateFolder_WhenPromptSuppliesName_CreatesFolderAndDirectoryPath()
        {
            var createdFolder = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\FY26",
                new Dictionary<string, OutlookFolder>()
            );
            var childFolders = new Mock<OutlookFolders>();
            childFolders
                .Setup(x => x.Add("FY26", It.IsAny<object>()))
                .Returns((Outlook.MAPIFolder)createdFolder.Object);
            childFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList().GetEnumerator());
            childFolders.Setup(x => x[It.IsAny<object>()]).Returns((Outlook.MAPIFolder)null);
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder>()
            );
            parent.SetupGet(x => x.Folders).Returns(childFolders.Object);
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = parent.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new TestableFolderPredictor(globals.Object, "FY26");

            var result = predictor.CreateFolder("Projects", "\\\\ArchiveRoot", "C:\\OneDrive");

            result.Should().BeSameAs(createdFolder.Object);
            predictor.CreatedDirectories.Should().ContainSingle("C:\\OneDrive\\Projects\\FY26");
        }

        [TestMethod]
        public void CreateFolder_WhenAncestorIsNull_UsesArchiveRootAndCreatesFolder()
        {
            var createdFolder = CreateFolder(
                "\\\\ArchiveRoot\\Projects\\FY26",
                new Dictionary<string, OutlookFolder>()
            );
            var childFolders = new Mock<OutlookFolders>();
            childFolders
                .Setup(x => x.Add("FY26", It.IsAny<object>()))
                .Returns((Outlook.MAPIFolder)createdFolder.Object);
            childFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList().GetEnumerator());
            childFolders.Setup(x => x[It.IsAny<object>()]).Returns((Outlook.MAPIFolder)null);
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder>()
            );
            parent.SetupGet(x => x.Folders).Returns(childFolders.Object);
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = parent.Object },
                parent.Object
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new TestableFolderPredictor(globals.Object, "FY26");

            var result = predictor.CreateFolder("Projects", null, "C:\\OneDrive");

            result.Should().BeSameAs(createdFolder.Object);
            predictor.CreatedDirectories.Should().ContainSingle("C:\\OneDrive\\Projects\\FY26");
        }

        [TestMethod]
        public async Task InjectedUi_CreateFolderAsync_WhenPromptReturnsNull_DoesNotCreateDirectory()
        {
            var childFolders = new Mock<OutlookFolders>();
            childFolders
                .Setup(x => x.GetEnumerator())
                .Returns(() => new ArrayList().GetEnumerator());
            childFolders.Setup(x => x[It.IsAny<object>()]).Returns((Outlook.MAPIFolder)null);
            var parent = CreateFolder(
                "\\\\ArchiveRoot\\Projects",
                new Dictionary<string, OutlookFolder>()
            );
            parent.SetupGet(x => x.Folders).Returns(childFolders.Object);
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder> { ["Projects"] = parent.Object }
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new TestableFolderPredictor(globals.Object, null);

            var result = await predictor.CreateFolderAsync(
                "Projects",
                "\\\\ArchiveRoot",
                "C:\\OneDrive",
                CancellationToken.None
            );

            result.Should().BeNull();
            predictor.CreatedDirectories.Should().BeEmpty();
            predictor.EnterUiContextCalls.Should().BeGreaterThan(0);
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

        private sealed class TestableFolderPredictor : FolderPredictor
        {
            private readonly Queue<string> promptResponses;

            public TestableFolderPredictor(
                IApplicationGlobals globals,
                params string[] promptResponses
            )
                : base(globals)
            {
                this.promptResponses = new Queue<string>(promptResponses ?? Array.Empty<string>());
            }

            public List<string> Messages { get; } = new();

            public List<string> CreatedDirectories { get; } = new();

            public int EnterUiContextCalls { get; private set; }

            internal override string PromptForFolderName(
                string prompt,
                string title,
                string defaultValue = null
            )
            {
                return promptResponses.Count > 0 ? promptResponses.Dequeue() : null;
            }

            internal override void ShowPromptMessage(string message)
            {
                Messages.Add(message);
            }

            internal override Task EnterUiContextAsync()
            {
                EnterUiContextCalls++;
                return Task.CompletedTask;
            }

            internal override DirectoryInfo CreateDirectoryPath(string path)
            {
                CreatedDirectories.Add(path);
                return new DirectoryInfo(path);
            }
        }

        private sealed class ImmediateSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state)
            {
                d(state);
            }
        }
    }
}
