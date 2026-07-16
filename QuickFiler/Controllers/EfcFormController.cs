using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    [ExcludeFromCodeCoverage]
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

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcDataModel _dataModel;
        private EfcViewer _formViewer;

        // Presented folder-suggestion rows and the host-neutral tree they build. The tree is rebuilt
        // whenever the presented rows change; _folderRows retains the last raw rows so the delete path
        // can prepend "Trash to Delete" and rebind.
        private FolderSuggestionTree _folderTree;
        private string[] _folderRows = Array.Empty<string>();

        // The currently highlighted tree node, cached from the TreeListView SelectionChanged event so
        // SelectedFolder/IsValidSelection derive the filing path from the selected node.
        private FolderSuggestionNode _selectedNode;

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

        public void Cleanup()
        {
            _globals.Ol.PropertyChanged -= DarkMode_Changed;
            _globals = null;
            _formViewer = null;
            _dataModel = null;
            _parentCleanup.Invoke();
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
            get => Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
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
            _themes[activeTheme].SetTheme();
            return activeTheme;
        }

        private bool _darkMode;
        public bool DarkMode
        {
            get =>
                Initializer.GetOrLoad(
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
            // The selected TreeListView model is a FolderSuggestionNode (cached in _selectedNode from
            // SelectionChanged); its FullPath is the folder identity used for filing. Banner nodes
            // carry the banner text (which starts with "===="), so IsValidSelection continues to
            // reject them.
            get => _selectedNode?.FullPath;
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
            ConfigureFolderTreeView();
            _formViewer.FolderListBox.KeyDown += FolderListBox_KeyDown;
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
                _formViewer.FolderListBox.Select();
            }
        }

        public void FolderListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && _formViewer.FolderListBox.SelectedIndex == 0)
            {
                _formViewer.SearchText.Select();
            }
            else if (e.KeyCode == Keys.Left)
            {
                // Collapse the highlighted node. The TreeListView (configured with
                // CanExpandGetter/ChildrenGetter) already collapses the focused row natively on the
                // Left arrow; this delegates to the host-neutral state model to keep it in sync
                // without duplicating the control's own toggle (LeftArrow is a no-op on a leaf, an
                // already-collapsed node, or a banner).
                _folderTree?.LeftArrow(_selectedNode);
            }
            else if (e.KeyCode == Keys.Right)
            {
                // Expand the highlighted node; mirrors the native TreeListView Right-arrow behavior in
                // the host-neutral state model (RightArrow is a no-op on a leaf, an already-expanded
                // node, or a banner).
                _folderTree?.RightArrow(_selectedNode);
            }
        }

        public async void ButtonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await ActionCancelAsync();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async void ButtonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await ActionOkAsync();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (SynchronizationContext.Current is null)
                    SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

                await RefreshSuggestionsAsync();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async void ButtonCreate_Click(object sender, EventArgs e)
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
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public async void ButtonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                await ActionDeleteAsync();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
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
            BindFolderRows(_dataModel.FindMatches(_formViewer.SearchText.Text));
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
            }
        }

        #endregion

        #region Major Actions

        async public Task ActionOkAsync()
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            var selectedFolder = SelectedFolder;
            if (selectedFolder is null || selectedFolder.StartsWith("===="))
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

        public async Task ActionDeleteAsync()
        {
            await _formViewer.UiSyncContext;
            // Prepend the "Trash to Delete" pseudo-row to the current presented rows and rebind, so the
            // user can select it as the delete target (preserving the pre-TreeListView delete path).
            var itemList = _folderRows.ToList();
            itemList.Insert(0, "Trash to Delete");
            BindFolderRows(itemList.ToArray());
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

        public async Task RefreshSuggestionsAsync()
        {
            await Task.Run(() => _dataModel.RefreshSuggestions(), Token);
            var matches = await Task.Run(
                () => _dataModel.FindMatches(_formViewer.SearchText.Text),
                Token
            );

            BindFolderRows(matches);
        }

        #endregion

        #region Helper Methods

        async public Task KbdExecuteAsync(Func<Task> action)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            await action();
        }

        public async Task KbdExecuteAsync(System.Action action)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            action();
        }

        internal async Task JumpToAsync(Control control)
        {
            await _homeController.KeyboardHandler.ToggleKeyboardDialogAsync();
            //await _formViewer.UiSyncContext;
            control.Focus();
        }

        // Configures the TreeListView once: the tree glyph/children come from the host-neutral node
        // model, and the two columns render the folder name and the right-aligned whole-number percent.
        private void ConfigureFolderTreeView()
        {
            var tlv = _formViewer.FolderListBox;
            tlv.CanExpandGetter = model => (model as FolderSuggestionNode)?.HasChildren ?? false;
            tlv.ChildrenGetter = model => (model as FolderSuggestionNode)?.Children;
            tlv.SelectionChanged += FolderListBox_SelectionChanged;
            _formViewer.olvColumnFolder.AspectGetter = model =>
                (model as FolderSuggestionNode)?.DisplayName ?? string.Empty;
            _formViewer.olvColumnPercent.AspectGetter = model =>
                PercentageFormatter.FormatPercent((model as FolderSuggestionNode)?.Probability);
        }

        // Caches the highlighted node so SelectedFolder/IsValidSelection and the Left/Right key
        // transitions read the current selection without touching the control on each access.
        private void FolderListBox_SelectionChanged(object sender, EventArgs e)
        {
            _selectedNode = _formViewer?.FolderListBox.SelectedObject as FolderSuggestionNode;
        }

        // Builds the host-neutral suggestion tree from the presented rows, joins upstream
        // probabilities, and feeds the TreeListView. Selects the first row after the leading banner,
        // matching the prior ListBox behavior (index 1). Uses a local viewer reference so a concurrent
        // Cleanup() that nulls _formViewer cannot cause a NullReferenceException.
        private void BindFolderRows(string[] rows)
        {
            var formViewer = _formViewer;
            if (formViewer == null)
            {
                return;
            }

            _folderRows = rows ?? Array.Empty<string>();
            var tree = FolderSuggestionTree.BuildFromRows(_folderRows);

            var source = BuildProbabilitySource();
            if (source != null)
            {
                new FolderProbabilityAdapter(source).Apply(tree);
            }

            _folderTree = tree;
            _selectedNode = null;

            var tlv = formViewer.FolderListBox;
            tlv.SetObjects(tree.Roots);
            if (tlv.Items.Count > 1)
            {
                tlv.SelectedIndex = 1;
            }
        }

        // Projects the scored suggestions into a full-path -> probability lookup consumed by the
        // FolderProbabilityAdapter. Returns null when no scored suggestions are available, in which
        // case every percentage cell renders blank.
        private IFolderProbabilitySource BuildProbabilitySource()
        {
            var scores = _dataModel?.FolderHelper?.Suggestions?.ToScoredArray();
            if (scores == null || scores.Length == 0)
            {
                return null;
            }

            return new DictionaryProbabilitySource(scores);
        }

        // Thin, exempt implementation of the host-neutral 9001 seam backed by the merged
        // FolderScore projection (FolderPath -> Probability in [0,1]).
        [ExcludeFromCodeCoverage]
        private sealed class DictionaryProbabilitySource : IFolderProbabilitySource
        {
            private readonly Dictionary<string, double> _map;

            public DictionaryProbabilitySource(IEnumerable<FolderScore> scores)
            {
                _map = new Dictionary<string, double>(StringComparer.Ordinal);
                foreach (var score in scores)
                {
                    // Last value wins for duplicate paths; avoids an add-collision exception.
                    _map[score.FolderPath] = score.Probability;
                }
            }

            public bool TryGetProbability(string fullFolderPath, out double probability)
            {
                return _map.TryGetValue(fullFolderPath, out probability);
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

        public async Task PopulateFolderCombobox(object folderList = null)
        {
            // Capture _formViewer in a local variable before the first await. Cleanup() may set
            // _formViewer to null while InitFolderHandlerAsync is executing (e.g. the user
            // dismisses the form), so all post-await access must go through this local reference.
            var formViewer = _formViewer;
            if (formViewer == null)
                return;

            await _dataModel.InitFolderHandlerAsync(folderList);

            await formViewer.UiSyncContext;

            BindFolderRows(_dataModel.FolderHelper.FolderArray);
        }

        internal bool IsValidSelection
        {
            get
            {
                var selectedFolder = SelectedFolder;
                return !(
                    selectedFolder is null
                    || selectedFolder == ""
                    || selectedFolder.Length < 3
                    || selectedFolder.Substring(0, 3) == "==="
                );
            }
        }

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
