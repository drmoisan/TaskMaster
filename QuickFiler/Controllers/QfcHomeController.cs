using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;
using static QuickFiler.QfEnums;

[assembly: InternalsVisibleTo("QuickFiler.Test")]

namespace QuickFiler.Controllers
{
    public partial class QfcHomeController : IQfcHomeController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors, Initializers, and Destructors

        private QfcHomeController() { }

        public QfcHomeController(IApplicationGlobals globals, System.Action parentCleanup)
        {
            Globals = globals;
            ParentCleanup = parentCleanup;
        }

        public static async Task<QfcHomeController> LaunchAsync(
            IApplicationGlobals appGlobals,
            System.Action parentCleanup,
            TimeProvider timeProvider = null
        )
        {
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} {nameof(QfcHomeController)}.{nameof(LaunchAsync)} is beginning");

            // Establish a SynchronizationContext for the UI thread
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext()
                );

            // Create uninitialized instance of QfcHomeController
            var controller = new QfcHomeController();
            controller.TimeProvider = timeProvider ?? TimeProvider.System;

            // Create cancellation token and progress tracker
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();

            try
            {
                await controller.InitAsync(
                    appGlobals,
                    parentCleanup,
                    tokenSource,
                    token,
                    progress.SpawnChild(86)
                );
                controller.Loaded = true;

                await controller.RunAsync(progress.SpawnChild());
            }
            catch (OperationCanceledException)
            {
                logger.Info(
                    $"{controller.TimeProvider.GetLocalNow().LocalDateTime.ToString("mm:ss.fff")} "
                        + $"{nameof(QfcHomeController)}.{nameof(LaunchAsync)} was cancelled"
                );
                if (progress is not null)
                    progress.Report(100);

                controller = null;
            }

            return controller;
        }

        public IQfcHomeController Init()
        {
            _datamodel = QfcDataModelLoader(Globals, this.Token);
            _explorerController = QfcExplorerControllerLoader(InitTypeEnum.Sort, Globals, this);
            _formViewer = new QfcFormViewer();
            _formViewer.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            _uiSyncContext = _formViewer.UiSyncContext;
            _keyboardHandler = QfcKeyboardHandlerLoader(_formViewer, this);
            QfcQueue = QfcQueueLoader(this.Token, this, Globals);
            _formController = QfcFormControllerLoader(
                Globals,
                _formViewer,
                QfcQueue,
                InitTypeEnum.Sort,
                Cleanup,
                this,
                this._tokenSource,
                this._token
            );
            return this;
        }

        internal async Task InitAsync(
            IApplicationGlobals appGlobals,
            System.Action parentCleanup,
            CancellationTokenSource tokenSource,
            CancellationToken token,
            ProgressTracker progress
        )
        {
            _token = token;
            _tokenSource = tokenSource;
            Globals = appGlobals;
            ParentCleanup = parentCleanup;

            // Load the data model in the background
            var dataModelTask = QfcAsyncDataModelLoader(
                Globals,
                this.Token,
                this.TokenSource,
                progress
            );

            // Load all components Synchronously with minimal initialization
            _formViewer = new QfcFormViewer();
            _formViewer.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            _uiSyncContext = _formViewer.UiSyncContext;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _explorerController = QfcExplorerControllerLoader(InitTypeEnum.Sort, Globals, this);
            _keyboardHandler = QfcKeyboardHandlerLoader(_formViewer, this);
            QfcQueue = QfcQueueLoader(this.Token, this, Globals);
            _formController = QfcFormControllerLoader(
                Globals,
                _formViewer,
                QfcQueue,
                InitTypeEnum.Sort,
                Cleanup,
                this,
                TokenSource,
                Token
            );

            // Wait for the data model to finish loading asynchronously
            _datamodel = await dataModelTask;
        }

        internal IApplicationGlobals Globals { get; set; }
        internal IQfcQueue QfcQueue { get; set; }
        internal System.Action ParentCleanup { get; set; }

        internal Func<
            IApplicationGlobals,
            CancellationToken,
            IQfcDatamodel
        > QfcDataModelLoader { get; set; } = (globals, cancel) => new QfcDatamodel(globals, cancel);

        internal Func<
            IApplicationGlobals,
            CancellationToken,
            CancellationTokenSource,
            ProgressTracker,
            Task<IQfcDatamodel>
        > QfcAsyncDataModelLoader { get; set; } =
            async (globals, cancel, cancelSource, progress) =>
                await QfcDatamodel.LoadAsync(globals, cancel, cancelSource, progress);

        internal Func<
            InitTypeEnum,
            IApplicationGlobals,
            IFilerHomeController,
            IQfcExplorerController
        > QfcExplorerControllerLoader { get; set; } =
            (initType, globals, homeController) =>
                new QfcExplorerController(initType, globals, homeController);

        internal Func<
            IQfcFormViewer,
            IFilerHomeController,
            IQfcKeyboardHandler
        > QfcKeyboardHandlerLoader { get; set; } =
            (formViewer, homeController) => new KeyboardHandler(formViewer, homeController);

        internal Func<
            CancellationToken,
            QfcHomeController,
            IApplicationGlobals,
            IQfcQueue
        > QfcQueueLoader { get; set; } =
            (token, homeController, globals) => new QfcQueue(token, homeController, globals);

        internal Func<
            IApplicationGlobals,
            IQfcFormViewer,
            IQfcQueue,
            InitTypeEnum,
            System.Action,
            QfcHomeController,
            CancellationTokenSource,
            CancellationToken,
            IQfcFormController
        > QfcFormControllerLoader { get; set; } =
            (
                globals,
                formViewer,
                qfcQueue,
                initType,
                cleanup,
                homeController,
                tokenSource,
                token
            ) =>
                new QfcFormController(
                    globals,
                    formViewer,
                    qfcQueue,
                    initType,
                    cleanup,
                    homeController,
                    tokenSource,
                    token
                ).Init();

        /// <summary>
        /// Injectable seam for the high-confidence pre-UI scoring/filter pass. Defaults to
        /// <see cref="QfcHighConfidencePreFilter.FilterAsync"/>; tests override it to assert the
        /// pre-filter is invoked (and ordered) without running live Outlook scoring.
        /// </summary>
        internal Func<
            IList<MailItem>,
            IApplicationGlobals,
            double,
            CancellationToken,
            Task<IList<QfcPreScoredItem>>
        > HighConfidencePreFilterLoader { get; set; } =
            (items, globals, threshold, token) =>
                QfcHighConfidencePreFilter.FilterAsync(items, globals, threshold, token);

        #endregion Constructors, Initializers, and Destructors

        public void Run()
        {
            bool highConfidenceModeEnabled = Globals?.QfSettings?.HighConfidenceModeEnabled == true;
            int itemsPerIteration = _formController.ItemsPerIteration;
            int initializationBatchSize = highConfidenceModeEnabled ? 0 : itemsPerIteration;

            IList<MailItem> listEmail = _datamodel.InitEmailQueue(
                initializationBatchSize,
                _formViewer.Worker
            );
            if (highConfidenceModeEnabled)
            {
                listEmail = _datamodel
                    .DequeueNextItemGroupAsync(itemsPerIteration, 1000)
                    .GetAwaiter()
                    .GetResult();
            }

            _formController.LoadItems(listEmail);
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();
        }

        public async Task RunAsync(ProgressTracker progress)
        {
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(QfcDatamodel.InitEmailQueueAsync)} ...");
            progress.Report(0, "Initializing Email Queue");

            bool highConfidenceModeEnabled = Globals?.QfSettings?.HighConfidenceModeEnabled == true;
            int itemsPerIteration = _formController.ItemsPerIteration;
            int initializationBatchSize = highConfidenceModeEnabled ? 0 : itemsPerIteration;

            IList<MailItem> listEmail = await Task.Run(async () =>
                await _datamodel.InitEmailQueueAsync(
                    initializationBatchSize,
                    _formViewer.Worker,
                    Token,
                    TokenSource
                )
            );

            if (highConfidenceModeEnabled)
            {
                listEmail = await _datamodel.DequeueNextItemGroupAsync(itemsPerIteration, 1000);
            }

            progress.Report(30, "Initializing Qfc Items");

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(QfcFormController.LoadItemsAsync)} ...");
            await _formController.LoadItemsAsync(listEmail);

            progress?.Report(100);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Showing and Refreshing {nameof(QfcFormViewer)} ...");
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            //_formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            //_formViewer.Show();
            //_formViewer.Refresh();
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} {nameof(QfcHomeController)}.{nameof(RunAsync)} is complete");

            //_ = IterateQueueAsync();
            await Task.Run(IterateQueueAsync);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.
                //MessageBox.Show("Operation was canceled");
                //logger.Debug($"{nameof(QfcDatamodel)} background worker cancelled");
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                MessageBox.Show(msg);
            }
            else
            {
                //logger.Debug("Background load of email database complete.");
                UiThread.Dispatcher.Invoke(() =>
                {
                    _formViewer.ItemsPerLoadEnabled = true;
                    _formViewer.SkipButtonEnabled = true;
                });
                //_ = IterateQueueAsync();
                WorkerComplete = true;
            }
        }

        private BlockingCollection<string> _metrics = new BlockingCollection<string>(
            new ConcurrentQueue<string>()
        );
        private int _metricsConsumers = 0;
        private static object _lockObject = new object();
        private static string _fileName;

        //private static string _folderPath;

        private async void TimedConsumerAsync(object source, ElapsedEventArgs e)
        {
            try
            {
                Interlocked.Decrement(ref _metricsConsumers);
                var strOutput = _metrics.GetConsumingEnumerable().ToArray();
                if (strOutput.Length > 0)
                {
                    if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
                    {
                        await FileIO2.WriteTextFileAsync(
                            Globals.FS.Filenames.EmailSession,
                            strOutput,
                            myDocuments,
                            default
                        );
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public void Cleanup()
        {
            _datamodel.Cleanup();
            Globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            ParentCleanup.Invoke();
        }

        private bool _loaded = false;
        public bool Loaded
        {
            get => _loaded;
            internal set => _loaded = value;
        }

        #region Public Properties

        private IQfcExplorerController _explorerController;
        public IQfcExplorerController ExplorerController
        {
            get => _explorerController;
            set => _explorerController = value;
        }

        private IQfcFormController _formController;
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

        private IQfcDatamodel _datamodel;
        public IQfcDatamodel DataModel
        {
            get => _datamodel;
            internal set => _datamodel = value;
        }

        public FilerQueue FilerQueue { get; } = new FilerQueue();

        private TaskScheduler _uiScheduler;
        internal TaskScheduler UiScheduler
        {
            get => _uiScheduler;
        }

        private Stopwatch _stopWatchMoved;
        private Stopwatch _stopWatch;
        public Stopwatch StopWatch
        {
            get => _stopWatch;
        }

        private IQfcFormViewer _formViewer;

        //public QfcFormViewer FormViewer { get => _formViewer; }

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

        private bool _workerComplete = false;
        public bool WorkerComplete
        {
            get => _workerComplete;
            private set => _workerComplete = value;
        }

        private SynchronizationContext _uiSyncContext;
        public SynchronizationContext UiSyncContext
        {
            get => _uiSyncContext;
        }

        #endregion
    }
}
