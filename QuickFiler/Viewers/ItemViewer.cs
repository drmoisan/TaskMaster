using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class ItemViewer : UserControl
    {
        public ItemViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            InitControlGroups();
        }
        
        private IList<Control> _rightControls;

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

        public void RemoveControlsRightOf(Control furthestRight)
        {
            if (furthestRight.Parent is TableLayoutPanel) 
            {
                var tlp = (TableLayoutPanel)furthestRight.Parent;
                var columnNumber = tlp.GetColumn(furthestRight);
                if (++columnNumber < tlp.ColumnCount)
                {
                    var columnsToRemove = tlp.ColumnCount - columnNumber;
                    tlp.RemoveSpecificColumn(columnNumber, columnsToRemove);
                }
            }
            else 
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
        }

        private void InitControlGroups()
        {
            _tipsLabels = new List<Label>
            {
                LblAcOpen,
                LblAcPopOut,
                LblAcTask,
                LblAcDelete,
                LblAcAttachments,
                LblAcConversation,
                LblAcEmail,
                LblAcFolder,
                LblAcSearch,
            };

            _leftTipsLabels = new List<Label>
            {
                LblAcOpen,
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
    }
}

