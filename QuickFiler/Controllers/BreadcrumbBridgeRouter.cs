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
    public sealed class BreadcrumbBridgeRouter
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
                    if (row.CollapseAfter(message.SegmentIndex!.Value))
                    {
                        PostRowRender(row);
                    }

                    break;
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

        private async Task HandleLeafToggleAsync(BreadcrumbRow row)
        {
            if (row.IsCollapsed)
            {
                if (row.ReExpand())
                {
                    PostRowRender(row);
                }

                return;
            }

            if (row.IsLeafExpanded)
            {
                if (row.ToggleLeafExpanded())
                {
                    PostRowRender(row);
                }

                return;
            }

            await ExpandLeafAsync(row);
        }

        private async Task HandleArrowKeyAsync(BreadcrumbRow row, string key)
        {
            switch (key)
            {
                case "Right":
                    if (row.IsCollapsed)
                    {
                        if (row.ReExpand())
                        {
                            PostRowRender(row);
                        }
                    }
                    else if (!row.IsLeafExpanded)
                    {
                        await ExpandLeafAsync(row);
                    }

                    break;
                case "Left":
                    if (row.LeftArrow())
                    {
                        PostRowRender(row);
                    }

                    break;
                case "Up":
                    HandleUpArrow(row);
                    break;
                case "Down":
                    MoveSelection(row, step: 1);
                    break;
                default:
                    log.Error($"Unknown breadcrumb arrow key '{key}' for row '{row.RowId}'.");
                    break;
            }
        }

        private void HandleUpArrow(BreadcrumbRow row)
        {
            BreadcrumbRow? previous = FindSelectable(IndexOf(row) - 1, step: -1);
            if (previous == null)
            {
                // Up at the top row: hand focus back to the search box.
                PostOutbound(new BreadcrumbFocusSearchMessage());
                FocusSearchRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            SelectRow(previous);
        }

        private void MoveSelection(BreadcrumbRow row, int step)
        {
            BreadcrumbRow? next = FindSelectable(IndexOf(row) + step, step);
            if (next != null)
            {
                SelectRow(next);
            }
        }

        private async Task ExpandLeafAsync(BreadcrumbRow row)
        {
            BreadcrumbSegment? activeSegment = row.ActiveSegment;
            if (row.Kind != BreadcrumbRowKind.Suggestion || activeSegment?.HasSubfolders != true)
            {
                return; // Active segment without subfolders (or non-suggestion row): no-op.
            }

            string requestId = "req-" + (++_requestSequence);
            try
            {
                FolderTreeNodeKey? key = row.ActiveSegmentKey;
                if (key == null)
                {
                    log.Error(
                        $"Breadcrumb expand {requestId}: no provider key for '{activeSegment.FullPath}'; row '{row.RowId}' left unchanged."
                    );
                    return;
                }

                IReadOnlyList<FolderBreadcrumbSegment> children =
                    await _provider.GetImmediateSubfoldersAsync(key, CancellationToken.None);
                IReadOnlyList<BreadcrumbSegment> mapped = BreadcrumbRowBuilder.MapSegments(
                    children
                );
                row.SetLeafChildren(mapped);
                row.ToggleLeafExpanded();
                PostOutbound(new BreadcrumbSubfolderResultMessage(requestId, row.RowId, mapped));
                PostRowRender(row);
            }
            catch (OperationCanceledException)
            {
                log.Error(
                    $"Breadcrumb expand {requestId} canceled for row '{row.RowId}'; state unchanged."
                );
            }
            catch (Exception ex)
            {
                // Provider I/O boundary: log the specific failure and leave row state unchanged.
                log.Error(
                    $"Breadcrumb expand {requestId} failed for row '{row.RowId}': {ex.Message}",
                    ex
                );
            }
        }

        private void ActivateSegment(BreadcrumbRow row, int segmentIndex)
        {
            if (!row.ActivateSegment(segmentIndex))
            {
                log.Error(
                    $"Breadcrumb segment activation rejected for row '{row.RowId}' and index '{segmentIndex}'."
                );
                return;
            }

            BreadcrumbSegment? activeSegment = row.ActiveSegment;
            if (activeSegment == null)
            {
                return;
            }

            SelectHierarchyPath(row, activeSegment.FullPath);
        }

        private void ActivateChild(BreadcrumbRow row, int childIndex)
        {
            BreadcrumbSegment? child = row.GetActiveChild(childIndex);
            if (child == null)
            {
                log.Error(
                    $"Breadcrumb child activation rejected for row '{row.RowId}' and index '{childIndex}'."
                );
                return;
            }

            SelectHierarchyPath(row, child.FullPath);
        }

        private async Task<IReadOnlyList<FolderBreadcrumbSegment>?> FetchChainAsync(
            string folderPath,
            CancellationToken cancellationToken
        )
        {
            try
            {
                FolderTreeNodeKey? key = await _provider.ResolveLeafKeyAsync(
                    folderPath,
                    cancellationToken
                );
                if (key == null)
                {
                    return null;
                }

                return await _provider.GetAncestorChainAsync(key, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                log.Error(
                    $"Breadcrumb chain fetch canceled for '{folderPath}'; rendering fallback."
                );
                return null;
            }
            catch (Exception ex)
            {
                // Provider I/O boundary: fall back to the builder's single-segment rendering.
                log.Error($"Breadcrumb chain fetch failed for '{folderPath}': {ex.Message}", ex);
                return null;
            }
        }

        private void SelectRow(BreadcrumbRow row)
        {
            if (row.Kind == BreadcrumbRowKind.Banner)
            {
                return; // Banner rows are never selectable.
            }

            _selectedRowId = row.RowId;
            SelectedFolderPath =
                row.Kind == BreadcrumbRowKind.TrashPseudoRow
                    ? BreadcrumbRowBuilder.TrashRowText
                    : row.FilingTarget;
            PostOutbound(
                new BreadcrumbRenderMessage(_renderer.RenderRows(_rows, _selectedRowId), null)
            );
            SelectedFolderPathChanged?.Invoke(this, SelectedFolderPath);
        }

        private void SelectHierarchyPath(BreadcrumbRow row, string fullPath)
        {
            _selectedRowId = row.RowId;
            SelectedFolderPath = ToArchiveRelativePath(fullPath);
            PostOutbound(
                new BreadcrumbRenderMessage(_renderer.RenderRows(_rows, _selectedRowId), null)
            );
            SelectedFolderPathChanged?.Invoke(this, SelectedFolderPath);
        }

        private string ToArchiveRelativePath(string fullPath)
        {
            string root = _archiveRootPath.TrimEnd('\\', '/');
            if (root.Length == 0)
            {
                return fullPath;
            }

            if (string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (
                fullPath.StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase)
                || fullPath.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase)
            )
            {
                return fullPath.Substring(root.Length).TrimStart('\\', '/');
            }

            return fullPath;
        }

        private void PostRowRender(BreadcrumbRow row)
        {
            PostOutbound(
                new BreadcrumbRenderMessage(
                    _renderer.RenderRowFragment(row, row.RowId == _selectedRowId),
                    row.RowId
                )
            );
        }

        private void PostOutbound(BreadcrumbOutboundMessage message)
        {
            _outboundQueue.PostOrQueue(_codec.SerializeOutbound(message));
        }

        private void DeliverDocument()
        {
            string document = _renderer.RenderDocument(_rows, _darkMode, _selectedRowId);
            if (_host.IsCoreInitialized)
            {
                _host.NavigateToString(document);
                _pendingDocument = null;
            }
            else
            {
                _pendingDocument = document;
            }
        }

        private BreadcrumbRow? FindRow(string rowId)
        {
            foreach (BreadcrumbRow row in _rows)
            {
                if (row.RowId == rowId)
                {
                    return row;
                }
            }

            return null;
        }

        private int IndexOf(BreadcrumbRow row)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (ReferenceEquals(_rows[i], row))
                {
                    return i;
                }
            }

            return -1;
        }

        private BreadcrumbRow? FindSelectable(int startIndex, int step)
        {
            for (int i = startIndex; i >= 0 && i < _rows.Count; i += step)
            {
                if (_rows[i].Kind != BreadcrumbRowKind.Banner)
                {
                    return _rows[i];
                }
            }

            return null;
        }
    }
}
