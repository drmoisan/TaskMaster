#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Non-exempt bridge router for the EfcViewer breadcrumb control (#349): binds suggestion
    /// rows to breadcrumb rows via the 9101 provider, routes inbound bridge messages to row
    /// state transitions and provider queries, and delivers outbound documents/messages through
    /// the <see cref="IBreadcrumbWebHost"/> seam. Contains no WebView2/WinForms/COM types and
    /// derives no hierarchy from suggestion-row prefix matching.
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private readonly IFolderHierarchyProvider _provider;

        // #799 AC7: obtained by an `as` cast in the constructor, so no constructor signature
        // changes and no existing test breaks. A Mock&lt;IFolderHierarchyProvider&gt; is not an
        // IFolderLabelAbsenceReport, so this stays null and suppression is inert in every existing
        // router test.
        private readonly IFolderLabelAbsenceReport? _absenceReport;
        private readonly IBreadcrumbWebHost _host;
        private readonly BreadcrumbMessageCodec _codec;
        private readonly BreadcrumbHtmlRenderer _renderer;
        private readonly BreadcrumbOutboundQueue _outboundQueue;
        private readonly BreadcrumbRowBuilder _builder = new BreadcrumbRowBuilder();

        private IReadOnlyList<BreadcrumbRow> _rows = Array.Empty<BreadcrumbRow>();
        private string? _selectedRowId;
        private string? _pendingDocument;
        private string _boundRoot = string.Empty;
        private bool _darkMode;
        private int _requestSequence;

        /// <summary>Creates the router over its collaborator seams.</summary>
        /// <exception cref="ArgumentNullException">Any collaborator is null.</exception>
        public BreadcrumbBridgeRouter(
            IFolderHierarchyProvider provider,
            IBreadcrumbWebHost host,
            BreadcrumbMessageCodec codec,
            BreadcrumbHtmlRenderer renderer,
            BreadcrumbOutboundQueue outboundQueue
        )
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _absenceReport = provider as IFolderLabelAbsenceReport;
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _outboundQueue =
                outboundQueue ?? throw new ArgumentNullException(nameof(outboundQueue));
            _host.MessageReceived += OnHostMessageReceived;
        }

        /// <summary>Full path of the selected folder row, or null when nothing is selected.</summary>
        public string? SelectedFolderPath { get; private set; }

        /// <summary>Raised when <see cref="SelectedFolderPath"/> changes via a selection action.</summary>
        public event EventHandler<string?>? SelectedFolderPathChanged;

        /// <summary>Raised when Up is pressed on the top row (SearchText focus parity).</summary>
        public event EventHandler? FocusSearchRequested;

        /// <summary>
        /// Builds breadcrumb rows from the presented suggestion rows (9101 ancestor chain per
        /// suggestion via <c>ResolveLeafKeyAsync</c> + <c>GetAncestorChainAsync</c>), renders the
        /// document, and delivers it via NavigateToString (or defers it until core init).
        /// </summary>
        /// <param name="presentedRows">Presented row texts in display order.</param>
        /// <param name="scores">Score projections joined by full-path equality.</param>
        /// <param name="cancellationToken">Token observed by the provider calls.</param>
        public async Task BindRowsAsync(
            IReadOnlyList<string> presentedRows,
            IEnumerable<FolderScore> scores,
            CancellationToken cancellationToken
        )
        {
            await BindRowsAsync(presentedRows, scores, string.Empty, cancellationToken);
        }

        /// <summary>
        /// Builds breadcrumb rows while using the archive root only for hierarchy lookups. The
        /// displayed and selected filing targets remain the original presented values.
        /// </summary>
        /// <param name="presentedRows">Presented row texts in display order.</param>
        /// <param name="scores">Score projections joined by presented filing-target equality.</param>
        /// <param name="archiveRootPath">The full Outlook archive root used by the hierarchy provider.</param>
        /// <param name="cancellationToken">Token observed by the provider calls.</param>
        internal async Task BindRowsAsync(
            IReadOnlyList<string> presentedRows,
            IEnumerable<FolderScore> scores,
            string archiveRootPath,
            CancellationToken cancellationToken
        )
        {
            if (presentedRows == null)
            {
                throw new ArgumentNullException(nameof(presentedRows));
            }

            var chains = new Dictionary<string, IReadOnlyList<FolderBreadcrumbSegment>>(
                StringComparer.OrdinalIgnoreCase
            );
            HashSet<string>? suppressed = null;
            _boundRoot = string.IsNullOrWhiteSpace(archiveRootPath)
                ? string.Empty
                : archiveRootPath.TrimEnd('\\', '/');
            foreach (string text in presentedRows)
            {
                if (
                    text == null
                    || chains.ContainsKey(text)
                    || BreadcrumbRowBuilder.Classify(text) != BreadcrumbRowKind.Suggestion
                )
                {
                    continue;
                }

                string? hierarchyPath = ToHierarchyPath(text);
                IReadOnlyList<FolderBreadcrumbSegment>? chain =
                    hierarchyPath == null
                        ? null
                        : await FetchChainAsync(hierarchyPath, cancellationToken);
                if (chain != null)
                {
                    chains[text] = chain;
                    continue;
                }

                // #799 AC7 (Efc surface only, per decision D5). A null chain arising from
                // cancellation or from a provider fault is NOT suppressed: those rows are not
                // known-absent, and only the zero-candidate classification is.
                if (
                    hierarchyPath != null
                    && _absenceReport != null
                    && _absenceReport.IsAbsentLabel(hierarchyPath)
                )
                {
                    suppressed ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    suppressed.Add(text);
                }
            }

            IReadOnlyList<string> retainedRows = RetainedRows(presentedRows, suppressed);
            _rows = _builder.BuildRows(
                retainedRows,
                text => chains.TryGetValue(text, out var chain) ? chain : null,
                WithProjectedScoreKeys(scores)
            );

            // The SAME retained list is handed to both calls: AttachSegmentKeys indexes the
            // presented rows by row index, so an unfiltered list here would mis-align every row
            // after the suppressed one.
            AttachSegmentKeys(retainedRows, chains);
            _selectedRowId = null;

            // #499: the rows just rebuilt are a new set, so a folder path selected against the
            // previous set is stale. Clear it with the row id and notify subscribers, but only
            // when the value actually changed, so a re-bind with no prior selection is silent.
            if (SelectedFolderPath != null)
            {
                SelectedFolderPath = null;
                SelectedFolderPathChanged?.Invoke(this, null);
            }

            DeliverDocument();
        }

        /// <summary>
        /// AC6: emits every original score UNCHANGED and, additionally, one archive-relative alias
        /// for each score whose path is archive-rooted. The addition is what makes it safe — a
        /// substitution would fix the stem-presented case and silently break the rooted-presented
        /// case — and the row builder's probability index assigns through its indexer, so a
        /// duplicate key is tolerated rather than throwing.
        /// </summary>
        private IEnumerable<FolderScore> WithProjectedScoreKeys(IEnumerable<FolderScore> scores)
        {
            // An empty bound root makes the projection the identity, so the public three-argument
            // overload's callers see no change and allocate nothing. A null sequence is passed
            // through, null-forgiving, so the row builder keeps raising its own
            // ArgumentNullException rather than this method raising a different one.
            if (scores == null || _boundRoot.Length == 0)
            {
                return scores!;
            }

            var joined = new List<FolderScore>();
            foreach (FolderScore score in scores)
            {
                joined.Add(score);
                if (score.FolderPath == null)
                {
                    continue;
                }

                // Null-forgiving: ToDisplayStem returns null only for a null folderPath, which the
                // guard above excludes; unsuppressed the construction below is CS8604.
                string projected = ArchiveStemProjection.ToDisplayStem(
                    score.FolderPath,
                    _boundRoot
                )!;
                if (!string.Equals(projected, score.FolderPath, StringComparison.Ordinal))
                {
                    joined.Add(new FolderScore(projected, score.Score, score.Probability));
                }
            }

            return joined;
        }

        /// <summary>
        /// AC7: the presented sequence with the known-absent labels removed, filtered BEFORE row
        /// construction because row ids are assigned as <c>row-&lt;index&gt;</c> over this sequence.
        /// Returns the original instance when nothing was suppressed.
        /// </summary>
        private IReadOnlyList<string> RetainedRows(
            IReadOnlyList<string> presentedRows,
            HashSet<string>? suppressed
        )
        {
            if (suppressed == null || suppressed.Count == 0)
            {
                return presentedRows;
            }

            var retained = new List<string>(presentedRows.Count);
            foreach (string text in presentedRows)
            {
                if (!string.IsNullOrEmpty(text) && suppressed.Contains(text))
                {
                    continue;
                }

                // Null-forgiving: a null entry is carried through exactly as the unfiltered list
                // carried it, so the row builder's handling of it is unchanged.
                retained.Add(text!);
            }

            log.Debug(
                $"#799 AC7: suppressed {suppressed.Count} zero-candidate breadcrumb row(s) of {presentedRows.Count} presented."
            );
            return retained;
        }

        private string? ToHierarchyPath(string presentedTarget)
        {
            // #609 preserved: a RELATIVE presented target stays root-prefixed for the lookup.
            // #614 D3: an out-of-root full path is not fabricated into a hierarchy identity;
            // null keeps the row on the existing single-segment fallback rendering.
            if (_boundRoot.Length == 0 || !ArchiveStemContract.IsFullOutlookPath(presentedTarget))
            {
                return _boundRoot.Length == 0
                    ? presentedTarget
                    : _boundRoot + "\\" + presentedTarget;
            }

            return ArchiveStemContract.TryMakeArchiveRelative(presentedTarget, _boundRoot, out _)
                ? presentedTarget
                : null;
        }

        private void AttachSegmentKeys(
            IReadOnlyList<string> presentedRows,
            IReadOnlyDictionary<string, IReadOnlyList<FolderBreadcrumbSegment>> chains
        )
        {
            for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
            {
                string? text = presentedRows[rowIndex];
                BreadcrumbRow row = _rows[rowIndex];
                if (
                    text == null
                    || row.Kind != BreadcrumbRowKind.Suggestion
                    || !chains.TryGetValue(text, out IReadOnlyList<FolderBreadcrumbSegment> chain)
                )
                {
                    continue;
                }

                int segmentCount = Math.Min(row.Segments.Count, chain.Count);
                for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
                {
                    row.SetSegmentKey(segmentIndex, chain[segmentIndex].Key);
                }
            }
        }

        /// <summary>Selects the first selectable (non-banner) row and posts the updated render.</summary>
        public void SelectFirstRow()
        {
            BreadcrumbRow? first = FindSelectable(startIndex: 0, step: 1);
            if (first != null)
            {
                SelectRow(first);
            }
        }

        /// <summary>Re-renders and re-delivers the document with the requested theme.</summary>
        /// <param name="darkMode">True for the dark CSS block.</param>
        public void ApplyTheme(bool darkMode)
        {
            _darkMode = darkMode;
            DeliverDocument();
        }

        /// <summary>
        /// Signals CoreWebView2 initialization completion: delivers any deferred document and
        /// flushes queued outbound payloads. Idempotent for pooled-viewer re-initialization.
        /// </summary>
        public void NotifyCoreInitialized()
        {
            if (_pendingDocument != null)
            {
                _host.NavigateToString(_pendingDocument);
                _pendingDocument = null;
            }

            _outboundQueue.OnInitializationCompleted();
        }

        /// <summary>
        /// Routes one inbound bridge payload. Malformed payloads fail fast with the codec's
        /// <see cref="BreadcrumbMessageException"/> (already logged) and leave state unchanged.
        /// </summary>
        /// <param name="json">The raw inbound JSON payload.</param>
        public async Task ProcessInboundAsync(string json)
        {
            BreadcrumbInboundMessage message = _codec.DeserializeInbound(json);
            BreadcrumbRow? row = FindRow(message.RowId);
            if (row == null)
            {
                log.Error($"Inbound breadcrumb message targets unknown row '{message.RowId}'.");
                return;
            }

            switch (message.Type)
            {
                case BreadcrumbMessageTypes.SegmentDoubleClick:
                {
                    // #498: the bridge is an untrusted boundary, so the segment index is validated
                    // here rather than relying on BreadcrumbRow.CollapseAfter to throw. An index
                    // that escaped this arm reached the async void host-event seam as an unhandled
                    // exception, which the single catch (BreadcrumbMessageException) cannot contain.
                    int? requestedIndex = message.SegmentIndex;
                    if (
                        !requestedIndex.HasValue
                        || requestedIndex.Value < 0
                        || requestedIndex.Value >= row.Segments.Count
                    )
                    {
                        log.Error(
                            $"Inbound segmentDoubleClick for row '{row.RowId}' carries segment index "
                                + $"'{requestedIndex}', which is outside the valid range "
                                + $"[0, {row.Segments.Count - 1}]; rejected without a transition."
                        );
                        break;
                    }

                    if (row.CollapseAfter(requestedIndex.Value))
                    {
                        PostRowRender(row);
                    }

                    break;
                }
                case BreadcrumbMessageTypes.SegmentActivate:
                    ActivateSegment(row, message.SegmentIndex!.Value);
                    break;
                case BreadcrumbMessageTypes.RenderedChildActivate:
                    ActivateChild(row, message.ChildIndex!.Value);
                    break;
                case BreadcrumbMessageTypes.LeafExpandToggle:
                    await HandleLeafToggleAsync(row);
                    break;
                case BreadcrumbMessageTypes.ArrowKey:
                    await HandleArrowKeyAsync(row, message.Key!);
                    break;
                case BreadcrumbMessageTypes.RowSelected:
                    SelectRow(row);
                    break;
            }
        }

        private async void OnHostMessageReceived(object? sender, string json)
        {
            try
            {
                await ProcessInboundAsync(json);
            }
            catch (BreadcrumbMessageException)
            {
                // Boundary: the codec already logged the specific malformed-payload error; the
                // router state is unchanged and the UI message pump must not be crashed.
            }
        }
    }
}
