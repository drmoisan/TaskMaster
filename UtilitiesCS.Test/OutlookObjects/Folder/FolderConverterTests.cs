using System;
using System.Collections.Concurrent;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using UtilitiesCS;

using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderConverterTests
    {
        [TestMethod]
        public void ToFsFolderpath_WithStringInputs_MapsOutlookBranchIntoFilesystemBranch()
        {
            const string olBranchPath = "first.last@company.com\\Ol Level 1\\Common Level A\\Common Level B";
            const string olAncestorPath = "first.last@company.com\\Ol Level 1";
            const string fsAncestorEquivalent = "C:\\Fs Level 1\\Fs Level 2\\Fs Level 3";

            string actual = FolderConverter.ToFsFolderpath(olBranchPath, olAncestorPath, fsAncestorEquivalent);

            actual.Should().Be("C:\\Fs Level 1\\Fs Level 2\\Fs Level 3\\Common Level A\\Common Level B");
        }

        [TestMethod]
        public void SanitizeFilename_ReplacesInvalidCharactersWithUnderscores()
        {
            string actual = FolderConverter.SanitizeFilename("Quarterly<Report>|2026?.msg");

            actual.Should().Be("Quarterly_Report_2026_.msg");
        }

        [TestMethod]
        public void ToFsFolderpath_WhenMappedBranchContainsIllegalCharacters_ThrowsArgumentException()
        {
            Action act = () => FolderConverter.ToFsFolderpath(
                "Archive\\Needs?Cleanup",
                "Archive",
                "C:\\OneDriveRoot");

            act.Should().Throw<ArgumentException>()
                .WithParameterName("fsPath");
        }

        [TestMethod]
        public void ResolveOlRoot_WhenBranchIsUnderArchiveRoot_ReturnsArchiveRootPath()
        {
            var globals = CreateGlobals(archiveRootPath: "\\Archive", inboxPath: "\\Inbox", oneDrivePath: "C:\\OneDrive");

            string actual = FolderConverter.ResolveOlRoot("\\Archive\\Projects\\2026", globals.Object);

            actual.Should().Be("\\Archive");
        }

        [TestMethod]
        public void ResolveOlRoot_WhenBranchIsUnderInboxRoot_ReturnsInboxPath()
        {
            var globals = CreateGlobals(archiveRootPath: "\\Archive", inboxPath: "\\Inbox", oneDrivePath: "C:\\OneDrive");

            string actual = FolderConverter.ResolveOlRoot("\\Inbox\\Triage", globals.Object);

            actual.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void ResolveOlRoot_WhenBranchDoesNotMatchKnownRoots_ThrowsArgumentException()
        {
            var globals = CreateGlobals(archiveRootPath: "\\Archive", inboxPath: "\\Inbox", oneDrivePath: "C:\\OneDrive");

            Action act = () => FolderConverter.ResolveOlRoot("\\Elsewhere\\Folder", globals.Object);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*is not a branch of any known root folder*");
        }

        [TestMethod]
        public void ToFsFolderpath_WithAppGlobalsAndOneDriveFolder_ReturnsMappedFilesystemPath()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Projects\\2026");
            var globals = CreateGlobals(archiveRootPath: "\\Archive", inboxPath: "\\Inbox", oneDrivePath: "C:\\OneDrive");

            string actual = FolderConverter.ToFsFolderpath(folder.Object, globals.Object);

            actual.Should().Be("C:\\OneDrive\\Projects\\2026");
        }

        [TestMethod]
        public void ToFsFolderpath_WithAppGlobalsAndMissingOneDriveFolder_ReturnsNull()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Inbox\\Triage");
            var globals = CreateGlobals(archiveRootPath: "\\Archive", inboxPath: "\\Inbox", oneDrivePath: null);

            string actual = FolderConverter.ToFsFolderpath(folder.Object, globals.Object);

            actual.Should().BeNull();
        }

        private static Mock<IApplicationGlobals> CreateGlobals(string archiveRootPath, string inboxPath, string oneDrivePath)
        {
            var fileSystem = new Mock<IFileSystemFolderPaths>();
            var specialFolders = new ConcurrentDictionary<string, string>();
            if (!string.IsNullOrEmpty(oneDrivePath))
            {
                specialFolders["OneDrive"] = oneDrivePath;
            }

            fileSystem.SetupGet(x => x.SpecialFolders).Returns(specialFolders);

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(archiveRootPath);
            olObjects.SetupGet(x => x.InboxPath).Returns(inboxPath);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.FS).Returns(fileSystem.Object);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }
    }
}
