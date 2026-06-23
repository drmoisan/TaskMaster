# AC9 Runtime Capture — Engines-Phase Attribution (NON-DEBUGGER) — PLACEHOLDER (PENDING)

STATUS: PENDING MAINTAINER EXECUTION. This file is a placeholder. It asserts NO timing
values. The capture must be produced by a maintainer following the instructions in
`ac9-nondebugger-recapture-instructions-2026-06-23T14-30.md`. The capture requires a live
Outlook cold start outside the Visual Studio debugger and is not CI-automatable.

When the capture is performed, copy this file to
`runtime-capture-engines-nondebugger-<ISO-8601-timestamp>.md` and fill in the schema below.

## Required schema (to be filled in on capture)

Timestamp: <ISO-8601 of the capture, e.g. 2026-06-24T09-15>

Environment:
- Branch / build: <branch + build configuration; must include EngineInitTimingProbe>
- Outlook version: <version>
- Launch mode: NON-DEBUGGER (launched from Start menu / taskbar; NOT F5 / not attached)
- Teams installed/running: <yes/no>
- StartupTimingEnabled: <true — required>
- log4net Debug sink: <DebugView OutputDebugString | file appender path>

Captured lines (paste verbatim, in emission order):

[engine-init-config] configMs=<F1> threadId=<id>

[engine-init] engineName=Spam engineMs=<F1> engineNull=<bool> threadId=<id> costHint=<Deserialization|Skip>
[engine-init] engineName=Triage engineMs=<F1> engineNull=<bool> threadId=<id> costHint=<Deserialization|Skip>
[engine-init] engineName=Project engineMs=<F1> engineNull=<bool> threadId=<id> costHint=<Deserialization|Skip>
[engine-init] engineName=Context engineMs=<F1> engineNull=<bool> threadId=<id> costHint=<Deserialization|Skip>
[engine-init] engineName=Actionable engineMs=<F1> engineNull=<bool> threadId=<id> costHint=<Deserialization|Skip>
(include only engines that were active in this run)

[Startup timing] table (paste the full issue #202 table, including the Engines-phase total):
<paste table>

## Attribution (to be filled in on capture)

- Engines-phase total (from [Startup timing]): <ms>
- Sum of configMs + per-engine engineMs: <ms>
- Dominant contributor: <Configuration | engine name> with <ms>
- Phase 4 fix target implied: <Fix A/B/C/D per the instructions> (NOT implemented by this plan)

NOTE: No timing values are asserted by this placeholder. AC9 is satisfied for Phase 3 by the
instrumentation + instructions + this pending placeholder; the runtime numbers are recorded by
the maintainer on capture and gate Phase 4 (AC10).
