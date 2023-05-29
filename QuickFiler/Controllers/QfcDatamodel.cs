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
using UtilitiesVB;
using UtilitiesCS;
using Deedle;
using UtilitiesCS.ReusableTypeClasses;
//using static UtilitiesCS.OlItemSummary;



namespace QuickFiler.Controllers
{
    public class QfcDatamodel : IQfcDatamodel
    {
        public QfcDatamodel(Explorer ActiveExplorer, Application OlApp) 
        { 
            _activeExplorer = ActiveExplorer;
            _olApp = OlApp;
            var listEmailsInFolder = LoadEmailDataBase(_activeExplorer);
            _masterQueue = new Queue<MailItem>(listEmailsInFolder);            
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Explorer _activeExplorer;
        private Queue<MailItem> _masterQueue;
        private StackObjectCS<MailItem> _movedObjects;
        private Application _olApp;

        public StackObjectCS<MailItem> StackMovedItems { get => _movedObjects; set => _movedObjects = value; }

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

        public bool MoveItems(ref StackObjectCS<MailItem> StackMovedItems)
        {
            throw new NotImplementedException();
        }

                
        public IList<MailItem> LoadEmailDataBase(Explorer activeExplorer) 
        {
            DebugTextWriter tw = new DebugTextWriter();
            Console.SetOut(tw);
            Frame<int, string> df = DataFrameExtensionsDeedle.GetEmailDataInView(activeExplorer);
            df = df.FilterRowsBy("MessageClass", "IPM.Note");
            
            var topics = df.GetColumn<string>("Conversation").Values.Distinct().ToArray();
            var rows = topics.Select(topic =>
            {
                var dfConversation = df.FilterRowsBy("Conversation", topic);
                var maxSentOn = dfConversation.GetColumn<DateTime>("SentOn").Values.Max();
                var dfDateIdx = dfConversation.IndexRows<DateTime>("SentOn", keepColumn: true);
                var addr = dfDateIdx.RowIndex.Locate(maxSentOn);
                var idx = (int)dfDateIdx.RowIndex.AddressOperations.OffsetOf(addr);
                var row = dfConversation.Rows.GetAt(idx);
                return row;
            });

            var dfFiltered = Frame.FromRows(rows);

            var df2 = dfFiltered.IndexRows<DateTime>("SentOn", false);
            var df3 = df2.GroupRowsBy<string>("Triage");
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

            return new List<MailItem>();
        }
        public void DeedleDoodles()
        {

            DebugTextWriter tw = new DebugTextWriter();
            Console.SetOut(tw);
            // Create a collection of anonymous types
            var rnd = new Random();
            var objects = Enumerable.Range(0, 10).Select(i =>
              new { Key = "ID_" + i.ToString(), Number = rnd.Next() });

            // Create data frame with properties as column names
            var dfObjects = Frame.FromRecords(objects);
            dfObjects.Print();

        }

        internal (string[], object[,]) GetEmailDataInView(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            table.Columns.Add("SentOn");
            table.Columns.Add(ConvHelper.SchemaFolderName);
            table.Columns.Add(ConvHelper.SchemaTriage);

            string[] columnHeaders = table.GetColumnHeaders();
            object[,] data = table.GetArray(table.GetRowCount());
            return (columnHeaders, data);
        }

        internal (string[], object[,]) GetEmailDataInView2(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            table.Columns.Add("SentOn");
            table.Columns.Add(ConvHelper.SchemaFolderName);
            table.Columns.Add(ConvHelper.SchemaTriage);

            string[] columnHeaders = table.GetColumnHeaders();
            object[,] data = table.GetArray(table.GetRowCount());
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
            View objCurView;
            string strFilter;
            Items OlItems;
            // TODO: Move this to Model Component of the MVC

            if (listEmailsToLoad is null)
            {
                OlFolder = (Folder)activeExplorer.CurrentFolder;
                objCurView = (View)activeExplorer.CurrentView;
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
                        } // If TypeOf ObjItem Is mailItem Then
                    } // For Each ObjItem In OlItemsTmp
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
                    } // If TypeOf ObjItem Is mailItem Then
                } // For Each ObjItem In OlItemsRemainder
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
                    } // If TypeOf ObjItem Is mailItem Then
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
}
