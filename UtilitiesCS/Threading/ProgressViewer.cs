#nullable enable
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
    public partial class ProgressViewer : Form //, IProgressViewer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public ProgressViewer()
        {
            InitializeComponent();
            _uiThreadNumber = Thread.CurrentThread.ManagedThreadId;
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.ButtonCancel.Enabled = false;
        }

        private System.Windows.Threading.Dispatcher? _dispatcher;
        public System.Windows.Threading.Dispatcher? UiDispatcher
        {
            get => _dispatcher;
            set => _dispatcher = value;
        }

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext
        {
            get => _context;
        }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler
        {
            get => _uiScheduler;
        }

        private int _uiThreadNumber;
        public int UiThreadNumber
        {
            get => _uiThreadNumber;
            set => _uiThreadNumber = value;
        }

        private CancellationTokenSource? _cancelSource;
        public CancellationTokenSource? CancelSource
        {
            get => _cancelSource;
            set
            {
                _cancelSource = value;
                ButtonCancel.Enabled = value != null;
            }
        }

        public void SetCancellationTokenSource(CancellationTokenSource tokenSource)
        {
            // Delegates to the CancelSource setter rather than assigning the field and enabling the
            // button itself. That setter already enables only for a non-null source, so delegating
            // removes the duplicated enabling logic instead of duplicating and patching it.
            CancelSource = tokenSource;
        }

        // Requests cancellation on the borrowed source. This viewer borrows the source and never
        // disposes it; a different holder owns disposal. A null source is a host wiring defect,
        // because the button was enabled without a source ever being supplied, so it fails fast with
        // a diagnosable message. A disposed source is a lifecycle race rather than a defect: the
        // owner disposed it because the tracked operation is over, so there is nothing to cancel and
        // the correct response is to return quietly.
        internal void RequestCancel()
        {
            CancellationTokenSource source =
                _cancelSource
                ?? throw new InvalidOperationException(
                    "ProgressViewer cancellation was requested with no CancellationTokenSource. "
                        + "Assign CancelSource or call SetCancellationTokenSource before enabling ButtonCancel."
                );

            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
                logger.Debug(
                    "Cancel requested after the token source was disposed; nothing to cancel."
                );
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Boundary: this is a WinForms handler in a VSTO add-in, so an escaping exception
            // surfaces to the Outlook user. RequestCancel carries the diagnosable message; this
            // frame logs it and still closes in the finally, so a wiring defect can neither strand
            // the dialog open nor reach the user as a stack trace.
            try
            {
                RequestCancel();
            }
            catch (System.Exception ex)
            {
                logger.Error("ProgressViewer cancel request failed.", ex);
            }
            finally
            {
                this.Close();
            }
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
