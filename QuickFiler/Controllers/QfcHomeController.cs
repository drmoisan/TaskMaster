using Microsoft.Office.Interop.Outlook;
using static QuickFiler.Enums;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using System.IO;

namespace QuickFiler.Controllers
{
    public class QfcHomeController : IQfcHomeController
    {
        public QfcHomeController(IApplicationGlobals AppGlobals, System.Action ParentCleanup)
        {
            QfcFormViewer.InitializeDPI();
            _globals = AppGlobals;
            InitAfObjects();
            _parentCleanup = ParentCleanup;
            _datamodel = new QfcDatamodel(_globals.Ol.App.ActiveExplorer(), _globals.Ol.App);
            _explorerController = new QfcExplorerController();
            _formViewer = new QfcFormViewer();
            _keyboardHandler = new QfcKeyboardHandler(_formViewer, this);
            _formController = new QfcFormController(_globals, _formViewer, InitTypeEnum.InitSort, Cleanup, this);
        }

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcFormViewer _formViewer;
        private IQfcDatamodel _datamodel;
        private IQfcExplorerController _explorerController;
        private IQfcFormController _formController;
        private IQfcCollectionController _collectionController;
        private IQfcKeyboardHandler _keyboardHandler;
        private cStopWatch _stopWatch;

        internal void InitAfObjects() 
        {
            if (_globals.AF.CtfMap is null) { throw new ArgumentNullException($"Error trying to initialize {nameof(_globals.AF.CtfMap)}"); }
            if (_globals.AF.RecentsList is null) { throw new ArgumentNullException($"Error trying to initialize {nameof(_globals.AF.RecentsList)}"); }
            if (_globals.AF.CommonWords is null) { throw new ArgumentNullException($"Error trying to initialize {nameof(_globals.AF.CommonWords)}"); }
            if (_globals.AF.SubjectMap is null) { throw new ArgumentNullException($"Error trying to initialize {nameof(_globals.AF.SubjectMap)}"); }
            if (_globals.AF.Encoder is null) { throw new ArgumentNullException($"Error trying to initialize {nameof(_globals.AF.Encoder)}"); }
            _globals.AF.SubjectMap.Where(x => x.Encoder is null).ForEach(x => x.Encoder = _globals.AF.Encoder);
        }

        public void Run()
        {
            //_formViewer.Show();
            //_formViewer.Refresh();
            Initialize();
            _formViewer.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            _formViewer.Show();
            _formViewer.Refresh();
        }

        public bool Loaded { get => _formViewer is not null; }

        internal void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            _parentCleanup.Invoke();
        }

        public IQfcExplorerController ExplCtrlr { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IQfcFormController FrmCtrlr { get => _formController; }
        public IQfcKeyboardHandler KbdHndlr { get => _keyboardHandler; set => _keyboardHandler = value; }
        public IQfcDatamodel DataModel { get => _datamodel; }
        public cStopWatch StopWatch { get => _stopWatch; }

        public void Initialize()
        {
            IList<MailItem> listEmail = _datamodel.InitEmailQueueAsync(_formController.ItemsPerIteration, _formViewer.Worker);
            _formController.LoadItems(listEmail);
            _stopWatch = new cStopWatch();
            _stopWatch.Start();
        }

        public void Iterate()
        {
            _stopWatch = new cStopWatch();
            _stopWatch.Start();

            IList<MailItem> listObjects = _datamodel.DequeueNextItemGroup(_formController.ItemsPerIteration);
            _formController.LoadItems(listObjects);
        }

        public void ExecuteMoves()
        {
            _formController.Groups.MoveEmails(DataModel.MovedItems);
            QuickFileMetrics_WRITE("9999TimeWritingEmail.csv");
            _formController.Groups.RemoveControls();
            Iterate();
        }

        internal void QuickFileMetrics_WRITE(string filename)
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
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curDateText = " & curDateText

            curTimeText = DateTime.Now.ToString("hh:mm");
            // If DebugLVL And vbCommand Then Debug.Print SubNm & " Variable curTimeText = " & curTimeText

            dataLineBeg = curDateText + "," + curTimeText + ",";

            LOC_TXT_FILE = Path.Combine(_globals.FS.FldrMyD, filename);

            Duration = _stopWatch.timeElapsed;
            OlEndTime = DateTime.Now;
            OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));

            var emailsLoaded = _formController.Groups.EmailsLoaded;

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


    }
}
