using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Properties;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    internal partial class EfcFormController : IFilerFormController
    {
        #region Constructors

        public EfcFormController(
            IApplicationGlobals AppGlobals,
            EfcDataModel dataModel,
            EfcViewer formViewer,
            EfcHomeController homeController,
            System.Action ParentCleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            _token = token;
            _globals = AppGlobals;
            _parentCleanup = ParentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _dataModel = dataModel;
            _initType = initType;
            _itemViewer = _formViewer.ItemViewer;
            _itemTlp = _formViewer.L0vh_TLP;
        }

        public EfcFormController(
            IApplicationGlobals globals,
            EfcViewer formViewer,
            EfcHomeController homeController,
            System.Action parentCleanup,
            QfEnums.InitTypeEnum initType,
            CancellationToken token
        )
        {
            _token = token;
            _globals = globals;
            _parentCleanup = parentCleanup;
            _formViewer = formViewer;
            _homeController = homeController;
            _initType = initType;
            _itemViewer = _formViewer.ItemViewer;
            _itemController = new EfcItemController(
                globals,
                homeController,
                this,
                _itemViewer,
                token
            );
            _itemTlp = _formViewer.L0vh_TLP;
        }

        private EfcFormController() { }

        internal EfcFormController Initialize()
        {
            LoadUserSettings();
            CaptureConfigureItemViewer();
            ConfigureFind();
            ResolveControlGroups();
            _itemController = new EfcItemController(
                _globals,
                _homeController,
                this,
                _itemViewer,
                _dataModel,
                _token
            );
            SetupThemes();
            WireEventHandlers();
            _ = PopulateFolderCombobox();
            return this;
        }

        internal EfcFormController InitializeWithoutData()
        {
            LoadUserSettings();
            CaptureConfigureItemViewer();
            ConfigureFind();
            ResolveControlGroups();
            _itemController.InitializeWithoutData();
            SetupThemes();
            WireEventHandlers();
            return this;
        }

        internal EfcFormController InitializeDataFields(EfcDataModel dataModel)
        {
            _dataModel = dataModel;
            _itemController.InitializeDataFields(dataModel);
            _ = PopulateFolderCombobox();
            return this;
        }

        #endregion Constructors

        #region Private Properties

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>Fault-boundary sink; an injectable seam over the static logger above.</summary>
        internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } =
            DefaultBoundaryErrorSink;

        /// <summary>
        /// Issue #736 finding 4: the default boundary sink. It logs the fault exactly as the
        /// previous lambda did and then surfaces it through <see cref="UserFaultNotifier"/>, whose
        /// own default returns without blocking the calling thread. A static method group is used
        /// because an instance property initializer cannot reference <c>this</c>.
        /// </summary>
        private static void DefaultBoundaryErrorSink(string message, System.Exception exception)
        {
            logger.Error(message, exception);
            UserFaultNotifier?.Invoke(message);
        }

        /// <summary>
        /// Issue #726 finding 5: invokes <see cref="BoundaryErrorSink"/> defensively so a null or
        /// throwing sink delegate cannot silently reinstate the unobserved-fault behavior this
        /// boundary exists to prevent -- these call sites all sit in an <c>async void</c> handler's
        /// catch block, where an escaping exception would crash the process rather than merely fail
        /// to log.
        /// </summary>
        private void TryReportBoundaryFault(string message, System.Exception exception)
        {
            var sink = BoundaryErrorSink;
            if (sink is null)
            {
                logger.Error(message, exception);
                return;
            }

            try
            {
                sink(message, exception);
            }
            catch (System.Exception sinkException)
            {
                logger.Error($"{message} (and the error sink itself threw)", sinkException);
                logger.Error(message, exception);
            }
        }

        // Issue #736 finding 4. Per-async-flow storage rather than a plain shared static: a shared
        // static races under the ClassLevel parallelization configured in the CLI runsettings, which
        // is the same reason MyBox.DialogInvoker in UtilitiesCS.Dialogs is written this way.
        private static readonly AsyncLocal<System.Action<string>> _userFaultNotifier =
            new AsyncLocal<System.Action<string>>();

        /// <summary>
        /// Issue #736 finding 4: the injectable user-facing surface the default boundary sink
        /// reports a fault through. The default is the non-blocking modeless notice below; a test
        /// installs its own capture and restores the previous value afterwards.
        /// </summary>
        internal static System.Action<string> UserFaultNotifier
        {
            get => _userFaultNotifier.Value ?? ShowModelessFaultNotice;
            set => _userFaultNotifier.Value = value;
        }

        /// <summary>
        /// Default user-facing surface for a boundary fault: a modeless, self-disposing, read-only
        /// notice. <c>Show()</c> returns immediately, so the calling thread is never blocked, and no
        /// modal dialog is invoked from this path.
        /// </summary>
        /// <remarks>
        /// Excluded from coverage by inspection: every statement past the early return constructs
        /// WinForms controls, which the headless test host cannot exercise. The early return is
        /// load-bearing rather than defensive — the pre-existing default-delegate test invokes the
        /// sink directly in the test host, and without it that invocation would construct a window
        /// on an MSTest thread. The accepted consequence is that a fault raised while the add-in has
        /// no open WinForms window is logged and not displayed.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private static void ShowModelessFaultNotice(string message)
        {
            if (System.Windows.Forms.Application.OpenForms.Count == 0)
            {
                return;
            }

            var notice = new Form();
            notice.Text = "QuickFiler";
            notice.TopMost = true;
            notice.ShowInTaskbar = false;
            notice.FormBorderStyle = FormBorderStyle.FixedDialog;
            notice.MinimizeBox = false;
            notice.MaximizeBox = false;
            notice.StartPosition = FormStartPosition.CenterScreen;
            notice.Width = 460;
            notice.Height = 160;

            var body = new TextBox();
            body.Text = message;
            body.ReadOnly = true;
            body.Multiline = true;
            body.Dock = DockStyle.Fill;
            body.BorderStyle = BorderStyle.None;
            notice.Controls.Add(body);

            notice.FormClosed += (sender, args) => notice.Dispose();
            notice.Show();
        }

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private EfcDataModel _dataModel;
        private EfcViewer _formViewer;

        // Presented folder-suggestion rows; retained so the delete path can prepend
        // "Trash to Delete" and rebind through the breadcrumb router.
        private string[] _folderRows = Array.Empty<string>();

        // Breadcrumb WebView2 wiring (#349): the exempt host adapter over the Designer control and
        // the non-exempt router that owns all breadcrumb logic. This controller stays wiring-only.
        private WebView2BreadcrumbHost _breadcrumbHost;
        private BreadcrumbBridgeRouter _router;

        private EfcHomeController _homeController;
        private EfcItemController _itemController;
        private ItemViewer _itemViewer;

        //private FolderHandler _folderHandler;
        //private MailItem _mailItem;
        private QfEnums.InitTypeEnum _initType;
        private IList<IQfcTipsDetails> _listTipsDetails;
        private TableLayoutPanel _itemTlp;
        private int _itemViewerTlpRow;
        private int _tlpHeightExpanded;
        private int _tlpHeightCollapsed;
        private int _tlpHeightDiff;
        private Dictionary<string, Theme> _themes;
        private List<Button> _listButtons;
        private List<Control> _listDefault;
        private List<Control> _listCheckBox;
        private List<Control> _listHighlighted;

        #endregion Private Properties
    }
}
