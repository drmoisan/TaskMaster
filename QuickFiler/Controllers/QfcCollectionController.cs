using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesVB;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


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
            _itemTLP = _viewer.L1v0L2L3v_TableLayout;
            _initType = InitType;
            _globals = AppGlobals;
            _parent = ParentObject;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcFormViewer _viewer;
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IQfcFormController _parent;
        private int _itemHeight;
        private TableLayoutPanel _itemTLP;
        private List<ItemGroup> itemGroups = new List<ItemGroup>();

        public int EmailsLoaded => throw new NotImplementedException();

        public bool ReadyForMove => throw new NotImplementedException();

        public void LoadControlsAndHandlers(IList<object> listObjects, RowStyle template)
        {
            int i = 0;
            foreach (object objItem in listObjects)
            {
                if (objItem is MailItem)
                {
                    ItemGroup grp = new();
                    grp.ItemViewer = LoadItemViewer(++i, template, true);
                    grp.ItemController = new QfcItemController(_globals, grp.ItemViewer, (MailItem)objItem, this);
                    itemGroups.Add(grp);
                }
                else
                {
                    log.Debug($"Skipping Item {OlItemSummary.Extract(objItem,OlItemSummary.Details.All)}");
                }
                
            }
            
            _viewer.WindowState = FormWindowState.Maximized;
        }

        

        public QfcItemViewer LoadItemViewer(int itemNumber,
                                            RowStyle template,
                                            bool blGroupConversation)
        {
            _viewer.Refresh();
            QfcItemViewer itemViewer = new();
            _itemTLP.MinimumSize = new System.Drawing.Size(
                _itemTLP.MinimumSize.Width, 
                _itemTLP.MinimumSize.Height + 
                (int)Math.Round(template.Height, 0));
            TableLayoutHelper.InsertSpecificRow(_itemTLP, itemNumber - 1, template.Clone());
            itemViewer.Parent = _itemTLP;
            _itemTLP.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(0, itemNumber - 1));
            itemViewer.AutoSize = true;
            itemViewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            itemViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            itemViewer.Dock = DockStyle.Fill;
            _viewer.Refresh();
            return itemViewer;
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

        public class ItemGroup
        {
            public ItemGroup() { }

            private QfcItemViewer _itemViewer;
            private IQfcItemController _itemController;

            internal QfcItemViewer ItemViewer { get => _itemViewer; set => _itemViewer = value; }
            internal IQfcItemController ItemController { get => _itemController; set => _itemController = value; }

        }

        
    }
}
