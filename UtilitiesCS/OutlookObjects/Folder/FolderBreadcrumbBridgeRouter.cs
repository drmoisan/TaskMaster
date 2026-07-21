#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Pure async message router for the QuickFiler breadcrumb bridge (#351 P3-T7): JSON string in
    /// -&gt; typed message -&gt; <see cref="BreadcrumbStateModel"/> transition and/or
    /// <see cref="IFolderHierarchyProvider"/> call -&gt; JSON string(s) out. Also owns the
    /// host-driven population entry points (suggestions, plain items, clear, selection) so all
    /// correctness lives in this host-neutral, fully unit-testable type. No WebView2, WinForms, or
    /// COM references; the only I/O reachable from here is behind the injected provider (G6).
    /// </summary>
    public sealed class FolderBreadcrumbBridgeRouter
    {
        private readonly IFolderHierarchyProvider _provider;
        private readonly BreadcrumbStateModel _model = new BreadcrumbStateModel();
        private readonly object _sync = new object();
        private int _suggestionGeneration;

        /// <summary>
        /// Creates a router over the injected 9101 provider.
        /// </summary>
        /// <param name="provider">The merged 9101 hierarchy provider. Required.</param>
        /// <exception cref="ArgumentNullException"><paramref name="provider"/> is null.</exception>
        public FolderBreadcrumbBridgeRouter(IFolderHierarchyProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>The routed state model (selection reads go through the selection map).</summary>
        public BreadcrumbStateModel Model => _model;

        /// <summary>
        /// Populates Path A suggestion rows: each scored row's ancestor chain comes from the
        /// provider (<c>ResolveLeafKeyAsync</c> + <c>GetAncestorChainAsync</c>, FR-1/FR-4); scored
        /// rows whose path cannot be resolved fall back to a plain row carrying the score's folder
        /// path so the selection contract still yields the exact path (G10). Non-scored rows
        /// (separators, search results, recents) become plain verbatim rows. Returns the render
        /// payload JSON.
        /// </summary>
        public async Task<string> SetSuggestionsAsync(
            IReadOnlyList<FolderRow> rows,
            CancellationToken cancellationToken
        )
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            int generation = Interlocked.Increment(ref _suggestionGeneration);

            // #398: resolve every row's ancestor chain into a LOCAL collection first, mutating no
            // shared model state while awaiting the provider, then swap the completed set into the
            // model atomically. This removes the mid-rebuild empty window that let a concurrent host
            // SelectRow race a transiently cleared or partially-populated model.
            var built = new List<BreadcrumbStateRow>(rows.Count);
            foreach (var row in rows)
            {
                if (row.Score.HasValue)
                {
                    string path = row.Score.Value.FolderPath;
                    var fallback = new BreadcrumbStateRow(path, path, row.Score.Value.Probability);
                    try
                    {
                        var key = await _provider
                            .ResolveLeafKeyAsync(path, cancellationToken)
                            .ConfigureAwait(false);
                        IReadOnlyList<FolderBreadcrumbSegment> chain =
                            key == null
                                ? new FolderBreadcrumbSegment[0]
                                : await _provider
                                    .GetAncestorChainAsync(key, cancellationToken)
                                    .ConfigureAwait(false);
                        built.Add(
                            chain != null && chain.Count > 0
                                ? new BreadcrumbStateRow(path, chain, row.Score.Value.Probability)
                                : fallback
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        built.Add(fallback);
                    }
                }
                else
                {
                    built.Add(new BreadcrumbStateRow(row.Text));
                }
            }

            lock (_sync)
            {
                if (generation != _suggestionGeneration)
                {
                    return RenderJsonCore();
                }
                ReplaceRowsPreservingIdentity(built);
                return RenderJsonCore();
            }
        }

        /// <summary>
        /// Synchronously publishes scored fallbacks before asynchronous hierarchy decoration.
        /// </summary>
        public string SetSuggestionFallbacks(IReadOnlyList<FolderRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            Interlocked.Increment(ref _suggestionGeneration);
            var built = new List<BreadcrumbStateRow>(rows.Count);
            foreach (var row in rows)
            {
                if (row.Score.HasValue)
                {
                    string path = row.Score.Value.FolderPath;
                    built.Add(new BreadcrumbStateRow(path, path, row.Score.Value.Probability));
                }
                else
                {
                    built.Add(new BreadcrumbStateRow(row.Text));
                }
            }

            lock (_sync)
            {
                ReplaceRowsPreservingIdentity(built);
                return RenderJsonCore();
            }
        }

        /// <summary>
        /// Populates Path B plain rows verbatim (search results, including the literal
        /// "Trash to Delete"; G10). Returns the render payload JSON.
        /// </summary>
        public string SetItems(IReadOnlyList<string> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Interlocked.Increment(ref _suggestionGeneration);
            lock (_sync)
            {
                _model.Clear();
                foreach (var item in items)
                {
                    _model.AddPlainRow(item);
                }
                return RenderJsonCore();
            }
        }

        /// <summary>Appends Path B plain rows without clearing (legacy AddRange semantics).</summary>
        public string AddItems(IReadOnlyList<string> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Interlocked.Increment(ref _suggestionGeneration);
            lock (_sync)
            {
                foreach (var item in items)
                {
                    _model.AddPlainRow(item);
                }
                return RenderJsonCore();
            }
        }

        /// <summary>Clears all rows and the selection. Returns the (empty) render payload JSON.</summary>
        public string Clear()
        {
            Interlocked.Increment(ref _suggestionGeneration);
            lock (_sync)
            {
                _model.Clear();
                return RenderJsonCore();
            }
        }

        /// <summary>Host-driven row selection (SetFolderSelectedIndex). Returns the render payload JSON.</summary>
        public string SelectRow(int index)
        {
            lock (_sync)
            {
                _model.SelectRow(index);
                return RenderJsonCore();
            }
        }

        /// <summary>The current render payload JSON (full re-render message, FR-6).</summary>
        public string RenderJson()
        {
            lock (_sync)
            {
                return RenderJsonCore();
            }
        }

        private string RenderJsonCore()
        {
            return BreadcrumbBridgeSerializer.Serialize(
                new RenderMessage(BreadcrumbRenderProjection.Project(_model))
            );
        }

        /// <summary>
        /// Routes one inbound bridge message and returns the ordered outbound JSON messages.
        /// Malformed input, unroutable messages, invalid indexes, and provider failures surface as
        /// an explicit <c>error</c> response (fail fast at this boundary — never silently dropped);
        /// cancellation propagates.
        /// </summary>
        public async Task<IReadOnlyList<string>> RouteAsync(
            string inboundJson,
            CancellationToken cancellationToken
        )
        {
            BreadcrumbBridgeMessage message;
            try
            {
                message = BreadcrumbBridgeSerializer.Parse(inboundJson);
            }
            catch (FormatException ex)
            {
                return ErrorResponse(ex.Message);
            }

            try
            {
                switch (message)
                {
                    case SegmentDoubleClickMessage m:
                        RowAt(m.RowIndex).CollapseAfter(m.SegmentIndex);
                        return new[] { RenderJson() };
                    case AffordanceToggleMessage m:
                        return await ToggleAsync(m.RowIndex, cancellationToken)
                            .ConfigureAwait(false);
                    case ArrowKeyMessage m:
                        return await ArrowAsync(m.Direction, cancellationToken)
                            .ConfigureAwait(false);
                    case SelectionChangeMessage m:
                        lock (_sync)
                        {
                            _model.SelectRow(m.RowIndex);
                            if (m.SubfolderIndex >= 0)
                            {
                                _model.SelectSubfolder(m.SubfolderIndex);
                            }
                        }
                        // The ack carries no mapped folder; the coordinator resolves the output
                        // string through the selection map when raising SelectionChanged (FR-7).
                        return new[]
                        {
                            RenderJson(),
                            BreadcrumbBridgeSerializer.Serialize(
                                new SelectionChangeMessage(m.RowIndex, m.SubfolderIndex, null)
                            ),
                        };
                    case SubfolderRequestMessage m:
                        return await SubfolderResponseAsync(m.RowIndex, cancellationToken)
                            .ConfigureAwait(false);
                    case ThemeChangeMessage m:
                        return new[]
                        {
                            BreadcrumbBridgeSerializer.Serialize(new ThemeChangeMessage(m.Theme)),
                            RenderJson(),
                        };
                    case UnhandledArrowMessage m:
                        // The JS-side report is re-emitted so the coordinator can invoke the
                        // legacy fall-through behavior (FR-6).
                        return new[] { BreadcrumbBridgeSerializer.Serialize(m) };
                    default:
                        return ErrorResponse(
                            $"Message type '{message.Type}' is not routable inbound."
                        );
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Boundary catch (JS bridge edge): state/provider failures must surface to the page
                // as an explicit error response instead of tearing down the message pump.
                return ErrorResponse(ex.Message);
            }
        }

        private async Task<IReadOnlyList<string>> ToggleAsync(
            int rowIndex,
            CancellationToken cancellationToken
        )
        {
            var row = RowAt(rowIndex);
            if (row.CollapsedAfterIndex != null)
            {
                row.ReExpand();
                return new[] { RenderJson() };
            }
            if (row.LeafExpanded)
            {
                row.TryCollapseLeaf();
                return new[] { RenderJson() };
            }
            if (!row.TryExpandLeaf())
            {
                return ErrorResponse($"Row {rowIndex} has no expand affordance.");
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<string>> ArrowAsync(
            BreadcrumbArrowDirection direction,
            CancellationToken cancellationToken
        )
        {
            bool handled =
                direction == BreadcrumbArrowDirection.Right
                    ? _model.RightArrow()
                    : _model.LeftArrow();
            if (!handled)
            {
                return new[]
                {
                    BreadcrumbBridgeSerializer.Serialize(new UnhandledArrowMessage(direction)),
                };
            }

            var row = _model.SelectedRow!;
            if (row.LeafExpanded && row.Subfolders.Count == 0)
            {
                return await FetchAndAttachSubfoldersAsync(
                        _model.SelectedIndex,
                        row,
                        cancellationToken
                    )
                    .ConfigureAwait(false);
            }
            return new[] { RenderJson() };
        }

        private async Task<IReadOnlyList<string>> FetchAndAttachSubfoldersAsync(
            int rowIndex,
            BreadcrumbStateRow row,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var leafKey = row.Chain[row.Chain.Count - 1].Key;
                var subfolders = await _provider
                    .GetImmediateSubfoldersAsync(leafKey, cancellationToken)
                    .ConfigureAwait(false);
                row.SetSubfolders(subfolders);
            }
            catch (OperationCanceledException)
            {
                row.TryCollapseLeaf();
                throw;
            }
            catch (Exception ex)
            {
                // Boundary catch: revert the expansion so the model stays consistent, then surface
                // the provider failure explicitly (P3-T8 negative contract).
                row.TryCollapseLeaf();
                return ErrorResponse($"Subfolder query failed: {ex.Message}");
            }

            var renderJson = RenderJson();
            var responseJson = BreadcrumbBridgeSerializer.Serialize(
                new SubfolderResponseMessage(
                    rowIndex,
                    BreadcrumbRenderProjection.Project(_model)[rowIndex].Subfolders
                )
            );
            return new[] { renderJson, responseJson };
        }

        private async Task<IReadOnlyList<string>> SubfolderResponseAsync(
            int rowIndex,
            CancellationToken cancellationToken
        )
        {
            var row = RowAt(rowIndex);
            if (!row.IsSuggestion)
            {
                return ErrorResponse($"Row {rowIndex} is a plain row without a subfolder query.");
            }
            if (!row.LeafExpanded && !row.TryExpandLeaf())
            {
                return ErrorResponse($"Row {rowIndex} has no expand affordance.");
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row, cancellationToken)
                .ConfigureAwait(false);
        }

        private BreadcrumbStateRow RowAt(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _model.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rowIndex),
                    rowIndex,
                    $"Row index must be in [0, {_model.Rows.Count - 1}]."
                );
            }
            return _model.Rows[rowIndex];
        }

        private void ReplaceRowsPreservingIdentity(IReadOnlyList<BreadcrumbStateRow> rows)
        {
            string? selectedIdentity = _model.SelectedRow?.Identity;
            _model.ReplaceRows(rows);
            if (selectedIdentity == null)
            {
                return;
            }
            for (int index = 0; index < _model.Rows.Count; index++)
            {
                if (
                    string.Equals(
                        _model.Rows[index].Identity,
                        selectedIdentity,
                        StringComparison.Ordinal
                    )
                )
                {
                    _model.SelectRow(index);
                    return;
                }
            }
        }

        private static IReadOnlyList<string> ErrorResponse(string message)
        {
            return new[] { BreadcrumbBridgeSerializer.Serialize(new BridgeErrorMessage(message)) };
        }
    }
}
