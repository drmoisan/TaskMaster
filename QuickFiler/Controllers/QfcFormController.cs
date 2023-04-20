using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesVB;
using UtilitiesCS;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{    
    internal class QfcFormController : IQfcFormController
    {
        public QfcFormController(IApplicationGlobals AppGlobals,
                                 QfcFormViewer FormViewer,
                                 Enums.InitTypeEnum InitType,
                                 System.Action ParentCleanup)
        { 
            _globals = AppGlobals;
            _formViewer = FormViewer;
            _parentCleanup = ParentCleanup;
            CaptureItemSettings();
            RemoveItemTemplate();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcFormViewer _formViewer;
        private IQfcCollectionController _groups;
        private RowStyle _rowStyleTemplate;
        private Padding _itemMarginTemplate;

        public void CaptureItemSettings()
        {
            _rowStyleTemplate = _formViewer.L1v0L2L3v_TableLayout.RowStyles[0];
            _itemMarginTemplate = _formViewer.QfcItemViewerTemplate.Margin;
        }

        public void RemoveItemTemplate()
        {
            TableLayoutHelper.RemoveSpecificRow(_formViewer.L1v0L2L3v_TableLayout, 0);
        }

        public int SpaceForEmail 
        { 
            get
            {
                var _screen = Screen.FromControl(_formViewer);
                int nonEmailSpace = (int)Math.Round(_formViewer.L1v_TableLayout.RowStyles[1].Height,0);
                int workingSpace = _screen.WorkingArea.Height;
                return workingSpace - nonEmailSpace;
            } 
        }

        public int ItemsPerIteration { get => (int)Math.Round(SpaceForEmail / _rowStyleTemplate.Height, 0); }
        
        public void LoadItems(IList<object> listObjects) 
        { 
            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  InitType: Enums.InitTypeEnum.InitSort,
                                                  ParentObject: this);
            _groups.LoadControlsAndHandlers(listObjects, _rowStyleTemplate);
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
