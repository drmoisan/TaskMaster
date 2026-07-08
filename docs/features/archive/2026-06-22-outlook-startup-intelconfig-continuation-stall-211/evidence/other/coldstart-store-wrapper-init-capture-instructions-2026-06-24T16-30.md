# Cold-Start Capture Instructions — [store-wrapper-init] + [phase-net] (issue #211, Phase 3.6)

Timestamp: 2026-06-24T16-30
Status: Maintainer-gated, runtime. NOT CI-automatable (requires a live Outlook process and a cold start). Do not launch Outlook from automation.

## Purpose

Diagnose the maintainer hypothesis that `StoreWrapper.Init()` is a SHARED blocking cost (the
failing-store logon) absorbed by whichever startup phase timer is running when it first fires. The
two diagnostic line families added in Phase 3.6 let a slow cold start show which phase absorbed the
store-init cost and how much of each phase's gross time is store-init versus net.

## Where the lines appear

Both line families are emitted through the existing log4net Debug logger:
- `[store-wrapper-init]` lines come from `StoreWrapper.Init()` (UtilitiesCS).
- `[phase-net]` lines come from `ApplicationGlobals.LoadSequentialAsync` (TaskMaster), one per phase.

They appear in the same Debug log sink as the existing `[Startup timing]`, `[gc-delta]`,
`[ui-heartbeat]`, `[continuation-resume]`, `[engine-init]`, `[startup-lifetime-heartbeat]`,
`[store-filter]`, and `[spam-init]` lines. Confirm the log4net configuration routes `DEBUG` level
to a persistent appender (file appender preferred) before the run.

## Procedure (non-debugger cold start)

1. Close Outlook completely. Confirm no `OUTLOOK.EXE` process remains.
2. Optionally clear or note the current log file so the new run's lines are easy to isolate.
3. Start Outlook normally (NOT under a debugger; the multi-minute stall is attributed to the
   failing-store logon and should reproduce without a debugger attached).
4. Wait for the add-in to finish loading globals (watch for the "Finished loading globals" log
   point or the post-load lifetime-heartbeat stage label `PostLoad`).
5. Capture the log. During a SLOW startup, record the items below.

## What to record

### Every `[store-wrapper-init]` line

Format: `[store-wrapper-init] store=<DisplayName-or-<null>> totalMs=<F1> threadId=<id>`

Record, for each line:
- `store` — the store DisplayName (or `<null>`).
- `totalMs` — total milliseconds spent inside that store's `Init()`.
- `threadId` — the managed thread id that ran the Init.

A single store with a very large `totalMs` (for example in the tens of thousands of ms) is the
failing-store logon signal.

### The six per-phase `[phase-net]` lines

Format: `[phase-net] phase=<name> grossMs=<F1> storeWrapperInitMs=<F1> netMs=<F1>`

Record one line for EACH phase, in startup order:
1. `phase=IntelConfig`
2. `phase=OlObjects`
3. `phase=ToDo`
4. `phase=AutoFile`
5. `phase=Engines`
6. `phase=Events`

For each: `grossMs` (phase gross wall-clock), `storeWrapperInitMs` (store-init attributed to the
phase window), and `netMs` (gross minus store-init, clamped at 0.0).

## Expected diagnostic signal

If `StoreWrapper.Init` is the shared blocking cost, the phase whose `[phase-net]` line shows a large
`storeWrapperInitMs` (with a correspondingly small `netMs`) identifies which phase absorbed the
failing-store logon. Because store inits run on background work and the slow store can be touched at
different points across runs, the phase that shows the large `storeWrapperInitMs` is EXPECTED to
shift run-to-run (for example IntelConfig in one run, ToDo or Engines in another). A phase with a
large `grossMs` but ALSO a large `netMs` (small `storeWrapperInitMs`) would instead indicate the
slow cost is NOT the store-init logon and lies in that phase's own work.

## Caveats

- The `[phase-net]` `storeWrapperInitMs` is sampled from a process-global accumulator. If a store
  init on a background thread overlaps two phases, its cost is attributed to whichever phase window
  the sampling brackets; this is the intended shared-cost attribution and is why the clamp rule
  (`netMs = max(0, grossMs - storeWrapperInitMs)`) exists.
- This capture is observation-only; Phase 3.6 adds NO fix. The fix is out of scope for this plan.
