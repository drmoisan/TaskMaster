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
    public partial class QfcItemViewerExpandedLight : UserControl
    {
        public QfcItemViewerExpandedLight()
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

    }
}
