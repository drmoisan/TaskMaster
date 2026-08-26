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
        private readonly IBreadcrumbWebHost _host;
        private readonly BreadcrumbMessageCodec _codec;
        private readonly BreadcrumbHtmlRenderer _renderer;
        private readonly BreadcrumbOutboundQueue _outboundQueue;
        private readonly BreadcrumbRowBuilder _builder = new BreadcrumbRowBuilder();

        private IReadOnlyList<BreadcrumbRow> _rows = Array.Empty<BreadcrumbRow>();
        private string? _selectedRowId;
        private string? _pendingDocument;
        private string _archiveRootPath = string.Empty;
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
            _archiveRootPath = archiveRootPath ?? string.Empty;
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

                string hierarchyPath = ToHierarchyPath(text, _archiveRootPath);
                IReadOnlyList<FolderBreadcrumbSegment>? chain = await FetchChainAsync(
                    hierarchyPath,
                    cancellationToken
                );
                if (chain != null)
                {
                    chains[text] = chain;
                }
            }

            _rows = _builder.BuildRows(
                presentedRows,
                text => chains.TryGetValue(text, out var chain) ? chain : null,
                scores
            );
            AttachSegmentKeys(presentedRows, chains);
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

        private static string ToHierarchyPath(string presentedTarget, string archiveRootPath)
        {
            if (string.IsNullOrWhiteSpace(archiveRootPath))
            {
                return presentedTarget;
            }

            string root = archiveRootPath.TrimEnd('\\', '/');
            if (root.Length == 0)
            {
                return presentedTarget;
            }

            if (
                string.Equals(presentedTarget, root, StringComparison.OrdinalIgnoreCase)
                || presentedTarget.StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase)
                || presentedTarget.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase)
            )
            {
                return presentedTarget;
            }

            return root + "\\" + presentedTarget.TrimStart('\\', '/');
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
