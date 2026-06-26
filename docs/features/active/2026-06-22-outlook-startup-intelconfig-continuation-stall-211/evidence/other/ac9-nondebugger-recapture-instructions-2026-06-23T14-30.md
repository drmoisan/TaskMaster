# AC9 — Maintainer Non-Debugger Re-Capture Instructions (#211 Phase 3)

Timestamp: 2026-06-23T14-30

Scope: This document instructs a maintainer how to produce the non-debugger cold-start
runtime capture that records the new per-engine attribution lines added in Phase 3. The
capture itself is a manual, maintainer-run step and is NOT CI-automatable: it requires a
live Outlook process with the add-in loaded outside the Visual Studio debugger. This
document does NOT launch Outlook.

## Why a non-debugger capture is required

The dominant `Engines`-phase wall-clock cost (measured at `1:52.59` of a `1:58.79` total in
the prior non-debugger capture) is observable only under a real cold start with the COM host
and Teams present. Running under the Visual Studio debugger changes JIT, GC, and STA-pump
timing and is not representative. The Phase 3 instrumentation now subdivides that
`Engines`-phase number into one upfront `Configuration` deserialize cost plus one per-engine
factory cost, so the attribution can be read directly from the log.

## Prerequisites

1. Build the add-in from this branch (`bug/outlook-startup-intelconfig-continuation-stall-211`)
   in `Debug` (or the configuration normally used for diagnostic captures) so that
   `TaskMaster.dll` contains `EngineInitTimingProbe` and the instrumented `AppItemEngines.InitAsync()`.
2. Ensure the startup-timing flag is ON: set `TaskMaster` user setting
   `Settings.Default.StartupTimingEnabled = true` (this is the same flag that drives the
   `[Startup timing]` table from issue #202). With the flag on, the concrete
   `StartupTimingRecorder` is selected in `ApplicationGlobals.LoadAsync` and the end-of-startup
   `[Startup timing]` table is emitted.
3. Ensure `log4net` is emitting at `Debug` level to a sink you can capture. The new
   attribution lines are written via `logger.Debug(...)`. The simplest capture path is
   `OutputDebugString` via Sysinternals DebugView:
   - Launch DebugView (`Dbgview.exe`) as Administrator BEFORE starting Outlook.
   - Enable `Capture > Capture Win32` and `Capture > Capture Global Win32` so messages from the
     Outlook process are recorded.
   - Confirm the log4net configuration routes `Debug` to an `OutputDebugStringAppender` (or an
     equivalent file appender you will collect). If only a file appender is configured, collect
     that log file instead of DebugView output.

## Capture procedure (maintainer-run; NOT performed by this plan)

1. Close all Outlook instances. Ensure Teams is installed and running, matching the original
   capture environment (Teams presence affects STA occupancy during startup).
2. Start DebugView (or arm the file appender) and clear its buffer.
3. Launch Outlook normally from the Start menu / taskbar — OUTSIDE the Visual Studio debugger
   (do not press F5; do not attach the debugger). This is the defining condition of the capture.
4. Wait for the add-in to finish startup (until the `[Startup timing]` table line appears).
5. Save the full captured log to:
   `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/other/runtime-capture-engines-nondebugger-<timestamp>.md`
   (fill in the placeholder `runtime-capture-engines-nondebugger-PLACEHOLDER.md` created by P4-T2).

## Exact log markers to collect

Collect every line matching these markers, in emission order:

1. One `[engine-init-config]` line (upfront `Configuration` deserialize attribution):
   - Format: `[engine-init-config] configMs=<F1> threadId=<id>`
   - `configMs` is the wall-clock time for `await Globals.AF.Manager.Configuration` (the
     research Candidate 2 cost).

2. One `[engine-init]` line per ACTIVE engine (only engines whose `config.Value.Engine` is true
   AND that have an `EngineInitializer` entry are emitted; engines are Spam, Triage, Project,
   Context, Actionable when active):
   - Format: `[engine-init] engineName=<name> engineMs=<F1> engineNull=<True|False> threadId=<id> costHint=<Deserialization|Skip>`
   - `engineMs` is the per-engine factory wall-clock time. `costHint=Deserialization` for a
     non-null engine, `costHint=Skip` when the factory returned null.

3. The single end-of-startup `[Startup timing]` table (issue #202 output) containing the
   per-phase wall-clock breakdown, including the `Engines` phase total.

Optional context (already present from Phase 1): the `[continuation-resume] priorPhase=Engines ...`
line that follows the `Engines` phase yield.

## Attribution method

1. Read the `Engines`-phase total from the `[Startup timing]` table.
2. Sum: `configMs` (from `[engine-init-config]`) + the `engineMs` values from each `[engine-init]`
   line. This sum should account for the large majority of the `Engines`-phase total. Any
   residual is coordinator/await overhead.
3. Rank the individual `engineMs` values (and `configMs`) to identify the dominant contributor:
   - If `configMs` dominates, the `Configuration` deserialize (research Candidate 2) is the
     target for Phase 4 Fix C/D (pre-warming `Configuration` / `PreserveReferencesHandling`).
   - If a single `engineMs` dominates, that engine's classifier load is the target for Fix A/B
     (parallelizing engine init / deferring to `IdleAsyncQueue`).
4. Record the dominant contributor and the supporting numbers in the capture file. Phase 4
   (the fix, AC10) is gated on this attribution and is intentionally NOT implemented by this
   plan.

## Notes

- This is a diagnosis-only capture. The instrumentation is behavior-preserving; phase order,
  the engine set, and load semantics are unchanged.
- Do not run this capture under the debugger; debugger timing is not representative of the
  reported stall.
