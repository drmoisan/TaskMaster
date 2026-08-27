using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Issue #614 regression matrix for <see cref="FolderConverter"/> (defects D5a through D5g).
    /// Every case is a pure string transform or a Moq seam; no test touches the filesystem, the
    /// network, Outlook COM, or a temporary file.
    /// </summary>
    [TestClass]
    public class FolderConverterIssue614Tests
    {
        private const string MailboxArchive = @"\\mailbox@example.com\Archive";
        private const string OneDriveRoot = @"C:\Users\testuser\OneDrive - Contoso";

        // ------------------------------------------------------------------------ AC6 (D5a)

        [TestMethod]
        public void ToFsFolderpath_DottedAndHyphenatedFilesystemRoot_Succeeds()
        {
            // Arrange / Act: the fs ancestor carries a dot, a space and a hyphen; it is the
            // CALLER's root and this converter must never validate it as a folder name.
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Clients\North",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\Clients\North");
        }

        [TestMethod]
        public void ToFsFolderpath_BracketedFilesystemRoot_Succeeds()
        {
            // Arrange / Act: '[' and ']' are legal in a Windows folder name and no longer banned.
            const string bracketedRoot = @"C:\Mail Archive [2026]";

            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Clients",
                MailboxArchive,
                bracketedRoot
            );

            // Assert
            actual.Should().Be(bracketedRoot + @"\Clients");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentContainingADot_Succeeds()
        {
            // Arrange / Act: a dot inside a derived folder name is legal (the #614 field crash).
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Acme Corp. Ltd\Invoices",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\Acme Corp. Ltd\Invoices");
        }

        // ------------------------------------------------------------------------ AC7 (D5b)

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentWithInvalidCharacter_Throws()
        {
            // Arrange
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    MailboxArchive + @"\Needs?Cleanup",
                    MailboxArchive,
                    OneDriveRoot
                );

            // Act / Assert
            act.Should().Throw<ArgumentException>().WithParameterName("fsPath");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentWithoutInvalidCharacter_Succeeds()
        {
            // Arrange / Act
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Needs Cleanup",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\Needs Cleanup");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentWithTrailingDot_Throws()
        {
            // Arrange
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    MailboxArchive + @"\Clients.\North",
                    MailboxArchive,
                    OneDriveRoot
                );

            // Act / Assert
            act.Should().Throw<ArgumentException>().WithParameterName("fsPath");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentWithInteriorDot_Succeeds()
        {
            // Arrange / Act: only a TRAILING dot is invalid.
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Cli.ents\North",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\Cli.ents\North");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentWithTrailingSpace_Throws()
        {
            // Arrange
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    MailboxArchive + @"\Clients \North",
                    MailboxArchive,
                    OneDriveRoot
                );

            // Act / Assert
            act.Should().Throw<ArgumentException>().WithParameterName("fsPath");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentWithInteriorSpace_Succeeds()
        {
            // Arrange / Act: only a TRAILING space is invalid.
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\North Clients\North",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\North Clients\North");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentIsAReservedDeviceName_Throws()
        {
            // Arrange
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    MailboxArchive + @"\COM1\North",
                    MailboxArchive,
                    OneDriveRoot
                );

            // Act / Assert
            act.Should().Throw<ArgumentException>().WithParameterName("fsPath");
        }

        [TestMethod]
        public void ToFsFolderpath_DerivedSegmentResemblingADeviceName_Succeeds()
        {
            // Arrange / Act: COM10 is not a reserved device name.
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\COM10\North",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\COM10\North");
        }

        // ------------------------------------------------------------------------ AC8 (D5c)

        [TestMethod]
        public void ToFsFolderpath_UncAncestor_NeitherThrowsNorManglesThePath()
        {
            // Arrange / Act: the removed Substring(3) drive-prefix assumption ate the first
            // three characters, which for a UNC ancestor are part of the store name.
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Clients",
                MailboxArchive,
                @"\\fileserver\Archive"
            );

            // Assert
            actual.Should().Be(@"\\fileserver\Archive\Clients");
        }

        [TestMethod]
        public void ToFsFolderpath_AncestorShorterThanThreeCharacters_DoesNotThrowOutOfRange()
        {
            // Arrange
            Func<string> act = () => FolderConverter.ToFsFolderpath(@"A\B", "A", "C:");

            // Act / Assert
            act.Should().NotThrow<ArgumentOutOfRangeException>();
            act().Should().Be(@"C:\B");
        }

        // ------------------------------------------------------------------------ AC9 (D5d)

        [TestMethod]
        public void ToFsFolderpath_RepeatedAncestorSubstring_IsStrippedOnlyAtThePrefix()
        {
            // Arrange / Act: "Archive" recurs deeper in the branch and must survive there.
            string actual = FolderConverter.ToFsFolderpath(
                MailboxArchive + @"\Clients\Archive\North",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\Clients\Archive\North");
        }

        [TestMethod]
        public void ToFsFolderpath_CaseDifferingAncestor_StillMatches()
        {
            // Arrange / Act
            string actual = FolderConverter.ToFsFolderpath(
                @"\\MAILBOX@EXAMPLE.COM\aRcHiVe\Clients",
                MailboxArchive,
                OneDriveRoot
            );

            // Assert
            actual.Should().Be(OneDriveRoot + @"\Clients");
        }

        [TestMethod]
        public void ToFsFolderpath_BranchOutsideTheAncestor_ThrowsWithoutLeakingIdentifiers()
        {
            // Arrange
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    @"\\mailbox@example.com\Inbox\Triage",
                    MailboxArchive,
                    OneDriveRoot
                );

            // Act
            ArgumentException thrown = act.Should().Throw<ArgumentException>().Which;

            // Assert
            thrown.Message.Should().NotContain("mailbox@example.com");
            thrown.Message.Should().NotContain("testuser");
        }

        // ----------------------------------------------------------------------- AC10 (D5e)

        [TestMethod]
        public void ToFsFolderpath_InvalidSegment_MessageLeaksNeitherMailboxNorFsAncestor()
        {
            // Arrange
            Action act = () =>
                FolderConverter.ToFsFolderpath(
                    MailboxArchive + @"\Needs?Cleanup",
                    MailboxArchive,
                    OneDriveRoot
                );

            // Act
            ArgumentException thrown = act.Should().Throw<ArgumentException>().Which;

            // Assert
            thrown.Message.Should().NotContain("mailbox@example.com");
            thrown.Message.Should().NotContain(OneDriveRoot);
            thrown.Message.Should().NotContain("testuser");
        }

        // ----------------------------------------------------------------------- AC12 (D5g)

        [TestMethod]
        public void ResolveOlRoot_SeparatorBoundaryNearMiss_DoesNotMatchTheArchiveBranch()
        {
            // Arrange: "Archive2" is a sibling of "Archive", so neither known root matches.
            Mock<IApplicationGlobals> globals = CreateGlobals(
                archiveRootPath: @"\Archive",
                inboxPath: @"\Inbox"
            );

            Action act = () => FolderConverter.ResolveOlRoot(@"\Archive2\Clients", globals.Object);

            // Act / Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*is not a branch of any known root folder*");
        }

        [TestMethod]
        public void ResolveOlRoot_UnderTheArchiveRoot_ReturnsTheArchiveRoot()
        {
            // Arrange
            Mock<IApplicationGlobals> globals = CreateGlobals(
                archiveRootPath: @"\Archive",
                inboxPath: @"\Inbox"
            );

            // Act
            string actual = FolderConverter.ResolveOlRoot(
                @"\Archive\Clients\North",
                globals.Object
            );

            // Assert
            actual.Should().Be(@"\Archive");
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            string archiveRootPath,
            string inboxPath
        )
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(objects => objects.ArchiveRootPath).Returns(archiveRootPath);
            olObjects.SetupGet(objects => objects.InboxPath).Returns(inboxPath);
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(value => value.Ol).Returns(olObjects.Object);
            return globals;
        }
    }
}
