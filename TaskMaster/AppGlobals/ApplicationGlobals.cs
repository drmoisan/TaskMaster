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
            //
            // Diagnosis-only instrumentation (issue #211, Phase 3.2): the UI/STA-responsiveness
            // heartbeat and the per-phase GC delta probe now span the ENTIRE sequential startup,
            // not only the Engines phase, so the next capture shows whether the STA is actually
            // frozen during the slow phase (heartbeat gaps approximately equal to phase duration)
            // or whether an async continuation merely waits while the STA stays responsive.
            // Behavior-preserving: the only inserted statements are the current-phase marker
            // assignments, the heartbeat start/stop, and the before/after GC reads bracketing each
            // existing phase await; the phase awaits, RecordPhase calls, and the inter-phase yields
            // are unchanged. The host-bound scheduling (DispatcherTimer on UiThread.Dispatcher) and
            // the live GC.*/GCSettings.* reads stay behind the protected internal virtual seams
            // below so focused MSTests can no-op them without a live UI host; only the gap/GC-delta
            // formatting goes through the coverable StartupDiagnosticsProbe helper. The heartbeat is
            // started before the first phase and stopped in a finally so it always stops even if a
            // phase throws.
            var stopwatch = Stopwatch.StartNew();
            var diagnosticsProbe = new StartupDiagnosticsProbe(s => logger.Debug(s));
            StartStartupUiHeartbeat(diagnosticsProbe);
            try
            {
                // Per-phase NET attribution (issue #211, Phase 3.6): sample the process-global
                // StoreWrapperInitClock immediately before each phase await; the gross elapsed is
                // captured inline by the existing RecordPhase(StopAndRestart(...)) statement (kept as
                // the sole statement between each phase await and its yield); the resulting
                // [phase-net] line is emitted alongside the per-phase [gc-delta] after the yield. The
                // store-init delta is computed at emit time (no StoreWrapperInitClock.Add occurs
                // during a Task.Yield, so the post-yield after-sample equals the pre-yield value).
                // The RecordPhase gross-table call, the YieldWithContinuationProbeAsync call, and the
                // EmitPhaseGcDelta call are unchanged and keep their existing order. The only live
                // clock read is SampleStoreWrapperInitTotalMs (a seam).
                double storeInitBefore;
                TimeSpan phaseElapsed;

                BeginPhase("IntelConfig");
                storeInitBefore = SampleStoreWrapperInitTotalMs();
                await LoadIntelConfigPhaseAsync();
                _timingRecorder.RecordPhase(
                    "IntelConfig",
                    phaseElapsed = StopAndRestart(stopwatch)
                );
                await YieldWithContinuationProbeAsync("IntelConfig");
                EmitPhaseGcDelta(diagnosticsProbe, "IntelConfig");
                EmitPhaseNet(diagnosticsProbe, "IntelConfig", phaseElapsed, storeInitBefore);

                BeginPhase("OlObjects");
                storeInitBefore = SampleStoreWrapperInitTotalMs();
                await LoadOlObjectsPhaseAsync();
                _timingRecorder.RecordPhase("OlObjects", phaseElapsed = StopAndRestart(stopwatch));
                await YieldWithContinuationProbeAsync("OlObjects");
                EmitPhaseGcDelta(diagnosticsProbe, "OlObjects");
                EmitPhaseNet(diagnosticsProbe, "OlObjects", phaseElapsed, storeInitBefore);

                BeginPhase("ToDo");
                storeInitBefore = SampleStoreWrapperInitTotalMs();
                await LoadToDoPhaseAsync();
                _timingRecorder.RecordPhase("ToDo", phaseElapsed = StopAndRestart(stopwatch));
                await YieldWithContinuationProbeAsync("ToDo");
                EmitPhaseGcDelta(diagnosticsProbe, "ToDo");
                EmitPhaseNet(diagnosticsProbe, "ToDo", phaseElapsed, storeInitBefore);

                BeginPhase("AutoFile");
                storeInitBefore = SampleStoreWrapperInitTotalMs();
                await LoadAutoFilePhaseAsync();
                _timingRecorder.RecordPhase("AutoFile", phaseElapsed = StopAndRestart(stopwatch));
                await YieldWithContinuationProbeAsync("AutoFile");
                EmitPhaseGcDelta(diagnosticsProbe, "AutoFile");
                EmitPhaseNet(diagnosticsProbe, "AutoFile", phaseElapsed, storeInitBefore);

                BeginPhase("Engines");
                storeInitBefore = SampleStoreWrapperInitTotalMs();
                await InitializeEnginesPhaseAsync();
                _timingRecorder.RecordPhase("Engines", phaseElapsed = StopAndRestart(stopwatch));
                await YieldWithContinuationProbeAsync("Engines");
                EmitPhaseGcDelta(diagnosticsProbe, "Engines");
                EmitPhaseNet(diagnosticsProbe, "Engines", phaseElapsed, storeInitBefore);

                BeginPhase("Events");
                storeInitBefore = SampleStoreWrapperInitTotalMs();
                await LoadEventsPhaseAsync();
                _timingRecorder.RecordPhase("Events", phaseElapsed = StopAndRestart(stopwatch));
                EmitPhaseGcDelta(diagnosticsProbe, "Events");
                EmitPhaseNet(diagnosticsProbe, "Events", phaseElapsed, storeInitBefore);
            }
            finally
            {
                StopStartupUiHeartbeat();
            }
        }

        // Marks the upcoming startup phase as in-flight (issue #211, Phase 3.2): records the
        // current-phase name so each heartbeat tick is attributed to this phase, and takes the
        // before-GC snapshot for the per-phase [gc-delta]. This is a thin marker + GC-snapshot
        // bracket immediately before the phase await; it adds no awaits and does not change phase
        // order, the phase set, or load semantics. The live GC reads stay inside BeginPhaseGcCapture.
        private void BeginPhase(string phase)
        {
            _currentStartupPhase = phase;
            BeginPhaseGcCapture(phase);
        }

        // Host-bound heartbeat scheduling held in a per-load field so the start/stop seams share
        // the running timer without exposing it on the public surface. Null in the flag-off/test
        // seams (the seams below are overridden to no-op). The per-phase GC before-snapshot fields
        // are overwritten at each phase boundary by BeginPhaseGcCapture. The current-phase marker
        // is read by each heartbeat tick so each [ui-heartbeat] line is attributed to the phase
        // whose body/await is in flight (issue #211, Phase 3.2).
        private System.Windows.Threading.DispatcherTimer? _startupHeartbeat;
        private string _currentStartupPhase = string.Empty;
        private int _phaseGcGen0Before;
        private int _phaseGcGen1Before;
        private int _phaseGcGen2Before;
        private long _phaseGcBytesBefore;

        // Diagnosis-only (issue #211, Phase 3.2) seam: starts a recurring UI-thread responsiveness
        // heartbeat on UiThread.Dispatcher at a 250 ms nominal interval spanning the entire
        // sequential startup. Each tick reads the actual elapsed interval since the previous tick
        // from a restart-per-tick Stopwatch and the current-phase marker, and emits one phase-
        // annotated [ui-heartbeat] line via the coverable probe. The DispatcherTimer scheduling and
        // the per-tick Stopwatch are host-bound and live here in the thin call site; only the gap
        // formatting is coverable. The started timer is held in a private field so the seam
        // signature does not leak the WindowsBase DispatcherTimer type onto the (overridable)
        // surface. protected internal virtual so focused MSTests override it to a no-op without
        // constructing a live Dispatcher.
        protected internal virtual void StartStartupUiHeartbeat(StartupDiagnosticsProbe probe)
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
                    probe.EmitHeartbeat(_currentStartupPhase, nominalMs, actualMs);
                },
                UiThread.Dispatcher
            );
            heartbeat.Start();
            _startupHeartbeat = heartbeat;
        }

        // Diagnosis-only (issue #211, Phase 3.2) seam: stops/disposes the heartbeat after the last
        // phase. protected internal virtual so focused MSTests override it to a no-op.
        protected internal virtual void StopStartupUiHeartbeat()
        {
            _startupHeartbeat?.Stop();
            _startupHeartbeat = null;
        }

        // Diagnosis-only (issue #211, Phase 3.2) seam: captures the live GC collection counts and
        // allocated bytes immediately before the named phase. The GC.* reads are host-state reads
        // and stay here in the thin call site. protected internal virtual so focused MSTests
        // override it to a no-op.
        protected internal virtual void BeginPhaseGcCapture(string phase)
        {
            _phaseGcGen0Before = GC.CollectionCount(0);
            _phaseGcGen1Before = GC.CollectionCount(1);
            _phaseGcGen2Before = GC.CollectionCount(2);
            _phaseGcBytesBefore = GC.GetTotalMemory(false);
        }

        // Diagnosis-only (issue #211, Phase 3.2) seam: reads the live GC counts/bytes and GCSettings
        // again after the named phase, computes the deltas, and emits one phase-annotated [gc-delta]
        // line via the coverable probe. The GC.*/GCSettings.* reads stay here in the thin call site;
        // only the delta formatting is coverable. protected internal virtual so focused MSTests
        // override it to a no-op.
        protected internal virtual void EmitPhaseGcDelta(
            StartupDiagnosticsProbe probe,
            string phase
        )
        {
            probe.EmitGcDelta(
                phase,
                GC.CollectionCount(0) - _phaseGcGen0Before,
                GC.CollectionCount(1) - _phaseGcGen1Before,
                GC.CollectionCount(2) - _phaseGcGen2Before,
                GC.GetTotalMemory(false) - _phaseGcBytesBefore,
                System.Runtime.GCSettings.IsServerGC,
                System.Runtime.GCSettings.LatencyMode.ToString()
            );
        }

        // Diagnosis-only (issue #211, Phase 3.6) seam: reads the live process-global
        // StoreWrapperInitClock snapshot. This is the ONLY live clock read for the [phase-net]
        // probe; it stays here in the thin call site (mirroring the live GC reads above) so the
        // per-phase NET arithmetic and formatting in the coverable StartupDiagnosticsProbe stay
        // testable. protected internal virtual so focused MSTests override it to a deterministic
        // value without touching the process-global accumulator.
        protected internal virtual double SampleStoreWrapperInitTotalMs() =>
            UtilitiesCS.OutlookObjects.Store.StoreWrapperInitClock.TotalMs;

        // Diagnosis-only (issue #211, Phase 3.6) bracket: samples the StoreWrapperInitClock again
        // after the named phase, computes the StoreWrapper-init delta attributed to the phase window
        // (afterSample - beforeSample), and emits one additive [phase-net] line via the coverable
        // probe. The net arithmetic (gross - storeWrapperInitMs, clamped at 0.0) and the formatting
        // both live in StartupDiagnosticsProbe; only the live after-sample read goes through the
        // SampleStoreWrapperInitTotalMs seam. Behavior-preserving: emits one line, changes no phase
        // order, await, or recorded gross value.
        private void EmitPhaseNet(
            StartupDiagnosticsProbe probe,
            string phase,
            TimeSpan phaseElapsed,
            double storeInitBeforeMs
        )
        {
            var grossMs = phaseElapsed.TotalMilliseconds;
            var storeInitDeltaMs = SampleStoreWrapperInitTotalMs() - storeInitBeforeMs;
            if (storeInitDeltaMs < 0.0)
            {
                storeInitDeltaMs = 0.0;
            }

            probe.EmitPhaseNet(
                phase,
                grossMs,
                storeInitDeltaMs,
                StartupDiagnosticsProbe.ComputeNetMs(grossMs, storeInitDeltaMs)
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
