#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookObjects.Folder
{
    public sealed partial class FolderBreadcrumbBridgeRouter
    {
        /// <summary>Commits a host-known descendant through the existing selector transition.</summary>
        public BreadcrumbSelectionTransition ActivateSelectorDescendant(
            string rowIdentity,
            string nodeIdentity
        ) => Mutate(() => _selectionSession.ActivateDescendant(rowIdentity, nodeIdentity));

        /// <summary>Expands or collapses one known descendant without selecting it.</summary>
        public async Task<IReadOnlyList<string>> ToggleDescendantAsync(
            string rowIdentity,
            string nodeIdentity,
            CancellationToken cancellationToken
        )
        {
            BreadcrumbStateRow? row;
            BreadcrumbDescendantNode? node;
            int rowIndex;
            lock (_sync)
            {
                row = _model.Rows.FirstOrDefault(candidate => candidate.Identity == rowIdentity);
                node = row?.DescendantRoot?.Find(nodeIdentity);
                if (
                    node == null
                    || node == row!.DescendantRoot
                    || !node.Segment.HasChildren
                    || !BreadcrumbSelectionMap.TryDescendantValue(_model, node, out _)
                )
                {
                    return Array.Empty<string>();
                }
                rowIndex = _model.Rows.ToList().IndexOf(row);
                if (node.Expanded)
                {
                    node.Collapse();
                    // #1007 AC7: a pending node hidden by this collapse moves to its nearest visible ancestor.
                    _selectionSession.ReconcilePendingNode();
                    return new[] { RenderJsonCore() };
                }
                node.Expanded = true;
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row!, cancellationToken, node)
                .ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<string>> ToggleAsync(
            int rowIndex,
            CancellationToken cancellationToken
        )
        {
            BreadcrumbStateRow row;
            lock (_sync)
            {
                row = RowAt(rowIndex);
                // #1007 R3 (D-Q23): a row shortened at its active segment lists that segment's
                // children and stays shortened; only a collapse anchored elsewhere is restored.
                if (
                    row.CollapsedAfterIndex != null
                    && row.CollapsedAfterIndex != row.ActiveSegmentIndex
                )
                {
                    row.ReExpand();
                    return new[] { RenderJsonCore() };
                }
                if (row.LeafExpanded)
                {
                    row.TryCollapseLeaf();
                    _selectionSession.ReconcilePendingNode();
                    return new[] { RenderJsonCore() };
                }
                if (!row.TryExpandActiveSegment())
                {
                    return ErrorResponse($"Row {rowIndex} has no expand affordance.");
                }
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<string>> ArrowAsync(
            BreadcrumbArrowDirection direction,
            CancellationToken cancellationToken
        )
        {
            if (Read(() => _selectionSession.IsOpen))
            {
                // #1007: with the list open, Left and Right act on the pending entry.
                return await PendingArrowAsync(direction, cancellationToken).ConfigureAwait(false);
            }
            BreadcrumbStateRow row;
            int rowIndex;
            lock (_sync)
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
                row = _model.SelectedRow!;
                rowIndex = _model.SelectedIndex;
                if (!row.LeafExpanded || row.Subfolders.Count != 0)
                {
                    return new[] { RenderJsonCore() };
                }
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<string>> FetchAndAttachSubfoldersAsync(
            int rowIndex,
            BreadcrumbStateRow row,
            CancellationToken cancellationToken,
            BreadcrumbDescendantNode? descendant = null
        )
        {
            BreadcrumbDescendantNode node;
            int population;
            int request;
            lock (_sync)
            {
                node = descendant ?? row.DescendantRoot!;
                population = _suggestionGeneration;
                request = ++node.RequestGeneration;
            }
            IReadOnlyList<FolderBreadcrumbSegment> children;
            try
            {
                children = await _provider
                    .GetImmediateSubfoldersAsync(node.Segment.Key, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lock (_sync)
                {
                    if (IsCurrentRequest(rowIndex, row, node, population, request))
                    {
                        node.Collapse();
                        if (descendant == null)
                        {
                            row.TryCollapseLeaf();
                        }
                    }
                }
                if (ex is OperationCanceledException)
                {
                    throw;
                }
                return ErrorResponse($"Subfolder query failed: {ex.Message}");
            }
            lock (_sync)
            {
                if (!IsCurrentRequest(rowIndex, row, node, population, request))
                {
                    return new[] { RenderJsonCore() };
                }
                if (descendant == null)
                {
                    row.SetSubfolders(children);
                }
                else
                {
                    node.Attach(children ?? Array.Empty<FolderBreadcrumbSegment>());
                }
                string render = RenderJsonCore();
                if (descendant != null)
                {
                    return new[] { render };
                }
                return new[]
                {
                    render,
                    BreadcrumbBridgeSerializer.Serialize(
                        new SubfolderResponseMessage(
                            rowIndex,
                            BreadcrumbRenderProjection.Project(_model)[rowIndex].Subfolders
                        )
                    ),
                };
            }
        }

        private bool IsCurrentRequest(
            int rowIndex,
            BreadcrumbStateRow row,
            BreadcrumbDescendantNode node,
            int population,
            int request
        ) =>
            population == _suggestionGeneration
            && rowIndex < _model.Rows.Count
            && ReferenceEquals(_model.Rows[rowIndex], row)
            && row.LeafExpanded
            && row.DescendantRoot?.Find(node.Identity) == node
            && node.RequestGeneration == request
            && node.Expanded;

        private async Task<IReadOnlyList<string>> SubfolderResponseAsync(
            int rowIndex,
            CancellationToken cancellationToken
        )
        {
            BreadcrumbStateRow row;
            lock (_sync)
            {
                row = RowAt(rowIndex);
                if (!row.IsSuggestion)
                {
                    return ErrorResponse(
                        $"Row {rowIndex} is a plain row without a subfolder query."
                    );
                }
                if (!row.LeafExpanded && !row.TryExpandLeaf())
                {
                    return ErrorResponse($"Row {rowIndex} has no expand affordance.");
                }
            }
            return await FetchAndAttachSubfoldersAsync(rowIndex, row, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
