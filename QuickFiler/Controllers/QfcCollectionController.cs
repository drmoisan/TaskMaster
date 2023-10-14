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
using QuickFiler.Helper_Classes;
using System.Threading;
using System.Net.NetworkInformation;

namespace QuickFiler.Controllers
{
    public class QfcCollectionController : IQfcCollectionController
    {
        #region Constructors

        public QfcCollectionController(IApplicationGlobals AppGlobals,
                                       QfcFormViewer viewerInstance,
                                       bool darkMode,
                                       QfEnums.InitTypeEnum InitType,
                                       IFilerHomeController homeController,
                                       IFilerFormController parent,
                                       CancellationTokenSource tokenSource,
                                       CancellationToken token)
        {
            _token = token;
            _tokenSource = tokenSource;
            _formViewer = viewerInstance;
            _itemTlp = _formViewer.L1v0L2L3v_TableLayout;
            _itemPanel = _formViewer.L1v0L2_PanelMain;
            _initType = InitType;
            _globals = AppGlobals;
            _homeController = homeController;
            _kbdHandler = _homeController.KeyboardHndlr;
            _parent = parent;
            SetupLightDark(darkMode);
        }

        #endregion

        #region Private Variables

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcFormViewer _formViewer;
        private QfEnums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IFilerHomeController _homeController;
        private IFilerFormController _parent;
        //private int _itemHeight;
        private Panel _itemPanel;
        private TableLayoutPanel _itemTlp;
        private TableLayoutPanel _itemTlpToMove;
        private TableLayoutPanel _templateTlp;
        private List<QfcItemGroup> _itemGroups;
        private List<QfcItemGroup> _itemGroupsToMove;
        private bool _darkMode;
        private RowStyle _template;
        private RowStyle _templateExpanded;
        private IQfcKeyboardHandler _kbdHandler;
        private delegate int ActionDelegate(int intNewSelection, bool blExpanded);

        #endregion

        #region Public Properties

        private int _activeIndex = -1;
        public int ActiveIndex { get => _activeIndex; set => _activeIndex = value; }
        public int ActiveSelection { get => _activeIndex + 1; set => _activeIndex = value - 1; }

        private CancellationToken _token;
        public CancellationToken Token { get => _token; set => _token = value; }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        public int EmailsLoaded => _itemGroups.Count;

        public int EmailsToMove => _itemGroupsToMove.Count;

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
                        _itemTlp.ResumeLayout(true);
                    }
                    else
                    {
                        _itemTlp.SuspendLayout();
                    }
                }
            }
        }    

        #endregion

        #region UI Add and Remove QfcItems

        public void LoadControlsAndHandlers_01(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)
        {
            _formViewer.SuspendLayout();
            SwapTlp(tlp);
            SwapItemGroups(itemGroups);
            _formViewer.ResumeLayout();
        }

        public void LoadControlsAndHandlers_01(IList<MailItem> listMailItems, RowStyle template, RowStyle templateExpanded)
        {
            // Freeze the form while loading controls
            _formViewer.SuspendLayout();
            var tlpState = TlpLayout;
            TlpLayout = false;

            // Save the QfcItem template styles
            _template = template;
            _templateExpanded = templateExpanded;

            // Save the TLP template
            CaptureTlpTemplate();

            LoadItemGroupsAndViewers_02(listMailItems, template);

            _formViewer.WindowState = FormWindowState.Maximized;
            TlpLayout = tlpState;

            _formViewer.ResumeLayout();
            //WireUpKeyboardHandler();
            WireUpAsyncKeyboardHandler();
            LoadConversationsAndFolders_04();

        }

        public async Task LoadControlsAndHandlersAsync_01(IList<MailItem> listMailItems, RowStyle template, RowStyle templateExpanded)
        {
            // Freeze the form while loading controls
            _formViewer.SuspendLayout();
            var tlpState = TlpLayout;
            TlpLayout = false;

            // Save the QfcItem template styles
            _template = template;
            _templateExpanded = templateExpanded;
            
            // Prep the TLP
            TableLayoutHelper.InsertSpecificRow(_itemTlp, 0, template, listMailItems.Count);
            CaptureTlpTemplate();

            // Load the Item Viewers, Item Controllers, and Initialize
            await LoadGroups_02b(listMailItems, template);
            //LoadItemGroupsAndViewers_02(listMailItems, template);
            WireUpAsyncKeyboardHandler();
            //await LoadConversationsAndFoldersAsync();
            
            // Restore state of window
            _formViewer.WindowState = FormWindowState.Maximized;
            TlpLayout = tlpState;
            _formViewer.ResumeLayout();

        }

        public async Task LoadGroups_02b(IList<MailItem> items, RowStyle template) 
        {
            //_itemGroups = new List<QfcItemGroup>();
            _kbdHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
            _kbdHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
            
            var grpTasks = items.Select((mailItem, i) => LoadGroup_03b(template, mailItem, i)).ToList();

            await Task.WhenAll(grpTasks);

            _itemGroups = grpTasks.Select(x => x.Result).ToList();
            //var tmp = items.ToAsyncEnumerable().Select((mailItem, i) => LoadGroup_03(template, mailItem, i)).ToListAsync();
                           
        }

        //public async Task LoadGroups_02(IList<MailItem> items, RowStyle template)
        //{
        //    //_itemGroups = new List<QfcItemGroup>();
        //    _kbdHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
        //    _kbdHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
        //    _itemGroups = await items.ToAsyncEnumerable()
        //                       .SelectAwait((mailItem, i) => LoadGroup_03(template, mailItem, i))
        //                       .ToListAsync();
        //    //var tmp = items.ToAsyncEnumerable().Select((mailItem, i) => LoadGroup_03(template, mailItem, i)).ToListAsync();

        //}

        private Task<QfcItemGroup> LoadGroup_03b(RowStyle template, MailItem mailItem, int i)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();

            var grpTask = Task.Factory.StartNew(() =>
            {
                var grp = new QfcItemGroup(mailItem);
                grp.ItemViewer = LoadItemViewer_03(i, template, true);
                return grp;
            }, Token, TaskCreationOptions.None, ui);
            
            var grpTask2 = grpTask.ContinueWith(async x =>
            {
                var grp = x.Result;
                grp.ItemController = await QfcItemController.CreateSequentialAsync(
                    _globals, _homeController, this, grp.ItemViewer, i + 1, grp.MailItem, Token);
                return grp;
            }, Token, TaskContinuationOptions.OnlyOnRanToCompletion, ui).Unwrap();
            
            var grpTask3 = grpTask2.ContinueWith(x => 
            {
                var grp = x.Result;
                //var tasks = new List<Task>
                //{
                //    grp.ItemController.PopulateConversationAsync(TokenSource, Token, false),
                //    Task.Run(() => grp.ItemController.PopulateConversation()),
                //    Task.Run(() => grp.ItemController.PopulateFolderCombobox()),
                //};
                Task.Factory.StartNew(() => grp.ItemController.PopulateConversationAsync(TokenSource, Token, false), Token, TaskCreationOptions.AttachedToParent, ui);
                Task.Factory.StartNew(() => grp.ItemController.PopulateFolderComboboxAsync(Token), Token, TaskCreationOptions.AttachedToParent, ui);
                //Task.Factory.StartNew(() =>
                //{
                //    if (_darkMode) { grp.ItemController.SetThemeDark(async: true); }
                //    else { grp.ItemController.SetThemeLight(async: true); }
                //}, Token, TaskCreationOptions.AttachedToParent, ui);
                return grp;
            }, Token, TaskContinuationOptions.OnlyOnRanToCompletion, ui);
            
            return grpTask3;
        }

        private async ValueTask<QfcItemGroup> LoadGroup_03(RowStyle template, MailItem mailItem, int i)
        {
            var grp = new QfcItemGroup(mailItem);
            grp.ItemViewer = LoadItemViewer_03(i, template, true);
            grp.ItemController = await QfcItemController.CreateSequentialAsync(_globals,
                _homeController, this, grp.ItemViewer, i + 1, grp.MailItem, Token);
            grp.ItemController.PopulateFolderCombobox();
            if (_darkMode) { grp.ItemController.SetThemeDark(async: true); }
            else { grp.ItemController.SetThemeLight(async: true); }
            return grp;
        }

        public void LoadItemGroupsAndViewers_02(IList<MailItem> items, RowStyle template)
        {
            _itemGroups = new List<QfcItemGroup>();
            _kbdHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
            _kbdHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
            
            int i = 0;
            foreach (MailItem mailItem in items)
            {
                QfcItemGroup grp = new(mailItem);
                _itemGroups.Add(grp);
                grp.ItemViewer = LoadItemViewer_03(i, template, true);
                i++;
            }

        }

        public void LoadConversationsAndFolders_04()
        {
            LoadSequential_5(); 
        }

        public async Task LoadConversationsAndFoldersAsync()
        {
            await AsyncEnumerable.Range(0, _itemGroups.Count)
                                 .Select(i => (i,grp:_itemGroups[i]))
                                 .ForEachAsync(async x => 
                                 { 
                                     x.grp.ItemController = new QfcItemController(AppGlobals: _globals,
                                                                                  homeController: _homeController,
                                                                                  parent: this,
                                                                                  itemViewer: x.grp.ItemViewer,
                                                                                  viewerPosition: x.i + 1,
                                                                                  x.grp.MailItem);
                                     var tasks = new List<Task>
                                     {
                                        x.grp.ItemController.InitializeAsync(),
                                        Task.Run(() => x.grp.ItemController.PopulateConversation()),
                                        Task.Run(() => x.grp.ItemController.PopulateFolderCombobox()),
                                     };
                                     await Task.WhenAll(tasks).ConfigureAwait(false);
                                 });
        }

        public void LoadSequential_5()
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
                grp.ItemController.Initialize(false);
                grp.ItemController.PopulateConversation();
                grp.ItemController.PopulateFolderCombobox();
                if (_darkMode) { grp.ItemController.SetThemeDark(async: false); }
                else { grp.ItemController.SetThemeLight(async: false); }
            }
        }

        public async Task LoadSequentialAsync()
        {
            await AsyncEnumerable.Range(0, _itemGroups.Count)
                                 .Select(i => (i, grp: _itemGroups[i]))
                                 .ForEachAsync(async x =>
                                 {
                                     x.grp.ItemController = new QfcItemController(AppGlobals: _globals,
                                                                                  homeController: _homeController,
                                                                                  parent: this,
                                                                                  itemViewer: x.grp.ItemViewer,
                                                                                  viewerPosition: x.i + 1,
                                                                                  x.grp.MailItem);
                                     var tasks = new List<Task>
                                     {
                                        x.grp.ItemController.InitializeAsync(),
                                        Task.Run(() => x.grp.ItemController.PopulateConversation()),
                                        Task.Run(() => x.grp.ItemController.PopulateFolderCombobox()),
                                     };
                                     await Task.WhenAll(tasks).ConfigureAwait(false);
                                 });
        }
        
        internal void SwapTlp(TableLayoutPanel tlp)
        {
            // Cache parent of current tlp and cache the original tlp to a variable for background processing
            var tlpParent = _formViewer.L1v0L2L3v_TableLayout.Parent;
            _itemTlpToMove = _formViewer.L1v0L2L3v_TableLayout;

            // Remove current tlp from parent and replace with new tlp
            _formViewer.L1v0L2L3v_TableLayout.Parent = null;
            _formViewer.L1v0L2L3v_TableLayout = tlp;
            _formViewer.L1v0L2L3v_TableLayout.Parent = tlpParent;
            _formViewer.L1v0L2L3v_TableLayout.Visible = true;
            // Cache handle of new tlp
            _itemTlp = _formViewer.L1v0L2L3v_TableLayout;
        }

        internal void SwapItemGroups(List<QfcItemGroup> itemGroups)
        {
            // Cache current item groups to process in background
            _itemGroupsToMove = _itemGroups;

            // Replace current item groups with new item groups to process
            _itemGroups = itemGroups;
        }

        public void CacheMoveObjects()
        {
            _itemTlpToMove = _formViewer.L1v0L2L3v_TableLayout;
            _itemGroupsToMove = _itemGroups;
        }
                
        public ItemViewer LoadItemViewer_03(int indexNumber,
                                         RowStyle template,
                                         bool blGroupConversation = true,
                                         int columnNumber = 0)
        {
            var itemViewer = ItemViewerQueue.Dequeue(_homeController.Token);

            itemViewer.Parent = _itemTlp;
            if (columnNumber == 0)
            {
                _itemTlp.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(columnNumber, indexNumber));
                _itemTlp.SetColumnSpan(itemViewer, 2);
            }
            else
            {
                _itemTlp.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(1, indexNumber));
                _itemTlp.SetColumnSpan(itemViewer, 1);
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

            var popOutForm = new EfcHomeController(_globals, () => { }, mailItem);
            popOutForm.Run();
        }

        public async Task PopOutControlGroupAsync(int selection)
        {
            Token.ThrowIfCancellationRequested();

            // Get mail item from the group            
            MailItem mailItem = _itemGroups[selection - 1].MailItem;

            // Remove the group from the form
            await RemoveSpecificControlGroupAsync(selection);

            var popOutForm = new EfcHomeController(_globals, () => { }, mailItem);
            await popOutForm.RunAsync();
        }

        public void RemoveControls()
        {
            if (_itemGroups is not null)
            {
                var tlpState = TlpLayout;
                TlpLayout = false;

                // Remove Item Viewers and Rows from the form
                TableLayoutHelper.RemoveSpecificRow(_itemTlp, 0, _itemGroups.Count);

                ResetPanelHeight();

                _itemGroups.ForEach(grp => grp.ItemController.Cleanup());

                _itemGroups.Clear();

                TlpLayout = tlpState;
            }
        }

        public void CleanupBackground()
        {
            if (_itemGroupsToMove is not null)
            {
                _itemGroupsToMove.ForEach(grp => grp.ItemController.Cleanup());
                _itemGroupsToMove.Clear();
            }
            if (_itemTlpToMove is not null)
                _itemTlpToMove.Dispose();
        }

        async public Task RemoveControlsAsync()
        {
            if (_itemGroups is not null)
            {
                await _formViewer.UiSyncContext;

                var tlpState = TlpLayout;
                TlpLayout = false;

                // Remove Item Viewers and Rows from the form
                TableLayoutHelper.RemoveSpecificRow(_itemTlp, 0, _itemGroups.Count);

                await ResetPanelHeightAsync();

                _itemGroups.ForEach(grp => grp.ItemController.Cleanup());
                
                _itemGroups.Clear();

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
            TableLayoutHelper.RemoveSpecificRow(_itemTlp, selection - 1);

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
            else if (_itemGroups.Count == 0 && _kbdHandler.KbdActive) 
            { 
                _kbdHandler.ToggleKeyboardDialog(); 
            }

            TlpLayout = tlpState;
            ResetPanelHeight();

        }

        public async Task RemoveSpecificControlGroupAsync(int selection)
        {
            // If the group is active, turn off the active item and select a new item
            bool activeUI = _itemGroups[selection - 1].ItemController.IsActiveUI;
            bool expanded = _itemGroups[selection - 1].ItemController.IsExpanded;
            if (activeUI) { await ToggleOffActiveItemAsync(false); }

            UpdateSelectionForRemoval(selection);

            bool tlpState = TlpLayout;
            
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                tlpState = TlpLayout;
                TlpLayout = false;

                // Remove the controls from the form
                TableLayoutHelper.RemoveSpecificRow(_itemTlp, selection - 1);
            });
            
            // Remove the group from the list of groups
            _itemGroups.RemoveAt(selection - 1);

            if (_itemGroups.Count > 0)
            {
                // Renumber the remaining groups
                await UIThreadExtensions.UiDispatcher.InvokeAsync(() => RenumberGroups());

                // Restore UI to previous state with newly selected item
                if (activeUI)
                {
                    await _itemGroups[ActiveIndex].ItemController.ToggleFocusAsync(Enums.ToggleState.On);
                    if (expanded) { await _itemGroups[ActiveIndex].ItemController.ToggleExpansionAsync(); }
                }
            }
            else if (_itemGroups.Count == 0 && _kbdHandler.KbdActive)
            {
                await _kbdHandler.ToggleKeyboardDialogAsync();
            }

            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => 
            { 
                TlpLayout = tlpState;
                ResetPanelHeight();
            });

        }

        public void WireUpKeyboardHandler()
        {
            // Treatment as char limits to 9 numbered items and 26 character items
            for (int i = 0; i < _itemGroups.Count && i < 10; i++)
            {
                _kbdHandler.CharActions.Add(
                    "Collection",
                    (i + 1).ToString()[0],
                    (c) => ChangeByIndex(int.Parse(c.ToString()) - 1));
            }
            _kbdHandler.KeyActions = new KbdActions<Keys, KaKey, Action<Keys>>(
                new List<KaKey>
                {
                    new KaKey("Collection", Keys.Up, (k) => SelectPreviousItem()),
                    new KaKey("Collection", Keys.Down, (k) => SelectNextItem())
                });
        }

        public void WireUpAsyncKeyboardHandler()
        {
            // Treatment as char limits to 9 numbered items and 26 character items
            for (int i = 0; i < _itemGroups.Count && i < 10; i++)
            {
                _kbdHandler.CharActionsAsync.Add(
                    "Collection",
                    (i + 1).ToString()[0],
                    (c) => ChangeByIndexAsync(int.Parse(c.ToString()) - 1));
            }
            _kbdHandler.KeyActionsAsync = new KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>(
                new List<KaKeyAsync>
                {
                    new KaKeyAsync("Collection", Keys.Up, (k) => SelectPreviousItemAsync()),
                    new KaKeyAsync("Collection", Keys.Down, (k) => SelectNextItemAsync())
                });
        }



        #endregion

        #region UI Select QfcItems

        public int ActivateByIndex(int newIndex, bool blExpanded)
        {
            return ActivateBySelection(newIndex + 1, blExpanded);
        }

        public async Task<int> ActivateByIndexAsync(int newIndex, bool blExpanded)
        {
            
            return await ActivateBySelectionAsync(newIndex + 1, blExpanded);
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
                itemViewer.LblSubject.Focus();

                ActiveSelection = intNewSelection;

                TlpLayout = tlpState;
            }
            return ActiveSelection;
        }

        public async Task<int> ActivateBySelectionAsync(int intNewSelection, bool blExpanded)
        {
            if (intNewSelection > 0 & intNewSelection <= _itemGroups.Count)
            {
                var tlpState = TlpLayout;
                TlpLayout = false;

                var itemController = _itemGroups[intNewSelection - 1].ItemController;
                var itemViewer = _itemGroups[intNewSelection - 1].ItemViewer;

                await itemController.ToggleFocusAsync();
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

        public async Task ChangeByIndexAsync(int idx)
        {
            bool expanded = false;
            if ((ActiveIndex != idx) && (idx < _itemGroups.Count))
            {
                bool tlpState = true;
                await UIThreadExtensions.UiDispatcher.InvokeAsync(() => 
                { 
                    tlpState = TlpLayout;
                    TlpLayout = false;
                });

                if (ActiveIndex != -1)
                    expanded = await ToggleOffActiveItemAsync(false);
                await ActivateBySelectionAsync(idx + 1, expanded);

                await UIThreadExtensions.UiDispatcher.InvokeAsync(() => TlpLayout = tlpState);
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

        public async Task SelectNextItemAsync()
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => SelectNextItem());
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

        public async Task SelectPreviousItemAsync()
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => SelectPreviousItem());
        }

        internal void ScrollIntoView(ItemViewer item)
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
                heightChange = _templateExpanded.Height - _itemTlp.RowStyles[itemIndex].Height;
                _itemTlp.RowStyles[itemIndex] = _templateExpanded.Clone();
            }
            else 
            {
                heightChange = _template.Height - _itemTlp.RowStyles[itemIndex].Height;
                _itemTlp.RowStyles[itemIndex] = _template.Clone();
            }
                
            _itemTlp.MinimumSize = new System.Drawing.Size(
                    _itemTlp.MinimumSize.Width,
                    _itemTlp.MinimumSize.Height +
                    (int)Math.Round(heightChange, 0));
            
            if (heightChange < 0)
            {
                _itemTlp.Invoke(new System.Action(() => _itemTlp.Height += (int)Math.Round(heightChange, 0)));
            }

            if (desiredState == Enums.ToggleState.On)
                ScrollIntoView(_itemGroups[itemIndex].ItemViewer);
        }

        public async Task ToggleExpansionStyleAsync(int itemIndex, Enums.ToggleState desiredState)
        {
            Token.ThrowIfCancellationRequested();

            await UIThreadExtensions.UiDispatcher.InvokeAsync(()=>ToggleExpansionStyle(itemIndex, desiredState));
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

        public async Task ToggleOffNavigationAsync()
        {
            bool tlpState = true;
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                tlpState = TlpLayout;
                TlpLayout = false;
            });

            if (ActiveIndex != -1) { await ToggleOffActiveItemAsync(false); }
            var tasks = _itemGroups.Select(itemGroup => itemGroup.ItemController.ToggleNavigationAsync(Enums.ToggleState.Off)).ToList();
            await Task.WhenAll(tasks);

            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => TlpLayout = tlpState);
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

        public async Task ToggleOnNavigationAsync()
        {
            bool tlpState = true;
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                tlpState = TlpLayout;
                TlpLayout = false;
            });

            var tasks = _itemGroups.Select(itemGroup => itemGroup.ItemController.ToggleNavigationAsync(Enums.ToggleState.On)).ToList();
            await Task.WhenAll(tasks);

            if (ActiveIndex != -1)
            {
                await ActivateByIndexAsync(ActiveIndex, false);
            }

            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => TlpLayout = tlpState);
        }

        public bool ToggleOffActiveItem(bool parentBlExpanded)
        {
            bool blExpanded = parentBlExpanded;
            if ((ActiveIndex != -1) && _kbdHandler.KbdActive)
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

        public async Task<bool> ToggleOffActiveItemAsync(bool parentBlExpanded)
        {
            bool blExpanded = parentBlExpanded;
            if ((ActiveIndex != -1) && _kbdHandler.KbdActive)
            {
                //adjusted to _intActiveSelection -1 to accommodate zero based
                IQfcItemController itemController = _itemGroups[ActiveIndex].ItemController;

                if (itemController.IsExpanded)
                {
                    //TODO: Replace MoveDownPix Function
                    //MoveDownPix(_intActiveSelection + 1, (int)Math.Round(itemController.ItemPanel.Height * -0.5d));
                    await itemController.ToggleExpansionAsync();
                    blExpanded = true;
                }
                await itemController.ToggleFocusAsync(Enums.ToggleState.Off);
            }
            return blExpanded;
        }

        #endregion

        #region UI Converations Expansion

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
        public void ChangeConversationSilently(QfcItemGroup grp, bool desiredState)
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
            TableLayoutHelper.RemoveSpecificRow(_itemTlp, removalIndex, childCount);
            
            for (int i = 0; i < childCount; i++)
            {
                _itemGroups[removalIndex].ItemController.Cleanup();
                _itemGroups.RemoveAt(removalIndex);
            }

            RenumberGroups();

            _itemTlp.MinimumSize = new System.Drawing.Size(
                _itemTlp.MinimumSize.Width,
                _itemTlp.MinimumSize.Height -
                (int)Math.Round(_template.Height * childCount, 0));

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
        public void ToggleUnGroupConv(ConversationResolver resolver,
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
                                             resolver,
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
        /// <param name="mailInfoList">List of MailItems in a conversation</param>
        /// <param name="insertionIndex">Location of the Item Group collection where the base member is stored</param>
        /// <param name="conversationCount">Number of qualifying conversation members</param>
        /// <param name="folderList">Folder suggestions for the first email</param>
        public void EnumerateConversationMembers(string entryID, ConversationResolver resolver, int insertionIndex, int conversationCount, object folderList)
        {
            var insertions = resolver.ConversationItems.SameFolder
                                     .Where(mailItem => mailItem.EntryID != entryID)
                                     .OrderByDescending(mailItem => mailItem.SentOn)
                                     .ToList();

            _itemTlp.MinimumSize = new System.Drawing.Size(
                _itemTlp.MinimumSize.Width,
                _itemTlp.MinimumSize.Height +
                (int)Math.Round(_template.Height * insertions.Count, 0));

            //Enumerable.Range(0, insertions.Count).AsParallel().ForEach(i =>
            Enumerable.Range(0, insertions.Count).ForEach(i =>
            {
                var grp = _itemGroups[i + insertionIndex];
                grp.ItemViewer = LoadItemViewer_03(i + insertionIndex, _template, false, 1);
                grp.MailItem = insertions[i];
                grp.ItemController = new QfcItemController(AppGlobals: _globals,
                                                           homeController: _homeController,
                                                           parent: this,
                                                           itemViewer: grp.ItemViewer,
                                                           viewerPosition: i + insertionIndex + 1,
                                                           grp.MailItem);

                grp.ItemController.Initialize(false);
                grp.ItemController.PopulateConversation(resolver);
                grp.ItemController.PopulateFolderCombobox(folderList);
                grp.ItemController.IsChild = true;
                grp.ItemController.ConvOriginID = _itemGroups[insertionIndex-1].MailItem.EntryID;
                if (_kbdHandler.KbdActive) { grp.ItemController.ToggleNavigation(async: true, desiredState: Enums.ToggleState.On); }

                if (_darkMode) { grp.ItemController.SetThemeDark(async: true); }
                else { grp.ItemController.SetThemeLight(async: true); }
                ChangeConversationSilently(grp, false);
                
            });
        }

        public int PromoteFirstChild(string originalId, ref int childCount)
        {
            int indexOriginal = _itemGroups.FindIndex(itemGroup => itemGroup.ItemController.ConvOriginID == originalId);
            var itemViewer = _itemGroups[indexOriginal].ItemViewer;
            _itemTlp.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(0, indexOriginal));
            _itemTlp.SetColumnSpan(itemViewer, 2);
            _itemGroups[indexOriginal].ItemController.ConvOriginID = "";
            _itemGroups[indexOriginal].ItemController.IsChild = false;
            childCount--;
            return indexOriginal;
        }

        #endregion

        #region Helper Functions

        internal void CaptureTlpTemplate() 
        {
            //logger.Debug($"Current Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            _templateTlp = _formViewer.L1v0L2L3v_TableLayout.Clone();
            _templateTlp.Name = "TemplateTableLayout";
        }

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
                var grp = new QfcItemGroup();
                _itemGroups.Insert(insertionIndex, grp);
            }
        }
        
        public void MakeSpaceToEnumerateConversation(int insertionIndex, int insertCount)
        {
            TableLayoutHelper.InsertSpecificRow(panel: _itemTlp,
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

        async public Task ResetPanelHeightAsync()
        {
            await _formViewer.UiSyncContext;
            var ht = (int)Math.Round(_itemTlp.RowStyles
                                             .Cast<RowStyle>()
                                             .Sum(rowStyle => rowStyle.Height)
                                     ,0);
            
            _itemTlp.MinimumSize = new System.Drawing.Size(
                _itemTlp.MinimumSize.Width, ht);

            _itemTlp.Height = ht;
            _itemTlp.Parent.Height = ht;
        }
        
        public void ResetPanelHeight()
        {
            var ht = 0;
            _itemTlp.Invoke(new System.Action(() =>
            {
                for (int i = 0; i < _itemTlp.RowStyles.Count - 1; i++)
                {
                    ht += (int)Math.Round(_itemTlp.RowStyles[i].Height, 0);
                }

                _itemTlp.MinimumSize = new System.Drawing.Size(
                    _itemTlp.MinimumSize.Width, ht);
                _itemTlp.Height = ht;
            }));
            var panel = _itemTlp.Parent;
            panel.Invoke(new System.Action(() => panel.Height = ht));
        }

        #endregion

        #region UI Light Dark

        public void SetupLightDark(bool initDarkMode)
        {
            _darkMode = initDarkMode;
            //_formViewer.DarkMode.CheckedChanged += new System.EventHandler(DarkMode_CheckedChanged);
            _globals.Ol.PropertyChanged += DarkMode_CheckedChanged;
            
        }

        public void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            //if (_formViewer.DarkMode.Checked==true)
            if (_globals.Ol.DarkMode)
            {
                SetDarkMode(async: true);
            }
            else
            {
                SetLightMode(async: true);
            }
            _darkMode = _globals.Ol.DarkMode;
        }

        public void SetDarkMode(bool async)
        {
            foreach (QfcItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeDark(async);
            }
        }

        public void SetLightMode(bool async)
        {
            foreach (QfcItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeLight(async);
            }
        }

        #endregion

        #region Major Actions

        async public Task CleanupAsync()
        {
            await RemoveControlsAsync();
            _formViewer = null;
            _globals = null;
            _parent = null;
            _itemTlp = null;
            _itemGroups = null;
        }
        
        public void Cleanup()
        {
            RemoveControls();
            _formViewer = null;
            _globals = null;
            _parent = null;
            _itemTlp = null;
            _itemGroups = null;
        }

        async public Task MoveEmailsAsync(ScoStack<IMovedMailInfo> stackMovedItems)
        {
            await Task.WhenAll(_itemGroupsToMove.Select(grp => grp.ItemController.MoveMailAsync()));
        }

        public string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment)
        {
            int k;
            string[] strOutput = new string[_itemGroupsToMove.Count + 1];
            var loopTo = _itemGroupsToMove.Count;
            for (k = 0; k < loopTo; k++)
            {
                var QF = _itemGroupsToMove[k].ItemController;
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

        public static string xComma(string str)
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


    }
}
