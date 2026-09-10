using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

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
        ///
        /// Retargeted by issue #799 AC4: an EMPTY archive root is now the identity projection.
        /// The former behaviour formed an archive prefix of a single separator and stripped it,
        /// which produced a value that was neither a valid full path nor a valid archive-relative
        /// stem. The other five boundary cases are unchanged, because the shared projection
        /// reproduces each of them exactly.
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
                    @"\\Archive\Projects\Active",
                    "AC4 of issue #799 removed the empty-root strip, so an empty archive root "
                        + "is now the identity projection"
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
        /// Issue #678, remediation R2, re-derived against the issue #799 AC4 behaviour: a non-null
        /// globals whose <c>ArchiveRootPath</c> is EMPTY, with a leading-separator suggestion path.
        /// The shared projection now leaves BOTH the <c>FolderArray</c> entries and the carried
        /// <c>PredeterminedFolder</c> unchanged in that state, so the two must agree on the
        /// unstripped value. The invariant under test is unchanged and is the one that matters:
        /// the carried value must be projected exactly as the array entries are, or
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

            // #799 AC4: an empty archive root is the identity projection, so the projected value
            // and the raw value are the same string.
            const string ProjectedSuggestion = RawSuggestion;

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
                "an empty archive root is the identity projection in FolderPredictor, so the "
                    + "carried value must be carried through the same way to match"
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

        /// <summary>
        /// Builds a <see cref="FolderPredictor"/> via the globals-providing constructor so
        /// <c>Suggestions</c> is non-null (matching production initialization), with a known
        /// <c>FolderArray</c> seeded the same way <see cref="BuildFolderHandlerWithArray"/> does.
        /// Used only by the #813 regression test below, which must observe
        /// <c>SetFolderSuggestions</c> being invoked.
        /// </summary>
        private static FolderPredictor BuildFolderHandlerWithSuggestions(
            IApplicationGlobals globals,
            params string[] folders
        )
        {
            var fp = new FolderPredictor(globals);
            typeof(FolderPredictor)
                .GetField("_folderList", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(fp, new List<string>(folders));
            return fp;
        }

        /// <summary>
        /// Issue #813. <c>Ol.ArchiveRootPath</c> throws <see cref="InvalidOperationException"/> when the
        /// archive root is unconfigured or unresolvable. The read at <c>AssignFolderComboBox</c>'s
        /// predetermined-folder projection step must not propagate that exception onto the UI dispatcher
        /// thread; it must degrade to no preselection while leaving the combo box and suggestion rows
        /// populated.
        /// </summary>
        [TestMethod]
        public void AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing()
        {
            // Arrange
            var mock = new Mock<IItemViewer>();
            mock.SetupGet(v => v.InvokeRequired).Returns(false);
            // FolderContains returns false because the predetermined folder text is not present in the
            // populated array; this isolates the assertion to the archive-root fallback behavior
            // (AC3) independent of any containment match.
            mock.Setup(v => v.FolderContains(It.IsAny<string>())).Returns(false);
            mock.Setup(v => v.GetSelectedFolder()).Returns(string.Empty);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.Ol.ArchiveRootPath).Throws<InvalidOperationException>();
            globals.SetupGet(g => g.AF.RecentsList).Returns(new SloLinkedList<string>());

            var controller = new FolderController();
            SetPrivate(controller, "_itemViewer", mock.Object);
            SetPrivate(controller, "_globals", globals.Object);
            SetPrivate(controller, "_predeterminedFolder", @"\\A\chosen");
            SetPrivate(
                controller,
                "_folderHandler",
                BuildFolderHandlerWithSuggestions(globals.Object, @"\\A\header", @"\\A\top")
            );

            // Act
            Action act = () => controller.AssignFolderComboBox();

            // Assert
            act.Should()
                .NotThrow<InvalidOperationException>(
                    "an unresolvable archive root must degrade to no preselection instead of "
                        + "propagating onto the UI dispatcher thread"
                );
            mock.Verify(
                v => v.AddFolderItems(It.IsAny<string[]>()),
                Times.Once(),
                "the combo box must still populate even though the archive-root read fails"
            );
            mock.Verify(
                v => v.SetFolderSuggestions(It.IsAny<IReadOnlyList<FolderRow>>()),
                Times.Once(),
                "suggestion rows must still populate even though the archive-root read fails"
            );
            mock.Verify(
                v => v.SetFolderSelectedItem(It.IsAny<string>()),
                Times.Never(),
                "no preselection can occur once the archive-root read fails"
            );
            mock.Verify(
                v => v.SetFolderSelectedIndex(It.IsAny<int>()),
                Times.Once(),
                "the index-fallback path must run instead of preselection"
            );
        }
    }
}
