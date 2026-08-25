#nullable enable
using System;
using System.Collections.Generic;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Discriminator values for the breadcrumb JS&lt;-&gt;.NET bridge messages (#349).
    /// </summary>
    public static class BreadcrumbMessageTypes
    {
        /// <summary>Inbound: double-click on a non-leaf breadcrumb segment.</summary>
        public const string SegmentDoubleClick = "segmentDoubleClick";

        /// <summary>Inbound: activation of a non-leaf breadcrumb segment.</summary>
        public const string SegmentActivate = "segmentActivate";

        /// <summary>Inbound: activation of a rendered child of the active segment.</summary>
        public const string RenderedChildActivate = "renderedChildActivate";

        /// <summary>Inbound: activation of the leaf (or re-expand) affordance.</summary>
        public const string LeafExpandToggle = "leafExpandToggle";

        /// <summary>Inbound: arrow-key press routed from the hosted document.</summary>
        public const string ArrowKey = "arrowKey";

        /// <summary>Inbound: a row was selected in the hosted document.</summary>
        public const string RowSelected = "rowSelected";

        /// <summary>Outbound: render a full document or per-row fragment.</summary>
        public const string Render = "render";

        /// <summary>Outbound: correlated response to a leaf expand request.</summary>
        public const string SubfolderResult = "subfolderResult";

        /// <summary>Outbound: move focus to the search text box (Up at the top row).</summary>
        public const string FocusSearch = "focusSearch";
    }

    /// <summary>
    /// Inbound bridge message (JS -&gt; .NET), discriminated by <see cref="Type"/>:
    /// <c>{ type, rowId, segmentIndex?, childIndex?, key? }</c>. net48-safe plain class (no
    /// record/init).
    /// </summary>
    public sealed class BreadcrumbInboundMessage
    {
        /// <summary>
        /// Creates an inbound message.
        /// </summary>
        /// <param name="type">Message discriminator (an inbound <see cref="BreadcrumbMessageTypes"/> value).</param>
        /// <param name="rowId">Target row identifier. Required.</param>
        /// <param name="segmentIndex">Segment index for segment-scoped messages, or null.</param>
        /// <param name="childIndex">Child index for rendered-child activation messages, or null.</param>
        /// <param name="key">Arrow-key name (<c>Left</c>/<c>Right</c>/<c>Up</c>/<c>Down</c>), or null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="rowId"/> is null.</exception>
        public BreadcrumbInboundMessage(
            string type,
            string rowId,
            int? segmentIndex,
            int? childIndex,
            string? key
        )
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            RowId = rowId ?? throw new ArgumentNullException(nameof(rowId));
            SegmentIndex = segmentIndex;
            ChildIndex = childIndex;
            Key = key;
        }

        /// <summary>Message discriminator.</summary>
        public string Type { get; }

        /// <summary>Target row identifier.</summary>
        public string RowId { get; }

        /// <summary>Segment index for segment-scoped messages; null otherwise.</summary>
        public int? SegmentIndex { get; }

        /// <summary>Child index for rendered-child activation messages; null otherwise.</summary>
        public int? ChildIndex { get; }

        /// <summary>Arrow-key name for <see cref="BreadcrumbMessageTypes.ArrowKey"/>; null otherwise.</summary>
        public string? Key { get; }
    }

    /// <summary>
    /// Base outbound bridge message (.NET -&gt; JS), discriminated by <see cref="Type"/>.
    /// </summary>
    public abstract class BreadcrumbOutboundMessage
    {
        /// <summary>Creates the outbound base with its discriminator.</summary>
        /// <param name="type">Message discriminator (an outbound <see cref="BreadcrumbMessageTypes"/> value).</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
        protected BreadcrumbOutboundMessage(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>Message discriminator.</summary>
        public string Type { get; }
    }

    /// <summary>
    /// Outbound <c>render</c> message carrying generated HTML — a full document
    /// (<see cref="RowId"/> null) or a per-row update fragment (<see cref="RowId"/> set).
    /// </summary>
    public sealed class BreadcrumbRenderMessage : BreadcrumbOutboundMessage
    {
        /// <summary>Creates a render message.</summary>
        /// <param name="html">The generated HTML payload. Required.</param>
        /// <param name="rowId">Target row for a fragment update, or null for a full document.</param>
        /// <exception cref="ArgumentNullException"><paramref name="html"/> is null.</exception>
        public BreadcrumbRenderMessage(string html, string? rowId)
            : base(BreadcrumbMessageTypes.Render)
        {
            Html = html ?? throw new ArgumentNullException(nameof(html));
            RowId = rowId;
        }

        /// <summary>The generated HTML payload.</summary>
        public string Html { get; }

        /// <summary>Target row for a fragment update; null for a full document.</summary>
        public string? RowId { get; }
    }

    /// <summary>
    /// Outbound <c>subfolderResult</c> message correlated to its originating leaf expand request
    /// by <see cref="RequestId"/>, carrying the child-segment payload.
    /// </summary>
    public sealed class BreadcrumbSubfolderResultMessage : BreadcrumbOutboundMessage
    {
        /// <summary>Creates a subfolder-result message.</summary>
        /// <param name="requestId">Correlation id of the originating expand request. Required.</param>
        /// <param name="rowId">Row whose leaf was expanded. Required.</param>
        /// <param name="children">Immediate child segments of the expanded leaf. Required.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public BreadcrumbSubfolderResultMessage(
            string requestId,
            string rowId,
            IReadOnlyList<BreadcrumbSegment> children
        )
            : base(BreadcrumbMessageTypes.SubfolderResult)
        {
            RequestId = requestId ?? throw new ArgumentNullException(nameof(requestId));
            RowId = rowId ?? throw new ArgumentNullException(nameof(rowId));
            Children = children ?? throw new ArgumentNullException(nameof(children));
        }

        /// <summary>Correlation id of the originating expand request.</summary>
        public string RequestId { get; }

        /// <summary>Row whose leaf was expanded.</summary>
        public string RowId { get; }

        /// <summary>Immediate child segments of the expanded leaf.</summary>
        public IReadOnlyList<BreadcrumbSegment> Children { get; }
    }

    /// <summary>
    /// Outbound <c>focusSearch</c> message (Up-arrow at the top row); carries no payload.
    /// </summary>
    public sealed class BreadcrumbFocusSearchMessage : BreadcrumbOutboundMessage
    {
        /// <summary>Creates a focus-search message.</summary>
        public BreadcrumbFocusSearchMessage()
            : base(BreadcrumbMessageTypes.FocusSearch) { }
    }
}
