using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickFiler.QfcLauncher;
using ToDoModel;
using UtilitiesVB;
using UtilitiesCS;
using static QuickFiler.Enums;

namespace QuickFiler
{
    internal class QfcHomeController : IQfcHomeController
    {
        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcFormLegacyViewer _formViewer;
        private IQfcDatamodel _datamodel;
        private IQfcExplorerController _explorerController;
        private IQfcFormController _formController;
        private IQfcCollectionController _collectionController;
        private IQfcKeyboardHandler _keyboardHandler;
        private cStopWatch _stopWatch;



        public QfcHomeController(IApplicationGlobals AppGlobals, System.Action ParentCleanup)
        {
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _datamodel = new QfcDatamodel(_globals.Ol.App.ActiveExplorer());
            _explorerController = new QfcExplorerController();
            _formViewer = new QfcFormLegacyViewer();
            _keyboardHandler = new QfcKeyboardHandler();
            _formController = new QfcFormController(_globals, _formViewer, InitTypeEnum.InitSort, Cleanup);
                        
            //_formController = new QuickFileController(_globals, _viewer, MasterQueue, Cleanup);
        }

        public int EmailsPerIteration
        {
            get
            {
                int MaxPixelsForEmail = _formController.MaxPixelsForEmail;
                var qfv = new QfcItemViewerForm();
                int PixelsPerEmail = qfv.Height;
                return (int)Math.Round(MaxPixelsForEmail / (double)PixelsPerEmail,0);
            }
        }

        public void Run()
        {
            // _formViewer.Show()
        }

        public bool Loaded
        {
            get
            {
                if (_formViewer is not null)
                {
                    // If _formViewer.IsDisposed = False Then
                    return true;
                }
                // Else
                // Return False
                // End If
                else
                {
                    return false;
                }
            }
        }

        internal void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _collectionController = null;
            _keyboardHandler = null;
            _parentCleanup.Invoke();
        }

        public IQfcExplorerController ExplCtrlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IQfcFormController FrmCtrlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IQfcCollectionController QfcColCtrlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IQfcKeyboardHandler KbdHndlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Iterate()
        {
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
            
            IList<MailItem> listEmails = _datamodel.DequeueNextEmailGroup(EmailsPerIteration);
            _collectionController.LoadControlsAndHandlers(listEmails);
        }
    }
}
