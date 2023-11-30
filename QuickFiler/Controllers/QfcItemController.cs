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
using System.IO;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using TaskVisualization;
using System.Threading;
using System.Windows.Threading;
using QuickFiler.Viewers;

namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController, INotifyPropertyChanged, IItemControler
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #region Constructors

        private QfcItemController() { }
        
        public QfcItemController(IApplicationGlobals AppGlobals,
                                 IFilerHomeController homeController,
                                 IQfcCollectionController parent,
                                 ItemViewer itemViewer,
                                 int viewerPosition,
                                 int itemNumberDigits,
                                 MailItem mailItem,
                                 TlpCellStates tlpStates)
        {
            //Initialize(AppGlobals, homeController, parent, itemViewer, viewerPosition, mailItem, async: true);
            SaveParameters(AppGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
        }

        public QfcItemController(IApplicationGlobals AppGlobals,
                                 IFilerHomeController homeController,
                                 IQfcCollectionController parent,
                                 ItemViewer itemViewer,
                                 int viewerPosition,
                                 int itemNumberDigits,
                                 MailItem mailItem,
                                 TlpCellStates tlpStates,
                                 bool async)
        {
            //Initialize(AppGlobals, homeController, parent, itemViewer, viewerPosition, mailItem, async);
            SaveParameters(AppGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
        }

        private void Initialize(IApplicationGlobals AppGlobals,
                                IFilerHomeController homeController,
                                IQfcCollectionController parent,
                                ItemViewer itemViewer,
                                int viewerPosition,
                                int itemNumberDigits,
                                MailItem mailItem,
                                TlpCellStates tlpStates,
                                bool async)
        {
            SaveParameters(AppGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);

            Initialize(async);
        }

        public void Initialize(bool async)
        {
            // Group controls into collections
            ResolveControlGroups(_itemViewer);

            // Setup the theme dictionary (Note: need control groups established prior to this)
            _themes = QfcThemeHelper.SetupThemes(this, _itemViewer, this.HtmlDarkConverter);

            // Populate placeholder controls with values
            PopulateControls(Mail, ItemNumber);

            // Adjust item viewer for desired state
            ToggleTips(async: async, desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            ToggleNavigation(async: async, desiredState: Enums.ToggleState.Off);

            // Activate event management
            WireEvents();

            // Fire and forget WebView initialization
            Task.Run(() => InitializeWebViewAsync());
        }

        public async Task InitializeAsync()
        {
            TraceUtility.LogMethodCall();

            // Group controls into collections
            _token.ThrowIfCancellationRequested();
            await ResolveControlGroupsAsync(_itemViewer);

            var tasks = new List<Task>
            {
                Task.Run(()=>
                {
                    _themes = QfcThemeHelper.SetupThemes(this, _itemViewer, this.HtmlDarkConverter);
                    if (_globals.Ol.DarkMode) { SetThemeDark(async: true); }
                    else { SetThemeLight(async: true); }
                },_token),
                PopulateControlsAsync(Mail, ItemNumber, true),
                ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force),
                ToggleNavigationAsync(desiredState: Enums.ToggleState.Off),
                Task.Run(()=>PopulateConversation()),
                Task.Run(()=>PopulateFolderComboBox()),
            };

            await Task.WhenAll(tasks);

            await Task.Run(() => WireEvents());

            await InitializeWebViewAsync();
            
        }

        public async Task InitializeSequentialAsync()
        {
            _token.ThrowIfCancellationRequested();
            
            // Group controls into collections
            ResolveControlGroups(_itemViewer);
                        
            _themes = QfcThemeHelper.SetupThemes(this, _itemViewer, this.HtmlDarkConverter);
            if (_globals.Ol.DarkMode) { SetThemeDark(async: true); }
            else { SetThemeLight(async: true); }
            
            await PopulateControlsAsync(Mail, ItemNumber, false);
            
            await ToggleTipsAsync(desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            //ToggleTips(async: true, desiredState: Enums.ToggleState.Off | Enums.ToggleState.Force);
            await ToggleNavigationAsync(desiredState: Enums.ToggleState.Off);
            WireEvents();

            //var cr = await ConversationResolver.LoadAsync(_globals, Mail, _tokenSource, _token, false, SetTopicThread);
            //await PopulateConversationAsync(cr, _token, false);
                        
            _ = InitializeWebViewAsync();

        }

        internal void SaveParameters(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates)
        {
            // Save parameters to private fields
            _globals = AppGlobals;
            _homeController = homeController;
            _parent = parent;
            _itemViewer = itemViewer;
            _mailItem = mailItem;
            _tlpStates = tlpStates;
            _itemNumberDigits = itemNumberDigits;
            ItemNumber = viewerPosition;

            // Set references to other controllers
            _itemViewer.Controller = this;
            _kbdHandler = _homeController.KeyboardHandler;
            _explorerController = _homeController.ExplorerController;
            _token = _homeController.Token;
            _tokenSource = _homeController.TokenSource;
        }

        public static async Task<QfcItemController> CreateAsync(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var controller = new QfcItemController();
            controller.SaveParameters(AppGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
            await controller.InitializeAsync();
            return controller;
        }

        public static async Task<QfcItemController> CreateSequentialAsync(
            IApplicationGlobals AppGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var controller = new QfcItemController();
            controller.SaveParameters(AppGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
            await controller.InitializeSequentialAsync();
            return controller;
        }

        #endregion

        #region ItemViewer Setup and Disposal

        internal void InitializeWebView()
        {
            // Create the cache directory 
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new("–incognito ");

            _itemViewer.L0v2h2_WebView2.BeginInvoke(new System.Action(() =>
            {
                // Create the environment manually
                Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

                // Do this so the task is continued on the UI Thread
                TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();
                //TaskScheduler ui = _itemViewer.UiScheduler;

                task.ContinueWith(t =>
                {
                    _webViewEnvironment = task.Result;
                    _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
                }, ui);
            }));
        }

        internal async Task InitializeWebViewAsync()
        {
            TraceUtility.LogMethodCall();

            _token.ThrowIfCancellationRequested();

            // Create the cache directory 
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string cacheFolder = Path.Combine(localAppData, "WindowsFormsWebView2");

            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
            CoreWebView2EnvironmentOptions options = new("–incognito ");

            // Switch to UI Thread
            await _itemViewer.UiSyncContext;

            // Do this so the task is continued on the UI Thread
            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            // Create the environment manually
            var task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);

            await task.ContinueWith(t =>
            {
                _webViewEnvironment = task.Result;
                _itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);
            }, _token, TaskContinuationOptions.OnlyOnRanToCompletion,ui);
        }

        internal void ResolveControlGroups(ItemViewer itemViewer)
        {
            var controls = itemViewer.GetAllChildren();

            _listTipsDetails = _itemViewer.TipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();

            _listTipsExpanded = _itemViewer.ExpandedTipsLabels
                               .Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                               .ToList();

            _itemPositionTips = new QfcTipsDetails(_itemViewer.LblItemNumber);

            var navColNum = _itemPositionTips.ColumnNumber;

            _listTipsDetails.ForEach(x => { if (x.ColumnNumber == navColNum) { x.IsNavColumn = true; } });

            _listTipsExpanded.ForEach(x => { if (x.ColumnNumber == navColNum) { x.IsNavColumn = true; } });


            _tableLayoutPanels = controls.Where(x => x is TableLayoutPanel)
                         .Select(x => (TableLayoutPanel)x)
                         .ToList();

            Buttons = controls.Where(x => x is Button)
                            .Select(x => (Button)x)
                            .ToList();



        }

        internal async Task ResolveControlGroupsAsync(ItemViewer itemViewer)
        {
            _token.ThrowIfCancellationRequested();

            _itemPositionTips = await QfcTipsDetails.CreateAsync(_itemViewer.LblItemNumber,
                                                                 _itemViewer.UiSyncContext,
                                                                 _token);
            var navColNum = _itemPositionTips.ColumnNumber;

            await itemViewer.UiSyncContext;
            var controls = itemViewer.GetAllChildren();
                        

            _listTipsDetails = await _itemViewer.TipsLabels
                                     .ToAsyncEnumerable()
                                     .SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, _token))
                                     .ToListAsync();
                        
            _listTipsExpanded = await _itemViewer.ExpandedTipsLabels
                                      .ToAsyncEnumerable()
                                      .SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, _token))
                                      .ToListAsync();
            
            _listTipsDetails.ForEach(x => { if (x.ColumnNumber == navColNum) { x.IsNavColumn = true; } });

            _listTipsExpanded.ForEach(x => { if (x.ColumnNumber == navColNum) { x.IsNavColumn = true; } });
                        
            _tableLayoutPanels = controls.Where(x => x is TableLayoutPanel)
                         .Select(x => (TableLayoutPanel)x)
                         .ToList();

            Buttons = controls.Where(x => x is Button)
                            .Select(x => (Button)x)
                            .ToList();

        }

        public void PopulateControls(MailItem mailItem, int viewerPosition)
        {
            _itemInfo = new MailItemInfo(mailItem);
            _itemInfo.LoadPriority(_globals.Ol.EmailPrefixToStrip, _token);
            AssignControls(_itemInfo, viewerPosition);

        }
        
        internal async Task PopulateControlsAsync(MailItem mailItem, int viewerPosition, bool loadAll)
        {
            TraceUtility.LogMethodCall(mailItem, viewerPosition, loadAll);

            _token.ThrowIfCancellationRequested();

            _itemInfo = await MailItemInfo.FromMailItemAsync(mailItem, _globals.Ol.EmailPrefixToStrip, _token, loadAll);
            
            AssignControls(_itemInfo, viewerPosition);

        }

        internal void AssignControls(MailItemInfo itemInfo, int viewerPosition)
        {
            TraceUtility.LogMethodCall(itemInfo, viewerPosition);

            _itemViewer.LblSender.Text = itemInfo.SenderName;
            _itemViewer.LblSubject.Text = itemInfo.Subject;
            _itemViewer.TxtboxBody.Text = itemInfo.Body;
            _itemViewer.LblTriage.Text = itemInfo.Triage;
            _itemViewer.LblSentOn.Text = itemInfo.SentOn;
            _itemViewer.LblActionable.Text = itemInfo.Actionable;
            if (itemInfo.IsTaskFlagSet) { _itemViewer.BtnFlagTask.DialogResult = DialogResult.OK; }
            else { _itemViewer.BtnFlagTask.DialogResult = DialogResult.Cancel; }
            _itemViewer.LblItemNumber.Text = viewerPosition.ToString();
            
            
            _optionConversationChecked = _globals.QfSettings.MoveEntireConversation;
            _itemViewer.ConversationMenuItem.Checked = _optionConversationChecked;

            _optionEmailCopy = _globals.QfSettings.SaveEmailCopy;
            _itemViewer.SaveEmailMenuItem.Checked = _optionEmailCopy;

            _optionAttachments = _globals.QfSettings.SaveAttachments;
            _itemViewer.SaveAttachmentsMenuItem.Checked = _optionAttachments;

            _optionsPictures = _globals.QfSettings.SavePictures;
            _itemViewer.SavePicturesMenuItem.Checked = _optionsPictures;
        }

        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to 
        /// a Dataframe. Count is inferred from the df row count
        /// </summary>
        public void PopulateConversation()
        {
            ConversationResolver = new ConversationResolver(_globals, Mail, _tokenSource, _token, SetTopicThread);
           
            PopulateConversation(ConversationResolver.Count.SameFolder);
            //PopulateConversation(_mailItem.GetConversationDf());
        }

        public void PopulateConversation(ConversationResolver resolver)
        {
            ConversationResolver = resolver;
            PopulateConversation(ConversationResolver.Count.SameFolder);
        }

        public async Task PopulateConversationAsync(CancellationTokenSource tokenSource, CancellationToken token, bool loadAll)
        {
            token.ThrowIfCancellationRequested();

            ConversationResolver = await ConversationResolver.LoadAsync(_globals, Mail, tokenSource, token, loadAll, SetTopicThread);
            await RenderConversationCountAsync(ConversationResolver.Count.SameFolder, token, loadAll);
        }
        
        public async Task PopulateConversationAsync(ConversationResolver resolver, CancellationToken token, bool loadAll)
        {
            token.ThrowIfCancellationRequested();

            ConversationResolver = resolver;
            await RenderConversationCountAsync(ConversationResolver.Count.SameFolder, token, loadAll);
        }

        /// <summary>
        /// TBD if this overload will be of use. Depends on whether _dfConversation
        /// is needed by any individual element when expanded
        /// </summary>
        /// <param name="df"></param>
        //public void PopulateConversation(DataFrame df)
        //{
        //    DfConversationExpanded = df.FilterConversation(((Folder)Mail.Parent).FolderPath, false, true);
        //    DfConversation = DfConversationExpanded.FilterConversation(((Folder)Mail.Parent).Name, true, true);
        //    int count = DfConversation.Rows.Count();
        //    PopulateConversation(count);
        //}

        /// <summary>
        /// Sets the conversation count of the visual without altering the
        /// _dfConversation. Useful when expanding or collapsing the 
        /// conversation to show how many items will be moved
        /// </summary>
        /// <param name="count"></param>
        public void PopulateConversation(int count)
        {
            //_itemViewer.LblConvCt.BeginInvoke(new System.Action(() =>
            UIThreadExtensions.UiDispatcher.BeginInvoke(() =>
            {
                _itemViewer.LblConvCt.Text = count.ToString();
                if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }
            });
        }

        public async Task RenderConversationCountAsync(int count, CancellationToken token, bool backgroundLoad) 
        {
            token.ThrowIfCancellationRequested();

            DispatcherPriority priority = backgroundLoad ? DispatcherPriority.Background : DispatcherPriority.Normal;

            await UIThreadExtensions.UiDispatcher.InvokeAsync(
                () =>
                {
                    _itemViewer.LblConvCt.Text = count.ToString();
                    if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }
                },
                priority,
                token);
        }

        internal void LoadFolderHandler(object varList = null)
        {
            if (varList is null)
            {
                _folderHandler = new FolderHandler(
                    _globals, _mailItem, FolderHandler.InitOptions.FromField);
            }
            else
            {
                _folderHandler = new FolderHandler(
                    _globals, varList, FolderHandler.InitOptions.FromArrayOrString);
            }
        }

        internal async Task LoadFolderHandlerAsync(object varList = null) 
        { 
            await Task.Factory.StartNew(()=>LoadFolderHandler(varList), _token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        
        public void PopulateFolderComboBox(object varList = null)
        {
            TraceUtility.LogMethodCall(varList);

            LoadFolderHandler(varList);

            UIThreadExtensions.UiDispatcher.BeginInvoke(()=>
            //_itemViewer.CboFolders.BeginInvoke(new System.Action(() =>
            {
                if (_folderHandler.FolderArray.Length > 0)
                {
                    _itemViewer.CboFolders.Items.AddRange(_folderHandler.FolderArray);
                    _itemViewer.CboFolders.SelectedIndex = 1;
                    _selectedFolder = _itemViewer.CboFolders.SelectedItem as string;
                }
            });

        }

        public async Task PopulateFolderComboBoxAsync(CancellationToken token, object varList = null)
        {
            token.ThrowIfCancellationRequested();

            await LoadFolderHandlerAsync(varList);

            _itemViewer.CboFolders.Items.AddRange(_folderHandler.FolderArray);
            _itemViewer.CboFolders.SelectedIndex = 1;
            _selectedFolder = _itemViewer.CboFolders.SelectedItem as string;
        }

        public void Cleanup()
        {
            _globals = null;
            _itemViewer = null;
            _parent = null;
            _listTipsDetails = null;
            _mailItem = null;
            //_dfConversation = null;
            _folderHandler = null;
            _webViewEnvironment = null;
            _themes = null;
            _folderHandler = null;
            _tableLayoutPanels = null;
            _explorerController = null;
            //_formController = null;
            _homeController = null;
            _kbdHandler = null;
            _itemPositionTips = null;
            _itemInfo = null;
            _itemViewer = null;
            _emailIsReadTimer = null;
        }

        #endregion

        #region private fields and variables

        //private bool _isDarkMode;
        private bool _isWebViewerInitialized = false;
        private bool _suppressEvents = false;
        private CoreWebView2Environment _webViewEnvironment;
        private Dictionary<string, Theme> _themes;
        private FolderHandler _folderHandler;
        private IApplicationGlobals _globals;
        private IList<TableLayoutPanel> _tableLayoutPanels;
        private IQfcCollectionController _parent;
        private IQfcExplorerController _explorerController;
        //private IFilerFormController _formController;
        private IFilerHomeController _homeController;
        private IQfcKeyboardHandler _kbdHandler;
        private IQfcTipsDetails _itemPositionTips;
        private MailItemInfo _itemInfo;
        private ItemViewer _itemViewer;
        private string _activeTheme;
        private System.Threading.Timer _emailIsReadTimer;
        private bool _optionConversationChecked;
        private bool _optionEmailCopy;
        private bool _optionAttachments;
        private bool _optionsPictures;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;
        private TlpCellStates _tlpStates;

        #endregion

        #region Exposed properties

        private IList<Button> _buttons;
        public IList<Button> Buttons { get => _buttons; private set => _buttons = value; }

        private string _convOriginID = "";
        public string ConvOriginID { get => _convOriginID; set => _convOriginID = value; }

        private ConversationResolver _conversationResolver;
        public ConversationResolver ConversationResolver { get => _conversationResolver; private set => _conversationResolver = value; }
        
        private int _intEnterCounter = 0;
        public int CounterEnter { get => _intEnterCounter; set => _intEnterCounter = value; }

        private int _intComboRightCtr = 0;
        public int CounterComboRight { get => _intComboRightCtr; set => _intComboRightCtr = value; }
        
        public int Height { get => _itemViewer.Height; }

        public bool IsExpanded { get => _expanded; }
        private bool _expanded = false;

        public bool IsChild { get => _isChild; set => _isChild = value; }
        private bool _isChild;

        public bool IsActiveUI { get => _activeUI; set => _activeUI = value; }
        private bool _activeUI = false;

        private IList<IQfcTipsDetails> _listTipsDetails;
        public IList<IQfcTipsDetails> ListTipsDetails { get => _listTipsDetails; }

        //private ValueTask<List<IQfcTipsDetails>> _listTipsDetailsAsync;
        //public ValueTask<List<IQfcTipsDetails>> ListTipsDetailsAsync { get => _listTipsDetailsAsync; }

        private IList<IQfcTipsDetails> _listTipsExpanded;
        public IList<IQfcTipsDetails> ListTipsExpanded { get => _listTipsExpanded; }

        //private ValueTask<List<IQfcTipsDetails>> _listTipsExpandedAsync;
        //public ValueTask<List<IQfcTipsDetails>> ListTipsExpandedAsync { get => _listTipsExpandedAsync; }

        private MailItem _mailItem;
        public MailItem Mail { get => _mailItem; set => _mailItem = value; }

        public IQfcCollectionController Parent { get => _parent; }

        private int _itemNumber;
        public int ItemNumber
        {
            get => _itemNumber;
            set
            {
                _itemNumber = value;
                if (ItemNumberDigits == 1)
                {
                    _itemViewer.LblItemNumber.Text = _itemNumber.ToString();
                }
                else
                {
                    _itemViewer.LblItemNumber.Text = _itemNumber.ToString("00");
                }
            }
        }
        public int ItemIndex { get => ItemNumber - 1; set => _itemNumber = value + 1; }

        private int _itemNumberDigits = 1;
        public int ItemNumberDigits 
        { 
            get => _itemNumberDigits;
            set 
            { 
                _itemNumberDigits = value; 
                if (value == 1)
                {
                    _itemViewer.LblItemNumber.Text = _itemNumber.ToString();
                }
                else
                {
                    _itemViewer.LblItemNumber.Text = _itemNumber.ToString("00");
                }
            }
        }

        private string _selectedFolder;
        public string SelectedFolder { get => _selectedFolder; }

        public string Sender { get => _itemInfo.SenderName; }

        public string SentDate { get => _itemInfo.SentDate.ToString("MM/dd/yyyy"); }

        public string SentTime { get => _itemInfo.SentDate.ToString("HH:mm"); }

        public string Subject { get => _itemInfo.Subject; }

        public bool SuppressEvents { get => _suppressEvents; set => _suppressEvents = value; }

        public string To { get => _itemInfo.ToRecipientsName; }

        public IList<TableLayoutPanel> TableLayoutPanels { get => _tableLayoutPanels; }

        #endregion

        #region INotifyPropertyChanged implementation

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged is not null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //public void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(DfConversationExpanded))
        //    {
        //        _ = GetConversationInfoAsync().ConfigureAwait(false);
        //    }
        //}

        //internal async Task GetConversationInfoAsync()
        //{
        //    var olNs = _globals.Ol.App.GetNamespace("MAPI");
        //    DataFrame df = DfConversationExpanded;

        //    // Initialize the ConversationInfo list from the Dataframe with Synchronous code
        //    ConversationInfo = Enumerable.Range(0, df.Rows.Count())
        //                                 .Select(indexRow => new MailItemInfo(df, indexRow))
        //                                 .OrderByDescending(itemInfo => itemInfo.ConversationIndex)
        //                                 .ToList();

        //    // Switch to UI Thread
        //    await _itemViewer.UiSyncContext;

        //    // Set the TopicThread to the ConversationInfo list
        //    _itemViewer.TopicThread.SetObjects(ConversationInfo);
        //    _itemViewer.TopicThread.Sort(_itemViewer.SentDate, SortOrder.Descending);

        //    // Run the async code in parallel to resolve the mail item and load extended properties
        //    ConversationItems = Task.WhenAll(ConversationInfo.Select(async itemInfo =>
        //                                    {
        //                                        await itemInfo.LoadAsync(olNs, _isDarkMode).ConfigureAwait(false);
        //                                        return itemInfo.Item;
        //                                    }))
        //                            .Result
        //                            .ToList();
        //}

        public void SetTopicThread(List<MailItemInfo> conversationInfo)
        {
            // Set the TopicThread to the ConversationInfo list
            _itemViewer.TopicThread.SetObjects(conversationInfo);
            _itemViewer.TopicThread.Sort(_itemViewer.SentDate, SortOrder.Descending);
        }

        #endregion

        #region Wire Events

        internal void WireEvents()
        {
            _itemViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_kbdHandler.KeyboardHandler_PreviewKeyDownAsync);
                //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_kbdHandler.KeyboardHandler_KeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_kbdHandler.KeyboardHandler_KeyDownAsync);
            },
            //new List<Control> { _itemViewer.CboFolders, _itemViewer.TxtboxSearch, _itemViewer.TopicThread });
            new List<Control> { _itemViewer.CboFolders, });

            _itemViewer.ConversationMenuItem.CheckedChanged += this.CbxConversation_CheckedChanged;
            _itemViewer.BtnFlagTask.Click += this.BtnFlagTask_Click;
            _itemViewer.BtnPopOut.Click += this.BtnPopOut_Click;
            _itemViewer.BtnDelItem.Click += this.BtnDelItem_Click;
            _itemViewer.BtnReply.Click += this.BtnReply_Click;
            _itemViewer.BtnReplyAll.Click += this.BtnReplyAll_Click;
            _itemViewer.BtnForward.Click += this.BtnForward_Click;
            _itemViewer.TxtboxBody.DoubleClick += this.TxtboxBody_DoubleClick;
            
            foreach (var btn in Buttons)
            {
                btn.MouseEnter += this.Button_MouseEnter;
                btn.MouseLeave += this.Button_MouseLeave;
            }

            foreach (ToolStripMenuItem menuItem in _itemViewer.MenuItems)
            {
                menuItem.MouseEnter += this.MenuItem_MouseEnter;
                menuItem.MouseLeave += this.MenuItem_MouseLeave;
            }

            _itemViewer.TxtboxSearch.TextChanged += new System.EventHandler(this.TextBoxSearch_TextChanged);
            //_itemViewer.TxtboxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSearch_KeyDown);
            _itemViewer.CboFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(_kbdHandler.CboFolders_KeyDownAsync);
            //_itemViewer.CboFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(_kbdHandler.CboFolders_KeyDown);
            _itemViewer.CboFolders.SelectedIndexChanged += this.CboFolders_SelectedIndexChanged;
            _itemViewer.L0v2h2_WebView2.CoreWebView2InitializationCompleted += WebView2Control_CoreWebView2InitializationCompleted;
            _itemViewer.TopicThread.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(this.TopicThread_ItemSelectionChanged);
            _itemViewer.TxtboxSearch.KeyDown += this.TextBoxSearch_KeyDown;
            _itemViewer.SaveEmailMenuItem.CheckedChanged += this.CbxEmailCopy_CheckedChanged; 
            _itemViewer.SaveAttachmentsMenuItem.CheckedChanged += this.CbxAttachments_CheckedChanged;
            
        }

        internal void WebView2Control_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                throw (e.InitializationException);
            }
            _isWebViewerInitialized = true;
            _itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.Html);
            //_itemViewer.L0v2h2_Panel.Visible = false;
        }

        internal void RegisterFocusActions()
        {
            _kbdHandler.KeyActions.Add(
                _itemInfo.EntryId, Keys.Right, (x) => this.ToggleConversationCheckbox(Enums.ToggleState.Off));
            _kbdHandler.KeyActions.Add(
                _itemInfo.EntryId, Keys.Left, (x) => this.ToggleConversationCheckbox(Enums.ToggleState.On));
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'O', (x) => _ = _explorerController.OpenQFItem(Mail));
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'C', (x) => this.ToggleConversationCheckbox());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'A', (x) => this.ToggleSaveAttachments());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'M', (x) => this.ToggleSaveCopyOfMail());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'E', (x) => this.ToggleExpansion());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'S', (x) => this.JumpToSearchTextbox());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'T', (x) => this.FlagAsTask());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'P', (x) => this._parent.PopOutControlGroup(ItemNumber));
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'R', (x) => this._parent.RemoveSpecificControlGroup(ItemNumber));
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'X', (x) => this.MarkItemForDeletion());
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'F', (x) => this.JumpToFolderDropDown());
            if (_expanded) { RegisterExpandedActions(); }
        }

        internal void RegisterFocusAsyncActions()
        {
            // TODO: Reference controls from new menu
            //_kbdHandler.KeyActionsAsync.Add(_itemInfo.EntryId, Keys.Right, (x) => ToggleCheckboxAsync(_itemViewer.CbxConversation, Enums.ToggleState.Off));
            //_kbdHandler.KeyActionsAsync.Add(_itemInfo.EntryId, Keys.Left, (x) => ToggleCheckboxAsync(_itemViewer.CbxConversation, Enums.ToggleState.On));
            //_kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'A', (x) => this.ToggleCheckboxAsync(_itemViewer.CbxAttachments));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'C', (x) => this.ToggleCbMenuItemAsync(_itemViewer.ConversationMenuItem));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'O', (x) => _ = _explorerController.OpenQFItem(Mail));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'M', (x) => this.KbdExecuteAsync(MenuDropDown, true));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'R', (x) => this.KbdExecuteAsync(Reply, true));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'L', (x) => this.KbdExecuteAsync(ReplyAll, true));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'W', (x) => this.KbdExecuteAsync(Forward, true));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'E', (x) => this.ToggleExpansionAsync());
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'S', (x) => this.JumpToAsync(_itemViewer.TxtboxSearch));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'T', (x) => this.KbdExecuteAsync(FlagAsTaskAsync, true));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'P', (x) => this.KbdExecuteAsync(_parent.PopOutControlGroupAsync, ItemNumber, false));    
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'Z', (x) => this.KbdExecuteAsync(_parent.RemoveSpecificControlGroupAsync, ItemNumber, false)); 
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'X', (x) => this.KbdExecuteAsync(this.MarkItemForDeletionAsync, false));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'F', (x) => this.JumpToFolderDropDownAsync());
            if (_expanded) { RegisterExpandedAsyncActions(); }
            
            
        }

        internal void RegisterExpandedActions()
        {
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'B', async (x) => await JumpToAsync(_itemViewer.L0v2h2_WebView2));
            _kbdHandler.CharActions.Add(_itemInfo.EntryId, 'D', async (x) => await JumpToAsync(_itemViewer.TopicThread));
        }

        internal void RegisterExpandedAsyncActions()
        {
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'B', (x) => JumpToAsync(_itemViewer.L0v2h2_WebView2));
            _kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'D', (x) => JumpToAsync(_itemViewer.TopicThread));
        }

        internal void UnregisterFocusActions()
        {
            _kbdHandler.KeyActions.Remove(_itemInfo.EntryId, Keys.Right);
            _kbdHandler.KeyActions.Remove(_itemInfo.EntryId, Keys.Left);
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'O');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'C');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'A');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'M');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'E');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'S');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'T');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'P');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'R');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'X');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'F');
            if (_expanded) { UnregisterExpandedActions(); }
        }

        internal void UnregisterFocusAsyncActions()
        {
            //_kbdHandler.KeyActionsAsync.Remove(_itemInfo.EntryId, Keys.Right);
            //_kbdHandler.KeyActionsAsync.Remove(_itemInfo.EntryId, Keys.Left);
            //_kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'A');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'C');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'O');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'M');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'R');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'L');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'W');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'E');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'S');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'T');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'P');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'Z');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'X');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'F');
            if (_expanded) { UnregisterExpandedAsyncActions(); }
        }

        internal void UnregisterExpandedActions() 
        {
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'B');
            _kbdHandler.CharActions.Remove(_itemInfo.EntryId, 'D');
        }

        internal void UnregisterExpandedAsyncActions()
        {
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'B');
            _kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'D');
        }

        #endregion

        #region Event Handlers

        internal void CbxConversation_CheckedChanged(object sender, EventArgs e)
        {
            TraceUtility.LogMethodCall(sender, e);

            _optionConversationChecked = _itemViewer.ConversationMenuItem.Checked;
            if (!SuppressEvents)
            {
                if (_optionConversationChecked) { CollapseConversation(); }
                else { EnumerateConversation(); }
            }
        }

        internal void BtnFlagTask_Click(object sender, EventArgs e) => FlagAsTask();
        
        internal async void BtnPopOut_Click(object sender, EventArgs e) => await _parent.PopOutControlGroupAsync(ItemNumber);

        internal void BtnDelItem_Click(object sender, EventArgs e) => MarkItemForDeletion();

        internal async void BtnReply_Click(object sender, EventArgs e) => await Reply();

        internal async void BtnReplyAll_Click(object sender, EventArgs e) => await ReplyAll();

        internal async void BtnForward_Click(object sender, EventArgs e) => await Forward();

        internal async void TxtboxBody_DoubleClick(object sender, EventArgs e)
        {
            await Task.Run(() => Mail.Display());
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = _themes[_activeTheme].ButtonMouseOverColor; 
        }

        private void MenuItem_MouseEnter(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).BackColor = _themes[_activeTheme].ButtonMouseOverColor;
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

        private void MenuItem_MouseLeave(object sender, EventArgs e)
        { 
            ((ToolStripMenuItem)sender).BackColor = _themes[_activeTheme].ButtonBackColor;
            
        }

        internal void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            _itemViewer.CboFolders.Items.Clear();
            _itemViewer.CboFolders.Items.AddRange(
                _folderHandler.FindFolder(searchString: "*" + 
                _itemViewer.TxtboxSearch.Text + "*",
                reloadCTFStagingFiles: false,
                recalcSuggestions: false,
                objItem: Mail));

            if (_itemViewer.CboFolders.Items.Count >= 2)
                _itemViewer.CboFolders.SelectedIndex = 1;
            _itemViewer.CboFolders.DroppedDown = true;
        }

        internal void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                _itemViewer.CboFolders.DroppedDown = true;
                _itemViewer.CboFolders.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void TopicThread_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var objects = _itemViewer.TopicThread.SelectedObjects;
            if ((objects is not null)&&(objects.Count !=0))
            {
                var info = objects[0] as MailItemInfo;
                _itemViewer.L0v2h2_WebView2.NavigateToString(info.Html);
            }
           
        }

        private void CbxEmailCopy_CheckedChanged(object sender, EventArgs e)
        {
            _optionEmailCopy = _itemViewer.SaveEmailMenuItem.Checked;
        }

        private void CboFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedFolder = _itemViewer.CboFolders.SelectedItem as string;
        }

        private void CbxAttachments_CheckedChanged(object sender, EventArgs e)
        {
            _optionAttachments = _itemViewer.SaveAttachmentsMenuItem.Checked;
        }

        #endregion

        #region UI Navigation Methods

        public void JumpToFolderDropDown()
        {
            _kbdHandler.ToggleKeyboardDialog();
            _itemViewer.Invoke(new System.Action(() =>
            {
                _itemViewer.CboFolders.Focus();
                _itemViewer.CboFolders.DroppedDown = true;
                _intEnterCounter = 0;
            }));
        }

        public async Task JumpToFolderDropDownAsync()
        {
            await _kbdHandler.ToggleKeyboardDialogAsync();
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                _itemViewer.CboFolders.Focus();
                _itemViewer.CboFolders.DroppedDown = true;
                _intEnterCounter = 0;
            });
        }

        public void JumpToSearchTextbox()
        {
            _kbdHandler.ToggleKeyboardDialog();
            _itemViewer.TxtboxSearch.Invoke(new System.Action(() => _itemViewer.TxtboxSearch.Focus()));
        }

        async internal Task JumpToAsync(Control control)
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => control.Focus());
            await _kbdHandler.ToggleKeyboardDialogAsync();
        }

        async public Task KbdExecuteAsync(Func<Task> action, bool deactivateKbd)
        {
            if (deactivateKbd) { _homeController.KeyboardHandler.ToggleKeyboardDialog(); }
            await action();
        }

        async public Task KbdExecuteAsync<T>(Func<T, Task> action, T parameter, bool deactivateKbd)
        {
            if (deactivateKbd) { _homeController.KeyboardHandler.ToggleKeyboardDialog(); }
            await action(parameter);
        }

        async public Task MenuDropDown()
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(
                ()=>_itemViewer.MoveOptionsMenu.ShowDropDown());
        }

        async public Task Reply()
        {
            var reply = await UIThreadExtensions.UiDispatcher.InvokeAsync(
                ()=> this.Mail.Reply());
            reply.Display();
        }

        async public Task ReplyAll()
        {
            var reply = await UIThreadExtensions.UiDispatcher.InvokeAsync(
                () => this.Mail.ReplyAll());
            reply.Display();
        }

        async public Task Forward()
        {
            var forward = await UIThreadExtensions.UiDispatcher.InvokeAsync(
                () => this.Mail.Forward());
            forward.Display();
        }

        async public Task ToggleCbMenuItemAsync(ToolStripMenuItemCb menuItem)
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => menuItem.Checked = !menuItem.Checked);
        }

        async public Task ToggleCbMenuItemAsync(ToolStripMenuItemCb menuItem, Enums.ToggleState desiredState)
        {
            var booleanState = desiredState.HasFlag(Enums.ToggleState.On);

            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                if (menuItem.Checked != booleanState) { menuItem.Checked = booleanState; }
            });
        }

        async public Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => checkBox.Checked = !checkBox.Checked);
        }

        async public Task ToggleCheckboxAsync(CheckBox checkBox, Enums.ToggleState desiredState)
        {
            var booleanState = desiredState.HasFlag(Enums.ToggleState.On);
            
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                if (checkBox.Checked != booleanState) { checkBox.Checked = booleanState; }
            });
            //await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
        }

        /// <summary>
        /// Function programmatically clicks the "Conversation" checkbox
        /// </summary>
        public void ToggleConversationCheckbox()
        {
            UIThreadExtensions.UiDispatcher.Invoke(() => 
                _itemViewer.ConversationMenuItem.Checked = 
                !_itemViewer.ConversationMenuItem.Checked);
        }

        /// <summary>
        /// Function programmatically sets the "Conversation" checkbox to the desired state 
        /// if it is not already in that state
        /// </summary>
        /// <param name="desiredState">State of checkbox desired</param>
        public void ToggleConversationCheckbox(Enums.ToggleState desiredState)
        {
            UIThreadExtensions.UiDispatcher.Invoke(() =>
            {
                switch (desiredState)
                {
                    case Enums.ToggleState.On:
                        if (_itemViewer.ConversationMenuItem.Checked == false)
                            _itemViewer.ConversationMenuItem.Checked = true;
                        break;
                    case Enums.ToggleState.Off:
                        if (_itemViewer.ConversationMenuItem.Checked == true)
                            _itemViewer.ConversationMenuItem.Checked = false;
                        break;
                    default:
                        _itemViewer.ConversationMenuItem.Checked = 
                        !_itemViewer.ConversationMenuItem.Checked;
                        break;
                }
            });

        }

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
            _parent.ToggleExpansionStyle(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                ToggleExpansionOn();
                RegisterExpandedActions();
            }
            else
            {
                ToggleExpansionOff();
                UnregisterExpandedActions();
            }
        }

        public async Task ToggleExpansionAsync(Enums.ToggleState desiredState)
        {
            await _parent.ToggleExpansionStyleAsync(ItemIndex, desiredState);
            if (desiredState == Enums.ToggleState.On)
            {
                await UIThreadExtensions.UiDispatcher.InvokeAsync(() => ToggleExpansionOn());
                RegisterExpandedAsyncActions();
            }
            else
            {
                await UIThreadExtensions.UiDispatcher.InvokeAsync(() => ToggleExpansionOff());
                UnregisterExpandedAsyncActions();
            }
        }
        
        private void ToggleExpansionOff()
        {
            _tlpStates["Compressed"].ApplyState(_itemViewer);
            _expanded = false;
            if (_emailIsReadTimer is not null) { _emailIsReadTimer.Dispose(); }
        }

        private void ToggleExpansionOn()
        {
            _tlpStates["Expanded"].ApplyState(_itemViewer);
            _expanded = true;
            if ((_itemInfo is not null) && _itemInfo.UnRead == true)
            {
                _emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat);
                _emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);
            }
        }

        public void ToggleFocus(Enums.ToggleState desiredState)
        {
            _itemViewer.Invoke(new System.Action(() =>
            {
                if ((desiredState == Enums.ToggleState.On) && (!_activeUI))
                {
                    // If not active and we want to turn on, then we are turning on
                    _activeUI = true;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkActive"; }
                    else { _activeTheme = "LightActive"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.On);
                    //RegisterFocusActions();
                    RegisterFocusAsyncActions();
                }
                else if ((desiredState == Enums.ToggleState.Off) && (_activeUI))
                {
                    // If active and we want to turn off, then we are turning off
                    _activeUI = false;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkNormal"; }
                    else { _activeTheme = "LightNormal"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.Off);
                    //UnregisterFocusActions();
                    UnregisterFocusAsyncActions();
                }
                _themes[_activeTheme].SetQfcTheme(async: false);
            }));
        }

        public async Task ToggleFocusAsync(Enums.ToggleState desiredState) 
        {
            var boolDesiredState = desiredState.HasFlag(Enums.ToggleState.On);
            if (_activeUI && !boolDesiredState) { await ToggleFocusOffAsync(); }
            else if (!_activeUI && boolDesiredState) { await ToggleFocusOnAsync(); }
            await _themes[_activeTheme].SetQfcThemeAsync();
        }

        public void ToggleFocus()
        {
            _itemViewer.Invoke(new System.Action(() =>
            {
                if (_activeUI)
                {
                    // If active, then we are turning off
                    _activeUI = false;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkNormal"; }
                    else { _activeTheme = "LightNormal"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.Off);
                    //UnregisterFocusActions();
                    UnregisterFocusAsyncActions();
                }
                else
                {
                    // If not active, then we are turning on
                    _activeUI = true;
                    if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkActive"; }
                    else { _activeTheme = "LightActive"; }
                    ToggleTips(async: false, desiredState: Enums.ToggleState.On);
                    //RegisterFocusActions();
                    RegisterFocusAsyncActions();
                }
                _themes[_activeTheme].SetQfcTheme(async: false);
            }));
        }

        public async Task ToggleFocusAsync()
        {
            if (_activeUI) { await ToggleFocusOffAsync(); }
            else { await ToggleFocusOnAsync(); }
            await _themes[_activeTheme].SetQfcThemeAsync(); 
        }

        private async Task ToggleFocusOnAsync()
        {
            _activeUI = true;
            if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkActive"; }
            else { _activeTheme = "LightActive"; }
            await ToggleTipsAsync(desiredState: Enums.ToggleState.On);
            RegisterFocusAsyncActions();
        }

        private async Task ToggleFocusOffAsync()
        {
            _activeUI = false;
            if (_activeTheme.Contains("Dark")) { _activeTheme = "DarkNormal"; }
            else { _activeTheme = "LightNormal"; }
            await ToggleTipsAsync(desiredState: Enums.ToggleState.Off);
            UnregisterFocusAsyncActions();
        }

        public void ToggleNavigation(bool async)
        {
            _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));
            if (async)
            {
                _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));
            }
            else
            {
                _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(false)));
            }
        }

        public void ToggleNavigation(bool async, Enums.ToggleState desiredState)
        {
            if (async)
            {
                _itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(desiredState, false)));
            }
            else
            {
                _itemViewer.Invoke(new System.Action(() => _itemPositionTips.Toggle(desiredState, false)));
            }
            
        }

        public async Task ToggleNavigationAsync(Enums.ToggleState desiredState)
        {
            await _itemPositionTips.ToggleAsync(desiredState, false);
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            InvokeBeginInvoke(async, new System.Action(() => 
            { 
                _tableLayoutPanels.ForEach(x => x.SuspendLayout());
                ListTipsDetails.ForEach(x => x.Toggle(desiredState, shareColumn: false));
                if (_expanded || desiredState.HasFlag(Enums.ToggleState.Force))
                { 
                    ListTipsExpanded.ForEach(x => x.Toggle(desiredState, shareColumn: false)); 
                }
                _tableLayoutPanels.ForEach(x => x.ResumeLayout());
            }));
        }

        public async Task ToggleTipsAsync(Enums.ToggleState desiredState)
        {
            TraceUtility.LogMethodCall(desiredState);

            _token.ThrowIfCancellationRequested();

            //List<Task> tasks = new List<Task>();
            //tasks.Add(ListTipsDetails.ToAsyncEnumerable().ForEachAsync(async x => await x.ToggleAsync(desiredState, shareColumn: true)));
            
            foreach (var tip in ListTipsDetails)
            {
                await tip.ToggleAsync(desiredState, shareColumn: false);
            }
            //await ListTipsExpanded.ToAsyncEnumerable().ForEachAsync(async x => await x.ToggleAsync(desiredState, shareColumn: true));
            //var tasks = ListTipsExpanded.Select(x => x.ToggleAsync(desiredState, shareColumn: true));
            //ListTipsExpanded.ForEach(async x => await x.ToggleAsync(desiredState, shareColumn: true));
            
            if (_expanded || desiredState.HasFlag(Enums.ToggleState.Force))
            {
                foreach (var tip in ListTipsExpanded)
                {
                    await tip.ToggleAsync(desiredState, shareColumn: false);
                }
                //await ListTipsExpanded.ToAsyncEnumerable().ForEachAsync(async x => await x.ToggleAsync(desiredState, shareColumn: true));
                //tasks = tasks.Concat(ListTipsExpanded.Select(x => x.ToggleAsync(desiredState, shareColumn: true)));
                //ListTipsExpanded.ForEach(async x => await x.ToggleAsync(desiredState, shareColumn: true));
            }
            
        }

        public void InvokeBeginInvoke(bool async, System.Action action)
        {
            if (async) { _itemViewer.BeginInvoke(action); }
            else { _itemViewer.Invoke(action); }
        }

        public void ToggleSaveAttachments()
        {
            // Connect method to new menu
            //_itemViewer.CbxAttachments.Invoke(new System.Action(() => 
            //    _itemViewer.CbxAttachments.Checked = 
            //    !_itemViewer.CbxAttachments.Checked));
        }

        public void ToggleSaveCopyOfMail()
        {
            UIThreadExtensions.UiDispatcher.Invoke(() =>
                _itemViewer.SaveEmailMenuItem.Checked = 
                !_itemViewer.SaveEmailMenuItem.Checked);            
        }

        #endregion

        #region UI Visual Helper Methods

        public void SetThemeDark(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["DarkNormal"].SetQfcTheme(async);
                _activeTheme = "DarkNormal";
            }
            else
            {
                _themes["DarkActive"].SetQfcTheme(async);
                _activeTheme = "DarkActive";
            }
        }

        public void HtmlDarkConverter(Enums.ToggleState desiredState)
        {
            if (_isWebViewerInitialized)
            {
                _itemViewer.L0v2h2_WebView2.NavigateToString(_itemInfo.ToggleDark(desiredState));
                if (ConversationResolver.Count.Expanded > 0)
                {
                    ConversationResolver.ConversationInfo.Expanded.ForEach(item => item.ToggleDark(desiredState));
                }
            }
        }

        public void SetThemeLight(bool async)
        {
            if ((_activeTheme is null) || _activeTheme.Contains("Normal"))
            {
                _themes["LightNormal"].SetQfcTheme(async);
                _activeTheme = "LightNormal";
            }
            else
            {
                _themes["LightActive"].SetQfcTheme(async);
                _activeTheme = "LightActive";
            }
            //_isDarkMode = false;
        }

        public void ApplyReadEmailFormat(object state)
        {
            _itemInfo.UnRead = false;
            _themes[_activeTheme].SetMailRead(async: true);
            Mail.UnRead = false;
            Mail.Save();
        }

        #endregion

        #region Major Action Methods

        internal void CollapseConversation()
        {
            TraceUtility.LogMethodCall();

            var folderList = _itemViewer.CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            var entryID = _convOriginID != "" ? _convOriginID :  Mail.EntryID;
            _parent.ToggleGroupConv(entryID);
        }

        internal void EnumerateConversation() 
        {
            TraceUtility.LogMethodCall();

            var folderList = _itemViewer.CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            _parent.ToggleUnGroupConv(ConversationResolver,
                                       Mail.EntryID,
                                       ConversationResolver.Count.SameFolder,
                                       folderList);
        }

        internal async Task EnumerateConversationAsync()
        {
            await UIThreadExtensions.UiDispatcher.InvokeAsync(EnumerateConversation);
        }

        public Dictionary<string, System.Action> RightKeyActions { get => new() 
        {
            { "&Pop Out", ()=>this._parent.PopOutControlGroup(ItemNumber)},
            { "&Expand", ()=>{_itemViewer.LblSubject.Focus(); this.EnumerateConversation(); } },
            { "&Cancel", ()=>{ } }
        }; }

        public Dictionary<string, Func<Task>> RightKeyActionsAsync
        {
            get => new()
            {
                { "&Pop Out", ()=>this._parent.PopOutControlGroupAsync(ItemNumber)},
                { "&Expand", ()=>this.EnumerateConversationAsync() },
                { "&Cancel", ()=>Task.CompletedTask }
            };
        }

        async public Task MoveMailAsync()
        {
            TraceUtility.LogMethodCall();

            if (Mail is not null)
            {
                IList<MailItem> selItems = PackageItems();
                bool attachments = SelectedFolder != "Trash to Delete" && _optionAttachments;
                try
                {
                    await SortEmail.SortAsync(
                        mailItems: selItems,
                        savePictures: _optionsPictures,
                        destinationOlStem: SelectedFolder,
                        saveMsg: _optionEmailCopy,
                        saveAttachments: attachments,
                        removePreviousFsFiles: false,
                        appGlobals: _globals,
                        olAncestor: _globals.Ol.ArchiveRootPath,
                        fsAncestorEquivalent: _globals.FS.FldrRoot);
                }
                catch (System.Exception e)
                {
                    logger.Debug($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
                    logger.Error($"{e}");
                    MessageBox.Show($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
                }

                SortEmail.Cleanup_Files();
            }
        }

        internal IList<MailItem> PackageItems()
        {
            return _optionConversationChecked ? ConversationResolver.ConversationItems.SameFolder : new List<MailItem> { Mail };
        }
               
        public void FlagAsTask()
        {
            List<MailItem> itemList = [Mail];
            var flagTask = new FlagTasks(AppGlobals: _globals,
                                         ItemList: itemList,
                                         blFile: false,
                                         hWndCaller: _homeController.FormController.FormHandle);
            _itemViewer.BtnFlagTask.DialogResult = flagTask.Run(modal: true);
            if (_itemViewer.BtnFlagTask.DialogResult == DialogResult.OK)
            {
                _itemViewer.BtnFlagTask.BackColor = _themes[_activeTheme].ButtonClickedColor;
            }
        }

        public async Task FlagAsTaskAsync()
        {
            List<MailItem> itemList = [Mail];
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => 
            {
                var flagTask = new FlagTasks(AppGlobals: _globals,
                                         ItemList: itemList,
                                         blFile: false,
                                         hWndCaller: _homeController.FormController.FormHandle);
                _itemViewer.BtnFlagTask.DialogResult = flagTask.Run(modal: true);
                if (_itemViewer.BtnFlagTask.DialogResult == DialogResult.OK)
                {
                    _itemViewer.BtnFlagTask.BackColor = _themes[_activeTheme].ButtonClickedColor;
                }
            });
        }

        public void MarkItemForDeletion()
        {
            if (!_itemViewer.CboFolders.Items.Contains("Trash to Delete"))
            {
                _itemViewer.CboFolders.Items.Add("Trash to Delete");
            }
            _itemViewer.CboFolders.SelectedItem = "Trash to Delete";
        }

        public async Task MarkItemForDeletionAsync()
        {
            _token.ThrowIfCancellationRequested();
            await UIThreadExtensions.UiDispatcher.InvokeAsync(() =>
            {
                if (!_itemViewer.CboFolders.Items.Contains("Trash to Delete"))
                {
                    _itemViewer.CboFolders.Items.Add("Trash to Delete");
                }
                _itemViewer.CboFolders.SelectedItem = "Trash to Delete";
            });
        }

        #endregion
    }
}
