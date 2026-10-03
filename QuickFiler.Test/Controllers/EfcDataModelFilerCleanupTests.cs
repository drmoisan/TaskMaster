using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #959 regression tests for the prompt-state reset of the five-parameter
    /// <c>MoveToFolderAsync</c> of <see cref="EfcDataModel"/>: the reset runs whether the filer
    /// returns or throws, and does not run when a guard returns early. The probe overrides the
    /// two virtual seams, so no filer and no dialog is constructed. The fixture helpers mirror
    /// those of EfcDataModelArchiveRootTests, which is deliberately left unchanged.
    /// </summary>
    [TestClass]
    public class EfcDataModelFilerCleanupTests
    {
        private const string ArchiveRootLiteral = @"\\mailbox@example.com\Archive";
        private const string DestinationStem = @"Clients\North";

        /// <summary>
        /// Scenario: the filer throws. Expected: the exception propagates to the caller and the
        /// prompt state was reset exactly once on the way out (the sticky-answer defect).
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates()
        {
            // Arrange
            var probe = CreateProbe(SpecialFoldersWithOneDrive());
            probe.Filer = () =>
                Task.FromException<bool>(new InvalidOperationException("filer failed"));

            // Act
            Func<Task> act = () => MoveAsync(probe);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("filer failed");
            probe.ResetCalls.Should().Be(1);
        }

        /// <summary>
        /// Scenario (control): the filer returns true. Expected: the move reports success and the
        /// prompt state was reset exactly once.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce()
        {
            // Arrange
            var probe = CreateProbe(SpecialFoldersWithOneDrive());
            probe.Filer = () => Task.FromResult(true);

            // Act
            bool moved = await MoveAsync(probe);

            // Assert
            moved.Should().BeTrue();
            probe.ResetCalls.Should().Be(1);
        }

        /// <summary>
        /// Scenario (control): the OneDrive guard returns before the filer is invoked. Expected:
        /// the move reports failure and the prompt state is not reset, as today.
        /// </summary>
        [TestMethod]
        public async Task MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState()
        {
            // Arrange
            var probe = CreateProbe(SpecialFoldersWithoutOneDrive());
            probe.Filer = () => Task.FromResult(true);

            // Act
            bool moved = await MoveAsync(probe);

            // Assert
            moved.Should().BeFalse();
            probe.ResetCalls.Should().Be(0);
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
        /// Builds a probe whose archive root resolves and whose special folders are the supplied
        /// dictionary; the guard test passes a dictionary without the OneDrive entry.
        /// </summary>
        private static FilerCleanupProbe CreateProbe(
            ConcurrentDictionary<string, string> specialFolders
        )
        {
            var olObjects = CreateOlObjects();
            olObjects.SetupGet(value => value.ArchiveRootPath).Returns(ArchiveRootLiteral);
            var globals = CreateGlobals(olObjects, specialFolders);
            return new FilerCleanupProbe(globals.Object);
        }

        /// <summary>
        /// A strict <see cref="IOlObjects"/> mock with no member configured, so an unexpected
        /// read fails loudly.
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
        /// The data model under test with both virtual seams overridden. The base constructor
        /// receives a null mail item, so the first-selection lookup absorbs the strict mock's
        /// failure and builds no resolver; the derived constructor then assigns a two-argument
        /// resolver carrying a parameterless <see cref="MailItemHelper"/>, which makes the mail
        /// information non-null without an Outlook fixture (the EfcDataModelArchiveRootTests
        /// arrangement). The filer seam runs the scripted delegate; the reset seam counts calls.
        /// </summary>
        private sealed class FilerCleanupProbe : EfcDataModel
        {
            public FilerCleanupProbe(IApplicationGlobals globals)
                : base(globals, null, new CancellationTokenSource(), CancellationToken.None)
            {
                ConversationResolver = new ConversationResolver(globals, null)
                {
                    MailHelper = new MailItemHelper(),
                };
            }

            public Func<Task<bool>> Filer { get; set; } = () => Task.FromResult(true);

            public int ResetCalls { get; private set; }

            protected internal override Task<bool> InvokeFilerAsync(
                EmailFilerConfig config,
                IList<MailItemHelper> mailHelpers
            ) => Filer();

            protected internal override void ResetFilerPromptState()
            {
                ResetCalls++;
            }
        }
    }
}
