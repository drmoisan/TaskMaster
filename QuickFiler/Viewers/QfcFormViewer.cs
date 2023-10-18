using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler
{
    public partial class QfcFormViewer : Form
    {
        public QfcFormViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //this.KeyPreview = true;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IFilerFormController _formController;
        private IQfcKeyboardHandler _keyboardHandler;

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler { get => _uiScheduler; }

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
                SynchronizationContext.SetSynchronizationContext(UiSyncContext);
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                //_keyboardHandler.ToggleKeyboardDialog(sender, e);
                e.Handled = true;
                _ = _keyboardHandler.ToggleKeyboardDialogAsync();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        

    }
}
