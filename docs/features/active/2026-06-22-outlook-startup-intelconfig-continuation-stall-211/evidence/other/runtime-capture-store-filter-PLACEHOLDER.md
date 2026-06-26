# PENDING MAINTAINER CAPTURE — store-filter cold-start (issue #211, Phase 3.4)

Timestamp: PENDING (replace with ISO-8601 yyyy-MM-ddTHH-mm when captured)
Command: PENDING (non-debugger cold start of outlook.exe with DebugView capture; see coldstart-store-filter-capture-instructions-2026-06-24T14-30.md)
EXIT_CODE: PENDING

## Status

PENDING MAINTAINER CAPTURE. This is a runtime maintainer task not executed by the automated toolchain.
It requires a live Outlook process with the Gmail/GWSO store present and a non-debugger cold start.

Replace this placeholder with the dated capture artifact `runtime-capture-store-filter-<ISO-8601>.md`
once collected, following `coldstart-store-filter-capture-instructions-2026-06-24T14-30.md`.

## Expected contents once captured

- One `[store-filter] displayName=... exchangeStoreTypeMs=... filePathMs=... included=... rule=...` line per enumerated store.
- One `[store-filter] GetFilteredStores completed: <count> stores in <ms> ms` synchronous-Init summary line.
- Confirmation that the existing `[Startup timing]`, `[ui-heartbeat]`, `[gc-delta]`, `[continuation-resume]`, `[engine-init]`, and `[startup-lifetime-heartbeat]` lines still appear.
- Analysis: the largest `filePathMs`/`exchangeStoreTypeMs`, and the Gmail/GWSO store's `included`/`rule`.
