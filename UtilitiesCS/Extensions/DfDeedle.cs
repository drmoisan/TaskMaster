using Deedle;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.OutlookExtensions;
using System.Data;

namespace UtilitiesCS
{
    public static class DfDeedle
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Frame<int, string> GetEmailDataInView(Explorer activeExplorer)
        {
            Outlook.Table table = activeExplorer.GetTableInView();
            var storeID = activeExplorer.CurrentFolder.StoreID;
            table.Columns.Add("SentOn");
            table.Columns.Add(OlTableExtensions.SchemaConversationId);
            table.Columns.Add(OlTableExtensions.SchemaTriage);
            table.Columns.Remove("Subject");
            table.Columns.Remove("CreationTime");
            table.Columns.Remove("LastModificationTime");

            (object[,] data, Dictionary<string, int> columnInfo) = table.ETL();
            
            var records = Enumerable.Range(0, data.GetLength(0)).Select(i =>
            {
                DateTime sentOn = DateTime.MaxValue;
                var dateField = data[i, columnInfo["SentOn"]];
                if (dateField is not null) { DateTime.TryParse(dateField.ToString(), out sentOn); }
                if (dateField is null) { sentOn = DateTime.MaxValue; }
                
                return new
                {
                    EntryId = data[i, columnInfo["EntryID"]],
                    MessageClass = data[i, columnInfo["MessageClass"]].ToString(),
                    SentOn = sentOn,
                    ConversationId = data[i, columnInfo["ConversationId"]],
                    Triage = (string)data[i, columnInfo["Triage"]] ?? "Z",
                    StoreId = storeID
                };
            });

            //string[,] strAry = new string[records.Count(), 6];
            //var r2 = records.ToList();
            //Enumerable.Range(0, data.GetLength(0)).ForEach(i =>
            //{
            //    strAry[i,0] = r2[i].EntryId.ToString();
            //    strAry[i, 1] = r2[i].MessageClass.ToString();
            //    strAry[i, 2] = r2[i].SentOn.ToString();
            //    strAry[i, 3] = r2[i].ConversationId.ToString();
            //    strAry[i, 4] = r2[i].Triage.ToString();
            //    strAry[i, 5] = r2[i].StoreId.ToString();
            //});
            //logger.Debug(strAry.ToFormattedText());

            var df = Frame.FromRecords(records);
            
            return df;
        }

        internal static Series<int, string> GetColumnEid(object[] slice)
        {
            return slice.CastNullSafe<string>().ToOrdinalSeries();
        }

        internal static object GetFirstNonNull(IEnumerable<object> columnData)
        {
            if ((columnData is null) || (columnData.Count() == 0)) { return null; }

            var filteredData = columnData.Where(x => x is not null).ToArray();
            if ((filteredData is null) || (filteredData.Count() == 0)) { return null; }

            return filteredData.First();
        }

        public static Frame<int, string> FromArray2D(object[,] data, Dictionary<string, int> columnDictionary)
        {
            var rows = Enumerable.Range(0, data.GetLength(0)).Select(i =>
            {
                var sb = new SeriesBuilder<string>();
                foreach (var key in columnDictionary.Keys)
                {
                    var value = data[i, columnDictionary[key]];
                    sb.Add(key, value);
                }
                return KeyValue.Create(i, sb.Series);
            });
            var dfTemp = Frame.FromRows(rows);
            return dfTemp;
        }

        public static Frame<int, string> FromDefaultFolder(Store store,
                                                           OlDefaultFolders folderEnum,
                                                           string[] removeColumns,
                                                           string[] addColumns)
        {
            var table = store.GetTable(folderEnum: folderEnum,
                                       removeColumns: removeColumns,
                                       addColumns: addColumns);

            (var data, var columnInfo) = table.ETL();

            Frame<int, string> df = FromArray2D(data: data, columnInfo);

            return df;   
        }

        public static Frame<int, string> FromDefaultFolder(Stores stores,
                                                           OlDefaultFolders folderEnum,
                                                           string[] removeColumns,
                                                           string[] addColumns)
        {
            Frame<int, string> df = null;
            foreach (Outlook.Store store in stores)
            {
                var dfTemp = DfDeedle.FromDefaultFolder(store: store,
                                                        folderEnum: folderEnum,
                                                        removeColumns: removeColumns,
                                                        addColumns: addColumns);
                if (df is null) { df = dfTemp; }
                else if (dfTemp is not null) { df.Merge(dfTemp); }
            }
            return df;
        }

        public static void Display(this Frame<int, string> df, IEnumerable<string> rowKeyNames)
        {
            DataTable table = df.ToDataTable(rowKeyNames);
            table.Display();
        }

        //public static  GetDfColumn(string columnName, object[] columnData)
        //{
        //    object T = GetFirstNonNull(columnData);
        //    if (T is string) { return new StringDataFrameColumn(columnName, columnData.CastNullSafe<string>().ToArray()); }
        //    else if (T is bool) { return new PrimitiveDataFrameColumn<bool>(columnName, columnData.CastNullSafe<bool>().ToArray()); }
        //    else if (T is byte) { return new PrimitiveDataFrameColumn<byte>(columnName, columnData.CastNullSafe<byte>().ToArray()); }
        //    else if (T is sbyte) { return new PrimitiveDataFrameColumn<sbyte>(columnName, columnData.CastNullSafe<sbyte>().ToArray()); }
        //    else if (T is char) { return new PrimitiveDataFrameColumn<char>(columnName, columnData.CastNullSafe<char>().ToArray()); }
        //    else if (T is decimal) { return new PrimitiveDataFrameColumn<decimal>(columnName, columnData.CastNullSafe<decimal>().ToArray()); }
        //    else if (T is double) { return new PrimitiveDataFrameColumn<double>(columnName, columnData.CastNullSafe<double>().ToArray()); }
        //    else if (T is float) { return new PrimitiveDataFrameColumn<float>(columnName, columnData.CastNullSafe<float>().ToArray()); }
        //    else if (T is int) { return new PrimitiveDataFrameColumn<int>(columnName, columnData.CastNullSafe<int>().ToArray()); }
        //    else if (T is uint) { return new PrimitiveDataFrameColumn<uint>(columnName, columnData.CastNullSafe<uint>().ToArray()); }
        //    else if (T is nint) { return new PrimitiveDataFrameColumn<nint>(columnName, columnData.CastNullSafe<nint>().ToArray()); }
        //    else if (T is nuint) { return new PrimitiveDataFrameColumn<nuint>(columnName, columnData.CastNullSafe<nuint>().ToArray()); }
        //    else if (T is long) { return new PrimitiveDataFrameColumn<long>(columnName, columnData.CastNullSafe<long>().ToArray()); }
        //    else if (T is ulong) { return new PrimitiveDataFrameColumn<ulong>(columnName, columnData.CastNullSafe<ulong>().ToArray()); }
        //    else if (T is short) { return new PrimitiveDataFrameColumn<short>(columnName, columnData.CastNullSafe<short>().ToArray()); }
        //    else if (T is ushort) { return new PrimitiveDataFrameColumn<ushort>(columnName, columnData.CastNullSafe<ushort>().ToArray()); }
        //    else { return new StringDataFrameColumn(columnName, columnData.ToStringArray(nullReplacement: "")); }
        //}



        //internal static object GetFirstNonNull(object[] columnData)
        //{
        //    if ((columnData is null) || (columnData.Length == 0)) { return null; }

        //    var filteredData = columnData.Where(x => x is not null).ToArray();
        //    if ((filteredData is null) || (filteredData.Length == 0)) { return null; }

        //    return filteredData.First();
        //}
    }
}
