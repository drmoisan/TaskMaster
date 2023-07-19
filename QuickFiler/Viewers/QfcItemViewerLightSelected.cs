using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace QuickFiler
{
    public partial class QfcItemViewerLightSelected : UserControl
    {
        public QfcItemViewerLightSelected()
        {
            InitializeComponent();
            InitTipsLabelsList();
        }

        private IList<Label> _tipsLabels;
        public IList<Label> TipsLabels { get => _tipsLabels; }
        private void InitTipsLabelsList()
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
                LblAcSearch
            };

        }

        private void CboFolders_DrawItem(object sender, DrawItemEventArgs e)
        {
            {
                var Brush = Brushes.Black;

                var Point = new Point(2, e.Index * e.Bounds.Height + 1);
                int index = e.Index >= 0 ? e.Index : 0;
                e.Graphics.FillRectangle(new SolidBrush(CboFolders.BackColor), new Rectangle(Point, e.Bounds.Size));
                e.Graphics.DrawString(CboFolders.Items[index].ToString(), e.Font, Brush, e.Bounds, StringFormat.GenericDefault);
            }
        }
    }
}
