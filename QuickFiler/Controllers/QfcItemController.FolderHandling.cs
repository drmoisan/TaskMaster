using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        internal void LoadFolderHandler(object varList = null)
        {
            if (varList is null)
            {
                _folderHandler = _folderPredictorFactory(
                    _globals,
                    ItemHelper,
                    FolderPredictor.InitOptions.FromField
                );
                logger.Debug(
                    $"Probability debug [QfcItemController.LoadFolderHandler (FromField)] "
                        + $"Subject='{ItemHelper?.Subject}' EntryID='{ItemHelper?.EntryId}' "
                        + $"TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}"
                );
            }
            else
            {
                _folderHandler = _folderPredictorFactory(
                    _globals,
                    varList,
                    FolderPredictor.InitOptions.FromArrayOrString
                );
                logger.Debug(
                    $"Probability debug [QfcItemController.LoadFolderHandler (FromArrayOrString)] "
                        + $"Subject='{ItemHelper?.Subject}' EntryID='{ItemHelper?.EntryId}' "
                        + $"TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}"
                );
            }
        }

        public async Task LoadFolderHandlerAsync(CancellationToken cancel, object varList = null)
        {
            //TraceUtility.LogMethodCall(varList);
            if (varList is null)
            {
                // #678: an item that arrived from the dequeue-time confidence gate already carries a
                // fully initialised handler for THIS item, scored with the same
                // FolderPredictor.InitOptions.FromField sequence this branch would run. Adopting it
                // is what removes the second scoring pass. The adoption is confined to this branch:
                // the FromArrayOrString branch below is a search over a caller-supplied list, not a
                // per-item scoring pass, so a carried handler is never valid there.
                if (_carriedFolderHandler is not null)
                {
                    // #678 R3: the cancellation observation sits INSIDE this branch rather than at
                    // the top of the member. Every pre-change route reached the predictor through
                    // await Task.Run(..., cancel) below, inside the try that follows this branch,
                    // so an already-cancelled token surfaced as an OperationCanceledException that
                    // the catch (System.Exception e) logged through logger.Error before rethrowing.
                    // Hoisting the throw to the top of the member would place it before that try
                    // and silently remove that logger.Error for the FromField route, which is a
                    // second behaviour change this remediation is not authorised to make.
                    cancel.ThrowIfCancellationRequested();
                    _folderHandler = _carriedFolderHandler;
                    logger.Debug(
                        $"Probability debug [QfcItemController.LoadFolderHandlerAsync (carried)] "
                            + $"Subject='{ItemHelper?.Subject}' EntryID='{ItemHelper?.EntryId}' "
                            + $"TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}"
                    );
                    return;
                }

                try
                {
                    _folderHandler = await Task.Run(
                            async () =>
                            {
                                var fp = _folderPredictorFactory(
                                    _globals,
                                    ItemHelper.ThrowIfNull(),
                                    FolderPredictor.InitOptions.FromField
                                );

                                return await fp.InitAsync(
                                    ItemHelper,
                                    FolderPredictor.InitOptions.FromField
                                );
                            },
                            cancel
                        )
                        .ConfigureAwait(false);
                    logger.Debug(
                        $"Probability debug [QfcItemController.LoadFolderHandlerAsync (FromField)] "
                            + $"Subject='{ItemHelper?.Subject}' EntryID='{ItemHelper?.EntryId}' "
                            + $"TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}"
                    );
                }
                catch (ArgumentNullException e)
                {
                    logger.Error(e.Message);
                    logger.Debug("Loading empty folder handler");
                    try
                    {
                        _folderHandler = _folderPredictorEmptyFactory(_globals);
                    }
                    catch (System.Exception e2)
                    {
                        logger.Error(e2.Message, e);
                        throw;
                    }
                }
                catch (System.Exception e)
                {
                    logger.Error(e.Message, e);
                    throw;
                }
            }
            else
            {
                _folderHandler = await Task.Run(
                        async () =>
                        {
                            var fp = _folderPredictorFactory(
                                _globals,
                                varList,
                                FolderPredictor.InitOptions.FromArrayOrString
                            );
                            return await fp.InitAsync(
                                varList,
                                FolderPredictor.InitOptions.FromArrayOrString
                            );
                        },
                        cancel
                    )
                    .ConfigureAwait(false);
                logger.Debug(
                    $"Probability debug [QfcItemController.LoadFolderHandlerAsync (FromArrayOrString)] "
                        + $"Subject='{ItemHelper?.Subject}' EntryID='{ItemHelper?.EntryId}' "
                        + $"TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}"
                );
            }
        }

        // #677: the deactivate fan-out hop. Forwards the cancel intent to the narrowed viewer seam;
        // a released or not-yet-attached viewer makes this a no-op rather than a failure.
        public void CancelBreadcrumbSelector() => _itemViewer?.CancelBreadcrumbSelector();

        public void PopulateFolderComboBox(object varList = null)
        {
            //TraceUtility.LogMethodCall(varList);

            LoadFolderHandler(varList);

            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => AssignFolderComboBox());
            }
            else
            {
                AssignFolderComboBox();
            }
        }

        public async Task PopulateFolderComboBoxAsync(
            CancellationToken token,
            object varList = null
        )
        {
            //TraceUtility.LogMethodCall(token, varList);
            token.ThrowIfCancellationRequested();

            await Task.Run(() => LoadFolderHandlerAsync(token, varList), token);
            await _itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox);
        }

        public void AssignFolderComboBox()
        {
            //TraceUtility.LogMethodCall();
            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => AssignFolderComboBox());
                return;
            }

            if (_folderHandler?.FolderArray?.Length > 0)
            {
                // #351: the breadcrumb pipeline (injected 9101 provider behind the coordinator)
                // must exist before population so ancestor chains come from
                // IFolderHierarchyProvider.GetAncestorChainAsync instead of the decommissioned
                // FolderHierarchyBuilder.Build (AC-5); no-op once initialized.
                EnsureBreadcrumbPipeline();

                // Intent-member equivalent of PopulateAndSelectFolder (Seam C): predetermined
                // high-confidence folder is preselected when present; otherwise the index-1 top
                // suggestion is selected. The standalone static PopulateAndSelectFolder is retained
                // unchanged for its existing unit tests.
                _itemViewer.AddFolderItems(_folderHandler.FolderArray);

                // #325: additionally hand the row model (folder identity + prediction probability)
                // to the tree/percentage population path. Sourced verbatim from the #324 contract
                // FolderPredictor.FolderRowArray; scores are not recomputed here. The Suggestions
                // guard avoids evaluating the row-model getter on an under-initialized predictor
                // (production predictors always have a scorer).
                if (_folderHandler.Suggestions != null)
                {
                    _itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray);
                }
                // #678 AC12: FolderArray entries are archive-prefix-stripped by
                // FolderPredictor.ProjectSuggestionPath, while the carried PredeterminedFolder is
                // the RAW suggestion path the scorer read from Suggestions. Without projecting the
                // carried value the same way, FolderContains misses every archive-rooted
                // suggestion and the selection silently falls back to the index-1 entry. The
                // projection is duplicated here rather than reused because
                // FolderPredictor.ProjectSuggestionPath is private and lives under UtilitiesCS,
                // which this change may not modify.
                string predetermined = ProjectPredeterminedFolder(
                    _predeterminedFolder,
                    _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)
                );
                if (
                    !string.IsNullOrEmpty(predetermined)
                    && _itemViewer.FolderContains(predetermined)
                )
                {
                    _itemViewer.SetFolderSelectedItem(predetermined);
                }
                else
                {
                    _itemViewer.SetFolderSelectedIndex(
                        _folderHandler.FolderArray.Length == 1 ? 0 : 1
                    );
                }
                _selectedFolder = _itemViewer.GetSelectedFolder();
            }
        }

        /// <summary>
        /// #678 AC12. Projects a raw suggestion path onto the form <c>FolderPredictor.FolderArray</c>
        /// stores, so a containment probe against the combo box can match: strip
        /// <paramref name="archiveRootPath"/> plus a trailing separator from the front of
        /// <paramref name="folderPath"/>, case-insensitively, but only when the remainder is
        /// non-empty. #678 R2: the projection mirrors <c>FolderPredictor.ProjectSuggestionPath</c>
        /// for every non-null <paramref name="folderPath"/> and non-null
        /// <paramref name="archiveRootPath"/>. A NULL <paramref name="archiveRootPath"/> stands for
        /// that member's <c>_globals is null</c> guard and yields the identity; an EMPTY one does
        /// not, because that member forms its prefix unconditionally and so strips a single leading
        /// separator in that state.
        ///
        /// Two divergences from that member remain and are deliberate, and both are null-safety
        /// differences rather than projection differences. First, a null or empty
        /// <paramref name="folderPath"/> is returned unchanged rather than dereferenced;
        /// <c>ProjectSuggestionPath</c> does not guard it because its input comes from
        /// <c>Suggestions</c>. Second, a non-null globals with a null <c>Ol</c> is treated by the
        /// call site as an empty archive root rather than reproducing that member's null
        /// dereference.
        /// </summary>
        internal static string ProjectPredeterminedFolder(string folderPath, string archiveRootPath)
        {
            if (string.IsNullOrEmpty(folderPath) || archiveRootPath is null)
            {
                return folderPath;
            }

            string archivePrefix = archiveRootPath + "\\";
            return
                folderPath.StartsWith(archivePrefix, StringComparison.OrdinalIgnoreCase)
                && folderPath.Length > archivePrefix.Length
                ? folderPath.Substring(archivePrefix.Length)
                : folderPath;
        }

        /// <summary>
        /// Populates <paramref name="comboBox"/> with <paramref name="folderArray"/> and selects the
        /// folder to display. High-confidence mode (Issue #171): when
        /// <paramref name="predeterminedFolder"/> is non-empty and present in the combo box, that
        /// folder is preselected; otherwise the existing index-1 behavior (the top suggestion) is
        /// used. Pure WinForms-only logic with no <c>InvokeRequired</c> marshaling, so it is unit
        /// testable without a fully-constructed item viewer.
        /// </summary>
        /// <returns>The selected folder text, or null when nothing is selected.</returns>
        internal static string PopulateAndSelectFolder(
            System.Windows.Forms.ComboBox comboBox,
            string[] folderArray,
            string predeterminedFolder
        )
        {
            comboBox.Items.AddRange(folderArray);

            int predeterminedIndex = string.IsNullOrEmpty(predeterminedFolder)
                ? -1
                : comboBox.Items.IndexOf(predeterminedFolder);
            comboBox.SelectedIndex =
                predeterminedIndex >= 0 ? predeterminedIndex : (folderArray.Length == 1 ? 0 : 1);
            return comboBox.SelectedItem as string;
        }
    }
}
