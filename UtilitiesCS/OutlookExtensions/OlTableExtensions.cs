using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deedle.Internal;
using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using static UtilitiesCS.ConvHelper;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class OlTableExtensions
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        const string PT_STRING8 = "001e"; /* Null-terminated 8-bit (1-byte) character string. 
                                           * This property type is the same as the OLE type VT_LPSTR */

        const string PR_RECEIVED_BY_NAME = "0x0040"; //PidTagReceivedByName
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

        const string PR_CONVERSATION_KEY = "0x000B"; // PT_BINARY
        const string PR_CONVERSATION_ID = "0x3013"; // PT_BINARY

        const string PR_MESSAGE_RECIPIENTS = "0x0e12";
        const string PR_SENDER_NAME = "0x0C1A"; // PT_TSTRING
        const string PR_SENDER_SMTP_ADDRESS = "0x5D01"; // PT_TSTRING
        const string PR_SENDER_ADDRTYPE = "0x0C1E"; // PT_TSTRING

        public static string SchemaConversationTopic = PROPTAG_SPECIFIER + PR_CONVERSATION_TOPIC + PT_TSTRING;
        public static string SchemaFolderName = PROPTAG_SPECIFIER + PR_PARENT_DISPLAY + PT_TSTRING;
        public static string SchemaMessageStore = PROPTAG_SPECIFIER + PR_STORE_ENTRYID + PT_BINARY;
        public static string SchemaConversationDepth = PROPTAG_SPECIFIER + PR_DEPTH + PT_LONG;
        public static string SchemaConversationIndex = PROPTAG_SPECIFIER + PR_CONVERSATION_INDEX + PT_BINARY;
        public static string SchemaTriage = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/Triage";
        public static string SchemaToDoID = "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/ToDoID";
        //public static string SchemaReceivedByName = PROPTAG_SPECIFIER + PR_RECEIVED_BY_NAME + PT_TSTRING;
        //public static string SchemaConversationId = "http://schemas.microsoft.com/mapi/proptag/0x30130102";
        public static string SchemaConversationId = PROPTAG_SPECIFIER + PR_CONVERSATION_ID + PT_BINARY;
        public static string SchemaSenderName = PROPTAG_SPECIFIER + PR_SENDER_NAME + PT_TSTRING;
        public static string SchemaSenderSmtpAddress = PROPTAG_SPECIFIER + PR_SENDER_SMTP_ADDRESS + PT_TSTRING;
        public static string SchemaSenderAddrType = PROPTAG_SPECIFIER + PR_SENDER_ADDRTYPE + PT_TSTRING;
        public static string SchemaReceivedByName = "http://schemas.microsoft.com/mapi/proptag/0x0040001E";
        public static string SchemaMessageRecipients = "http://schemas.microsoft.com/mapi/proptag/0x0E12000D";
        //public static string SchemaConversationKey = PROPTAG_SPECIFIER + PR_CONVERSATION_KEY + PT_BINARY; does not work

        public static Dictionary<string, string> SchemaToField = new()
        {
            {SchemaFolderName, "Folder Name" },
            {SchemaMessageStore, "Store"},
            {SchemaConversationDepth, "ConvDepth" },
            {SchemaConversationIndex, "ConversationIndex" },
            {SchemaConversationTopic, "ConversationTopic" },
            {SchemaConversationId, "ConversationId" },
            {SchemaToDoID, "ToDoID" },
            {SchemaTriage, "Triage" },
            {SchemaSenderName, "SenderName" },
            {SchemaSenderSmtpAddress, "SenderSmtpAddress" },
            {SchemaSenderAddrType, "SenderAddrType" },
            {SchemaReceivedByName, "ReceivedByName" },
            {SchemaMessageRecipients, "MessageRecipients" }
        };
        public static Dictionary<string, string> FieldToSchema = new()
        {
            {"Folder Name", SchemaFolderName },
            {"Store", SchemaMessageStore},
            {"ConvDepth", SchemaConversationDepth },
            {"ConversationIndex", SchemaConversationIndex },
            {"ConversationTopic", SchemaConversationTopic },
            {"ConversationId", SchemaConversationId },
            {"ToDoID", SchemaToDoID },
            {"Triage", SchemaTriage },
            {"SenderName", SchemaSenderName },
            {"SenderSmtpAddress", SchemaSenderSmtpAddress },
            {"SenderAddrType", SchemaSenderAddrType },
            {"ReceivedByName", SchemaReceivedByName },
            {"MessageRecipients", SchemaMessageRecipients }
        };

        public static List<string> BinaryToStringFields = new()
        {
            "ConversationIndex",
            "ConversationId",
            "Store"//,
            //"ReceivedByName"
        };

        public static List<string> ObjectFields = new()
        {
            "MessageRecipients"
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
            if (table is not null && columnNames is not null && columnNames.Count() > 0)
            {
                columnNames.ForEach(column => table.Columns.Remove(column));
            }
        }

        public static void RemoveColumns(this Outlook.Table table)
        {
            if (table is not null)
            {
                table.Columns.RemoveAll(); 
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
            catch (System.InvalidOperationException)
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
        public static (object[,] data, Dictionary<string, int> columnInfo) ExtractData2(this Outlook.Table table)
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
        /// Extract, transform, and load data from an Outlook Table object into a 2D object array
        /// </summary>
        /// <param name="table">Outlook.Table</param>
        /// <param name="objectConverters">Dictionary with column names and functions to convert the 
        /// object in the column into string representation</param>
        /// <returns>2D object array with string data</returns>
        public static (object[,] data, Dictionary<string, int> columnInfo) ETL(this Outlook.Table table,
                                                                               Dictionary<string, Func<object, string>> objectConverters = null)
        {
            //logger.Debug($"Calling {nameof(GetColumnDictionary)} ...");
            var columnDictionary = table.GetColumnDictionary();
            object[,] data = null;
            
            table.MoveToStart();

            if (BinaryToStringFields.Any(x => columnDictionary.ContainsKey(x))||
               (objectConverters is not null && 
               objectConverters.Keys.Any(x => columnDictionary.ContainsKey(x))))
            {
                //logger.Debug($"Calling {nameof(EtlByRow)} ...");
                data = EtlByRow(table, objectConverters, columnDictionary);
            }
            else { data = (object[,])table.GetArray(table.GetRowCount()); }
            return (data, columnDictionary);
        }

        public static async Task<(object[,] data, Dictionary<string, int> columnInfo)> EtlAsync(
            this Outlook.Table table,
            CancellationToken token,
            CancellationTokenSource tokenSource,
            int counter,
            Dictionary<string, Func<object, string>> objectConverters = null)
        {
            token.ThrowIfCancellationRequested();

            var rowCount = table.GetRowCount();
            int milliseconds = 250 * rowCount;
            var attempts = 3;
            object[,] data = null;
            Dictionary<string, int> columnInfo = null;

            //logger.Debug($"Calling {nameof(ETL)} with a timeout of {milliseconds.ToString("#,##0")}");
            try
            {
                (data, columnInfo) = await Task.Factory.StartNew(() => table.ETL(objectConverters),
                    token, TaskCreationOptions.LongRunning, TaskScheduler.Default).TimeoutAfter(milliseconds, attempts);
            }
            catch (TimeoutException)
            {
                logger.Error($"{nameof(ETL)} timed out {attempts} times with a timeout of {milliseconds} milliseconds. Canceling");
                tokenSource.Cancel();
            }
            
            
            return (data, columnInfo);
        }

        /// <summary>
        /// Extract, transform, and load data from an Outlook Table object into a 2D object 
        /// array by row to convert non-string data to string equivalents
        /// </summary>
        /// <param name="table">Outlook.Table</param>
        /// <param name="objectConverters">Dictionary with column names and functions to convert the 
        /// object in the column into string representation</param>
        /// <param name="columnDictionary">Dictionary with column indices and column names</param>
        /// <returns>2D object array with string data</returns>
        private static object[,] EtlByRow(Table table, Dictionary<string, Func<object, string>> objectConverters, Dictionary<string, int> columnDictionary)
        {
            //logger.Debug($"Setting up EtlByRow");
            
            // Get the column headers of the binary fields
            var binFields = BinaryToStringFields.Where(x => columnDictionary.ContainsKey(x));
            
            // Get the indices of the binary fields
            var binIndices = binFields.Select(x => columnDictionary[x]).OrderBy(x => x);

            (var objFields, var objIndices) = GetObjectFields(objectConverters, columnDictionary);

            // Cast the table to an IEnumerable of Outlook.Row
            //logger.Debug($"Casting {nameof(Outlook.Table)} to IEnumerable<{nameof(Outlook.Row)}");
            var rows = table.GetRows().ToArray();

            //logger.Debug($"Converting rows to jagged array of object[]");
            var query = Enumerable.Range(0, rows.Count());
            if (rows is not null && rows.Count() > 200)
            {
                query = query.AsParallel();
            }
            var jagged = query.Select(i => EtlRow(rows.ElementAt(i), 
                                                  objectConverters, 
                                                  columnDictionary, 
                                                  binIndices, 
                                                  objFields, 
                                                  objIndices, 
                                                  i)).ToArray();
            //logger.Debug($"Converting jagged array of object[] to 2D object array");
            var data = jagged.To2D();

            // Create the data array to hold the extracted data
            // var data = new object[table.GetRowCount(), columnDictionary.Count];
            //
            // Iterate over each row in the Outlook.Table
            // int rowNumber = -1;//
            //while (!table.EndOfTable)
            //{
            //    logger.Debug($"Getting row {rowNumber}");
            //    Outlook.Row row = table.GetNextRow();
            //    EtlRow(ref data, row, objectConverters, columnDictionary, binIndices, objFields, objIndices, ++rowNumber);
            //}

            return data;
        }

        /// <summary>
        /// Check if objectConverters is null and if not, get the object fields and indices
        /// </summary>
        /// <param name="objectConverters">Dictionary with column names and functions to convert the 
        /// object in the column into string representation</param>
        /// <param name="columnDictionary">Dictionary with column indices and column names</param>
        /// <returns>Tuple with IEnumerable of Field names with object data and 
        /// IEnumerable of indices of columns with object data</returns>
        private static (IEnumerable<string>, IEnumerable<int>) GetObjectFields(
            Dictionary<string, Func<object, string>> objectConverters, 
            Dictionary<string, int> columnDictionary)
        {
            if (objectConverters is null) { return (null, null); }
            else
            {
                // Get the column headers of the object fields
                var objFields = objectConverters.Keys.Where(x => columnDictionary.ContainsKey(x));

                // Get the indices of the object fields
                var objIndices = objFields.Select(x => columnDictionary[x]);
                
                return (objFields, objIndices);
            }
        }

        /// <summary>
        /// Extract, Transform, and Load a row of data from an Outlook.Table into a 
        /// 2D object array containing the data from all rows
        /// </summary>
        /// <param name="data">Reference to 2D object array</param>
        /// <param name="row">Outlook.Row</param>
        /// <param name="objectConverters">Dictionary with column names and functions to convert the 
        /// object in the column into string representation</param>
        /// <param name="columnDictionary">Dictionary with column indices and column names</param>
        /// <param name="binIndices">Indices of columns with binary information</param>
        /// <param name="objFields">Field names with object data</param>
        /// <param name="objIndices">Indices of columns with object data</param>
        /// <param name="rowNumber">Zero based counter to map row to </param>
        private static void EtlRow(ref object[,] data, 
                                   Outlook.Row row, 
                                   Dictionary<string, Func<object, string>> objectConverters, 
                                   Dictionary<string, int> columnDictionary, 
                                   IOrderedEnumerable<int> binIndices, 
                                   IEnumerable<string> objFields,
                                   IEnumerable<int> objIndices, 
                                   int rowNumber)
        {
            object[] rawValues = (object[])row.GetValues();
            var binStrings = ConvertBinColumnsToString(row, binIndices);
            var objStrings = ConvertObjectColumnsToString(row, objIndices, objFields, objectConverters);
            WriteValuesToData(ref data, columnDictionary, binIndices, rowNumber, binStrings, objIndices, objStrings, rawValues);
        }

        private static object[] EtlRow(Outlook.Row row,
                                   Dictionary<string, Func<object, string>> objectConverters,
                                   Dictionary<string, int> columnDictionary,
                                   IOrderedEnumerable<int> binIndices,
                                   IEnumerable<string> objFields,
                                   IEnumerable<int> objIndices,
                                   int rowNumber)
        {
            object[] rawValues = (object[])row.GetValues();
            var binStrings = ConvertBinColumnsToString(row, binIndices);
            var objStrings = ConvertObjectColumnsToString(row, objIndices, objFields, objectConverters);
            return rawValues.ToObjectRow(binIndices, binStrings, objIndices, objStrings);
        }

        /// <summary>
        /// Load transformed values into the data array
        /// </summary>
        /// <param name="data">Reference to 2D object array</param>
        /// <param name="columnDictionary">Dictionary with column indices and column names</param>
        /// <param name="binIndices">Indices of columns with binary information</param>
        /// <param name="rowNumber">Zero based counter to map row to </param>
        /// <param name="binStrings">Dictionary of column indices and string representation of binary data</param>
        /// <param name="objIndices">Indices of columns with object data</param>
        /// <param name="objStrings">Dictionary of column indices and string representation of object data</param>
        /// <param name="rawValues">Raw values obtained from table</param>
        internal static void WriteValuesToData(ref object[,] data, 
                                               Dictionary<string, int> columnDictionary, 
                                               IOrderedEnumerable<int> binIndices, 
                                               int rowNumber, 
                                               Dictionary<int, string> binStrings,
                                               IEnumerable<int> objIndices,
                                               Dictionary<int, string> objStrings,
                                               object[] rawValues)
        {
            for (int j = 0; j < columnDictionary.Count; j++)
            {
                if ((binIndices is not null) && binIndices.Contains(j)) { data[rowNumber, j] = binStrings[j]; }
                else if (objIndices is not null && objIndices.Contains(j)) { data[rowNumber, j] = objStrings[j]; }
                else { data[rowNumber, j] = rawValues[j]; }
            }
            //logger.Debug(data.SliceRow(rowNumber).Select(x => (x ?? "null").ToString()).SentenceJoin());
        }

        internal static object[] ToObjectRow(this object[] rawValues,
                                             IOrderedEnumerable<int> binIndices,
                                             Dictionary<int, string> binStrings,
                                             IEnumerable<int> objIndices,
                                             Dictionary<int, string> objStrings)
        {
            if (binIndices is not null) { binIndices.ForEach(i => rawValues[i] = binStrings[i]); }
            if (objIndices is not null) { objIndices.ForEach(i => rawValues[i] = objStrings[i]); }

            //logger.Debug(rawValues.Select(x => (x ?? "null").ToString()).SentenceJoin());
            return rawValues;
        }

        /// <summary>
        /// Transform binary columns into string representations
        /// </summary>
        /// <param name="row">Outlook.Row</param>
        /// <param name="binIndices">Indices of columns with binary information</param>
        /// <returns>Dictionary of column indices and string representation of binary data</returns>
        internal static Dictionary<int,string> ConvertBinColumnsToString(
            Outlook.Row row, 
            IOrderedEnumerable<int> binIndices)
        {
            return binIndices.Select(binIndex =>
                        new KeyValuePair<int, string>(
                            binIndex,
                            row.BinaryToString(binIndex + 1)))
                        .ToDictionary();
        }

        /// <summary>
        /// Transform object columns into string representations
        /// </summary>
        /// <param name="row">Outlook.Row</param>
        /// <param name="objIndices">Indices of columns with object data</param>
        /// <param name="objFields">Field names with object data</param>
        /// <param name="objectConverters">Dictionary with column names and functions to convert the 
        /// object in the column into string representation</param>
        /// <returns>Dictionary of column indices and string representation of object data</returns>
        internal static Dictionary<int, string> ConvertObjectColumnsToString(
            Outlook.Row row,
            IEnumerable<int> objIndices,
            IEnumerable<string> objFields,
            Dictionary<string, Func<object, string>> objectConverters)
        {
            var objStrings = new Dictionary<int, string>();
            if(objectConverters is not null && objIndices is not null && objFields is not null)
            {
                foreach (var objIndex in objIndices)
                {
                    var element = row[objIndex + 1];
                    var objString = objectConverters[objFields.ElementAt(objIndex)](element);
                    objStrings[objIndex] = objString;
                }
            }
            return objStrings;
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

        public static async Task<Outlook.Table> GetTableInViewAsync(this Explorer activeExplorer, CancellationToken token, int counter)
        {
            Outlook.Table table = null;
            Outlook.TableView view = activeExplorer.CurrentView as Outlook.TableView;
            if (view is null)
            {
                throw new InvalidOperationException(
                    $"Current view in Outlook, {((Outlook.View)activeExplorer.CurrentView).Name}," +
                    $" cannot be cast to {nameof(Outlook.TableView)}");
            }
            
            try
            {
                table = await Task.Factory.StartNew(
                    () => view.GetTable(), 
                    token, 
                    TaskCreationOptions.LongRunning, 
                    TaskScheduler.Default).TimeoutAfter(1000);

                //table = await Task.Run(() => view.GetTable(), combinedTokenSource.Token);
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
                        table = await activeExplorer.GetTableInViewAsync(token, counter+1);
                    }
                    else
                    {
                        table = null;
                    }
                } 
            }

            return table;
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

        public static Outlook.Table GetTable(this Conversation conversation, string[] removeColumns, string[] addColumns)
        {
            var table = conversation.GetTable();
            table.RemoveColumns(removeColumns);
            table.AddColumns(addColumns);
            return table;
        }

        public static IEnumerable<Outlook.Row> GetRows(this Outlook.Table table)
        {
            //int i = 0;
            table.MoveToStart();
            while (!table.EndOfTable)
            {
                //logger.Debug($"Getting row {i++}");
                yield return table.GetNextRow();
            }
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
