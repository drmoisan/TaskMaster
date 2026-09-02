#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Selection, chain-fetch and outbound-delivery members of the breadcrumb bridge router,
    /// separated from <c>BreadcrumbBridgeRouter.cs</c> so that each part of the type remains below
    /// the 500-line file limit. Selection methods enforce the archive-relative filing contract.
    /// </summary>
    public sealed partial class BreadcrumbBridgeRouter
    {
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

            string selection =
                row.Kind == BreadcrumbRowKind.TrashPseudoRow
                    ? BreadcrumbRowBuilder.TrashRowText
                    : row.FilingTarget;
            // #614 D2: normalize eligible rooted targets, preserving no-bound-root pass-through.
            if (_boundRoot.Length != 0 && ArchiveStemContract.IsFullOutlookPath(selection))
            {
                if (
                    !ArchiveStemContract.TryMakeArchiveRelative(
                        selection,
                        _boundRoot,
                        out string stem
                    )
                )
                {
                    log.Error("Breadcrumb row rejected: target is outside the archive root.");
                    return;
                }

                if (stem.Length == 0)
                {
                    log.Error("Breadcrumb row rejected: target is the archive root itself.");
                    return;
                }

                selection = stem;
            }

            CommitSelection(row, selection);
        }

        private void SelectHierarchyPath(BreadcrumbRow row, string fullPath)
        {
            if (_boundRoot.Length == 0)
            {
                CommitSelection(row, fullPath); // Preserved no-archive-root binding mode.
                return;
            }

            // #614 D1/D9: a path outside the archive root, and the root itself, are deterministic
            // non-selections; the prior selection stays unchanged and is never nulled (#499).
            if (
                !ArchiveStemContract.TryMakeArchiveRelative(fullPath, _boundRoot, out string stem)
                || stem.Length == 0
            )
            {
                log.Error("Breadcrumb selection rejected: not a folder inside the archive root.");
                return;
            }

            CommitSelection(row, stem);
        }

        private void CommitSelection(BreadcrumbRow row, string selection)
        {
            _selectedRowId = row.RowId;
            SelectedFolderPath = selection;
            PostOutbound(
                new BreadcrumbRenderMessage(_renderer.RenderRows(_rows, _selectedRowId), null)
            );
            SelectedFolderPathChanged?.Invoke(this, SelectedFolderPath);
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
