# PENDING MAINTAINER CAPTURE — All-Phase UI-Heartbeat + Per-Phase GC (#211 Phase 3.2)

Timestamp: PENDING (replace with ISO-8601 yyyy-MM-ddTHH-mm at capture time)
Command: PENDING (how Outlook was launched for the non-debugger cold start)
EXIT_CODE: PENDING (or N/A for a GUI launch)

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
