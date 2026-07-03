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
        }

        public QfcDatamodel(IApplicationGlobals appGlobals, CancellationToken token)
        {
            _globals = appGlobals;
            _token = token;
            _olApp = _globals.Ol.App;
            _activeExplorer = _olApp.ActiveExplorer();
            _frame = InitDf(_activeExplorer);
            _globals.Ol.App.NewMailEx += Application_NewMailEx;
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
            _globals.Ol.App.NewMailEx -= Application_NewMailEx;
            _moveMonitor.UnhookAll();
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
        private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();
        private Outlook.Application _olApp;
        private Frame<int, string> _frame;
        private BackgroundWorker _worker;

        /// <summary>
        /// Injectable time/delay seam. Defaults to <see cref="TimeProvider.System"/> so production
        /// timing is unchanged; tests assign a fake provider to make async delays deterministic.
        /// </summary>
        internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        #endregion Private Variables

        #region Public Properties

        private bool _complete = false;
        public bool Complete
        {
            get => _complete;
            set => _complete = value;
        }

        public ScoStack<IMovedMailInfo> MovedItems
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
                e.Result = await LoadRemainingEmailsToQueueAsync(_token);

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

        internal async Task<bool> TryQueueRemainingMailItemAsync(
            MailItem mailItem,
            CancellationToken cancel
        )
        {
            var admission = new QfcRemainingQueueAdmission(
                _globals,
                ScoreRemainingQueueMailItemAsync,
                _masterQueue.AddLast,
                _moveMonitor.HookItem,
                x => _masterQueue.Remove(x)
            );
            return await admission.TryQueueAsync(mailItem, cancel).ConfigureAwait(false);
        }

        private async Task<long> ScoreRemainingQueueMailItemAsync(
            MailItem mailItem,
            CancellationToken cancel
        )
        {
            var scoringService = new FolderScoringService();
            var score = await scoringService
                .ScoreAsync(mailItem, _globals, cancel)
                .ConfigureAwait(false);
            logger.Debug(
                $"Probability debug [QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)] "
                    + $"Subject='{mailItem.Subject}' EntryID='{mailItem.EntryID}' Score={score.Score}"
            );
            return score.Score;
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
