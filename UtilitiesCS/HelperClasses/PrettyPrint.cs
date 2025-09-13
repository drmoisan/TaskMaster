using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    using Microsoft.Data.Analysis;
    using Microsoft.Office.Interop.Outlook;
    using Svg;
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

        public static string Pretty(this DataFrameRow row) => row.Select(x => x?.ToString() ?? string.Empty).StringJoin(" ");

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
            DataTable dt = new();
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

            DataTable dt = new();
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
            int[] columnLengths =
            [
                dict.Keys.Select(key => key.ToString().Length).Max(),
                dict.Values.Select(value => value.ToString().Length).Max(),
            ];
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

        public static string ToFormattedText<TKey, TValue>(
            this IDictionary<TKey, TValue> dict, 
            Func<TKey, string> keyConverter, 
            Func<TValue, string> valueConverter, 
            string[] headers = null, 
            Enums.Justification[] justifications = default, 
            string title = null)
        {
            var jagged = dict.Select(kvp => new string[] { keyConverter(kvp.Key), valueConverter(kvp.Value) }).ToArray();
            return jagged.ToFormattedText(headers, justifications, title);
        }

        public static string ToFormattedText(this string[][] jagged, string[] headers = null, Enums.Justification[] justifications = default, string title = null)
        {
            int columnCount = GetJaggedColumnCount(ref jagged, headers, title);
            if (columnCount == 0) { return "Object is empty and has no headers or title"; }
            

            int[] columnWidths = GetJaggedColumnWidths(jagged, headers, columnCount);
            int tableWidth = columnWidths.Sum() + columnWidths.Length * 2 + 3;

            StringBuilder sb = new();
            
            AppendJaggedTitle(ref sb, title, tableWidth);

            justifications ??= InferDefaultJustifications(jagged, columnCount);

            AppendJaggedHeaders(ref sb, headers, title, columnWidths, tableWidth);
            
            sb.AppendLine(new string('=', tableWidth));

            AppendJaggedEmptyMessage(ref sb, jagged, tableWidth);

            AppendJaggedRows(ref sb, jagged, justifications, columnWidths, tableWidth);
            
            sb.AppendLine(new string('=', tableWidth));

            return sb.ToString();

        }

        private static Enums.Justification[] InferDefaultJustifications(string[][] jagged, int columnCount)
        {
            if (jagged.Length == 0) { return Enumerable.Repeat(Enums.Justification.Left, columnCount).ToArray(); }
            return Enumerable
                .Range(0, columnCount)
                .Select(i => 
                {
                    if (double.TryParse(jagged[0][i], out _))
                        return Enums.Justification.Right;
                    else
                        return Enums.Justification.Left;
                })
                .ToArray();
        }

        private static void AppendJaggedRows(
            ref StringBuilder sb,
            string[][] jagged, 
            Enums.Justification[] justifications, 
            int[] columnWidths, 
            int tableWidth)
        {   
            for (int i = 0; i < jagged.Length; i++)
            {
                // Add divider if the row is an aggregator
                var isAggregator = _aggregators.Contains(jagged[i][0].ToLower());
                if (isAggregator) { sb.AppendLine(new string('_', tableWidth)); }

                AppendJaggedRow(jagged[i], justifications, columnWidths, sb);
            }
        }

        private static void AppendJaggedRow(
            string[] row, 
            Enums.Justification[] justifications, 
            int[] columnWidths, 
            StringBuilder sb)
        {
            // Add left border
            sb.Append("| ");
            
            // Format and append each cell
            for (int j = 0; j < row.Length; j++)
            {
                var cellText = FormatJaggedCell(row[j], justifications[j], columnWidths[j]);
                sb.Append(cellText);
            }
            
            // Add right border
            sb.AppendLine("|");
        }

        private static string FormatJaggedCell(string cell, Enums.Justification justification, int columnWidth)
        {
            switch (justification)
            {
                case Enums.Justification.Right:
                    return cell.PadLeft(columnWidth)
                               .PadRight(columnWidth + 2);

                case Enums.Justification.Center:
                    if (cell.Length > columnWidth)
                    {
                        cell = cell.Substring(0, columnWidth);
                        return cell.PadRight(columnWidth + 2);
                    }
                    else
                    {
                        var padLeft = cell.Length + (int)Math.Round((columnWidth - cell.Length) / (double)2, 0);
                        return cell.PadLeft(padLeft).PadRight(columnWidth + 2);
                    }

                case Enums.Justification.Justified:
                    cell = cell.ToJustifiedText(columnWidth);
                    return cell.PadRight(columnWidth + 2);

                case Enums.Justification.Left:
                    return cell.PadRight(columnWidth + 2);

                default:
                    return cell.PadRight(columnWidth + 2);
            }
        }

        public static string ToJustifiedText(this string input, int width)
        {
            if (width <= 0 ) { throw new ArgumentOutOfRangeException(nameof(width), $"{nameof(width)} must be greater than 0");}
            
            var text = input?.Trim();
            if (text.IsNullOrEmpty()) { return new string(' ', width); }

            if (text.Length >= width)
                return text.Substring(0, width);
            
            var spacesPerLetter = (int)Math.Truncate(width / (double)text.Length);

            Regex rx = new(@"([^ ]+)");
            var words = rx.Matches(text).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var wordCount = words.Count();
            var letterCount = (int)(words.Sum(w => w.Length + (w.Length-1) * (double)(spacesPerLetter-1)));

            var spacesPerWord = Math.Max(1,(int)Math.Truncate((width - letterCount) / (double)(wordCount - 1)));
            var wordSpacer = wordCount <= 1 ? " ": new string(' ', spacesPerWord);

            if (spacesPerLetter > 1)
            {
                var letterSpacer = new string(' ', spacesPerLetter - 1);
                text = words.Select(w => w.ToCharArray().StringJoin(letterSpacer)).StringJoin(wordSpacer);
            }
            else
            {
                text = words.StringJoin(wordSpacer);
            }

            return text.PadRight(width);    
        }

        private static void FormatJaggedCell2(string[] row, Enums.Justification[] justifications, int[] columnWidths, StringBuilder sb, int j)
        {
            switch (justifications[j])
            {
                case Enums.Justification.Right:
                    sb.Append((row[j]
                        .PadLeft(columnWidths[j]))
                        .PadRight(columnWidths[j] + 2));
                    break;

                case Enums.Justification.Center:
                    if (row[j].Length > columnWidths[j])
                    {
                        row[j] = row[j].Substring(0, columnWidths[j]);
                    }
                    var padLeft = row[j].Length + (int)Math.Round((double)(columnWidths[j] - row[j].Length) / (double)2, 0);
                    sb.Append(row[j].PadLeft(padLeft).PadRight(columnWidths[j] + 2));
                    break;

                default:
                    sb.Append(row[j].PadRight(columnWidths[j] + 2));
                    break;
            }
        }

        private static void AppendJaggedEmptyMessage(ref StringBuilder sb, string[][] jagged, int tableWidth)
        {
            if (jagged.Length == 0)
            {
                string message = tableWidth switch
                {
                    int w when w >= 8 && w < 18 => "Empty",
                    int w when w >= 18 && w < 34 => "Object is empty",
                    int w when w >= 34 => "Object is empty and has no data",
                    _ => "",
                };

                sb.AppendLine($"| {message.PadToCenter(tableWidth - 3)}|");
            }
        }

        private static void AppendJaggedHeaders(ref StringBuilder sb, string[] headers, string title, int[] columnWidths, int tableWidth)
        {
            if (headers != null)
            {
                if (title.IsNullOrEmpty())
                    sb.AppendLine(new string('=', tableWidth));
                sb.Append("| ");
                for (int i = 0; i < headers.Length; i++)
                {
                    sb.Append(headers[i].PadRight(columnWidths[i] + 2));
                }
                sb.AppendLine("|");
            }
        }

        private static void AppendJaggedTitle(ref StringBuilder sb, string title, int tableWidth)
        {
            if (!title.IsNullOrEmpty())
            {
                sb.AppendLine(new string('=', tableWidth));
                if (title.Length + 3 <= tableWidth)
                {
                    sb.AppendLine($"| {title.PadToCenter(tableWidth - 3)}|");
                }
                else
                {
                    var rx = new Regex(@"([^ ]+)");
                    var matches = rx.Matches(title).Cast<Match>().Select(m => m.Groups[1].Value);
                    List<string> lines = [];
                    StringBuilder line = new();
                    foreach (var match in matches)
                    {
                        if (line.Length + match.Length + 4 <= tableWidth)
                        {
                            line.Append(" ");
                            line.Append(match);
                        }
                        else
                        {
                            lines.Add($"| {line.ToString().PadToCenter(tableWidth - 3)}|");
                            line.Clear();
                            line.Append(match);
                        }

                    }
                    if (line.Length > 0)
                        lines.Add($"| {line.ToString().PadToCenter(tableWidth - 3)}|");
                    
                    foreach (var l in lines) { sb.AppendLine(l); }
                    
                }
            }
        }

        private static int[] GetJaggedColumnWidths(string[][] jagged, string[] headers, int columnCount)
        {
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

            // Adjust Column widths for headers
            if (headers != null)
            {
                Enumerable.Range(0, columnCount).ForEach(i => columnLengths[i] = Math.Max(columnLengths[i], headers[i].Length));
            }

            return columnLengths;
        }

        private static int GetJaggedColumnCount(ref string[][] jagged, string[] headers, string title)
        {
            // Get the max number of columns
            int columnCount = 0;
            if (jagged.Length == 0)
            {
                if (headers?.Count() > 0)
                {
                    columnCount = headers.Count();
                }
                else if (!title.IsNullOrEmpty())
                {
                    columnCount = 1;
                    jagged = [["Object is empty and has no headers"]];
                }
            }
            else
            {
                // Get the max number of columns (rows with the most columns
                columnCount = jagged.GroupBy(row => row.Length).Select(x => x.Key).Max();
            }

            return columnCount;
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
            DgvForm dfViewer = new();

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
            Debug.WriteLine($"Size is {dfViewer.Size}");
            dfViewer.Dgv.Dock = DockStyle.Fill;
        }
        public static void DisplayDialog(this DataTable table)
        {
            DgvForm dfViewer = new();
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
            Debug.WriteLine($"Size is {dfViewer.Size}");
            dfViewer.Dgv.Dock = DockStyle.Fill;
            dfViewer.ShowDialog();
        }
    }
}
