#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;
using static System.Net.WebRequestMethods;
using Outlook = Microsoft.Office.Interop.Outlook;
using Reflection = System.Reflection;

namespace UtilitiesCS
{
    //public enum

    public static partial class ConvHelper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private static string DescribeSynchronizationContext(SynchronizationContext syncContext)
        {
            return syncContext?.GetType().FullName ?? "null";
        }

        private static string BuildConversationTimingContext()
        {
            return $"threadId={Thread.CurrentThread.ManagedThreadId}; syncContext={DescribeSynchronizationContext(SynchronizationContext.Current)}";
        }

        private static void LogConversationTiming(string phase, string? details = null)
        {
            var detailSegment = string.IsNullOrWhiteSpace(details) ? string.Empty : $" | {details}";
            var phaseLabel = phase.StartsWith("[Conversation timing]", StringComparison.Ordinal)
                ? phase
                : $"[Conversation timing] {phase}";
            logger.Debug($"{phaseLabel} | {BuildConversationTimingContext()}{detailSegment}");
        }

        internal static object? SafeResolveConversationItem(
            object? namespaceRef,
            Func<object, string, string, object>? resolver
        )
        {
            if (namespaceRef is null || resolver is null)
            {
                return null;
            }

            try
            {
                return resolver(namespaceRef, string.Empty, string.Empty);
            }
            catch
            {
                return null;
            }
        }

        public enum Justify
        {
            Right = 1,
            Left = 2,
            Center = 4,
        }

        public static IList GetMailItemList(
            DataFrame df,
            string storeID,
            Outlook.Application olApp,
            bool strict
        )
        {
            IList emails = new List<MailItem>();
            string EntryID = "EntryID";

            if (df == null)
            {
                if (strict)
                {
                    throw new ArgumentNullException(nameof(df));
                }
                else
                {
                    return emails;
                }
            }
            else if (!df.Columns.GetNames().Contains(EntryID))
            {
                if (strict)
                {
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(df)} is missing {EntryID} columns: {string.Join(",", df.Columns.GetNames())}"
                    );
                }
                else
                {
                    return emails;
                }
            }
            else if (df.Rows.Count == 0)
            {
                if (strict)
                {
                    throw new ArgumentOutOfRangeException("df is empty");
                }
                else
                {
                    return emails;
                }
            }
            else
            {
                emails = df["EntryID"]
                    [0, (int)df.Rows.Count]
                    .Select(x => olApp.GetNamespace("MAPI").GetItemFromID((string)x, storeID))
                    .ToList();
                return emails;
            }
        }

        public static async Task<T> GetItemAsync<T>(
            this DataFrameRow row,
            Outlook.NameSpace olNs,
            int indexEntryId,
            int indexStoreId
        )
            where T : MailItem, TaskItem, AppointmentItem, MeetingItem
        {
            string entryId = (string)row[indexEntryId];
            string storeId = (string)row[indexStoreId];
            var item = await Task.FromResult((T)olNs.GetItemFromID(entryId, storeId));
            return item;
        }

        public static IList GetMailItemList(DataFrame df, string storeID, Outlook.Application olApp)
        {
            IList emails = new List<MailItem>();
            string EntryID = "EntryID";

            if ((df == null) || (!df.Columns.GetNames().Contains(EntryID)) || (df.Rows.Count == 0))
            {
                return emails;
            }
            else
            {
                emails = df["EntryID"]
                    [0, (int)df.Rows.Count]
                    .Select(x => olApp.GetNamespace("MAPI").GetItemFromID((string)x, storeID))
                    .ToList();
                return emails;
            }
        }

        public static int ConversationCt(this object ObjItem, bool SameFolder, bool MailOnly)
        {
            if (ObjItem is MailItem)
            {
                MailItem mailItem = (MailItem)ObjItem;
                return mailItem.ConversationCt(SameFolder, MailOnly);
            }
            return -1;
        }

        public static int ConversationCt(this MailItem ObjItem, bool SameFolder, bool MailOnly)
        {
            Outlook.Conversation conv = ObjItem.GetConversation();
            if (conv != null)
            {
                //Outlook.Table table = ObjItem
                //                      .GetConversation()
                //                      .GetTable(true, false);
                DataFrame? df = conv.GetDataFrame();
                if (df is null)
                {
                    return 0;
                }

                Debug.WriteLine(df.PrettyText());
                string? folderName = null;
                if (SameFolder)
                {
                    folderName =
                        ObjItem.PropertyAccessor?.GetProperty(MAPIFields.Schemas.FolderName)
                        as string;
                }

                df = df.FilterConversation(folderName, SameFolder, MailOnly);

                return (int)(df?.Rows.Count ?? 0);
            }
            return 0;
        }

        public static DataFrame? GetConversationDf(this object ObjItem)
        {
            if (ObjItem is MailItem)
            {
                MailItem mailItem = (MailItem)ObjItem;
                return mailItem.GetConversationDf();
            }
            return null;
        }

        //PERFORMANCE: Add async version of GetConversationDf
        public static DataFrame? GetConversationDf(this Conversation? conversation)
        {
            if (conversation != null)
            {
                bool retry = true;
                int retryCount = 0;
                DataFrame? df = null;
                while (retry)
                {
                    try
                    {
                        retry = false;
                        df = conversation.GetDataFrame();
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        retry = retryCount++ < 2;
                    }
                }

                //Console.WriteLine(df.PrettyText());
                return df;
            }
            return null;
        }

        public static async Task<DataFrame?> GetConversationDfAsync(
            this MailItem mailItem,
            CancellationToken token
        )
        {
            var conversationStopwatch = Stopwatch.StartNew();
            LogConversationTiming(
                "[Conversation timing] GetConversationDfAsync conversation acquisition start | conversation acquisition",
                "timeoutMs=1000; retryCount=3"
            );
            var conv = await TimeOutTask.RunWithTimeout(
                () => mailItem.GetConversation(),
                token,
                1000,
                3,
                false
            );

            LogConversationTiming(
                "GetConversationDfAsync conversation acquisition complete | conversation acquisition",
                $"timeoutMs=1000; retryCount=3; elapsedMs={conversationStopwatch.ElapsedMilliseconds}"
            );

            // Capture the GetConversationTable snapshot before background transform handoff.
            LogConversationTiming(
                "GetConversationDfAsync snapshot handoff start | snapshot handoff",
                $"source={nameof(GetConversationTable)}"
            );
            var df = await conv.GetDataFrameAsync(token);
            LogConversationTiming(
                "GetConversationDfAsync snapshot handoff complete | snapshot handoff",
                $"source={nameof(GetConversationTable)}; elapsedMs={conversationStopwatch.ElapsedMilliseconds}"
            );
            return df;
        }

        public static async Task<DataFrame?> GetConversationDfAsync(
            this MailItem mailItem,
            CancellationToken token,
            int timeout,
            int retryCount,
            TaskCreationOptions options,
            TaskScheduler scheduler
        )
        {
            token.ThrowIfCancellationRequested();

            var retryStopwatch = Stopwatch.StartNew();
            LogConversationTiming(
                "GetConversationDfAsync retryable conversation acquisition start | conversation acquisition",
                $"timeoutMs={timeout}; retryCount={retryCount}"
            );

            var timeoutCancellation = new CancellationTokenSource(timeout);
            var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                token,
                timeoutCancellation.Token
            );

            DataFrame? df = null;

            try
            {
                df = await TimeOutTask.RunWithTimeout(
                    () =>
                    {
                        Outlook.Conversation conv = mailItem.GetConversation();
                        return conv.GetDataFrame();
                    },
                    combinedCancellation.Token,
                    timeout,
                    retryCount + 1,
                    false
                );
                LogConversationTiming(
                    "GetConversationDfAsync retryable conversation acquisition complete | conversation acquisition",
                    $"timeoutMs={timeout}; retryCount={retryCount}; elapsedMs={retryStopwatch.ElapsedMilliseconds}"
                );
            }
            catch (OperationCanceledException e)
            {
                token.ThrowIfCancellationRequested();

                logger.Warn(
                    $"{nameof(GetConversationDfAsync)} timed out {retryCount + 1} time for email {mailItem.Subject}"
                );
                if (retryCount < 2)
                {
                    df = await mailItem.GetConversationDfAsync(
                        token,
                        timeout,
                        retryCount + 1,
                        options,
                        scheduler
                    );
                }
                else
                {
                    var message =
                        $"{nameof(GetConversationDfAsync)} timed out {retryCount + 1} times for email {mailItem.Subject} and was canceled";
                    logger.Warn($"{message} {e.StackTrace}");
                    MyBox.ShowDialog(
                        message,
                        "Operation Cancelled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }

            return df;
        }

        public static DataFrame? GetConversationDf(this MailItem mailItem)
        {
            Outlook.Conversation conv = mailItem.GetConversation();
            return conv.GetConversationDf();
        }

        //PERFORMANCE: Add async version of FilterConversation
        public static DataFrame? FilterConversation(
            this DataFrame? df,
            string? foldername,
            bool SameFolder,
            bool MailOnly
        )
        {
            if (df != null)
            {
                var columnNames = df.Columns.GetNames();
                if (SameFolder)
                {
                    if (columnNames.Contains("Folder Name"))
                    {
                        df = df.Filter(df["Folder Name"].ElementwiseEquals<string>(foldername!));
                    }
                }
                if (MailOnly)
                {
                    if (columnNames.Contains("MessageClass"))
                    {
                        df = df.Filter(df["MessageClass"].ElementwiseEquals<string>("IPM.Note"));
                    }
                }
                return df;
            }
            return null;
        }

        //WAITING: If GetInfoMethod can get all the data, map this method to MailItemInfo class
    }
}
