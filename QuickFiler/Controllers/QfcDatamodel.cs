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
using UtilitiesCS.ReusableTypeClasses;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using static Deedle.FrameBuilder;
using log4net.Repository.Hierarchy;
//using static UtilitiesCS.OlItemSummary;



namespace QuickFiler.Controllers
{
    public class QfcDatamodel : IQfcDatamodel
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcDatamodel(IApplicationGlobals appGlobals) 
        { 
            _globals = appGlobals;
            _olApp = _globals.Ol.App;
            _activeExplorer = _olApp.ActiveExplorer();
        }

        public QfcDatamodel(IApplicationGlobals appGlobals, CancellationToken token) 
        { 
            _globals = appGlobals;
            _token = token;
            _olApp = _globals.Ol.App;
            _activeExplorer = _olApp.ActiveExplorer();
            _frame = InitDf(_activeExplorer);
        }

        public static async Task<QfcDatamodel> LoadAsync(IApplicationGlobals appGlobals, CancellationToken token, CancellationTokenSource tokenSource, ProgressTracker progress) 
        {
            logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Creating new {nameof(QfcDatamodel)} ... ");
            progress.Report(0, "Initializing Data Model");

            var model = new QfcDatamodel(appGlobals);
            model.Token = token;
            model.TokenSource = tokenSource;

            logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(InitDfAsync)} ... ");
            await model.InitDfAsync(appGlobals.Ol.App.ActiveExplorer(), progress.Increment(2)).ConfigureAwait(false);
            return model;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IApplicationGlobals _globals;
        private Explorer _activeExplorer;
        private ConcurrentQueue<MailItem> _masterQueue;
        private Outlook.Application _olApp;
        private Frame<int, string> _frame;
        private BackgroundWorker _worker;

        private bool _complete = false;
        public bool Complete { get => _complete; set => _complete = value; }
        
        public ScoStack<IMovedMailInfo> MovedItems { get => _globals.AF.MovedMails; }
        
        private CancellationToken _token;
        public CancellationToken Token { get => _token; set => _token = value; }
        
        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; set => _tokenSource = value; }

        public IList<MailItem> InitEmailQueue(int batchSize, BackgroundWorker worker)
        {
            _worker = worker;
            
            // Extract first batch
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

            var emailList = await Task.Factory.StartNew(() => InitEmailQueue(batchSize, worker), 
                                                        token,
                                                        TaskCreationOptions.LongRunning, TaskScheduler.Default);

            return emailList;
        }

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
            e.Result = LoadRemainingEmailsToQueue(bw);

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

        private bool LoadRemainingEmailsToQueue(BackgroundWorker bw)
        {
            if((_frame is null) || (_frame.RowCount == 0))
            {
                MessageBox.Show("Email Frame is empty");
                _masterQueue = new ConcurrentQueue<MailItem>();
                return false;
            }
            
            // Cast Frame to array of IEmailInfo
            var rows = _frame.GetRowsAs<IEmailSortInfo>().Values.ToArray();

            // Batch Process
            // Convert array of IEmailInfo to List<MailItem>
            var emailList = rows.Select(row => (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId)).ToList();

            //// Cast list to concurrent queue
            _masterQueue = new ConcurrentQueue<MailItem>(emailList);

            //_masterQueue = new ConcurrentQueue<MailItem>();

            rows.Select(row => (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId)).ForEach(item => _masterQueue.Enqueue(item));

            return true;
            
        }
                                
        public async Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut)
        {
            int i;
            CancellationTokenSource toSrc = new CancellationTokenSource();
            toSrc.CancelAfter(timeOut);
            CancellationToken toToken = toSrc.Token;

            IList<MailItem> listObjects = new List<MailItem>();

            // Wait for the queue to be sufficiently populated or terminate populating
            await WaitForQueue(quantity, toToken);

            toToken.ThrowIfCancellationRequested();
            _token.ThrowIfCancellationRequested();

            // Adjust quantity to the lesser of the queue size or the requested quantity
            int adjustedQuantity = quantity < _masterQueue.Count ? quantity : _masterQueue.Count;

            if (adjustedQuantity == 0) { Complete = true;}
            
            for (i = 1; i <= adjustedQuantity; i++)
            {
                if (_masterQueue.TryDequeue(out MailItem item))
                    listObjects.Add(item);
            }

            return listObjects;
        }

        internal async Task WaitForQueue(int quantity, CancellationToken toToken)
        {
            while (_worker.IsBusy && (_masterQueue is null || _masterQueue.Count < quantity))
            {
                toToken.ThrowIfCancellationRequested();
                await Task.Delay(200);
            }
        }

        //TODO: Implement UndoMove()
        public void UndoMove()
        {
            throw new NotImplementedException();
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
            logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Toggle offline mode");

            var offline = await ToggleOfflineMode(_globals.Ol.NamespaceMAPI.Offline);
            
            Frame<int, string> df = null;
                        
            logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(DfDeedle.GetEmailDataInViewAsync)} ... ");
            try
            {
                df = await DfDeedle.GetEmailDataInViewAsync(
                    activeExplorer, Token, TokenSource, progress.Increment(3).SpawnChild(78))
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                await ToggleOfflineMode(offline);
            }
            catch (System.Exception e)
            {
                await ToggleOfflineMode(offline);
                throw e;
            }

            if (df is not null)
            {
                // Restore online mode if it was previously so
                await ToggleOfflineMode(offline);

                logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Filtering df ... ");
                // Filter out non-email items
                df = df.FilterRowsBy("MessageClass", "IPM.Note");

                // Filter to the latest email in each conversation
                var dfFiltered = MostRecentByConversation(df);

                logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Sorting df ... ");
                // Sort by triage classification and then date
                _frame = SortTriageDate(dfFiltered);

                progress.Report(100);
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
                
    }

    internal class EmailSorter
    {
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
            if (_options.HasFlag(SortOptionsEnum.TriageImportantFirst) && _options.HasFlag(SortOptionsEnum.DateRecentFirst))
            {
                return (long)(100000000000000 * _triageImportantLast[triage]) + GetDateKey(dateTime); 
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
