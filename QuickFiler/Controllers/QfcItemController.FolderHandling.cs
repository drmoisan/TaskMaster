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
        // Residual (bucket-iii): constructs a FolderPredictor (COM/Outlook-bound folder analysis, an
        // out-of-scope collaborator with no seam this cycle). Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal void LoadFolderHandler(object varList = null)
        {
            if (varList is null)
            {
                _folderHandler = new FolderPredictor(
                    _globals,
                    ItemHelper,
                    FolderPredictor.InitOptions.FromField
                );
            }
            else
            {
                _folderHandler = new FolderPredictor(
                    _globals,
                    varList,
                    FolderPredictor.InitOptions.FromArrayOrString
                );
            }
        }

        // Residual (bucket-iii): async counterpart of LoadFolderHandler; constructs the COM-bound
        // FolderPredictor (out-of-scope collaborator, no seam this cycle). Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
                                var fp = new FolderPredictor(
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
                }
                catch (ArgumentNullException e)
                {
                    logger.Error(e.Message);
                    logger.Debug("Loading empty folder handler");
                    try
                    {
                        _folderHandler = new FolderPredictor(_globals);
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
                            var fp = new FolderPredictor(
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
            }
        }

        // Residual (bucket-iii): calls LoadFolderHandler (COM-bound FolderPredictor) before the
        // mockable IItemViewer combo population; barrier inherited from LoadFolderHandler.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        // Residual (bucket-iii): calls LoadFolderHandlerAsync (COM-bound FolderPredictor); barrier
        // inherited from the folder-prediction step. Not unit-reachable.
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
