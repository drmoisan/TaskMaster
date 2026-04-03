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
using Deedle;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json.Linq;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;
using UtilitiesCS.ReusableTypeClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class DfDeedle
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// Testability seam for <see cref="System.Windows.Forms.MessageBox.Show"/>.
        /// Tests replace this delegate to avoid real modal dialogs.
        /// </summary>
        internal static Func<
            string,
            string,
            System.Windows.Forms.MessageBoxButtons,
            System.Windows.Forms.MessageBoxIcon,
            System.Windows.Forms.DialogResult
        > MessageBoxInvoker = System.Windows.Forms.MessageBox.Show;

        /// <summary>
        /// Testability seam for the <see cref="OlTableExtensions.ETL"/> extension method
        /// that converts an Outlook Table to a 2-D data array.
        /// Tests replace this delegate to supply pre-built data without a live COM Table.
        /// Uses <see cref="object"/> as the parameter type to avoid CS1769 (embedded interop
        /// types cannot be used as generic type arguments across assembly boundaries).
        /// </summary>
        internal static Func<
            object,
            (object[,] data, Dictionary<string, int> columnInfo)
        > TableEtlInvoker = t => ((Outlook.Table)t).ETL();

        /// <summary>
        /// Testability seam for the <see cref="OlTableExtensions.ETL"/> extension method
        /// as used inside <see cref="FromDefaultFolder(Store,OlDefaultFolders,string[],string[])"/>.
        /// Tests replace this delegate to supply pre-built data without a live COM Table.
        /// Uses <see cref="object"/> as the parameter type to avoid CS1769 (embedded interop
        /// types cannot be used as generic type arguments across assembly boundaries).
        /// </summary>
        internal static Func<
            object,
            (object[,] data, Dictionary<string, int> columnInfo)
        > StoreTableEtlInvoker = t => ((Outlook.Table)t).ETL();

        public static Frame<int, string> GetEmailDataInView(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            var currentFolder = activeExplorer.CurrentFolder;
            var storeID = activeExplorer.CurrentFolder.StoreID;

            AddQfcColumns(table, currentFolder);

            (object[,] data, Dictionary<string, int> columnInfo) = TableEtlInvoker(table);

            return GetEmailDataFromTable(storeID, data, columnInfo);
        }

        /// <summary>
        /// Converts a pre-built 2-D email data array and column-index map to a Deedle DataFrame.
        /// Extracted from <see cref="GetEmailDataInView"/> to allow unit-testing the
        /// pure data-transformation logic without a live Outlook Table or Explorer.
        /// </summary>
        /// <param name="storeID">The store identifier to stamp on every row.</param>
        /// <param name="data">2-D array where rows are emails and columns are field values.</param>
        /// <param name="columnInfo">Maps field name to column index in <paramref name="data"/>.</param>
        /// <returns>A Deedle Frame with one row per email and columns for each field.</returns>
        internal static Frame<int, string> GetEmailDataFromTable(
            string storeID,
            object[,] data,
            Dictionary<string, int> columnInfo
        )
        {
            var records = Enumerable
                .Range(0, data.GetLength(0))
                .Select(i =>
                {
                    return new
                    {
                        EntryId = data[i, columnInfo["EntryID"]],
                        MessageClass = data[i, columnInfo["MessageClass"]].ToString(),
                        SentOn = DateFrom2dPosition(data, columnInfo["SentOn"], i),
                        ConversationId = data[i, columnInfo["ConversationId"]],
                        Triage = (string)data[i, columnInfo["Triage"]] ?? "Z",
                        StoreId = storeID,
                    };
                });

            var df = Frame.FromRecords(records);

            return df;
        }

        public static async Task<Frame<int, string>> GetEmailDataInViewAsync(
            Explorer activeExplorer,
            CancellationToken token,
            CancellationTokenSource tokenSource,
            ProgressTracker progress
        )
        {
            token.ThrowIfCancellationRequested();

            //logger.Debug($"{nameof(GetEmailDataInViewAsync)}: {activeExplorer.CurrentFolder.Name}");

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(OlTableExtensions.GetTableInViewAsync)} ...");
            Outlook.Table table = await activeExplorer.GetTableInViewAsync(token, 0);
            //table.EnumerateTable();
            var currentFolder = activeExplorer.CurrentFolder;
            var storeID = activeExplorer.CurrentFolder.StoreID;

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(AddQfcColumnsAsync)} ...");
            await AddQfcColumnsAsync(table, currentFolder, token, 0);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(OlTableExtensions.EtlAsync)} ...");
            (object[,] data, Dictionary<string, int> columnInfo) = await table.EtlAsync(
                token,
                tokenSource,
                0,
                progress.Increment(2).SpawnChild(96)
            );
            //(PrettyPrinters.ArraytoDatatable(data, columnInfo.Keys.Cast<string>().ToArray())).DisplayDialog();

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(Email2dArrayToDf)} ...");
            Frame<int, string> df = await Task.Run(
                    () => Email2dArrayToDf(storeID, data, columnInfo),
                    token
                )
                .TimeoutAfter(1000, 2);
            //Frame<int, string> df = await Task.Factory.StartNew(() => Email2dArrayToDf(storeID, data, columnInfo),
            //    token, TaskCreationOptions.LongRunning, TaskScheduler.Default).TimeoutAfter(1000, 2);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} {nameof(GetEmailDataInViewAsync)} complete");
            progress.Report(100);
            return df;
        }

        private static Frame<int, string> Email2dArrayToDf(
            string storeID,
            object[,] data,
            Dictionary<string, int> columnInfo
        )
        {
            IEnumerable<EmailRecord> records = Email2dToRecords(storeID, data, columnInfo);
            var df = Frame.FromRecords(records);
            return df;
        }

        private static IEnumerable<EmailRecord> Email2dToRecords(
            string storeID,
            object[,] data,
            Dictionary<string, int> columnInfo
        )
        {
            var acceptableTriage = new string[] { "Z", "A", "B", "C" };
            var records = Enumerable
                .Range(0, data.GetLength(0))
                .Select(i =>
                {
                    var record = new EmailRecord(
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

            public EmailRecord(
                string entryId,
                string messageClass,
                DateTime sentOn,
                string conversationId,
                string triage,
                string storeId
            )
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
            var acceptableTriage = new string[] { "Z", "A", "B", "C" };
            if (!acceptableTriage.Contains(triage))
            {
                return "Z";
            }
            return triage;
        }

        private static DateTime DateFrom2dPosition(object[,] data, int column, int row)
        {
            DateTime date = DateTime.MaxValue;
            var dateField = data[row, column];
            if (
                dateField is not null
                && DateTime.TryParse(dateField.ToString(), out DateTime parsedDate)
            )
            {
                date = parsedDate;
            }

            return date;
        }

        private static void AddQfcColumns(Table table, MAPIFolder folder)
        {
            if (!EnsureTriageColumnExists(folder))
            {
                MessageBoxInvoker(
                    "Cannot proceed without the required 'Triage' column. Execution will stop.",
                    "Missing Required Column",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );

                throw new InvalidOperationException("Required column 'Triage' does not exist.");
            }

            table.Columns.Add("SentOn");
            table.Columns.Add(MAPIFields.Schemas.ConversationId);
            table.Columns.Add(MAPIFields.Schemas.Triage);
            table.Columns.Remove("Subject");
            table.Columns.Remove("CreationTime");
            table.Columns.Remove("LastModificationTime");
        }

        private static async Task AddQfcColumnsAsync(
            Table table,
            MAPIFolder folder,
            CancellationToken token,
            int counter
        )
        {
            try
            {
                await Task.Run(() => AddQfcColumns(table, folder), token).TimeoutAfter(3000);
            }
            catch (TaskCanceledException)
            {
                if (!token.IsCancellationRequested && counter < 2)
                {
                    await AddQfcColumnsAsync(table, folder, token, counter + 1);
                }
            }
            catch (TimeoutException)
            {
                if (!token.IsCancellationRequested && counter < 2)
                {
                    await AddQfcColumnsAsync(table, folder, token, counter + 1);
                }
            }
        }

        private static bool EnsureTriageColumnExists(MAPIFolder folder)
        {
            if (folder is null)
            {
                return false;
            }

            if (HasUserDefinedProperty(folder, "Triage"))
            {
                return true;
            }

            var createResult = MessageBoxInvoker(
                "The required 'Triage' column does not exist in this folder.\nWould you like to create it now?",
                "Create Required Column",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning
            );

            if (createResult != System.Windows.Forms.DialogResult.Yes)
            {
                return false;
            }

            try
            {
                folder.UserDefinedProperties.Add(
                    "Triage",
                    OlUserPropertyType.olText,
                    true,
                    Type.Missing
                );
                return true;
            }
            catch (System.Exception ex)
            {
                MessageBoxInvoker(
                    $"Failed to create 'Triage' column.\n{ex.Message}",
                    "Column Creation Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error
                );

                return false;
            }
        }

        private static bool HasUserDefinedProperty(MAPIFolder folder, string propertyName)
        {
            if (folder?.UserDefinedProperties is null || string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            foreach (UserDefinedProperty property in folder.UserDefinedProperties)
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal static Series<int, string> GetColumnEid(object[] slice)
        {
            return slice.CastNullSafe<string>().ToOrdinalSeries();
        }

        internal static object GetFirstNonNull(IEnumerable<object> columnData)
        {
            if ((columnData is null) || (columnData.Count() == 0))
            {
                return null;
            }

            var filteredData = columnData.Where(x => x is not null).ToArray();
            if ((filteredData is null) || (filteredData.Count() == 0))
            {
                return null;
            }

            return filteredData.First();
        }

        public static Frame<int, string> FromArray2D(
            object[,] data,
            Dictionary<string, int> columnDictionary
        )
        {
            if (data is null)
            {
                return null;
            }
            if (columnDictionary is null)
            {
                return null;
            }

            if (data.GetLength(0) == 0)
            {
                var emptyColumns = columnDictionary.Keys.Select(columnName =>
                {
                    var sb = new SeriesBuilder<int>();
                    return KeyValue.Create(columnName, sb.Series);
                });

                return Frame.FromColumns(emptyColumns);
            }

            var rows = Enumerable
                .Range(0, data.GetLength(0))
                .Select(i =>
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

        private static async Task<Frame<int, string>> FromDefaultFolderAsync(
            Store store,
            OlDefaultFolders folderEnum,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            var table =
                await store.GetTableAsync(
                    folderEnum: folderEnum,
                    removeColumns: removeColumns,
                    addColumns: addColumns,
                    cancel: cancel,
                    maxAttempts: maxAttempts
                ) as Table;

            if (table is null)
            {
                return null;
            }

            (
                IAsyncEnumerable<Row> rows,
                Dictionary<string, int> columnDictionary,
                Dictionary<string, Func<object, string>> objectConverters,
                IOrderedEnumerable<int> binIndices,
                IEnumerable<string> objFields,
                IEnumerable<int> objIndices
            ) = await table.EtlPrepAsync(cancel);
            var jagged = await rows.EtlByRowAsync(
                    objectConverters,
                    binIndices,
                    objFields,
                    objIndices
                )
                .ToArrayAsync();

            var data = jagged.To2D();
            Frame<int, string> df = FromArray2D(data: data, columnDictionary);

            return df;
        }

        public static Frame<int, string> FromDefaultFolder(
            Store store,
            OlDefaultFolders folderEnum,
            string[] removeColumns,
            string[] addColumns
        )
        {
            var table = store.GetTable(
                folderEnum: folderEnum,
                removeColumns: removeColumns,
                addColumns: addColumns
            );

            if (table is null)
            {
                return null;
            }

            (var data, var columnInfo) = StoreTableEtlInvoker(table);

            Frame<int, string> df = FromArray2D(data: data, columnInfo);

            return df;
        }

        public static Frame<int, string> FromDefaultFolder(
            Stores stores,
            OlDefaultFolders folderEnum,
            string[] removeColumns,
            string[] addColumns
        )
        {
            Frame<string, string> df = null;
            foreach (Outlook.Store store in stores)
            {
                var dfTemp = DfDeedle.FromDefaultFolder(
                    store: store,
                    folderEnum: folderEnum,
                    removeColumns: removeColumns,
                    addColumns: addColumns
                );

                if (
                    dfTemp is null
                    || dfTemp.RowCount == 0
                    || !dfTemp.ColumnKeys.Contains("EntryID")
                )
                {
                    continue;
                }

                // Set the index to the EntryID to avoid duplicate integer index
                var dfEid = dfTemp.IndexRowsWith<int, string, string>(
                    dfTemp.GetColumn<string>("EntryID").Values
                );
                if (df is null)
                {
                    df = dfEid;
                }
                else if (dfEid is not null)
                {
                    df = df.Merge(dfEid);
                }
            }

            if (df is null)
            {
                return Frame.FromColumns(new Dictionary<string, Series<int, object>>());
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
            var rowNames = new List<string> { "Rows" };
            DataTable table = df.ToDataTable(rowNames);
            table.DisplayDialog();
        }

        public static void DisplayDialog(
            this Frame<int, string> df,
            IEnumerable<string> rowKeyNames
        )
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

        public static void PrintToLog<TRowKey, TColumnKey>(
            this Frame<TRowKey, TColumnKey> frame,
            log4net.ILog logger,
            [CallerArgumentExpression(nameof(frame))] string frameName = ""
        )
        {
            var frameText = frame.Format(15, 15, 15, 15, printTypes: false, showInfo: true);

            // Find the width of the frame in characters. If multi-line, find the position of the newline character.
            // Else use the length of the entire string
            var loc = frameText.IndexOf("\n");
            if (loc == -1)
            {
                loc = frameText.Length;
            }
            var separator = new string('_', loc);
            logger.Debug(
                $"\n{frameName}\n{separator}\n{frame.Format(15, 15, 15, 15, printTypes: false, showInfo: true)}\n"
            );
        }

        public static Frame<TRowKey, TColumnKey> DropFirstN<TRowKey, TColumnKey>(
            this Frame<TRowKey, TColumnKey> df,
            int n
        )
        {
            n = n < df.RowCount ? n : df.RowCount;
            return df.GetRowsAt(Enumerable.Range(n, df.RowCount - n).ToArray());
        }

        public static Frame<TRowKey, TColumnKey> Exclude<TRowKey, TColumnKey>(
            this Frame<TRowKey, TColumnKey> df,
            Frame<TRowKey, TColumnKey> other
        )
        {
            var idx = other.RowIndex.Keys.ToArray();
            if (idx.Length == 0)
            {
                return df;
            }
            df = df.Where(row => !idx.Contains(row.Key));
            return df;
        }

        public static TColumnData[] GetDuplicateEntriesByColumn<TRow, TColumn, TColumnData>(
            this Frame<TRow, TColumn> df,
            TColumn columnId
        )
        {
            var column = df.GetColumn<TColumnData>(columnId);
            var duplicates = column
                .Values.GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            return duplicates;
        }
    }
}
