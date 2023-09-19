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

namespace QuickFiler.Viewers
{
    public partial class ItemViewerLight : UserControl
    {
        public ItemViewerLight()
        {
            InitializeComponent();
        }

        private List<Resizer> _resizers = new List<Resizer>();

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

        private void InitTips()
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

            _expandedTipsLabels = new List<Label>
            {
                LblAcBody,
            };
        }

        private List<int> GetNavColWidths()
        {
            return new List<int> 
            { 
                LblItemNumber.Width,
                LblAcConversation.Width,
                LblAcAttachments.Width,
                LblAcEmail.Width,
            };
        }

        private void InitResizers()
        {
            var navCols = GetNavColWidths();
            //Column 1 - Nav Column
            _resizers.Add(new Resizer(control: this.LblItemNumber, shiftRatio: new PointF(0.0f, 0.0f),stretchRatio: new PointF(0.0f, 0.0f),navShift: new Size(0, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblAcOpen, shiftRatio: new PointF(0.0f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(0, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblAcBody, shiftRatio: new PointF(0.0f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(0, 0), navStretch: new Size(0, 0)));

            //Column Group 2
            var navWidth1 = navCols.Take(1).Sum();
            _resizers.Add(new Resizer(control: this.LblSender, shiftRatio: new PointF(0.0f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblSubject, shiftRatio: new PointF(0.0f, 0.0f), stretchRatio: new PointF(0.45f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0,0)));
            _resizers.Add(new Resizer(control: this.TxtboxBody, shiftRatio: new PointF(0.0f, 0.0f), stretchRatio: new PointF(0.45f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));

            //Column Group 3
            _resizers.Add(new Resizer(control: this.lblCaptionTriage, shiftRatio: new PointF(0.2f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblTriage, shiftRatio: new PointF(0.2f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));

            //Column Group 4
            _resizers.Add(new Resizer(control: this.LblCaptionPredicted, shiftRatio: new PointF(0.25f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblActionable, shiftRatio: new PointF(0.25f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));

            //Column Group 5
            _resizers.Add(new Resizer(control: this.LblSentOn, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblConvCt, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblSearch, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblFolder, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(0, 0)));

            //Column Group 6
            var navWidth6 = navCols.Take(2).Sum();
            _resizers.Add(new Resizer(control: this.CbxConversation, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth6, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.TxtboxSearch, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth6, 0), navStretch: new Size(-navCols[2], 0)));
            var navStretch2 = navCols[2] + navCols[3];
            _resizers.Add(new Resizer(control: this.CboFolders, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(1.0f, 0.0f), navShift: new Size(-navWidth6, 0), navStretch: new Size(-navStretch2, 0)));


            //Column Group 7
            var navWidth7 = navCols.Take(3).Sum();
            _resizers.Add(new Resizer(control: this.CbxAttachments, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth7, 0), navStretch: new Size(0, 0)));

            //Column Group 8
            var navWidth8 = navCols.Take(4).Sum();
            _resizers.Add(new Resizer(control: this.BtnFlagTask, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth8, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.BtnPopOut, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth8, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.BtnDelItem, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth8, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblAcTask, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth8, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblAcPopOut, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth8, 0), navStretch: new Size(0, 0)));
            _resizers.Add(new Resizer(control: this.LblAcDelete, shiftRatio: new PointF(0.45f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth8, 0), navStretch: new Size(0, 0)));

            //Webview
            _resizers.Add(new Resizer(control: this.L0v2h2_WebView2, shiftRatio: new PointF(0.0f, 0.0f), stretchRatio: new PointF(0.0f, 0.0f), navShift: new Size(-navWidth1, 0), navStretch: new Size(navCols.Skip(1).Sum(), 0)));
        }
    }
}
