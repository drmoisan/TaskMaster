using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace UtilitiesCS
{
    public enum BoxIcon
    {
        None = 0,
        Critical = 1,
        Warning = 2,
        Question = 4
    }

    public static class MyBoxController
    {
        public static DialogResult CustomDialog(string Message, string Title, BoxIcon icon, IList<DelegateButton> delegateButtons)
        {
            using (MyBoxViewer _viewer = new MyBoxViewer())
            {
                _viewer.Show();
                _viewer.RemoveStandardButtons();
                _viewer.Text = Title;
                _viewer.TextMessage.Text = Message;
                int columnWidth = 115;

                Size tmp = _viewer.MinimumSize;

                foreach (var delegateButton in delegateButtons)
                {
                    AppendButtonInColumn(_viewer.L2Bottom, delegateButton, columnWidth);
                    tmp.Width += columnWidth;
                }

                _viewer.MinimumSize = tmp;
                _viewer.Hide();
                DialogResult result = _viewer.ShowDialog();
                return result;
            }
        }

        private static void AppendButtonInColumn(TableLayoutPanel tlp, DelegateButton dlb, Single width)
        {
            tlp.ColumnCount++;
            tlp.ColumnStyles.Insert(tlp.ColumnCount-2, 
                                    new System.Windows.Forms.ColumnStyle(
                                        System.Windows.Forms.SizeType.Absolute,
                                        width));
            tlp.Controls.Add(dlb.Button, tlp.ColumnCount -2,0);
        }

        private static void SetDialogIcon(BoxIcon icon)
        {
            switch (icon)
            {
                case BoxIcon.None:
                    break;
                case BoxIcon.Critical: break;
                case BoxIcon.Warning: break;
                case BoxIcon.Question: break;
                default: break;
            }
        }

    }
}