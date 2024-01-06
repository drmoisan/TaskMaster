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
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Windows.Input;

    /// <summary>
    /// Class written to transform Dataframe objects for printing
    /// </summary>
    public static class PrettyPrinters
    {
        public static void PrettyPrint(this DataFrame df) => Console.WriteLine(PrettyText(df));
        public static void PrettyPrint(this DataFrameRow row) => Console.WriteLine(Pretty(row));

        public static string PrettyText(this DataFrame df) => ToStringArray2D(df).ToFormattedText();

        public static string PrettyText<TKey,TValue>(this IDictionary<TKey,TValue> dict)
        {
            
            return "";
        }

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

        public static DataTable ArraytoDatatable(object[,] numbers)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < numbers.GetLength(1); i++)
            {
                dt.Columns.Add("Column" + (i + 1));
            }

            for (var i = 0; i < numbers.GetLength(0); ++i)
            {
                DataRow row = dt.NewRow();
                for (var j = 0; j < numbers.GetLength(1); ++j)
                {
                    row[j] = numbers[i, j];
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static DataTable ArraytoDatatable(object[,] numbers, string[] headers)
        {
            if (numbers.GetLength(1) != headers.Length)
            {
                throw new ArgumentException($"Number of headers {headers.Length} " +
                    $"must match number of columns {numbers.GetLength(1)}");
            }

            DataTable dt = new DataTable();
            for (int i = 0; i < numbers.GetLength(1); i++)
            {
                dt.Columns.Add(headers[i]);
            }

            for (var i = 0; i < numbers.GetLength(0); ++i)
            {
                DataRow row = dt.NewRow();
                for (var j = 0; j < numbers.GetLength(1); ++j)
                {
                    row[j] = numbers[i, j];
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        internal static int[] GetMaxLengthsByColumn(this string[,] strings)
        {
            int[] maxLengthsByColumn = new int[strings.GetLength(1)];

            for (int y = 0; y < strings.GetLength(0); y++)
                for (int x = 0; x < strings.GetLength(1); x++)
                    maxLengthsByColumn[x] = Math.Max(maxLengthsByColumn[x], strings[y, x].Length);

            return maxLengthsByColumn;
        }

        internal static int[] GetMaxLengthsByColumn<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            int[] columnLengths = new int[2];

            columnLengths[0] = dict.Keys.Select(key => key.ToString().Length).Max();
            columnLengths[1] = dict.Values.Select(value => value.ToString().Length).Max();

            return columnLengths;
        }

        private static readonly string[] _aggregators = ["total", "subtotal","average",
            "mean","median","min","max","stddev", "variance", "count", 
            "sum", "mode", "range","skewness", "kurtosis", "percentile", "quartile"];

        public static string ToFormattedText(this IDictionary<string, float> dict, float decimalPlaces)
        {
            var keyConverter = new Func<string, string>(key => key);
            var valueConverter = new Func<float, string>(value => value.ToString($"N{decimalPlaces}"));
            return dict.ToFormattedText(keyConverter, valueConverter);
        }

        public static string ToFormattedText(this IDictionary<string, long> dict)
        {
            var keyConverter = new Func<string, string>(key => key);
            var valueConverter = new Func<long, string>(value => value.ToString($"N0"));
            return dict.ToFormattedText(keyConverter, valueConverter);
        }

        public static string ToFormattedText<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, string> keyConverter, Func<TValue, string> valueConverter)
        {
            int[] columnLengths = dict.GetMaxLengthsByColumn();

            var texts = dict.Select(kvp => 
                $"{keyConverter(kvp.Key).PadRight(columnLengths[0])} " +
                $"{valueConverter(kvp.Value).PadLeft(columnLengths[1])}").ToArray();
            var text = string.Join(Environment.NewLine, texts);
            return text;
        }

        public static string ToFormattedText(this string[][] jagged, string[] headers = null, string title = null) 
        {
            // Get the max number of columns
            var columnCount = jagged.GroupBy(row => row.Length).Select(x => x.Key).Max();
            
            // Get the max length of each data column
            var columnLengths = new int[columnCount];
            jagged.ForEach(row => 
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (row[i] is null) { row[i] = ""; }
                    columnLengths[i] = Math.Max(columnLengths[i], row[i].Length);
                }
            });
            StringBuilder sb = new();

            // Adjust Column widths for headers
            if (headers != null)
            {
                Enumerable.Range(0, columnCount).ForEach(i => columnLengths[i] = Math.Max(columnLengths[i], headers[i].Length));
            }
            
            // Calculate Table Width
            int width = columnLengths.Sum() + columnLengths.Length * 2 + 3;

            if (!title.IsNullOrEmpty()) 
            {
                sb.AppendLine(new string('=', width));
                if (title.Length + 3 <= width)
                {                    
                    sb.AppendLine($"| {title.PadToCenter(width - 3)}|");   
                }
                else
                {
                    var rx = new Regex(@"([^ ]+)");
                    var matches = rx.Matches(title).Cast<Match>().Select(m => m.Groups[1].Value);
                    List<string> lines = new();
                    StringBuilder line = new();
                    foreach (var match in matches)
                    {
                        if (line.Length + match.Length + 4 <= width)
                        {
                            line.Append(" ");
                            line.Append(match);
                        }
                        else 
                        {
                            lines.Add($"| {line.ToString().PadToCenter(width - 3)}|");
                            line.Clear();
                        }
                        
                    }
                    if (line.Length > 0)
                        lines.Add($"| {line.ToString().PadToCenter(width - 3)}|");
                    lines.ForEach(line => sb.AppendLine(line));
                }
            }

            if (headers != null) 
            { 
                if (title.IsNullOrEmpty())
                    sb.AppendLine(new string('=', width));
                sb.Append("| ");
                for (int i = 0; i < headers.Length; i++) 
                {
                    sb.Append(headers[i].PadRight(columnLengths[i] + 2));
                }
                sb.AppendLine("|");
            }
                                    
            sb.AppendLine(new string('=', width));

            for (int i = 0; i < jagged.Length; i++)
            {
                var isAggregator = _aggregators.Contains(jagged[i][0].ToLower());
                if (isAggregator) { sb.AppendLine(new string('_', width));}

                sb.Append("| ");
                for (int j = 0; j < jagged[i].Length; j++)
                {
                    sb.Append(jagged[i][j].PadRight(columnLengths[j] + 2));
                }
                sb.AppendLine("|");

                if (isAggregator) { sb.AppendLine(new string('_', width)); }
            }
            sb.AppendLine(new string('=', width));

            return sb.ToString();

        }

        public static string ToFormattedText(this string[,] strings)
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

        public static string ToMarkdown(this string[,] strings)
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
        public static void DisplayDialog(this DataTable table)
        {
            DgvForm dfViewer = new DgvForm();
            dfViewer.Show();
            
            int diffHeight = dfViewer.Height - dfViewer.Dgv.Height;
            int diffWidth = dfViewer.Width - dfViewer.Dgv.Width;
            dfViewer.Dgv.Dock = DockStyle.None;

            dfViewer.Dgv.DataSource = table;

            
            for (int i = 0; i < dfViewer.Dgv.Columns.Count - 1; i++)
            {
                dfViewer.Dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            dfViewer.Dgv.Columns[dfViewer.Dgv.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


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

            dfViewer.Hide();
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
            dfViewer.ShowDialog();
        }
    }
}
