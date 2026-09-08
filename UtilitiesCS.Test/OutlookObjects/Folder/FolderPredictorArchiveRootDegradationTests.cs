using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for AC1 through AC3 of issue #812: the two display surfaces
    /// <see cref="UtilitiesCS.FolderPredictor.FolderArray"/> and
    /// <see cref="UtilitiesCS.FolderPredictor.FolderRowArray"/> must degrade gracefully when the
    /// archive root cannot be resolved. Today both surfaces read
    /// <c>IOlObjects.ArchiveRootPath</c> purely to compute a cosmetic display stem, so a store
    /// that cannot resolve its archive root turns the whole folder list into an exception rather
    /// than into an unprojected list.
    /// <para>
    /// The archive-root read is arranged to THROW rather than to return a value, which is the
    /// production failure mode. Two overloads are supplied: an
    /// <see cref="InvalidOperationException"/> overload for the degradation cases, and a
    /// <see cref="COMException"/> overload that pins the non-degradation boundary - only the
    /// documented exception type is absorbed and every other failure still propagates.
    /// </para>
    /// <para>
    /// The harness is a pure Moq fixture. No live Outlook process, no COM server, no filesystem
    /// access, and no temporary file is used, and nothing here depends on wall-clock time.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class FolderPredictorArchiveRootDegradationTests
    {
        private const string SuggestionsSeparator = "========= SUGGESTIONS =========";
        private const string RecentsSeparator = "======= RECENT SELECTIONS ========";

        private const string ThrowingRootReason = "archive root unresolvable";
        private const string ComFailureReason = "com failure";

        // Every seeded path is archive-rooted, so a resolvable root WOULD project each of them to
        // a shorter stem. That makes the unprojected pass-through observable: when the root read
        // fails, each entry must come back byte-identical to the string that was stored.
        private static readonly string[] FiveSuggestions = new[]
        {
            "\\\\ArchiveRoot\\Suggested\\One",
            "\\\\ArchiveRoot\\Suggested\\Two",
            "\\\\ArchiveRoot\\Suggested\\Three",
            "\\\\ArchiveRoot\\Suggested\\Four",
            "\\\\ArchiveRoot\\Suggested\\Five",
        };

        private static readonly string[] ThreeRecents = new[]
        {
            "\\\\ArchiveRoot\\Recent\\One",
            "\\\\ArchiveRoot\\Recent\\Two",
            "\\\\ArchiveRoot\\Recent\\Three",
        };

        private static readonly string[] NoEntries = Array.Empty<string>();

        /// <summary>
        /// AC1, legacy string surface, recents only: an unresolvable archive root must not turn
        /// the recent selections into an exception. Each entry comes back exactly as stored.
        /// </summary>
        [TestMethod]
        public void FolderArray_RecentsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(NoEntries, ThreeRecents);
            string[] folderArray = null;

            // Act
            Action act = () => folderArray = predictor.FolderArray;

            // Assert
            act.Should().NotThrow("an unresolvable archive root is a display concern only");
            folderArray.Should().Equal(ExpectedUnprojectedSequence(NoEntries, ThreeRecents));
        }

        /// <summary>
        /// AC1, legacy string surface, suggestions only: the scored suggestions render
        /// unprojected rather than failing.
        /// </summary>
        [TestMethod]
        public void FolderArray_SuggestionsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(FiveSuggestions, NoEntries);
            string[] folderArray = null;

            // Act
            Action act = () => folderArray = predictor.FolderArray;

            // Assert
            act.Should().NotThrow("an unresolvable archive root is a display concern only");
            folderArray.Should().Equal(ExpectedUnprojectedSequence(FiveSuggestions, NoEntries));
        }

        /// <summary>
        /// AC1, legacy string surface, both populated: the combined list renders in full, with
        /// both separators and every entry unprojected.
        /// </summary>
        [TestMethod]
        public void FolderArray_SuggestionsAndRecentsWithThrowingArchiveRoot_ReturnsEntriesUnchanged()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(FiveSuggestions, ThreeRecents);
            string[] folderArray = null;

            // Act
            Action act = () => folderArray = predictor.FolderArray;

            // Assert
            act.Should().NotThrow("an unresolvable archive root is a display concern only");
            folderArray.Should().Equal(ExpectedUnprojectedSequence(FiveSuggestions, ThreeRecents));
        }

        /// <summary>
        /// AC1, row-model surface, recents only. The row model is an independent code path from
        /// the string surface and reads the archive root separately, so it is pinned separately.
        /// </summary>
        [TestMethod]
        public void FolderRowArray_RecentsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(NoEntries, ThreeRecents);
            UtilitiesCS.FolderRow[] rows = null;

            // Act
            Action act = () => rows = predictor.FolderRowArray;

            // Assert
            act.Should().NotThrow("an unresolvable archive root is a display concern only");
            rows.Select(r => r.Text)
                .Should()
                .Equal(ExpectedUnprojectedSequence(NoEntries, ThreeRecents));
        }

        /// <summary>AC1, row-model surface, suggestions only.</summary>
        [TestMethod]
        public void FolderRowArray_SuggestionsOnlyWithThrowingArchiveRoot_ReturnsEntriesUnchanged()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(FiveSuggestions, NoEntries);
            UtilitiesCS.FolderRow[] rows = null;

            // Act
            Action act = () => rows = predictor.FolderRowArray;

            // Assert
            act.Should().NotThrow("an unresolvable archive root is a display concern only");
            rows.Select(r => r.Text)
                .Should()
                .Equal(ExpectedUnprojectedSequence(FiveSuggestions, NoEntries));
        }

        /// <summary>AC1, row-model surface, both populated.</summary>
        [TestMethod]
        public void FolderRowArray_SuggestionsAndRecentsWithThrowingArchiveRoot_ReturnsEntriesUnchanged()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(FiveSuggestions, ThreeRecents);
            UtilitiesCS.FolderRow[] rows = null;

            // Act
            Action act = () => rows = predictor.FolderRowArray;

            // Assert
            act.Should().NotThrow("an unresolvable archive root is a display concern only");
            rows.Select(r => r.Text)
                .Should()
                .Equal(ExpectedUnprojectedSequence(FiveSuggestions, ThreeRecents));
        }

        /// <summary>
        /// The text-parity contract documented on <c>FolderRowArray</c> must hold in the degraded
        /// case too. Degrading one surface without the other would break parity silently, so it is
        /// pinned as its own criterion rather than as a side effect of the six tests above.
        /// </summary>
        [TestMethod]
        public void FolderArrayAndFolderRowArray_WithThrowingArchiveRoot_ProduceIdenticalText()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(FiveSuggestions, ThreeRecents);

            // Act
            var folderArray = predictor.FolderArray;
            var rows = predictor.FolderRowArray;

            // Assert
            rows.Select(r => r.Text)
                .Should()
                .Equal(folderArray, "the row model mirrors FolderArray byte for byte");
        }

        /// <summary>
        /// The read must be hoisted above the projection loop, not taken per entry. With five
        /// suggestions and three recents a single <c>FolderArray</c> access reads the archive root
        /// exactly twice - once for the suggestion block and once for the recents block - so a
        /// per-entry read would log up to five warnings for one projection.
        /// </summary>
        [TestMethod]
        public void FolderArray_WithThrowingArchiveRootAndBothPopulated_ReadsArchiveRootPathExactlyTwice()
        {
            // Arrange
            var (predictor, olObjects) = PredictorWithThrowingRoot(FiveSuggestions, ThreeRecents);

            // Act
            var folderArray = predictor.FolderArray;

            // Assert
            folderArray.Should().NotBeEmpty();
            olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Exactly(2));
        }

        /// <summary>
        /// The row-model surface carries the same hoisting obligation as the string surface.
        /// </summary>
        [TestMethod]
        public void FolderRowArray_WithThrowingArchiveRootAndBothPopulated_ReadsArchiveRootPathExactlyTwice()
        {
            // Arrange
            var (predictor, olObjects) = PredictorWithThrowingRoot(FiveSuggestions, ThreeRecents);

            // Act
            var rows = predictor.FolderRowArray;

            // Assert
            rows.Should().NotBeEmpty();
            olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Exactly(2));
        }

        /// <summary>
        /// With no suggestions held, only the recents block reads the archive root, so the count
        /// is one rather than two. This pins that the suggestion block is genuinely skipped rather
        /// than reading and discarding.
        /// </summary>
        [TestMethod]
        public void FolderArray_WithThrowingArchiveRootAndRecentsOnly_ReadsArchiveRootPathExactlyOnce()
        {
            // Arrange
            var (predictor, olObjects) = PredictorWithThrowingRoot(NoEntries, ThreeRecents);

            // Act
            var folderArray = predictor.FolderArray;

            // Assert
            folderArray.Should().NotBeEmpty();
            olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Once());
        }

        /// <summary>
        /// AC2 boundary: only <see cref="InvalidOperationException"/> is absorbed. A COM failure
        /// reaching the display read is not a resolvable-root problem and must still propagate,
        /// so a bare catch would be a defect rather than a stronger guarantee.
        /// </summary>
        [TestMethod]
        public void FolderArray_WhenArchiveRootPathThrowsComException_PropagatesComException()
        {
            // Arrange
            var (predictor, _) = PredictorWithComThrowingRoot(FiveSuggestions, ThreeRecents);

            // Act
            Action act = () =>
            {
                var unused = predictor.FolderArray;
            };

            // Assert
            act.Should().Throw<COMException>("only InvalidOperationException is absorbed");
        }

        /// <summary>
        /// AC3 boundary: the FUNCTIONAL archive-root reads are not degraded. When
        /// <c>FindFolder</c> is called without explicit search roots it must still fall back to the
        /// archive root, and an unresolvable root there is a genuine failure rather than a cosmetic
        /// one, so the exception still surfaces.
        /// </summary>
        [TestMethod]
        public void FindFolder_WithNullEmailSearchRootsAndThrowingArchiveRoot_StillThrowsInvalidOperationException()
        {
            // Arrange
            var (predictor, _) = PredictorWithThrowingRoot(FiveSuggestions, ThreeRecents);

            // Act
            Action act = () => predictor.FindFolder("x", new object());

            // Assert
            act.Should()
                .Throw<InvalidOperationException>(
                    "the functional search-root fallback is not a display projection"
                );
        }

        // ---- Mocked-Outlook harness (derived from FolderPredictorRecentsProjectionTests) ----

        /// <summary>
        /// Builds a predictor whose <c>ArchiveRootPath</c> getter throws
        /// <see cref="InvalidOperationException"/>, together with the mocked
        /// <c>IOlObjects</c> so a test body can count reads with <c>VerifyGet</c>.
        /// </summary>
        private static (
            UtilitiesCS.FolderPredictor Predictor,
            Mock<IOlObjects> OlObjects
        ) PredictorWithThrowingRoot(IEnumerable<string> suggestions, IEnumerable<string> recents) =>
            CreatePredictor(
                new InvalidOperationException(ThrowingRootReason),
                suggestions,
                recents
            );

        /// <summary>
        /// The <see cref="COMException"/> overload of <see cref="PredictorWithThrowingRoot"/>,
        /// used to pin that a COM failure is NOT absorbed by the guarded accessor.
        /// </summary>
        private static (
            UtilitiesCS.FolderPredictor Predictor,
            Mock<IOlObjects> OlObjects
        ) PredictorWithComThrowingRoot(
            IEnumerable<string> suggestions,
            IEnumerable<string> recents
        ) => CreatePredictor(new COMException(ComFailureReason), suggestions, recents);

        private static (
            UtilitiesCS.FolderPredictor Predictor,
            Mock<IOlObjects> OlObjects
        ) CreatePredictor(
            Exception archiveRootFailure,
            IEnumerable<string> suggestions,
            IEnumerable<string> recents
        )
        {
            var autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(x => x.RecentsList).Returns(new SloLinkedList<string>(recents));

            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.App).Returns(new Mock<Outlook.Application>().Object);
            olObjects.SetupGet(x => x.ArchiveRootPath).Throws(archiveRootFailure);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.AF).Returns(autoFile.Object);
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);

            var predictor = new UtilitiesCS.FolderPredictor(globals.Object);
            SeedSuggestions(predictor, suggestions);
            return (predictor, olObjects);
        }

        /// <summary>
        /// Seeds the scorer with strictly descending scores so the ranked order the predictor
        /// emits is the seeded array order. Equal scores would fall back to an ordinal key
        /// tie-break and the expected sequences below would no longer read in seeded order.
        /// </summary>
        private static void SeedSuggestions(
            UtilitiesCS.FolderPredictor predictor,
            IEnumerable<string> suggestions
        )
        {
            var index = 0;
            foreach (var suggestion in suggestions)
            {
                predictor.Suggestions.AddSuggestion(suggestion, 1000 - (index * 100));
                index++;
            }
        }

        /// <summary>
        /// The sequence both surfaces must emit when the archive root cannot be read: each
        /// separator followed by its entries, every entry unprojected and unchanged.
        /// </summary>
        private static string[] ExpectedUnprojectedSequence(
            IEnumerable<string> suggestions,
            IEnumerable<string> recents
        )
        {
            var expected = new List<string>();
            var suggestionList = suggestions.ToList();
            var recentList = recents.ToList();
            if (suggestionList.Count > 0)
            {
                expected.Add(SuggestionsSeparator);
                expected.AddRange(suggestionList);
            }
            if (recentList.Count > 0)
            {
                expected.Add(RecentsSeparator);
                expected.AddRange(recentList);
            }
            return expected.ToArray();
        }
    }
}
