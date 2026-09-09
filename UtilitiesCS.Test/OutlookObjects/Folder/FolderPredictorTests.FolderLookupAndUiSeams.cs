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

    }
}
