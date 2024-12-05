using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using Deedle;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using QuickFiler.Helper_Classes;




namespace QuickFiler.Controllers
{
    public class QfcDatamodel : IQfcDatamodel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        public static async Task<QfcDatamodel> LoadAsync(IApplicationGlobals appGlobals, CancellationToken token, CancellationTokenSource tokenSource, ProgressTracker progress) 
        {
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Creating new {nameof(QfcDatamodel)} ... ");
            progress.Report(0, "Initializing Data Model");

            var model = new QfcDatamodel(appGlobals);
            model.Token = token;
            model.TokenSource = tokenSource;

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(InitDfAsync)} ... ");
            await model.InitDfAsync(appGlobals.Ol.App.ActiveExplorer(), progress.Increment(2)).ConfigureAwait(false);
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

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IApplicationGlobals _globals;
        private Explorer _activeExplorer;
        private LockingLinkedList<MailItem> _masterQueue = [];
        private EmailMoveMonitor _moveMonitor = new();
        private Outlook.Application _olApp;
        private Frame<int, string> _frame;
        private BackgroundWorker _worker;

        #endregion Private Variables

        #region Public Properties

        private bool _complete = false;
        public bool Complete { get => _complete; set => _complete = value; }
        
        public ScoStack<IMovedMailInfo> MovedItems { get => _globals.AF.MovedMails; }
        
        private CancellationToken _token;
        public CancellationToken Token { get => _token; set => _token = value; }
        
        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        #endregion Public Properties

        #region BackgroundWorker

        public void SetupWorker(System.ComponentModel.BackgroundWorker worker) 
        {
            worker.WorkerSupportsCancellation = true;
            _token.Register(() => worker.CancelAsync());
            worker.DoWork += new System.ComponentModel.DoWorkEventHandler(Worker_DoWork);
            //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            // Extract the argument.
            //zxxint arg = (int)e.Argument;

            // Start the time-consuming operation.
            //e.Result = await LoadRemainingEmailsToQueueAsync(bw, _token);
            e.Result = LoadRemainingEmailsToQueue(bw, _token);

            // If the operation was canceled by the user,
            // set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
            {
                e.Cancel = true;
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
            _frame = _frame.GetRowsAt(Enumerable.Range(batchSize,_frame.RowCount-batchSize).ToArray());
            
            // Cast Frame to array of IEmailInfo
            var rows = firstIteration.GetRowsAs<IEmailSortInfo>().Values.ToArray();

            //BUGFIX: StoreId ID is being converted to the literal string "byte[]" instead of the string equivalent of the byte array
            // Convert array of IEmailInfo to List<MailItem>
            var emailList = rows.Select(row => (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId)).ToList();

            SetupWorker(worker);
            worker.RunWorkerAsync();

            return emailList;
        }

        public async Task<IList<MailItem>> InitEmailQueueAsync(int batchSize,
                                                               BackgroundWorker worker,
                                                               CancellationToken token,
                                                               CancellationTokenSource tokenSource)
        {
            token.ThrowIfCancellationRequested();

            _token = token;
            _tokenSource = tokenSource;
            _worker = worker;

            var emailList = await Task.Run(() => InitEmailQueue(batchSize, worker), token);

            return emailList;
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
                    logger.Error($"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}");
                    throw e;
                }
            }
            return true;
            
        }

        private async Task<bool> LoadRemainingEmailsToQueueAsync(BackgroundWorker bw, CancellationToken token)
        {
            if ((_frame is null) || (_frame.RowCount == 0))
            {
                MessageBox.Show("Email Frame is empty");
                return false;
            }

            try
            {
                await _frame.GetRowsAs<IEmailSortInfo>().Values.ToAsyncEnumerable().ForEachAwaitWithCancellationAsync(
                    async (row, token) => await Task.Run(() =>
                    {
                        token.ThrowIfCancellationRequested();    
                        var item = (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId);
                        _masterQueue.AddLast(item);
                        _moveMonitor.HookItem(item, (x) => _masterQueue.Remove(x));
                    }, token),
                    token);
                return true;
            }
            catch (TaskCanceledException)
            {
                //logger.Debug($"{nameof(LoadRemainingEmailsToQueueAsync)} Task cancelled");
                return false;
            }
            

            

            

        }

        public Frame<int, string> InitDf(Explorer activeExplorer)
        {
            var df = DfDeedle.GetEmailDataInView(activeExplorer);

            // Filter out non-email items
            df = df.FilterRowsBy("MessageClass", "IPM.Note");
            //df.Display(new List<string> { "RowKey" });
            // Filter to the latest email in each conversation
            var dfFiltered = MostRecentByConversation(df);
            
            // Sort by triage classification and then date
            var dfSorted = SortTriageDate(dfFiltered);

            return dfSorted;
            
        }

        /// <summary>
        /// If Outlook is not in offline mode, save the state and toggle it to offline mode
        /// </summary>
        /// <param name="offline"></param>
        /// <returns></returns>
        private async Task<bool> ToggleOfflineMode(bool offline)
        {
            if (!offline)
            {
                var commandBars = _activeExplorer.CommandBars;
                if (!offline) { commandBars.ExecuteMso("ToggleOnline"); }
                await Task.Delay(5);
            }
            return offline;
        }

        public async Task InitDfAsync(Explorer activeExplorer, ProgressTracker progress)
        {
            
            var df = await GetEmailsInViewDfAsync(activeExplorer, progress).ConfigureAwait(false);

            if (df is not null)
            {
                //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Filtering df ... ");
                // Filter out non-email items
                df = df.FilterRowsBy("MessageClass", "IPM.Note");

                // Filter to the latest email in each conversation
                var dfFiltered = MostRecentByConversation(df);

                //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Sorting df ... ");
                // Sort by triage classification and then date
                _frame = SortTriageDate(dfFiltered);

                progress.Report(100);
            }
        }

        private async Task<Frame<int, string>> GetEmailsInViewDfAsync(Explorer activeExplorer, ProgressTracker progress)
        {
            Frame<int, string> df = null;

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Toggle offline mode");
            var offline = await ToggleOfflineMode(_globals.Ol.NamespaceMAPI.Offline);
            
            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(DfDeedle.GetEmailDataInViewAsync)} ... ");
            try
            {
                df = await DfDeedle.GetEmailDataInViewAsync(
                    activeExplorer, Token, TokenSource, progress.Increment(3).SpawnChild(78))
                    .ConfigureAwait(false);
                await ToggleOfflineMode(offline);
                
                //df.DisplayDialog();
                
                return df;
            }
            catch (TaskCanceledException)
            {
                //logger.Debug($"{nameof(DfDeedle.GetEmailDataInViewAsync)} Task cancelled");
                await ToggleOfflineMode(offline);
                return null;
            }
            catch (System.Exception e)
            {
                await ToggleOfflineMode(offline);
                logger.Error($"{nameof(DfDeedle.GetEmailDataInViewAsync)} Error. \n {e.Message}\n{e.StackTrace}");
                throw e;
            }

        }

        public Frame<int, string> SortTriageDate(Frame<int, string> df)
        {
            var sorter = new EmailSorter(SortOptionsEnum.Default);

            var dfClone = df.Clone();

            var s1 = dfClone.GetColumn<DateTime>("SentOn");
            var s2 = dfClone.GetColumn<string>("Triage");
            var added = s1.ZipInner(s2).Select(t => sorter.GetSortKey(triage: t.Value.Item2, dateTime: t.Value.Item1));
            dfClone.AddColumn("NewKey", added);

            dfClone = dfClone.SortRows("NewKey");

            var dfSorted = dfClone.IndexRowsWith(Enumerable.Range(0, dfClone.RowCount).Reverse());

            dfSorted = dfSorted.SortRowsByKey();

            dfSorted.DropColumn("NewKey");
            return dfSorted;
        }

        public Frame<int, string> MostRecentByConversation(Frame<int, string> df)
        {
            var topics = df.GetColumn<string>("ConversationId").Values.Distinct().ToArray();

            var rows = topics.Select(topic =>
            {
                var dfConversation = df.FilterRowsBy("ConversationId", topic);
                var maxSentOn = dfConversation.GetColumn<DateTime>("SentOn").Values.Max();
                var row = dfConversation.FilterRowsBy("SentOn", maxSentOn).Rows.FirstValue();
                //var dfDateIdx = dfConversation.IndexRows<DateTime>("SentOn", keepColumn: true);
                //var addr = dfDateIdx.RowIndex.Locate(maxSentOn);
                //var idx = (int)dfDateIdx.RowIndex.AddressOperations.OffsetOf(addr);
                //var row = dfConversation.Rows.GetAt(idx);
                return row;
            });

            var dfFiltered = Frame.FromRows(rows);
            return dfFiltered;
        }

        #endregion Email Queue Initial Setup

        #region Email Queue Processing

        //TODO: Implement UndoMove()
        public void UndoMove()
        {
            throw new NotImplementedException();
        }

        public async Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut)
        {
            _token.ThrowIfCancellationRequested();

            if (_masterQueue.Count < quantity)
                await WaitForQueue(quantity, _token);

            var nodes = _masterQueue.TryTakeFirst(quantity)?.ToList();
            try
            {
                if (nodes is not null)
                {
                    await Task.Run(async () => 
                    {
                        foreach (var node in nodes)
                        {
                            await _moveMonitor.UnhookItemAsync(node, _token);
                        }
                    }, _token);                    
                }
                
            }
            catch (System.Exception e)
            {
                logger.Error("Error unhooking items from move monitor", e);
                throw;
            }
            
            
            return nodes;
        }

        internal async Task WaitForQueue(int quantity, CancellationToken token)
        {
            while (_worker.IsBusy && (_masterQueue?.Count < quantity))
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(200);
            }
        }

        #endregion Email Queue Processing

        #region Linked List Locking



        #endregion Linked List Locking

        #region Event Handlers

        void Application_NewMailEx(string entryID)
        {
            //var item = _globals.Ol.App.Session.GetItemFromID(entryID, System.Reflection.Missing.Value);
            try
            {
                var item = _globals.Ol.App.Session.GetItemFromID(entryID) as MailItem;
                if (item is not null) { _masterQueue.AddFirst(item); }
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
            
            
        }

        #endregion Event Handlers
    }

    internal class EmailSorter
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EmailSorter() { }
        public EmailSorter(SortOptionsEnum options) { _options = options; }

        private SortOptionsEnum _options = SortOptionsEnum.Default;
        private Dictionary<string, int> _triageImportantFirst = new Dictionary<string, int>
        {
            { "A", 1 },
            { "B", 2 },
            { "C", 3 },
            { "Z", 4 }
        };

        private Dictionary<string, int> _triageImportantLast = new Dictionary<string, int>
        {
            { "A", 4 },
            { "B", 3 },
            { "C", 2 },
            { "Z", 1 }
        };

        public SortOptionsEnum Options { get => _options; set => _options = value; }

        public long GetSortKey(string triage, DateTime dateTime)
        {
            if (_options.HasFlag(SortOptionsEnum.TriageImportantFirst) && 
                _options.HasFlag(SortOptionsEnum.DateRecentFirst))
            {
                try
                {
                    var triageKey = (long)(100000000000000 * _triageImportantLast[triage]) 
                        + GetDateKey(dateTime);
                    return triageKey;
                }
                catch (KeyNotFoundException e)
                {
                    logger.Error($"Triage value {triage} not found in " +
                        $"dictionary from date {GetDateKey(dateTime)} " +
                        $"\n {e.Message} \n {e.StackTrace}");
                    throw;
                }
            }
            return -1;
        }

        public long GetDateKey(DateTime dateTime) 
        { 
            return long.Parse(dateTime.ToString("yyyyMMddHHmmss")); 
        }
    }

    public interface IEmailSortInfo
    {
        string EntryId { get; }
        string MessageClass { get; }
        DateTime SentOn { get; }
        string ConversationId { get; }
        string Triage { get; }
        string StoreId { get; }
    }

}
