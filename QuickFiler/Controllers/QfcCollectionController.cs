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
using QuickFiler;

namespace QuickFiler.Controllers
{
    internal class QfcCollectionController : IQfcCollectionController
    {
        #region Constructors

        public QfcCollectionController(IApplicationGlobals AppGlobals,
                                       QfcFormViewer viewerInstance,
                                       bool darkMode,
                                       Enums.InitTypeEnum InitType,
                                       IFilerHomeController homeController,
                                       IFilerFormController parent)
        {

            _formViewer = viewerInstance;
            _itemTLP = _formViewer.L1v0L2L3v_TableLayout;
            _itemPanel = _formViewer.L1v0L2_PanelMain;
            _initType = InitType;
            _globals = AppGlobals;
            _homeController = homeController;
            _keyboardHandler = _homeController.KeyboardHndlr;
            _parent = parent;
            SetupLightDark(darkMode);
        }

        #endregion

        #region Private Variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcFormViewer _formViewer;
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IFilerHomeController _homeController;
        private IFilerFormController _parent;
        private int _itemHeight;
        private Panel _itemPanel;
        private TableLayoutPanel _itemTLP;
        private List<ItemGroup> _itemGroups;
        private bool _darkMode;
        private RowStyle _template;
        private RowStyle _templateExpanded;
        private int _activeIndex = -1;
        private IQfcKeyboardHandler _keyboardHandler;
        private delegate int ActionDelegate(int intNewSelection, bool blExpanded);

        #endregion

        #region Public Properties

        public int ActiveIndex { get => _activeIndex; set => _activeIndex = value; }
        
        public int ActiveSelection { get => _activeIndex + 1; set => _activeIndex = value - 1; }

        public int EmailsLoaded
        {
            get 
            {
                return _itemGroups.Count;
            } 
        }

        public bool ReadyForMove
        {
            get
            {
                bool blReadyForMove = true;
                string strNotifications = "Can't complete actions! Not all emails assigned to folder" + System.Environment.NewLine;

                foreach (var grp in _itemGroups)
                {
                    string[] headers = {"======= SEARCH RESULTS =======",
                                        "======= RECENT SELECTIONS ========",
                                        "========= SUGGESTIONS =========" };
                    if ((grp.ItemController.SelectedFolder is null) || 
                        headers.Contains(grp.ItemController.SelectedFolder))
                    {
                        blReadyForMove = false;
                        strNotifications = strNotifications + 
                                           grp.ItemController.ItemNumber + 
                                           "  " + 
                                           grp.ItemController.Mail.SentOn.ToString("MM/dd/yyyy") +
                                           "  " + 
                                           grp.ItemController.Mail.Subject + 
                                           Environment.NewLine;
                    }
                }
                if (!blReadyForMove)
                    MessageBox.Show("Error Notification", strNotifications, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return blReadyForMove;
            }
        }

        private bool _tlpLayout = true;
        public bool TlpLayout 
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _tlpLayout;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set 
            { 
                if (_tlpLayout != value)
                {
                    _tlpLayout = value;
                    if (_tlpLayout)
                    {
                        _itemTLP.ResumeLayout(true);
                    }
                    else
                    {
                        _itemTLP.SuspendLayout();
                    }
                }
            }
        }    

        #endregion

        #region UI Add and Remove QfcItems

        public void LoadItemGroupsAndViewers(IList<MailItem> items, RowStyle template)
        {
            _itemGroups = new List<ItemGroup>();
            _keyboardHandler.KdCharActions = new Dictionary<char, Action<char>>();
            int i = 0;
            foreach (MailItem mailItem in items)
            {
                ItemGroup grp = new(mailItem);
                _itemGroups.Add(grp);
                grp.ItemViewer = LoadItemViewer(i, template, true);
                i++;
            }

        }

        public void WireUpKeyboardHandler()
        {
            // Treatment as char limits to 10 items
            for (int i = 0; i < _itemGroups.Count && i < 10; i++)
            {
                _keyboardHandler.KdCharActions.Add(
                    (i + 1).ToString()[0],
                    (c) => ChangeByIndex(int.Parse(c.ToString()) - 1));
            }
            _keyboardHandler.KdKeyActions = new Dictionary<Keys, Action<Keys>>
            {
                { Keys.Up, (k) => SelectPreviousItem() },
                { Keys.Down, (k) => SelectNextItem() }
            };
        }

        public void LoadConversationsAndFolders()
        {
            bool parallel = Properties.Settings.Default.ParallelLoad;
            if (parallel) { LoadParallelCF(); }
            else { LoadSequentialCF(); }
        }

        public void LoadParallelCF()
        {
            Parallel.ForEach(_itemGroups, grp =>
            {
                grp.ItemController = new QfcItemController(AppGlobals: _globals,
                                                           homeController: _homeController,
                                                           parent: this,
                                                           itemViewer: grp.ItemViewer,
                                                           viewerPosition: _itemGroups.FindIndex(x => x.MailItem == grp.MailItem) + 1,
                                                           grp.MailItem);
                
                Parallel.Invoke(
                    () => grp.ItemController.PopulateConversation(),
                    () => grp.ItemController.PopulateFolderCombobox(),
                    () =>
                    {
                        if (_darkMode) { grp.ItemController.SetThemeDark(async: true); }
                        else { grp.ItemController.SetThemeLight(async: true); }
                    });
            });
        }

        public void LoadSequentialCF()
        {
            int i = 0;
            foreach (var grp in _itemGroups)
            {
                grp.ItemController = new QfcItemController(AppGlobals: _globals,
                                                           homeController: _homeController,
                                                           parent: this,
                                                           itemViewer: grp.ItemViewer,
                                                           viewerPosition: ++i,
                                                           grp.MailItem);
                grp.ItemController.PopulateConversation();
                grp.ItemController.PopulateFolderCombobox();
                if (_darkMode) { grp.ItemController.SetThemeDark(async: false); }
                else { grp.ItemController.SetThemeLight(async: false); }
            }
        }

        public void LoadControlsAndHandlers(IList<MailItem> listMailItems, RowStyle template, RowStyle templateExpanded)
        {
            _formViewer.SuspendLayout();
            var tlpState = TlpLayout;
            TlpLayout = false;
            //_itemTLP.SuspendLayout();
            _template = template;
            _templateExpanded = templateExpanded;
            TableLayoutHelper.InsertSpecificRow(_itemTLP, 0, template, listMailItems.Count);
            LoadItemGroupsAndViewers(listMailItems, template);
            _formViewer.WindowState = FormWindowState.Maximized;
            TlpLayout = tlpState;
            //_itemTLP.ResumeLayout();
            _formViewer.ResumeLayout();
            WireUpKeyboardHandler();
            LoadConversationsAndFolders();

        }

        public QfcItemViewer LoadItemViewer(int indexNumber,
                                            RowStyle template,
                                            bool blGroupConversation = true,
                                            int columnNumber = 0)
        {
            QfcItemViewer itemViewer = new();
            _itemTLP.MinimumSize = new System.Drawing.Size(
                _itemTLP.MinimumSize.Width,
                _itemTLP.MinimumSize.Height +
                (int)Math.Round(template.Height, 0));
            // moved to caller for efficencies of multiple insertions
            //TableLayoutHelper.InsertSpecificRow(_itemTLP, itemNumber - 1, template.Clone());
            itemViewer.Parent = _itemTLP;
            if (columnNumber == 0)
            {
                _itemTLP.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(columnNumber, indexNumber));
                _itemTLP.SetColumnSpan(itemViewer, 2);
            }
            else
            {
                _itemTLP.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(1, indexNumber));
                _itemTLP.SetColumnSpan(itemViewer, 1);
            }

            itemViewer.AutoSize = true;
            itemViewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            itemViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            itemViewer.Dock = DockStyle.Fill;
            return itemViewer;
        }

        public void PopOutControlGroup(int selection)
        {
            // Get mail item from the group            
            MailItem mailItem = _itemGroups[selection - 1].MailItem;

            // Remove the group from the form
            RemoveSpecificControlGroup(selection);

            // TODO: Add the group to the pop out form 

        }

        public void RemoveControls()
        {
            //TODO: Optimize removal so all are removed at once using new helper
            if (_itemGroups is not null)
            {
                var tlpState = TlpLayout;
                TlpLayout = false;

                // Remove Item Viewers and Rows from the form
                TableLayoutHelper.RemoveSpecificRow(_itemTLP, 0, _itemGroups.Count);

                ResetPanelHeight();

                int max = _itemGroups.Count - 1;
                for (int i = max; i >= 0; i--)
                {
                    // Remove event managers and dispose unmanaged
                    _itemGroups[i].ItemController.Cleanup();

                    // Remove Handle on item viewer and controller
                    _itemGroups.RemoveAt(i);
                }

                TlpLayout = tlpState;
            }
        }

        /// <summary>
        /// Remove a specific control group from the form, 
        /// remove the group from the list of groups,
        /// and renumber the remaining groups
        /// </summary>
        /// <param name="selection">Number representing the item to remove</param>
        public void RemoveSpecificControlGroup(int selection)
        {
            // If the group is active, turn off the active item and select a new item
            bool activeUI = _itemGroups[selection - 1].ItemController.IsActiveUI;
            bool expanded = _itemGroups[selection - 1].ItemController.IsExpanded;
            if (activeUI) { ToggleOffActiveItem(false); }

            UpdateSelectionForRemoval(selection);

            var tlpState = TlpLayout;
            TlpLayout = false;

            // Remove the controls from the form
            TableLayoutHelper.RemoveSpecificRow(_itemTLP, selection - 1);

            // Remove the group from the list of groups
            _itemGroups.RemoveAt(selection - 1);

            if (_itemGroups.Count > 0)
            {
                // Renumber the remaining groups
                RenumberGroups();

                // Restore UI to previous state with newly selected item
                if (activeUI)
                {
                    _itemGroups[ActiveIndex].ItemController.ToggleFocus(Enums.ToggleState.On);
                    if (expanded) { _itemGroups[ActiveIndex].ItemController.ToggleExpansion(); }
                }
            }
            

            TlpLayout = tlpState;
            ResetPanelHeight();

        }

        #endregion

        #region UI Select QfcItems

        public int ActivateByIndex(int newIndex, bool blExpanded)
        {
            return ActivateBySelection(newIndex + 1, blExpanded);
        }

        public int ActivateBySelection(int intNewSelection, bool blExpanded)
        {
            if (intNewSelection > 0 & intNewSelection <= _itemGroups.Count)
            {
                var tlpState = TlpLayout;
                TlpLayout = false;

                var itemController = _itemGroups[intNewSelection - 1].ItemController;
                var itemViewer = _itemGroups[intNewSelection - 1].ItemViewer;

                itemController.ToggleFocus();
                if (blExpanded) { itemController.ToggleExpansion(); }
                ScrollIntoView(itemViewer);

                ActiveSelection = intNewSelection;

                TlpLayout = tlpState;
            }
            return ActiveSelection;
        }

        public void ChangeByIndex(int idx)
        {
            bool expanded = false;
            if ((ActiveIndex != idx) && (idx < _itemGroups.Count))
            {
                var tlpState = TlpLayout;
                TlpLayout = false;

                if (ActiveIndex != -1)
                    expanded = ToggleOffActiveItem(false);
                ActivateBySelection(idx + 1, expanded);

                TlpLayout = tlpState;
            }
        }

        public void SelectNextItem()
        {
            if (ActiveSelection < _itemGroups.Count)
            {
                var tlpState = TlpLayout;
                TlpLayout = false;

                ChangeByIndex(ActiveIndex + 1);

                TlpLayout = tlpState;
            }
        }

        public void SelectPreviousItem()
        {
            if (ActiveIndex > 0)
            {
                var tlpState = TlpLayout;
                TlpLayout = false;
                
                ChangeByIndex(ActiveIndex - 1);
                
                TlpLayout = tlpState;
            }
        }

        internal void ScrollIntoView(QfcItemViewer item)
        {
            // If top is not visible, scroll top into view
            if (_itemPanel.Top - _itemPanel.AutoScrollPosition.Y > item.Top)
            {
                _itemPanel.AutoScrollPosition = new System.Drawing.Point(_itemPanel.AutoScrollPosition.X, item.Top);
            }
            // Else if bottom is not visible, scroll bottom into view
            else if (item.Bottom > (_itemPanel.Bottom - _itemPanel.AutoScrollPosition.Y))
            {
                int yScroll = Math.Max(0, item.Bottom - _itemPanel.Height + _itemPanel.Top);
                _itemPanel.AutoScrollPosition = new System.Drawing.Point(_itemPanel.AutoScrollPosition.X, yScroll);
            }
            // Else do nothing
        }
        
        public void ToggleExpansionStyle(int itemIndex, Enums.ToggleState desiredState)
        {
            if (itemIndex < 0 || itemIndex >= _itemGroups.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(itemIndex), 
                    $"{nameof(itemIndex)} value of {itemIndex} must be in the range of 0 to {_itemGroups.Count -1}");
            }

            if (!_itemGroups[itemIndex].ItemController.IsActiveUI)
            {
                var c = _itemGroups[itemIndex].ItemController;
                var msg = $"Cannot expand item with index {itemIndex} because UI is not active.\n"+
                    $"Controller for message \"{c.Subject} sent on {c.SentDate} at {c.SentTime} "+
                    $"by {c.Sender} has a value of {c.IsActiveUI} for {nameof(c.IsActiveUI)}";
                throw new InvalidOperationException(msg);
            }   
            
            float heightChange = 0;
            if (desiredState == Enums.ToggleState.On)
            {
                heightChange = _templateExpanded.Height - _itemTLP.RowStyles[itemIndex].Height;
                _itemTLP.RowStyles[itemIndex] = _templateExpanded.Clone();
            }
            else 
            {
                heightChange = _template.Height - _itemTLP.RowStyles[itemIndex].Height;
                _itemTLP.RowStyles[itemIndex] = _template.Clone();
            }
                
            _itemTLP.MinimumSize = new System.Drawing.Size(
                    _itemTLP.MinimumSize.Width,
                    _itemTLP.MinimumSize.Height +
                    (int)Math.Round(heightChange, 0));
            
            if (heightChange < 0)
            {
                _itemTLP.Invoke(new System.Action(() => _itemTLP.Height += (int)Math.Round(heightChange, 0)));
            }

            if (desiredState == Enums.ToggleState.On)
                ScrollIntoView(_itemGroups[itemIndex].ItemViewer);
        }

        public void ToggleOffNavigation(bool async)
        {
            if (ActiveIndex != -1) { ToggleOffActiveItem(false); }
            _itemGroups.ForEach(
                        itemGroup => itemGroup
                        .ItemController
                        .ToggleNavigation(
                            async: async,
                            desiredState: Enums.ToggleState.Off));
            //_keyboardHandler.KbdActive = false;
        }

        public void ToggleOnNavigation(bool async)
        {
            _itemGroups.ForEach(
                        itemGroup => itemGroup
                        .ItemController
                        .ToggleNavigation(
                            async: async,
                            desiredState: Enums.ToggleState.On));
            if (ActiveIndex != -1)
            {
                ActivateByIndex(ActiveIndex, false);
            }
        }

        public bool ToggleOffActiveItem(bool parentBlExpanded)
        {
            bool blExpanded = parentBlExpanded;
            if ((ActiveIndex != -1) && _keyboardHandler.KbdActive)
            {
                //adjusted to _intActiveSelection -1 to accommodate zero based
                IQfcItemController itemController = _itemGroups[ActiveIndex].ItemController;

                if (itemController.IsExpanded)
                {
                    //TODO: Replace MoveDownPix Function
                    //MoveDownPix(_intActiveSelection + 1, (int)Math.Round(itemController.ItemPanel.Height * -0.5d));
                    itemController.ToggleExpansion();
                    blExpanded = true;
                }
                itemController.ToggleFocus(Enums.ToggleState.Off);
            }
            return blExpanded;
        }

        #endregion
                
        # region UI Converations Expansion

        /// <summary>
        /// Changes the conversation checkbox state of the item viewer at the 
        /// specified index without raising events
        /// </summary>
        /// <param name="indexOriginal">Index of the group to change</param>
        /// <param name="desiredState">Checked is true or false</param>
        public void ChangeConversationSilently(int indexOriginal, bool desiredState) 
        {
            ChangeConversationSilently(_itemGroups[indexOriginal], desiredState);
        }

        /// <summary>
        /// Changes the conversation checkbox state of the item viewer within 
        /// the group without raising events
        /// </summary>
        /// <param name="grp">Item group containing the item viewer</param>
        /// <param name="desiredState">Checked is true or false</param>
        public void ChangeConversationSilently(ItemGroup grp, bool desiredState)
        {
            var suppressionState = grp.ItemController.SuppressEvents;
            grp.ItemController.SuppressEvents = true;
            grp.ItemViewer.CbxConversation.Checked = desiredState;
            grp.ItemController.SuppressEvents = suppressionState;
        }

        public void ToggleGroupConv(string originalId)
        {
            int childCount = _itemGroups.Where(itemGroup => itemGroup.ItemController.ConvOriginID == originalId).Count();
            int indexOriginal = _itemGroups.FindIndex(itemGroup => itemGroup.ItemController.Mail.EntryID == originalId);

            // if original has been removed, find the first child and set it as the original
            if (indexOriginal == -1) { indexOriginal = PromoteFirstChild(originalId, ref childCount); }

            // ensure the original is checked
            ChangeConversationSilently(indexOriginal, true);

            // if there are children, collapse them into the original
            if (childCount > 0) 
            {
                bool reactivate = false;
                if (ActiveIndex!=-1 && (ActiveIndex != indexOriginal))
                {
                    reactivate = true;
                    ToggleOffActiveItem(false);
                }
                ToggleGroupConv(childCount, indexOriginal); 
                if (reactivate) { ActivateByIndex(indexOriginal, false);}
            }
        }

        public void ToggleGroupConv(int childCount, int indexOriginal)
        {
            var tlpState = TlpLayout;
            TlpLayout = false;

            int removalIndex = indexOriginal + 1;

            var qfOriginal = _itemGroups[indexOriginal].ItemController;
            TableLayoutHelper.RemoveSpecificRow(_itemTLP, removalIndex, childCount);
            
            for (int i = 0; i < childCount; i++)
            {
                _itemGroups[removalIndex].ItemController.Cleanup();
                _itemGroups.RemoveAt(removalIndex);
            }

            RenumberGroups();

            TlpLayout = tlpState;
        }

        /// <summary>
        /// Expands each member of a conversation into its own ItemViewer/ItemController while replicating
        /// the sorting suggestions of the base member
        /// </summary>
        /// <param name="mailItems">Qualifying Conversation Members</param>
        /// <param name="baseEmailIndex">Index of base member in collection</param>
        /// <param name="conversationCount">Number of qualifying conversation members</param>
        /// <param name="folderList">Sorting suggestions from base member</param>
        public void ToggleUnGroupConv(IList<MailItem> mailItems,
                                       string entryID,
                                       int conversationCount,
                                       object folderList)
        {
            var tlpState = TlpLayout;
            TlpLayout = false;
            int baseEmailIndex = _itemGroups.FindIndex(itemGroup => itemGroup.ItemController.Mail.EntryID == entryID);
            int insertionIndex = baseEmailIndex + 1;
            int insertCount = conversationCount - 1;

            if (insertCount > 0)
            {
                MakeSpaceToEnumerateConversation(insertionIndex,
                                                 insertCount);
                
                EnumerateConversationMembers(entryID,
                                             mailItems,
                                             insertionIndex,
                                             conversationCount,
                                             folderList);
            }
            TlpLayout = tlpState;
        }

        /// <summary>
        /// Parallel function to expand each member of a conversation into individual ItemViewers/Controllers.
        /// Expanded members are inserted into the base collection and conversation count and folder suggestions
        /// are replicated from the base member. This enables distinct actions to be taken with each member
        /// </summary>
        /// <param name="mailItems">List of MailItems in a conversation</param>
        /// <param name="insertionIndex">Location of the Item Group collection where the base member is stored</param>
        /// <param name="conversationCount">Number of qualifying conversation members</param>
        /// <param name="folderList">Folder suggestions for the first email</param>
        public void EnumerateConversationMembers(string entryID, IList<MailItem> mailItems, int insertionIndex, int conversationCount, object folderList)
        {
            var insertions = mailItems.Where(mailItem => mailItem.EntryID != entryID)
                                      .OrderByDescending(mailItem => mailItem.SentOn)
                                      .ToList();

            //Enumerable.Range(0, insertions.Count).AsParallel().ForEach(i =>
            Enumerable.Range(0, insertions.Count).AsParallel().ForEach(i =>
            {
                var grp = _itemGroups[i + insertionIndex];
                grp.ItemViewer = LoadItemViewer(i + insertionIndex, _template, false, 1);
                grp.MailItem = insertions[i];
                grp.ItemController = new QfcItemController(AppGlobals: _globals,
                                                           homeController: _homeController,
                                                           parent: this,
                                                           itemViewer: grp.ItemViewer,
                                                           viewerPosition: i + insertionIndex + 1,
                                                           grp.MailItem);

                grp.ItemController.PopulateConversation(conversationCount);
                grp.ItemController.PopulateFolderCombobox(folderList);
                grp.ItemController.IsChild = true;
                grp.ItemController.ConvOriginID = _itemGroups[insertionIndex-1].MailItem.EntryID;
                if (_keyboardHandler.KbdActive) { grp.ItemController.ToggleNavigation(async: true, desiredState: Enums.ToggleState.On); }

                if (_darkMode) { grp.ItemController.SetThemeDark(async: true); }
                else { grp.ItemController.SetThemeLight(async: true); }
                ChangeConversationSilently(grp, false);
                
            });
        }

        public int PromoteFirstChild(string originalId, ref int childCount)
        {
            int indexOriginal = _itemGroups.FindIndex(itemGroup => itemGroup.ItemController.ConvOriginID == originalId);
            var itemViewer = _itemGroups[indexOriginal].ItemViewer;
            _itemTLP.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(0, indexOriginal));
            _itemTLP.SetColumnSpan(itemViewer, 2);
            _itemGroups[indexOriginal].ItemController.ConvOriginID = "";
            _itemGroups[indexOriginal].ItemController.IsChild = false;
            childCount--;
            return indexOriginal;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Creates empty item groups and inserts them into the 
        /// collection at the targeted location
        /// </summary>
        /// <param name="insertionIndex">Targeted location for the insertion</param>
        /// <param name="insertCount">Number of elements to insert</param>
        public void InsertItemGroups(int insertionIndex, int insertCount)
        {
            for (int i = 0; i < insertCount; i++)
            {
                var grp = new ItemGroup();
                _itemGroups.Insert(insertionIndex, grp);
            }
        }
        
        public void MakeSpaceToEnumerateConversation(int insertionIndex, int insertCount)
        {
            TableLayoutHelper.InsertSpecificRow(panel: _itemTLP,
                                                rowIndex: insertionIndex,
                                                templateStyle: _template,
                                                insertCount: insertCount);
            InsertItemGroups(insertionIndex, insertCount);
            RenumberGroups(insertionIndex+insertCount);
        }

        public void UpdateSelectionForRemoval(int selection)
        {
            // Adjust the active selection if necessary
            if (ActiveSelection == selection)
            {
                if (selection == _itemGroups.Count)
                {
                    // Removing the last item so select the previous item
                    ActiveSelection--;
                }
                // Else do nothing becauuse the next item will become the active selection when renumbered
            }
            else if (ActiveSelection > selection)
            {
                // Else if the active selection is greater than the selection,
                // decrement the active index to keep it in sync
                ActiveIndex--;
            }
        }

        public void RemoveSpaceToCollapseConversation()
        {
            // Perhaps can eliminate
            throw new NotImplementedException();
        }
        
        public void RenumberGroups()
        {
            for (int i = 0; i < _itemGroups.Count; i++)
            {
                _itemGroups[i].ItemController.ItemNumber = i + 1;
            }
        }

        public void RenumberGroups(int beginningIndex)
        {
            for (int i = beginningIndex; i < _itemGroups.Count; i++)
            {
                _itemGroups[i].ItemController.ItemNumber = i + 1;
            }
        }

        public void ResetPanelHeight()
        {
            var ht = 0;
            _itemTLP.Invoke(new System.Action(() =>
            {
                for (int i = 0; i < _itemTLP.RowStyles.Count - 1; i++)
                {
                    ht += (int)Math.Round(_itemTLP.RowStyles[i].Height, 0);
                }

                _itemTLP.MinimumSize = new System.Drawing.Size(
                    _itemTLP.MinimumSize.Width, ht);
                //ht = _itemTLP.Height - (int)Math.Round(_itemTLP.RowStyles[_itemTLP.RowStyles.Count - 1].Height, 0);
                //ht = Math.Max(_parent.SpaceForEmail, ht);
                _itemTLP.Height = ht;
            }));
            var panel = _itemTLP.Parent;
            panel.Invoke(new System.Action(() => panel.Height = ht));
        }

        #endregion

        #region UI Light Dark

        public void SetupLightDark(bool initDarkMode)
        {
            _darkMode = initDarkMode;
            _formViewer.DarkMode.CheckedChanged += new System.EventHandler(DarkMode_CheckedChanged);
            
        }

        public void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_formViewer.DarkMode.Checked==true)
            {
                SetDarkMode(async: true);
            }
            else
            {
                SetLightMode(async: true);
            }
        }

        public void SetDarkMode(bool async)
        {
            foreach (ItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeDark(async);
            }
        }

        public void SetLightMode(bool async)
        {
            foreach (ItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeLight(async);
            }
        }

        #endregion

        #region Major Actions

        public void Cleanup()
        {
            RemoveControls();
            _formViewer = null;
            _globals = null;
            _parent = null;
            _itemTLP = null;
            _itemGroups = null;
        }

        public void MoveEmails(StackObjectCS<MailItem> stackMovedItems)
        {
            foreach (var grp in _itemGroups)
            {
                //TODO: function needed to shut off KeyboardDialog at this step if active
                grp.ItemController.MoveMail();
                stackMovedItems.Push(grp.MailItem);
            }
        }

        public string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment)
        {
            int k;
            string[] strOutput = new string[EmailsLoaded + 1];
            var loopTo = EmailsLoaded;
            for (k = 0; k < loopTo; k++)
            {
                var QF = _itemGroups[k].ItemController;
                var infoMail = new cInfoMail();
                if (infoMail.Init_wMail(QF.Mail, OlEndTime: OlEndTime, lngDurationSec: (int)Math.Round(Duration)))
                {
                    if (string.IsNullOrEmpty(OlAppointment.Body))
                    {
                        OlAppointment.Body = infoMail.ToString;
                        OlAppointment.Save();
                    }
                    else
                    {
                        OlAppointment.Body = OlAppointment.Body + System.Environment.NewLine + infoMail.ToString;
                        OlAppointment.Save();
                    }
                }
                string dataLine = dataLineBeg + xComma(QF.Subject);
                dataLine = dataLine + "," + "QuickFiled";
                dataLine = dataLine + "," + durationText;
                dataLine = dataLine + "," + durationMinutesText;
                dataLine = dataLine + "," + xComma(QF.To);
                dataLine = dataLine + "," + xComma(QF.Sender);
                dataLine = dataLine + "," + "Email";
                dataLine = dataLine + "," + xComma(QF.SelectedFolder);           // Target Folder
                dataLine = dataLine + "," + QF.SentDate;
                dataLine = dataLine + "," + QF.SentTime;
                strOutput[k] = dataLine;
            }

            return strOutput;
        }

        public string xComma(string str)
        {
            string xCommaRet = default;
            string strTmp;

            strTmp = str.Replace(", ", "_");
            strTmp = strTmp.Replace(",", "_");
            xCommaRet = StringManipulation.GetStrippedText(strTmp);
            return xCommaRet;
            // xComma = StripAccents(strTmp)
        }

        #endregion

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
