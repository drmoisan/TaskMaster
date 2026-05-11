using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class DfDeedle
    {
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

        public static void PrintToLog<TRowKey, TColumnKey>(
            this Frame<TRowKey, TColumnKey> frame,
            log4net.ILog logger,
            [CallerArgumentExpression(nameof(frame))] string frameName = ""
        )
        {
            var frameText = frame.Format(15, 15, 15, 15, printTypes: false, showInfo: true);
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
