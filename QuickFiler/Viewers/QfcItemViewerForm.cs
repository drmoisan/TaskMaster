using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler
{
    public partial class QfcItemViewerForm : Form
    {
        private bool _tipsActive = true;
        private List<Label> _tipsLabels = new List<Label>();
        private Dictionary<ColumnStyle, float> _tipsColumns = new Dictionary<ColumnStyle, float>();

        public QfcItemViewerForm()
        {

            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            InitTipsLabels();
            InitTipsColumns();
            ToggleAccelerator();

        }

        private void InitTipsLabels()
        {
            {
                ref var withBlock = ref this._tipsLabels;
                withBlock.Add(LblAcOpen);
                withBlock.Add(LblAcPopOut);
                withBlock.Add(LblAcTask);
                withBlock.Add(LblAcDelete);
                withBlock.Add(LblAcAttachments);
                withBlock.Add(LblAcConversation);
                withBlock.Add(LblAcEmail);
                withBlock.Add(LblAcFolder);
                withBlock.Add(LblAcSearch);
            }
        }

        private void InitTipsColumns()
        {
            {
                ref var withBlock = ref this._tipsColumns;
                withBlock.Add(L1h1L2v.ColumnStyles[0], 50f);
                withBlock.Add(L1h2L2v1h.ColumnStyles[1], 20f);
                withBlock.Add(L1h2L2v2h.ColumnStyles[1], 20f);
                withBlock.Add(L1h2L2v3h.ColumnStyles[1], 20f);
                withBlock.Add(L1h2L2v3h.ColumnStyles[5], 20f);
                withBlock.Add(L1h2L2v3h.ColumnStyles[7], 20f);
            }
        }

        private void QfcViewer_Paint(object sender, PaintEventArgs e)
        {

            if (this.FormBorderStyle == FormBorderStyle.FixedSingle)
            {
                int thickness = 2;
                int halfThickness = (int)Math.Round(thickness / 2d);

                using (var p = new Pen(Color.Black, thickness))
                {
                    e.Graphics.DrawRectangle(p, new Rectangle(halfThickness, halfThickness, ClientSize.Width - thickness, ClientSize.Height - thickness));
                }
            }
        }

        public void ToggleAccelerator()
        {
            if (_tipsActive)
            {
                // Make tips invisible
                foreach (Label tip in _tipsLabels)
                    tip.Visible = false;

                LblPos.Visible = false;

                // Make tips columns 0 pixels in _width
                foreach (ColumnStyle col in _tipsColumns.Keys)
                    col.Width = 0f;
                _tipsActive = false;
            }
            else
            {
                // Make tips visible
                foreach (Label tip in _tipsLabels)
                    tip.Visible = true;
                LblPos.Visible = true;

                // Make tips columns 20 pixels in _width
                foreach (ColumnStyle col in _tipsColumns.Keys)
                    col.Width = _tipsColumns[col];
                _tipsActive = true;
            }
        }       
    }
}