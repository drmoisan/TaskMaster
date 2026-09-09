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

namespace UtilitiesCS.EmailIntelligence.TaskPane
{
    public partial class ProgressPane : UserControl
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public ProgressPane()
        {
            InitializeComponent();
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

        private CancellationTokenSource? _tokenSource;

        public void SetCancellationTokenSource(CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource;

            // Enable only for a non-null source, so the button cannot be clickable in a state the
            // handler cannot serve. The pane keeps this inline check rather than delegating to a
            // property setter, because unlike the viewer it exposes no such property.
            this.ButtonCancel.Enabled = tokenSource is not null;
        }

        // Requests cancellation on the borrowed source. This pane borrows the source and never
        // disposes it; a different holder owns disposal. A null source is a host wiring defect,
        // because the button was enabled without a source ever being supplied, so it fails fast with
        // a diagnosable message. A disposed source is a lifecycle race rather than a defect: the
        // owner disposed it because the tracked operation is over, so there is nothing to cancel and
        // the correct response is to return quietly.
        internal void RequestCancel()
        {
            CancellationTokenSource source =
                _tokenSource
                ?? throw new InvalidOperationException(
                    "ProgressPane cancellation was requested with no CancellationTokenSource. "
                        + "Call SetCancellationTokenSource before enabling ButtonCancel."
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
            // frame logs it and still disposes in the finally, so a wiring defect can neither strand
            // the pane alive nor reach the user as a stack trace.
            try
            {
                RequestCancel();
            }
            catch (System.Exception ex)
            {
                logger.Error("ProgressPane cancel request failed.", ex);
            }
            finally
            {
                this.Dispose();
            }
        }
    }
}
