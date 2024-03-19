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
using UtilitiesCS;

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

        private List<Control> _panels;
        public List<Control> Panels => Initializer.GetOrLoad(ref _panels, LoadPanels); 
        private List<Control> LoadPanels()
        {
            var panels = new List<Control> 
            { 
                this.L1v_TableLayout,
                this.L1v1L2h_TableLayout,
                this.L1v0L2L3v_TableLayout,
                this.L1v0L2_PanelMain,
            };
            return panels;
        }

        private List<Control> _buttons;
        public List<Control> Buttons => Initializer.GetOrLoad(ref _buttons, LoadButtons);
        private List<Control> LoadButtons()
        {
            var buttons = new List<Control>
            {
                this.L1v1L2h2_ButtonOK,
                this.L1v1L2h3_ButtonCancel,
                this.L1v1L2h4_ButtonUndo,
                this.ButtonFilters,
                this.L1v1L2h5_BtnSkip,
            };
            return buttons;
        }

    }
}
