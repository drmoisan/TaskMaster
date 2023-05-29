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
                                       bool darkMode,
                                       Enums.InitTypeEnum InitType,
                                       IQfcFormController ParentObject)
        {

            _formViewer = viewerInstance;
            _itemTLP = _formViewer.L1v0L2L3v_TableLayout;
            _initType = InitType;
            _globals = AppGlobals;
            _parent = ParentObject;
            SetupLightDark(darkMode);
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcFormViewer _formViewer;
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IQfcFormController _parent;
        private int _itemHeight;
        private TableLayoutPanel _itemTLP;
        private List<ItemGroup> _itemGroups = new List<ItemGroup>();
        private bool _darkMode;

        public int EmailsLoaded => throw new NotImplementedException();

        public bool ReadyForMove => throw new NotImplementedException();

        public void LoadControlsAndHandlers(IList<MailItem> listObjects, RowStyle template)
        {
            _itemTLP.SuspendLayout();
            int i = 0;
            foreach (MailItem objItem in listObjects)
            {
                ItemGroup grp = new(objItem);
                grp.ItemViewer = LoadItemViewer(++i, template, true);
                _itemGroups.Add(grp);
            }

            _formViewer.WindowState = FormWindowState.Maximized;

            //foreach (var grp in _itemGroups)
            Parallel.ForEach(_itemGroups, grp =>
            {
            grp.ItemController = new QfcItemController(_globals, grp.ItemViewer, i, grp.MailItem, this);
            Parallel.Invoke(
                () => grp.ItemController.PopulateConversation(),
                () => grp.ItemController.PopulateFolderCombobox(),
                () => 
                {
                    if (_darkMode) { grp.ItemController.SetThemeDark(); }
                    else { grp.ItemController.SetThemeLight(); } 
                });
            });

            
            _itemTLP.ResumeLayout();
        }

        

        public QfcItemViewer LoadItemViewer(int itemNumber,
                                            RowStyle template,
                                            bool blGroupConversation)
        {
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
            return itemViewer;
        }

        public void AddEmailControlGroup(object objItem, int posInsert = 0, bool blGroupConversation = true, int ConvCt = 0, object varList = null, bool blChild = false)
        {
            throw new NotImplementedException();
        }

        public void RemoveControls()
        {
            if (_itemGroups is not null)
            {
                _itemTLP.SuspendLayout();
                while (_itemGroups.Count > 0)
                {
                    int i = _itemGroups.Count - 1;

                    // Remove event managers and dispose unmanaged
                    _itemGroups[i].ItemController.Cleanup();

                    // Remove Item Viewer and Row from the form
                    TableLayoutHelper.RemoveSpecificRow(_itemTLP, i);

                    // Remove Handle on item viewer and controller
                    _itemGroups.RemoveAt(i);  
                }
                _itemTLP.ResumeLayout();
            }
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

        private void SetupLightDark(bool initDarkMode)
        {
            _darkMode = initDarkMode;
            _formViewer.DarkMode.CheckedChanged += new System.EventHandler(DarkMode_CheckedChanged);
            
        }

        private void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_formViewer.DarkMode.Checked==true)
            {
                SetDarkMode();
            }
            else
            {
                SetLightMode();
            }
        }

        public void SetDarkMode()
        {
            foreach (ItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeDark();
            }
        }

        public void SetLightMode()
        {
            foreach (ItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeLight();
            }
        }

        public void Cleanup()
        {
            RemoveControls();
            _formViewer = null;
            _globals = null;
            _parent = null;
            _itemTLP = null;
            _itemGroups = null;
        }

        public class ItemGroup
        {
            public ItemGroup() { }
            public ItemGroup(MailItem mailItem) { _mailItem = mailItem; }

            private QfcItemViewer _itemViewer;
            private IQfcItemController _itemController;
            private MailItem _mailItem;

            internal QfcItemViewer ItemViewer { get => _itemViewer; set => _itemViewer = value; }
            internal IQfcItemController ItemController { get => _itemController; set => _itemController = value; }
            internal MailItem MailItem { get => _mailItem; set => _mailItem = value; }

        }

        
    }
}
