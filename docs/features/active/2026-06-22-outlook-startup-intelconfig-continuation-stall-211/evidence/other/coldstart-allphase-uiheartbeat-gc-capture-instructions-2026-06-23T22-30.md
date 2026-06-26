# Maintainer Capture Instructions — All-Phase UI-Heartbeat + Per-Phase GC (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30

## Purpose

Phase 3.2 widened the diagnosis-only UI-responsiveness heartbeat and the GC-delta probe to span
the ENTIRE sequential startup (`ApplicationGlobals.LoadSequentialAsync`), not only the Engines
phase. This capture determines, for whichever phase is slow on a given cold start, whether the
STA/UI thread is actually frozen during that phase (heartbeat gaps approximately equal to the
phase duration) or whether an async continuation merely waits while the STA stays responsive
(heartbeat continues to fire near the 250 ms nominal interval). It also records per-phase GC
activity so a GC-induced stall can be confirmed or ruled out for every phase.

This is a runtime maintainer task; it is not executed by the automated toolchain.

## Preconditions

- Build the branch in Debug (the build the executor validated).
- Enable diagnostic startup timing: set `Settings.Default.StartupTimingEnabled = true`
  (the same flag used by the issue #202 startup-timing table and the issue #211 probes).
- Have a DebugView-class tool ready to capture `OutputDebugString` / log4net DEBUG output
  (for example Sysinternals DebugView, with "Capture Global Win32" enabled if the add-in logs
  via the debug appender).

## Procedure

1. Close Outlook fully (confirm no `OUTLOOK.EXE` remains in Task Manager).
2. Start the capture tool and clear its buffer.
3. Perform a NON-DEBUGGER cold start of Outlook (launch Outlook directly, NOT via the Visual
   Studio debugger; debugger attachment perturbs the timing being measured).
4. Let startup complete through the add-in initialization (the `[Startup timing]` table marks
   the end of the sequential startup).
5. Save the full captured log to:
   `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/other/runtime-capture-allphase-uiheartbeat-gc-<ISO-8601 timestamp>.md`
   replacing the placeholder produced in P4-T2.

## Expected line shapes (all six phases)

For the entire sequential startup, the following lines are expected. Phase names, in startup
order, are: `IntelConfig`, `OlObjects`, `ToDo`, `AutoFile`, `Engines`, `Events`.

- UI heartbeat (recurring, ~every 250 ms, spanning ALL phases — start before IntelConfig, stop
  after Events). Each line is phase-annotated:
  ```
  [ui-heartbeat] phase=<name> nominalMs=250.0 actualMs=<f1> gapMs=<f1>
  ```
  - A large positive `gapMs` (approximately equal to the phase wall-clock) during a phase
    indicates the STA was starved/suspended during that phase.
  - `gapMs` near 0 during a long phase indicates the STA stayed responsive while an async
    continuation waited.

- Per-phase GC delta (exactly ONE per phase, six total), phase-annotated, after each phase:
  ```
  [gc-delta] phase=<name> gen0=<n> gen1=<n> gen2=<n> allocatedBytesDelta=<n> isServerGC=<bool> latencyMode=<mode>
  ```

- Unchanged lines (must appear exactly as before this increment — verify they were NOT altered):
  - Continuation-resume probe, one per inter-phase boundary (five total):
    ```
    [continuation-resume] priorPhase=<name> waitMs=<f1> resumeThreadId=<n> resumeSyncContext=<...> staIsIdle=<bool> staCpuUsage=<f3> staGuiActivity=<f1>
    ```
  - Engine-init line(s) from `EngineInitTimingProbe` (unchanged shape).

- Startup-timing table (`[Startup timing]`), one per startup, listing each phase's wall-clock.

## What to record in the capture artifact

- `Timestamp:`, `Command:` (how Outlook was launched), `EXIT_CODE:` (or N/A for a GUI launch).
- The full `[ui-heartbeat]`, `[gc-delta]`, `[continuation-resume]`, `[engine-init]`, and
  `[Startup timing]` lines.
- A short attribution note: which phase was slow this run, whether its heartbeat gaps tracked the
  phase duration (STA frozen) or stayed near nominal (STA responsive, async wait), and whether the
  per-phase `[gc-delta]` for the slow phase shows significant GC activity.
