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

namespace UtilitiesCS
{
    //public enum 

    public static class ConvHelper
    {
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

        public static DataFrame GetConversationDf(this object ObjItem, bool SameFolder, bool MailOnly)
        {
            if (ObjItem is MailItem)
            {
                MailItem mailItem = (MailItem)ObjItem;
                return mailItem.GetConversationDf(SameFolder, MailOnly);
            }
            return null;
        }

        public static DataFrame GetConversationDf(this MailItem ObjItem, bool SameFolder, bool MailOnly)
        {
            Outlook.Conversation conv = ObjItem.GetConversation();
            if (conv != null)
            {
                DataFrame df = conv.GetDataFrame();
                
                //Console.WriteLine(df.PrettyText());
                if (SameFolder)
                {
                    //string FolderName = ObjItem.PropertyAccessor.GetProperty(OlTableExtensions.SchemaFolderName) as string;
                    //Console.WriteLine($"Parent is of com type {ComType.TypeInformation.GetTypeName(ObjItem.Parent)}");
                    string FolderName = ((MAPIFolder)ObjItem.Parent).Name;
                    df = df.Filter(df["Folder Name"].ElementwiseEquals<string>(FolderName));
                }
                if (MailOnly)
                {
                    df = df.Filter(df["MessageClass"].ElementwiseEquals<string>("IPM.Note"));
                }
                return df;
            }
            return null;
        }

        public static DataFrame GetDataFrame(this Outlook.Conversation conversation)
        {
            Outlook.Table table = conversation.GetTable();
            if (table != null)
            {
                // add From
                string[] columnsToAdd = new string[5] 
                { 
                    "SentOn", 
                    OlTableExtensions.SchemaFolderName, 
                    OlTableExtensions.SchemaMessageStore, 
                    OlTableExtensions.SchemaConversationDepth, 
                    OlTableExtensions.SchemaConversationIndex 
                };
                foreach (string columnName in columnsToAdd) { table.Columns.Add(columnName); }
            }
            string[] columnHeaders = table.GetColumnHeaders();
            
            object[,] data = (object[,])table.GetArray(table.GetRowCount());

            //DataFrame df = DataFrame.FromColumns()
            //return new DataFrame();
            return data.ToDataFrame(columnHeaders);
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
