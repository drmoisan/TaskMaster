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
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal class EfcFormController : IFilerFormController
    {
        #region Constructors, Initializers, and Destructors

        public EfcFormController(IApplicationGlobals AppGlobals,
                                 EfcDataModel dataModel,
                                 EfcViewer formViewer,
                                 EfcHomeController homeController,
                                 System.Action ParentCleanup,
                                 QfEnums.InitTypeEnum initType)
        {
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _dataModel = dataModel;
            _initType = initType;
            _itemViewer = _formViewer.ItemViewer;
            _itemTlp = _formViewer.L0vh_TLP;
            
            LoadSettings();
            CaptureConfigureItemViewer();
            ResolveControlGroups();

            _formViewer.Show();
            _formViewer.Refresh();

            _itemController = new EfcItemController(_globals, _homeController, this, _itemViewer, _dataModel);

            _formViewer.Hide();

            _themes = EfcThemeHelper.SetupFormThemes(_formViewer.TipsLabels.Cast<Control>().ToList(),
                                                     _listHighlighted,
                                                     _listDefault, 
                                                     _listButtons.Cast<Control>().ToList(),
                                                     _listCheckBox);
            
            _activeTheme = LoadTheme();

            WireEventHandlers();
            _ = PopulateFolderCombobox();
        }

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

        #endregion

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
                Settings.Default.SaveAttachments = value;
            }
        }

        private bool _saveEmail;
        public bool SaveEmail
        {
            get => _saveEmail;
            set
            {
                _saveEmail = value;
                Settings.Default.SaveEmail = value;
            }
        }

        private bool _savePictures;
        public bool SavePictures
        {
            get => _savePictures;
            set
            {
                _savePictures = value;
                Settings.Default.SavePictures = value;
            }
        }

        private bool _moveConversation;
        public bool MoveConversation
        {
            get => _moveConversation;
            set
            {
                _moveConversation = value;
                Settings.Default.MoveConversation = value;
            }
        }

        #endregion

        #region Event Handlers

        public void WireEventHandlers()
        {
            _homeController.KeyboardHndlr.KdCharActions = new Dictionary<char, Action<char>>();
            _formViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                    _homeController.KeyboardHndlr.KeyboardHandler_PreviewKeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                    _homeController.KeyboardHndlr.KeyboardHandler_KeyDown);                            
            },
            new List<Control> {  });
            _formViewer.SaveAttachments.CheckedChanged += SaveAttachments_CheckedChanged;
            _formViewer.SaveEmail.CheckedChanged += SaveEmail_CheckedChanged;
            _formViewer.SavePictures.CheckedChanged += SavePictures_CheckedChanged;
            _formViewer.MoveConversation.CheckedChanged += MoveConversation_CheckedChanged;
            _formViewer.Ok.Click += ButtonOK_Click;
            _formViewer.Cancel.Click += ButtonCancel_Click;
            _formViewer.RefreshPredicted.Click += ButtonRefresh_Click;
            _formViewer.NewFolder.Click += ButtonCreate_Click;
            _formViewer.BtnDelItem.Click += ButtonDelete_Click;
            _formViewer.SearchText.TextChanged += SearchText_TextChanged;
            _globals.Ol.PropertyChanged += DarkMode_Changed;

            //_listHover.ForEach(x => 
            //{
            //    x.MouseEnter += Button_MouseEnter;
            //    x.MouseLeave += Button_MouseLeave;
            //});
            //_formViewer.Ok.MouseEnter += Button_MouseEnter;
            //_formViewer.Cancel.MouseEnter += Button_MouseEnter;
            //_formViewer.RefreshPredicted.MouseEnter += Button_MouseEnter;
            //_formViewer.NewFolder.MouseEnter += Button_MouseEnter;
            //_formViewer.Ok.MouseLeave += Button_MouseLeave;
            //_formViewer.Cancel.MouseLeave += Button_MouseLeave;
            //_formViewer.RefreshPredicted.MouseLeave += Button_MouseLeave;
            //_formViewer.NewFolder.MouseLeave += Button_MouseLeave;
        }
               
        async public void ButtonCancel_Click(object sender, EventArgs e)
        {
            await ActionCancelAsync();
        }

        async public void ButtonOK_Click(object sender, EventArgs e)
        {
            await ActionOkAsync();
        }

        async public void ButtonRefresh_Click(object sender, EventArgs e)
        {
            //_dataModel.RefreshSuggestions();
            //_formViewer.FolderListBox.DataSource = _dataModel.FindMatches(_formViewer.SearchText.Text);
            //if (_formViewer.FolderListBox.Items.Count > 0) { _formViewer.FolderListBox.SelectedIndex = 1; }
            await RefreshSuggestionsAsync();
        }

        async public void ButtonCreate_Click(object sender, EventArgs e)
        {
            if (_initType == QfEnums.InitTypeEnum.Find) { throw new NotImplementedException(); }

            if (!IsValidSelection)
            {
                MessageBox.Show("Please select a valid parent folder where you would like to place the new folder.");
            }
            else 
            {
                var folder = await Task.FromResult(_dataModel
                                                   .FolderHandler
                                                   .CreateFolder(SelectedFolder, 
                                                                 _globals.Ol.ArchiveRootPath, 
                                                                 _globals.FS.FldrRoot));
                if (folder is not null) 
                { 
                    await _dataModel.MoveToFolder(folder,
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
            SaveAttachments = _formViewer.SaveAttachments.Checked;
        }

        private void SaveEmail_CheckedChanged(object sender, EventArgs e)
        {
            SaveEmail = _formViewer.SaveEmail.Checked;
        }

        private void SavePictures_CheckedChanged(object sender, EventArgs e)
        {
            SavePictures = _formViewer.SavePictures.Checked;
        }

        private void MoveConversation_CheckedChanged(object sender, EventArgs e)
        {
            MoveConversation = _formViewer.MoveConversation.Checked;
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            _formViewer.FolderListBox.DataSource = _dataModel.FindMatches(_formViewer.SearchText.Text);
            if (_formViewer.FolderListBox.Items.Count > 0) { _formViewer.FolderListBox.SelectedIndex = 1; }
        }

        private Dictionary<char, Action<char>> _keyboardActions;
        internal Dictionary<char, Action<char>> KeyboardActions => Initialized(_keyboardActions, ()=> _keyboardActions = GetKbdActions());
        internal Dictionary<char, Action<char>> GetKbdActions()
        {
            return new()
            {
                { 'S', async (x) => await JumpToAsync(_formViewer.SearchText) },
                { 'F', async (x) => await JumpToAsync(_formViewer.FolderListBox) },
                { 'A', async (x) => await ToggleCheckboxAsync(_formViewer.SaveAttachments) },
                { 'M', async (x) => await ToggleCheckboxAsync(_formViewer.SaveEmail) },
                { 'P', async (x) => await ToggleCheckboxAsync(_formViewer.SavePictures) },
                { 'C', async (x) => await ToggleCheckboxAsync(_formViewer.MoveConversation) },
                { 'K', async (x) => await KbdExecuteAsync(ActionOkAsync) },
                { 'X', async (x) => await KbdExecuteAsync(ActionCancelAsync) },
                { 'R', async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync) },
                { 'N', async (x) => await KbdExecuteAsync(CreateFolderAsync) },
                { 'T', async (x) => await KbdExecuteAsync(ActionDeleteAsync) }
            };
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
            await _formViewer.UiSyncContext;
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
                                                   .FolderHandler
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
            await TaskPriority.Run(PriorityScheduler.AboveNormal, ()=> _dataModel.RefreshSuggestions());
            var matches = await TaskPriority<string[]>.Run(
                PriorityScheduler.AboveNormal, ()=> _dataModel.FindMatches(_formViewer.SearchText.Text));
            
            await _formViewer.UiSyncContext;
            _formViewer.FolderListBox.DataSource = matches;
            if (_formViewer.FolderListBox.Items.Count > 0) { _formViewer.FolderListBox.SelectedIndex = 1; }
        }

        #endregion

        #region Helper Methods

        async public Task KbdExecuteAsync(Func<Task> action)
        {
            _homeController.KeyboardHndlr.ToggleKeyboardDialog();
            await action();
        }

        async internal Task JumpToAsync(Control control)
        {
            _homeController.KeyboardHndlr.ToggleKeyboardDialog();
            await _formViewer.UiSyncContext;
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
        
        async public Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await _formViewer.UiSyncContext;
            checkBox.Checked = !checkBox.Checked;
            _homeController.KeyboardHndlr.ToggleKeyboardDialog();
        }

        public void ToggleOffNavigation(bool async)
        {
            KeyboardActions.Keys.ForEach(key => _homeController.KeyboardHndlr.KdCharActions.Remove(key));
            ToggleTips(async, Enums.ToggleState.Off);
            _itemController.ToggleNavigation(async, Enums.ToggleState.Off);
        }

        public void ToggleOnNavigation(bool async)
        {
            KeyboardActions.ForEach(x => _homeController.KeyboardHndlr.KdCharActions.Add(x.Key, x.Value));
            ToggleTips(async, Enums.ToggleState.On);
            _itemController.ToggleNavigation(async, Enums.ToggleState.On);
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

        internal void LoadSettings()
        {
            _saveAttachments = Settings.Default.SaveAttachments;
            _formViewer.SaveAttachments.Checked = _saveAttachments;

            _saveEmail = Settings.Default.SaveEmail;
            _formViewer.SaveEmail.Checked = _saveEmail;

            _savePictures = Settings.Default.SavePictures;
            _formViewer.SavePictures.Checked = _savePictures;

            _moveConversation = Settings.Default.MoveConversation;
            _formViewer.MoveConversation.Checked = _moveConversation;
        }

        async public Task PopulateFolderCombobox(object folderList = null)
        {
            await _dataModel.InitFolderHandlerAsync(folderList);

            await _formViewer.UiSyncContext;

            _formViewer.FolderListBox.DataSource = _dataModel.FolderHandler.FolderArray;
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

        public T Initialized<T>(T instance, Func<T> initializer)
        {
            if (instance is null)
            {
                instance = initializer();
            }
            return instance;
        }

        #endregion

        public void ToggleExpansionStyle(Enums.ToggleState desiredState)
        {
            if (desiredState == Enums.ToggleState.On)
            {
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightExpanded;
                _formViewer.MinimumSize = new Size(_formViewer.MinimumSize.Width, _formViewer.MinimumSize.Height + _tlpHeightDiff);
                _formViewer.Size = new Size(_formViewer.Size.Width, _formViewer.Size.Height + _tlpHeightDiff);
            }
            else
            {
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightCollapsed;
                _formViewer.MinimumSize = new Size(_formViewer.MinimumSize.Width, _formViewer.MinimumSize.Height - _tlpHeightDiff);
                _formViewer.Size = new Size(_formViewer.Size.Width, _formViewer.Size.Height - _tlpHeightDiff);
            }
        
        }

    }
}
