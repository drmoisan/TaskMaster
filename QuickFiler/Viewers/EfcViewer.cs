using QuickFiler.Controllers;
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
    public partial class EfcViewer : Form
    {
        public EfcViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private EfcFormController _formController;
        internal void SetController(EfcFormController controller)
        {
            _formController = controller;
        }

        private IQfcKeyboardHandler _keyboardHandler;
        public void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler)
        {
            _keyboardHandler = keyboardHandler;
        }
    }
}
