# PENDING MAINTAINER CAPTURE — Full Add-In-Startup-Lifetime UI Heartbeat (#211 Phase 3.3)

Timestamp: PENDING (replace with ISO-8601 yyyy-MM-ddTHH-mm at capture time)

Command: PENDING (how Outlook was launched; e.g., direct non-debugger cold start)

EXIT_CODE: PENDING (or N/A for a GUI launch)

## Status

This is a placeholder for the runtime maintainer cold-start capture described in
`coldstart-startup-lifetime-heartbeat-capture-instructions-2026-06-24T11-00.md`. It is a runtime
maintainer task and is NOT executed by the automated toolchain.

Replace this file with the dated capture artifact
`runtime-capture-startup-lifetime-heartbeat-<ISO-8601 timestamp>.md` once the full ~2-minute
cold-start window has been captured.

## To be filled in at capture time

- The full `[startup-lifetime-heartbeat] stageLabel=<...> nominalMs=250.0 actualMs=<f1> gapMs=<f1>`
  line stream, continuous from before `Application_Startup() complete` through
  `Finished loading globals` and beyond, with stage labels progressing
  `PreGlobalsCtor -> GlobalsCtor -> AwaitingIdleQueue -> Loading -> PostLoad`.
- Confirmation the heartbeat self-stopped (max cap ~180 s OR sustained post-load responsiveness).
- Confirmation the existing `[ui-heartbeat]`, `[gc-delta]`, `[continuation-resume]`,
  `[engine-init]`, and `[Startup timing]` lines still appear unchanged.
- Attribution note: the largest-gap tick(s), the `stageLabel` at that tick, and the duration the
  STA was frozen.
