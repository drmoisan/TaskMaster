using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
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

        public EfcHomeController(IApplicationGlobals appGlobals, System.Action parentCleanup, MailItem mail = null)
        {
            _globals = appGlobals;
            _parentCleanup = parentCleanup;
            _dataModel = new EfcDataModel(_globals, mail);

            if (_dataModel.Mail is not null)
            {
                _initType = Enums.InitTypeEnum.Sort | Enums.InitTypeEnum.SortConv;
                _stopWatch = new cStopWatch();
                _formViewer = new EfcViewer();
                _keyboardHandler = new QfcKeyboardHandler(_formViewer, this);
                _formController = new EfcFormController(_globals, _dataModel, _formViewer, this, Cleanup, _initType);
            }
        }

        private EfcViewer _formViewer;
        private IApplicationGlobals _globals;
        private Enums.InitTypeEnum _initType;
        private System.Action _parentCleanup;
        private ConversationResolver _conversationResolver;

        public void Run() 
        { 
            if (_dataModel.Mail is not null)
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

        private EfcDataModel _dataModel;
        internal EfcDataModel DataModel { get => _dataModel; set => _dataModel = value; }
                
        private cStopWatch _stopWatch;
        public cStopWatch StopWatch { get => _stopWatch; }

        public bool Loaded => throw new NotImplementedException();

        #endregion

        #region Major Actions

        async public Task ExecuteMoves() => await _dataModel.MoveToFolder(
            _formController.SelectedFolder,
            _formController.SaveAttachments,
            _formController.SaveEmail,
            _formController.SavePictures,
            _formController.MoveConversation);

        
        //TODO: Implement QuickFileMetrics_WRITE
        public void QuickFileMetrics_WRITE(string filename)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Methods
                
        //public IList<MailItem> PackageItems() => _conversationResolver.ConversationItems;

        
        #endregion
    }
}
