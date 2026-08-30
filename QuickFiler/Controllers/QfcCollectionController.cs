using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace QuickFiler.Controllers
{
    [ExcludeFromCodeCoverage]
    public class QfcCollectionController : IQfcCollectionController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors

        public QfcCollectionController(
            IApplicationGlobals AppGlobals,
            IQfcFormViewer viewerInstance,
            QfEnums.InitTypeEnum InitType,
            IFilerHomeController homeController,
            IQfcFormController parent,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            TlpCellStates tlpStates
        )
        {
            _token = token;
            _tokenSource = tokenSource;
            _formViewer = viewerInstance;
            _itemTlp = _formViewer.L1v0L2L3v_TableLayout;
            _itemPanel = _formViewer.L1v0L2_PanelMain;
            _initType = InitType;
            _globals = AppGlobals;
            _homeController = homeController;
            _kbdHandler = _homeController.KeyboardHandler;
            _parent = parent;
            _tlpStates = tlpStates;
            SetupLightDark(_globals.Ol.DarkMode);
        }

        #endregion

        #region Private Variables


        private IQfcFormViewer _formViewer;
        private QfEnums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IFilerHomeController _homeController;
        private IQfcFormController _parent;

        //private int _itemHeight;
        private Panel _itemPanel;
        private TableLayoutPanel _itemTlp;
        private TableLayoutPanel _itemTlpToMove;

        // Issue #469 defect 3: an ordered contract, not a hash-based one. TryGetItemGroupByIndex,
        // MoveEmailsAsync and GetMoveDiagnostics all resolve a group by position, so the cached
        // snapshot must preserve the order of _itemGroups. A ConcurrentDictionary's enumeration
        // order is unspecified and can change on rehash, which silently paired one message's move
        // with another message's diagnostics.
        private IReadOnlyList<QfcItemGroup> _itemGroupsToMove;
        private bool _darkMode;
        private RowStyle _template;
        private RowStyle _templateExpanded;
        private IQfcKeyboardHandler _kbdHandler;
        private delegate int ActionDelegate(int intNewSelection, bool blExpanded);
        private TlpCellStates _tlpStates;
        private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();

        internal ConcurrentBag<Task> BackgroundLoadingTasks = [];

        #endregion

        #region Public Properties

        private int _activeIndex = -1;
        public int ActiveIndex
        {
            get => _activeIndex;
            set => _activeIndex = value;
        }
        public int ActiveSelection
        {
            get => _activeIndex + 1;
            set => _activeIndex = value - 1;
        }

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
            set => _token = value;
        }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource
        {
            get => _tokenSource;
            set => _tokenSource = value;
        }

        private bool _digitRefreshNeeded = false;
        private int _digits = 1;

        // Issue #644: the exact (SourceId, Key) pairs the last RegisterNavigation added, so an
        // _itemGroups mutation between register and unregister cannot orphan a registration.
        private List<(string SourceId, string Key)> _registeredNavigationKeys;

        private List<(string SourceId, string Key)> RegisteredNavigationKeys =>
            _registeredNavigationKeys ??= new List<(string SourceId, string Key)>();

        internal int Digits
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                var digitNeed = _itemGroups?.Count >= 10 ? 2 : 1;
                if (_digits != digitNeed)
                {
                    //SetVisualDigits(digitNeed);
                    _digitRefreshNeeded = true;
                    _digits = digitNeed;
                }
                return _digits;
            }
        }

        private void SetVisualDigits(int digits)
        {
            if (EmailsLoaded > 0)
            {
                var format = string.Join(
                    "",
                    Enumerable.Range(0, digits).Select(x => "0").ToArray()
                );
                _itemGroups.ForEach(grp =>
                {
                    // Issue #470 defect 3: skip the group whole. The controller was dereferenced
                    // unguarded on the first line and null-conditionally on the third, while the
                    // viewer was dereferenced unguarded on the second, so the null-conditional
                    // protected nothing. Guarding only the controller is also insufficient:
                    // execution would then reach the viewer dereference on the next line with the
                    // same arrangement. Both members must dominate every dereference below.
                    if (grp?.ItemController is null || grp.ItemViewer is null)
                    {
                        return;
                    }

                    grp.ItemController.ItemNumberDigits = digits;
                    grp.ItemViewer.LblItemNumber.Text = grp.ItemController.ItemNumber.ToString(
                        format
                    );
                });
            }
            _digitRefreshNeeded = false;
        }

        public int EmailsLoaded => _itemGroups?.Count ?? 0;

        public int EmailsToMove => _itemGroupsToMove?.Count ?? 0;

        /// <summary>
        /// Seam used by <see cref="ReadyForMove"/> to present the "not ready" notification.
        /// Defaults to the exact modal call the getter previously made inline, with the same
        /// message, caption, buttons and icon. Tests inject a recording delegate so the readiness
        /// evaluation can be asserted without presenting a dialog, which a unit test cannot do.
        /// </summary>
        private Action<string> _notifyNotReady;

        private Action<string> NotifyNotReady =>
            _notifyNotReady ??= notifications =>
                MessageBox.Show(
                    notifications,
                    "Error Notification",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

        /// <summary>
        /// Evaluates whether every loaded item group has a real destination folder assigned,
        /// presenting nothing. This is the evaluation half of <see cref="ReadyForMove"/>, split out
        /// so the decision can be inspected independently of the notification.
        /// </summary>
        /// <param name="notifications">
        /// Receives the text describing every unassigned group when the method returns
        /// <see langword="false"/>, and <see cref="string.Empty"/> when it returns
        /// <see langword="true"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when every group has a destination folder that is not one of the
        /// three list-header sentinel strings.
        /// </returns>
        internal bool TryGetMoveReadiness(out string notifications)
        {
            bool blReadyForMove = true;
            string strNotifications =
                "Can't complete actions! Not all emails assigned to folder"
                + System.Environment.NewLine;

            foreach (var grp in _itemGroups)
            {
                string[] headers =
                {
                    "======= SEARCH RESULTS =======",
                    "======= RECENT SELECTIONS ========",
                    "========= SUGGESTIONS =========",
                };
                if (
                    (grp.ItemController.SelectedFolder is null)
                    || headers.Contains(grp.ItemController.SelectedFolder)
                )
                {
                    blReadyForMove = false;
                    strNotifications =
                        strNotifications
                        + grp.ItemController.ItemNumber
                        + "  "
                        + grp.ItemController.Mail.SentOn.ToString("MM/dd/yyyy")
                        + "  "
                        + grp.ItemController.Mail.Subject
                        + Environment.NewLine;
                }
            }

            notifications = blReadyForMove ? string.Empty : strNotifications;
            return blReadyForMove;
        }

        public bool ReadyForMove
        {
            get
            {
                if (TryGetMoveReadiness(out string notifications))
                {
                    return true;
                }

                NotifyNotReady(notifications);
                return false;
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
                        if (_itemTlp.InvokeRequired)
                        {
                            _itemTlp.Invoke(() => _itemTlp.ResumeLayout(true));
                        }
                        else
                        {
                            _itemTlp.ResumeLayout(true);
                        }
                    }
                    else
                    {
                        if (_itemTlp.InvokeRequired)
                        {
                            _itemTlp.Invoke(() => _itemTlp.SuspendLayout());
                        }
                        else
                        {
                            _itemTlp.SuspendLayout();
                        }
                    }
                }
            }
        }

        public bool SafeSetTlpLayout(bool state)
        {
            var originalState = TlpLayout;
            TlpLayout = state;
            return originalState;
        }

        private List<QfcItemGroup> _itemGroups;
        public List<QfcItemGroup> ItemGroups
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _itemGroups;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _itemGroups = value;
        }

        #endregion

        #region UI Add and Remove QfcItems

        public void LoadControlsAndHandlers_01(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)
        {
            itemGroups.ForEach(grp =>
                _moveMonitor.HookItem(grp.MailItem, (x) => RemovedItemMonitor(x.EntryID))
            );
            _formViewer.SuspendLayout();
            ActivateQueuedTlp(tlp);
            // Route the item-groups swap through SwapItemGroups so the outgoing page's "Collection"
            // navigation keys are unregistered and the incoming page's keys are registered as part of
            // the swap (Issue #232). Calling ActivateQueuedItemGroups directly left stale keys behind.
            SwapItemGroups(itemGroups);
            _formViewer.ResumeLayout();
            ActiveIndex = -1;
        }

        public void LoadControlsAndHandlers_01(
            IList<MailItem> listMailItems,
            RowStyle template,
            RowStyle templateExpanded
        )
        {
            // Freeze the form while loading controls
            _formViewer.SuspendLayout();
            var tlpState = SafeSetTlpLayout(false);

            // Save the QfcItem template styles
            _template = template;
            _templateExpanded = templateExpanded;

            // Hook the move monitor to the mail items
            listMailItems.ForEach(mailItem =>
                _moveMonitor.HookItem(mailItem, (x) => RemovedItemMonitor(x.EntryID))
            );

            LoadItemGroupsAndViewers_02(listMailItems, template);

            _formViewer.WindowState = FormWindowState.Maximized;
            TlpLayout = tlpState;

            _formViewer.ResumeLayout();

            WireUpAsyncKeyboardHandler();
            LoadConversationsAndFolders_04();
        }

        public async Task<MailItemHelper> GetPartiallyInitializedHelperAsync(MailItem mailItem)
        {
            var helper = await MailItemHelper.FromMailItemAsync(
                mailItem.ThrowIfNull(),
                _globals.ThrowIfNull(),
                Token,
                false
            );
            await Task.Run(() =>
            {
                _ = helper.SenderName;
                _ = helper.Subject;
                _ = helper.Body;
                _ = helper.Triage;
                _ = helper.SentOn;
                _ = helper.Actionable;
                _ = helper.IsTaskFlagSet;
            });

            return helper;
        }

        private void ValidateParams(
            IList<MailItem> listMailItems,
            RowStyle template,
            RowStyle templateExpanded
        )
        {
            listMailItems.ThrowIfNull();
            template.ThrowIfNull();
            templateExpanded.ThrowIfNull();
            if (_formViewer.InvokeRequired)
            {
                // Method must be run on the UI thread
                var trace = TraceUtility.TryGetMyTraceString(new System.Diagnostics.StackTrace());
                throw new InvalidOperationException(
                    $"{nameof(LoadControlsAndHandlers_01Async)} must be run on the UI thread. Instead it was run"
                        + $"on thread {Thread.CurrentThread.Name}. Method trace: {trace}"
                );
            }
            Token.ThrowIfCancellationRequested();
        }

        public async Task LoadControlsAndHandlers_01Async(
            IList<MailItem> items,
            RowStyle template,
            RowStyle templateExpanded
        )
        {
            ValidateParams(items, template, templateExpanded);

            // Start loading mail item helpers
            var helpers = items.Select(GetPartiallyInitializedHelperAsync).ToList();

            // Freeze the form while loading controls
            _formViewer.SuspendLayout();
            var tlpLayoutState = SafeSetTlpLayout(false);

            // Save the QfcItem template styles
            _template = template;
            _templateExpanded = templateExpanded;

            // Hook the move monitor to the mail items
            BackgroundLoadingTasks.Add(
                Task.Run(() =>
                    items.ForEach(mailItem =>
                        _moveMonitor.HookItem(mailItem, (x) => RemovedItemMonitor(x.EntryID))
                    )
                )
            );

            // Create empty keyboard handler actions
            BackgroundLoadingTasks.Add(Task.Run(CreateEmptyKbdHandlerCharActions, Token));

            // Create the item groups
            var digits = items.Count >= 10 ? 2 : 1;
            _itemGroups =
            [
                .. items.Select(
                    (mailItem, i) => EncapsulateItemGroup(template, mailItem, i, digits, _tlpStates)
                ),
            ];

            // Initialize graphics
            foreach (var group in _itemGroups)
            {
                await group.ItemController.InitializeGraphicsAsync();
            }

            while (helpers.Count > 0)
            {
                var helperTask = await Task.WhenAny(helpers);
                var helper = await helperTask;
                helpers.Remove(helperTask);
                var grp = _itemGroups.FirstOrDefault(x => x.MailItem.EntryID == helper.EntryId);
                grp.ItemController.PopulateControls(helper, grp.ItemController.ItemNumber);
            }

            // Wait until Background Loading Tasks finish and then clear the collection

            await DrainBackgroundLoadingTasksAsync();

            // Load the Item Viewers, Item Controllers, and Initialize
            WireUpAsyncKeyboardHandler();

            // Restore state of window
            TlpLayout = tlpLayoutState;
            if (_formViewer.InvokeRequired)
            {
                _formViewer.Invoke(() => _formViewer.ResumeLayout());
            }
            else
            {
                _formViewer.ResumeLayout();
            }

            //// Load the conversations and folders
            //var conversationTasks = _itemGroups.Select(grp => grp.ItemController.LoadConversationResolverAsync(TokenSource, Token, false)).ToList();
        }

        /// <summary>
        /// High-confidence (Issue #171) carrier-list overload. Builds UI item controllers for the
        /// pre-filtered survivors in <paramref name="preScored"/>, mirroring the standard
        /// <see cref="LoadControlsAndHandlers_01Async(IList{MailItem}, RowStyle, RowStyle)"/> path but
        /// threading each survivor's predetermined folder into its <see cref="QfcItemGroup"/> and item
        /// controller so the folder is preselected instead of selected by index.
        /// </summary>
        public async Task LoadControlsAndHandlers_01Async(
            IList<QfcPreScoredItem> preScored,
            RowStyle template,
            RowStyle templateExpanded
        )
        {
            var items = preScored.Select(x => x.MailItem).ToList();
            ValidateParams(items, template, templateExpanded);

            // Start loading mail item helpers
            var helpers = items.Select(GetPartiallyInitializedHelperAsync).ToList();

            // Freeze the form while loading controls
            _formViewer.SuspendLayout();
            var tlpLayoutState = SafeSetTlpLayout(false);

            // Save the QfcItem template styles
            _template = template;
            _templateExpanded = templateExpanded;

            // Hook the move monitor to the mail items
            BackgroundLoadingTasks.Add(
                Task.Run(() =>
                    items.ForEach(mailItem =>
                        _moveMonitor.HookItem(mailItem, (x) => RemovedItemMonitor(x.EntryID))
                    )
                )
            );

            // Create empty keyboard handler actions
            BackgroundLoadingTasks.Add(Task.Run(CreateEmptyKbdHandlerCharActions, Token));

            // Create the item groups, carrying each survivor's predetermined folder
            var digits = preScored.Count >= 10 ? 2 : 1;
            _itemGroups =
            [
                .. preScored.Select(
                    (scored, i) =>
                        EncapsulateItemGroup(
                            template,
                            scored.MailItem,
                            i,
                            digits,
                            _tlpStates,
                            scored.PredeterminedFolder
                        )
                ),
            ];

            // Initialize graphics
            foreach (var group in _itemGroups)
            {
                await group.ItemController.InitializeGraphicsAsync();
            }

            while (helpers.Count > 0)
            {
                var helperTask = await Task.WhenAny(helpers);
                var helper = await helperTask;
                helpers.Remove(helperTask);
                var grp = _itemGroups.FirstOrDefault(x => x.MailItem.EntryID == helper.EntryId);
                grp.ItemController.PopulateControls(helper, grp.ItemController.ItemNumber);
            }

            // Wait until Background Loading Tasks finish and then clear the collection
            await DrainBackgroundLoadingTasksAsync();

            WireUpAsyncKeyboardHandler();

            // Restore state of window
            TlpLayout = tlpLayoutState;
            if (_formViewer.InvokeRequired)
            {
                _formViewer.Invoke(() => _formViewer.ResumeLayout());
            }
            else
            {
                _formViewer.ResumeLayout();
            }
        }

        //public async Task LoadSecondaryAsync()
        //{
        //    // Ensure the token is not canceled before starting
        //    Token.ThrowIfCancellationRequested();

        //    // Load the conversations and folders asynchronously for each item group
        //    await _itemGroups.ToAsyncEnumerable()
        //        .ForEachAwaitWithCancellationAsync(
        //            async (grp, token) => await Task.Run(
        //                async () => await grp
        //                    .ItemController
        //                    .LoadConversationResolverAsync(TokenSource, token, false)),
        //            Token);

        //}

        public async Task LoadSecondaryAsync()
        {
            // Ensure the token is not canceled before starting
            Token.ThrowIfCancellationRequested();

            // Load the conversations and folders asynchronously for each item group
            var convTasks = _itemGroups
                .Select(grp =>
                    Task.Run(
                        async () =>
                            await grp.ItemController.LoadConversationResolverAsync(
                                TokenSource,
                                Token,
                                false
                            ),
                        Token
                    )
                )
                .ToList();

            var folderTasks = _itemGroups
                .Select(grp =>
                    Task.Run(
                        async () => await grp.ItemController.LoadFolderHandlerAsync(Token),
                        Token
                    )
                )
                .ToList();

            var combinedTasks = folderTasks.Concat(convTasks).ToList();

            while (combinedTasks.Count > 0)
            {
                var task = await Task.WhenAny(combinedTasks);
                combinedTasks.Remove(task);
                if (convTasks.Contains(task))
                {
                    // Handle conversation task completion
                    var idx = convTasks.IndexOf(task);
                    var grp = _itemGroups[idx];
                    grp.ItemController.RenderConversationCount();
                }
                else if (folderTasks.Contains(task))
                {
                    // Handle folder task completion
                    var idx = folderTasks.IndexOf(task);
                    var grp = _itemGroups[idx];
                    grp.ItemController.AssignFolderComboBox();
                }
                else
                {
                    throw new InvalidOperationException($"Task {task} is not recognized.");
                }
            }
        }

        public void CreateEmptyKbdHandlerCharActions()
        {
            _kbdHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
            _kbdHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
        }

        internal QfcItemGroup EncapsulateItemGroup(
            RowStyle template,
            MailItem mailItem,
            int i,
            int digits,
            TlpCellStates tlpStates,
            string predeterminedFolder = null
        )
        {
            var grp = new QfcItemGroup(mailItem) { PredeterminedFolder = predeterminedFolder };
            var itemViewer = ItemViewerQueue.Dequeue(_homeController.Token);
            LoadItemToTlp(itemViewer, i, template, true, 0);
            grp.ItemViewer = itemViewer;
            grp.ItemController = new QfcItemController(
                _globals,
                _homeController,
                this,
                grp.ItemViewer,
                i + 1,
                digits,
                grp.MailItem,
                tlpStates,
                predeterminedFolder
            );
            grp.ItemController.Token = Token;
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

        public void LoadSequential_5()
        {
            int i = 0;
            foreach (var grp in _itemGroups)
            {
                grp.ItemController = new QfcItemController(
                    appGlobals: _globals,
                    homeController: _homeController,
                    parent: this,
                    itemViewer: grp.ItemViewer,
                    viewerPosition: ++i,
                    itemNumberDigits: _itemGroups.Count >= 10 ? 2 : 1,
                    grp.MailItem,
                    _tlpStates
                );
                grp.ItemController.Initialize(false);
                grp.ItemController.PopulateConversation();
                grp.ItemController.PopulateFolderComboBox();
                if (_darkMode)
                {
                    grp.ItemController.SetThemeDark(async: false);
                }
                else
                {
                    grp.ItemController.SetThemeLight(async: false);
                }
            }
        }

        internal void ActivateQueuedTlp(TableLayoutPanel tlp)
        {
            _formViewer.SwapItemTableLayout(tlp);
            _itemTlp = _formViewer.L1v0L2L3v_TableLayout;
        }

        internal void CacheItemGroupsForMove()
        {
            // Snapshot in list order (issue #469 defect 3). ToList copies, so later mutation of
            // _itemGroups does not disturb the cached move order.
            _itemGroupsToMove = _itemGroups.ToList();
        }

        internal void ActivateQueuedItemGroups(List<QfcItemGroup> itemGroups)
        {
            _itemGroups = itemGroups;
        }

        internal void SwapItemGroups(List<QfcItemGroup> itemGroups)
        {
            UnregisterNavigation();

            CacheItemGroupsForMove();
            ActivateQueuedItemGroups(itemGroups);

            RegisterNavigation();
        }

        public void CacheMoveObjects()
        {
            _itemTlpToMove = _formViewer.L1v0L2L3v_TableLayout;
            CacheItemGroupsForMove();
        }

        public void LoadItemToTlp(
            ItemViewer itemViewer,
            int indexNumber,
            RowStyle template,
            bool blGroupConversation = true,
            int columnNumber = 0
        )
        {
            if (itemViewer.InvokeRequired)
            {
                itemViewer.Invoke(() =>
                    LoadItemToTlp(
                        itemViewer,
                        indexNumber,
                        template,
                        blGroupConversation,
                        columnNumber
                    )
                );
                return;
            }

            itemViewer.Parent = _itemTlp;
            if (columnNumber == 0)
            {
                _itemTlp.SetCellPosition(
                    itemViewer,
                    new TableLayoutPanelCellPosition(columnNumber, indexNumber)
                );
                _itemTlp.SetColumnSpan(itemViewer, 2);
            }
            else
            {
                _itemTlp.SetCellPosition(
                    itemViewer,
                    new TableLayoutPanelCellPosition(1, indexNumber)
                );
                _itemTlp.SetColumnSpan(itemViewer, 1);
            }

            itemViewer.AutoSize = true;
            itemViewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            itemViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            itemViewer.Dock = DockStyle.Fill;
            return;
        }

        public ItemViewer LoadItemViewer_03(
            int indexNumber,
            RowStyle template,
            bool blGroupConversation = true,
            int columnNumber = 0
        )
        {
            var itemViewer = ItemViewerQueue.Dequeue(_homeController.Token);

            LoadItemToTlp(itemViewer, indexNumber, template, blGroupConversation, columnNumber);
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

                _moveMonitor.UnhookAll();

                TlpLayout = tlpState;
            }
        }

        public void CleanupBackground()
        {
            if (_itemGroupsToMove is not null)
            {
                foreach (var group in _itemGroupsToMove)
                {
                    group.ItemController?.Cleanup();
                }

                // The cached snapshot is read-only, so it is released by resetting the field
                // rather than by mutating the collection in place. An empty list rather than null
                // preserves the post-Clear() semantics the previous code had: EmailsToMove and
                // GetMoveDiagnostics continue to observe a non-null, zero-length collection.
                _itemGroupsToMove = Array.Empty<QfcItemGroup>();
            }
            if (_itemTlpToMove is not null)
                _itemTlpToMove.Dispose();
        }

        public async Task RemoveControlsAsync()
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

        internal void RemovedItemMonitor(string entryID)
        {
            UnregisterNavigation();
            RemoveSpecificControlGroup(entryID);
            RegisterNavigation();
        }

        internal void RemoveSpecificControlGroup(string entryID)
        {
            var group = _itemGroups.Where(x => x.MailItem.EntryID == entryID).FirstOrDefault();
            if (group is not null)
                RemoveSpecificControlGroup(group.ItemController.ItemNumber);
        }

        /// <summary>
        /// Seam used by <see cref="RemoveBelowThresholdAsync(double)"/> to remove a single group
        /// by EntryID. Defaults to the existing UI-thread removal path
        /// (<see cref="RemoveSpecificControlGroup(string)"/>), which unhooks the move monitor and
        /// renumbers remaining groups. Tests inject a recording delegate so the below-threshold
        /// selection logic can be verified without WinForms/COM state.
        /// </summary>
        private Func<string, Task> _removeGroupByEntryId;

        private Func<string, Task> RemoveGroupByEntryId =>
            _removeGroupByEntryId ??= entryID =>
            {
                RemoveSpecificControlGroup(entryID);
                return Task.CompletedTask;
            };

        /// <inheritdoc/>
        public async Task RemoveBelowThresholdAsync(double threshold)
        {
            if (_itemGroups is null)
            {
                return;
            }

            long cutoff = (long)Math.Round(threshold * 1000, 0);

            // Capture EntryIDs of below-threshold groups before removing any, so renumbering and
            // list mutation during removal cannot cause index drift mid-iteration.
            var entryIdsToRemove = _itemGroups
                .Where(group => group.ItemController.TopFolderScore < cutoff)
                .Select(group => group.MailItem.EntryID)
                .ToList();

            foreach (var entryID in entryIdsToRemove)
            {
                await RemoveGroupByEntryId(entryID);
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
            if (activeUI)
            {
                ToggleOffActiveItem(false);
            }

            UpdateSelectionNumberForRemoval(selection);

            var tlpState = TlpLayout;
            TlpLayout = false;

            // Remove the controls from the form
            TableLayoutHelper.RemoveSpecificRow(_itemTlp, selection - 1);

            // Unhook the email from the move monitor
            _moveMonitor.UnhookItem(_itemGroups[selection - 1].MailItem);

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
                    if (expanded)
                    {
                        _itemGroups[ActiveIndex].ItemController.ToggleExpansion();
                    }
                }
            }
            else if (_itemGroups.Count == 0 && _kbdHandler.KbdActive)
            {
                _kbdHandler.ToggleKeyboardDialog();
            }

            TlpLayout = tlpState;
            ResetPanelHeight();
            if (_itemGroups.Count == 0)
            {
                _parent.ActionOkAsync();
            }
        }

        private static int removespecificcontrolgroupcounter = 0;

        public async Task RemoveSpecificControlGroupAsync(int selection)
        {
            Interlocked.Increment(ref removespecificcontrolgroupcounter);
            try
            {
                UnregisterNavigation();

                // If the group is active, turn off the active item and select a new item
                bool activeUI = _itemGroups[selection - 1].ItemController.IsActiveUI;
                bool expanded = _itemGroups[selection - 1].ItemController.IsExpanded;
                if (activeUI)
                {
                    await ToggleOffActiveItemAsync(false);
                }

                UpdateSelectionNumberForRemoval(selection);

                bool tlpState = TlpLayout;

                //Removed dispatcher call because synchronization context should be set
                //await UiThread.Dispatcher.InvokeAsync(() =>
                //{
                tlpState = TlpLayout;
                TlpLayout = false;

                // Remove the controls from the form
                TableLayoutHelper.RemoveSpecificRow(_itemTlp, selection - 1);
                //});

                // Unhook the email from the move monitor
                _moveMonitor.UnhookItem(_itemGroups[selection - 1].MailItem);

                // Remove the group from the list of groups
                _itemGroups.RemoveAt(selection - 1);

                if (_itemGroups.Count > 0)
                {
                    // Renumber the remaining groups
                    await UiThread.Dispatcher.InvokeAsync(() =>
                    {
                        var digits = Digits;
                        if (_digitRefreshNeeded)
                        {
                            SetVisualDigits(digits);
                        }
                        RenumberGroups();
                    });

                    // Restore UI to previous state with newly selected item
                    if (activeUI)
                    {
                        await _itemGroups[ActiveIndex]
                            .ItemController.ToggleFocusAsync(Enums.ToggleState.On);
                        if (expanded)
                        {
                            await _itemGroups[ActiveIndex].ItemController.ToggleExpansionAsync();
                        }
                    }
                }
                else if (_itemGroups.Count == 0 && _kbdHandler.KbdActive)
                {
                    await _kbdHandler.ToggleKeyboardDialogAsync();
                }

                // Guards against double-registration: when the zero-item branch skips to the next page,
                // SkipGroupAsync -> LoadControlsAndHandlers_01 -> SwapItemGroups already registers the
                // incoming page's navigation keys. Registering again below would re-add the same keys and
                // throw ArgumentException from KbdActions.Add (Issue #232).
                bool swapAlreadyRegistered = false;
                await UiThread.Dispatcher.InvokeAsync(async () =>
                {
                    TlpLayout = tlpState;
                    ResetPanelHeight();
                    if (_itemGroups.Count == 0)
                    {
                        await _parent.SkipGroupAsync();
                        swapAlreadyRegistered = true;
                        //_parent.ActionOkAsync();
                    }
                });
                if (removespecificcontrolgroupcounter > 1)
                {
                    logger.Error(
                        "RemoveSpecificControlGroupAsync: Counter is greater than 1. Race Condition Exists"
                    );
                }
                if (!swapAlreadyRegistered)
                {
                    RegisterNavigation();
                }
            }
            finally
            {
                // Issue #286: the decrement must also run when the body throws. Before this
                // change it was the method's last statement, so any exception left the
                // process-wide counter permanently one higher and eventually tripped the
                // race-condition error branch above on a later, legitimate call.
                Interlocked.Decrement(ref removespecificcontrolgroupcounter);
            }
        }

        #endregion

        #region Event Wiring

        public void WireUpAsyncKeyboardHandler()
        {
            RegisterNavigation();
            RegisterAsyncKeyActions();
            RegisterAlwaysOnAsyncKeyActions();
        }

        internal void RegisterAsyncKeyActions()
        {
            _kbdHandler.KeyActionsAsync = new KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>(
                new List<KaKeyAsync>
                {
                    new KaKeyAsync("Collection", Keys.Up, (k) => SelectPreviousItemAsync()),
                    new KaKeyAsync("Collection", Keys.Down, (k) => SelectNextItemAsync()),
                }
            );
        }

        internal void RegisterAlwaysOnAsyncKeyActions()
        {
            _kbdHandler.AlwaysOnKeyActionsAsync = new KbdActions<
                Keys,
                KaKeyAsync,
                Func<Keys, Task>
            >(
                new List<KaKeyAsync>
                {
                    new KaKeyAsync("Collection", Keys.Return, (k) => CustomReturnKeyHandler()),
                }
            );
        }

        internal async Task CustomReturnKeyHandler()
        {
            var anyOpen = AnyOpenDropDowns(true, Token);
            if (!anyOpen)
            {
                await _parent.ActionOkAsync();
            }
        }

        // #351: the WebView2 breadcrumb replaced the folder ComboBox and has no dropped-down list
        // state, so no viewer can hold an open dropdown; the Return-key gate that used to close
        // open dropdowns before acting is now always clear.
        internal bool AnyOpenDropDowns(bool close, CancellationToken token)
        {
            return false;
        }

        public void RegisterNavigation()
        {
            var digits = Digits;
            if (_digitRefreshNeeded)
            {
                SetVisualDigits(digits);
            }
            for (int i = 0; i < _itemGroups.Count; i++)
            {
                RegisterNavigationAsyncAction(i, digits);
            }
        }

        public void UnregisterNavigation()
        {
            // Issue #644: replay the recorded registration set verbatim and drain it. A count-bound
            // loop orphaned every key past the live count when a group was removed unbracketed.
            foreach (var (sourceId, key) in RegisteredNavigationKeys)
            {
                _kbdHandler.StringActionsAsync.Remove(sourceId, key);
            }
            RegisteredNavigationKeys.Clear();
        }

        internal void RegisterNavigationAsyncAction(int itemIndex, int digits)
        {
            var action = GenerateStringKbdAction(itemIndex, digits);
            _kbdHandler.StringActionsAsync.Add(action);

            // Issue #644: record strictly after a successful Add, reading the key back off the
            // constructed instance, so a duplicate-key ArgumentException leaves the ledger clean.
            RegisteredNavigationKeys.Add((action.SourceId, action.Key));
        }

        internal KaStringAsync GenerateStringKbdAction(int i, int digits)
        {
            var grp = _itemGroups[i];
            string key = "";
            if (digits == 1)
            {
                key = (i + 1).ToString();
            }
            else if (digits == 2)
            {
                key = (i + 1).ToString("00");
            }

            var stringAsyncAction = new KaStringAsync(
                "Collection",
                key,
                (s) => ChangeByIndexAsync(int.Parse(s) - 1),
                //(s) => grp.ItemViewer.LblItemNumber.Text = s,
                null,
                null
            );
            return stringAsyncAction;
        }

        #endregion Event Wiring

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
                if (blExpanded)
                {
                    itemController.ToggleExpansion();
                }
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
                if (blExpanded)
                {
                    itemController.ToggleExpansion();
                }
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
                await UiThread.Dispatcher.InvokeAsync(() =>
                {
                    tlpState = TlpLayout;
                    TlpLayout = false;
                });

                if (ActiveIndex != -1)
                    expanded = await ToggleOffActiveItemAsync(false);
                await ActivateBySelectionAsync(idx + 1, expanded);

                await UiThread.Dispatcher.InvokeAsync(() => TlpLayout = tlpState);
            }
        }

        public void SelectNextItem()
        {
            if (ActiveSelection < _itemGroups.Count)
            {
                var tlpState = SafeSetTlpLayout(false);

                ChangeByIndex(ActiveIndex + 1);

                TlpLayout = tlpState;
            }
        }

        public async Task SelectNextItemAsync()
        {
            await UiThread.Dispatcher.InvokeAsync(() => SelectNextItem());
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
            await UiThread.Dispatcher.InvokeAsync(() => SelectPreviousItem());
        }

        internal void ScrollIntoView(ItemViewer item)
        {
            // If top is not visible, scroll top into view
            if (_itemPanel.Top - _itemPanel.AutoScrollPosition.Y > item.Top)
            {
                _itemPanel.AutoScrollPosition = new System.Drawing.Point(
                    _itemPanel.AutoScrollPosition.X,
                    item.Top
                );
            }
            // Else if bottom is not visible, scroll bottom into view
            else if (item.Bottom > (_itemPanel.Bottom - _itemPanel.AutoScrollPosition.Y))
            {
                int yScroll = Math.Max(0, item.Bottom - _itemPanel.Height + _itemPanel.Top);
                _itemPanel.AutoScrollPosition = new System.Drawing.Point(
                    _itemPanel.AutoScrollPosition.X,
                    yScroll
                );
            }
            // Else do nothing
        }

        public void ToggleExpansionStyle(int itemIndex, Enums.ToggleState desiredState)
        {
            if (itemIndex < 0 || itemIndex >= _itemGroups.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(itemIndex),
                    $"{nameof(itemIndex)} value of {itemIndex} must be in the range of 0 to {_itemGroups.Count - 1}"
                );
            }

            if (!_itemGroups[itemIndex].ItemController.IsActiveUI)
            {
                var c = _itemGroups[itemIndex].ItemController;
                var msg =
                    $"Cannot expand item with index {itemIndex} because UI is not active.\n"
                    + $"Controller for message \"{c.ItemHelper.Subject} sent on {c.ItemHelper.SentDate.ToString("MM/dd/yyyy")} at {c.ItemHelper.SentDate.ToString("HH:mm")} "
                    + $"by {c.ItemHelper.SenderName} has a value of {c.IsActiveUI} for {nameof(c.IsActiveUI)}";
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
                _itemTlp.MinimumSize.Height + (int)Math.Round(heightChange, 0)
            );

            if (heightChange < 0)
            {
                _itemTlp.Invoke(
                    new System.Action(() => _itemTlp.Height += (int)Math.Round(heightChange, 0))
                );
            }

            if (desiredState == Enums.ToggleState.On)
                ScrollIntoView(_itemGroups[itemIndex].ItemViewer);
        }

        public async Task ToggleExpansionStyleAsync(int itemIndex, Enums.ToggleState desiredState)
        {
            Token.ThrowIfCancellationRequested();

            await UiThread.Dispatcher.InvokeAsync(() =>
                ToggleExpansionStyle(itemIndex, desiredState)
            );
        }

        public void ToggleOffNavigation(bool async)
        {
            if (ActiveIndex != -1)
            {
                ToggleOffActiveItem(false);
            }
            _itemGroups.ForEach(itemGroup =>
                itemGroup.ItemController.ToggleNavigation(
                    async: async,
                    desiredState: Enums.ToggleState.Off
                )
            );
            //_keyboardHandler.KbdActive = false;
        }

        public async Task ToggleOffNavigationAsync()
        {
            var tlpState = SafeSetTlpLayout(false);
            TlpLayout = false;

            if (ActiveIndex != -1)
            {
                await ToggleOffActiveItemAsync(false);
            }
            var tasks = _itemGroups
                .Select(itemGroup =>
                    itemGroup.ItemController.ToggleNavigationAsync(Enums.ToggleState.Off)
                )
                .ToList();
            await Task.WhenAll(tasks);

            TlpLayout = tlpState;
        }

        public void ToggleOnNavigation(bool async)
        {
            _itemGroups.ForEach(itemGroup =>
                itemGroup.ItemController.ToggleNavigation(
                    async: async,
                    desiredState: Enums.ToggleState.On
                )
            );
            if (ActiveIndex != -1)
            {
                ActivateByIndex(ActiveIndex, false);
            }
        }

        public async Task ToggleOnNavigationAsync()
        {
            var tlpState = SafeSetTlpLayout(false);

            var tasks = _itemGroups
                .Select(itemGroup =>
                    itemGroup.ItemController.ToggleNavigationAsync(Enums.ToggleState.On)
                )
                .ToList();
            await Task.WhenAll(tasks);

            if (ActiveIndex != -1)
            {
                await ActivateByIndexAsync(ActiveIndex, false);
            }

            TlpLayout = tlpState;
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
                IQfcItemController itemController = _itemGroups[ActiveIndex].ItemController;

                //if (itemController.IsExpanded)
                //{
                //    await itemController.ToggleExpansionAsync();
                //    blExpanded = true;
                //}
                await itemController.ToggleFocusAsync(Enums.ToggleState.Off);
            }
            return blExpanded;
        }

        #endregion

        #region UI Conversation Expansion

        /// <summary>
        /// Changes the conversation checkbox state of the item viewer at the
        /// specified index without raising events
        /// </summary>
        /// <param name="indexOriginal">Index of the group to change</param>
        /// <param name="desiredState">Checked is true or false</param>
        public void ChangeConversationSilently(int indexOriginal, bool desiredState)
        {
            // Issue #470 defect 1: guard this overload's own subscript too. It is public and
            // reachable from callers other than ToggleGroupConv, so relying on every caller to
            // filter the -1 sentinel would leave the same defect one call site away.
            if (_itemGroups is null || indexOriginal < 0 || indexOriginal >= _itemGroups.Count)
            {
                logger.Warn(
                    $"Cannot change the conversation state at index {indexOriginal}: the index is "
                        + "outside the item group collection. Skipping."
                );
                return;
            }

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
            grp.ItemViewer.ConversationMenuItem.Checked = desiredState;
            grp.ItemController.SuppressEvents = suppressionState;
        }

        public void ToggleGroupConv(string originalId)
        {
            int childCount = _itemGroups
                .Where(itemGroup => itemGroup.ItemController.ConvOriginID == originalId)
                .Count();
            int indexOriginal = _itemGroups.FindIndex(itemGroup =>
                itemGroup.ItemController.Mail.EntryID == originalId
            );

            // if original has been removed, find the first child and set it as the original
            if (indexOriginal == -1)
            {
                indexOriginal = PromoteFirstChild(originalId, ref childCount);
            }

            // Issue #470 defect 1: PromoteFirstChild returns the -1 sentinel when the conversation
            // has neither its original nor a promotable child. Nothing remains to check or
            // collapse, so return instead of carrying -1 into the subscripts below.
            if (indexOriginal == -1)
            {
                return;
            }

            // ensure the original is checked
            ChangeConversationSilently(indexOriginal, true);

            // if there are children, collapse them into the original
            if (childCount > 0)
            {
                bool reactivate = false;
                if (ActiveIndex != -1 && (ActiveIndex != indexOriginal))
                {
                    reactivate = true;
                    ToggleOffActiveItem(false);
                }
                ToggleGroupConv(childCount, indexOriginal);
                if (reactivate)
                {
                    ActivateByIndex(indexOriginal, false);
                }
            }
        }

        public void ToggleGroupConv(int childCount, int indexOriginal)
        {
            var tlpState = TlpLayout;
            TlpLayout = false;

            UnregisterNavigation();

            int removalIndex = indexOriginal + 1;

            var qfOriginal = _itemGroups[indexOriginal].ItemController;
            //TableLayoutHelper.RemoveSpecificRow(_itemTlp, removalIndex, childCount);
            EliminateSpaceForItems(removalIndex, childCount);

            for (int i = 0; i < childCount; i++)
            {
                _itemGroups[removalIndex].ItemController.Cleanup();
                _itemGroups.RemoveAt(removalIndex);
            }

            RenumberGroups();

            //_itemTlp.MinimumSize = new System.Drawing.Size(
            //    _itemTlp.MinimumSize.Width,
            //    _itemTlp.MinimumSize.Height -
            //    (int)Math.Round(_template.Height * childCount, 0));

            //_itemTlp.Size = _itemTlp.MinimumSize;

            RegisterNavigation();
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
        public void ToggleUnGroupConv(
            ConversationResolver resolver,
            string entryID,
            int conversationCount,
            object folderList
        )
        {
            var tlpState = SafeSetTlpLayout(false);

            UnregisterNavigation();

            int baseEmailIndex = _itemGroups.FindIndex(itemGroup =>
                itemGroup.ItemController.Mail.EntryID == entryID
            );

            // Issue #470 defect 2: FindIndex returns -1 when the base email is no longer in the
            // collection, and every downstream index derives from it. Expanding from -1 reserved
            // rows at index 0 and then subscripted _itemGroups[insertionIndex - 1] with -1.
            // Restore the navigation and layout state this method turned off, then return.
            if (baseEmailIndex == -1)
            {
                logger.Warn(
                    $"Cannot expand the conversation for entryID={entryID}: the base email is no "
                        + "longer in the item group collection. Skipping the expansion."
                );
                RegisterNavigation();
                TlpLayout = tlpState;
                return;
            }

            int insertionIndex = baseEmailIndex + 1;

            // Issue #470 defect 2: resolve the conversation members exactly once, here, and
            // derive the insertion count from the resolved list. The caller-supplied
            // conversationCount is only a reservation hint; when the two disagree the resolved
            // count wins and the disagreement is logged once. The loop is deliberately not
            // clamped, because clamping would silently drop conversation members.
            IReadOnlyList<MailItem> insertions = ResolveConversationInsertions(resolver, entryID);
            int insertCount = ReconcileInsertionCount(
                entryID,
                conversationCount,
                insertions.Count,
                resolver.Count.SameFolder,
                resolver.Count.Expanded,
                baseEmailIndex,
                message => logger.Warn(message)
            );

            if (insertCount > 0)
            {
                MakeSpaceForItems(insertionIndex, insertCount);

                InsertItemGroups(insertionIndex, insertCount);
                RenumberGroups(insertionIndex + insertCount);

                EnumerateConversationMembers(
                    entryID,
                    resolver,
                    insertionIndex,
                    insertions,
                    folderList
                );
                if (_digitRefreshNeeded)
                {
                    SetVisualDigits(Digits);
                }
            }

            RegisterNavigation();
            TlpLayout = tlpState;
        }

        internal void InitializeGroup(QfcItemGroup grp, int index, MailItem mailItem, bool child)
        {
            grp.ItemViewer = LoadItemViewer_03(index, _template, false, child ? 1 : 0);
            grp.MailItem = mailItem;
            grp.ItemController = new QfcItemController(
                appGlobals: _globals,
                homeController: _homeController,
                parent: this,
                itemViewer: grp.ItemViewer,
                viewerPosition: index + 1,
                itemNumberDigits: Digits,
                grp.MailItem,
                tlpStates: _tlpStates
            );
            grp.ItemController.IsChild = child;
        }

        /// <summary>
        /// Resolves the conversation members that must be expanded beneath the base email,
        /// newest first, excluding the base email itself.
        /// </summary>
        /// <param name="resolver">Resolver holding the conversation snapshot.</param>
        /// <param name="entryID">Entry identifier of the base email, which is excluded.</param>
        /// <returns>The members to insert, ordered by sent time descending.</returns>
        /// <remarks>
        /// Issue #470 defect 2. This is the member-resolution expression that previously lived
        /// inline in <see cref="EnumerateConversationMembers"/>, extracted unchanged. Extracting it
        /// lets <c>ToggleUnGroupConv</c> resolve the list once, before it reserves rows, so the
        /// count it reserves and the count it fills come from the same evaluation. The method is
        /// pure: it reads no field and touches no WinForms control.
        /// </remarks>
        internal static IReadOnlyList<MailItem> ResolveConversationInsertions(
            ConversationResolver resolver,
            string entryID
        )
        {
            return resolver
                .ConversationItems.SameFolder.Where(mailItem => mailItem.EntryID != entryID)
                .OrderByDescending(mailItem => mailItem.SentOn)
                .ToList();
        }

        /// <summary>
        /// Reconciles the caller-supplied conversation count against the resolved insertion count
        /// and returns the insertion count as the single source of truth.
        /// </summary>
        /// <param name="entryID">Entry identifier of the base email.</param>
        /// <param name="conversationCount">Caller-supplied count, used only as a reservation.</param>
        /// <param name="insertionsCount">Count of members actually resolved for insertion.</param>
        /// <param name="sameFolderCount">Resolver same-folder count, for diagnosis.</param>
        /// <param name="expandedCount">Resolver expanded count, for diagnosis.</param>
        /// <param name="baseEmailIndex">Index of the base email, for diagnosis.</param>
        /// <param name="warn">Warning sink, invoked once on disagreement.</param>
        /// <returns><paramref name="insertionsCount"/>, always.</returns>
        /// <remarks>
        /// Issue #470 defect 2. The reservation was derived from <paramref name="conversationCount"/>
        /// while the loop was driven by the independently resolved member list, and nothing
        /// compared them. A disagreement is recoverable and this method sits on the VSTO UI event
        /// path, so the resolution is log-and-proceed rather than throw, matching the precedent in
        /// <c>ConversationResolver.Loading</c>. The message carries all six values because the two
        /// counts alone do not identify which snapshot moved.
        /// </remarks>
        internal static int ReconcileInsertionCount(
            string entryID,
            int conversationCount,
            int insertionsCount,
            int sameFolderCount,
            int expandedCount,
            int baseEmailIndex,
            System.Action<string> warn
        )
        {
            if (insertionsCount != conversationCount - 1)
            {
                warn?.Invoke(
                    $"Conversation insertion count disagreement for entryID={entryID}: "
                        + $"conversationCount={conversationCount}, "
                        + $"insertionsCount={insertionsCount}, "
                        + $"sameFolderCount={sameFolderCount}, "
                        + $"expandedCount={expandedCount}, "
                        + $"baseEmailIndex={baseEmailIndex}. "
                        + "Proceeding with insertionsCount as the single source of truth."
                );
            }

            return insertionsCount;
        }

        /// <summary>
        /// Parallel function to expand each member of a conversation into individual ItemViewers/Controllers.
        /// Expanded members are inserted into the base collection and conversation count and folder suggestions
        /// are replicated from the base member. This enables distinct actions to be taken with each member
        /// </summary>
        /// <param name="entryID">
        /// Entry identifier of the base email. Since issue #470 defect 2 moved member filtering into
        /// <see cref="ResolveConversationInsertions"/>, this method no longer reads the parameter.
        /// It is retained because the scoped signature change for that defect replaces
        /// <c>conversationCount</c> with <paramref name="insertions"/> only; removing a second
        /// parameter is a separate change.
        /// </param>
        /// <param name="resolver">Resolver replicated onto each expanded member.</param>
        /// <param name="insertionIndex">Location of the Item Group collection where the base member is stored</param>
        /// <param name="insertions">
        /// The members to expand, already resolved and ordered by the caller. Passing them in is what
        /// guarantees the rows reserved and the rows filled come from one evaluation.
        /// </param>
        /// <param name="folderList">Folder suggestions for the first email</param>
        public void EnumerateConversationMembers(
            string entryID,
            ConversationResolver resolver,
            int insertionIndex,
            IReadOnlyList<MailItem> insertions,
            object folderList
        )
        {
            Enumerable
                .Range(0, insertions.Count)
                .ForEach(i =>
                {
                    // Initialize Group
                    var grp = _itemGroups[i + insertionIndex];
                    InitializeGroup(grp, i + insertionIndex, insertions[i], child: true);

                    // Initialize Item Controller
                    grp.ItemController.Initialize(false);
                    grp.ItemController.PopulateConversation(resolver);
                    grp.ItemController.PopulateFolderComboBox(folderList);
                    grp.ItemController.ConvOriginID = _itemGroups[insertionIndex - 1]
                        .MailItem
                        .EntryID;

                    // Set Current UI State and Theme
                    if (_kbdHandler.KbdActive)
                    {
                        grp.ItemController.ToggleNavigation(
                            async: true,
                            desiredState: Enums.ToggleState.On
                        );
                    }
                    if (_darkMode)
                    {
                        grp.ItemController.SetThemeDark(async: true);
                    }
                    else
                    {
                        grp.ItemController.SetThemeLight(async: true);
                    }
                    ChangeConversationSilently(grp, false);
                });
        }

        public void AddItemGroup(MailItem mailItem)
        {
            UnregisterNavigation();
            var tlpState = SafeSetTlpLayout(false);

            var index = _itemGroups.Count;
            MakeSpaceForItems(index, 1);
            InsertItemGroups(index, 1);
            RenumberGroups(index + 1);

            var grp = _itemGroups[index];
            InitializeGroup(grp, index, mailItem, child: false);
            if (_digitRefreshNeeded)
            {
                SetVisualDigits(Digits);
            }

            // Hook the email to the move monitor
            _moveMonitor.HookItem(mailItem, (x) => RemovedItemMonitor(x.EntryID));

            // Initialize Item Controller
            grp.ItemController.Initialize(false);
            grp.ItemController.PopulateConversation();
            grp.ItemController.PopulateFolderComboBox();

            // Set Current UI State and Theme
            if (_kbdHandler.KbdActive)
            {
                grp.ItemController.ToggleNavigation(
                    async: true,
                    desiredState: Enums.ToggleState.On
                );
            }
            if (_darkMode)
            {
                grp.ItemController.SetThemeDark(async: true);
            }
            else
            {
                grp.ItemController.SetThemeLight(async: true);
            }

            RegisterNavigation();
            TlpLayout = tlpState;
        }

        public int PromoteFirstChild(string originalId, ref int childCount)
        {
            int indexOriginal = _itemGroups.FindIndex(itemGroup =>
                itemGroup.ItemController.ConvOriginID == originalId
            );

            // Issue #470 defect 1: FindIndex returns -1 when no group carries this conversation
            // origin, and the next statement subscripted the list with it. Per D4 the contract is
            // a sentinel return rather than a throw, because this sits on the VSTO UI event path
            // and the state is recoverable. The child count is left alone: none was promoted.
            if (indexOriginal == -1)
            {
                logger.Warn(
                    $"No conversation child carries originalId={originalId}. Nothing to promote; "
                        + "leaving the caller's child count unchanged."
                );
                return -1;
            }

            var itemViewer = _itemGroups[indexOriginal].ItemViewer;
            _itemTlp.SetCellPosition(
                itemViewer,
                new TableLayoutPanelCellPosition(0, indexOriginal)
            );
            _itemTlp.SetColumnSpan(itemViewer, 2);
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
                var grp = new QfcItemGroup();
                _itemGroups.Insert(insertionIndex, grp);
            }
        }

        /// <summary>
        /// Awaits every task queued in <see cref="BackgroundLoadingTasks"/>, including any task
        /// added while the drain is already in flight, and leaves the collection empty.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bag is taken with a single <see cref="Interlocked.Exchange{T}(ref T, T)"/> that
        /// installs a fresh bag in the same instant it hands the old one back, so no producer can
        /// add to a bag that has already been snapshotted and then discarded. The loop repeats
        /// while the bag it swapped out was non-empty, which is what makes a late arrival — added
        /// after the previous swap but before this one — still get awaited.
        /// </para>
        /// <para>
        /// The replacement bag is built with an explicit constructor call rather than a
        /// target-typed collection expression because the generic <c>Interlocked.Exchange</c>
        /// overload infers its type argument from the arguments and cannot bind an expression that
        /// has no type of its own.
        /// </para>
        /// <para>
        /// Extracted from two byte-identical statement pairs on the two load paths, so the drain
        /// has a single definition. Both former sites call this member.
        /// </para>
        /// </remarks>
        internal async Task DrainBackgroundLoadingTasksAsync()
        {
            ConcurrentBag<Task> drained;
            do
            {
                drained = Interlocked.Exchange(
                    ref BackgroundLoadingTasks,
                    new ConcurrentBag<Task>()
                );
                await Task.WhenAll(drained);
            } while (!drained.IsEmpty);
        }

        /// <summary>
        /// Returns <paramref name="current"/> with its height reduced by
        /// <paramref name="removalCount"/> template rows.
        /// </summary>
        /// <param name="current">The size to adjust. It is not mutated.</param>
        /// <param name="templateHeight">Height of a single template row, in pixels.</param>
        /// <param name="removalCount">
        /// Number of template rows to remove. A <em>negative</em> row count grows the size instead
        /// of shrinking it, which is how the insertion path expresses "make room for N rows".
        /// </param>
        /// <remarks>
        /// This is pure arithmetic on a value type: it reads no field and touches no WinForms
        /// control, so the rounding contract shared by the removal and insertion paths can be
        /// asserted directly by a unit test that needs no message pump.
        /// </remarks>
        internal static System.Drawing.Size ShrinkByRows(
            System.Drawing.Size current,
            float templateHeight,
            int removalCount
        )
        {
            return new System.Drawing.Size(
                current.Width,
                current.Height - (int)Math.Round(templateHeight * removalCount, 0)
            );
        }

        public void EliminateSpaceForItems(int removalInex, int removalCount)
        {
            TableLayoutHelper.RemoveSpecificRow(_itemTlp, removalInex, removalCount);

            var rowsToShrinkBy = removalCount;
            _itemTlp.MinimumSize = ShrinkByRows(
                _itemTlp.MinimumSize,
                _template.Height,
                rowsToShrinkBy
            );

            _itemTlp.Size = ShrinkByRows(_itemTlp.Size, _template.Height, rowsToShrinkBy);
        }

        public void MakeSpaceForItems(int insertionIndex, int insertCount)
        {
            // A negative row count grows the panel, so an insertion of N rows is expressed as a
            // shrink by -N rows.
            _itemTlp.MinimumSize = ShrinkByRows(
                _itemTlp.MinimumSize,
                _template.Height,
                -insertCount
            );

            TableLayoutHelper.InsertSpecificRow(
                panel: _itemTlp,
                rowIndex: insertionIndex,
                templateStyle: _template,
                insertCount: insertCount
            );
        }

        public void UpdateSelectionNumberForRemoval(int selection)
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

        public async Task ResetPanelHeightAsync()
        {
            await _formViewer.UiSyncContext;
            var ht = (int)
                Math.Round(_itemTlp.RowStyles.Cast<RowStyle>().Sum(rowStyle => rowStyle.Height), 0);

            _itemTlp.MinimumSize = new System.Drawing.Size(_itemTlp.MinimumSize.Width, ht);

            _itemTlp.Height = ht;
            _itemTlp.Parent.Height = ht;
        }

        public void ResetPanelHeight()
        {
            var ht = 0;
            //_itemTlp.Invoke(new System.Action(() =>
            //{
            for (int i = 0; i < _itemTlp.RowStyles.Count - 1; i++)
            {
                ht += (int)Math.Round(_itemTlp.RowStyles[i].Height, 0);
            }

            _itemTlp.MinimumSize = new System.Drawing.Size(_itemTlp.MinimumSize.Width, ht);
            _itemTlp.Height = ht;
            //}));
            var panel = _itemTlp.Parent;
            panel?.Invoke(new System.Action(() => panel.Height = ht));
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
            // Defensive guard (Issue #251): a cleaned-up controller nulls _formViewer in
            // Cleanup()/CleanupAsync(). If a stale subscription still fires (e.g. unsubscribe raced
            // with an in-flight event), bail out instead of dereferencing cleaned-up state.
            if (_formViewer is null)
            {
                return;
            }

            // Prefer the dark-mode state carried by the event's sender (the IOlObjects that raised
            // PropertyChanged) over _globals.Ol, so the handler does not depend on _globals staying
            // alive for the lifetime of the subscription.
            bool darkMode;
            if (sender is IOlObjects senderOl)
            {
                darkMode = senderOl.DarkMode;
            }
            else if (_globals is not null)
            {
                darkMode = _globals.Ol.DarkMode;
            }
            else
            {
                return;
            }

            if (darkMode)
            {
                SetDarkMode(async: true);
            }
            else
            {
                SetLightMode(async: true);
            }
            _darkMode = darkMode;
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
            if (_globals?.Ol is not null)
            {
                _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged;
            }
            _globals = null;
            _parent = null;
            _itemTlp = null;
            _itemGroups = null;
        }

        public void Cleanup()
        {
            RemoveControls();
            _formViewer = null;
            if (_globals?.Ol is not null)
            {
                _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged;
            }
            _globals = null;
            _parent = null;
            _itemTlp = null;
            _itemGroups = null;
        }

        /// <summary>
        /// Moves every cached item group's message to its assigned destination folder.
        /// </summary>
        /// <param name="stackMovedItems">
        /// The undo stack. This parameter does not carry the undo records: the stack is populated
        /// by the email filer's push-to-undo-stack path, which pushes onto
        /// <c>Globals.AF.MovedMails</c>. That is the same instance the caller passes here, because
        /// the caller reads it from the same globals object. Passing a different instance would not
        /// redirect the undo records, and passing <c>null</c> does not suppress them. The parameter
        /// is retained only for source compatibility with existing callers; removing it is a
        /// follow-up candidate, not part of this change.
        /// </param>
        public async Task MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)
        {
            //TraceUtility.LogMethodCall(stackMovedItems);

            // The parameter is deliberately discarded rather than left untouched. The undo records
            // reach the stack through the email filer, not through this argument, and the discard
            // states that at the point of use so the parameter cannot be read as an oversight.
            _ = stackMovedItems;

            var count = _itemGroupsToMove?.Count() ?? 0;
            if (count <= 0)
            {
                return;
            }
            // ForEachAwaitAsync (System.Linq.Async) is obsolete (CS0618) per the framework's
            // migration guidance ("Use the language support for async foreach instead"), but
            // replacing it with `await foreach` here is a control-flow change to a production
            // async method, not an annotation-only edit. Suppressing narrowly preserves the exact
            // pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
            await Enumerable
                .Range(0, count)
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(TryMoveEmailByGroupIndexAsync);
#pragma warning restore CS0618

            //await _itemGroupsToMove.ToAsyncEnumerable().ForEachAwaitAsync(
            //    async grp => await grp.ItemController.MoveMailAsync());
        }

        private async Task TryMoveEmailByGroupIndexAsync(int i)
        {
            var group = TryGetItemGroupByIndex(i);
            if (group is null)
            {
                // Issue #473 defect 2: TryGetItemGroupByIndex reports a missing group as null.
                // Guarding at this boundary keeps that null from reaching the dereference inside
                // TryMoveEmailByGroupAsync, where it previously produced two log entries for one
                // root cause. One entry is emitted here instead, and the batch continues.
                logger.Error($"No cached item group at index {i}. Continuing execution.");
                return;
            }

            await TryMoveEmailByGroupAsync(group);
        }

        /// <summary>
        /// Moves the message owned by <paramref name="group"/>, logging and continuing when the
        /// move fails, and letting a cancellation reach the caller.
        /// </summary>
        private static async Task TryMoveEmailByGroupAsync(QfcItemGroup group)
        {
            try
            {
                await group.ItemController.MoveMailAsync();
            }
            catch (OperationCanceledException)
            {
                // Issue #473 defect 2: a cancellation is a control-flow signal, not a move
                // failure. It must reach the caller so an aborted batch actually stops, and it
                // must not be recorded as an error. This clause has to precede the broad clause
                // below, because OperationCanceledException derives from System.Exception and the
                // first matching clause wins.
                throw;
            }
            catch (System.Exception e)
            {
                // Issue #473 defect 2: log once, then return. The previous body went on to read
                // group.MailItem.Subject from inside this catch, which dereferenced the same
                // failed group a second time and raised a second exception into a nested catch, so
                // a single root cause emitted two misleading log entries. Log-and-proceed is
                // retained: the caller continues with the remaining cached groups.
                logger.Error($"Error moving message. Continuing execution.\n{e.Message}", e);
                return;
            }
        }

        /// <summary>
        /// Resolves the cached move group at <paramref name="index"/>, or <see langword="null"/>
        /// when the cache is absent or the index falls outside it.
        /// </summary>
        /// <remarks>
        /// Issue #469 defect 3: an explicit null-and-bounds check replaces a broad
        /// <c>catch (System.Exception)</c>. The old form also swallowed genuine faults raised by a
        /// collaborator and reported them as a missing group, which is indistinguishable from a
        /// legitimately out-of-range index.
        /// </remarks>
        private QfcItemGroup TryGetItemGroupByIndex(int index)
        {
            if (_itemGroupsToMove is null || index < 0 || index >= _itemGroupsToMove.Count)
            {
                return null;
            }

            return _itemGroupsToMove[index];
        }

        public string[] GetMoveDiagnostics(
            string durationText,
            string durationMinutesText,
            double duration,
            string dataLineBeg,
            DateTime endTime,
            ref AppointmentItem olAppointment
        )
        {
            //TraceUtility.LogMethodCall(durationText, durationMinutesText, Duration, dataLineBeg, OlEndTime, OlAppointment);

            int k;
            // Issue #469 defect 1: exactly one diagnostics line per cached move group. The array
            // was allocated as Count + 1 while the loop bound stayed Count, so the trailing
            // element was never assigned and QfcHomeController.Metrics wrote it out as a blank
            // diagnostics row for a message that does not exist.
            string[] strOutput = new string[_itemGroupsToMove.Count];
            var loopTo = _itemGroupsToMove.Count;
            for (k = 0; k < loopTo; k++)
            {
                var qf = TryGetItemGroupByIndex(k)?.ItemController;

                // Issue #469 defect 2: the null test must dominate every dereference of qf. It
                // previously sat below this ItemHelper read and below the interpolation of
                // qf.ItemHelper.Subject into the data line, so a group with no controller raised
                // NullReferenceException and the Unknown branch below was unreachable. The empty
                // subject column keeps the data line at its pre-existing column count.
                if (qf is null)
                {
                    strOutput[k] =
                        $"{dataLineBeg} ,QuickFiled,{durationText},{durationMinutesText},"
                        + "To Unknown,Sender Unknown,Email,Folder Unknown,Sent Date Unknown,Sent Time Unknown";
                    continue;
                }

                var helper = qf.ItemHelper;

                var minutes = Math.Floor(duration / 60d);
                var seconds = Math.Round(duration - (minutes * 60d), 0);
                var infoMailString =
                    $"|{endTime:G} | Duration: {minutes:N0} minutes {seconds:N1} | Action: "
                    + $" | Subject: {helper.Subject} | From: {helper.SenderName} | To: {helper.ToRecipientsName}";

                if (olAppointment is not null)
                {
                    if (string.IsNullOrEmpty(olAppointment.Body))
                    {
                        olAppointment.Body = infoMailString;
                        olAppointment.Save();
                    }
                    else
                    {
                        olAppointment.Body += $"{System.Environment.NewLine}{infoMailString}";
                        olAppointment.Save();
                    }
                }

                var dataLine =
                    $"{dataLineBeg} {xComma(qf.ItemHelper.Subject)},QuickFiled,{durationText},{durationMinutesText},";
                dataLine +=
                    $"{xComma(qf.ItemHelper.ToRecipientsName)},{xComma(qf.ItemHelper.SenderName)},Email,{xComma(qf.SelectedFolder)},{qf.ItemHelper.SentDate.ToString("MM/dd/yyyy")},{qf.ItemHelper.SentDate.ToString("HH:mm")}";

                strOutput[k] = dataLine;
            }

            return strOutput;
        }

        public static string xComma(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return "";
            }

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
