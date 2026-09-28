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
        #region Major Actions

        async public Task ActionOkAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            var selectedFolder = SelectedFolder;
            // Classifies through the single owner and retains #614's rooted-path rejection.
            if (
                selectedFolder is null
                || IsBannerRow(selectedFolder)
                || !EfcSelectionGuard.IsValidFilingSelection(selectedFolder)
            )
            {
                MessageBox.Show("Please select a valid folder.");
                return;
            }
            else
            {
                _formViewer.Hide();
                if (_initType.HasFlag(QfEnums.InitTypeEnum.Sort))
                {
                    await _homeController.ExecuteMovesAsync();
                }
                else if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
                {
                    await _homeController.OpenOlFolderAsync(SelectedFolder);
                }
                else
                {
                    throw new NotImplementedException();
                }
                _formViewer.Dispose();
                Cleanup();
            }
        }

        public async Task ActionCancelAsync()
        {
            //Debug.WriteLine($"Thread Id before await: {Thread.CurrentThread.ManagedThreadId}");
            await _formViewer.UiSyncContext;
            //Debug.WriteLine($"Thread Id after await: {Thread.CurrentThread.ManagedThreadId}");
            _formViewer.Close();
            Cleanup();
        }

        /// <summary>The pseudo-row that marks the delete target.</summary>
        internal const string TrashRowText = "Trash to Delete";

        /// <summary>Prepends the trash pseudo-row, idempotently.</summary>
        internal static string[] WithTrashRow(string[] rows)
        {
            if (rows is null)
            {
                return new[] { TrashRowText };
            }
            if (rows.Length > 0 && rows[0] == TrashRowText)
            {
                return rows;
            }
            var itemList = rows.ToList();
            itemList.Insert(0, TrashRowText);
            return itemList.ToArray();
        }

        /// <summary>Retains the delete-gesture rows, then binds them.</summary>
        internal void ApplyDeleteGesture()
        {
            _folderRows = WithTrashRow(_folderRows);
            BindFolderRows(_folderRows);
        }

        public async Task ActionDeleteAsync()
        {
            await _formViewer.UiSyncContext;
            ApplyDeleteGesture();
        }

        public async Task CreateFolderAsync()
        {
            if (!IsValidSelection)
            {
                MessageBox.Show("Please select a valid folder");
            }
            else if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                await _homeController.OpenFsFolderAsync(SelectedFolder);
            }
            else
            {
                await _formViewer.UiSyncContext;
                _formViewer.Hide();
                if (!_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive))
                {
                    return;
                }
                var folder = await Task.FromResult(
                        _dataModel.FolderHelper.CreateFolder(
                            SelectedFolder,
                            _globals.Ol.ArchiveRootPath,
                            oneDrive
                        )
                    )
                    .ConfigureAwait(false);
                if (folder is not null)
                {
                    await _dataModel
                        .MoveToFolderAsync(
                            folder,
                            _globals.Ol.ArchiveRootPath,
                            SaveAttachments,
                            SaveEmail,
                            SavePictures,
                            MoveConversation
                        )
                        .ConfigureAwait(false);
                    await _formViewer.UiSyncContext;
                    _formViewer.Dispose();
                    Cleanup();
                }
            }
        }

        /// <summary>Applies a match delegate to a search string; never returns null.</summary>
        internal static string[] MatchesForSearchText(
            System.Func<string, string[]> findMatches,
            string searchText
        )
        {
            if (findMatches is null)
            {
                return Array.Empty<string>();
            }
            return findMatches(searchText ?? string.Empty) ?? Array.Empty<string>();
        }

        /// <summary>
        /// #465 B (RC8): the control read happens here, on the UI thread, before any
        /// <c>Task.Run</c>, carrying an unchanged value into the worker.
        /// </summary>
        public async Task RefreshSuggestionsAsync()
        {
            var searchText = _formViewer.SearchText.Text;

            await Task.Run(() => _dataModel.RefreshSuggestions(), Token);
            var matches = await Task.Run(
                () => MatchesForSearchText(_dataModel.FindMatches, searchText),
                Token
            );

            BindSourceFolderRows(matches);
        }

        #endregion
    }
}
