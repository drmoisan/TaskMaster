using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickFiler.QuickFileController;
using UtilitiesVB;
using UtilitiesCS;
using System.Windows.Forms;

namespace QuickFiler
{    
    internal class QfcFormController : IQfcFormController
    {
        public QfcFormController(IApplicationGlobals AppGlobals,
                                 QfcFormLegacyViewer FormViewer,
                                 Enums.InitTypeEnum InitType,
                                 System.Action ParentCleanup)
        { 
            _globals = AppGlobals;
            _formViewer = FormViewer;
            _parentCleanup = ParentCleanup;
        }

        
        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcFormLegacyViewer _formViewer;
        private IQfcCollectionController _groups;


        public int MaxPixelsForEmail 
        { 
            get
            {
                var _screen = Screen.FromControl(_formViewer);
                int workingSpace = _screen.WorkingArea.Height;
                int nonEmailSpace = (int)Math.Round(_formViewer.L1v.RowStyles[1].Height,0);
                return workingSpace = nonEmailSpace;
            } 
        }
        
        public void LoadEmailsOnForm(IList<MailItem> listEmails) 
        { 
            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  InitType: Enums.InitTypeEnum.InitSort,
                                                  ParentObject: this);
            _groups.LoadControlsAndHandlers(listEmails);
        }

        public void FormResize(bool Force = false)
        {
            throw new NotImplementedException();
        }

        public void ButtonCancel_Click()
        {
            throw new NotImplementedException();
        }

        public void ButtonOK_Click()
        {
            throw new NotImplementedException();
        }

        public void ButtonUndo_Click()
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public void QFD_Maximize()
        {
            throw new NotImplementedException();
        }

        public void QFD_Minimize()
        {
            throw new NotImplementedException();
        }

        public void SpnEmailPerLoad_Change()
        {
            throw new NotImplementedException();
        }

        public void Viewer_Activate()
        {
            throw new NotImplementedException();
        }
    }
}
