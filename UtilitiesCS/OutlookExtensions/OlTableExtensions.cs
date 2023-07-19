using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deedle.Internal;
using Microsoft.Office.Interop.Outlook;
using static UtilitiesCS.ConvHelper;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class OlTableExtensions
    {
        #region Property Schema Constants

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
        const string PR_STORE_RECORD_KEY = "0x0FFA"; //
        const string PR_CONVERSATION_TOPIC = "0x0070"; // Normalized Conversation Subject for message group
        const string PR_PARENT_DISPLAY = "0x0e05"; //Message parent folder
        const string PR_DEPTH = "0x3005"; /* Represents the relative level of indentation, 
                                           * or depth, of an object in a hierarchical table
                                           * Data type is PT_LONG */
        const string PR_CONVERSATION_INDEX = "0x0071"; /* PT_BINARY ScCreateConversationIndex 
                                                        * implements the index as a header block 
                                                        * that is 22 bytes in length, followed 
                                                        * by zero or more child blocks each 
                                                        * 5 bytes in length */

        public static string SchemaConversationTopic = PROPTAG_SPECIFIER + PR_CONVERSATION_TOPIC + PT_TSTRING;
        public static string SchemaFolderName = PROPTAG_SPECIFIER + PR_PARENT_DISPLAY + PT_TSTRING;
        public static string SchemaMessageStore = PROPTAG_SPECIFIER + PR_STORE_ENTRYID + PT_BINARY;
        public static string SchemaConversationDepth = PROPTAG_SPECIFIER + PR_DEPTH + PT_LONG;
        public static string SchemaConversationIndex = PROPTAG_SPECIFIER + PR_CONVERSATION_INDEX + PT_BINARY;
        public static string SchemaTriage = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage";
        public static string SchemaToDoID = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/ToDoID";

        public static Dictionary<string, string> SchemaToField = new()
        {
            {SchemaFolderName, "Folder Name" },
            {SchemaMessageStore, "Store"},
            {SchemaConversationDepth, "ConvDepth" },
            {SchemaConversationIndex, "ConversationIndex" },
            {SchemaConversationTopic, "ConversationTopic" },
            {SchemaToDoID, "ToDoID" },
            {SchemaTriage, "Triage" }
        };
        public static Dictionary<string, string> FieldToSchema = new()
        {
            {"Folder Name", SchemaFolderName },
            {"Store", SchemaMessageStore},
            {"ConvDepth", SchemaConversationDepth },
            {"ConversationIndex", SchemaConversationIndex },
            {"ConversationTopic", SchemaConversationTopic },
            {"ToDoID", SchemaToDoID },
            {"Triage", SchemaTriage }
        };

        #endregion

        /// <summary>
        /// Extension method that removes all columns in the supplied array 
        /// from an Outlook Table object
        /// </summary>
        /// <param name="table">Outlook table object</param>
        /// <param name="columnNames">Array of column names to remove</param>
        public static void RemoveColumns(this Outlook.Table table, string[] columnNames)
        {
            if (table is not null)
            {
                if ((columnNames is not null)&&(columnNames.Count() > 0))
                {
                    if (columnNames[0].ToLower() == "removeall") { table.Columns.RemoveAll(); }
                    else { foreach (var column in columnNames) { table.Columns.Remove(column); } }   
                }  
            }
        }

        /// <summary>
        /// Extension method that iterates through an array of column names or
        /// schemas and adds the corresponding columns to an Outlook Table object
        /// </summary>
        /// <param name="table">Source Table</param>
        /// <param name="columnNames">Array of column names or schemas</param>
        public static void AddColumns(this Outlook.Table table, string[] columnNames)
        {
            if (table is not null)
            {
                foreach (var column in columnNames) { table.Columns.Add(column); }
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
            var kvps = Enumerable.Range(1, table.Columns.Count)
                                 .Select(i =>
                                 {
                                     var name = table.Columns[i].Name;
                                     if (SchemaToField.TryGetValue(name, out var adjustedName))
                                     { return new KeyValuePair<string, int>(adjustedName, i -1); }
                                     else { return new KeyValuePair<string, int>(name, i - 1); }
                                 });
                                 
            Dictionary<string, int> dict = new();
            try 
            {
                dict = kvps.ToDictionary();
            }
            catch (System.InvalidOperationException ex)
            {
                foreach(var kvp in kvps)
                {
                    if (!dict.ContainsKey(kvp.Key)) { dict.Add(kvp.Key, kvp.Value); }
                    else { dict[$"{kvp.Key}{kvp.Value}"] = kvp.Value; }                        
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
        public static (object[,] data, Dictionary<string, int> columnInfo) ExtractData(this Outlook.Table table)
        {
            var columnDictionary = table.GetColumnDictionary();
            var rowCount = table.GetRowCount();
            var columnCount = columnDictionary.Count;
            //EnumerateTable(table);
            table.MoveToStart();
            object[,] data = null;

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
                        if (j == storeIndex) { data[i,j] = storeID; }
                        else { data[i, j] = values[j]; }
                    }
                }
            }
            else { data = (object[,])table.GetArray(rowCount); }
            return (data, columnDictionary);
        }

        /// <summary>
        /// Get an Outlook <seealso cref="Table"/> object representing items that are
        /// visible in the current view within the active instance of the 
        /// Outlook.<seealso cref="Explorer"/>. Throws an exception if the current 
        /// view cannot be cast to Outlook.<seealso cref="TableView"/> type. 
        /// </summary>
        /// <param name="activeExplorer">Object representing the active <seealso cref="Explorer"/></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Outlook.Table GetTableInView(this Explorer activeExplorer)
        {
            Outlook.TableView view = activeExplorer.CurrentView as Outlook.TableView;
            if (view is null)
            {
                throw new InvalidOperationException(
                    $"Current view in Outlook, {((Outlook.View)activeExplorer.CurrentView).Name}," +
                    $" cannot be cast to {nameof(Outlook.TableView)}");
            }
            return view.GetTable();
        }

        public static Outlook.Table GetTable(this Store store, OlDefaultFolders folderEnum, string[] removeColumns, string[] addColumns)
        {
            if (store is null) { throw new ArgumentNullException(nameof(store)); }
            var folder = store.GetDefaultFolder(folderEnum);
            return folder.GetTable(removeColumns: removeColumns, addColumns: addColumns);
        }

        public static Outlook.Table GetTable(this MAPIFolder folder, string[] removeColumns, string[] addColumns)
        {
            var table = folder.GetTable();
            table.RemoveColumns(removeColumns);
            table.AddColumns(addColumns);
            return table;
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

        public static void EnumerateTable(this Outlook.Table table)
        {
            int columnCount = table.Columns.Count;
            int[] charSpacing = Enumerable.Repeat(20, columnCount).ToArray();
            Justify[] justification = Enumerable.Repeat(Justify.Left, columnCount).ToArray();
            Justify[] headerCenter = Enumerable.Repeat(Justify.Center, columnCount).ToArray();
            var styleParams = charSpacing.Zip(justification, (space, align) => (Spacing: space, Justification: align)).ToArray();
            var headerStyles = charSpacing.Zip(headerCenter, (space, align) => (Spacing: space, Justification: align)).ToArray();

            string columnDivider = "   ";
            string rowBookends = " ";
            int lineLength = charSpacing.Sum() + columnDivider.Length * (columnCount - 1) + rowBookends.Length * 2;
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
