using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
#nullable enable

    public static partial class DfDeedle
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!
        );

        private static string DescribeSynchronizationContext(SynchronizationContext? syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildDfTimingContext()
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}";
        }

        private static void LogDfTiming(string phase, string? details = null)
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[Df timing]", StringComparison.Ordinal)
                ? phase
                : $"[Df timing] {phase}";
            logger.Debug($"{phaseLabel} | {BuildDfTimingContext()}{detailSegment}");
        }

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
        /// The production ETL over an Outlook Table, used whenever no <c>etl</c> argument is
        /// supplied. Uses <see cref="object"/> as the parameter type to avoid CS1769 (embedded
        /// interop types cannot be used as generic type arguments across assembly boundaries).
        /// </summary>
        private static readonly Func<
            object,
            (object[,] data, Dictionary<string, int> columnInfo)
        > DefaultTableEtl = t => ((Outlook.Table)t).ETL();

        public static Frame<int, string> GetEmailDataInView(
            Explorer activeExplorer,
            Func<object, (object[,] data, Dictionary<string, int> columnInfo)>? etl = null
        )
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            var currentFolder = activeExplorer.CurrentFolder;
            var storeID = activeExplorer.CurrentFolder.StoreID;

            AddQfcColumns(table, currentFolder);

            (object[,] data, Dictionary<string, int> columnInfo) = (etl ?? DefaultTableEtl)(table);

            // Closes the second set of unchecked dictionary reads, the ones GetEmailDataFromTable
            // performs on this synchronous path. The asynchronous path never reaches them, so its
            // guard cannot cover this route.
            ValidateRequiredEmailColumns(columnInfo, currentFolder?.Name ?? "(unknown folder)");

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
            ProgressTracker progress,
            // A null timeProvider resolves to the system clock, so production timing is unchanged.
            // Tests inject a FakeTimeProvider to drive every deadline on this path deterministically.
            TimeProvider? timeProvider = null
        )
        {
            token.ThrowIfCancellationRequested();

            var getEmailDataStopwatch = Stopwatch.StartNew();
            LogDfTiming(
                "[Df timing] GetEmailDataInViewAsync explorer/table acquisition start | explorer/table acquisition"
            );

            //logger.Debug($"{nameof(GetEmailDataInViewAsync)}: {activeExplorer.CurrentFolder.Name}");

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(OlTableExtensions.GetTableInViewAsync)} ...");
            Outlook.Table table = await activeExplorer.GetTableInViewAsync(
                token,
                0,
                timeProvider: timeProvider
            );
            //table.EnumerateTable();
            var currentFolder = activeExplorer.CurrentFolder;
            var storeID = activeExplorer.CurrentFolder.StoreID;

            // The folder name is read once, here on the STA, and the resulting string is what
            // travels onward. Reading Name from a thread-pool continuation would be a
            // cross-apartment property read on a live COM object. The null-coalesce is required
            // rather than defensive: currentFolder?.Name carries maybe-null flow state under this
            // file's #nullable enable while the validator parameter is a non-nullable string.
            var folderName = currentFolder?.Name ?? "(unknown folder)";

            LogDfTiming(
                "GetEmailDataInViewAsync explorer/table acquisition complete | explorer/table acquisition",
                $"folder={folderName}; storeId={storeID}; elapsedMs={getEmailDataStopwatch.ElapsedMilliseconds}"
            );

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(AddQfcColumnsAsync)} ...");
            // currentFolder is the live Explorer's CurrentFolder (non-null); the defensive
            // ?.Name in the preceding log line set its flow state to maybe-null.
            await AddQfcColumnsAsync(table, currentFolder!, token, 0, timeProvider: timeProvider);

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(OlTableExtensions.EtlAsync)} ...");
            var etlStopwatch = Stopwatch.StartNew();
            var tableSnapshot = await table.EtlAsync(
                token,
                tokenSource,
                0,
                progress.Increment(2).SpawnChild(96),
                timeProvider: timeProvider
            );

            // EtlAsync swallows its TimeoutException and returns a null data array through a
            // nullable tuple element, so this is the first point the failure can be named.
            if (tableSnapshot.data is null)
            {
                throw new InvalidOperationException(
                    $"The table snapshot for folder '{folderName}' was not produced: the table ETL "
                        + "timed out or was cancelled before returning any rows, so the email data "
                        + "frame cannot be built for this folder."
                );
            }

            LogDfTiming(
                "GetEmailDataInViewAsync table snapshot ready | table snapshot",
                $"rowCount={tableSnapshot.data.GetLength(0)}; columnCount={tableSnapshot.data.GetLength(1)}; etlElapsedMs={etlStopwatch.ElapsedMilliseconds}"
            );

            // Guards the row builder's unchecked column indexing. A swallowed column-add timeout
            // leaves Outlook's five default columns in place, and without this check the first
            // missing key surfaces two layers away as a KeyNotFoundException naming neither the
            // folder nor the step that failed.
            ValidateRequiredEmailColumns(tableSnapshot.Item2, folderName);
            //(PrettyPrinters.ArraytoDatatable(data, columnInfo.Keys.Cast<string>().ToArray())).DisplayDialog();

            //logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling {nameof(Email2dArrayToDf)} ...");
            var dataframeStopwatch = Stopwatch.StartNew();
            LogDfTiming(
                "GetEmailDataInViewAsync dataframe transform start | dataframe transform",
                "table snapshot captured before dataframe transform"
            );
            Frame<int, string> df = await Task.Run(
                    () => Email2dArrayToDf(storeID, tableSnapshot.data, tableSnapshot.Item2),
                    token
                )
                .TimeoutAfter(1000, timeProvider);
            LogDfTiming(
                "GetEmailDataInViewAsync dataframe transform complete | dataframe transform",
                $"rowCount={df.RowCount}; columnCount={df.ColumnCount}; elapsedMs={dataframeStopwatch.ElapsedMilliseconds}; totalElapsedMs={getEmailDataStopwatch.ElapsedMilliseconds}"
            );
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

            // `= default!` keeps EmailRecord a plain struct (no record/init, which fail CS0518
            // on net481) while satisfying the non-null string field contract; the parameterless
            // ctor path is only used by Deedle's record reflection, which overwrites these fields.
            public string EntryId = default!;
            public string MessageClass = default!;
            public DateTime SentOn = default;
            public string ConversationId = default!;
            public string Triage = default!;
            public string StoreId = default!;
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
    }
}
