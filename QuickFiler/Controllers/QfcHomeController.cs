using Microsoft.Office.Interop.Outlook;
using static QuickFiler.Enums;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesVB;
using UtilitiesCS;


namespace QuickFiler.Controllers
{
    public class QfcHomeController : IQfcHomeController
    {
        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcFormViewer _formViewer;
        private IQfcDatamodel _datamodel;
        private IQfcExplorerController _explorerController;
        private IQfcFormController _formController;
        private IQfcCollectionController _collectionController;
        private IQfcKeyboardHandler _keyboardHandler;
        private cStopWatch _stopWatch;

        public QfcHomeController(IApplicationGlobals AppGlobals, System.Action ParentCleanup)
        {
            QfcFormViewer.Main();
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _datamodel = new QfcDatamodel(_globals.Ol.App.ActiveExplorer(), _globals.Ol.App);
            _explorerController = new QfcExplorerController();
            _formViewer = new QfcFormViewer();
            _keyboardHandler = new QfcKeyboardHandler();
            _formController = new QfcFormController(_globals, _formViewer, InitTypeEnum.InitSort, Cleanup);           
        }

        public void Run() 
        {
            Iterate();
            _formViewer.Show();
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Refresh();
        }

        public bool Loaded { get => _formViewer is not null; }
        
        internal void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            _parentCleanup.Invoke();
        }

        public IQfcExplorerController ExplCtrlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IQfcFormController FrmCtrlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IQfcKeyboardHandler KbdHndlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Iterate()
        {
            //_stopWatch = new cStopWatch();
            //_stopWatch.Start();
            
            IList<MailItem> listObjects = _datamodel.DequeueNextItemGroup(_formController.ItemsPerIteration);
            _formController.LoadItems(listObjects);
        }
    }
}
