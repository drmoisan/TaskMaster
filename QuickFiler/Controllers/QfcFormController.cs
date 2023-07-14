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

namespace QuickFiler.Controllers
{    
    internal class QfcFormController : IQfcFormController
    {
        public QfcFormController(IApplicationGlobals appGlobals,
                                 QfcFormViewer formViewer,
                                 Enums.InitTypeEnum initType,
                                 System.Action parentCleanup,
                                 IQfcHomeController parent)
        { 
            _globals = appGlobals;
            _initType = initType;
            _formViewer = formViewer;
            _formViewer.SetController(this);
            _parentCleanup = parentCleanup;
            _parent = parent;
            CaptureItemSettings();
            RemoveItemTemplate();
            SetupLightDark();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private QfcFormViewer _formViewer;
        private IQfcCollectionController _groups;
        private RowStyle _rowStyleTemplate;
        private Padding _itemMarginTemplate;
        private Enums.InitTypeEnum _initType;
        private bool _blRunningModalCode = false;
        private bool _blSuppressEvents = false;
        private IQfcHomeController _parent;

        public void CaptureItemSettings()
        {
            _rowStyleTemplate = _formViewer.L1v0L2L3v_TableLayout.RowStyles[0];
            _itemMarginTemplate = _formViewer.QfcItemViewerTemplate.Margin;
        }

        public void RemoveItemTemplate()
        {
            TableLayoutHelper.RemoveSpecificRow(_formViewer.L1v0L2L3v_TableLayout, 0);
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

        public int SpaceForEmail 
        { 
            get
            {
                var _screen = Screen.FromControl(_formViewer);
                int nonEmailSpace = (int)Math.Round(_formViewer.L1v_TableLayout.RowStyles[1].Height,0);
                int workingSpace = _screen.WorkingArea.Height;
                return workingSpace - nonEmailSpace;
            } 
        }

        public int ItemsPerIteration { get => (int)Math.Round(SpaceForEmail / _rowStyleTemplate.Height, 0); }

        public void LoadItems(IList<MailItem> listObjects)
        {
            _formViewer.ForAllControls(x =>
            {
                x.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_parent.KbdHndlr.KeyboardHandler_PreviewKeyDown);
                x.KeyDown += new System.Windows.Forms.KeyEventHandler(_parent.KbdHndlr.KeyboardHandler_KeyDown);
                x.KeyUp += new System.Windows.Forms.KeyEventHandler(_parent.KbdHndlr.KeyboardHandler_KeyUp);
                x.KeyPress += new System.Windows.Forms.KeyPressEventHandler(_parent.KbdHndlr.KeyboardHandler_KeyPress);
                // Debug.WriteLine($"Registered handler for {x.Name}");
            },
            new List<Control> { _formViewer.QfcItemViewerTemplate });

            _groups = new QfcCollectionController(AppGlobals: _globals,
                                                  viewerInstance: _formViewer,
                                                  darkMode: Properties.Settings.Default.DarkMode,
                                                  InitType: Enums.InitTypeEnum.InitSort,
                                                  keyboardHandler: _parent.KbdHndlr,
                                                  ParentObject: this);
            _groups.LoadControlsAndHandlers(listObjects, _rowStyleTemplate);
        }

        

        //public void FormResize(bool Force = false)
        //{
        //    throw new NotImplementedException();
        //}

        public void ButtonCancel_Click()
        {
            _formViewer.Hide();
            _groups.Cleanup();
            _globals = null;
            _groups = null;
            _formViewer.Close();
            _parentCleanup.Invoke();
    }

        public void ButtonOK_Click()
        {
            if (_initType.HasFlag(Enums.InitTypeEnum.InitSort))
            {
                if (_blRunningModalCode == false)
                {
                    _blRunningModalCode = true;

                    if (_groups.ReadyForMove)
                    {
                        _blSuppressEvents = true;
                        _parent.ExecuteMoves();
                        _blSuppressEvents = false;
                    }
                    _blRunningModalCode = false;
                }
                else
                {
                    MessageBox.Show("Can't Execute While Running Modal Code", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _formViewer.Close();
            }
        }

        public void ButtonUndo_Click()
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public void QFD_Maximize()
        {
            throw new NotImplementedException();
        }

        public void QFD_Minimize()
        {
            throw new NotImplementedException();
        }

        public void SpnEmailPerLoad_Change()
        {
            throw new NotImplementedException();
        }

        public void Viewer_Activate()
        {
            throw new NotImplementedException();
        }

        public IQfcCollectionController Groups { get => _groups; }

        

    }
}
