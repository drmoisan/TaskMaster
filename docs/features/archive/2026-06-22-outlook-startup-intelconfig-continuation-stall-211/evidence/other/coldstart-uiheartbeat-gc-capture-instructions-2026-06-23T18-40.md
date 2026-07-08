# Maintainer Cold-Start Capture Instructions — UI-Heartbeat + GC Probe (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Issue: #211 (AC11, AC12, AC13)
Branch: bug/outlook-startup-latency-211

This document gives the step-by-step maintainer procedure to produce the non-debugger
cold-start capture that validates the Phase 3.1 instrumentation at runtime. This task does
NOT run Outlook; the maintainer performs the capture and records the result in
`runtime-capture-uiheartbeat-gc-PLACEHOLDER.md` (same directory).

The instrumentation added in this increment is diagnosis-only and behavior-preserving:
- A UI-thread responsiveness heartbeat scheduled on `UiThread.Dispatcher` (250 ms nominal
  interval, `DispatcherPriority.Background`), running only around the `Engines` startup phase
  and disposed immediately after it. Each tick emits one `[ui-heartbeat]` line.
- A single `[gc-delta]` line emitted after the `Engines` phase.
- Two new worker-thread-context fields on the existing per-engine `[engine-init]` line.

## Prerequisites

1. Build the add-in from this branch (`bug/outlook-startup-latency-211`) in `Debug`:
   - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
   - Confirm `TaskMaster\bin\Debug\TaskMaster.dll` is the freshly built output and that the
     add-in is registered to load this build.
2. Ensure the diagnostic startup-timing flag is ON:
   - `TaskMaster` user setting `StartupTimingEnabled = true` (this selects the concrete
     `StartupTimingRecorder` so the `[Startup timing]` table is emitted; the sequential
     load path is the one carrying the new probes).
   - Confirm the add-in runs the sequential load path (`LoadAsync(parallel: false)` →
     `LoadSequentialAsync`); the heartbeat/GC/engine-init probes are wired into
     `LoadSequentialAsync`.
3. Ensure `Debug`-level log4net output is captured:
   - The probe lines are emitted via `logger.Debug(...)`. Confirm the active log4net
     configuration routes `DEBUG` for the `TaskMaster.ApplicationGlobals` and
     `TaskMaster.EngineInitTimingProbe` loggers to an appender you can read
     (OutputDebugString/DebugView, a rolling file appender, or the console appender).
   - If using DebugView (Sysinternals): run it as administrator, enable
     "Capture Global Win32" so OutputDebugString from the Outlook process is captured.

## Capture Procedure (non-debugger cold start)

1. Close Outlook completely. Confirm no `OUTLOOK.EXE` process remains.
2. To approximate a cold start (the slow-path condition under investigation):
   - Ensure Teams is installed and running (prior captures attribute slow starts to a
     loaded/contended machine state).
   - Optionally clear OS file cache pressure by rebooting, so the SpamBayes classifier
     data file is read cold from disk.
3. Start the log capture tool (DebugView or your file-appender tail) BEFORE launching Outlook.
4. Launch Outlook OUTSIDE the Visual Studio debugger (start `OUTLOOK.EXE` directly, not via
   F5). Attaching a debugger perturbs JIT/GC timing and invalidates the cold-start attribution.
5. Wait for startup to complete (the `[Startup timing]` table is the end-of-startup marker).
6. Save the full captured log.

## Markers To Collect (exact)

- Recurring `[ui-heartbeat]` lines (one per ~250 ms tick during the `Engines` phase). Each line:
  `[ui-heartbeat] nominalMs=250.0 actualMs=<F1> gapMs=<F1>`
  Collect ALL of them for the `Engines` phase window, and explicitly note the MAXIMUM observed
  `gapMs`.
- The single `[gc-delta]` line (emitted once, immediately after the `Engines` phase):
  `[gc-delta] gen0=<n> gen1=<n> gen2=<n> allocatedBytesDelta=<n> isServerGC=<True|False> latencyMode=<mode>`
- The per-engine `[engine-init]` lines, in particular the `Spam` line, now including the new
  worker-thread-context fields:
  `[engine-init] engineName=Spam engineMs=<F1> engineNull=<bool> threadId=<n> costHint=<...> threadPriority=<ThreadPriority> isThreadPoolThread=<True|False>`
- The `[Startup timing]` table (end-of-startup), for the per-phase totals including `Engines`.
- For correlation: the per-phase `[continuation-resume]` lines are also present (pre-existing
  Phase 1 probe) and may be collected as supporting context.

## Attribution Method / Decision Rule

The log4net appender already prefixes each line with a wall-clock timestamp, so the
`[ui-heartbeat]`, `[gc-delta]`, and `Spam` `[engine-init]` lines can be aligned in time.

1. Establish the `Engines`-phase window from the first `[ui-heartbeat]` after the `AutoFile`
   phase to the `[gc-delta]` line (which is emitted right after the `Engines` phase completes).
2. Inspect the `gapMs` values across that window:
   - Regular ticks (`gapMs` near 0, i.e. `actualMs` near 250 ms): the UI/STA thread was
     responsive throughout the `Engines` phase. This corroborates the Phase 3 finding that
     SpamBayes deserialization runs on a background thread-pool thread (confirm via the
     `Spam` `[engine-init]` line: `isThreadPoolThread=True`), NOT by suspending the UI thread.
   - One or more large positive `gapMs` spikes (e.g. `gapMs` of several hundred ms or more,
     `actualMs` far above 250 ms): the UI/STA thread was starved or suspended between those
     ticks. Cross-reference the timestamp of each spike against the `[gc-delta]` line:
3. Decision rule:
   - Regular ~250 ms ticks across the whole `Engines` window => UI thread responsive; the
     observed startup latency is NOT a UI-thread suspension during `Engines`.
   - Large `gapMs` spike(s) whose timing coincides with a non-zero `gen2` delta in the
     `[gc-delta]` line (a blocking Gen2 collection during the Spam load) => the UI stall is
     blocking-GC-attributable. Record `isServerGC` and `latencyMode` as the GC-configuration
     context for that attribution.
   - Large `gapMs` spike(s) NOT coinciding with a Gen2 collection => the stall is attributable
     to something other than GC during `Engines`; record the spike timing and the surrounding
     markers for follow-up (no fix is in scope here).

This capture is diagnosis-only. It does not implement or validate any latency fix; the fix
remains AC10-gated and out of scope for this increment.
