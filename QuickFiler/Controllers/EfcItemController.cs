using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using System.Windows.Forms;
using QuickFiler.Helper_Classes;
using System.Diagnostics;
using System.IO;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using TaskVisualization;
using System.Threading;
using BrightIdeasSoftware;
using static Deedle.FrameBuilder;

namespace QuickFiler.Controllers
{
    internal class EfcItemController : IItemControler
    {
        #region Constructors and Initializers

        public EfcItemController(IApplicationGlobals AppGlobals,
                                 IFilerHomeController homeController,
                                 EfcFormController parent,
                                 ItemViewer itemViewer,
                                 EfcDataModel dataModel)
        {
            Initialize(AppGlobals, homeController, parent, itemViewer, dataModel, async: true);

        }

        public EfcItemController(IApplicationGlobals AppGlobals,
                                 IFilerHomeController homeController,
                                 EfcFormController parent,
                                 ItemViewer itemViewer,
                                 EfcDataModel dataModel,
                                 bool async)
        {
            Initialize(AppGlobals, homeController, parent, itemViewer, dataModel, async);
        }

        private void Initialize(IApplicationGlobals AppGlobals,
                                IFilerHomeController homeController,
                                EfcFormController parent,
                                ItemViewer itemViewer,
                                EfcDataModel dataModel,
                                bool async)
        {
            _globals = AppGlobals;
            _homeController = homeController;

            // Grab handle on viewer and controllers
            _itemViewer = itemViewer;
            _itemViewer.Controller = this;
            _dataModel = dataModel;
            _keyboardHandler = _homeController.KeyboardHndlr;
            _parent = parent;
            _explorerController = _homeController.ExplorerCtlr;

            // Adjust the viewer for Efc purposes
            AdjustViewerForEfc();

            ResolveControlGroups(itemViewer);

            _themes = EfcThemeHelper.SetupThemes(_navCtrls,
                                                 _tipsCtrls,
                                                 _dflt2Ctrls,
                                                 _selectorsCtrls,
                                                 _mailCtrls,
                                                 () => !_dataModel.Mail.UnRead,
                                                 _itemViewer.TopicThread.Columns.Cast<object>().ToList(),
                                                 (columns, fore, back) => SetOlvTheme(columns, fore, back),
                                                 _itemViewer.L0v2h2_WebView2,
                                                 this.HtmlDarkConverter);
            _activeTheme = LoadTheme();

            // Populate placeholder controls with 
            PopulateControls(dataModel);

            PopulateConversation();
            
            // Toggle off Tips and Navigation directly since we are definitely on the UI thread
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off));
            _itemPositionTips.Toggle(Enums.ToggleState.Off, shareColumn: true);
            
            WireEvents();
            Task.Run(()=>InitializeWebViewAsync());
        }

        #endregion

        #region Item Setup and Disposal Methods

        internal void InitializeWebView()
        {
            // Create the cache directory 
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("–incognito ");
                        
            // Create the environment manually
            Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();
            
            task.ContinueWith(t =>
            {
                _webViewEnvironment = task.Result;
                _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
            }, ui);
            
        }

        internal async Task InitializeWebViewAsync()
        {
            // Create the cache directory 
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("–incognito ");

            await _itemViewer.UiSyncContext;
            Debug.WriteLine($"Ui Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            // Create the environment manually
            Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            await task.ContinueWith(t =>
            {
                _webViewEnvironment = task.Result;
                _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
            }, ui);
        }

        internal void AdjustViewerForEfc()
        {
            // Collapse the right side of the navigation, disable all right side controls, and make them invisible
            //_itemViewer.L1h1L2v.ForAllControl(c => { c.Enabled = false; c.Visible = false; });
            //_itemViewer.L1h.Panel2Collapsed = true;
            _itemViewer.RemoveControlsColsRightOf(_itemViewer.LblConvCt);
            
            // Adjust the navigation formatting to account for the fact that there is no item position label
            var widthAdjustment = _itemViewer.LblItemNumber.Width - _itemViewer.LblAcOpen.Width;
            var columnNumber = _itemViewer.L0vh_Tlp.GetColumn(_itemViewer.LblAcOpen);
            _itemViewer.L0vh_Tlp.ColumnStyles[columnNumber].Width -= widthAdjustment;
        }
        
        public void Cleanup()
        {
            Buttons.ForEach(x =>
            {
                x.MouseEnter -= new EventHandler(this.Button_MouseEnter);
                x.MouseLeave -= new EventHandler(this.Button_MouseLeave);
            });
            _globals.Ol.PropertyChanged -= DarkMode_Changed;
            _globals = null;
            _itemViewer = null;
            _parent = null;
            _listTipsDetails = null;
            _dataModel = null;
            _webViewEnvironment = null;
            _themes = null;
            _tableLayoutPanels = null;
            _explorerController = null;
            _homeController = null;
            _keyboardHandler = null;
            _itemPositionTips = null;
            _itemInfo = null;
            _itemViewer = null;
            _timer = null;
        }

        public void PopulateControls(EfcDataModel dataModel)
        {
            _itemInfo = dataModel.MailInfo;
            _itemViewer.LblSender.Text = _itemInfo.SenderName;
            _itemViewer.LblSubject.Text = _itemInfo.Subject;
            _itemViewer.TxtboxBody.Text = _itemInfo.Body;
            _itemViewer.LblTriage.Text = _itemInfo.Triage;
            _itemViewer.LblSentOn.Text = _itemInfo.SentOn;
            _itemViewer.LblActionable.Text = _itemInfo.Actionable;
            if (_itemInfo.IsTaskFlagSet) { _itemViewer.BtnFlagTask.DialogResult = DialogResult.OK; }
            else { _itemViewer.BtnFlagTask.DialogResult = DialogResult.Cancel; }
        }

        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to 
        /// a Dataframe. Count is inferred from the df rowcount
        /// </summary>
        public void PopulateConversation()
        {
            var count = _dataModel.ConversationResolver.Count.SameFolder;
            _itemViewer.LblConvCt.Text = count.ToString();
            if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }

            // Could be redundant to event handler in ConversationResolver
            _ = Task.Run(() => _dataModel.ConversationResolver.LoadConversationItemsAsync(_homeController.Token, backgroundLoad: true));
        }
                
        internal void ResolveControlGroups(ItemViewer itemViewer)
        {
            var ctrls = itemViewer.GetAllChildren();

            _listTipsDetails = _itemViewer.LeftTipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();

            _itemPositionTips = new QfcTipsDetails(_itemViewer.LblItemNumber);

            _tableLayoutPanels = ctrls.Where(x => x is TableLayoutPanel)
                         .Select(x => (TableLayoutPanel)x)
                         .ToList();

            _buttons = ctrls.Where(x => x is Button)
                            .Select(x => (Button)x)
                            .ToList();

            _navCtrls = new List<Control> { _itemViewer.LblItemNumber };
            _tipsCtrls = _itemViewer.LeftTipsLabels.Select(x=>(Control)x).ToList();
            _dflt2Ctrls = new List<Control> { _itemViewer.L0vh_Tlp, _itemViewer.TxtboxBody, _itemViewer.TopicThread };
            _mailCtrls = new List<Control> { _itemViewer.LblSender, _itemViewer.LblSubject, };
        }
        
        #endregion

        #region Private Fields and Variables

        private bool _isWebViewerInitialized = false;
        private bool _suppressEvents = false;
        private CoreWebView2Environment _webViewEnvironment;
        private Dictionary<string, Theme> _themes;
        private IApplicationGlobals _globals;
        private IList<TableLayoutPanel> _tableLayoutPanels;
        private EfcDataModel _dataModel;
        private IQfcExplorerController _explorerController;
        private IFilerHomeController _homeController;
        private IQfcKeyboardHandler _keyboardHandler;
        private IQfcTipsDetails _itemPositionTips;
        private MailItemInfo _itemInfo;
        private ItemViewer _itemViewer;
        private System.Threading.Timer _timer;
        private List<Control> _navCtrls;
        private List<Control> _tipsCtrls;
        private List<Control> _dflt2Ctrls;
        private List<Control> _selectorsCtrls = null;
        private List<Control> _mailCtrls;

        #endregion

        #region Exposed properties

        private string _activeTheme;
        public string ActiveTheme
        {
            get => Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
            set => Initializer.SetAndSave<string>(ref _activeTheme, value, (x) => _themes[x].SetTheme(async: true)); 
        }
        internal string LoadTheme()
        {
            var activeTheme = DarkMode ? "DarkNormal" : "LightNormal";
            _themes[activeTheme].SetTheme();
            return activeTheme;
        }

        private IList<Button> _buttons;
        public IList<Button> Buttons { get => _buttons; }

        private string _convOriginID = "";
        public string ConvOriginID { get => _convOriginID; set => _convOriginID = value; }

        private int _intEnterCounter = 0;
        public int CounterEnter { get => _intEnterCounter; set => _intEnterCounter = value; }

        private int _intComboRightCtr = 0;
        public int CounterComboRight { get => _intComboRightCtr; set => _intComboRightCtr = value; }
        
        private bool _darkMode;
        public bool DarkMode 
        {
            get => Initializer.GetOrLoad(ref _darkMode, () => _globals.Ol.DarkMode, false, _globals, _globals.Ol);
            set => Initializer.SetAndSave(ref _darkMode, value, (x) => _globals.Ol.DarkMode = x); 
        }

        //private List<MailItemInfo> _conversationInfo;
        //public List<MailItemInfo> ConversationInfo { get => _conversationInfo; set => _conversationInfo = value; }

        //private IList<MailItem> _conversationItems;
        //public IList<MailItem> ConversationItems
        //{
        //    get
        //    {
        //        if (_conversationItems is null)
        //        {
        //            _conversationItems = ConvHelper.GetMailItemList(DfConversation,
        //                                                           ((Folder)Mail.Parent).StoreID,
        //                                                           _globals.Ol.App,
        //                                                           true)
        //                                           .Cast<MailItem>()
        //                                           .ToList();
        //        }
        //        return _conversationItems;
        //    }

        //    set => _conversationItems = value;
        //}

        //private IList<MailItem> _conversationItemsExpanded;
        //public IList<MailItem> ConversationItemsExpanded
        //{
        //    get
        //    {
        //        if (_conversationItemsExpanded is null)
        //        {
        //            _conversationItemsExpanded = ConvHelper.GetMailItemList(DfConversation,
        //                                                                   ((Folder)Mail.Parent).StoreID,
        //                                                                   _globals.Ol.App,
        //                                                                   true)
        //                                                   .Cast<MailItem>()
        //                                                   .ToList();
        //        }
        //        return _conversationItemsExpanded;
        //    }

        //    set => _conversationItemsExpanded = value;
        //}

        //private DataFrame _dfConversation;
        //public DataFrame DfConversation
        //{
        //    get
        //    {
        //        if ((_dfConversation is null) && (_mailItem is not null))
        //        {
        //            var conversation = Mail.GetConversation();
        //            DfConversationExpanded = conversation.GetConversationDf();
        //            DfConversation = DfConversationExpanded.FilterConversation(((Folder)Mail.Parent).FolderPath, false, true);
        //        }
        //        return _dfConversation;
        //    }
        //    internal set
        //    {
        //        _dfConversation = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        //private DataFrame _dfConversationExpanded;
        //public DataFrame DfConversationExpanded
        //{
        //    get
        //    {
        //        if ((_dfConversationExpanded is null) && (_mailItem is not null))
        //        {
        //            var conversation = Mail.GetConversation();
        //            DfConversationExpanded = conversation.GetConversationDf();
        //            DfConversation = DfConversationExpanded.FilterConversation(((Folder)Mail.Parent).FolderPath, false, true);
        //        }
        //        return _dfConversationExpanded;
        //    }
        //    internal set
        //    {
        //        _dfConversationExpanded = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        public int Height { get => _itemViewer.Height; }

        public bool IsExpanded { get => _expanded; }
        private bool _expanded = false;

        public bool IsChild { get => _isChild; set => _isChild = value; }
        private bool _isChild;

        public bool IsActiveUI { get => _activeUI; set => _activeUI = value; }
        private bool _activeUI = false;

        private IList<IQfcTipsDetails> _listTipsDetails;
        public IList<IQfcTipsDetails> ListTipsDetails { get => _listTipsDetails; }

        private EfcFormController _parent;
        public EfcFormController Parent { get => _parent; }

        private int _itemNumber;
        public int ItemNumber
        {
            get => _itemNumber;
            set
            {
                _itemNumber = value;
                _itemViewer.LblItemNumber.Text = _itemNumber.ToString();
            }
        }
        public int ItemIndex { get => ItemNumber - 1; set => _itemNumber = value + 1; }

        public string SelectedFolder { get => _itemViewer.CboFolders.SelectedItem.ToString(); }

        public string Sender { get => _itemInfo.SenderName; }

        public string SentDate { get => _itemInfo.SentDate.ToString("MM/dd/yyyy"); }

        public string SentTime { get => _itemInfo.SentDate.ToString("HH:mm"); }

        public string Subject { get => _itemViewer.LblSubject.Text; }

        public bool SuppressEvents { get => _suppressEvents; set => _suppressEvents = value; }

        public string To { get => _itemInfo.ToRecipientsName; }

        public IList<TableLayoutPanel> TableLayoutPanels { get => _tableLayoutPanels; }

        #endregion

        #region Event Wiring

        internal void WireEvents()
        {
            //Debug.WriteLine($"Wiring keyboard for item {this.Position}, {this.Subject}");
            _itemViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_keyboardHandler.KeyboardHandler_PreviewKeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_keyboardHandler.KeyboardHandler_KeyDown);
            },
            new List<Control> { _itemViewer.CboFolders, _itemViewer.TxtboxSearch, _itemViewer.TopicThread });
                        
            _itemViewer.L0v2h2_WebView2.CoreWebView2InitializationCompleted += WebView2Control_CoreWebView2InitializationCompleted;
            _dataModel.ConversationResolver.PropertyChanged += new PropertyChangedEventHandler(ConversationResolverPropertyChanged);
            _itemViewer.TopicThread.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.TopicThread_ItemSelectionChanged);
            _globals.Ol.PropertyChanged += DarkMode_Changed;
            Buttons.ForEach(x =>
            {
                x.MouseEnter += new EventHandler(this.Button_MouseEnter);
                x.MouseLeave += new EventHandler(this.Button_MouseLeave);
            });
        }

        internal void RegisterActions(Dictionary<char, Action<char>> actions, bool overwriteDuplicates) 
        {
            if (!overwriteDuplicates) 
            {
                actions = actions.Where(action => !_keyboardHandler.CharActions.ContainsKey(action.Key)).ToDictionary();       
            }
            actions.ForEach(action => _keyboardHandler.CharActions[action.Key] = action.Value);
        }
        
        internal void RegisterFocusActions()
        {
            _keyboardHandler.CharActions.Add("Item", 'O', (x) => _ = _explorerController.OpenQFItem(_itemInfo.Item));
            _keyboardHandler.CharActions.Add("Item", 'E', async (x) => await KbdExecuteAsync(this.ToggleExpansionAsync));
            if (_expanded)
            {
                _keyboardHandler.CharActions.Add("Item", 'B', async (x) => await JumpToAsync(_itemViewer.L0v2h2_WebView2));
                _keyboardHandler.CharActions.Add("Item", 'D', async (x) => await JumpToAsync(_itemViewer.TopicThread));
            }
        }

        internal void UnregisterFocusActions()
        {
            _keyboardHandler.CharActions.Remove("Item", 'O');
            _keyboardHandler.CharActions.Remove("Item", 'E');
            if (_expanded)
            {
                _keyboardHandler.CharActions.Remove("Item", 'B');
                _keyboardHandler.CharActions.Remove("Item", 'D');
            }
        }

        internal void UnregisterActions(List<char> keys)
        {
            keys.ForEach(key => _keyboardHandler.CharActions.Remove("Item", key));
        }
        
        #endregion

        #region Event Handlers

        public async void ConversationResolverPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded))
            {
                // Switch to UI Thread
                await _itemViewer.UiSyncContext;
                _itemViewer.TopicThread.SetObjects(_dataModel.ConversationResolver.ConversationInfo.Expanded);
                _itemViewer.TopicThread.Sort(_itemViewer.SentDate, SortOrder.Descending);
            }
        }
                
        private void TopicThread_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var objects = _itemViewer.TopicThread.SelectedObjects;
            if ((objects is not null) && (objects.Count != 0))
            {
                var info = objects[0] as MailItemInfo;
                _itemViewer.L0v2h2_WebView2.NavigateToString(info.Html);
            }

        }
        
        internal void WebView2Control_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                throw (e.InitializationException);
            }
            _isWebViewerInitialized = true;
            if (DarkMode)
            {
                _itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.ToggleDark(Enums.ToggleState.On));
            }
            else
            {
                _itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.ToggleDark(Enums.ToggleState.Off));
            }
            //_itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.Html);
            _itemViewer.L0v2h2_WebView2.Visible = false;
        }
        
        internal void DarkMode_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_globals.Ol.DarkMode))
            {
                _darkMode = _globals.Ol.DarkMode;
                if (DarkMode) { ActiveTheme = "DarkNormal"; }
                else { ActiveTheme = "LightNormal"; }
            }
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = _themes[_activeTheme].ButtonMouseOverColor;
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (((Button)sender).DialogResult == DialogResult.OK)
            {
                ((Button)sender).BackColor = _themes[_activeTheme].ButtonClickedColor;
            }
            else
            {
                ((Button)sender).BackColor = _themes[_activeTheme].ButtonBackColor;
            }
        }

        #endregion

        #region UI Navigation Methods

        public void ToggleExpansion()
        {
            if (_expanded) { ToggleExpansion(Enums.ToggleState.Off); }
            else { ToggleExpansion(Enums.ToggleState.On); }
        }

        public async Task ToggleExpansionAsync()
        {
            if (_expanded) { await ToggleExpansionAsync(Enums.ToggleState.Off); }
            else { await ToggleExpansionAsync(Enums.ToggleState.On); }
        }

        public void ToggleExpansion(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 0;
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 100;
                _itemViewer.TopicThread.Visible = true;
                //_itemViewer.L0v2h2_Panel.Visible = true;
                _itemViewer.L0v2h2_WebView2.Visible = true;
                _expanded = true;
                if ((_itemInfo is not null) && _itemInfo.UnRead == true)
                {
                    _timer = new System.Threading.Timer(ApplyReadEmailFormat);
                    _timer.Change(4000, System.Threading.Timeout.Infinite);
                }
                // Register the keyboard actions and overwrite any others silently
                _keyboardHandler.CharActions.Add("Item", 'B', async (x) => await JumpToAsync(_itemViewer.L0v2h2_WebView2));
                _keyboardHandler.CharActions.Add("Item", 'D', async (x) => await JumpToAsync(_itemViewer.TopicThread));
            }
            else
            {
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 100;
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 0;
                _itemViewer.TopicThread.Visible = false;
                //_itemViewer.L0v2h2_Panel.Visible = false;
                _itemViewer.L0v2h2_WebView2.Visible = false;
                _expanded = false;
                if (_timer is not null) { _timer.Dispose(); }
                _keyboardHandler.CharActions.Remove("Item", 'B');
                _keyboardHandler.CharActions.Remove("Item", 'D');
            }
        }

        public async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(desiredState);
            
            await _itemViewer.UiSyncContext;
            if (desiredState == Enums.ToggleState.On)
            {
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 0;
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 100;
                _itemViewer.TopicThread.Visible = true;
                //_itemViewer.L0v2h2_Panel.Visible = true;
                _itemViewer.L0v2h2_WebView2.Visible = true;
                _expanded = true;
                if ((_itemInfo is not null) && _itemInfo.UnRead == true)
                {
                    _timer = new System.Threading.Timer(ApplyReadEmailFormat);
                    _timer.Change(4000, System.Threading.Timeout.Infinite);
                }
            }
            else
            {
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 100;
                _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 0;
                _itemViewer.TopicThread.Visible = false;
                //_itemViewer.L0v2h2_Panel.Visible = false;
                _itemViewer.L0v2h2_WebView2.Visible = false;
                _expanded = false;
                if (_timer is not null) { _timer.Dispose(); }
            }
        }

        public void ToggleNavigation(bool async)
        {
            //if (async)
            //{
            //    _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(true)));
            //}
            //else
            //{
            //    _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(true)));
            //}
            ToggleTips(async);
            if (_activeUI)
            {
                _activeUI = false;
                UnregisterFocusActions();
            }
            else
            {
                _activeUI = true;
                RegisterFocusActions();
            }
        }

        public void ToggleNavigation(bool async, Enums.ToggleState desiredState)
        {
            ToggleTips(async, desiredState);
            if (desiredState == Enums.ToggleState.Off && _activeUI)
            {
                _activeUI = false;
                UnregisterFocusActions();
            }
            else if(desiredState == Enums.ToggleState.On && !_activeUI)
            {
                _activeUI = true;
                RegisterFocusActions();
            }
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _itemViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(true))); }
                else { _itemViewer.Invoke(new System.Action(() => tipsDetails.Toggle(true))); }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async) { _itemViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(desiredState, true))); }
                else { _itemViewer.Invoke(new System.Action(() => tipsDetails.Toggle(desiredState, true))); }
            }
        }

        public void ToggleSaveAttachments()
        {
            _itemViewer.CbxAttachments.Invoke(new System.Action(() =>
                _itemViewer.CbxAttachments.Checked =
                !_itemViewer.CbxAttachments.Checked));
        }

        public void ToggleSaveCopyOfMail()
        {
            _itemViewer.CbxEmailCopy.Invoke(new System.Action(() =>
                _itemViewer.CbxEmailCopy.Checked =
                !_itemViewer.CbxEmailCopy.Checked));
        }

        #endregion

        #region UI Visual Helper Methods

        public void SetThemeDark(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["DarkNormal"].SetTheme(async);
                _activeTheme = "DarkNormal";
            }
            else
            {
                _themes["DarkActive"].SetTheme(async);
                _activeTheme = "DarkActive";
            }
            _darkMode = true;
        }

        public void HtmlDarkConverter(Enums.ToggleState desiredState)
        {
            if (_isWebViewerInitialized)
            {
                _itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.ToggleDark(desiredState));
                _dataModel.ConversationResolver.ConversationInfo.Expanded.ForEach(item => item.ToggleDark(desiredState));
                //ConversationInfo.ForEach(item => item.ToggleDark(desiredState));
            }
        }

        public void SetThemeLight(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["LightNormal"].SetTheme(async);
                _activeTheme = "LightNormal";
            }
            else
            {
                _themes["LightActive"].SetTheme(async);
                _activeTheme = "LightActive";
            }
            _darkMode = false;
        }

        public void ApplyReadEmailFormat(object state)
        {   
            _itemInfo.UnRead = false;
            _themes[_activeTheme].ControlGroups["MailRelated"].ApplyTheme(async: true);
        }

        public void SetOlvTheme(IList<object> columns, Color fore, Color back)
        {
            var headerstyle = new HeaderFormatStyle();
            headerstyle.SetForeColor(fore);
            headerstyle.SetBackColor(back);

            columns.ForEach(column => ((OLVColumn)column).HeaderFormatStyle = headerstyle);
        }

        #endregion

        #region UI Keyboard Methods

        async public Task KbdExecuteAsync(Func<Task> action)
        {
            _homeController.KeyboardHndlr.ToggleKeyboardDialog();
            await action();
        }

        async internal Task JumpToAsync(Control control)
        {
            _homeController.KeyboardHndlr.ToggleKeyboardDialog();
            await _itemViewer.UiSyncContext;
            control.Focus();
        }
        
        public Dictionary<string, System.Action> RightKeyActions
        {
            get => new()
        {
            //{ "&Pop Out", ()=>this._parent.PopOutControlGroup(ItemNumber)},
            //{ "&Expand", ()=>{_itemViewer.lblSubject.Focus(); this.EnumerateConversation(); } },
            { "&Cancel", ()=>{ } }
        };
        }

        #endregion
    }
}
