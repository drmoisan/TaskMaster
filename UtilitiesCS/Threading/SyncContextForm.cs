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

namespace QuickFiler.Viewers
{
    public partial class SyncContextForm : Form
    {
        public SyncContextForm()
        {
            InitializeComponent();
            //this.Shown += SyncContextForm_Shown;
        }

        public System.Drawing.SizeF FormAutoScaleFactor { get; private set; }
                
        public SynchronizationContext UiSyncContext { get; private set; }

        public Dispatcher UiDispatcher { get; private set; }

        public int UiThreadId { get; private set; }

        public void CaptureUiVariables() 
        {
            UiSyncContext = SynchronizationContext.Current;
            FormAutoScaleFactor = this.AutoScaleFactor;
            UiDispatcher = Dispatcher.CurrentDispatcher;
            UiThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        //private void SyncContextForm_Shown(object sender, EventArgs e)
        //{
        //    UiSyncContext = SynchronizationContext.Current;
        //    FormAutoScaleFactor = this.AutoScaleFactor;
        //    UiDispatcher = Dispatcher.CurrentDispatcher;
        //    UiThreadId = Thread.CurrentThread.ManagedThreadId;
        //}
    }
}
