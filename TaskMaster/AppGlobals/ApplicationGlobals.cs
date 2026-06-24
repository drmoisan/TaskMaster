using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using TaskMaster.Properties;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{
    public class ApplicationGlobals : IApplicationGlobals
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        private Application _outlookApp;

        // Diagnostic startup-timing instrumentation (issue #202). The recorder is selected in
        // LoadAsync from Settings.Default.StartupTimingEnabled: the concrete recorder when
        // enabled, the no-op recorder when disabled, so the coordinator records and emits
        // unconditionally. _loadBasicElapsed holds the LoadBasicMethod() measurement, which is
        // taken at construction time (via the BasicLoaded Lazy) before LoadAsync runs.
        private IStartupTimingRecorder _timingRecorder = new NullStartupTimingRecorder();
        private TimeSpan _loadBasicElapsed;

        public ApplicationGlobals(Application olApp)
        {
            _outlookApp = olApp;
            BasicLoaded = new Lazy<bool>(() =>
            {
                LoadBasicMethod();
                return true;
            });
        }

        public ApplicationGlobals(Application olApp, bool loadBasic)
        {
            _outlookApp = olApp;
            BasicLoaded = new Lazy<bool>(() =>
            {
                LoadBasicMethod();
                return true;
            });
            if (loadBasic)
            {
                ForceBasicLoad();
            }
        }

        public async Task LoadAsync(bool parallel = true)
        {
            ForceBasicLoad();

            // Read the diagnostic timing flag exactly once (mirrors the Settings.Default
            // consumption pattern used for EventsHooked). When enabled, record the
            // construction-time LoadBasic measurement as the first phase; when disabled, the
            // no-op recorder absorbs all recording and emission.
            var timingEnabled = Settings.Default.StartupTimingEnabled;
            if (timingEnabled)
            {
                _timingRecorder = new StartupTimingRecorder();
                _timingRecorder.RecordPhase("LoadBasic", _loadBasicElapsed);
            }
            else
            {
                _timingRecorder = new NullStartupTimingRecorder();
            }

            if (parallel)
            {
                await LoadParallelAsync();
            }
            else
            {
                await LoadSequentialAsync();
            }

            // Emit the single [Startup timing] table at the end of startup. On the flag-off
            // path the no-op recorder emits nothing.
            _timingRecorder.EmitTable(logger);
        }

        internal Lazy<bool> BasicLoaded;

        private void ForceBasicLoad()
        {
            _ = BasicLoaded.Value;
        }

        // protected internal virtual to provide a test seam: focused MSTests override this to
        // set _loadBasicElapsed deterministically and skip live COM collaborator construction
        // while still driving LoadAsync end-to-end. Production behavior is unchanged.
        protected internal virtual void LoadBasicMethod()
        {
            // The LoadBasic measurement is UNCONDITIONAL: ForceBasicLoad() runs inside the
            // ApplicationGlobals(Application, loadBasic: true) constructor, materializing the
            // BasicLoaded Lazy BEFORE LoadAsync runs, so measuring around ForceBasicLoad() in
            // LoadAsync would record ~0. A single Stopwatch start/stop with no allocation is
            // negligible overhead, satisfying the "negligible overhead when flag off" constraint.
            // Stopwatch (hardware-counter based) is used instead of DateTime.Now/UtcNow.
            var stopwatch = Stopwatch.StartNew();
            _fs = new AppFileSystemFolderPaths();
            _olObjects = new AppOlObjects(_outlookApp, this);
            _toDoObjects = new AppToDoObjects(this);
            _autoFileObjects = new AppAutoFileObjects(this);
            _events = new AppEvents(this);
            _quickFilerSettings = new AppQuickFilerSettings();
            Engines = new AppItemEngines(this);
            stopwatch.Stop();
            _loadBasicElapsed = stopwatch.Elapsed;
        }

        public async Task LoadParallelAsync()
        {
            await LoadIntelConfigAsync();
            await Task.WhenAll(
                _toDoObjects.LoadAsync(),
                _autoFileObjects.LoadAsync(),
                _olObjects.LoadAsync()
            );
            await Engines.InitAsync();
            await _events.LoadAsync();
        }

        public async Task LoadSequentialAsync()
        {
            // Each phase keeps its existing direct await (COM-sensitive phases stay on the
            // caller thread) and is wrapped with a Stopwatch so its elapsed time is recorded
            // once, in startup order. The recorder is the no-op recorder unless the timing flag
            // is on, so nothing is recorded when disabled. Yield calls are not recorded.
            var stopwatch = Stopwatch.StartNew();
            await LoadIntelConfigPhaseAsync();
            _timingRecorder.RecordPhase("IntelConfig", StopAndRestart(stopwatch));
            await YieldWithContinuationProbeAsync("IntelConfig");
            await LoadOlObjectsPhaseAsync();
            _timingRecorder.RecordPhase("OlObjects", StopAndRestart(stopwatch));
            await YieldWithContinuationProbeAsync("OlObjects");
            await LoadToDoPhaseAsync();
            _timingRecorder.RecordPhase("ToDo", StopAndRestart(stopwatch));
            await YieldWithContinuationProbeAsync("ToDo");
            await LoadAutoFilePhaseAsync();
            _timingRecorder.RecordPhase("AutoFile", StopAndRestart(stopwatch));
            await YieldWithContinuationProbeAsync("AutoFile");

            // Diagnosis-only instrumentation (issue #211, Phase 3.1): measure whether the UI/STA
            // thread is starved or suspended during the Engines-phase SpamBayes deserialization,
            // and attribute any stall to GC. Behavior-preserving: the only inserted statements are
            // the heartbeat start/stop and the before/after GC reads; the Engines phase await,
            // RecordPhase call, and the following yield are unchanged. The host-bound scheduling
            // (DispatcherTimer on UiThread.Dispatcher) and the live GC.* reads stay behind the
            // protected internal virtual seams below so focused MSTests can no-op them without a
            // live UI host; only the gap/GC-delta formatting goes through the coverable
            // StartupDiagnosticsProbe helper.
            var diagnosticsProbe = new StartupDiagnosticsProbe(s => logger.Debug(s));
            StartEnginesUiHeartbeat(diagnosticsProbe);
            BeginEnginesGcCapture();
            try
            {
                await InitializeEnginesPhaseAsync();
                _timingRecorder.RecordPhase("Engines", StopAndRestart(stopwatch));
            }
            finally
            {
                StopEnginesUiHeartbeat();
            }
            EmitEnginesGcDelta(diagnosticsProbe);

            await YieldWithContinuationProbeAsync("Engines");
            await LoadEventsPhaseAsync();
            _timingRecorder.RecordPhase("Events", StopAndRestart(stopwatch));
        }

        // Host-bound heartbeat scheduling held in a per-load field so the start/stop seams share
        // the before-phase GC snapshot and the running timer without exposing them on the public
        // surface. Null in the flag-off/test seams (the seams below are overridden to no-op).
        private System.Windows.Threading.DispatcherTimer? _enginesHeartbeat;
        private int _enginesGcGen0Before;
        private int _enginesGcGen1Before;
        private int _enginesGcGen2Before;
        private long _enginesGcBytesBefore;

        // Diagnosis-only (issue #211, Phase 3.1) seam: starts a recurring UI-thread responsiveness
        // heartbeat on UiThread.Dispatcher at a 250 ms nominal interval. Each tick reads the actual
        // elapsed interval since the previous tick from a restart-per-tick Stopwatch and emits one
        // [ui-heartbeat] line via the coverable probe. The DispatcherTimer scheduling and the
        // per-tick Stopwatch are host-bound and live here in the thin call site; only the gap
        // formatting is coverable. The started timer is held in a private field so the seam
        // signature does not leak the WindowsBase DispatcherTimer type onto the (overridable)
        // surface. protected internal virtual so focused MSTests override it to a no-op without
        // constructing a live Dispatcher.
        protected internal virtual void StartEnginesUiHeartbeat(StartupDiagnosticsProbe probe)
        {
            const double nominalMs = 250d;
            var tickStopwatch = Stopwatch.StartNew();
            var heartbeat = new System.Windows.Threading.DispatcherTimer(
                TimeSpan.FromMilliseconds(nominalMs),
                System.Windows.Threading.DispatcherPriority.Background,
                (sender, e) =>
                {
                    var actualMs = tickStopwatch.Elapsed.TotalMilliseconds;
                    tickStopwatch.Restart();
                    probe.EmitHeartbeat(nominalMs, actualMs);
                },
                UiThread.Dispatcher
            );
            heartbeat.Start();
            _enginesHeartbeat = heartbeat;
        }

        // Diagnosis-only (issue #211, Phase 3.1) seam: stops/disposes the heartbeat after the
        // Engines phase. protected internal virtual so focused MSTests override it to a no-op.
        protected internal virtual void StopEnginesUiHeartbeat()
        {
            _enginesHeartbeat?.Stop();
            _enginesHeartbeat = null;
        }

        // Diagnosis-only (issue #211, Phase 3.1) seam: captures the live GC collection counts and
        // allocated bytes immediately before the Engines phase. The GC.* reads are host-state reads
        // and stay here in the thin call site. protected internal virtual so focused MSTests
        // override it to a no-op.
        protected internal virtual void BeginEnginesGcCapture()
        {
            _enginesGcGen0Before = GC.CollectionCount(0);
            _enginesGcGen1Before = GC.CollectionCount(1);
            _enginesGcGen2Before = GC.CollectionCount(2);
            _enginesGcBytesBefore = GC.GetTotalMemory(false);
        }

        // Diagnosis-only (issue #211, Phase 3.1) seam: reads the live GC counts/bytes and GCSettings
        // again after the Engines phase, computes the deltas, and emits one [gc-delta] line via the
        // coverable probe. The GC.*/GCSettings.* reads stay here in the thin call site; only the
        // delta formatting is coverable. protected internal virtual so focused MSTests override it
        // to a no-op.
        protected internal virtual void EmitEnginesGcDelta(StartupDiagnosticsProbe probe)
        {
            probe.EmitGcDelta(
                GC.CollectionCount(0) - _enginesGcGen0Before,
                GC.CollectionCount(1) - _enginesGcGen1Before,
                GC.CollectionCount(2) - _enginesGcGen2Before,
                GC.GetTotalMemory(false) - _enginesGcBytesBefore,
                System.Runtime.GCSettings.IsServerGC,
                System.Runtime.GCSettings.LatencyMode.ToString()
            );
        }

        // Captures the elapsed time of the just-completed phase and resets the shared stopwatch
        // for the next phase. The yield between phases is excluded because the stopwatch is read
        // and restarted immediately after each phase's await completes.
        private static TimeSpan StopAndRestart(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
            stopwatch.Restart();
            return elapsed;
        }

        // These narrow wrappers keep production behavior unchanged while letting focused MSTests
        // drive the real coordinator sequence without constructing the full Outlook/VSTO runtime.
        protected internal virtual Task LoadIntelConfigPhaseAsync() => LoadIntelConfigAsync();

        // Continuation-latency attribution probe (issue #211). Measures how long the inter-phase
        // continuation waits to resume on the STA after the single Task.Yield (waitMs is the
        // attribution number), and captures cheap STA-occupancy signals at the moment the
        // continuation resumes. Behavior is preserved: this still performs exactly one Task.Yield
        // back to the Dispatcher. Stopwatch only; no banned timing APIs.
        protected internal virtual async Task YieldWithContinuationProbeAsync(string priorPhaseName)
        {
            var sw = Stopwatch.StartNew();
            await Task.Yield();
            sw.Stop();
            logger.Debug(
                $"[continuation-resume] priorPhase={priorPhaseName} "
                    + $"waitMs={sw.Elapsed.TotalMilliseconds:F1} "
                    + $"resumeThreadId={System.Threading.Thread.CurrentThread.ManagedThreadId} "
                    + $"resumeSyncContext={System.Threading.SynchronizationContext.Current?.GetType().FullName ?? "null"} "
                    + $"staIsIdle={UtilitiesCS.Threading.ApplicationIdleTimer.IsIdle} "
                    + $"staCpuUsage={UtilitiesCS.Threading.ApplicationIdleTimer.CurrentCPUUsage:F3} "
                    + $"staGuiActivity={UtilitiesCS.Threading.ApplicationIdleTimer.CurrentGUIActivity:F1}"
            );
        }

        protected internal virtual Task LoadOlObjectsPhaseAsync() => _olObjects.LoadAsync();

        protected internal virtual Task LoadToDoPhaseAsync() => _toDoObjects.LoadAsync(false);

        protected internal virtual Task LoadAutoFilePhaseAsync() =>
            _autoFileObjects.LoadAsync(false);

        protected internal virtual Task InitializeEnginesPhaseAsync() =>
            Task.Run(() => Engines.InitAsync());

        protected internal virtual Task LoadEventsPhaseAsync() => _events.LoadAsync();

        public void LoadWhenIdle()
        {
            IdleAsyncQueue.AddEntry(
                false,
                () => Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync())
            );
            IdleAsyncQueue.AddEntry(false, Engines.InitAsync);
            IdleAsyncQueue.AddEntry(false, _events.LoadAsync);
        }

        private AppFileSystemFolderPaths _fs;
        public IFileSystemFolderPaths FS => _fs;

        private AppOlObjects _olObjects;
        public IOlObjects Ol => _olObjects;

        private AppToDoObjects _toDoObjects;
        public IToDoObjects TD => _toDoObjects;

        private AppAutoFileObjects _autoFileObjects;
        public IAppAutoFileObjects AF => _autoFileObjects;

        private AppEvents _events;
        public IAppEvents Events => _events;

        private AppQuickFilerSettings _quickFilerSettings;
        public IAppQuickFilerSettings QfSettings => _quickFilerSettings;
        internal AppQuickFilerSettings InternalQfSettings => _quickFilerSettings;

        public IntelligenceConfig IntelRes { get; private set; }

        private async Task LoadIntelConfigAsync() =>
            await Task.Run(
                async () => IntelRes = await IntelligenceConfig.LoadAsync(this),
                default
            );

        public IAppItemEngines Engines { get; private set; }

        public List<Type> GetClasses()
        {
            return ReflectionHelper.GetAllClassesInSolution();
        }

        public string[] GetProjectNames()
        {
            //ProjectCollection.GlobalProjectCollection.LoadedProjects
            return AppDomain
                .CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetName().Name)
                .ToArray();
        }

        #region Legacy Definitions and Constants


        #endregion
    }
}
