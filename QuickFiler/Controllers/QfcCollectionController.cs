using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesVB;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace QuickFiler.Controllers
{
    internal class QfcCollectionController : IQfcCollectionController
    {
        public QfcCollectionController(IApplicationGlobals AppGlobals,
                                       QfcFormViewer viewerInstance,
                                       Enums.InitTypeEnum InitType,
                                       IQfcFormController ParentObject)
        {

            _viewer = viewerInstance;
            _initType = InitType;
            _globals = AppGlobals;
            _parent = ParentObject;
            _itemHeight = new QfcItemViewerForm().Height;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcFormViewer _viewer;
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IQfcFormController _parent;
        private int _itemHeight;

        public int EmailsLoaded => throw new NotImplementedException();

        public bool ReadyForMove => throw new NotImplementedException();

        public void LoadControlsAndHandlers(IList<object> listObjects)
        {
            int i = 0;
            foreach (object objItem in listObjects)
            {
                if (objItem is MailItem)
                {
                    QfcItemViewer itemViewer = LoadItemViewer(++i, true);
                }
                else
                {
                    log.Debug($"Skipping Item {OlItemSummary.Extract(objItem,OlItemSummary.Details.All)}");
                }
                
            }
            
            _viewer.WindowState = FormWindowState.Maximized;
        }

        

        public QfcItemViewer LoadItemViewer(int itemNumber,
                                            bool blGroupConversation)
        {
            QfcItemViewer itemViewer = new();
            RowStyle rowStyle = new RowStyle(SizeType.Absolute, _itemHeight+6);
            //_viewer.L1v1L2L3v
            return itemViewer;
        }

        QfcItemViewerForm IQfcCollectionController.LoadItemViewer(int intItemNumber, bool blGroupConversation)
        {
            throw new NotImplementedException();
        }

        public void AddEmailControlGroup(object objItem, int posInsert = 0, bool blGroupConversation = true, int ConvCt = 0, object varList = null, bool blChild = false)
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

        public int ActivateByIndex(int intNewSelection, bool blExpanded)
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

        public void MoveDownControlGroups(int intPosition, int intMoves)
        {
            throw new NotImplementedException();
        }

        public void MoveDownPix(int intPosition, int intPix)
        {
            throw new NotImplementedException();
        }

        public void ResizeChildren(int intDiffx)
        {
            throw new NotImplementedException();
        }

        public void ConvToggle_Group(IList<object> selItems, int intOrigPosition)
        {
            throw new NotImplementedException();
        }

        public void ConvToggle_UnGroup(IList<object> selItems, int intPosition, int ConvCt, object varList)
        {
            throw new NotImplementedException();
        }

        public void MakeSpaceToEnumerateConversation()
        {
            throw new NotImplementedException();
        }

        public bool IsSelectionBelowMax(int intNewSelection)
        {
            throw new NotImplementedException();
        }
    }
}
