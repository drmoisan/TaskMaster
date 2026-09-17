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
        #region Event Handlers

        internal void RegisterAlwaysOnAsyncKeyActions()
        {
            _formViewer.KeyboardHandler.AlwaysOnKeyActionsAsync = new KbdActions<
                Keys,
                KaKeyAsync,
                Func<Keys, Task>
            >(
                new List<KaKeyAsync>
                {
                    new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync()),
                }
            );
        }

        public void WireEventHandlers()
        {
            //_homeController.KeyboardHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
            //_homeController.KeyboardHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();

            _formViewer.ForAllControls(
                x =>
                {
                    x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                        _homeController.KeyboardHandler.KeyboardHandler_PreviewKeyDownAsync
                    );
                    x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                        _homeController.KeyboardHandler.KeyboardHandler_KeyDownAsync
                    );
                },
                new List<Control> { }
            );
            _formViewer.SaveAttachmentsMenuItem.CheckedChanged += SaveAttachments_CheckedChanged;
            _formViewer.SaveEmailMenuItem.CheckedChanged += SaveEmail_CheckedChanged;
            _formViewer.SavePicturesMenuItem.CheckedChanged += SavePictures_CheckedChanged;
            _formViewer.ConversationMenuItem.CheckedChanged += MoveConversation_CheckedChanged;
            _formViewer.Ok.Click += ButtonOK_Click;
            RegisterAlwaysOnAsyncKeyActions();
            ConfigureBreadcrumbControl();
            _formViewer.Cancel.Click += ButtonCancel_Click;
            _formViewer.RefreshPredicted.Click += ButtonRefresh_Click;
            _formViewer.NewFolder.Click += ButtonCreate_Click;
            _formViewer.BtnDelItem.Click += ButtonDelete_Click;
            _formViewer.SearchText.TextChanged += SearchText_TextChanged;
            _formViewer.SearchText.KeyDown += SearchText_DownArrow;
            _formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;
            _globals.Ol.PropertyChanged += DarkMode_Changed;
        }

        public void SearchText_DownArrow(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                // Enter the breadcrumb list and select its first row (parity with the prior
                // TreeListView down-arrow behavior); further key handling happens in-document.
                _formViewer.FolderListBox.Select();
                _router?.SelectFirstRow();
            }
        }

        public async void ButtonCancel_Click(object sender, EventArgs e) =>
            await ButtonCancelClickAsync();

        internal async Task ButtonCancelClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await ActionCancelAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonOK_Click(object sender, EventArgs e) => await ButtonOkClickAsync();

        internal async Task ButtonOkClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await ActionOkAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonRefresh_Click(object sender, EventArgs e) =>
            await ButtonRefreshClickAsync();

        internal async Task ButtonRefreshClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await RefreshSuggestionsAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonCreate_Click(object sender, EventArgs e) =>
            await ButtonCreateClickAsync();

        internal async Task ButtonCreateClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                if (!IsValidSelection)
                {
                    MessageBox.Show(
                        "Please select a valid parent folder where you would like to place the new folder."
                    );
                }
                else if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
                {
                    await _homeController.OpenFsFolderAsync(SelectedFolder);

                    _formViewer.Close();
                    Cleanup();
                }
                else
                {
                    if (!_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
                    {
                        logger.Debug($"Cannot create folder without OneDrive location");
                        return;
                    }
                    var folder =
                        (
                            await _dataModel.FolderHelper.CreateFolderAsync(
                                SelectedFolder,
                                _globals.Ol.ArchiveRootPath,
                                folderRoot,
                                Token
                            )
                        ) as MAPIFolder;

                    if (folder is not null)
                    {
                        await _dataModel.MoveToFolderAsync(
                            folder,
                            _globals.Ol.ArchiveRootPath,
                            SaveAttachments,
                            SaveEmail,
                            SavePictures,
                            MoveConversation
                        );

                        _formViewer.Close();
                        Cleanup();
                    }
                }
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonDelete_Click(object sender, EventArgs e) =>
            await ButtonDeleteClickAsync();

        internal async Task ButtonDeleteClickAsync()
        {
            try
            {
                await ActionDeleteAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        private void SaveAttachments_CheckedChanged(object sender, EventArgs e)
        {
            SaveAttachments = _formViewer.SaveAttachmentsMenuItem.Checked;
        }

        private void SaveEmail_CheckedChanged(object sender, EventArgs e)
        {
            SaveEmail = _formViewer.SaveEmailMenuItem.Checked;
        }

        private void SavePictures_CheckedChanged(object sender, EventArgs e)
        {
            SavePictures = _formViewer.SavePicturesMenuItem.Checked;
        }

        private void MoveConversation_CheckedChanged(object sender, EventArgs e)
        {
            MoveConversation = _formViewer.ConversationMenuItem.Checked;
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            BindSourceFolderRows(_dataModel.FindMatches(_formViewer.SearchText.Text));
        }

        public void EditFiltersMenuItem_Click(object sender, EventArgs e)
        {
            var filters = new ManageFilters();
            filters.LoadFilters(_globals);
            filters.Show();
        }

        private KbdActions<char, KaCharAsync, Func<char, Task>> _characterAsyncActions;
        internal KbdActions<char, KaCharAsync, Func<char, Task>> CharacterAsyncActions =>
            Initializer.GetOrLoad(ref _characterAsyncActions, GetAsyncCharacterActions);

        internal KbdActions<char, KaCharAsync, Func<char, Task>> GetAsyncCharacterActions()
        {
            return new KbdActions<char, KaCharAsync, Func<char, Task>>(
                new List<KaCharAsync>
                {
                    new KaCharAsync("Controller", 'S', (x) => JumpToAsync(_formViewer.SearchText)),
                    new KaCharAsync(
                        "Controller",
                        'F',
                        (x) => JumpToAsync(_formViewer.FolderListBox)
                    ),
                    //new KaCharAsync("Controller", 'A', (x) => ToggleCheckboxAsync(_formViewer.SaveAttachments)),
                    //new KaCharAsync("Controller", 'M', (x) => ToggleCheckboxAsync(_formViewer.SaveEmail)),
                    //new KaCharAsync("Controller", 'P', (x) => ToggleCheckboxAsync(_formViewer.SavePictures)),
                    //new KaCharAsync("Controller", 'C', (x) => ToggleCheckboxAsync(_formViewer.MoveConversation)),
                    new KaCharAsync("Controller", 'K', (x) => KbdExecuteAsync(ActionOkAsync)),
                    new KaCharAsync("Controller", 'X', (x) => KbdExecuteAsync(ActionCancelAsync)),
                    new KaCharAsync(
                        "Controller",
                        'R',
                        (x) => KbdExecuteAsync(RefreshSuggestionsAsync)
                    ),
                    new KaCharAsync("Controller", 'N', (x) => KbdExecuteAsync(CreateFolderAsync)),
                    new KaCharAsync("Controller", 'T', (x) => KbdExecuteAsync(ActionDeleteAsync)),
                    new KaCharAsync(
                        "Controller",
                        'M',
                        (x) => KbdExecuteAsync(() => ShowMenu(_formViewer.MoveOptionsMenu))
                    ),
                }
            );
        }

        //private Dictionary<char, Action<char>> _kbdActions;
        //public Dictionary<char, Action<char>> KbdActions => Initializer.GetOrLoad(ref _kbdActions, GetKbdActions);
        //internal Dictionary<char, Action<char>> GetKbdActions()
        //{
        //    return new()
        //    {
        //        { 'S', async (x) => await JumpToAsync(_formViewer.SearchText) },
        //        { 'F', async (x) => await JumpToAsync(_formViewer.FolderListBox) },
        //        { 'A', async (x) => await ToggleCheckboxAsync(_formViewer.SaveAttachments) },
        //        { 'M', async (x) => await ToggleCheckboxAsync(_formViewer.SaveEmail) },
        //        { 'P', async (x) => await ToggleCheckboxAsync(_formViewer.SavePictures) },
        //        { 'C', async (x) => await ToggleCheckboxAsync(_formViewer.MoveConversation) },
        //        { 'K', async (x) => await KbdExecuteAsync(ActionOkAsync) },
        //        { 'X', async (x) => await KbdExecuteAsync(ActionCancelAsync) },
        //        { 'R', async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync) },
        //        { 'N', async (x) => await KbdExecuteAsync(CreateFolderAsync) },
        //        { 'T', async (x) => await KbdExecuteAsync(ActionDeleteAsync) }
        //    };
        //}

        private KbdActions<char, KaChar, Action<char>> _characterActions;
        public KbdActions<char, KaChar, Action<char>> CharacterActions =>
            Initializer.GetOrLoad(ref _characterActions, GetKbdActions);

        internal KbdActions<char, KaChar, Action<char>> GetKbdActions()
        {
            return new KbdActions<char, KaChar, Action<char>>(
                new List<KaChar>
                {
                    new KaChar(
                        "Controller",
                        'S',
                        async (x) => await JumpToAsync(_formViewer.SearchText)
                    ),
                    new KaChar(
                        "Controller",
                        'F',
                        async (x) => await JumpToAsync(_formViewer.FolderListBox)
                    ),
                    new KaChar(
                        "Controller",
                        'K',
                        async (x) => await KbdExecuteAsync(ActionOkAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'X',
                        async (x) => await KbdExecuteAsync(ActionCancelAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'R',
                        async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'N',
                        async (x) => await KbdExecuteAsync(CreateFolderAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'T',
                        async (x) => await KbdExecuteAsync(ActionDeleteAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'M',
                        async (x) =>
                            await KbdExecuteAsync(() => ShowMenu(_formViewer.MoveOptionsMenu))
                    ),
                }
            );
        }

        internal void DarkMode_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_globals.Ol.DarkMode))
            {
                _darkMode = _globals.Ol.DarkMode;
                if (DarkMode)
                {
                    ActiveTheme = "DarkNormal";
                }
                else
                {
                    ActiveTheme = "LightNormal";
                }

                // Re-theme the breadcrumb document alongside the WinForms theme swap.
                _router?.ApplyTheme(DarkMode);
            }
        }

        #endregion
    }
}
