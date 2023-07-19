using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public static class DfMLNet
    {
        public static DataFrame ToDataFrame(this object[,] data, string[] columnNames)
        {
            if (data.GetLength(1) != columnNames.Length)
            {
                throw new ArgumentException($"data is {data.GetLength(1)} columns and column names is {columnNames.Length} columns. They must be of the same size");
            }

            List<DataFrameColumn> dfCols = new List<DataFrameColumn>();
            for (int j = 0; j < columnNames.Length; j++)
            {
                dfCols.Add(GetDfColumn(columnNames[j], data.SliceColumn<object>(j).ToArray()));
            }

            return new DataFrame(dfCols.ToArray());
        }

        public static DataFrameColumn GetDfColumn(string columnName, object[] columnData)
        {
            object T = GetFirstNonNull(columnData);
            if (T is string) { return new StringDataFrameColumn(columnName, columnData.CastNullSafe<string>().ToArray()); }
            else if (T is bool) { return new PrimitiveDataFrameColumn<bool>(columnName, columnData.CastNullSafe<bool>().ToArray()); }
            else if (T is byte) { return new PrimitiveDataFrameColumn<byte>(columnName, columnData.CastNullSafe<byte>().ToArray()); }
            else if (T is sbyte) { return new PrimitiveDataFrameColumn<sbyte>(columnName, columnData.CastNullSafe<sbyte>().ToArray()); }
            else if (T is char) { return new PrimitiveDataFrameColumn<char>(columnName, columnData.CastNullSafe<char>().ToArray()); }
            else if (T is decimal) { return new PrimitiveDataFrameColumn<decimal>(columnName, columnData.CastNullSafe<decimal>().ToArray()); }
            else if (T is double) { return new PrimitiveDataFrameColumn<double>(columnName, columnData.CastNullSafe<double>().ToArray()); }
            else if (T is float) { return new PrimitiveDataFrameColumn<float>(columnName, columnData.CastNullSafe<float>().ToArray()); }
            else if (T is int) { return new PrimitiveDataFrameColumn<int>(columnName, columnData.CastNullSafe<int>().ToArray()); }
            else if (T is uint) { return new PrimitiveDataFrameColumn<uint>(columnName, columnData.CastNullSafe<uint>().ToArray()); }
            else if (T is nint) { return new PrimitiveDataFrameColumn<nint>(columnName, columnData.CastNullSafe<nint>().ToArray()); }
            else if (T is nuint) { return new PrimitiveDataFrameColumn<nuint>(columnName, columnData.CastNullSafe<nuint>().ToArray()); }
            else if (T is long) { return new PrimitiveDataFrameColumn<long>(columnName, columnData.CastNullSafe<long>().ToArray()); }
            else if (T is ulong) { return new PrimitiveDataFrameColumn<ulong>(columnName, columnData.CastNullSafe<ulong>().ToArray()); }
            else if (T is short) { return new PrimitiveDataFrameColumn<short>(columnName, columnData.CastNullSafe<short>().ToArray()); }
            else if (T is ushort) { return new PrimitiveDataFrameColumn<ushort>(columnName, columnData.CastNullSafe<ushort>().ToArray()); }
            else { return new StringDataFrameColumn(columnName, columnData.ToStringArray(nullReplacement: "")); }
        }

        internal static object GetFirstNonNull(object[] columnData)
        {
            if ((columnData is null)||(columnData.Length == 0)) { return null; }

            var filteredData = columnData.Where(x => x is not null).ToArray();
            if ((filteredData is null) || (filteredData.Length == 0)) { return null; }

            return filteredData.First();
        }
    
        public static string[] GetNames(this DataFrameColumnCollection columns) => columns.Select(x => x.Name).ToArray();

        public static Type[] GetTypes(this DataFrameColumnCollection columns) => columns.Select(x => x.DataType).ToArray();

        public static DataTable ToDataTable(this DataFrame df) 
        {
            // Create new DataTable.
            DataTable table = new DataTable();
            
            // Create DataColumns from the definition in the Dataframe
            foreach (var dfColumn in df.Columns)
            {
                DataColumn tableColumn = new DataColumn(dfColumn.Name, dfColumn.DataType);
                table.Columns.Add(tableColumn);
            }

            string[] columnNames = df.Columns.GetNames();

            // Create DataRows from the rows in the Dataframe
            foreach (var dfRow in df.Rows)
            {
                DataRow tableRow = table.NewRow();
                foreach (string columnName in columnNames)
                {
                    int idx = Array.IndexOf(columnNames, columnName);
                    tableRow[columnName] = dfRow[idx];
                }
                //columnNames.Select(x => tableRow[x] = dfRow[Array.IndexOf(columnNames, x)]);
                table.Rows.Add(tableRow);
            }
            return table; 
        }

        public static void Display(this DataFrame df)
        {
            DataTable table = df.ToDataTable();
            DgvForm dfViewer = new DgvForm();
            
            int diffHeight = dfViewer.Height - dfViewer.Dgv.Height;
            int diffWidth = dfViewer.Width - dfViewer.Dgv.Width;
            dfViewer.Dgv.Dock = DockStyle.None;

            dfViewer.Dgv.DataSource = table;

            foreach (DataGridViewColumn dgvColumn in dfViewer.Dgv.Columns)
            {
                dgvColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            dfViewer.Show();
            int dgvWidth = 0;
            for (int i = 0; i <= dfViewer.Dgv.Columns.Count - 1; i++)
            {
                // Store Auto Sized Widths:
                int colw = dfViewer.Dgv.Columns[i].Width;

                // Remove AutoSizing:
                dfViewer.Dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                // Set Width to calculated AutoSize value:
                dfViewer.Dgv.Columns[i].Width = colw;
                dgvWidth += colw;
            }
            dfViewer.Dgv.Width = dgvWidth + dfViewer.Dgv.RowHeadersWidth;
            int lastRowHeight = dfViewer.Dgv.Rows[dfViewer.Dgv.Rows.Count - 1].Height;
            dfViewer.Dgv.Height = dfViewer.Dgv.Rows
                                  .Cast<DataGridViewRow>()
                                  .Select(row => row.Height)
                                  .Sum() + dfViewer.Dgv.ColumnHeadersHeight;
            //dfViewer.Width = dgvWidth + diffWidth + dfViewer.Dgv.RowHeadersWidth;
            dfViewer.Width = dfViewer.Dgv.Width + diffWidth + 6;
            dfViewer.Height = dfViewer.Dgv.Height + diffHeight + 6;
            dfViewer.Refresh();
            Debug.WriteLine($"Size is {dfViewer.Size.ToString()}");
            dfViewer.Dgv.Dock = DockStyle.Fill;
            
        }

        private static void MakeDataTableAndDisplay()
        {
            // Create new DataTable.
            DataTable table = new DataTable();

            // Declare DataColumn and DataRow variables.
            DataColumn column;
            DataRow row;

            // Create new DataColumn, set DataType, columnName
            // and add to DataTable.
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "id";
            table.Columns.Add(column);

            // Create second column.
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "item";
            table.Columns.Add(column);

            // Create new DataRow objects and add to DataTable.
            for (int i = 0; i < 10; i++)
            {
                row = table.NewRow();
                row["id"] = i;
                row["item"] = "item " + i;
                table.Rows.Add(row);
            }
            // Set to DataGrid.DataSource property to the table.
            //dataGrid1.DataSource = table;
        }

        //else { return new ObjectDataFrameColumn(columnName, new object[rowCount]); }
        //DataFrameColumn column = null;
        //if (columnData[0] is string)
        //{
        //    return new StringDataFrameColumn(columnName, columnData.Cast<string>().ToArray());
        //}
        //else
        //{
        //    Type columnDataType = columnData[0].GetType();
        //    // Use reflection to create an instance of the PrimitiveDataFrameColumn<T> class with the correct type parameter
        //    Type columnType = typeof(PrimitiveDataFrameColumn<>).MakeGenericType(columnDataType);
        //    column = (DataFrameColumn)Activator.CreateInstance(columnType, columnName);
        //    for (int i = 0; i < columnData.Length; i++)
        //    {
        //        column[i] = Convert.ChangeType(columnData[i], columnDataType);
        //    }
        //}
        //return column;
    }
}

