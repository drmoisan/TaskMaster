using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderConverterTests
    {
        [TestMethod]
        public void ToFsFolderpath_WithStringInputs_MapsOutlookBranchIntoFilesystemBranch()
        {
            const string olBranchPath =
                "first.last@company.com\\Ol Level 1\\Common Level A\\Common Level B";
            const string olAncestorPath = "first.last@company.com\\Ol Level 1";
            const string fsAncestorEquivalent = "C:\\Fs Level 1\\Fs Level 2\\Fs Level 3";

            string actual = FolderConverter.ToFsFolderpath(
                olBranchPath,
                olAncestorPath,
                fsAncestorEquivalent
            );

            actual
                .Should()
                .Be("C:\\Fs Level 1\\Fs Level 2\\Fs Level 3\\Common Level A\\Common Level B");
        }

        [TestMethod]
        public void SanitizeFilename_ReplacesInvalidCharactersWithUnderscores()
        {
            string actual = FolderConverter.SanitizeFilename("Quarterly<Report>|2026?.msg");

            actual.Should().Be("Quarterly_Report_2026_.msg");
        }

        [TestMethod]
        public void SanitizeFilename_WithNullArgument_ThrowsArgumentNullException()
        {
            Action act = () => FolderConverter.SanitizeFilename(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("filename");
        }

        [TestMethod]
        public void ToFsFolderpath_WhenMappedBranchContainsIllegalCharacters_ThrowsArgumentException()
        {
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    "Archive\\Needs?Cleanup",
                    "Archive",
                    "C:\\OneDriveRoot"
                );

            act.Should().Throw<ArgumentException>().WithParameterName("fsPath");
        }

        [TestMethod]
        public void ResolveOlRoot_WhenBranchIsUnderArchiveRoot_ReturnsArchiveRootPath()
        {
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: "C:\\OneDrive"
            );

            string actual = FolderConverter.ResolveOlRoot(
                "\\Archive\\Projects\\2026",
                globals.Object
            );

            actual.Should().Be("\\Archive");
        }

        [TestMethod]
        public void ResolveOlRoot_WhenBranchIsUnderInboxRoot_ReturnsInboxPath()
        {
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: "C:\\OneDrive"
            );

            string actual = FolderConverter.ResolveOlRoot("\\Inbox\\Triage", globals.Object);

            actual.Should().Be("\\Inbox");
        }

        [TestMethod]
        public void ResolveOlRoot_WhenBranchDoesNotMatchKnownRoots_ThrowsArgumentException()
        {
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: "C:\\OneDrive"
            );

            Action act = () => FolderConverter.ResolveOlRoot("\\Elsewhere\\Folder", globals.Object);

            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*is not a branch of any known root folder*");
        }

        [TestMethod]
        public void ToFsFolderpath_WithAppGlobalsAndOneDriveFolder_ReturnsMappedFilesystemPath()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Projects\\2026");
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: "C:\\OneDrive"
            );

            string actual = FolderConverter.ToFsFolderpath(folder.Object, globals.Object);

            actual.Should().Be("C:\\OneDrive\\Projects\\2026");
        }

        [TestMethod]
        public void ToFsFolderpath_WithAppGlobalsAndMissingOneDriveFolder_ReturnsNull()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Inbox\\Triage");
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: null
            );

            string actual = FolderConverter.ToFsFolderpath(folder.Object, globals.Object);

            actual.Should().BeNull();
        }

        [TestMethod]
        public void ToFsFolderpath_WithMAPIFolderOverload_MapsOutlookBranchIntoFilesystemBranch()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Projects\\2026");

            string actual = FolderConverter.ToFsFolderpath(
                (Outlook.MAPIFolder)folder.Object,
                "\\Archive",
                "C:\\OneDrive"
            );

            actual.Should().Be("C:\\OneDrive\\Projects\\2026");
        }

        [TestMethod]
        public void ToFsFolderpath_WithArgumentPathValidation_ThrowsWhenAncestorPathIsNull()
        {
            Action act = () =>
                FolderConverter.ToFsFolderpath("\\Archive\\Projects", null, "C:\\OneDrive");

            act.Should().Throw<ArgumentNullException>().WithParameterName("olAncestorPath");
        }

        [TestMethod]
        public void InjectedPrompt_IsLegalFolderName_WhenPromptReturnsReplacement_UsesInjectedAlternative()
        {
            var method = typeof(FolderConverter).GetMethod(
                "IsLegalFolderName",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(bool) },
                null
            );
            method.Should().NotBeNull();
            var original = FolderConverter.AlternativeFolderPrompt;

            try
            {
                FolderConverter.AlternativeFolderPrompt = _ => (true, "Clean_Name");

                var result = ((bool legal, string revisedFolder))
                    method.Invoke(null, new object[] { "Bad?Name", true });

                result.legal.Should().BeTrue();
                result.revisedFolder.Should().Be("Clean_Name");
            }
            finally
            {
                FolderConverter.AlternativeFolderPrompt = original;
            }
        }

        [TestMethod]
        public void AskUserForAlternatives_WhenDialogReturnsEmpty_KeepsOriginalIllegalName()
        {
            var method = typeof(FolderConverter).GetMethod(
                "AskUserForAlternatives",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            method.Should().NotBeNull();
            var original = FolderConverter.AlternativeFolderSelectionDialog;

            try
            {
                FolderConverter.AlternativeFolderSelectionDialog = (_, _, _, _) => string.Empty;

                var result = ((bool legal, string revisedFolder))
                    method.Invoke(null, new object[] { "Bad?Name" });

                result.legal.Should().BeFalse();
                result.revisedFolder.Should().Be("Bad?Name");
            }
            finally
            {
                FolderConverter.AlternativeFolderSelectionDialog = original;
            }
        }

        [TestMethod]
        public void AskUserForAlternatives_WhenDialogReturnsIllegalThenLegal_RepeatsUntilLegal()
        {
            var method = typeof(FolderConverter).GetMethod(
                "AskUserForAlternatives",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            method.Should().NotBeNull();
            var original = FolderConverter.AlternativeFolderSelectionDialog;
            var responses = new Queue<string>(new[] { "Still?Bad", "Clean_Name" });

            try
            {
                FolderConverter.AlternativeFolderSelectionDialog = (_, _, _, _) =>
                    responses.Dequeue();

                var result = ((bool legal, string revisedFolder))
                    method.Invoke(null, new object[] { "Bad?Name" });

                result.legal.Should().BeTrue();
                result.revisedFolder.Should().Be("Clean_Name");
                responses.Should().BeEmpty();
            }
            finally
            {
                FolderConverter.AlternativeFolderSelectionDialog = original;
            }
        }

        [TestMethod]
        public void DialogAlternatives_BuildAlternativesDictionary_ContainsExpectedChoices()
        {
            var method = typeof(FolderConverter).GetMethod(
                "BuildAlternativesDictionary",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            method.Should().NotBeNull();

            var result = (System.Collections.Generic.Dictionary<string, Func<Task<string>>>)
                method.Invoke(null, new object[] { "Bad?Name" });

            result
                .Keys.Should()
                .Contain(
                    new[]
                    {
                        "Skip",
                        "Replace with underscore",
                        "Remove illegal characters",
                        "Enter new folder name",
                    }
                );
        }

        [TestMethod]
        public void PrivateIsLegalFolderName_WithNullEmptyAndInvalidValues_ReturnsExpectedResults()
        {
            var method = typeof(FolderConverter).GetMethod(
                "IsLegalFolderName",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(string) },
                null
            );
            method.Should().NotBeNull();

            ((bool)method.Invoke(null, new object[] { null })).Should().BeFalse();
            ((bool)method.Invoke(null, new object[] { string.Empty })).Should().BeFalse();
            ((bool)method.Invoke(null, new object[] { "ValidName" })).Should().BeTrue();
            ((bool)method.Invoke(null, new object[] { "Bad?Name" })).Should().BeFalse();
        }

        [TestMethod]
        public void PrivateIsLegalFolderName_WithAskUserFalse_RetainsIllegalName()
        {
            var method = typeof(FolderConverter).GetMethod(
                "IsLegalFolderName",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(bool) },
                null
            );
            method.Should().NotBeNull();

            var result = ((bool legal, string revisedFolder))
                method.Invoke(null, new object[] { "Bad?Name", false });

            result.legal.Should().BeFalse();
            result.revisedFolder.Should().Be("Bad?Name");
        }

        [TestMethod]
        public void DialogAlternatives_BuildAlternativesDictionary_RunsNonInteractiveChoices()
        {
            var method = typeof(FolderConverter).GetMethod(
                "BuildAlternativesDictionary",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            method.Should().NotBeNull();

            var result = (System.Collections.Generic.Dictionary<string, Func<Task<string>>>)
                method.Invoke(null, new object[] { "Bad?Name" });

            result["Skip"]().GetAwaiter().GetResult().Should().BeEmpty();
            result["Replace with underscore"]().GetAwaiter().GetResult().Should().Be("Bad_Name");
            result["Remove illegal characters"]().GetAwaiter().GetResult().Should().Be("BadName");
        }

        [TestMethod]
        public void DialogAlternatives_BuildAlternativesDictionary_UsesInjectedInputDialogForManualEntry()
        {
            var method = typeof(FolderConverter).GetMethod(
                "BuildAlternativesDictionary",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            method.Should().NotBeNull();
            var original = FolderConverter.AlternativeFolderInputDialog;
            var capturedDefaults = new List<string>();

            try
            {
                FolderConverter.AlternativeFolderInputDialog = (_, _, defaultValue) =>
                {
                    capturedDefaults.Add(defaultValue);
                    return "Manual_Name";
                };

                var result = (System.Collections.Generic.Dictionary<string, Func<Task<string>>>)
                    method.Invoke(null, new object[] { "Bad?Name" });

                result["Enter new folder name"]
                    ()
                    .GetAwaiter()
                    .GetResult()
                    .Should()
                    .Be("Manual_Name");
                capturedDefaults.Should().ContainSingle().Which.Should().Be("Bad_Name");
            }
            finally
            {
                FolderConverter.AlternativeFolderInputDialog = original;
            }
        }

        [TestMethod]
        public void ToFsFolderpath_WithOtherNullArguments_ThrowsArgumentNullException()
        {
            Action nullBranch = () =>
                FolderConverter.ToFsFolderpath((string)null, "\\Archive", "C:\\OneDrive");
            Action nullFsAncestor = () =>
                FolderConverter.ToFsFolderpath("\\Archive\\Projects", "\\Archive", null);

            nullBranch.Should().Throw<ArgumentNullException>().WithParameterName("olBranchPath");
            nullFsAncestor
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("fsAncestorEquivalent");
        }

        [TestMethod]
        public void ToFsFolderpath_WithMapiFolderAndMissingOneDrive_ReturnsNull()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Inbox\\Triage");
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: null
            );

            string actual = FolderConverter.ToFsFolderpath(
                (Outlook.MAPIFolder)folder.Object,
                globals.Object
            );

            actual.Should().BeNull();
        }

        [TestMethod]
        public void ToFsFolderpath_WithMapiFolderAndOneDriveFolder_ReturnsMappedFilesystemPath()
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Projects\\2026");
            var globals = CreateGlobals(
                archiveRootPath: "\\Archive",
                inboxPath: "\\Inbox",
                oneDrivePath: "C:\\OneDrive"
            );

            string actual = FolderConverter.ToFsFolderpath(
                (Outlook.MAPIFolder)folder.Object,
                globals.Object
            );

            actual.Should().Be("C:\\OneDrive\\Projects\\2026");
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            string archiveRootPath,
            string inboxPath,
            string oneDrivePath
        )
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
