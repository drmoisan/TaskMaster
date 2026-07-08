# Maintainer Cold-Start Capture Instructions — store-filter attribution (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30

This is a runtime maintainer task. It is NOT executed by the automated toolchain (it requires a live Outlook process and the Gmail/GWSO store). The automated executor does not launch Outlook.

## Goal

Determine whether reading `FilePath` or `ExchangeStoreType` on the Gmail/GWSO store during the
synchronous `StoresWrapper.Init()` filter path shows a large `Stopwatch` ms value (indicating the
previously-untimed filter path blocks the STA), and whether the Gmail store is being `included=true`.

## Procedure

1. Build the TaskMaster add-in from the current branch (`bug/outlook-startup-latency-211`) in Debug, registered into the live Outlook profile that contains the Gmail/GWSO store.
2. Enable the same startup diagnostic logging used for the existing `[Startup timing]` lines (log4net Debug-level output to the OutputDebugString/DebugView sink).
3. Start a DebugView capture (Sysinternals DebugView, "Capture Win32" + "Capture Global Win32" enabled).
4. Perform a NON-DEBUGGER COLD start of Outlook (close Outlook fully; do NOT attach Visual Studio; launch `outlook.exe` directly). A cold start is required so the slow-startup window reproduces.
5. Capture the full startup window in DebugView (from process launch until the startup timing table completes), then save the log to this evidence folder as `runtime-capture-store-filter-<ISO-8601>.md`.

## Expected line shapes

Per enumerated store (one line each, emitted from `ShouldIncludeStoreInstrumented`):

```
[store-filter] displayName=<...> exchangeStoreTypeMs=<F1> filePathMs=<F1> included=<true|false> rule=<PublicFolder|NameContains|GwsoFilePath|FilePathContains|Included>
```

Synchronous `Init()` summary (one line, emitted once after enumeration):

```
[store-filter] GetFilteredStores completed: <count> stores in <ms> ms
```

## Confirm the existing instrumentation is unaffected

The following pre-existing lines MUST still appear in the same capture (this change does not modify them):

- `[Startup timing]` lines from `StoreWrapper.Init` and `GetSmtpAddressFromStore` (per-store COM-call timing).
- `[Startup timing] GetFilteredStores completed: ...` from the ASYNC `RewireOlObjectsAsync` path (note: this is the `[Startup timing]` tag, distinct from the new sync-path `[store-filter]` summary).
- `[ui-heartbeat]`, `[gc-delta]`, `[continuation-resume]`, `[engine-init]`, and `[startup-lifetime-heartbeat]` lines.

## Analysis goal

- Identify the store with the largest `filePathMs` and/or `exchangeStoreTypeMs`. A large `filePathMs`
  on the Gmail/GWSO store would indicate the filter path's `store.FilePath` read blocks the STA.
- Record the Gmail/GWSO store's `included` value and `rule`. With default config (`ExcludeGwsoStores=true`,
  the two `\Google\Google ... Sync\` tokens) the expected result is `included=false rule=GwsoFilePath`.
- Compare the `[store-filter] GetFilteredStores completed` total ms against the previously-observed
  Engines/IntelConfig/ToDo phase stalls to assess whether store enumeration is a contributor.

Acceptance: this instructions file enumerates the expected line shapes (per-store + summary) and the analysis goal.
