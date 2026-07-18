#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Builds <see cref="BreadcrumbRow"/> instances from presented suggestion rows plus 9101
    /// ancestor chains (#349). Pure and host-neutral; consumes the 9101
    /// <see cref="FolderBreadcrumbSegment"/> type directly (P0-T6 record) and derives NO hierarchy
    /// from suggestion-row prefix matching — every chain comes from the injected lookup.
    /// </summary>
    public sealed class BreadcrumbRowBuilder
    {
        /// <summary>The exact presented text of the trash pseudo-row.</summary>
        public const string TrashRowText = "Trash to Delete";

        /// <summary>Prefix identifying non-interactive section banner rows.</summary>
        public const string BannerPrefix = "====";

        private static readonly char[] PathSeparators = { '\\', '/' };

        /// <summary>
        /// Builds breadcrumb rows for the presented rows, preserving presented order. Row ids are
        /// <c>row-&lt;index&gt;</c> over the presented sequence.
        /// </summary>
        /// <param name="presentedRows">Presented row texts in display order.</param>
        /// <param name="ancestorChainLookup">
        /// Root-to-leaf 9101 ancestor chain per suggestion folder path; may return null or an
        /// empty list when the path is unknown.
        /// </param>
        /// <param name="scores">Score projections joined to rows by full-path equality.</param>
        /// <returns>The breadcrumb rows in presented order.</returns>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public IReadOnlyList<BreadcrumbRow> BuildRows(
            IReadOnlyList<string> presentedRows,
            Func<string, IReadOnlyList<FolderBreadcrumbSegment>?> ancestorChainLookup,
            IEnumerable<FolderScore> scores
        )
        {
            if (presentedRows == null)
            {
                throw new ArgumentNullException(nameof(presentedRows));
            }

            if (ancestorChainLookup == null)
            {
                throw new ArgumentNullException(nameof(ancestorChainLookup));
            }

            var probabilityByPath = BuildProbabilityIndex(scores);
            var rows = new List<BreadcrumbRow>(presentedRows.Count);
            for (int i = 0; i < presentedRows.Count; i++)
            {
                string text = presentedRows[i] ?? string.Empty;
                rows.Add(BuildRow($"row-{i}", text, ancestorChainLookup(text), probabilityByPath));
            }

            return rows;
        }

        /// <summary>
        /// Builds a single breadcrumb row from one presented row text.
        /// </summary>
        /// <param name="rowId">Stable row identifier.</param>
        /// <param name="presentedText">The exact presented row text (path, banner, or trash row).</param>
        /// <param name="ancestorChain">
        /// The 9101 root-to-leaf chain for a suggestion row, or null/empty when unknown.
        /// </param>
        /// <param name="probabilityByPath">Probability values keyed by folder full path.</param>
        /// <returns>The constructed row.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="rowId"/>, <paramref name="presentedText"/>, or
        /// <paramref name="probabilityByPath"/> is null.
        /// </exception>
        public BreadcrumbRow BuildRow(
            string rowId,
            string presentedText,
            IReadOnlyList<FolderBreadcrumbSegment>? ancestorChain,
            IReadOnlyDictionary<string, double> probabilityByPath
        )
        {
            if (rowId == null)
            {
                throw new ArgumentNullException(nameof(rowId));
            }

            if (presentedText == null)
            {
                throw new ArgumentNullException(nameof(presentedText));
            }

            if (probabilityByPath == null)
            {
                throw new ArgumentNullException(nameof(probabilityByPath));
            }

            BreadcrumbRowKind kind = Classify(presentedText);
            switch (kind)
            {
                case BreadcrumbRowKind.Banner:
                    // Banner text travels as a single inert segment (banner rows never
                    // collapse/expand, so the segment is display data only).
                    return new BreadcrumbRow(
                        rowId,
                        BreadcrumbRowKind.Banner,
                        new[] { new BreadcrumbSegment(presentedText, presentedText, false) },
                        null
                    );

                case BreadcrumbRowKind.TrashPseudoRow:
                    return new BreadcrumbRow(
                        rowId,
                        BreadcrumbRowKind.TrashPseudoRow,
                        Array.Empty<BreadcrumbSegment>(),
                        null
                    );

                default:
                    IReadOnlyList<BreadcrumbSegment> segments = MapSegments(ancestorChain);
                    if (segments.Count == 0)
                    {
                        // Unknown/empty chain fallback: render the presented path as a single
                        // leaf-only segment so the suggestion stays visible and selectable.
                        segments = new[]
                        {
                            new BreadcrumbSegment(presentedText, LeafToken(presentedText), false),
                        };
                    }

                    string joinPath = segments[segments.Count - 1].FullPath;
                    double? probability = probabilityByPath.TryGetValue(joinPath, out double p)
                        ? p
                        : (double?)null;
                    return new BreadcrumbRow(
                        rowId,
                        BreadcrumbRowKind.Suggestion,
                        segments,
                        probability
                    );
            }
        }

        /// <summary>
        /// Classifies a presented row text: <c>"===="</c>-prefixed rows are banners, the exact
        /// <see cref="TrashRowText"/> is the trash pseudo-row, everything else is a suggestion.
        /// </summary>
        /// <param name="presentedText">The exact presented row text.</param>
        /// <returns>The row kind.</returns>
        public static BreadcrumbRowKind Classify(string presentedText)
        {
            if (presentedText == null)
            {
                throw new ArgumentNullException(nameof(presentedText));
            }

            if (presentedText.StartsWith(BannerPrefix, StringComparison.Ordinal))
            {
                return BreadcrumbRowKind.Banner;
            }

            if (string.Equals(presentedText, TrashRowText, StringComparison.Ordinal))
            {
                return BreadcrumbRowKind.TrashPseudoRow;
            }

            return BreadcrumbRowKind.Suggestion;
        }

        /// <summary>
        /// Maps an ordered root-to-leaf 9101 chain to pure breadcrumb segments
        /// (<c>FolderPath</c> to <c>FullPath</c>, <c>HasChildren</c> to <c>HasSubfolders</c>).
        /// </summary>
        /// <param name="chain">The 9101 segments, or null.</param>
        /// <returns>The mapped segments in the same order; empty when the chain is null/empty.</returns>
        public static IReadOnlyList<BreadcrumbSegment> MapSegments(
            IReadOnlyList<FolderBreadcrumbSegment>? chain
        )
        {
            if (chain == null || chain.Count == 0)
            {
                return Array.Empty<BreadcrumbSegment>();
            }

            var mapped = new List<BreadcrumbSegment>(chain.Count);
            foreach (FolderBreadcrumbSegment segment in chain)
            {
                if (segment == null)
                {
                    throw new ArgumentException(
                        "Ancestor chains must not contain null segments.",
                        nameof(chain)
                    );
                }

                mapped.Add(
                    new BreadcrumbSegment(
                        segment.FolderPath,
                        segment.DisplayName,
                        segment.HasChildren
                    )
                );
            }

            return mapped;
        }

        private static IReadOnlyDictionary<string, double> BuildProbabilityIndex(
            IEnumerable<FolderScore> scores
        )
        {
            if (scores == null)
            {
                throw new ArgumentNullException(nameof(scores));
            }

            var index = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (FolderScore score in scores)
            {
                if (!string.IsNullOrEmpty(score.FolderPath))
                {
                    index[score.FolderPath] = score.Probability;
                }
            }

            return index;
        }

        private static string LeafToken(string path)
        {
            string trimmed = path.TrimEnd(PathSeparators);
            int last = trimmed.LastIndexOfAny(PathSeparators);
            return last >= 0 ? trimmed.Substring(last + 1) : trimmed;
        }
    }
}
