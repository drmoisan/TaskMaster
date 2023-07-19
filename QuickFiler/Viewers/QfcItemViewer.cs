using QuickFiler.Interfaces;
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
    public partial class QfcItemViewer : UserControl
    {
        public QfcItemViewer()
        {
            InitializeComponent();
            InitTipsLabelsList();
        }

        private IList<Label> _tipsLabels;
        private IQfcItemController _controller;

        public IList<Label> TipsLabels { get => _tipsLabels; }
        public IQfcItemController Controller { get => _controller; set => _controller = value; }

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
