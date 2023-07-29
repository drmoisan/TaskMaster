using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    using Microsoft.Data.Analysis;
    using Microsoft.Office.Interop.Outlook;
    using System.Data;
    using System.Diagnostics;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Class written to transform Dataframe objects for printing
    /// </summary>
    public static class PrettyPrinters
    {
        public static void PrettyPrint(this DataFrame df) => Console.WriteLine(PrettyText(df));
        public static void PrettyPrint(this DataFrameRow row) => Console.WriteLine(Pretty(row));

        public static string PrettyText(this DataFrame df) => ToStringArray2D(df).ToFormattedText();

        public static string Pretty(this DataFrameRow row) => row.Select(x => x?.ToString() ?? string.Empty).StringJoin();

        public static string ToMarkdown(this DataFrame df) => ToStringArray2D(df).ToMarkdown();
        
        private static string StringJoin(this IEnumerable<string> strings) => string.Join(" ", strings.Select(x => x.ToString()));

        private static string[,] ToStringArray2D(DataFrame df)
        {
            string[,] strings = new string[df.Rows.Count + 1, df.Columns.Count];

            for (int i = 0; i < df.Columns.Count; i++)
                strings[0, i] = df.Columns[i].Name;

            for (int i = 0; i < df.Rows.Count; i++)
                for (int j = 0; j < df.Columns.Count; j++)
                    strings[i + 1, j] = df[i, j]?.ToString() ?? string.Empty;

            return strings;
        }

        private static int[] GetMaxLengthsByColumn(this string[,] strings)
        {
            int[] maxLengthsByColumn = new int[strings.GetLength(1)];

            for (int y = 0; y < strings.GetLength(0); y++)
                for (int x = 0; x < strings.GetLength(1); x++)
                    maxLengthsByColumn[x] = Math.Max(maxLengthsByColumn[x], strings[y, x].Length);

            return maxLengthsByColumn;
        }

        private static string ToFormattedText(this string[,] strings)
        {
            StringBuilder sb = new();
            int[] maxLengthsByColumn = GetMaxLengthsByColumn(strings);

            for (int y = 0; y < strings.GetLength(0); y++)
            {
                for (int x = 0; x < strings.GetLength(1); x++)
                {
                    sb.Append(strings[y, x].PadRight(maxLengthsByColumn[x] + 2));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string ToMarkdown(this string[,] strings)
        {
            StringBuilder sb = new();
            int[] maxLengthsByColumn = GetMaxLengthsByColumn(strings);

            for (int y = 0; y < strings.GetLength(0); y++)
            {
                for (int x = 0; x < strings.GetLength(1); x++)
                {
                    sb.Append(strings[y, x].PadRight(maxLengthsByColumn[x]));
                    if (x < strings.GetLength(1) - 1)
                        sb.Append(" | ");
                }
                sb.AppendLine();

                if (y == 0)
                {
                    for (int i = 0; i < strings.GetLength(1); i++)
                    {
                        int bars = maxLengthsByColumn[i] + 2;
                        if (i == 0)
                            bars -= 1;
                        sb.Append(new String('-', bars));

                        if (i < strings.GetLength(1) - 1)
                            sb.Append("|");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public static void Display(this DataTable table)
        {
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
    }
}
