using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using static Deedle.FrameBuilder;

namespace QuickFiler.Controllers
{
    [ExcludeFromCodeCoverage]
    internal class EfcItemController : IItemControler
    {
        #region Constructors and Initializers

        public EfcItemController(
            IApplicationGlobals globals,
            IFilerHomeController homeController,
            EfcFormController parent,
            ItemViewer itemViewer,
            EfcDataModel dataModel,
            CancellationToken token
        )
            : this(globals, homeController, parent, itemViewer, token)
        {
            _dataModel = dataModel;
            Initialize(async: true);
        }

        public EfcItemController(
            IApplicationGlobals globals,
            IFilerHomeController homeController,
            EfcFormController parent,
            ItemViewer itemViewer,
            CancellationToken token
        )
        {
            _globals = globals;
            _homeController = homeController;
            _keyboardHandler = _homeController.KeyboardHandler;
            _explorerController = _homeController.ExplorerController;
            _parent = parent;
            _itemViewer = itemViewer;
            _token = token;
        }

        public EfcItemController InitializeWithoutData()
        {
            // Adjust the viewer for Efc purposes
            AdjustViewerForEfc();

            ResolveControlGroups(_itemViewer);

            // Toggle off Tips and Navigation directly since we are definitely on the UI thread
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off));
            _itemPositionTips.Toggle(Enums.ToggleState.Off, shareColumn: true);

            return this;
        }

        public EfcItemController InitializeDataFields(EfcDataModel dataModel)
        {
            _dataModel = dataModel;
            _themes = EfcThemeHelper.SetupThemes(
                _navCtrls,
                _tipsCtrls,
                _dflt2Ctrls,
                // The EFC surface has no selector controls; the field this argument used to
                // carry was declared null and never assigned, so an explicit null is identical.
                null,
                _mailCtrls,
                () => !_dataModel.Mail?.UnRead ?? false,
                _itemViewer.TopicThread.Columns.Cast<object>().ToList(),
                (columns, fore, back) => SetOlvTheme(columns, fore, back),
                _itemViewer.L0v2h2_WebView2,
                this.HtmlDarkConverter
            );
            _activeTheme = LoadTheme();

            PopulateControls(dataModel);
            PopulateConversation();
            WireEvents();
            Task.Run(() => InitializeWebViewAsync());
            return this;
        }

        private void Initialize(bool async)
        //private void Initialize(IApplicationGlobals AppGlobals,
        //                        IFilerHomeController homeController,
        //                        EfcFormController parent,
        //                        ItemViewer itemViewer,
        //                        EfcDataModel dataModel,
        //                        bool async,
        //                        CancellationToken token)
        {
            //_token = token;
            //_globals = AppGlobals;
            //_homeController = homeController;

            //// Grab handle on viewer and controllers
            //_itemViewer = itemViewer;
            //_itemViewer.Controller = this;
            //_dataModel = dataModel;
            //_keyboardHandler = _homeController.KeyboardHandler;
            //_parent = parent;
            //_explorerController = _homeController.ExplorerController;

            // Adjust the viewer for Efc purposes
            AdjustViewerForEfc();

            ResolveControlGroups(_itemViewer);

            _themes = EfcThemeHelper.SetupThemes(
                _navCtrls,
                _tipsCtrls,
                _dflt2Ctrls,
                // The EFC surface has no selector controls; the field this argument used to
                // carry was declared null and never assigned, so an explicit null is identical.
                null,
                _mailCtrls,
                () => !_dataModel.Mail.UnRead,
                _itemViewer.TopicThread.Columns.Cast<object>().ToList(),
                (columns, fore, back) => SetOlvTheme(columns, fore, back),
                _itemViewer.L0v2h2_WebView2,
                this.HtmlDarkConverter
            );
            _activeTheme = LoadTheme();

            // Populate placeholder controls with
            PopulateControls(_dataModel);

            PopulateConversation();

            // Toggle off Tips and Navigation directly since we are definitely on the UI thread
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off));
            _itemPositionTips.Toggle(Enums.ToggleState.Off, shareColumn: true);

            WireEvents();
            Task.Run(() => InitializeWebViewAsync());
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        #endregion

        #region Item Setup and Disposal Methods

        /// <summary>
        /// The additional browser argument handed to <see cref="CoreWebView2EnvironmentOptions"/>
        /// so that the item preview keeps no browsing data.
        /// </summary>
        /// <remarks>
        /// Hoisted to a constant so the value has exactly one owner and can be asserted directly.
        /// A direct assertion is the only instrument available for it: the enclosing member needs
        /// the real WebView2 runtime, so it cannot be executed under the unit-test policy.
        /// </remarks>
        internal const string IncognitoArgument = "--incognito ";

        internal async Task InitializeWebViewAsync()
        {
            // Create the cache directory
            string localAppData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions(
                IncognitoArgument
            );

            await _itemViewer.UiSyncContext;
            //logger.Debug($"Ui Thread Id: {Thread.CurrentThread.ManagedThreadId}");
            // Create the environment manually
            Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(
                null,
                cacheFolder,
                options
            );

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            await task.ContinueWith(
                t =>
                {
                    _webViewEnvironment = task.Result;
                    _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
                },
                ui
            );
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

        /// <summary>
        /// Releases the collaborators this controller holds. Callable on a partially constructed
        /// controller whose <c>Initialize</c> never ran, and idempotent: a second call detaches
        /// nothing and releases nothing further.
        /// </summary>
        public void Cleanup()
        {
            // Every detach precedes the nulling of the field it detaches from, so a subscription
            // is always released before its owner becomes unreachable.
            var buttons = _buttons;
            if (buttons is not null)
            {
                foreach (var button in buttons)
                {
                    button.MouseEnter -= new EventHandler(this.Button_MouseEnter);
                    button.MouseLeave -= new EventHandler(this.Button_MouseLeave);
                }
            }
            _buttons = null;

            var globals = _globals;
            if (globals?.Ol is not null)
            {
                globals.Ol.PropertyChanged -= DarkMode_Changed;
            }
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

            // Disposal precedes the nulling, otherwise the thread pool retains a live callback
            // into a torn-down controller with no remaining reference through which to stop it.
            _timer?.Dispose();
            _timer = null;
        }

        public void PopulateControls(EfcDataModel dataModel)
        {
            _itemInfo = dataModel.MailInfo;
            if (_itemInfo is null)
            {
                return;
            }
            _itemViewer.LblSender.Text = _itemInfo.SenderName;
            _itemViewer.LblSubject.Text = _itemInfo.Subject;
            _itemViewer.TxtboxBody.Text = _itemInfo.Body;
            _itemViewer.LblTriage.Text = _itemInfo.Triage;
            _itemViewer.LblSentOn.Text = _itemInfo.SentOn;
            _itemViewer.LblActionable.Text = _itemInfo.Actionable;
            if (_itemInfo.IsTaskFlagSet)
            {
                _itemViewer.BtnFlagTask.DialogResult = DialogResult.OK;
            }
            else
            {
                _itemViewer.BtnFlagTask.DialogResult = DialogResult.Cancel;
            }
        }

        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to
        /// a Dataframe. Count is inferred from the df rowcount
        /// </summary>
        public void PopulateConversation()
        {
            if (_dataModel.ConversationResolver is null)
            {
                return;
            }
            _dataModel.ConversationResolver.UpdateUI = SetTopicThread;
            var count = _dataModel.ConversationResolver.Count.SameFolder;
            _itemViewer.LblConvCt.Text = count.ToString();
            if (count == 0)
            {
                _itemViewer.LblConvCt.BackColor = Color.Red;
            }

            // Could be redundant to event handler in ConversationResolver
            // _ = Task.Run(() => _dataModel.ConversationResolver.LoadConversationItemsAsync(_homeController.Token, backgroundLoad: true));
        }

        internal void ResolveControlGroups(ItemViewer itemViewer)
        {
            var ctrls = itemViewer.GetAllChildren();

            _listTipsDetails = _itemViewer
                .LeftTipsLabels.Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                .ToList();

            _itemPositionTips = new QfcTipsDetails(_itemViewer.LblItemNumber);

            _tableLayoutPanels = ctrls
                .Where(x => x is TableLayoutPanel)
                .Select(x => (TableLayoutPanel)x)
                .ToList();

            _buttons = ctrls.Where(x => x is Button).Select(x => (Button)x).ToList();

            _navCtrls = new List<Control> { _itemViewer.LblItemNumber };
            _tipsCtrls = _itemViewer.LeftTipsLabels.Select(x => (Control)x).ToList();
            _dflt2Ctrls = new List<Control>
            {
                _itemViewer.L0vh_Tlp,
                _itemViewer.TxtboxBody,
                _itemViewer.TopicThread,
            };
            _mailCtrls = new List<Control> { _itemViewer.LblSender, _itemViewer.LblSubject };
        }

        public void SetTopicThread(List<MailItemHelper> conversationInfo)
        {
            // Set the TopicThread to the ConversationInfo list
            _itemViewer.TopicThread.SetObjects(conversationInfo);
            _itemViewer.TopicThread.Sort(_itemViewer.SentDate, SortOrder.Descending);
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
        private ItemViewer _itemViewer;
        private System.Threading.Timer _timer;
        private List<Control> _navCtrls;
        private List<Control> _tipsCtrls;
        private List<Control> _dflt2Ctrls;
        private List<Control> _mailCtrls;

        #endregion

        #region Exposed properties


        private MailItemHelper _itemInfo;
        internal MailItemHelper ItemInfo => _itemInfo;

        private string _activeTheme;
        public string ActiveTheme
        {
            // The theme dictionary is released by Cleanup(), and Initializer.GetOrLoad throws
            // under strict: true once a dependency is null. Testing at the call site returns the
            // backing field on the torn-down path instead, and keeps the loaded path unchanged.
            get =>
                _themes is null
                    ? _activeTheme
                    : Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
            set =>
                Initializer.SetAndSave<string>(
                    ref _activeTheme,
                    value,
                    (x) => _themes[x].SetTheme(async: true)
                );
        }

        internal string LoadTheme()
        {
            var activeTheme = DarkMode ? "DarkNormal" : "LightNormal";
            if (_themes is not null && _themes.ContainsKey(activeTheme))
            {
                _themes[activeTheme].SetTheme();
            }
            return activeTheme;
        }

        private IList<Button> _buttons;
        public IList<Button> Buttons
        {
            get => _buttons;
        }

        private string _convOriginID = "";
        public string ConvOriginID
        {
            get => _convOriginID;
            set => _convOriginID = value;
        }

        private int _intEnterCounter = 0;
        public int CounterEnter
        {
            get => _intEnterCounter;
            set => _intEnterCounter = value;
        }

        private int _intComboRightCtr = 0;
        public int CounterComboRight
        {
            get => _intComboRightCtr;
            set => _intComboRightCtr = value;
        }

        private bool _darkMode;
        public bool DarkMode
        {
            // The dependency array is a params object[], so every argument is materialised before
            // Initializer.GetOrLoad is entered and _globals.Ol would be dereferenced even on the
            // path that exists to reject a null dependency. Testing at the call site means the
            // array is never built once the controller has been torn down.
            get =>
                _globals?.Ol is null
                    ? _darkMode
                    : Initializer.GetOrLoad(
                        ref _darkMode,
                        () => _globals.Ol.DarkMode,
                        false,
                        _globals,
                        _globals.Ol
                    );
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

        public int Height
        {
            get => _itemViewer.Height;
        }

        public bool IsExpanded
        {
            get => _expanded;
        }
        private bool _expanded = false;

        public bool IsChild
        {
            get => _isChild;
            set => _isChild = value;
        }
        private bool _isChild;

        public bool IsActiveUI
        {
            get => _activeUI;
            set => _activeUI = value;
        }
        private bool _activeUI = false;

        private IList<IQfcTipsDetails> _listTipsDetails;
        public IList<IQfcTipsDetails> ListTipsDetails
        {
            get => _listTipsDetails;
        }

        private EfcFormController _parent;
        public EfcFormController Parent
        {
            get => _parent;
        }

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
        public int ItemIndex
        {
            get => ItemNumber - 1;
            set => _itemNumber = value + 1;
        }

        public string SelectedFolder
        {
            // #351: the folder control is the WebView2 breadcrumb; the selection contract is the
            // same full-path/verbatim string the old ComboBox SelectedItem produced (G10).
            get => _itemViewer.GetSelectedFolder();
        }

        public string Sender
        {
            get => _itemInfo?.SenderName;
        }

        public string SentDate
        {
            get => _itemInfo.SentDate.ToString("MM/dd/yyyy");
        }

        public string SentTime
        {
            get => _itemInfo.SentDate.ToString("HH:mm");
        }

        public string Subject
        {
            // #464 A: read the cached mail-item model like Sender and To, not the label text of
            // a control that Cleanup() has already released.
            get => _itemInfo?.Subject;
        }

        public bool SuppressEvents
        {
            get => _suppressEvents;
            set => _suppressEvents = value;
        }

        public string To
        {
            get => _itemInfo?.ToRecipientsName;
        }

        public IList<TableLayoutPanel> TableLayoutPanels
        {
            get => _tableLayoutPanels;
        }

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
            set => _token = value;
        }

        #endregion

        #region Event Wiring

        internal void WireEvents()
        {
            //Debug.WriteLine($"Wiring keyboard for item {this.Position}, {this.Subject}");
            _itemViewer.ForAllControls(
                x =>
                {
                    x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                        _keyboardHandler.KeyboardHandler_PreviewKeyDownAsync
                    );
                    //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_keyboardHandler.KeyboardHandler_KeyDown);
                    x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                        _keyboardHandler.KeyboardHandler_KeyDownAsync
                    );
                },
                new List<Control>
                {
                    _itemViewer.L0vhBreadcrumb_WebView2,
                    _itemViewer.TxtboxSearch,
                    _itemViewer.TopicThread,
                }
            );

            _itemViewer.L0v2h2_WebView2.CoreWebView2InitializationCompleted +=
                WebView2Control_CoreWebView2InitializationCompleted;
            _itemViewer.TopicThread.ItemSelectionChanged +=
                new ListViewItemSelectionChangedEventHandler(this.TopicThread_ItemSelectionChanged);
            _globals.Ol.PropertyChanged += DarkMode_Changed;
            Buttons.ForEach(x =>
            {
                x.MouseEnter += new EventHandler(this.Button_MouseEnter);
                x.MouseLeave += new EventHandler(this.Button_MouseLeave);
            });
        }

        internal void RegisterAsyncFocusActions()
        {
            _keyboardHandler.CharActionsAsync.Add(
                "Item",
                'O',
                (x) => _ = _explorerController.OpenQFItem(_itemInfo.Item)
            );
            _keyboardHandler.CharActionsAsync.Add(
                "Item",
                'E',
                async (x) => await KbdExecuteAsync(this.ToggleExpansionAsync)
            );
            if (_expanded)
            {
                _keyboardHandler.CharActionsAsync.Add(
                    "Item",
                    'B',
                    async (x) => await JumpToAsync(_itemViewer.L0v2h2_WebView2)
                );
                _keyboardHandler.CharActionsAsync.Add(
                    "Item",
                    'D',
                    async (x) => await JumpToAsync(_itemViewer.TopicThread)
                );
            }
        }

        internal void UnregisterAsyncFocusActions()
        {
            _keyboardHandler.CharActionsAsync.Remove("Item", 'O');
            _keyboardHandler.CharActionsAsync.Remove("Item", 'E');
            if (_expanded)
            {
                _keyboardHandler.CharActionsAsync.Remove("Item", 'B');
                _keyboardHandler.CharActionsAsync.Remove("Item", 'D');
            }
        }

        internal void UnregisterActions(List<char> keys)
        {
            keys.ForEach(key => _keyboardHandler.CharActions.Remove("Item", key));
        }

        #endregion

        #region Event Handlers

        private void TopicThread_ItemSelectionChanged(
            object sender,
            ListViewItemSelectionChangedEventArgs e
        )
        {
            var objects = _itemViewer.TopicThread.SelectedObjects;
            if ((objects is not null) && (objects.Count != 0))
            {
                var info = objects[0] as MailItemHelper;
                _itemViewer.L0v2h2_WebView2.NavigateToString(info.Html);
            }
        }

        /// <summary>
        /// Rethrows the exception that a failed WebView2 core initialization reported.
        /// </summary>
        /// <param name="initializationException">
        /// The exception carried by the initialization-completed event argument.
        /// </param>
        internal static void ThrowInitializationFailure(System.Exception initializationException)
        {
            // #464 E: a plain `throw expression;` overwrites StackTrace with this rethrow site,
            // discarding the frames that identify where the initialization actually failed.
            // Capturing first and calling Throw() rethrows the same instance with its original
            // trace intact.
            System
                .Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(initializationException)
                .Throw();
        }

        internal void WebView2Control_CoreWebView2InitializationCompleted(
            object sender,
            CoreWebView2InitializationCompletedEventArgs e
        )
        {
            if (!e.IsSuccess)
            {
                ThrowInitializationFailure(e.InitializationException);
            }
            _isWebViewerInitialized = true;
            // Do not initialize if there is no item
            if (_itemInfo is null)
            {
                return;
            }
            if (DarkMode)
            {
                _itemViewer.L0v2h2_WebView2.NavigateToString(
                    _itemInfo.ToggleDark(Enums.ToggleState.On)
                );
            }
            else
            {
                _itemViewer.L0v2h2_WebView2.NavigateToString(
                    _itemInfo.ToggleDark(Enums.ToggleState.Off)
                );
            }
            //_itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.Html);
            _itemViewer.L0v2h2_WebView2.Visible = false;
        }

        internal void DarkMode_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_globals.Ol.DarkMode))
            {
                _darkMode = _globals.Ol.DarkMode;
                if (DarkMode)
                {
                    ActiveTheme = "DarkNormal";
                }
                else
                {
                    ActiveTheme = "LightNormal";
                }
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

        public async Task ToggleExpansionAsync()
        {
            if (_expanded)
            {
                await ToggleExpansionAsync(Enums.ToggleState.Off);
            }
            else
            {
                await ToggleExpansionAsync(Enums.ToggleState.On);
            }
        }

        public async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            _parent.ToggleExpansionStyle(desiredState);

            if (desiredState == Enums.ToggleState.On)
            {
                await _itemViewer.UiDispatcher.InvokeAsync(ToggleExpansionOn);
                //await Task.Factory.StartNew(
                //    () => ToggleExpansionOn(),
                //    default,
                //    TaskCreationOptions.None,
                //    _itemViewer.UiScheduler);
            }
            else
            {
                await _itemViewer.UiDispatcher.InvokeAsync(ToggleExpansionOff);
                //await Task.Factory.StartNew(
                //    () => ToggleExpansionOff(),
                //    default,
                //    TaskCreationOptions.None,
                //    _itemViewer.UiScheduler);
            }
        }

        private void ToggleExpansionOff()
        {
            _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 100;
            _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 0;
            _itemViewer.TopicThread.Visible = false;
            _itemViewer.L0v2h2_WebView2.Visible = false;
            _expanded = false;
            if (_timer is not null)
            {
                _timer.Dispose();
            }
        }

        private void ToggleExpansionOn()
        {
            _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[0].Width = 0;
            _itemViewer.L1h0L2hv3h_TlpBodyToggle.ColumnStyles[1].Width = 100;
            _itemViewer.TopicThread.Visible = true;
            _itemViewer.L0v2h2_WebView2.Visible = true;
            _expanded = true;
            if ((_itemInfo is not null) && _itemInfo.UnRead == true)
            {
                _timer = new System.Threading.Timer(ApplyReadEmailFormat);
                _timer.Change(4000, System.Threading.Timeout.Infinite);
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
                UnregisterAsyncFocusActions();
            }
            else
            {
                _activeUI = true;
                RegisterAsyncFocusActions();
            }
        }

        public void ToggleNavigation(bool async, Enums.ToggleState desiredState)
        {
            ToggleTips(async, desiredState);
            if (desiredState == Enums.ToggleState.Off && _activeUI)
            {
                _activeUI = false;
                UnregisterAsyncFocusActions();
            }
            else if (desiredState == Enums.ToggleState.On && !_activeUI)
            {
                _activeUI = true;
                RegisterAsyncFocusActions();
            }
        }

        public async Task ToggleNavigationAsync(Enums.ToggleState desiredState)
        {
            await ToggleTipsAsync(desiredState);
            if (desiredState == Enums.ToggleState.Off && _activeUI)
            {
                _activeUI = false;
                UnregisterAsyncFocusActions();
            }
            else if (desiredState == Enums.ToggleState.On && !_activeUI)
            {
                _activeUI = true;
                RegisterAsyncFocusActions();
            }
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async)
                {
                    _itemViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(true)));
                }
                else
                {
                    _itemViewer.Invoke(new System.Action(() => tipsDetails.Toggle(true)));
                }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async)
                {
                    _itemViewer.BeginInvoke(
                        new System.Action(() => tipsDetails.Toggle(desiredState, true))
                    );
                }
                else
                {
                    _itemViewer.Invoke(
                        new System.Action(() => tipsDetails.Toggle(desiredState, true))
                    );
                }
            }
        }

        public async Task ToggleTipsAsync(Enums.ToggleState desiredState)
        {
            Token.ThrowIfCancellationRequested();

            // Attempt to remove blocking await code and start all tasks simultaneously.
            var tasks = _listTipsDetails
                .Select(x => x.ToggleAsync(desiredState, shareColumn: true))
                .ToList();
            // TODO: Check if this creates a deadlock
            await Task.WhenAll(tasks);

            // Original async code
            //foreach (var tip in _listTipsDetails)
            //{
            //    await tip.ToggleAsync(desiredState, shareColumn: true);
            //}
        }

        public void ToggleSaveAttachments()
        {
            UiThread.Dispatcher.Invoke(() =>
                _itemViewer.SaveAttachmentsMenuItem.Checked = !_itemViewer
                    .SaveAttachmentsMenuItem
                    .Checked
            );
        }

        public void ToggleSaveCopyOfMail()
        {
            UiThread.Dispatcher.Invoke(() =>
                _itemViewer.SaveEmailMenuItem.Checked = !_itemViewer.SaveEmailMenuItem.Checked
            );
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
                _dataModel.ConversationResolver.ConversationInfo.Expanded.ForEach(item =>
                    item.ToggleDark(desiredState)
                );
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

        /// <summary>
        /// Thread-pool timer callback that marks the displayed mail read and reapplies the
        /// mail-related theme group.
        /// </summary>
        /// <remarks>
        /// The callback can still be in flight when <c>Cleanup()</c> runs. A post-teardown
        /// invocation is the expected steady state rather than an error, so it returns silently
        /// without logging once any collaborator it needs has been released.
        /// </remarks>
        public void ApplyReadEmailFormat(object state)
        {
            if (
                _itemInfo is null
                || _themes is null
                || _activeTheme is null
                || !_themes.ContainsKey(_activeTheme)
            )
            {
                return;
            }
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
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            await action();
        }

        internal async Task JumpToAsync(Control control)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            await _itemViewer.UiSyncContext;
            control.Focus();
        }

        public Dictionary<string, System.Action> RightKeyActions
        {
            get =>
                new()
                {
                    //{ "&Pop Out", ()=>this._parent.PopOutControlGroup(ItemNumber)},
                    //{ "&Expand", ()=>{_itemViewer.lblSubject.Focus(); this.EnumerateConversation(); } },
                    { "&Cancel", () => { } },
                };
        }

        #endregion
    }
}
