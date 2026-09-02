using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

// Allows Moq's dynamic proxy generator to mock the internal IFolderScoringService seam in tests.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Pre-UI scoring and filtering for QuickFiler high-confidence mode. Scores a candidate
    /// <see cref="MailItem"/> batch off the UI thread and returns only the items whose top suggested
    /// folder score meets or exceeds the configured cutoff, each paired with its predetermined top
    /// folder path. The new logic lives here (not in the oversized controllers) and reuses the
    /// existing <see cref="FolderPredictor"/> / <see cref="FolderScorer"/> scoring path through
    /// <see cref="IFolderScoringService"/>.
    /// </summary>
    internal static class QfcHighConfidencePreFilter
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// Scores all items in <paramref name="items"/> in parallel and returns the survivors whose
        /// top folder score is at or above the cutoff derived from <paramref name="threshold"/> and
        /// strictly greater than zero. Survivors preserve input order and carry their predetermined
        /// top folder path. The cutoff is <c>(long)Math.Round(threshold * 1000, 0)</c>; the
        /// comparison is inclusive of the boundary (a score equal to the cutoff is retained).
        /// </summary>
        /// <param name="items">Candidate mail items. Null or empty yields an empty result.</param>
        /// <param name="globals">Application globals providing the trained classifier.</param>
        /// <param name="threshold">A probability in [0.0, 1.0]; default mode threshold is 0.90.</param>
        /// <param name="token">
        /// Cancellation token. A token already cancelled when this method is called causes an
        /// <see cref="OperationCanceledException"/> before any scoring occurs.
        /// </param>
        /// <param name="scoringService">
        /// Scoring seam; defaults to <see cref="FolderScoringService"/> when null. Injected by tests.
        /// </param>
        /// <returns>The surviving items in input order, each with its predetermined folder path.</returns>
        public static async Task<IList<QfcPreScoredItem>> FilterAsync(
            IList<MailItem> items,
            IApplicationGlobals globals,
            double threshold,
            CancellationToken token,
            IFolderScoringService scoringService = null
        )
        {
            token.ThrowIfCancellationRequested();

            if (items is null || items.Count == 0)
            {
                return new List<QfcPreScoredItem>();
            }

            long cutoff = (long)Math.Round(threshold * 1000, 0);
            var service = scoringService ?? new FolderScoringService();

            // Score every item in parallel, preserving index so survivors keep input order.
            var scoringTasks = items
                .Select(
                    async (item, index) =>
                    {
                        var (score, topFolder, handler) = await service.ScoreAsync(
                            item,
                            globals,
                            token
                        );
                        logger.Debug(
                            $"Probability debug [QfcHighConfidencePreFilter.FilterAsync] "
                                + $"Subject='{item.Subject}' EntryID='{item.EntryID}' "
                                + $"Score={score} TopFolder='{topFolder}'"
                        );
                        return (index, item, score, topFolder, handler);
                    }
                )
                .ToList();

            var scored = await Task.WhenAll(scoringTasks);

            return scored
                .Where(result => result.score >= cutoff && result.score > 0)
                .OrderBy(result => result.index)
                .Select(result => new QfcPreScoredItem(
                    result.item,
                    result.topFolder,
                    result.handler
                ))
                .ToList();
        }
    }

    /// <summary>
    /// Pairs a surviving <see cref="MailItem"/> with the predetermined top-suggestion folder path
    /// that the high-confidence pre-filter resolved for it. Instances are produced by
    /// <see cref="QfcHighConfidencePreFilter.FilterAsync"/> for items whose top folder score meets
    /// or exceeds the configured cutoff, so the UI item controller can preselect the predetermined
    /// folder instead of selecting by index.
    /// </summary>
    public readonly struct QfcPreScoredItem
    {
        /// <summary>
        /// Creates a carrier pairing a surviving mail item with its predetermined folder path.
        /// </summary>
        /// <param name="mailItem">The surviving mail item. Never null for a produced survivor.</param>
        /// <param name="predeterminedFolder">
        /// The top-suggestion folder path for the item. Coerced to <see cref="string.Empty"/> when
        /// null so the property contract (non-null) holds.
        /// </param>
        /// <param name="folderHandler">
        /// Issue #678. The folder search handler the scorer already initialised for this item, so
        /// the item controller can adopt it instead of running a second
        /// <c>FolderPredictor.InitAsync(FromField)</c> pass. Optional and nullable: a carrier built
        /// on a path where no handler is available (a test double, or a scorer that produced none)
        /// leaves it null, and the item controller then falls back to its existing behaviour.
        /// </param>
        public QfcPreScoredItem(
            MailItem mailItem,
            string predeterminedFolder,
            IFolderSearchHandler folderHandler = null
        )
        {
            MailItem = mailItem;
            PredeterminedFolder = predeterminedFolder ?? string.Empty;
            FolderHandler = folderHandler;
        }

        /// <summary>The surviving mail item. Never null for a produced survivor.</summary>
        public MailItem MailItem { get; }

        /// <summary>
        /// The top-suggestion folder path for the item. Non-null; empty only if no folder path was
        /// available (such an item is not produced as a survivor by the filter).
        /// </summary>
        public string PredeterminedFolder { get; }

        /// <summary>
        /// Issue #678. The already-initialised folder search handler the scorer produced for this
        /// item, or <see langword="null"/> when none is available. Unlike the two members above this
        /// one has no non-null contract, because the carrier is also constructed on paths that have
        /// no handler to publish.
        /// </summary>
        public IFolderSearchHandler FolderHandler { get; }
    }

    /// <summary>
    /// Scoring seam for the high-confidence pre-filter. Abstracts the per-item Bayesian folder
    /// scoring so the pre-filter is unit-testable without live Outlook COM. The default
    /// implementation (<see cref="FolderScoringService"/>) reuses the existing
    /// <see cref="FolderPredictor"/> / <see cref="FolderScorer"/> path.
    /// </summary>
    internal interface IFolderScoringService
    {
        /// <summary>
        /// Scores a single mail item and returns its top folder score (0-1000 scale), the
        /// top-ranked suggested folder path, and the folder search handler the scoring pass
        /// initialised.
        /// </summary>
        /// <param name="mailItem">The mail item to score.</param>
        /// <param name="globals">Application globals providing the trained classifier.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>
        /// A tuple of the top score (max value in the folder scorer, 0 when no suggestion), the
        /// top-ranked folder path (empty string when no suggestion), and the initialised handler.
        /// Issue #678: the handler is published rather than discarded so the consumer can adopt it
        /// instead of running a second <c>FolderPredictor.InitAsync(FromField)</c> pass. It is
        /// <see langword="null"/> only for an implementation that produces no handler.
        /// </returns>
        Task<(long Score, string TopFolder, IFolderSearchHandler Handler)> ScoreAsync(
            MailItem mailItem,
            IApplicationGlobals globals,
            CancellationToken token
        );
    }

    /// <summary>
    /// Default <see cref="IFolderScoringService"/> implementation. Reuses the existing scoring
    /// sequence (<see cref="MailItemHelper"/> -> <see cref="FolderPredictor"/> with
    /// <see cref="FolderPredictor.InitOptions.FromField"/>) rather than duplicating the Bayesian
    /// classification body, so the pre-filter and the existing item-controller path share one
    /// scoring path.
    /// </summary>
    /// <remarks>
    /// This is the I/O-boundary adapter for the scoring seam: its body is COM-bound
    /// (<see cref="MailItemHelper.FromMailItemAsync"/> + live Outlook classification) and therefore
    /// cannot be exercised by a unit test without live Outlook COM, which repo policy prohibits.
    /// It is excluded from code coverage so the testable filter surface
    /// (<see cref="QfcHighConfidencePreFilter.FilterAsync"/>, <see cref="QfcPreScoredItem"/>,
    /// <see cref="IFolderScoringService"/>) is measured on its own. The adapter is verified via the
    /// existing item-controller scoring path it reuses and through the seam mock in unit tests.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal sealed class FolderScoringService : IFolderScoringService
    {
        /// <inheritdoc />
        public async Task<(long Score, string TopFolder, IFolderSearchHandler Handler)> ScoreAsync(
            MailItem mailItem,
            IApplicationGlobals globals,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var helper = await MailItemHelper.FromMailItemAsync(mailItem, globals, token, false);
            var predictor = new FolderPredictor(
                globals,
                helper,
                FolderPredictor.InitOptions.FromField
            );
            predictor = await predictor.InitAsync(helper, FolderPredictor.InitOptions.FromField);

            long score = predictor.Suggestions.TopScore();
            string topFolder = predictor.Suggestions.ToArray(1).FirstOrDefault() ?? string.Empty;

            // Issue #678: publish the predictor this pass already initialised instead of letting it
            // fall out of scope. Before this change only the two scalars escaped, so every consumer
            // that needed FolderArray, Suggestions or FolderRowArray had to build and initialise a
            // second predictor for the same item.
            return (score, topFolder, predictor);
        }
    }
}
