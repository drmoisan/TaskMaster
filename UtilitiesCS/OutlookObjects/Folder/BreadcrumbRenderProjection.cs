#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>The kind of one rendered breadcrumb cell.</summary>
    public enum BreadcrumbCellKind
    {
        /// <summary>A folder-name segment.</summary>
        Segment,

        /// <summary>The arrow separator between segments.</summary>
        Arrow,

        /// <summary>A plus affordance (re-expand a collapsed chain, or open the leaf expansion).</summary>
        Plus,

        /// <summary>A minus affordance (close the open leaf expansion).</summary>
        Minus,
    }

    /// <summary>One rendered cell of a breadcrumb row (segment, arrow, or affordance).</summary>
    public sealed class BreadcrumbCellRender
    {
        internal BreadcrumbCellRender(
            BreadcrumbCellKind kind,
            string text,
            int segmentIndex,
            bool truncationEligible
        )
        {
            Kind = kind;
            Text = text;
            SegmentIndex = segmentIndex;
            TruncationEligible = truncationEligible;
        }

        /// <summary>The cell kind.</summary>
        public BreadcrumbCellKind Kind { get; }

        /// <summary>The display text (segment name; empty for arrows/affordances).</summary>
        public string Text { get; }

        /// <summary>The chain index for segment cells; -1 for arrows and affordances.</summary>
        public int SegmentIndex { get; }

        /// <summary>True when the cell may ellipsize (interior segments of long chains, FR-1).</summary>
        public bool TruncationEligible { get; }
    }

    /// <summary>One rendered immediate subfolder in an open leaf expansion (FR-4).</summary>
    public sealed class BreadcrumbSubfolderRender
    {
        internal BreadcrumbSubfolderRender(string displayName, string folderPath, bool hasChildren)
        {
            DisplayName = displayName;
            FolderPath = folderPath;
            HasChildren = hasChildren;
        }

        /// <summary>The subfolder display name.</summary>
        public string DisplayName { get; }

        /// <summary>The subfolder full path (the selection value).</summary>
        public string FolderPath { get; }

        /// <summary>True when the subfolder itself has children.</summary>
        public bool HasChildren { get; }
    }

    /// <summary>One rendered breadcrumb row: ordered cells, percentage text, and subfolders.</summary>
    public sealed class BreadcrumbRowRender
    {
        internal BreadcrumbRowRender(
            int rowIndex,
            bool isSuggestion,
            bool selected,
            bool collapsed,
            bool leafExpanded,
            string percentText,
            IReadOnlyList<BreadcrumbCellRender> cells,
            IReadOnlyList<BreadcrumbSubfolderRender> subfolders
        )
        {
            RowIndex = rowIndex;
            IsSuggestion = isSuggestion;
            Selected = selected;
            Collapsed = collapsed;
            LeafExpanded = leafExpanded;
            PercentText = percentText;
            Cells = cells;
            Subfolders = subfolders;
        }

        /// <summary>The row's index in display order.</summary>
        public int RowIndex { get; }

        /// <summary>True for a Path A suggestion row; false for a Path B plain row.</summary>
        public bool IsSuggestion { get; }

        /// <summary>True when the row is the current selection.</summary>
        public bool Selected { get; }

        /// <summary>True when the chain is collapsed after a pivot segment (FR-3).</summary>
        public bool Collapsed { get; }

        /// <summary>True while the leaf subfolder expansion is open (FR-2).</summary>
        public bool LeafExpanded { get; }

        /// <summary>The formatted percentage; empty for probability-free rows (FR-5 cell content).</summary>
        public string PercentText { get; }

        /// <summary>The ordered cells (segments, arrows, affordances).</summary>
        public IReadOnlyList<BreadcrumbCellRender> Cells { get; }

        /// <summary>The rendered subfolders of an open leaf expansion; empty otherwise.</summary>
        public IReadOnlyList<BreadcrumbSubfolderRender> Subfolders { get; }
    }

    /// <summary>
    /// Pure projection from <see cref="BreadcrumbStateModel"/> state to the ordered render DTO list
    /// the breadcrumb page's JS consumes (#351 P3-T3). Percentage text comes from the existing
    /// <see cref="PercentageFormatter"/> (consumed read-only); Path B rows render as ancestor-split
    /// chains with an empty percentage cell. No I/O, WinForms, or WebView2 references.
    /// </summary>
    public static class BreadcrumbRenderProjection
    {
        private static readonly char[] PathSeparators = { '\\' };

        /// <summary>
        /// Projects the model's rows into render DTOs in display order.
        /// </summary>
        /// <param name="model">The state model to project. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is null.</exception>
        public static IReadOnlyList<BreadcrumbRowRender> Project(BreadcrumbStateModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var rows = new List<BreadcrumbRowRender>(model.Rows.Count);
            for (int i = 0; i < model.Rows.Count; i++)
            {
                rows.Add(ProjectRow(model.Rows[i], i, i == model.SelectedIndex));
            }
            return rows;
        }

        private static BreadcrumbRowRender ProjectRow(
            BreadcrumbRow row,
            int rowIndex,
            bool selected
        )
        {
            var names = row.IsSuggestion
                ? row.Chain.Select(segment => segment.DisplayName).ToArray()
                : SplitVerbatim(row.VerbatimText!);

            int visibleCount = row.CollapsedAfterIndex.HasValue
                ? row.CollapsedAfterIndex.Value + 1
                : names.Length;

            var cells = new List<BreadcrumbCellRender>();
            for (int s = 0; s < visibleCount; s++)
            {
                bool isTerminal = s == visibleCount - 1;
                if (s > 0)
                {
                    cells.Add(
                        new BreadcrumbCellRender(BreadcrumbCellKind.Arrow, string.Empty, -1, false)
                    );
                }
                if (row.CollapsedAfterIndex.HasValue && isTerminal)
                {
                    // The re-expand plus sits to the left of the now-terminal segment (FR-3).
                    cells.Add(
                        new BreadcrumbCellRender(BreadcrumbCellKind.Plus, string.Empty, -1, false)
                    );
                }
                bool interior = s > 0 && !isTerminal;
                cells.Add(
                    new BreadcrumbCellRender(BreadcrumbCellKind.Segment, names[s], s, interior)
                );
            }

            if (row.LeafHasSubfolders)
            {
                // Leaf-only affordance: plus when the expansion is closed, minus when open (FR-2).
                cells.Add(
                    new BreadcrumbCellRender(
                        row.LeafExpanded ? BreadcrumbCellKind.Minus : BreadcrumbCellKind.Plus,
                        string.Empty,
                        -1,
                        false
                    )
                );
            }

            var subfolders = row
                .Subfolders.Select(s => new BreadcrumbSubfolderRender(
                    s.DisplayName,
                    s.FolderPath,
                    s.HasChildren
                ))
                .ToArray();

            return new BreadcrumbRowRender(
                rowIndex,
                row.IsSuggestion,
                selected,
                row.CollapsedAfterIndex.HasValue,
                row.LeafExpanded,
                row.IsSuggestion
                    ? PercentageFormatter.FormatPercent(row.Probability)
                    : string.Empty,
                cells,
                subfolders
            );
        }

        private static string[] SplitVerbatim(string verbatimText)
        {
            var parts = verbatimText.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? new[] { verbatimText } : parts;
        }
    }
}
