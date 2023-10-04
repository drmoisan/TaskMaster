using Microsoft.Office.Interop.Outlook;
using Microsoft.Data.Analysis;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Reflection = System.Reflection;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using System.Threading;
using System.Windows.Forms;

namespace UtilitiesCS
{
    //public enum 

    public static class ConvHelper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum Justify
        {
            Right = 1, Left = 2, Center = 4
        }
        
        public static IList GetMailItemList(DataFrame df,
                                            string storeID,
                                            Outlook.Application olApp,
                                            bool strict)
        {
            IList emails = new List<MailItem>();
            string EntryID = "EntryID";
            
            if (df == null) 
            { 
                if (strict) { throw new ArgumentNullException(nameof(df)); } 
                else { return emails; }
            }
            
            else if (!df.Columns.GetNames().Contains(EntryID)) 
            {
                if (strict) 
                { 
                    throw new ArgumentOutOfRangeException(
                        $"{nameof(df)} is missing {EntryID} columns: {string.Join(",",df.Columns.GetNames())}"); 
                }
                else { return emails; }
            }

            else if (df.Rows.Count == 0) 
            { 
                if (strict) { throw new ArgumentOutOfRangeException("df is empty"); } 
                else { return emails; } 
            }

            else
            {
                emails = df["EntryID"][0, (int)df.Rows.Count]
                    .Select(x => olApp.GetNamespace("MAPI")
                    .GetItemFromID((string)x, storeID))
                    .ToList();
                return emails;
            }
        }

        async public static Task<T> GetItemAsync<T>(this DataFrameRow row, Outlook.NameSpace olNs, int indexEntryId, int indexStoreId) where T: MailItem, TaskItem, AppointmentItem, MeetingItem
        {
            string entryId = (string)row[indexEntryId];
            string storeId = (string)row[indexStoreId];
            var item = await Task.FromResult((T)olNs.GetItemFromID(entryId, storeId));
            return item;            
        }

        public static IList GetMailItemList(DataFrame df,
                                            string storeID,
                                            Outlook.Application olApp)
        {
            IList emails = new List<MailItem>();
            string EntryID = "EntryID";

            if ((df == null) || (df.Columns.GetNames().Contains(EntryID)) || (df.Rows.Count == 0))
            {
                return emails; 
            }
            else
            {
                emails = df["EntryID"][0, (int)df.Rows.Count]
                    .Select(x => olApp.GetNamespace("MAPI")
                    .GetItemFromID((string)x, storeID))
                    .ToList();
                return emails;
            }
        }

        public static int ConversationCt(this object ObjItem, bool SameFolder, bool MailOnly)
        {
            if (ObjItem is MailItem)
            {
                MailItem mailItem = (MailItem)ObjItem;
                return mailItem.ConversationCt(true, true);
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
                DataFrame df = conv.GetDataFrame();
                Debug.WriteLine(df.PrettyText());
                if (SameFolder)
                {
                    string FolderName = ObjItem.PropertyAccessor.GetProperty(OlTableExtensions.SchemaFolderName) as string;
                    df = df.Filter(df["Folder Name"].ElementwiseEquals<string>(FolderName));
                }
                if (MailOnly)
                {
                    df = df.Filter(df["MessageClass"].ElementwiseEquals<string>("IPM.Note"));
                }

                return (int)df.Rows.Count;
            }
            return 0;
        }

        public static DataFrame GetConversationDf(this object ObjItem)
        {
            if (ObjItem is MailItem)
            {
                MailItem mailItem = (MailItem)ObjItem;
                return mailItem.GetConversationDf();
            }
            return null;
        }

        

        //PERFORMANCE: Add async version of GetConversationDf 
        public static DataFrame GetConversationDf(this Conversation conversation)
        {
            if (conversation != null)
            {
                bool retry = true;
                int retryCount = 0;
                DataFrame df = null;
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

        public static async Task<DataFrame> GetConversationDfAsync(
            this MailItem mailItem, 
            CancellationTokenSource tokenSource,
            CancellationToken token, 
            int timeout,
            TaskCreationOptions options,
            TaskScheduler scheduler)
        {
            token.ThrowIfCancellationRequested();

            var timeoutCancellation = new CancellationTokenSource(timeout);
            var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCancellation.Token);
            bool retry = true;
            int retryCount = 0;
            DataFrame df = null;
            
            while (retry)
            {
                try
                {
                    retry = false;
                    await Task.Factory.StartNew(
                        () =>
                        {
                            Outlook.Conversation conv = mailItem.GetConversation();
                            df = conv.GetDataFrame();
                        },
                        combinedCancellation.Token,
                        options,
                        scheduler);
                }
                catch (OperationCanceledException e)
                {
                    token.ThrowIfCancellationRequested();
                    
                    logger.Debug($"{nameof(GetConversationDfAsync)} timed out {retryCount + 1} time for email {mailItem.Subject}");
                    retry = retryCount++ < 2;
                    if (!retry) 
                    { 
                        //tokenSource.Cancel(); 
                        var message = $"{nameof(GetConversationDfAsync)} timed out {retryCount + 1} times for email {mailItem.Subject} and was canceled";
                        logger.Debug($"{message} {e.StackTrace}");
                        MessageBox.Show(message, "Operation Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return df;
        }

        public static DataFrame GetConversationDf(this MailItem mailItem)
        {
            Outlook.Conversation conv = mailItem.GetConversation();
            return conv.GetConversationDf();
        }

        //PERFORMANCE: Add async version of FilterConversation
        public static DataFrame FilterConversation(this DataFrame df, string foldername, bool SameFolder, bool MailOnly)
        {
            if (df != null)
            {
                if (SameFolder)
                {
                    df = df.Filter(df["Folder Name"].ElementwiseEquals<string>(foldername));
                }
                if (MailOnly)
                {
                    df = df.Filter(df["MessageClass"].ElementwiseEquals<string>("IPM.Note"));
                }
                return df;
            }
            return null;
        }
        
        //WAITING: If GetInfoMethod can get all the data, map this method to MailItemInfo class
        public static DataFrame GetInfoDf(this Conversation conversation)
        {
            Outlook.Table table = conversation.GetInfoTable();
            (object[,] data, Dictionary<string, int> columnInfo) = table.ETL();
            var df = data.ToDataFrame(columnInfo.Keys.ToArray());
            df.Display();
            return df;
        }
        
        //QUESTION: Can we get all the info we need from the GetInfoTable method?
        public static Table GetInfoTable(this Conversation conversation)
        {
            Outlook.Table table = conversation.GetTable();
            if (table != null)
            {
                // add From
                string[] columnsToAdd = new string[]
                {
                    "SentOn",
                    OlTableExtensions.SchemaFolderName,
                    //OlTableExtensions.SchemaMessageStore,
                    OlTableExtensions.SchemaConversationDepth,
                    OlTableExtensions.SchemaConversationIndex,
                    OlTableExtensions.SchemaConversationTopic,
                    OlTableExtensions.SchemaConversationId,
                    OlTableExtensions.SchemaReceivedByName
                    
                };
                foreach (string columnName in columnsToAdd) { table.Columns.Add(columnName); }
            }
            return table;
        }

        public static DataFrame GetDataFrame(this Outlook.Conversation conversation)
        {
            var columnsToAdd = new string[]
                {
                    "SentOn",
                    OlTableExtensions.SchemaFolderName,
                    OlTableExtensions.SchemaSenderName,
                    OlTableExtensions.SchemaSenderSmtpAddress,
                    OlTableExtensions.SchemaSenderAddrType,
                    "EntryID",
                    OlTableExtensions.SchemaMessageStore,
                    OlTableExtensions.SchemaConversationDepth,
                    OlTableExtensions.SchemaConversationIndex

                };

            Outlook.Table table = conversation.GetTable();
            if (table != null)
            {
                table.RemoveColumns(new string[] { "EntryID"});
                
                foreach (string columnName in columnsToAdd) { table.Columns.Add(columnName); }
            }
            
            (object[,] data, Dictionary<string, int> columnInfo) = table.ETL();
            
            return data.ToDataFrame(columnInfo.Keys.ToArray());
        }
                
        public static Outlook.Table GetTable(this Outlook.Conversation conversation, bool WithFolder, bool WithStore) 
        { 
            if (conversation != null)
            {
                Outlook.Table table = conversation.GetTable();
                table.Columns.Add("SentOn");
                if (WithFolder) { table.Columns.Add(OlTableExtensions.SchemaFolderName); }
                if (WithStore) { table.Columns.Add(OlTableExtensions.SchemaMessageStore); }
                return table;
            }
            else { return null; }
        }
        
        public static string EnumerateColumnHeaders(this Outlook.Table table, (int FieldWidth, Justify Justification)[] styleParams, string columnDivider, string rowBookends)
        {
            string[] headers = table.GetColumnHeaders();
            //for (int j = 0; j < headers.Length; j++)
            //{
            //    var style = styleParams[j];
            //    string header = headers[j];
            //    header = header.PadOrTrunc(style.FieldWidth, style.Justification, ' ');
            //}
            //string headerString = rowBookends + string.Join(columnDivider, headers) + rowBookends;
            string headerString = headers.JoinFixedWidth(styleParams, columnDivider, rowBookends);

            Debug.WriteLine(headerString);
            
            return headerString;
        }
        
        internal static string PadOrTrunc(this string fieldName, int fieldWidth, Justify justification, char paddingChar)
        {
            switch (justification)
            {
                case Justify.Right:
                    if (fieldName.Length > fieldWidth)
                    { fieldName = ".." + fieldName.Substring(fieldName.Length - fieldWidth - 2); }
                    else
                    { fieldName = fieldName.PadLeft(fieldWidth, paddingChar); }
                    break;
                case Justify.Left:
                    if (fieldName.Length > fieldWidth)
                    { fieldName = fieldName.Substring(0, fieldWidth -2) + ".."; }
                    else
                    { fieldName = fieldName.PadRight(fieldWidth, paddingChar); }
                    break;
                case Justify.Center:
                    if (fieldName.Length > fieldWidth)
                    { fieldName = fieldName.Substring(0, fieldWidth -2 ) + ".."; }
                    else
                    {
                        int paddingLength = fieldWidth - fieldName.Length;
                        int lenWithPadLeft = (int)Math.Round(paddingLength / 2.0, 0) + fieldName.Length;
                        fieldName = fieldName.PadLeft(lenWithPadLeft, paddingChar);
                        fieldName = fieldName.PadRight(fieldWidth, paddingChar);
                    }
                    break;
            }
            return fieldName;
        }

        public static string JoinFixedWidth(this string[] rowCells, (int FieldWidth, Justify Justification)[] styleParams, string columnDivider, string rowBookends)
        {
            for (int j = 0; j < rowCells.Length; j++)
            {
                rowCells[j] = rowCells[j].PadOrTrunc(styleParams[j].FieldWidth, styleParams[j].Justification, ' ');
            }
            string rowString = rowBookends + string.Join(columnDivider, rowCells) + rowBookends;
            
            Debug.WriteLine(rowString);
            return rowString;
        }

        public static Outlook.Conversation GetConversation(this object ObjItem)
        { 
            if (ObjItem == null) {  return null; }
            else if (ObjItem is MailItem) { return ((MailItem)ObjItem).GetConversation(); }
            else if (ObjItem is MeetingItem) { return ((MeetingItem)ObjItem).GetConversation(); }
            else if (ObjItem is PostItem) { return ((PostItem)ObjItem).GetConversation(); }
            return null;
        }

        // dynamic type version of GetConversation
        //public static Conversation GetConversation(object ObjItem)
        //{
        //    if (ObjItem.IsSupportedType())
        //    {
        //        dynamic Item = ObjItem;
        //        Folder folder = Item.Parent;
        //        Store store = folder.Store;
        //        if (store.IsConversationEnabled == true)
        //        {
        //            return Item.GetConversation();
        //        }
        //    }
        //    return null;
        //}
                
        public static bool IsSupportedType(this object ObjItem)
        { return ((ObjItem is MailItem)||(ObjItem is MeetingItem)||(ObjItem is PostItem));}
                
        internal static Type ResolveType(object Item)
        {
            string errMessage = $"{Item.GetType()} is not a member of Outlook.Conversation. " 
                + "Item must belong to one of the following \n" 
                + typeof(MailItem) + "\n"
                + typeof(PostItem) + " or\n"
                + typeof(MeetingItem);

            if (Item is MailItem) { return typeof(MailItem); }
            else if (Item is MeetingItem) { return typeof(MeetingItem); }
            else if (Item is PostItem) { return typeof(PostItem); }
            else { throw new ArgumentException(errMessage); }
        }
    }

}
