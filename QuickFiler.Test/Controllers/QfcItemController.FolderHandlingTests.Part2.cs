using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #678 folder-handling tests: the single-initialisation invariant for a carried folder
    /// handler, the negative guard that a carried handler is ignored on the
    /// <c>FromArrayOrString</c> path, and the archive-rooted path-normalisation case. These live in
    /// a partial part because
    /// <c>QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs</c> is at 498 lines
    /// and has two lines of headroom to the 500-line cap. No second <c>[TestClass]</c> attribute is
    /// declared here; the attribute on the base part covers the whole class.
    /// </summary>
    public partial class QfcItemController_FolderHandlingTests
    {
        /// <summary>
        /// Builds a Moq mock of the predictor-construction delegate seam, configured to throw a
        /// sentinel when invoked. Moq mocks a delegate type directly, so the <c>Times</c> assertion
        /// AC16 requires is expressible without introducing a new interface.
        /// </summary>
        private static Mock<
            Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>
        > BuildThrowingPredictorFactoryMock()
        {
            var factory =
                new Mock<
                    Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>
                >();
            factory
                .Setup(f =>
                    f(
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<object>(),
                        It.IsAny<FolderPredictor.InitOptions>()
                    )
                )
                .Throws(
                    new InvalidOperationException(
                        "sentinel: the predictor factory must not be invoked for a carried handler"
                    )
                );
            return factory;
        }

        /// <summary>Verifies the predictor-construction seam was never invoked.</summary>
        private static void VerifyFactoryTimes(
            Mock<
                Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>
            > factory,
            Times times,
            string because
        ) =>
            factory.Verify(
                f =>
                    f(
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<object>(),
                        It.IsAny<FolderPredictor.InitOptions>()
                    ),
                times,
                because
            );

        /// <summary>
        /// AC16, the single-initialisation invariant. An item that arrives carrying an already
        /// initialised <see cref="IFolderSearchHandler"/> must adopt it: the predictor-construction
        /// seam is invoked exactly zero times and no second
        /// <c>FolderPredictor.InitAsync(FromField)</c> pass runs. Fails against the pre-change code,
        /// which always builds a predictor through the factory.
        /// </summary>
        [TestMethod]
        public async Task LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory()
        {
            // Arrange
            var controller = new FolderController();
            controller.ItemHelper = new MailItemHelper();
            SetPrivate(controller, "_globals", new Mock<IApplicationGlobals>().Object);
            var factory = BuildThrowingPredictorFactoryMock();
            SetPrivate(controller, "_folderPredictorFactory", factory.Object);
            var carried = new Mock<IFolderSearchHandler>().Object;
            SetPrivate(controller, "_carriedFolderHandler", carried);

            // Act
            await controller.LoadFolderHandlerAsync(CancellationToken.None);

            // Assert
            VerifyFactoryTimes(
                factory,
                Times.Never(),
                "an item carrying an initialised handler must not be scored a second time"
            );
            QfcItemControllerTestSupport
                .GetField(controller, "_folderHandler")
                .Should()
                .BeSameAs(
                    carried,
                    "the carried handler is adopted as the item controller's folder handler"
                );
        }

        /// <summary>
        /// AC9 negative guard. A carried handler is adopted in the <c>varList is null</c> branch
        /// only. A non-null <c>varList</c> is a caller-supplied folder search, not a per-item
        /// scoring pass, so the carried handler must be ignored and the predictor-construction seam
        /// must still be invoked with <c>FolderPredictor.InitOptions.FromArrayOrString</c>. Without
        /// this guard, an adoption placed before the branch test would silently return the scan-time
        /// suggestion set in response to a search the user typed.
        /// </summary>
        [TestMethod]
        public async Task LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory()
        {
            // Arrange — BOTH a carried handler and a non-null varList.
            var controller = new FolderController();
            SetPrivate(controller, "_globals", new Mock<IApplicationGlobals>().Object);
            var factory = BuildThrowingPredictorFactoryMock();
            SetPrivate(controller, "_folderPredictorFactory", factory.Object);
            SetPrivate(
                controller,
                "_carriedFolderHandler",
                new Mock<IFolderSearchHandler>().Object
            );
            object varList = new[] { "search-term" };

            // Act — the sentinel-throwing factory surfaces the invocation as the thrown exception.
            Func<Task> act = () =>
                controller.LoadFolderHandlerAsync(CancellationToken.None, varList);

            // Assert — the factory IS invoked despite the carried handler being present.
            await act.Should()
                .ThrowAsync<InvalidOperationException>(
                    "the FromArrayOrString path must build a predictor, not adopt a carried handler"
                );
            VerifyFactoryTimes(
                factory,
                Times.Once(),
                "a carried handler must be ignored when varList is non-null"
            );
        }

        /// <summary>
        /// AC12, the raw-versus-projected path mismatch. <c>FolderScoringService.ScoreAsync</c>
        /// returns the RAW top-suggestion path, while <c>FolderPredictor.FolderArray</c> stores the
        /// archive-prefix-stripped projection produced by <c>ProjectSuggestionPath</c>. For an
        /// archive-rooted suggestion the two forms differ, so an unnormalised
        /// <c>_itemViewer.FolderContains</c> probe misses, the preselection silently falls back to
        /// the index-1 entry, and the carried predetermined folder has no effect at all.
        ///
        /// This test models production: the archive root is <c>\\Archive</c>, the carried
        /// predetermined folder is the raw <c>\\Archive\Projects\Active</c>, and the folder array
        /// holds the projected <c>Projects\Active</c> exactly as <c>FolderArray</c> would. The
        /// viewer reports containment for the projected form only. Against the unnormalised code the
        /// probe misses and <c>SetFolderSelectedIndex</c> is called instead; with the projection in
        /// place the archive-rooted suggestion is preselected by name.
        /// </summary>
        [TestMethod]
        public void AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder()
        {
            // Arrange
            const string ArchiveRoot = @"\\Archive";
            const string RawSuggestion = @"\\Archive\Projects\Active";
            const string ProjectedSuggestion = @"Projects\Active";

            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.FolderContains(ProjectedSuggestion)).Returns(true);
            mock.Setup(v => v.GetSelectedFolder()).Returns(ProjectedSuggestion);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.Ol.ArchiveRootPath).Returns(ArchiveRoot);

            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_globals", globals.Object);
            SetPrivate(controller, "_predeterminedFolder", RawSuggestion);
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithArray(@"\\A\header", @"\\A\top", ProjectedSuggestion)
            );

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(
                v => v.SetFolderSelectedItem(ProjectedSuggestion),
                Times.Once(),
                "the archive-rooted suggestion must be preselected by name once both sides use the "
                    + "same normalisation"
            );
            mock.Verify(
                v => v.SetFolderSelectedIndex(It.IsAny<int>()),
                Times.Never(),
                "falling back to index selection is the defect this criterion removes"
            );
            controller.SelectedFolder.Should().Be(ProjectedSuggestion);
        }

        /// <summary>
        /// AC12 boundary cases for the projection helper itself. A null or empty archive root, a
        /// path that does not start with the root, a path equal to the root plus a separator with
        /// nothing after it, and a case-differing root are each pinned, so the helper cannot be
        /// simplified into something that mangles a non-archive path.
        /// </summary>
        [TestMethod]
        public void ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection()
        {
            QfcItemController
                .ProjectPredeterminedFolder(@"\\Archive\Projects\Active", null)
                .Should()
                .Be(@"\\Archive\Projects\Active", "a null archive root is the identity projection");
            QfcItemController
                .ProjectPredeterminedFolder(@"\\Archive\Projects\Active", string.Empty)
                .Should()
                .Be(
                    @"\Archive\Projects\Active",
                    "a non-null globals with an EMPTY archive root gives FolderPredictor an "
                        + "archivePrefix of one separator, which it strips"
                );
            QfcItemController
                .ProjectPredeterminedFolder(null, @"\\Archive")
                .Should()
                .BeNull("a null path is returned unchanged");
            QfcItemController
                .ProjectPredeterminedFolder(@"\\Other\Projects", @"\\Archive")
                .Should()
                .Be(@"\\Other\Projects", "a path outside the archive root is not stripped");
            QfcItemController
                .ProjectPredeterminedFolder(@"\\Archive\", @"\\Archive")
                .Should()
                .Be(@"\\Archive\", "stripping must not produce an empty remainder");
            QfcItemController
                .ProjectPredeterminedFolder(@"\\ARCHIVE\Projects", @"\\archive")
                .Should()
                .Be(@"Projects", "the prefix comparison is case-insensitive");
        }

        /// <summary>
        /// Issue #678, remediation R2. The boundary case the projection previously got wrong: a
        /// non-null globals whose <c>ArchiveRootPath</c> is EMPTY, with a leading-separator
        /// suggestion path. <c>FolderPredictor.ProjectSuggestionPath</c> guards only on
        /// <c>_globals is null</c> and then forms <c>ArchiveRootPath + "\\"</c> unconditionally, so
        /// in this state its prefix is a single separator and its <c>FolderArray</c> entries ARE
        /// stripped. The carried <c>PredeterminedFolder</c> must be projected the same way, or
        /// <c>FolderContains</c> misses and the selection falls back to the index-1 entry — the
        /// exact AC12 defect the change set out to close.
        ///
        /// The assertion is made at the <c>FolderContains</c> boundary rather than on the equality
        /// of two helper bodies, because that boundary is what decides whether the row shows the
        /// predetermined folder or an arbitrary index-1 suggestion.
        /// </summary>
        [TestMethod]
        public void AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder()
        {
            // Arrange
            const string RawSuggestion = @"\Projects\Active";
            const string ProjectedSuggestion = @"Projects\Active";

            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            mock.Setup(v => v.FolderContains(ProjectedSuggestion)).Returns(true);
            mock.Setup(v => v.GetSelectedFolder()).Returns(ProjectedSuggestion);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.Ol.ArchiveRootPath).Returns(string.Empty);

            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_globals", globals.Object);
            SetPrivate(controller, "_predeterminedFolder", RawSuggestion);
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithArray(@"\\A\header", @"\\A\top", ProjectedSuggestion)
            );

            // Act
            controller.AssignFolderComboBox();

            // Assert
            mock.Verify(
                v => v.SetFolderSelectedItem(ProjectedSuggestion),
                Times.Once(),
                "an empty archive root still strips the leading separator in FolderPredictor, so "
                    + "the carried value must be stripped the same way to match"
            );
            mock.Verify(
                v => v.SetFolderSelectedIndex(It.IsAny<int>()),
                Times.Never(),
                "falling back to index selection is the defect this remediation removes"
            );
        }

        /// <summary>
        /// Issue #678, remediation R3. Every pre-change route into the predictor ran inside
        /// <c>await Task.Run(..., cancel)</c>, which returns a cancelled task for an
        /// already-cancelled token, so the await threw an <c>OperationCanceledException</c> and
        /// <c>_folderHandler</c> was never assigned. The carried-handler adoption branch added by
        /// this change bypassed that route entirely and returned normally, silently adopting the
        /// handler for work the caller had already cancelled.
        ///
        /// The invariant is that an already-cancelled token produces the same observable outcome on
        /// the adoption path as it did on the pre-change path: the exception propagates and
        /// <c>_folderHandler</c> is not assigned.
        /// </summary>
        [TestMethod]
        public async Task LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation()
        {
            // Arrange
            var controller = new FolderController();
            SetPrivate(controller, "_globals", new Mock<IApplicationGlobals>().Object);
            var factory = BuildThrowingPredictorFactoryMock();
            SetPrivate(controller, "_folderPredictorFactory", factory.Object);
            SetPrivate(
                controller,
                "_carriedFolderHandler",
                new Mock<IFolderSearchHandler>().Object
            );

            // A using STATEMENT rather than a using declaration: QuickFiler.Test compiles at
            // C# 7.3, where a using declaration is CS8370.
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();

                // Act
                Func<Task> act = () => controller.LoadFolderHandlerAsync(cancelled.Token);

                // Assert
                await act.Should()
                    .ThrowAsync<OperationCanceledException>(
                        "the pre-change Task.Run(..., cancel) route threw for an already-cancelled "
                            + "token, and the adoption path must reproduce that outcome"
                    );
                QfcItemControllerTestSupport
                    .GetField(controller, "_folderHandler")
                    .Should()
                    .BeNull("a cancelled request must not adopt the carried handler");
                VerifyFactoryTimes(
                    factory,
                    Times.Never(),
                    "cancellation is observed before any predictor construction is attempted"
                );
            }
        }
    }
}
