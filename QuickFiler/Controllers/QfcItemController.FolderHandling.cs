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
                // Intent-member equivalent of PopulateAndSelectFolder (Seam C): predetermined
                // high-confidence folder is preselected when present; otherwise the index-1 top
                // suggestion is selected. The standalone static PopulateAndSelectFolder is retained
                // unchanged for its existing unit tests.
                _itemViewer.SetFolderItems(_folderHandler.FolderArray);

                // #325: additionally hand the row model (folder identity + prediction probability)
                // to the tree/percentage population path. Sourced verbatim from the #324 contract
                // FolderPredictor.FolderRowArray; scores are not recomputed here. The Suggestions
                // guard avoids evaluating the row-model getter on an under-initialized predictor
                // (production predictors always have a scorer).
                if (_folderHandler.Suggestions != null)
                {
                    _itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray);
                }
                if (
                    !string.IsNullOrEmpty(_predeterminedFolder)
                    && _itemViewer.FolderContains(_predeterminedFolder)
                )
                {
                    _itemViewer.SetFolderSelectedItem(_predeterminedFolder);
                }
                else
                {
                    _itemViewer.SetFolderSelectedIndex(1);
                }
                _selectedFolder = _itemViewer.GetSelectedFolder();
            }
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
            comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : 1;
            return comboBox.SelectedItem as string;
        }
    }
}
