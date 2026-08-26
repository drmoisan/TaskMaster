using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class EmailFilerConfig_Tests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesDefaultValues()
        {
            // Act
            var config = new EmailFilerConfig();

            // Assert
            config.SavePictures.Should().BeFalse();
            config.DestinationOlStem.Should().BeEmpty();
            config.SaveMsg.Should().BeFalse();
            config.SaveAttachments.Should().BeFalse();
            config.RemovePreviousFsFiles.Should().BeFalse();
            config.Globals.Should().BeNull();
            config.OlAncestor.Should().BeEmpty();
            config.FsAncestorEquivalent.Should().BeNull();
            config.CanSort.Should().BeFalse();
            config.DeleteAndUnTrain.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithAllParameters_SetsAllProperties()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();

            // Act
            var config = new EmailFilerConfig(
                savePictures: true,
                destinationOlStem: "Archive",
                saveMsg: true,
                saveAttachments: true,
                removePreviousFsFiles: true,
                appGlobals: mockGlobals.Object,
                olAncestor: @"\\Mailbox\Root",
                fsAncestorEquivalent: @"C:\Mail"
            );

            // Assert
            config.SavePictures.Should().BeTrue();
            config.DestinationOlStem.Should().Be("Archive");
            config.SaveMsg.Should().BeTrue();
            config.SaveAttachments.Should().BeTrue();
            config.RemovePreviousFsFiles.Should().BeTrue();
            config.Globals.Should().BeSameAs(mockGlobals.Object);
            config.OlAncestor.Should().Be(@"\\Mailbox\Root");
            config.FsAncestorEquivalent.Should().Be(@"C:\Mail");
        }

        [TestMethod]
        public void Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new EmailFilerConfig();

            // Act
            config.SavePictures = true;
            config.DestinationOlStem = "Projects";
            config.DestinationOlPath = @"\\Root\Projects";
            config.SaveMsg = true;
            config.SaveAttachments = true;
            config.RemovePreviousFsFiles = true;
            config.SaveFsPath = @"C:\data\Projects";
            config.DeleteFsPath = @"C:\data\Inbox";
            config.OriginOlStem = "Inbox";
            config.DeleteAndUnTrain = true;
            config.CanSort = true;

            // Assert
            config.SavePictures.Should().BeTrue();
            config.DestinationOlStem.Should().Be("Projects");
            config.DestinationOlPath.Should().Be(@"\\Root\Projects");
            config.SaveMsg.Should().BeTrue();
            config.SaveAttachments.Should().BeTrue();
            config.RemovePreviousFsFiles.Should().BeTrue();
            config.SaveFsPath.Should().Be(@"C:\data\Projects");
            config.DeleteFsPath.Should().Be(@"C:\data\Inbox");
            config.OriginOlStem.Should().Be("Inbox");
            config.DeleteAndUnTrain.Should().BeTrue();
            config.CanSort.Should().BeTrue();
        }

        [TestMethod]
        public void IsDeleteRelevant_WhenFolderIsInbox_ReturnsFalse()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(x => x.InboxPath).Returns(@"\\Mailbox\Inbox");
            mockGlobals.Setup(x => x.Ol).Returns(mockOl.Object);

            var mockFolder = new Mock<Microsoft.Office.Interop.Outlook.Folder>();
            mockFolder.Setup(x => x.FolderPath).Returns(@"\\Mailbox\Inbox");

            var config = new EmailFilerConfig
            {
                Globals = mockGlobals.Object,
                OlAncestor = @"\\Mailbox",
            };

            // Act
            var result = config.IsDeleteRelevant(mockFolder.Object);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsDeleteRelevant_WhenFolderIsChildOfAncestor_ReturnsTrue()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(x => x.InboxPath).Returns(@"\\Mailbox\Inbox");
            mockGlobals.Setup(x => x.Ol).Returns(mockOl.Object);

            var mockFolder = new Mock<Microsoft.Office.Interop.Outlook.Folder>();
            mockFolder.Setup(x => x.FolderPath).Returns(@"\\Mailbox\Archive\Sub");

            var config = new EmailFilerConfig
            {
                Globals = mockGlobals.Object,
                OlAncestor = @"\\Mailbox",
            };

            // Act
            var result = config.IsDeleteRelevant(mockFolder.Object);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsDeleteRelevant_WhenFolderIsAncestorItself_ReturnsFalse()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(x => x.InboxPath).Returns(@"\\Mailbox\Inbox");
            mockGlobals.Setup(x => x.Ol).Returns(mockOl.Object);

            var mockFolder = new Mock<Microsoft.Office.Interop.Outlook.Folder>();
            mockFolder.Setup(x => x.FolderPath).Returns(@"\\Mailbox");

            var config = new EmailFilerConfig
            {
                Globals = mockGlobals.Object,
                OlAncestor = @"\\Mailbox",
            };

            // Act
            var result = config.IsDeleteRelevant(mockFolder.Object);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void FolderProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new EmailFilerConfig();
            var originFolder = new Mock<Folder>();
            var destinationFolder = new Mock<Folder>();

            // Act
            config.OriginFolder = originFolder.Object;
            config.DestinationOlFolder = destinationFolder.Object;

            // Assert
            config.OriginFolder.Should().BeSameAs(originFolder.Object);
            config.DestinationOlFolder.Should().BeSameAs(destinationFolder.Object);
        }

        [TestMethod]
        public void GetStem_RemovesAncestorAndLeadingSlash()
        {
            // Arrange
            var config = new EmailFilerConfig();

            // Act
            var stem = config.GetStem(@"\\Mailbox", @"\\Mailbox\Archive\Projects");

            // Assert
            stem.Should().Be(@"Archive\Projects");
        }

        [TestMethod]
        public void ResolvePaths_WithCurrentFolder_SetsDerivedPropertiesAndLeavesDestinationNullWhenUnresolved()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(x => x.InboxPath).Returns(@"\\Mailbox\Inbox");
            mockOl.Setup(x => x.App).Returns((Application)null);
            mockGlobals.Setup(x => x.Ol).Returns(mockOl.Object);

            var currentFolder = new Mock<Folder>();
            currentFolder.Setup(x => x.FolderPath).Returns(@"\\Mailbox\Archive\Projects");

            var config = new EmailFilerConfig
            {
                Globals = mockGlobals.Object,
                DestinationOlStem = "Filed",
                OlAncestor = @"\\Mailbox",
                FsAncestorEquivalent = @"C:\Mail",
            };

            // Act
            config.ResolvePaths(currentFolder.Object);

            // Assert
            config.DestinationOlPath.Should().Be(@"\\Mailbox\Filed");
            config.SaveFsPath.Should().Be(@"C:\Mail\Filed");
            config.DeleteAndUnTrain.Should().BeTrue();
            config.DeleteFsPath.Should().Be(@"C:\Mail\Archive\Projects");
            config.OriginFolder.Should().BeSameAs(currentFolder.Object);
            config.OriginOlStem.Should().Be(@"Archive\Projects");
            config.DestinationOlFolder.Should().BeNull();
            config.CanSort.Should().BeFalse();
        }

        [TestMethod]
        public void ResolvePaths_WithoutCurrentFolder_SetsDestinationPathAndSavePath()
        {
            // Arrange
            var config = new EmailFilerConfig
            {
                Globals = null,
                DestinationOlStem = "Filed",
                OlAncestor = @"\\Mailbox",
                FsAncestorEquivalent = @"C:\Mail",
            };

            // Act
            config.ResolvePaths();

            // Assert
            config.DestinationOlPath.Should().Be(@"\\Mailbox\Filed");
            config.SaveFsPath.Should().Be(@"C:\Mail\Filed");
            config.DestinationOlFolder.Should().BeNull();
        }

        [TestMethod]
        public void Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce()
        {
            // Arrange: this is a pure configuration seam with no Outlook, filesystem, or store access.
            var config = new EmailFilerConfig
            {
                Globals = null,
                OlAncestor = @"\\mailbox@example.com\Archive",
                DestinationOlStem = @"Clients\North",
                FsAncestorEquivalent = @"C:\Mail",
            };

            // Act
            config.ResolvePaths();

            // Assert
            config.DestinationOlPath.Should().Be(@"\\mailbox@example.com\Archive\Clients\North");
            config.SaveFsPath.Should().Be(@"C:\Mail\Clients\North");
        }

        [TestMethod]
        public void TryResolveDestinationFolder_WhenGlobalsAreMissing_ReturnsNull()
        {
            // Arrange
            var config = new EmailFilerConfig
            {
                Globals = null,
                DestinationOlPath = @"\\Mailbox\Filed",
            };

            // Act
            var result = config.TryResolveDestinationFolder();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers()
        {
            // Arrange: the pure configuration seam (no Outlook, filesystem, or store access).
            // DestinationOlStem carries a store-root Outlook path instead of an archive-relative
            // stem, which is the #614 leak shape reported from the field. All identifiers are
            // fabricated placeholders (#602 redaction).
            const string olAncestor = @"\\mailbox@example.com\Archive";
            const string storeRootStem = @"\\mailbox@example.com";
            const string fsAncestor = @"C:\Users\testuser\OneDrive - Contoso";
            var config = new EmailFilerConfig
            {
                Globals = null,
                OlAncestor = olAncestor,
                DestinationOlStem = storeRootStem,
                FsAncestorEquivalent = fsAncestor,
            };

            // Act
            System.Action act = () => config.ResolvePaths();

            // Assert: the contract exception must name the offending parameter and the
            // archive-relative rule, and must leak neither the mailbox address nor the
            // filesystem ancestor.
            System.ArgumentException thrown = act.Should()
                .Throw<System.ArgumentException>(
                    "a non-relative DestinationOlStem must be rejected before concatenation"
                )
                .Which;

            thrown
                .Message.Should()
                .Contain(
                    nameof(EmailFilerConfig.DestinationOlStem),
                    "the diagnostic must identify the offending parameter"
                )
                .And.Contain(
                    "archive-relative",
                    "the diagnostic must state the violated archive-relative stem rule"
                );
            thrown
                .Message.Should()
                .NotContain("mailbox@example.com", "the message must not leak a mailbox address")
                .And.NotContain(fsAncestor, "the message must not leak a user-profile path");
        }

        [TestMethod]
        public void Issue614_ResolvePathsWithFolder_RejectsStoreRootStemThroughTheFolderOverload()
        {
            // Arrange: the same #614 rejection must hold on the ResolvePaths(Folder) overload.
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(globals => globals.InboxPath).Returns(@"\\mailbox@example.com\Inbox");
            mockGlobals.Setup(globals => globals.Ol).Returns(mockOl.Object);
            var currentFolder = new Mock<Folder>();
            currentFolder
                .Setup(folder => folder.FolderPath)
                .Returns(@"\\mailbox@example.com\Archive\Clients");
            var config = new EmailFilerConfig
            {
                Globals = mockGlobals.Object,
                OlAncestor = @"\\mailbox@example.com\Archive",
                DestinationOlStem = @"\\mailbox@example.com",
                FsAncestorEquivalent = @"C:\Users\testuser\OneDrive - Contoso",
            };

            // Act
            System.Action act = () => config.ResolvePaths(currentFolder.Object);

            // Assert
            act.Should()
                .Throw<System.ArgumentException>()
                .WithParameterName(nameof(EmailFilerConfig.DestinationOlStem))
                .And.Message.Should()
                .NotContain("mailbox@example.com");
        }

        [TestMethod]
        public void Issue614_ResolvePaths_RejectsSingleSeparatorLeadingStem()
        {
            // Arrange: the D8 stem derivation currently emits single-separator-leading values.
            var config = new EmailFilerConfig
            {
                Globals = null,
                OlAncestor = @"\\mailbox@example.com\Archive",
                DestinationOlStem = @"\Clients\North",
                FsAncestorEquivalent = @"C:\Mail",
            };

            // Act
            System.Action act = () => config.ResolvePaths();

            // Assert
            act.Should()
                .Throw<System.ArgumentException>()
                .WithParameterName(nameof(EmailFilerConfig.DestinationOlStem));
        }

        [TestMethod]
        public void Issue614_ResolvePaths_RejectsEmptyStem()
        {
            // Arrange: an empty stem would resolve filing to the archive root itself.
            var config = new EmailFilerConfig
            {
                Globals = null,
                OlAncestor = @"\\mailbox@example.com\Archive",
                DestinationOlStem = string.Empty,
                FsAncestorEquivalent = @"C:\Mail",
            };

            // Act
            System.Action act = () => config.ResolvePaths();

            // Assert
            act.Should()
                .Throw<System.ArgumentException>()
                .WithParameterName(nameof(EmailFilerConfig.DestinationOlStem));
        }

        [TestMethod]
        public void Issue614_IsDeleteRelevant_NonPrefixAncestorSubstring_ReturnsFalse()
        {
            // Arrange: the ancestor name occurs DEEPER in the path, never as its prefix.
            var config = ConfigForDeleteRelevance(@"\\mailbox@example.com\Archive");
            var mockFolder = new Mock<Folder>();
            mockFolder
                .Setup(folder => folder.FolderPath)
                .Returns(@"\\mailbox@example.com\Inbox\Archive");

            // Act
            bool result = config.IsDeleteRelevant(mockFolder.Object);

            // Assert
            result.Should().BeFalse("the ancestor match must be prefix-anchored, not a substring");
        }

        [TestMethod]
        public void Issue614_IsDeleteRelevant_SeparatorBoundaryNearMiss_ReturnsFalse()
        {
            // Arrange: Archive2 is a SIBLING of Archive, not a folder inside it.
            var config = ConfigForDeleteRelevance(@"\\mailbox@example.com\Archive");
            var mockFolder = new Mock<Folder>();
            mockFolder
                .Setup(folder => folder.FolderPath)
                .Returns(@"\\mailbox@example.com\Archive2\Clients");

            // Act
            bool result = config.IsDeleteRelevant(mockFolder.Object);

            // Assert
            result.Should().BeFalse("the prefix match must be separator-terminated");
        }

        private static EmailFilerConfig ConfigForDeleteRelevance(string olAncestor)
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            mockOl.Setup(globals => globals.InboxPath).Returns(@"\\mailbox@example.com\Inbox");
            mockGlobals.Setup(globals => globals.Ol).Returns(mockOl.Object);
            return new EmailFilerConfig { Globals = mockGlobals.Object, OlAncestor = olAncestor };
        }
    }
}
