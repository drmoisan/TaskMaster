using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.Threading;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
[assembly: InternalsVisibleTo("TaskMaster.Test")]

namespace TaskMaster
{
    [ExcludeFromCodeCoverage]
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            logger.Debug("ThisAddIn_Startup() fired");

            // Ensure that forms are ready for high resolution
            InitializeDPI();

            // Grab the sync context for the UI thread
            UiThread.Init(monitorUiThread: false);

            Application.Startup += Application_Startup;
        }

        private void Application_Startup()
        {
            // why: Diagnosis-only (issue #211, Phase 3.3). Start the full add-in-startup-lifetime UI
            // heartbeat as the FIRST action so it measures the ENTIRE add-in startup, independent of
            // ApplicationGlobals.LoadSequentialAsync (which only spans ~3 s of a ~108 s freeze). Each
            // 250 ms DispatcherTimer tick emits one cheap [startup-lifetime-heartbeat] line whose
            // gapMs reveals exactly when and for how long the STA/UI thread was blocked. The
            // heartbeat self-stops on a max cap (~180 s) or sustained post-load responsiveness, and
            // is to be removed or gated after the latency is diagnosed. This does NOT change startup
            // order, the IdleAsyncQueue enqueue, or load semantics; the stage-label assignments below
            // are thin field writes only.
            StartStartupLifetimeHeartbeat();

            logger.Debug("Application_Startup() fired");
            //IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() =>
            //{
            SetUpBrightIdeasSettings();
            SetUpDeedle();
            //}));

            _currentStartupStageLabel = StartupStageLabels.GlobalsCtor;
            _globals = new ApplicationGlobals(Application, true);
            _ribbonController.SetGlobals(_globals);
            _externalUtilities.SetGlobals(_globals, _ribbonController);

            _currentStartupStageLabel = StartupStageLabels.AwaitingIdleQueue;
            IdleAsyncQueue.AddEntry(
                true,
                async () =>
                {
                    _currentStartupStageLabel = StartupStageLabels.Loading;
                    await _globals.LoadAsync(false);
                    logger.Debug("Finished loading globals");
                    _currentStartupStageLabel = StartupStageLabels.PostLoad;
                    _startupPostLoadReached = true;
                }
            );

            //IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() => _ribbonController.SetGlobals(_globals)));
            //IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() => _externalUtilities.SetGlobals(_globals, _ribbonController)));
            IdleAsyncQueue.AddEntry(
                false,
                async () => await Task.Run(() => logger.Debug("IdleAsyncQueue Complete"))
            );
            logger.Debug("Application_Startup() complete");
        }

        private void SetUpDeedle()
        {
            // Redirect the console output to the debug window for Deedle df.Print() calls
            DebugTextWriter tw = new();
            Console.SetOut(tw);
        }

        /// <summary>
        /// Set the indent for TreeListView Renderer which does not autoscale.
        /// Default pixels per level was 16 + 1 but designed for 100% scaling.
        /// This add-in is designed for 200% scaling.
        /// </summary>
        private void SetUpBrightIdeasSettings()
        {
            var tlvIndent = 34;
            tlvIndent = (int)(tlvIndent * UiThread.AutoScaleFactor.Width);
            BrightIdeasSoftware.TreeListView.TreeRenderer.PIXELS_PER_LEVEL = tlvIndent;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        private ApplicationGlobals _globals;
        private AddInUtilities _externalUtilities;
        private RibbonController _ribbonController;

        // Diagnosis-only full add-in-startup-lifetime UI-heartbeat seam (issue #211, Phase 3.3).
        // The DispatcherTimer/Stopwatch are host-bound and live here in the lifecycle-exempt class;
        // all formatting/stop decisions delegate to the coverable StartupDiagnosticsProbe and
        // StartupLifetimeStopDecider. Stopwatch only; no banned timing APIs.
        private const double StartupHeartbeatNominalMs = 250d;
        private DispatcherTimer _startupLifetimeHeartbeat;
        private Stopwatch _startupLifetimeTickStopwatch;
        private Stopwatch _startupLifetimeOverallStopwatch;
        private StartupDiagnosticsProbe _startupLifetimeProbe;
        private StartupLifetimeStopDecider _startupLifetimeStopDecider;
        private string _currentStartupStageLabel = StartupStageLabels.PreGlobalsCtor;
        private bool _startupPostLoadReached;

        // Diagnosis-only (issue #211, Phase 3.3): constructs the 250 ms DispatcherTimer on
        // UiThread.Dispatcher, starts both Stopwatches, and wires the per-tick handler. Each tick
        // measures the actual interval since the previous tick, emits one
        // [startup-lifetime-heartbeat] line via the probe, then feeds the overall-elapsed ms, the
        // current gap, and whether PostLoad was reached into the stop decider; when the decider
        // returns stop, the heartbeat self-stops. All formatting/decisions are delegated to the
        // coverable helpers; only the live timer/Stopwatch are host-bound and live here.
        private void StartStartupLifetimeHeartbeat()
        {
            _startupLifetimeProbe = new StartupDiagnosticsProbe(s => logger.Debug(s));
            _startupLifetimeStopDecider = new StartupLifetimeStopDecider();
            _startupLifetimeTickStopwatch = Stopwatch.StartNew();
            _startupLifetimeOverallStopwatch = Stopwatch.StartNew();
            var heartbeat = new DispatcherTimer(
                TimeSpan.FromMilliseconds(StartupHeartbeatNominalMs),
                DispatcherPriority.Background,
                (sender, e) =>
                {
                    var actualMs = _startupLifetimeTickStopwatch.Elapsed.TotalMilliseconds;
                    _startupLifetimeTickStopwatch.Restart();
                    _startupLifetimeProbe.EmitLifetimeHeartbeat(
                        _currentStartupStageLabel,
                        StartupHeartbeatNominalMs,
                        actualMs
                    );
                    var gapMs = actualMs - StartupHeartbeatNominalMs;
                    var elapsedMs = _startupLifetimeOverallStopwatch.Elapsed.TotalMilliseconds;
                    if (
                        _startupLifetimeStopDecider.ShouldStop(
                            elapsedMs,
                            gapMs,
                            _startupPostLoadReached
                        )
                    )
                    {
                        StopStartupLifetimeHeartbeat();
                    }
                },
                UiThread.Dispatcher
            );
            heartbeat.Start();
            _startupLifetimeHeartbeat = heartbeat;
        }

        // Diagnosis-only (issue #211, Phase 3.3): stops the heartbeat timer, detaches it, and
        // releases the reference so no permanent timer leaks. Idempotent: a second call is a no-op.
        private void StopStartupLifetimeHeartbeat()
        {
            if (_startupLifetimeHeartbeat is null)
            {
                return;
            }

            _startupLifetimeHeartbeat.Stop();
            _startupLifetimeHeartbeat = null;
        }

        /// <summary>
        /// Overrides the default behavior of the COM add-in to create an XML ribbon
        /// <seealso cref="RibbonViewer"/> which is controlled by
        /// <seealso cref="RibbonController"/>.
        /// </summary>
        /// <returns><seealso cref="IRibbonExtensibility"/> object</returns>
        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            _ribbonController = new RibbonController();
            return new RibbonViewer(_ribbonController);
        }

        /// <summary>
        /// Sets the DPI awareness for the application to enable high resolution with text scaling
        /// </summary>
        [STAThread]
        public static void InitializeDPI()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        }

        /// <summary>
        /// Overrides the default behavior of the COM add-in to expose specific methods
        /// to other office applications so that they can be called from VBA.
        /// </summary>
        /// <returns>Instance of the <seealso cref="AddInUtilities"/> class</returns>
        protected override object RequestComAddInAutomationService()
        {
            _externalUtilities ??= new AddInUtilities();

            return _externalUtilities;
        }

        //private async Task FinishLoadingGlobalsAsync()
        //{
        //    await loadGlobals;
        //    logger.Debug("Finished loading globals");

        //}

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
