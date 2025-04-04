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
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using System.Net.NetworkInformation;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal class QfcItemController : IQfcItemController, INotifyPropertyChanged, IItemControler
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        private QfcItemController() { }

        public QfcItemController(IApplicationGlobals appGlobals,
                                 IFilerHomeController homeController,
                                 IQfcCollectionController parent,
                                 ItemViewer itemViewer,
                                 int viewerPosition,
                                 int itemNumberDigits,
                                 MailItem mailItem,
                                 TlpCellStates tlpStates)
        {
            //TraceUtility.LogMethodCall(appGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
            SaveParameters(appGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);
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
            //TraceUtility.LogMethodCall();

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
            };

            await Task.WhenAll(tasks);

            tasks = new List<Task> 
            {
                PopulateConversationAsync(_tokenSource, _token, false),
                PopulateFolderComboBoxAsync(default, null)
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
            IApplicationGlobals appGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController parent,
            ItemViewer itemViewer,
            int viewerPosition,
            int itemNumberDigits,
            MailItem mailItem,
            TlpCellStates tlpStates)
        {
            //TraceUtility.LogMethodCall(appGlobals, homeController, parent, itemViewer, viewerPosition, itemNumberDigits, mailItem, tlpStates);

            // Save parameters to private fields
            _globals = appGlobals;
            _homeController = homeController;
            _parent = parent;
            _itemViewer = itemViewer;
            _mailItem = mailItem;
            _tlpStates = tlpStates;
            _itemNumberDigits = itemNumberDigits;
            //ItemNumber = viewerPosition;
            _itemNumber = viewerPosition;

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

        #endregion ctor

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
            //TraceUtility.LogMethodCall();

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
            }, _token, TaskContinuationOptions.OnlyOnRanToCompletion, ui);
        }

        internal void ResolveControlGroups(ItemViewer itemViewer)
        {
            if (itemViewer.InvokeRequired)
            {
                itemViewer.Invoke(() => ResolveControlGroups(itemViewer));
                return;
            }

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
            ItemHelper = new MailItemHelper(mailItem, _globals);
            //_itemInfo.LoadPriority(_globals, _token);
            AssignControls(ItemHelper, viewerPosition);

        }

        internal async Task PopulateControlsAsync(MailItem mailItem, int viewerPosition, bool loadAll)
        {
            //TraceUtility.LogMethodCall(mailItem, viewerPosition, loadAll);

            _token.ThrowIfCancellationRequested();

            ItemHelper = await MailItemHelper.FromMailItemAsync(mailItem, _globals, _token, loadAll);

            AssignControls(ItemHelper, viewerPosition);
            //await AssignControlsAsync(ItemHelper, viewerPosition);

        }

        internal async Task AssignControlsAsync(MailItemHelper itemInfo, int viewerPosition)
        {
            //if (_itemViewer.InvokeRequired)
            //{
            //    //await Task.Factory.StartNew(() => AssignControls(itemInfo, viewerPosition), _token, TaskCreationOptions.None, _itemViewer.UiScheduler);
            //    await _itemViewer.UiDispatcher.InvokeAsync(() => AssignControls(itemInfo, viewerPosition));
            //}
            //else
            //{
            //    AssignControls(itemInfo, viewerPosition);
            //}
            await _itemViewer.UiDispatcher.InvokeAsync(() => AssignControls(itemInfo, viewerPosition));
        }
        
        internal void AssignControls(MailItemHelper itemInfo, int viewerPosition)
        {
            //TraceUtility.LogMethodCall(itemInfo, viewerPosition);
            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => AssignControls(itemInfo, viewerPosition));
                return;
            }

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
            //TraceUtility.LogMethodCall(tokenSource, token, loadAll);
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
            UiThread.Dispatcher.BeginInvoke(() =>
            {
                _itemViewer.LblConvCt.Text = count.ToString();
                if (count == 0) { _itemViewer.LblConvCt.BackColor = Color.Red; }
            });
        }

        public async Task RenderConversationCountAsync(int count, CancellationToken token, bool backgroundLoad)
        {
            //TraceUtility.LogMethodCall(count, token, backgroundLoad);
            token.ThrowIfCancellationRequested();

            DispatcherPriority priority = backgroundLoad ? DispatcherPriority.Background : DispatcherPriority.Normal;

            await UiThread.Dispatcher.InvokeAsync(
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
                _folderHandler = new FolderPredictor(
                    _globals, ItemHelper, FolderPredictor.InitOptions.FromField);
            }
            else
            {
                _folderHandler = new FolderPredictor(
                    _globals, varList, FolderPredictor.InitOptions.FromArrayOrString);
            }
        }

        internal async Task LoadFolderHandlerAsync(object varList = null)
        {
            //TraceUtility.LogMethodCall(varList);
            if (varList is null)
            {
                try
                {
                    _folderHandler = await new FolderPredictor(
                        _globals, ItemHelper.ThrowIfNull(), FolderPredictor.InitOptions.FromField)
                        .InitAsync(ItemHelper, FolderPredictor.InitOptions.FromField);
                }
                catch(ArgumentNullException e)
                {
                    logger.Error(e.Message);
                    logger.Debug("Loading empty folder handler");
                    try
                    {
                        _folderHandler = new FolderPredictor(_globals);
                    }
                    catch (System.Exception e2)
                    {
                        logger.Error(e2.Message, e);
                        throw;
                    }
                    
                }                
                catch (System.Exception e)
                {
                    logger.Error(e.Message, e);
                    throw;
                }
                
            }
            else
            {
                _folderHandler = await new FolderPredictor(
                    _globals, varList, FolderPredictor.InitOptions.FromArrayOrString)
                    .InitAsync(varList, FolderPredictor.InitOptions.FromArrayOrString);
            }
        }

        public void PopulateFolderComboBox(object varList = null)
        {
            //TraceUtility.LogMethodCall(varList);

            LoadFolderHandler(varList);

            if (_itemViewer.InvokeRequired) 
            { 
                _itemViewer.Invoke(() => AssignFolderComboBox());
            }
            else
            {
                AssignFolderComboBox();
            }

        }

        public async Task PopulateFolderComboBoxAsync(CancellationToken token, object varList = null)
        {
            //TraceUtility.LogMethodCall(token, varList);
            token.ThrowIfCancellationRequested();

            await LoadFolderHandlerAsync(varList);
            await _itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox);
            
        }

        private void AssignFolderComboBox()
        {
            //TraceUtility.LogMethodCall();
            if (_folderHandler?.FolderArray?.Length > 0)
            {
                _itemViewer.CboFolders.Items.AddRange(_folderHandler.FolderArray);
                _itemViewer.CboFolders.SelectedIndex = 1;
                _selectedFolder = _itemViewer.CboFolders.SelectedItem as string;
            }
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
            ItemHelper = null;
            _itemViewer = null;
            _emailIsReadTimer = null;
        }

        internal string GetItemSummary() => $"Subject: {ItemHelper.Subject} sent on {ItemHelper.SentDate.ToString("MM/dd/yyyy")} at {ItemHelper.SentDate.ToString("HH:mm")} by {ItemHelper.SenderName}";

        #endregion ItemViewer Setup and Disposal

        #region private fields and variables

        //private bool _isDarkMode;
        private bool _isWebViewerInitialized = false;
        private bool _suppressEvents = false;
        private CoreWebView2Environment _webViewEnvironment;
        private Dictionary<string, Theme> _themes;
        private FolderPredictor _folderHandler;
        private IApplicationGlobals _globals;
        private IList<TableLayoutPanel> _tableLayoutPanels;
        private IQfcCollectionController _parent;
        private IQfcExplorerController _explorerController;
        //private IFilerFormController _formController;
        private IFilerHomeController _homeController;
        private IQfcKeyboardHandler _kbdHandler;
        private IQfcTipsDetails _itemPositionTips;
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

        public MailItemHelper ItemHelper { get => _itemInfo; set => _itemInfo = value; }
        private MailItemHelper _itemInfo;

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
                    if (_itemViewer is not null) 
                    { 
                        _itemViewer.LblItemNumber.Text = _itemNumber.ToString();
                    }
                }
                else
                {
                    if (_itemViewer is not null)
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

        public bool SuppressEvents { get => _suppressEvents; set => _suppressEvents = value; }

        public IList<TableLayoutPanel> TableLayoutPanels { get => _tableLayoutPanels; }

        #endregion Exposed properties

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

        public void SetTopicThread(List<MailItemHelper> conversationInfo)
        {
            // Run on the UI Thread if necessary
            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => SetTopicThread(conversationInfo));
                return;
            }

            // Set the TopicThread to the ConversationInfo list
            _itemViewer.TopicThread.SetObjects(conversationInfo);
            _itemViewer.TopicThread.Sort(_itemViewer.SentDate, SortOrder.Descending);
        }

        #endregion INotifyPropertyChanged implementation

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
            _itemViewer.L0v2h2_WebView2.NavigateToString(ItemHelper.Html);
            //_itemViewer.L0v2h2_Panel.Visible = false;
        }

        internal void RegisterFocusActions()
        {
            _kbdHandler.KeyActions.Add(
                ItemHelper.EntryId, Keys.Right, (x) => this.ToggleConversationCheckbox(Enums.ToggleState.Off));
            _kbdHandler.KeyActions.Add(
                ItemHelper.EntryId, Keys.Left, (x) => this.ToggleConversationCheckbox(Enums.ToggleState.On));
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'O', (x) => _ = _explorerController.OpenQFItem(Mail));
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'C', (x) => this.ToggleConversationCheckbox());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'A', (x) => this.ToggleSaveAttachments());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'M', (x) => this.ToggleSaveCopyOfMail());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'E', (x) => this.ToggleExpansion());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'S', (x) => this.JumpToSearchTextbox());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'T', (x) => this.FlagAsTask());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'P', (x) => this._parent.PopOutControlGroup(ItemNumber));
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'R', (x) => this._parent.RemoveSpecificControlGroup(ItemNumber));
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'X', (x) => this.MarkItemForDeletion());
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'F', (x) => this.JumpToFolderDropDown());
            if (_expanded) { RegisterExpandedActions(); }
        }

        internal void RegisterFocusAsyncActions()
        {
            // TODO: Reference controls from new menu
            //_kbdHandler.KeyActionsAsync.Add(_itemInfo.EntryId, Keys.Right, (x) => ToggleCheckboxAsync(_itemViewer.CbxConversation, Enums.ToggleState.Off));
            //_kbdHandler.KeyActionsAsync.Add(_itemInfo.EntryId, Keys.Left, (x) => ToggleCheckboxAsync(_itemViewer.CbxConversation, Enums.ToggleState.On));
            //_kbdHandler.CharActionsAsync.Add(_itemInfo.EntryId, 'A', (x) => this.ToggleCheckboxAsync(_itemViewer.CbxAttachments));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'C', (x) => this.ToggleCbMenuItemAsync(_itemViewer.ConversationMenuItem));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'O', (x) => _ = _explorerController.OpenQFItem(Mail));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'M', (x) => this.KbdExecuteAsync(MenuDropDown, true));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'R', (x) => this.KbdExecuteAsync(Reply, true));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'L', (x) => this.KbdExecuteAsync(ReplyAll, true));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'W', (x) => this.KbdExecuteAsync(Forward, true));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'E', (x) => this.ToggleExpansionAsync());
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'S', (x) => this.JumpToAsync(_itemViewer.TxtboxSearch));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'T', (x) => this.KbdExecuteAsync(FlagAsTaskAsync, true));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'P', (x) => this.KbdExecuteAsync(_parent.PopOutControlGroupAsync, ItemNumber, false));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'Z', (x) => this.KbdExecuteAsync(_parent.RemoveSpecificControlGroupAsync, ItemNumber, false));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'X', (x) => this.KbdExecuteAsync(this.MarkItemForDeletionAsync, false));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'F', (x) => this.JumpToFolderDropDownAsync());
            if (_expanded) { RegisterExpandedAsyncActions(); }


        }

        internal void RegisterExpandedActions()
        {
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'B', async (x) => await JumpToAsync(_itemViewer.L0v2h2_WebView2));
            _kbdHandler.CharActions.Add(ItemHelper.EntryId, 'D', async (x) => await JumpToAsync(_itemViewer.TopicThread));
        }

        internal void RegisterExpandedAsyncActions()
        {
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'B', (x) => JumpToAsync(_itemViewer.L0v2h2_WebView2));
            _kbdHandler.CharActionsAsync.Add(ItemHelper.EntryId, 'D', (x) => JumpToAsync(_itemViewer.TopicThread));
        }

        internal void UnregisterFocusActions()
        {
            _kbdHandler.KeyActions.Remove(ItemHelper.EntryId, Keys.Right);
            _kbdHandler.KeyActions.Remove(ItemHelper.EntryId, Keys.Left);
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'O');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'C');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'A');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'M');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'E');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'S');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'T');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'P');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'R');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'X');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'F');
            if (_expanded) { UnregisterExpandedActions(); }
        }

        internal void UnregisterFocusAsyncActions()
        {
            //_kbdHandler.KeyActionsAsync.Remove(_itemInfo.EntryId, Keys.Right);
            //_kbdHandler.KeyActionsAsync.Remove(_itemInfo.EntryId, Keys.Left);
            //_kbdHandler.CharActionsAsync.Remove(_itemInfo.EntryId, 'A');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'C');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'O');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'M');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'R');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'L');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'W');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'E');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'S');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'T');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'P');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'Z');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'X');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'F');
            if (_expanded) { UnregisterExpandedAsyncActions(); }
        }

        internal void UnregisterExpandedActions()
        {
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'B');
            _kbdHandler.CharActions.Remove(ItemHelper.EntryId, 'D');
        }

        internal void UnregisterExpandedAsyncActions()
        {
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'B');
            _kbdHandler.CharActionsAsync.Remove(ItemHelper.EntryId, 'D');
        }

        #endregion Wire Events

        #region Event Handlers

        internal void CbxConversation_CheckedChanged(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            //TraceUtility.LogMethodCall(sender, e);

            _optionConversationChecked = _itemViewer.ConversationMenuItem.Checked;
            if (!SuppressEvents)
            {
                if (_optionConversationChecked) { CollapseConversation(); }
                else { EnumerateConversation(); }
            }
        }

        internal void BtnFlagTask_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            FlagAsTask();
        }

        internal async void BtnPopOut_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            await _parent.PopOutControlGroupAsync(ItemNumber);
        }

        internal void BtnDelItem_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            MarkItemForDeletion();
        }

        internal async void BtnReply_Click(object sender, EventArgs e) 
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            await Reply(); 
        }

        internal async void BtnReplyAll_Click(object sender, EventArgs e) 
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            await ReplyAll(); 
        }

        internal async void BtnForward_Click(object sender, EventArgs e) 
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
            await Forward(); 
        }

        internal async void TxtboxBody_DoubleClick(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());
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
                var info = objects[0] as MailItemHelper;
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

        #endregion Event Handlers

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
            await UiThread.Dispatcher.InvokeAsync(() =>
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
            await UiThread.Dispatcher.InvokeAsync(() => control.Focus());
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
            await UiThread.Dispatcher.InvokeAsync(
                ()=>_itemViewer.MoveOptionsMenu.ShowDropDown());
        }

        async public Task Reply()
        {
            var reply = await UiThread.Dispatcher.InvokeAsync(
                ()=> this.Mail.Reply());
            reply.Display();
        }

        async public Task ReplyAll()
        {
            var reply = await UiThread.Dispatcher.InvokeAsync(
                () => this.Mail.ReplyAll());
            reply.Display();
        }

        async public Task Forward()
        {
            var forward = await UiThread.Dispatcher.InvokeAsync(
                () => this.Mail.Forward());
            forward.Display();
        }

        async public Task ToggleCbMenuItemAsync(ToolStripMenuItemCb menuItem)
        {
            await UiThread.Dispatcher.InvokeAsync(() => menuItem.Checked = !menuItem.Checked);
        }

        async public Task ToggleCbMenuItemAsync(ToolStripMenuItemCb menuItem, Enums.ToggleState desiredState)
        {
            var booleanState = desiredState.HasFlag(Enums.ToggleState.On);

            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                if (menuItem.Checked != booleanState) { menuItem.Checked = booleanState; }
            });
        }

        async public Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await UiThread.Dispatcher.InvokeAsync(() => checkBox.Checked = !checkBox.Checked);
        }

        async public Task ToggleCheckboxAsync(CheckBox checkBox, Enums.ToggleState desiredState)
        {
            var booleanState = desiredState.HasFlag(Enums.ToggleState.On);
            
            await UiThread.Dispatcher.InvokeAsync(() =>
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
            UiThread.Dispatcher.Invoke(() => 
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
            UiThread.Dispatcher.Invoke(() =>
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
                await UiThread.Dispatcher.InvokeAsync(() => ToggleExpansionOn());
                RegisterExpandedAsyncActions();
            }
            else
            {
                await UiThread.Dispatcher.InvokeAsync(() => ToggleExpansionOff());
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
            if ((ItemHelper is not null) && ItemHelper.UnRead == true)
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
            //TraceUtility.LogMethodCall(desiredState);

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
            UiThread.Dispatcher.Invoke(() =>
                _itemViewer.SaveEmailMenuItem.Checked = 
                !_itemViewer.SaveEmailMenuItem.Checked);            
        }

        #endregion UI Navigation Methods

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
                _itemViewer.L0v2h2_WebView2.NavigateToString(ItemHelper.ToggleDark(desiredState));
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
            ItemHelper.UnRead = false;
            _themes[_activeTheme].SetMailRead(async: true);
            Mail.UnRead = false;
            Mail.Save();
        }

        #endregion UI Visual Helper Methods

        #region Major Action Methods

        internal void CollapseConversation()
        {
            //TraceUtility.LogMethodCall();

            var folderList = _itemViewer.CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            var entryID = _convOriginID != "" ? _convOriginID :  Mail.EntryID;
            _parent.ToggleGroupConv(entryID);
        }

        internal void EnumerateConversation() 
        {
            //TraceUtility.LogMethodCall();

            var folderList = _itemViewer.CboFolders.Items.Cast<object>().Select(item => item.ToString()).ToArray();
            _parent.ToggleUnGroupConv(ConversationResolver,
                                       Mail.EntryID,
                                       ConversationResolver.Count.SameFolder,
                                       folderList);
        }

        internal async Task EnumerateConversationAsync()
        {
            await UiThread.Dispatcher.InvokeAsync(EnumerateConversation);
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
            //TraceUtility.LogMethodCall();

            if (ItemHelper is not null)
            {
                IList<MailItemHelper> helpers = PackageItems();
                bool attachments = SelectedFolder != "Trash to Delete" && _optionAttachments;
                try
                {
                    if (!_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive))
                    {
                        logger.Debug($"{nameof(MoveMailAsync)} aborted due to lack of OneDrive location");
                        return;
                    }
                    var config = new EmailFilerConfig()
                    {
                        SavePictures = _optionsPictures,
                        DestinationOlStem = SelectedFolder,
                        SaveMsg = _optionEmailCopy,
                        SaveAttachments = attachments,
                        Globals = _globals,
                        OlAncestor = _globals.Ol.ArchiveRootPath,
                        FsAncestorEquivalent = oneDrive,
                    };
                    var filer = new EmailFiler(config);
                    _homeController.FilerQueue.Enqueue(filer, helpers);
                    await Task.CompletedTask;
                    //await filer.SortAsync(helpers);
                }
                catch (System.Exception e)
                {
                    //logger.Debug($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
                    logger.Error($"{e}");
                    MessageBox.Show($"Error moving mail {ItemHelper.Subject} from {ItemHelper.Sender} on {ItemHelper.SentDate}. Skipping");
                }

                //SortEmail.Cleanup_Files();
            }
        }
        //async public Task MoveMailAsync()
        //{
        //    //TraceUtility.LogMethodCall();

        //    if (Mail is not null)
        //    {
        //        IList<MailItem> selItems = PackageItems();
        //        bool attachments = SelectedFolder != "Trash to Delete" && _optionAttachments;
        //        try
        //        {
        //            await SortEmail.SortAsync(
        //                mailItems: selItems,
        //                savePictures: _optionsPictures,
        //                destinationOlStem: SelectedFolder,
        //                saveMsg: _optionEmailCopy,
        //                saveAttachments: attachments,
        //                removePreviousFsFiles: false,
        //                appGlobals: _globals,
        //                olAncestor: _globals.Ol.ArchiveRootPath,
        //                fsAncestorEquivalent: _globals.FS.FldrOneDrive);
        //        }
        //        catch (System.Exception e)
        //        {
        //            //logger.Debug($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
        //            logger.Error($"{e}");
        //            MessageBox.Show($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
        //        }

        //        SortEmail.Cleanup_Files();
        //    }
        //}

        internal IList<MailItemHelper> PackageItems()
        {
            return _optionConversationChecked ? ConversationResolver.ConversationInfo.SameFolder : new List<MailItemHelper> { ItemHelper };
        }
               
        public void FlagAsTask()
        {
            List<MailItem> itemList = [Mail];
            var flagTask = new FlagTasks(globals: _globals,
                                         itemList: itemList,
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
            await UiThread.Dispatcher.InvokeAsync(() => 
            {
                var flagTask = new FlagTasks(globals: _globals,
                                         itemList: itemList,
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
            await UiThread.Dispatcher.InvokeAsync(() =>
            {
                if (!_itemViewer.CboFolders.Items.Contains("Trash to Delete"))
                {
                    _itemViewer.CboFolders.Items.Add("Trash to Delete");
                }
                _itemViewer.CboFolders.SelectedItem = "Trash to Delete";
            });
        }

        #endregion Major Action Methods
    }
}
