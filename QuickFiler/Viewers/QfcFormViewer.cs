using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler
{
    public partial class QfcFormViewer : Form
    {
        public QfcFormViewer()
        {
            InitializeComponent();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IQfcFormController _formController;
        private IQfcKeyboardHandler _keyboardHandler;

        public void SetController(IQfcFormController controller)
        {
            _formController = controller;
        }

        public void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler)
        {
            _keyboardHandler = keyboardHandler;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.HasFlag(Keys.Alt))
            {
                // If keyData = Keys.Up OrElse keyData = Keys.Down OrElse keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Alt Then
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                _keyboardHandler.KeyboardHandler_KeyDown(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            _formController.ButtonOK_Click();
        }

        private void Button_OK_KeyDown(object sender, KeyEventArgs e)
        {
            _keyboardHandler.KeyboardHandler_KeyDown(sender, e);
        }

        private void Button_OK_KeyUp(object sender, KeyEventArgs e)
        {
            _keyboardHandler.KeyboardDialog_KeyUp(sender, e);
        }

        private void Button_Undo_Click(object sender, EventArgs e)
        {
            _formController.ButtonUndo_Click();
        }

        private void PanelMain_KeyDown(object sender, KeyEventArgs e)
        {
            _keyboardHandler.PanelMain_KeyDown(sender, e);
        }

        private void PanelMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            _keyboardHandler.PanelMain_KeyPress(sender, e);
        }

        private void PanelMain_KeyUp(object sender, KeyEventArgs e)
        {
            _keyboardHandler.PanelMain_KeyUp(sender, e);
        }

        private void spn_EmailPerLoad_ValueChanged(object sender, EventArgs e)
        {
            _formController.SpnEmailPerLoad_Change();
        }

        private void QuickFileViewer_Activated(object sender, EventArgs e)
        {
            _formController.Viewer_Activate();
        }

        private void L1v2L2h4_ButtonCancel_Click(object sender, EventArgs e)
        {
            _formController.ButtonCancel_Click();
        }

        private void QuickFileViewer_Closing(object sender, CancelEventArgs e)
        {
            _formController.Cleanup();
        }

        private void AcceleratorDialogue_KeyDown(object sender, KeyEventArgs e)
        {
            _keyboardHandler.KeyboardHandler_KeyDown(sender, e);
        }

        private void AcceleratorDialogue_KeyUp(object sender, KeyEventArgs e)
        {
            _keyboardHandler.KeyboardDialog_KeyUp(sender, e);
        }

        private void AcceleratorDialogue_TextChanged(object sender, EventArgs e)
        {
            _keyboardHandler.KeyboardDialog_Change();
        }

        private void QuickFileViewer_Resize(object sender, EventArgs e)
        {
            if (_formController is not null)
                _formController.FormResize();
        }
    }
}
