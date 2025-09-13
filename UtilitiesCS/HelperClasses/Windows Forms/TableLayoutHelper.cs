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
        public static void InsertSpecificRow(this TableLayoutPanel panel, int rowIndex, RowStyle templateStyle, int insertCount = 1) 
        {
            // If not on the UI thread, invoke the method on the UI thread
            if (panel.InvokeRequired)
            {
                panel.Invoke(() => InsertSpecificRow(panel, rowIndex, templateStyle, insertCount));
                return;
            }

            if ((rowIndex < 0)||(rowIndex>panel.RowCount))
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }
            if (insertCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(insertCount));
            }
            int lastRowIdx = panel.RowCount - 1;
            panel.RowCount += insertCount;
            for (int i = 1; i <= insertCount; i++)
            {
                panel.RowStyles.Insert(rowIndex, templateStyle.Clone());
            }

            for (int i = lastRowIdx; i >= rowIndex; i--)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i + insertCount);
                    }
                }
            }

        }

        public static void RemoveSpecificRow(this TableLayoutPanel panel, int rowIndex, int removeCount = 1)
        {
            // If not on the UI thread, invoke the method on the UI thread
            if (panel.InvokeRequired) 
            {
                panel.Invoke(() => RemoveSpecificRow(panel, rowIndex, removeCount));
                return;
            }
            
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
            for (int i = rowIndex + removeCount; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - removeCount);
                    }
                }
            }

            // remove specific styles
            //for (int i = rowIndex; i < rowIndex + removeCount; i++)
            for (int i = rowIndex + removeCount -1; i >= rowIndex; i--)
            {
                panel.RowStyles.RemoveAt(i);
            }

            panel.RowCount -= removeCount;
        }

        public static void RemoveSpecificColumn(this TableLayoutPanel panel, int colIndex, int removeCount = 1)
        {
            // If not on the UI thread, invoke the method on the UI thread
            if (panel.InvokeRequired)
            {
                panel.Invoke(() => RemoveSpecificColumn(panel, colIndex, removeCount));
                return;
            }
            
            if (colIndex >= panel.ColumnCount)
            {
                return;
            }

            for (int i = colIndex; i < colIndex + removeCount; i++)
            {
                // delete all controls of column or set of columns that we want to delete
                for (int j = 0; j < panel.RowCount; j++)
                {
                    var control = panel.GetControlFromPosition(i,j);
                    if (control is not null)
                        panel.Controls.Remove(control);
                }

            }

            // move over column controls that come after row we want to remove
            for (int i = colIndex + removeCount; i < panel.ColumnCount; i++)
            {
                for (int j = 0; j < panel.RowCount; j++)
                {
                    var control = panel.GetControlFromPosition(i, j);
                    if (control != null)
                    {
                        panel.SetColumn(control, i - removeCount);
                    }
                }
            }

            for (int i = colIndex + removeCount - 1; i >= colIndex; i--)
            {
                panel.ColumnStyles.RemoveAt(i);
            }
            
            

            panel.ColumnCount -= removeCount;
        }

        //public static RowStyle Clone(this RowStyle sourceStyle)
        //{
        //    if (sourceStyle == null) { throw new ArgumentNullException(); }
        //    return new RowStyle(sourceStyle.SizeType, sourceStyle.Height);
        //}
    }
}
