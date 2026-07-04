using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace QuickFiler
{
    public partial class EfcHomeController : IFilerHomeController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType
        );

        private static Func<EfcHomeControllerDependencies> _defaultDependenciesFactory = () =>
            new EfcHomeControllerDependencies();

        internal static void SetDefaultDependenciesFactory(
            Func<EfcHomeControllerDependencies> factory
        )
        {
            _defaultDependenciesFactory =
                factory ?? throw new ArgumentNullException(nameof(factory));
        }

        internal static void ResetDefaultDependenciesFactory()
        {
            _defaultDependenciesFactory = () => new EfcHomeControllerDependencies();
        }

        private static EfcHomeControllerDependencies CreateDefaultDependencies()
        {
            return _defaultDependenciesFactory();
        }

        #region Constructors, Initializers, and Destructors

        public EfcHomeController(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            MailItem mail = null
        )
            : this(globals, parentCleanup, CreateDefaultDependencies(), mail) { }

        internal EfcHomeController(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            EfcHomeControllerDependencies dependencies,
            MailItem mail = null
        )
        {
            dependencies.ThrowIfNull();
            CreateCancellationToken();
            Globals = globals;
            _parentCleanup = parentCleanup;
            _dependencies = dependencies;
            DataModel = _dependencies.DataModelFactory(
                _globals,
                mail,
                this.TokenSource,
                this.Token
            );

            if (DataModel.Mail is not null)
            {
                InitType = QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv;
                _stopWatch = new Stopwatch();
                FormViewer = _dependencies.ViewerFactory();
                _uiSyncContext = FormViewer.UiSyncContext;
                _keyboardHandler = _dependencies.KeyboardHandlerFactory(FormViewer, this);
                _explorerController = _dependencies.ExplorerControllerFactory(
                    QfEnums.InitTypeEnum.Sort,
                    globals,
                    this
                );
                _formController = _dependencies.FormControllerWithDataFactory(
                    Globals,
                    _dataModel,
                    FormViewer,
                    this,
                    Cleanup,
                    InitType,
                    Token
                );
            }
        }

        private EfcHomeController(IApplicationGlobals globals, System.Action parentCleanup)
        {
            Globals = globals;
            _parentCleanup = parentCleanup;
            _dependencies = CreateDefaultDependencies();
        }

        public static async Task<EfcHomeController> CreateAsync(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            MailItem mail = null
        )
        {
            return await CreateAsync(globals, parentCleanup, CreateDefaultDependencies(), mail);
        }

        internal static async Task<EfcHomeController> CreateAsync(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            EfcHomeControllerDependencies dependencies,
            MailItem mail = null
        )
        {
            globals.ThrowIfNull();
            parentCleanup.ThrowIfNull();
            dependencies.ThrowIfNull();

            var home = new EfcHomeController(globals, parentCleanup);
            home._dependencies = dependencies;
            home.CreateCancellationToken();
            var mailItems = LoadToList(globals, mail, dependencies);

            if (mailItems.Count() > 0)
            {
                await home.HandleSelectionChangedAsync(
                    globals,
                    mailItems,
                    QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv
                );
            }
            return home;
        }

        public static async Task<EfcHomeController> LoadFinderAsync(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            MailItem mail = null
        )
        {
            return await LoadFinderAsync(globals, parentCleanup, CreateDefaultDependencies(), mail);
        }

        internal static async Task<EfcHomeController> LoadFinderAsync(
            IApplicationGlobals globals,
            System.Action parentCleanup,
            EfcHomeControllerDependencies dependencies,
            MailItem mail = null
        )
        {
            globals.ThrowIfNull();
            parentCleanup.ThrowIfNull();
            dependencies.ThrowIfNull();

            var home = new EfcHomeController(globals, parentCleanup);
            home._dependencies = dependencies;
            home.CreateCancellationToken();
            var mailItems = LoadToList(globals, mail, dependencies);

            await home.HandleSelectionChangedAsync(globals, mailItems, QfEnums.InitTypeEnum.Find);

            return home;
        }

        protected internal async Task HandleSelectionChangedAsync(
            IApplicationGlobals globals,
            List<MailItem> mailItems,
            QfEnums.InitTypeEnum initType
        )
        {
            var selectionStopwatch = Stopwatch.StartNew();
            var selectionSnapshot = CaptureSelectionSnapshot(mailItems);
            var selectedItemCount = selectionSnapshot.Count;
            LogFirstSelectionTiming(
                "[First-selection timing] HandleSelectionChangedAsync selection snapshot | selection snapshot",
                globals,
                selectedItemCount,
                "selection snapshot captured before background model load"
            );

            await InitAsync(globals, selectionSnapshot, initType);

            LogFirstSelectionTiming(
                "[First-selection timing] HandleSelectionChangedAsync final UI publish | final UI publish",
                globals,
                selectedItemCount,
                $"final UI publish after model initialization; elapsedMs={selectionStopwatch.ElapsedMilliseconds}"
            );
        }

        private static List<MailItem> CaptureSelectionSnapshot(List<MailItem> mailItems)
        {
            // Freeze the caller-visible selection membership before any asynchronous model staging
            // begins so controller orchestration no longer depends on a live, mutable selection list.
            return mailItems is null ? [] : [.. mailItems];
        }

        protected async Task InitAsync(
            IApplicationGlobals globals,
            List<MailItem> mailItems,
            QfEnums.InitTypeEnum initType
        )
        {
            // The controller publishes only two coarse UI stages: shell initialization without data,
            // then a single data-field publication after the staged model load completes.
            Task<EfcDataModel> modelTask = null;
            if (mailItems.Count() > 0)
            {
                modelTask = _dependencies.AsyncDataModelFactory(
                    globals,
                    mailItems,
                    TokenSource,
                    Token,
                    false
                );
            }

            // Initialize the rest of the home controller
            InitType = initType;
            _stopWatch = new Stopwatch();
            FormViewer = _dependencies.ViewerFactory();
            _uiSyncContext = FormViewer.UiSyncContext;
            _keyboardHandler = _dependencies.KeyboardHandlerFactory(FormViewer, this);
            _explorerController = _dependencies.ExplorerControllerFactory(initType, globals, this);
            _formController = _dependencies.FormControllerWithoutDataFactory(
                globals,
                FormViewer,
                this,
                Cleanup,
                initType,
                Token
            );

            if (mailItems.Count() > 0)
            {
                // Wait for data model to finish initializing
                DataModel = await modelTask;

                // Initialize data fields in form controller
                _formController = _dependencies.InitializeDataFields(_formController, DataModel);
            }
            else
            {
                // Dummy data model
                DataModel = _dependencies.DataModelFactory(globals, null, TokenSource, Token);
                _formController = _dependencies.InitializeDataFields(_formController, DataModel);
            }
        }

        private static List<MailItem> LoadToList(
            IApplicationGlobals globals,
            MailItem mail,
            EfcHomeControllerDependencies dependencies
        )
        {
            return dependencies.SelectionLoader(globals, mail);
        }

        private EfcViewer _formViewer;
        internal EfcViewer FormViewer
        {
            get => _formViewer;
            private set => _formViewer = value;
        }

        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals
        {
            get => _globals;
            private set => _globals = value;
        }

        private QfEnums.InitTypeEnum _initType;
        internal QfEnums.InitTypeEnum InitType
        {
            get => _initType;
            set => _initType = value;
        }

        private System.Action _parentCleanup;
        internal System.Action ParentCleanup
        {
            get => _parentCleanup;
            private set => _parentCleanup = value;
        }

        private EfcHomeControllerDependencies _dependencies;

        internal Action<EfcViewer> ViewerShowAction { get; set; } = viewer => viewer.Show();

        internal Func<EfcViewer, Task> ViewerShowAsyncAction { get; set; } =
            async viewer => await UiThread.Dispatcher.InvokeAsync(() => viewer.Show());

        internal Action<
            string,
            string,
            MessageBoxButtons,
            MessageBoxIcon
        > MessageBoxShowAction { get; set; } =
            (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);

        //[STAThread]
        public void Run()
        {
            if (_dataModel?.Mail is not null || InitType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                ViewerShowAction(_formViewer);
            }
            else
            {
                MessageBoxShowAction(
                    "Error",
                    "No MailItem Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public async Task RunAsync(ProgressTracker progress = null)
        {
            if (_dataModel?.Mail is not null || InitType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                await ViewerShowAsyncAction(_formViewer);
            }
            else
            {
                MessageBoxShowAction(
                    "Error",
                    "No MailItem Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            _parentCleanup.Invoke();
        }

        #endregion

        #region Public Properties

        private IQfcExplorerController _explorerController;
        public IQfcExplorerController ExplorerController
        {
            get => _explorerController;
            set => _explorerController = value;
        }

        private EfcFormController _formController;
        public IFilerFormController FormController
        {
            get => _formController;
        }

        private IQfcKeyboardHandler _keyboardHandler;
        public IQfcKeyboardHandler KeyboardHandler
        {
            get => _keyboardHandler;
            set => _keyboardHandler = value;
        }

        private EfcDataModel _dataModel;
        internal EfcDataModel DataModel
        {
            get => _dataModel;
            set => _dataModel = value;
        }

        private System.Diagnostics.Stopwatch _stopWatch;
        public System.Diagnostics.Stopwatch StopWatch
        {
            get => _stopWatch;
        }

        private volatile bool _isExecuting;

        public bool Loaded => throw new NotImplementedException();

        internal void CreateCancellationToken()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource
        {
            get => _tokenSource;
        }

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
        }

        private SynchronizationContext _uiSyncContext;
        public SynchronizationContext UiSyncContext
        {
            get => _uiSyncContext;
        }

        public FilerQueue FilerQueue => throw new NotImplementedException();

        #endregion

        #region Major Actions

        async public Task ExecuteMovesAsync()
        {
            if (_isExecuting)
                return;

            _isExecuting = true;
            try
            {
                var selectedFolder = _formController.SelectedFolder;
                var moveConversation = _formController.MoveConversation;
                var convInfo = DataModel.ConversationResolver.ConversationInfo.SameFolder;
                if (!moveConversation)
                {
                    convInfo = convInfo
                        .Where(itemInfo => itemInfo.EntryId == DataModel.Mail.EntryID)
                        .ToList();
                }

                // Capture _globals before the await: Cleanup() may null the field while
                // MoveToFolderAsync is in flight, causing NullReferenceException on resume.
                var globals = _globals;
                var result = await _dataModel.MoveToFolderAsync(
                    selectedFolder,
                    _formController.SaveAttachments,
                    _formController.SaveEmail,
                    _formController.SavePictures,
                    moveConversation
                );

                if (!result)
                {
                    MessageBox.Show($"Cannot move to folderpath {selectedFolder}");
                }
                else
                {
                    QuickFileMetrics_WRITE(
                        globals.FS.Filenames.EmailSession,
                        selectedFolder,
                        convInfo
                    );
                }
            }
            finally
            {
                _isExecuting = false;
            }
        }

        internal async Task OpenOlFolderAsync(string selectedFolder)
        {
            await DataModel.OpenOlFolderAsync(selectedFolder);
        }

        internal async Task OpenFsFolderAsync(string selectedFolder)
        {
            await DataModel.OpenFsFolderAsync(selectedFolder);
        }

        #endregion

        #region Helper Methods

        //public IList<MailItem> PackageItems() => _conversationResolver.ConversationItems;

        #endregion
    }
}
