using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
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
//using static UtilitiesCS.OlItemSummary;



namespace QuickFiler.Controllers
{
    public class QfcDatamodel : IQfcDatamodel
    {
        public QfcDatamodel(Explorer ActiveExplorer, Outlook.Application OlApp) 
        { 
            _activeExplorer = ActiveExplorer;
            _olApp = OlApp;
            _frame = InitDf(_activeExplorer);
            //_masterQueue = new Queue<MailItem>(listEmailsInFolder);            
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Explorer _activeExplorer;
        private Queue<MailItem> _masterQueue;
        private StackObjectCS<MailItem> _movedObjects;
        private Outlook.Application _olApp;
        private Frame<int, string> _frame;

        public StackObjectCS<MailItem> MovedItems 
        {
            get 
            {
                if (_movedObjects is null)
                {
                    _movedObjects = new StackObjectCS<MailItem>();
                }
                return _movedObjects; 
            }
        }

        public IList<MailItem> InitEmailQueueAsync(int batchSize, BackgroundWorker worker)
        {
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

        public void SetupWorker(System.ComponentModel.BackgroundWorker worker) 
        {
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new System.ComponentModel.DoWorkEventHandler(Worker_DoWork);
            worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
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
                _masterQueue = new Queue<MailItem>();
                return false;
            }
            //try 
            //{
            // Cast Frame to array of IEmailInfo
            var rows = _frame.GetRowsAs<IEmailSortInfo>().Values.ToArray();

            // Convert array of IEmailInfo to List<MailItem>
            var emailList = rows.Select(row => (MailItem)_olApp.GetNamespace("MAPI").GetItemFromID(row.EntryId, row.StoreId)).ToList();

            // Cast list to queue
            _masterQueue = new Queue<MailItem>(emailList);

            return true;
            //}
            //catch (System.Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //    _masterQueue = new Queue<MailItem>();
            //    return false;
            //}
        }
                                
        public IList<MailItem> DequeueNextItemGroup(int quantity)
        {
            int i;
            IList<MailItem> listObjects = new List<MailItem>();
            int adjustedQuantity = quantity < _masterQueue.Count ? quantity : _masterQueue.Count;
            for (i = 1; i <= adjustedQuantity; i++)
                listObjects.Add(_masterQueue.Dequeue());
            return listObjects;
        }

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
        private int _sortCode = -1;
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
