#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Web.UI;

//using System.Web.UI;
//using System.Web.UI.WebControls;
using Deedle.Internal;
using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;
using static UtilitiesCS.ConvHelper;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class OlTableExtensions
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildTableTimingContext()
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}";
        }

        private static void LogTableTiming(string phase, string? details = null)
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[Table timing]", StringComparison.Ordinal)
                ? phase
                : $"[Table timing] {phase}";
            logger.Debug($"{phaseLabel} | {BuildTableTimingContext()}{detailSegment}");
        }

        internal static Dictionary<string, object> GetColumnDictionary(
            string[] names,
            object[] values
        )
        {
            var result = new Dictionary<string, object>();
            if (names is null || values is null)
            {
                return result;
            }

            var len = Math.Min(names.Length, values.Length);
            for (var i = 0; i < len; i++)
            {
                result[names[i]] = values[i];
            }

            return result;
        }

        internal static T? RunTableRetry<T>(Func<T> action, int maxAttempts)
        {
            var tries = Math.Max(1, maxAttempts);
            for (var i = 0; i < tries; i++)
            {
                try
                {
                    return action();
                }
                catch
                {
                    if (i == tries - 1)
                    {
                        return default;
                    }
                }
            }

            return default;
        }

        internal static object[] ToObjectRow(object[] rawValues) =>
            rawValues ?? System.Array.Empty<object>();

        /// <summary>
        /// Extension method that removes all columns in the supplied array
        /// from an Outlook Table object
        /// </summary>
        /// <param name="table">Outlook table object</param>
        /// <param name="columnNames">Array of column names to remove</param>
        public static void RemoveColumns(this Outlook.Table table, string[] columnNames)
        {
            if (table is not null && columnNames is not null && columnNames.Count() > 0)
            {
                foreach (var column in columnNames)
                {
                    try
                    {
                        table.Columns.Remove(column);
                    }
                    catch (COMException e)
                    {
                        var inner = e.InnerException;
                        logger.Warn(
                            $"Error in {nameof(RemoveColumns)}\ne.Message  {e.Message}\n"
                                + $"e.ErrorCode  {e.ErrorCode}\ne.HResult  {e.HResult}\nStackTrace\n{e.StackTrace}"
                        );
                        if (inner is not null)
                        {
                            logger.Error(
                                $"InnerException in {nameof(RemoveColumns)}\ninner.Message  {inner.Message}\n"
                                    + $"e.HResult  {inner.HResult}\nStackTrace\n{inner.StackTrace}"
                            );
                        }
                        if (e.ErrorCode == -2147221233)
                        {
                            logger.Warn($"Column {column} not found in table");
                        }
                        else if (e.ErrorCode == -2147352567)
                        {
                            logger.Warn($"Column {column} is read-only");
                        }
                        else if (e.ErrorCode == -555728891)
                        {
                            throw new TimeoutException(e.Message, e);
                        }
                        else if (e.Message.Contains("timeout"))
                        {
                            throw new TimeoutException(e.Message, e);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                //columnNames.ForEach(column => table.Columns.Remove(column));
            }
        }

        public static void RemoveColumns(this Outlook.Table table)
        {
            if (table is not null)
            {
                table.Columns.RemoveAll();
            }
        }

        //TODO: Wire this to an asnyc version of GetConversationTable
        public static async Task RemoveColumnsAsync(
            this Outlook.Table table,
            string[] columnNames,
            CancellationToken cancel,
            int timeOutMiliseconds
        )
        {
            await Task.Run(() => RemoveColumns(table, columnNames), cancel)
                .TimeoutAfter(timeOutMiliseconds);
        }

        /// <summary>
        /// Extension method that iterates through an array of column names or
        /// schemas and adds the corresponding columns to an Outlook Table object
        /// </summary>
        /// <param name="table">Source Table</param>
        /// <param name="columnNames">Array of column names or schemas</param>
        public static void AddColumns(this Outlook.Table table, string[] columnNames)
        {
            if (table is null)
            {
                return;
            }
            try
            {
                foreach (var column in columnNames)
                {
                    table.Columns.Add(column);
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        /// <summary>
        /// Extension that extracts a dictionary of column names and zero-based
        /// positional indices. If columns are supplied as schemas and the schemas
        /// are known, columnn headers are converted from schema to semantic name
        /// </summary>
        /// <param name="table">Source Outlook Table object</param>
        /// <returns>Resulting Outlook Table object</returns>
        public static Dictionary<string, int> GetColumnDictionary(this Outlook.Table table)
        {
            var kvps = Enumerable
                .Range(1, table.Columns.Count)
                .Select(i =>
                {
                    var name = table.Columns[i].Name;
                    if (MAPIFields.SchemaToField.TryGetValue(name, out var adjustedName))
                    {
                        return new KeyValuePair<string, int>(adjustedName, i - 1);
                    }
                    else
                    {
                        return new KeyValuePair<string, int>(name, i - 1);
                    }
                });

            Dictionary<string, int> dict = new();
            try
            {
                dict = kvps.ToDictionary();
            }
            catch (System.InvalidOperationException)
            {
                foreach (var kvp in kvps)
                {
                    if (!dict.ContainsKey(kvp.Key))
                    {
                        dict.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        dict[$"{kvp.Key}{kvp.Value}"] = kvp.Value;
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Extension method extracts a 2D object array of data and a dictionary of
        /// column names and indices. See <see cref="GetColumnDictionary(Table)"/>
        /// </summary>
        /// <param name="table"></param>
        /// <returns>ValueTuple of a 2D object array and a column dictionary</returns>
        public static (object[,] data, Dictionary<string, int> columnInfo) ExtractData2(
            this Outlook.Table table
        )
        {
            var columnDictionary = table.GetColumnDictionary();
            var rowCount = table.GetRowCount();
            var columnCount = columnDictionary.Count;
            //EnumerateTable(table);
            table.MoveToStart();
            object[,]? data = null;

            if (columnDictionary.ContainsKey("Store"))
            {
                var storeIndex = columnDictionary["Store"];
                data = new object[rowCount, columnCount];
                int i = -1;
                while (!table.EndOfTable)
                {
                    i++;
                    Outlook.Row row = table.GetNextRow();
                    var storeID = row.BinaryToString(storeIndex + 1);
                    object[] values = (object[])row.GetValues();
                    //values[columnDictionary["Store"]] = storeID;
                    for (int j = 0; j < columnCount; j++)
                    {
                        if (j == storeIndex)
                        {
                            data[i, j] = storeID;
                        }
                        else
                        {
                            data[i, j] = values[j];
                        }
                    }
                }
            }
            else
            {
                data = (object[,])table.GetArray(rowCount);
            }
            return (data, columnDictionary);
        }

        /// <summary>
        /// Extract, transform, and load data from an Outlook Table object into a 2D object array
        /// </summary>
        /// <param name="table">Outlook.Table</param>
        /// <param name="objectConverters">Dictionary with column names and functions to convert the
        /// object in the column into string representation</param>
        /// <returns>2D object array with string data</returns>
    }
}
