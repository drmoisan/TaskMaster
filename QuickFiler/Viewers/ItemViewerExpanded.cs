using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Web.UI.WebControls;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class ItemViewerExpanded : UserControl
    {
        public ItemViewerExpanded()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            InitControlGroups();
            MenuItem_CheckedChanged(this.ConversationMenuItem);
            MenuItem_CheckedChanged(this.SaveAttachmentsMenuItem);
            MenuItem_CheckedChanged(this.SaveEmailMenuItem);
            MenuItem_CheckedChanged(this.SavePicturesMenuItem);
        }

        //private IList<Control> _rightControls;

        private IList<Label> _tipsLabels;
        public IList<Label> TipsLabels { get => _tipsLabels; }

        private IList<Label> _leftTipsLabels;
        public IList<Label> LeftTipsLabels { get => _leftTipsLabels; }

        private IList<Label> _expandedTipsLabels;
        public IList<Label> ExpandedTipsLabels { get => _expandedTipsLabels; }

        private IItemControler _controller;
        public IItemControler Controller { get => _controller; set => _controller = value; }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler { get => _uiScheduler; }

        public void RemoveControlsColsRightOf(Control furthestRight)
        {
            if (furthestRight.Parent is TableLayoutPanel)
            {
                var tlp = (TableLayoutPanel)furthestRight.Parent;
                var columnNumber = tlp.GetColumn(furthestRight);
                tlp.SetColumnSpan(L0v2h2_WebView2, 10);

                if (++columnNumber < tlp.ColumnCount)
                {
                    var columnsToRemove = tlp.ColumnCount - columnNumber;
                    tlp.RemoveSpecificColumn(columnNumber, columnsToRemove);

                }
            }
            else
            {
                RemoveControlsRightOf(furthestRight);
            }
        }

        private void RemoveControlsRightOf(Control furthestRight)
        {
            var controlsToRemove = ControlsRightOf(furthestRight);
            for (int i = controlsToRemove.Count - 1; i >= 0; i--)
            {
                var control = controlsToRemove[i];
                control.Parent.Controls.Remove(control);
                controlsToRemove.RemoveAt(i);
                control.Dispose();
            }
        }

        private void InitControlGroups()
        {
            _tipsLabels = new List<Label>
            {
                // TODO: Add new labels for reply, reply all, forward
                LblAcOpen,
                LblAcPopOut,
                LblAcTask,
                LblAcDelete,
                LblAcMoveOptions,
                //LblAcAttachments,
                //LblAcConversation,
                //LblAcEmail,
                LblAcFolder,
                LblAcSearch,
                LblAcReply,
                LblAcReplyAll,
                LblAcFwd,
                LblAcBody,
            };

            _leftTipsLabels = new List<Label>
            {
                LblAcOpen,
                LblAcBody,
            };

            //_rightControls = ControlsRightOf(this.LblConvCt);

            _expandedTipsLabels = new List<Label>
            {
                LblAcBody,
            };
        }

        private List<Control> ControlsRightOf(Control furthestRight)
        {
            var controlLocation = new List<(Control Control, Point Point)>();
            this.ForAllControls(new Point(0, 0), (Control control, Point point) =>
            {
                var trueLocation = control.Location + new Size(point);
                controlLocation.Add((control, trueLocation));
                return trueLocation;
            });

            Point limit;
            if (controlLocation.Any(tup => tup.Control == furthestRight))
            {
                var tup = controlLocation.First(tup => tup.Control == furthestRight);
                limit = tup.Point + furthestRight.Size;
            }
            else
            {
                limit = furthestRight.Location + furthestRight.Size;
            }
            return controlLocation.Where(tup => tup.Point.X > limit.X).Select(tup => tup.Control).ToList();
        }

        private void L0v2h2_WebView2_ParentChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Parent Changed");
        }

        private void MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            MenuItem_CheckedChanged(menuItem);
        }

        private void MenuItem_CheckedChanged(ToolStripMenuItem menuItem)
        {
            if (menuItem.Checked)
            {
                menuItem.Image = global::QuickFiler.Properties.Resources.CheckBoxChecked;
            }
            else
            {
                menuItem.Image = null;
            }
        }
    }
}

