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
using UtilitiesCS.Extensions;
using log4net.Repository.Hierarchy;

namespace QuickFiler
{
    public class EfcHomeController : IFilerHomeController
    {
        #region Constructors, Initializers, and Destructors

        public EfcHomeController(IApplicationGlobals globals, System.Action parentCleanup, MailItem mail = null)
        {
            CreateCancellationToken();
            Globals = globals;
            _parentCleanup = parentCleanup;
            DataModel = new EfcDataModel(_globals, mail, this.TokenSource, this.Token);

            if (DataModel.Mail is not null)
            {
                InitType = QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv;
                _stopWatch = new Stopwatch();
                FormViewer = EfcViewerQueue.Dequeue();
                _uiSyncContext = FormViewer.UiSyncContext;
                _keyboardHandler = new KeyboardHandler(FormViewer, this);
                _explorerController = new QfcExplorerController(QfEnums.InitTypeEnum.Sort, globals, this);
                _formController = new EfcFormController(Globals, _dataModel, FormViewer, this, Cleanup, InitType, Token).Initialize();
            }
        }

        private EfcHomeController(IApplicationGlobals globals, System.Action parentCleanup)
        {
            Globals = globals;
            _parentCleanup = parentCleanup;
        }

        public static async Task<EfcHomeController> CreateAsync(IApplicationGlobals globals, System.Action parentCleanup, MailItem mail = null)
        {
            globals.ThrowIfNull();
            parentCleanup.ThrowIfNull();

            var home = new EfcHomeController(globals, parentCleanup);
            home.CreateCancellationToken();
            var mailItems = LoadToList(globals, mail);

            if (mailItems.Count() > 0)
            {
                await home.InitAsync(globals, mailItems, QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv);
            }
            return home;
        }

        public static async Task<EfcHomeController> LoadFinderAsync(IApplicationGlobals globals, System.Action parentCleanup, MailItem mail = null)
        {
            globals.ThrowIfNull();
            parentCleanup.ThrowIfNull();

            var home = new EfcHomeController(globals, parentCleanup);
            home.CreateCancellationToken();
            var mailItems = LoadToList(globals, mail);

            await home.InitAsync(globals, mailItems, QfEnums.InitTypeEnum.Find);
            
            return home;
        }
        
        protected async Task InitAsync(IApplicationGlobals globals, List<MailItem> mailItems, QfEnums.InitTypeEnum initType)
        {
            // Start initializing data model
            Task<EfcDataModel> modelTask = null;
            if (mailItems.Count() > 0)
            {
                modelTask = Task.Run(() => EfcDataModel.CreateAsync(globals, mailItems, TokenSource, Token, false));
            }

            // Initialize the rest of the home controller
            InitType = initType;
            _stopWatch = new Stopwatch();
            FormViewer = EfcViewerQueue.Dequeue();
            _uiSyncContext = FormViewer.UiSyncContext;
            _keyboardHandler = new KeyboardHandler(FormViewer, this);
            _explorerController = new QfcExplorerController(initType, globals, this);
            _formController = new EfcFormController(globals, FormViewer, this, Cleanup, initType, Token).InitializeWithoutData();

            if (mailItems.Count() > 0)
            {
                // Wait for data model to finish initializing
                DataModel = await modelTask;

                // Initialize data fields in form controller
                _formController.InitializeDataFields(DataModel);
            }
            else
            {
                // Dummy data model
                DataModel = new EfcDataModel(globals, null, TokenSource, Token);
                _formController.InitializeDataFields(DataModel);
            }
        }

        private static List<MailItem> LoadToList(IApplicationGlobals globals, MailItem mail)
        {
            List<MailItem> mailItems = [];

            if (mail is not null) { mailItems.Add(mail); }
            else
            {
                var selection = globals.Ol.App.ActiveExplorer().Selection;
                if (selection.Count > 0)
                {
                    mailItems = selection
                        .Cast<object>()
                        .Where(x => x is MailItem)
                        .Cast<MailItem>()
                        .ToList();
                }
            }

            return mailItems;
        }

        private EfcViewer _formViewer;
        internal EfcViewer FormViewer { get => _formViewer; private set => _formViewer = value; }
        
        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals { get => _globals; private set => _globals = value; }

        private QfEnums.InitTypeEnum _initType;
        internal QfEnums.InitTypeEnum InitType { get => _initType; set => _initType = value; }

        private System.Action _parentCleanup;
        internal System.Action ParentCleanup { get => _parentCleanup; private set => _parentCleanup = value; }

        //[STAThread]
        public void Run() 
        { 
            if (_dataModel?.Mail is not null || InitType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                _formViewer.Show();
            }
            else { MessageBox.Show("Error", "No MailItem Selected", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public async Task RunAsync(ProgressTracker progress = null)
        {
            if (_dataModel?.Mail is not null || InitType.HasFlag(QfEnums.InitTypeEnum.Find))
            {
                await UiThread.Dispatcher.InvokeAsync(() => _formViewer.Show());
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
        public IQfcExplorerController ExplorerController { get => _explorerController; set => _explorerController = value; }

        private EfcFormController _formController;
        public IFilerFormController FormController { get => _formController; }

        private IQfcKeyboardHandler _keyboardHandler;
        public IQfcKeyboardHandler KeyboardHandler { get => _keyboardHandler; set => _keyboardHandler = value; }

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

        public FilerQueue FilerQueue => throw new NotImplementedException();

        #endregion

        #region Major Actions

        async public Task ExecuteMovesAsync()
        {
            var selectedFolder = _formController.SelectedFolder;
            var moveConversation = _formController.MoveConversation;
            var convInfo = DataModel.ConversationResolver.ConversationInfo.SameFolder;
            if (!moveConversation)
            {
                convInfo = convInfo.Where(itemInfo => itemInfo.EntryId == DataModel.Mail.EntryID).ToList();
            }

            await _dataModel.MoveToFolderAsync(selectedFolder,
                                          _formController.SaveAttachments,
                                          _formController.SaveEmail,
                                          _formController.SavePictures,
                                          moveConversation);
            
            QuickFileMetrics_WRITE(_globals.FS.Filenames.EmailSession, selectedFolder, convInfo);
        }

        internal async Task OpenOlFolderAsync(string selectedFolder)
        {
            await DataModel.OpenOlFolderAsync(selectedFolder);
        }

        internal async Task OpenFsFolderAsync(string selectedFolder)
        {
            await DataModel.OpenFsFolderAsync(selectedFolder);
        }

        public void QuickFileMetrics_WRITE(string filename, string selectedFolder, List<MailItemHelper> moved)
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

                if (Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var folderRoot))
                {
                    FileIO2.WriteTextFile(filename, dataLines, folderRoot);                    
                }
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
