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

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _dataModel = null;
            _parentCleanup.Invoke();
        }

        #endregion

        #region Public Properties

        public IntPtr FormHandle => throw new NotImplementedException();

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
            _formViewer.SaveAttachments.CheckedChanged += SaveAttachments_CheckedChanged;
            _formViewer.SaveEmail.CheckedChanged += SaveEmail_CheckedChanged;
            _formViewer.SavePictures.CheckedChanged += SavePictures_CheckedChanged;
            _formViewer.MoveConversation.CheckedChanged += MoveConversation_CheckedChanged;
            _formViewer.Ok.Click += ButtonOK_Click;
            _formViewer.Cancel.Click += ButtonCancel_Click;
            _formViewer.Refresh.Click += ButtonRefresh_Click;
            _formViewer.Create.Click += ButtonCreate_Click;
            _formViewer.SearchText.TextChanged += SearchText_TextChanged;
        }
        
        public void ButtonCancel_Click()
        {
            _formViewer.Close();
            Cleanup();
        }

        public void ButtonCancel_Click(object sender, EventArgs e)
        {
            ButtonCancel_Click();
        }

        //TODO: Implement ButtonOK_Click
        public void ButtonOK_Click()
        {
            throw new NotImplementedException();
        }

        async public void ButtonOK_Click(object sender, EventArgs e)
        {
            await _homeController.ExecuteMoves();
        }

        public void ButtonRefresh_Click(object sender, EventArgs e)
        {
            _dataModel.RefreshSuggestions();
            _formViewer.FolderListBox.DataSource = _dataModel.FindMatches(_formViewer.SearchText.Text);
            if (_formViewer.FolderListBox.Items.Count > 0) { _formViewer.FolderListBox.SelectedIndex = 1; }
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
                    await _dataModel.MoveToFolder(folder.FolderPath,
                                                  SaveAttachments,
                                                  SaveEmail,
                                                  SavePictures,
                                                  MoveConversation);
                    _formViewer.Close();
                    Cleanup();
                }
            }
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

        #endregion

        #region Helper Methods

        public void MaximizeFormViewer()
        {
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        public void MinimizeFormViewer()
        {
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }

        //TODO: Implement ToggleOffNavigation
        public void ToggleOffNavigation(bool async)
        {
            throw new NotImplementedException();
        }

        //TODO: Implement ToggleOnNavigation
        public void ToggleOnNavigation(bool async)
        {
            throw new NotImplementedException();
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

        #endregion

    }
}