using Microsoft.Office.Interop.Outlook;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;
using System.Diagnostics;

namespace QuickFiler
{
    public class EfcHomeController : IFilerHomeController
    {
        #region Constructors, Initializers, and Destructors

        public EfcHomeController(IApplicationGlobals appGlobals, System.Action parentCleanup, MailItem mail = null)
        {
            CreateCancellationToken();
            _globals = appGlobals;
            _parentCleanup = parentCleanup;
            _dataModel = new EfcDataModel(_globals, mail, this.TokenSource, this.Token);

            if (_dataModel.Mail is not null)
            {
                _initType = QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv;
                _stopWatch = new Stopwatch();
                //_formViewer = new EfcViewer();
                _formViewer = EfcViewerQueue.Dequeue();
                _uiSyncContext = _formViewer.UiSyncContext;
                _keyboardHandler = new QfcKeyboardHandler(_formViewer, this);
                _explorerController = new QfcExplorerController(QfEnums.InitTypeEnum.Sort, appGlobals, this);
                _formController = new EfcFormController(_globals, _dataModel, _formViewer, this, Cleanup, _initType, Token);
            }
        }

        private EfcViewer _formViewer;
        private IApplicationGlobals _globals;
        private QfEnums.InitTypeEnum _initType;
        private System.Action _parentCleanup;

        [STAThread]
        public void Run() 
        { 
            if (_dataModel.Mail is not null)
            {
                _formViewer.Show();
            }
            else { MessageBox.Show("Error", "No MailItem Selected", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public async Task RunAsync(ProgressTracker progress = null)
        {
            if (_dataModel.Mail is not null)
            {
                await UIThreadExtensions.UiDispatcher.InvokeAsync(()=>_formViewer.Show());
            }
            else { MessageBox.Show("Error", "No MailItem Selected", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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

        #endregion

        #region Public Properties

        private IQfcExplorerController _explorerController;
        public IQfcExplorerController ExplorerCtlr { get => _explorerController; set => _explorerController = value; }

        private EfcFormController _formController;
        public IFilerFormController FormCtrlr { get => _formController; }

        private IQfcKeyboardHandler _keyboardHandler;
        public IQfcKeyboardHandler KeyboardHndlr { get => _keyboardHandler; set => _keyboardHandler = value; }

        private EfcDataModel _dataModel;
        internal EfcDataModel DataModel { get => _dataModel; set => _dataModel = value; }
                
        private System.Diagnostics.Stopwatch _stopWatch;
        public System.Diagnostics.Stopwatch StopWatch { get => _stopWatch; }

        public bool Loaded => throw new NotImplementedException();

        internal void CreateCancellationToken()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }
        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; }

        private CancellationToken _token;
        public CancellationToken Token { get => _token; }

        private SynchronizationContext _uiSyncContext;
        public SynchronizationContext UiSyncContext { get => _uiSyncContext; }

        #endregion

        #region Major Actions

        async public Task ExecuteMoves()
        {
            var selectedFolder = _formController.SelectedFolder;
            var moveConversation = _formController.MoveConversation;
            var convInfo = DataModel.ConversationResolver.ConversationInfo.SameFolder;
            if (!moveConversation)
            {
                convInfo.Where(itemInfo => itemInfo.EntryId == DataModel.Mail.EntryID).ToList();
            }

            await _dataModel.MoveToFolder(selectedFolder,
                                          _formController.SaveAttachments,
                                          _formController.SaveEmail,
                                          _formController.SavePictures,
                                          moveConversation);
            
            QuickFileMetrics_WRITE(_globals.FS.Filenames.EmailSession, selectedFolder, convInfo);
        }
                
        public void QuickFileMetrics_WRITE(string filename, string selectedFolder, List<MailItemInfo> moved)
        {
            if (moved is not null && moved.Count == 0) 
            { 
            
                var curDateText = DateTime.Now.ToString("MM/dd/yyyy");
                var curTimeText = DateTime.Now.ToString("hh:mm");
                var dataLineBeg = curDateText + "," + curTimeText + ",";

                var Duration = _stopWatch.Elapsed.Seconds;
                var OlEndTime = DateTime.Now;
                var OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));
           
                Duration /= moved.Count;
                var durationText = Duration.ToString("##0");
                var durationMinutesText = (Duration / 60d).ToString("##0.00");

                var dataLines = moved.Select(itemInfo => dataLineBeg + QfcCollectionController.xComma(itemInfo.Subject) +
                    $",SingleSorted,{durationText},{durationMinutesText},{itemInfo.ToRecipientsName}" +
                    $"{itemInfo.SenderName},Email,{selectedFolder},{itemInfo.SentDate.ToString("MM/dd/yyyy")}," +
                    $"{itemInfo.SentDate.ToString("HH:mm:ss")}").ToArray();

                FileIO2.WriteTextFile(filename, dataLines, _globals.FS.FldrMyD);
            }
        }

        public void QuickFileMetrics_WRITE(string filename)
        {
            throw new NotImplementedException();
        }
                
        #endregion

        #region Helper Methods

        //public IList<MailItem> PackageItems() => _conversationResolver.ConversationItems;


        #endregion
    }
}
