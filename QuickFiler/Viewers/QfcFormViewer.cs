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
    public partial class QfcFormViewer : Form, IQfcFormViewer
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

        public virtual void SetController(IFilerFormController controller)
        {
            _formController = controller;
        }

        public virtual void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler)
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
        public virtual List<Control> Panels => Initializer.GetOrLoad(ref _panels, LoadPanels);
        private List<Control> LoadPanels()
        {
            var panels = new List<Control>
            {
                this._l1v_TableLayout,
                this.L1v1L2h_TableLayout,
                this._l1v0L2L3v_TableLayout,
                this._l1v0L2_PanelMain,
            };
            return panels;
        }

        private List<Control> _buttons;
        public virtual List<Control> Buttons => Initializer.GetOrLoad(ref _buttons, LoadButtons);


        private List<Control> LoadButtons()
        {
            var buttons = new List<Control>
            {
                this._l1v1L2h2_ButtonOK,
                this._l1v1L2h3_ButtonCancel,
                this._l1v1L2h4_ButtonUndo,
                this.ButtonFilters,
                this._l1v1L2h5_BtnSkip,
            };
            return buttons;
        }

        #region IQfcFormViewer
        
        public BackgroundWorker Worker => WorkerInternal;
        public TableLayoutPanel L1v0L2L3v_TableLayout { get => _l1v0L2L3v_TableLayout; set => _l1v0L2L3v_TableLayout = value; }
        public ItemViewer QfcItemViewerTemplate => _QfcItemViewerTemplate;
        public ItemViewerExpanded QfcItemViewerExpandedTemplate => _qfcItemViewerExpandedTemplate;
        public TableLayoutPanel L1v_TableLayout => _l1v_TableLayout;
        public System.Windows.Forms.NumericUpDown L1v1L2h5_SpnEmailPerLoad => _l1v1L2h5_SpnEmailPerLoad;
        public System.Windows.Forms.Button L1v1L2h2_ButtonOK => _l1v1L2h2_ButtonOK;
        public System.Windows.Forms.Button L1v1L2h3_ButtonCancel => _l1v1L2h3_ButtonCancel;
        public System.Windows.Forms.Button L1v1L2h4_ButtonUndo => _l1v1L2h4_ButtonUndo;
        public System.Windows.Forms.Button L1v1L2h5_BtnSkip => _l1v1L2h5_BtnSkip;
        public Panel L1v0L2_PanelMain => _l1v0L2_PanelMain;
        #endregion IQfcFormViewer

    }
}
