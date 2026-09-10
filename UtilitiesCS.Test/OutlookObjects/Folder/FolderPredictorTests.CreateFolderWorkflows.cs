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
        public void CreateFolder_WhenParentBranchPathIsEmpty_DoesNotThrowIndexOutOfRangeException()
        {
            // Regression test for issue #732: CreateFolder indexed parentBranchPath[0]
            // unconditionally via a bitwise `|` short-circuit-free OR, so an empty
            // parentBranchPath threw IndexOutOfRangeException instead of taking the
            // olAncestor.EndsWith('\\') branch. Expected: no exception is thrown.
            var archiveRoot = CreateFolder(
                "\\\\ArchiveRoot",
                new Dictionary<string, OutlookFolder>()
            );
            var app = CreateApplication(
                new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object }
            );
            var globals = CreateGlobals(app, archiveRoot.Object);
            var predictor = new TestableFolderPredictor(globals.Object, "FY26");

            Action act = () =>
                predictor.CreateFolder(string.Empty, "\\\\ArchiveRoot", "C:\\OneDrive");

            act.Should().NotThrow();
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
    }
}
