using Deedle;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;
using UtilitiesCS.ReusableTypeClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class DfDeedle
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Frame<int, string> GetEmailDataInView(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            var storeID = activeExplorer.CurrentFolder.StoreID;

            AddQfcColumns(table);

            (object[,] data, Dictionary<string, int> columnInfo) = table.ETL();
            
            var records = Enumerable.Range(0, data.GetLength(0)).Select(i =>
            {
                DateTime sentOn = DateTime.MaxValue;
                var dateField = data[i, columnInfo["SentOn"]];
                if (dateField is not null) { DateTime.TryParse(dateField.ToString(), out sentOn); }
                if (dateField is null) { sentOn = DateTime.MaxValue; }
                
                return new
                {
                    EntryId = data[i, columnInfo["EntryID"]],
                    MessageClass = data[i, columnInfo["MessageClass"]].ToString(),
                    SentOn = sentOn,
                    ConversationId = data[i, columnInfo["ConversationId"]],
                    Triage = (string)data[i, columnInfo["Triage"]] ?? "Z",
                    StoreId = storeID
                };
            });

            //string[,] strAry = new string[records.Count(), 6];
            //var r2 = records.ToList();
            //Enumerable.Range(0, data.GetLength(0)).ForEach(i =>
            //{
            //    strAry[i,0] = r2[i].EntryId.ToString();
            //    strAry[i, 1] = r2[i].MessageClass.ToString();
            //    strAry[i, 2] = r2[i].SentOn.ToString();
            //    strAry[i, 3] = r2[i].ConversationId.ToString();
            //    strAry[i, 4] = r2[i].Triage.ToString();
            //    strAry[i, 5] = r2[i].StoreId.ToString();
            //});
            //logger.Debug(strAry.ToFormattedText());

            var df = Frame.FromRecords(records);
            
            return df;
        }

        public static async Task<Frame<int, string>> GetEmailDataInViewAsync(Explorer activeExplorer, CancellationToken token, CancellationTokenSource tokenSource, ProgressTracker progress)
        {
            token.ThrowIfCancellationRequested();
            
            //logger.Debug($"{nameof(GetEmailDataInViewAsync)}: {activeExplorer.CurrentFolder.Name}");

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(OlTableExtensions.GetTableInViewAsync)} ...");
            Outlook.Table table = await activeExplorer.GetTableInViewAsync(token, 0);
            //table.EnumerateTable();
            var storeID = activeExplorer.CurrentFolder.StoreID;

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(AddQfcColumnsAsync)} ...");
            await AddQfcColumnsAsync(table, token, 0);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(OlTableExtensions.EtlAsync)} ...");
            (object[,] data, Dictionary<string, int> columnInfo) = await table.EtlAsync(token, tokenSource, 0, progress.Increment(2).SpawnChild(96));
            //(PrettyPrinters.ArraytoDatatable(data, columnInfo.Keys.Cast<string>().ToArray())).DisplayDialog();

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(Email2dArrayToDf)} ...");
            Frame<int, string> df = await Task.Run(() => Email2dArrayToDf(storeID, data, columnInfo),
                token).TimeoutAfter(1000, 2);
            //Frame<int, string> df = await Task.Factory.StartNew(() => Email2dArrayToDf(storeID, data, columnInfo),
            //    token, TaskCreationOptions.LongRunning, TaskScheduler.Default).TimeoutAfter(1000, 2);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} {nameof(GetEmailDataInViewAsync)} complete");
            progress.Report(100);
            return df;
        }

        private static Frame<int, string> Email2dArrayToDf(string storeID, object[,] data, Dictionary<string, int> columnInfo)
        {
            IEnumerable<EmailRecord> records = Email2dToRecords(storeID, data, columnInfo);
            var df = Frame.FromRecords(records);
            return df;
        }

        private static IEnumerable<EmailRecord> Email2dToRecords(string storeID, object[,] data, Dictionary<string, int> columnInfo)
        {
            var acceptableTriage = new string[] { "Z", "A", "B", "C", };
            var records = Enumerable.Range(0, data.GetLength(0)).Select(i =>
            {
                var record = new EmailRecord
                (
                    entryId: (string)data[i, columnInfo["EntryID"]],
                    messageClass: data[i, columnInfo["MessageClass"]].ToString(),
                    sentOn: DateFrom2dPosition(data, columnInfo["SentOn"], i),
                    conversationId: (string)data[i, columnInfo["ConversationId"]],
                    triage: AcceptableTriage((string)data[i, columnInfo["Triage"]] ?? "Z"),
                    storeId: (string)storeID
                );
                return record;
            });
            
            
            return records;
        }

        private struct EmailRecord
        {
            public EmailRecord() { }
            public EmailRecord(string entryId, string messageClass, DateTime sentOn, string conversationId, string triage, string storeId)
            {
                EntryId = entryId;
                MessageClass = messageClass;
                SentOn = sentOn;
                ConversationId = conversationId;
                Triage = triage;
                StoreId = storeId;
            }
            public string EntryId = default;
            public string MessageClass = default;
            public DateTime SentOn = default;
            public string ConversationId = default;
            public string Triage = default;
            public string StoreId = default;
        }

        private static string AcceptableTriage(string triage)
        {
            var acceptableTriage = new string[] { "Z", "A", "B", "C", };
            if (!acceptableTriage.Contains(triage)) { return "Z"; }
            return triage;
        }
        
        private static DateTime DateFrom2dPosition(object[,] data, int column, int row)
        {
            DateTime date = DateTime.MaxValue;
            var dateField = data[row, column];
            if (dateField is not null) { DateTime.TryParse(dateField.ToString(), out date); }
            if (dateField is null) { date = DateTime.MaxValue; }

            return date;
        }

        private static void AddQfcColumns(Table table)
        {
            table.Columns.Add("SentOn");
            table.Columns.Add(MAPIFields.Schemas.ConversationId);
            table.Columns.Add(MAPIFields.Schemas.Triage);
            table.Columns.Remove("Subject");
            table.Columns.Remove("CreationTime");
            table.Columns.Remove("LastModificationTime");
        }

        private static async Task AddQfcColumnsAsync(Table table, CancellationToken token, int counter)
        {
            try
            {
                await Task.Run(() => AddQfcColumns(table), token).TimeoutAfter(3000);
            }
            catch (TaskCanceledException)
            {
                if (!token.IsCancellationRequested && counter < 2)
                {
                    await AddQfcColumnsAsync(table, token, counter + 1);
                }
            }
            catch (TimeoutException)
            {
                if (!token.IsCancellationRequested && counter < 2)
                {
                    await AddQfcColumnsAsync(table, token, counter + 1);
                }
            }

        }

        internal static Series<int, string> GetColumnEid(object[] slice)
        {
            return slice.CastNullSafe<string>().ToOrdinalSeries();
        }

        internal static object GetFirstNonNull(IEnumerable<object> columnData)
        {
            if ((columnData is null) || (columnData.Count() == 0)) { return null; }

            var filteredData = columnData.Where(x => x is not null).ToArray();
            if ((filteredData is null) || (filteredData.Count() == 0)) { return null; }

            return filteredData.First();
        }

        public static Frame<int, string> FromArray2D(object[,] data, Dictionary<string, int> columnDictionary)
        {
            var rows = Enumerable.Range(0, data.GetLength(0)).Select(i =>
            {
                var sb = new SeriesBuilder<string>();
                foreach (var key in columnDictionary.Keys)
                {
                    var value = data[i, columnDictionary[key]];
                    sb.Add(key, value);
                }
                return KeyValue.Create(i, sb.Series);
            });
            var dfTemp = Frame.FromRows(rows);
            return dfTemp;
        }

        private static async Task<Frame<int, string>> FromDefaultFolderAsync(Store store,
                                                           OlDefaultFolders folderEnum,
                                                           string[] removeColumns,
                                                           string[] addColumns,
                                                           CancellationToken cancel,
                                                           int maxAttempts)
        {
            var table = await store.GetTableAsync(folderEnum: folderEnum,
                                    removeColumns: removeColumns,
                                    addColumns: addColumns,
                                    cancel: cancel,
                                    maxAttempts: maxAttempts) as Table;

            if (table is null) { return null; }

            (IAsyncEnumerable<Row> rows,
                Dictionary<string, int> columnDictionary,
                Dictionary<string, Func<object, string>> objectConverters,
                IOrderedEnumerable<int> binIndices,
                IEnumerable<string> objFields,
                IEnumerable<int> objIndices) = await table.EtlPrepAsync(cancel);
            var jagged = await rows.EtlByRowAsync(objectConverters, binIndices, objFields, objIndices).ToArrayAsync();
            
            var data = jagged.To2D();
            Frame<int, string> df = FromArray2D(data: data, columnDictionary);

            return df;
        }

        public static Frame<int, string> FromDefaultFolder(Store store,
                                                           OlDefaultFolders folderEnum,
                                                           string[] removeColumns,
                                                           string[] addColumns)
        {
            var table = store.GetTable(folderEnum: folderEnum,
                                       removeColumns: removeColumns,
                                       addColumns: addColumns);

            if (table is null) { return null; }

            (var data, var columnInfo) = table.ETL();

            Frame<int, string> df = FromArray2D(data: data, columnInfo);

            return df;   
        }

        public static Frame<int, string> FromDefaultFolder(Stores stores,
                                                           OlDefaultFolders folderEnum,
                                                           string[] removeColumns,
                                                           string[] addColumns)
        {
            Frame<string, string> df = null;
            foreach (Outlook.Store store in stores)
            {
                var dfTemp = DfDeedle.FromDefaultFolder(store: store,
                                                        folderEnum: folderEnum,
                                                        removeColumns: removeColumns,
                                                        addColumns: addColumns);
                
                // Set the index to the EntryID to avoid duplicate integer index
                var dfEid = dfTemp?.IndexRowsWith<int, string, string>(dfTemp.GetColumn<string>("EntryID").Values);
                if (df is null) { df = dfEid; }
                else if (dfTemp is not null) 
                {
                    df = df.Merge(dfEid);
                }
            }
            // Set the index to the integer index as originally designed to maintain forward compatibility
            var df2 = df.IndexRowsWith(Enumerable.Range(0, df.RowCount));
            return df2;
        }

        public static void Display(this Frame<int, string> df, IEnumerable<string> rowKeyNames)
        {
            DataTable table = df.ToDataTable(rowKeyNames);
            table.Display();
        }

        public static void DisplayDialog(this Frame<int, string> df)
        {
            var rowNames = new List<string> { "Rows"};
            DataTable table = df.ToDataTable(rowNames);
            table.DisplayDialog();
        }

        public static void DisplayDialog(this Frame<int, string> df, IEnumerable<string> rowKeyNames)
        {
            var rowNames = rowKeyNames.ToArray();
            DataTable table = df.ToDataTable(rowNames);
            table.DisplayDialog();
        }

        //public static void Log<TRowKey,TColumnKey>(this Frame<TRowKey, TColumnKey> frame) 
        //{            
        //    var caller = TraceUtility.GetCallerMethod(new System.Diagnostics.StackTrace());
        //    var declaringType = caller.DeclaringType;
        //    log4net.ILog logger = log4net.LogManager.GetLogger(declaringType);
        //    logger.Debug(frame.Format(15, 15, 15, 15, printTypes: false, showInfo: true));
        //}
        
        public static void PrintToLog<TRowKey, TColumnKey>(this Frame<TRowKey, TColumnKey> frame, log4net.ILog logger, [CallerArgumentExpression(nameof(frame))] string frameName = "")
        {
            var frameText = frame.Format(15, 15, 15, 15, printTypes: false, showInfo: true);
            
            // Find the width of the frame in characters. If multi-line, find the position of the newline character.
            // Else use the length of the entire string
            var loc = frameText.IndexOf("\n");
            if (loc == -1) { loc = frameText.Length; }
            var separator = new string('_',loc);
            logger.Debug($"\n{frameName}\n{separator}\n{frame.Format(15, 15, 15, 15, printTypes: false, showInfo: true)}\n");
        }

        public static Frame<TRowKey, TColumnKey> DropFirstN<TRowKey, TColumnKey>(this Frame<TRowKey, TColumnKey> df, int n)
        {
            n = n < df.RowCount ? n : df.RowCount;
            return df.GetRowsAt(Enumerable.Range(n, df.RowCount - n).ToArray());
        }

        public static Frame<TRowKey, TColumnKey> Exclude<TRowKey, TColumnKey>(this Frame<TRowKey, TColumnKey> df, Frame<TRowKey, TColumnKey> other)
        {
            var idx = other.RowIndex.Keys.ToArray();
            if (idx.Length == 0) { return df; }
            df = df.Where(row => !idx.Contains(row.Key));            
            return df;
        }

        

        public static TColumn[] GetDuplicateEntriesByColumn<TRow, TColumn, TColumnData>(this Frame<TRow, TColumn> df, TColumn columnId)
        {
            var column = df.GetColumn<TColumn>(columnId);
            var duplicates = column.Values
                .GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            return duplicates;
        }


    }
}
