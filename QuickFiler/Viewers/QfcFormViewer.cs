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
            //this.KeyPreview = true;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IFilerFormController _formController;
        private IQfcKeyboardHandler _keyboardHandler;

        public void SetController(IFilerFormController controller)
        {
            _formController = controller;
        }

        public void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler)
        {
            _keyboardHandler = keyboardHandler;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt)))
            {
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                _keyboardHandler.ToggleKeyboardDialog(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
                        
    }
}
