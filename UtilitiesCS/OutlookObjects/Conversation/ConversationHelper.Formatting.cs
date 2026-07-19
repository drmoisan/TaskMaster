#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class ConvHelper
    {
        public static DataFrame GetInfoDf(this Conversation conversation)
        {
            Outlook.Table table = conversation.GetInfoTable()!;
            (object[,]? data, Dictionary<string, int>? columnInfo) = table.ETL();
            var df = data!.ToDataFrame(columnInfo!.Keys.ToArray());
            df.Display();
            return df;
        }

        public static Table? GetInfoTable(this Conversation conversation)
        {
            Outlook.Table table = conversation.GetTable();
            if (table != null)
            {
                string[] columnsToAdd = new string[]
                {
                    "SentOn",
                    MAPIFields.Schemas.FolderName,
                    MAPIFields.Schemas.ConversationDepth,
                    MAPIFields.Schemas.ConversationIndex,
                    MAPIFields.Schemas.ConversationTopic,
                    MAPIFields.Schemas.ConversationId,
                    MAPIFields.Schemas.ReceivedByName,
                };
                foreach (string columnName in columnsToAdd)
                {
                    table.Columns.Add(columnName);
                }
            }
            return table;
        }

        internal static string[] ConversationColumnSchemas =>
            new string[]
            {
                "SentOn",
                MAPIFields.Schemas.FolderName,
                MAPIFields.Schemas.SenderName,
                MAPIFields.Schemas.SenderSmtpAddress,
                MAPIFields.Schemas.SenderAddrType,
                "EntryID",
                MAPIFields.Schemas.MessageStore,
                MAPIFields.Schemas.ConversationDepth,
                MAPIFields.Schemas.ConversationIndex,
            };

        public static DataFrame GetDataFrame(this Outlook.Conversation conversation)
        {
            Table table = conversation.GetConversationTable();
            (object[,]? data, Dictionary<string, int>? columnInfo) = table.ETL();
            return data!.ToDataFrame(columnInfo!.Keys.ToArray());
        }

        public static async Task<DataFrame?> GetDataFrameAsync(
            this Outlook.Conversation? conversation,
            CancellationToken token
        )
        {
            var tableStopwatch = Stopwatch.StartNew();
            LogConversationTiming(
                "GetDataFrameAsync conversation-table creation start | conversation-table creation",
                "timeoutMs=1000; retryCount=3"
            );
            Table conversationTableSnapshot = await TimeOutTask.RunWithTimeout(
                GetConversationTable,
                conversation,
                token,
                1000,
                3,
                false
            );
            if (conversationTableSnapshot is null)
            {
                return null;
            }

            LogConversationTiming(
                "GetDataFrameAsync conversation-table creation complete | conversation-table creation",
                $"timeoutMs=1000; retryCount=3; elapsedMs={tableStopwatch.ElapsedMilliseconds}"
            );
            (object[,]? data, Dictionary<string, int>? columnInfo) =
                await TimeOutTask.RunWithTimeout(
                    () => conversationTableSnapshot.ETL(),
                    token,
                    1000,
                    3,
                    false
                );
            LogConversationTiming(
                "GetDataFrameAsync snapshot handoff complete | snapshot handoff",
                $"rowCount={data!.GetLength(0)}; columnCount={data.GetLength(1)}; elapsedMs={tableStopwatch.ElapsedMilliseconds}"
            );
            return data!.ToDataFrame(columnInfo!.Keys.ToArray());
        }

        public static Table GetConversationTable(this Conversation? conversation)
        {
            Outlook.Table table = conversation!.GetTable();
            table.RemoveColumns(["EntryID"]);
            ConversationColumnSchemas.ForEach(schema => table.Columns.Add(schema));
            return table;
        }

        public static Outlook.Table? GetTable(
            this Outlook.Conversation? conversation,
            bool WithFolder,
            bool WithStore
        )
        {
            if (conversation != null)
            {
                Outlook.Table table = conversation.GetTable();
                table.Columns.Add("SentOn");
                if (WithFolder)
                {
                    table.Columns.Add(MAPIFields.Schemas.FolderName);
                }
                if (WithStore)
                {
                    table.Columns.Add(MAPIFields.Schemas.MessageStore);
                }
                return table;
            }

            return null;
        }

        public static string EnumerateColumnHeaders(
            this Outlook.Table table,
            (int FieldWidth, Justify Justification)[] styleParams,
            string columnDivider,
            string rowBookends
        )
        {
            string[] headers = table.GetColumnHeaders();
            string headerString = headers.JoinFixedWidth(styleParams, columnDivider, rowBookends);

            Debug.WriteLine(headerString);

            return headerString;
        }

        internal static string PadOrTrunc(
            this string fieldName,
            int fieldWidth,
            Justify justification,
            char paddingChar
        )
        {
            switch (justification)
            {
                case Justify.Right:
                    if (fieldName.Length > fieldWidth)
                    {
                        fieldName = ".." + fieldName.Substring(fieldName.Length - fieldWidth - 2);
                    }
                    else
                    {
                        fieldName = fieldName.PadLeft(fieldWidth, paddingChar);
                    }
                    break;
                case Justify.Left:
                    if (fieldName.Length > fieldWidth)
                    {
                        fieldName = fieldName.Substring(0, fieldWidth - 2) + "..";
                    }
                    else
                    {
                        fieldName = fieldName.PadRight(fieldWidth, paddingChar);
                    }
                    break;
                case Justify.Center:
                    if (fieldName.Length > fieldWidth)
                    {
                        fieldName = fieldName.Substring(0, fieldWidth - 2) + "..";
                    }
                    else
                    {
                        int paddingLength = fieldWidth - fieldName.Length;
                        int lenWithPadLeft =
                            (int)Math.Round(paddingLength / 2.0, 0) + fieldName.Length;
                        fieldName = fieldName.PadLeft(lenWithPadLeft, paddingChar);
                        fieldName = fieldName.PadRight(fieldWidth, paddingChar);
                    }
                    break;
            }
            return fieldName;
        }

        public static string JoinFixedWidth(
            this string[] rowCells,
            (int FieldWidth, Justify Justification)[] styleParams,
            string columnDivider,
            string rowBookends
        )
        {
            for (int j = 0; j < rowCells.Length; j++)
            {
                rowCells[j] = rowCells[j]
                    .PadOrTrunc(styleParams[j].FieldWidth, styleParams[j].Justification, ' ');
            }
            string rowString = rowBookends + string.Join(columnDivider, rowCells) + rowBookends;

            Debug.WriteLine(rowString);
            return rowString;
        }

        public static Outlook.Conversation? GetConversation(this object? ObjItem)
        {
            if (ObjItem == null)
            {
                return null;
            }
            else if (ObjItem is MailItem)
            {
                return ((MailItem)ObjItem).GetConversation();
            }
            else if (ObjItem is MeetingItem)
            {
                return ((MeetingItem)ObjItem).GetConversation();
            }
            else if (ObjItem is PostItem)
            {
                return ((PostItem)ObjItem).GetConversation();
            }
            return null;
        }

        public static bool IsSupportedType(this object ObjItem)
        {
            return ((ObjItem is MailItem) || (ObjItem is MeetingItem) || (ObjItem is PostItem));
        }

        internal static Type ResolveType(object Item)
        {
            string errMessage =
                $"{Item.GetType()} is not a member of Outlook.Conversation. "
                + "Item must belong to one of the following \n"
                + typeof(MailItem)
                + "\n"
                + typeof(PostItem)
                + " or\n"
                + typeof(MeetingItem);

            if (Item is MailItem)
            {
                return typeof(MailItem);
            }
            else if (Item is MeetingItem)
            {
                return typeof(MeetingItem);
            }
            else if (Item is PostItem)
            {
                return typeof(PostItem);
            }
            else
            {
                throw new ArgumentException(errMessage);
            }
        }
    }
}
