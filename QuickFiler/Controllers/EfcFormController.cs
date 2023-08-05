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
                                 MailItem mailItem,
                                 EfcViewer formViewer,
                                 EfcHomeController homeController,
                                 System.Action ParentCleanup,
                                 Enums.InitTypeEnum initType)
        {
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _mailItem = mailItem;
            _initType = initType;
            LoadSettings();
            WireEventHandlers();
            _ = PopulateFolderCombobox();
        }

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcViewer _formViewer;
        private EfcHomeController _homeController;
        private FolderHandler _folderHandler;
        private MailItem _mailItem;
        private Enums.InitTypeEnum _initType;

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _parentCleanup.Invoke();
        }

        #endregion

        #region Public Properties

        public IntPtr FormHandle => throw new NotImplementedException();

        //TODO: Implement SelectedFolder
        public string SelectedFolder
        {
            get
            {
                return _formViewer.FolderListBox.SelectedItem as string;
            }
        }

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

        //TODO: Implement ButtonOK_Click
        public void ButtonOK_Click(object sender, EventArgs e)
        {
            ;
        }

        //TODO: Implement ButtonRefresh_Click
        public void ButtonRefresh_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        //TODO: Implement ButtonCreate_Click
        public void ButtonCreate_Click(object sender, EventArgs e)
        {
            var selectedValue = _formViewer.FolderListBox.SelectedItem as string;
            if (selectedValue == "" || selectedValue.Length < 3 || selectedValue.Substring(0, 3) == "===")
            {
                MessageBox.Show("Please select a root folder to create a new folder under.");
            }
            else 
            { 
                if (_initType == Enums.InitTypeEnum.Find)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var newFolderName = InputBox.ShowDialog(
                        $"Please enter a new subfolder for {_formViewer.FolderListBox.SelectedItem}",
                        "New folder dialog");
                    if (newFolderName != "")
                    {
                        //Check if a filesystem folder exists and create it if it doesn't
                        var newFolder = Path.Combine(selectedValue, newFolderName);
                        if (!Directory.Exists(newFolder))
                        {
                            Directory.CreateDirectory(newFolder);
                        }
                        //Check if an outlook folder exists and create it if it doesn't
                        //var currentFolder = _globals.Ol.App.Session.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                        //var folderPath = selectedValue.Split('\\');
                        //Folder newOutlookFolder = null;
                        //foreach (var folder in folderPath)
                        //{
                        //    if (folder != "")
                        //    {
                        //        newOutlookFolder = currentFolder.Folders[folder];
                        //        if (newOutlookFolder == null)
                        //        {
                        //            newOutlookFolder = currentFolder.Folders.Add(folder);
                        //        }
                        //        currentFolder = newOutlookFolder;
                        //    }
                        //}

                    }
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
            //_formViewer.FolderListBox.Items.Clear();
            //_formViewer.FolderListBox.Items.AddRange(
            //    _folderHandler.FindFolder(SearchString: "*" +
            //    _formViewer.SearchText.Text + "*",
            //    ReloadCTFStagingFiles: false,
            //    ReCalcSuggestions: false,
            //    objItem: _mailItem));
            _formViewer.FolderListBox.DataSource = _folderHandler.FindFolder(
                SearchString: "*" +
                _formViewer.SearchText.Text + "*",
                ReloadCTFStagingFiles: false,
                ReCalcSuggestions: false,
                objItem: _mailItem);
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
            if (folderList is null)
            {
                _folderHandler = await Task.Run(()=> new FolderHandler(
                    _globals, _mailItem, FolderHandler.Options.FromField));
            }
            else
            {
                _folderHandler = await Task.Run(()=> new FolderHandler(
                    _globals, folderList, FolderHandler.Options.FromArrayOrString));
            }

            await _formViewer.UiSyncContext;

            _formViewer.FolderListBox.DataSource = _folderHandler.FolderArray;
        }

            #endregion

        }
}