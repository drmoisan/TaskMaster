using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Extensions;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler.Controllers
{
    internal partial class QfcFormController : IQfcFormController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region Contructors

        public QfcFormController(
            IApplicationGlobals appGlobals,
            IQfcFormViewer formViewer,
            IQfcQueue qfcQueue,
            QfEnums.InitTypeEnum initType,
            System.Action parentCleanup,
            IQfcHomeController parent,
            CancellationTokenSource tokenSource,
            CancellationToken token
        )
        {
            _token = token;
            _tokenSource = tokenSource;
            _globals = appGlobals;
            _initType = initType;
            _formViewer = formViewer;
            _globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer;
            _formViewer.SetController(this);
            _parentCleanup = parentCleanup;
            _parent = parent;
            WriteMetrics = parent.WriteMetricsAsync;
            Iterate = parent.Iterate;
            _movedItems = _globals.AF.MovedMails;
            _qfcQueue = qfcQueue;
        }

        public IQfcFormController Init()
        {
            CaptureItemSettings();
            RemoveTemplatesAndSetupTlp();
            SetupLightDark();
            RegisterFormEventHandlers();

            return this;
        }

        #endregion

        #region Private Variables

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private IApplicationGlobals _globals;
        private System.Action _parentCleanup;
        private RowStyle _rowStyleTemplate;
        private RowStyle _rowStyleExpanded;

        private Padding _itemMarginTemplate;
        private QfEnums.InitTypeEnum _initType;

        //private bool _blRunningModalCode = false;
        //private bool _blSuppressEvents = false;
        private IQfcHomeController _parent;
        private delegate Task WriteMetricsDelegate(string filename);
        private WriteMetricsDelegate WriteMetrics;
        private delegate void IterateDelegate();
        private IterateDelegate Iterate;
        private ScoStack<IMovedMailInfo> _movedItems;
        private IQfcQueue _qfcQueue;
        private TlpCellStates _states;
        private Dictionary<string, Theme> _themes;
        private BlockingCollection<IMovedMailInfo> _undoQueue = [];
        private Task _undoConsumerTask;
        private List<Task<MailItemHelper>> _helperTasks = [];

        #endregion


        #region Public Properties

        private string _activeTheme;
        public string ActiveTheme
        {
            get =>
                _themes is null
                    ? _activeTheme
                    : Initializer.GetOrLoad(ref _activeTheme, LoadTheme, strict: true, _themes);
            set =>
                Initializer.SetAndSave<string>(
                    ref _activeTheme,
                    value,
                    (x) =>
                    {
                        if (_themes is not null && _themes.TryGetValue(x, out var theme))
                        {
                            theme.SetTheme(async: true);
                        }
                    }
                );
        }

        internal string LoadTheme()
        {
            var activeTheme = (_globals?.Ol?.DarkMode ?? _darkMode) ? "DarkNormal" : "LightNormal";
            if (_themes is not null && _themes.TryGetValue(activeTheme, out var theme))
            {
                theme.SetTheme();
            }
            return activeTheme;
        }

        private bool _darkMode;
        public bool DarkMode
        {
            get =>
                _globals?.Ol is null
                    ? _darkMode
                    : Initializer.GetOrLoad(
                        ref _darkMode,
                        () => _globals.Ol.DarkMode,
                        false,
                        _globals,
                        _globals.Ol
                    );
            set =>
                Initializer.SetAndSave(
                    ref _darkMode,
                    value,
                    (x) =>
                    {
                        if (_globals?.Ol is not null)
                        {
                            _globals.Ol.DarkMode = x;
                        }
                    }
                );
        }

        private IQfcCollectionController _groups;
        public IQfcCollectionController Groups
        {
            get => _groups;
        }

        public IntPtr FormHandle
        {
            get => _formViewer.Handle;
        }

        private IQfcFormViewer _formViewer;
        public IQfcFormViewer FormViewer
        {
            get => _formViewer;
        }

        public void ToggleOffNavigation(bool async) => _groups.ToggleOffNavigation(async);

        public async Task ToggleOffNavigationAsync() => await _groups.ToggleOffNavigationAsync();

        public void ToggleOnNavigation(bool async) => _groups.ToggleOnNavigation(async);

        public async Task ToggleOnNavigationAsync() => await _groups.ToggleOnNavigationAsync();

        private CancellationToken _token;
        public CancellationToken Token
        {
            get => _token;
        }

        private CancellationTokenSource _tokenSource;
        public CancellationTokenSource TokenSource
        {
            get => _tokenSource;
        }

        #endregion
    }
}
