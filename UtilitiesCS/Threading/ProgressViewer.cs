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
using System.Windows.Threading;

namespace UtilitiesCS
{
    public partial class ProgressViewer : Form//, IProgressViewer
    {
        public ProgressViewer()
        {
            InitializeComponent();
            _uiThreadNumber = Thread.CurrentThread.ManagedThreadId;
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

        private int _uiThreadNumber;
        public int UiThreadNumber { get => _uiThreadNumber; set => _uiThreadNumber = value; }

        private CancellationTokenSource _cancelSource;
        public CancellationTokenSource CancelSource { get => _cancelSource; set => _cancelSource = value; }
        public void SetCancellationTokenSource(CancellationTokenSource tokenSource)
        {
            _cancelSource = tokenSource;
            this.ButtonCancel.Enabled = true;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            _cancelSource.Cancel();
            this.Close();
        }

        #region IProgressViewer
        
        //void IProgressViewer.SetCancellationTokenSource(CancellationTokenSource tokenSource)
        //{
        //    this.SetCancellationTokenSource(tokenSource);
        //}
        //ProgressBar IProgressViewer.Bar => this.Bar;
        //Label IProgressViewer.JobName => this.JobName;
        //Button IProgressViewer.ButtonCancel => this.ButtonCancel;
        //Dispatcher IProgressViewer.UiDispatcher { get => this.UiDispatcher; set => this.UiDispatcher = value; }
        
        #endregion IProgressViewer
    }
}
