# Runtime Capture — UI-Heartbeat + GC Probe (#211 Phase 3.1) — SUPERSEDED

STATUS: SUPERSEDED. The capture was performed and is recorded at
`runtime-capture-uiheartbeat-gc-2026-06-23T21-55.md` (same directory).

Result: GC disproven as the cause (1 Gen0, 0 Gen2 during Engines); SpamBayes was a
single-run artifact (Spam 2.5 s this run). The latency moved to IntelConfig (60 s)
and ToDo (55.7 s); it is a cross-cutting intermittent STA stall correlated with
external Outlook MAPI/Gmail-sync/address-book provider churn. The heartbeat was
scoped to the Engines phase only, so it did not cover the slow phases; the next
increment widens the probe to the entire LoadSequentialAsync. See the recorded
capture for full analysis.

## Required Schema (to be filled in on capture)

- Timestamp: <ISO-8601 yyyy-MM-ddTHH-mm of the capture>
- Environment:
  - Build: TaskMaster Debug from branch bug/outlook-startup-latency-211, commit <sha>
  - OS / Outlook version: <...>
  - Cold/slow-start conditions: <Teams running? reboot? cold disk read of SpamBayes data?>
  - Debugger attached: NO (required — non-debugger capture)
  - StartupTimingEnabled: true
  - Log capture method: <DebugView global capture | file appender | console>

- `[ui-heartbeat]` lines (representative sample across the Engines-phase window):
  ```
  <paste a representative sample, e.g. first few, last few, and any spike ticks>
  ```
  - Tick count during Engines phase: <n>
  - Maximum observed gapMs: <value> (the decisive UI-starvation/suspension signal)

- `[gc-delta]` line (single, emitted right after the Engines phase):
  ```
  [gc-delta] gen0=<n> gen1=<n> gen2=<n> allocatedBytesDelta=<n> isServerGC=<bool> latencyMode=<mode>
  ```

- `Spam` `[engine-init]` line (with the new worker-thread-context fields):
  ```
  [engine-init] engineName=Spam engineMs=<F1> engineNull=<bool> threadId=<n> costHint=<...> threadPriority=<...> isThreadPoolThread=<bool>
  ```

- `[Startup timing]` table (end-of-startup, per-phase totals):
  ```
  <paste the table, including the Engines row>
  ```

## Attribution Result (to be filled in on capture)

- UI thread responsive throughout Engines phase? <YES/NO> (regular ~250 ms ticks => YES)
- If NO: did the max gapMs spike coincide (by timestamp) with a non-zero gen2 delta in the
  `[gc-delta]` line? <YES => blocking-GC-attributable UI stall | NO => non-GC cause, record for follow-up>
- Decision (per the instructions' decision rule): <...>

Note: completion of this capture is maintainer-run and is NOT a gate on the code/tests of this
increment, which are validated by the deterministic MSTest suite and the C# toolchain.
