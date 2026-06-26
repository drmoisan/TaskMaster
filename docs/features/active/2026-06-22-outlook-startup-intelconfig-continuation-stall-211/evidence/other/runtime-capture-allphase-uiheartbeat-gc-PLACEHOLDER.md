# PARTIALLY SATISFIED — All-Phase UI-Heartbeat + Per-Phase GC (#211 Phase 3.2)

A capture was performed: `runtime-capture-allphase-uiheartbeat-gc-2026-06-24T10-24.md`
(same directory). It validated the all-phase heartbeat + per-phase GC end-to-end,
but that run was FAST (TOTAL 0:03.16) and did NOT reproduce the multi-minute stall.
A SLOW-run capture with this build is still needed to observe heartbeat behavior
during a 60 s+ phase. This placeholder remains open for that slow-run capture.

Timestamp: PENDING (slow-run capture)
Command: PENDING (non-debugger cold start during a slow startup occurrence)
EXIT_CODE: PENDING

## Status

PENDING MAINTAINER CAPTURE. This is a runtime maintainer task and is NOT executed by the
automated toolchain. Replace this placeholder with the dated capture artifact
`runtime-capture-allphase-uiheartbeat-gc-<timestamp>.md` once the all-phase cold-start capture is
collected, following
`coldstart-allphase-uiheartbeat-gc-capture-instructions-2026-06-23T22-30.md`.

## Expected contents once captured

- `[ui-heartbeat] phase=<name> ...` lines spanning all six phases (IntelConfig, OlObjects, ToDo,
  AutoFile, Engines, Events).
- One `[gc-delta] phase=<name> ...` line per phase (six total).
- Unchanged `[continuation-resume]` lines (five) and `[engine-init]` line(s).
- The `[Startup timing]` table.
- An attribution note for whichever phase was slow this run (STA frozen vs async-wait; GC ruled
  in or out).
