using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for <see cref="QfcHighConfidencePreFilter.FilterAsync"/>. The scoring step is
    /// abstracted behind <see cref="IFolderScoringService"/> and replaced with a Moq scripted mock,
    /// so these tests exercise the filter logic (cutoff, inclusive boundary, zero-score exclusion,
    /// order preservation, carried predetermined folder, edge cases, cancellation) without any live
    /// Outlook COM or temporary files.
    /// </summary>
    [TestClass]
    public class QfcHighConfidencePreFilterTests
    {
        private Mock<IApplicationGlobals> _globals;

        [TestInitialize]
        public void Setup()
        {
            _globals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
        }

        private static string ReadControllerSource(string fileName)
        {
            string path = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\..\QuickFiler\Controllers",
                    fileName
                )
            );
            return File.ReadAllText(path);
        }

        [TestMethod]
        public void FilterAsync_ProbabilityDebugLog_IncludesCallerSubjectEntryIdScoreAndTopFolder()
        {
            string source = ReadControllerSource("QfcHighConfidencePreFilter.cs");

            source.Should().Contain("Probability debug [QfcHighConfidencePreFilter.FilterAsync]");
            source.Should().Contain("Subject='{item.Subject}'");
            source.Should().Contain("EntryID='{item.EntryID}'");
            source.Should().Contain("Score={score}");
            source.Should().Contain("TopFolder='{topFolder}'");
        }

        /// <summary>
        /// Builds a loose MailItem mock. The filter never reads MailItem members directly (scoring
        /// is mocked), so a bare mock object suffices as a reference-identity key.
        /// </summary>
        private static MailItem NewMailItem() => new Mock<MailItem>(MockBehavior.Loose).Object;

        /// <summary>
        /// Builds a scripted <see cref="IFolderScoringService"/> mock returning the supplied
        /// (score, topFolder) per MailItem reference. Items not in the script return (0, "").
        /// </summary>
        private static Mock<IFolderScoringService> BuildScoringMock(
            IDictionary<MailItem, (long score, string topFolder)> script
        )
        {
            var mock = new Mock<IFolderScoringService>(MockBehavior.Strict);
            mock.Setup(s =>
                    s.ScoreAsync(
                        It.IsAny<MailItem>(),
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(
                    (MailItem item, IApplicationGlobals g, CancellationToken t) =>
                    {
                        t.ThrowIfCancellationRequested();
                        // Issue #678 widened the seam's third element to the initialised handler.
                        // This scripted double publishes none, which the carrier tolerates.
                        if (script.TryGetValue(item, out var entry))
                        {
                            return Task.FromResult(
                                (entry.score, entry.topFolder, (IFolderSearchHandler)null)
                            );
                        }
                        return Task.FromResult((0L, string.Empty, (IFolderSearchHandler)null));
                    }
                );
            return mock;
        }

        // ---------------------------------------------------------------------
        // [P2-T2] Trivial setup test: filter returns survivors with a scripted mock.
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_WithSingleAboveThresholdItem_ReturnsThatItem()
        {
            // Arrange
            var item = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)>
            {
                [item] = (950, @"\\Archive\Projects"),
            };
            var scoring = BuildScoringMock(script);

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem> { item },
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Should().HaveCount(1, "the single item scores above the 900 cutoff");
            result[0].MailItem.Should().BeSameAs(item);
            result[0].PredeterminedFolder.Should().Be(@"\\Archive\Projects");
        }

        // ---------------------------------------------------------------------
        // [P2-T3] Below-cutoff items are excluded; only >= cutoff survive. (AC2)
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_ExcludesItemsBelowCutoff()
        {
            // Arrange — threshold 0.90 => cutoff 900. Mixed batch.
            var above1 = NewMailItem();
            var below1 = NewMailItem();
            var above2 = NewMailItem();
            var below2 = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)>
            {
                [above1] = (901, @"\\A\1"),
                [below1] = (899, @"\\A\2"),
                [above2] = (1000, @"\\A\3"),
                [below2] = (500, @"\\A\4"),
            };
            var scoring = BuildScoringMock(script);

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem> { above1, below1, above2, below2 },
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Select(r => r.MailItem).Should().Equal(above1, above2);
            result
                .Select(r => r.MailItem)
                .Should()
                .NotContain(new[] { below1, below2 }, "below-cutoff items must be excluded");
        }

        // ---------------------------------------------------------------------
        // [P2-T4] Zero-score / no-suggestion items are excluded. (AC3)
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_ExcludesZeroScoreNoSuggestion()
        {
            // Arrange
            var withSuggestion = NewMailItem();
            var noSuggestion = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)>
            {
                [withSuggestion] = (950, @"\\A\keep"),
                [noSuggestion] = (0, string.Empty),
            };
            var scoring = BuildScoringMock(script);

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem> { withSuggestion, noSuggestion },
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Should().HaveCount(1);
            result[0].MailItem.Should().BeSameAs(withSuggestion);
            result
                .Select(r => r.MailItem)
                .Should()
                .NotContain(noSuggestion, "a zero-score no-suggestion item must be excluded");
        }

        // ---------------------------------------------------------------------
        // [P2-T5] Inclusive boundary: an item scoring exactly at the cutoff is retained. (AC5)
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_RetainsItemExactlyAtCutoff()
        {
            // Arrange — threshold 0.90 => cutoff 900; item scores exactly 900.
            var atCutoff = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)>
            {
                [atCutoff] = (900, @"\\A\boundary"),
            };
            var scoring = BuildScoringMock(script);

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem> { atCutoff },
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Should().HaveCount(1, "score == cutoff is inclusive and must be retained");
            result[0].MailItem.Should().BeSameAs(atCutoff);
        }

        // ---------------------------------------------------------------------
        // [P2-T6] Survivors carry their scripted predetermined top folder. (AC4)
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_SurvivorsCarryPredeterminedTopFolder()
        {
            // Arrange
            var a = NewMailItem();
            var b = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)>
            {
                [a] = (920, @"\\A\folderA"),
                [b] = (980, @"\\A\folderB"),
            };
            var scoring = BuildScoringMock(script);

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem> { a, b },
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Single(r => r.MailItem == a).PredeterminedFolder.Should().Be(@"\\A\folderA");
            result.Single(r => r.MailItem == b).PredeterminedFolder.Should().Be(@"\\A\folderB");
        }

        // ---------------------------------------------------------------------
        // [P2-T7] Edge cases: null items, empty items, all-below-threshold.
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_NullItems_ReturnsEmpty()
        {
            // Arrange
            var scoring = BuildScoringMock(new Dictionary<MailItem, (long, string)>());

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                null,
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public async Task FilterAsync_EmptyItems_ReturnsEmpty()
        {
            // Arrange
            var scoring = BuildScoringMock(new Dictionary<MailItem, (long, string)>());

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem>(),
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public async Task FilterAsync_AllBelowThreshold_ReturnsEmpty()
        {
            // Arrange
            var a = NewMailItem();
            var b = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)>
            {
                [a] = (100, @"\\A\1"),
                [b] = (0, string.Empty),
            };
            var scoring = BuildScoringMock(script);

            // Act
            var result = await QfcHighConfidencePreFilter.FilterAsync(
                new List<MailItem> { a, b },
                _globals.Object,
                0.90,
                CancellationToken.None,
                scoring.Object
            );

            // Assert
            result.Should().BeEmpty("no item meets the cutoff");
        }

        // ---------------------------------------------------------------------
        // [P2-T8] Cancellation contract: a pre-cancelled token throws before scoring.
        // ---------------------------------------------------------------------
        [TestMethod]
        public async Task FilterAsync_HonorsCancellation()
        {
            // Arrange
            var item = NewMailItem();
            var script = new Dictionary<MailItem, (long, string)> { [item] = (950, @"\\A\keep") };
            var scoring = BuildScoringMock(script);
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                // Act
                Func<Task> act = () =>
                    QfcHighConfidencePreFilter.FilterAsync(
                        new List<MailItem> { item },
                        _globals.Object,
                        0.90,
                        cts.Token,
                        scoring.Object
                    );

                // Assert — a token cancelled before the call throws OperationCanceledException
                // before any scoring occurs.
                await act.Should().ThrowAsync<OperationCanceledException>();
                scoring.Verify(
                    s =>
                        s.ScoreAsync(
                            It.IsAny<MailItem>(),
                            It.IsAny<IApplicationGlobals>(),
                            It.IsAny<CancellationToken>()
                        ),
                    Times.Never(),
                    "scoring must not run when the token is already cancelled"
                );
            }
        }
    }
}
