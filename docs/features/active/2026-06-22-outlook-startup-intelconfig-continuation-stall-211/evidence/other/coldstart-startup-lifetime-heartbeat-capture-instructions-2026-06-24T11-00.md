# Maintainer Capture Instructions — Full Add-In-Startup-Lifetime UI Heartbeat (#211 Phase 3.3)

Timestamp: 2026-06-24T11-00

## Purpose

Phase 3.3 adds a full add-in-startup-lifetime UI heartbeat that starts as the FIRST action in
`ThisAddIn.Application_Startup` and runs CONTINUOUSLY across the ENTIRE add-in startup, independent
of `ApplicationGlobals.LoadSequentialAsync`. The Phase 3.1/3.2 heartbeat covered only
`LoadSequentialAsync` (~3 s); the maintainer confirms the UI locks for ~2 minutes on every cold
start, so most of the freeze window had no heartbeat coverage. This capture measures the whole
startup lifetime so the next trace shows exactly when the STA (UI thread) is frozen — before
`LoadSequentialAsync`, during it, and after globals finish — and for how long.

This is a runtime maintainer task; it is not executed by the automated toolchain.

## Preconditions

- Build the branch in Debug (the build the executor validated for this plan).
- Enable diagnostic startup timing: set `Settings.Default.StartupTimingEnabled = true` (the same
  flag used by the issue #202 startup-timing table and the issue #211 probes). Confirm the
  full-lifetime heartbeat is enabled as built (Phase 3.3 starts it unconditionally as the first
  action of `Application_Startup`; if a gating flag is added later, enable it here).
- Have a DebugView-class tool ready to capture `OutputDebugString` / log4net DEBUG output (for
  example Sysinternals DebugView, with "Capture Global Win32" enabled if the add-in logs via the
  debug appender).

## Procedure

1. Close Outlook fully (confirm no `OUTLOOK.EXE` remains in Task Manager).
2. Start the capture tool and clear its buffer.
3. Perform a NON-DEBUGGER COLD start of Outlook (launch Outlook directly, NOT via the Visual Studio
   debugger; debugger attachment perturbs the timing being measured).
4. Capture the FULL ~2-minute startup window. Do not stop the capture at `Application_Startup()
   complete` or at `Finished loading globals`; keep capturing until the external Outlook provider
   churn settles (the prior capture observed `GLookSyncer` / `GmailSyncImpl::Init` /
   `WrappedMSProvider::Logon` activity continuing to ~108 s after `Application_Startup`), or until
   the heartbeat self-stops (see below), whichever is later.
5. Save the full captured log to:
   `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/other/runtime-capture-startup-lifetime-heartbeat-<ISO-8601 timestamp>.md`
   replacing the placeholder produced in P4-T2.

## Expected line shape (full lifetime)

The full add-in-startup-lifetime heartbeat emits one line per ~250 ms tick, continuously, from
before `Application_Startup() complete` through `Finished loading globals` and beyond:

```
[startup-lifetime-heartbeat] stageLabel=<...> nominalMs=250.0 actualMs=<f1> gapMs=<f1>
```

- A `gapMs` far larger than nominal (approximately equal to the wall-clock the STA was blocked)
  proves the STA/UI thread was frozen for that interval.
- `gapMs` near 0 indicates the STA stayed responsive across that tick.

### Stage-label progression

The `stageLabel` field is a coarse marker that should progress, in order:

`PreGlobalsCtor` -> `GlobalsCtor` -> `AwaitingIdleQueue` -> `Loading` -> `PostLoad`

- `PreGlobalsCtor`: heartbeat started, before `new ApplicationGlobals(...)`.
- `GlobalsCtor`: during `new ApplicationGlobals(Application, true)`.
- `AwaitingIdleQueue`: after the load lambda is enqueued, before the idle queue runs it.
- `Loading`: while `_globals.LoadAsync(false)` is in flight.
- `PostLoad`: after the `Finished loading globals` log point.

### Bounded self-stop

The heartbeat self-stops and disposes its timer when EITHER:
- (a) a max cap (~180 s after start) is reached, OR
- (b) after `PostLoad` is reached, the UI has been continuously responsive (gapMs below the small
  responsiveness threshold) for the required sustained run of consecutive ticks.

Confirm in the capture that the `[startup-lifetime-heartbeat]` lines stop (no permanent timer leak).

### Existing instrumentation must still appear (unchanged)

Verify the following Phase 3.1/3.2 lines still appear exactly as before (this increment must NOT
alter them):
- `[ui-heartbeat] phase=<name> nominalMs=250.0 actualMs=<f1> gapMs=<f1>` (LoadSequentialAsync-scoped)
- `[gc-delta] phase=<name> gen0=<n> gen1=<n> gen2=<n> allocatedBytesDelta=<n> isServerGC=<bool> latencyMode=<mode>`
- `[continuation-resume] priorPhase=<name> waitMs=<f1> resumeThreadId=<n> ...`
- `[engine-init] ...`
- The `[Startup timing]` table.

## Analysis goal

From the captured `[startup-lifetime-heartbeat]` lines, identify the tick(s) with the largest
`gapMs` and the `stageLabel` in effect at that tick. This locates exactly WHEN (which startup
stage) and FOR HOW LONG the STA was frozen across the full ~2-minute startup, closing the
diagnostic gap left by the LoadSequentialAsync-scoped heartbeat.

## What to record in the capture artifact

- `Timestamp:`, `Command:` (how Outlook was launched), `EXIT_CODE:` (or N/A for a GUI launch).
- The full `[startup-lifetime-heartbeat]` line stream (with stage-label progression), plus the
  unchanged `[ui-heartbeat]`, `[gc-delta]`, `[continuation-resume]`, `[engine-init]`, and
  `[Startup timing]` lines.
- A short attribution note: the largest-gap tick(s), the `stageLabel` at that tick, the duration
  the STA was frozen, and whether the heartbeat self-stopped on the max cap or on sustained
  responsiveness.
