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
    public partial class SyncContextForm : Form
    {
        public SyncContextForm()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
        }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }
    }
}
