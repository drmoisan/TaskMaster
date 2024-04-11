using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
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
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal class EfcFormController : IFilerFormController
    {
        #region Constructors

        public EfcFormController(
            IApplicationGlobals AppGlobals,
            EfcDataModel dataModel,
            EfcViewer formViewer,
            EfcHomeController homeController,
            System.Action ParentCleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token)
        {
            _token = token;
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _dataModel = dataModel;
            _initType = initType;
            _itemViewer = _formViewer.ItemViewer;
            _itemTlp = _formViewer.L0vh_TLP;

            Initialize();
        }

        internal void Initialize()
        {
            LoadUserSettings();
            CaptureConfigureItemViewer();
            ResolveControlGroups();
            _itemController = new EfcItemController(_globals, _homeController, this, _itemViewer, _dataModel, _token);
            SetupThemes();
            WireEventHandlers();
            _ = PopulateFolderCombobox();

        }

        #endregion Constructors

        #region Private Properties

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcDataModel _dataModel;
        private EfcViewer _formViewer;
        private EfcHomeController _homeController;
        private EfcItemController _itemController;
        private ItemViewer _itemViewer;
        //private FolderHandler _folderHandler;
        //private MailItem _mailItem;
        private QfEnums.InitTypeEnum _initType;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private TableLayoutPanel _itemTlp;
        private int _itemViewerTlpRow;
        private int _tlpHeightExpanded;
        private int _tlpHeightCollapsed;
        private int _tlpHeightDiff;
        private Dictionary<string, Theme> _themes;
        private List<Button> _listButtons;
        private List<Control> _listDefault;
        private List<Control> _listCheckBox;
        private List<Control> _listHighlighted;

        #endregion Private Properties

        #region Setup and Cleanup Methods

        internal void CaptureConfigureItemViewer()
        {
            _tlpHeightExpanded = (int)Math.Round(_itemTlp.RowStyles[1].Height, 0);
            var heightDiff = _tlpHeightExpanded - _itemViewer.Height;
            _tlpHeightCollapsed = _itemViewer.MinimumSize.Height + heightDiff;
            _tlpHeightDiff = _tlpHeightExpanded - _tlpHeightCollapsed;
            _itemViewerTlpRow = _itemTlp.GetPositionFromControl(_itemViewer).Row;
            ToggleExpansionStyle(Enums.ToggleState.Off);
        }
        
        public void Cleanup()
        {
            _globals.Ol.PropertyChanged -= DarkMode_Changed;
            _globals = null;
            _formViewer = null;
            _dataModel = null;
            _parentCleanup.Invoke();
        }

        internal void ResolveControlGroups()
        {
            _listTipsDetails = _formViewer.TipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off, true));

            var starter = _formViewer.GetAllChildren(except: new List<Control> { _itemViewer, });

            _listButtons  = starter.Where(x => x is Button).Cast<Button>().ToList();

            _listCheckBox = starter.Where(x => (x is CheckBox)).ToList();

            _listHighlighted = new List<Control> { _formViewer.SearchText, _formViewer.FolderListBox, };

            _listDefault = starter.Where(x => !_formViewer.TipsLabels.Contains(x) && 
                                              !_listButtons.Contains(x) && 
                                              !_listHighlighted.Contains(x) &&
                                              !_listCheckBox.Contains(x)) 
                                  .ToList();
        }

        internal void SetupThemes()
        {
            _themes = EfcThemeHelper.SetupFormThemes(
                _formViewer.TipsLabels.Cast<Control>().ToList(),
                _listHighlighted,
                _listDefault,
                _listButtons.Cast<Control>().ToList(),
                _listCheckBox);

            _activeTheme = LoadTheme(); 
        }

        #endregion Setup and Cleanup Methods

        #region Public Properties

        private string _activeTheme;
        public string ActiveTheme
        {
            get => Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
            set => Initializer.SetAndSave<string>(ref _activeTheme, value, (x) => _themes[x].SetTheme(async: true));
        }
        internal string LoadTheme()
        {
            var activeTheme = DarkMode ? "DarkNormal" : "LightNormal";
            _themes[activeTheme].SetTheme();
            return activeTheme;
        }

        private bool _darkMode;
        public bool DarkMode
        {
            get => Initializer.GetOrLoad(ref _darkMode, () => _globals.Ol.DarkMode, false, _globals, _globals.Ol);
            set => Initializer.SetAndSave(ref _darkMode, value, (x) => _globals.Ol.DarkMode = x);
        }

        public IntPtr FormHandle => _formViewer.Handle;

        public string SelectedFolder { get => _formViewer.FolderListBox.SelectedItem as string; }
        
        private bool _saveAttachments;
        public bool SaveAttachments
        {
            get => _saveAttachments;
            set
            {
                _saveAttachments = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SaveAttachments = value;
            }
        }

        private bool _saveEmail;
        public bool SaveEmail
        {
            get => _saveEmail;
            set
            {
                _saveEmail = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SaveEmail = value;
            }
        }

        private bool _savePictures;
        public bool SavePictures
        {
            get => _savePictures;
            set
            {
                _savePictures = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SavePictures = value;
            }
        }

        private bool _moveConversation;
        public bool MoveConversation
        {
            get => _moveConversation;
            set
            {
                _moveConversation = value;
                // Should be set elsewhere as a user default
                //Settings.Default.MoveConversation = value;
            }
        }

        private CancellationToken _token;
        public CancellationToken Token { get => _token; set => _token = value; }

        #endregion

        #region Event Handlers

        public void WireEventHandlers()
        {
            //_homeController.KeyboardHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
            //_homeController.KeyboardHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
            
            _formViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                    _homeController.KeyboardHandler.KeyboardHandler_PreviewKeyDownAsync);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                    _homeController.KeyboardHandler.KeyboardHandler_KeyDownAsync);
            },
            new List<Control> {  });
            _formViewer.SaveAttachmentsMenuItem.CheckedChanged += SaveAttachments_CheckedChanged;
            _formViewer.SaveEmailMenuItem.CheckedChanged += SaveEmail_CheckedChanged;
            _formViewer.SavePicturesMenuItem.CheckedChanged += SavePictures_CheckedChanged;
            _formViewer.ConversationMenuItem.CheckedChanged += MoveConversation_CheckedChanged;
            _formViewer.Ok.Click += ButtonOK_Click;
            _formViewer.Cancel.Click += ButtonCancel_Click;
            _formViewer.RefreshPredicted.Click += ButtonRefresh_Click;
            _formViewer.NewFolder.Click += ButtonCreate_Click;
            _formViewer.BtnDelItem.Click += ButtonDelete_Click;
            _formViewer.SearchText.TextChanged += SearchText_TextChanged;
            _formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;
            _globals.Ol.PropertyChanged += DarkMode_Changed;
        }
               
        async public void ButtonCancel_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            await ActionCancelAsync();
        }

        async public void ButtonOK_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            await ActionOkAsync();
        }

        async public void ButtonRefresh_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
            
            await RefreshSuggestionsAsync();
        }

        async public void ButtonCreate_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            if (_initType == QfEnums.InitTypeEnum.Find) { throw new NotImplementedException(); }
                        
            if (!IsValidSelection)
            {
                MessageBox.Show("Please select a valid parent folder where you would like to place the new folder.");
            }
            else 
            {
                var folder = (await _dataModel.FolderHelper.CreateFolderAsync(
                    SelectedFolder, 
                    _globals.Ol.ArchiveRootPath, 
                    _globals.FS.FldrRoot, 
                    Token)) as MAPIFolder;

                if (folder is not null)
                {
                    await _dataModel.MoveToFolder(
                        folder,
                        _globals.Ol.ArchiveRootPath,
                        SaveAttachments,
                        SaveEmail,
                        SavePictures,
                        MoveConversation);

                    _formViewer.Close();
                    Cleanup();
                }
            }
            
        }

        async public void ButtonDelete_Click(object sender, EventArgs e)
        {
            await ActionDeleteAsync();
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
            _formViewer.FolderListBox.DataSource = _dataModel.FindMatches(_formViewer.SearchText.Text);
            if (_formViewer.FolderListBox.Items.Count > 0) { _formViewer.FolderListBox.SelectedIndex = 1; }
        }

        public void EditFiltersMenuItem_Click(object sender, EventArgs e)
        {
            var filters = new ManageFilters();
            filters.LoadFilters(_globals);
            filters.Show();
        }

        private KbdActions<char, KaCharAsync, Func<char, Task>> _characterAsyncActions;
        internal KbdActions<char, KaCharAsync, Func<char, Task>> CharacterAsyncActions => Initializer.GetOrLoad(ref _characterAsyncActions, GetAsyncCharacterActions);
        internal KbdActions<char, KaCharAsync, Func<char, Task>> GetAsyncCharacterActions()
        {
            return new KbdActions<char, KaCharAsync, Func<char, Task>>(new List<KaCharAsync>
            {
                new KaCharAsync("Controller", 'S', (x) => JumpToAsync(_formViewer.SearchText)),
                new KaCharAsync("Controller", 'F', (x) => JumpToAsync(_formViewer.FolderListBox)),
                //new KaCharAsync("Controller", 'A', (x) => ToggleCheckboxAsync(_formViewer.SaveAttachments)),
                //new KaCharAsync("Controller", 'M', (x) => ToggleCheckboxAsync(_formViewer.SaveEmail)),
                //new KaCharAsync("Controller", 'P', (x) => ToggleCheckboxAsync(_formViewer.SavePictures)),
                //new KaCharAsync("Controller", 'C', (x) => ToggleCheckboxAsync(_formViewer.MoveConversation)),
                new KaCharAsync("Controller", 'K', (x) => KbdExecuteAsync(ActionOkAsync)),
                new KaCharAsync("Controller", 'X', (x) => KbdExecuteAsync(ActionCancelAsync)),
                new KaCharAsync("Controller", 'R', (x) => KbdExecuteAsync(RefreshSuggestionsAsync)),
                new KaCharAsync("Controller", 'N', (x) => KbdExecuteAsync(CreateFolderAsync)),
                new KaCharAsync("Controller", 'T', (x) => KbdExecuteAsync(ActionDeleteAsync)),
                new KaCharAsync("Controller", 'M', (x) => KbdExecuteAsync(()=>ShowMenu(_formViewer.MoveOptionsMenu))),
            });
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
        public KbdActions<char, KaChar, Action<char>> CharacterActions => Initializer.GetOrLoad(ref _characterActions, GetKbdActions);
        internal KbdActions<char, KaChar, Action<char>> GetKbdActions()
        {
            return new KbdActions<char, KaChar, Action<char>>(new List<KaChar>
            {
                new KaChar("Controller", 'S', async (x) => await JumpToAsync(_formViewer.SearchText)),
                new KaChar("Controller", 'F', async (x) => await JumpToAsync(_formViewer.FolderListBox)),
                //new KaChar("Controller", 'A', async (x) => await ToggleCheckboxAsync(_formViewer.SaveAttachments)),
                //new KaChar("Controller", 'M', async (x) => await ToggleCheckboxAsync(_formViewer.SaveEmail)),
                //new KaChar("Controller", 'P', async (x) => await ToggleCheckboxAsync(_formViewer.SavePictures)),
                //new KaChar("Controller", 'C', async (x) => await ToggleCheckboxAsync(_formViewer.MoveConversation)),
                new KaChar("Controller", 'K', async (x) => await KbdExecuteAsync(ActionOkAsync)),
                new KaChar("Controller", 'X', async (x) => await KbdExecuteAsync(ActionCancelAsync)),
                new KaChar("Controller", 'R', async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync)),
                new KaChar("Controller", 'N', async (x) => await KbdExecuteAsync(CreateFolderAsync)),
                new KaChar("Controller", 'T', async (x) => await KbdExecuteAsync(ActionDeleteAsync)),
                new KaChar("Controller", 'M', async (x) => await KbdExecuteAsync(()=>ShowMenu(_formViewer.MoveOptionsMenu))),
            });
        }

        internal void DarkMode_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_globals.Ol.DarkMode))
            {
                _darkMode = _globals.Ol.DarkMode;
                if (DarkMode) { ActiveTheme = "DarkNormal"; }
                else { ActiveTheme = "LightNormal"; }
            }
        }

        #endregion

        #region Major Actions

        async public Task ActionOkAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            _formViewer.Hide();
            await _homeController.ExecuteMoves().ConfigureAwait(false);
            await _formViewer.UiSyncContext;
            _formViewer.Dispose();
            Cleanup();
        }

        async public Task ActionCancelAsync()
        {
            //Debug.WriteLine($"Thread Id before await: {Thread.CurrentThread.ManagedThreadId}");
            await _formViewer.UiSyncContext;
            //Debug.WriteLine($"Thread Id after await: {Thread.CurrentThread.ManagedThreadId}");
            _formViewer.Close();
            Cleanup();
        }

        async public Task ActionDeleteAsync()
        {             
            await _formViewer.UiSyncContext;
            var items = (string[])_formViewer.FolderListBox.DataSource;
            var itemList = items.ToList();
            itemList.Insert(0, "Trash to Delete");
            _formViewer.FolderListBox.DataSource = itemList.ToArray();
        }

        async public Task CreateFolderAsync()
        {
            if (_initType == QfEnums.InitTypeEnum.Find) { throw new NotImplementedException(); }

            if (!IsValidSelection)
            {
                MessageBox.Show("Please select a valid parent folder where you would like to place the new folder.");
            }
            else
            {
                await _formViewer.UiSyncContext;
                _formViewer.Hide();
                var folder = await Task.FromResult(_dataModel
                                                   .FolderHelper
                                                   .CreateFolder(SelectedFolder,
                                                                 _globals.Ol.ArchiveRootPath,
                                                                 _globals.FS.FldrRoot)).ConfigureAwait(false);
                if (folder is not null)
                {
                    await _dataModel.MoveToFolder(folder,
                                                  _globals.Ol.ArchiveRootPath,
                                                  SaveAttachments,
                                                  SaveEmail,
                                                  SavePictures,
                                                  MoveConversation).ConfigureAwait(false);
                    await _formViewer.UiSyncContext;
                    _formViewer.Dispose();
                    Cleanup();
                }
            }
        }

        async public Task RefreshSuggestionsAsync()
        {
            await Task.Run(() => _dataModel.RefreshSuggestions(), Token);
            var matches = await Task.Run(() => _dataModel.FindMatches(_formViewer.SearchText.Text), Token);
            
            
            _formViewer.FolderListBox.DataSource = matches;
            if (_formViewer.FolderListBox.Items.Count > 0) { _formViewer.FolderListBox.SelectedIndex = 1; }
        }

        #endregion

        #region Helper Methods
        
        async public Task KbdExecuteAsync(Func<Task> action)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            await action();
        }

        async public Task KbdExecuteAsync(System.Action action)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            action();
        }

        async internal Task JumpToAsync(Control control)
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

        async public Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            checkBox.Checked = !checkBox.Checked;
        }

        public void ToggleOffNavigation(bool async)
        {
            CharacterActions.Keys.ForEach(key => _homeController.KeyboardHandler.CharActions.Remove("Controller", key));
            ToggleTips(async, Enums.ToggleState.Off);
            _itemController.ToggleNavigation(async, Enums.ToggleState.Off);
        }

        public async Task ToggleOffNavigationAsync()
        {
            CharacterAsyncActions.Keys.ForEach(key => _homeController.KeyboardHandler.CharActionsAsync.Remove("Controller", key));
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
            CharacterAsyncActions.ForEach(x => _homeController.KeyboardHandler.CharActionsAsync.Add(x));
            await ToggleTipsAsync(Enums.ToggleState.On);
            await _itemController.ToggleNavigationAsync(Enums.ToggleState.On);
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _formViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(true))); }
                else { _formViewer.Invoke(new System.Action(() => tipsDetails.Toggle(true))); }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _formViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(desiredState, true))); }
                else { _formViewer.Invoke(new System.Action(() => tipsDetails.Toggle(desiredState, true))); }
            }
        }

        public async Task ToggleTipsAsync(Enums.ToggleState desiredState)
        {
            Token.ThrowIfCancellationRequested();

            // Attempt to remove blocking await code and start all tasks simultaneously. 
            var tasks = _listTipsDetails.Select(x => x.ToggleAsync(desiredState, shareColumn: true)).ToList();
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

        async public Task PopulateFolderCombobox(object folderList = null)
        {
            await _dataModel.InitFolderHandlerAsync(folderList);

            await _formViewer.UiSyncContext;

            _formViewer.FolderListBox.DataSource = _dataModel.FolderHelper.FolderArray;
            if (_formViewer.FolderListBox.Items.Count > 0)
            {
                _formViewer.FolderListBox.SelectedIndex = 1;
            }
        }

        internal bool IsValidSelection
        {
            get
            {
                var selectedFolder = SelectedFolder;
                return !(selectedFolder is null || selectedFolder == "" || selectedFolder.Length < 3 || selectedFolder.Substring(0, 3) == "===");
            }
        }

        #endregion

        public void ToggleExpansionStyle(Enums.ToggleState desiredState)
        {
            if (desiredState == Enums.ToggleState.On)
            {
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightExpanded;
                _formViewer.MinimumSize = new Size(_formViewer.MinimumSize.Width, _formViewer.MinimumSize.Height + _tlpHeightDiff);
                _formViewer.Size = new Size(_formViewer.Size.Width, _formViewer.Size.Height + _tlpHeightDiff);
                _formViewer.WindowState = FormWindowState.Maximized;
            }
            else
            {
                _formViewer.WindowState = FormWindowState.Normal;
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightCollapsed;
                _formViewer.MinimumSize = new Size(_formViewer.MinimumSize.Width, _formViewer.MinimumSize.Height - _tlpHeightDiff);
                _formViewer.Size = new Size(_formViewer.Size.Width, _formViewer.Size.Height - _tlpHeightDiff);
            }
        
        }

    }
}
