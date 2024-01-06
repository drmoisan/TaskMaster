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

namespace UtilitiesCS.EmailIntelligence.TaskPane
{
    public partial class ProgressPane : UserControl
    {
        public ProgressPane()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.ButtonCancel.Enabled = false;
        }

        private System.Windows.Threading.Dispatcher _dispatcher;
        public System.Windows.Threading.Dispatcher UiDispatcher { get => _dispatcher; set => _dispatcher = value; }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler { get => _uiScheduler; }


        private CancellationTokenSource _tokenSource;
        public void SetCancellationTokenSource(CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource;
            this.ButtonCancel.Enabled = true;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _tokenSource.Cancel();
            this.Dispose();
        }
    }
}
