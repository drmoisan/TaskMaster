# Runtime Capture — UI-Heartbeat + GC Probe (#211 Phase 3.1) — PENDING MAINTAINER EXECUTION

STATUS: PENDING. This capture has not been performed. It awaits a maintainer non-debugger
cold-start run per `coldstart-uiheartbeat-gc-capture-instructions-2026-06-23T18-40.md`
(same directory). This file asserts NO timing or GC values; it is a schema placeholder only.

The instrumentation that produces these markers is diagnosis-only and behavior-preserving
(issue #211 Phase 3.1, AC11/AC12/AC13). No latency fix is implemented.

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
