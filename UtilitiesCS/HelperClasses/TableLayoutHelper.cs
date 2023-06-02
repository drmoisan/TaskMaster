using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS
{
    public static class TableLayoutHelper
    {
        public static void InsertSpecificRow(TableLayoutPanel panel, int rowIndex, RowStyle templateStyle, int insertCount = 1) 
        {
            if ((rowIndex < 0)||(rowIndex>panel.RowCount))
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
            if (insertCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(insertCount));
            }
            panel.RowCount += insertCount;
            for (int i = 1; i <= insertCount; i++)
            {
                panel.RowStyles.Insert(rowIndex, templateStyle.Clone());
            }

            for (int i = panel.RowCount - 1; i >= rowIndex; i--)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i + 1);
                    }
                }
            }

        }

        public static void RemoveSpecificRow(TableLayoutPanel panel, int rowIndex, int removeCount = 1)
        {
            if (rowIndex >= panel.RowCount)
            {
                return;
            }

            for (int i = rowIndex; i < rowIndex + removeCount; i++)
            {
                // delete all controls of row that we want to delete
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    panel.Controls.Remove(control);
                }
            }
            

            // move up row controls that comes after row we want to remove
            for (int i = rowIndex + 1; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - 1);
                    }
                }
            }

            // remove specific styles
            for (int i = rowIndex; i < rowIndex + removeCount; i++)
            {
                panel.RowStyles.RemoveAt(i);
            }

            panel.RowCount -= removeCount;
        }

        public static void RemoveSpecificColumn(TableLayoutPanel panel, int colIndex)
        {
            if (colIndex >= panel.ColumnCount)
            {
                return;
            }

            // delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, colIndex);
                panel.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
            for (int i = colIndex + 1; i < panel.ColumnCount; i++)
            {
                for (int j = 0; j < panel.RowCount; j++)
                {
                    var control = panel.GetControlFromPosition(i,j);
                    if (control != null)
                    {
                        panel.SetColumn(control, i - 1);
                    }
                }
            }

            var removeStyle = panel.ColumnCount - 1;

            if (panel.ColumnStyles.Count > removeStyle)
                panel.ColumnStyles.RemoveAt(removeStyle);

            panel.ColumnCount--;
        }

        public static RowStyle Clone(this RowStyle sourceStyle)
        {
            if (sourceStyle == null) { throw new ArgumentNullException(); }
            return new RowStyle(sourceStyle.SizeType, sourceStyle.Height);
        }
    }
}
