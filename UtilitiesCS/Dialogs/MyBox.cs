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

    public static class MyBox
    {
        public static DialogResult ShowDialog(string Message, string Title, BoxIcon icon, IList<DelegateButton> delegateButtons)
        {
            using (MyBoxViewer _viewer = new MyBoxViewer())
            {
                _viewer.Show();
                int columnWidth = _viewer.L2Bottom.GetColumnWidths()[1];
                _viewer.RemoveStandardButtons();
                _viewer.Text = Title;
                _viewer.TextMessage.Text = Message;
                

                Size tmp = _viewer.MinimumSize;

                foreach (var delegateButton in delegateButtons)
                {
                    AppendButtonInColumn(_viewer.L2Bottom, delegateButton, columnWidth);
                    tmp.Width += columnWidth;
                }

                _viewer.MinimumSize = tmp;
                _viewer.SetDialogIcon(icon);
                _viewer.Hide();
                DialogResult result = _viewer.ShowDialog();
                return result;
            }
        }

        public static DialogResult ShowDialog(MyBoxViewer viewer, string Message, string Title, BoxIcon icon, IList<ActionButton> actionButtons)
        {
            viewer.Show();
            ReplaceButtons(viewer, actionButtons);
            viewer.Text = Title;
            viewer.TextMessage.Text = Message;
            viewer.SetDialogIcon(icon);
            viewer.Hide();
            DialogResult result = viewer.ShowDialog();
            return result;
        }

        public static DialogResult ShowDialog(string message, string title, BoxIcon icon, Dictionary<string, Action> actions)
        {
            using (MyBoxViewer viewer = new MyBoxViewer())
            {
                var actionButtons = actions.ToActionButtons(viewer);
                return ShowDialog(viewer, message, title, icon, actionButtons);
            }
        }
        
        internal static void ReplaceButtons(MyBoxViewer viewer, IList<ActionButton> actionButtons)
        {
            int columnWidth = viewer.L2Bottom.GetColumnWidths()[1];
            viewer.RemoveStandardButtons();

            Size minSize = viewer.MinimumSize;

            foreach (var actionButton in actionButtons)
            {
                AppendButtonInColumn(viewer.L2Bottom, actionButton, columnWidth);
                minSize.Width += columnWidth;
            }
            viewer.MinimumSize = minSize;
        }

        internal static IList<ActionButton> ToActionButtons(this Dictionary<string, Action> actions, MyBoxViewer _viewer)
        {
            IList<ActionButton> actionButtons = new List<ActionButton>();
            int i = 0;
            foreach (var actionPair in actions)
            {
                ActionButton actionButton;

                if (actionPair.Key.Contains("Cancel"))
                {
                    actionButton = new ActionButton(
                        $"button{i}", actionPair.Key, DialogResult.Cancel, actionPair.Value);
                }
                else
                {
                    actionButton = new ActionButton(
                        $"button{i}", actionPair.Key, DialogResult.OK, actionPair.Value);
                }

                actionButtons.Add(actionButton);
                i++;
            }
            return actionButtons;
        }

        internal static void AppendButtonInColumn(TableLayoutPanel tlp, DelegateButton dlb, Single width)
        {
            tlp.ColumnCount++;
            tlp.ColumnStyles.Insert(tlp.ColumnCount-2, 
                                    new System.Windows.Forms.ColumnStyle(
                                        System.Windows.Forms.SizeType.Absolute,
                                        width));
            tlp.Controls.Add(dlb.Button, tlp.ColumnCount -2,0);
        }

        internal static void AppendButtonInColumn(TableLayoutPanel tlp, ActionButton actionButton, Single width)
        {
            tlp.ColumnCount++;
            tlp.ColumnStyles.Insert(tlp.ColumnCount - 2,
                                    new System.Windows.Forms.ColumnStyle(
                                        System.Windows.Forms.SizeType.Absolute,
                                        width));
            tlp.Controls.Add(actionButton.Button, tlp.ColumnCount - 2, 0);
        }

        private static void SetDialogIcon(this MyBoxViewer viewer, BoxIcon icon)
        {
            switch (icon)
            {
                case BoxIcon.None:
                    viewer.SvgIcon.Visible = false;
                    break;
                case BoxIcon.Critical:
                    viewer.SvgIcon.Image = SystemIcons.Error.ToBitmap();
                    break;
                case BoxIcon.Warning:
                    viewer.SvgIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
                case BoxIcon.Question:
                    viewer.SvgIcon.Image = SystemIcons.Question.ToBitmap();
                    break;
                default: break;
            }
        }

    }
}