using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace QuickFiler
{

    public partial class QuickFileViewer
    {
        private QuickFileController _controller;

        public QuickFileViewer()
        {
            InitializeComponent();
        }

        public void SetController(QuickFileController controller)
        {
            _controller = controller;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.HasFlag(Keys.Alt))
            {
                // If keyData = Keys.Up OrElse keyData = Keys.Down OrElse keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Alt Then
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                _controller.KeyboardHandler_KeyDown(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            _controller.ButtonOK_Click();
        }

        private void Button_OK_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.Button_OK_KeyDown(sender, e);
        }

        private void Button_OK_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.Button_OK_KeyUp(sender, e);
        }

        private void Button_Undo_Click(object sender, EventArgs e)
        {
            _controller.ButtonUndo_Click();
        }

        private void PanelMain_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.PanelMain_KeyDown(sender, e);
        }

        private void PanelMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            _controller.PanelMain_KeyPress(sender, e);
        }

        private void PanelMain_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.PanelMain_KeyUp(sender, e);
        }

        private void spn_EmailPerLoad_ValueChanged(object sender, EventArgs e)
        {
            _controller.SpnEmailPerLoad_Change();
        }

        private void QuickFileViewer_Activated(object sender, EventArgs e)
        {
            _controller.Viewer_Activate();
        }

        private void L1v2L2h4_ButtonCancel_Click(object sender, EventArgs e)
        {
            _controller.ButtonCancel_Click();
        }

        private void QuickFileViewer_Closing(object sender, CancelEventArgs e)
        {
            _controller.Cleanup();
        }

        private void AcceleratorDialogue_KeyDown(object sender, KeyEventArgs e)
        {
            _controller.AcceleratorDialogue_KeyDown(sender, e);
        }

        private void AcceleratorDialogue_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.AcceleratorDialogue_KeyUp(sender, e);
        }

        private void AcceleratorDialogue_TextChanged(object sender, EventArgs e)
        {
            _controller.AcceleratorDialogue_Change();
        }

        private void QuickFileViewer_Resize(object sender, EventArgs e)
        {
            if (_controller is not null)
                _controller.FormResize();
        }
    }
}