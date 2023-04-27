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

namespace UtilitiesCS
{
    //public enum 

    public static class ConvHelper
    {
        public enum Justify
        {
            Right = 1, Left = 2, Center = 4
        }
        
        const string PROPTAG_SPECIFIER = "http://schemas.microsoft.com/mapi/proptag/";

        // PropTag Types
        const string PT_BINARY = "0102";
        const string PT_LONG = "0003";
        const string PT_TSTRING = "001f"; /* Null-terminated 16-bit (2-byte) character string. 
                                           * Properties with this type have the property type 
                                           * reset to PT_UNICODE when compiling with the UNICODE 
                                           * symbol and to PT_STRING8 when not compiling with the 
                                           * UNICODE symbol. This property type is the same as the 
                                           * OLE type VT_LPSTR for resulting PT_STRING8 properties 
                                           * and VT_LPWSTR for PT_UNICODE properties */

        const string PR_STORE_ENTRYID = "0x0FFB"; //Message store PID + PT_BINARY
        const string PR_PARENT_DISPLAY = "0x0e05"; //Message parent folder
        const string PR_DEPTH = "0x3005"; /* Represents the relative level of indentation, 
                                           * or depth, of an object in a hierarchical table
                                           * Data type is PT_LONG */
        const string PR_CONVERSATION_INDEX = "0x0071"; /* PT_BINARY ScCreateConversationIndex 
                                                        * implements the index as a header block 
                                                        * that is 22 bytes in length, followed 
                                                        * by zero or more child blocks each 
                                                        * 5 bytes in length */


        public static string SchemaFolderName = PROPTAG_SPECIFIER + PR_PARENT_DISPLAY + PT_TSTRING;
        public static string SchemaMessageStore = PROPTAG_SPECIFIER + PR_STORE_ENTRYID + PT_BINARY;
        public static string SchemaConversationDepth = PROPTAG_SPECIFIER + PR_DEPTH + PT_LONG;
        public static string SchemaConversationIndex = PROPTAG_SPECIFIER + PR_CONVERSATION_INDEX + PT_BINARY;

        public static Dictionary<string, string> SchemaToField = new()
        {
            {SchemaFolderName, "Folder Name" },
            {SchemaMessageStore, "Store"},
            {SchemaConversationDepth, "ConvDepth" },
            {SchemaConversationIndex, "ConversationIndex" }
        };
        public static Dictionary<string, string> FieldToSchema = new()
        {
            {"Folder Name", SchemaFolderName },
            {"Store", SchemaMessageStore},
            {"ConvDepth", SchemaConversationDepth },
            {"ConversationIndex", SchemaConversationIndex }
        };

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
                    string FolderName = ObjItem.PropertyAccessor.GetProperty(SchemaFolderName) as string;
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
                Debug.WriteLine(df.PrettyText());
                if (SameFolder)
                {
                    string FolderName = ObjItem.PropertyAccessor.GetProperty(SchemaFolderName) as string;
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
                string[] columnsToAdd = new string[5] { "SentOn", SchemaFolderName, SchemaMessageStore, SchemaConversationDepth, SchemaConversationIndex };
                foreach (string columnName in columnsToAdd) { table.Columns.Add(columnName); }
            }
            string[] columnHeaders = table.GetColumnHeaders();
            object[,] data = table.GetArray(table.GetRowCount());

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
                if (WithFolder) { table.Columns.Add(SchemaFolderName); }
                if (WithStore) { table.Columns.Add(SchemaMessageStore); }
                return table;
            }
            else { return null; }
        }

        public static void EnumerateTable(this Outlook.Table table)
        {
            int columnCount = table.Columns.Count;
            int[] charSpacing = Enumerable.Repeat(15, columnCount).ToArray();
            charSpacing[1] = 30;
            charSpacing[2] = 22;
            charSpacing[3] = 22;
            Justify[] justification = Enumerable.Repeat(Justify.Left, columnCount).ToArray();
            Justify[] headerCenter = Enumerable.Repeat(Justify.Center, columnCount).ToArray();
            var styleParams = charSpacing.Zip(justification, (space, align) => (Spacing: space, Justification: align)).ToArray();
            var headerStyles = charSpacing.Zip(headerCenter, (space, align) => (Spacing: space, Justification: align)).ToArray();

            string columnDivider = "   ";
            string rowBookends = " ";
            int lineLength = charSpacing.Sum() + columnDivider.Length * (columnCount -1) + rowBookends.Length * 2;
            //string lineDivider = string.Join("",Enumerable.Repeat("*", lineLength));
            
            string[] dividerParts = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                dividerParts[i] = string.Join("", Enumerable.Repeat("=", charSpacing[i]));
            }
            string lineDivider = rowBookends + string.Join(columnDivider, dividerParts) + rowBookends;
            
            //+ string.Join(columnDivider, Enumerable.Repeat("_"))

            
            string[] headers = table.GetColumnHeaders();
            List<string> rows = new List<string>
            {
                lineDivider,
                table.EnumerateColumnHeaders(headerStyles, columnDivider, rowBookends),
                lineDivider
            };
            object[,] array = table.GetArray(table.GetRowCount());
            string[,] stringArray = array.ToStringArray();
            
            for (int i = 0; i < stringArray.GetLength(0); i++)
            {
                string[] row = stringArray.SliceRow(i).ToArray();
                rows.Add(row.JoinFixedWidth(styleParams, columnDivider, rowBookends));
            }
            
            rows.Add(lineDivider);
            string output = string.Join("\n", rows.ToArray());
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(output);
        }
        
        public static string[] GetColumnHeaders(this Outlook.Table table)
        {
            string[] headers = new string[table.Columns.Count];
            int i = -1;
            foreach (Column column in table.Columns)
            {
                string name = column.Name;
                if (SchemaToField.ContainsKey(name))
                    name = SchemaToField[name];
                headers[++i] = name;
            }
            return headers;
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
