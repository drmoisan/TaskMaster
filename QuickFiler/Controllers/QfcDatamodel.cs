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

        public StackObjectCS<MailItem> MovedItems { get => _movedObjects; }

        public IList<MailItem> InitEmailQueueAsync(int batchSize, BackgroundWorker worker)
        {
            // Extract first batch
            var firstIteration = _frame.GetRowsAt(Enumerable.Range(0, batchSize).ToArray());

            // Drop extracted range from source table
            _frame = _frame.GetRowsAt(Enumerable.Range(batchSize,_frame.RowCount-batchSize).ToArray());
            
            // Cast Frame to array of IEmailInfo
            var rows = firstIteration.GetRowsAs<IEmailInfo>().Values.ToArray();

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
            var rows = _frame.GetRowsAs<IEmailInfo>().Values.ToArray();

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
            Frame<int, string> df = DfDeedle.GetEmailDataInView(activeExplorer);
            df = df.FilterRowsBy("MessageClass", "IPM.Note");            
            var topics = df.GetColumn<string>("Conversation").Values.Distinct().ToArray();

            
            var rows = topics.Select(topic =>
            {
                var dfConversation = df.FilterRowsBy("Conversation", topic);
                var maxSentOn = dfConversation.GetColumn<DateTime>("SentOn").Values.Max();
                var row = dfConversation.FilterRowsBy("SentOn", maxSentOn).Rows.FirstValue();
                //var dfDateIdx = dfConversation.IndexRows<DateTime>("SentOn", keepColumn: true);
                //var addr = dfDateIdx.RowIndex.Locate(maxSentOn);
                //var idx = (int)dfDateIdx.RowIndex.AddressOperations.OffsetOf(addr);
                //var row = dfConversation.Rows.GetAt(idx);
                return row;
            });

            var dfFiltered = Frame.FromRows(rows);
            var sorter = new EmailSorter(SortOptionsEnum.Default);

            var s1 = dfFiltered.GetColumn<DateTime>("SentOn");
            var s2 = dfFiltered.GetColumn<string>("Triage");
            var added = s1.ZipInner(s2).Select(t => sorter.GetSortKey(triage: t.Value.Item2, dateTime: t.Value.Item1));
            dfFiltered.AddColumn("NewKey", added);

            dfFiltered = dfFiltered.SortRows("NewKey");
            
            var df2 = dfFiltered;
            df2 = df2.IndexRowsWith(Enumerable.Range(0, dfFiltered.RowCount).Reverse());
            //dfFiltered.Print();
            //df2.Print();

            df2 = df2.SortRowsByKey();
            //dfFiltered.IndexRowsOrdinally();
            //var df2 = dfFiltered.RealignRows(dfFiltered.RowKeys.Reverse());
            //df2.Print();
            df2.DropColumn("NewKey");
            //df2.Print();
            //var df2 = dfFiltered.IndexRows<DateTime>("SentOn", false);
            //var df3 = df2.GroupRowsBy<string>("Triage");
            //var s1 = dfConversation.GetColumn<DateTime>("SentOn");
            //var s2 = dfConversation.GetColumn<string>("Triage");
            //var added = s1.ZipInner(s2).Select(t => t.Value.Item1.ToString("yyyyMMddhhmmss") + t.Value.Item2);
            //dfConversation.AddColumn("NewKey", added);

            //dfFiltered.Print();

            //var dfgroup = df.GroupRowsBy<string>("Conversation");

            //var dfgroup2 = df.Rows.GroupBy<string>(row => MultiKeyExtensions.Lookup1of2<Tuple<string, int>,int>(row.Key))
            //var keys = dfgroup.RowKeys;
            //var keysDistinct = keys.Distinct();
            //var convCol = dfgroup.GetColumn<string>("Conversation").Values.Distinct().ToArray();
            //var rows = convCol.Select(key => dfgroup.GetRows)
            //var rows = dfgroup.Rows[MultiKeyExtensions.Lookup1Of2<string, int>(convCol[0])];
            //var rows = dfgroup.Rows.GetByLevel(MultiKeyExtensions.Lookup1Of2<string, int>(convCol[0]));
            //var lookups = keysDistinct.Select(key => MultiKeyExtensions.Lookup1Of2<Tuple<string, int>, string>(key));
            //var keysDistinct = keys.Select(x => MultiKeyExtensions.Lookup1Of2<Tuple<string, int>, string>(x));

            //MultiKeyExtensions.Lookup1Of2<string, int>(keys)
            //dfgroup.Print();

            return df2;
        }
        
        internal (string[], object[,]) GetEmailDataInView(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            table.Columns.Add("SentOn");
            table.Columns.Add(OlTableExtensions.SchemaFolderName);
            table.Columns.Add(OlTableExtensions.SchemaTriage);

            string[] columnHeaders = table.GetColumnHeaders();
            object[,] data = (object[,])table.GetArray(table.GetRowCount());
            return (columnHeaders, data);
        }

        internal (string[], object[,]) GetEmailDataInView2(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            table.Columns.Add("SentOn");
            table.Columns.Add(OlTableExtensions.SchemaFolderName);
            table.Columns.Add(OlTableExtensions.SchemaTriage);

            string[] columnHeaders = table.GetColumnHeaders();
            object[,] data = (object[,])table.GetArray(table.GetRowCount());
            for (int i = 0; i < columnHeaders.Length; i++)
            {
                var header = columnHeaders[i];
                var columnData = data.SliceColumn(i);
            }
            
            return (columnHeaders, data);
        }

        public IList<MailItem> LoadEmailDataBase(Explorer activeExplorer, IList<MailItem> listEmailsToLoad)
        {
            Folder OlFolder;
            Outlook.View objCurView;
            string strFilter;
            Items OlItems;
            // TODO: Move this to Model Component of the MVC

            if (listEmailsToLoad is null)
            {
                OlFolder = (Folder)activeExplorer.CurrentFolder;
                objCurView = (Outlook.View)activeExplorer.CurrentView;
                strFilter = objCurView.Filter;
                if (!string.IsNullOrEmpty(strFilter))
                {
                    strFilter = "@SQL=" + strFilter;
                    OlItems = OlFolder.Items.Restrict(strFilter);
                }
                else
                {
                    OlItems = OlFolder.Items;
                }
                return MailItemsSort(OlItems, (SortOptionsEnum)((int)SortOptionsEnum.DateRecentFirst + (int)SortOptionsEnum.TriageImportantFirst + (int)SortOptionsEnum.ConversationUniqueOnly));
            }

            else
            {
                return (IList<MailItem>)listEmailsToLoad;
            }

        }

        public IList<MailItem> MailItemsSort(Items OlItems, SortOptionsEnum options)
        {
            string strFilter;
            string strFilter2;
            Folder OlFolder;
            
            Items OlItemsTmp;
            Items OlItemsRemainder;
            object objItem;
            MailItem OlMailTmp;
            MailItem OlMailTmp2;
            
            IList<MailItem> listEmails;
            var StrTriageOpts = new string[4];
            int i;
            int j;
            bool BlUniqueConv;
            var intFrom = default(int);
            var intTo = default(int);
            var intStep = default(int);
            var blTriage = default(bool);


            // Originally written to maintain filter of the view
            // Need to add in the option to eliminate the filter
            // for two cases: 1) If it is not called from the active view,
            // and 2) in the case that we want to see all emails anyway

            StrTriageOpts[1] = "A";
            StrTriageOpts[2] = "B";
            StrTriageOpts[3] = "C";

            listEmails = new List<MailItem>();
            

            if (options.HasFlag(SortOptionsEnum.DateRecentFirst))
            {
                OlItems.Sort("Received", true);
            }
            else if (options.HasFlag(SortOptionsEnum.DateOldestFirst))
            {
                OlItems.Sort("Received", false);
            }

            // Output_Items OlItems

            if (options.HasFlag(SortOptionsEnum.TriageImportantFirst))
            {
                blTriage = true;
                intFrom = 1;
                intTo = 3;
                intStep = 1;
            }
            else if (options.HasFlag(SortOptionsEnum.TriageImportantLast))
            {
                blTriage = true;
                intFrom = 3;
                intTo = 1;
                intStep = -1;
            }

            if (blTriage)
            {
                OlItemsRemainder = OlItems;
                var loopTo = intTo;
                for (i = intFrom; intStep >= 0 ? i <= loopTo : i >= loopTo; i += intStep)
                {
                    strFilter = "[Triage] = " + '"' + StrTriageOpts[i] + '"';
                    strFilter2 = "[Triage] <> " + '"' + StrTriageOpts[i] + '"';
                    OlItemsTmp = OlItems.Restrict(strFilter);
                    OlItemsRemainder = OlItemsRemainder.Restrict(strFilter2);


                    foreach (var currentObjItem in OlItemsTmp)
                    {
                        objItem = currentObjItem;
                        BlUniqueConv = true;
                        if (objItem is MailItem)
                        {
                            OlMailTmp = (MailItem)objItem;
                            if (!MailResolution_ToRemove.IsMailUnReadable(OlMailTmp))
                            {
                                if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
                                {
                                    var loopTo1 = listEmails.Count - 1;
                                    for (j = 0; j <= loopTo1; j++)
                                    {
                                        OlMailTmp2 = (MailItem)listEmails[j];
                                        if ((OlMailTmp.ConversationID ?? "") == (OlMailTmp2.ConversationID ?? ""))
                                        {
                                            BlUniqueConv = false;
                                        }
                                    }
                                } // Options And ConversationUniqueOnly Then

                                if (BlUniqueConv)
                                    listEmails.Add(OlMailTmp);

                            } // If IsMailUnReadable
                        } // If TypeOf Mail Is mailItem Then
                    } // For Each Mail In OlItemsTmp
                } // For i = 1 To 4

                foreach (var currentObjItem1 in OlItemsRemainder)
                {
                    objItem = currentObjItem1;
                    BlUniqueConv = true;
                    if (objItem is MailItem)
                    {
                        OlMailTmp = (MailItem)objItem;
                        if (!MailResolution_ToRemove.IsMailUnReadable(OlMailTmp))
                        {
                            if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
                            {
                                var loopTo2 = listEmails.Count - 1;
                                for (j = 0; j <= loopTo2; j++)
                                {
                                    OlMailTmp2 = (MailItem)listEmails[j];
                                    if ((OlMailTmp.ConversationID ?? "") == (OlMailTmp2.ConversationID ?? ""))
                                    {
                                        BlUniqueConv = false;
                                    }
                                }
                            } // Options And ConversationUniqueOnly Then

                            if (BlUniqueConv)
                                listEmails.Add(OlMailTmp);

                        } // If IsMailUnReadable
                    } // If TypeOf Mail Is mailItem Then
                } // For Each Mail In OlItemsRemainder
            }

            else
            {
                foreach (var currentObjItem2 in OlItems)
                {
                    objItem = currentObjItem2;
                    BlUniqueConv = true;
                    if (objItem is MailItem)
                    {
                        OlMailTmp = (MailItem)objItem;
                        if (!MailResolution_ToRemove.IsMailUnReadable(OlMailTmp))
                        {
                            if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
                            {
                                var loopTo3 = listEmails.Count;
                                for (j = 1; j <= loopTo3; j++)
                                {
                                    OlMailTmp2 = (MailItem)listEmails[j];
                                    if ((OlMailTmp.ConversationID ?? "") == (OlMailTmp2.ConversationID ?? ""))
                                    {
                                        BlUniqueConv = false;
                                    }
                                }
                            } // Options And ConversationUniqueOnly Then

                            if (BlUniqueConv)
                                listEmails.Add(OlMailTmp);
                        } // Not IsMailUnReadable(OlMailTmp) Then
                    } // If TypeOf Mail Is mailItem Then
                }
            }

            return listEmails;


            OlFolder = null;
            OlItems = null;
            OlItemsTmp = null;
            objItem = null;
            OlMailTmp = null;
            OlMailTmp2 = null;


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

    public interface IEmailInfo
    {
        string EntryId { get; }
        string MessageClass { get; }
        DateTime SentOn { get; }
        string Conversation { get; }
        string Triage { get; }
        string StoreId { get; }
    }

}
