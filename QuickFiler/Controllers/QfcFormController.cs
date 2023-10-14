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
using OneOf.Types;

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
            WriteMetrics = parent.QuickFileMetrics_WRITE;
            Iterate = parent.Iterate;
            _movedItems = _globals.AF.MovedMails;
            _qfcQueue = qfcQueue;
            
            CaptureItemSettings();
            RemoveTemplatesAndSetupTlp();
            SetupLightDark();
            RegisterFormEventHandlers();
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
        private bool _blRunningModalCode = false;
        //private bool _blSuppressEvents = false;
        private QfcHomeController _parent;
        private delegate void WriteMetricsDelegate(string filename);
        private WriteMetricsDelegate WriteMetrics;
        private delegate void IterateDelegate();
        private IterateDelegate Iterate;
        private ScoStack<IMovedMailInfo> _movedItems;
        private QfcQueue _qfcQueue;

        #endregion

        #region Setup and Disposal

        public void CaptureItemSettings()
        {
            _rowStyleTemplate = _formViewer.L1v0L2L3v_TableLayout.RowStyles[0];
            _rowStyleExpanded = _formViewer.L1v0L2L3v_TableLayout.RowStyles[1];
            _itemMarginTemplate = _formViewer.QfcItemViewerTemplate.Margin;
            //_formViewer.L1v0L2_PanelMain.Height
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
        }

        public void SetupLightDark()
        {
            if (_globals.Ol.DarkMode == true)
            {
                SetDarkMode();
            }
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
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_parent.KeyboardHndlr.KeyboardHandler_PreviewKeyDownAsync);
                //x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyDownAsync);
            },
            new List<Control> { _formViewer.QfcItemViewerTemplate });

            _formViewer.L1v1L2h2_ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            _formViewer.L1v1L2h3_ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            _formViewer.L1v1L2h4_ButtonUndo.Click += new System.EventHandler(this.ButtonUndo_Click);
            _formViewer.L1v1L2h5_SpnEmailPerLoad.ValueChanged += new System.EventHandler(this.SpnEmailPerLoad_ValueChanged);
        }
        
        /// <summary>
        /// Release all resources and call the parent cleanup
        /// </summary>
        public void Cleanup()
        {
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
            //if (_formViewer.DarkMode.Checked == true)
            if (_globals.Ol.DarkMode == true)
            {
                SetDarkMode();
            }
            else
            {
                SetLightMode();
            }
        }

        private void SetDarkMode()
        {
            _formViewer.L1v1L2h0_KeyboardDialog.BackColor = System.Drawing.Color.DimGray;
            _formViewer.L1v1L2h2_ButtonOK.BackColor = System.Drawing.Color.DimGray;
            _formViewer.L1v1L2h2_ButtonOK.ForeColor = System.Drawing.Color.WhiteSmoke;
            _formViewer.L1v1L2h2_ButtonOK.UseVisualStyleBackColor = false;
            _formViewer.L1v1L2h3_ButtonCancel.BackColor = System.Drawing.Color.DimGray;
            _formViewer.L1v1L2h3_ButtonCancel.ForeColor = System.Drawing.Color.WhiteSmoke;
            _formViewer.L1v1L2h3_ButtonCancel.UseVisualStyleBackColor = false;
            _formViewer.L1v1L2h4_ButtonUndo.BackColor = System.Drawing.Color.DimGray;
            _formViewer.L1v1L2h4_ButtonUndo.ForeColor = System.Drawing.Color.WhiteSmoke;
            _formViewer.L1v1L2h5_SpnEmailPerLoad.BackColor = System.Drawing.Color.DimGray;
            _formViewer.L1v1L2h5_SpnEmailPerLoad.ForeColor = System.Drawing.Color.Gainsboro;
            _formViewer.BackColor = Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
        }

        private void SetLightMode()
        {
            _formViewer.L1v1L2h0_KeyboardDialog.BackColor = System.Drawing.SystemColors.Window;
            _formViewer.L1v1L2h2_ButtonOK.BackColor = System.Drawing.SystemColors.Control;
            _formViewer.L1v1L2h2_ButtonOK.ForeColor = System.Drawing.SystemColors.ControlText;
            _formViewer.L1v1L2h2_ButtonOK.UseVisualStyleBackColor = true;
            _formViewer.L1v1L2h3_ButtonCancel.BackColor = System.Drawing.SystemColors.Control;
            _formViewer.L1v1L2h3_ButtonCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            _formViewer.L1v1L2h3_ButtonCancel.UseVisualStyleBackColor = true;
            _formViewer.L1v1L2h4_ButtonUndo.BackColor = System.Drawing.SystemColors.Control;
            _formViewer.L1v1L2h4_ButtonUndo.ForeColor = System.Drawing.SystemColors.ControlText;
            _formViewer.L1v1L2h5_SpnEmailPerLoad.BackColor = System.Drawing.SystemColors.Window;
            _formViewer.L1v1L2h5_SpnEmailPerLoad.ForeColor = System.Drawing.SystemColors.WindowText;
            _formViewer.BackColor = System.Drawing.SystemColors.ControlLightLight;
        }

        async public void ButtonCancel_Click(object sender, EventArgs e) => await ActionCancelAsync();

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
            await ActionOkAsync(); 
        }

        async public Task ActionOkAsync()
        {
            if (_initType.HasFlag(QfEnums.InitTypeEnum.Sort))
            {
                if (_blRunningModalCode == false)
                {
                    if (await UIThreadExtensions.UiDispatcher.InvokeAsync(()=>_groups.ReadyForMove))
                    {
                        _blRunningModalCode = true;
                        
                        if (_parent.KeyboardHndlr.KbdActive) { _parent.KeyboardHndlr.ToggleKeyboardDialog(); }

                        if (_qfcQueue.Count > 0) 
                        {
                            (var tlp, var itemGroups) = _qfcQueue.Dequeue();
                            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => LoadItems(tlp, itemGroups));
                        }
                        else 
                        { 
                            _groups.CacheMoveObjects(); 
                        }
                        _parent.SwapStopWatch();
                        _ = BackGroundMove();
                        _ = _parent.IterateQueueAsync();

                        //_blSuppressEvents = false;
                        _blRunningModalCode = false;
                    }
                }
                else
                {
                    MessageBox.Show("Can't Execute While Running Modal Code", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                throw new NotImplementedException(
                    $"Method {nameof(QfcFormController)}.{nameof(ActionOkAsync)} has not been "+
                    $"implemented for {nameof(_initType)} {_initType}");
            }
        }

        internal async Task BackGroundMove()
        {
            // Move emails
            await _groups.MoveEmailsAsync(_movedItems).ConfigureAwait(false);

            // Write Move Metrics
            await UIThreadExtensions.UiDispatcher.InvokeAsync(
                () => WriteMetrics(_globals.FS.Filenames.EmailSession),
                System.Windows.Threading.DispatcherPriority.ContextIdle);

            await UIThreadExtensions.UiDispatcher.InvokeAsync(() => _groups.CleanupBackground());

        }

        public void ButtonUndo_Click(object sender, EventArgs e) => ButtonUndo_Click();

        public void ButtonUndo_Click()
        {
            SortEmail.Undo(_movedItems, _globals.Ol.App);
        }

        public void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e)
        {
            var count = (int)_formViewer.L1v1L2h5_SpnEmailPerLoad.Value;
            if (count != _itemsPerIteration && count > 0)
            {
                ref TableLayoutPanel tlp = ref _formViewer.L1v0L2L3v_TableLayout;
                AdjustTlp(_formViewer.L1v0L2L3v_TableLayout, count);
                AdjustTlp(_qfcQueue.TlpTemplate, count);
                _itemsPerIteration = count;
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

        #endregion

        #region Major Actions

        public void LoadItems(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)
        {
            _groups.LoadControlsAndHandlers_01(tlp, itemGroups);
        }

        

        public void LoadItems(IList<MailItem> listObjects)
        {            
            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  darkMode: _globals.Ol.DarkMode,
                                                  InitType: QfEnums.InitTypeEnum.Sort,
                                                  homeController: _parent,
                                                  parent: this,
                                                  tokenSource: TokenSource,
                                                  token: Token);
            _groups.LoadControlsAndHandlers_01(listObjects, _rowStyleTemplate, _rowStyleExpanded);
        }

        public async Task LoadItemsAsync(IList<MailItem> listObjects)
        {
            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  darkMode: Properties.Settings.Default.DarkMode,
                                                  InitType: QfEnums.InitTypeEnum.Sort,
                                                  homeController: _parent,
                                                  parent: this,
                                                  tokenSource: TokenSource,
                                                  token: Token);
            await _groups.LoadControlsAndHandlersAsync_01(listObjects, _rowStyleTemplate, _rowStyleExpanded);
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

        // TODO: Implement Viewer_Activate
        public void Viewer_Activate()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
