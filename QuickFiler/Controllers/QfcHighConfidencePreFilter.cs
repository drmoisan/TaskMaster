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
                        var (score, topFolder) = await service.ScoreAsync(item, globals, token);
                        return (index, item, score, topFolder);
                    }
                )
                .ToList();

            var scored = await Task.WhenAll(scoringTasks);

            return scored
                .Where(result => result.score >= cutoff && result.score > 0)
                .OrderBy(result => result.index)
                .Select(result => new QfcPreScoredItem(result.item, result.topFolder))
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
        public QfcPreScoredItem(MailItem mailItem, string predeterminedFolder)
        {
            MailItem = mailItem;
            PredeterminedFolder = predeterminedFolder ?? string.Empty;
        }

        /// <summary>The surviving mail item. Never null for a produced survivor.</summary>
        public MailItem MailItem { get; }

        /// <summary>
        /// The top-suggestion folder path for the item. Non-null; empty only if no folder path was
        /// available (such an item is not produced as a survivor by the filter).
        /// </summary>
        public string PredeterminedFolder { get; }
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
        /// Scores a single mail item and returns its top folder score (0-1000 scale) and the
        /// top-ranked suggested folder path.
        /// </summary>
        /// <param name="mailItem">The mail item to score.</param>
        /// <param name="globals">Application globals providing the trained classifier.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>
        /// A tuple of the top score (max value in the folder scorer, 0 when no suggestion) and the
        /// top-ranked folder path (empty string when no suggestion).
        /// </returns>
        Task<(long Score, string TopFolder)> ScoreAsync(
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
        public async Task<(long Score, string TopFolder)> ScoreAsync(
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
            return (score, topFolder);
        }
    }
}
