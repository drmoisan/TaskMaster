using Microsoft.Web.WebView2.Core;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler
{
    public partial class QfcItemViewer : UserControl
    {
        public QfcItemViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            InitTipsLabelsList();
            InitLeftTipsLabelsList();
        }


        private IList<Label> _tipsLabels;
        public IList<Label> TipsLabels { get => _tipsLabels; }

        private IList<Label> _leftTipsLabels;
        public IList<Label> LeftTipsLabels { get => _leftTipsLabels; }
        
        private IItemControler _controller;
        public IItemControler Controller { get => _controller; set => _controller = value; }
        
        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

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
                LblAcSearch,
                LblAcBody
            };

        }

        private void InitLeftTipsLabelsList()
        {
            _leftTipsLabels = new List<Label>
            {
                LblAcOpen,
                LblAcBody
            };

        }


    }
}
