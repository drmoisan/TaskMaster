using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers
{
    [ExcludeFromCodeCoverage]
    public partial class QfcDatamodel : IQfcDatamodel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Constructors and Initializers

        private QfcDatamodel(IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
            _olApp = _globals.Ol.App;
            _activeExplorer = _olApp.ActiveExplorer();
            _globals.Ol.App.NewMailEx += Application_NewMailEx;
            RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;
        }

        public QfcDatamodel(IApplicationGlobals appGlobals, CancellationToken token)
        {
            _globals = appGlobals;
            _token = token;
            _olApp = _globals.Ol.App;
            _activeExplorer = _olApp.ActiveExplorer();
            _frame = InitDf(_activeExplorer);
            _globals.Ol.App.NewMailEx += Application_NewMailEx;
            RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;
        }

        public static async Task<QfcDatamodel> LoadAsync(
            IApplicationGlobals appGlobals,
            CancellationToken token,
            CancellationTokenSource tokenSource,
            ProgressTracker progress
        )
        {
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Creating new {nameof(QfcDatamodel)} ... ");
            progress.Report(0, "Initializing Data Model");

            var model = new QfcDatamodel(appGlobals);
            model.Token = token;
            model.TokenSource = tokenSource;

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(InitDfAsync)} ... ");
            await model
                .InitDfAsync(appGlobals.Ol.App.ActiveExplorer(), progress.Increment(2))
                .ConfigureAwait(false);
            return model;
        }

        public void Cleanup()
        {
            _tokenSource?.Cancel();
            _worker?.CancelAsync();

            // Issue #791: both dereferences are null-conditional because this method is reachable
            // with the fields already released — a second Cancel, or a Cancel after a partially
            // failed launch. Unguarded, the NullReferenceException aborted the teardown before the
            // ribbon release callback, leaving both ribbon buttons inert for the session.
            IApplicationGlobals globals = _globals;
            if (globals?.Ol?.App is not null)
            {
                globals.Ol.App.NewMailEx -= Application_NewMailEx;
            }

            _moveMonitor?.UnhookAll();
            _moveMonitor = null;
            _activeExplorer = null;
            _olApp = null;
            _globals = null;
            _frame = null;
            _masterQueue = null;
            //_blockingQueue = null;
            //_priorityQueue = null;
            //_queues = null;
            _worker = null;
        }

        #endregion Constructors and Initializers

        #region Private Variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        private IApplicationGlobals _globals;
        private Explorer _activeExplorer;
        private LockingLinkedList<MailItem> _masterQueue = [];

        // Deliberately one monitor instance per owner, not a shared singleton: EmailMoveMonitor.BeforeItemMove dispatches at most one action per MailItem via FirstOrDefault, and UnhookAll is instance-scoped and clears the whole hook list, so a shared instance would both drop sibling owners' actions and unhook them all on any one owner's teardown (issue #731 finding 1, issue #620).
        private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();
        private Outlook.Application _olApp;
        private Frame<int, string> _frame;
        private BackgroundWorker _worker;

        /// <summary>
        /// Injectable time/delay seam. Defaults to <see cref="TimeProvider.System"/> so production
        /// timing is unchanged; tests assign a fake provider to make async delays deterministic.
        /// </summary>
        internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        /// <summary>
        /// Injectable worker-body seam for <see cref="Worker_DoWork"/>. Defaulted (in the instance
        /// constructors, below) to the single-argument <see cref="LoadRemainingEmailsToQueueAsync(CancellationToken)"/>
        /// overload so production behavior is unchanged; tests assign an inert delegate so a started
        /// <see cref="BackgroundWorker"/> never reaches <see cref="MessageBox.Show(string)"/> or live
        /// Outlook COM. A property initializer cannot be used here: a method-group conversion that
        /// captures the instance method target is a "this" reference and C# forbids referencing
        /// instance members (including from within a nested lambda) in an instance field/property
        /// initializer (CS0236); the default is therefore assigned in each constructor instead. Test
        /// instances built via <see cref="System.Runtime.Serialization.FormatterServices.GetUninitializedObject"/>
        /// bypass constructors entirely, so this property remains <see langword="null"/> on such
        /// instances until a test assigns it explicitly, exactly as a property initializer would have
        /// behaved.
        /// </summary>
        internal Func<CancellationToken, Task<bool>> RemainingEmailLoader { get; set; }

        #endregion Private Variables

        #region Public Properties

        private bool _complete = false;
        public bool Complete
        {
            get => _complete;
            set => _complete = value;
        }

        public SloStack<IMovedMailInfo> MovedItems
        {
            get => _globals.AF.MovedMails;
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

        #endregion Public Properties

        #region BackgroundWorker

        public void SetupWorker(System.ComponentModel.BackgroundWorker worker)
        {
            worker.WorkerSupportsCancellation = true;

            _token.Register(() => worker.CancelAsync());
            worker.DoWork += new System.ComponentModel.DoWorkEventHandler(Worker_DoWork);
            //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
        }

        private async void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // Do not access the form's BackgroundWorker reference directly.
                // Instead, use the reference provided by the sender parameter.
                BackgroundWorker bw = sender as BackgroundWorker;

                // Extract the argument.
                //zxxint arg = (int)e.Argument;

                // Start the time-consuming operation.
                //e.Result = await LoadRemainingEmailsToQueueAsync(bw, _token);
                //e.Result = LoadRemainingEmailsToQueue(bw, _token);
                try
                {
                    // Issue #791: capture the loader task before awaiting it. This method is
                    // async void, so without a handle nothing downstream can observe the loader's
                    // completion, and the Cancel path nulled fields underneath a loader that was
                    // still producing. QuiesceLoaderAsync awaits exactly this task.
                    Task<bool> loaderTask = RemainingEmailLoader(_token);
                    _remainingLoadTask = loaderTask;
                    e.Result = await loaderTask;
                }
                finally
                {
                    // Issue #424: clear the producer-liveness flag exactly when the awaited loader
                    // completes — including on the throwing path. This method is async void, so
                    // BackgroundWorker.IsBusy has already gone false; this continuation is the only
                    // point that truthfully marks the end of production.
                    _remainingLoadActive = false;
                }

                // If the operation was canceled by the user,
                // set the DoWorkEventArgs.Cancel property to true.
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            catch (System.Exception ex)
            {
                logger.Error($"Error in Worker_DoWork {ex.Message}", ex);
            }
        }

        // This event handler demonstrates how to interpret
        // the outcome of the asynchronous operation implemented
        // in the DoWork event handler.
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.
                MessageBox.Show("Operation was canceled");
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                MessageBox.Show(msg);
            }
            else
            {
                // The operation completed normally.
                //string msg = String.Format("Result = {0}", e.Result);
                //MessageBox.Show(msg);
            }
        }

        #endregion BackgroundWorker

        #region Email Queue Initial Setup

        public IList<MailItem> InitEmailQueue(int batchSize, BackgroundWorker worker)
        {
            _worker = worker;

            // Issue #244: a zero (or negative) batch size must not attempt to slice-and-project an
            // empty range through GetRowsAs<IEmailSortInfo>(), which throws once the sliced frame's
            // column index is empty. Skip straight to the empty result while still starting the
            // background worker so remaining emails continue to stream into the master queue.
            if (batchSize <= 0)
            {
                SetupWorker(worker);
                // Issue #424: mark the producer live before starting it, so a dequeue that runs
                // before Worker_DoWork's first await cannot mistake an empty queue for exhaustion.
                _remainingLoadActive = true;
                worker.RunWorkerAsync();
                return new List<MailItem>();
            }

            // Extract first batch
            batchSize = batchSize < _frame.RowCount ? batchSize : _frame.RowCount;
            var firstIteration = _frame.GetRowsAt(Enumerable.Range(0, batchSize).ToArray());

            // Drop extracted range from source table
            _frame = _frame.GetRowsAt(
                Enumerable.Range(batchSize, _frame.RowCount - batchSize).ToArray()
            );

            // Cast Frame to array of IEmailInfo
            var rows = firstIteration.GetRowsAs<IEmailSortInfo>().Values.ToArray();

            //BUGFIX: StoreId ID is being converted to the literal string "byte[]" instead of the string equivalent of the byte array
            // Convert array of IEmailInfo to List<MailItem>
            var emailList = rows.Select(row =>
                    (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId)
                )
                .ToList();

            SetupWorker(worker);
            // Issue #424: see the zero-batch path above — the flag is the honest producer-liveness
            // signal and must be set before the worker starts.
            _remainingLoadActive = true;
            worker.RunWorkerAsync();

            return emailList;
        }

        public async Task<IList<MailItem>> InitEmailQueueAsync(
            int batchSize,
            BackgroundWorker worker,
            CancellationToken token,
            CancellationTokenSource tokenSource
        )
        {
            token.ThrowIfCancellationRequested();

            _token = token;
            _tokenSource = tokenSource;
            _worker = worker;

            var emailList = await Task.Run(() => InitEmailQueue(batchSize, worker), token);

            return emailList;
        }

        private async Task<bool> LoadRemainingEmailsToQueueAsync(CancellationToken cancel)
        {
            if ((_frame is null) || (_frame.RowCount == 0))
            {
                MessageBox.Show("Email Frame is empty");
                return false;
            }

            // Cast Frame to array of IEmailInfo
            var rows = await Task.Run(() => _frame.GetRowsAs<IEmailSortInfo>().Values.ToArray());

            foreach (var row in rows)
            {
                try
                {
                    cancel.ThrowIfCancellationRequested();
                    //var item = (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId);
                    var item = await Task.Run(
                        () => _olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId),
                        cancel
                    );
                    if (item is not null && item is MailItem mailItem)
                    {
                        await TryQueueRemainingMailItemAsync(mailItem, cancel);
                    }
                }
                catch (OperationCanceledException)
                {
                    //logger.Debug($"{nameof(LoadRemainingEmailsToQueue)} Task cancelled");
                    return false;
                }
                catch (System.Exception e)
                {
                    logger.Error(
                        $"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}"
                    );
                    throw e;
                }
                await Task.Yield();
            }
            return true;
        }

        private bool LoadRemainingEmailsToQueue(BackgroundWorker bw, CancellationToken token)
        {
            if ((_frame is null) || (_frame.RowCount == 0))
            {
                MessageBox.Show("Email Frame is empty");
                return false;
            }

            // Cast Frame to array of IEmailInfo
            var rows = _frame.GetRowsAs<IEmailSortInfo>().Values.ToArray();

            foreach (var row in rows)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    //var item = (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId);
                    var item = _olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId);
                    if (item is not null && item is MailItem mailItem)
                    {
                        _masterQueue.AddLast(mailItem);
                        _moveMonitor.HookItem(mailItem, (x) => _masterQueue.Remove(x));
                    }
                }
                catch (OperationCanceledException)
                {
                    //logger.Debug($"{nameof(LoadRemainingEmailsToQueue)} Task cancelled");
                    return false;
                }
                catch (System.Exception e)
                {
                    logger.Error(
                        $"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}"
                    );
                    throw e;
                }
            }
            return true;
        }

        private async Task<bool> LoadRemainingEmailsToQueueAsync(
            BackgroundWorker bw,
            CancellationToken token
        )
        {
            if ((_frame is null) || (_frame.RowCount == 0))
            {
                MessageBox.Show("Email Frame is empty");
                return false;
            }

            try
            {
                // ForEachAwaitWithCancellationAsync (System.Linq.Async) is obsolete (CS0618) per
                // the framework's migration guidance ("Use the language support for async foreach
                // instead"), but replacing it with `await foreach` here is a control-flow change
                // to a production async method, not an annotation-only edit. Suppressing narrowly
                // preserves the exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
                await _frame
                    .GetRowsAs<IEmailSortInfo>()
                    .Values.ToAsyncEnumerable()
                    .ForEachAwaitWithCancellationAsync(
                        async (row, token) =>
                            await Task.Run(
                                () =>
                                {
                                    token.ThrowIfCancellationRequested();
                                    var item = (MailItem)
                                        _olApp
                                            .GetNamespace("MAPI")
                                            .GetItemFromID(row.EntryId, row.StoreId);
                                    _masterQueue.AddLast(item);
                                    _moveMonitor.HookItem(item, (x) => _masterQueue.Remove(x));
                                },
                                token
                            ),
                        token
                    );
#pragma warning restore CS0618
                return true;
            }
            catch (TaskCanceledException)
            {
                //logger.Debug($"{nameof(LoadRemainingEmailsToQueueAsync)} Task cancelled");
                return false;
            }
        }

        #endregion Email Queue Initial Setup

        #region Linked List Locking


        #endregion Linked List Locking

        #region Event Handlers

        void Application_NewMailEx(string entryID)
        {
            //var item = _globals.Ol.App.Session.GetItemFromID(entryID, System.Reflection.Missing.Value);
            try
            {
                var item = _globals.Ol.App.Session.GetItemFromID(entryID) as MailItem;
                if (item is not null)
                {
                    _masterQueue.AddFirst(item);
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        #endregion Event Handlers
    }
}
