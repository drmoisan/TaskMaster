#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>Host-neutral breadcrumb message router and population boundary.</summary>
    public sealed partial class FolderBreadcrumbBridgeRouter
    {
        private readonly IFolderHierarchyProvider _provider;
        private readonly BreadcrumbStateModel _model = new BreadcrumbStateModel();
        private readonly BreadcrumbSelectionSession _selectionSession;
        private readonly object _sync = new object();
        private int _suggestionGeneration;

        /// <summary>Creates a router over the injected hierarchy provider.</summary>
        public FolderBreadcrumbBridgeRouter(IFolderHierarchyProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _selectionSession = new BreadcrumbSelectionSession(_model);
        }

        /// <summary>Test-visible mutable model; production callers use router snapshots.</summary>
        internal BreadcrumbStateModel Model => _model;

        /// <summary>Resolves scored rows to chains, retaining exact-path fallbacks.</summary>
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

            // Resolve locally, then atomically swap, so provider awaits expose no partial model.
            var built = new List<BreadcrumbStateRow>(rows.Count);
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                FolderRow row = rows[rowIndex];
                BreadcrumbStateRow fallback = CreateFallbackRow(row, rowIndex);
                if (row.Score.HasValue)
                {
                    string path = row.Score.Value.FolderPath;
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
                                // Decision D7: the row still files into the presented path.
                                ? new BreadcrumbStateRow(
                                    fallback.Identity,
                                    chain,
                                    row.Score.Value.Probability,
                                    path
                                )
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
                    built.Add(fallback);
                }
            }

            lock (_sync)
            {
                if (generation != _suggestionGeneration)
                {
                    return RenderJsonCore();
                }
                ReplaceRowsPreservingSession(built);
                return RenderJsonCore();
            }
        }

        /// <summary>Publishes scored fallbacks before asynchronous hierarchy decoration.</summary>
        public string SetSuggestionFallbacks(IReadOnlyList<FolderRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            Interlocked.Increment(ref _suggestionGeneration);
            var built = new List<BreadcrumbStateRow>(rows.Count);
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                built.Add(CreateFallbackRow(rows[rowIndex], rowIndex));
            }

            lock (_sync)
            {
                ReplaceRowsPreservingSession(built);
                return RenderJsonCore();
            }
        }

        /// <summary>Replaces the model with exact Path B plain-row values.</summary>
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
                AddPlainRows(items);
                _selectionSession.SynchronizeCommittedSelection();
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
                AddPlainRows(items);
                _selectionSession.SynchronizeCommittedSelection();
                return RenderJsonCore();
            }
        }

        private void AddPlainRows(IReadOnlyList<string> items)
        {
            int firstOccurrence = _model.Rows.Count;
            for (int index = 0; index < items.Count; index++)
            {
                string item = items[index];
                _model.AddPlainRow(
                    BreadcrumbRowIdentity.ForPlainRow(item, firstOccurrence + index),
                    item,
                    !BreadcrumbStateRow.IsBanner(item)
                );
            }
        }

        /// <summary>Clears rows and closes any open selector session.</summary>
        public BreadcrumbSelectionTransition Clear()
        {
            Interlocked.Increment(ref _suggestionGeneration);
            return Mutate(_selectionSession.ClearSelector);
        }

        /// <summary>Selects one row and synchronizes committed selector state.</summary>
        public BreadcrumbSelectionTransition SelectRow(int index) =>
            Mutate(() => _selectionSession.SelectRow(index));

        /// <summary>Selects the first row whose output equals <paramref name="item"/>.</summary>
        public BreadcrumbSelectionTransition SelectItem(string item) =>
            Mutate(() => _selectionSession.SelectItem(item));

        public BreadcrumbSelectionTransition OpenSelector() =>
            Mutate(_selectionSession.OpenSelector);

        public BreadcrumbSelectionTransition MoveSelector(bool previous) =>
            Mutate(() => _selectionSession.MoveSelector(previous));

        public BreadcrumbSelectionTransition CommitSelector() =>
            Mutate(_selectionSession.CommitSelector);

        public BreadcrumbSelectionTransition ActivateSelector(string identity) =>
            Mutate(() => _selectionSession.ActivateSelector(identity));

        /// <summary>Atomically commits an expanded subfolder in the current selector session.</summary>
        /// <param name="rowIdentity">The unique stable identity of the containing row.</param>
        /// <param name="subfolderIndex">The zero-based expanded subfolder index.</param>
        /// <returns>
        /// A handled selection/open-state/render transition for a valid open-session activation;
        /// otherwise an unhandled no-op for a closed session, unknown or non-subfolder row, or
        /// invalid index.
        /// </returns>
        public BreadcrumbSelectionTransition ActivateSelectorSubfolder(
            string rowIdentity,
            int subfolderIndex
        ) => Mutate(() => _selectionSession.ActivateSubfolder(rowIdentity, subfolderIndex));

        public BreadcrumbSelectionTransition CancelSelector() =>
            Mutate(_selectionSession.CancelSelector);

        public BreadcrumbSelectorState GetSelectorState() => Read(_selectionSession.Snapshot);

        public string? GetSelectedFolder() =>
            Read(() => BreadcrumbSelectionMap.GetSelectedFolder(_model));

        public string[] GetFolderItems() =>
            Read(() => BreadcrumbSelectionMap.GetFolderItems(_model));

        public bool Contains(string item) =>
            Read(() => BreadcrumbSelectionMap.FolderContains(_model, item));

        /// <summary>The current render payload JSON (full re-render message, FR-6).</summary>
        public string RenderJson() => Read(RenderJsonCore);

        private string RenderJsonCore()
        {
            IReadOnlyList<BreadcrumbRowRender> rows = BreadcrumbRenderProjection.Project(_model);
            return BreadcrumbBridgeSerializer.Serialize(
                new RenderMessage(
                    rows,
                    _model.SelectedSubfolderIndex,
                    BreadcrumbSelectionMap.GetSelectedFolder(_model)
                )
            );
        }

        private BreadcrumbSelectionTransition Mutate(Func<BreadcrumbSelectionEffects> mutation)
        {
            lock (_sync)
            {
                return Transition(mutation());
            }
        }

        private static BreadcrumbStateRow CreateFallbackRow(FolderRow row, int index)
        {
            string identity = BreadcrumbRowIdentity.ForFolderRow(row, index);
            return row.Score.HasValue
                ? new BreadcrumbStateRow(
                    identity,
                    row.Score.Value.FolderPath,
                    row.Score.Value.Probability
                )
                : new BreadcrumbStateRow(identity, row.Text, row.Kind != FolderRowKind.Separator);
        }

        private T Read<T>(Func<T> reader)
        {
            lock (_sync)
            {
                return reader();
            }
        }

        private BreadcrumbSelectionTransition Transition(BreadcrumbSelectionEffects effects) =>
            new BreadcrumbSelectionTransition(
                HasEffect(effects, BreadcrumbSelectionEffects.Handled),
                HasEffect(effects, BreadcrumbSelectionEffects.SelectionChanged),
                HasEffect(effects, BreadcrumbSelectionEffects.OpenStateChanged),
                HasEffect(effects, BreadcrumbSelectionEffects.RenderRequired)
                    ? RenderJsonCore()
                    : null,
                _selectionSession.Snapshot()
            );

        private static bool HasEffect(
            BreadcrumbSelectionEffects effects,
            BreadcrumbSelectionEffects expected
        ) => (effects & expected) != 0;

        /// <summary>Routes one inbound message and returns ordered outbound JSON.</summary>
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
                            _selectionSession.SynchronizeCommittedSelection();
                        }
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

        private void ReplaceRowsPreservingSession(IReadOnlyList<BreadcrumbStateRow> rows)
        {
            _model.ReplaceRows(rows);
            _selectionSession.ReconcileRowsReplaced();
        }

        private static IReadOnlyList<string> ErrorResponse(string message)
        {
            return new[] { BreadcrumbBridgeSerializer.Serialize(new BridgeErrorMessage(message)) };
        }
    }
}
