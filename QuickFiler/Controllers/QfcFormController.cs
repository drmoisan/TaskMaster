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

namespace QuickFiler.Controllers
{    
    internal class QfcFormController : IFilerFormController
    {
        #region Contructors

        public QfcFormController(IApplicationGlobals appGlobals,
                                 QfcFormViewer formViewer,
                                 Enums.InitTypeEnum initType,
                                 System.Action parentCleanup,
                                 QfcHomeController parent)
        { 
            _globals = appGlobals;
            _initType = initType;
            _formViewer = formViewer;
            _globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer;
            _formViewer.SetController(this);
            _parentCleanup = parentCleanup;
            _parent = parent;
            WriteMetrics = parent.QuickFileMetrics_WRITE;
            Iterate = parent.Iterate;
            _movedItems = parent.DataModel.MovedItems;
            
            CaptureItemSettings();
            RemoveItemTemplate();
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
        private int itemPanelHeight;
        private Padding _itemMarginTemplate;
        private Enums.InitTypeEnum _initType;
        private bool _blRunningModalCode = false;
        private bool _blSuppressEvents = false;
        private IFilerHomeController _parent;
        private int _itemsPerIteration = -1;
        private delegate void WriteMetricsDelegate(string filename);
        private WriteMetricsDelegate WriteMetrics;
        private delegate void IterateDelegate();
        private IterateDelegate Iterate;
        private ScoStack<IMovedMailInfo> _movedItems;

        #endregion

        #region Setup and Disposal

        public void CaptureItemSettings()
        {
            _rowStyleTemplate = _formViewer.L1v0L2L3v_TableLayout.RowStyles[0];
            _rowStyleExpanded = _formViewer.L1v0L2L3v_TableLayout.RowStyles[1];
            _itemMarginTemplate = _formViewer.QfcItemViewerTemplate.Margin;
            //_formViewer.L1v0L2_PanelMain.Height
        }

        public void RemoveItemTemplate()
        {
            TableLayoutHelper.RemoveSpecificRow(_formViewer.L1v0L2L3v_TableLayout, 0, 2);
        }

        public void SetupLightDark()
        {
            if (Properties.Settings.Default.DarkMode == true)
            {
                SetDarkMode();
            }
            _formViewer.DarkMode.Checked = Properties.Settings.Default.DarkMode;
            _formViewer.DarkMode.CheckedChanged += new System.EventHandler(DarkMode_CheckedChanged);
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

        public int ItemsPerIteration
        {
            get
            {
                if (_itemsPerIteration == -1)
                {
                    _itemsPerIteration = (int)Math.Round(SpaceForEmail / _rowStyleTemplate.Height, 0);
                    _formViewer.Invoke(new System.Action(() => _formViewer.L1v1L2h5_SpnEmailPerLoad.Value = _itemsPerIteration));
                }
                return _itemsPerIteration;
            }
            set => _itemsPerIteration = value;
        }

        public void RegisterFormEventHandlers()
        {
            _formViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_parent.KeyboardHndlr.KeyboardHandler_PreviewKeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyDown);
                //x.KeyUp += new System.Windows.Forms.KeyEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyUp);
                //x.KeyPress += new System.Windows.Forms.KeyPressEventHandler(_parent.KeyboardHndlr.KeyboardHandler_KeyPress);
                // Debug.WriteLine($"Registered handler for {x.Name}");
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
        
        private IQfcCollectionController _groups;
        public IQfcCollectionController Groups { get => _groups; }
        
        public IntPtr FormHandle { get => _formViewer.Handle; }
        
        private QfcFormViewer _formViewer;
        public QfcFormViewer FormViewer { get => _formViewer; }
        
        public void ToggleOffNavigation(bool async) => _groups.ToggleOffNavigation(async);
        
        public void ToggleOnNavigation(bool async) => _groups.ToggleOnNavigation(async);

        #endregion

        #region Event Handlers

        private void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_formViewer.DarkMode.Checked == true)
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

        public void ButtonCancel_Click(object sender, EventArgs e) => ActionCancel();

        public void ActionCancel()
        {
            _formViewer.Hide();
            _groups.Cleanup();
            _globals = null;
            _groups = null;
            _formViewer.Close();
            _parentCleanup.Invoke();
        }

        async public void ButtonOK_Click(object sender, EventArgs e) => await ActionOk();

        async public Task ActionOk()
        {
            if (_initType.HasFlag(Enums.InitTypeEnum.Sort))
            {
                if (_blRunningModalCode == false)
                {
                    if (_groups.ReadyForMove)
                    {
                        _blRunningModalCode = true;
                        _blSuppressEvents = true;

                        // Move emails
                        await _groups.MoveEmails(_movedItems);
                        
                        // Write move metrics
                        await Task.Run(()=>WriteMetrics(_globals.FS.Filenames.EmailSession)).ConfigureAwait(false);
                        
                        // Switch to UI thread
                        await _formViewer.UiSyncContext;
                        
                        // Cleanup the viewers and controllers for moved items
                        _groups.RemoveControls();
                        
                        // Launch viewers and controllers for the next items in queue
                        Iterate();

                        _blSuppressEvents = false;
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
                    $"Method {nameof(QfcFormController)}.{nameof(ActionOk)} has not been "+
                    $"implemented for {nameof(_initType)} {_initType}");
            }
        }

        public void ButtonUndo_Click(object sender, EventArgs e) => ButtonUndo_Click();

        // TODO: Implement ButtonUndo_Click
        public void ButtonUndo_Click()
        {
            throw new NotImplementedException();
        }

        public void SpnEmailPerLoad_ValueChanged(object sender, EventArgs e)
        {
            ItemsPerIteration = (int)_formViewer.L1v1L2h5_SpnEmailPerLoad.Value;
        }

        #endregion

        #region Major Actions

        public void LoadItems(IList<MailItem> listObjects)
        {            
            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  darkMode: Properties.Settings.Default.DarkMode,
                                                  InitType: Enums.InitTypeEnum.Sort,
                                                  homeController: _parent,
                                                  parent: this);
            _groups.LoadControlsAndHandlers(listObjects, _rowStyleTemplate, _rowStyleExpanded);
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
