using Microsoft.Office.Interop.Outlook;
using static QuickFiler.QfEnums;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using UtilitiesCS.Threading;
using System.Threading;

namespace QuickFiler.Controllers
{
    public class QfcHomeController : IFilerHomeController
    {
        #region Constructors, Initializers, and Destructors

        public QfcHomeController(IApplicationGlobals AppGlobals, System.Action ParentCleanup)
        {
            CreateCancellationToken();
            _globals = AppGlobals;
            //InitAfObjects();
            _parentCleanup = ParentCleanup;
            _datamodel = new QfcDatamodel(_globals, this.Token);
            _explorerController = new QfcExplorerController(QfEnums.InitTypeEnum.Sort, _globals, this);
            _formViewer = new QfcFormViewer();
            _formViewer.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            _keyboardHandler = new QfcKeyboardHandler(_formViewer, this);
            _qfcQueue = new QfcQueue(Token);
            _formController = new QfcFormController(_globals, _formViewer, _qfcQueue, InitTypeEnum.Sort, Cleanup, this, Token);
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcQueue _qfcQueue;

        #endregion Constructors, Initializers, and Destructors


        public void Run()
        {
            IList<MailItem> listEmail = _datamodel.InitEmailQueue(_formController.ItemsPerIteration, _formViewer.Worker);
            _formController.LoadItems(listEmail);
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();
        }

        // Twice as slow as the synchronous version
        public async Task RunAsync()
        {
            IList<MailItem> listEmail = _datamodel.InitEmailQueue(_formController.ItemsPerIteration, _formViewer.Worker);
            await _formController.LoadItemsAsync(listEmail);
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // The user canceled the operation.
                //MessageBox.Show("Operation was canceled");
                logger.Debug($"{nameof(QfcDatamodel)} background worker cancelled");
            }
            else if (e.Error != null)
            {
                // There was an error during the operation.
                string msg = String.Format("An error occurred: {0}", e.Error.Message);
                MessageBox.Show(msg);
            }
            else
            {
                _ = IterateQueueAsync();
            }
        }

        public async Task IterateQueueAsync()
        {
            if (this.Token.IsCancellationRequested) { throw new OperationCanceledException();}

            try
            {
                var listObjects = await _datamodel.DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000);
                await _qfcQueue.EnqueueAsync(listObjects, _globals, this, _formController.Groups).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
            }
            catch (System.Exception ex)
            {
                if (this.Token.IsCancellationRequested)
                {
                    logger.Debug($"{nameof(IterateQueueAsync)} cancelled");
                }
                else
                {
                    throw ex;
                }     
            }
            
        }
        
        public void Iterate()
        {
            _stopWatch = new cStopWatch();
            _stopWatch.Start();

            IList<MailItem> listObjects = _datamodel.DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000).GetAwaiter().GetResult();
            _formController.LoadItems(listObjects);
        }

        public void Iterate2()
        {
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
            (var tlp, var itemGroups) = _qfcQueue.Dequeue();
            _formController.LoadItems(tlp, itemGroups);
            _ = IterateQueueAsync();
        }

        public void SwapStopWatch()
        {
            _stopWatchMoved = _stopWatch;
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
        }
        
        public void QuickFileMetrics_WRITE(string filename)
        {

            string LOC_TXT_FILE;
            string curDateText, curTimeText, durationText, durationMinutesText;
            double Duration;
            string dataLineBeg;
            DateTime OlEndTime;
            DateTime OlStartTime;
            AppointmentItem OlAppointment;
            Folder OlEmailCalendar;

            // Create a line of comma seperated valued to store data
            curDateText = DateTime.Now.ToString("MM/dd/yyyy");
            
            curTimeText = DateTime.Now.ToString("hh:mm");
            
            dataLineBeg = curDateText + "," + curTimeText + ",";

            LOC_TXT_FILE = Path.Combine(_globals.FS.FldrMyD, filename);

            Duration = _stopWatchMoved.timeElapsed;
            OlEndTime = DateTime.Now;
            OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));

            var emailsLoaded = _formController.Groups.EmailsToMove;

            if (emailsLoaded > 0)
            {
                Duration /= emailsLoaded;
            }

            durationText = Duration.ToString("##0");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable durationText = " & durationText

            durationMinutesText = (Duration / 60d).ToString("##0.00");

            OlEmailCalendar = Calendar.GetCalendar("Email Time", _globals.Ol.App.Session);
            OlAppointment = (AppointmentItem)OlEmailCalendar.Items.Add();
            {
                OlAppointment.Subject = $"Quick Filed {emailsLoaded} emails";
                OlAppointment.Start = OlStartTime;
                OlAppointment.End = OlEndTime;
                OlAppointment.Categories = "@ Email";
                OlAppointment.ReminderSet = false;
                OlAppointment.Sensitivity = OlSensitivity.olPrivate;
                OlAppointment.Save();
            }


            string[] strOutput = _formController.Groups
                .GetMoveDiagnostics(durationText, durationMinutesText, Duration,
                dataLineBeg, OlEndTime, ref OlAppointment);

            FileIO2.WriteTextFile(filename, strOutput, _globals.FS.FldrMyD);
        }

        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            _parentCleanup.Invoke();
        }

        private bool _loaded = false;
        public bool Loaded { get => _loaded; }

        private IQfcExplorerController _explorerController;
        public IQfcExplorerController ExplorerCtlr { get => _explorerController; set => _explorerController = value; }
        
        private QfcFormController _formController;
        public IFilerFormController FormCtrlr { get => _formController; }
        
        private IQfcKeyboardHandler _keyboardHandler;
        public IQfcKeyboardHandler KeyboardHndlr { get => _keyboardHandler; set => _keyboardHandler = value; }
        
        private IQfcDatamodel _datamodel;
        public IQfcDatamodel DataModel { get => _datamodel; }

        private cStopWatch _stopWatchMoved;
        private cStopWatch _stopWatch;
        public cStopWatch StopWatch { get => _stopWatch; }

        private QfcFormViewer _formViewer;
        //public QfcFormViewer FormViewer { get => _formViewer; }

        internal void CreateCancellationToken()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }
        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; }

        private CancellationToken _token;
        public CancellationToken Token { get => _token; }

    }
}
