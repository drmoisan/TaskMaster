using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesVB;

namespace QuickFiler
{
    internal class QfcCollectionController : IQfcCollectionController
    {

        public QfcCollectionController(IApplicationGlobals AppGlobals,
                                       QfcFormLegacyViewer viewerInstance,
                                       Enums.InitTypeEnum InitType,
                                       IQfcFormController ParentObject)
        {

            _viewer = viewerInstance;
            _initType = InitType;
            _globals = AppGlobals;
            _parent = ParentObject;
            _rowHeight = new QfcItemViewerForm().Height;
        }

        
        private QfcFormLegacyViewer _viewer;
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IQfcFormController _parent;
        private int _rowHeight;


        public int EmailsLoaded => throw new NotImplementedException();

        public bool ReadyForMove => throw new NotImplementedException();

        public int ActivateByIndex(int intNewSelection, bool blExpanded)
        {
            throw new NotImplementedException();
        }

        public void AddEmailControlGroup(object objItem,
                                         int posInsert = 0,
                                         bool blGroupConversation = true,
                                         int ConvCt = 0,
                                         object varList = null,
                                         bool blChild = false)
        {
            throw new NotImplementedException();
        }

        public void ConvToggle_Group(IList<MailItem> selItems, int intOrigPosition)
        {
            throw new NotImplementedException();
        }

        public void ConvToggle_UnGroup(IList<MailItem> selItems, int intPosition, int ConvCt, object varList)
        {
            throw new NotImplementedException();
        }

        public bool IsSelectionBelowMax(int intNewSelection)
        {
            throw new NotImplementedException();
        }

        public void LoadControlsAndHandlers(IList<MailItem> colEmails)
        {
            int i = 0;
            foreach (MailItem mailItem in colEmails)
            {
                QfcItemViewerForm itemViewer = LoadItemViewer(++i, true);
            }
            
            _viewer.WindowState = FormWindowState.Maximized;
        }

        public QfcItemViewerForm LoadItemViewer(int itemNumber,
                                            bool blGroupConversation)
        {
            QfcItemViewerForm itemViewer = new();
            //_viewer.L1v1L2L3v
            return itemViewer;
        }

        public void MakeSpaceToEnumerateConversation()
        {
            throw new NotImplementedException();
        }

        public void MoveDownControlGroups(int intPosition, int intMoves)
        {
            throw new NotImplementedException();
        }

        public void MoveDownPix(int intPosition, int intPix)
        {
            throw new NotImplementedException();
        }

        public void RemoveControls()
        {
            throw new NotImplementedException();
        }

        public void RemoveSpaceToCollapseConversation()
        {
            throw new NotImplementedException();
        }

        public void RemoveSpecificControlGroup(int intPosition)
        {
            throw new NotImplementedException();
        }

        public void ResizeChildren(int intDiffx)
        {
            throw new NotImplementedException();
        }

        public void SelectNextItem()
        {
            throw new NotImplementedException();
        }

        public void SelectPreviousItem()
        {
            throw new NotImplementedException();
        }
    }
}
