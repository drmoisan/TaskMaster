using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                                 Enums.InitTypeEnum initType)
        {
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _dataModel = dataModel;
            //_mailItem = mailItem;
            _initType = initType;
            LoadSettings();
            _listTipsDetails = _formViewer.TipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off));
            WireEventHandlers();
            _ = PopulateFolderCombobox();
        }

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcDataModel _dataModel;
        private EfcViewer _formViewer;
        private EfcHomeController _homeController;
        //private FolderHandler _folderHandler;
        //private MailItem _mailItem;
        private Enums.InitTypeEnum _initType;
        private IList<IQfcTipsDetails> _listTipsDetails;

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _dataModel = null;
            _parentCleanup.Invoke();
        }

        #endregion

        #region Public Properties

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
            _formViewer.Refresh.Click += ButtonRefresh_Click;
            _formViewer.NewFolder.Click += ButtonCreate_Click;
            _formViewer.BtnDelItem.Click += ButtonDelete_Click;
            _formViewer.SearchText.TextChanged += SearchText_TextChanged;
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
            if (_initType == Enums.InitTypeEnum.Find) { throw new NotImplementedException(); }

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
        internal Dictionary<char, Action<char>> KeyboardActions => Initialized(_keyboardActions, GetKbdActions);
        internal Dictionary<char, Action<char>> GetKbdActions()
        {
            return new()
            {
                { 'S', async (x) => await JumpToAsync(_formViewer.SearchText) },
                { 'F', async (x) => await JumpToAsync(_formViewer.FolderListBox) },
                { 'A', async (x) => await ToggleCheckboxAsync(_formViewer.SaveAttachments) },
                { 'E', async (x) => await ToggleCheckboxAsync(_formViewer.SaveEmail) },
                { 'P', async (x) => await ToggleCheckboxAsync(_formViewer.SavePictures) },
                { 'C', async (x) => await ToggleCheckboxAsync(_formViewer.MoveConversation) },
                { 'O', async (x) => await KbdExecuteAsync(ActionOkAsync) },
                { 'X', async (x) => await KbdExecuteAsync(ActionCancelAsync) },
                { 'R', async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync) },
                { 'N', async (x) => await KbdExecuteAsync(CreateFolderAsync) },
                { 'T', async (x) => await KbdExecuteAsync(ActionDeleteAsync) }
            };
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
            await _formViewer.UiSyncContext;
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
            if (_initType == Enums.InitTypeEnum.Find) { throw new NotImplementedException(); }

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
        }

        public void ToggleOnNavigation(bool async)
        {
            KeyboardActions.ForEach(x => _homeController.KeyboardHndlr.KdCharActions.Add(x.Key, x.Value));
            ToggleTips(async, Enums.ToggleState.On);
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _formViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle())); }
                else { _formViewer.Invoke(new System.Action(() => tipsDetails.Toggle())); }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _formViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(desiredState))); }
                else { _formViewer.Invoke(new System.Action(() => tipsDetails.Toggle(desiredState))); }
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
            await _dataModel.InitFolderHandler(folderList);

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

    }
}