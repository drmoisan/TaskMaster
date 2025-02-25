using Microsoft.Office.Interop.Outlook;
using static QuickFiler.QfEnums;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Timers;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("QuickFiler.Test")]
namespace QuickFiler.Controllers
{
    public class QfcHomeController : IQfcHomeController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors, Initializers, and Destructors

        private QfcHomeController() { }

        public QfcHomeController(IApplicationGlobals globals,
                                 System.Action parentCleanup)
        {
            Globals = globals;
            ParentCleanup = parentCleanup;
        }
        
        public static async Task<QfcHomeController> LaunchAsync(IApplicationGlobals appGlobals,
                                                                System.Action parentCleanup)
        {
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} {nameof(QfcHomeController)}.{nameof(LaunchAsync)} is beginning");

            // Establish a SynchronizationContext for the UI thread
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(
                    new WindowsFormsSynchronizationContext());

            // Create uninitialized instance of QfcHomeController
            var controller = new QfcHomeController();

            // Create cancellation token and progress tracker
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var progress = new ProgressTracker(tokenSource).Initialize();

            try
            {
                await controller.InitAsync(appGlobals, parentCleanup, tokenSource, token, progress.SpawnChild(86));
                controller.Loaded = true;

                await controller.RunAsync(progress.SpawnChild());

            }
            catch (OperationCanceledException)
            {
                logger.Info($"{DateTime.Now.ToString("mm:ss.fff")} " +
                    $"{nameof(QfcHomeController)}.{nameof(LaunchAsync)} was cancelled");
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
            _formController = QfcFormControllerLoader(Globals, _formViewer, QfcQueue, InitTypeEnum.Sort, Cleanup, this, this._tokenSource, this._token);
            return this;
        }

        internal async Task InitAsync(IApplicationGlobals appGlobals,
                                      System.Action parentCleanup,
                                      CancellationTokenSource tokenSource,
                                      CancellationToken token,
                                      ProgressTracker progress)
        {
            _token = token;
            _tokenSource = tokenSource;
            Globals = appGlobals;
            ParentCleanup = parentCleanup;

            // Load the data model in the background            
            var dataModelTask = QfcAsyncDataModelLoader(Globals, this.Token, this.TokenSource, progress);

            // Load all components Synchronously with minimal initialization
            _formViewer = new QfcFormViewer();
            _formViewer.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            _uiSyncContext = _formViewer.UiSyncContext;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            _explorerController = QfcExplorerControllerLoader(InitTypeEnum.Sort, Globals, this);
            _keyboardHandler = QfcKeyboardHandlerLoader(_formViewer, this);
            QfcQueue = QfcQueueLoader(this.Token, this, Globals);
            _formController = QfcFormControllerLoader(Globals, _formViewer, QfcQueue, InitTypeEnum.Sort, Cleanup, this, TokenSource, Token);

            // Wait for the data model to finish loading asynchronously
            _datamodel = await dataModelTask;
        }


        internal IApplicationGlobals Globals { get; set; }
        internal IQfcQueue QfcQueue { get; set; }
        internal System.Action ParentCleanup { get; set; }

        internal Func<IApplicationGlobals, CancellationToken, IQfcDatamodel> QfcDataModelLoader { get; set; }
            = (globals, cancel) => new QfcDatamodel(globals, cancel);

        internal Func<IApplicationGlobals, CancellationToken, CancellationTokenSource, ProgressTracker, Task<IQfcDatamodel>> QfcAsyncDataModelLoader { get; set; }
            = async (globals, cancel, cancelSource, progress) => await QfcDatamodel.LoadAsync(globals, cancel, cancelSource, progress);

        internal Func<InitTypeEnum, IApplicationGlobals, IFilerHomeController, IQfcExplorerController> QfcExplorerControllerLoader { get; set; }
            = (initType, globals, homeController) => new QfcExplorerController(initType, globals, homeController);

        internal Func<IQfcFormViewer, IFilerHomeController, IQfcKeyboardHandler> QfcKeyboardHandlerLoader { get; set; }
            = (formViewer, homeController) => new KeyboardHandler(formViewer, homeController);

        internal Func<CancellationToken, QfcHomeController, IApplicationGlobals, IQfcQueue> QfcQueueLoader { get; set; }
            = (token, homeController, globals) => new QfcQueue(token, homeController, globals);

        internal Func<IApplicationGlobals, IQfcFormViewer, IQfcQueue, InitTypeEnum, System.Action, QfcHomeController,
            CancellationTokenSource, CancellationToken, IQfcFormController> QfcFormControllerLoader
        { get; set; } =
            (globals, formViewer, qfcQueue, initType, cleanup, homeController, tokenSource, token) =>
            new QfcFormController(globals, formViewer, qfcQueue, initType, cleanup, homeController, tokenSource, token)
            .Init();

        #endregion Constructors, Initializers, and Destructors

        public void Run()
        {
            IList<MailItem> listEmail = _datamodel.InitEmailQueue(_formController.ItemsPerIteration, _formViewer.Worker);
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

            IList<MailItem> listEmail = await _datamodel.InitEmailQueueAsync(_formController.ItemsPerIteration, _formViewer.Worker, Token, TokenSource);

            progress.Report(30, "Initializing Qfc Items");

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(QfcFormController.LoadItemsAsync)} ...");
            await _formController.LoadItemsAsync(listEmail);

            progress.Report(100);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Showing and Refreshing {nameof(QfcFormViewer)} ...");
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} {nameof(QfcHomeController)}.{nameof(RunAsync)} is complete");

            //_ = IterateQueueAsync();
            await IterateQueueAsync();
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
                    _formViewer.L1v1L2h5_SpnEmailPerLoad.Enabled = true;
                    _formViewer.L1v1L2h5_BtnSkip.Enabled = true;
                });
                //_ = IterateQueueAsync();
                WorkerComplete = true;
            }
        }

        public async Task IterateQueueAsync()
        {
            Token.ThrowIfCancellationRequested();

            if (_datamodel.Complete) { return; }
            try
            {
                var listObjects = await _datamodel.DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000);
                if (listObjects.Count > 0)
                {
                    await QfcQueue.EnqueueAsync(listObjects, _formController.Groups).ConfigureAwait(false);
                }
                else
                {
                    //logger.Debug($"{nameof(IterateQueueAsync)} completed");
                    await QfcQueue.CompleteAddingAsync(Token, 10000);
                }
            }
            catch (OperationCanceledException)
            {
                //logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
            }
            catch (System.Exception)
            {
                if (this.Token.IsCancellationRequested)
                {
                    //logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
                }
                else
                {
                    throw;
                }
            }

        }

        public void Iterate()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            IList<MailItem> listObjects = _datamodel.DequeueNextItemGroup(_formController.ItemsPerIteration);
            _formController.LoadItems(listObjects);
        }

        public void Iterate2()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            (var tlp, var itemGroups) = QfcQueue.Dequeue();
            _formController.LoadItems(tlp, itemGroups);
            _ = IterateQueueAsync();
        }

        public void SwapStopWatch()
        {
            _stopWatchMoved = _stopWatch;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public void QuickFileMetrics_WRITE(string filename)
        {

            string LOC_TXT_FILE;
            string curDateText, curTimeText, durationText, durationMinutesText;
            double Duration;
            string dataLineBeg;
            DateTime OlEndTime;
            DateTime OlStartTime;
            AppointmentItem OlAppointment;
            Folder OlEmailCalendar;

            // Create a line of comma seperated valued to store data
            curDateText = DateTime.Now.ToString("MM/dd/yyyy");

            curTimeText = DateTime.Now.ToString("hh:mm");

            dataLineBeg = curDateText + "," + curTimeText + ",";

            if (!Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var folderRoot))
            {
                logger.Debug($"{nameof(QuickFileMetrics_WRITE)} aborted due to lack of MyDocuments location");
                return;
            }
            LOC_TXT_FILE = Path.Combine(folderRoot, filename);

            Duration = _stopWatchMoved.Elapsed.Seconds;
            OlEndTime = DateTime.Now;
            OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                Duration /= emailsLoaded;
            }

            durationText = Duration.ToString("##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (Duration / 60d).ToString("##0.00");

            OlEmailCalendar = UtilitiesCS.Calendar.GetCalendar("Email Time", Globals.Ol.App.Session);
            OlAppointment = (AppointmentItem)OlEmailCalendar.Items.Add();
            {
                OlAppointment.Subject = $"Quick Filed {emailsLoaded} emails";
                OlAppointment.Start = OlStartTime;
                OlAppointment.End = OlEndTime;
                OlAppointment.Categories = "@ Email";
                OlAppointment.ReminderSet = false;
                OlAppointment.Sensitivity = OlSensitivity.olPrivate;
                OlAppointment.Save();
            }


            string[] strOutput = _formController.Groups
                .GetMoveDiagnostics(durationText, durationMinutesText, Duration,
                dataLineBeg, OlEndTime, ref OlAppointment);

            if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                FileIO2.WriteTextFile(filename, strOutput, myDocuments);
            }
        }

        public async Task WriteMetricsAsync(string filename)
        {
            //TraceUtility.LogMethodCall(filename);

            string LOC_TXT_FILE;
            string curDateText, curTimeText, durationText, durationMinutesText;
            double Duration;
            string dataLineBeg;
            DateTime OlEndTime;
            DateTime OlStartTime;
            AppointmentItem OlAppointment;
            Folder OlEmailCalendar;

            // Create a line of comma seperated valued to store data
            curDateText = DateTime.Now.ToString("MM/dd/yyyy");

            curTimeText = DateTime.Now.ToString("hh:mm");

            dataLineBeg = curDateText + "," + curTimeText + ",";

            if (!Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments)) { return; }
            LOC_TXT_FILE = Path.Combine(myDocuments, filename);

            //Duration = _stopWatchMoved.Elapsed.Seconds;
            Duration = StopWatch.Elapsed.Seconds;
            OlEndTime = DateTime.Now;
            OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                Duration /= emailsLoaded;
            }

            durationText = Duration.ToString("##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (Duration / 60d).ToString("##0.00");
            WriteMoveToCalendar(OlEndTime, OlStartTime, emailsLoaded, out OlAppointment, out OlEmailCalendar);

            string[] strOutput = _formController.Groups
                .GetMoveDiagnostics(durationText, durationMinutesText, Duration,
                dataLineBeg, OlEndTime, ref OlAppointment);

            _fileName = filename;
            await NonBlockingProducer(strOutput, Token);
        }

        private void WriteMoveToCalendar(DateTime OlEndTime, DateTime OlStartTime, int emailsLoaded, out AppointmentItem OlAppointment, out Folder OlEmailCalendar)
        {
            //TraceUtility.LogMethodCall(OlEndTime, OlStartTime, emailsLoaded);

            OlEmailCalendar = UtilitiesCS.Calendar.GetCalendar("Email Time", Globals.Ol.App.Session);
            OlAppointment = (AppointmentItem)OlEmailCalendar.Items.Add();
            {
                OlAppointment.Subject = $"Quick Filed {emailsLoaded} emails";
                OlAppointment.Start = OlStartTime;
                OlAppointment.End = OlEndTime;
                OlAppointment.Categories = "@ Email";
                OlAppointment.ReminderSet = false;
                OlAppointment.Sensitivity = OlSensitivity.olPrivate;
                OlAppointment.Save();
            }
        }

        private BlockingCollection<string> _metrics = new BlockingCollection<string>(new ConcurrentQueue<string>());
        private int _metricsConsumers = 0;
        private static object _lockObject = new object();
        private static string _fileName;
        //private static string _folderPath;

        private async Task NonBlockingProducer(string[] lines, CancellationToken ct)
        {
            //TraceUtility.LogMethodCall(lines, ct);

            foreach (string line in lines)
            {
                ct.ThrowIfCancellationRequested();
                await NonBlockingProducer(line, ct);
            }
        }

        private async Task NonBlockingProducer(string line, CancellationToken ct)
        {
            bool success = false;

            do
            {
                // Cancellation causes OCE. We know how to handle it.
                try
                {
                    // A shorter timeout causes more failures.
                    success = _metrics.TryAdd(line, 20, ct);
                }
                catch (OperationCanceledException)
                {
                    if (ct.IsCancellationRequested) { break; }
                    else
                    {
                        //logger.Debug($"Timeout adding {line}");
                        await Task.Delay(20);
                    }
                }
            } while (!success);
            if (Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2)
            {
                Interlocked.Decrement(ref _metricsConsumers);
                var timer = new System.Timers.Timer(2000);
                timer.Elapsed += TimedConsumerAsync;
            }

        }

        private async void TimedConsumerAsync(object source, ElapsedEventArgs e)
        {
            try
            {
                Interlocked.Decrement(ref _metricsConsumers);
                var strOutput = _metrics.GetConsumingEnumerable().ToArray();
                if (strOutput.Length > 0)
                {
                    if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
                    { await FileIO2.WriteTextFileAsync(Globals.FS.Filenames.EmailSession, strOutput, myDocuments, default); }
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
        public bool Loaded { get => _loaded; internal set => _loaded = value; }

        #region Public Properties

        private IQfcExplorerController _explorerController;
        public IQfcExplorerController ExplorerController { get => _explorerController; set => _explorerController = value; }

        private IQfcFormController _formController;
        public IFilerFormController FormController { get => _formController; }

        private IQfcKeyboardHandler _keyboardHandler;
        public IQfcKeyboardHandler KeyboardHandler { get => _keyboardHandler; set => _keyboardHandler = value; }

        private IQfcDatamodel _datamodel;
        public IQfcDatamodel DataModel { get => _datamodel; internal set => _datamodel = value; }

        public FilerQueue FilerQueue { get; } = new FilerQueue();

        private TaskScheduler _uiScheduler;
        internal TaskScheduler UiScheduler { get => _uiScheduler; }

        private Stopwatch _stopWatchMoved;
        private Stopwatch _stopWatch;
        public Stopwatch StopWatch { get => _stopWatch; }

        private IQfcFormViewer _formViewer;
        //public QfcFormViewer FormViewer { get => _formViewer; }

        internal void CreateCancellationToken()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }
        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; }

        private CancellationToken _token;
        public CancellationToken Token { get => _token; }

        private bool _workerComplete = false;
        public bool WorkerComplete { get => _workerComplete; private set => _workerComplete = value; }

        private SynchronizationContext _uiSyncContext;
        public SynchronizationContext UiSyncContext { get => _uiSyncContext; }

        #endregion

    }
}
