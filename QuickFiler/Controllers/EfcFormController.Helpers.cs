using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal partial class EfcFormController
    {
        #region Helper Methods

        /// <summary>
        /// Issue #736 finding 2: the single containment point for the keyboard-dispatch path. Both
        /// <c>KbdExecuteAsync</c> overloads route their two statements through this member, so the
        /// handling is written once rather than duplicated, and a fault raised by the
        /// keyboard-dialog toggle is covered as well as one raised by the dispatched action.
        /// </summary>
        internal async Task RunKbdGuardedAsync(System.Func<Task> body)
        {
            try
            {
                await body();
            }
            catch (OperationCanceledException)
            {
                // Cancellation is not a fault, so it is recorded at debug level and deliberately
                // not reported through the sink, matching the existing distinction in
                // BindBreadcrumbRowsAsync.
                logger.Debug("Keyboard dispatch canceled.");
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault($"Keyboard dispatch failed: {ex.Message}", ex);
            }
        }

        public async Task KbdExecuteAsync(Func<Task> action)
        {
            await RunKbdGuardedAsync(async () =>
            {
                await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
                await action();
            });
        }

        public async Task KbdExecuteAsync(System.Action action)
        {
            await RunKbdGuardedAsync(async () =>
            {
                await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
                action();
            });
        }

        internal async Task JumpToAsync(Control control)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            //await _formViewer.UiSyncContext;
            control.Focus();
        }

        public void MaximizeFormViewer()
        {
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        public void MinimizeFormViewer()
        {
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }

        internal void ShowMenu(ToolStripMenuItem menu) => menu.ShowDropDown();

        public async Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            checkBox.Checked = !checkBox.Checked;
        }

        public void ToggleOffNavigation(bool async)
        {
            CharacterActions.Keys.ForEach(key =>
                _homeController.KeyboardHandler.CharActions.Remove("Controller", key)
            );
            ToggleTips(async, Enums.ToggleState.Off);
            _itemController.ToggleNavigation(async, Enums.ToggleState.Off);
        }

        public async Task ToggleOffNavigationAsync()
        {
            CharacterAsyncActions.Keys.ForEach(key =>
                _homeController.KeyboardHandler.CharActionsAsync.Remove("Controller", key)
            );
            await ToggleTipsAsync(Enums.ToggleState.Off);
            await _itemController.ToggleNavigationAsync(Enums.ToggleState.Off);
        }

        public void ToggleOnNavigation(bool async)
        {
            CharacterActions.ForEach(x => _homeController.KeyboardHandler.CharActions.Add(x));
            ToggleTips(async, Enums.ToggleState.On);
            _itemController.ToggleNavigation(async, Enums.ToggleState.On);
        }

        public async Task ToggleOnNavigationAsync()
        {
            CharacterAsyncActions.ForEach(x =>
                _homeController.KeyboardHandler.CharActionsAsync.Add(x)
            );
            await ToggleTipsAsync(Enums.ToggleState.On);
            await _itemController.ToggleNavigationAsync(Enums.ToggleState.On);
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async)
                {
                    _formViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(true)));
                }
                else
                {
                    _formViewer.Invoke(new System.Action(() => tipsDetails.Toggle(true)));
                }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async)
                {
                    _formViewer.BeginInvoke(
                        new System.Action(() => tipsDetails.Toggle(desiredState, true))
                    );
                }
                else
                {
                    _formViewer.Invoke(
                        new System.Action(() => tipsDetails.Toggle(desiredState, true))
                    );
                }
            }
        }

        public async Task ToggleTipsAsync(Enums.ToggleState desiredState)
        {
            Token.ThrowIfCancellationRequested();

            // Attempt to remove blocking await code and start all tasks simultaneously.
            var tasks = _listTipsDetails
                .Select(x => x.ToggleAsync(desiredState, shareColumn: true))
                .ToList();
            // TODO: Check if this creates a deadlock
            await Task.WhenAll(tasks);

            // Original async code
            //foreach (var tip in _listTipsDetails)
            //{
            //    await tip.ToggleAsync(desiredState, shareColumn: true);
            //}
        }

        internal void LoadUserSettings()
        {
            _saveAttachments = Settings.Default.SaveAttachments;
            _formViewer.SaveAttachmentsMenuItem.Checked = _saveAttachments;

            _saveEmail = Settings.Default.SaveEmail;
            _formViewer.SaveEmailMenuItem.Checked = _saveEmail;

            _savePictures = Settings.Default.SavePictures;
            _formViewer.SavePicturesMenuItem.Checked = _savePictures;

            _moveConversation = Settings.Default.MoveConversation;
            _formViewer.ConversationMenuItem.Checked = _moveConversation;
        }

        /// <summary>#464 C: both call sites discard the result, so the boundary is here.</summary>
        public async Task PopulateFolderCombobox(object folderList = null)
        {
            try
            {
                // Capture _formViewer in a local variable before the first await. Cleanup() may set
                // _formViewer to null while InitFolderHandlerAsync is executing (e.g. the user
                // dismisses the form), so all post-await access must go through this local reference.
                var formViewer = _formViewer;
                if (formViewer == null)
                    return;

                await _dataModel.InitFolderHandlerAsync(folderList);

                await formViewer.UiSyncContext;

                BindSourceFolderRows(_dataModel.FolderHelper.FolderArray);
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        // #465 D (RC7): single classification owner. StartsWith, never Substring, over the
        internal static bool IsBannerRow(string row) =>
            row is not null
            && row.StartsWith(
                UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix,
                StringComparison.Ordinal
            );

        // The rest of IsValidSelection's pure logic, routed through the owner above.
        internal static bool IsSelectableFolder(string selectedFolder) =>
            !IsBannerRow(selectedFolder)
            && EfcSelectionGuard.IsValidCreationSelection(selectedFolder);

        internal bool IsValidSelection => IsSelectableFolder(SelectedFolder);

        #endregion

        public void ToggleExpansionStyle(Enums.ToggleState desiredState)
        {
            if (desiredState == Enums.ToggleState.On)
            {
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightExpanded;
                _formViewer.MinimumSize = new Size(
                    _formViewer.MinimumSize.Width,
                    _formViewer.MinimumSize.Height + _tlpHeightDiff
                );
                _formViewer.Size = new Size(
                    _formViewer.Size.Width,
                    _formViewer.Size.Height + _tlpHeightDiff
                );
                _formViewer.WindowState = FormWindowState.Maximized;
            }
            else
            {
                _formViewer.WindowState = FormWindowState.Normal;
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightCollapsed;
                _formViewer.MinimumSize = new Size(
                    _formViewer.MinimumSize.Width,
                    _formViewer.MinimumSize.Height - _tlpHeightDiff
                );
                _formViewer.Size = new Size(
                    _formViewer.Size.Width,
                    _formViewer.Size.Height - _tlpHeightDiff
                );
            }
        }
    }
}
