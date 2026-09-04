using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal class EfcFormController : IFilerFormController
    {
        #region Constructors

        public EfcFormController(
            IApplicationGlobals AppGlobals,
            EfcDataModel dataModel,
            EfcViewer formViewer,
            EfcHomeController homeController,
            System.Action ParentCleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            _token = token;
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _dataModel = dataModel;
            _initType = initType;
            _itemViewer = _formViewer.ItemViewer;
            _itemTlp = _formViewer.L0vh_TLP;
        }

        public EfcFormController(
            IApplicationGlobals globals,
            EfcViewer formViewer,
            EfcHomeController homeController,
            System.Action parentCleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            _token = token;
            _globals = globals;
            _parentCleanup = parentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _initType = initType;
            _itemViewer = _formViewer.ItemViewer;
            _itemController = new EfcItemController(
                globals,
                homeController,
                this,
                _itemViewer,
                token
            );
            _itemTlp = _formViewer.L0vh_TLP;
        }

        private EfcFormController() { }

        internal EfcFormController Initialize()
        {
            LoadUserSettings();
            CaptureConfigureItemViewer();
            ConfigureFind();
            ResolveControlGroups();
            _itemController = new EfcItemController(
                _globals,
                _homeController,
                this,
                _itemViewer,
                _dataModel,
                _token
            );
            SetupThemes();
            WireEventHandlers();
            _ = PopulateFolderCombobox();
            return this;
        }

        internal EfcFormController InitializeWithoutData()
        {
            LoadUserSettings();
            CaptureConfigureItemViewer();
            ConfigureFind();
            ResolveControlGroups();
            _itemController.InitializeWithoutData();
            SetupThemes();
            WireEventHandlers();
            return this;
        }

        internal EfcFormController InitializeDataFields(EfcDataModel dataModel)
        {
            _dataModel = dataModel;
            _itemController.InitializeDataFields(dataModel);
            _ = PopulateFolderCombobox();
            return this;
        }

        #endregion Constructors

        #region Private Properties

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>Fault-boundary sink; an injectable seam over the static logger above.</summary>
        internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } =
            DefaultBoundaryErrorSink;

        /// <summary>
        /// Issue #736 finding 4: the default boundary sink. It logs the fault exactly as the
        /// previous lambda did and then surfaces it through <see cref="UserFaultNotifier"/>, whose
        /// own default returns without blocking the calling thread. A static method group is used
        /// because an instance property initializer cannot reference <c>this</c>.
        /// </summary>
        private static void DefaultBoundaryErrorSink(string message, System.Exception exception)
        {
            logger.Error(message, exception);
            UserFaultNotifier?.Invoke(message);
        }

        /// <summary>
        /// Issue #726 finding 5: invokes <see cref="BoundaryErrorSink"/> defensively so a null or
        /// throwing sink delegate cannot silently reinstate the unobserved-fault behavior this
        /// boundary exists to prevent -- these call sites all sit in an <c>async void</c> handler's
        /// catch block, where an escaping exception would crash the process rather than merely fail
        /// to log.
        /// </summary>
        private void TryReportBoundaryFault(string message, System.Exception exception)
        {
            var sink = BoundaryErrorSink;
            if (sink is null)
            {
                logger.Error(message, exception);
                return;
            }

            try
            {
                sink(message, exception);
            }
            catch (System.Exception sinkException)
            {
                logger.Error($"{message} (and the error sink itself threw)", sinkException);
                logger.Error(message, exception);
            }
        }

        // Issue #736 finding 4. Per-async-flow storage rather than a plain shared static: a shared
        // static races under the ClassLevel parallelization configured in the CLI runsettings, which
        // is the same reason MyBox.DialogInvoker in UtilitiesCS.Dialogs is written this way.
        private static readonly AsyncLocal<System.Action<string>> _userFaultNotifier =
            new AsyncLocal<System.Action<string>>();

        /// <summary>
        /// Issue #736 finding 4: the injectable user-facing surface the default boundary sink
        /// reports a fault through. The default is the non-blocking modeless notice below; a test
        /// installs its own capture and restores the previous value afterwards.
        /// </summary>
        internal static System.Action<string> UserFaultNotifier
        {
            get => _userFaultNotifier.Value ?? ShowModelessFaultNotice;
            set => _userFaultNotifier.Value = value;
        }

        /// <summary>
        /// Default user-facing surface for a boundary fault: a modeless, self-disposing, read-only
        /// notice. <c>Show()</c> returns immediately, so the calling thread is never blocked, and no
        /// modal dialog is invoked from this path.
        /// </summary>
        /// <remarks>
        /// Excluded from coverage by inspection: every statement past the early return constructs
        /// WinForms controls, which the headless test host cannot exercise. The early return is
        /// load-bearing rather than defensive — the pre-existing default-delegate test invokes the
        /// sink directly in the test host, and without it that invocation would construct a window
        /// on an MSTest thread. The accepted consequence is that a fault raised while the add-in has
        /// no open WinForms window is logged and not displayed.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static void ShowModelessFaultNotice(string message)
        {
            if (System.Windows.Forms.Application.OpenForms.Count == 0)
            {
                return;
            }

            var notice = new Form();
            notice.Text = "QuickFiler";
            notice.TopMost = true;
            notice.ShowInTaskbar = false;
            notice.FormBorderStyle = FormBorderStyle.FixedDialog;
            notice.MinimizeBox = false;
            notice.MaximizeBox = false;
            notice.StartPosition = FormStartPosition.CenterScreen;
            notice.Width = 460;
            notice.Height = 160;

            var body = new TextBox();
            body.Text = message;
            body.ReadOnly = true;
            body.Multiline = true;
            body.Dock = DockStyle.Fill;
            body.BorderStyle = BorderStyle.None;
            notice.Controls.Add(body);

            notice.FormClosed += (sender, args) => notice.Dispose();
            notice.Show();
        }

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcDataModel _dataModel;
        private EfcViewer _formViewer;

        // Presented folder-suggestion rows; retained so the delete path can prepend
        // "Trash to Delete" and rebind through the breadcrumb router.
        private string[] _folderRows = Array.Empty<string>();

        // Breadcrumb WebView2 wiring (#349): the exempt host adapter over the Designer control and
        // the non-exempt router that owns all breadcrumb logic. This controller stays wiring-only.
        private WebView2BreadcrumbHost _breadcrumbHost;
        private BreadcrumbBridgeRouter _router;

        private EfcHomeController _homeController;
        private EfcItemController _itemController;
        private ItemViewer _itemViewer;

        //private FolderHandler _folderHandler;
        //private MailItem _mailItem;
        private QfEnums.InitTypeEnum _initType;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private TableLayoutPanel _itemTlp;
        private int _itemViewerTlpRow;
        private int _tlpHeightExpanded;
        private int _tlpHeightCollapsed;
        private int _tlpHeightDiff;
        private Dictionary<string, Theme> _themes;
        private List<Button> _listButtons;
        private List<Control> _listDefault;
        private List<Control> _listCheckBox;
        private List<Control> _listHighlighted;

        #endregion Private Properties

        #region Setup and Cleanup Methods

        internal void CaptureConfigureItemViewer()
        {
            var explorerSize = _globals.Ol.GetExplorerScreenSize();
            _tlpHeightExpanded = (int)Math.Round(_itemTlp.RowStyles[1].Height, 0);
            var heightDiff = _tlpHeightExpanded - _itemViewer.Height;
            _tlpHeightCollapsed = _itemViewer.MinimumSize.Height + heightDiff;
            _tlpHeightDiff = _tlpHeightExpanded - _tlpHeightCollapsed;
            _itemViewerTlpRow = _itemTlp.GetPositionFromControl(_itemViewer).Row;
            ToggleExpansionStyle(Enums.ToggleState.Off);
            var itemTlpRows = _itemViewer.L0vh_Tlp.RowStyles.Cast<RowStyle>().Take(5);
            var bodyRow = itemTlpRows.ElementAt(4);
            var bodyRowHeight =
                _tlpHeightCollapsed
                - itemTlpRows.Select(x => x.Height).Sum(x => x)
                + bodyRow.Height;
            bodyRow.Height = bodyRowHeight;
            _formViewer.MinimumSize = new Size(
                (int)(explorerSize.Width * 0.75),
                (int)(explorerSize.Height * 0.75)
            );
            _formViewer.Size = _formViewer.MinimumSize;
        }

        /// <summary>Releases collaborators. Safe on a partial controller and idempotent.</summary>
        public void Cleanup()
        {
            // Detach before nulling: release the subscription before dropping its owner.
            var globals = _globals;
            if (globals?.Ol is not null)
            {
                globals.Ol.PropertyChanged -= DarkMode_Changed;
            }
            _globals = null;
            _formViewer = null;
            _dataModel = null;

            // Clearing before invoking is what makes the single invocation structural.
            var parentCleanup = _parentCleanup;
            _parentCleanup = null;
            if (parentCleanup is not null)
            {
                parentCleanup.Invoke();
            }
        }

        public void ConfigureFind()
        {
            if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                _formViewer.Text = "Quick Filer - Find Folder";
                _formViewer.Ok.Text = "Open Outlook Folder";
                _formViewer.NewFolder.Text = "Open File System Folder";
            }
        }

        internal void ResolveControlGroups()
        {
            _listTipsDetails = _formViewer
                .TipsLabels.Select(x => (IQfcTipsDetails)new QfcTipsDetails(x))
                .ToList();
            _listTipsDetails.ForEach(x => x.Toggle(Enums.ToggleState.Off, true));

            var starter = _formViewer.GetAllChildren(except: new List<Control> { _itemViewer });

            _listButtons = starter.Where(x => x is Button).Cast<Button>().ToList();

            _listCheckBox = starter.Where(x => (x is CheckBox)).ToList();

            _listHighlighted = new List<Control>
            {
                _formViewer.SearchText,
                _formViewer.FolderListBox,
            };

            _listDefault = starter
                .Where(x =>
                    !_formViewer.TipsLabels.Contains(x)
                    && !_listButtons.Contains(x)
                    && !_listHighlighted.Contains(x)
                    && !_listCheckBox.Contains(x)
                )
                .ToList();
        }

        internal void SetupThemes()
        {
            _themes = EfcThemeHelper.SetupFormThemes(
                _formViewer.TipsLabels.Cast<Control>().ToList(),
                _listHighlighted,
                _listDefault,
                _listButtons.Cast<Control>().ToList(),
                _listCheckBox
            );

            _activeTheme = LoadTheme();
        }

        #endregion Setup and Cleanup Methods

        #region Public Properties

        private string _activeTheme;
        public string ActiveTheme
        {
            // GetOrLoad throws under strict: true once _themes is null, so test at the call
            // site and return the backing field on the torn-down path.
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

        private bool _darkMode;
        public bool DarkMode
        {
            // The params object[] dependency array is materialised before GetOrLoad is entered,
            // so _globals.Ol must be tested at the call site or the null path still dereferences.
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

        public IntPtr FormHandle => _formViewer.Handle;

        public string SelectedFolder
        {
            // Derived from the bridge router's selection tracking. IsValidSelection routes to
            // IsSelectableFolder, which composes IsBannerRow, matching the producers' "===="
            // prefix, with the guard's deliberately broader three-character rejection.
            get => _router?.SelectedFolderPath;
        }

        private bool _saveAttachments;
        public bool SaveAttachments
        {
            get => _saveAttachments;
            set
            {
                _saveAttachments = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SaveAttachments = value;
            }
        }

        private bool _saveEmail;
        public bool SaveEmail
        {
            get => _saveEmail;
            set
            {
                _saveEmail = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SaveEmail = value;
            }
        }

        private bool _savePictures;
        public bool SavePictures
        {
            get => _savePictures;
            set
            {
                _savePictures = value;
                // Should be set elsewhere as a user default
                //Settings.Default.SavePictures = value;
            }
        }

        private bool _moveConversation;
        public bool MoveConversation
        {
            get => _moveConversation;
            set
            {
                _moveConversation = value;
                // Should be set elsewhere as a user default
                //Settings.Default.MoveConversation = value;
            }
        }

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
            set => _token = value;
        }

        #endregion

        #region Event Handlers

        internal void RegisterAlwaysOnAsyncKeyActions()
        {
            _formViewer.KeyboardHandler.AlwaysOnKeyActionsAsync = new KbdActions<
                Keys,
                KaKeyAsync,
                Func<Keys, Task>
            >(
                new List<KaKeyAsync>
                {
                    new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync()),
                }
            );
        }

        public void WireEventHandlers()
        {
            //_homeController.KeyboardHandler.CharActions = new KbdActions<char, KaChar, Action<char>>();
            //_homeController.KeyboardHandler.CharActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();

            _formViewer.ForAllControls(
                x =>
                {
                    x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(
                        _homeController.KeyboardHandler.KeyboardHandler_PreviewKeyDownAsync
                    );
                    x.KeyDown += new System.Windows.Forms.KeyEventHandler(
                        _homeController.KeyboardHandler.KeyboardHandler_KeyDownAsync
                    );
                },
                new List<Control> { }
            );
            _formViewer.SaveAttachmentsMenuItem.CheckedChanged += SaveAttachments_CheckedChanged;
            _formViewer.SaveEmailMenuItem.CheckedChanged += SaveEmail_CheckedChanged;
            _formViewer.SavePicturesMenuItem.CheckedChanged += SavePictures_CheckedChanged;
            _formViewer.ConversationMenuItem.CheckedChanged += MoveConversation_CheckedChanged;
            _formViewer.Ok.Click += ButtonOK_Click;
            RegisterAlwaysOnAsyncKeyActions();
            ConfigureBreadcrumbControl();
            _formViewer.Cancel.Click += ButtonCancel_Click;
            _formViewer.RefreshPredicted.Click += ButtonRefresh_Click;
            _formViewer.NewFolder.Click += ButtonCreate_Click;
            _formViewer.BtnDelItem.Click += ButtonDelete_Click;
            _formViewer.SearchText.TextChanged += SearchText_TextChanged;
            _formViewer.SearchText.KeyDown += SearchText_DownArrow;
            _formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;
            _globals.Ol.PropertyChanged += DarkMode_Changed;
        }

        public void SearchText_DownArrow(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                // Enter the breadcrumb list and select its first row (parity with the prior
                // TreeListView down-arrow behavior); further key handling happens in-document.
                _formViewer.FolderListBox.Select();
                _router?.SelectFirstRow();
            }
        }

        public async void ButtonCancel_Click(object sender, EventArgs e) =>
            await ButtonCancelClickAsync();

        internal async Task ButtonCancelClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await ActionCancelAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonOK_Click(object sender, EventArgs e) => await ButtonOkClickAsync();

        internal async Task ButtonOkClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await ActionOkAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonRefresh_Click(object sender, EventArgs e) =>
            await ButtonRefreshClickAsync();

        internal async Task ButtonRefreshClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await RefreshSuggestionsAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonCreate_Click(object sender, EventArgs e) =>
            await ButtonCreateClickAsync();

        internal async Task ButtonCreateClickAsync()
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                if (!IsValidSelection)
                {
                    MessageBox.Show(
                        "Please select a valid parent folder where you would like to place the new folder."
                    );
                }
                else if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
                {
                    await _homeController.OpenFsFolderAsync(SelectedFolder);

                    _formViewer.Close();
                    Cleanup();
                }
                else
                {
                    if (!_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot))
                    {
                        logger.Debug($"Cannot create folder without OneDrive location");
                        return;
                    }
                    var folder =
                        (
                            await _dataModel.FolderHelper.CreateFolderAsync(
                                SelectedFolder,
                                _globals.Ol.ArchiveRootPath,
                                folderRoot,
                                Token
                            )
                        ) as MAPIFolder;

                    if (folder is not null)
                    {
                        await _dataModel.MoveToFolderAsync(
                            folder,
                            _globals.Ol.ArchiveRootPath,
                            SaveAttachments,
                            SaveEmail,
                            SavePictures,
                            MoveConversation
                        );

                        _formViewer.Close();
                        Cleanup();
                    }
                }
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        public async void ButtonDelete_Click(object sender, EventArgs e) =>
            await ButtonDeleteClickAsync();

        internal async Task ButtonDeleteClickAsync()
        {
            try
            {
                await ActionDeleteAsync();
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        private void SaveAttachments_CheckedChanged(object sender, EventArgs e)
        {
            SaveAttachments = _formViewer.SaveAttachmentsMenuItem.Checked;
        }

        private void SaveEmail_CheckedChanged(object sender, EventArgs e)
        {
            SaveEmail = _formViewer.SaveEmailMenuItem.Checked;
        }

        private void SavePictures_CheckedChanged(object sender, EventArgs e)
        {
            SavePictures = _formViewer.SavePicturesMenuItem.Checked;
        }

        private void MoveConversation_CheckedChanged(object sender, EventArgs e)
        {
            MoveConversation = _formViewer.ConversationMenuItem.Checked;
        }

        private void SearchText_TextChanged(object sender, EventArgs e)
        {
            BindSourceFolderRows(_dataModel.FindMatches(_formViewer.SearchText.Text));
        }

        public void EditFiltersMenuItem_Click(object sender, EventArgs e)
        {
            var filters = new ManageFilters();
            filters.LoadFilters(_globals);
            filters.Show();
        }

        private KbdActions<char, KaCharAsync, Func<char, Task>> _characterAsyncActions;
        internal KbdActions<char, KaCharAsync, Func<char, Task>> CharacterAsyncActions =>
            Initializer.GetOrLoad(ref _characterAsyncActions, GetAsyncCharacterActions);

        internal KbdActions<char, KaCharAsync, Func<char, Task>> GetAsyncCharacterActions()
        {
            return new KbdActions<char, KaCharAsync, Func<char, Task>>(
                new List<KaCharAsync>
                {
                    new KaCharAsync("Controller", 'S', (x) => JumpToAsync(_formViewer.SearchText)),
                    new KaCharAsync(
                        "Controller",
                        'F',
                        (x) => JumpToAsync(_formViewer.FolderListBox)
                    ),
                    //new KaCharAsync("Controller", 'A', (x) => ToggleCheckboxAsync(_formViewer.SaveAttachments)),
                    //new KaCharAsync("Controller", 'M', (x) => ToggleCheckboxAsync(_formViewer.SaveEmail)),
                    //new KaCharAsync("Controller", 'P', (x) => ToggleCheckboxAsync(_formViewer.SavePictures)),
                    //new KaCharAsync("Controller", 'C', (x) => ToggleCheckboxAsync(_formViewer.MoveConversation)),
                    new KaCharAsync("Controller", 'K', (x) => KbdExecuteAsync(ActionOkAsync)),
                    new KaCharAsync("Controller", 'X', (x) => KbdExecuteAsync(ActionCancelAsync)),
                    new KaCharAsync(
                        "Controller",
                        'R',
                        (x) => KbdExecuteAsync(RefreshSuggestionsAsync)
                    ),
                    new KaCharAsync("Controller", 'N', (x) => KbdExecuteAsync(CreateFolderAsync)),
                    new KaCharAsync("Controller", 'T', (x) => KbdExecuteAsync(ActionDeleteAsync)),
                    new KaCharAsync(
                        "Controller",
                        'M',
                        (x) => KbdExecuteAsync(() => ShowMenu(_formViewer.MoveOptionsMenu))
                    ),
                }
            );
        }

        //private Dictionary<char, Action<char>> _kbdActions;
        //public Dictionary<char, Action<char>> KbdActions => Initializer.GetOrLoad(ref _kbdActions, GetKbdActions);
        //internal Dictionary<char, Action<char>> GetKbdActions()
        //{
        //    return new()
        //    {
        //        { 'S', async (x) => await JumpToAsync(_formViewer.SearchText) },
        //        { 'F', async (x) => await JumpToAsync(_formViewer.FolderListBox) },
        //        { 'A', async (x) => await ToggleCheckboxAsync(_formViewer.SaveAttachments) },
        //        { 'M', async (x) => await ToggleCheckboxAsync(_formViewer.SaveEmail) },
        //        { 'P', async (x) => await ToggleCheckboxAsync(_formViewer.SavePictures) },
        //        { 'C', async (x) => await ToggleCheckboxAsync(_formViewer.MoveConversation) },
        //        { 'K', async (x) => await KbdExecuteAsync(ActionOkAsync) },
        //        { 'X', async (x) => await KbdExecuteAsync(ActionCancelAsync) },
        //        { 'R', async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync) },
        //        { 'N', async (x) => await KbdExecuteAsync(CreateFolderAsync) },
        //        { 'T', async (x) => await KbdExecuteAsync(ActionDeleteAsync) }
        //    };
        //}

        private KbdActions<char, KaChar, Action<char>> _characterActions;
        public KbdActions<char, KaChar, Action<char>> CharacterActions =>
            Initializer.GetOrLoad(ref _characterActions, GetKbdActions);

        internal KbdActions<char, KaChar, Action<char>> GetKbdActions()
        {
            return new KbdActions<char, KaChar, Action<char>>(
                new List<KaChar>
                {
                    new KaChar(
                        "Controller",
                        'S',
                        async (x) => await JumpToAsync(_formViewer.SearchText)
                    ),
                    new KaChar(
                        "Controller",
                        'F',
                        async (x) => await JumpToAsync(_formViewer.FolderListBox)
                    ),
                    new KaChar(
                        "Controller",
                        'K',
                        async (x) => await KbdExecuteAsync(ActionOkAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'X',
                        async (x) => await KbdExecuteAsync(ActionCancelAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'R',
                        async (x) => await KbdExecuteAsync(RefreshSuggestionsAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'N',
                        async (x) => await KbdExecuteAsync(CreateFolderAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'T',
                        async (x) => await KbdExecuteAsync(ActionDeleteAsync)
                    ),
                    new KaChar(
                        "Controller",
                        'M',
                        async (x) =>
                            await KbdExecuteAsync(() => ShowMenu(_formViewer.MoveOptionsMenu))
                    ),
                }
            );
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

                // Re-theme the breadcrumb document alongside the WinForms theme swap.
                _router?.ApplyTheme(DarkMode);
            }
        }

        #endregion

        #region Major Actions

        async public Task ActionOkAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            var selectedFolder = SelectedFolder;
            // Classifies through the single owner and retains #614's rooted-path rejection.
            if (
                selectedFolder is null
                || IsBannerRow(selectedFolder)
                || !EfcSelectionGuard.IsValidFilingSelection(selectedFolder)
            )
            {
                MessageBox.Show("Please select a valid folder.");
                return;
            }
            else
            {
                _formViewer.Hide();
                if (_initType.HasFlag(QfEnums.InitTypeEnum.Sort))
                {
                    await _homeController.ExecuteMovesAsync();
                }
                else if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
                {
                    await _homeController.OpenOlFolderAsync(SelectedFolder);
                }
                else
                {
                    throw new NotImplementedException();
                }
                _formViewer.Dispose();
                Cleanup();
            }
        }

        public async Task ActionCancelAsync()
        {
            //Debug.WriteLine($"Thread Id before await: {Thread.CurrentThread.ManagedThreadId}");
            await _formViewer.UiSyncContext;
            //Debug.WriteLine($"Thread Id after await: {Thread.CurrentThread.ManagedThreadId}");
            _formViewer.Close();
            Cleanup();
        }

        /// <summary>The pseudo-row that marks the delete target.</summary>
        internal const string TrashRowText = "Trash to Delete";

        /// <summary>Prepends the trash pseudo-row, idempotently.</summary>
        internal static string[] WithTrashRow(string[] rows)
        {
            if (rows is null)
            {
                return new[] { TrashRowText };
            }
            if (rows.Length > 0 && rows[0] == TrashRowText)
            {
                return rows;
            }
            var itemList = rows.ToList();
            itemList.Insert(0, TrashRowText);
            return itemList.ToArray();
        }

        /// <summary>Retains the delete-gesture rows, then binds them.</summary>
        internal void ApplyDeleteGesture()
        {
            _folderRows = WithTrashRow(_folderRows);
            BindFolderRows(_folderRows);
        }

        public async Task ActionDeleteAsync()
        {
            await _formViewer.UiSyncContext;
            ApplyDeleteGesture();
        }

        public async Task CreateFolderAsync()
        {
            if (!IsValidSelection)
            {
                MessageBox.Show("Please select a valid folder");
            }
            else if (_initType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                await _homeController.OpenFsFolderAsync(SelectedFolder);
            }
            else
            {
                await _formViewer.UiSyncContext;
                _formViewer.Hide();
                if (!_globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive))
                {
                    return;
                }
                var folder = await Task.FromResult(
                        _dataModel.FolderHelper.CreateFolder(
                            SelectedFolder,
                            _globals.Ol.ArchiveRootPath,
                            oneDrive
                        )
                    )
                    .ConfigureAwait(false);
                if (folder is not null)
                {
                    await _dataModel
                        .MoveToFolderAsync(
                            folder,
                            _globals.Ol.ArchiveRootPath,
                            SaveAttachments,
                            SaveEmail,
                            SavePictures,
                            MoveConversation
                        )
                        .ConfigureAwait(false);
                    await _formViewer.UiSyncContext;
                    _formViewer.Dispose();
                    Cleanup();
                }
            }
        }

        /// <summary>Applies a match delegate to a search string; never returns null.</summary>
        internal static string[] MatchesForSearchText(
            System.Func<string, string[]> findMatches,
            string searchText
        )
        {
            if (findMatches is null)
            {
                return Array.Empty<string>();
            }
            return findMatches(searchText ?? string.Empty) ?? Array.Empty<string>();
        }

        /// <summary>
        /// #465 B (RC8): the control read happens here, on the UI thread, before any
        /// <c>Task.Run</c>, carrying an unchanged value into the worker.
        /// </summary>
        public async Task RefreshSuggestionsAsync()
        {
            var searchText = _formViewer.SearchText.Text;

            await Task.Run(() => _dataModel.RefreshSuggestions(), Token);
            var matches = await Task.Run(
                () => MatchesForSearchText(_dataModel.FindMatches, searchText),
                Token
            );

            BindSourceFolderRows(matches);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Issue #736 finding 2: the single containment point for the keyboard-dispatch path. Both
        /// <c>KbdExecuteAsync</c> overloads route their two statements through this member, so the
        /// handling is written once rather than duplicated, and a fault raised by the
        /// keyboard-dialog toggle is covered as well as one raised by the dispatched action.
        /// </summary>
        internal async Task RunKbdGuardedAsync(System.Func<Task> body)
        {
            try
            {
                await body();
            }
            catch (OperationCanceledException)
            {
                // Cancellation is not a fault, so it is recorded at debug level and deliberately
                // not reported through the sink, matching the existing distinction in
                // BindBreadcrumbRowsAsync.
                logger.Debug("Keyboard dispatch canceled.");
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault($"Keyboard dispatch failed: {ex.Message}", ex);
            }
        }

        public async Task KbdExecuteAsync(Func<Task> action)
        {
            await RunKbdGuardedAsync(async () =>
            {
                await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
                await action();
            });
        }

        public async Task KbdExecuteAsync(System.Action action)
        {
            await RunKbdGuardedAsync(async () =>
            {
                await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
                action();
            });
        }

        internal async Task JumpToAsync(Control control)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            //await _formViewer.UiSyncContext;
            control.Focus();
        }

        // Wiring-only breadcrumb setup (#349): constructs the exempt WebView2 host adapter and the
        // non-exempt router where ConfigureFolderTreeView previously wired the TreeListView, then
        // connects the router's events back to the form. All breadcrumb logic lives in the router.
        private void ConfigureBreadcrumbControl()
        {
            _breadcrumbHost = new WebView2BreadcrumbHost(
                _formViewer.BreadcrumbWebView,
                new WebView2CoreInitializer()
            );
            var provider = new UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider(
                _globals.Ol.FolderTreeService
            );
            _router = new BreadcrumbBridgeRouter(
                provider,
                _breadcrumbHost,
                new UtilitiesCS.OutlookObjects.Folder.BreadcrumbMessageCodec(),
                new UtilitiesCS.OutlookObjects.Folder.BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(_breadcrumbHost)
            );
            _breadcrumbHost.CoreInitialized += (s, e) => _router.NotifyCoreInitialized();
            _router.FocusSearchRequested += (s, e) => _formViewer?.SearchText.Select();
            _router.ApplyTheme(DarkMode);
            _ = InitializeBreadcrumbHostAsync();
        }

        // Fire-and-forget host initialization with an error boundary (the router queues every
        // outbound payload until CoreWebView2InitializationCompleted fires).
        private async Task InitializeBreadcrumbHostAsync()
        {
            try
            {
                await _breadcrumbHost.InitializeAsync(_formViewer.UiSyncContext);
            }
            catch (System.Exception ex)
            {
                logger.Error($"Breadcrumb WebView2 initialization failed: {ex.Message}", ex);
            }
        }

        // Presentation only. #465 C (RC9) removed the _folderRows write-back: neither assigns
        // nor reads the field.
        private void BindFolderRows(string[] rows)
        {
            var formViewer = _formViewer;
            if (formViewer == null || _router == null)
            {
                return;
            }

            _ = BindBreadcrumbRowsAsync(rows ?? Array.Empty<string>());
        }

        // Retention plus presentation for the three source paths; retaining here rather than in
        // BindFolderRows is what stops the delete gesture accumulating.
        private void BindSourceFolderRows(string[] rows)
        {
            var formViewer = _formViewer;
            if (formViewer == null || _router == null)
            {
                return;
            }

            _folderRows = rows ?? Array.Empty<string>();
            BindFolderRows(_folderRows);
        }

        // Async bind boundary: joins the feature-324 score projection and delegates to the router.
        internal async Task BindBreadcrumbRowsAsync(string[] rows)
        {
            try
            {
                var scores =
                    _dataModel?.FolderHelper?.Suggestions?.ToScoredArray()
                    ?? Array.Empty<FolderScore>();
                await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);
            }
            catch (OperationCanceledException)
            {
                logger.Debug("Breadcrumb bind canceled.");
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault($"Breadcrumb bind failed: {ex.Message}", ex);
            }
        }

        public void MaximizeFormViewer()
        {
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }

        public void MinimizeFormViewer()
        {
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }

        internal void ShowMenu(ToolStripMenuItem menu) => menu.ShowDropDown();

        public async Task ToggleCheckboxAsync(CheckBox checkBox)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            checkBox.Checked = !checkBox.Checked;
        }

        public void ToggleOffNavigation(bool async)
        {
            CharacterActions.Keys.ForEach(key =>
                _homeController.KeyboardHandler.CharActions.Remove("Controller", key)
            );
            ToggleTips(async, Enums.ToggleState.Off);
            _itemController.ToggleNavigation(async, Enums.ToggleState.Off);
        }

        public async Task ToggleOffNavigationAsync()
        {
            CharacterAsyncActions.Keys.ForEach(key =>
                _homeController.KeyboardHandler.CharActionsAsync.Remove("Controller", key)
            );
            await ToggleTipsAsync(Enums.ToggleState.Off);
            await _itemController.ToggleNavigationAsync(Enums.ToggleState.Off);
        }

        public void ToggleOnNavigation(bool async)
        {
            CharacterActions.ForEach(x => _homeController.KeyboardHandler.CharActions.Add(x));
            ToggleTips(async, Enums.ToggleState.On);
            _itemController.ToggleNavigation(async, Enums.ToggleState.On);
        }

        public async Task ToggleOnNavigationAsync()
        {
            CharacterAsyncActions.ForEach(x =>
                _homeController.KeyboardHandler.CharActionsAsync.Add(x)
            );
            await ToggleTipsAsync(Enums.ToggleState.On);
            await _itemController.ToggleNavigationAsync(Enums.ToggleState.On);
        }

        public void ToggleTips(bool async)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async)
                {
                    _formViewer.BeginInvoke(new System.Action(() => tipsDetails.Toggle(true)));
                }
                else
                {
                    _formViewer.Invoke(new System.Action(() => tipsDetails.Toggle(true)));
                }
            }
        }

        public void ToggleTips(bool async, Enums.ToggleState desiredState)
        {
            foreach (IQfcTipsDetails tipsDetails in _listTipsDetails)
            {
                if (async)
                {
                    _formViewer.BeginInvoke(
                        new System.Action(() => tipsDetails.Toggle(desiredState, true))
                    );
                }
                else
                {
                    _formViewer.Invoke(
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

        internal void LoadUserSettings()
        {
            _saveAttachments = Settings.Default.SaveAttachments;
            _formViewer.SaveAttachmentsMenuItem.Checked = _saveAttachments;

            _saveEmail = Settings.Default.SaveEmail;
            _formViewer.SaveEmailMenuItem.Checked = _saveEmail;

            _savePictures = Settings.Default.SavePictures;
            _formViewer.SavePicturesMenuItem.Checked = _savePictures;

            _moveConversation = Settings.Default.MoveConversation;
            _formViewer.ConversationMenuItem.Checked = _moveConversation;
        }

        /// <summary>#464 C: both call sites discard the result, so the boundary is here.</summary>
        public async Task PopulateFolderCombobox(object folderList = null)
        {
            try
            {
                // Capture _formViewer in a local variable before the first await. Cleanup() may set
                // _formViewer to null while InitFolderHandlerAsync is executing (e.g. the user
                // dismisses the form), so all post-await access must go through this local reference.
                var formViewer = _formViewer;
                if (formViewer == null)
                    return;

                await _dataModel.InitFolderHandlerAsync(folderList);

                await formViewer.UiSyncContext;

                BindSourceFolderRows(_dataModel.FolderHelper.FolderArray);
            }
            catch (System.Exception ex)
            {
                TryReportBoundaryFault(ex.Message, ex);
            }
        }

        // #465 D (RC7): single classification owner. StartsWith, never Substring, over the
        internal static bool IsBannerRow(string row) =>
            row is not null
            && row.StartsWith(
                UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix,
                StringComparison.Ordinal
            );

        // The rest of IsValidSelection's pure logic, routed through the owner above.
        internal static bool IsSelectableFolder(string selectedFolder) =>
            !IsBannerRow(selectedFolder)
            && EfcSelectionGuard.IsValidCreationSelection(selectedFolder);

        internal bool IsValidSelection => IsSelectableFolder(SelectedFolder);

        #endregion

        public void ToggleExpansionStyle(Enums.ToggleState desiredState)
        {
            if (desiredState == Enums.ToggleState.On)
            {
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightExpanded;
                _formViewer.MinimumSize = new Size(
                    _formViewer.MinimumSize.Width,
                    _formViewer.MinimumSize.Height + _tlpHeightDiff
                );
                _formViewer.Size = new Size(
                    _formViewer.Size.Width,
                    _formViewer.Size.Height + _tlpHeightDiff
                );
                _formViewer.WindowState = FormWindowState.Maximized;
            }
            else
            {
                _formViewer.WindowState = FormWindowState.Normal;
                _itemTlp.RowStyles[_itemViewerTlpRow].Height = _tlpHeightCollapsed;
                _formViewer.MinimumSize = new Size(
                    _formViewer.MinimumSize.Width,
                    _formViewer.MinimumSize.Height - _tlpHeightDiff
                );
                _formViewer.Size = new Size(
                    _formViewer.Size.Width,
                    _formViewer.Size.Height - _tlpHeightDiff
                );
            }
        }
    }
}
