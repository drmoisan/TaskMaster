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
    internal partial class QfcFormViewer : Form
    {
        public QfcFormViewer()
        {
            InitializeComponent();
        }

        [STAThread]
        public static void Main()
        {
            if(!DpiInitialized)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                DpiInitialized = true;
            }
        }

        public static bool DpiInitialized { get; set; } = false;

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

        private void L1v1L2h2_ButtonOK_Click(object sender, EventArgs e)
        {
            _formController.ButtonOK_Click();
        }

        private void L1v1L2h3_ButtonCancel_Click(object sender, EventArgs e)
        {
            _formController.ButtonCancel_Click();
        }

        private void L1v1L2h4_ButtonUndo_Click(object sender, EventArgs e)
        {
            _formController.ButtonUndo_Click();
        }
    }
}
