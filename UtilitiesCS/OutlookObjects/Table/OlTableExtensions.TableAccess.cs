using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;
using static UtilitiesCS.ConvHelper;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class OlTableExtensions
    {
        public static Outlook.Table GetTableInView(this Explorer activeExplorer)
        {
            Outlook.TableView view = activeExplorer.CurrentView as Outlook.TableView;
            if (view is null)
            {
                throw new InvalidOperationException(
                    $"Current view in Outlook, {((Outlook.View)activeExplorer.CurrentView).Name},"
                        + $" cannot be cast to {nameof(Outlook.TableView)}"
                );
            }
            return view.GetTable();
        }

        public static async Task<Outlook.Table> GetTableInViewAsync(
            this Explorer activeExplorer,
            CancellationToken token,
            int counter,
            int timeoutMs = 2000
        )
        {
            var acquisitionStopwatch = Stopwatch.StartNew();
            LogTableTiming(
                "[Table timing] GetTableInViewAsync table acquisition start | table acquisition",
                $"retryCount={counter}"
            );
            Outlook.Table table = null;
            Outlook.TableView view = activeExplorer.CurrentView as Outlook.TableView;
            if (view is null)
            {
                throw new InvalidOperationException(
                    $"Current view in Outlook, {((Outlook.View)activeExplorer.CurrentView).Name},"
                        + $" cannot be cast to {nameof(Outlook.TableView)}"
                );
            }

            try
            {
                table = await TimeOutTask.RunWithTimeout(view.GetTable, token, timeoutMs, 1, false);

                LogTableTiming(
                    "GetTableInViewAsync table acquisition complete | table acquisition",
                    $"retryCount={counter}; elapsedMs={acquisitionStopwatch.ElapsedMilliseconds}"
                );
            }
            catch (TaskCanceledException)
            {
                if (token.IsCancellationRequested)
                {
                    table = null;
                }
                else
                {
                    Console.WriteLine($"Task timed out on try {counter}");
                    if (counter < 2)
                    {
                        table = await activeExplorer.GetTableInViewAsync(
                            token,
                            counter + 1,
                            timeoutMs
                        );
                    }
                    else
                    {
                        table = null;
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Task timed out on try {counter}");
                if (counter < 2)
                {
                    table = await activeExplorer.GetTableInViewAsync(token, counter + 1);
                }
                else
                {
                    table = null;
                }
            }

            return table;
        }

        public static async Task<object> TryGetTableAsync(
            this Store store,
            OlDefaultFolders folderEnum,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            if (store is null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            MAPIFolder folder = null;
            try
            {
                folder = store.GetDefaultFolder(folderEnum);
            }
            catch (System.Exception e)
            {
                logger.Error($"Error in {nameof(GetTableAsync)}\n{e.Message}\n{e.StackTrace}");
                return null;
            }
            return await folder.TryGetTableAsync(removeColumns, addColumns, cancel, maxAttempts);
        }

        public static async Task<object> GetTableAsync(
            this Store store,
            OlDefaultFolders folderEnum,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            if (store is null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            MAPIFolder folder = null;
            try
            {
                folder = store.GetDefaultFolder(folderEnum);
            }
            catch (System.Exception e)
            {
                logger.Error($"Error in {nameof(GetTableAsync)}\n{e.Message}\n{e.StackTrace}");
                throw;
            }
            return await folder.GetTableAsync(removeColumns, addColumns, cancel, maxAttempts);
        }

        public static Outlook.Table GetTable(
            this Store store,
            OlDefaultFolders folderEnum,
            string[] removeColumns,
            string[] addColumns
        )
        {
            if (store is null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            MAPIFolder folder = null;
            try
            {
                folder = store.GetDefaultFolder(folderEnum);
            }
            catch (System.Exception)
            {
                return null;
            }
            return folder.GetTable(removeColumns: removeColumns, addColumns: addColumns);
        }

        public static async Task<object> TryGetTableAsync(
            this MAPIFolder folder,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            try
            {
                return await folder.GetTableAsync(removeColumns, addColumns, cancel, maxAttempts);
            }
            catch (TaskCanceledException e)
            {
                logger.Info(
                    $"Task canceled in {nameof(TryGetTableAsync)}\n{e.Message}\n{e.StackTrace}"
                );
                return null;
            }
            catch (System.Exception)
            {
                logger.Warn(
                    $"{nameof(GetTableAsync)} failed after {maxAttempts} attempts. Returning null"
                );
                return null;
            }
        }

        public static Task<object> GetTableAsync(
            this MAPIFolder folder,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            try
            {
                return Task.FromResult<object>(folder.GetTable(removeColumns, addColumns));
            }
            catch (COMException e)
            {
                logger.Warn(
                    $"Error in {nameof(GetTableAsync)}\ne.Message  {e.Message}\n"
                        + $"e.ErrorCode  {e.ErrorCode}\ne.HResult  {e.HResult}\nStackTrace\n{e.StackTrace}"
                );

                if (maxAttempts > 1)
                {
                    logger.Info($"Retrying {maxAttempts - 1} times ...");
                    return folder.GetTableAsync(removeColumns, addColumns, cancel, maxAttempts - 1);
                }
                else
                {
                    throw;
                }
            }
        }

        public static Outlook.Table GetTable(
            this MAPIFolder folder,
            string[] removeColumns,
            string[] addColumns
        )
        {
            var table = folder.GetTable();
            table.RemoveColumns(removeColumns);
            table.AddColumns(addColumns);
            return table;
        }

        public static async Task<object> TryGetTableAsync(
            this Conversation conversation,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            try
            {
                return await conversation.GetTableAsync(
                    removeColumns,
                    addColumns,
                    cancel,
                    maxAttempts
                );
            }
            catch (TaskCanceledException e)
            {
                logger.Info(
                    $"Task canceled in {nameof(TryGetTableAsync)}\n{e.Message}\n{e.StackTrace}"
                );
                return null;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static Task<object> GetTableAsync(
            this Conversation conversation,
            string[] removeColumns,
            string[] addColumns,
            CancellationToken cancel,
            int maxAttempts
        )
        {
            try
            {
                return Task.FromResult<object>(conversation.GetTable(removeColumns, addColumns));
            }
            catch (COMException e)
            {
                logger.Warn(
                    $"Error in {nameof(GetTableAsync)}\ne.Message  {e.Message}\n"
                        + $"e.ErrorCode  {e.ErrorCode}\ne.HResult  {e.HResult}\nStackTrace\n{e.StackTrace}"
                );

                if (maxAttempts > 1)
                {
                    logger.Debug($"Retrying {maxAttempts - 1} times ...");
                    return conversation.GetTableAsync(
                        removeColumns,
                        addColumns,
                        cancel,
                        maxAttempts - 1
                    );
                }
                else
                {
                    throw;
                }
            }
        }

        public static Outlook.Table GetTable(
            this Conversation conversation,
            string[] removeColumns,
            string[] addColumns
        )
        {
            var table = conversation.GetTable();
            table.RemoveColumns(removeColumns);
            table.AddColumns(addColumns);
            return table;
        }

        public static IEnumerable<Outlook.Row> GetRows(this Outlook.Table table)
        {
            table.MoveToStart();
            while (!table.EndOfTable)
            {
                yield return table.GetNextRow();
            }
        }

        public static string[] GetColumnHeaders(this Outlook.Table table)
        {
            if (table?.Columns is null)
            {
                return System.Array.Empty<string>();
            }

            var columns = table.Columns;
            if (columns.Count <= 0)
            {
                return System.Array.Empty<string>();
            }

            string[] headers = new string[columns.Count];
            for (var i = 1; i <= columns.Count; i++)
            {
                string name = columns[i]?.Name ?? string.Empty;
                if (MAPIFields.SchemaToField.ContainsKey(name))
                {
                    name = MAPIFields.SchemaToField[name];
                }

                headers[i - 1] = name;
            }

            return headers;
        }

        public static void EnumerateTable(this Outlook.Table table)
        {
            int columnCount = table.Columns.Count;
            int[] charSpacing = Enumerable.Repeat(20, columnCount).ToArray();
            Justify[] justification = Enumerable.Repeat(Justify.Left, columnCount).ToArray();
            Justify[] headerCenter = Enumerable.Repeat(Justify.Center, columnCount).ToArray();
            var styleParams = charSpacing
                .Zip(justification, (space, align) => (FieldWidth: space, Justification: align))
                .ToArray();
            var headerStyles = charSpacing
                .Zip(headerCenter, (space, align) => (FieldWidth: space, Justification: align))
                .ToArray();

            string columnDivider = "   ";
            string rowBookends = " ";
            string[] dividerParts = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                dividerParts[i] = string.Join("", Enumerable.Repeat("=", charSpacing[i]));
            }
            string lineDivider =
                rowBookends + string.Join(columnDivider, dividerParts) + rowBookends;

            string[] headers = table.GetColumnHeaders();
            List<string> rows = new List<string>
            {
                lineDivider,
                table.EnumerateColumnHeaders(headerStyles, columnDivider, rowBookends),
                lineDivider,
            };
            object[,] array = (object[,])table.GetArray(table.GetRowCount());
            string[,] stringArray = array.ToStringArray();

            for (int i = 0; i < stringArray.GetLength(0); i++)
            {
                string[] row = stringArray.SliceRow(i).ToArray();
                rows.Add(row.JoinFixedWidth(styleParams, columnDivider, rowBookends));
            }

            rows.Add(lineDivider);
            string output = string.Join("\n", rows.ToArray());

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine(output);

            table.MoveToStart();
        }
    }
}
