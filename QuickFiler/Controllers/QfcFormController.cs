using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using ToDoModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Concurrent;
using log4net.Repository.Hierarchy;

namespace QuickFiler.Controllers
{    
    internal class QfcFormController : IFilerFormController
    {
        
        #region Contructors

        public QfcFormController(IApplicationGlobals appGlobals,
                                 QfcFormViewer formViewer,
                                 QfcQueue qfcQueue,
                                 QfEnums.InitTypeEnum initType,
                                 System.Action parentCleanup,
                                 QfcHomeController parent,
                                 CancellationTokenSource tokenSource,
                                 CancellationToken token)
        {
            _token = token;
            _tokenSource = tokenSource;
            _globals = appGlobals;
            _initType = initType;
            _formViewer = formViewer;
            _globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer;
            _formViewer.SetController(this);
            _parentCleanup = parentCleanup;
            _parent = parent;
            //WriteMetrics = parent.QuickFileMetrics_WRITE;
            WriteMetrics = parent.WriteMetricsAsync;
            Iterate = parent.Iterate;
            _movedItems = _globals.AF.MovedMails;
            _qfcQueue = qfcQueue;
            
            CaptureItemSettings();
            RemoveTemplatesAndSetupTlp();
            SetupLightDark();
            RegisterFormEventHandlers();
            //_undoConsumerTask = Task.Run(UndoConsumer);
        }

        #endregion

        #region Private Variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private RowStyle _rowStyleTemplate;
        private RowStyle _rowStyleExpanded;
        
        private Padding _itemMarginTemplate;
        private QfEnums.InitTypeEnum _initType;
        //private bool _blRunningModalCode = false;
        //private bool _blSuppressEvents = false;
        private QfcHomeController _parent;
        private delegate Task WriteMetricsDelegate(string filename);
        private WriteMetricsDelegate WriteMetrics;
        private delegate void IterateDelegate();
        private IterateDelegate Iterate;
        private ScoStack<IMovedMailInfo> _movedItems;
        private QfcQueue _qfcQueue;
        private TlpCellStates _states;
        private Dictionary<string, Theme> _themes;
        private BlockingCollection<IMovedMailInfo> _undoQueue = [];
        private Task _undoConsumerTask;

        #endregion

        #region Setup and Disposal

        public void CaptureItemSettings()
        {
            _formViewer.Show();
            _rowStyleTemplate = _formViewer.L1v0L2L3v_TableLayout.RowStyles[0];
            _rowStyleExpanded = _formViewer.L1v0L2L3v_TableLayout.RowStyles[1];
            _itemMarginTemplate = _formViewer.QfcItemViewerTemplate.Margin;

            _states = new(new List<KeyValuePair<string, List<TlpCellSnapShot>>>()
            {
                new KeyValuePair<string, List<TlpCellSnapShot>>("Expanded", new List<TlpCellSnapShot>()
                {
                    new TlpCellSnapShot(_formViewer.QfcItemViewerExpandedTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerExpandedTemplate.L1h0L2hv3h_TlpBodyToggle),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerExpandedTemplate.L1h0L2hv3h_TlpBodyToggle,
                        _formViewer.QfcItemViewerExpandedTemplate.TxtboxBody),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerExpandedTemplate.L1h0L2hv3h_TlpBodyToggle,
                        _formViewer.QfcItemViewerExpandedTemplate.TopicThread),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerExpandedTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerExpandedTemplate.L0v2h2_WebView2),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerExpandedTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerExpandedTemplate.LblAcOpen),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerExpandedTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerExpandedTemplate.LblAcBody),
                }),
                new KeyValuePair<string, List<TlpCellSnapShot>>("Compressed", new List<TlpCellSnapShot>()
                {
                    new TlpCellSnapShot(_formViewer.QfcItemViewerTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerTemplate.L1h0L2hv3h_TlpBodyToggle),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerTemplate.L1h0L2hv3h_TlpBodyToggle,
                        _formViewer.QfcItemViewerTemplate.TxtboxBody),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerTemplate.L1h0L2hv3h_TlpBodyToggle,
                        _formViewer.QfcItemViewerTemplate.TopicThread),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerTemplate.L0v2h2_WebView2),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerTemplate.LblAcOpen),
                    new TlpCellSnapShot(_formViewer.QfcItemViewerTemplate.L0vh_Tlp,
                        _formViewer.QfcItemViewerTemplate.LblAcBody),
                }),
            }); 
            _formViewer.Hide();
        }

        public void RemoveTemplatesAndSetupTlp()
        {
            ref TableLayoutPanel tlp = ref _formViewer.L1v0L2L3v_TableLayout;
            TableLayoutHelper.RemoveSpecificRow(tlp, 0, 2);

            var count = ItemsPerIteration;
            //_itemsPerIteration = 1;
            //count = 1;
            tlp.InsertSpecificRow(0, _rowStyleTemplate, count);
            tlp.MinimumSize = new System.Drawing.Size(
                tlp.MinimumSize.Width,
                tlp.MinimumSize.Height +
                (int)Math.Round(_rowStyleTemplate.Height * count, 0));
            _qfcQueue.TlpTemplate = tlp;
            _qfcQueue.TlpStates = _states;
        }

        public void SetupLightDark()
        {
            _themes = QfcThemeHelper.SetupFormThemes(_formViewer.Panels, _formViewer.Buttons);
            _activeTheme = LoadTheme();
            _globals.Ol.PropertyChanged += DarkMode_CheckedChanged;
        }

        public int SpaceForEmail
        {
            get
            {
                var outerSize = _formViewer.Size;
                var innerSize = _formViewer.ClientSize;
                var frameSize = outerSize - innerSize;
                var _screen = Screen.FromControl(_formViewer);
                int nonEmailSpace = (int)Math.Round(_formViewer.L1v_TableLayout.RowStyles[1].Height, 0) + frameSize.Height;
                int workingSpace = _screen.WorkingArea.Height;
                return workingSpace - nonEmailSpace;
            }
        }

        private int _itemsPerIteration = -1;
        public int ItemsPerIteration 
        {
            get => Initializer.GetOrLoad(ref _itemsPerIteration, (x) => x != -1, LoadItemsPerIteration);
            set => Initializer.SetAndSave(ref _itemsPerIteration, value, (x) => _formViewer.Invoke(new System.Action(() => _formViewer.L1v1L2h5_SpnEmailPerLoad.Value = x)));
        }
        public int LoadItemsPerIteration()
        {
            var result = (int)Math.Round(SpaceForEmail / _rowStyleTemplate.Height, 0);
            _formViewer.Invoke(new System.Action(() => _formViewer.L1v1L2h5_SpnEmailPerLoad.Value = result));
            return result;
        }

        public void RegisterFormEventHandlers()
        {
            _formViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_parent.KeyboardHandler.KeyboardHandler_PreviewKeyDownAsync);
                //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHandler.KeyboardHandler_KeyDownAsync);
            },
            new List<Control> { _formViewer.QfcItemViewerTemplate });

            _formViewer.L1v1L2h2_ButtonOK.Click += this.ButtonOK_Click;
            _formViewer.L1v1L2h3_ButtonCancel.Click += this.ButtonCancel_Click;
            _formViewer.L1v1L2h4_ButtonUndo.Click += this.ButtonUndo_Click;
            _formViewer.L1v1L2h5_SpnEmailPerLoad.ValueChanged += this.SpnEmailPerLoad_ValueChanged;
            _formViewer.L1v1L2h5_BtnSkip.Click += this.ButtonSkip_Click;
        }
        
        /// <summary>
        /// Release all resources and call the parent cleanup
        /// </summary>
        public void Cleanup()
        {
            _undoConsumerTask.Dispose();
            _undoQueue.Dispose();
            _globals = null;
            _formViewer = null;
            _groups = null;
            _rowStyleTemplate = null;
            _parent = null;
            _movedItems = null;
            WriteMetrics = null;
            Iterate = null;
            _parentCleanup.Invoke();
            _parentCleanup = null;
        }

        #endregion

        #region Public Properties

        private string _activeTheme;
        public string ActiveTheme
        {
            get => Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
            set => Initializer.SetAndSave<string>(ref _activeTheme, value, (x) => _themes[x].SetTheme(async: true));
        }
        internal string LoadTheme()
        {
            var activeTheme = DarkMode ? "DarkNormal" : "LightNormal";
            _themes[activeTheme].SetTheme();
            return activeTheme;
        }

        private bool _darkMode;
        public bool DarkMode
        {
            get => Initializer.GetOrLoad(ref _darkMode, () => _globals.Ol.DarkMode, false, _globals, _globals.Ol);
            set => Initializer.SetAndSave(ref _darkMode, value, (x) => _globals.Ol.DarkMode = x);
        }

        private QfcCollectionController _groups;
        public IQfcCollectionController Groups { get => _groups; }
        
        public IntPtr FormHandle { get => _formViewer.Handle; }
        
        private QfcFormViewer _formViewer;
        public QfcFormViewer FormViewer { get => _formViewer; }

        public void ToggleOffNavigation(bool async) => _groups.ToggleOffNavigation(async);
        public async Task ToggleOffNavigationAsync() => await _groups.ToggleOffNavigationAsync();
        public void ToggleOnNavigation(bool async) => _groups.ToggleOnNavigation(async);
        public async Task ToggleOnNavigationAsync() => await _groups.ToggleOnNavigationAsync();

        private CancellationToken _token;
        public CancellationToken Token { get => _token; }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource { get => _tokenSource; }

        #endregion

        #region Event Handlers

        private void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
            _darkMode = _globals.Ol.DarkMode;
            if (DarkMode) { ActiveTheme = "DarkNormal"; }
            else { ActiveTheme = "LightNormal"; }
        }

        //private void SetDarkMode()
        //{
        //    _formViewer.L1v1L2h2_ButtonOK.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h2_ButtonOK.ForeColor = System.Drawing.Color.WhiteSmoke;
        //    _formViewer.L1v1L2h2_ButtonOK.UseVisualStyleBackColor = false;
        //    _formViewer.L1v1L2h3_ButtonCancel.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h3_ButtonCancel.ForeColor = System.Drawing.Color.WhiteSmoke;
        //    _formViewer.L1v1L2h3_ButtonCancel.UseVisualStyleBackColor = false;
        //    _formViewer.L1v1L2h4_ButtonUndo.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h4_ButtonUndo.ForeColor = System.Drawing.Color.WhiteSmoke;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.BackColor = System.Drawing.Color.DimGray;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.ForeColor = System.Drawing.Color.Gainsboro;
        //    _formViewer.BackColor = Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
        //}

        //private void SetLightMode()
        //{
        //    _formViewer.L1v1L2h2_ButtonOK.BackColor = System.Drawing.SystemColors.Control;
        //    _formViewer.L1v1L2h2_ButtonOK.ForeColor = System.Drawing.SystemColors.ControlText;
        //    _formViewer.L1v1L2h2_ButtonOK.UseVisualStyleBackColor = true;
        //    _formViewer.L1v1L2h3_ButtonCancel.BackColor = System.Drawing.SystemColors.Control;
        //    _formViewer.L1v1L2h3_ButtonCancel.ForeColor = System.Drawing.SystemColors.ControlText;
        //    _formViewer.L1v1L2h3_ButtonCancel.UseVisualStyleBackColor = true;
        //    _formViewer.L1v1L2h4_ButtonUndo.BackColor = System.Drawing.SystemColors.Control;
        //    _formViewer.L1v1L2h4_ButtonUndo.ForeColor = System.Drawing.SystemColors.ControlText;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.BackColor = System.Drawing.SystemColors.Window;
        //    _formViewer.L1v1L2h5_SpnEmailPerLoad.ForeColor = System.Drawing.SystemColors.WindowText;
        //    _formViewer.BackColor = System.Drawing.SystemColors.ControlLightLight;
        //}

        async public void ButtonCancel_Click(object sender, EventArgs e) 
        {
            SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
            await ActionCancelAsync(); 
        }

        async public Task ActionCancelAsync()
        {
            _parent.TokenSource.Cancel();
            await _formViewer.UiSyncContext;
            _formViewer.Hide();
            _groups.Cleanup();
            _globals = null;
            _groups = null;
            _formViewer.Dispose();
            _parentCleanup.Invoke();
        }

        async public void ButtonOK_Click(object sender, EventArgs e) 
        {
            SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
            await ActionOkAsync(); 
        }

        async public Task ActionOkAsync()
        {
            //TraceUtility.LogMethodCall();

            if (!_initType.HasFlag(QfEnums.InitTypeEnum.Sort))
            {
                throw new NotImplementedException(
                    $"Method {nameof(QfcFormController)}.{nameof(ActionOkAsync)} has not been " +
                    $"implemented for {nameof(_initType)} {_initType}");
            }
            
            //else if (_blRunningModalCode)
            //{
            //    MessageBox.Show("Can't Execute While Running Modal Code", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            else if (_groups.ReadyForMove)
            {
                //_blRunningModalCode = true;
                
                if (_parent.KeyboardHandler.KbdActive) { _parent.KeyboardHandler.ToggleKeyboardDialog(); }
                
                await MoveAndIterate();
                
                //_blRunningModalCode = false;
            }

        }

        private async Task LoadUiFromQueue() 
        {
            //TraceUtility.LogMethodCall();
            
            (var tlp, var itemGroups) = await _qfcQueue.TryDequeueAsync(Token, 4000);
            LoadItems(tlp, itemGroups);
            _parent.SwapStopWatch();
        }
        
        private async Task MoveAndIterate()
        {
            //TraceUtility.LogMethodCall();

            if ((_qfcQueue.Count + _qfcQueue.JobsRunning) > 0)
            {
                _groups.CacheMoveObjects();
                var moveTask = BackGroundMoveAsync();

                try
                {
                    await LoadUiFromQueue();
                    await _parent.IterateQueueAsync();
                }
                catch (System.Exception e)
                {
                    await moveTask;
                    log.Error(e.Message, e);
                    log.Debug("Shutting down QuickFiler");
                    await ActionCancelAsync();
                }

                //var iterate = _parent.IterateQueueAsync();
                
                await moveTask;
                //await iterate;
            }
            else if (_formViewer.Worker.IsBusy)
            {
                MessageBox.Show("Still loading emails. Please try again in a few seconds.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Either end of email database or error loading queue
                _groups.CacheMoveObjects();
                _parent.SwapStopWatch();
                await BackGroundMoveAsync();
                
                // If DataModel is not Complete then an error happened loading the queue
                if (!_parent.DataModel.Complete)
                {
                    // Since most common error is cross-thread error, we will try to load the queue again using the Ui Dispatcher
                    await UiThread.Dispatcher.InvokeAsync(_parent.IterateQueueAsync);
                }

                // We have reached the end of the email database
                else
                {
                    MessageBox.Show("Finished Moving Emails", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await ActionCancelAsync();
                }
                                
            }
        }

        internal async Task BackGroundMoveAsync()
        {
            //TraceUtility.LogMethodCall();

            // Move emails
            await _groups.MoveEmailsAsync(_movedItems);

            // Write Move Metrics
            await UiThread.Dispatcher.InvokeAsync(
                async () => await WriteMetrics(_globals.FS.Filenames.EmailSession),
                System.Windows.Threading.DispatcherPriority.ContextIdle);

            await UiThread.Dispatcher.InvokeAsync(() => _groups.CleanupBackground());

        }

        public void ButtonUndo_Click(object sender, EventArgs e) 
        {
            UndoDialog();
        }

        public void ButtonUndo_Click()
        {
            UndoDialog();
            //SortEmail.Undo(_movedItems, _globals.Ol.App);
        }

        public async void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null)
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);

            while (!_parent.WorkerComplete)
            {
                
                await Task.Delay(100);
            }
                        
            var count = (int)_formViewer.L1v1L2h5_SpnEmailPerLoad.Value;
            switch (count)
            {
                case int n when n == _itemsPerIteration:
                    // group actions for count equal to _itemsPerIteration. Do nothing.
                    break;
                case int n when n > _itemsPerIteration:
                    // group actions for count greater than _itemsPerIteration
                    _groups.UnregisterNavigation();
                    await _qfcQueue.ChangeIterationSize(
                        (_formViewer.L1v0L2L3v_TableLayout, _groups.ItemGroups), count, _rowStyleTemplate);
                    _groups.RegisterNavigation();
                    _itemsPerIteration = count;
                    break;
                case int n when n > 0:
                    // group actions for count less than _itemsPerIteration but greater than 0
                    break;
                default:
                    // group actions for count less than or equal to 0
                    // invalid value. maintain current setting.
                    _formViewer.L1v1L2h5_SpnEmailPerLoad.Value = _itemsPerIteration;
                    break;
            }
            

            
        }

        internal void AdjustTlp(TableLayoutPanel tlp, int newCount)
        {
            var oldCount = tlp.RowCount - 1;
            if (oldCount != newCount) 
            { 
                var diff = newCount - Math.Max(0, oldCount);
                if (diff > 0)
                {
                    tlp.InsertSpecificRow(oldCount, _rowStyleTemplate, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height +
                        (int)Math.Round(_rowStyleTemplate.Height * diff, 0));
                }
                else
                {
                    tlp.RemoveSpecificRow(newCount, diff);
                    tlp.MinimumSize = new System.Drawing.Size(
                        tlp.MinimumSize.Width,
                        tlp.MinimumSize.Height -
                        (int)Math.Round(_rowStyleTemplate.Height * diff, 0));
                }
            }
        }

        async public void ButtonSkip_Click(object sender, EventArgs e)
        {
            if (SynchronizationContext.Current is null) 
                SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
            
            _formViewer.L1v1L2h5_BtnSkip.Enabled = false;
            _formViewer.L1v1L2h5_BtnSkip.Text = "Skipping...";
            await SkipGroupAsync();
            _formViewer.L1v1L2h5_BtnSkip.Text = "Skip Group";
            _formViewer.L1v1L2h5_BtnSkip.Enabled = true;
        }

        async public Task SkipGroupAsync()
        {
            if ((_qfcQueue.Count + _qfcQueue.JobsRunning) > 0)
            {
                (var tlp, var itemGroups) = await _qfcQueue.TryDequeueAsync(Token, 4000);
                LoadItems(tlp, itemGroups);
                _parent.SwapStopWatch();
                var iterate = _parent.IterateQueueAsync();
                _groups.CleanupBackground();
                await iterate;
            }
            else if (_formViewer.Worker.IsBusy)
            {
                MessageBox.Show("Still loading emails. Please try again in a few seconds.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Cannot skip. This is the last group.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion Event Handlers

        #region Major Actions

        public void LoadItems(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)
        {
            _groups.LoadControlsAndHandlers_01(tlp, itemGroups);
        }

        public void LoadItems(IList<MailItem> listObjects)
        {            
            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  InitType: QfEnums.InitTypeEnum.Sort,
                                                  homeController: _parent,
                                                  parent: this,
                                                  tokenSource: TokenSource,
                                                  token: Token,
                                                  _states);
            _groups.LoadControlsAndHandlers_01(listObjects, _rowStyleTemplate, _rowStyleExpanded);
        }

        public async Task LoadItemsAsync(IList<MailItem> listObjects)
        {
            Token.ThrowIfCancellationRequested();

            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  InitType: QfEnums.InitTypeEnum.Sort,
                                                  homeController: _parent,
                                                  parent: this,
                                                  tokenSource: TokenSource,
                                                  token: Token,
                                                  _states);
            await _groups.LoadControlsAndHandlers_01Async(listObjects, _rowStyleTemplate, _rowStyleExpanded);
        }

        /// <summary>
        /// Maximizes the QfcFormViewer
        /// </summary>
        public void MaximizeFormViewer()
        {
            _formViewer.Invoke(new System.Action(() => _formViewer.WindowState = FormWindowState.Maximized));
        }
        
        /// <summary>
        /// Minimizes the QfcFormViewer
        /// </summary>
        public void MinimizeFormViewer()
        {
            _formViewer.Invoke(new System.Action(() => _formViewer.WindowState = FormWindowState.Minimized));
        }

        internal void UndoDialog()
        {
            _undoConsumerTask ??= Task.Run(UndoConsumer);
            var olApp = _globals.Ol.App;
            DialogResult repeatResponse = DialogResult.Yes;
            var i = 0;

            
            while (i < _movedItems.Count && repeatResponse == DialogResult.Yes)
            {
                var message = _movedItems[i].UndoMoveMessage(olApp);
                if (message is null) { i++; }
                else
                {
                    var undoResponse = MessageBox.Show(message, "Undo Dialog", MessageBoxButtons.YesNo);
                    if (undoResponse == DialogResult.Yes)
                    {
                        _undoQueue.Add(_movedItems.Pop(i));
                    }
                    else { i++; }
                    repeatResponse = MessageBox.Show("Continue Undoing Moves?", "Undo Dialog", MessageBoxButtons.YesNo);
                }
            }
            

            if (repeatResponse == DialogResult.Yes) { MessageBox.Show("Nothing to undo"); }
            _movedItems.Serialize();
        }

        internal async Task UndoConsumer()
        {
            var sw = new Stopwatch();
            sw.Start();
            bool exit = false;
            while (!_undoQueue.IsCompleted || exit)
            {
                if (_undoQueue.TryTake(out var item))
                {
                    var helper = await MailItemHelper.FromMailItemAsync(item.MailItem, _globals, default, true);
                    (await _globals.AF.Manager["Folder"]).UnTrain(helper.FolderInfo.RelativePath, helper.Tokens, 1);
                    var mail = item.UndoMove();
                    await UiThread.Dispatcher.InvokeAsync(
                        () => _groups.AddItemGroup(mail),
                        System.Windows.Threading.DispatcherPriority.ContextIdle);
                }
                else if (sw.ElapsedMilliseconds > 10000) { exit = true; }
                else { await Task.Delay(200); }
            }
            if (exit) { _undoConsumerTask = null;  }
        }

        // TODO: Implement Viewer_Activate
        public void Viewer_Activate()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
