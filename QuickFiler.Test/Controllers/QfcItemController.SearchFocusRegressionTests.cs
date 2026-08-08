using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #438 regression suite for the folder-search keystroke focus steal.
    /// <para>
    /// Every keystroke in the QuickFiler folder-search textbox raises
    /// <c>TextBoxSearch_TextChanged</c>. Before the fix the handler unconditionally issued
    /// <c>SetFolderDroppedDown(true)</c>, which opens the breadcrumb popup and moves keyboard focus
    /// onto the popup surface, and <c>SetFolderSelectedIndex(1)</c>, which mutates the committed
    /// model selection. The user therefore lost the caret after one to two characters.
    /// </para>
    /// <para>
    /// These tests pin the negative half of AC-1: the search path issues no focus-transfer intent
    /// and no committed-selection change. The seam is entirely headless — a
    /// <see cref="Mock{IItemViewer}"/> and a <see cref="Mock{IFolderSearchHandler}"/>, with no
    /// WinForms control, window handle, or message pump — mirroring the arrangement of
    /// <c>QfcItemController.EventHandlersTests</c>.
    /// </para>
    /// </summary>
    [TestClass]
    public class QfcItemController_SearchFocusRegressionTests
    {
        private const string SearchQuery = "query";

        private static readonly string[] MatchedFolders = { @"\\A\one", @"\\A\two" };

        private const string PreSearchFolder = @"\\A\committed-before-search";

        /// <summary>
        /// AC-1 (negative): a search keystroke must never ask the viewer to change the drop-down
        /// state, because opening the drop-down is what transfers focus off the search textbox.
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_NeverRequestsADropDownStateChange()
        {
            // Arrange and act
            Mock<IItemViewer> viewer = InvokeSearch(MatchedFolders);

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never());
        }

        /// <summary>
        /// AC-1 (positive): a search keystroke issues the presentation intent exactly once, carrying
        /// the exact <c>FindFolder</c> result. One intent per keystroke is what lets the coordinator
        /// layer own the replace/open/highlight sequencing on its posted FIFO queue.
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_IssuesThePresentationIntentExactlyOnce()
        {
            // Arrange and act
            Mock<IItemViewer> viewer = InvokeSearch(MatchedFolders);

            // Assert
            viewer.Verify(v => v.PresentFolderSearchResults(MatchedFolders), Times.Once());
        }

        /// <summary>
        /// AC-1 (negative): a search keystroke must never focus the folder drop-down. Focus moves to
        /// the drop-down only on an explicit gesture (Down arrow, drop-down arrow click, row click).
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_NeverFocusesTheFolderDropDown()
        {
            // Arrange and act
            Mock<IItemViewer> viewer = InvokeSearch(MatchedFolders);

            // Assert
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Never());
        }

        /// <summary>
        /// AC-1 (negative): a search keystroke must never commit a folder selection.
        /// <c>SetFolderSelectedIndex</c> mutates the committed model selection, raises
        /// <c>SelectionChanged</c>, and leaves a stale controller-cached folder after Escape.
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_NeverCommitsAFolderSelection()
        {
            // Arrange and act
            Mock<IItemViewer> viewer = InvokeSearch(MatchedFolders);

            // Assert
            viewer.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
        }

        /// <summary>
        /// AC-1 / AC-9 (edge): a single-row result set must take the same non-focusing, non-committing
        /// path, so the row-count branch cannot reintroduce a focus transfer.
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_SingleResult_StillTransfersNoFocusAndCommitsNothing()
        {
            // Arrange and act
            Mock<IItemViewer> viewer = InvokeSearch(new[] { @"\\A\only" });

            // Assert
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Never());
            viewer.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
        }

        /// <summary>
        /// AC-1 / AC-9 (edge): an empty result set must be a deterministic no-op for focus and
        /// selection, and must not throw.
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_EmptyResult_TransfersNoFocusAndDoesNotThrow()
        {
            // Arrange
            Mock<IItemViewer> viewer = BuildViewer();
            HarnessController controller = BuildController(viewer, Array.Empty<string>());

            // Act
            Action act = () => controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);

            // Assert
            act.Should().NotThrow();
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Never());
            viewer.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
        }

        /// <summary>
        /// AC-5 (controller-cache half): after a search sequence followed by an Escape / uncommitted
        /// cancel, the controller's cached selected folder still equals the value committed before
        /// the search began.
        /// <para>
        /// The mechanism: the search path no longer calls <c>SetFolderSelectedIndex</c>, so the
        /// breadcrumb pipeline raises no <c>FolderSelectionChanged</c> while typing, so
        /// <c>CboFolders_SelectedIndexChanged</c> never caches a mid-search value. Because
        /// <c>CancelSelector</c> deliberately reports no selection change, an Escape that restores
        /// the model raises nothing either — which is exactly why a mid-search cache would have been
        /// stranded before the fix.
        /// </para>
        /// </summary>
        [TestMethod]
        public void SearchThenCancel_LeavesTheCachedFolderAtThePreSearchCommittedValue()
        {
            // Arrange — the viewer reports the pre-search committed folder, and the controller
            // caches it once through the same handler the live pipeline drives.
            Mock<IItemViewer> viewer = BuildViewer();
            viewer.Setup(v => v.GetSelectedFolder()).Returns(PreSearchFolder);
            HarnessController controller = BuildController(viewer, MatchedFolders);
            QfcItemControllerTestSupport.InvokeNonPublic(
                controller,
                "CboFolders_SelectedIndexChanged",
                null,
                EventArgs.Empty
            );
            CachedFolder(controller).Should().Be(PreSearchFolder);

            // Act — several keystrokes, then an Escape/uncommitted cancel. The cancel raises no
            // FolderSelectionChanged, so no further handler invocation occurs.
            controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);
            controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);
            controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);

            // Assert
            CachedFolder(controller)
                .Should()
                .Be(
                    PreSearchFolder,
                    "no mid-search value may be cached, so the cancel has nothing stale to leave behind"
                );
            viewer.Verify(v => v.SetFolderSelectedIndex(It.IsAny<int>()), Times.Never());
            viewer.Verify(v => v.PresentFolderSearchResults(MatchedFolders), Times.Exactly(3));
        }

        /// <summary>
        /// AC-6 (controller half): a multi-character query typed one character at a time issues one
        /// presentation intent per keystroke, each carrying the wildcard-wrapped complete text read
        /// from <c>SearchText</c> at that moment — no truncation after one to two characters.
        /// </summary>
        [TestMethod]
        public void TextBoxSearch_TextChanged_PerKeystroke_QueriesTheCompleteSearchTextEachTime()
        {
            // Arrange
            string[] typed = { "i", "in", "inv", "invo", "invoi", "invoic", "invoice", "invoices" };
            int keystroke = 0;
            var capturedQueries = new List<string>();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.SearchText).Returns(() => typed[keystroke]);
            HarnessController controller = BuildController(viewer, MatchedFolders, capturedQueries);

            // Act — one TextChanged per typed character.
            for (; keystroke < typed.Length; keystroke++)
            {
                controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);
            }

            // Assert
            capturedQueries
                .Should()
                .Equal(
                    new[]
                    {
                        "*i*",
                        "*in*",
                        "*inv*",
                        "*invo*",
                        "*invoi*",
                        "*invoic*",
                        "*invoice*",
                        "*invoices*",
                    },
                    "every keystroke queries the complete search text, wildcard-wrapped"
                );
            viewer.Verify(
                v => v.PresentFolderSearchResults(It.IsAny<string[]>()),
                Times.Exactly(typed.Length)
            );
            viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Never());
        }

        /// <summary>Reads the controller's private cached folder field.</summary>
        private static string CachedFolder(HarnessController controller) =>
            (string)QfcItemControllerTestSupport.GetField(controller, "_selectedFolder");

        /// <summary>
        /// Builds the headless controller/viewer pair, raises one search keystroke, and returns the
        /// viewer mock for assertion.
        /// </summary>
        private static Mock<IItemViewer> InvokeSearch(string[] folders)
        {
            Mock<IItemViewer> viewer = BuildViewer();
            HarnessController controller = BuildController(viewer, folders);
            controller.TextBoxSearch_TextChanged(null, EventArgs.Empty);
            return viewer;
        }

        /// <summary>A viewer mock that reports the search text and records every folder intent.</summary>
        private static Mock<IItemViewer> BuildViewer()
        {
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.SearchText).Returns(SearchQuery);
            return viewer;
        }

        /// <summary>
        /// Injects the viewer and a stubbed <see cref="IFolderSearchHandler"/> returning
        /// <paramref name="folders"/> into a bare controller harness by reflection.
        /// </summary>
        private static HarnessController BuildController(
            Mock<IItemViewer> viewer,
            string[] folders,
            List<string> capturedQueries = null
        )
        {
            Mock<IFolderSearchHandler> folderHandler = new Mock<IFolderSearchHandler>();
            folderHandler
                .Setup(f =>
                    f.FindFolder(
                        It.IsAny<string>(),
                        It.IsAny<object>(),
                        It.IsAny<bool>(),
                        It.IsAny<List<string>>(),
                        It.IsAny<bool>(),
                        It.IsAny<
                            IEnumerable<(string root, string excludedFolder, bool excludeChildren)>
                        >()
                    )
                )
                .Callback(
                    (
                        string searchString,
                        object objItem,
                        bool reload,
                        List<string> roots,
                        bool recalc,
                        IEnumerable<(
                            string root,
                            string excludedFolder,
                            bool excludeChildren
                        )> exclusions
                    ) => capturedQueries?.Add(searchString)
                )
                .Returns(folders);

            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_folderHandler",
                folderHandler.Object
            );
            return controller;
        }
    }
}
