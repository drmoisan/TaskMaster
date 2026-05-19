using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class OlTableExtensions
    {
        public static (object[,] data, Dictionary<string, int> columnInfo) ETL(
            this Outlook.Table table,
            Dictionary<string, Func<object, string>> objectConverters = null,
            ProgressTracker progress = null
        )
        {
            if (table is null)
            {
                logger.Error($"Parameter {nameof(table)} is null");
                return (null, null);
            }

            var etlStopwatch = Stopwatch.StartNew();
            LogTableTiming("ETL start | ETL over table snapshots");

            var columnDictionary = table.GetColumnDictionary();
            object[,] data = null;

            table.MoveToStart();
            var rowCount = table.GetRowCount();

            if (rowCount == 0)
            {
                data = new object[0, columnDictionary.Count];
                return (data, columnDictionary);
            }

            if (
                MAPIFields.BinaryToStringFields.Any(x => columnDictionary.ContainsKey(x))
                || (
                    objectConverters is not null
                    && objectConverters.Keys.Any(x => columnDictionary.ContainsKey(x))
                )
            )
            {
                data = EtlByRow(table, objectConverters, columnDictionary, progress);
            }
            else
            {
                data = table?.GetArray(rowCount) as object[,];
            }
            LogTableTiming(
                "ETL complete | ETL over table snapshots",
                $"rowCount={rowCount}; columnCount={columnDictionary.Count}; elapsedMs={etlStopwatch.ElapsedMilliseconds}"
            );
            return (data, columnDictionary);
        }

        public static async Task<(object[,] data, Dictionary<string, int> columnInfo)> EtlAsync(
            this Outlook.Table table,
            CancellationToken token,
            CancellationTokenSource tokenSource,
            int counter,
            ProgressTracker progress,
            Dictionary<string, Func<object, string>> objectConverters = null
        )
        {
            token.ThrowIfCancellationRequested();

            var etlStopwatch = Stopwatch.StartNew();
            LogTableTiming("EtlAsync start | ETL over table snapshots");

            var rowCount = table.GetRowCount();
            int milliseconds = 250 * rowCount;
            var attempts = 3;
            object[,] data = null;
            var columnDictionary = table.GetColumnDictionary();

            table.MoveToStart();

            try
            {
                if (
                    MAPIFields.BinaryToStringFields.Any(x => columnDictionary.ContainsKey(x))
                    || (
                        objectConverters is not null
                        && objectConverters.Keys.Any(x => columnDictionary.ContainsKey(x))
                    )
                )
                {
                    data = await EtlByRowAsync(
                        table,
                        objectConverters,
                        columnDictionary,
                        token,
                        milliseconds,
                        attempts,
                        progress
                    );
                }
                else
                {
                    data = await Task.Run(
                            () => table?.GetArray(table.GetRowCount()) as object[,],
                            token
                        )
                        .TimeoutAfter(milliseconds, attempts);
                }
            }
            catch (TimeoutException)
            {
                logger.Error(
                    $"{DateTime.Now.ToString("mm:ss.fff")} {nameof(ETL)} timed out {attempts} times with a timeout of {milliseconds} milliseconds. Canceling"
                );
                tokenSource.Cancel();
            }

            LogTableTiming(
                "EtlAsync complete | ETL over table snapshots",
                $"rowCount={rowCount}; columnCount={columnDictionary.Count}; elapsedMs={etlStopwatch.ElapsedMilliseconds}"
            );
            return (data, columnDictionary);
        }

        public static async Task<(object[,] data, Dictionary<string, int> columnInfo)> EtlAsyncOld(
            this Outlook.Table table,
            CancellationToken token,
            CancellationTokenSource tokenSource,
            int counter,
            ProgressTracker progress,
            Dictionary<string, Func<object, string>> objectConverters = null
        )
        {
            token.ThrowIfCancellationRequested();

            var rowCount = table.GetRowCount();
            int milliseconds = 250 * rowCount;
            var attempts = 3;
            object[,] data = null;
            Dictionary<string, int> columnInfo = null;

            try
            {
                (data, columnInfo) = await Task.Run(
                        () => table.ETL(objectConverters, progress),
                        token
                    )
                    .TimeoutAfter(milliseconds, attempts);
            }
            catch (TimeoutException)
            {
                logger.Error(
                    $"{DateTime.Now.ToString("mm:ss.fff")} {nameof(ETL)} timed out {attempts} times with a timeout of {milliseconds} milliseconds. Canceling"
                );
                tokenSource.Cancel();
            }

            return (data, columnInfo);
        }

        private static async Task<IAsyncEnumerable<object[]>> EtlByRowAsync(
            Table table,
            Dictionary<string, Func<object, string>> objectConverters,
            Dictionary<string, int> columnDictionary,
            CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            (var binFields, var binIndices) = GetBinFields(columnDictionary);
            (var objFields, var objIndices) = GetObjectFields(objectConverters, columnDictionary);

            var rows = await Task.Run(() => table.GetRows().ToArray().ToAsyncEnumerable(), token);

            token.ThrowIfCancellationRequested();

            int completed = 0;
            var jagged = rows.Select(rows =>
                EtlRow(ref completed, rows, objectConverters, binIndices, objFields, objIndices)
            );
            return jagged;
        }

        public static async Task<(
            IAsyncEnumerable<Row> rows,
            Dictionary<string, int> columnDictionary,
            Dictionary<string, Func<object, string>> objectConverters,
            IOrderedEnumerable<int> binIndices,
            IEnumerable<string> objFields,
            IEnumerable<int> objIndices
        )> EtlPrepAsync(
            this Outlook.Table table,
            CancellationToken cancel,
            Dictionary<string, Func<object, string>> objectConverters = null
        )
        {
            var columnDictionary = await Task.Run(table.GetColumnDictionary);
            (var binFields, var binIndices) = await Task.Run(() => GetBinFields(columnDictionary));
            (var objFields, var objIndices) = await Task.Run(() =>
                GetObjectFields(objectConverters, columnDictionary)
            );
            var rows = await Task.Run(() => table.GetRows().ToAsyncEnumerable(), cancel);
            return (rows, columnDictionary, objectConverters, binIndices, objFields, objIndices);
        }

        public static IAsyncEnumerable<object[]> EtlByRowAsync(
            this IAsyncEnumerable<Row> rows,
            Dictionary<string, Func<object, string>> objectConverters,
            IOrderedEnumerable<int> binIndices,
            IEnumerable<string> objFields,
            IEnumerable<int> objIndices
        )
        {
            return rows.Select(rows =>
                EtlRow(rows, objectConverters, binIndices, objFields, objIndices)
            );
        }

        private static async Task<object[,]> EtlByRowAsync(
            Table table,
            Dictionary<string, Func<object, string>> objectConverters,
            Dictionary<string, int> columnDictionary,
            CancellationToken token,
            int timeout,
            int attempts,
            ProgressTracker progress = null
        )
        {
            token.ThrowIfCancellationRequested();

            (var binFields, var binIndices) = GetBinFields(columnDictionary);
            (var objFields, var objIndices) = GetObjectFields(objectConverters, columnDictionary);

            var rows = await Task.Run(() => table.CastToRowArray(progress?.SpawnChild(65)), token)
                .TimeoutAfter(timeout, attempts);

            token.ThrowIfCancellationRequested();
            var jagged = await Task.Run(
                    () =>
                        rows.EtlByRow(
                            objectConverters,
                            binIndices,
                            objFields,
                            objIndices,
                            progress?.SpawnChild()
                        ),
                    token
                )
                .TimeoutAfter(timeout, attempts);

            var data = jagged.To2D();
            return data;
        }

        private static object[,] EtlByRow(
            Table table,
            Dictionary<string, Func<object, string>> objectConverters,
            Dictionary<string, int> columnDictionary,
            ProgressTracker progress = null
        )
        {
            (var binFields, var binIndices) = GetBinFields(columnDictionary);
            (var objFields, var objIndices) = GetObjectFields(objectConverters, columnDictionary);
            var rows = table.CastToRowArray(progress?.SpawnChild(65));

            var jagged = rows.EtlByRow(
                objectConverters,
                binIndices,
                objFields,
                objIndices,
                progress?.SpawnChild()
            );
            var data = jagged.To2D();
            return data;
        }

        private static object[][] EtlByRow(
            this Row[] rows,
            Dictionary<string, Func<object, string>> objectConverters,
            IOrderedEnumerable<int> binIndices,
            IEnumerable<string> objFields,
            IEnumerable<int> objIndices,
            ProgressTracker progress
        )
        {
            int completed = 0;
            int rowCount = rows.Count();
            var query = Enumerable.Range(0, rowCount);
            if (rows is not null && rowCount > 200)
            {
                query = query.AsParallel();
            }
            var query2 = query.Select(i =>
                EtlRow(ref completed, rows[i], objectConverters, binIndices, objFields, objIndices)
            );

            object[][] jagged;

            if (progress is null)
            {
                jagged = query2.ToArray();
            }
            else
            {
                using (
                    new Timer(
                        _ =>
                            progress.Report(
                                (int)((double)completed / rowCount),
                                $"Etl row {completed} of {rowCount}"
                            ),
                        null,
                        0,
                        500
                    )
                )
                {
                    jagged = query2.ToArray();
                }
            }

            return jagged;
        }

        private static (IEnumerable<string>, IOrderedEnumerable<int>) GetBinFields(
            Dictionary<string, int> columnDictionary
        )
        {
            var binFields = MAPIFields.BinaryToStringFields.Where(x =>
                columnDictionary.ContainsKey(x)
            );
            var binIndices = binFields.Select(x => columnDictionary[x]).OrderBy(x => x);

            return (binFields, binIndices);
        }

        private static Row[] CastToRowArray(this Table table, ProgressTracker progress)
        {
            var rowExtractionStopwatch = Stopwatch.StartNew();
            LogTableTiming(
                "CastToRowArray row extraction start | row extraction",
                $"rowCount={table.GetRowCount()}"
            );
            Row[] rows;
            var rowCount = table.GetRowCount();
            int completed = 0;
            if (progress is not null)
            {
                progress.Report(
                    0,
                    $"Capturing email table rows {(int)((double)completed * (double)rowCount / 100)} of {rowCount}"
                );
                using (
                    new Timer(
                        _ =>
                            progress.Report(
                                completed,
                                $"Capturing email table rows {(int)((double)completed * (double)rowCount / 100)} of {rowCount}"
                            ),
                        null,
                        0,
                        500
                    )
                )
                {
                    rows = table
                        .GetRows()
                        .WithProgressReporting(rowCount, (x) => completed = x)
                        .ToArray();
                }
            }
            else
            {
                rows = table.GetRows().ToArray();
            }
            LogTableTiming(
                "CastToRowArray row extraction complete | row extraction",
                $"rowCount={rows.Length}; elapsedMs={rowExtractionStopwatch.ElapsedMilliseconds}"
            );
            return rows;
        }

        private static (IEnumerable<string>, IEnumerable<int>) GetObjectFields(
            Dictionary<string, Func<object, string>> objectConverters,
            Dictionary<string, int> columnDictionary
        )
        {
            if (objectConverters is null)
            {
                return (null, null);
            }

            var objFields = objectConverters.Keys.Where(x => columnDictionary.ContainsKey(x));
            var objIndices = objFields.Select(x => columnDictionary[x]);

            return (objFields, objIndices);
        }

        private static void EtlRow(
            ref object[,] data,
            Outlook.Row row,
            Dictionary<string, Func<object, string>> objectConverters,
            Dictionary<string, int> columnDictionary,
            IOrderedEnumerable<int> binIndices,
            IEnumerable<string> objFields,
            IEnumerable<int> objIndices,
            int rowNumber
        )
        {
            object[] rawValues = (object[])row.GetValues();
            var binStrings = ConvertBinColumnsToString(row, binIndices);
            var objStrings = ConvertObjectColumnsToString(
                row,
                objIndices,
                objFields,
                objectConverters
            );
            WriteValuesToData(
                ref data,
                columnDictionary,
                binIndices,
                rowNumber,
                binStrings,
                objIndices,
                objStrings,
                rawValues
            );
        }

        private static object[] EtlRow(
            ref int rowsCompleted,
            Outlook.Row row,
            Dictionary<string, Func<object, string>> objectConverters,
            IOrderedEnumerable<int> binIndices,
            IEnumerable<string> objFields,
            IEnumerable<int> objIndices
        )
        {
            var objectRow = EtlRow(row, objectConverters, binIndices, objFields, objIndices);
            Interlocked.Increment(ref rowsCompleted);
            return objectRow;
        }

        private static object[] EtlRow(
            Outlook.Row row,
            Dictionary<string, Func<object, string>> objectConverters,
            IOrderedEnumerable<int> binIndices,
            IEnumerable<string> objFields,
            IEnumerable<int> objIndices
        )
        {
            object[] rawValues = (object[])row.GetValues();
            var binStrings = ConvertBinColumnsToString(row, binIndices);
            var objStrings = ConvertObjectColumnsToString(
                row,
                objIndices,
                objFields,
                objectConverters
            );
            var objectRow = rawValues.ToObjectRow(binIndices, binStrings, objIndices, objStrings);
            return objectRow;
        }
    }
}
