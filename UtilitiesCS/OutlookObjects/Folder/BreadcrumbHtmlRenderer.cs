#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Generates the breadcrumb HTML document and per-row update fragments from
    /// <see cref="BreadcrumbRow"/> collections plus a theme flag (#349). Pure: consumes only the
    /// row model and <see cref="BreadcrumbDocumentAssets"/> — no I/O, no WebView2 types.
    /// </summary>
    /// <remarks>
    /// Renderer invariants: percent markup (via <see cref="PercentageFormatter.FormatPercent"/>)
    /// is ALWAYS the trailing <c>.pct</c> flex item on every row; the plus/minus leaf affordance
    /// is emitted only when the relevant segment's <c>HasSubfolders</c> is true (plus when
    /// collapsed, minus when expanded); banner rows are non-interactive (no handlers, no
    /// affordance); folder display names are HTML-encoded; collapsed rows render the re-expand
    /// plus at the now-terminal segment.
    /// </remarks>
    public sealed class BreadcrumbHtmlRenderer
    {
        /// <summary>
        /// Renders the full HTML document (inline CSS/JS) for delivery via NavigateToString.
        /// </summary>
        /// <param name="rows">The breadcrumb rows in presented order.</param>
        /// <param name="darkMode">True to embed the dark theme CSS block, false for light.</param>
        /// <param name="selectedRowId">Row id to mark selected, or null.</param>
        /// <returns>The complete HTML document.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rows"/> is null.</exception>
        public string RenderDocument(
            IReadOnlyList<BreadcrumbRow> rows,
            bool darkMode,
            string? selectedRowId
        )
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><style>");
            sb.Append(BreadcrumbDocumentAssets.BaseCss);
            sb.Append(
                darkMode
                    ? BreadcrumbDocumentAssets.DarkThemeCss
                    : BreadcrumbDocumentAssets.LightThemeCss
            );
            sb.Append("</style></head><body><div class=\"rows\" id=\"rows\">");
            sb.Append(RenderRows(rows, selectedRowId));
            sb.Append("</div><script>");
            sb.Append(BreadcrumbDocumentAssets.BridgeJs);
            sb.Append("</script></body></html>");
            return sb.ToString();
        }

        /// <summary>
        /// Renders the row-list fragment (the <c>#rows</c> innerHTML) for a full-list
        /// <c>render</c> message update.
        /// </summary>
        /// <param name="rows">The breadcrumb rows in presented order.</param>
        /// <param name="selectedRowId">Row id to mark selected, or null.</param>
        /// <returns>The concatenated row fragments.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rows"/> is null.</exception>
        public string RenderRows(IReadOnlyList<BreadcrumbRow> rows, string? selectedRowId)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            var sb = new StringBuilder();
            foreach (BreadcrumbRow row in rows)
            {
                sb.Append(
                    RenderRowFragment(row, selectedRowId != null && row.RowId == selectedRowId)
                );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Renders one row's update fragment (the <c>[data-row-id]</c> wrapper element).
        /// </summary>
        /// <param name="row">The row to render.</param>
        /// <param name="isSelected">True to mark the row selected.</param>
        /// <returns>The row fragment HTML.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="row"/> is null.</exception>
        public string RenderRowFragment(BreadcrumbRow row, bool isSelected)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            var sb = new StringBuilder();
            string wrapClass = "rowwrap" + (isSelected ? " selected" : string.Empty);
            sb.Append("<div class=\"")
                .Append(wrapClass)
                .Append("\" data-row-id=\"")
                .Append(WebUtility.HtmlEncode(row.RowId))
                .Append("\">");

            switch (row.Kind)
            {
                case BreadcrumbRowKind.Banner:
                    AppendBannerRow(sb, row);
                    break;
                case BreadcrumbRowKind.TrashPseudoRow:
                    AppendTrashRow(sb);
                    break;
                default:
                    AppendSuggestionRow(sb, row);
                    break;
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        private static void AppendBannerRow(StringBuilder sb, BreadcrumbRow row)
        {
            // Non-interactive: no selectable class, no seg indices, no affordance, no handlers.
            string text = row.Segments.Count > 0 ? row.Segments[0].DisplayName : string.Empty;
            sb.Append("<div class=\"row banner\"><div class=\"crumb\">")
                .Append(WebUtility.HtmlEncode(text))
                .Append("</div>");
            AppendPercent(sb, row.Probability);
            sb.Append("</div>");
        }

        private static void AppendTrashRow(StringBuilder sb)
        {
            // Selectable pseudo-row without segments or affordance.
            sb.Append("<div class=\"row selectable trash\"><div class=\"crumb\">")
                .Append(WebUtility.HtmlEncode(BreadcrumbRowBuilder.TrashRowText))
                .Append("</div>");
            AppendPercent(sb, null);
            sb.Append("</div>");
        }

        private static void AppendSuggestionRow(StringBuilder sb, BreadcrumbRow row)
        {
            sb.Append("<div class=\"row selectable suggestion\"><div class=\"crumb\">");

            IReadOnlyList<BreadcrumbSegment> visible = row.VisibleSegments();
            for (int i = 0; i < visible.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append("<span class=\"sep\">→</span>");
                }

                bool isCollapsedTerminal = row.IsCollapsed && i == visible.Count - 1;
                if (isCollapsedTerminal)
                {
                    // Re-expand plus to the LEFT of the now-terminal segment.
                    sb.Append(
                        "<span class=\"affordance reexpand\" data-role=\"reexpand\">+</span>"
                    );
                }

                AppendSegment(sb, visible[i], i, i < row.Segments.Count - 1);
            }

            if (!row.IsCollapsed)
            {
                AppendLeafAffordance(sb, row);
            }

            sb.Append("</div>");
            AppendPercent(sb, row.Probability);
            sb.Append("</div>");
            AppendChildren(sb, row);
        }

        private static void AppendSegment(
            StringBuilder sb,
            BreadcrumbSegment segment,
            int index,
            bool isNonLeaf
        )
        {
            sb.Append("<span class=\"seg\" data-segment-index=\"")
                .Append(index)
                .Append(isNonLeaf ? "\" data-segment-activate=\"true" : string.Empty)
                .Append("\" title=\"")
                .Append(WebUtility.HtmlEncode(segment.FullPath))
                .Append("\">")
                .Append(WebUtility.HtmlEncode(segment.DisplayName))
                .Append("</span>");
        }

        private static void AppendLeafAffordance(StringBuilder sb, BreadcrumbRow row)
        {
            // Emitted only when the active segment has subfolders: plus collapsed, minus expanded.
            if (row.ActiveSegment?.HasSubfolders != true)
            {
                return;
            }

            sb.Append("<span class=\"affordance leaf\" data-role=\"leaf\">")
                .Append(row.IsLeafExpanded ? "&#8722;" : "+")
                .Append("</span>");
        }

        private static void AppendPercent(StringBuilder sb, double? probability)
        {
            // Invariant: the percent is ALWAYS the trailing fixed .pct flex item on every row.
            sb.Append("<span class=\"pct\">")
                .Append(WebUtility.HtmlEncode(PercentageFormatter.FormatPercent(probability)))
                .Append("</span>");
        }

        private static void AppendChildren(StringBuilder sb, BreadcrumbRow row)
        {
            sb.Append("<div class=\"children\">");
            if (row.IsLeafExpanded)
            {
                for (int i = 0; i < row.LeafChildren.Count; i++)
                {
                    BreadcrumbSegment child = row.LeafChildren[i];
                    sb.Append("<div class=\"child\" data-child-index=\"")
                        .Append(i)
                        .Append("\" title=\"")
                        .Append(WebUtility.HtmlEncode(child.FullPath))
                        .Append("\">")
                        .Append(WebUtility.HtmlEncode(child.DisplayName))
                        .Append("</div>");
                }
            }

            sb.Append("</div>");
        }
    }
}
