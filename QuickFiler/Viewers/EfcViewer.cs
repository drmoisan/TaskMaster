using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace QuickFiler
{
    public partial class EfcViewer : Form
    {
        public EfcViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            InitTipsLabelsList();
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SynchronizationContext _context;
        public SynchronizationContext UiSyncContext { get => _context; }

        private TaskScheduler _uiScheduler;
        public TaskScheduler UiScheduler { get => _uiScheduler; }

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

        private IList<Label> _tipsLabels;
        public IList<Label> TipsLabels { get => _tipsLabels; }

        private void InitTipsLabelsList()
        {
            _tipsLabels = new List<Label>
            {
                LblAcSearch,
                LblAcFolderList,
                LblAcTrash,
                LblAcAttachments,
                LblAcEmail,
                LblAcPictures,
                LblAcConversation,
                LblAcOk,
                LblAcCancel,
                LblAcRefresh,
                LblAcNewFolder
            };

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt)))
            {
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                _keyboardHandler.ToggleKeyboardDialogAsync(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
