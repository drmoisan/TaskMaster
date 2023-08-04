using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToDoModel;
using UtilitiesCS;

namespace QuickFiler
{
    public class EfcHomeController : IFilerHomeController
    {
        #region Constructors, Initializers, and Destructors

        public EfcHomeController(IApplicationGlobals appGlobals, System.Action parentCleanup)
        {
            _globals = appGlobals;
            _parentCleanup = parentCleanup;
            if (Mail is not null)
            {
                _initType = Enums.InitTypeEnum.Sort | Enums.InitTypeEnum.SortConv;
                _stopWatch = new cStopWatch();
                _formViewer = new EfcViewer();
                _keyboardHandler = new QfcKeyboardHandler(_formViewer, this);
                _formController = new EfcFormController(_globals, Mail, _formViewer, Cleanup, _initType);
            }
        }

        private EfcViewer _formViewer;
        private IApplicationGlobals _globals;
        private Enums.InitTypeEnum _initType;
        private System.Action _parentCleanup;

        public void Run() 
        { 
            if (Mail is not null)
            {
                _formViewer.Show();
            }
            else { MessageBox.Show("Error", "No MailItem Selected", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            _parentCleanup.Invoke();
        }
              

        #endregion

        #region Public Properties

        private IQfcExplorerController _explorerController;
        public IQfcExplorerController ExplorerCtlr { get => _explorerController; set => _explorerController = value; }

        private EfcFormController _formController;
        public IFilerFormController FormCtrlr { get => _formController; }

        private IQfcKeyboardHandler _keyboardHandler;
        public IQfcKeyboardHandler KeyboardHndlr { get => _keyboardHandler; set => _keyboardHandler = value; }

        private MailItem _mail;
        public MailItem Mail 
        {
            get 
            {
                if (_mail is null)
                    _mail = _globals.Ol.App.ActiveExplorer().Selection[1] as MailItem;
                return _mail;
            } 
            set => _mail = value; 
        }

        private cStopWatch _stopWatch;
        public cStopWatch StopWatch { get => _stopWatch; }

        public bool Loaded => throw new NotImplementedException();

        #endregion

        #region Major Actions

        public void ExecuteMoves()
        {
            //grp.ItemController.MoveMail();
            if (Mail is not null)
            {
                IList<MailItem> selItems = PackageItems();
                bool attchments = (SelectedFolder != "Trash to Delete") ? false : SaveAttachments;

                //LoadCTFANDSubjectsANDRecents.Load_CTF_AND_Subjects_AND_Recents();
                SortItemsToExistingFolder.MASTER_SortEmailsToExistingFolder(selItems: selItems,
                                                                            picturesCheckbox: false,
                                                                            sortFolderpath: SelectedFolder,
                                                                            saveMsg: SaveMsg,
                                                                            attchments: attchments,
                                                                            removeFlowFile: false,
                                                                            appGlobals: _globals,
                                                                            strRoot: _globals.Ol.ArchiveRootPath);
                SortItemsToExistingFolder.Cleanup_Files();
                // blDoMove
            }
            //stackMovedItems.Push(grp.MailItem);
        }

        //TODO: Implement QuickFileMetrics_WRITE
        public void QuickFileMetrics_WRITE(string filename)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Methods
                
        //TODO: Implement package items
        public IList<MailItem> PackageItems()
        {
            throw new NotImplementedException();
        }

        //TODO: Implement SelectedFolder
        public string SelectedFolder
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //TODO: Implement SaveAttachments
        public bool SaveAttachments
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //TODO: Implement SaveMsg
        public bool SaveMsg
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
