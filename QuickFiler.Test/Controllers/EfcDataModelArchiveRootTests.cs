using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #638 regression tests for the three unguarded <c>ArchiveRootPath</c> reads in
    /// <see cref="EfcDataModel"/>. Each read sits inside an <c>EmailFilerConfig</c> object
    /// initializer and previously let the archive-root guard's exception escape onto the UI
    /// thread. Reachable through the assembly's existing InternalsVisibleTo("QuickFiler.Test").
    /// </summary>
    [TestClass]
    public class EfcDataModelArchiveRootTests
    {
        // ArchiveRootPathGuard is internal to the TaskMaster assembly, so QuickFiler.Test
        // cannot reference its constants. These two fields are verbatim copies of
        // TaskMaster/AppGlobals/ArchiveRootPathGuard.cs UnresolvableRule and CrossStoreRule.
        private const string UnresolvableRuleText =
            "The Outlook archive root folder could not be resolved in the default store. The path is withheld from this message because it contains a mailbox address.";

        private const string CrossStoreRuleText =
            "The Outlook archive root resolved to a folder outside the composed archive root path, which indicates a cross-store or renamed archive. The paths are withheld from this message because they contain a mailbox address.";

        private const string ArchiveRootLiteral = @"\\mailbox@example.com\Archive";

        private const string MailboxAddress = "mailbox@example.com";

        private const string DestinationStem = @"Clients\North";

        /// <summary>
        /// Scenario: the archive root cannot be resolved in the default store, so the guard
        /// throws while the move configuration is being built.
        /// Expected outcome: the move reports failure by returning false instead of letting the
        /// exception escape onto the UI thread.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);

            // Act
            bool moved = await MoveAsync(dataModel);

            // Assert
            moved.Should().BeFalse();
        }

        /// <summary>
        /// Scenario: the archive root resolves to a folder outside the composed path, the second
        /// documented throw condition of the archive-root guard.
        /// Expected outcome: the move reports failure by returning false instead of throwing.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(CrossStoreRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);

            // Act
            bool moved = await MoveAsync(dataModel);

            // Assert
            moved.Should().BeFalse();
        }

        /// <summary>
        /// Scenario: the Outlook folder-open path reads an unresolvable archive root.
        /// Expected outcome: the call returns without throwing and raises exactly one
        /// user-facing diagnostic through the injectable seam.
        /// </summary>
        [TestMethod]
        public async Task OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);
            var reported = new List<string>();
            dataModel.UserDiagnosticAction = text => reported.Add(text);

            // Act
            await dataModel.OpenOlFolderAsync(DestinationStem);

            // Assert
            reported.Should().ContainSingle();
        }

        /// <summary>
        /// Scenario: the file-system folder-open path reads an unresolvable archive root.
        /// Expected outcome: the call returns without throwing and raises exactly one
        /// user-facing diagnostic through the injectable seam.
        /// </summary>
        [TestMethod]
        public async Task OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);
            var reported = new List<string>();
            dataModel.UserDiagnosticAction = text => reported.Add(text);

            // Act
            await dataModel.OpenFsFolderAsync(DestinationStem);

            // Assert
            reported.Should().ContainSingle();
        }

        /// <summary>
        /// Scenario: an archive-root failure raises a user-facing diagnostic.
        /// Expected outcome: the message carries neither the mailbox address nor the archive
        /// root path, matching the redaction contract the archive-root guard already honours.
        /// </summary>
        [TestMethod]
        public async Task ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);
            var reported = new List<string>();
            dataModel.UserDiagnosticAction = text => reported.Add(text);

            // Act
            await dataModel.OpenOlFolderAsync(DestinationStem);

            // Assert
            reported.Should().ContainSingle();
            reported[0].Should().NotContain(MailboxAddress);
            reported[0].Should().NotContain(ArchiveRootLiteral);
        }

        /// <summary>
        /// Scenario: the archive root resolves normally, so the guard must not change the
        /// success path.
        /// Expected outcome: the archive root is read exactly once. The move still fails deeper
        /// in the filer with a null reference, because the test mail helper carries no folder
        /// information; that is the barrier that stops any second archive-root read.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects.SetupGet(value => value.ArchiveRootPath).Returns(ArchiveRootLiteral);
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);
            Func<Task> act = () => MoveAsync(dataModel);

            // Act
            await act.Should().ThrowAsync<NullReferenceException>();

            // Assert
            olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once());
        }

        /// <summary>
        /// Scenario: no mail item is selected, so the model's mail information is null and the
        /// method's first guard returns immediately.
        /// Expected outcome: the move returns false and the archive root is never read.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new EfcDataModel(
                globals.Object,
                null,
                new CancellationTokenSource(),
                CancellationToken.None
            );

            // Act
            bool moved = await MoveAsync(dataModel);

            // Assert
            moved.Should().BeFalse();
            olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Never());
        }

        /// <summary>
        /// Scenario: the OneDrive special folder is missing, so the OneDrive guard returns before
        /// the archive-root read.
        /// Expected outcome: the move returns false and the archive root is never read. This pins
        /// the ordering constraint from the production side.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithoutOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);

            // Act
            bool moved = await MoveAsync(dataModel);

            // Assert
            moved.Should().BeFalse();
            olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Never());
        }

        /// <summary>
        /// Scenario: the archive-root read fails with a COM failure rather than with the guard's
        /// own exception type.
        /// Expected outcome: the COM exception still propagates. The guard narrows only the
        /// documented archive-root failure and must not become a broad catch.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new COMException("com failure"));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);
            Func<Task> act = () => MoveAsync(dataModel);

            // Act / Assert
            await act.Should().ThrowAsync<COMException>();
        }

        /// <summary>
        /// Scenario: the OneDrive special folder is missing on the Outlook folder-open path.
        /// Expected outcome: the call returns without throwing and never reads the archive root.
        /// </summary>
        [TestMethod]
        public async Task OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithoutOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);

            // Act
            await dataModel.OpenOlFolderAsync(DestinationStem);

            // Assert
            olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Never());
        }

        /// <summary>
        /// Scenario: the OneDrive special folder is missing on the file-system folder-open path.
        /// Expected outcome: the call returns without throwing and never reads the archive root.
        /// </summary>
        [TestMethod]
        public async Task OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot()
        {
            // Arrange
            var olObjects = CreateOlObjects();
            olObjects
                .SetupGet(value => value.ArchiveRootPath)
                .Throws(() => new InvalidOperationException(UnresolvableRuleText));
            var globals = CreateGlobals(olObjects, SpecialFoldersWithoutOneDrive());
            var dataModel = new TestableEfcDataModel(globals.Object);

            // Act
            await dataModel.OpenFsFolderAsync(DestinationStem);

            // Assert
            olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Never());
        }

        /// <summary>
        /// Invokes the five-argument move overload with the argument values every test in this
        /// class shares, so no test repeats the argument list.
        /// </summary>
        private static Task<bool> MoveAsync(EfcDataModel dataModel)
        {
            return dataModel.MoveToFolderAsync(
                DestinationStem,
                saveAttachments: false,
                saveEmail: false,
                savePictures: false,
                moveConversation: false
            );
        }

        /// <summary>
        /// A strict <see cref="IOlObjects"/> mock with no member configured. Callers add the
        /// single archive-root behavior the scenario needs; every other member stays
        /// unconfigured so an unexpected read fails loudly.
        /// </summary>
        private static Mock<IOlObjects> CreateOlObjects()
        {
            return new Mock<IOlObjects>(MockBehavior.Strict);
        }

        /// <summary>
        /// A strict <see cref="IApplicationGlobals"/> mock whose <c>Ol</c> getter returns the
        /// supplied Outlook seam and whose <c>FS</c> getter returns a stub exposing the supplied
        /// special-folder dictionary.
        /// </summary>
        private static Mock<IApplicationGlobals> CreateGlobals(
            Mock<IOlObjects> olObjects,
            ConcurrentDictionary<string, string> specialFolders
        )
        {
            var fileSystem = new Mock<IFileSystemFolderPaths>(MockBehavior.Strict);
            fileSystem.SetupGet(value => value.SpecialFolders).Returns(specialFolders);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(value => value.Ol).Returns(olObjects.Object);
            globals.SetupGet(value => value.FS).Returns(fileSystem.Object);
            return globals;
        }

        /// <summary>A special-folder dictionary that resolves the OneDrive root.</summary>
        private static ConcurrentDictionary<string, string> SpecialFoldersWithOneDrive()
        {
            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders["OneDrive"] = "OneDriveRoot";
            return specialFolders;
        }

        /// <summary>A special-folder dictionary with no OneDrive entry.</summary>
        private static ConcurrentDictionary<string, string> SpecialFoldersWithoutOneDrive()
        {
            return new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// The only arrangement in these tests that yields a non-null mail information snapshot
        /// without an Outlook COM fixture. The base constructor receives a null mail item, so the
        /// first-selection lookup absorbs the strict mock's failure on the unstubbed Outlook
        /// application and builds no resolver; the derived constructor then assigns the protected
        /// setter with a two-argument resolver, whose constructor stores its two fields and does
        /// no work, carrying a parameterless <see cref="MailItemHelper"/> whose folder information
        /// is null and which installs no lazy folder factory. The five-parameter resolver
        /// constructor must not be used here: it builds its helper through the lazy factory, whose
        /// materialization reads the archive root a second time.
        /// </summary>
        private sealed class TestableEfcDataModel : EfcDataModel
        {
            public TestableEfcDataModel(IApplicationGlobals globals)
                : base(globals, null, new CancellationTokenSource(), CancellationToken.None)
            {
                ConversationResolver = new ConversationResolver(globals, null)
                {
                    MailHelper = new MailItemHelper(),
                };
            }
        }
    }
}
